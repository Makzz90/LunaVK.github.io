using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.Library
{
    public class PodcastsService
    {
        private static PodcastsService _instance;
        public static PodcastsService Instance
        {
            get
            {
                if (PodcastsService._instance == null)
                    PodcastsService._instance = new PodcastsService();
                return PodcastsService._instance;
            }
        }

        public void GetPodcasts(int offset, int count, int ownerId, Action<VKResponse<VKCountedItemsObject<VKPodcast>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["count"] = count.ToString();

            if (offset > 0)
                parameters["offset"] = offset.ToString();

            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKPodcast>>("podcasts.getEpisodes", parameters, callback);
        }

        public void GetPodcast(uint id, int ownerId, Action<VKResponse<VKPodcast>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["episode_id"] = id.ToString();
            
            VKRequestsDispatcher.DispatchRequestToVK<VKPodcast>("podcasts.getEpisode", parameters, callback);
        }
    }
}
