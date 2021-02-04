using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.DataObjects
{
    public class UsersAndGroups
    {
        public List<VKUser> users { get; set; }

        public List<VKGroup> pages { get; set; }

        public List<VKGroup> groups { get; set; }

        public UsersAndGroups()
        {
            this.users = new List<VKUser>();
            this.pages = new List<VKGroup>();
            this.groups = new List<VKGroup>();
        }
    }
}
