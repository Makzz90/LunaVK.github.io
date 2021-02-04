using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Library.Events
{
    public class UserBecameOnlineEvent
    {
        public int user_id;
        public bool status;

        public UserBecameOnlineEvent(int _user_id, bool _status)
        {
            this.user_id = _user_id;
            this.status = _status;
        }
    }
}
