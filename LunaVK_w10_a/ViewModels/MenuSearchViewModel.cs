using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using LunaVK.Core.DataObjects;
using Newtonsoft.Json;
using LunaVK.Core.Json;
using LunaVK.Core.Network;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Enums;

namespace LunaVK.ViewModels
{
    public class MenuSearchViewModel : GenericCollectionViewModel<SearchHint>
    {
        public string SearchName;

        private bool _canLoadMore = true;
        public override bool HasMoreDownItems
        {
            get
            {
                return this._canLoadMore;
            }
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<SearchHint>> callback)
        {
            if (offset == 0)
            {
                base._totalCount = null;
                this._canLoadMore = true;
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["q"] = this.SearchName;
            parameters["limit"] = "20";
            if (offset > 0)
                parameters["offset"] = offset.ToString();
            
            // a("filters", TextUtils.join((CharSequence)",", (Iterable)paramList));
            parameters["fields"] = "first_name,last_name,photo_100,verified";//a("fields", "photo_50,photo_100,photo_200,domain,city,online,members_count,activity,verified,trending,career,university_name,graduation,company,country");
            parameters["extended"] = "1";
            /*
             * "type": "vk_app",
        "app": {
          "type": "mini_app",
          "id": 7644481,
          "title": "Рассказать друзьям",
          "author_owner_id": -22822305,
          "is_installed": true,
          "icon_139": "https://sun9-36.userapi.com/UcDZaIDc96uDHOJSAQFJkvMjAFBoYffBoIAiSQ/XfAu0U48gok.jpg",
          "icon_150": "https://sun9-28.userapi.com/1D9imI_hp23lE2A7SUFekXtxo-v-GWHAqNmIRw/wZr3nxScOtk.jpg",
          "icon_278": "https://sun9-32.userapi.com/DqL7ajOebGhkqT2FpKyMrrs0LP1mQqYCRj5UdA/4lfn1wLIJeE.jpg",
          "icon_576": "https://sun9-66.userapi.com/o4CqAVyhuA9z0P0LANFOV_S8HIiYWFuSg5c0Uw/7cKbI_6hGXQ.jpg",
          "icon_75": "https://sun9-52.userapi.com/aDnx4hCo08ieq_D29mvzv5I9VNYhS8M2XbY3Jg/04PFgXb0HlI.jpg",
          "author_url": "https://vk.com/club22822305",
          "banner_1120": "https://sun9-22.userapi.com/xVvIsT-d3r8JID7lMKDz_qywQq6lrptuZk5ybA/SfT2GeerT1c.jpg",
          "banner_560": "https://sun9-60.userapi.com/KLRFF0_V-QMPzZf30zsc0X8Biiyyo9fr7vHxWg/1kLqLfrQSCY.jpg",
          "catalog_banner": {
            "background_color": "fff",
            "description_color": "000000",
            "title_color": "000000"
          },
          "international": false,
          "is_in_catalog": 0,
          "leaderboard_type": 0,
          "members_count": 5485458,
          "hide_tabbar": 0
        },
        "section": "vk_apps",
        "description": "Communication"
        */
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<SearchHint>>("search.getHints", parameters, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    if (result.response.count < 20)
                    {
                        base._totalCount = (uint)base.Items.Count + result.response.count;
                        this._canLoadMore = false;
                    }

                    int deleted = result.response.items.RemoveAll((a) => a.type != "profile" && a.type != "group");
                    base._totalCount -= (uint)deleted;
                    callback(result.error, result.response.items);
                }
                else
                {
                    callback(result.error, null);
                }
            });
        }


    }

    public class SearchHint
    {
        /// <summary>
        /// тип объекта. Возможные значения:
        /// mini_app
        /// group — сообщество;
        /// profile — профиль.
        /// </summary>
        public string type { get; set; }

        public VKGroup group { get; set; }

        public VKUser profile { get; set; }

        /// <summary>
        /// тип объекта. Возможные значения для сообществ:
        /// groups — группы;
        /// events — мероприятия;
        /// publics — публичные страницы.
        /// Возможные значения для профилей:
        /// correspondents — собеседники по переписке;
        /// people — популярные пользователи;
        /// friends — друзья;
        /// mutual_friends — пользователи, имеющие общих друзей с текущим. 
        /// </summary>
        public string section { get; set; }

        /// <summary>
        /// описание объекта (для сообществ — тип и число участников,
        /// например, Group, 269,136 members, для профилей друзей или пользователями,
        /// которые не являются возможными друзьями — название университета или город,
        /// для профиля текущего пользователя — That's you, для профилей возможных друзей — N mutual friends). 
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// поле возвращается, если объект был найден в глобальном поиске, всегда содержит 1.
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool global { get; set; }

        #region VM
        public string Photo
        {
            get
            {
                if (this.group != null)
                {
                    return this.group.photo_100;
                }

                if (this.profile != null)
                {
                    return this.profile.photo_100;
                }
                return "";
            }
        }

        public string Title
        {
            get
            {
                if (this.group != null)
                {
                    return this.group.name;
                }

                if (this.profile != null)
                {
                    return this.profile.Title;
                }
                return "";
            }
        }

        public bool verified
        {
            get
            {
                if (this.group != null)
                {
                    return this.group.verified;
                }

                if (this.profile != null)
                {
                    return this.profile.verified;
                }
                return false;
            }
        }

        public string PlatformIcon { get; set; }
        #endregion
    }
}
