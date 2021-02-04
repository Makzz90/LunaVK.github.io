using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using System;

namespace LunaVK.Core
{
    public class CountersService
    {
        private static CountersService _instance;
        public static CountersService Instance
        {
            get
            {
                if (CountersService._instance == null)
                    CountersService._instance = new CountersService();
                return CountersService._instance;
            }
        }

        public void GetCountersWithLastMessage(Action<VKResponse<CountersWithMessageInfo>> callback)
        {
//#if DEBUG
//            string str = "var msgs = API.messages.getConversations({count:5,filter:\"unread\",extended:1}); var friends = API.friends.getRequests({extended:1,count:3,need_viewed:1,fields:\"photo_100\"}); var counters = API.getCounters(); return { Convs: msgs, Counters: counters,Friends: friends};";
//#else
            string str = "var msgs = API.messages.getConversations({count:5,filter:\"unread\",extended:1}); var friends = API.friends.getRequests({extended:1,count:5,need_viewed:0,fields:\"photo_100\"}); var counters = API.getCounters(); return { Convs: msgs, Counters: counters,Friends: friends};";
//#endif

            VKRequestsDispatcher.Execute<CountersWithMessageInfo>(str, callback, (json) => {
                return json.Replace("user_id", "id");
            });
        }

        public class CountersWithMessageInfo
        {
            /// <summary>
            /// Список последних непрочитанных соощений
            /// </summary>
            public VKCountedItemsObject<ConvWithLastMsg> Convs { get; set; }

            /// <summary>
            /// Заявки в друзья
            /// </summary>
            public VKCountedItemsObject<VKUser> Friends { get; set; }

            public CountersArgs Counters { get; set; }

            
        }
    }
}
