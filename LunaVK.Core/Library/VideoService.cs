using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Json;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LunaVK.Core.Library
{
    public class VideoService
    {
        private static VideoService _instance;
        public static VideoService Instance
        {
            get
            {
                if (VideoService._instance == null)
                    VideoService._instance = new VideoService();
                return VideoService._instance;
            }
        }

        public void AddRemovedToFromAlbum(bool add, int targetId, int album_id, int owner_id, uint video_id, Action<VKResponse<int>> calback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["target_id"] = targetId.ToString();
            parameters["album_id"] = album_id.ToString();
            parameters["owner_id"] = owner_id.ToString();
            parameters["video_id"] = video_id.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>(add ? "video.addToAlbum" : "video.removeFromAlbum", parameters, calback);
            /*
            if (add && album_id == VKVideoAlbum.ADDED_ALBUM_ID)
                methodName = "video.add";
            */
        }

        public void Delete(int ownerId, uint vid, Action<VKResponse<int>> calback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["video_id"] = vid.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("video.delete", parameters, calback);
        }

        /*
        public void UploadVideo(Stream stream, bool isPrivate, long albumId, long groupId, string name, string description, Action<VKResponse<SaveVideoResponse, ResultCode>> callback, Action<double> progressCallback = null, Cancellation c = null, PrivacyInfo privacyViewInfo = null, PrivacyInfo privacyCommentInfo = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["is_private"] = isPrivate ? "1" : "0";
            if (groupId != 0L)
                parameters["group_id"] = groupId.ToString();
            if (albumId != 0L)
                parameters["album_id"] = albumId.ToString();
            if (!string.IsNullOrEmpty(name))
                parameters["name"] = name;
            if (!string.IsNullOrEmpty(description))
                parameters["description"] = description;
            if (privacyViewInfo != null && groupId == 0L)
                parameters["privacy_view"] = privacyViewInfo.ToString();
            if (privacyCommentInfo != null && groupId == 0L)
                parameters["privacy_comment"] = privacyCommentInfo.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<SaveVideoResponse>("video.save", parameters, (Action<VKResponse<SaveVideoResponse, ResultCode>>)(res =>
            {
                if (res.ResultCode != ResultCode.Succeeded)
                {
                    callback(new VKResponse<SaveVideoResponse, ResultCode>(res.ResultCode));
                }
                else
                {
                    SaveVideoResponse svr = res.ResultData;
                    JsonWebRequest.Upload(svr.upload_url, stream, "video_file", "video", (Action<JsonResponseData>)(uploadRes =>
                    {
                        if (uploadRes.IsSucceeded)
                            callback(new VKResponse<SaveVideoResponse, ResultCode>(ResultCode.Succeeded, svr));
                        else
                            callback(new VKResponse<SaveVideoResponse, ResultCode>(ResultCode.UnknownError));
                    }), null, progressCallback, c);
                }
            }), null, false, true, new CancellationToken?(), null);
        }*/

        public void EditComment(uint cid, string text, int ownerId, List<string> attachments, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["comment_id"] = cid.ToString();
            parameters["message"] = text;
            parameters["owner_id"] = ownerId.ToString();
            if (!attachments.IsNullOrEmpty())
                parameters["attachments"] = attachments.GetCommaSeparated(",");
            VKRequestsDispatcher.DispatchRequestToVK<int>("video.editComment", parameters, callback);
        }
        

        public class UploadStoryResponseData
        {
            public Resp response { get; set; }

            public string _sig { get; set; }

            public class Resp
            {
                public VKStory story { get; set; }
            }
        }

        public void UploadVideoToHistory(bool add_to_news, int reply_to_story, IReadOnlyList<int> user_ids, string link_text, string link_url, int group_id, byte[] photoData, Action<VKErrors> callback, Action<double> progressCallback = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (user_ids != null)
            {
                parameters["user_ids"] = user_ids.GetCommaSeparated();
            }
            else
            {
                parameters["add_to_news"] = add_to_news ? "1" : "0";
            }

            if (reply_to_story != 0)
                parameters["reply_to_story"] = reply_to_story.ToString();
            //link_text
            //link_url
            VKRequestsDispatcher.DispatchRequestToVK<UploadServerAddress>("stories.getVideoUploadServer", parameters,(result)=>
            {
                if (result.error.error_code != VKErrors.None)
                {
                    callback(result.error.error_code);
                    return;
                }

                string uploadUrl = result.response.upload_url;

                if (!string.IsNullOrEmpty(uploadUrl))
                {
                    JsonWebRequest.Upload(uploadUrl, photoData, "video_file", "video", (jsonString, jsonResult) =>
                    {
                        //UploadStoryResponseData
                        callback(jsonResult == true ? VKErrors.UnknownError : VKErrors.None);
                    }, "video.mp4", progressCallback);
                }
                else
                    callback(VKErrors.UnknownError);
                /*
                 * {"response":{"story":{"id":456239017,"owner_id":460389,"date":1553952488,"can_see":1,"type":"photo","photo":{"id":456240345,"album_id":-81,"owner_id":460389,"sizes":[{"type":"temp","url":"https:\/\/sun9-28.userapi.com\/c841016\/v841016330\/58845\/JQ3cfo8b1i8.jpg","width":50,"height":30},{"type":"s","url":"https:\/\/sun9-13.userapi.com\/c841016\/v841016330\/58846\/dzVnYQ13Vnc.jpg","width":75,"height":45},{"type":"m","url":"https:\/\/sun9-24.userapi.com\/c841016\/v841016330\/58847\/EqT9rWA87kM.jpg","width":130,"height":78},{"type":"j","url":"https:\/\/sun9-17.userapi.com\/c841016\/v841016330\/58848\/TlyZXEPBNqs.jpg","width":256,"height":153},{"type":"x","url":"https:\/\/sun9-9.userapi.com\/c841016\/v841016330\/58849\/VXcAPHvL19s.jpg","width":604,"height":360},{"type":"y","url":"https:\/\/sun9-26.userapi.com\/c841016\/v841016330\/5884a\/H1ZMTMMlXTQ.jpg","width":807,"height":481},{"type":"z","url":"https:\/\/sun9-34.userapi.com\/c841016\/v841016330\/5884b\/rsWjlHumQ_Y.jpg","width":1280,"height":763},{"type":"w","url":"https:\/\/sun9-21.userapi.com\/c841016\/v841016330\/5884c\/oSPDcFAyXS4.jpg","width":2560,"height":1527},{"type":"o","url":"https:\/\/sun9-17.userapi.com\/c841016\/v841016330\/5884d\/73pRo3Sd-SI.jpg","width":130,"height":87},{"type":"p","url":"https:\/\/sun9-10.userapi.com\/c841016\/v841016330\/5884e\/cj8kuxFaNTM.jpg","width":200,"height":133},{"type":"q","url":"https:\/\/sun9-2.userapi.com\/c841016\/v841016330\/5884f\/dWPONv2_UNQ.jpg","width":320,"height":213},{"type":"r","url":"https:\/\/sun9-26.userapi.com\/c841016\/v841016330\/58850\/EWKZLvoNY7o.jpg","width":510,"height":340}],"text":"","date":1553952488},"can_share":1,"can_comment":0,"can_reply":0,"can_hide":1,"views":0,"access_key":"74548676bbc8e58419"}},"_sig":"2657ca97c01a4b7cbfa1d835688d3c0e"}
                 * */
            });

            

        }

        public void GetComments(int ownerId, uint vid, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKComment>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["video_id"] = vid.ToString();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["extended"] = "1";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKComment>>("video.getComments", parameters,(result)=> {
                if(result.error.error_code == VKErrors.None)
                {
                    foreach(var comment in result.response.items)
                    {
                        int owner_Id = comment.owner_id == 0 ? comment.from_id : comment.owner_id;

                        if (owner_Id > 0)
                            comment.User = result.response.profiles.Find((u)=>u.id == owner_Id);
                        else
                            comment.User = result.response.groups.Find((u) => u.id == (-owner_Id));
                    }
                }

                callback(result);
            });
        }

        /// <summary>
        /// Работает только на офф вк
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="vid"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="callback"></param>
        public void GetRecommendations(int ownerId, uint vid, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKVideoBase>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["video_id"] = vid.ToString();
            parameters["offset"] = offset.ToString();
            parameters["count"] = Math.Max(10,count).ToString();
            parameters["extended"] = "1";
            //source: \"{5}\", context: \"{6}
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKVideoBase>>("video.getRecommendations", parameters, (result) => {
                if (result.error.error_code == VKErrors.None)
                {
                    foreach (var video in result.response.items)
                    {
                        if (video.owner_id > 0)
                            video.Owner = result.response.profiles.Find((u) => u.id == video.owner_id);
                        else
                            video.Owner = result.response.groups.Find((u) => u.id == (-video.owner_id));
                    }
                }

                callback(result);
            });
        }
        /*
        public void GetVideoById(int oid, uint vid, string accessKey, Action<VKErrors, VKVideoBase> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["videos"] = oid.ToString() + "_" + vid.ToString();
            parameters["extended"] = "1";//Вместе с владельцем
            if (!string.IsNullOrEmpty(accessKey))
                parameters["access_key"] = accessKey;
            //parameters["width"] = "320";
            parameters["fields"] = "members_count,photo_100";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKVideoBase>>("video.get", parameters, (result)=> {
                if(result.error.error_code == VKErrors.None)
                {
                    if(result.response.items.Count==0)
                    {
                        //Странно, бывает с видео от ютуба
                        callback(result.error, null);
                        return;
                    }

                    var video = result.response.items[0];

                    if (video.owner_id > 0)
                        video.Owner = result.response.profiles.Find((u) => u.id == video.owner_id);
                    else
                        video.Owner = result.response.groups.Find((u) => u.id == (-video.owner_id));

                    callback(result.error,video);
                }
                else
                {
                    callback(result.error, null);
                }
            });
        }
        */
        public void GetVideoAndLikes(int oid, uint vid, string accessKey, Action<VKResponse<VideoLikesCommentsData>> callback)
        {
            string code = "var likesAll = null;var videos = API.video.get({videos:\"" + oid + "_" + vid + "\", extended:1,fields:\"members_count,photo_100\",";
            if (!string.IsNullOrEmpty(accessKey))
                code += ("access_key:\"" + accessKey + "\", ");
            code += "});var video = videos.items[0];";

            code += string.Format("if(video.can_like==1){{ likesAll=API.likes.getList({{item_id:{0}, owner_id:{1}, count:20, extended:1,fields:\"photo_50\", type:\"video\"}}); }}", vid, oid);
            
            code += "return {videos:videos,likes:likesAll};";

            VKRequestsDispatcher.Execute<VideoLikesCommentsData>(code, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    if (result.response.videos.items.Count == 0)
                    {
                        //Странно, бывает с видео от ютуба
                        callback(result);
                        return;
                    }

                    var video = result.response.videos.items[0];

                    if (video.owner_id > 0)
                        video.Owner = result.response.videos.profiles.Find((u) => u.id == video.owner_id);
                    else
                        video.Owner = result.response.videos.groups.Find((u) => u.id == (-video.owner_id));
                }
                callback(result);
            },(jsonStr) => { return VKRequestsDispatcher.FixFalseArray(jsonStr, "likes"); });
        }

        public void Report(int ownerId, uint id, ReportReason reportReason, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["video_id"] = id.ToString();
            parameters["reason"] = ((byte)reportReason).ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("video.report", parameters, callback);
            /*
            comment	комментарий для жалобы.
            search_query	поисковой запрос, если видеозапись была найдена через поиск.
            */
        }

        public void GetAlbums(int userOrGroupId, bool need_system, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKVideoAlbum>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if(userOrGroupId!=0)//Удивительно, но на нуль реагирует
                parameters["owner_id"] = userOrGroupId.ToString();
            if (need_system)
                parameters["need_system"] = "1";
            parameters["extended"] = "1";
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKVideoAlbum>>("video.getAlbums", parameters, callback);
        }
        
        public void GetVideos(int userOrGroupId, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKVideoBase>>> callback, int albumId = 0)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if(userOrGroupId!=0)
                parameters["owner_id"] = userOrGroupId.ToString();
            if (albumId != 0)
                parameters["album_id"] = albumId.ToString();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["extended"] = "1" ;

            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKVideoBase>>("video.get", parameters, (result)=>
            {
                if(result.error.error_code == VKErrors.None)
                {
                    foreach (var item in result.response.items)
                    {
                        if (item.owner_id < 0 && result.response.groups != null)
                            item.Owner = result.response.groups.Find(ow => ow.id == (-item.owner_id));
                        else
                            item.Owner = result.response.profiles.Find(ow => ow.id == item.owner_id);
                    }
                }

                callback(result);
            });
        }

        public void GetRecommendedLiveVideos(Action<VKResponse<VKCountedItemsObject<VKVideoBase>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
           
            parameters["extended"] = "1";

            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKVideoBase>>("video.getRecommendedLiveVideos", parameters, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    foreach (var item in result.response.items)
                    {
                        if (item.owner_id < 0 && result.response.groups != null)
                            item.Owner = result.response.groups.Find(ow => ow.id == (-item.owner_id));
                        else
                            item.Owner = result.response.profiles.Find(ow => ow.id == item.owner_id);
                    }
                }

                callback(result);
            });
        }

        public void AddRemovedToFromAlbum(bool add, int targetId, int album_id, int owner_id, int video_id, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["target_id"] = targetId.ToString();
            parameters["album_id"] = album_id.ToString();
            parameters["owner_id"] = owner_id.ToString();
            parameters["video_id"] = video_id.ToString();
            string methodName = add ? "video.addToAlbum" : "video.removeFromAlbum";
            if (add && album_id == VKVideoAlbum.ADDED_ALBUM_ID)
                methodName = "video.add";
            VKRequestsDispatcher.DispatchRequestToVK<int>(methodName, parameters, callback);
        }

        /// <summary>
        /// Позволяет добавить видеозапись в альбом.
        /// </summary>
        /// <param name="videoId">идентификатор видеозаписи</param>
        /// <param name="ownerId">идентификатор владельца видеозаписи</param>
        /// <param name="targetId">идентификатор владельца альбома, в который нужно добавить видео. по умолчанию идентификатор текущего пользователя</param>
        /// <param name="albumsIds">идентификаторы альбомов, в которые нужно добавить видео</param>
        /// <param name="add"></param>
        /// <param name="callback"></param>
        public void AddRemoveToAlbums(uint videoId, int ownerId, int targetId, List<int> addToAlbumsIds, List<int> removeFromAlbumsIds, Action<VKResponse<ResponseWithId>> callback)
        {
            string format = "API.video.{0}({{video_id:{1},target_id:{2},album_ids:\"{3}\",owner_id:{4}}});";
            string code = "";
            if (!addToAlbumsIds.IsNullOrEmpty())
                code = code + string.Format(format, "addToAlbum", videoId, targetId, addToAlbumsIds.GetCommaSeparated(), ownerId) + Environment.NewLine;
            if (!removeFromAlbumsIds.IsNullOrEmpty())
                code += string.Format(format, "removeFromAlbum", videoId, targetId, removeFromAlbumsIds.GetCommaSeparated(), ownerId);
            VKRequestsDispatcher.Execute<ResponseWithId>(code, callback);
        }

        public void GetAddToAlbumInfo(int targetId, int ownerId, uint videoId, Action<VKResponse<GetAddToAlbumInfoResponse>> callback)
        {
            VKRequestsDispatcher.Execute<GetAddToAlbumInfoResponse>(string.Format("var target_id = {0}; var owner_id = {1}; var video_id = {2}; var groups = API.groups.get({{extended:1, filter:\"moder\", fields:\"can_upload_video\"}}); var albums = API.video.getAlbums({{need_system:1, extended:1, owner_id:target_id}}); var albumsByVideo = API.video.getAlbumsByVideo({{target_id:target_id, owner_id:owner_id, video_id:video_id}}); return {{ AlbumsByVideo:albumsByVideo, Albums:albums, Groups:groups}};", targetId, ownerId, videoId), callback);
        }
        /*
         * public a(Catalog paramCatalog, String paramString1, String paramString2, boolean paramBoolean1, boolean paramBoolean2)
  {
    super("video.getCatalog");
    this.a = paramCatalog;
    if (paramString1 != null) {
      c("section_id", paramString1);
    }
    if (paramString2 != null) {
      c("ref", paramString2);
    }
    b("need_blocks", k.a(paramBoolean1));
    b("extended", k.a(paramBoolean2));
    c("fields", "photo_50,photo_100,members_count,followers_count,career,city,country,education,friend_status,activity,verified,trending,likes");
  }
  */
        public void GetVideoCatalog(int count, int itemsCount, string from, Action<VKResponse<Catalogs>> callback)
        {
            /*
            id блоков, которые необходимо вернуть в ответе.
            Может содержать значения:

            feed — видео из ленты новостей пользователя;
            ugc — популярное;
            top — выбор редакции;
            series — сериалы и телешоу;
            other — прочие блоки.

            по умолчанию feed,ugc,series,other, список слов, разделенных через запятую
            */
            if (count >= 16)
                count = 15;

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["extended"] = "1";// Если был передан параметр extended = 1, возвращаются дополнительные объекты profiles и groups, 
            parameters["count"] = count.ToString();
            parameters["items_count"] = itemsCount.ToString();//по 4 видео в каталоге, но не больше 16
            if (!string.IsNullOrEmpty(from))
                parameters["from"] = from;
            parameters["fields"] = "members_count,is_member,verified,trending,likes";
            parameters["filters"] = "other,top,series,ugc,live";


            VKRequestsDispatcher.DispatchRequestToVK<Catalogs>("video.getCatalog", parameters, (result)=> {
                if (result.error.error_code == VKErrors.None)
                {
                    foreach (VideoCatalogCategory catalog in result.response.items)
                    {
                        if(catalog.type == "channel")
                        {
                            if (catalog.OwnerId < 0 && result.response.groups != null)
                            {
                                var owner = result.response.groups.Find(ow => ow.id == (-catalog.OwnerId));
                                catalog.groups = new List<VKGroup>() { owner };
                            }
                            else
                            {
                                var owner = result.response.profiles.Find(ow => ow.id == catalog.OwnerId);
                                catalog.profiles = new List<VKUser>() { owner };
                            }
                        }

                        foreach (var c in catalog.items)
                        {
                            if (c.type == "album")
                            {
                                int i = 0;
                            }

                            if (c.owner_id < 0 && result.response.groups != null)
                                c.Owner = result.response.groups.Find(ow => ow.id == (-c.owner_id));
                            else
                                c.Owner = result.response.profiles.Find(ow => ow.id == c.owner_id);
                        }
                    }
                }
                callback(result);
            },(json)=>
            {
                return json;
            }
            );
        }
        
        public void GetVideoCatalogSection(string categoryId, string fromStr, Action<VKResponse<VideoCatalogCategory>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["extended"] = "1";
            parameters["section_id"] = categoryId;
            parameters["from"] = fromStr ?? "";
            parameters["count"] = "16";
            parameters["fields"] = "members_count";

            VKRequestsDispatcher.DispatchRequestToVK<VideoCatalogCategory>("video.getCatalogSection", parameters, (result)=> {
                if(result.error.error_code == VKErrors.None)
                {
                    foreach (var video in result.response.items)
                    {
                        if (video.owner_id < 0 && result.response.groups != null)
                            video.Owner = result.response.groups.Find(ow => ow.id == (-video.owner_id));
                        else
                            video.Owner = result.response.profiles.Find(ow => ow.id == video.owner_id);
                    }
                }
                callback(result);
            });
        }

        public void UploadVideo(byte[] data, bool isPrivate, int albumId, int groupId, string name, string description, Action<UploadVideoResponseData, VKErrors> callback, Action<double> progressCallback = null, CancellationToken? cancellationToken = null, PrivacyInfo privacyViewInfo = null, PrivacyInfo privacyCommentInfo = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["is_private"] = isPrivate ? "1" : "0";
            if (groupId != 0)
                parameters["group_id"] = groupId.ToString();//идентификатор сообщества, в которое будет сохранен видеофайл
            if (albumId != 0)
                parameters["album_id"] = albumId.ToString();//идентификатор альбома, в который будет загружен видео файл.
            if (!string.IsNullOrEmpty(name))
                parameters["name"] = name;
            if (!string.IsNullOrEmpty(description))
                parameters["description"] = description;
            if (privacyViewInfo != null && groupId == 0)
                parameters["privacy_view"] = privacyViewInfo.ToString();
            if (privacyCommentInfo != null && groupId == 0)
                parameters["privacy_comment"] = privacyCommentInfo.ToString();

            VKRequestsDispatcher.DispatchRequestToVK<SaveVideoResponse>("video.save", parameters, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    SaveVideoResponse svr = result.response;
                    JsonWebRequest.Upload(svr.upload_url, data, "video_file", "video", (JsonString, jsonResult) =>
                    {
                        //UploadVideoResponseData
                        //callback(uploadRes, VKErrors.None);
                        if(jsonResult)
                        {
                            UploadVideoResponseData uploadData = JsonConvert.DeserializeObject<UploadVideoResponseData>(JsonString);
                            callback(uploadData, VKErrors.None);
                        }
                        else
                        {
                            callback(null, VKErrors.UnknownError);
                        }
                    }, null, progressCallback, cancellationToken);
                }
                else
                {
                    callback(null, result.error.error_code);
                }
            });

        }

        public void Search(Dictionary<string, object> searchParams, string query, int offset, int count, Action<VKResponse< VKCountedItemsObject<VKVideoBase>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["q"] = query;
            parameters["extended"] = "1";
            if(offset>0)
                parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();

            if (searchParams.ContainsKey("sort"))
                parameters["sort"] = ((int)searchParams["sort"]).ToString();

            if (searchParams.ContainsKey("hd"))
                parameters["hd"] = "1";

            if (searchParams.ContainsKey("adult"))
                parameters["adult"] = "1";

            List<string> filters = new List<string>();
            if (searchParams.ContainsKey("is_mp4"))
                filters.Add("mp4");
            if (searchParams.ContainsKey("is_youtube"))
                filters.Add("youtube");
            if (searchParams.ContainsKey("is_vimeo"))
                filters.Add("vimeo");
            if (searchParams.ContainsKey("is_short"))
                filters.Add("short");
            if (searchParams.ContainsKey("is_long"))
                filters.Add("long");

            if (filters.Count>0)
                parameters["filters"] = string.Join(",", filters);

            if (searchParams.ContainsKey("search_own"))
                parameters["search_own"] = "1";

            VKRequestsDispatcher.DispatchRequestToVK< VKCountedItemsObject<VKVideoBase>> ("video.search", parameters, (result)=>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    foreach (var video in result.response.items)
                    {
                        if (video.owner_id < 0 && result.response.groups != null)
                            video.Owner = result.response.groups.Find(ow => ow.id == (-video.owner_id));
                        else
                            video.Owner = result.response.profiles.Find(ow => ow.id == video.owner_id);
                    }
                }

                callback(result);
            });
        }

        /// <summary>
        /// Создает пустой альбом видеозаписей.
        /// </summary>
        /// <param name="albumName">название альбома.</param>
        /// <param name="albumPrivacy"></param>
        /// <param name="callback">ИД альбома</param>
        /// <param name="groupId"></param>
        public void AddAlbum(string albumName, string albumPrivacy, Action<VKResponse<int>> callback, uint? groupId = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["title"] = albumName;
            if (groupId.HasValue && groupId.Value != 0)
                parameters["group_id"] = groupId.Value.ToString();
            if (!string.IsNullOrEmpty(albumPrivacy))
                parameters["privacy"] = albumPrivacy;
            VKRequestsDispatcher.DispatchRequestToVK<AddVideoAlbumResponse>("video.addAlbum", parameters, (result)=> {
                if (result.error.error_code == VKErrors.None)
                    callback(new VKResponse<int>() { error = result.error, response = result.response.album_id });
                else
                    callback(new VKResponse<int>() { error = result.error });
            });
        }

        public void EditAlbum(string albumName, int albumId, string albumPrivacy, Action<VKResponse<int>> callback, uint? groupId = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["title"] = albumName;
            parameters["album_id"] = albumId.ToString();
            if (groupId.HasValue && groupId.Value != 0)
                parameters["group_id"] = groupId.Value.ToString();
            if (!string.IsNullOrEmpty(albumPrivacy))
                parameters["privacy"] = albumPrivacy.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("video.editAlbum", parameters, callback);
        }

        public void DeleteAlbum(int albumId, Action<VKResponse<int>> callback, uint? groupId = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["album_id"] = albumId.ToString();
            if (groupId.HasValue && groupId.Value > 0)
                parameters["group_id"] = groupId.Value.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("video.deleteAlbum", parameters, callback);
        }
        /* никогда не используется
        public void MoveVideoToAlbum(List<uint> videoIds, int albumId, Action<VKResponse<object>> callback, uint? groupId = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["vids"] = videoIds.GetCommaSeparated();
            parameters["album_id"] = albumId.ToString();
            if (groupId.HasValue)
                parameters["group_id"] = groupId.Value.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<object>("video.moveToAlbum", parameters, callback, null);
        }
        */

        public void GetVideoById(int oid, uint vid, string accessKey, Action<VKResponse<List<VKVideoBase>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["videos"] = oid.ToString() + "_" + vid.ToString();
            parameters["extended"] = "1";
            if (!string.IsNullOrEmpty(accessKey))
                parameters["access_key"] = accessKey;
            parameters["width"] = "320";
            VKRequestsDispatcher.DispatchRequestToVK<List<VKVideoBase>>("video.get", parameters, callback);
        }

        public void EditVideo(uint videoId, int ownerId, string name, string description, PrivacyInfo privacyView, PrivacyInfo privacyComment, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (ownerId != 0)
                parameters["owner_id"] = ownerId.ToString();
            parameters["video_id"] = videoId.ToString();
            parameters["name"] = name;
            parameters["desc"] = description;
            parameters["privacy_view"] = privacyView.ToString();
            parameters["privacy_comment"] = privacyComment.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("video.edit", parameters, callback);
        }




        private class AddVideoAlbumResponse
        {
            public int album_id { get; set; }
        }

        public class SaveVideoResponse
        {
            public string upload_url { get; set; }

            public int video_id { get; set; }

            public string title { get; set; }

            public string description { get; set; }

            public int owner_id { get; set; }

            public string access_key { get; set; }
        }

        public class Catalogs
        {
            public List<VideoCatalogCategory> items { get; set; }

            public List<VKUser> profiles { get; set; }

            public List<VKGroup> groups { get; set; }

            public string next { get; set; }
        }

        /// <summary>
        /// https://vk.com/dev/objects/video_cat_block
        /// </summary>
        public class VideoCatalogCategory
        {
            /// <summary>
            /// идентификатор блока. 
            /// если минус, то от группы
            /// если плюс, то блок является категорией
            /// если текст, то блок из фильтра
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// предпочтительный способ отображения контента.
            /// horizontal — горизонтально с дополнительной информацией; 
            /// horizontal_compact — горизонтально без дополнительной информации; 
            /// vertical — вертикально с дополнительной информацией; 
            /// vertical_compact — вертикально без дополнительной информации. 
            /// </summary>
            public string view { get; set; }

            /// <summary>
            /// параметр для получения следующей страницы результатов. 
            /// </summary>
            public string next { get; set; }

            /// <summary>
            /// название блока. 
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// наличие возможности скрыть блок. 
            /// </summary>
            public int can_hide { get; set; }

            /// <summary>
            /// тип блока. Может содержать одно из значений: 
            /// channel — видеозаписи сообщества, тогда ИД это -группа; 
            /// category — подборки видеозаписей.
            /// если тип пустой, то ориентируемся на ид, он как текст
            /// </summary>
            public string type { get; set; }

            /// <summary>
            /// массив объектов - элементов видеокаталога. 
            /// </summary>
            public ObservableCollection<VideoCatalogItem> items { get; set; }


            public List<VKUser> profiles { get; set; }

            public List<VKGroup> groups { get; set; }


            public string icon { get; set; }

            public string icon_2x { get; set; }

#region VM
            public VideoCatalogCategory()
            {
                //this.items = new ObservableCollection<VideoCatalogItem>();
            }

//            public string uc_icon { get; set; }

            public void LoadMore()
            {
                VideoService.Instance.GetVideoCatalogSection(this.id, this.next, (result) => {
                    if(result.error.error_code == VKErrors.None)
                    {
                        this.next = result.response.next;
                        Execute.ExecuteOnUIThread(() => { 
                            foreach(var item in result.response.items)
                            {
                                this.items.Add(item);
                            }
                        });
                    }
                });
            }

            public string IconUri
            {
                get
                {
                    if (!string.IsNullOrEmpty(this.icon_2x))
                        return this.icon;

                    if (this.id == "ugc")
                    {
                        return "..\\Resources\\VideoCatalog\\VideosPopular.png";
                    }
                    else if (this.id == "my")
                    {
                        return "..\\Resources\\VideoCatalog\\VideosMy.png";
                    }
                    else if (this.id == "series")
                    {
                        return "..\\Resources\\VideoCatalog\\VideosShows.png";
                    }
                    else if (this.id == "feed")
                    {
                        //if (!this.AllowFeedNavigateToAll)
                        //{
                        //    listHeaderViewModel.ShowAllVisibility = Visibility.Collapsed;
                        //    listHeaderViewModel.OnHeaderTap = null;
                        //}
                        return "..\\Resources\\VideoCatalog\\VideosUpdates.png";
                    }
                    else if (this.type == "channel")
                    {
                        return this.GetImageUriFor(this.OwnerId);
                    }
                    else
                    {
                        return string.Format("..\\Resources\\VideoCatalog\\Categories\\VideosCat{0}.png", this.id);
                    }
                }
            }

            public int OwnerId
            {
                get
                {
                    int result = 0;
                    int.TryParse(this.id, out result);
                    return result;
                }
            }

            private string GetImageUriFor(int owner_id)
            {
                if (owner_id < 0)
                {
                    VKGroup group = this.groups.FirstOrDefault((g => g.id == -owner_id));
                    if (group != null)
                        return group.photo_200;
                }
                else
                {
                    VKUser user = this.profiles.FirstOrDefault((u => u.id == owner_id));
                    if (user != null)
                        return user.photo_max;
                }
                return "";
            }

            #endregion


            /// <summary>
            /// Элемент видеокаталога
            /// https://vk.com/dev/objects/video_cat_element
            /// </summary>
            public class VideoCatalogItem
            {
                /// <summary>
                /// идентификатор элемента.
                /// Может быть и отрицательным
                /// </summary>
                public int id { get; set; }

                /// <summary>
                /// идентификатор владельца элемента. 
                /// </summary>
                public int owner_id { get; set; }

                /// <summary>
                /// название элемента. 
                /// </summary>
                public string title { get; set; }

                /// <summary>
                /// тип элемента. Может содержать одно из значений:
                /// video — видеоролик;
                /// album — альбом. 
                /// </summary>
                public string type { get; set; }


#region Дополнительные поля для элемента-видеоролика (type="video")

                /// <summary>
                /// текст описания. 
                /// </summary>
                public string description { get; set; }

                /// <summary>
                /// длительность ролика в секундах. 
                /// </summary>
                public int duration { get; set; }

                /*
                /// <summary>
                /// URL изображения-обложки ролика шириной 130px. 
                /// </summary>
                [Obsolete]
                public string photo_130 { get; set; }

                [Obsolete]
                public string photo_320 { get; set; }

                /// <summary>
                /// URL изображения-обложки ролика шириной 640px (если размер есть). 
                /// </summary>
                [Obsolete]
                public string photo_640 { get; set; }

                /// <summary>
                /// URL изображения-обложки ролика шириной 800px (если размер есть). 
                /// </summary>
                [Obsolete]
                public string photo_800 { get; set; }
                */

                /// <summary>
                /// дата создания видеозаписи в формате Unixtime. 
                /// </summary>
                [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
                public DateTime date { get; set; }

                /// <summary>
                /// дата добавления видеозаписи пользователем или группой в формате Unixtime.
                /// </summary>
                [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
                public DateTime adding_date { get; set; }

                /// <summary>
                /// количество просмотров видеозаписи. 
                /// </summary>
                public int views { get; set; }

                /// <summary>
                /// количество комментариев к видеозаписи. 
                /// </summary>
                public int comments { get; set; }

                /// <summary>
                /// наличие возможности добавить ролик в свой список. 
                /// </summary>
                [JsonConverter(typeof(VKBooleanConverter))]
                public bool can_add { get; set; }

                /// <summary>
                /// наличие возможности редактировать видео. 
                /// </summary>
                [JsonConverter(typeof(VKBooleanConverter))]
                public bool can_edit { get; set; }

                /// <summary>
                /// приватность ролика (0 — нет, 1 — есть). 
                /// </summary>
                [JsonConverter(typeof(VKBooleanConverter))]
                public bool is_private { get; set; }

                
                //----
                /// <summary>
                /// количество просмотров видеозаписи. 
                /// </summary>
                public int local_views { get; set; }

                public string access_key { get; set; }

                /// <summary>
                /// YouTube
                /// </summary>
                public string platform { get; set; }

                public List<VKImageWithSize> image { get; set; }
                //----

#region VM

                public VKBaseDataForGroupOrUser Owner;

                [JsonConverter(typeof(VKBooleanConverter))]
                public bool watched { get; set; }

                [JsonConverter(typeof(VKBooleanConverter))]
                public bool live { get; set; }

                public Visibility AlreadyViewedVisibility
                {
                    get { return this.watched == false ? Visibility.Collapsed : Visibility.Visible; }
                }

                public string Title
                {
                    get { return this.title; }
                }

                public string Subtitle1
                {
                    get { return this.Owner.Title; }
                }

                public string Subtitle2
                {
                    get { return UIStringFormatterHelper.FormatNumberOfSomething(this.views, "OneViewFrm", "TwoFourViewsFrm", "FiveViewsFrm"); }
                }

                public string ImageUri
                {
                    get
                    {
                        if (this.image == null)
                            return "";

                        if(this.image.Count>1)
                            return this.image[1].url;
                        return this.image[0].url;
                        //return this.photo_320;
                    }
                }

                public string UIDuration
                {
                    get
                    {
                        if (this.live == true)
                            return LocalizedStrings.GetString("VideoCatalog_LIVE");
                        //if (this.duration <= 0)
                        //    return "";
                        return UIStringFormatterHelper.FormatDuration(this.duration);
                    }
                }

                public Visibility IsVideoVisibility
                {
                    get { return Visibility.Visible; }
                }

                public Visibility PrivacyVisibility
                {
                    get { return Visibility.Collapsed; }
                }
                #endregion

#endregion


#region Дополнительные поля для элемента-видеоальбома (type="album")

                /// <summary>
                /// число видеозаписей в альбоме. 
                /// </summary>
                public int count { get; set; }

                /*
                /// <summary>
                /// URL изображения-обложки альбома с размером 272x150px. 
                /// </summary>
                [Obsolete]
                public string photo_160 { get; set; }
                */

                /// <summary>
                /// время последнего обновления альбома в формате unixtime. 
                /// </summary>
                public int updated_time { get; set; }


                public int is_system { get; set; }//no in documentation
#endregion










                //      public int live { get; set; }

                //        public int watched { get; set; }


                //public string owner_name { get; set; }

            }
        }

        public class VideoLikesCommentsData
        {
            public VKCountedItemsObject<VKVideoBase> videos { get; set; }

            public VKCountedItemsObject<VKUser> likes { get; set; }
        }

        public class GetAddToAlbumInfoResponse
        {
            public VKCountedItemsObject<VKGroup> Groups { get; set; }

            public List<int> AlbumsByVideo { get; set; }

            public VKCountedItemsObject<VKVideoAlbum> Albums { get; set; }

            public GetAddToAlbumInfoResponse()
            {
                this.Groups = new VKCountedItemsObject<VKGroup>();
                this.AlbumsByVideo = new List<int>();
                this.Albums = new VKCountedItemsObject<VKVideoAlbum>();
            }
        }
    }
}
