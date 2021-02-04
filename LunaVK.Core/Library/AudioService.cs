using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunaVK.Core.Library
{
    public class AudioService
    {
        //private static readonly Func<string, List<VKAudio>> _deserializeAudioList = (Func<string, List<VKAudio>>)(jsonStr => JsonConvert.DeserializeObject<GenericRoot<VKList<VKAudio>>>(jsonStr).response.items);
        private Dictionary<string, string> _cachedResults = new Dictionary<string, string>();

        private static AudioService _instance;
        public static AudioService Instance
        {
            get
            {
                return AudioService._instance ?? (AudioService._instance = new AudioService());
            }
        }
        /*
        public void GetLyrics(int lyrics_id, Action<VKResponse<Lyrics>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["lyrics_id"] = lyrics_id.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<Lyrics>("audio.getLyrics", parameters, callback);
        }
        */
        public void GetAllTracksAndAlbums(int userOrGroupId, int offset, int count, Action<VKResponse<AudioPageGet>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = userOrGroupId.ToString();
            //parameters["need_owner"] = "1";
            //parameters["need_playlists"] = "1";
            //parameters["playlists_count"] = "12";
            parameters["audio_offset"] = offset.ToString();
            parameters["audio_count"] = count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<AudioPageGet>("execute.getMusicPage", parameters, callback);
        }
        
        public void MoveToAlbum(List<uint> aids, uint albumId, Action<VKResponse<object>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["audio_ids"] = aids.GetCommaSeparated();
            parameters["album_id"] = albumId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<object>("audio.moveToAlbum", parameters, callback);
        }

        public void EditAlbum(int albumId, string albumName, Action<VKResponse<object>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["title"] = albumName;
            parameters["album_id"] = albumId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<object>("audio.editAlbum", parameters, callback);
        }

        public void DeleteAlbum(int albumId, Action<VKResponse<object>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["album_id"] = albumId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<object>("audio.deleteAlbum", parameters, callback);
        }

        public void CreateAlbum(string albumName, Action<VKResponse<VKPlaylist>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["title"] = albumName;
            VKRequestsDispatcher.DispatchRequestToVK<VKPlaylist>("audio.addAlbum", parameters, callback);
        }

        public void GetAllAudio(Action<VKResponse<VKCountedItemsObject<VKAudio>>> callback, int? userOrGroupId = null, int? albumId = null, int offset = 0, int count = 0)
        {
            
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (userOrGroupId.HasValue && userOrGroupId.Value > 0)
                parameters["user_id"] = userOrGroupId.Value.ToString();
            if (userOrGroupId.HasValue && userOrGroupId.Value < 0)
                parameters["group_id"] = userOrGroupId.Value.ToString();
            if (albumId.HasValue)
                parameters["album_id"] = albumId.Value.ToString();
            parameters["offset"] = offset.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKAudio>>("audio.get", parameters, callback);
        }

        public void GetRecommended(int uid, int offset, int count, Action<VKResponse<List<VKAudio>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["user_id"] = uid.ToString();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<List<VKAudio>>("audio.getRecommendations", parameters, callback);
        }

        public void GetPopular(int offset, int count, Action<VKResponse<List<VKAudio>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<List<VKAudio>>("audio.getPopular", parameters, callback);
        }

        public void GetUserAlbums(Action<VKResponse<VKCountedItemsObject<VKPlaylist>>> callback, int? userOrGroupId = null, bool isGroup = false, int offset = 0, int count = 0)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (userOrGroupId.HasValue && !isGroup)
                parameters["user_id"] = userOrGroupId.Value.ToString();
            if (userOrGroupId.HasValue & isGroup)
                parameters["group_id"] = userOrGroupId.Value.ToString();
            parameters["offset"] = offset.ToString();
            //parameters["count"] = count == 0 ? VKConstants.AlbumsReadCount.ToString() : count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKPlaylist>>("audio.getAlbums", parameters, callback);
        }

        public void GetAllAudioForUser(int uid, int guid, int albumId, List<int> aids, int count, int offset, Action<VKResponse<List<VKAudio>>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<List<VKAudio>>("audio.get", new Dictionary<string, string>(), callback);
        }

        public void SearchTracks(string query, int offset, int count, Action<VKResponse<AudioPageSearch>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["q"] = query;
            parameters["count"] = count.ToString();
            parameters["offset"] = offset.ToString();
            parameters["search_own"] = "1";
            
            VKRequestsDispatcher.DispatchRequestToVK<AudioPageSearch>("audio.search", parameters, callback);
        }
        
        public void GetAudio(int ownerId, uint aid, Action<VKResponse<VKAudio>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["audio_id"] = aid.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<List<VKAudio>>("audio.getById", parameters, (result)=> {
                if(result.error.error_code == Enums.VKErrors.None)
                {
                    callback(new VKResponse<VKAudio>() { error = result.error, execute_errors = result.execute_errors, response = result.response[0]});
                }
                else
                {
                    callback(new VKResponse<VKAudio>() { error = result.error, execute_errors = result.execute_errors, response = null });
                }
            });
        }
        
        public void AddAudio(int ownerId, int aid, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["audio_id"] = aid.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("audio.add", parameters, callback);
        }

        public void DeleteAudios(int aid, int ownerId, Action<VKResponse<int>> callback)
        {
            /*
            string format = "API.audio.delete({{ \"audio_id\":{0}, \"owner_id\":{1} }});";
            int loggedInUserId = AppGlobalStateManager.Current.LoggedInUserId;
            string str = "";
            foreach (int num in list)
                str = str + string.Format(format, num, loggedInUserId) + Environment.NewLine;
            
            VKRequestsDispatcher.Execute<VKClient.Common.Backend.DataObjects.ResponseWithId>(str, callback, (Func<string, VKClient.Common.Backend.DataObjects.ResponseWithId>)(jsonStr => new VKClient.Common.Backend.DataObjects.ResponseWithId()), false, true, cancellationToken);
            */
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["audio_id"] = aid.ToString();
            parameters["owner_id"] = ownerId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("audio.delete", parameters, callback);
        }

        public void ReorderAudio(int aid, int oid, int album_id, int after, int before, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["audio_id"] = aid.ToString();
            if (oid != 0)
                parameters["owner_id"] = oid.ToString();
            if (album_id != 0L)
                parameters["album_id"] = album_id.ToString();
            parameters["after"] = after.ToString();
            parameters["before"] = before.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("audio.reorder", parameters, callback);
        }

        public void StatusSet(string audio, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["audio"] = audio;
            VKRequestsDispatcher.DispatchRequestToVK<int>("audio.setBroadcast", parameters, callback);
        }

        public void ResetBroadcast(Action<VKResponse<int>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<int>("audio.setBroadcast", new Dictionary<string, string>(), callback);
        }

        public void EditAudio(int ownerId, int id, string artist, string title, Action<VKResponse<int>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<int>("audio.edit", new Dictionary<string, string>()
            {
                { "owner_id", ownerId.ToString() },
                { "audio_id", id.ToString() },
                { "artist", artist },
                { "title", title }
            }, callback);
        }

        public void UploadAudio(byte[] data, string artist, string title, Action<VKResponse<VKAudio>> callback, Action<double> progressCallback = null, CancellationToken? cancellation = null)
        {
            VKRequestsDispatcher.DispatchRequestToVK<UploadServerAddress>("audio.getUploadServer", new Dictionary<string, string>(), (result) =>
            {
                if (result.error.error_code != Enums.VKErrors.None)
                    callback(new VKResponse<VKAudio>() { error = result.error });
                else
                {
                    JsonWebRequest.Upload(result.response.upload_url, data, "file", "audio", (JsonString, jsonResult) =>
                    {
                        if (!jsonResult)
                            callback(null);
                        else
                        {
                            UploadResponseData uploadResponseData = JsonConvert.DeserializeObject<UploadResponseData>(JsonString);
                            this.SaveAudio(uploadResponseData.audio, uploadResponseData.hash, uploadResponseData.server, callback);
                        }
                    }, "track.mp3", progressCallback, cancellation);
                }
            });
        }

        private void SaveAudio(string audio, string hash, string server, Action<VKResponse<VKAudio>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<VKAudio>("audio.save", new Dictionary<string, string>()
            {
                {  "server", server },
                { "audio", audio },
                { "hash", hash },
                //{ "artist", artist },
                //{ "title", title }
            }, callback);
        }
        
        public void GetAlbumArtwork(string search, Action<string> callback)
        {
            string format = "https://itunes.apple.com/search?media=music&limit=1&version=2&term={0}";
            search = System.Net.WebUtility.UrlEncode(search);
            if (this._cachedResults.ContainsKey(search))
                callback(this._cachedResults[search]);
            else
            {
                JsonWebRequest.SendHTTPRequestAsync(string.Format(format, search), (jsonResp, IsSucceeded) =>
                {
                    if (IsSucceeded)
                    {
                        try
                        {
                            AudioService.ItunesList itunesList = JsonConvert.DeserializeObject<AudioService.ItunesList>(jsonResp);
                            if (itunesList.results.Count > 0)
                            {
                                AudioService.ItunesAlbumArt result = itunesList.results[0];
                                var albArt = result.artworkUrl100.Replace("100x100", "600x600");
                                this._cachedResults[search] = albArt;
                                callback(albArt);
                            }
                            else
                                callback("");
                        }
                        catch (Exception)
                        {
                            callback(null);
                        }
                    }
                    else
                        callback(null);
                });
            }
        }

        public void ReloadAudio(IReadOnlyList<VKAudio> audios, Action<VKResponse<List<VKAudio>>> callback)
        {
            List<string> list = new List<string>();
            foreach(var audio in audios)
            {
                list.Add(audio.owner_id + "_" + audio.id + "_" + audio.actionHash + "_" + audio.urlHash);
                if (list.Count == 3)
                    break;
            }

            string ids = string.Join(",", list);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["ids"] = ids;
            VKRequestsDispatcher.DispatchRequestToVK<List<VKAudio>>("audio.reload", parameters, callback);
        }

        private class ItunesAlbumArt
        {
            public string wrapperType { get; set; }
            public string kind { get; set; }
            public string artistName { get; set; }
            public string collectionName { get; set; }
            public string trackName { get; set; }

            public string previewUrl { get; set; }

            public string artworkUrl30 { get; set; }
            public string artworkUrl60 { get; set; }
            public string artworkUrl100 { get; set; }
        }

        private class ItunesList
        {
            public int resultCount { get; set; }

            public List<AudioService.ItunesAlbumArt> results { get; set; }
        }

        public class AudioPageGet
        {
            public uint audios_count { get; set; }
            public List<VKAudio> audios { get; set; }

            public uint albums_count { get; set; }
            public List<VKPlaylist> albums { get; set; }
        }

        public class AudioPageSearch
        {
            public uint audios_count { get; set; }
            public List<VKAudio> audios { get; set; }

            public uint albums_count { get; set; }
            public List<VKPlaylist> albums { get; set; }

            public uint artists_count { get; set; }
            public List<VKGroup> artists { get; set; }
        }
    }
}
