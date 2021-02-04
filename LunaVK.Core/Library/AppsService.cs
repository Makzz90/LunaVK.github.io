using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace LunaVK.Core.Library
{
    public class AppsService
    {
        private static AppsService _instance;
        public static AppsService Instance
        {
            get
            {
                return AppsService._instance ?? (AppsService._instance = new AppsService());
            }
        }

        public void GetCatalog(int offset, int count, Action<VKResponse<VKCountedItemsObject<VKGame>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            //parameters["platform"] = "android";//"winphone";
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            /*
            //dictionary["filter"] = "installed";
            if (paramInt1 > 0) {
      a("section_id", paramInt1);
      a("return_friends", 1);// возвращать список друзей, установивших это приложение
            */
            //parameters["sort"] = "popular_week";
            parameters["platform"] = "web";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKGame>>("apps.getCatalog", parameters, callback);
        }

        public void GetMyGames(int offset, int count, Action<VKResponse<VKCountedItemsObject<VKGame>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["platform"] = "winphone";
            parameters["filter"] = "installed";
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKGame>>("apps.getCatalog", parameters, callback);
        }
        /*
        public void GetActivity(long gameId, int count, string start_from, Action<VKResponse<GamesFriendsActivityResponse> callback)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary["platform"] = "winphone";
            dictionary["fields"] = "photo_max,sex";
            dictionary["count"] = count.ToString();
            if (gameId > 0L)
                dictionary["filter_app_id"] = gameId.ToString();
            if (!string.IsNullOrEmpty(start_from))
                dictionary["start_from"] = start_from;
            string methodName = "apps.getActivity";
            Dictionary<string, string> parameters = dictionary;
            int num1 = 0;
            int num2 = 1;
            CancellationToken? cancellationToken = new CancellationToken?();
            // ISSUE: variable of the null type

            VKRequestsDispatcher.DispatchRequestToVK<GamesFriendsActivityResponse>(methodName, parameters, callback, (Func<string, GamesFriendsActivityResponse>)(jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<GamesFriendsActivityResponse>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
        }

        public void GetInviteRequests(int offset, int count, Action<VKResponse<GamesRequestsResponse> callback)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary["platform"] = "winphone";
            dictionary["filter_type"] = "invite";
            dictionary["fields"] = "photo_max,sex";
            dictionary["count"] = count.ToString();
            dictionary["offset"] = offset.ToString();
            string methodName = "apps.getRequests";
            Dictionary<string, string> parameters = dictionary;
            Action<VKResponse<GamesRequestsResponse> callback1 = callback;
            int num1 = 0;
            int num2 = 1;
            CancellationToken? cancellationToken = new CancellationToken?();
            // ISSUE: variable of the null type

            VKRequestsDispatcher.DispatchRequestToVK<GamesRequestsResponse>(methodName, parameters, callback1, (Func<string, GamesRequestsResponse>)(jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<GamesRequestsResponse>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
        }

        public void DeleteRequest(long requestId, Action<VKResponse<OwnCounters> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<OwnCounters>("execute", new Dictionary<string, string>()
      {
        {
          "code",
          string.Format("var requestId = {0};\r\n                                           API.apps.deleteRequest({{\"request_ids\": requestId}});\r\n                                           return API.getCounters();", requestId)
        }
      }, callback, new Func<string, OwnCounters>(CountersDeserializerHelper.Deserialize), false, true, new CancellationToken?(), null);
        }
        */
        public void GetDashboard(int offset, int count, Action<VKResponse<GamesDashboardResponse>> callback)
        {
            string str = string.Format("var platform = \"{0}\";\r\n                                            var fields = \"{1}\";\r\n                                            var count = {2};\r\n                                            var offset = {3};\r\n\r\n                                            var requests = {{}};\r\n                                            var myGames = {{}};\r\n                                            var activity = {{}};\r\n                                            var banners = {{}};\r\n\r\n                                            if (offset > 0) {{\r\n                                            \r\n                                            requests = null;\r\n                                            myGames = null;\r\n                                            activity = null;\r\n                                            banners = null;\r\n\r\n                                            }} else {{\r\n\r\n                                            requests = API.apps.getRequests({{\"platform\": platform, \"fields\": fields}});\r\n                                            myGames = API.apps.getCatalog({{\"platform\": platform, \"count\": 12, \"filter\": \"installed\"}});\r\n                                            activity = API.apps.getActivity({{\"platform\": platform, \"count\": 4, \"fields\": fields}});\r\n                                            banners = API.apps.getCatalog({{\"platform\": platform, \"filter\": \"featured\"}});\r\n\r\n                                            }}\r\n\r\n                                            var catalog = API.apps.getCatalog({{\"platform\": platform, \"count\": count, \"offset\": offset}});\r\n                                            return {{\"requests\": requests, \"myGames\": myGames, \"activity\": activity, \"banners\": banners, \"catalog\": catalog}};", "winphone", "photo_max,sex", count, offset);
            
            VKRequestsDispatcher.Execute<GamesDashboardResponse>(str, callback,(jsonStr) => {
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "requests", true);
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "activity", true);
                return jsonStr;
            });
        }

        public void GetEmbeddedUrl(uint appId, int ownerId, Action<VKResponse<EmbeddedUrlResponse>> callback)
        {
            //не доступа для сторонних приложений
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["app_id"] = appId.ToString();
            if (ownerId != 0)
                parameters["owner_id"] = ownerId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<EmbeddedUrlResponse>("apps.getEmbeddedUrl", parameters, callback);
        }

        public class EmbeddedUrlResponse
        {
            public string original_url { get; set; }

            public string view_url { get; set; }

            public string screen_title { get; set; }
        }

        public class GamesDashboardResponse
        {
            public GamesRequestsResponse requests { get; set; }

            public VKCountedItemsObject<VKGame> myGames { get; set; }

            public GamesFriendsActivityResponse activity { get; set; }

            public VKCountedItemsObject<VKGame> banners { get; set; }

            public VKCountedItemsObject<VKGame> catalog { get; set; }
        }

        public class GamesRequestsResponse
        {
            public int count { get; set; }

            public List<GameRequest> items { get; set; }

            public List<VKGame> apps { get; set; }

            public List<VKUser> profiles { get; set; }
        }

        public class GamesFriendsActivityResponse
        {
            public int count { get; set; }

            public List<GameActivity> items { get; set; }

            public List<VKGame> apps { get; set; }

            public List<VKUser> profiles { get; set; }

            public string next_from { get; set; }
        }

        public class GameActivity
        {
            public string type { get; set; }

            public long app_id { get; set; }

            public long user_id { get; set; }

            public int level { get; set; }

            public string activity { get; set; }

            public int score { get; set; }

            public int date { get; set; }

            public string text { get; set; }
        }

        public class GameRequest
        {
            public long id { get; set; }

            public string type { get; set; }

            public long app_id { get; set; }

            public string text { get; set; }

            public long date { get; set; }

            public long from_id { get; set; }

            public List<GameRequestFrom> from { get; set; }

            public string key { get; set; }

            public string name { get; set; }

            public int unread { get; set; }
        }

        public class GameRequestFrom
        {
            public long id { get; set; }

            public long from_id { get; set; }

            public long date { get; set; }

            public string key { get; set; }
        }
        /*
        public void GetGameDashboard(long appId, Action<VKResponse<GameDashboardResponse> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["fields"] = "photo_max,sex";
            parameters["appId"] = appId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<GameDashboardResponse>("execute.getGameDashboard", parameters, callback, (Func<string, GameDashboardResponse>)(jsonStr =>
            {
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "requests", true);
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "activity", true);
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "leaderboard", false);
                return JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<GameDashboardResponse>>(jsonStr).response;
            }), false, true, new CancellationToken?(), null);
        }
        */
        public void GetApp(int id, Action<VKResponse<VKCountedItemsObject<VKGame>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["app_id"] = id.ToString();
            parameters["extended"] = "1";
            parameters["platform"] = "winphone";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKGame>>("apps.get", parameters, callback);
        }
        /*
        public void ToggleRequests(long id, bool isEnabled, Action<VKResponse<int> callback)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary["id"] = id.ToString();
            dictionary["enabled"] = isEnabled ? "1" : "0";
            string methodName = "apps.toggleRequests";
            Dictionary<string, string> parameters = dictionary;
            Action<VKResponse<int> callback1 = callback;
            int num1 = 0;
            int num2 = 1;
            CancellationToken? cancellationToken = new CancellationToken?();
            // ISSUE: variable of the null type

            VKRequestsDispatcher.DispatchRequestToVK<int>(methodName, parameters, callback1, (Func<string, int>)(jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<int>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
        }

        public void MarkRequestAsRead(IEnumerable<long> requestIds, Action<VKResponse<OwnCounters> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<OwnCounters>("execute", new Dictionary<string, string>()
      {
        {
          "code",
          string.Format("API.apps.markRequestAsRead({{\"request_ids\": \"{0}\"}});\r\n                                           return API.getCounters();", string.Join<long>(",", requestIds))
        }
      }, callback, new Func<string, OwnCounters>(CountersDeserializerHelper.Deserialize), false, true, new CancellationToken?(), null);
        }

        public void Remove(long id, Action<VKResponse<OwnCounters> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<OwnCounters>("execute", new Dictionary<string, string>()
      {
        {
          "code",
          string.Format("var id = {0};\r\n                                           API.apps.remove({{\"id\": id}});\r\n                                           return API.getCounters();", id)
        }
      }, callback, new Func<string, OwnCounters>(CountersDeserializerHelper.Deserialize), false, true, new CancellationToken?(), null);
        }

        public void GetEmbeddedUrl(long appId, long ownerId, Action<VKResponse<EmbeddedUrlResponse> callback)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string index = "app_id";
            string str = appId.ToString();
            dictionary[index] = str;
            Dictionary<string, string> parameters = dictionary;
            if (ownerId != 0L)
                parameters["owner_id"] = ownerId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<EmbeddedUrlResponse>("apps.getEmbeddedUrl", parameters, callback);
        }
       
        public void Report(long appId, ReportAppReason reason, long ownerId = 0, string comment = "", Action<VKResponse<int> callback = null)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string index1 = "app_id";
            string str1 = appId.ToString();
            dictionary[index1] = str1;
            string index2 = "reason";
            string str2 = ((int)reason).ToString();
            dictionary[index2] = str2;
            Dictionary<string, string> parameters = dictionary;
            if (ownerId != 0L)
                parameters["owner_id"] = ownerId.ToString();
            if (!string.IsNullOrWhiteSpace(comment))
                parameters["comment"] = comment;
            VKRequestsDispatcher.DispatchRequestToVK<int>("apps.report", parameters, callback);
        }
         */
        public void SendLog(string log, Action<bool> callback = null)
        {
            if (string.IsNullOrEmpty(log))
                return;

            string str1 = "";
            try
            {
                Encoding encoding = Encoding.UTF8;
                
                byte[] bytes = encoding.GetBytes(log);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                    {
                        gzipStream.Write(bytes, 0, bytes.Length);
                        str1 = Convert.ToBase64String(memoryStream.ToArray());
                    }
                }


                //str1 = Convert.ToBase64String(ServiceLocator.Resolve<IGZipEncoder>().Compress(log, utF8));
                
                Task.Run(async () =>
                {
                    using (HttpClient client = new HttpClient())
                    {
                        
                        Dictionary<string, string> postParams = new Dictionary<string, string>();
                        postParams["user_id"] = Settings.UserId.ToString();
                        postParams["data"] = log;
                        var response = await client.PostAsync(new Uri("http://www.xn-----nlcaiebdb9andydgfuq5v.xn--p1ai/log_upload.php"), new HttpFormUrlEncodedContent(postParams));
                        var content = await response.Content.ReadAsStringAsync();
                        callback?.Invoke(true);
                    }
                });
            }
            catch
            {
                callback?.Invoke(false);
            }
            /*
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["log"] = str1;
            VKRequestsDispatcher.DispatchRequestToVK<int>("apps.sendLog", parameters, callback);
            */
            
        }
    }
}