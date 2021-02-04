using System.Collections.Generic;
using System.Text;
using LunaVK.Core.Network;

namespace LunaVK.Core.DataObjects
{
    public class VKGroupsGetObject
    {
        public VKCountedItemsObject<VKGroup> groups { get; set; }

        public VKCountedItemsObject<VKGroup> invitations { get; set; }

        public List<VKUser> inviters { get; set; }
    }
}
