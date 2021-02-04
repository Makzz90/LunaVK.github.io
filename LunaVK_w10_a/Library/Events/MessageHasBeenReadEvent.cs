using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Library.Events
{
    public class MessageHasBeenReadEvent
    {
        public long msg_id;
        public int user_id;
        public long chat_id;
        public bool is_chat;

        public MessageHasBeenReadEvent(long _msg_id, int _user_id, long _chat_id, bool _is_chat)
        {
            this.msg_id = _msg_id;
            this.user_id = _user_id;
            this.chat_id = _chat_id;
            this.is_chat = _is_chat;
        }
    }
}
