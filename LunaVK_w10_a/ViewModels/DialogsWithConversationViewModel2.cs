using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Enums;
using LunaVK.Core;
using LunaVK.Core.ViewModels;
using LunaVK.Library;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Controls;
using LunaVK.Core.Framework;
using System.IO;

namespace LunaVK.ViewModels
{
    //ConversationsViewModel
    public class DialogsViewModel : GenericCollectionViewModel<ConversationWithLastMsg>, IBinarySerializable
    {
        private static object _instLock = new object();
        private uint? group_id;

        private static DialogsViewModel _instance;
        public static DialogsViewModel Instance
        {

            get
            {
                if (DialogsViewModel._instance == null)
                {
                    lock (DialogsViewModel._instLock)
                    {
                        DialogsViewModel conversationsViewModel = new DialogsViewModel();
                        CacheManager.TryDeserialize(conversationsViewModel, "Dialogs");
                        if (DialogsViewModel._instance == null)
                            DialogsViewModel._instance = conversationsViewModel;
                    }
                }
                return DialogsViewModel._instance;
            }
#if DEBUG
            set
            {
                DialogsViewModel._instance = value;
            }
#endif
        }



#region Data
        public override bool HasMoreDownItems
        {
            get
            {
                if (this.InSearch)
                    return this.SearchItems.Count == 0 || this.SearchItems.Count < this.searchMaximum;
                return base.HasMoreDownItems;
            }
        }

        public int DialogsSource { get; set; }

        public bool IsOnScreen { get; set; }
#endregion




        public DialogsViewModel()
        {
            Network.LongPollServerService.Instance.ReceivedUpdates += this.ProcessInstantUpdates;
        }

        public DialogsViewModel(uint groupId):this()
        {
            this.group_id = groupId;

            if (groupId>0)
                Network.LongPollServerService.Instance.SwitchGroup(groupId);
        }

        ~DialogsViewModel()
        {
            Network.LongPollServerService.Instance.ReceivedUpdates -= this.ProcessInstantUpdates;

            if (this.group_id.HasValue)
                Network.LongPollServerService.Instance.SwitchGroup(0);
        }

        /// <summary>
        /// Контейнер со списком диалогов
        /// </summary>
        public ListView SubscribedListView;

        private void ProcessInstantUpdates(List<UpdatesResponse.LongPollServerUpdateData> updates)
        {
            Execute.ExecuteOnUIThread(() =>
            {
                foreach (UpdatesResponse.LongPollServerUpdateData update in updates)
                {
                    ConversationWithLastMsg dialog = base.Items.FirstOrDefault((d) => { return d.conversation.peer.id == update.peer_id; });

                    switch (update.UpdateType)
                    {
                        #region MessageAdd
                        case LongPollServerUpdateType.MessageAdd://MessageHasBeenAdded
                            {

                                if (dialog == null)
                                {

                                    //Новый диалог, с кем мы ещё не общались или не загрузились
                                    dialog = new ConversationWithLastMsg();

                                    dialog.conversation = new VKConversation();
                                    dialog.conversation.peer = new VKConversation.ConversationPeer();
                                    dialog.conversation.peer.id = update.peer_id;
                                    dialog.conversation.peer.type = update.peer_id > 0 ? VKConversationPeerType.User : VKConversationPeerType.Group;
                                    dialog.last_message = new VKMessage();

                                    if (!string.IsNullOrEmpty(update.source_text))
                                    {
                                        dialog.conversation.chat_settings = new VKConversation.ConversationSettings();
                                        dialog.conversation.chat_settings.title = update.source_text;
                                        dialog.conversation.chat_settings.active_ids = new List<int>();
                                    }



                                }

                                if (update.message != null)
                                    dialog.last_message = update.message;
                                else
                                {
                                    dialog.last_message.from_id = update.user_id;//BugFix: в беседе
                                    dialog.last_message.@out = update.flags.OUTBOX == true ? VKMessageType.Sent : VKMessageType.Received;
                                    //                                    d.last_message.read_state = !update.flags.UNREAD;
                                    dialog.last_message.date = Core.Json.UnixtimeToDateTimeConverter.GetDateTimeFromUnixTime(update.timestamp);
                                    dialog.last_message.text = update.text;
                                    dialog.last_message.id = update.message_id;


                                    if (update.source_act != VKChatMessageActionType.None)
                                    {
                                        dialog.last_message.action = new VKMessage.MsgAction() { type = update.source_act, member_id = update.source_mid, text = update.source_text };
                                        dialog.conversation.peer.type = VKConversationPeerType.Chat;
                                    }
                                    //
                                    dialog.last_message.attachments = null;//bugfix?
                                    dialog.last_message.fwd_messages = null;//bugfix?
                                }

                                dialog.conversation.last_message_id = update.message_id;
                                //dialog.UIBody = update.text;
                                if (update.flags.OUTBOX == false)
                                    dialog.conversation.unread_count++;

                                string temp2 = DialogsViewModel.GetMessageHeaderText(dialog.last_message, null, null);
                                dialog.UIBody = temp2;
                                dialog.SetUserIsNotTyping(update.user_id);//dialog.StopAnimTyping();

                                if (!base.Items.Contains(dialog))
                                {
                                    base.Items.Insert(0, dialog);
                                }
                                else
                                {
                                    if (base.Items.IndexOf(dialog) != 0)
                                    {
                                        object selected = null;
                                        if (this.SubscribedListView != null)
                                            selected = this.SubscribedListView.SelectedItem;

                                        base.Items.Remove(dialog);
                                        base.Items.Insert(0, dialog);

                                        if (this.SubscribedListView != null)
                                            this.SubscribedListView.SelectedItem = selected;
                                    }
                                    dialog.RefreshUIProperties();
                                }

                                if (!this.IsOnScreen)
                                    continue;
/*
                                if (dialog._historyVM != null)
                                {
                                    if (dialog._historyVM.PeerId != update.peer_id)
                                        continue;

                                    dialog._historyVM.Conversation.last_message_id = dialog.last_message.id;
                                    //
                                    var item = dialog._historyVM.Items.FirstOrDefault((m) => { return m.id == 0 || m.id == update.message_id; });
                                    if (item != null)
                                    {
                                        if (item.id == 0)
                                            item.id = update.message_id;
                                        continue;
                                    }
                                    //

                                    VKMessage msgVM = new VKMessage();

                                    dialog.last_message.CopyTo(msgVM);

                                    //dialog.HistoryVM.Items.Insert(0, msgVM);
                                    dialog._historyVM.Items.Add(msgVM);

                                    dialog._historyVM.SetUserIsNotTyping(update.user_id);
                                    
                                    dialog._historyVM.EnsureUnreadItem();

                                    if (dialog._historyVM.IsAtBottom)
                                    {
                                        if (Settings.DEV_DisableMarkSeen == false)
                                            dialog._historyVM.SetReadStatusIfNeeded();
                                    }
                                }
*/

                                break;
                            }
                        #endregion

                        #region UserIsTyping
                        case LongPollServerUpdateType.UserIsTypingInChat:
                        case LongPollServerUpdateType.UserIsTyping:
                            {
                                if (dialog == null)
                                    continue;

                                //dialog.AnimTyping(update.user_id);
                                dialog.SetUserIsTypingWithDelayedReset(update.user_id);
/*
                                if (dialog._historyVM != null)
                                {
                                    dialog._historyVM.SetUserIsTypingWithDelayedReset(update.user_id);
                                }
*/
                                break;
                            }
                        #endregion

                        case LongPollServerUpdateType.UserIsRecordingVoice:
                            {
                                if (dialog == null)
                                    continue;

                                //dialog.AnimRecording(update.user_id);
                                break;
                            }

#region MessageHasBeenRead
                        case LongPollServerUpdateType.IncomingMessagesRead:
                        case LongPollServerUpdateType.OutcominggMessagesRead:
                        case LongPollServerUpdateType.MessageHasBeenRead:
                            {
                                if (dialog == null)
                                    continue;


                                //todo: возможный баг: если человек прочёл не все сообщения

                                uint max_lm = Math.Max(dialog.conversation.last_message_id, update.message_id);


                                dialog.conversation.last_message_id = max_lm;

                                if (dialog.conversation.unread_count > 0)
                                {
                                    //Это я прочёл чужое сообщение
                                    dialog.conversation.unread_count = 0;
                                    dialog.conversation.in_read = max_lm;
                                    dialog.conversation.out_read = max_lm;
                                }
                                else
                                {
                                    if (update.message_id > 0)
                                        dialog.conversation.out_read = Math.Max(update.message_id, dialog.conversation.out_read);
                                }
                                //if (update.message_id > d.conversation.in_read && update.message_id > d.conversation.out_read)//BugFix: прочитал несколько сообщений, а иди удут по-убыванию
                                //    d.conversation.in_read = d.conversation.out_read = update.message_id;
                                //d.conversation.unread_count = 0;
                                //d.conversation.last_message_id = Math.Max(d.conversation.last_message_id, update.message_id);
                                dialog.RefreshUIProperties();
/*
                                if (dialog._historyVM != null)
                                {
                                    if (update.peer_id != dialog._historyVM.PeerId)
                                        continue;

                                    for(int i= dialog._historyVM.Items.Count-1;i>=0;i--)
                                    {
                                        var msg = dialog._historyVM.Items[i];
                                        
                                        if (msg.id > update.message_id)
                                            continue;

                                        if (msg.read_state)
                                            break;

                                        msg.read_state = true;
                                        msg.RefreshUIProperties();
                                    }
                                }
*/
                                break;
                            }
#endregion
                        case LongPollServerUpdateType.MessageUpdate:
                            {
/*
                                if (dialog._historyVM != null)
                                {
                                    if (update.peer_id != dialog._historyVM.PeerId)
                                        continue;

                                    var item = dialog._historyVM.Items.FirstOrDefault((m) => { return m is VKMessage && (m as VKMessage).id == update.message_id; });
                                    if (item != null)
                                    {
                                        //VKMessage msg = item as VKMessage;
                                        //msg.read_state = true;
                                        //msg.RefreshUIProperties();
                                        int pos = dialog._historyVM.Items.IndexOf(item);
                                        dialog._historyVM.Items.Remove(item);

                                        VKMessage msgVM = new VKMessage();

                                        update.message.CopyTo(msgVM);

                                        dialog._historyVM.Items.Insert(pos, msgVM);
                                    }
                                }
*/
                                break;
                            }
                        case LongPollServerUpdateType.MessageHasBeenDeleted:
                            {
                                if (dialog == null)//мы удалили беседу и её же нет
                                    continue;
/*
                                if (dialog._historyVM != null)
                                {
                                    if (update.peer_id != dialog._historyVM.PeerId)
                                        continue;
                                    var item = dialog._historyVM.Items.FirstOrDefault((m) => { return m is VKMessage && (m as VKMessage).id == update.message_id; });
                                    if (item != null)//А вдруг удалено очень старое сообщение?
                                    {
                                        dialog._historyVM.Items.Remove(item);
                                    }
                                }
*/
                                if (dialog.last_message.id == update.message_id)
                                {
                                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                                    parameters["peer_id"] = dialog.conversation.peer.id.ToString();
                                    parameters["count"] = "1";

                                    VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKMessage>>("messages.getHistory", parameters, (result) =>
                                    {
                                        if (result.error.error_code == VKErrors.None)
                                        {
                                            VKMessage last_msg = result.response.items[0];
                                            if (last_msg.id < dialog.last_message.id)
                                                dialog.conversation.unread_count = 0;
                                            dialog.last_message = last_msg;
                                            //todo:
                                            //UsersService.Instance.GetCachedUser();
                                            VKUser user1 = null;//result.response.profiles.Find((pro) => pro.id == msg.from_id);
                                            VKUser user02 = null;

                                            //if (msg.action != null)
                                            //    user02 = result.response.profiles.Find((pro) => pro.id == msg.action.member_id);

                                            Execute.ExecuteOnUIThread(() =>
                                            {
                                                dialog.UIBody = DialogsViewModel.GetMessageHeaderText(last_msg, user1, user02);
                                                dialog.RefreshUIProperties();

                                                int offset = 0;
                                                object selected = null;
                                                if (this.SubscribedListView != null)
                                                    selected = this.SubscribedListView.SelectedItem;

                                                foreach (var item in base.Items)
                                                {
                                                    if (item == dialog)
                                                        continue;

                                                    if (item.last_message.date > last_msg.date)
                                                    {
                                                        offset++;
                                                    }
                                                    else
                                                    {
                                                        if (offset != 0)
                                                        {
                                                            base.Items.Remove(dialog);
                                                            base.Items.Insert(offset, dialog);
                                                        }

                                                        break;
                                                    }

                                                    if (offset == base.Items.Count - 1)
                                                    {
                                                        base.Items.Remove(dialog);
                                                        break;
                                                    }
                                                }

                                                if(this.SubscribedListView != null)
                                                    this.SubscribedListView.SelectedItem = selected;//BugFix: позвращаем выделение
                                            });


                                        }
                                    });
                                }
                                break;
                            }
                        case LongPollServerUpdateType.ChatParamsChanged:
                            {
                                if (dialog == null)
                                    continue;

                                ChatService.Instance.GetChatInfo(update.peer_id - 2000000000, (chatInfo) =>
                                {
                                    if (chatInfo == null)
                                        return;

                                    Execute.ExecuteOnUIThread(() => {
                                        dialog.conversation.chat_settings.title = chatInfo.response.chat.title;
                                        //todo: UpdateUI Title
                                        //conversationHeader._associatedUsers.Clear();
                                        /*
                                        foreach (ChatUser chatParticipant in chatInfo.chat_participants)
                                        {
                                            GroupOrUser user = new GroupOrUser()
                                            {
                                                id = chatParticipant.uid,
                                                online = chatParticipant.online,
                                                photo_max = chatParticipant.photo_max,
                                                first_name = chatParticipant.first_name,
                                                last_name = chatParticipant.last_name,
                                                first_name_acc = chatParticipant.first_name_acc,
                                                last_name_acc = chatParticipant.last_name_acc
                                            };
                                            conversationHeader._associatedUsers.Add(user);
                                        }*/
                                        //conversationHeader._message.chat_active_str = chatInfo.chat_participants.Select<VKProfileBase, int>((c => c.id)).ToList<int>().GetCommaSeparated();

                                        dialog.RefreshUIProperties();
                                    });

                                });
                                break;
                            }
                        case LongPollServerUpdateType.UserBecameOffline:
                            {
                                dialog = base.Items.FirstOrDefault((d) => { return d.conversation.peer.id == update.user_id; });
                                if (dialog != null && this.IsOnScreen)
                                {
                                    if (update.user_id > 0)
                                    {
                                        var user = UsersService.Instance.GetCachedUser((uint)update.user_id);
                                        if (user == null)
                                            break;

                                        user.online = false;

                                        if (user.last_seen == null)
                                            user.last_seen = new VKLastSeen();

                                        user.last_seen.time = DateTime.Now;


                                        dialog.UpdateOn();
                                    }
                                }
                                break;
                            }
                        case LongPollServerUpdateType.UserBecameOnline:
                            {
                                dialog = base.Items.FirstOrDefault((d) => { return d.conversation.peer.id == update.user_id; });
                                if (dialog != null && this.IsOnScreen)
                                {
                                    if (update.user_id > 0)
                                    {
                                        var user = UsersService.Instance.GetCachedUser((uint)update.user_id);
                                        if (user == null)
                                            break;

                                        user.online = true;

                                        if (user.last_seen == null)
                                            user.last_seen = new VKLastSeen();

                                        user.last_seen.time = DateTime.Now;
                                        user.last_seen.platform = (VKPlatform)update.Platform;

                                        dialog.UpdateOn();
                                    }

                                }
                                break;
                            }
                    }
                }
            });
        }

        public override void OnRefresh()
        {
            //заглушка, чтобы список не очищался
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<ConversationWithLastMsg>> callback)
        {
            if(offset==0 && this.Items.Count>0)
                base.SetInProgress(true);

            if (this.InSearch)
            {
                this.SearchNext();
                return;
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["count"] = "20";

            if (this.group_id.HasValue)
                parameters["group_id"] = this.group_id.Value.ToString();

            if (this.DialogsSource == 2)
                parameters["filter"] = "unread";


            parameters["fields"] = "online,first_name_acc,last_name_acc,photo_100,last_seen,verified,push_settings";
            parameters["extended"] = "1";

            if (offset>0)
                parameters["offset"] = offset.ToString();

            //VKMessenger.Backend.BackendServices.MessagesService.GetDialogs(new GetDialogsRequest(offset, count, 60), callback);
//#if DEBUG
//            RequestsDispatcher.GetResponseFromDump<VKDialogsGetObject>(100, "conv.json", (result) =>
//#else
             VKRequestsDispatcher.DispatchRequestToVK<VKDialogsGetObject>("messages.getConversations", parameters,(result) =>
//#endif
            {
                base.SetInProgress(false);

                if (result.error.error_code == VKErrors.None)
                {
                    base._totalCount = result.response.count;

                    UsersService.Instance.SetCachedUsers(result.response.profiles);
                    GroupsService.Instance.SetCachedGroups(result.response.groups);

                    Execute.ExecuteOnUIThread(() =>
                    {
                        if (offset==0)
                        {
                            int num_to_leave = Math.Min((int)base._totalCount, 20);
                            int num_to_remove = base.Items.Count - num_to_leave;

                            while (num_to_remove > 0)
                            {
                                num_to_remove--;
                                base.Items.RemoveAt(base.Items.Count - 1);
                            }
                        }

                        uint pos = 0;
                        foreach (var dialog in result.response.items)
                        {
                            VKConversation conversation = dialog.conversation;
                            VKMessage msg = dialog.last_message;

                            VKUser user1 = result.response.profiles.Find((pro) => pro.id == msg.from_id);
                            VKUser user02 = null;

                            if (msg.action != null)
                                user02 = result.response.profiles.Find((pro) => pro.id == msg.action.member_id);

                            dialog.UIBody = DialogsViewModel.GetMessageHeaderText(msg, user1, user02);

                            if (offset==0)
                                this.Merge(dialog, pos);
                            else
                                base.Items.Add(dialog);

                            pos++;
                        }
                    });
                    callback(result.error, null);
                }
                else
                {
                    callback(result.error, null);
                    /*
                    if(result.error.error_code!= VKErrors.NoNetwork)
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            new UC.GenericInfoUC(3000).ShowAndHideLater(result.error.error_msg);
                        });
                    }*/
                }
            }, (jsonStr)=> {
                return VKRequestsDispatcher.FixFalseArray(jsonStr, "last_message", true);
            });
        }

        public override string GetFooterTextForCount
        {
            get
            {
                if (base._totalCount <= 0)
                    return LocalizedStrings.GetString("Conversations_NoDialogs");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "Conversations_OneDialogFrm", "Conversations_TwoFourDialogsFrm", "Conversations_FiveDialogsFrm");
            }
        }

        public static string GetMessageHeaderText(VKMessage message, VKUser user, VKUser user2)
        {
            if (!string.IsNullOrWhiteSpace(message.text))
            {
                string input = NavigatorImpl.Regex_DomainMention.Replace(message.text, "[$2|$4]");
                return NavigatorImpl.Regex_Mention.Replace(input, "$4");
            }
            if (message.action != null && message.action.type != VKChatMessageActionType.None)
                return DialogsViewModel.GenerateText(message, user, user2, false);
            if (message.attachments != null && message.attachments.Count > 0)
            {
                VKAttachment firstAttachment = message.attachments[0];
                int num = message.attachments.FindAll((a => a.type == firstAttachment.type)).Count;

                if(num != message.attachments.Count)
                {
                    return UIStringFormatterHelper.FormatNumberOfSomething(message.attachments.Count, "OneAttachment", "TwoFourAttachmentsFrm", "FiveMoreAttachmentsFrm");
                }

                //if (firstAttachment.type == VKAttachmentType. "money_transfer")
                //    return "CommonResources.MoneyTransfer";
                if (firstAttachment.type == VKAttachmentType.Link)
                {
                    //https://m.vk.com/settings?act=transfers
                    return LocalizedStrings.GetString(firstAttachment.link.url.Contains("vk.com/story") ? "Story" : "Link");
                }
                else if (firstAttachment.type == VKAttachmentType.Wall)
                    return LocalizedStrings.GetString("WallPost");
                else if (firstAttachment.type == VKAttachmentType.Gift)
                    return LocalizedStrings.GetString("Gift");
                else if (firstAttachment.type == VKAttachmentType.Photo)
                    return UIStringFormatterHelper.FormatNumberOfSomething(num, "OnePhoto", "TwoFourPhotosFrm", "FiveOrMorePhotosFrm");
                else if (firstAttachment.type == VKAttachmentType.Wall_reply)
                    return LocalizedStrings.GetString("Comment");
                else if (firstAttachment.type == VKAttachmentType.Sticker)
                    return LocalizedStrings.GetString("Sticker");
                else if (firstAttachment.type == VKAttachmentType.Poll)
                    return LocalizedStrings.GetString("Poll");
                else if (firstAttachment.type == VKAttachmentType.Market)
                    return LocalizedStrings.GetString("Product");
                //new api
                else if (firstAttachment.type == VKAttachmentType.Audio_Message)
                    return LocalizedStrings.GetString("VoiceMessage");
                else if (firstAttachment.type == VKAttachmentType.Graffiti)
                    return LocalizedStrings.GetString("AttachmentType_Graffiti");
                else if (firstAttachment.type == VKAttachmentType.Call)
                    return LocalizedStrings.GetString(firstAttachment.call.initiator_id == Settings.UserId ? "AttachmentType_OutcomingCall" : "AttachmentType_IncomingCall");
                //new api 5.101
                else if (firstAttachment.type == VKAttachmentType.Event)
                    return LocalizedStrings.GetString("CommunityType_Event/Text");
                else if (firstAttachment.type == VKAttachmentType.Doc)
                {
                    VKDocument doc1 = firstAttachment.doc;
                    if (doc1 != null && doc1.IsGraffiti)
                        return LocalizedStrings.GetString("AttachmentType_Graffiti");
                    if (doc1 != null && doc1.IsVoiceMessage)
                        return LocalizedStrings.GetString("VoiceMessage");

                    return UIStringFormatterHelper.FormatNumberOfSomething(num, "OneDocument", "TwoFourDocumentsFrm", "FiveMoreDocumentsFrm");
                }

                else if (firstAttachment.type == VKAttachmentType.Audio)
                {
                    return UIStringFormatterHelper.FormatNumberOfSomething(num, "OneAudio", "TwoFourAudioFrm", "FiveOrMoreAudioFrm");
                }

                else if (firstAttachment.type == VKAttachmentType.Video)
                {
                    return UIStringFormatterHelper.FormatNumberOfSomething(num, "OneVideoFrm", "TwoFourVideosFrm", "FiveOrMoreVideosFrm");
                }

                else if (firstAttachment.type == VKAttachmentType.Podcast)
                {
                    return UIStringFormatterHelper.FormatNumberOfSomething(num, "OnePodcastFrm", "TwoFourPodcastsFrm", "FiveOrMorePodcastsFrm");
                }
            }

            if (message.geo != null)
                return LocalizedStrings.GetString("AttachmentType_Location");
            if (message.fwd_messages == null || message.fwd_messages.Count <= 0)
                return string.Empty;

            return UIStringFormatterHelper.FormatNumberOfSomething(message.fwd_messages.Count, "OneForwardedMessage", "TwoFourForwardedMessagesFrm", "FiveMoreForwardedMessagesFrm");
        }

        /// <summary>
        /// Текст от событий в чате
        /// </summary>
        /// <param name="message"></param>
        /// <param name="user1"></param>
        /// <param name="user2"></param>
        /// <param name="extendedText"></param>
        /// <returns></returns>
        public static string GenerateText(VKMessage message, VKUser user1, VKUser user2, bool extendedText)
        {
            if (message == null)
                return "";
            if (user1 == null)
                user1 = new VKUser();
            if (user2 == null)
                user2 = new VKUser();

            string user_name = DialogsViewModel.CreateUserNameText(user1, false, extendedText);
            string str2 = /*user2.id > -2000000000L ?*/ DialogsViewModel.CreateUserNameText(user2, true, extendedText);// : message.action.email;//todo: было user2.id > -2000000000L
            //if (!extendedText)
            //    str1 = "";
            string str3 = "";
            VKChatMessageActionType action = message.action.type;

            switch (action)
            {
                case VKChatMessageActionType.ChatPhotoUpdate:
                    {
                        str3 = string.Format(LocalizedStrings.GetString(user1.IsFemale ? "ChatPhotoUpdateFemaleFrm" : "ChatPhotoUpdateMaleFrm"), user_name);
                        break;
                    }
                case VKChatMessageActionType.ChatPhotoRemove:
                    {
                        str3 = string.Format(LocalizedStrings.GetString(user1.IsFemale ? "ChatPhotoDeleteFemaleFrm" : "ChatPhotoDeleteMaleFrm"), user_name);
                        break;
                    }
                case VKChatMessageActionType.ChatCreate:
                    {
                        str3 = string.Format(LocalizedStrings.GetString(user1.IsFemale ? "ChatCreateFemaleFrm" : "ChatCreateMaleFrm"), user_name, message.action.text);
                        break;
                    }
                case VKChatMessageActionType.ChatTitleUpdate:
                    {
                        str3 = string.Format(LocalizedStrings.GetString(user1.IsFemale ? "ChatTitleUpdateFemaleFrm" : "ChatTitleUpdateMaleFrm"), user_name, message.action.text);
                        break;
                    }
                case VKChatMessageActionType.ChatInviteUser:
                    {
                        str3 = message.action.member_id != (long)message.from_id ? (string.Format(LocalizedStrings.GetString(user1.IsFemale ? "ChatInviteFemaleFrm" : "ChatInviteMaleFrm"), user_name, str2)) : (string.Format(LocalizedStrings.GetString(user1.IsFemale ? "ChatReturnedToConversationFemaleFrm" : "ChatReturnedToConversationMaleFrm"), user_name));
                        break;
                    }
                case VKChatMessageActionType.ChatKickUser:
                    {
                        str3 = message.action.member_id != (long)message.from_id ? (string.Format(LocalizedStrings.GetString(user1.IsFemale ? "ChatKickoutFemaleFrm" : "ChatKickoutMaleFrm"), user_name, str2)) : (string.Format(LocalizedStrings.GetString(user1.IsFemale ? "ChatLeftConversationFemaleFrm" : "ChatLeftConversationMaleFrm"), user_name));
                        break;
                    }
                case VKChatMessageActionType.ChatPinMessage:
                    {
                        str3 = string.Format(LocalizedStrings.GetString(user1.IsFemale ? "ChatPinMsgFemaleFrm" : "ChatPinMsgMaleFrm"), user_name, "");
                        break;
                    }
                case VKChatMessageActionType.ChatUnpinMessage:
                    {
                        str3 = string.Format(LocalizedStrings.GetString(user1.IsFemale ? "ChatUnPinMsgFemaleFrm" : "ChatUnPinMsgMaleFrm"), user_name);
                        break;
                    }
                case VKChatMessageActionType.ChatInviteUserByLink:
                    {
                        str3 = string.Format(LocalizedStrings.GetString(user1.IsFemale ? "ChatInviteUserByLinkFemaleFrm" : "ChatInviteUserByLinkMaleFrm"), user_name);
                        break;
                    }
            }
            return str3.Trim();
        }

        private static string CreateUserNameText(VKUser user, bool isAcc, bool extendedText)
        {
            string str = isAcc ? user.NameAcc : user.Title;
            if (!extendedText)
                return str;
            if (user.id > 0)
                return string.Format("[id{0}|{1}]", user.id, str);
            return string.Format("[club{0}|{1}]", -user.id, str);
        }

        /// <summary>
        /// Сращивание списка диалогов
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="position"></param>
        private void Merge(ConversationWithLastMsg dialog, uint position)
        {
            if (position >= base.Items.Count)
            {
                base.Items.Add(dialog);
                return;
            }

            //1) Получаем сообщение на нужной позиции
            ConversationWithLastMsg d = base.Items[(int)position];

            //2) Если он не такое же
            if (d.conversation.peer.id != dialog.conversation.peer.id)
            {
                object selected = null;
                if (this.SubscribedListView != null)
                    selected = this.SubscribedListView.SelectedItem;

                //получаем позицию диалога с новым сообщением в списке
                ConversationWithLastMsg true_pos_d = base.Items.FirstOrDefault((di) => di.conversation.peer.id == dialog.conversation.peer.id);

                if (true_pos_d == null)
                {
                    //не существует
                    base.Items.RemoveAt((int)position);//удаляем то что на нужной нам позиции
                    base.Items.Insert((int)position, dialog);//и помещаем туда диалог с новым сообщением
                }
                else
                {//существует
                    base.Items.RemoveAt((int)position);//удаляем то что на нужной нам позиции
                                                         //base.Items.Insert((int)position, dialog);//помещаем туда диалог с новым сообщением
                    base.Items.Remove(true_pos_d);//base.Items.RemoveAt(true_pos);

                    if (position >= base.Items.Count)
                    {
                        base.Items.Add(true_pos_d);
                    }
                    else
                    {
                        base.Items.Insert((int)position, true_pos_d);
                    }

                    /*
                    d.last_message = dialog.last_message;
                    d.UIBody = dialog.UIBody;
                    d.conversation = dialog.conversation;


                    d.RefreshUIProperties();
                    d.UpdateOn();
                    */

                    true_pos_d.last_message = dialog.last_message;
                    true_pos_d.UIBody = dialog.UIBody;
                    true_pos_d.conversation = dialog.conversation;


                    true_pos_d.RefreshUIProperties();
                    true_pos_d.UpdateOn();
                }

                if (this.SubscribedListView != null)
                    this.SubscribedListView.SelectedItem = selected;
            }
            else
            {
                d.last_message = dialog.last_message;
                d.UIBody = dialog.UIBody;
                d.conversation = dialog.conversation;



                d.RefreshUIProperties();
            }

        }

        public async void MultipleSend(IList<ConversationWithLastMsg> list, string text, string attachment)
        {
            string code = "";

            foreach (var dialog in list)
            {
                int peedId = dialog.conversation.peer.id;
                code += ("API.messages.send({peer_id:" + peedId);

                code += (",random_id:" + Convert.ToInt32((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds));

                if (!string.IsNullOrEmpty(text))
                    code += (",message:\"" + text + "\"");
                if (!string.IsNullOrEmpty(attachment))
                    code += (",attachment:\"" + attachment + "\"");
                code += "});";
            }

            VKRequestsDispatcher.Execute<VKResponse<int>>(code,null);
        }

        public void SilentUnsilent(ConversationWithLastMsg c)
        {
            bool need_silence = !c.conversation.AreDisabledNow;
            AccountService.Instance.SetSilenceMode(PushNotifications.Instance.GetHardwareID,(need_silence) ? -1 : 0, (res)=>{
                if (res.error.error_code== VKErrors.None && res.response==1)
                {
                    if(need_silence)
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
            
        }

        public void DeleteConversation(ConversationWithLastMsg c)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (c.conversation.peer.type == VKConversationPeerType.Chat)//todo: для группы как?
                parameters["peer_id"] = c.conversation.peer.id.ToString();
            else
                parameters["user_id"] = c.conversation.peer.id.ToString();
            //MessagesService.Instance.dele
            VKRequestsDispatcher.DispatchRequestToVK<LastDeleted>("messages.deleteConversation", parameters,(result)=> { 
            if (result.error.error_code == VKErrors.None)
            {
                    Execute.ExecuteOnUIThread(() => { 
                        base.Items.Remove(c);
                    });
                }
            });
        }

#region Search
        public ObservableCollection<ConversationWithLastMsg> SearchItems { get; private set; }
        public string q;
        private uint searchMaximum;

        private bool _inSearch;
        public bool InSearch
        {
            get { return this._inSearch; }
            set
            {
                this._inSearch = value;

                this.searchMaximum = 0;

                if (value)
                {
                    this.SearchItems = new ObservableCollection<ConversationWithLastMsg>();
                }
                else
                {
                    this.SearchItems = null;
                }
            }
        }

        public void ServerSearch(string text)
        {
            this.SearchItems.Clear();
            this.q = text;
            this.SearchNext();
        }

        private void SearchNext()
        {
//            this.UpdateLoadingStatus(ProfileLoadingStatus.Loading);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["q"] = this.q;
            parameters["count"] = "20";
            parameters["offset"] = this.SearchItems.Count.ToString();
            parameters["extended"] = "1";

            VKRequestsDispatcher.DispatchRequestToVK<VKDialogsSearchObject>("messages.search", parameters,(result)=>
            {
                if (result != null && result.error.error_code == VKErrors.None)
                {
                    if (result.response.count == 0)
                        return;

                    List<VKGroup> groups = result.response.groups;
                    List<VKUser> profiles = result.response.profiles;
                    
                    List<VKMessage> msgs = result.response.items;

                    UsersService.Instance.SetCachedUsers(profiles);//todo: не кешировать, а присваивать
                    GroupsService.Instance.SetCachedGroups(groups);

                    List<int> list = VKMessage.GetAssociatedUserIds(msgs);


                    foreach(var conversation in result.response.conversations)
                    {
                        if(conversation.chat_settings!=null && conversation.chat_settings.active_ids!=null)
                        {
                            list.AddRange(conversation.chat_settings.active_ids);
                        }
                    }

                    var l = list.Except<int>(profiles.Select((u => (int)u.id)));
                    var l2 = l.Where((val => val>0));
                    var l3 = l2.Select((val => (uint)val)).ToList();
                    UsersService.Instance.GetUsers(l3, (resUsers =>
                    {
                        if (resUsers != null)
                        {
                            result.response.profiles.AddRange(resUsers);




                            this.searchMaximum = result.response.count;
                            List<VKConversation> conversations = result.response.conversations;

                            Execute.ExecuteOnUIThread(() =>
                            {
                                foreach (var msg in result.response.items)
                                {

                                    ConversationWithLastMsg dialog = new ConversationWithLastMsg();
                                    dialog.conversation = conversations.First((c) => c.peer.id == msg.peer_id);
                                    dialog.last_message = msg;

                                    VKUser user1 = result.response.profiles.Find((pro) => pro.id == msg.from_id);
                                    VKUser user02 = null;

                                    if (msg.action != null)
                                        user02 = result.response.profiles.Find((pro) => pro.id == msg.action.member_id);

                                    dialog.UIBody = DialogsViewModel.GetMessageHeaderText(msg, user1, user02);

                                    this.SearchItems.Add(dialog);
                                }
                            });

//                            this.UpdateLoadingStatus(ProfileLoadingStatus.Loaded);
                        }
                        else
                        {
 //                           this.UpdateLoadingStatus(ProfileLoadingStatus.LoadingFailed);//Не удалось получить пользователей
                        }
                    }));

                    

                }
                else
                {
//                    this.UpdateLoadingStatus(ProfileLoadingStatus.LoadingFailed);//не удалось найти
                }
            });

            

            
        }
#endregion

        public static void Save()
        {
            if(DialogsViewModel.Instance.Items.Count>0)
                CacheManager.TrySerialize(DialogsViewModel.Instance, "Dialogs", true);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(2);
            
            List<VKUser> _associatedUsers = new List<VKUser>();
            List<VKGroup> _associatedGroups = new List<VKGroup>();
            foreach (var dialog in base.Items)
            {
                if(dialog.conversation.peer.type == VKConversationPeerType.Chat)
                {
                    if (dialog.conversation.chat_settings != null)
                    {
                        foreach (var id in dialog.conversation.chat_settings.active_ids)
                        {
                            if (id > 0)
                            {
                                var user = UsersService.Instance.GetCachedUser((uint)id);
                                if (user != null)
                                    _associatedUsers.Add(user);
                            }
                            else
                            {
                                var group = GroupsService.Instance.GetCachedGroup((uint)(-id));
                                if (group != null)
                                    _associatedGroups.Add(group);
                            }
                        }
                    }
                }
                else if (dialog.conversation.peer.type == VKConversationPeerType.Group)
                {
                    var group = GroupsService.Instance.GetCachedGroup((uint)(-dialog.conversation.peer.id));
                    if (group != null)
                        _associatedGroups.Add(group);
                }
                else if (dialog.conversation.peer.type == VKConversationPeerType.User)
                {
                    var user = UsersService.Instance.GetCachedUser((uint)dialog.conversation.peer.id);
                    if (user != null)
                        _associatedUsers.Add(user);
                }
            }

            writer.WriteList<VKUser>(_associatedUsers);
            writer.WriteList<VKGroup>(_associatedGroups);

            writer.WriteList<ConversationWithLastMsg>(base.Items.Take(20).ToList());

            if(base._totalCount.HasValue)
                writer.Write(base._totalCount.Value);
            else
                writer.Write((uint)0);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            
            List<VKUser> _associatedUsers = reader.ReadList<VKUser>();
            List<VKGroup> _associatedGroups = reader.ReadList<VKGroup>();

            UsersService.Instance.SetCachedUsers(_associatedUsers);
            GroupsService.Instance.SetCachedGroups(_associatedGroups);

            var items = reader.ReadList<ConversationWithLastMsg>();
            foreach(var item in items)
                base.Items.Add(item);
            
            uint total = reader.ReadUInt32();
            if (total > 0)
                base._totalCount = total;
        }
    }

    
    public class VKDialogsSearchObject
    {
        /// <summary>
        /// Общее количество элементов.
        /// </summary>
        public uint count { get; set; }

        public List<VKMessage> items { get; set; }

        public List<VKConversation> conversations { get; set; }

        /// <summary>
        /// Список пользователей.
        /// </summary>
        public List<VKUser> profiles { get; set; }

        /// <summary>
        /// Список сообществ.
        /// </summary>
        public List<VKGroup> groups { get; set; }
    }

    /*
    public class VKDialogsGetObject : VKCountedItemsObject<ConversationWithLastMsg>
    {
        /// <summary>
        /// число непрочитанных бесед. 
        /// </summary>
        public int unread_count { get; set; }
    }
    */
    public class LastDeleted
    {
        public int last_deleted_id { get; set; }
    }

    public class VKDialogsGetObject : VKCountedItemsObject<ConversationWithLastMsg>
    {
        /// <summary>
        /// число непрочитанных бесед. 
        /// </summary>
        public int unread_count { get; set; }
    }
}
