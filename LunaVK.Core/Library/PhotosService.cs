using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LunaVK.Core.Library
{
    public class PhotosService
    {
        private static PhotosService _instance;
        public static PhotosService Instance
        {
            get
            {
                if (PhotosService._instance == null)
                    PhotosService._instance = new PhotosService();
                return PhotosService._instance;
            }
        }

        public void GetAllPhotos(int userOrGroupId, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKPhoto>>> callback)
        {
            this.GetPhotosImpl("photos.getAll", userOrGroupId, offset, count, callback);
        }

        public void GetUserPhotos(int userId, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKPhoto>>> callback)
        {
            this.GetPhotosImpl("photos.getUserPhotos", userId, offset, count, callback);
        }

        public void GetWallPhotos(int userOrGroupId, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKPhoto>>> callback)
        {
            this.GetAlbumPhotosImpl(userOrGroupId, "wall", offset, count, callback);
        }

        public void GetSavedPhotos(int userOrGroupId, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKPhoto>>> callback)
        {
            this.GetAlbumPhotosImpl(userOrGroupId, "saved", offset, count, callback);
        }

        public void GetProfilePhotos(int userId, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKPhoto>>> callback)
        {
            this.GetAlbumPhotosImpl(userId, "profile", offset, count, callback);
        }

        public void GetAlbumPhotos(int userOrGroupId, string albumId, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKPhoto>>> callback)
        {
            this.GetAlbumPhotosImpl(userOrGroupId, albumId.ToString(), offset, count, callback);
        }

        private void GetAlbumPhotosImpl(int userOrGroupId, string albumId, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKPhoto>>> callback)
        {
            /*
            long result;
            if (long.TryParse(albumId, out result))
            {
                var temp = await RequestsDispatcher.Execute<PhotosListWithCount>(string.Format("var photos = API.photos.get({{\"owner_id\":{0}, \"album_id\":{1}, \"offset\":{2}, \"count\":{3}, \"extended\":1, \"rev\":1}});                   var albums  = API.photos.getAlbums({{\"owner_id\":{0}, \"album_ids\":{1}}});                   var thumbId = albums.items@.thumb_id[0];                   var ownerPlusThumb = {0} + \"_\" + thumbId;                   var p= API.photos.getById({{\"photos\":ownerPlusThumb}});                   return {{\"Album\": albums.items[0], \"Photos\":photos, \"Thumb\": p[0]}};", parameters["owner_id"], result, offset, count), callback, (Func<string, PhotosListWithCount>)(jsonStr =>
                {
                    PhotosService.AlbumAndPhotosData response = JsonConvert.DeserializeObject<GenericRoot<PhotosService.AlbumAndPhotosData>>(jsonStr).response;
                    PhotosListWithCount photosListWithCount = new PhotosListWithCount()
                    {
                        album = response.Album,
                        photosCount = response.Photos.count,
                        response = response.Photos.items
                    };
                    if (photosListWithCount.album != null && response.Thumb != null)
                        this.UpdateThumbSrc(new List<Album>()
            {
              photosListWithCount.album
            }, new List<Photo>() { response.Thumb });
                    return photosListWithCount;
                }), false, true, new CancellationToken?());
            }
            else
            {*/
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = userOrGroupId.ToString();
            parameters["album_id"] = albumId;
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["extended"] = "1";
            parameters["rev"] = "1";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKPhoto>>("photos.get", parameters, callback);
        }

        private void GetPhotosImpl(string methodName, int userOrGroupId, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKPhoto>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = userOrGroupId.ToString();
            parameters["user_id"] = parameters["owner_id"];
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["extended"] = "1";
            parameters["sort"] = "0";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKPhoto>>(methodName, parameters, callback);
        }

        public void GetPhotos(int userOrGroupId, string aid, List<int> pids, long feed, string feedType, Action<VKResponse<IList<VKPhoto>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = userOrGroupId.ToString();
            parameters["album_id"] = aid;
            if (pids != null && pids.Count > 0)
                parameters["photo_ids"] = pids.GetCommaSeparated();
            parameters["extended"] = "1";
            if (feed != 0)
                parameters["feed"] = feed.ToString();
            if (!string.IsNullOrEmpty(feedType))
                parameters["feed_type"] = feedType;
            VKRequestsDispatcher.DispatchRequestToVK<IList<VKPhoto>>("photos.get", parameters, callback);
        }
        

        /// <summary>
        /// Получаем сервера для выгрузки фотографий
        /// </summary>
        /// <param name="userOrGroupId"></param>
        /// <param name="callback"></param>
        private void GetPhotoUploadServerWall(int userOrGroupId, Action<string> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (userOrGroupId != 0)
                parameters[userOrGroupId < 0 ? "group_id" : "user_id"] = userOrGroupId.ToString();

            VKRequestsDispatcher.DispatchRequestToVK<UploadServerAddress>("photos.getWallUploadServer", parameters, (result) =>
            {
                callback(result.error.error_code == VKErrors.None ? result.response.upload_url : null);
            });

        }

        public void UploadPhotoToWall(int userOrGroupId, byte[] photoData, Action<VKPhoto> callback, Action<double> progressCallback = null)
        {
            this.GetPhotoUploadServerWall(userOrGroupId, (uploadUrl =>
            {
                if (!string.IsNullOrEmpty(uploadUrl))
                {
                    JsonWebRequest.Upload(uploadUrl, photoData, "file1", "image", (JsonString,jsonResult) =>
                    {
                        if (!jsonResult)
                            callback(null);
                        else
                            this.SaveWallPhoto(userOrGroupId, JsonConvert.DeserializeObject<UploadPhotoResponseData>(JsonString), callback);
                        //if (!string.IsNullOrEmpty(jsonResult.photo))
                        //{
                        //    this.SaveWallPhoto(userOrGroupId, jsonResult, callback);
                        //}
                        //else
                        //    callback(null);
                    }, "MyImage.jpg", progressCallback);
                }
                else
                    callback(null);

            }));
        }

        public void UploadPhotoToHistory(bool add_to_news, int reply_to_story, IReadOnlyList<int> user_ids, string link_text, string link_url, int group_id, byte[] photoData, Action<VKErrors> callback, Action<double> progressCallback = null)
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
            //group_id
            //clickable_stickers
            VKRequestsDispatcher.DispatchRequestToVK<UploadServerAddress>("stories.getPhotoUploadServer", parameters,(result)=>
            {
                if (result.error.error_code != VKErrors.None)
                {
                    callback(result.error.error_code);
                    return;
                }

                string uploadUrl = result.response.upload_url;

                if (!string.IsNullOrEmpty(uploadUrl))
                {
                    JsonWebRequest.Upload(uploadUrl, photoData, "file1", "image", (JsonString, jsonResult) =>
                    {
                        //UploadStoryResponseData
                        if (!jsonResult)
                            callback(VKErrors.UnknownError);
                        else
                            callback(VKErrors.None);
                        /*
                        if (jsonResult == null)
                        {
                            callback(VKErrors.UnknownError);
                        }
                        else
                            callback(VKErrors.None);*/
                    }, "MyImage.jpg", progressCallback);
                }
                else
                    callback(VKErrors.UnknownError);
                /*
                 * {"response":{"story":{"id":456239017,"owner_id":460389,"date":1553952488,"can_see":1,"type":"photo","photo":{"id":456240345,"album_id":-81,"owner_id":460389,"sizes":[{"type":"temp","url":"https:\/\/sun9-28.userapi.com\/c841016\/v841016330\/58845\/JQ3cfo8b1i8.jpg","width":50,"height":30},{"type":"s","url":"https:\/\/sun9-13.userapi.com\/c841016\/v841016330\/58846\/dzVnYQ13Vnc.jpg","width":75,"height":45},{"type":"m","url":"https:\/\/sun9-24.userapi.com\/c841016\/v841016330\/58847\/EqT9rWA87kM.jpg","width":130,"height":78},{"type":"j","url":"https:\/\/sun9-17.userapi.com\/c841016\/v841016330\/58848\/TlyZXEPBNqs.jpg","width":256,"height":153},{"type":"x","url":"https:\/\/sun9-9.userapi.com\/c841016\/v841016330\/58849\/VXcAPHvL19s.jpg","width":604,"height":360},{"type":"y","url":"https:\/\/sun9-26.userapi.com\/c841016\/v841016330\/5884a\/H1ZMTMMlXTQ.jpg","width":807,"height":481},{"type":"z","url":"https:\/\/sun9-34.userapi.com\/c841016\/v841016330\/5884b\/rsWjlHumQ_Y.jpg","width":1280,"height":763},{"type":"w","url":"https:\/\/sun9-21.userapi.com\/c841016\/v841016330\/5884c\/oSPDcFAyXS4.jpg","width":2560,"height":1527},{"type":"o","url":"https:\/\/sun9-17.userapi.com\/c841016\/v841016330\/5884d\/73pRo3Sd-SI.jpg","width":130,"height":87},{"type":"p","url":"https:\/\/sun9-10.userapi.com\/c841016\/v841016330\/5884e\/cj8kuxFaNTM.jpg","width":200,"height":133},{"type":"q","url":"https:\/\/sun9-2.userapi.com\/c841016\/v841016330\/5884f\/dWPONv2_UNQ.jpg","width":320,"height":213},{"type":"r","url":"https:\/\/sun9-26.userapi.com\/c841016\/v841016330\/58850\/EWKZLvoNY7o.jpg","width":510,"height":340}],"text":"","date":1553952488},"can_share":1,"can_comment":0,"can_reply":0,"can_hide":1,"views":0,"access_key":"74548676bbc8e58419"}},"_sig":"2657ca97c01a4b7cbfa1d835688d3c0e"}
                 * */
            });

            

        }

        private void SaveWallPhoto(int userOrGroupId, UploadPhotoResponseData uploadData, Action<VKPhoto> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["server"] = uploadData.server;
            parameters["photo"] = uploadData.photo;
            parameters["hash"] = uploadData.hash;
            if (userOrGroupId != 0)
            {
                parameters[userOrGroupId < 0 ? "group_id" : "user_id"] = userOrGroupId.ToString();
            }

            VKRequestsDispatcher.DispatchRequestToVK<List<VKPhoto>>("photos.saveWallPhoto", parameters,(result)=> {
                callback(result.error.error_code == VKErrors.None ? result.response[0] : null);
            });
        }

        public void GetPhotoWithFullInfo(int ownerId, uint pid, string accessKey, int offset, Action<VKResponse<PhotoWithFullInfo>> callback)
        {
            string str1 = ownerId + "_" + pid;
            if (!string.IsNullOrWhiteSpace(accessKey))
                str1 = str1 + "_" + accessKey;
            /*
             * string str2 = string.Format("var ownerId = {0};
             * var pid  = {1}; 
             * var commentsCount = {2}; 
             * var offset = {3}; 
             * var countToRead = {4}; 
             * var likesAll = API.likes.getList({{ \"type\": \"photo\", \"owner_id\":ownerId, \"item_id\":pid, \"count\":10}}).items;
             * var photo = API.photos.getById({{\"photos\" : \"{6}\", \"extended\":1 }}); 
             * if (commentsCount == -1)
             *  {{
             *      commentsCount = photo[0].comments.count;   
             *      }} 
             *  var calculatedOffset = commentsCount - offset - countToRead; 
             *   if (calculatedOffset < 0)
             *   {{
             *       calculatedOffset = 0;
             *   }}
             *    var comments = API.photos.getComments({{ \"photo_id\" : pid, \"owner_id\": ownerId, \"offset\":calculatedOffset,  \"count\":countToRead, \"sort\":\"asc\", \"need_likes\":1, \"access_key\":\"{5}\", \"allow_group_comments\":1 }}); 
             *  var users2 = API.users.get({{ \"user_ids\":comments.items@.reply_to_user, \"fields\":\"first_name_dat,last_name_dat\"}}); 
             *  var photoTags = API.photos.getTags({{\"owner_id\":ownerId, \"photo_id\":pid, \"access_key\":\"{5}\"}});
             *  var userOrGroupIds = []; 
             *  var repostsCount = 0;
             *  if(likesAll+\"\"!=\"\")
             *  {{
             *  repostsCount =  API.likes.getList({{ \"type\": \"photo\", \"owner_id\":ownerId, \"item_id\":pid, \"filter\":\"copies\"}}).count;
             *  userOrGroupIds = likesAll;
             *  }} 
             *  if (commentsCount>0)
             *  {{ userOrGroupIds = userOrGroupIds + comments.items@.from_id;
             *  }} var userIds = [];
             *  var groupIds = [];
             *  var i = 0; if (ownerId < 0)
             *  {{  
             *  var negOwner = -ownerId;
             *  groupIds = groupIds + negOwner;  
             *  if (photo[0].user_id != 0 && photo[0].user_id != 100)
             *  {{    
             *  userIds = userIds + photo[0].user_id;  }}
             *  }}
             *  else
             *  {{
             *  userIds = userIds + ownerId;
             *  }}
             *  var length = userOrGroupIds.length;
             *  while (i < length)
             *  {{ 
             *  var id = parseInt(userOrGroupIds[i]);
             *  if (id > 0)
             *  {{ 
             *  if (userIds.length > 0)  
             *  {{    
             *  userIds = userIds + \",\";  
             *  }}  
             *  userIds = userIds + id;
             *  }}
             *  else if (id < 0)
             *  {{  
             *  id = -id;  
             *  if (groupIds.length > 0) 
             *  {{  
             *  groupIds = groupIds + \",\";  
             *  }}   
             *  groupIds = groupIds + id; 
             *  }} 
             *  i = i + 1;
             *  }} 
             *  var users  = API.users.get({{\"user_ids\":userIds, \"fields\":\"sex,photo_max,online,online_mobile\" }});
             *  var users3 =API.users.get({{\"user_ids\":userIds, \"fields\":\"first_name_dat,last_name_dat\" }}); 
             *  var groups = API.groups.getById({{\"group_ids\":groupIds}}); 
             *  return {{\"Photo\":photo[0], \"comments\": comments, \"LikesAllIds\":likesAll, \"Users\":users, \r\n\"Groups\":groups, \"Users2\":users2, \"Users3\": users3, \"PhotoTags\": photoTags, \"RepostsCount\":repostsCount}};"
    */        
            string code = "var comments = null;var likesAll = API.likes.getList({ type: \"photo\", owner_id:" + ownerId+", item_id:"+ pid+ ", count:15,extended:1, fields:\"photo_50\"});";
            code += "var photo = API.photos.getById({photos:\""+ str1+"\", extended:1 });";

            code += "if(photo.can_comment==1){";
            code += "comments = API.photos.getComments({ photo_id:" + pid + ", owner_id: " + ownerId + ", offset:"+ offset+",  count:20, sort:\"asc\", need_likes:1, access_key:\""+ accessKey+"\", allow_group_comments:1, extended:1 });";
            code += "var users2 = API.users.get({ user_ids:comments.items@.reply_to_user, fields:\"first_name_dat,last_name_dat\"});";
            code += "comments.profiles = comments.profiles + users2;";
            code += "}";

            code += "var photoTags = API.photos.getTags({owner_id:" + ownerId + ", photo_id:" + pid + ", access_key:\"" + accessKey + "\"});";
            code += "var repostsCount =  API.likes.getList({ type: \"photo\", owner_id:" + ownerId + ", item_id:" + pid + ", filter:\"copies\"}).count;";
            

            if(offset==0)
            {
                if (ownerId > 0)
                {
                    code += "var users3 =API.users.get({user_ids:"+ ownerId+", fields:\"photo_50\" }); ";
                    code += "return {Photo:photo[0], Comments:comments, LikesAllIds:likesAll, PhotoTags:photoTags, RepostsCount:repostsCount, Users:users3};";
                }
                else
                {
                    code += "var groups = API.groups.getById({group_ids:" + (-ownerId) + "}); ";
                    code += "return {Photo:photo[0], Comments:comments, LikesAllIds:likesAll, PhotoTags:photoTags, RepostsCount:repostsCount, Groups:groups};";
                }
            }
            else
            {
                code += "return {Photo:photo[0], Comments:comments, LikesAllIds:likesAll, PhotoTags:photoTags, RepostsCount:repostsCount};";
            }
            

            VKRequestsDispatcher.Execute<PhotoWithFullInfo>(code, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    if (result.response.Comments != null && result.response.Comments.items != null)
                    {
                        foreach (VKComment c in result.response.Comments.items)
                        {
                            if (c.reply_to_user != 0)
                            {
                                if (c.reply_to_user < 0)
                                {
                                    VKGroup g = result.response.Comments.groups.Find((group) => group.id == -c.reply_to_user);
                                    c._replyToUserDat = g.Title;
                                }
                                else
                                {
                                    VKUser r = result.response.Comments.profiles.Find((user) => user.id == c.reply_to_user);
                                    c._replyToUserDat = r.first_name_dat + " " + r.last_name_dat;
                                }
                            }

                            VKBaseDataForGroupOrUser owner = null;

                            if (c.from_id < 0)
                                owner = result.response.Comments.groups.Find((pro) => pro.id == (-c.from_id));
                            else
                                owner = result.response.Comments.profiles.Find((pro) => pro.id == (c.from_id));

                            if (owner == null)
                            {
                                owner = new VKUser() { photo_50 = "https://vk.com/images/wall/deleted_avatar_50.png", first_name = "Комментарий удалён пользователем или руководителем страницы" };
                            }

                            c.User = owner;

                            //c.ReplyTapped = () => { this.ReplyToComment(c); };
                            //c.DeleteTapped = () => { this.DeleteComment(c); };

                            if (c.thread != null && c.thread.count > 0)
                            {
                                VKBaseDataForGroupOrUser buttonOwner = null;

                                var thread_c = c.thread.items[0];
                                if (thread_c.from_id < 0)
                                    owner = result.response.Comments.groups.Find((pro) => pro.id == (-thread_c.from_id));
                                else
                                    owner = result.response.Comments.profiles.Find((pro) => pro.id == (thread_c.from_id));
                                thread_c.User = owner;
                                if (buttonOwner == null)
                                    buttonOwner = owner;

                                if (c.thread.count == 1)
                                {
                                    //c.thread.items[0].ReplyTapped = () => { this.ReplyToComment(c); };
                                    //c.thread.items[0].DeleteTapped = () => { this.DeleteComment(c); };
                                }
                                else
                                {
                                    c.Button = new VKComment.BottomButtonData() { TotalComments = c.thread.count, User = buttonOwner };
                                    c.thread.items.Clear();
                                    //c.LoadMoreInThread = () => { this.LoadMoreInThread(c); };
                                }
                            }

                            //c.Marked = this._commentId == c.id;

                            //this.Items.Add(c);
                        }
                    }
                }

                callback(result);
            }, (jsonStr) => {
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Comments", true);//Если нет доступа к комментам
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Users");
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Users2");
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Users3");
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Groups");
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "PhotoTags");
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "LikesAllIds", true);
                jsonStr = jsonStr.Replace("\"RepostsCount\":null", "\"RepostsCount\":0");
                return jsonStr;
            });
        }

        public void CreateComment(int ownerId, uint pid, uint replyCid, string message, bool fromGroup, List<string> attachmentIds, Action<VKResponse<VKComment>> callback, string accessKey = "", uint sticker_id = 0, string stickerReferrer = "")
        {
            string code = "var new_comment_id = API.photos.createComment({owner_id: " + ownerId + ", photo_id: " + pid + ", message: \"" + message.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
            code += ", from_group: " + (fromGroup ? "1" : "0");
            if(sticker_id!=0)
                code += ", sticker_id: "+ sticker_id;
            if (replyCid != 0)
                code += ", reply_to_comment: " + replyCid;
            if(attachmentIds.Any())
                code += ", attachments: \"" + attachmentIds.GetCommaSeparated() + "\"";
            if(!string.IsNullOrEmpty(accessKey))
                code += ", access_key: \"" + accessKey + "\"";
            if (!string.IsNullOrEmpty(stickerReferrer))
                code += ", sticker_referrer: \"" + stickerReferrer + "\"";
            code += "});";
            code += "var last_comments = API.photos.getComments({ owner_id: " + ownerId + ", photo_id: " + pid + ", need_likes: 1, count: 10, sort: \"desc\", preview_length: 0, access_key: \"" + accessKey + "\", allow_group_comments: 1}).items;";
            code += "return last_comments[0];";
            VKRequestsDispatcher.Execute<VKComment>(code, callback);
        }
        
        public void DeleteComment(int ownerId, uint pid, uint cid, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["photo_id"] = pid.ToString();
            parameters["comment_id"] = cid.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("photos.deleteComment", parameters, callback);
        }

        public void EditComment(uint cid, string text, int ownerId, List<string> attachmentIds, Action<VKResponse<ResponseWithId>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["comment_id"] = cid.ToString();
            parameters["message"] = text;
            parameters["owner_id"] = ownerId.ToString();
            if (!attachmentIds.IsNullOrEmpty())
                parameters["attachments"] = attachmentIds.GetCommaSeparated();
            VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("photos.editComment", parameters, callback);
        }

        public void Report(int ownerId, uint id, ReportReason reportReason, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["photo_id"] = id.ToString();
            parameters["reason"] = ((int)reportReason).ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("photos.report", parameters, callback);
        }

        private void GetPhotoUploadServerAlbum(int aid, Action<string> callback, int optionalGroupId = 0)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["album_id"] = aid.ToString();
            if (optionalGroupId != 0)
                parameters["group_id"] = optionalGroupId.ToString();

            VKRequestsDispatcher.DispatchRequestToVK<UploadServerAddress>("photos.getUploadServer", parameters, (result) => {
                callback(result.error.error_code == VKErrors.None ? result.response.upload_url : null);
            });
        }

        public void Search(string q, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKPhoto>>> callback, Dictionary<string, object> searchParams = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["q"] = q;
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKPhoto>>("photos.search", parameters, callback);
        }

        public void CopyPhotos(int ownerId, uint photoId, string accessKey, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["photo_id"] = photoId.ToString();
            parameters["access_key"] = accessKey.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("photos.copy", parameters, callback);
        }

        public void DeleteAlbum(int aid, Action<VKResponse<int>> callback, uint gid = 0)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["album_id"] = aid.ToString();
            if (gid != 0)
                parameters["group_id"] = gid.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("photos.deleteAlbum", parameters, callback);
        }

        public void DeletePhoto(uint pid, int ownerId, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["photo_id"] = pid.ToString();
            if (ownerId != 0)
                parameters["owner_id"] = ownerId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("photos.delete", parameters, callback);
        }

        /// <summary>
        /// Создает пустой альбом для фотографий.
        /// </summary>
        /// <param name="title">название альбома</param>
        /// <param name="description">текст описания альбома.</param>
        /// <param name="privacyView">настройки приватности просмотра альбома</param>
        /// <param name="privacyComment">настройки приватности комментирования альбома</param>
        /// <param name="callback"></param>
        /// <param name="gid">идентификатор сообщества, в котором создаётся альбом.</param>
        /// <param name="upload_by_admins_only">фотографии могут добавлять только редакторы и администраторы</param>
        /// <param name="comments_disabled">отключено ли комментирование альбома (только для альбома сообщества)</param>
        public void CreateAlbum(string title, string description, string privacyView, string privacyComment, Action<VKResponse<VKAlbumPhoto>> callback, uint gid = 0, bool upload_by_admins_only = false, bool comments_disabled = false)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["title"] = title;

            if (!string.IsNullOrEmpty(description))
                parameters["description"] = description;

            if (gid != 0)
            {
                parameters["group_id"] = gid.ToString();
                if(upload_by_admins_only)
                    parameters["upload_by_admins_only"] = "1";
                if(comments_disabled)
                    parameters["comments_disabled"] = "1";
            }
            else
            {
                parameters["privacy_view"] = privacyView;
                parameters["privacy_comment"] = privacyComment;
            }
            
            VKRequestsDispatcher.DispatchRequestToVK<VKAlbumPhoto>("photos.createAlbum", parameters, callback);
        }

        public void EditAlbum(int id, string title, string description, string privacyView, string privacyComment, Action<VKResponse<int>> callback, uint gid = 0)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["title"] = title;
            parameters["privacy_view"] = privacyView;
            parameters["privacy_comment"] = privacyComment;
            parameters["description"] = description;
            parameters["album_id"] = id.ToString();
            if (gid != 0)
                parameters["owner_id"] = (-gid).ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("photos.editAlbum", parameters, callback);
        }
    }
}
