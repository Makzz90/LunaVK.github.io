using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.Library
{
    public class MarketService
    {
        private static MarketService _instance;

        public static MarketService Instance
        {
            get
            {
                return MarketService._instance ?? (MarketService._instance = new MarketService());
            }
        }
        /*
        public void GetFeed(long ownerId, int count, int offset, Action<VKResponse<MarketFeedResponse>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<MarketFeedResponse>("execute.getMarketFeed", new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "offset",
          offset.ToString()
        }
      }, callback);
        }
        */
        public void GetProducts(int ownerId, int count, int offset, Action<VKResponse<VKCountedItemsObject<VKMarketItem>>> callback)
        {
            this.GetProducts(ownerId, 0, count, offset, callback);
        }

        public void GetProducts(int ownerId, int albumId, int count, int offset, Action<VKResponse<VKCountedItemsObject<VKMarketItem>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "offset",
          offset.ToString()
        },
        {
          "extended",
          "1"
        }
      };
            if (albumId > 0L)
                parameters["album_id"] = albumId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKMarketItem>>("market.get", parameters, callback);
        }
        /*
        public void GetAlbumTitleWithProducts(long ownerId, long albumId, int count, int offset, Action<VKResponse<MarketAlbum>> callback)
        {
            string str = string.Format("\r\n\r\nreturn\r\n{{\r\n    \"title\": API.market.getAlbumById({{ owner_id: {0}, album_ids: {1} }}).items[0].title,\r\n    \"products\": API.market.get({{ owner_id: {2}, count: {3}, offset: {4}, extended: 1, album_id: {5} }})\r\n}};", ownerId, albumId, ownerId, count, offset, albumId);
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("code", str);
            CancellationToken? cancellationToken = new CancellationToken?();
            // ISSUE: variable of the null type
            VKRequestsDispatcher.DispatchRequestToVK<MarketAlbum>("execute", parameters, callback, null, false, true, cancellationToken, null);
        }
        */
        public void GetProduct(int ownerId, uint productId, Action<VKResponse<VKCountedItemsObject<VKMarketItem>>> callback)
        {
            this.GetProductsByIds(new List<string>() { string.Format("{0}_{1}", ownerId, productId) }, callback);
        }

        public void GetProductsByIds(IEnumerable<string> productIds, Action<VKResponse<VKCountedItemsObject<VKMarketItem>>> callback)
        {
            /*
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKMarketItem>>("market.getById", new Dictionary<string, string>()
      {
        {
          "item_ids",
          string.Join(",", productIds)
        },
        {
          "extended",
          "1"
        }
      }, callback);
      */
            string code = "var m = API.market.getById({item_ids:\"" + string.Join(",", productIds) + "\",extended:1});";
            code += "var ownerId = m.items[0].owner_id;";
            code += "if (ownerId > 0)";
            code += "{";
            code +=     "var u = API.users.get({ user_ids: ownerId,fields: \"photo_100\"})[0];";
            code +=     "m.profiles=[]; m.profiles.push(u);";
            code += "}";
            code += "else";
            code += "{";
            code +=     "var g = API.groups.getById({ group_id: (-ownerId), fields: \"market\"})[0];";
            code +=     "m.groups=[]; m.groups.push(g);";
            code += "}";
            code += "return m;";
            VKRequestsDispatcher.Execute<VKCountedItemsObject<VKMarketItem>>(code, callback);
        }



        /*
        public void GetProductData(long ownerId, long productId, Action<VKResponse<ProductData>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<ProductData>("execute.getProductData", new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "product_id",
          productId.ToString()
        }
      }, callback);
        }
        
        public void GetAlbums(long ownerId, int count, int offset, Action<VKResponse<VKCountedItemsObject<MarketAlbum>>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<MarketAlbum>>("market.getAlbums", new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "offset",
          offset.ToString()
        }
      }, callback);
        }
        
        public void Search(long ownerId, long albumId, SearchParams searchParams, string query, int count, int offset, Action<VKResponse<VKCountedItemsObject<VKMarketItem>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "album_id",
          albumId.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "offset",
          offset.ToString()
        },
        {
          "extended",
          "1"
        }
      };
            if (!string.IsNullOrWhiteSpace(query))
                parameters["q"] = query;
            if (searchParams != null)
            {
                long num1 = searchParams.GetValue<long>("price_from");
                long num2 = searchParams.GetValue<long>("price_to");
                if (num1 > 0L || num2 > 0L)
                {
                    long num3;
                    if (num1 > 0L)
                    {
                        Dictionary<string, string> dictionary = parameters;
                        string index = "price_from";
                        num3 = num1 * 100L;
                        string str = num3.ToString();
                        dictionary[index] = str;
                    }
                    if (num2 > 0L)
                    {
                        Dictionary<string, string> dictionary = parameters;
                        string index = "price_to";
                        num3 = num2 * 100L;
                        string str = num3.ToString();
                        dictionary[index] = str;
                    }
                }
                int num4 = searchParams.GetValue<int>("sort");
                if (num4 > 0)
                    parameters["sort"] = num4.ToString();
                bool flag = searchParams.GetValue<bool>("rev");
                parameters["rev"] = flag ? "1" : "0";
            }
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKMarketItem>>("market.search", parameters, callback);
        }
        
        public void GetComments(long ownerId, long productId, int knownCount, int offset, int count, Action<VKResponse<ProductLikesCommentsData>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<ProductLikesCommentsData>("execute.getProductComments", new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "product_id",
          productId.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "offset",
          offset.ToString()
        },
        {
          "known_count",
          knownCount.ToString()
        },
        {
          "func_v",
          "2"
        }
      }, callback, (Func<string, ProductLikesCommentsData>)(jsonStr =>
      {
          jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "users", false);
          jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "users2", false);
          jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "users3", false);
          jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "groups", false);
          jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "comments", true);
          jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "tags", false);
          jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "likesAllIds", false);
          ProductLikesCommentsData response = JsonConvert.DeserializeObject<GenericRoot<ProductLikesCommentsData>>(jsonStr).response;
          GroupsService.Current.AddCachedGroups(response.groups);
          if (knownCount < 0)
              response.Comments.Reverse();
          response.users2.AddRange((IEnumerable<GroupOrUser>)response.users3);
          return response;
      }), false, true, new CancellationToken?(), null);
        }

        public void CreateComment(long ownerId, long itemId, string message, List<string> attachmentIds, bool fromGroup, long replyToCommentId, Action<VKResponse<Comment>> callback, int sticker_id = 0, string stickerReferrer = "")
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>()
      {
        {
          "code",
          string.Format("\r\n\r\nvar new_comment_id = API.market.createComment({{\r\n    owner_id: {0},\r\n    item_id: {1},\r\n    message: \"{2}\",\r\n    from_group: {3},\r\n    sticker_id: {4},\r\n    reply_to_comment: {5},\r\n    attachments: \"{6}\",\r\n    sticker_referrer: \"{7}\"\r\n}});\r\n\r\nvar last_comments = API.market.getComments({{\r\n    owner_id: {8},\r\n    item_id: {9},\r\n    need_likes: 1,\r\n    count: 10,\r\n    sort: \"desc\",\r\n    preview_length: 0,\r\n    allow_group_comments: 1\r\n}}).items;\r\n\r\nvar i = last_comments.length - 1;\r\nwhile (i >= 0)\r\n{{\r\n    if (last_comments[i].id == new_comment_id)\r\n        return last_comments[i];\r\n\r\n    i = i - 1;\r\n}}\r\n\r\nreturn null;\r\n\r\n                ", ownerId, itemId, message.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r"), (fromGroup ? "1" : "0"), sticker_id, replyToCommentId, attachmentIds.GetCommaSeparated(","), stickerReferrer, ownerId, itemId)
        }
      };
            string methodName = "execute";
            Dictionary<string, string> parameters = dictionary;
            Action<VKResponse<Comment>> callback1 = callback;
            int num1 = 0;
            int num2 = 1;
            CancellationToken? cancellationToken = new CancellationToken?();
            // ISSUE: variable of the null type

            VKRequestsDispatcher.DispatchRequestToVK<Comment>(methodName, parameters, callback1, (Func<string, Comment>)(jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<Comment>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
        }

        public void EditComment(long ownerId, long commentId, string message, List<string> attachmentIds, Action<VKResponse<VKClient.Audio.Base.ResponseWithId>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "comment_id",
          commentId.ToString()
        },
        {
          "message",
          message
        }
      };
            if (!attachmentIds.IsNullOrEmpty())
                parameters["attachments"] = attachmentIds.GetCommaSeparated(",");
            VKRequestsDispatcher.DispatchRequestToVK<VKClient.Audio.Base.ResponseWithId>("market.editComment", parameters, callback, (Func<string, VKClient.Audio.Base.ResponseWithId>)(jsonStr => new VKClient.Audio.Base.ResponseWithId()), false, true, new CancellationToken?(), null);
        }
        */
        public void DeleteComment(long ownerId, long commentId, Action<VKResponse<int>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<int>("market.deleteComment", new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "comment_id",
          commentId.ToString()
        }
      }, callback);
        }

        public void ReportComment(long ownerId, long commentId, ReportReason reportReason, Action<VKResponse<int>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<int>("market.reportComment", new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "comment_id",
          commentId.ToString()
        },
        {
          "reason",
          ((int) reportReason).ToString()
        }
      }, callback);
        }
    }
}
