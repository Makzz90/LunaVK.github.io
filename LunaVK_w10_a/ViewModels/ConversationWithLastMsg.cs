using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using LunaVK.Framework;
using LunaVK.Library;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace LunaVK.ViewModels
{
    public class ConversationWithLastMsg : ViewModelBase, IBinarySerializable, UsersTypingHelper.ISupportUsersTyping
    {
        public VKConversation conversation { get; set; }
        public VKMessage last_message { get; set; }

        /*
        public DialogHistoryViewModel _historyVM;
        
        private object _instLock = new object();

        
        public DialogHistoryViewModel HistoryVM
        {
            get
            {
                if (this._historyVM == null)
                {
                    lock (this._instLock)
                    {
                        int peerId = this.conversation.peer.id;
                        DialogHistoryViewModel vm = new DialogHistoryViewModel(peerId);
                        CacheManager.TryDeserialize(vm, "Dialog_" + peerId);

                        //if (vm == null)
                        //    vm = new DialogHistoryViewModel(peerId);
                        this._historyVM = vm;
                    }
                }

                return this._historyVM;
            }
        }
        */




#region UserTypping
        public UsersTypingHelper UsersTypingHelper;

        public void SetUserIsTypingWithDelayedReset(int userId)
        {
            if (this.UsersTypingHelper == null)
                this.UsersTypingHelper = new UsersTypingHelper(this);
            this.UsersTypingHelper.SetUserIsTypingWithDelayedReset(userId);
        }

        public void SetUserIsNotTyping(int userId)
        {
            if (this.UsersTypingHelper != null)
                this.UsersTypingHelper.SetUserIsNotTyping(userId);
        }

        public void UpdateTypingInUI()
        {
            base.NotifyPropertyChanged(nameof(this.UIBody));
            base.NotifyPropertyChanged(nameof(this.SomeAttachments));//закрашиваем тело в синий цвет
            base.NotifyPropertyChanged(nameof(this.UserThumbVisibility));//скрываем аватарку
//            base.NotifyPropertyChanged(nameof(this.TypingStr));
//
//
            base.NotifyPropertyChanged(nameof(this.UISubtitle));
        }
#endregion

        public void UpdateOn()
        {
            base.NotifyPropertyChanged(nameof(this.ConversationAvatarVM));
        }

#region VM
        public ConversationAvatarViewModel ConversationAvatarVM
        {
            get
            {
                ConversationAvatarViewModel temp_vm = new ConversationAvatarViewModel();
                
                if (conversation.peer.type == VKConversationPeerType.Chat)
                {
                    if (conversation.chat_settings.state == "kicked" || conversation.chat_settings.state == "left")
                    {
                        temp_vm.Images.Add(VKConstants.AVATAR_COMMUNITY + "_100.png");
                    }
                    else
                    {
                        if (conversation.chat_settings.photo != null)
                        {
                            temp_vm.Images.Add(conversation.chat_settings.photo.photo_100);
                        }
                        else
                        {
                            if (conversation.chat_settings.active_ids.Count > 0)
                            {
                                if(conversation.chat_settings.active_ids.Count==1 && conversation.chat_settings.active_ids[0] == Settings.UserId)
                                {
                                    temp_vm.Images.Add(VKConstants.AVATR_MULTICHAT + "_100.png");
                                }
                                else
                                {
                                    var user2 = UsersService.Instance.GetCachedUser((uint)conversation.chat_settings.active_ids[0]);
                                    temp_vm.Images.Add(user2.MinPhoto);
                                }
                                
                            }

                            if (conversation.chat_settings.active_ids.Count > 1)
                            {
                                var user3 = UsersService.Instance.GetCachedUser((uint)conversation.chat_settings.active_ids[1]);
                                temp_vm.Images.Add(user3.MinPhoto);
                            }

                            if (conversation.chat_settings.active_ids.Count > 2)
                            {
                                var user4 = UsersService.Instance.GetCachedUser((uint)conversation.chat_settings.active_ids[2]);
                                temp_vm.Images.Add(user4.MinPhoto);
                            }

                            if (conversation.chat_settings.active_ids.Count > 3)
                            {
                                var user5 = UsersService.Instance.GetCachedUser((uint)conversation.chat_settings.active_ids[3]);
                                temp_vm.Images.Add(user5.MinPhoto);
                            }
                        }
                    }
                }
                else if (conversation.peer.type == VKConversationPeerType.User)
                {
                    var owner = UsersService.Instance.GetCachedUser((uint)conversation.peer.id);
                    if (owner == null)
                    {
                        this.RefreshHeader();
                        return null;
                    }

                    temp_vm.Online = owner.online;
                    temp_vm.online_app = owner.online_app;

                    if (owner.last_seen != null)
                        temp_vm.platform = owner.last_seen.platform;

                    if (owner == null)
                    {
                        //todo: strange bug
                        int debugme = 0;
                    }
                    else
                    {
                        if (owner.Deactivated == VKIsDeactivated.None)
                        {
                            if(string.IsNullOrEmpty(owner.MinPhoto))
                                temp_vm.Images.Add(VKConstants.AVATAR_CAMERA + "_100.png");
                            else
                                temp_vm.Images.Add(owner.MinPhoto);
                            //Debug.Assert(string.IsNullOrEmpty(owner.MinPhoto) == false);
                        }
                        else
                        {
                            temp_vm.Images.Add(VKConstants.AVATAR_DEACTIVATED + "_100.png");
                        }
                    }
                }
                else if (conversation.peer.type == VKConversationPeerType.Group)
                {
                    var owner = GroupsService.Instance.GetCachedGroup((uint)(-conversation.peer.id));
                    if (owner == null || string.IsNullOrEmpty(owner.MinPhoto))
                    {
                        this.RefreshHeader();
                        return null;
                    }
                    
                    temp_vm.Images.Add(owner.MinPhoto);
                }
                //
                Debug.Assert(string.IsNullOrEmpty(temp_vm.Images[0]) == false);
                //
                return temp_vm;
            }
        }

        private void RefreshHeader()
        {
            if(this.conversation.peer.type == VKConversationPeerType.User)
            {
                UsersService.Instance.GetUsers(new List<uint>() { (uint)this.conversation.peer.id }, (result) =>
                {
                    if (result != null)
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            base.NotifyPropertyChanged(nameof(this.ConversationAvatarVM));
                            base.NotifyPropertyChanged(nameof(this.Title));
                            base.NotifyPropertyChanged(nameof(this.UISubtitle));
                            base.NotifyPropertyChanged(nameof(this.UserVerified));
                        });
                    }
                });
            }
            else if(this.conversation.peer.type == VKConversationPeerType.Group)
            {
                GroupsService.Instance.GetCommunity((uint)(-conversation.peer.id), "name,photo_50,photo_100", (result) =>
                {
                    if(result!=null)
                    {
                        GroupsService.Instance.SetCachedGroups(new List<VKGroup>() { result });
                        Execute.ExecuteOnUIThread(() =>
                        {
                            base.NotifyPropertyChanged(nameof(this.ConversationAvatarVM));
                            base.NotifyPropertyChanged(nameof(this.Title));
                            base.NotifyPropertyChanged(nameof(this.UISubtitle));
                            base.NotifyPropertyChanged(nameof(this.UserVerified));
                        });
                    }
                });
            }
            //todo: chat
        }

        private string _uibody = "";
        public string UIBody
        {
            get
            {
                if (this.UsersTypingHelper != null && this.UsersTypingHelper.AnyTypingNow)
                    return this.UsersTypingHelper.TypingString;
                //if (!string.IsNullOrEmpty(this.TypingStr))
                //    return this.TypingStr;
                return this._uibody;
            }
            set
            {
                this._uibody = value;
                base.NotifyPropertyChanged(nameof(this.UIBody));
                base.NotifyPropertyChanged(nameof(this.SomeAttachments));
            }
        }

        public bool SomeAttachments
        {
            get
            {
                if (this.UsersTypingHelper != null && this.UsersTypingHelper.AnyTypingNow)//if (!string.IsNullOrEmpty(this.TypingStr))
                    return true;
                if (!string.IsNullOrEmpty(this.last_message.text))
                    return false;
                return (this.last_message.attachments != null && this.last_message.attachments.Count > 0) || (this.last_message.fwd_messages != null && this.last_message.fwd_messages.Count > 0);
            }
        }

        /// <summary>
        /// Собеседник это администрация ВК?
        /// </summary>
        public bool IsAdminId
        {
            get { return this.conversation.peer.local_id == 100; }
        }

        public string Title
        {
            get
            {
                if (this.conversation.peer.type == VKConversationPeerType.Chat)
                {
                    return this.conversation.chat_settings.title;
                }
                else if (conversation.peer.type == VKConversationPeerType.User)
                {
                    var user = UsersService.Instance.GetCachedUser((uint)conversation.peer.id);
                    if (user != null)
                        return user.Title;
                }
                else if (conversation.peer.type == VKConversationPeerType.Group)
                {
                    var group = GroupsService.Instance.GetCachedGroup((uint)(-conversation.peer.id));
                    if (group != null)
                        return group.Title;
                }
                return "";
            }
        }

        /// <summary>
        /// Показать ли фон для нового сообщения
        /// </summary>
        public Visibility DialogBackgroundVisibility
        {
            get
            {
                if (this.last_message.@out == VKMessageType.Received && (this.conversation.in_read < this.last_message.id || this.conversation.unread_count > 0))
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public bool UserVerified
        {
            get
            {
                if (this.conversation.peer.type == VKConversationPeerType.Chat)
                {
                    return false;
                }
                else if (conversation.peer.type == VKConversationPeerType.User)
                {
                    var user = UsersService.Instance.GetCachedUser((uint)conversation.peer.id);
                    if (user != null)
                        return user.IsVerified;
                }
                else if (conversation.peer.type == VKConversationPeerType.Group)
                {
                    var group = GroupsService.Instance.GetCachedGroup((uint)(-conversation.peer.id));
                    if (group != null)
                        return group.IsVerified;
                }
                return false;
            }
        }

        public void UpdateUI()
        {
            base.NotifyPropertyChanged(nameof(this.AreNotificationsDisabled));
        }

        /// <summary>
        /// Уведомления у диалога отключены?
        /// </summary>
        public bool AreNotificationsDisabled
        {
            get { return this.conversation.AreDisabledNow; }
        }

        /// <summary>
        /// Мини-аватарка слева от текста внизу
        /// </summary>
        public Visibility UserThumbVisibility
        {
            get
            {
                if (this.conversation.peer.type != VKConversationPeerType.Chat && this.last_message.@out != VKMessageType.Sent || (this.UsersTypingHelper != null && this.UsersTypingHelper.AnyTypingNow))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility BodyTextBackgroundVisibility
        {
            get
            {
                return (this.conversation.out_read >= this.last_message.id || this.last_message.@out == VKMessageType.Received) ? Visibility.Collapsed : Visibility.Visible;//было ==
            }
        }

        public Visibility CounterVisibility
        {
            get
            {
                //return this.conversation.in_read != this.last_message.id ? Visibility.Visible : Visibility.Collapsed;
                return this.conversation.unread_count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string Unread
        {
            get
            {
                if (this.conversation.unread_count > 99)
                    return "99+";
                return this.conversation.unread_count.ToString();
            }
        }

        public void RefreshUIProperties()
        {
            //            this.NotifyPropertyChanged("MainBackgroundBrush");
            //            this.NotifyPropertyChanged("TextBackgroundBrush");
            base.NotifyPropertyChanged(nameof(this.Unread));
            base.NotifyPropertyChanged(nameof(this.UserThumbVisibility));
            base.NotifyPropertyChanged(nameof(this.CounterVisibility));
            base.NotifyPropertyChanged(nameof(this.UserThumb));
            base.NotifyPropertyChanged(nameof(this.DialogBackgroundVisibility));
            base.NotifyPropertyChanged(nameof(this.BodyTextBackgroundVisibility));
            base.NotifyPropertyChanged(nameof(this.last_message));
            //base.NotifyPropertyChanged(nameof(this.UIBody));
            base.NotifyPropertyChanged(nameof(this.AreNotificationsDisabled));
        }

        private Visibility _isCurrentDialogVisibility = Visibility.Collapsed;
        public Visibility IsCurrentDialogVisibility
        {
            get { return this._isCurrentDialogVisibility; }
            set
            {
                this._isCurrentDialogVisibility = value;
                this.NotifyPropertyChanged("IsCurrentDialogVisibility");
            }
        }

        public string UserThumb
        {
            get
            {
                if (this.last_message.@out == VKMessageType.Sent)
                {
                    return Settings.LoggedInUserPhoto;
                }
                if (this.conversation.peer.type == VKConversationPeerType.Chat)
                {
                    var author = UsersService.Instance.GetCachedUser((uint)this.last_message.from_id);

                    if (author != null)
                        return author.MinPhoto;
                }
                return null;
            }
        }


        /*
        public SolidColorBrush HaveUnreadMessagesBackground
        {
            get { return this.AreNotificationsDisabled ? new SolidColorBrush(Windows.UI.Colors.Gray) : (SolidColorBrush)Application.Current.Resources["AccentBrushHigh"]; }
        }
        */
        public bool IsChat { get { return this.conversation.peer.type == VKConversationPeerType.Chat; } }

        /*
        private string _typingStr;
        public string TypingStr
        {
            get
            {
                return this._typingStr;
            }
            set
            {
                this._typingStr = value;
                base.NotifyPropertyChanged("SomeAttachments");//закрашиваем тело в синий цвет
                this.NotifyPropertyChanged("UserThumbVisibility");//скрываем аватарку
                base.NotifyPropertyChanged("UIBody");
                base.NotifyPropertyChanged("TypingStr");
            }
        }
        */
        #endregion

        public void Write(BinaryWriter writer)
        {
            //this._typingStr = "";

            writer.Write(1);
            writer.Write<VKMessage>(this.last_message);
            writer.Write<VKConversation>(this.conversation);
            writer.WriteString(this.UIBody);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.last_message = reader.ReadGeneric<VKMessage>();
            this.conversation = reader.ReadGeneric<VKConversation>();
            this.UIBody = reader.ReadString();

            this.RefreshUIProperties();
        }


















        public string UISubtitle
        {
            get
            {
                if (this.UsersTypingHelper != null && this.UsersTypingHelper.AnyTypingNow)
                    return this.UsersTypingHelper.TypingString;




                if (this.conversation.peer.type == VKConversationPeerType.Chat)
                {
                    //                    this.UITitle = conversation.chat_settings.title;
                    string tempStr = UIStringFormatterHelper.FormatNumberOfSomething((int)this.conversation.chat_settings.members_count, "Conversation_OnePerson", "Conversation_TwoToFourPersonsFrm", "Conversation_FiveOrMorePersionsFrm");
                    return tempStr;
                }
                else if (this.conversation.peer.type == VKConversationPeerType.Group || this.conversation.peer.id < 0)//странный баг, бывает юзер с отрицательным пиром
                {
                    return LocalizedStrings.GetString("Community");
                    //                    var group = GroupsService.Instance.GetCachedGroup((uint)(-conversation.peer.id));
                    //                    this.UITitle = group.Title;
                }
                else if (this.conversation.peer.type == VKConversationPeerType.User)
                {
                    VKUser user = UsersService.Instance.GetCachedUser((uint)this.conversation.peer.id);
                    //user будет пустым, если мы пишем неизвестному нам человеку
                    if (user != null && user.deactivated == VKIsDeactivated.None)
                    {
                        if (user.last_seen == null)
                            return "";//user.last_seen = new VKLastSeen();
                        //user.last_seen.Online = user.online;
                        //user.last_seen.OnlineApp = user.online_app;
                        return user.GetUserStatusString();
                    }
                    else
                    {
                        return "";//todo: написать причину блокировки?
                    }
                }

                return "";
            }
        }

        public Visibility OptionsVisibility
        {
            get
            {
                return string.IsNullOrEmpty(this.Title) ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
}
