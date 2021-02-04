using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Framework
{
    /// <summary>
    /// UserId, ChatId, Attachments
    /// </summary>
    public class PagesParams
    {
        public int user_id;
        public int chat_id;
        public int peer_id
        {
            get
            {
                if (this.chat_id > 0)
                    return 2000000000 + this.chat_id;

                return this.user_id;
            }
        }
        public object attachment;
    }
}
