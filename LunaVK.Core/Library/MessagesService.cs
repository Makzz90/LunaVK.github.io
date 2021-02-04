using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LunaVK.Core.Library
{
    public class MessagesService
    {
        private static MessagesService _instance;
        public static MessagesService Instance
        {
            get
            {
                if (MessagesService._instance == null)
                    MessagesService._instance = new MessagesService();
                return MessagesService._instance;
            }
        }

        //public void DeleteDialog(int peerId)
        //{
        //    Dictionary<string, string> parameters = new Dictionary<string, string>();
        //    parameters["peer_id"] = peerId.ToString();
        //    RequestsDispatcher.GetResponse<int>("messages.deleteConversation", parameters);
        //}

        public void UploadPhoto(byte[] photoData, Action<VKPhoto> callback, Action<double> progressCallback = null)
        {
            this.GetPhotoUploadServer((u =>
            {
                if (u == null)
                    callback(null);
                else
                {
                    JsonWebRequest.Upload(u, photoData, "photo", "image", (JsonString, result) =>
                    {
                        if (!result)
                            callback(null);
                        else
                            this.SavePhoto(JsonConvert.DeserializeObject<UploadPhotoResponseData>(JsonString), callback);
                    }, "MyImage.jpg", progressCallback);//todo: filename

                }
            }));
        }
        
        public void GetConversationsById(IReadOnlyList<int> ids, Action<VKResponse< VKCountedItemsObject<VKConversation>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["peer_ids"] = ids.GetCommaSeparated();
            parameters["extended"] = "1";
            parameters["fields"] = "photo_50";

            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKConversation>>("messages.getConversationsById", parameters, callback);
        }

        public void SavePhoto(UploadPhotoResponseData uploadData, Action<VKPhoto> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["server"] = uploadData.server;
            parameters["photo"] = uploadData.photo;
            parameters["hash"] = uploadData.hash;

            VKRequestsDispatcher.DispatchRequestToVK<List<VKPhoto>>("photos.saveMessagesPhoto", parameters,(result)=> {
                callback(result.error.error_code == VKErrors.None ? result.response[0] : null);
            });
        }
        
        public void SendMessage(int peer_id, Action<VKResponse<uint>> callback, string msg_string = "", List<IOutboundAttachment> attachments = null, List<uint> forward_msgs = null, uint sticker_id = 0, string payload = null, uint? groupId = null )
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["peer_id"] = peer_id.ToString();
            parameters["random_id"] = Convert.ToInt32((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();

            if (!string.IsNullOrWhiteSpace(msg_string))
                parameters["message"] = msg_string;
            //this.ProcessCommands(request.MessageBody);
            if (attachments != null && attachments.Count > 0)
                parameters["attachment"] = String.Join(",", attachments);
            if (forward_msgs != null && forward_msgs.Count > 0)
                parameters["forward_messages"] = forward_msgs.GetCommaSeparated();
            if (sticker_id != 0)
                parameters["sticker_id"] = sticker_id.ToString();
            if(payload!=null)
                parameters["payload"] = payload;

            if(groupId.HasValue)
                parameters["group_id"] = groupId.Value.ToString();
            //if (!string.IsNullOrEmpty(request.StickerReferrer))
            //    parameters["sticker_referrer"] = request.StickerReferrer;
            //if (request.IsGeoAttached)
            //{
            //    parameters["lat"] = request.Latitude.ToString((IFormatProvider)CultureInfo.InvariantCulture);
            //    parameters["long"] = request.Longitude.ToString((IFormatProvider)CultureInfo.InvariantCulture);
            //}
            VKRequestsDispatcher.DispatchRequestToVK<uint>("messages.send", parameters, callback);
        }
        /*
        public void GetDialogs(int offset, int count, int previewLength, Action<VKResponse<VKDialogsGetObject>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["preview_length"] = previewLength.ToString();
            parameters["fields"] = "online,first_name_acc,last_name_acc,photo_100,last_seen,verified,push_settings";
            parameters["extended"] = "1";
            VKRequestsDispatcher.DispatchRequestToVK<VKDialogsGetObject>("messages.getConversations", parameters, (res =>
            {
                if (res!= null && res.error.error_code == VKErrors.None)
                {
                    List<Message> list1 = res.ResultData.DialogHeaders.Select<DialogHeaderInfo, Message>((Func<DialogHeaderInfo, Message>)(d => d.message)).ToList<Message>();
                    UsersService.Instance.SetCachedUsers((IEnumerable<GroupOrUser>)res.ResultData.Users);
                    int num = 0;
                    List<long> list2 = Message.GetAssociatedUserIds(list1, num != 0).Select<long, long>((Func<long, long>)(uid => uid)).Distinct<long>().ToList<long>();
                    if (list2.Count > res.ResultData.Users.Count)
                        UsersService.Instance.GetUsers(list2.Except<long>(res.ResultData.Users.Select<GroupOrUser, long>((Func<GroupOrUser, long>)(u => u.id))).ToList<long>(), (Action<BackendResult<List<GroupOrUser>, ResultCode>>)(resUsers =>
                        {
                            if (resUsers.ResultCode == ResultCode.Succeeded)
                            {
                                res.ResultData.Users.AddRange((IEnumerable<GroupOrUser>)resUsers.ResultData);
                                callback(res);
                            }
                            else
                                callback(new BackendResult<MessageListResponse, ResultCode>(resUsers.ResultCode));
                        }));
                    else
                        callback(res);
                }
                else
                    callback(res);
            }));
        }
        */
        

        public void GetMessages(List<uint> messageIds, Action<VKGetMessagesHistoryObject> callback)
        {
            if (messageIds == null || messageIds.Count == 0)
            {
                callback(null);
            }
            else
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["message_ids"] = messageIds.GetCommaSeparated();
                parameters["fields"] = "first_name_acc,last_name_acc,online,online_mobile,photo_max,sex,friend_status,photo_200,is_messages_blocked";
                parameters["extended"] = "1";
                VKRequestsDispatcher.DispatchRequestToVK<VKGetMessagesHistoryObject>("messages.getById", parameters, (res =>
                {
                    //if (res.ResultCode == ResultCode.Succeeded)
                    if (res!=null && res.error.error_code == VKErrors.None)
                    {
                        /*
                        List<VKMessage> messages = res.response.items;
                        //UsersService.Instance.SetCachedUsers(res.ResultData.Users);
                        
                        List<int> list = VKMessage.GetAssociatedUserIds(messages);
                        if (list.Count > res.ResultData.Users.Count)
                            UsersService.Instance.GetUsers(list.Except<long>(res.ResultData.Users.Select<GroupOrUser, long>((Func<GroupOrUser, long>)(u => u.id))).ToList<long>(), (Action<BackendResult<List<GroupOrUser>, ResultCode>>)(resUsers =>
                            {
                                if (resUsers.ResultCode == ResultCode.Succeeded)
                                {
                                    res.ResultData.Users.AddRange((IEnumerable<GroupOrUser>)resUsers.ResultData);
                                    callback(res);
                                }
                                else
                                    callback(new BackendResult<MessageListResponse, ResultCode>(resUsers.ResultCode));
                            }));
                            */
                        
                            callback(res.response);
                    }
                    else
                        callback(null);
                }));
            }
        }

        public void GetHistory(int peerId, int offset, int count, int? startMessageId, Action<VKResponse<VKGetMessagesHistoryObject>> callback, uint? groupId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["peer_id"] = peerId.ToString();
            parameters["count"] = count.ToString();

            if (startMessageId.HasValue) //if (startMessageId!=null)
                parameters["start_message_id"] = startMessageId.Value.ToString();

            if(groupId.HasValue)
                parameters["group_id"] = groupId.Value.ToString();

            parameters["offset"] = offset.ToString();

            //parameters["fields"] = "first_name_acc,last_name_acc,online,online_mobile,photo_max,sex,friend_status,photo_200,first_name_dat,is_messages_blocked";//wp
            //parameters["fields"] = "first_name,last_name,photo_200";

            parameters["fields"] = "first_name,last_name,first_name_acc,last_name_acc,online,online_mobile,photo_100,photo_50,is_messages_blocked,last_seen,sex,push_settings,domain,verified";
            parameters["extended"] = "1";

            VKRequestsDispatcher.DispatchRequestToVK<VKGetMessagesHistoryObject>("messages.getHistory", parameters, (result)=>
            {
                if(result.error.error_code == VKErrors.None)
                {
                    UsersService.Instance.SetCachedUsers(result.response.profiles);
                    GroupsService.Instance.SetCachedGroups(result.response.groups);

                    var list = VKMessage.GetAssociatedUserIds(result.response.items, true);
                    var l = list.Where((u) => { return u > 0; }).Select((u)=>(uint)u);
                    //bug: группы!!
                    UsersService.Instance.GetUsers(l.ToList(),(resUsers)=>
                    {
                        foreach (VKMessage msg in result.response.items)
                        {
                            if (msg.from_id > 0)
                                msg.User = UsersService.Instance.GetCachedUser((uint)msg.from_id);
                            else
                                msg.User = GroupsService.Instance.GetCachedGroup((uint)(-msg.from_id));
                            Debug.Assert(msg.User != null);
                            if (msg.text == "" && (msg.fwd_messages == null ? true : msg.fwd_messages.Count == 0) && (msg.attachments == null ? true : msg.attachments.Count == 0) && msg.geo == null)
                                msg.text = "(контент удалён)";

                            if(msg.reply_message!=null)
                            {
                                msg.fwd_messages = new List<VKMessage>() { msg.reply_message };
                            }
                        }
                    });

                    //
                    //
                    foreach (VKMessage msg in result.response.items)
                    {
                        if(msg.attachments!=null && msg.attachments.Count>0)
                        {
                            foreach (var attachment in msg.attachments)
                            {
                                if(attachment.type == VKAttachmentType.Wall)
                                {
                                    VKWallPost p = attachment.wall;

                                    VKBaseDataForGroupOrUser owner = null;
                                    if (p.from_id != 0)
                                    {
                                        if (p.from_id < 0 && result.response.groups != null)
                                            owner = result.response.groups.Find(ow => ow.id == (-p.from_id));
                                        else
                                            owner = result.response.profiles.Find(ow => ow.id == p.from_id);
                                    }
                                    p.Owner = owner;

                                    if (p.copy_history != null)
                                    {
                                        for (int i = 0; i < p.copy_history.Count; i++)
                                        {
                                            VKWallPost item = p.copy_history[i];
                                            if (item.owner_id < 0 && result.response.groups != null)
                                                item.Owner = result.response.groups.Find(ow => ow.id == (-item.owner_id));
                                            else
                                                item.Owner = result.response.profiles.Find(ow => ow.id == item.owner_id);
                                        }
                                    }

                                    if (p.signer_id != 0)
                                    {
                                        if (p.signer_id < 0 && result.response.groups != null)
                                            owner = result.response.groups.Find(ow => ow.id == (-p.signer_id));
                                        else
                                            owner = result.response.profiles.Find(ow => ow.id == p.signer_id);
                                        p.Signer = owner;
                                    }
                                }
                                else if(attachment.type == VKAttachmentType.Event)
                                {
                                    VKEvent p = attachment.@event;

                                    VKBaseDataForGroupOrUser owner = result.response.groups.Find(ow => ow.id == p.id);
                                    p.Owner = owner;

                                    if (!p.friends.IsNullOrEmpty())
                                    {
                                        p.Friends = new List<VKUser>();
                                        foreach(var id in p.friends)
                                        {
                                            var fr = result.response.profiles.Find((u) => u.id == id);
                                            p.Friends.Add(fr);
                                        }
                                    }
                                }
                                else if (attachment.type == VKAttachmentType.Story)
                                {
                                    VKStory p = attachment.story;

                                    VKBaseDataForGroupOrUser owner = null;
                                    if (p.owner_id != 0)
                                    {
                                        if (p.owner_id < 0 && result.response.groups != null)
                                            owner = result.response.groups.Find(ow => ow.id == (-p.owner_id));
                                        else
                                            owner = result.response.profiles.Find(ow => ow.id == p.owner_id);
                                    }
                                    p.Owner = owner;
                                }
                            }
                        }
                    }
                    //
                    //
                }
                callback(result);
            });
        }


        public void GetConversationMaterials(int peerId, string mediaType, string startFrom, int count, Action<VKResponse<VKCountedItemsObject<ConversationMaterial>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "peer_id", peerId.ToString() },
                { "media_type", mediaType },
                { "count", count.ToString() }
            };

            if (!string.IsNullOrEmpty(startFrom))
                parameters.Add("start_from", startFrom);
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<ConversationMaterial>>("messages.getHistoryAttachments", parameters, callback);
        }

        public void CreateChat(IReadOnlyList<int> userIds, string title, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (userIds != null)
                parameters["user_ids"] = userIds.GetCommaSeparated();

            if (!string.IsNullOrEmpty(title))
                parameters["title"] = title;
            VKRequestsDispatcher.DispatchRequestToVK<int>("messages.createChat", parameters, callback);
        }

        public void GetPhotoUploadServer(Action<string> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            VKRequestsDispatcher.DispatchRequestToVK<UploadServerAddress>("photos.getMessagesUploadServer", parameters, (result)=> {
                callback(result.error.error_code == VKErrors.None ? result.response.upload_url : null);
            });
        }

        public void DeleteChatPhoto(int chatId, Action<VKResponse<ChatInfoWithMessageId>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["chat_id"] = chatId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<ChatInfoWithMessageId>("messages.deleteChatPhoto", parameters, callback);
        }

        public void MarkAsRead(uint messageId, int peerId, Action<VKResponse<int>> callback, uint? groupId = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["start_message_id"] = messageId.ToString();
            parameters["peer_id"] = peerId.ToString();

            if (groupId.HasValue)
                parameters["group_id"] = groupId.Value.ToString();

            VKRequestsDispatcher.DispatchRequestToVK<int>("messages.markAsRead", parameters, callback);
        }

        public void MarkAsImportant(uint messageId, bool important, Action<VKResponse<List<int>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["message_ids"] = messageId.ToString();
            parameters["important"] = important ? "1" : "0";
            VKRequestsDispatcher.DispatchRequestToVK<List<int>>("messages.markAsImportant", parameters, callback);
        }

        public void SetUserIsTyping(int userOrChatId, Action<VKResponse<int>> callback, bool isTyping = true, uint? groupId = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["peer_id"] = userOrChatId.ToString();
            parameters["type"] = isTyping ? "typing" : "audiomessage";
            VKRequestsDispatcher.DispatchRequestToVK<int>("messages.setActivity", parameters, callback);
        }

        public class ChatInfoWithMessageId
        {
            public int message_id { get; set; }

            public ChatInfo.Chat chat { get; set; }
        }

        public sealed class ConversationMaterial
        {
            public int message_id { get; set; }
            public VKAttachment attachment { get; set; }
        }
    }
}
