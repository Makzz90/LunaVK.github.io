using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;

namespace LunaVK.Core.Library
{
    public class AccountService
    {
        private static AccountService _instance;

        public static AccountService Instance
        {
            get
            {
                if (AccountService._instance == null)
                    AccountService._instance = new AccountService();
                return AccountService._instance;
            }
        }

        public void StatsTrackEvents(List<AppEventBase> appEvents, Action<VKResponse<ResponseWithId>> callback)
        {
            List<AppEventBase> appEventBaseList1 = new List<AppEventBase>();
            List<AppEventBase> appEventBaseList2 = new List<AppEventBase>();
            foreach (AppEventBase appEvent in appEvents)
            {
                if (appEvent is AppEventAdImpression)
                    appEventBaseList2.Add(appEvent);
                else
                    appEventBaseList1.Add(appEvent);
            }
            string format1 = "API.stats.trackEvents({{events:\"{0}\"}});";
            string format2 = "API.adsint.registerAdEvents({{events:\"{0}\"}});";
            string code = "";
            if (appEventBaseList2.Count > 0)
                code += string.Format(format2, JsonConvert.SerializeObject(appEventBaseList2).Replace("\"", "\\\""));
            if (appEventBaseList1.Count > 0)
                code += string.Format(format1, JsonConvert.SerializeObject(appEventBaseList1).Replace("\"", "\\\""));
            VKRequestsDispatcher.Execute<ResponseWithId>(code, callback);
        }

        public void GetBannedUsers(int offset, int count, Action<VKResponse<VKCountedItemsObject<int>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["fields"] = "photo_50";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<int>>("account.getBanned", parameters, callback);
        }

        public void UnbanUsers(IReadOnlyList<int> user_ids, Action<VKResponse<ResponseWithId>> callback)
        {
            string format = "API.account.unbanUser({{user_id:{0}}});";
            string code = "";
            foreach (int userId in user_ids)
                code = code + string.Format(format, userId) + Environment.NewLine;
            VKRequestsDispatcher.Execute<ResponseWithId>(code, callback);
        }

        public void BanUser(int owner_id, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = owner_id.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("account.ban", parameters, callback);
        }

        public void SetSilenceMode(string deviceId, int nrOfSeconds, Action<VKResponse<int>> callback, int peer_id = 0)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["device_id"] = deviceId;
            parameters["time"] = nrOfSeconds.ToString();
            if (peer_id != 0)
                parameters["peer_id"] = peer_id.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("account.setSilenceMode", parameters, callback);
        }

        public void RegisterDevice(string deviceId, string token, string settings, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["token"] = token;
            parameters["device_id"] = deviceId;
            //parameters["device_model"] = deviceModelViewable;//Fly FS407
            //parameters["system_version"] = systemVersion;//6.0
            //parameters["app_version"] = appVersion;//1206
            if (!string.IsNullOrEmpty(settings))//{"sdk_open":"on","new_post":"on","friend_accepted":"on","wall_publish":"on","group_accepted":"on","money_transfer":"on","msg":"on","chat":"on","friend":"on","friend_found":"on","reply":"on","comment":"on","mention":"on","like":"on","repost":"on","wall_post":"on","group_invite":"on","event_soon":"on","tag_photo":"on","tag_video":"on","app_request":"on","gift":"on","birthday":"on","live":"on"}
                parameters["settings"] = settings;
            VKRequestsDispatcher.DispatchRequestToVK<int>("account.registerDevice", parameters, callback);
        }

        public void UnregisterDevice(string deviceId, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["device_id"] = deviceId;
            VKRequestsDispatcher.DispatchRequestToVK<int>("account.unregisterDevice", parameters, callback);
        }

        public void SavePushSettings(string deviceId, Action<VKResponse<int>> callback = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["device_id"] = deviceId;
            parameters["settings"] = Settings.PushSettings.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("account.setPushSettings", parameters, callback);
        }

        public void SetPushSettings(string deviceId, string key, string value, Action<VKResponse<ResponseWithId>> callback = null, string key2 = "", string value2 = "")
        {
            string format = "API.account.setPushSettings({{device_id:\"{0}\", key:\"{1}\", value:\"{2}\"}});";
            string code = string.Format(format, deviceId, key, value);
            if (!string.IsNullOrWhiteSpace(key2) && !string.IsNullOrWhiteSpace(value2))
                code = code + Environment.NewLine + string.Format(format, deviceId, key2, value2);
            VKRequestsDispatcher.Execute<ResponseWithId>(code, callback);
        }

        public void GetPrivacySettings(Action<VKResponse<PrivacySettingsInfo>> callback)
        {
            string code = "var ps = API.account.getPrivacySettings();";
            code += "var ss = ps.settings;";
            code += "var owners =[];";
            code += "var i = ss.length - 1;";
            code += "while (i > 0)";
            code += "{";
            code +=     "var s = ss[i];";
            code +=     "if (s.value.owners.allowed != null)";
            code +=     "{";
            code +=         "owners = owners + s.value.owners.allowed;";
            code +=     "}";
            code +=     "if (s.value.owners.excluded != null)";
            code +=     "{";
            code +=         "owners = owners + s.value.owners.excluded;";
            code +=     "}";
            code +=     "i = i - 1;";
            code += "}";
            code += "var users = API.users.get({ user_ids: owners,fields: \"photo_50\"});";
            code += "return { profiles: users,lists: API.friends.getLists({ return_system: 1}),settings:ps.settings,sections:ps.sections,supported_categories:ps.supported_categories};";
            /*
             * profile, photos_with, photos_saved, groups, audios, gifts, places, hidden_friends, wall, wall_send, replies_view, status_replies, photos_tagme, mail_send, calls, appscall, groups_invite, apps_invite, friends_requests, search_by_reg_phone, stories, stories_replies, stories_questions, lives, lives_replies, online, chat_invite, vkrun_steps, page_access, company_messages, closed_profile, mobile or home",
             * */
            VKRequestsDispatcher.Execute<PrivacySettingsInfo>(code, (resutl) =>
            {
                if (resutl.error.error_code == Enums.VKErrors.None)
                {
                    /*
                    foreach (PrivacySection section in resutl.response.sections)
                    {
                        List<PrivacySetting> l = resutl.response.settings.FindAll((ss) => { return ss.section == section.name; });
                        section.Settings = l;
                    }
                    */
                    /*
                    foreach (PrivacySetting section in resutl.response.settings)
                    {
                        var temp = resutl.response.sections.Find((ss) => ss.name == section.section || ss.name == "others");
                        section.SectionTitle = temp.title;
                        section.Description = temp.description;

                        var temp2 = resutl.response.supported_categories.Find((ss) => ss.value == section.value.category);
                        if(temp2!=null)
                            section.FriendlyDescPart = temp2.title;
                    }
                    */
                }

                callback(resutl);
            });
        }

        public void SetPrivacy(string key, string value, Action<VKResponse<SetPrivacyResponse>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["key"] = key;
            parameters["value"] = value;
            VKRequestsDispatcher.DispatchRequestToVK<SetPrivacyResponse>("account.setPrivacy", parameters, callback);
        }

        public void GetSettingsAccountInfo(Action<VKResponse<SettingsAccountInfo>> callback)
        {
            VKRequestsDispatcher.Execute<SettingsAccountInfo>("var account = API.account.getInfo(); var pi = API.account.getProfileInfo(); return {\"Account\": account, \"ProfileInfo\":pi};", callback);
        }

        public void StatusSet(string text, string audio, long groupId, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(audio))
                parameters["audio"] = audio;
            else
                parameters["text"] = text;
            if (groupId > 0)
                parameters["group_id"] = groupId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("status.set", parameters, callback);
        }

        public void ResolveScreenName(string name, Action<VKResponse<ResolvedData>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<ResolvedData>("utils.resolveScreenName", new Dictionary<string, string>() { { "screen_name", name } }, callback, (json) =>
            {
                json = VKRequestsDispatcher.FixArrayToObject(json, "response");
                return json;
            });
        }

        /// <summary>
        /// Возвращает информацию о том, является ли внешняя ссылка заблокированной на сайте ВКонтакте. 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        public void CheckLink(string url, Action<VKResponse<CheLinkData>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<CheLinkData>("utils.checkLink", new Dictionary<string, string>() { { "url", url } }, callback);
        }

        [Obsolete("Не доступа к LookupContacts")]
        public void LookupContacts(string service, string myContact, List<string> contacts, Action<VKResponse<LookupContactsResponse>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["service"] = service;
            parameters["fields"] = "photo_max,verified,occupation,city,country,friend_status,common_count";
            if (!string.IsNullOrEmpty(myContact))
                parameters["mycontact"] = myContact;
            if (!contacts.IsNullOrEmpty())
                parameters["contacts"] = string.Join(",", contacts);
            VKRequestsDispatcher.DispatchRequestToVK<LookupContactsResponse>("account.lookupContacts", parameters, callback);
        }

        public void GetSettingsProfileInfo(Action<VKResponse<ProfileInfo>> callback)
        {
            VKRequestsDispatcher.Execute<GetProfileInfoResponse>("var pi = API.account.getProfileInfo(); var u = API.users.get({fields:\"photo_max\"}); var partner;if (pi.relation_partner + \"\"!=\"\"){    partner = API.users.get({\"user_ids\":pi.relation_partner.id, \"fields\":\"photo_max,first_name_gen,last_name_gen,sex\"});    return {\"User\":u[0], \"ProfileInfo\":pi, \"Partner\":partner[0]};}var relation_requests;if (pi.relation_requests + \"\"!=\"\"){    relation_requests = API.users.get({\"user_ids\":pi.relation_requests@.id, \"fields\":\"sex\"});}return {\"User\":u[0], \"ProfileInfo\":pi, \"RelationRequests\":relation_requests};", (result) =>
            {
                if (result.error.error_code == Enums.VKErrors.None)
                {
                    var response = result.response;
                    response.ProfileInfo.photo_max = response.User.photo_max;
                    if (response.partner != null && response.ProfileInfo.relation_partner != null)
                    {
                        response.ProfileInfo.relation_partner.photo_max = response.partner.photo_max;
                        response.ProfileInfo.relation_partner.first_name_gen = response.partner.first_name_gen;
                        response.ProfileInfo.relation_partner.last_name_gen = response.partner.last_name_gen;
                        response.ProfileInfo.relation_partner.sex = response.partner.sex;
                    }
                    if (response.RelationRequests != null)
                        response.ProfileInfo.relation_requests = response.RelationRequests;
                    callback(new VKResponse<ProfileInfo>() { error = result.error, response = response.ProfileInfo });
                }
                else
                {
                    callback(new VKResponse<ProfileInfo>() { error = result.error, response = null });
                }
            });
        }

        public void DeleteProfilePhoto(Action<VKResponse<VKUser>> callback)
        {
            VKRequestsDispatcher.Execute<VKUser>("var ph = API.photos.get({album_id:-6, rev:1, count:1}).items[0]; API.photos.delete({owner_id:ph.owner_id, photo_id:ph.id}); return API.users.get({fields:\"photo_max\"})[0];", callback);
        }

        public void SaveSettingsAccountInfo(Dictionary<string, string> parameters, Action<VKResponse<SaveProfileResponse>> callback, UploadPhotoResponseData newPhotoData = null)
        {
            if (parameters.Count == 0)
            {
                if (newPhotoData == null)
                    return;
                UsersService.Instance.SaveProfilePhoto(newPhotoData, (result => callback(new VKResponse<SaveProfileResponse>() { error = result.error, response = new SaveProfileResponse() })));
            }
            else
            {
                VKRequestsDispatcher.DispatchRequestToVK<SaveProfileResponse>("account.saveProfileInfo", parameters, (result =>
                {
                    if (result.error.error_code == Enums.VKErrors.None && newPhotoData != null)
                        UsersService.Instance.SaveProfilePhoto(newPhotoData, (pres => callback(result)));
                    else
                        callback(result);
                }));
            }
        }
    }



















    public class SetPrivacyResponse
    {
        public string category { get; set; }

        public bool is_enabled { get; set; }//для закрытого профиля
    }

    public class SaveProfileResponse
    {
        public int changed { get; set; }

        public ProfileInfo.NameChangeRequest name_request { get; set; }
    }

    public abstract class AppEventBase
    {
        public abstract string e { get; }
    }

    public class AppEventAdImpression : AppEventBase
    {
        public override string e { get { return ""; } }

        public string event_type { get { return "impression"; } }

        public string ad_data_impression { get; set; }
    }

    //Get
    public class PrivacySettingsInfo
    {
        public List<PrivacySetting> settings { get; set; }

        public List<PrivacySection> sections { get; set; }

        public List<ValueTitle> supported_categories { get; set; }

        public VKCountedItemsObject<FriendsList> lists { get; set; }

        public List<VKUser> profiles { get; set; }

        public class ValueTitle
        {
            /// <summary>
            /// all friends friends_of_friends friends_of_friends_only nobody only_me list{list_id} {user_id} -list{list_id} -{user_id} 
            /// </summary>
            public string value { get; set; }

            /// <summary>
            /// Друзья и друзья друзей
            /// </summary>
            public string title { get; set; }
        }
    }

    

    public class PrivacySetting
    {
        /// <summary>
        /// profile
        /// </summary>
        public string key { get; set; }

        /// <summary>
        /// My page
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// list, binary
        /// </summary>
        public string type { get; set; }

        public PrivacyTypeClass2 value { get; set; }

        public List<string> supported_categories { get; set; }

        public string section { get; set; }




        /*
        public Visibility PrivateVisibility
        {
            get
            {
                return (this.value.category == "only_me" || this.value.category == "friends_of_friends" || this.value.category == "nobody").ToVisiblity();
            }
        }

        //
        public string UserFriendlyDesc
        {
            get
            {
                if (string.IsNullOrEmpty(this.value.category))
                    return "";

                this.Parse(this.value.category);
                return this.BuildUserFriendlyDesc();
            }
        }

        public List<int> AllowedLists { get; set; }
        public List<int> DeniedUsers { get; set; }
        public List<int> DeniedLists { get; set; }
        public List<int> AllowedUsers { get; set; }
        public PrivacyTypeClass PrivacyType { get; set; }

        private void Parse(string inputString)
        {
            //if (string.IsNullOrEmpty(inputString))
            //    inputString = "all";
            //if (inputStrings.Count == 0)
            //    inputStrings.Add(new PrivacyTypeClass2() { category="all" });
            this.PrivacyType = PrivacyTypeClass.CertainUsers;
            //foreach (var inputString in inputStrings)
            //{
            if (inputString == "all")
                this.PrivacyType = PrivacyTypeClass.AllUsers;
            else if(inputString== "all_except_of_search_engines")
                this.PrivacyType = PrivacyTypeClass.AllUsersExceptSearchEngines;
            else if (inputString == "friends")
                this.PrivacyType = PrivacyTypeClass.Friends;
            else if (inputString == "friends_of_friends")
                this.PrivacyType = PrivacyTypeClass.FriendsOfFriends;
            else if (inputString == "friends_of_friends_only")
                this.PrivacyType = PrivacyTypeClass.FriendsOfFriendsOnly;
            else if (inputString == "only_me")
                this.PrivacyType = PrivacyTypeClass.OnlyMe;
            else if (inputString == "none")
                this.PrivacyType = PrivacyTypeClass.None;
            else
            {
                if (inputString == "nobody")
                    this.PrivacyType = PrivacyTypeClass.Nobody;
                if (inputString.StartsWith("list"))
                    this.AllowedLists.Add(int.Parse(inputString.Substring(4)));
                else if (inputString.StartsWith("-list"))
                    this.DeniedLists.Add(int.Parse(inputString.Substring(5)));
                else if (inputString.StartsWith("-"))
                    this.DeniedUsers.Add(int.Parse(inputString.Substring(1)));
                else
                {
                    int result = 0;
                    if (int.TryParse(inputString, out result))
                        this.AllowedUsers.Add(result);
                }
            }
            //}
        }

        public string FriendlyDescPart;

        private string BuildUserFriendlyDesc()
        {
            string str1 = this.FriendlyDescPart;
            

            string lowerInvariant = str1;
        string str2 = "";
        if (this.AllowedLists!=null)
            str2 = this.AllowedLists.Select((fh => fh.ToString())).ToList().GetCommaSeparated(", ");
        string str3 = "";
        if (this.DeniedUsers!=null)
            str3 = LocalizedStrings.GetString("Privacy_Excluding") + " " + this.DeniedUsers.Select((fr => fr.ToString())).ToList().GetCommaSeparated(", ");
        if (str3 != "")
        {
            if (lowerInvariant != "")
                lowerInvariant += ",";
            if (str2 != "")
                str2 += ",";
        }
        return string.Join(" ", new List<string>() { lowerInvariant, str2, str3 }.Where((s => !string.IsNullOrEmpty(s))));
        }
        */
        /*
        public override string ToString()
        {
            string str = this.value.category;

            List<string> list = new List<string>();//this.AllowedUsers.Select<int, string>((uid => uid.ToString())).Union<string>(this.DeniedUsers.Select<long, string>((duid => "-" + duid.ToString()))).Union<string>(this.AllowedLists.Select<long, string>((alid => "list" + alid))).Union<string>(this.DeniedLists.Select<long, string>((dlid => "-list" + dlid))).ToList<string>();
            if (list.Count > 0 && str != "")
                str += ",";
            return str + list.GetCommaSeparated(",");
        }
        */
        public class PrivacyTypeClass2
        {
            public string category { get; set; }

            /// <summary>
            /// Запрет людям
            /// </summary>
            public PrivacyTypeClassOwners owners { get; set; }

            /// <summary>
            /// Запрет спискам
            /// </summary>
            public PrivacyTypeClassOwners lists { get; set; }

            public bool is_enabled { get; set; }//возвращается только у "профиль закрыт"

            public class PrivacyTypeClassOwners
            {
                public List<int> excluded { get; set; }

                public List<int> allowed { get; set; }
            }
        }

        

        public enum PrivacyTypeClass
        {
            /// <summary>
            /// all — доступно всем пользователям; 
            /// </summary>
            AllUsers,

            /// <summary>
            /// friends — доступно друзьям текущего пользователя; 
            /// </summary>
            Friends,

            /// <summary>
            /// доступно друзьям и друзьям друзей
            /// </summary>
            FriendsOfFriends,

            /// <summary>
            /// доступно только мн
            /// </summary>
            OnlyMe,

            CertainUsers,

            /// <summary>
            /// друзьям друзей текущего пользователя
            /// </summary>
            FriendsOfFriendsOnly,

            /// <summary>
            /// недоступно никому из пользователей
            /// </summary>
            Nobody,

            /// <summary>
            /// недоступно никому из групп
            /// </summary>
            None,

            /// <summary>
            /// Все, кроме происковых сайтов
            /// </summary>
            AllUsersExceptSearchEngines,
        }
    }



    public class PrivacySection
    {
        /// <summary>
        /// profile
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// My page
        /// </summary>
        public string title { get; set; }

        public string description { get; set; }//возвращается только у "профиль закрыт"
        //
        //public List<PrivacySetting> Settings { get; set; }
    }

    public class SettingsAccountInfo
    {
        public AccountInfo Account { get; set; }

        public ProfilesAndGroupsIds NewsBanned { get; set; }

        public ProfileInfoClass ProfileInfo { get; set; }

        public class ProfilesAndGroupsIds
        {
            public List<int> groups { get; set; }

            public List<int> members { get; set; }
        }

        public class AccountInfo
        {
            public string country { get; set; }

            public int https_required { get; set; }

//          public int 2fa_required { get; set; }

            public int own_posts_default { get; set; }

            public int no_wall_replies { get; set; }

            public int intro { get; set; }

            public string lang { get; set; }



            public string support_url { get; set; }

            public string phone { get; set; }

            public string phone_status { get; set; }

            public string change_phone_url { get; set; }

            public string email { get; set; }

            public string email_status { get; set; }

            public string change_email_url { get; set; }

            //public int contacts_sync_status { get; set; }
            public MoneyTransfersSettings money_p2p_params { get; set; }

            public Audio_ads audio_ads { get; set; }

            public int profiler_enabled { get; set; }

            public AccountProfileSettings profiler_settings { get; set; }

            public List<AccountBaseInfoSettingsEntry> settings { get; set; }

            public class AccountBaseInfoSettingsEntry
            {
                //public const string GIF_AUTOPLAY_KEY = "gif_autoplay";
                //public const string PAYMENT_TYPE_KEY = "payment_type";
                //public const string MONEY_P2P_KEY = "money_p2p";
                //public const string MONEY_P2P_GROUPS_KEY = "money_clubs_p2p";

                public string name { get; set; }

                public bool available { get; set; }

                public bool forced { get; set; }

                public string value { get; set; }
            }

            public class AccountProfileSettings
            {
                public bool api_requests { get; set; }
                public List<DownloadPatterns> download_patterns { get; set; }

                public class DownloadPatterns
                {
                    public string type { get; set; }
                    public string pattern { get; set; }
                    public float probability { get; set; }
                    public float error_probability { get; set; }
                }
            }

            public sealed class Audio_ads
            {
                /*audio_ads":{
                * "day_limit":100,
                * "track_limit":1,
                * "types_allowed":["preroll"],
                * "sections":["my","user_playlists","group_playlists","my_playlists","recent","audio_feed","recs","recs_audio","recs_album","search","global_search","group_list","user_list","user_wall","group_wall","feed","other"]},

                * 
                * */
                public int day_limit { get; set; }
                public int track_limit { get; set; }
                public List<string> types_allowed { get; set; }
                public List<string> sections { get; set; }
            }

            public sealed class MoneyTransfersSettings
            {
                //money_p2p_params":{"min_amount":100,
                //"max_amount":75000,
                //"currency":"RUB"},
                public int min_amount { get; set; }

                public int max_amount { get; set; }

                public string currency { get; set; }
            }
        }

        public class ProfileInfoClass
        {
            public string photo_max { get; set; }

            public string first_name { get; set; }

            public string last_name { get; set; }

            public string screen_name { get; set; }

            public int sex { get; set; }

            public int relation { get; set; }

            public VKUser relation_partner { get; set; }

            public int relation_pending { get; set; }

            public List<VKUser> relation_requests { get; set; }

            public string bdate { get; set; }

            public int bdate_visibility { get; set; }

            public VKCity city { get; set; }

            public VKCountry country { get; set; }

            public string home_town { get; set; }

            public NameChangeRequest name_request { get; set; }

            public string phone { get; set; }

            public class NameChangeRequest
            {
                public int id { get; set; }

                public string status { get; set; }

                public string first_name { get; set; }

                public string last_name { get; set; }

                public string lang { get; set; }
            }
        }


    }

    public class ResolvedData
    {
        /// <summary>
        /// user group vk_app
        /// </summary>
        public string type { get; set; }

        public int object_id { get; set; }
    }

    public class LookupContactsResponse
    {
        public List<VKUser> found { get; set; }

        public List<VKUser> other { get; set; }
    }

    public class CheLinkData
    {
        /// <summary>
        ///  статус ссылки. Возможные значения:
        /// not_banned – ссылка не заблокирована;
        /// banned – ссылка заблокирована;
        /// processing – ссылка проверяется, необходимо выполнить повторный запрос через несколько секунд.
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// исходная ссылка (url) либо полная ссылка (если в url была передана сокращенная ссылка). 
        /// </summary>
        public string link { get; set; }
    }

    public class GetProfileInfoResponse
    {
        public VKUser User { get; set; }

        public ProfileInfo ProfileInfo { get; set; }

        public VKUser partner { get; set; }

        public List<VKUser> RelationRequests { get; set; }
    }
}
