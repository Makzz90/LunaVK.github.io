using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Library.Events
{
    public class ChatParamsWereChangedEvent
    {
        public long chat_id;
        public string title;
        public List<int> _associatedUsers;
        //chat_active_str

        public ChatParamsWereChangedEvent(long chatId)
        {
            this.chat_id = chatId;
        }
    }
}
