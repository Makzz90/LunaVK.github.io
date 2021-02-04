using System;
using System.Collections.Generic;
using System.Text;
using LunaVK.Core.Network;
using LunaVK.Core.DataObjects;
using Newtonsoft.Json;
using LunaVK.Core.Json;
using System.Collections.ObjectModel;
using LunaVK.Core.ViewModels;
using Windows.UI.Xaml;
using LunaVK.Core;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using System.Diagnostics;
using LunaVK.Framework;
using LunaVK.Library;

namespace LunaVK.ViewModels
{
    public class ChatEditViewModel : ViewModelBase
    {
        public ObservableCollection<ChatInfo.ChatMember> Members { get; set; }
        public int ChatId;
        private ChatInfo.Chat _chatInformation;

        public string Title
        {
            get
            {
                return _chatInformation != null ? _chatInformation.title : "";
            }
        }

        public bool IsPhotoMenuEnabled
        {
            get
            {
                if (_chatInformation != null)
                {

                    return !string.IsNullOrEmpty(_chatInformation.photo_100);
                }

                return false;

            }
        }

        public bool IsNotificationsSoundEnabled
        {
            get
            {
                if (this._chatInformation != null)
                {
                    return this._chatInformation.AreDisabledNow == false;
                }

                return false;
            }
        }

        public ConversationAvatarViewModel ConversationAvatarVM
        {
            get
            {
                if (this._chatInformation == null)
                    return null;

                ConversationAvatarViewModel temp_vm = new ConversationAvatarViewModel();
                
                    if (this._chatInformation.kicked || this._chatInformation.left)
                    {
                        temp_vm.Images.Add(VKConstants.AVATAR_COMMUNITY + "_100.png");
                    }
                    else
                    {
                    
                        if (!string.IsNullOrEmpty(this._chatInformation.photo_100))
                        {
                            temp_vm.Images.Add(this._chatInformation.photo_100);
                        }
                        else
                        {
                            if (this.Members.Count > 0)
                            {
                                if (this.Members.Count == 1 && this.Members[0].id == Settings.UserId)
                                {
                                    temp_vm.Images.Add(VKConstants.AVATR_MULTICHAT + "_100.png");
                                }
                                else
                                {
                                    var user2 = UsersService.Instance.GetCachedUser((uint)this.Members[0].id);
                                    temp_vm.Images.Add(user2.MinPhoto);
                                }

                            }

                            if (this.Members.Count > 1)
                            {
                                var user3 = UsersService.Instance.GetCachedUser((uint)this.Members[1].id);
                                temp_vm.Images.Add(user3.MinPhoto);
                            }

                            if (this.Members.Count > 2)
                            {
                                var user4 = UsersService.Instance.GetCachedUser((uint)this.Members[2].id);
                                temp_vm.Images.Add(user4.MinPhoto);
                            }

                            if (this.Members.Count > 3)
                            {
                                var user5 = UsersService.Instance.GetCachedUser((uint)this.Members[3].id);
                                temp_vm.Images.Add(user5.MinPhoto);
                            }
                        }
                    
                }

                //
                Debug.Assert(string.IsNullOrEmpty(temp_vm.Images[0]) == false);
                //
                return temp_vm;
            }
        }

        public ChatEditViewModel(int chat_id)
        {
            this.ChatId = chat_id;
            this.Members = new ObservableCollection<ChatInfo.ChatMember>();
        }

        public void LoadData()
        {
            ChatService.Instance.GetChatInfo(this.ChatId, (result) => {
                if (result.error.error_code == Core.Enums.VKErrors.None)
                {
                    this._chatInformation = result.response.chat;
                    Execute.ExecuteOnUIThread(() =>
                    {
                        base.NotifyPropertyChanged(nameof(this.Title));
                        base.NotifyPropertyChanged(nameof(this.IsNotificationsSoundEnabled));



                        int chatCreatorId = int.Parse(this._chatInformation.admin_id);

                        foreach (int memberId in result.response.chat.users)
                        {
                            var member = result.response.chat_participants.Find((u)=>u.id == memberId);

                            VKBaseDataForGroupOrUser invitedByUser = result.response.invited_by_users.Find((p) => p.id == member.invited_by);
                            //this.Members.Add(new ChatMember(member, invitedByUser, chatCreatorId));
                            member._invitedByUser = (VKUser)invitedByUser;
                            member._chatCreatorId = chatCreatorId;

                            this.Members.Add(member);
                        }

                        UsersService.Instance.SetCachedUsers(this.Members);
                        base.NotifyPropertyChanged(nameof(this.ConversationAvatarVM));
                    });
                }

            });


        }

        public void ExcludeMember(ChatInfo.ChatMember member)
        {
            /*
            this.SetInProgress(true, "");
            member.ExcludeButtonVisibility = Visibility.Collapsed;
            IChatService chatService = BackendServices.ChatService;
            long chatId = this._chatId;
            List<long> usersToBeRemoved = new List<long>();
            usersToBeRemoved.Add(member.Id);
            Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback = (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (result.ResultCode == ResultCode.Succeeded)
                {*/
            //string format = "API.messages.removeChatUser({{\"chat_id\":{0}, \"user_id\":{1} }});";
                    this.Members.Remove(member);
                /*}
                else
                {
                    member.ExcludeButtonVisibility = Visibility.Visible;
                    GenericInfoUC.ShowBasedOnResult(result.ResultCode, "", (VKRequestsDispatcher.Error)null);
                }
                this.SetInProgress(false, "");
            })));
            chatService.RemoveChatUsers(chatId, usersToBeRemoved, callback);*/
        }

        private bool IsChatLeaving;
        private bool IsNotificationsSoundModeSwitching;

        public void LeaveChat()
        {
            if (IsChatLeaving)
                return;

            this.IsChatLeaving = true;
            //this.SetInProgress(true, "");
            //IChatService chatService = BackendServices.ChatService;
            //long chatId = this._chatId;
            //List<long> usersToBeRemoved = new List<long>();
            //usersToBeRemoved.Add(AppGlobalStateManager.Current.LoggedInUserId);
            
            ChatService.Instance.RemoveChatUser(this.ChatId, (int)Settings.UserId, (result) => 
            {
                if (result.error.error_code == Core.Enums.VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() => {
                        CustomFrame.Instance.GoBack();
                    });

                    //this._navigationService.RemoveBackEntrySafe();
                    //this._navigationService.GoBackSafe();
                }
                //else
                //    GenericInfoUC.ShowBasedOnResult(result.ResultCode, "", (VKRequestsDispatcher.Error)null);
                //this.SetInProgress(false, "");
                this.IsChatLeaving = false;
            });
        }

        public void SwitchNotificationsSoundMode()
        {
            if (IsNotificationsSoundModeSwitching)
                return;

            this.IsNotificationsSoundModeSwitching = true;
            //this.SetInProgress(true, "");
            AccountService.Instance.SetSilenceMode(PushNotifications.Instance.GetHardwareID, this.IsNotificationsSoundEnabled ? -1 : 0, (result) => 
            {
                if (result.error.error_code == Core.Enums.VKErrors.None)
                {
                    
                    /*
                    if (this._chatInformation.push_settings == null)
                        this._chatInformation.push_settings = new VKPushSettings();

                    this._chatInformation.push_settings.sound = !this._chatInformation.push_settings.sound;//this._chatInformation.push_settings.disabled_until = this.IsNotificationsSoundEnabled ? -1 : 0;
                    */
                    if(this.IsNotificationsSoundEnabled==false)
                    {
                        this._chatInformation.push_settings = null;
                    }
                    else
                    {
                        this._chatInformation.push_settings = new VKPushSettings();
                        this._chatInformation.push_settings.disabled_forever = true;
                    }

                    Execute.ExecuteOnUIThread(() =>
                    {
                        base.NotifyPropertyChanged(nameof(this.IsNotificationsSoundEnabled));
                    });
                }
                //else
                //    GenericInfoUC.ShowBasedOnResult(result.ResultCode, "", (VKRequestsDispatcher.Error)null);
                //this.SetInProgress(false, "");
                this.IsNotificationsSoundModeSwitching = false;
            }, 2000000000 + this.ChatId);
        }

        public void ChangeTitle(string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle) || newTitle.Length < 2)
            {
                //errorAction();
            }
            else
            {
                //this.SetInProgress(true, "");
                //this.IsTitleBoxEnabled = false;
                ChatService.Instance.EditChat(this.ChatId, newTitle, (result) => 
                {
                    if (result.error.error_code == Core.Enums.VKErrors.None)
                    {
                        Execute.ExecuteOnUIThread(() =>
                        { 
                            this._chatInformation.title = newTitle;
                            base.NotifyPropertyChanged(nameof(this.Title));
                        });
                    }
                    else
                    {
                        //errorAction();
                        //GenericInfoUC.ShowBasedOnResult(result.ResultCode, "", (VKRequestsDispatcher.Error)null);
                    }
                    //this.SetInProgress(false, "");
                    //this.IsTitleBoxEnabled = true;
                });
            }
        }

        private bool IsPhotoChanging;

        public void DeletePhoto()
        {
            if (IsPhotoChanging)
                return;
            this.IsPhotoChanging = true;
            //this.SetInProgress(true, "");
            MessagesService.Instance.DeleteChatPhoto(this.ChatId, (result) => 
            {
                if (result.error.error_code == Core.Enums.VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        this._chatInformation.photo_100 = null;
                        base.NotifyPropertyChanged(nameof(this.ConversationAvatarVM));
                        //this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.PhotoPlaceholderVisibility));
                        //this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.IsPhotoMenuEnabled));
                    });
                }
                //else
                //    GenericInfoUC.ShowBasedOnResult(result.ResultCode, "", (VKRequestsDispatcher.Error)null);
                //this.SetInProgress(false, "");
                this.IsPhotoChanging = false;
            });
        }
        /*
        public class ChatInfo
        {
            public Chat chat { get; set; }

            public ChatLink chat_link { get; set; }

            /// <summary>
            /// Объект, описывающий чат
            /// </summary>
            public class Chat
            {
                /// <summary>
                /// Идентификатор беседы
                /// </summary>
                public int id { get; set; }

                /// <summary>
                /// Тип диалога
                /// chat
                /// </summary>
                public string type { get; set; }

                /// <summary>
                /// Название беседы
                /// </summary>
                public string title { get; set; }

                /// <summary>
                /// Идентификатор создателя беседы
                /// </summary>
                public string admin_id { get; set; }

                /// <summary>
                /// Список участников беседы
                /// </summary>
                public List<ChatMember> users { get; set; }

                public int members_count { get; set; }//no in documentation

                /// <summary>
                /// Настройки оповещения для беседы
                /// </summary>
                public VKPushSettings push_settings { get; set; }

                /// <summary>
                /// Обложка
                /// </summary>
                public string photo_50 { get; set; }
                public string photo_100 { get; set; }
                public string photo_200 { get; set; }

                /// <summary>
                /// Пользователь покинул беседу?
                /// </summary>
                [JsonConverter(typeof(VKBooleanConverter))]
                public bool left { get; set; }

                /// <summary>
                /// Пользователя выкинули из беседы?
                /// </summary>
                [JsonConverter(typeof(VKBooleanConverter))]
                public bool kicked { get; set; }
            }

            public class ChatLink
            {
                public string link { get; set; }
            }
        }

        public class ChatMember : VKUser
        {
            public int invited_by { get; set; }

            public ChatMember()
            {

            }

            public ChatMember(VKUser user, VKBaseDataForGroupOrUser invitedByUser, long chatCreatorId)
            {
                //this._user = user;
                this._invitedByUser = invitedByUser;
                this._chatCreatorId = chatCreatorId;
                long loggedInUserId = Settings.UserId;
            }

#region VM
            public Visibility ExcludeButtonVisibility
            {
                get
                {
                    long loggedInUserId = Settings.UserId;
                    if(this.id == loggedInUserId)
                        return Visibility.Collapsed;

                    //return this._chatCreatorId != loggedInUserId && this._invitedByUser.id != loggedInUserId || this.id == loggedInUserId ? Visibility.Collapsed : Visibility.Visible;
                    return this._chatCreatorId == loggedInUserId || this._invitedByUser.Id == loggedInUserId ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            //public VKProfileBase _user;
            public VKBaseDataForGroupOrUser _invitedByUser;

            public long _chatCreatorId;
            
            //public string Name
            //{
            //    get
            //    {
            //        return this._user.Title;
            //    }
            //}

            //public string Photo
            //{
            //    get
            //    {
            //        return this._user.photo_100;
            //    }
            //}
            
            public string Information
            {
                get
                {
                    if (this.id == this._chatCreatorId)
                        return "Создатель беседы";//CommonResources.ChatCreator.ToLower();
                    return "";// string.Format((this._invitedByUser.sex != 1 ? CommonResources.InvitedToChatBy_M : CommonResources.InvitedToChatBy_F).ToLower(), this._invitedByUser.Name);
                }
            }

            //private Visibility _excludeButtonVisibility;
            //public Visibility ExcludeButtonVisibility { get; set; }
#endregion
        }
        */
    }
}
