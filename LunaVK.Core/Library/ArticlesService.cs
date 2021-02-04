using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.Library
{
    public class ArticlesService
    {
        private static ArticlesService _instance;
        public static ArticlesService Instance
        {
            get
            {
                if (ArticlesService._instance == null)
                    ArticlesService._instance = new ArticlesService();
                return ArticlesService._instance;
            }
        }

        public void GetArticles(int offset, int count, int ownerId, Action<VKResponse<VKCountedItemsObject<VKArticle>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();//or domain
            parameters["count"] = count.ToString();
            //sort_by
            //fields", "friend_status,members_count,domain,followers_count,photo_100,photo_200,is_closed,member_status,counters,verified,trending");
            //extended;
            if (offset > 0)
                parameters["offset"] = offset.ToString();

            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKArticle>>("articles.getOwnerPublished", parameters, callback);
        }
    }
}
