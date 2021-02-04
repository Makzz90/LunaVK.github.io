using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Json;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.Core.Library
{
    public class GroupsService
    {
        private readonly object _lockObj = new object();
        private Dictionary<uint, VKGroup> _cachedGroups = new Dictionary<uint, VKGroup>();

        private static GroupsService _instance;
        public static GroupsService Instance
        {
            get
            {
                return GroupsService._instance ?? (GroupsService._instance = new GroupsService());
            }
        }

        public void GetGroupInfo(uint groupId, bool justBasicInfo, Action<VKResponse<VKGroup>> callback)
        {
            //description members_count start_date place site finish_date
            if (justBasicInfo)
            {
                string code = "var groups = API.groups.getById({group_id:" + groupId + ",fields:\"description,photo_100,verified,activity,cover,counters,can_message,member_status,status,members_count,site,can_post,is_subscribed,is_favorite,crop_photo,start_date,finish_date,city,country,action_button,main_section,live_covers,online_status\"});";
                code += "var g = groups[0];";
                code += "if(g.deactivated!=\"deleted\")";
                code += "{";
                code += "var mutualIds = API.groups.getMembers({group_id:" + groupId + ", count:10,filter:\"friends\"});";
                code += "var mutualUsers = API.users.get({user_ids:mutualIds.items,fields:\"photo_50\"});";
                code += "g.friends=[];g.friends.items=mutualUsers;g.friends.count=mutualIds.count;";
                code += "}";
                code += "return g;";

                VKRequestsDispatcher.Execute<VKGroup>(code, callback, (jsonStr) =>
                {
                    jsonStr = jsonStr.Replace("\"action_type\":\"\"", "\"action_type\":\"none\"");
                    jsonStr = VKRequestsDispatcher.FixArrayToObject(jsonStr, "target");
                    return VKRequestsDispatcher.FixArrayToObject(jsonStr, "counters");
                });
            }
            else
            {
                string code = "var groups = API.groups.getById({group_id:" + groupId + ",fields:\"contacts,links\"});";
                code += "var group = groups[0];";
                code += "var mutualUsers = API.users.get({ user_ids: group.contacts@.user_id,fields: \"photo_100\"});";
                code += "group.contactsUsers = mutualUsers;";
                code += "return group;";

                VKRequestsDispatcher.Execute<VKGroup>(code, callback, (jsonStr) =>
                {
                    return VKRequestsDispatcher.FixArrayToObject(jsonStr, "counters");
                });
            }
        }

        public void Join(uint groupId, bool? notSure, Action<bool> callback = null)
        {
            //#if DEBUG
            //            await Task.Delay(1000);
            //            callback(true);
            //#else


            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = groupId.ToString();
            if (notSure != null)
                parameters["not_sure"] = notSure.Value ? "1" : "0";

            VKRequestsDispatcher.DispatchRequestToVK<int>("groups.join", parameters, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                    callback?.Invoke(result.response == 1);
                else
                    callback?.Invoke(false);
            });

            //#endif
        }

        public void Leave(uint groupId, Action<bool> callback = null)
        {
            //#if DEBUG
            //            await Task.Delay(1000);
            //            callback(true);
            //#else
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = groupId.ToString();

            VKRequestsDispatcher.DispatchRequestToVK<int>("groups.leave", parameters, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                    callback?.Invoke(result.response == 1);
                else
                    callback?.Invoke(false);
            });
            //#endif
        }
        /*
        public async Task<VKCountedItemsObject<VKTopic>> GetTopics(int gid, int offset)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = gid.ToString();
            parameters["extended"] = "1";
            parameters["preview"] = "2";
            parameters["preview_length"] = "40";
            parameters["offset"] = offset.ToString();
            parameters["count"] = "30";
            var temp = await RequestsDispatcher.GetResponse<VKCountedItemsObject<VKTopic>>("board.getTopics", parameters);
            if (temp.error.error_code == VKErrors.None)
                return temp.response;

            return null;
        }*/

        public void GetSubscribers(uint communityId, int offset, int count, string filter, bool needManagersAndContacts, Action<VKResponse<VKCountedItemsObject<VKUser>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            //string code = string.Format("var subscribers = API.groups.getMembers({{ group_id: {0}, offset: {1}, count: {2}, filter: \"{3}\", fields: \"online,online_mobile,photo_50,first_name_acc,last_name_acc,occupation,city,bdate\" }}); var managers = []; var contacts = []; if ({4}) {{ managers = API.groups.getMembers({{ group_id: {5}, filter: \"managers\" }}).items; contacts = API.groups.getById({{ group_id: {6}, fields: \"contacts\" }})[0].contacts; }} return  {{ subscribers: subscribers, managers: managers, contacts: contacts}};", communityId, offset, count, filter, (needManagersAndContacts ? "true" : "false"), communityId, communityId);
            parameters["group_id"] = communityId.ToString();
            parameters["extended"] = "1";

            if (offset > 0)
                parameters["offset"] = offset.ToString();

            if (count > 0)
                parameters["count"] = count.ToString();

            parameters["filter"] = filter;
            parameters["fields"] = "online,online_mobile,photo_50,first_name_acc,last_name_acc,occupation,city,bdate";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKUser>>("groups.getMembers", parameters, callback);
        }

        public void CreateCommunity(string name, string type, string description, int subtype, Action<VKResponse<VKGroup>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<VKGroup>("groups.create", new Dictionary<string, string>()
            {
                { "title", name },
                { "type", type },
                { "description", description },
                { "subtype", subtype.ToString() }
            }, callback);
        }

        public void Search(string query, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKGroup>>> callback, Dictionary<string, object> searchParams = null)
        {
            if (string.IsNullOrEmpty(query))
                query = "*";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["q"] = query;
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["fields"] = "start_date,name,verified,photo_100,activity,members_count";
            if (searchParams != null)
            {
                if (searchParams.ContainsKey("country") /*&& !searchParams.ContainsKey("city")*/)
                    parameters["country_id"] = ((uint)searchParams["country"]).ToString();

                if (searchParams.ContainsKey("city"))//При передаче этого параметра поле country_id игнорируется. 
                    parameters["city_id"] = ((uint)searchParams["city"]).ToString();

                if (searchParams.ContainsKey("sort"))
                    parameters["sort"] = ((int)searchParams["sort"]).ToString();

                if (searchParams.ContainsKey("group_type"))
                {
                    int type = (int)searchParams["group_type"];
                    if (type == 0)
                    {
                        parameters["type"] = "group";
                    }
                    else if (type == 1)
                    {
                        parameters["type"] = "page";
                    }
                    else if (type == 2)
                    {
                        parameters["type"] = "event";
                    }
                }

                if (searchParams.ContainsKey("future"))
                    parameters["future"] = "1";
            }
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKGroup>>("groups.search", parameters, callback);
        }

        public void GetCatalogCategoriesPreview(Action<VKResponse<GroupCatalogInfoExtended>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["extended"] = "1";
            VKRequestsDispatcher.DispatchRequestToVK<GroupCatalogInfoExtended>("groups.getCatalogInfo", parameters, callback);
        }

        public void GetCommunity(uint communityId, string fields, Action<VKGroup> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = communityId.ToString();
            parameters["fields"] = fields;

            VKRequestsDispatcher.DispatchRequestToVK<List<VKGroup>>("groups.getById", parameters, (result) =>
            {
                callback(result.error.error_code == VKErrors.None ? result.response[0] : null);
            });
        }

        public void EditComment(int gid, uint tid, uint cid, string text, List<string> attachmentIds, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = gid.ToString();
            parameters["topic_id"] = tid.ToString();
            parameters["comment_id"] = cid.ToString();
            parameters["text"] = text;
            if (attachmentIds.Count > 0)
                parameters["attachments"] = attachmentIds.GetCommaSeparated();
            VKRequestsDispatcher.DispatchRequestToVK<int>("board.editComment", parameters, callback);
        }

        public void CreateTopic(uint group_id, string title, string text, List<string> attachments, bool fromGroup, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = group_id.ToString();
            parameters["title"] = title;
            parameters["text"] = text;
            parameters["attachments"] = attachments.GetCommaSeparated();
            parameters["from_group"] = fromGroup ? "1" : "0";

            VKRequestsDispatcher.DispatchRequestToVK<int>("board.addTopic", parameters, callback);
        }

        /// <summary>
        /// Возвращает список сообществ указанного пользователя
        /// </summary>
        /// <param name="userId">идентификатор пользователя, информацию о сообществах которого требуется получить</param>
        /// <param name="offset">смещение, необходимое для выборки определённого подмножества сообществ</param>
        /// <param name="count">количество сообществ, информацию о которых нужно вернуть</param>
        /// <param name="filter">список фильтров сообществ, которые необходимо вернуть, перечисленные через запятую. Доступны значения admin, editor, moder, advertiser, groups, publics, events, hasAddress. По умолчанию возвращаются все сообщества пользователя. При указании фильтра hasAddress вернутся сообщества, в которых указаны адреса в соответствующем блоке, admin будут возвращены сообщества, в которых пользователь является администратором, editor — администратором или редактором, moder — администратором, редактором или модератором, advertiser — рекламодателем.Если передано несколько фильтров, то их результат объединяется. </param>
        /// <param name="callback"></param>
        public void GetUserGroups(uint userId, int offset, int count, string filter, Action<VKResponse<VKCountedItemsObject<VKGroup>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["extended"] = "1";
            parameters["count"] = count.ToString();
            parameters["filter"] = filter;
            parameters["offset"] = offset.ToString();
            parameters["fields"] = "name,verified,photo_100,activity,members_count,start_date,finish_date";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKGroup>>("groups.get", parameters, callback);
        }

        public void GetManagers(uint communityId, int offset, int count, bool needContacts, Action<VKResponse<CommunityManagers>> callback)
        {
            string code = "var managers = API.groups.getMembers({ group_id: " + communityId + ", filter: \"managers\", fields: \"photo_50\" }); var contacts = API.groups.getById({ group_id: " + communityId + ", fields: \"contacts\" })[0].contacts; return { managers: managers, contacts: contacts };";
            VKRequestsDispatcher.Execute<CommunityManagers>(code, callback);
        }








        public class GroupCatalogInfoExtended
        {
            public int enabled { get; set; }

            public List<GroupCatalogCategoryPreview> categories { get; set; }
        }

        public class GroupCatalogCategoryPreview
        {
            /// <summary>
            /// идентификатор категории
            /// </summary>
            public int id { get; set; }

            /// <summary>
            /// название категории
            /// </summary>
            public string name { get; set; }



            // extended ---->

            /// <summary>
            /// Количество страниц
            /// </summary>
            public int page_count { get; set; }

            public List<VKGroup> page_previews { get; set; }

            #region VM
            public BitmapImage Preview1
            {
                get { return this.GetGroupAvatar(0); }
            }

            public BitmapImage Preview2
            {
                get { return this.GetGroupAvatar(1); }
            }

            public BitmapImage Preview3
            {
                get { return this.GetGroupAvatar(2); }
            }

            private BitmapImage GetGroupAvatar(int ind)
            {
                if (this.page_previews.Count > ind)
                    return new BitmapImage(new Uri(this.page_previews[ind].photo_50));
                return null;
            }

            public string Subtitle
            {
                get { return UIStringFormatterHelper.FormatNumberOfSomething(this.page_count, "OnePageFrm", "TwoFourPageFrm", "FiveMorePageFrm"); }
            }

            public List<SearchG> LoadedGroups = new List<SearchG>();

            public class SearchG : VKGroup
            {
                public string PlatformIcon { get; set; }//binding bugfix. todo

                public string Subtitle
                {
                    get
                    {
                        return LocalizedStrings.GetString(this.type == VKGroupType.Group ? "CommunityType_Group/Text" : "CommunityType_PublicPage/Text");
                    }
                }
            }
            #endregion
        }

        public sealed class CommunityManagers
        {
            public VKCountedItemsObject<VKUser> managers { get; set; }

            public List<VKGroupContact> contacts { get; set; }
        }

        /*
    public void AddCachedGroup(VKGroup group)
    {
        if (group == null)
            return;
        this._cachedGroups[(uint)group.id] = group;
    }
    */
        public void GetBlacklist(uint gid, int offset, int count, Action<VKResponse<BlockedUsers>> callback)
        {
            string code = string.Format("var blocked_users = API.groups.getBanned({{ group_id: {0}, offset: {1}, count: {2}, fields: \"photo_50,sex\" }}); var managers = API.users.get({{ user_ids: blocked_users.items@.ban_info@.admin_id, fields: \"sex\" }}); return {{ blocked_users: blocked_users, managers: managers }};", gid, offset, count);

            VKRequestsDispatcher.Execute<BlockedUsers>(code, callback);
        }

        public void GetCommunitySettings(uint id, Action<VKResponse<VKCommunitySettings>> callback)
        {
            //string code = string.Format("var settings = API.groups.getSettings({{group_id:{0}}}); var community = API.groups.getById({{ group_id: {0}}})[0]; settings.type = community.type; return settings;", id);
            //VKRequestsDispatcher.Execute<CommunitySettings>(code, callback);
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = id.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKCommunitySettings>("groups.getSettings", parameters, callback);

        }

        /*
        public void SetCommunityInformation(uint id, string name, string description, int category, int subcategory, string site, int accessLevel, string domain, int ageLimits, string foundationDate, long? eventOrganizerId, string eventOrganizerPhone, string eventOrganizerEmail, int eventStartDate, int eventFinishDate, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "group_id",//	идентификатор сообщества
          id.ToString()
        },
        {
          "title",//название сообщества
          name
        },
        {
          "description",//	описание сообщества
          description
        },
        {
          "subject",//	тематика сообщества
          category.ToString()
        },
        {
          "public_category",//категория публичной страницы
          category.ToString()
        },
        {
          "public_subcategory",//подкатегория публичной станицы.
          subcategory.ToString()
        },
        {
          "website",//адрес сайта, который будет указан в информации о группе.
          site
        },
        {
          "access",//тип группы. Возможные значения 0 — открытая; 1 — закрытая;  2 — частная. 
          accessLevel.ToString()
        },
        {
          "age_limits",
          ageLimits.ToString()
        }
      };
            if (domain != null)
                parameters.Add("screen_name", domain);//	короткое имя сообщества
            if (foundationDate != null)
                parameters.Add("public_date", foundationDate);//	дата основания компании,
            if (eventOrganizerId.HasValue)
            {
                //rss адрес rss для импорта новостей
                parameters.Add("event_group_id", eventOrganizerId.Value.ToString());//идентификатор группы, которая является организатором события (только для событий). 
                parameters.Add("phone", eventOrganizerPhone);//номер телефона организатора (для мероприятий).
                parameters.Add("email", eventOrganizerEmail);//электронный адрес организатора (для мероприятий).
                parameters.Add("event_start_date", eventStartDate.ToString());//дата начала события
                parameters.Add("event_finish_date", eventFinishDate.ToString());//дата окончания события
            }
            VKRequestsDispatcher.DispatchRequestToVK<int>("groups.edit", parameters, callback);
        }
        */
        public void SetCommunityInformation(uint id, Dictionary<string, string> infoParams, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = infoParams;

            parameters["group_id"] = id.ToString();
            

            VKRequestsDispatcher.DispatchRequestToVK<int>("groups.edit", parameters, callback);
        }

        public void SetCommunityPlacement(uint communityId, int countryId, int cityId, string address, string title, double latitude, double longitude, Action<VKResponse<PlacementEditingResult>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<PlacementEditingResult>("groups.editPlace", new Dictionary<string, string>()
      {
        {
          "group_id",
          communityId.ToString()
        },
        {
          "country_id",
          countryId.ToString()
        },
        {
          "city_id",
          cityId.ToString()
        },
        {
          "address",
          address
        },
        {
          "title",
          title
        },
        {
          "latitude",
          latitude.ToString()
        },
        {
          "longitude",
          longitude.ToString()
        }
      }, callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="wallOrComments">стена</param>
        /// <param name="photos">фотографии</param>
        /// <param name="videos">видеозаписи</param>
        /// <param name="audios">аудиозаписи</param>
        /// <param name="documents">документы сообщества</param>
        /// <param name="discussions">обсуждения</param>
        /// <param name="links">ссылки (доступно только для публичных страниц)</param>
        /// <param name="events">события (доступно только для публичных страниц)</param>
        /// <param name="contacts">контакты (доступно только для публичных страниц)</param>
        /// <param name="strongLanguageFilter">фильтр нецензурных выражений в комментариях</param>
        /// <param name="keyWordsFilter">фильтр по ключевым словам в комментариях</param>
        /// <param name="keyWords">ключевые слова для фильтра комментариев</param>
        /// <param name="callback"></param>
        public void SetCommunityServices(uint id, int wallOrComments, int photos, int videos, int audios, int documents, int discussions, int links, int events, int contacts, int strongLanguageFilter, int keyWordsFilter, string keyWords, Action<VKResponse<int>> callback)
        {
            //places места (доступно только для публичных страниц)
            //wiki wiki-материалы сообщества
            //messages сообщения сообщества
            //articles
            //addresses
            //market
            //market_comments
            //market_country
            //market_city
            //market_currency
            //market_contact
            //market_wiki
            //main_section
            //secondary_section
            //country
            //city
            //ads_easy_promote_disable_main_screen_button
            //ads_easy_promote_disable_promote_post_button
            VKRequestsDispatcher.DispatchRequestToVK<int>("groups.edit", new Dictionary<string, string>()
      {
        {
          "group_id",
          id.ToString()
        },
        {
          "wall",
          wallOrComments.ToString()
        },
        {
          "photos",
          photos.ToString()
        },
        {
          "video",
          videos.ToString()
        },
        {
          "audio",
          audios.ToString()
        },
        {
          "docs",
          documents.ToString()
        },
        {
          "topics",
          discussions.ToString()
        },
        {
          "links",
          links.ToString()
        },
        {
          "events",
          events.ToString()
        },
        {
          "contacts",
          contacts.ToString()
        },
        {
          "obscene_filter",
          strongLanguageFilter.ToString()
        },
        {
          "obscene_stopwords",
          keyWordsFilter.ToString()
        },
        {
          "obscene_words",
          keyWords
        }
      }, callback);
        }

        public void GetRequests(uint id, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKUser>>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKUser>>("groups.getRequests", new Dictionary<string, string>()
      {
        {
          "group_id",
          id.ToString()
        },
        {
          "offset",
          offset.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "fields",
          "photo_50,photo_100,occupation,bdate,city"
        }
      }, callback);
        }

        public void HandleRequest(uint communityId, uint userId, bool isAcception, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "group_id",
          communityId.ToString()
        },
        {
          "user_id",
          userId.ToString()
        }
      };
            VKRequestsDispatcher.DispatchRequestToVK<int>(isAcception ? "groups.approveRequest" : "groups.removeUser", parameters, callback);
        }

        public void GetTopics(uint gid, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKTopic>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = gid.ToString();
            parameters["extended"] = "1";
            parameters["preview"] = "2";
            parameters["preview_length"] = "40";
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKTopic>>("board.getTopics", parameters, callback);
        }

        public void GetCatalog(int categoryId, Action<VKResponse<VKCountedItemsObject<GroupCatalogCategoryPreview.SearchG>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["category_id"] = categoryId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<GroupCatalogCategoryPreview.SearchG>>("groups.getCatalog", parameters, callback);
        }

        /// <summary>
        /// Позволяет удалить ссылки из сообщества
        /// </summary>
        /// <param name="communityId">идентификатор сообщества, ссылки которого нужно удалить</param>
        /// <param name="linkId">идентификатор ссылки, которую необходимо удалить</param>
        /// <param name="callback"></param>
        public void DeleteLink(uint communityId, uint linkId, Action<VKResponse<int>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<int>("groups.deleteLink", new Dictionary<string, string>()
            {
                { "group_id", communityId.ToString() },
                { "link_id", linkId.ToString() }
            }, callback);
        }

        /// <summary>
        /// Позволяет добавлять ссылки в сообщество
        /// </summary>
        /// <param name="communityId">идентификатор сообщества, в которое добавляется ссылка</param>
        /// <param name="url">адрес ссылки</param>
        /// <param name="description">текст ссылки</param>
        /// <param name="callback"></param>
        public void AddLink(uint communityId, string url, string description, Action<VKResponse<VKGroupLink>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<VKGroupLink>("groups.addLink", new Dictionary<string, string>()
            {
                { "group_id", communityId.ToString() },
                { "link", url },
                { "text", description }
            }, callback);
        }

        /// <summary>
        /// Позволяет редактировать ссылки в сообществе.
        /// </summary>
        /// <param name="communityId">идентификатор сообщества, в которое добавляется ссылка</param>
        /// <param name="linkId">идентификатор ссылки</param>
        /// <param name="description">новый текст описания для ссылки</param>
        /// <param name="callback"></param>
        public void EditLink(uint communityId, uint linkId, string description, Action<VKResponse<int>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<int>("groups.editLink", new Dictionary<string, string>()
            {
                { "group_id", communityId.ToString() },
                { "link_id", linkId.ToString() },
                { "text", description }
            }, callback);
        }

        /// <summary>
        /// Позволяет менять местоположение ссылки в списке
        /// </summary>
        /// <param name="communityId">идентификатор группы, внутри которой перемещается ссылка</param>
        /// <param name="linkId">идентификатор ссылки, которую необходимо переместить</param>
        /// <param name="after">идентификатор ссылки после которой необходимо разместить перемещаемую ссылку. 0 – если ссылку нужно разместить в начале списка. </param>
        /// <param name="callback"></param>
        public void ReorderLink(uint communityId, uint linkId, uint after, Action<VKResponse<int>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<int>("groups.reorderLink", new Dictionary<string, string>()
            {
                { "group_id", communityId.ToString() },
                { "link_id", linkId.ToString() },
                { "after", after.ToString() }
            }, callback);
        }

        public void SetCachedGroups(IEnumerable<VKGroup> groups)
        {
            if (groups == null)
                return;

            lock (this._lockObj)
            {
                foreach (var group in groups)
                {
                    //user.CachedDateTime = DateTime.Now;
                    this._cachedGroups[(uint)group.id] = group;
                }
            }
        }

        public VKGroup GetCachedGroup(uint id)
        {
            if (this._cachedGroups.ContainsKey(id))
                return this._cachedGroups[id];
            return null;
        }

        public void GetTopicComments(uint gid, uint tid, int offset, int count, uint start_comment_id, Action<VKResponse<BoardComments>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = gid.ToString();
            parameters["topic_id"] = tid.ToString();

            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["extended"] = "1";
            parameters["need_likes"] = "1";
            //if (offset == 0)
                parameters["sort"] = "desc";
            if (start_comment_id > 0)
                parameters["start_comment_id"] = start_comment_id.ToString();
            parameters["fields"] = "first_name_dat";
            //"One of the parameters specified was missing or invalid: offset without start_comment_id should be positive"
            VKRequestsDispatcher.DispatchRequestToVK<BoardComments>("board.getComments", parameters, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    foreach (var item in result.response.items)
                    {
                        VKBaseDataForGroupOrUser owner = null;

                        if (item.from_id < 0)
                            owner = result.response.groups.Find((pro) => pro.id == (-item.from_id));
                        else
                            owner = result.response.profiles.Find((pro) => pro.id == (item.from_id));
                        item.User = owner;

                        item.Marked = item.id == start_comment_id;
                    }
                }

                callback(result);
            });
        }

        public void AddTopicComment(uint gid, uint tid, string text, List<string> attachments, Action<VKResponse<VKComment>> callback, uint stickerId = 0, bool fromGroup = false, string stickerReferrer = "")
        {
            string code = string.Format("var new_comment_id = API.board.createComment({{group_id: {0},topic_id: {1},text: \"{2}\",from_group: {3},sticker_id: {4},attachments: \"{5}\",sticker_referrer: \"{6}\"}}); var last_comments = API.board.getComments({{group_id: {7},topic_id: {8},need_likes: 1,count: 10,sort: \"desc\",preview_length: 0,allow_group_comments: 1}}).items; var i = last_comments.length - 1; while (i >= 0){{ if (last_comments[i].id == new_comment_id)        return last_comments[i];    i = i - 1;}} return null;", gid, tid, text.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r"), (fromGroup ? "1" : "0"), stickerId, attachments.GetCommaSeparated(","), stickerReferrer, gid, tid);

            VKRequestsDispatcher.Execute<VKComment>(code, callback);
        }

        public void DeleteComment(uint gid, uint tid, uint cid, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = gid.ToString();
            parameters["topic_id"] = tid.ToString();
            parameters["comment_id"] = cid.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("board.deleteComment", parameters, callback);
        }

        public void EditComment(uint gid, uint tid, uint cid, string text, List<string> attachmentIds, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = gid.ToString();
            parameters["topic_id"] = tid.ToString();
            parameters["comment_id"] = cid.ToString();
            parameters["text"] = text;
            if (attachmentIds.Count > 0)
                parameters["attachments"] = attachmentIds.GetCommaSeparated(",");
            VKRequestsDispatcher.DispatchRequestToVK<int>("board.editComment", parameters, callback);
        }


        /// <summary>
        /// BUG:метода не существует?
        /// </summary>
        /// <param name="gId"></param>
        /// <param name="reportReason"></param>
        /// <param name="callback"></param>
        public void Report(uint gId, ReportReason reportReason, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = gId.ToString();
            parameters["reason"] = ((int)reportReason).ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("group.report", parameters, callback);
        }

        public void Invate(uint groupId, uint userId, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = groupId.ToString();
            parameters["user_id"] = userId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("groups.invite", parameters, callback);
        }

        public void GetCommunityInvitations(int offset, int count, Action<VKResponse<CommunityInvitationsList>> callback)
        {
            string code = "var inviters=[]; var invitations=API.groups.getInvites({count:" + count + ",offset:" + offset + ",fields:\"members_count\"});";
            code += "if (invitations.items.length>0)";
            code += "{";
            code +=     "inviters=API.users.get({user_ids:invitations.items@.invited_by,fields:\"sex\"});";
            code += "}";
            code += "return {count:invitations.count, invitations:invitations.items, inviters:inviters};";
            
            VKRequestsDispatcher.Execute<CommunityInvitationsList>(code, callback);
        }

        public void GetInvitations(uint id, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKUser>>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKUser>>("groups.getInvitedUsers", new Dictionary<string, string>()
          {
            {
              "group_id", id.ToString()
            },
            {
              "offset", offset.ToString()
            },
            {
              "count", count.ToString()
            },
            {
              "fields", "photo_50,photo_100,photo_max,occupation"
            }
          }, callback);
        }

        public void GetCallbackServers(uint id, Action<VKResponse<VKCountedItemsObject<CallbackServer>>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<CallbackServer>>("groups.getCallbackServers", new Dictionary<string, string>()
          {
            { "group_id", id.ToString() }
            }, callback);
        }
        
        public class BoardComments : VKCountedItemsObject<VKComment>
        {
            public int real_offset { get; set; }

            /// <summary>
            /// количество комментариев в ветке. 
            /// </summary>
            public int current_level_count { get; set; }

            /// <summary>
            /// может ли текущий пользователь оставлять комментарии в этой ветке. 
            /// </summary>
            [JsonConverter(typeof(VKBooleanConverter))]
            public bool can_post { get; set; }

            /// <summary>
            /// нужно ли отображать кнопку «ответить» в ветке. 
            /// </summary>
            [JsonConverter(typeof(VKBooleanConverter))]
            public bool show_reply_button { get; set; }

            /// <summary>
            /// могут ли сообщества оставлять комментарии в ветке. 
            /// </summary>
            [JsonConverter(typeof(VKBooleanConverter))]
            public bool groups_can_post { get; set; }
        }

        public sealed class CommunitySubscribers
        {
            public VKCountedItemsObject<VKUser> subscribers { get; set; }

            public List<VKUser> managers { get; set; }

            public List<VKGroupContact> contacts { get; set; }
        }

        public sealed class BlockedUsers
        {
            public VKCountedItemsObject<BlockedUsersType> blocked_users { get; set; }

            public List<VKUser> managers { get; set; }
        }

        public class BlockedUsersType
        {
            public string type { get; set; }
            public VKUser profile { get; set; }
            public VKGroup group { get; set; }
            public VKUser.BlockInformation ban_info { get; set; }
        }

        public sealed class CommunityInvitationsList
        {
            public int count { get; set; }

            public VKGroup[] invitations { get; set; }

            public VKUser[] inviters { get; set; }
        }

        public sealed class PlacementEditingResult
        {
            public int success { get; set; }
        }

        public class CallbackServer
        {
            /// <summary>
            /// идентификатор сервера
            /// </summary>
            public int id { get; set; }

            /// <summary>
            /// название сервера
            /// </summary>
            public string title { get; set; }

            /// <summary>
            /// идентификатор пользователя, который добавил сервер (может содержать 0)
            /// </summary>
            public int creator_id { get; set; }

            /// <summary>
            /// URL сервера
            /// </summary>
            public string url { get; set; }

            /// <summary>
            /// секретный ключ
            /// </summary>
            public string secret_key { get; set; }

            /// <summary>
            ///  статус сервера (unconfigured,failed ,wait,ok)
            /// </summary>
            public string status { get; set; }
        }
    }
}
