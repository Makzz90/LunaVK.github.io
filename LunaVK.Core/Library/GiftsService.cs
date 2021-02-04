using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunaVK.Core.Library
{
    public class GiftsService
    {
        private static GiftsService _instance;
        public static GiftsService Instance
        {
            get
            {
                return GiftsService._instance ?? (GiftsService._instance = new GiftsService());
            }
        }

        public void GetGiftInfoFromStore(int productId, int userOrChatId, bool isChat, Action<VKResponse<GiftInfoFromStoreResponse>> callback, CancellationToken? cancellationToken = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["product_id"] = productId.ToString();
            parameters["user_or_chat_id"] = userOrChatId.ToString();
            parameters["is_chat"] = (isChat ? 1 : 0).ToString();
            VKRequestsDispatcher.DispatchRequestToVK<GiftInfoFromStoreResponse>("execute.getGiftInfoFromStore", parameters, callback, null, false, cancellationToken);
        }

        public void GetCatalog(int userId, Action<VKResponse<GiftsCatalogResponse>> callback)
        {//todo:check in android
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (userId > 0L)
                parameters["user_id"] = userId.ToString();
            parameters["no_inapp"] = "0";
            parameters["force_payment"] = "1";
            //VKRequestsDispatcher.DispatchRequestToVK<GiftsCatalogResponse>("execute.getGiftsCatalog", parameters, callback, null, false, true, new CancellationToken?(), null);
            VKRequestsDispatcher.DispatchRequestToVK<GiftsCatalogResponse>("execute.getGiftsCatalogMaterial", parameters, callback, null, false);
        }

        public void GetCatalog(int userId, string categoryName, Action<VKResponse<List<GiftsSection>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (userId > 0L)
                parameters["user_id"] = userId.ToString();
            if (!string.IsNullOrEmpty(categoryName))
                parameters["filters"] = categoryName;
            VKRequestsDispatcher.DispatchRequestToVK<List<GiftsSection>>("gifts.getCatalog", parameters, callback);
        }

        public void Get(uint userId, int count, int offset, Action<VKResponse<VKCountedItemsObject<GiftItemData>>> callback)
        {
            if (userId == 0)
                userId = Settings.UserId;

            string code = "var gifts = API.gifts.get({user_id:"+ userId + ",count:"+ count + ",offset:"+ offset + "});";

            code += "var userIds =[]; var groupIds = []; var i = 0;";
            code += "var userOrGroupIds = gifts.items@.from_id;";

            code += "while (i < gifts.items.length)";
            code += "{";
            code +=     "var id = parseInt(userOrGroupIds[i]);";
            code +=     "if (id > 0)";
            code +=         "userIds.push(id);";
            code +=     "else ";
            code +=         "groupIds.push(-id);";
            code +=     "i = i + 1;";
            code += "}";

            code += "if (userIds.length > 0) { gifts.profiles = API.users.get({ \"user_ids\":userIds,fields: \"photo_100\"}); }";
            code += "if (groupIds.length > 0) { gifts.groups = API.groups.getById({ \"group_ids\":groupIds}); }";

            code += "return gifts; ";

            VKRequestsDispatcher.Execute<VKCountedItemsObject<GiftItemData>>(code, callback);
        }

        public void GetGiftInfo(List<int> userIds, Action<VKResponse<GiftResponse>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string str = string.Join<int>(",", userIds);
            parameters["user_ids"] = str;
            VKRequestsDispatcher.DispatchRequestToVK<GiftResponse>("execute.getGiftInfo", parameters, callback, null, false, new CancellationToken?());
        }

        public void Send(List<int> userIds, int giftId, int guid, string message, GiftPrivacy privacy, string section = "", Action<VKResponse<VKCountedItemsObject<GiftSentResponse>>> callback = null)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary["user_ids"] = string.Join<int>(",", userIds);
            dictionary["gift_id"] = giftId.ToString();
            string index3 = "guid";
            string str3 = guid.ToString();
            dictionary[index3] = str3;
            string index4 = "privacy";
            string str4 = ((int)privacy).ToString();
            dictionary[index4] = str4;
            string index5 = "no_inapp";
            string str5 = "1";
            dictionary[index5] = str5;
            Dictionary<string, string> parameters = dictionary;
            if (!string.IsNullOrWhiteSpace(message))
                parameters["message"] = message;
            if (!string.IsNullOrWhiteSpace(section))
                parameters["section"] = section;
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<GiftSentResponse>>("gifts.send", parameters, callback, null, false, new CancellationToken?()/*, (() => EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.gift_page, GiftPurchaseStepsAction.purchase_window)))*/);
        }

        public void Delete(int id, string giftHash, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["id"] = id.ToString();
            parameters["gift_hash"] = giftHash;
            VKRequestsDispatcher.DispatchRequestToVK<int>("gifts.delete", parameters, callback, null, false, new CancellationToken?());
        }

        public void GetChatUsers(int chatId, Action<VKResponse<List<int>>> callback, CancellationToken? cancellationToken = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["chat_id"] = chatId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<List<int>>("execute.getGiftChatUsers", parameters, callback, null, false, cancellationToken);
        }

        public void GetChatUsersForProduct(int chatId, int productId, Action<VKResponse<List<int>>> callback, CancellationToken? cancellationToken = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string index1 = "chat_id";
            string str1 = chatId.ToString();
            parameters[index1] = str1;
            string index2 = "product_id";
            string str2 = productId.ToString();
            parameters[index2] = str2;
            VKRequestsDispatcher.DispatchRequestToVK<List<int>>("execute.getGiftChatUsersForProduct", parameters, callback, null, false, cancellationToken);
        }

        public class GiftInfoFromStoreResponse
        {
            public List<int> userIds { get; set; }

//            public GiftsSectionItem giftItem { get; set; }
        }

        public class GiftsCatalogResponse
        {
            public int balance { get; set; }
            public List<GiftsSection> gifts { get; set; }
            //user_notifications
        }

        public class GiftResponse
        {
            public List<VKUser> users { get; set; }

            public int balance { get; set; }
        }

        public enum GiftPrivacy
        {
            VisibleToAll,
            VisibleToRecipient,
            Hidden,
        }

        public class GiftSentResponse
        {
            public int success { get; set; }

            public List<int> user_ids { get; set; }

            public int widthdrawn_votes { get; set; }
        }

        public class GiftsSection
        {
            public string name { get; set; }

            public string title { get; set; }

            public List<GiftsSectionItem> items { get; set; }
        }

        public class GiftsSectionItem
        {
            private string _description;
            private string _priceStr;
            private string _realPriceStr;

            public VKGift gift { get; set; }//
                                          //payment_type=balance
            public string description { get; set; }

            public int disabled { get; set; }

            public int price { get; set; }//

            public string price_str { get; set; }

            public int gifts_left { get; set; }

            public int real_price { get; set; }

            public string real_price_str { get; set; }

            public bool IsDisabled
            {
                get
                {
                    return this.disabled == 1;
                }
            }
        }
        /*
        public class GiftsResponse
        {
            public List<GiftItemData> gifts { get; set; }

            public List<VKUser> users { get; set; }

            public List<VKGroup> groups { get; set; }
        }
        */
        public class GiftItemData
        {
            public int id { get; set; }//бывают и отрицательные

            public int from_id { get; set; }

            public string message { get; set; }

            public int date { get; set; }

            public VKGift gift { get; set; }

            /// <summary>
            /// 2 - Hidden
            /// 0 - VisibleToAll
            /// 1 - VisibleToRecipient
            /// </summary>
            public int privacy { get; set; }
            /*
            public int privacy
            {
                get
                {
                    return this._privacy;
                }
                set
                {
                    this._privacy = value;
                    if (value != 1)
                    {
                        if (value == 2)
                            this.Privacy = GiftPrivacy.Hidden;
                        else
                            this.Privacy = GiftPrivacy.VisibleToAll;
                    }
                    else
                        this.Privacy = GiftPrivacy.VisibleToRecipient;
                }
            }

            public GiftPrivacy Privacy { get; private set; }
            */

            public string gift_hash { get; set; }
            
        }
    }
}
