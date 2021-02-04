using System;
using System.Collections.Generic;
using System.Text;
using LunaVK.Core.DataObjects;
using LunaVK.Core.ViewModels;
using LunaVK.Core;
using LunaVK.Core.Library;
using LunaVK.Library;
using LunaVK.Core.Utils;
using LunaVK.Common;
using LunaVK.Framework;
using Windows.UI.Xaml;
using LunaVK.Core.Framework;
using System.Linq;
using Windows.Globalization;
using System.Globalization;

namespace LunaVK.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public bool PushNotificationsEnabledAndNotTempDisabled
        {
            get
            {
                if (this.PushNotificationsEnabled>0)
                    return !this.TempDisabled;
                return false;
            }
        }

        public bool TempDisabled
        {
            get
            {
                if (this.PushNotificationsEnabled>0)
                    return Settings.PushNotificationsBlockedUntil >= DateTime.UtcNow;
                return false;
            }
        }


        private uint _prevPushNotificationsEnabled;
        public uint PushNotificationsEnabled
        {
            get
            {
                return Settings.PushNotificationsEnabled;
            }
            set
            {
                if (base.IsInProgress || this.PushNotificationsEnabled == value)
                    return;
                
                Settings.LastPushNotificationsUri = null;//force request
                this._prevPushNotificationsEnabled = Settings.PushNotificationsEnabled;
                Settings.PushNotificationsEnabled = value;

                base.SetInProgress(true);
                PushNotifications.Instance.UpdateDeviceRegistration((result)=>
                {
                    Execute.ExecuteOnUIThread(() =>
                    { 
                        base.SetInProgress(false);
                        if(result)
                        {
                            this._prevPushNotificationsEnabled = Settings.PushNotificationsEnabled;
                        }
                        else
                        {
                            Settings.PushNotificationsEnabled = this._prevPushNotificationsEnabled;
                        }
                    });
                    base.NotifyPropertyChanged(nameof(this.PushNotificationsEnabled));
                    base.NotifyPropertyChanged(nameof(this.PushNotificationsEnabledAndNotTempDisabled));
                });
                
            }
        }

        public string CustomPushNotificationsServer
        {
            get { return Settings.CustomPushNotificationsServer; }
            set
            {
                Settings.CustomPushNotificationsServer = value;
                base.NotifyPropertyChanged(nameof(this.CustomPushNotificationsServer));
            }
        }

        /// <summary>
        /// личные сообщения
        /// </summary>
        public bool NewPrivateMessagesNotifications
        {
            get
            {
                return Settings.PushSettings.msg;
            }
            set
            {
                var temp = Settings.PushSettings;
                temp.msg = value;
                Settings.PushSettings = temp;
            }
        }

        /// <summary>
        /// не передавать текст сообщения
        /// </summary>
        public bool NewPrivateMessagesNoText
        {
            get
            {
                return Settings.PushSettings.msg_no_text;
            }
            set
            {
                //Settings.PushSettings.msg_no_text = value;
                var temp = Settings.PushSettings;
                temp.msg_no_text = value;
                Settings.PushSettings = temp;
            }
        }

        public bool NewPrivateMessagesNoSound
        {
            get
            {
                return Settings.PushSettings.msg_no_sound;
            }
            set
            {
                //Settings.PushSettings.msg_no_sound = value;
                var temp = Settings.PushSettings;
                temp.msg_no_sound = value;
                Settings.PushSettings = temp;
            }
        }

        public bool NewChatMessagesNotifications
        {
            get
            {
                return Settings.PushSettings.chat;
            }
            set
            {
                //Settings.PushSettings.chat = value;
                var temp = Settings.PushSettings;
                temp.chat = value;
                Settings.PushSettings = temp;
            }
        }

        public bool NewChatMessagesNoText
        {
            get
            {
                return Settings.PushSettings.chat_no_text;
            }
            set
            {
                //Settings.PushSettings.chat_no_text = value;
                var temp = Settings.PushSettings;
                temp.chat_no_text = value;
                Settings.PushSettings = temp;
            }
        }

        public bool NewChatMessagesNoSound
        {
            get
            {
                return Settings.PushSettings.chat_no_sound;
            }
            set
            {
                //Settings.PushSettings.chat_no_sound = value;
                var temp = Settings.PushSettings;
                temp.chat_no_sound = value;
                Settings.PushSettings = temp;
            }
        }

        public bool FriendRequestsNotifications
        {
            get
            {
                return Settings.PushSettings.friend;
            }
            set
            {
                //Settings.PushSettings.friend = value;
                var temp = Settings.PushSettings;
                temp.friend = value;
                Settings.PushSettings = temp;
            }
        }

        public bool FriendRequestsMutual
        {
            get
            {
                return Settings.PushSettings.friend_mutual;
            }
            set
            {
                //Settings.PushSettings.friend_mutual = value;
                var temp = Settings.PushSettings;
                temp.friend_mutual = value;
                Settings.PushSettings = temp;
            }
        }

        public bool FriendFoundNotifications
        {
            get
            {
                return Settings.PushSettings.friend_found;
            }
            set
            {
                //Settings.PushSettings.friend_found = value;
                var temp = Settings.PushSettings;
                temp.friend_found = value;
                Settings.PushSettings = temp;
            }
        }

        public bool FriendAcceptedNotifications
        {
            get
            {
                return Settings.PushSettings.friend_accepted;
            }
            set
            {
                //Settings.PushSettings.friend_accepted = value;
                var temp = Settings.PushSettings;
                temp.friend_accepted = value;
                Settings.PushSettings = temp;
            }
        }

        public bool RepliesNotifications
        {
            get
            {
                return Settings.PushSettings.reply;
            }
            set
            {
                //Settings.PushSettings.reply = value;
                var temp = Settings.PushSettings;
                temp.reply = value;
                Settings.PushSettings = temp;
            }
        }

        public bool CommentsNotifications
        {
            get
            {
                return Settings.PushSettings.comment;
            }
            set
            {
                //Settings.PushSettings.comment = value;
                var temp = Settings.PushSettings;
                temp.comment = value;
                Settings.PushSettings = temp;
            }
        }

        public bool CommentsFrOfFr
        {
            get
            {
                return Settings.PushSettings.comment_fr_of_fr;
            }
            set
            {
                //Settings.PushSettings.comment_fr_of_fr = value;
                var temp = Settings.PushSettings;
                temp.comment_fr_of_fr = value;
                Settings.PushSettings = temp;
            }
        }

        public bool MentionsNotifications
        {
            get
            {
                return Settings.PushSettings.mention;
            }
            set
            {
                //Settings.PushSettings.mention = value;
                var temp = Settings.PushSettings;
                temp.mention = value;
                Settings.PushSettings = temp;
            }
        }

        public bool MentionsFrOfFr
        {
            get
            {
                return Settings.PushSettings.mention_fr_of_fr;
            }
            set
            {
                //Settings.PushSettings.mention_fr_of_fr = value;
                var temp = Settings.PushSettings;
                temp.mention_fr_of_fr = value;
                Settings.PushSettings = temp;
            }
        }

        public bool LikesNotifications
        {
            get
            {
                return Settings.PushSettings.like;
            }
            set
            {
                //Settings.PushSettings.like = value;
                var temp = Settings.PushSettings;
                temp.like = value;
                Settings.PushSettings = temp;
            }
        }

        public bool LikesFrOfFr
        {
            get
            {
                return Settings.PushSettings.like_fr_of_fr;
            }
            set
            {
                //Settings.PushSettings.like_fr_of_fr = value;
                var temp = Settings.PushSettings;
                temp.like_fr_of_fr = value;
                Settings.PushSettings = temp;
            }
        }

        public bool RepostsNotifications
        {
            get
            {
                return Settings.PushSettings.repost;
            }
            set
            {
                //Settings.PushSettings.repost = value;
                var temp = Settings.PushSettings;
                temp.repost = value;
                Settings.PushSettings = temp;
            }
        }

        public bool RepostsFrOfFr
        {
            get
            {
                return Settings.PushSettings.repost_fr_of_fr;
            }
            set
            {
                //Settings.PushSettings.repost_fr_of_fr = value;
                var temp = Settings.PushSettings;
                temp.repost_fr_of_fr = value;
                Settings.PushSettings = temp;
            }
        }

        public bool WallPostsNotifications
        {
            get
            {
                return Settings.PushSettings.wall_post;
            }
            set
            {
                //Settings.PushSettings.wall_post = value;
                var temp = Settings.PushSettings;
                temp.wall_post = value;
                Settings.PushSettings = temp;
            }
        }

        public bool WallPublishNotifications
        {
            get
            {
                return Settings.PushSettings.wall_publish;
            }
            set
            {
                //Settings.PushSettings.wall_publish = value;
                var temp = Settings.PushSettings;
                temp.wall_publish = value;
                Settings.PushSettings = temp;
            }
        }

        public bool GroupInvitationsNotifications
        {
            get
            {
                return Settings.PushSettings.group_invite;
            }
            set
            {
                //Settings.PushSettings.group_invite = value;
                var temp = Settings.PushSettings;
                temp.group_invite = value;
                Settings.PushSettings = temp;
            }
        }

        public bool GroupAcceptedNotifications
        {
            get
            {
                return Settings.PushSettings.group_accepted;
            }
            set
            {
                //Settings.PushSettings.group_accepted = value;
                var temp = Settings.PushSettings;
                temp.group_accepted = value;
                Settings.PushSettings = temp;
            }
        }

        public bool ForthcomingEventsNotifications
        {
            get
            {
                return Settings.PushSettings.event_soon;
            }
            set
            {
                //Settings.PushSettings.event_soon = value;
                var temp = Settings.PushSettings;
                temp.event_soon = value;
                Settings.PushSettings = temp;
            }
        }

        public bool TagPhotoNotifications
        {
            get
            {
                return Settings.PushSettings.tag_photo;
            }
            set
            {
                //Settings.PushSettings.tag_photo = value;
                var temp = Settings.PushSettings;
                temp.tag_photo = value;
                Settings.PushSettings = temp;
            }
        }

        public bool TagPhotoFrOfFr
        {
            get
            {
                return Settings.PushSettings.tag_photo_fr_of_fr;
            }
            set
            {
                //Settings.PushSettings.tag_photo_fr_of_fr = value;
                var temp = Settings.PushSettings;
                temp.tag_photo_fr_of_fr = value;
                Settings.PushSettings = temp;
            }
        }

        public bool AppRequestNotifications
        {
            get
            {
                return Settings.PushSettings.app_request;
            }
            set
            {
                //Settings.PushSettings.app_request = value;
                var temp = Settings.PushSettings;
                temp.app_request = value;
                Settings.PushSettings = temp;
            }
        }

        public bool SDKOpenNotifications
        {
            get
            {
                return Settings.PushSettings.sdk_open;
            }
            set
            {
                //Settings.PushSettings.sdk_open = value;
                var temp = Settings.PushSettings;
                temp.sdk_open = value;
                Settings.PushSettings = temp;
            }
        }

        public bool NewPostNotifications
        {
            get
            {
                return Settings.PushSettings.new_post;
            }
            set
            {
                //Settings.PushSettings.new_post = value;
                var temp = Settings.PushSettings;
                temp.new_post = value;
                Settings.PushSettings = temp;
            }
        }

        public bool BirthdaysNotifications
        {
            get
            {
                return Settings.PushSettings.birthday;
            }
            set
            {
               // Settings.PushSettings.birthday = value;
                var temp = Settings.PushSettings;
                temp.birthday = value;
                Settings.PushSettings = temp;
            }
        }

        

        public bool InAppSound
        {
            get
            {
                return Settings.SoundEnabled;
            }
            set
            {
                Settings.SoundEnabled = value;
                base.NotifyPropertyChanged("InAppSound");
            }
        }

        public bool IsAppVibration
        {
            get
            {
                return Settings.VibrationsEnabled;
            }
            set
            {
                Settings.VibrationsEnabled = value;
                base.NotifyPropertyChanged("IsAppVibration");
            }
        }

        public bool InAppBanner
        {
            get
            {
                return Settings.NotificationsEnabled;
            }
            set
            {
                Settings.NotificationsEnabled = value;
                base.NotifyPropertyChanged("InAppBanner");
            }
        }

        public bool StickersAutoSuggestEnabled
        {
            get
            {
                return Settings.StickersAutoSuggestEnabled;
            }
            set
            {
                Settings.StickersAutoSuggestEnabled = value;
                base.NotifyPropertyChanged("StickersAutoSuggestEnabled");
            }
        }

        public string TempDisabledString
        {
            get
            {
                if (this.TempDisabled)
                    return string.Concat(LocalizedStrings.GetString("Settings_DisabledNotifications"), " ", (Settings.PushNotificationsBlockedUntil + (DateTime.Now - DateTime.UtcNow)).ToString());
                return "";
            }
        }

        public void Disable(int seconds)
        {
            if (base.IsInProgress)
                return;
            DateTime savedValue = Settings.PushNotificationsBlockedUntil;
            Settings.PushNotificationsBlockedUntil = seconds != 0 ? DateTime.UtcNow + TimeSpan.FromSeconds((double)seconds) : DateTime.MinValue;
            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.TempDisabledString));
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.TempDisabled));
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.PushNotificationsEnabledAndNotTempDisabled));
            base.SetInProgress(true);

            AccountService.Instance.SetSilenceMode(PushNotifications.Instance.GetHardwareID,seconds, (res)=> {
                base.SetInProgress(false);
                if (res.error.error_code != Core.Enums.VKErrors.None || res.response != 1)
                {
                    Settings.PushNotificationsBlockedUntil = savedValue;
                    this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.TempDisabledString));
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.TempDisabled));
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.PushNotificationsEnabledAndNotTempDisabled));
                }
            });

            
        }

        //
        public Threelen FriendListOrder
        {
            get { return Settings.FriendListOrder; }
            set { Settings.FriendListOrder = value; }
        }
#if WINDOWS_PHONE_APP
        public bool MenuSwipe
        {
            get { return Settings.MenuSwipe; }
            set { Settings.MenuSwipe = value; }
        }
#endif
        public Threelen GifAutoplayType
        {
            get { return Settings.GifAutoplayType; }
            set { Settings.GifAutoplayType = value; }
        }

        public bool MusicWithWebPage
        {
            get { return Settings.MusicWithWebPage; }
            set { Settings.MusicWithWebPage = value; }
        }

        public bool SendByEnter
        {
            get { return Settings.SendByEnter; }
            set { Settings.SendByEnter = value; }
        }

        public bool SendByCtrlEnter
        {
            get { return !Settings.SendByEnter; }
        }

        public byte UIScale
        {
            get { return Settings.UIScale; }
            set { Settings.UIScale = value; }
        }

        public byte RoundAvatar
        {
            get { return Settings.RoundAvatar; }
            set { Settings.RoundAvatar = value; }
        }

        public bool BackgroundType
        {
            get { return Settings.BackgroundType; }
            set { Settings.BackgroundType = value; }
        }

        public bool MenuDivider
        {
            get { return Settings.MenuDivider; }
            set { Settings.MenuDivider = value; }
        }

        public bool CmdBarDivider
        {
            get { return Settings.CmdBarDivider; }
            set { Settings.CmdBarDivider = value; }
        }

        public bool AnimatedStickers
        {
            get { return Settings.AnimatedStickers; }
            set { Settings.AnimatedStickers = value; }
        }

        public bool DisableAcrylicHeader
        {
            get
            {
                return Settings.DisableAcrylicHeader;
            }
            set
            {
                Settings.DisableAcrylicHeader = value;
                ThemeManager.ApplyColors();

                if (CustomFrame.Instance.ActualTheme == ElementTheme.Dark)
                {
                    CustomFrame.Instance.RequestedTheme = ElementTheme.Light;
                    CustomFrame.Instance.RequestedTheme = ElementTheme.Dark;
                }
                else
                {
                    CustomFrame.Instance.RequestedTheme = ElementTheme.Dark;
                    CustomFrame.Instance.RequestedTheme = ElementTheme.Light;
                }
            }
        }

        public bool DisableAcrylicMenu
        {
            get
            {
                return Settings.DisableAcrylicMenu;
            }
            set
            {
                Settings.DisableAcrylicMenu = value;
                ThemeManager.ApplyColors();

                this.ApplyTheme();
            }
        }

        public int AccentColor
        {
            get { return Settings.AccentColor; }
            set
            {
                Settings.AccentColor = (byte)value;
                ThemeManager.ApplyColors();

                this.ApplyTheme();
            }
        }

        public bool UseAcrylicHost
        {
            get { return Settings.UseAcrylicHost; }
            set
            {
                Settings.UseAcrylicHost = value;
                ThemeManager.ApplyColors();

                this.ApplyTheme();
            }
        }

        private void ApplyTheme()
        {
            //ActualTheme - api contr v5
            if (CustomFrame.Instance.RequestedTheme == ElementTheme.Dark)
            {
                CustomFrame.Instance.RequestedTheme = ElementTheme.Default;
                CustomFrame.Instance.RequestedTheme = ElementTheme.Light;
                CustomFrame.Instance.RequestedTheme = ElementTheme.Dark;
            }
            else if(CustomFrame.Instance.RequestedTheme == ElementTheme.Default)
            {
                CustomFrame.Instance.RequestedTheme = ElementTheme.Dark;
                CustomFrame.Instance.RequestedTheme = ElementTheme.Light;
                CustomFrame.Instance.RequestedTheme = ElementTheme.Default;
            }
            else
            {
                CustomFrame.Instance.RequestedTheme = ElementTheme.Default;
                CustomFrame.Instance.RequestedTheme = ElementTheme.Dark;
                CustomFrame.Instance.RequestedTheme = ElementTheme.Light;
            }
        }

        public bool HideHeader
        {
            get { return Settings.HideHeader; }
            set { Settings.HideHeader = value; }
        }


        public string SaveFolderDoc
        {
            get
            {
                return Settings.SaveFolderDoc;
            }
            set
            {
                Settings.SaveFolderDoc = value;
                base.NotifyPropertyChanged(nameof(SaveFolderDoc));
            }
        }

        public string SaveFolderPhoto
        {
            get
            {
                return Settings.SaveFolderPhoto;
            }
            set
            {
                Settings.SaveFolderPhoto = value;
                base.NotifyPropertyChanged(nameof(SaveFolderPhoto));
            }
        }

        public string SaveFolderVoice
        {
            get
            {
                return Settings.SaveFolderVoice;
            }
            set
            {
                Settings.SaveFolderVoice = value;
                base.NotifyPropertyChanged(nameof(SaveFolderVoice));
            }
        }

        public string SaveFolderVideo
        {
            get
            {
                return Settings.SaveFolderVideo;
            }
            set
            {
                Settings.SaveFolderVideo = value;
                base.NotifyPropertyChanged(nameof(SaveFolderVideo));
            }
        }

        public bool CustomPopUpMenu
        {
            get
            {
                return Settings.CustomPopUpMenu;
            }
            set
            {
                Settings.CustomPopUpMenu = value;
                base.NotifyPropertyChanged(nameof(CustomPopUpMenu));
            }
        }

        public bool DEV_IsLogsEnabled
        {
            get { return Settings.DEV_IsLogsEnabled; }
            set
            {
                Settings.DEV_IsLogsEnabled = value;
                if (value == false)
                    Logger.Instance.DeleteLogFromIsolatedStorage();
                base.NotifyPropertyChanged(nameof(this.IsLogsAutoSendingAvaible));
            }
        }

        public bool DEV_IsLogsAutoSending
        {
            get { return Settings.DEV_IsLogsAutoSending; }
            set { Settings.DEV_IsLogsAutoSending = value; }
        }

        public bool DEV_IsLogsPopupEnabled
        {
            get { return Settings.DEV_IsLogsPopupEnabled; }
            set { Settings.DEV_IsLogsPopupEnabled = value; }
        }

        public bool IsLogsAutoSendingAvaible
        {
            get { return !this.DEV_IsLogsEnabled; }
        }

        public bool DEV_AddDebugButton
        {
            get { return Settings.DEV_AddDebugButton; }
            set { Settings.DEV_AddDebugButton = value; }
        }

        public byte EmojiType
        {
            get
            {
                return Settings.EmojiType;
            }
            set
            {
                Settings.EmojiType = value;
                base.NotifyPropertyChanged(nameof(EmojiType));
            }
        }

        public bool UI_SmallPreview
        {
            get { return Settings.UI_SmallPreview; }
            set { Settings.UI_SmallPreview = value; }
        }

        public bool UseProxy
        {
            get
            {
                return Settings.UseProxy;
            }
            set
            {
                Settings.UseProxy = value;
                base.NotifyPropertyChanged(nameof(UseProxy));
            }
        }

        public bool IsPhoneIntegrationEnabled
        {
            get { return Settings.SyncContacts; }
            set
            {
                if (value == Settings.SyncContacts)
                    return;
                Settings.SyncContacts = value;
                ContactsManager.Instance.EnsureInSyncAsync(true);
            }
        }

        public bool IsAllowUseLocation
        {
            get { return Settings.AllowUseLocation; }
            set { Settings.AllowUseLocation = value; }
        }

        public bool IsAllowSendContacts
        {
            get { return Settings.AllowSendContacts; }
            set
            {
                if (value == Settings.AllowSendContacts)
                    return;
                Settings.AllowSendContacts = value;
            }
        }

        public bool DEV_DisableMarkSeen
        {
            get { return Settings.DEV_DisableMarkSeen; }
            set { Settings.DEV_DisableMarkSeen = value; }
        }

        public bool DEV_SetOffline
        {
            get { return Settings.DEV_SetOffline; }
            set { Settings.DEV_SetOffline = value; }
        }

        public int DEV_BackgroundAnswerMethode
        {
            get { return Settings.DEV_BackgroundAnswerMethode; }
            set { Settings.DEV_BackgroundAnswerMethode = (byte)value; }
        }

        public bool UI_HideApplicationFrame
        {
            get { return Settings.UI_HideApplicationFrame; }
            set
            {
                if (value == Settings.UI_HideApplicationFrame)
                    return;
                Settings.UI_HideApplicationFrame = value;
                CustomFrame.Instance.Header.UpdateApplictionFrame();
            }
        }
        /*
        public short UI_FontWeight
        {
            get { return Settings.UI_FontWeight; }
            set
            {
                if (value == Settings.UI_FontWeight)
                    return;
                Settings.UI_FontWeight = value;
                base.NotifyPropertyChanged(nameof(this.FontWeight));
            }
        }
        
        public Windows.UI.Text.FontWeight FontWeight
        {
            get { return new Windows.UI.Text.FontWeight() { Weight = (ushort)Settings.UI_FontWeight }; }
        }
        */



        public bool CompressPhotosOnUpload
        {
            get { return Settings.CompressPhotosOnUpload; }
            set
            {
                if (value == Settings.CompressPhotosOnUpload)
                    return;
                Settings.CompressPhotosOnUpload = value;
            }
        }

        public bool SaveLocationOnUpload
        {
            get { return Settings.SaveLocationOnUpload; }
            set
            {
                if (value == Settings.SaveLocationOnUpload)
                    return;
                Settings.SaveLocationOnUpload = value;
            }
        }

        public bool SaveEditedPhotos
        {
            get { return Settings.SaveEditedPhotos; }
            set
            {
                if (value == Settings.SaveEditedPhotos)
                    return;
                Settings.SaveEditedPhotos = value;
            }
        }

        public bool LoadBigPhotosOverMobile
        {
            get { return Settings.LoadBigPhotosOverMobile; }
            set
            {
                if (value == Settings.LoadBigPhotosOverMobile)
                    return;
                Settings.LoadBigPhotosOverMobile = value;
            }
        }

        public Visibility VisibleOnDevice
        {
            get { return (!CustomFrame.Instance.IsDevicePhone).ToVisiblity(); }
        }

        public List<string> Languages
        {
            get
            {
                List<string> list = new List<string>();
                list.Add("System");

                foreach(string lang in Windows.System.UserProfile.GlobalizationPreferences.Languages)
                {
                    CultureInfo temp = new CultureInfo(lang);
                    list.Add(temp.DisplayName);
                }
                
                return list;
            }
        }

        public int LanguageSettings
        {
            get { return Settings.LanguageSettings; }
            set
            {
                Settings.LanguageSettings = (byte)value;
                if (value == 0)
                    ApplicationLanguages.PrimaryLanguageOverride = "";
                else
                {
                    ApplicationLanguages.PrimaryLanguageOverride = Windows.System.UserProfile.GlobalizationPreferences.Languages[value - 1];
                    //var temp = new CultureInfo(Windows.System.UserProfile.GlobalizationPreferences.Languages[0]);
                    //int i = 0;
                }

                //Window.Current.Content = null;
                //Window.Current.Content = CustomFrame.Instance;

                
                //Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().Reset();
                //Windows.ApplicationModel.Resources.Core.ResourceContext.GetForViewIndependentUse().Reset();
            }
        }
    }
}
