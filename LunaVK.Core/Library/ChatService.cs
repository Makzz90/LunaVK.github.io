using System;
using System.Collections.Generic;
using LunaVK.Core.Network;
using LunaVK.Core.DataObjects;

namespace LunaVK.Core.Library
{
    public class ChatService
    {
        private static ChatService _instance;
        public static ChatService Instance
        {
            get
            {
                return ChatService._instance ?? (ChatService._instance = new ChatService());
            }
        }

        public void RemoveChatUser(int chatId, int user, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["chat_id"] = chatId.ToString();
            parameters["user_id"] = user.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("messages.removeChatUser", parameters, callback);
        }

        public void AddChatUsers(int chatId, List<int> uids, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["chat_id"] = chatId.ToString();
            parameters["user_id"] = String.Join(",", uids);
            VKRequestsDispatcher.DispatchRequestToVK<int>("messages.addChatUser", parameters, callback);

            //string format = "API.messages.addChatUser({{\"chat_id\":{0}, \"user_id\":{1} }});";
            //string str = "";
            //foreach (long uid in uids)
            //    str = str + string.Format(format, chatId, uid) + Environment.NewLine;
            //Dictionary<string, string> parameters = new Dictionary<string, string>();
            //parameters["code"] = str;
            //RequestsDispatcher.DispatchRequestToVK<ResponseWithId>("execute", parameters, callback, (Func<string, ResponseWithId>)(jsonStr => new ResponseWithId()), false, true, new CancellationToken?(), null);
        }

        public void EditChat(int chatId, string title, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["chat_id"] = chatId.ToString();
            parameters["title"] = title;
            VKRequestsDispatcher.DispatchRequestToVK<int>("messages.editChat", parameters, callback);
        }

        public void GetChatInfo(int chatId, Action<VKResponse<ChatInfo>> callback)
        {
            string code = string.Format("var chat = API.messages.getChat({{chat_id:{0}}});var chat_participants = API.messages.getChatUsers({{chat_id:{0},fields:\"first_name,last_name,first_name_acc,last_name_acc,online,online_mobile,photo_100,sex\"}}); return {{chat: chat, chat_participants: chat_participants, invited_by_users: API.users.get({{user_ids:chat_participants@.invited_by,fields:\"sex\"}})}};", chatId);

            VKRequestsDispatcher.Execute<ChatInfo>(code, callback, (json) => {
                json = VKRequestsDispatcher.FixFalseArray(json, "chat");
                json = VKRequestsDispatcher.FixFalseArray(json, "chat_participants");
                return json;
            });
        }
    }
}
