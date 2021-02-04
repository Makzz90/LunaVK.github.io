using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Library.Events
{
    public class UserIsTyping
    {
        public long _chatId;
        public long _userId;

        public UserIsTyping(long userId, long chatId)
        {
            this._userId = userId;
            this._chatId = chatId;
        }
    }
}
