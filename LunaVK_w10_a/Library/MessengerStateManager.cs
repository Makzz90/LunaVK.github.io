using System;
using System.Collections.Generic;
using System.Text;
using LunaVK.Core.DataObjects;
using LunaVK.Network;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using LunaVK.Framework;
using LunaVK.Core.ViewModels;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.ViewModels;
using LunaVK.Core;
using LunaVK.Core.Framework;
using System.Diagnostics;
using System.Linq;

namespace LunaVK.Library
{
    public class MessengerStateManager
    {
        private DateTime _lastSound = DateTime.Now;
        private static MessengerStateManager _instance;
        public static MessengerStateManager Instance
        {
            get
            {
                if (MessengerStateManager._instance == null)
                    MessengerStateManager._instance = new MessengerStateManager();
                return MessengerStateManager._instance;
            }
        }

        public void Initialize()
        {
            LongPollServerService.Instance.ReceivedUpdates += Instance_ReceivedUpdates;
        }

        void Instance_ReceivedUpdates(List<UpdatesResponse.LongPollServerUpdateData> updates)
        {
#if DEBUG
            foreach (var update in updates)
            {
                int count = updates.Count((u)=>u.peer_id== update.peer_id && u.message_id == update.message_id);
                //Debug.Assert(count == 1);
            }
#endif
                Execute.ExecuteOnUIThread(() =>
            {
                foreach (var update in updates)
                {
                    switch (update.UpdateType)
                    {
                        case LongPollServerUpdateType.MessageAdd:
                            {
                                if (update.flags.OUTBOX == true)
                                    continue;
                                
                                this.HandleInAppNotification(update);
                                break;
                            }
                    }
                }
            });
        }

        private void HandleInAppNotification(UpdatesResponse.LongPollServerUpdateData update)
        {
            if (update.user_id > 0)
            {
                List<uint> userIds = new List<uint>() { (uint)update.user_id };
                //todo: GetUsers больше не должно использоваться
                UsersService.Instance.GetUsers(userIds, (result) =>
                {
                    
                    if (result != null )
                    {
                        Debug.Assert(result.Count>0);
                        if (result.Count < 1)
                            return;

                        var user = result[0];

                        string temp = "";
                        if (update.message != null)
                        {
                            temp = DialogsViewModel.GetMessageHeaderText(update.message, null, null);
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(update.text))
                            {
                                string input = NavigatorImpl.Regex_DomainMention.Replace(update.text, "[$2|$4]");
                                temp = NavigatorImpl.Regex_Mention.Replace(input, "$4");
                            }
                        }
                        
                        Execute.ExecuteOnUIThread(() =>
                        {
                            this.HandleInAppNotification(user.Title, temp, update.peer_id, user.MinPhoto);
                        });


                    }
                });



            }
        }

        //паблик для теста
        public void HandleInAppNotification(string title, string message, int peer_id, string imageSrc)
        {
            if (Settings.PushNotificationsBlockedUntil >= DateTime.UtcNow)
                return;

            //Мы на странице сообщений?
            if (CustomFrame.Instance.Content is Pages.DialogsConversationPage2 conversationPage)
            {
                if (conversationPage.UserOrCharId != -1)
                {
                    if (conversationPage.UserOrCharId == peer_id)
                        return;
                }
            }

            var conversation = DialogsViewModel.Instance.Items.FirstOrDefault((c) => c.conversation.peer.id == peer_id);
            if(conversation!=null)
            {
                if (conversation.conversation.AreDisabledNow)
                    return;
            }

            if (Settings.NotificationsEnabled)
            {
                CustomFrame.Instance.NotificationsPanel.AddAndShowNotification(imageSrc, title, message, () =>
                {
                    if (peer_id < 2000000000)
                        NavigatorImpl.Instance.NavigateToConversation(peer_id);
                });
            }

            if (Settings.VibrationsEnabled)
            {
#if WINDOWS_PHONE_APP || WINDOWS_UWP
                TimeSpan vibrationTime = TimeSpan.FromMilliseconds(60);
                Windows.Phone.Devices.Notification.VibrationDevice.GetDefault().Vibrate(vibrationTime);
#endif
            }

            if (Settings.SoundEnabled)
            {
                if ((DateTime.Now - this._lastSound).TotalSeconds < 2)
                    return;

                this._lastSound = DateTime.Now;

                /*
                MediaElement mysong = new MediaElement();
                var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets\\Mp3");
                Windows.Storage.StorageFile file = await folder.GetFileAsync("bb2.mp3");
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                mysong.SetSource(stream, file.ContentType);
                mysong.Play();
                */
                (Window.Current.Content as CustomFrame).NotificationsPanel.Play();
            }
        }
    }
}
