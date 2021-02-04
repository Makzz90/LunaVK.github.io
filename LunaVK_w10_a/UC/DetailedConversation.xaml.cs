using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.ViewModels;
using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Linq;
using LunaVK.UC.AttachmentPickers;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using System.IO;
using Windows.Storage.Streams;
using LunaVK.Core.Framework;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Data;
using LunaVK.UC.Controls;
using Windows.System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.Foundation.Metadata;

namespace LunaVK.UC
{
    public sealed partial class DetailedConversation : UserControl, IScroll
    {
        //PopUP popMenu = null;
        //        UC.PopUP popMsg = null;
        public ObservableCollection<VKBaseDataForGroupOrUser> ChatMembers { get; private set; }
        private PopUpService _flyout;
        ScrollViewer scroll = null;
        public List<VKMessage> selectedMsgs = new List<VKMessage>();
        private bool IsOnScreen = true;
        
        public DialogHistoryViewModel HistoryVM { get; private set; }
        
        public ConversationWithLastMsg VM { get; private set; }
        
        public NewMessageUC UcNewMessage
        {
            get { return this.ucNewMessage; }
        }

        public CommandBar CmdBar;

        public DetailedConversation()
        {
            base.DataContext = null;

            this.InitializeComponent();

            this.ucNewMessage.IsVoiceMessageButtonEnabled = true;
            this.ucNewMessage.OnSendTap = this.BorderSend_Tapped;
            this.ucNewMessage.OnSendPayloadTap = this.BorderSendPayload_Tapped;
            this.ucNewMessage.TextBoxNewComment.TextChanged += this.TextBoxNewComment_TextChanged;
            this.ucNewMessage.OnAddAttachTap = this.AddAttachTap;
            this.ucNewMessage.StickerTapped += this.PanelControl_StickerTapped;
            this.ucNewMessage.AudioRecorder.RecordDone += this.ucNewMessage_RecordDone;
            this.ucNewMessage.OnImageDeleteTap = this.ImageDeleteTap;
            this.ucNewMessage.IsOpenedChanged += this.HandlePanelOpened;

            this.ChatMembers = new ObservableCollection<VKBaseDataForGroupOrUser>();

            this.eListView.Loaded += this.ExtendedListView2_Loaded;
            this.eListView.NeedReload = false;

            this.Loaded += this.DetailedConversation_Loaded;
            this.Unloaded += this.DetailedConversation_Unloaded;

            this._pinnedMsg.HideMsgCallback = this.HandleHideMsg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">NewMessageUC</param>
        /// <param name="height"></param>
        private void HandlePanelOpened(object sender, double height)
        {
            //var element = sender as NewMessageUC;
            this.ShowingMoveSpline.Value = -height;
            this.MoveMiddleOnShowing.Begin();
            //System.Diagnostics.Debug.Assert(this.ConversationVM!=null);
            if(this.HistoryVM != null && height > 0 && Settings.DEV_DisableMarkSeen == false)
                this.HistoryVM.SetReadStatusIfNeeded(0);
        }

        private void HandleHideMsg()
        {
            this.HistoryVM.UnPin();
        }

        private object _instLock = new object();

        private void UnHookVM()
        {
            if (this.HistoryVM == null)
                return;

            this.HistoryVM.LoadingStatusUpdated -= this.HandleLoadingStatusUpdated;
            this.HistoryVM.Attachments.CollectionChanged -= this.Attachments_CollectionChanged;
            this.HistoryVM._scroll = null;

            Network.LongPollServerService.Instance.ReceivedUpdates -= this.ReceivedUpdates;
        }

        /// <summary>
        /// Надо вызывать при смене диалога
        /// </summary>
        public void SetData(ConversationWithLastMsg conversation, bool inSearch = false, uint? grouoId = null)
        {
            if (this.HistoryVM != null && !inSearch)
            {
                //Переключение диалогов
                this.HistoryVM.MessageText = this.ucNewMessage.TextBoxNewComment.Text;
                this.HistoryVM.Save();
            }

            base.DataContext = null;
            this.ucNewMessage.MentionPicker.ItemsSource = null;
            this.UnHookVM();

            this.VM = conversation;

            if (conversation == null)
            {
                this.HistoryVM = null;
                return;
            }

            int peerId = conversation.conversation.peer.id;

            if (inSearch)
            {
                this.HistoryVM = new DialogHistoryViewModel(peerId, grouoId);
                this.HistoryVM._startMessageId = (int)conversation.last_message.id;
            }
            else
            {
                if (grouoId.HasValue)
                {
                    this.HistoryVM = new DialogHistoryViewModel(peerId, grouoId);
                }
                else
                {
                    lock (this._instLock)
                    {
                        DialogHistoryViewModel vm = new DialogHistoryViewModel(peerId);
                        CacheManager.TryDeserialize(vm, "Dialog_" + peerId);
                        this.HistoryVM = vm;
                    }
                }
            }


            base.DataContext = this;

            if (this.VM.conversation.chat_settings != null)
            {
                foreach (int id in this.VM.conversation.chat_settings.active_ids)
                {
                    Debug.Assert(id > 0);
                    var u = UsersService.Instance.GetCachedUser((uint)id);
                    this.HistoryVM.ChatMembers.Add(u);
                }
            }

            this.HistoryVM.LoadDownAsync(true);

            //
            Binding binding = new Binding() { Source = this.HistoryVM.Attachments, Mode = BindingMode.OneTime };
            this.ucNewMessage.ItemsControlAttachments.SetBinding(ItemsControl.ItemsSourceProperty, binding);
            //
            this.ucNewMessage.HidePanel();

            this.HistoryVM.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;
            this.HistoryVM.Attachments.CollectionChanged += this.Attachments_CollectionChanged;
            this.HistoryVM._scroll = this;

            if (this.selectedMsgs.Count > 0)
            {
                OutboundForwardedMessages at = new OutboundForwardedMessages(this.selectedMsgs.ToList<VKMessage>());
                this.HistoryVM.Attachments.Add(at);
                this.selectedMsgs.Clear();
            }
            //
            this.ucNewMessage.UpdateVoiceMessageAvailability();
            Network.LongPollServerService.Instance.ReceivedUpdates += this.ReceivedUpdates;
//            if (Settings.DEV_DisableMarkSeen == false)
//                this.MarkRead();

            this.ucNewMessage.TextBoxNewComment.Text = this.HistoryVM.MessageText ?? "";
        }

        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {
            switch (status)
            {
                case ProfileLoadingStatus.Loaded:
                    {
                        VisualStateManager.GoToState(this.ucNewMessage, "Ready", false);
                        this.ucNewMessage.UpdateVisibilityState();
                        break;
                    }
                case ProfileLoadingStatus.Reloading:
                //case ProfileLoadingStatus.Loading:
                    {
                        VisualStateManager.GoToState(this.ucNewMessage, "Loading", false);
                        break;
                    }
                case ProfileLoadingStatus.Deleted:
                case ProfileLoadingStatus.Banned:
                    {
                        VisualStateManager.GoToState(this.ucNewMessage, "Blocked", false);
                        break;
                    }
                case ProfileLoadingStatus.LoadingFailed:
                    {
                        VisualStateManager.GoToState(this.ucNewMessage, "Ready", false);
                        break;
                    }
            }
            //string str = status.ToString();
            //VisualStateManager.GoToState(this.loading, str, false);
            if (status == ProfileLoadingStatus.Loaded)
            {
/*
                if (this.ConversationVM._startMessageId != 0)
                {
                    var startMessageId = this.ConversationVM.Items.FirstOrDefault((m) =>
                    {
                        if (m is VKMessage item)
                        {
                            if (item.id == this.ConversationVM._startMessageId)
                                return true;
                        }
                        return false;
                    });
                    if (startMessageId != null)
                        this.eListView.ScrollToItem(startMessageId);

                }
                */
                //if (this.HistoryVM.BotKeyboardButtons.Count > 0)
                //    this.ucNewMessage.UpdateVisibilityState();//не сработает, т.к. в интерфейсе ещё нет кнопок
            }
        }
/*
        private async void MarkRead()
        {
            await System.Threading.Tasks.Task.Delay(4000);

            
            var unreadTitle = this.ConversationVM.Items.FirstOrDefault((i) => i.action!= null && i.action.type == VKChatMessageActionType.UNREAD_ITEM_ACTION);
            if (unreadTitle != null)
            {
                var position = this.eListView.GetItemPosition(unreadTitle);
                if (position != null)
                {
                    double y = position.Value.Y;

                    if (y > 0 && y < CustomFrame.Instance.ActualHeight)
                        this.ConversationVM.SetReadStatusIfNeeded(0);
                    //System.Diagnostics.Debug.WriteLine("Y:{0}",y);
                }
            }
        }
*/
        private void DetailedConversation_Unloaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.HeaderHeightChanged -= HeaderWithMenu_HeaderHeightChanged;

            this.UnHookVM();



            //InputPane.GetForCurrentView().Showing -= Keyboard_Showing;
            //InputPane.GetForCurrentView().Hiding -= Keyboard_Hiding;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                this.ProcessKeyboardAccelerators -= DialogsConversationPage2_ProcessKeyboardAccelerators;
            }

            Window.Current.VisibilityChanged -= this.Current_VisibilityChanged;

            if (this.HistoryVM != null && !this.HistoryVM._startMessageId.HasValue)
            {
                this.HistoryVM.MessageText = this.ucNewMessage.TextBoxNewComment.Text;
                this.HistoryVM.Save();
            }
        }

        private void DetailedConversation_Loaded(object sender, RoutedEventArgs e)
        {
            if (CustomFrame.Instance == null)
                return;

            CustomFrame.Instance.Header.HeaderHeightChanged += HeaderWithMenu_HeaderHeightChanged;
            //if (this.ConversationVM != null)
            //    this.ConversationVM.Attachments.CollectionChanged += this.Attachments_CollectionChanged;
            this.ucNewMessage.ActivateSendButton(false);
            

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                this.ProcessKeyboardAccelerators += this.DialogsConversationPage2_ProcessKeyboardAccelerators;
            }

            Window.Current.VisibilityChanged += this.Current_VisibilityChanged;
        }

        private void Current_VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            this.IsOnScreen = e.Visible;
            if (e.Visible && this.HistoryVM != null && this.HistoryVM._startMessageId == null)
                this.HistoryVM.EnsureConversationIsUpToDate(false,0);


        }

        private async void DialogsConversationPage2_ProcessKeyboardAccelerators(UIElement sender, ProcessKeyboardAcceleratorEventArgs args)
        {
            if (!this.VM.conversation.can_write.allowed)
                return;

            if (args.Key == VirtualKey.V && args.Modifiers == VirtualKeyModifiers.Control)
            {
                var dataPackageView = Clipboard.GetContent();
                if (dataPackageView.Contains(StandardDataFormats.Bitmap))
                {
                    IRandomAccessStreamReference imageReceived = null;
                    imageReceived = await dataPackageView.GetBitmapAsync();
                    if (imageReceived != null)
                    {
                        
                        using (var imageStream = await imageReceived.OpenReadAsync())
                        {
                            //todo:в папку
                            StorageFile storageFile = await CacheManager.GetStorageFileInCurrentUserCacheFolder("Clipboard" + ".jpg", CreationCollisionOption.GenerateUniqueName);

                            using (var fileStream = await storageFile.OpenStreamForWriteAsync())
                            {
                                await imageStream.AsStreamForRead().CopyToAsync(fileStream);
                                fileStream.Dispose();
                            }

                            OutboundPhotoAttachment o = await OutboundPhotoAttachment.CreateForUploadNewPhoto(storageFile);
                            this.HistoryVM.Attachments.Add(o);
                        }
                    }
                }
            }
        }

        private void ReceivedUpdates(List<UpdatesResponse.LongPollServerUpdateData> updates)
        {
            Execute.ExecuteOnUIThread(() =>
            {
                if (this.VM == null)
                    return;//BugFix: мы открыли страницу, а контрол создался с пустым datacontext

                foreach (UpdatesResponse.LongPollServerUpdateData update in updates)
                {
                    if (this.VM.conversation.peer.id != update.peer_id)
                        continue;

                    switch (update.UpdateType)
                    {
                        case LongPollServerUpdateType.MessageAdd:
                            {
                                if (update.flags.OUTBOX == false)
                                {
                                    this.VM.SetUserIsNotTyping(update.user_id);
                                }

                                if (!string.IsNullOrEmpty(update.extended))
                                {
                                    //update.extended = update.extended.Replace("\r\n","").Replace("    ","");

                                    this.HistoryVM.UpdateButtons(update.extended);
                                    if (this.HistoryVM.BotKeyboardButtons.Count > 0)
                                        this.ucNewMessage.UpdateVisibilityState();
                                }
                                //
                                //
                                //
                                this.VM.conversation.last_message_id = update.message_id;
                                
                                var item = this.HistoryVM.Items.FirstOrDefault((m) => { return (m.id == 0 && m.action == null) || m.id == update.message_id; });
                                if (item != null)
                                {
                                    if (item.id == 0)
                                        item.id = update.message_id;
                                    continue;
                                }
                                

                                VKMessage msgVM = new VKMessage();
                                //
                                //
                                if(this.VM.last_message==null)
                                {
                                    this.VM.last_message = new VKMessage();
                                    this.VM.last_message.from_id = update.user_id;//BugFix: в беседе
                                    this.VM.last_message.@out = update.flags.OUTBOX == true ? VKMessageType.Sent : VKMessageType.Received;
                                    //                                    d.last_message.read_state = !update.flags.UNREAD;
                                    this.VM.last_message.date = Core.Json.UnixtimeToDateTimeConverter.GetDateTimeFromUnixTime(update.timestamp);
                                    this.VM.last_message.text = update.text;
                                    this.VM.last_message.id = update.message_id;


                                    if (update.source_act != VKChatMessageActionType.None)
                                    {
                                        this.VM.last_message.action = new VKMessage.MsgAction() { type = update.source_act, member_id = update.source_mid, text = update.source_text };
                                        this.VM.conversation.peer.type = VKConversationPeerType.Chat;
                                    }
                                    //
                                    this.VM.last_message.attachments = null;//bugfix?
                                    this.VM.last_message.fwd_messages = null;//bugfix?
                                }
                                //
                                //
                                this.VM.last_message.CopyTo(msgVM);

                                this.HistoryVM.Items.Add(msgVM);

                                this.VM.SetUserIsNotTyping(update.user_id);

                                this.HistoryVM.EnsureUnreadItem();

                                if (this.HistoryVM.IsAtBottom && this.IsOnScreen)
                                {
                                    if (Settings.DEV_DisableMarkSeen == false)
                                        this.HistoryVM.SetReadStatusIfNeeded();
                                }
                                break;
                            }
                        case LongPollServerUpdateType.UserIsTypingInChat:
                        case LongPollServerUpdateType.UserIsTyping:
                            {
                                this.VM.SetUserIsTypingWithDelayedReset(update.user_id);
                                break;
                            }
                        case LongPollServerUpdateType.UserBecameOffline:
                            {
                                this.VM.ConversationAvatarVM.Online = false;
                                this.VM.ConversationAvatarVM.platform = VKPlatform.Unknown;
                                this.VM.UpdateOn();
//                                this.HistoryVM.RefreshUIPropertiesSafe();
                                break;
                            }
                        case LongPollServerUpdateType.UserBecameOnline:
                            {
                                this.VM.ConversationAvatarVM.Online = true;
                                this.VM.ConversationAvatarVM.platform = (VKPlatform)update.Platform;
                                this.VM.UpdateOn();
//                                this.HistoryVM.RefreshUIPropertiesSafe();
                                break;
                            }
                        case LongPollServerUpdateType.IncomingMessagesRead:
                        case LongPollServerUpdateType.OutcominggMessagesRead:
                        case LongPollServerUpdateType.MessageHasBeenRead:
                            {
                                uint max_lm = Math.Max(this.VM.conversation.last_message_id, update.message_id);


                                this.VM.conversation.last_message_id = max_lm;
                                if (this.VM.conversation.unread_count > 0)
                                {
                                    //Это я прочёл чужое сообщение
                                    this.VM.conversation.unread_count = 0;
                                    this.VM.conversation.in_read = max_lm;
                                    this.VM.conversation.out_read = max_lm;
                                }
                                else
                                {
                                    if (update.message_id > 0)
                                        this.VM.conversation.out_read = Math.Max(update.message_id, this.VM.conversation.out_read);
                                }

                                for (int i = this.HistoryVM.Items.Count - 1; i >= 0; i--)
                                {
                                    var msg = this.HistoryVM.Items[i];

                                    if (msg.id > update.message_id)
                                        continue;

                                    if (msg.read_state)
                                        break;

                                    msg.read_state = true;
                                    msg.RefreshUIProperties();
                                }
                                break;
                            }
                        case LongPollServerUpdateType.MessageUpdate:
                            {
                                var item = this.HistoryVM.Items.FirstOrDefault((m) => { return m is VKMessage && (m as VKMessage).id == update.message_id; });
                                if (item != null)
                                {
                                    //VKMessage msg = item as VKMessage;
                                    //msg.read_state = true;
                                    //msg.RefreshUIProperties();
                                    int pos = this.HistoryVM.Items.IndexOf(item);
                                    this.HistoryVM.Items.Remove(item);

                                    VKMessage msgVM = new VKMessage();

                                    update.message.CopyTo(msgVM);

                                    this.HistoryVM.Items.Insert(pos, msgVM);
                                }
                                
                                break;
                            }
                        case LongPollServerUpdateType.MessageHasBeenDeleted:
                            {
                                var item = this.HistoryVM.Items.FirstOrDefault((m) => { return m is VKMessage && (m as VKMessage).id == update.message_id; });
                                if (item != null)//А вдруг удалено очень старое сообщение?
                                {
                                    this.HistoryVM.Items.Remove(item);
                                }
                                this.HistoryVM.EnsureUnreadItem();
                                break;
                            }
                    }
                }

            });
        }

        void ucNewMessage_RecordDone(object sender, AudioRecorderUC.VoiceMessageSentEvent message)
        {
            this.HistoryVM.SendVoiceRecord(message.File, message.Duration, message.Waveform);
        }

        void PanelControl_StickerTapped(object sender, VKSticker sticker)
        {
            if (sticker.is_allowed)
                this.HistoryVM.SendMessage(sticker);
            else
                this.HistoryVM.SendStickerAsGraffiti(sticker);
        }


        private void ExtendedListView2_Loaded(object sender, RoutedEventArgs e)
        {
            ExtendedListView3 lv = sender as ExtendedListView3;
            lv.ItemTemplateSelector = this.Resources["msgTemplateSelector"] as DataTemplateSelector;

            Border border = (Border)VisualTreeHelper.GetChild(lv.GetListView, 0);
            this.scroll = (ScrollViewer)border.Child;//inside_scrollViewer
            this.scroll.ViewChanged += sv_ViewChanged;

            lv.GetListView.SelectionChanged += Lv_SelectionChanged;
        }

        private void Lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.InMsgSelectionMode)
                return;

            ListView lv = sender as ListView;

            if (lv.SelectedItems.Count == 0)
            {
                this.ucNewMessage.Visibility = Visibility.Visible;
//                this.Offset.Visibility = Visibility.Visible;
                this.CmdBar.PrimaryCommands.Clear();
                this.CmdBar.SecondaryCommands.Clear();
                this.CmdBar.Visibility = Visibility.Collapsed;
                this.InMsgSelectionMode = false;


                
                lv.SelectionMode = ListViewSelectionMode.None;
                //this.ClearSelection(this.eListView.GetListView);
                //lv.SelectedItems.Clear();//E_UNEXPECTED

                //
                //
                this.ucNewMessage.UpdateVisibilityState();
            }
        }

        void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;

            double val = sv.ScrollableHeight - sv.VerticalOffset;

            if(this.VM!=null && this.HistoryVM!=null && this.HistoryVM.Items.Count>0)
            {
                if (val < 100)
                {
                    if (Settings.DEV_DisableMarkSeen == false)
                    {
                        if(this.VM.conversation.in_read != this.VM.conversation.last_message_id)
                            this.HistoryVM.SetReadStatusIfNeeded(3000);
                    }
                    this.HistoryVM.IsAtBottom = true;
                }
                else
                {
                    this.HistoryVM.IsAtBottom = false;
                }
            }
            
            this.AnimateArrowDown(val > 700);
        }


        private void ItemMessageUC_Holding(object sender, HoldingRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                FrameworkElement element = sender as FrameworkElement;
                this.ShowMenu(element);
            }
        }

        private void ItemMessageUC_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                return;

            FrameworkElement element = sender as FrameworkElement;
            this.ShowMenu(element);
        }

        private void ClearSelection(ListViewBase listView)
        {
            switch (listView.SelectionMode)
            {
                case ListViewSelectionMode.None:
                    break;

                case ListViewSelectionMode.Single:
                    listView.SelectedItem = null;
                    break;

                case ListViewSelectionMode.Multiple:
                case ListViewSelectionMode.Extended:
                    listView.SelectedItems.Clear();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ShowMenu(FrameworkElement element)
        {
            if (this.InMsgSelectionMode)
                return;

            VKMessage vm = element.DataContext as VKMessage;

            PopUP2 menu = new PopUP2();
            menu.Opened += Menu_Opened;

            PopUP2.PopUpItem item = new PopUP2.PopUpItem() { Text = "Выбрать", Icon = new SymbolIcon(Symbol.List) };
            item.Command = new DelegateCommand((args) =>
            {
                //this.eListView.SelectionMode = ListViewSelectionMode.Multiple;
                //this.ClearSelection(this.eListView.GetListView);//this.eListView.GetListView.SelectedItems.Clear();
                //this.eListView.GetListView.SelectedItems.Add(vm);
                this.eListView.GetListView.SelectionMode = ListViewSelectionMode.Multiple;
                this.eListView.GetListView.SelectedItem = vm;
                this.ucNewMessage.ClosePanel();
                this.BuildAppBar();
                this.InMsgSelectionMode = true;
                //
                //
                this.HandlePanelOpened(this.ucNewMessage, 0);
            });
            menu.Items.Add(item);

            PopUP2.PopUpItem item2 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("Conversation_Quote"), Icon = new SymbolIcon(Symbol.MailReply) };
            item2.Command = new DelegateCommand((args) =>
            {
                List<VKMessage> list = new List<VKMessage>();
                list.Add(vm);
                this.HistoryVM.AddForwardedMessagesToOutboundMessage(list);
                this.ucNewMessage.TextBoxNewComment.Focus(FocusState.Programmatic);
            });
            menu.Items.Add(item2);
            
            PopUP2.PopUpItem item3 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("Conversation_Forward"), Icon = new SymbolIcon(Symbol.MailForward) };
            item3.Command = new DelegateCommand((args) =>
            {
                this.selectedMsgs.Add(vm);
                this.InMsgSelectionMode = false;
                this.Back_Tapped(null, null);
            });
            menu.Items.Add(item3);

            PopUP2.PopUpItem item4 = new PopUP2.PopUpItem();
            item4.Text = vm.important ? "Неважное" : "Важное";
            item4.Icon = new SymbolIcon(vm.important ? Symbol.UnFavorite : Symbol.Favorite);
            item4.Command = new DelegateCommand((args) =>
            {
                this.HistoryVM.MarkAsImportant(vm, !vm.important);
            });
            menu.Items.Add(item4);

            PopUP2.PopUpItem item5 = new PopUP2.PopUpItem() { Text = "Удалить", Icon = new SymbolIcon(Symbol.Delete) };
            item5.Command = new DelegateCommand((args) =>
            {
                List<VKMessage> temp = new List<VKMessage>();
                temp.Add(vm);
                this.HistoryVM.DeleteMessages(temp);
            });
            menu.Items.Add(item5);

            double h = (DateTime.Now - vm.date).TotalHours;

            if (vm.@out == VKMessageType.Sent && h < 24 && (vm.attachments == null ? true : vm.attachments.Count == 0) && (vm.fwd_messages == null ? true : vm.fwd_messages.Count == 0))
            {
                PopUP2.PopUpItem item6 = new PopUP2.PopUpItem() { Text = "Редактировать", Icon = new SymbolIcon(Symbol.Edit) };
                item6.Command = new DelegateCommand((args) =>
                {
                    this.ucNewMessage.TextBoxNewComment.Text = vm.text;
                    this.ucNewMessage.ControlMode = NewMessageUC.Mode.EditMessage;
                    this.EditMsgId = vm.id;
                });
                menu.Items.Add(item6);
            }

            if (this.VM.conversation.peer.type == VKConversationPeerType.Chat)
            {
                PopUP2.PopUpItem item7 = new PopUP2.PopUpItem() { Text = "Закрепить", Icon = new SymbolIcon(Symbol.Pin) };
                item7.Command = new DelegateCommand((args) =>
                {
                    this.HistoryVM.Pin(vm);
                });
                menu.Items.Add(item7);
            }

            menu.ShowAt(element);
        }

        private void Menu_Opened(object sender, object e)
        {
            MenuFlyout m = sender as MenuFlyout;
            Style s = new Style { TargetType = typeof(MenuFlyoutPresenter) };
            s.Setters.Add(new Setter(MinHeightProperty, "180"));
            m.MenuFlyoutPresenterStyle = s;
        }


        /// <summary>
        /// Мы в режиме выбора сообщений?
        /// </summary>
        private bool InMsgSelectionMode = false;

        

        private void ArrowDownTap(object sender, TappedRoutedEventArgs e)
        {
            this.scroll.ChangeView(null, this.scroll.ScrollableHeight, null);
        }

        private void Options_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            Point point = e.GetPosition(null);

            PopUP2 menu = new PopUP2();
            //if (popMenu == null)
            //{
            //    popMenu = new UC.PopUP();
            //    popMenu.ItemTapped += popMenu_ItemTapped;
            //}
            /*
             * настройка беседы
             * показать вложения
             * отключить уведомления
             * удалить историю
             * покинуть беседу
             * 
             * обновить
             * на рабочий стол
             * показать вложения
             * отключить уведомления
             * удалить диалог
             * */
            //popMenu.ClearItems();
            PopUP2.PopUpItem item0 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("Messenger_ShowMaterials"), Icon = new IconUC() { Glyph = "\xE723" }};
            item0.Command = new DelegateCommand((args) => { NavigatorImpl.Instance.NavigateToConversationMaterials(this.VM.conversation.peer.id); });
            menu.Items.Add(item0);
            //popMenu.AddItem(0, "Показать вложения", "\xE723");

            bool sound = !this.VM.conversation.AreDisabledNow;
            PopUP2.PopUpItem item1 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString(sound ? "TurnOffNotifications" : "TurnOnNotifications"), Icon = new IconUC() { Glyph = sound ? "\xE7ED" : "\xEA8F" } };
            item1.Command = new DelegateCommand((args) =>
            {
                var c = this.VM;
                AccountService.Instance.SetSilenceMode(PushNotifications.Instance.GetHardwareID,(sound) ? -1 : 0, (res)=>
                {
                    if (res.error.error_code == VKErrors.None && res.response == 1)
                    {
                        //if (vm.conversation.push_settings == null)
                        //    vm.conversation.push_settings = new VKPushSettings();
                        //vm.conversation.push_settings.disabled_until = sound ? -1 : 0;
                        //vm.conversation.push_settings.sound = !sound;
                        //vm.UpdateUI();
                        if (sound)
                        {
                            if (c.conversation.push_settings == null)
                                c.conversation.push_settings = new VKPushSettings();
                            c.conversation.push_settings.disabled_forever = true;//c.conversation.push_settings.disabled_until = -1;
                            //c.conversation.push_settings.sound = false;
                        }
                        else
                        {
                            c.conversation.push_settings = null;
                        }

                        Execute.ExecuteOnUIThread(() =>
                        {
                            c.UpdateUI();
                        });
                    }
                }, c.conversation.peer.id);
                

            });
            menu.Items.Add(item1);
            //if (this.VM.conversation.push_settings == null || this.VM.conversation.push_settings.sound)
            //    popMenu.AddItem(1, "Отключить уведомления", "\xE7ED");
            //else
            //    popMenu.AddItem(1, "Включить уведомления", "\xEA8F");

            PopUP2.PopUpItem item2 = new PopUP2.PopUpItem() { Text = "Удалить историю", Icon = new IconUC() { Glyph = "\xE74D" } };
            item2.Command = new DelegateCommand((args) => { });
            menu.Items.Add(item2);

            //popMenu.AddItem(2, "Удалить историю", "\xE74D", false);

            if (this.VM.conversation.peer.type == VKConversationPeerType.Chat)
            {
                if (this.VM.conversation.chat_settings.state == "left")
                {
                    PopUP2.PopUpItem item3 = new PopUP2.PopUpItem() { Text = "Вернуться в беседу", Icon = new IconUC() { Glyph = "\xE89B" } };
                    item3.Command = new DelegateCommand((args) => {
                        List<int> ids = new List<int>();
                        ids.Add((int)Settings.UserId);
                        ChatService.Instance.AddChatUsers(this.VM.conversation.peer.local_id, ids, null);
                    });
                    menu.Items.Add(item3);
                    //popMenu.AddItem(4, "Вернуться в беседу", "\xE89B");
                }
                else if (this.VM.conversation.chat_settings.state == "in")
                {
                    PopUP2.PopUpItem item4 = new PopUP2.PopUpItem() { Text = "Покинуть беседу", Icon = new IconUC() { Glyph = "\xE89B" } };
                    item4.Command = new DelegateCommand((args) => {
                        ChatService.Instance.RemoveChatUser(this.VM.conversation.peer.local_id, (int)Settings.UserId, null);
                    });
                    menu.Items.Add(item4);

                    PopUP2.PopUpItem item5 = new PopUP2.PopUpItem() { Text = "Добавить собеседника", Icon = new IconUC() { Glyph = "\xE8FA" } };
                    item5.Command = new DelegateCommand((args) => {  });
                    menu.Items.Add(item5);

                    //popMenu.AddItem(3, "Покинуть беседу", "\xE89B");
                    //popMenu.AddItem(6, "Добавить собеседника", "\xE8FA", false);
                }
            }

            PopUP2.PopUpItem item6 = new PopUP2.PopUpItem() { Text = "Обновить", Icon = new IconUC() { Glyph = "\xE895" } };
            item6.Command = new DelegateCommand((args) => {
                this.HistoryVM.RefreshConversations();
            });
            menu.Items.Add(item6);

            PopUP2.PopUpItem item7 = new PopUP2.PopUpItem() { Text = "На рабочий стол", Icon = new IconUC() { Glyph = "\xEDA4" } };
            item7.Command = new DelegateCommand((args) => {
                this.HistoryVM.PinToStart();
            });
            menu.Items.Add(item7);
            //popMenu.AddItem(5, "Обновить", "\xE895");
            //popMenu.AddItem(6, "На рабочий стол", "\xEDA4", false);
            /*
            if (this.VM.conversation.peer.type == VKConversationPeerType.User)
            {
                popMenu.AddItem(7, "Позвонить", "\xE717", false);
            }
            */
            menu.ShowAt(sender as FrameworkElement);
            //popMenu.Show(point);
        }
        /*
        void popMenu_ItemTapped(object argument, int e)
        {
            switch (e)
            {
                case 0:
                    {
                        NavigatorImpl.Instance.NavigateToConversationMaterials(this.VM.conversation.peer.id);
                        break;
                    }
                case 1:
                    {
                        var vm = this.VM;
                        bool need_silence = vm.conversation.push_settings == null || vm.conversation.push_settings.sound;
                        bool res = PushNotifications.Instance.SetSilenceMode((need_silence) ? -1 : 0, null, vm.conversation.peer.id);
                        if (res == true)
                        {
                            if (vm.conversation.push_settings == null)
                                vm.conversation.push_settings = new VKPushSettings();
                            vm.conversation.push_settings.sound = !need_silence;
                            vm.UpdateUI();
                        }
                        break;
                    }
                case 2:
                    {
                        break;
                    }
                case 3:
                    {
                        ChatService.Instance.RemoveChatUser(this.VM.conversation.peer.local_id, (int)Settings.UserId);
                        break;
                    }
                case 4:
                    {
                        List<int> ids = new List<int>();
                        ids.Add((int)Settings.UserId);
                        ChatService.Instance.AddChatUsers(this.VM.conversation.peer.local_id, ids);
                        break;
                    }
                case 5:
                    {
                        //this.ConversationVM.LoadDownAsync(true);
                        this.ConversationVM.RefreshConversations();
                        break;
                    }
                case 6:
                    {
                        this.ConversationVM.PinToStart();
                        break;
                    }
                case 7:
                    {
                        
          //               * g.default.api("messages.sendVoipEvent", {
          // peer_id: c,
          // random_id: (0, y.default)(0, 2147483647),
          // message: JSON.stringify(w)
          //});
          
                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters["peer_id"] = this.ConversationVM.PeerId.ToString();
                        parameters["random_id"] = "1461185421";
                        parameters["message"] = this.VoipMessage;
                        var temp = await Core.Network.RequestsDispatcher.GetResponse<int>("messages.sendVoipEvent", parameters);
                        break;
                    }
            }
        }
        */
        public class VoipMessageClass
        {
            public string sessionGuid { get; set; }
            public string signaling_data { get; set; }

            public class SignalingDataClass
            {
                public AudioClass audio { get; set; }
            }

            public class AudioClass
            {
                public List<string> codecs { get; set; }
            }

            public string type { get; set; }
            public string user_id { get; set; }
            public bool video { get; set; }
        }

        private string VoipMessage
        {
            get
            {

                /*
                VoipMessageClass dictionary = new VoipMessageClass();
                

                dictionary.sessionGuid = "933f960cc7f04862101381fdbb146e5b";
                dictionary.sessionGuid = "933f960cc7f04862101381fdbb146e5b";
                dictionary.sessionGuid = "933f960cc7f04862101381fdbb146e5b";
                dictionary.sessionGuid = "933f960cc7f04862101381fdbb146e5b";
                dictionary.sessionGuid = "933f960cc7f04862101381fdbb146e5b";


                return Newtonsoft.Json.JsonConvert.SerializeObject(dictionary);*/
                return "{\"sessionGuid\":\"933f960cc7f04862101381fdbb146e5b\",\"signaling_data\":\"{\"audio\":{\"codecs\":[\"opus-uwb\",\"opus\",\"isac\",\"speex-wb\",\"speex\",\"g729\",\"pcma\",\"pcmu\",\"g722\",\"ilbc\"]},\"candidate\":[{\"generation\":0,\"ip\":\"192.168.1.87\",\"name\":\"audio_rtp\",\"network_name\":\"0\",\"password\":\"NVReTCG4PNXtW1ws\",\"port\":\"56637\",\"priority\":1.0,\"proto\":\"udp\",\"type\":\"local\",\"username\":\"78LO3+hByuyyH2E+\"}],\"fast_connect\":2,\"jb_flags\":3,\"peerList\":[],\"timeoutSec\":60,\"useragent\":{\"caps\":3847,\"state\":7,\"ua_ver\":\"VKM_PP_build_10\",\"voip_ver\":\"voip win32 release version:0.0.0.0 date:Dec 15 2018 01:44:05\"},\"video\":{\"cap\":{\"cmpl\":-6,\"fps\":24,\"height\":720,\"width\":1280},\"codecs\":[\"h264\",\"vp8\"]},\"zrtp-hash\":\"1.10 6cc56fb4b9e0392aeb69b3998008cde38490d17d3a3bd09c17ab1d92d1721a9f\"}\n\",\"type\":\"invite\",\"user_id\":\"460389\",\"video\":false}";
            }
        }

        private void Avatar_Tapped(object sender, TappedRoutedEventArgs e)
        {//todo:use ConversationVM
            var cur_convrsation = this.VM;
            if (cur_convrsation.conversation.peer.type == VKConversationPeerType.Group || cur_convrsation.conversation.peer.type == VKConversationPeerType.User)
                NavigatorImpl.Instance.NavigateToProfilePage(this.HistoryVM.PeerId);
            if (cur_convrsation.conversation.peer.type == VKConversationPeerType.Chat)
                NavigatorImpl.Instance.NavigateToChatEditPage(cur_convrsation.conversation.peer.local_id);
        }

        private void BorderSend_Tapped()
        {
            string temp = this.ucNewMessage.TextBoxNewComment.Text.Trim().Replace("\r", "\n");

            //temp = "1\x0A\n2";

            this.ucNewMessage.TextBoxNewComment.Text = "";//Очищаем поле

            if (this.ucNewMessage.ControlMode == NewMessageUC.Mode.EditMessage)
            {
                this.HistoryVM.EditMessage(temp, this.EditMsgId);
                this.EditMsgId = 0;
                this.ucNewMessage.ControlMode = NewMessageUC.Mode.NewMessageEmpty;
                return;
            }

            this.HistoryVM.SendMessage(temp);
            this.HistoryVM.Attachments.Clear();
            //
            if(this.eListView.GetInsideScrollViewer.ScrollableHeight - this.eListView.GetInsideScrollViewer.VerticalOffset < 400)//после отправки сообщения перематываем список вниз
            {
                this.eListView.ScrollToBottom(true,false);
            }
        }

        private void BorderSendPayload_Tapped(string text, string payload)
        {
            //string temp = this.ucNewMessage.TextBoxNewComment.Text.Trim().Replace("\r", "\n");
            
            
            this.HistoryVM.SendMessage(text, payload);
        }

        private void TextBoxNewComment_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.HistoryVM.UserIsTyping();
            TextBox tb = sender as TextBox;
            this.ucNewMessage.ActivateSendButton(this.HistoryVM.Attachments.Count > 0 || !string.IsNullOrEmpty(tb.Text));

            this.UpdateMentionPicker();
        }

        private uint EditMsgId;

        private void UpdateMentionPicker()
        {
            if (this.HistoryVM.PeerId < 2000000000 /*|| this.ConversationVM.IsKickedFromChat*/)
                return;

            string text = this.ucNewMessage.TextBoxNewComment.Text;

            if (text.EndsWith("@") || text.EndsWith("*"))
            {
                if (this.ucNewMessage.MentionPicker.ItemsSource == null)
                    this.ucNewMessage.MentionPicker.ItemsSource = this.ChatMembers;

                this.ChatMembers.Clear();

                foreach (VKBaseDataForGroupOrUser u in this.HistoryVM.ChatMembers)
                {
                    this.ChatMembers.Add(u);
                }
            }
            else
            {
                this.ChatMembers.Clear();
            }
        }

        private void ImageDeleteTap(IOutboundAttachment attachment)
        {
            this.HistoryVM.Attachments.Remove(attachment);
        }

        private void AddAttachTap(FrameworkElement sender)
        {
            if (this.HistoryVM.Attachments.Count >= 10)
                return;
            
            if(this._flyout==null)
            {
                this._flyout = new PopUpService();
                this._flyout.OverrideBackKey = true;
                this._flyout.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            }
            
            AttachmentPickerUC uc = new AttachmentPickerUC((byte)this.HistoryVM.Attachments.Count, 10);
            uc.AttachmentsAction = this.HandleAttachmentsAction;
            uc.DrawGraffiti = this.HandleStartGraffitiAction;
            this._flyout.Child = uc;
            this._flyout.Show();
        }

        private void HandleStartGraffitiAction()
        {
            this._flyout.Closed += this.ShowGraffitiDraw;
            this._flyout.Hide();
        }

        private void ShowGraffitiDraw(object sender, EventArgs args)
        {
            this._flyout.Closed -= this.ShowGraffitiDraw;

            this._flyout = new PopUpService();

            GraffitiDrawUC uc = new GraffitiDrawUC();
            uc.SendAction = this.HandleGraffitiAction;
            this._flyout.OverrideBackKey = true;
            this._flyout.Child = uc;
            this._flyout.Show();
        }

        private void HandleGraffitiAction(RenderTargetBitmap data, string localFile)
        {
            this.HistoryVM.SendGraffiti(data, localFile);
            this._flyout.Hide();
        }

        public void HandleAttachmentsAction(IReadOnlyList<IOutboundAttachment> list)
        {
            foreach (var attach in list)
                this.HistoryVM.Attachments.Add(attach);
            
            if (list.Count == 1)
            {
                if (list[0] is OutboundDocumentAttachment doc)
                {
                    if (doc._pickedDocument != null && doc._pickedDocument.IsGraffiti)
                    {
                        this.BorderSend_Tapped();
                    }
                }
            }

            if(this._flyout!=null)
                this._flyout.Hide();
        }

        private void HandleDocumentAction(VKDocument doc)
        {
            OutboundDocumentAttachment o = new OutboundDocumentAttachment(doc);
            this.HistoryVM.Attachments.Add(o);
            this._flyout.Hide();
        }

        private bool ArrowDownAnimating;

        private void AnimateArrowDown(bool show)
        {
            if (this.ArrowDownAnimating)
                return;

            if (show && this.ArrowDownScale.ScaleX == 1.0)
                return;

            if (!show && this.ArrowDownScale.ScaleX == 0.0)
                return;

            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                from = this.ArrowDownScale.ScaleX,
                to = show ? 1.0 : 0.0,
                propertyPath = "ScaleX",
                duration = 200,
                target = this.ArrowDownScale,
                //easing = this.ANIMATION_EASING
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                from = this.ArrowDownScale.ScaleY,
                to = show ? 1.0 : 0.0,
                propertyPath = "ScaleY",
                duration = 200,
                target = this.ArrowDownScale,
                //easing = this.ANIMATION_EASING
            });

            this.ArrowDownAnimating = true;
            AnimationUtils.AnimateSeveral(animInfoList, null, this.AnimateCallback);
        }

        private void AnimateCallback()
        {
            this.ArrowDownAnimating = false;
        }
        
        private void BuildAppBar()
        {
            this.CmdBar.PrimaryCommands.Clear();
            this.CmdBar.SecondaryCommands.Clear();

            //CommandBar applicationBar = new CommandBar();

            AppBarButton btn = new AppBarButton() { Icon = new SymbolIcon(Symbol.MailForward), Label = "переслать", Command = new DelegateCommand((a) => this._appBarButtonForward_Click()) };
            AppBarButton btn2 = new AppBarButton() { Icon = new SymbolIcon(Symbol.Delete), Label = "удалить", Command = new DelegateCommand((a) => this._appBarButtonDelete_Click()) };
            AppBarButton btn3 = new AppBarButton() { Icon = new SymbolIcon(Symbol.Cancel), Label = "отмена", Command = new DelegateCommand((a) => this._appBarButtonCancel_Click()) };
            this.CmdBar.PrimaryCommands.Add(btn);
            this.CmdBar.PrimaryCommands.Add(btn2);
            this.CmdBar.PrimaryCommands.Add(btn3);

            this.CmdBar.Visibility = Visibility.Visible;

            this.ucNewMessage.Visibility = Visibility.Collapsed;
//            this.Offset.Visibility = Visibility.Collapsed;
        }

        private void _appBarButtonCancel_Click()
        {
            this.eListView.GetListView.SelectionMode = ListViewSelectionMode.None;//this.eListView.SelectionMode = ListViewSelectionMode.None;
            //this.eListView.GetListView.SelectedItems.Clear();//E_UNEXPECTED
            //this.ClearSelection(this.eListView.GetListView);

            this.ucNewMessage.Visibility = Visibility.Visible;
//            this.Offset.Visibility = Visibility.Visible;

            this.CmdBar.Visibility = Visibility.Collapsed;
            this.InMsgSelectionMode = false;
        }

        private void _appBarButtonDelete_Click()
        {
            //List<MessageViewModel> list = this.ConversationVM.Messages.Where<MessageViewModel>((Func<MessageViewModel, bool>)(m => m.IsSelected)).ToList<MessageViewModel>();
            //if (MessageBox.Show(CommonResources.Conversation_ConfirmDeletion, list.Count == 1 ? CommonResources.Conversation_DeleteMessage : CommonResources.Conversation_DeleteMessages, (MessageBoxButton)1) != MessageBoxResult.OK)
            //    return;

            List<VKMessage> list = new List<VKMessage>();
            foreach (VKMessage m in this.eListView.GetListView.SelectedItems)
            {
                list.Add(m);
            }


            this.eListView.GetListView.SelectionMode = ListViewSelectionMode.None;//this.eListView.SelectionMode = ListViewSelectionMode.None;
            //this.eListView.GetListView.SelectedItems.Clear();
            //this.ClearSelection(this.eListView.GetListView);

            this.selectedMsgs.Clear();

            this.HistoryVM.DeleteMessages(list,null);

            this.InMsgSelectionMode = false;
            this.ucNewMessage.Visibility = Visibility.Visible;
//            this.Offset.Visibility = Visibility.Visible;

            this.CmdBar.Visibility = Visibility.Collapsed;
        }

        private void _appBarButtonForward_Click()
        {
            //this.InDialogSelectionMode = true;
            this.CmdBar.Visibility = Visibility.Collapsed;
            this.InMsgSelectionMode = false;
            this.ucNewMessage.Visibility = Visibility.Visible;

            foreach (var item in this.eListView.GetListView.SelectedItems)
            {
                VKMessage msg = item as VKMessage;
                this.selectedMsgs.Add(msg);
            }
            this.eListView.GetListView.SelectionMode = ListViewSelectionMode.None;
            this.Back_Tapped(null, null);
        }

        private void HeaderWithMenu_HeaderHeightChanged(object sender, double e)
        {
            this._pinnedMsg.Margin = new Thickness(0, e, 0, 0);
        }
        /*
        private void ucNewMessage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Offset.Height = e.NewSize.Height;//binding плохо работает :(
        }
        */
        private void Attachments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.ucNewMessage.ActivateSendButton(this.HistoryVM.Attachments.Count > 0 || !string.IsNullOrEmpty(this.ucNewMessage.TextBoxNewComment.Text));

            if (this.HistoryVM.Attachments.Count == 0)
            {
                if(this.ShowingMoveSpline.Value > 0)
                    this.HandlePanelOpened(this.ucNewMessage, 0);
            }
            else
                this.HandlePanelOpened(this.ucNewMessage, 90);
                
        }

        public Action BackCall;

        private void Back_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //this.VM.HistoryVM.PeerId = 0;
            //this.VM.conversation.peer.id = 0;//todo: надо удалять ВМ чтобы приём событий не срабатывал
            BackCall?.Invoke();
        }

#region IScroll
        public void ScrollToUnreadItem()
        {
            //Execute.ExecuteOnUIThread((() =>
            //{
            //    if (this.ConversationItems != null)
                    this.ScrollToItem(this.HistoryVM.Items.FirstOrDefault((ci => ci.action != null && ci.action.type == VKChatMessageActionType.UNREAD_ITEM_ACTION)));
            //    else
            //        this._shouldScrollToUnreadItem = true;
            //}));
        }

        public void ScrollToMessageId(uint messageId)
        {
            //Execute.ExecuteOnUIThread((Action)(() =>
            //{
            //    if (this.ConversationItems != null)
                    this.ScrollToItem(this.HistoryVM.Items.FirstOrDefault((ci => ci.id == messageId)));
            //    else
            //        this._messageIdToScrollTo = messageId;
            //}));
        }

        private void ScrollToItem(object item)
        {
            if (item == null)
                return;
            //this.eListView.ScrollTo(Math.Max(0.0, this.myPanel.GetScrollOffsetForItem(this.ConversationItems.Messages.IndexOf(item)) + item.FixedHeight - (this.Orientation == PageOrientation.Landscape || this.Orientation == PageOrientation.LandscapeLeft || this.Orientation == PageOrientation.LandscapeRight ? 200.0 : 400.0)));
            this.eListView.ScrollToItem(item);
        }

        public void ScrollToBottom(bool animated = true, bool onlyIfInTheBottom = false)
        {
            if (!(!onlyIfInTheBottom | this.VerticalOffset < 50.0))
                return;
            if (animated)
                this.eListView.ScrollToBottom(false);
            else
            {
                //this.myPanel.ScrollTo(0.0); - в оригинале

                if(this.eListView.Items.Count>0)
                    this.eListView.ScrollToItem(this.eListView.Items.Last());//this.eListView.ScrollTo(this.eListView.GetInsideScrollViewer.ActualHeight);
                else
                    this.eListView.ScrollToBottom(true);




                //this.eListView.ScrollToBottom(true);//старый рабочий вариант
            }
        }

        public double VerticalOffset
        {
            get
            {
                if (this.eListView.GetInsideScrollViewer == null)
                    return 0;
                return this.eListView.GetInsideScrollViewer.VerticalOffset;
            }
        }

        public void ScrollToOffset(double offset)
        {
            this.eListView.ScrollTo(offset);
        }
#endregion

        private void ItemsStackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
            {
                //The KeepLastItemInView enum member was introduced in the 14393 SDK.
                ItemsStackPanel panel = sender as ItemsStackPanel;
                panel.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
            }
        }
    }
}
