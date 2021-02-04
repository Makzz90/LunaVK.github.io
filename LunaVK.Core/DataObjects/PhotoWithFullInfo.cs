using LunaVK.Core.Network;
using System.Collections.Generic;

namespace LunaVK.Core.DataObjects
{
    public class PhotoWithFullInfo
    {
        public VKPhoto Photo { get; set; }
        
        public VKCountedItemsObject<VKComment> Comments { get; set; }

        public List<VKGroup> Groups { get; set; }

        public List<VKUser> Users { get; set; }

        public List<VKPhotoVideoTag> PhotoTags { get; set; }

        public VKCountedItemsObject<VKUser> LikesAllIds { get; set; }

        public int RepostsCount { get; set; }
    }
}
