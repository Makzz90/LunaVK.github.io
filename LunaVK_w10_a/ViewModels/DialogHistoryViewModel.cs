using LunaVK.Library;
using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using System.Text.RegularExpressions;
using LunaVK.Core.Framework;
using System.Diagnostics;
using System.IO;
using Windows.Data.Xml.Dom;
using System.Text;

namespace LunaVK.ViewModels
{
    /// <summary>
    /// ConversationViewModel
    /// </summary>
    public class DialogHistoryViewModel : ViewModelBase, ISupportDownIncrementalLoading, ISupportUpIncrementalLoading, IBinarySerializable//, UsersTypingHelper.ISupportUsersTyping
    {
        public static Func<VKMessage, VKMessage, int> _comparisonFunc = ((m1, m2) =>
        {
            if (m2.id > 0 && m1.id > 0)
                return (int)(m2.id - m1.id);
            return (int)(m2.date - m1.date).TotalMilliseconds;
        });

        /// <summary>
        /// Сообщения
        /// </summary>
        public ObservableCollection<VKMessage> Items { get; private set; }
        public ObservableCollection<IOutboundAttachment> Attachments { get; set; }

        private DateTime _lastTimeUserIsTypingWasCalled = DateTime.MinValue;


//        private uint _maximum = 0;
        public uint? _totalCount;

        public int PeerId;

        public IScroll _scroll;

        public ObservableGroupingCollection<VKMessage> GroupedItems { get; private set; }

        /// <summary>
        /// Количество пропущенных элементов (если применимо).
        /// </summary>
        private uint _skipped;

        public ProfileLoadingStatus CurrentLoadingStatus { get; private set; }
        public Action<ProfileLoadingStatus> LoadingStatusUpdated { get; set; }

        /// <summary>
        /// 15
        /// </summary>
        private readonly int _numberOfMessagesToStore = 12;

        private uint? group_id;

        private void UpdateLoadingStatus(ProfileLoadingStatus status)
        {
            this.CurrentLoadingStatus = status;
            Execute.ExecuteOnUIThread(() => { this.LoadingStatusUpdated?.Invoke(status); });
        }

        public bool HasMoreDownItems
        {
            get
            {
                if (!this._totalCount.HasValue)
                    return this.Items.Count == 0;

                //int count = this.Items.Count((i) => { return i.action == null || (i.action != null && i.action.type != VKChatMessageActionType.UNREAD_ITEM_ACTION); });
                //return count < this._totalCount.Value;
                return this._startMessageId.HasValue;
            }
        }

        public bool HasMoreUpItems
        {
            get
            {
                if (!this._totalCount.HasValue)
                    return false;

                int count = this.Items.Count((i) => { return i.action == null || (i.action != null && i.action.type != VKChatMessageActionType.UNREAD_ITEM_ACTION); });
                return count < this._totalCount.Value;
                //if (this._skipped <= 0 || this.Items.Count <= 0)
                //    return false;
                //return true;
            }
        }

        public DialogHistoryViewModel(int peerId, uint? groupId=null) : this()
        {
            this.PeerId = peerId;
            this.group_id = groupId;

            if (this.IsChat)
                this.ChatMembers = new List<VKBaseDataForGroupOrUser>();
        }

        public DialogHistoryViewModel()
        {
            this.Items = new ObservableCollection<VKMessage>();
            //
            //if (this.GroupedItems != null)
            //    this.GroupedItems.Dispose();
            
            //this.GroupedItems = null;
            //
            this.GroupedItems = new ObservableGroupingCollection<VKMessage>(this.Items);

            this.Attachments = new ObservableCollection<IOutboundAttachment>();
            this.BotKeyboardButtons = new ObservableCollection<List<VKBotKeyboard.KeyboardButton>>();

            this.Items.CollectionChanged += this.Items_CollectionChanged;
        }
        
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
             // Эта функция нужна лишь для скрытия "Нет содержимого" в контейнере сообщений после добавления сообщения
             
            if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (this.CurrentLoadingStatus == ProfileLoadingStatus.Empty)
                {
                    this.UpdateLoadingStatus(ProfileLoadingStatus.Loaded); 
                }

                this.Items.CollectionChanged -= this.Items_CollectionChanged;
            }
        }
    
        public void Save()
        {
            if(this.Items.Count>0 && this._startMessageId == null)
                CacheManager.TrySerialize(this, "Dialog_" + this.PeerId);
        }

        /// <summary>
        /// Мы в начале списка сообщений?
        /// </summary>
        public bool IsAtBottom = true;

        private VKPinnedMessage _pinnedMessageVM;
        public VKPinnedMessage PinnedMessageVM
        {
            get
            {
                return this._pinnedMessageVM;
            }
            set
            {
                if (this._pinnedMessageVM == value)
                    return;
                this._pinnedMessageVM = value;
                base.NotifyPropertyChanged(nameof(this.PinnedMessageVM));
            }
        }

        public void UpdateOn2()
        {
            base.NotifyPropertyChanged(nameof(this.ConversationAvatarVM));
        }

        public ConversationAvatarViewModel ConversationAvatarVM { get; set; }
        //object itemToScroll = null;

        public int? _startMessageId;

        /// <summary>
        /// Обновляем бот-кнопки из лонгпула
        /// </summary>
        /// <param name="json"></param>
        public void UpdateButtons(string json)
        {
            this.BotKeyboardButtons.Clear();

            
            int pos = json.IndexOf("\"buttons");

            if (pos > 0)
            {
                json = json.Substring(pos);

                var builder = new StringBuilder();
                int opened = -1;
                foreach(var s in json)
                {
                    if(s=='[')
                    {
                        if (opened == -1)
                            opened = 0;

                        opened++;
                    }
                    else if(s==']')
                    {
                        opened--;
                    }
                    
                    builder.Append(s);

                    if (opened==0)
                    {
                        break;
                    }

                    
                }

                json = builder.ToString();

                Regex QueryStringRegex = new Regex(@"""buttons"":(.+|\n)+", RegexOptions.Singleline);

                var match = QueryStringRegex.Match(json);
                if (match.Success)
                {
                    var str = match.Groups[1].Value;
                    var buttons = Newtonsoft.Json.JsonConvert.DeserializeObject<List<List<VKBotKeyboard.KeyboardButton>>>(str);

                    foreach (var button in buttons)
                    {
                        this.BotKeyboardButtons.Add(button);
                    }

                    this.UpdateBotKeyboardVisibility();
                }
            }
        }

        public void Reload()
        {
            this.LoadDownAsync(true);
        }

        public void LoadUpAsync()
        {
            this.UpdateLoadingStatus(ProfileLoadingStatus.Loading);
            this.LoadMoreConversations();
        }

        public void LoadDownAsync(bool InReload = false)
        {
            this.UpdateLoadingStatus(InReload ? ProfileLoadingStatus.Reloading : ProfileLoadingStatus.Loading);

            if (this.Items.Count == 0)
                InReload = true;

            if (InReload)
            {
                //this.Items.Clear();//todo:Merge
                this.BotKeyboardButtons.Clear();
                this.UpdateBotKeyboardVisibility();
                this.PinnedMessageVM = null;
 //               this.CounterVisibility = Visibility.Collapsed;

                if (DialogsViewModel.Instance.Items.Count > 0)
                {
                    var c = DialogsViewModel.Instance.Items.FirstOrDefault((conv) => conv.conversation.peer.id == this.PeerId);
                    if (c != null)
                    {
//                        this.Conversation = c.conversation;
//                        this.RefreshUIPropertiesSafe();
//                        this.RefreshAvatar();
                    }
                }


            }
            
            if (InReload)
                this.EnsureConversationIsUpToDate(true, this._startMessageId == null ? 0 : this._startMessageId.Value);
            else
                this.LoadMoreConversations();
        }

        internal void EnsureConversationIsUpToDate(bool force, int startMessageId = 0)
        {
            if (!force && this._skipped > 0)
            {
                //if (callback == null)
                //    return;
                //callback(true);
            }
            else if (startMessageId == -1)//else if (startMessageId == null)
                this.LoadFromLastUnread();
            else if (startMessageId > 0)
                this.LoadFromMessageId(startMessageId, force);
            else
                this.LoadMessagesAsyncWithParams(0, 21, false, false, new int?(), false);
        }

        public void LoadFromLastUnread()
        {
            this.LoadMessagesAsyncWithParams(-13, 21, false, false, new int?(-1), false);
        }

        /// <summary>
        /// Загрузить сообщения рядом с конкретным сообщением
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="callback"></param>
        /// <param name="scrollToMessageId"></param>
        public void LoadFromMessageId(int? messageId, bool scrollToMessageId = false)
        {
            this.LoadMessagesAsyncWithParams(-13, 21, false, true, messageId, scrollToMessageId);
        }

        public void LoadMoreConversations()
        {
            this.LoadMessagesAsyncWithParams(this.Items.Where((mvm => mvm.id > 0)).Count() + (int)this._skipped, 16, false, false, this._startMessageId.HasValue ? this._startMessageId.Value : new int?());
        }
        /* Original
         * private void myPanel_ScrollPositionChanged(object sender, MyVirtualizingPanel2.ScrollPositionChangedEventAgrs e)
        {
            if (e.ScrollHeight != 0.0 && e.ScrollHeight - e.CurrentPosition < VKConstants.LoadMoreNewsThreshold)
            {
                this.ConversationVM.LoadMoreConversations(null);
            }
            else
            {
                if (e.ScrollHeight == 0.0 || e.CurrentPosition >= 100.0)
                    return;
                this.ConversationVM.LoadNewerConversations(null);
            }
        }
        */
        public void LoadNewerConversations()
        {
            //if (this._skipped <= 0 || this.Items.Count <= 0)
            //    return;
            this.LoadMessagesAsyncWithParams(-15, 15, false, true, new int?((int)this.Items.Last().id), false);
        }

        public void RefreshConversations()
        {
            this.LoadMessagesAsyncWithParams(0, 21, true, true, null, false);//8
        }

        private void LoadMessagesAsyncWithParams(int offset, int count, bool resetCollection, bool showProgress = true, int? startMessageId = null, bool scrollToMessageId = false)
        {
            //if (this._isBusyLoadingMessages)
            //    return;
            //this._isBusyLoadingMessages = true;
            //this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.NoMessages));
            //if (showProgress || this.Items.Count == 0)
            //    this.SetInProgress(true, "Conversation_LoadingMessages");
            bool scrolledToUnreadItem = false;

            if (this.Items.Any((m => m.action != null && m.action.type == VKChatMessageActionType.UNREAD_ITEM_ACTION)) && this._scroll != null)
            {
                //if (startMessageId != null )
                int? nullable = startMessageId;
                if ((nullable.GetValueOrDefault() == -1 ? (nullable.HasValue ? 1 : 0) : 0) != 0)
                {
                    this._scroll.ScrollToUnreadItem();
                    scrolledToUnreadItem = true;
                }
            }
            MessagesService.Instance.GetHistory(this.PeerId, offset, count, startMessageId, (result =>
            {
                //this.SetInProgress(false, "");
                //callback?.Invoke(result.error.error_code == VKErrors.None);
                
                if (result.error.error_code == VKErrors.None)
                {
                    //this.Conversation = result.response.conversations.First((c) => c.peer.id == this.PeerId);
                    VKConversation conversation = result.response.conversations.First((c) => c.peer.id == this.PeerId);

                    this._totalCount = result.response.count;

                    List <VKMessage> messages = result.response.items;
                    foreach(var msg in messages)
                    {
                        if (msg.@out == VKMessageType.Received)
                            msg.read_state = conversation.in_read >= msg.id;
                        else
                            msg.read_state = conversation.out_read >= msg.id;
                    }
                    //                    UsersService.Instance.GetUsers(VKMessage.GetAssociatedUserIds(messages, true), (r =>
                    //                    {
                    //                        if (r != null)
                    //                        {
                    if (startMessageId.HasValue)
                        this._skipped = result.response.skipped;
                    else if (offset == 0)
                        this._skipped = 0;
                    
                    Execute.ExecuteOnUIThread(() =>
                    {
                        //
                        
//                        this.RefreshUIPropertiesSafe();
                        //

                        if (conversation.current_keyboard != null && conversation.can_write.allowed == true)
                        {
                            this.BotKeyboardButtons.Clear();//todo: здесь сбрасываем?
                            foreach (var button in conversation.current_keyboard.buttons)
                            {
                                this.BotKeyboardButtons.Add(button);
                            }

                            this.UpdateBotKeyboardVisibility();
                        }
                        //
                        int? nullable;
                        if (offset != 0)
                        {
                            nullable = startMessageId;

                            //if ((nullable != null ? (nullable.HasValue ? true : false) : false) == false)
                            if ((nullable.GetValueOrDefault() == -1 ? (nullable.HasValue ? 1 : 0) : 0) == 0)
                            {
                                nullable = startMessageId;

                                //if ((nullable!= null ? (nullable.HasValue ? true : false) : false) != false && this.Items.Any())
                                if ((nullable.GetValueOrDefault() > 0 ? (nullable.HasValue ? 1 : 0) : 0) != 0 && this.Items.Any())
                                {
                                    nullable = startMessageId;
                                    uint id = this.Items.First().id;
                                    //if ((nullable.GetValueOrDefault() < id ? (nullable.HasValue ? true : false) : false) == false)
                                    if ((nullable.GetValueOrDefault() < id ? (nullable.HasValue ? 1 : 0) : 0) == 0)
                                        goto label_6;
                                }
                                else
                                    goto label_6;
                            }
                        }
                        var oldest = messages.FirstOrDefault();//Первые в списке это последние сообщения
                        //var oldest = messages.LastOrDefault();
                        if (oldest != null && !this.Items.Any((m => m.id == oldest.id)))
                            resetCollection = true;
label_6:
                        if (resetCollection)
                        {
                            var list = this.Items.Where((m => m.id == 0)).ToList();
                            this.Items.Clear();
                            foreach (var messageViewModel in list)
                            {
                                if (messageViewModel.action != null)
                                {
                                    //Debug.Assert(m.action.member_id > 0);
                                    var user2 = UsersService.Instance.GetCachedUser((uint)messageViewModel.action.member_id);
                                    string temp = DialogsViewModel.GenerateText(messageViewModel, (VKUser)messageViewModel.User, user2, false);
                                    messageViewModel.action.UIText = temp;
                                }
                                //
                                this.Items.Add(messageViewModel);
                            }
                        }
                        else
                        {
                            foreach (var message in this.Items)
                            {
                                if (message.action != null)
                                {
                                    //Debug.Assert(m.action.member_id > 0);
                                    var user2 = UsersService.Instance.GetCachedUser((uint)message.action.member_id);
                                    string temp = DialogsViewModel.GenerateText(message, (VKUser)message.User, user2, false);
                                    message.action.UIText = temp;
                                }
                                //
                                VKMessage messageViewModel = messages.FirstOrDefault((m => m.id == message.id));
                                if (messageViewModel != null && messageViewModel.read_state != message.read_state)
                                {
                                    message.read_state = messageViewModel.read_state;
                                    message.RefreshUIProperties();
                                }

                            }
                        }
                        messages.ForEach((messageVM =>
                        {
                            if (this.Items.Any((m => m.id == messageVM.id)))
                                return;
                            //
                            if (messageVM.action != null)
                            {
                                //Debug.Assert(m.action.member_id > 0);
                                var user2 = UsersService.Instance.GetCachedUser((uint)messageVM.action.member_id);
                                string temp = DialogsViewModel.GenerateText(messageVM, (VKUser)messageVM.User, user2, false);
                                messageVM.action.UIText = temp;
                            }
                            //
                            this.Items.AddOrdered(messageVM, DialogHistoryViewModel._comparisonFunc, false);
                        }));
                        //                               this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.NoMessages));
                        
                        nullable = startMessageId;
                        //if ((nullable.GetValueOrDefault() == -1 ? (nullable.HasValue ? true : false) : false) != false)
                            this.EnsureUnreadItem();

                        
                        if (this._scroll != null)
                        {
                            nullable = startMessageId;
                            //if ((nullable != null ? (nullable.HasValue ? true : false) : false) && !scrolledToUnreadItem)
                            if ((nullable.GetValueOrDefault() == -1 ? (nullable.HasValue ? 1 : 0) : 0) == 1 && !scrolledToUnreadItem)
                            {
                                this._scroll.ScrollToUnreadItem();
                                goto label_34;
                            }
                        }
                        
                        if (((this._scroll == null ? false : (messages.Count > 0 ? true : false)) & (resetCollection ? true : false)) != false)
                            this._scroll.ScrollToBottom(false, false);
                        else if (((this._scroll == null ? false : (offset == 0 ? true : false)) & (showProgress ? true : false)) != false)
                            this._scroll.ScrollToBottom(true, false);
                        else if (this._scroll != null & scrollToMessageId)
                        {
                            nullable = startMessageId;
                            //if ((nullable!=null ? (nullable.HasValue ? true : false) : false) != false)
                            if ((nullable.GetValueOrDefault() > 0 ? (nullable.HasValue ? 1 : 0) : 0) != 0)
                                this._scroll.ScrollToMessageId((uint)startMessageId.Value);
                        }
                        else if(offset==0)//этого не было
                        {
                            this._scroll.ScrollToBottom(false, false);
                        }
                    label_34:

                        if (Settings.DEV_DisableMarkSeen == false)
                            this.SetReadStatusIfNeeded(3000);
                        //this._isBusyLoadingMessages = false;
                    });
                    //this.UpdateLoadingStatus(ProfileLoadingStatus.Loaded);

                    if (conversation.can_write.allowed == false)
                    {
                        if (conversation.can_write.reason == VKConversationNotAllowedReason.UserDisabled)
                            this.UpdateLoadingStatus(ProfileLoadingStatus.Deleted);
                        else
                            this.UpdateLoadingStatus(ProfileLoadingStatus.Banned);
                        //
                        //
                        this.CurrentLoadingStatus = ProfileLoadingStatus.Loaded;//Так мы разрешим подгрузку сообщений
                    }
                    else
                        this.UpdateLoadingStatus(messages.Count == 0 ? ProfileLoadingStatus.Empty : ProfileLoadingStatus.Loaded);
                }
                else
                {
                    //this._isBusyLoadingMessages = false;
                    this.UpdateLoadingStatus(ProfileLoadingStatus.Loaded);//Уберём внизу ошибку
                    //this.UpdateLoadingStatus(ProfileLoadingStatus.LoadingFailed);
                }
//                    }));
//                }
                //else
                //    this._isBusyLoadingMessages = false;
            }), this.group_id);
        }
/*
        private void RefreshAvatar()
        {
            VKConversation conversation = this.Conversation;
            ConversationAvatarViewModel temp_vm = new ConversationAvatarViewModel();

            if (conversation.peer.type == VKConversationPeerType.Chat)
            {
                if (conversation.chat_settings.state == "kicked" || conversation.chat_settings.state == "left")
                {
                    temp_vm.Images.Add(Constants.AVATAR_COMMUNITY + "_100.png");
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
                            var user2 = UsersService.Instance.GetCachedUser((uint)conversation.chat_settings.active_ids[0]);
                            temp_vm.Images.Add(user2.MinPhoto);
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
            else
            {
                VKBaseDataForGroupOrUser owner = null;

                if (conversation.peer.type == VKConversationPeerType.Group)
                {
                    owner = GroupsService.Instance.GetCachedGroup((uint)(-conversation.peer.id));
                    temp_vm.Online = false;
                }
                else
                {
                    owner = UsersService.Instance.GetCachedUser((uint)conversation.peer.id);

                    temp_vm.Online = ((VKUser)owner).online;
                    //                  temp_vm.online_app = ((VKProfileBase)owner).online_app;

                    if (((VKUser)owner).last_seen != null)
                        temp_vm.platform = ((VKUser)owner).last_seen.platform;
                }

                Debug.Assert(owner != null);
                //               temp_vm.UsersCount = 1;
                temp_vm.Images.Add(owner.MinPhoto);
            }
            //
            Debug.Assert(string.IsNullOrEmpty(temp_vm.Images[0]) == false);
            //
            this.ConversationAvatarVM = temp_vm;
            base.NotifyPropertyChanged("ConversationAvatarVM");
        }
*/
        /*
                //private string _uiTitle = string.Empty;
                public string UITitle
                {
                    get
                    {
                        if (this.Conversation != null)
                        {
                            VKConversation conversation = this.Conversation;
                            if (conversation.peer.type == VKConversationPeerType.Chat)
                            {
                                return conversation.chat_settings.title;
                            }
                            else if (conversation.peer.type == VKConversationPeerType.Group || conversation.peer.id < 0)//странный баг, бывает юзер с отрицательным пиром
                            {
                                var group = GroupsService.Instance.GetCachedGroup((uint)(-conversation.peer.id));
                                return group.Title;
                            }
                            else if (conversation.peer.type == VKConversationPeerType.User)
                            {
                                VKUser user = UsersService.Instance.GetCachedUser((uint)conversation.peer.id);
                                return user.Title;
                            }
                        }
                        return "";
                    }
                    //set
                    //{
                    //    if (this._uiTitle == value)
                    //        return;
                    //    this._uiTitle = value;
                    //    base.NotifyPropertyChanged("UITitle");
                    //    base.NotifyPropertyChanged("OptionsVisibility");

                    //}
                }
        */
        //public bool CanMessage;
        /*
                public string UISubtitle
                {
                    get
                    {
                        if (this.UsersTypingHelper != null && this.UsersTypingHelper.AnyTypingNow)
                            return this.UsersTypingHelper.TypingString;




                        if (this.Conversation == null)
                            return "Updating...";

                        VKConversation conversation = this.Conversation;
                        if (conversation.peer.type == VKConversationPeerType.Chat)
                        {
        //                    this.UITitle = conversation.chat_settings.title;
                            string tempStr = UIStringFormatterHelper.FormatNumberOfSomething((int)conversation.chat_settings.members_count, "Conversation_OnePerson", "Conversation_TwoToFourPersonsFrm", "Conversation_FiveOrMorePersionsFrm");
                            return tempStr;
                        }
                        else if (conversation.peer.type == VKConversationPeerType.Group || conversation.peer.id < 0)//странный баг, бывает юзер с отрицательным пиром
                        {
                            return LocalizedStrings.GetString("Community");
        //                    var group = GroupsService.Instance.GetCachedGroup((uint)(-conversation.peer.id));
        //                    this.UITitle = group.Title;
                        }
                        else if (conversation.peer.type == VKConversationPeerType.User)
                        {
                            VKUser user = UsersService.Instance.GetCachedUser((uint)conversation.peer.id);
          //                  this.UITitle = user.Title;
                            if (user.deactivated == VKIsDeactivated.None)
                            {
                                if (user.last_seen == null)
                                    user.last_seen = new VKLastSeen();
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
        */
        /*
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
                    base.NotifyPropertyChanged(nameof(this.UISubtitle));
                }
        #endregion
        */
        public string MessageText;

#region VM
        public string TextWatermarkText
        {
            get { return LocalizedStrings.GetString("Message"); }
        }

        public ObservableCollection<List<VKBotKeyboard.KeyboardButton>> BotKeyboardButtons { get; private set; }

        public Visibility BotKeyboardVisibility
        {
            get
            {
                return this.BotKeyboardButtons.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void UpdateBotKeyboardVisibility()
        {
            base.NotifyPropertyChanged(nameof(this.BotKeyboardVisibility));
        }
        /*
        public Visibility OptionsVisibility
        {
            get
            {
                return string.IsNullOrEmpty(this.UITitle) ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        */
#endregion
/*
        public void RefreshUIPropertiesSafe()
        {
            base.NotifyPropertyChanged(nameof(this.UserVerified));
            base.NotifyPropertyChanged(nameof(this.UISubtitle));
            base.NotifyPropertyChanged(nameof(this.UITitle));
            base.NotifyPropertyChanged(nameof(this.AreNotificationsDisabled));

            if (this.ConversationAvatarVM == null)
                this.RefreshAvatar();
        }

        private Visibility _counterVisibility;
        public Visibility CounterVisibility
        {
            get
            {
                return this._counterVisibility;
            }
            set
            {
                if (this._counterVisibility == value)
                    return;
                this._counterVisibility = value;
                base.NotifyPropertyChanged(nameof(this.CounterVisibility));
                base.NotifyPropertyChanged(nameof(this.Unread));
            }
        }
*/
        /// <summary>
        /// Добавляем "Непрочитанный" элемент, если есть непрочитанные сообщения
        /// происходит после загрузки списка сообщений и после добавления нового сообщения
        /// </summary>
        public void EnsureUnreadItem()
        {
            /*
            this.RemoveUnreadMessagesItem();
            int num = 0;


            //В самом конце списка самые новые/последние сообщения
            //В начале - старые сообщения [0]
            for (int index = 0; index < this.Items.Count; index++)
            {
                if (this.Items[index].@out == VKMessageType.Received && this.Items[index].read_state == false)
                {
                    num = index;
                    break;
                }
            }
            
            if (num <= 0)
            {
                this.CounterVisibility = Visibility.Collapsed;
                return;
            }

            if (this.Items.Count < num)
                return;


            int i = num + 1;//+1 т.к. этот элемент уже в списке

            this.Items.Insert(i, new VKMessage() { action = new VKMessage.MsgAction() { type = VKChatMessageActionType.UNREAD_ITEM_ACTION }, date = this.Items[num].date });
            this.CounterVisibility = Visibility.Visible;
            */

            //ВТОРАЯ ВЕРСИЯ: UNREAD_ITEM_ACTION без нужды не удаляется

            uint? num = null;//позиция разделителя "непрочитанного сообщения"
            for (int index = 0; index < this.Items.Count; index++)
            {
                if (this.Items[index].@out == VKMessageType.Received && this.Items[index].read_state == false )
                {
                    num = (uint)index;
                    break;
                }
            }
            
            if (num==null)
            {
                //Нет непрочитанных сообщений
                this.RemoveUnreadMessagesItem();
                return;
            }

            
#if DEBUG
            Debug.WriteLine("UNREAD_ITEM_ACTION pos -> " + num);
#endif

            VKMessage messageViewModel = this.Items.FirstOrDefault(m => m.action != null && m.action.type == VKChatMessageActionType.UNREAD_ITEM_ACTION);
            if(messageViewModel==null)
            {
                messageViewModel = new VKMessage() { action = new VKMessage.MsgAction() { type = VKChatMessageActionType.UNREAD_ITEM_ACTION }, date = this.Items[(int)num].date };
                this.Items.Insert((int)num, messageViewModel);
            }
            else
            {
                int pos = this.Items.IndexOf(messageViewModel);

                if(pos == this.Items.Count - 1)
                {
                    //Странный баг, вроде больше не появляется
                    this.RemoveUnreadMessagesItem();
                    return;
                }

                int new_pos = (int)num;
                if (pos == new_pos)
                {
                    return;
                }
                else
                {
                    this.Items.Move(pos, new_pos);
                }
            }
        }
        /*
        Оригинальная функция:
        private void EnsureUnreadItem()
        {
            this.RemoveUnreadMessagesItem();
            int num = 0;
            for (int index = this._messages.Count - 1; index >= 0 && (this._messages[index].Message.@out == 0 && this._messages[index].Message.read_state == 0); --index)
                ++num;
            if (num <= 0)
                return;
            MessageViewModel messageViewModel = new MessageViewModel(new Message() { action = ConversationViewModel.UNREAD_ITEM_ACTION });
            if (this._messages.Count < num)
                return;
            this._messages.Insert(this._messages.Count - num, messageViewModel);
        }
        */


        /// <summary>
        /// Удаляем текст "Новые сообщения ниже"
        /// </summary>
        private void RemoveUnreadMessagesItem()
        {
            VKMessage messageViewModel = this.Items.FirstOrDefault(m => m.action != null && m.action.type == VKChatMessageActionType.UNREAD_ITEM_ACTION);
            if (messageViewModel != null)
            {
                this.Items.Remove(messageViewModel);
//                this.CounterVisibility = Visibility.Collapsed;
            }
#if DEBUG
            messageViewModel = this.Items.FirstOrDefault(m => m.action != null && m.action.type == VKChatMessageActionType.UNREAD_ITEM_ACTION);
            Debug.Assert(messageViewModel == null);
#endif
        }

        public List<VKBaseDataForGroupOrUser> ChatMembers { get; set; }
        /*
        public List<VKBaseDataForGroupOrUser> ChatMembers
        {
            get
            {
                List<VKBaseDataForGroupOrUser> ret = new List<VKBaseDataForGroupOrUser>();

                var dialog = DialogsViewModel.Instance.CurrentConversation;
                if (dialog != null)
                {
                    foreach (int id in dialog.conversation.chat_settings.active_ids)
                    {
                        Debug.Assert(id > 0);
                        var u = UsersService.Instance.GetCachedUser((uint)id);
                        ret.Add(u);
                    }
                }

                return ret;
            }
        }
        */

        public bool IsChat { get { return this.PeerId > 2000000000; } }

        public void EditMessage(string messageText, uint msgId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["peer_id"] = this.PeerId.ToString();
            parameters["message"] = messageText;
            parameters["message_id"] = msgId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("messages.edit", parameters,null);
        }

        public void UserIsTyping()
        {
            if ((DateTime.Now - this._lastTimeUserIsTypingWasCalled).TotalSeconds <= 8.0)
                return;

            MessagesService.Instance.SetUserIsTyping(this.PeerId, null, true, this.group_id);
            this._lastTimeUserIsTypingWasCalled = DateTime.Now;
        }
        
        public void SendGraffiti(RenderTargetBitmap graffitiAttachmentItem, string localFile)
        {
            VKGraffiti _graffiti = new VKGraffiti() { photo_200 = localFile };
            VKAttachment at = new VKAttachment() { graffiti = _graffiti, type = VKAttachmentType.Graffiti };

            List<VKAttachment> temp_msg_attachments = new List<VKAttachment>();
            temp_msg_attachments.Add(at);


            VKMessage msg = new VKMessage();
            msg.@out = VKMessageType.Sent;
            msg.date = DateTime.Now.AddSeconds(Settings.ServerMinusLocalTimeDelta);
            msg.attachments = temp_msg_attachments;
            msg.OutboundMessageVM = new OutboundMessageViewModel(this.PeerId,this.group_id);
            msg.OutboundMessageVM.Attachments = new List<IOutboundAttachment>();
            OutboundGraffitiAttachment gr = new OutboundGraffitiAttachment();
            gr.Data = graffitiAttachmentItem;

            msg.OutboundMessageVM.Attachments.Add(gr);
            msg.from_id = (int)Settings.UserId;
            msg.peer_id = this.PeerId;

            this.Items.Add(msg);

            msg.Send();
        }

        public void SendStickerAsGraffiti(VKSticker sticker)
        {
            VKMessage msg = new VKMessage();
            msg.@out = VKMessageType.Sent;
            msg.date = DateTime.Now.AddSeconds(Settings.ServerMinusLocalTimeDelta);
            msg.OutboundMessageVM = new OutboundMessageViewModel(this.PeerId, this.group_id);
            msg.OutboundMessageVM.Attachments = new List<IOutboundAttachment>();
            OutboundGraffitiAttachment gr = new OutboundGraffitiAttachment();
            gr.Sticker = sticker;

            msg.OutboundMessageVM.Attachments.Add(gr);
            msg.from_id = (int)Settings.UserId;
            msg.peer_id = this.PeerId;

            VKAttachment at = new VKAttachment() { sticker = sticker, type = VKAttachmentType.Sticker };

            List<VKAttachment> temp_msg_attachments = new List<VKAttachment>();
            temp_msg_attachments.Add(at);
            msg.attachments = temp_msg_attachments;

            this.Items.Add(msg);

            msg.Send();
        }

        public void SendMessage(VKSticker Sticker)
        {
            Sticker.product_id = 999;//BugFix: чтобы анимированные стикеры начали двигаться

            VKAttachment at = new VKAttachment() { sticker = Sticker, type = VKAttachmentType.Sticker };

            List<VKAttachment> temp_msg_attachments = new List<VKAttachment>();
            temp_msg_attachments.Add(at);

            VKMessage msg = new VKMessage();
            msg.@out = VKMessageType.Sent;
            msg.date = DateTime.Now.AddSeconds(Settings.ServerMinusLocalTimeDelta);
            msg.attachments = temp_msg_attachments;
            msg.OutboundMessageVM = new OutboundMessageViewModel(this.PeerId, this.group_id);
            msg.OutboundMessageVM.StickerItem = Sticker.sticker_id;
            msg.from_id = (int)Settings.UserId;
            msg.peer_id = this.PeerId;

            this.Items.Add(msg);

            msg.Send();
        }

        public void SendVoiceRecord(Windows.Storage.StorageFile file, int duration, List<int> waveform)
        {
            VKMessage msg = new VKMessage();
            msg.@out = VKMessageType.Sent;
            msg.date = DateTime.Now.AddSeconds(Settings.ServerMinusLocalTimeDelta);
            //msg.attachments = temp_msg_attachments;
            msg.OutboundMessageVM = new OutboundMessageViewModel(this.PeerId, this.group_id);
            msg.OutboundMessageVM.Attachments = new List<IOutboundAttachment>();
            msg.OutboundMessageVM.AddVoiceMessageAttachment(file, duration, waveform);
            msg.from_id = (int)Settings.UserId;
            msg.peer_id = this.PeerId;

            DocPreview.DocPreviewVoiceMessage audio_msg = new DocPreview.DocPreviewVoiceMessage() { duration = duration, waveform = waveform };
            DocPreview prew = new DocPreview() { audio_msg = audio_msg };
            VKDocument doc = new VKDocument() { preview = prew, type = VKDocumentType.AUDIO };
            VKAttachment at = new VKAttachment() { doc = doc, type = VKAttachmentType.Doc };

            msg.attachments = new List<VKAttachment>();
            msg.attachments.Add(at);

            this.Items.Add(msg);

            msg.Send();
        }

        /// <summary>
        /// ШАГ 1: создаём сообщение для интерфейса
        /// в него добавляем вложения
        /// </summary>
        /// <param name="messageText"></param>
        public void SendMessage(string messageText, string payload = null)
        {
            string str = messageText.Replace("\r\n", "\r").Replace("\r", "\r\n").Trim();
            this.RemoveUnreadMessagesItem();

            if (string.IsNullOrEmpty(str) && this.Attachments.Count == 0)
                return;

            VKMessage msg = new VKMessage();
            msg.text = str;
            msg.@out = VKMessageType.Sent;
            msg.date = DateTime.Now.AddSeconds(Settings.ServerMinusLocalTimeDelta);
            msg.OutboundMessageVM = new OutboundMessageViewModel(this.PeerId, this.group_id);
            msg.OutboundMessageVM.Attachments = this.Attachments.ToList();
            msg.OutboundMessageVM.MessageText = str;
            msg.OutboundMessageVM.Payload = payload;
            msg.from_id = (int)Settings.UserId;
            msg.peer_id = this.PeerId;
            //msg.payload = payload;


            //
            if (this.Attachments.Count>0)
                msg.attachments = new List<VKAttachment>();

            foreach (IOutboundAttachment outboundAttachment in this.Attachments)
            {
                if (outboundAttachment is OutboundForwardedMessages forwardedMessages)
                {
                    msg.fwd_messages = new List<VKMessage>();
                    foreach (VKMessage m in forwardedMessages.Messages)
                        msg.fwd_messages.Add(m);
                }

                VKAttachment attachment = outboundAttachment.GetAttachment();
                if (attachment != null)
                    msg.attachments.Add(attachment);
            }
            
            //В начале (у нуля) старые сообщение. Последние сообщения в конце
            //this.Items.Insert(0, msg);
            this.Items.Add(msg);

            msg.Send();
        }

        private bool InSync;

        public void SetReadStatusIfNeeded(int delayMillisec = 0)
        {
            if ((this.InSync || this.PeerId == 0) && delayMillisec > 0)
                return;

            if (this.Items.Count == 0)
                return;

            VKMessage msg = this.Items.Last();
            if(msg.@out == VKMessageType.Sent || msg.read_state == true)
                return;

            //this.InSync = true;

            this.MarkAsRead(msg.id, this.PeerId, delayMillisec, (result) =>
            {
                
                if (result == true)
                {
                    //cur_convrsation.in_read = cur_convrsation.last_message_id;
                    Execute.ExecuteOnUIThread(()=> {
                        List<VKMessage> listToBeMarkedAsRead = new List<VKMessage>();
                        foreach (VKMessage message1 in this.Items)
                        {
                            if (message1.read_state == false && message1.action == null && (message1.@out == VKMessageType.Received || message1.from_id == Settings.UserId))
                                listToBeMarkedAsRead.Add(message1);
                        }
                        this.InSync = false;
                        listToBeMarkedAsRead.ForEach((m => m.read_state = true));
                        this.RemoveUnreadMessagesItem();
                    });
                }
            });
        }

        public async void MarkAsRead(uint messageId, int peerId, int delayMillisec, Action<bool> calback = null)
        {
            this.InSync = true;
            await Task.Delay(delayMillisec);
            if (this.InSync==false)
                return;//Что-то изменилось за время ожидания :)

            MessagesService.Instance.MarkAsRead(messageId, peerId, (result) =>
            {
                //this.InSync = false;
                calback(result.error.error_code == VKErrors.None && result.response == 1);
            }, this.group_id);
        }

        public void AddForwardedMessagesToOutboundMessage(IList<VKMessage> forwardedMessages)
        {
            this.Attachments.Remove(this.Attachments.FirstOrDefault(a => a is OutboundForwardedMessages));//this.OutboundMessageVM.RemoveForwardedMessages();
            this.Attachments.Add(new OutboundForwardedMessages(forwardedMessages.ToList<VKMessage>()));
        }

        public void MarkAsImportant(VKMessage msg, bool important)
        {
            MessagesService.Instance.MarkAsImportant(msg.id, important, (result) => {

                if (result.error.error_code == VKErrors.None)
                {
                    msg.important = !msg.important;
                    msg.RefreshUIProperties();
                }
            });
        }

        public void Pin(VKMessage msg)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["message_id"] = msg.id.ToString();
            parameters["peer_id"] = this.PeerId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKPinnedMessage>("messages.pin", parameters, null);
        }

        public async void UnPin()
        {
            MessageDialog dialog = new MessageDialog("Продолжить?", "Открепить сообщение?");
            dialog.Commands.Add(new UICommand { Label = "Нет", Id = 0 });
            dialog.Commands.Add(new UICommand { Label = "Да", Id = 1 });

            IUICommand res = await dialog.ShowAsync();
            if ((int)res.Id != 1)
                return;

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["peer_id"] = this.PeerId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("messages.unpin", parameters, null);

        }

        private async Task<byte> InputTextDialogAsync(string title, bool for_all)
        {
            var inputTextBox = new CheckBox();
            inputTextBox.Content = "Удалить для всех.";
            inputTextBox.IsChecked = false;
            ContentDialog dialog = new ContentDialog();
            if (for_all)
                dialog.Content = inputTextBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Да";
            dialog.SecondaryButtonText = "Нет";
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                return inputTextBox.IsChecked.Value == true ? (byte)2 : (byte)1;
            }
            else
                return 0;
        }

        public async void DeleteMessages(List<VKMessage> msgs, Action<bool> callback = null)
        {
            bool for_all = false;

            //todo: .AddSeconds(Settings.ServerMinusLocalTimeDelta);
            var temp = msgs.FirstOrDefault((m) => (DateTime.Now - m.date).TotalHours < 24 && m.@out == VKMessageType.Sent);
            if(temp!=null)
                for_all = true;
            
            if (msgs.Count == 1)
            {
                if(msgs[0].FailedVisibility == Visibility.Visible)
                {
                    //bugfix: удаляем из списка ен ко не отправить, а иначе оно будет всегда в списке
                    this.Items.Remove(msgs[0]);
                    return;
                }
            }

            byte result = await InputTextDialogAsync("Удалить сообщения?", for_all);
            if (result == 0)
                return;

            this.RemoveUnreadMessagesItem();

            var list = msgs.Where((m) => m.id > 0);

            if(list.Count() == 0)//попытка удалить не отосланное сообщение
            {
                foreach (VKMessage msg in msgs)
                    this.Items.Remove(msg);

                callback?.Invoke(true);
                return;
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["message_ids"] = list.Select((m) => m.id).ToList().GetCommaSeparated();
            if (result == 2)
                parameters["delete_for_all"] = "1";


            VKRequestsDispatcher.DispatchRequestToVK<Dictionary<int, int>>("messages.delete", parameters, (res) =>
            {
                if (res.error.error_code == VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        foreach (VKMessage msg in msgs)
                            this.Items.Remove(msg);
                    });
                    
                    callback?.Invoke(true);
                }

                callback?.Invoke(false);
            });

        }

        private bool _isInProgressPinToStart;

        internal void PinToStart()
        {

            if (this._isInProgressPinToStart)
                return;
            /*
            this._isInProgressPinToStart = true;
            //this.SetInProgressMain(true, "");
            if (this.Conversation.peer.type != VKConversationPeerType.Chat)
            {
                Debug.Assert(this.PeerId > 0);
                UsersService.Instance.GetUsersForTile((uint)this.PeerId, (res =>
                {
                    if (res == null)
                    {
                        this._isInProgressPinToStart = false;
                        //this.SetInProgressMain(false, "");
                        //Execute.ExecuteOnUIThread((Action)(() => ExtendedMessageBox.ShowSafe(CommonResources.Error)));
                    }
                    else
                    {
                        string title = string.Format(LocalizedStrings.GetString("DialogWithFrm"), res.Title);
                        List<string> imageUris = new List<string>();
                        imageUris.Add(res.MinPhoto);

                        this.DoCreateTile(imageUris, title);
                    }
                }));
            }
            else
            {
                ChatService.Instance.GetChatInfo(this.PeerId, (res =>
                {
                    if (res == null)
                    {
                        this._isInProgressPinToStart = false;
                        //this.SetInProgressMain(false, "");
                        //Execute.ExecuteOnUIThread((Action)(() => MessageBox.Show(CommonResources.Error)));
                    }
                    else
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            List<string> list = res.response.chat_participants.Select<VKUser, string>((c => c.photo_max)).ToList<string>();
                            if (!string.IsNullOrWhiteSpace(res.response.chat.photo_200))
                                list.Insert(0, res.response.chat.photo_200);
                            this.DoCreateTile(list, res.response.chat.title);
                        });

                    }
                }));
            }
            */
        }

        private void DoCreateTile(List<string> imageUris, string title)
        {
            SecondaryTileCreator.CreateTileForConversation(this.PeerId, title, imageUris, (res =>
            {
                this._isInProgressPinToStart = false;
                //this.SetInProgressMain(false, "");
                if (res)
                    return;
                //Execute.ExecuteOnUIThread((Action)(() => ExtendedMessageBox.ShowSafe(CommonResources.Error)));
            }));
        }

        public string StatusText
        {
            get { return String.Empty; }
        }

        public string FooterText
        {
            get { return String.Empty; }
        }

        private List<VKMessage> TrimMessageViewModels()
        {
            return this.Items.Skip(Math.Max(0, this.Items.Count - this._numberOfMessagesToStore)).Take(this._numberOfMessagesToStore).ToList();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(2);
            writer.Write(this.PeerId);
            //writer.Write(this._isChat);
            //writer.Write<UserStatus>(this._userStatus, false);
            //writer.Write<GroupOrUser>(this._user, false);
            //if (trim)
            //    writer.WriteList<MessageViewModel>((IList<MessageViewModel>)this.TrimMessageViewModels(), 10000);
            //else
            writer.WriteList<VKMessage>(this.TrimMessageViewModels());

            //writer.Write<OutboundMessageViewModel>(this._outboundMessage, false);
            //
            writer.WriteList<OutboundAttachmentContainer>(this.Attachments.Select((a => new OutboundAttachmentContainer(a))).ToList());
            writer.WriteString(this.MessageText);
            //

            //writer.Write(this._isInSelectionMode);
//          writer.Write(this.Conversation);





            int temp = this._totalCount.HasValue ? (int)this._totalCount.Value : -1;
            writer.Write(temp);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.PeerId = reader.ReadInt32();
            //this._isChat = reader.ReadBoolean();
            //this._userStatus = reader.ReadGeneric<UserStatus>();
            //this._user = reader.ReadGeneric<GroupOrUser>();

            var items = reader.ReadList<VKMessage>();
            foreach (var item in items)
                this.Items.Add(item);

            //
            //if (this.GroupedItems != null)
            //    this.GroupedItems.Dispose();

           // this.GroupedItems = null;
            //
            if(this.GroupedItems==null)
                this.GroupedItems = new ObservableGroupingCollection<VKMessage>(this.Items);

            Execute.ExecuteOnUIThread(() => { base.NotifyPropertyChanged(nameof(this.GroupedItems)); });
            //this._outboundMessage = reader.ReadGeneric<OutboundMessageViewModel>();
            //
            List<OutboundAttachmentContainer> source = reader.ReadList<OutboundAttachmentContainer>();
            this.Attachments.Clear();
            foreach (IOutboundAttachment outboundAttachment in source.Select((c => c.OutboundAttachment)))
                this.Attachments.Add(outboundAttachment);

            this.MessageText = reader.ReadString();
            //
            //this._isInSelectionMode = reader.ReadBoolean();

            //          this.Conversation = reader.ReadGeneric<VKConversation>();
            //this._isInitialized = true;
            
            int temp = reader.ReadInt32();
            if (temp >= 0)
                this._totalCount = (uint)temp;
            //           this.RefreshUIPropertiesSafe();
        }
    }
}
