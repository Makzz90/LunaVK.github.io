using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using LunaVK.Library;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using YoutubeExtractor;

namespace LunaVK.ViewModels
{
    public class VideoCommentsViewModel : ViewModelBase
    {
        public int OwnerId { get; private set; }

        public uint VideoId { get; private set; }

        public VKVideoBase Video { get; private set; }

        public List<VKUser> Likes { get; private set; }

        private readonly string _accessKey;

        private bool _isLoading;

        /*
        public ObservableCollection<string> Resolutions { get; private set; }

        private int _resolution = -1;
        public int Resolution
        {
            get
            {
                return this._resolution;
            }
            set
            {
                this._resolution = value;
                base.NotifyPropertyChanged(nameof(this.Resolution));
            }
        }

        /// <summary>
        /// Информация о ссылках, получено левым путём
        /// </summary>
        public List<VideoInfo> Infos;
        */
        public ObservableCollection<VideoResolution> Resolutions { get; private set; }

        private VideoResolution _resolution;
        public VideoResolution Resolution
        {
            get
            {
                return this._resolution;
            }
            set
            {
                if (value == null)
                    return;//происходит при вызове списка :(

                if (this._resolution == value)
                    return;
                this._resolution = value;
                this.NotifyPropertyChanged(nameof(this.Resolution));
                
                Settings.DefaultVideoResolution = value.Resolution;//AppGlobalStateManager.Current.GlobalState.DefaultVideoResolution = this._resolution.Resolution.ToString();

            }
        }

        public GenericCollectionComments CommentsVM { get; private set; }

        //public GenericCollectionRecomendations RecomendationsVM { get; private set; }

        /// <summary>
        /// Владелец видео
        /// </summary>
        public VKBaseDataForGroupOrUser Owner
        {
            get
            {
                if (this.Video != null && this.Video.Owner != null)
                    return this.Video.Owner;

                return null;
            }
        }

        public string SubscribersCountStr
        {
            get
            {
                if (this.Owner != null && this.OwnerId < 0)
                {
                    return UIStringFormatterHelper.FormatNumberOfSomething((this.Owner as VKGroup).members_count, "OneSubscriberFrm", "TwoFourSubscribersFrm", "FiveSubscribersFrm");
                }
                return "";
            }
        }

        public Visibility SubscribersCountVisibility
        {
            get
            {
                if (string.IsNullOrEmpty(this.SubscribersCountStr))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Action<ProfileLoadingStatus> LoadingStatusUpdated { get; set; }

        



        public VideoCommentsViewModel(int ownerId, uint videoId, string accessKey, VKVideoBase video = null)
        {
            this.CommentsVM = new GenericCollectionComments(ownerId, videoId);
            //this.RecomendationsVM = new GenericCollectionRecomendations(ownerId, videoId);
            if (this.Resolutions == null)
                this.Resolutions = new ObservableCollection<VideoResolution>();
            //this.Resolutions.Clear();
            //this._lastTimeCompletedPlayRequest = DateTime.MinValue;

            this.OwnerId = ownerId;
            this.VideoId = videoId;
            this._accessKey = accessKey;
            this.Video = video;
        }

        public string VideoTitle
        {
            get
            {
                if (this.Video != null)
                    return this.Video.Title;

                return null;
            }
        }

        public string VideoDescription
        {
            get
            {
                if (this.Video != null)
                    return this.Video.description;

                return null;
            }
        }

        public string MetaDataStr
        {
            get
            {
                string str = "";
                if (this.Video == null)
                    return str;
                if (this.Video.views > 0)
                    str += UIStringFormatterHelper.FormatNumberOfSomething(this.Video.views, "OneViewFrm", "TwoFourViewsFrm", "FiveViewsFrm");
                if (this.Video.date != null)
                {
                    if (!string.IsNullOrEmpty(str))
                        str += " · ";
                    str += UIStringFormatterHelper.FormateDateForEventUI(this.Video.date);
                }
                return str;
            }
        }

        public void Load()
        {
            this.Resolutions.Clear();

            this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Reloading);

            VideoService.Instance.GetVideoAndLikes(this.OwnerId, this.VideoId, this._accessKey, (result) => {

                Execute.ExecuteOnUIThread(() =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        this.Video = result.response.videos.items[0];
                        if(result.response.likes!=null)
                            this.Likes = result.response.likes.items;

                        this.NotifyPropertiesChanged();
                        this.InitResolutionsCollection();
                    }
                    else
                    {
                        this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.ReloadingFailed);
                    }
                });
            });
        }

        private void NotifyPropertiesChanged()
        {
            //Execute.ExecuteOnUIThread(delegate
            //{
            base.NotifyPropertyChanged(nameof(this.Owner));
            //base.NotifyPropertyChanged<List<VideoResolution>>(() => this.Resolutions);
            //    base.NotifyPropertyChanged<VideoResolution>(() => this.Resolution);
            //base.NotifyPropertyChanged<Visibility>(() => this.HaveManyResolutionsVisibility);
            base.NotifyPropertyChanged(nameof(this.VideoTitle));
            //base.NotifyPropertyChanged<string>(() => this.MediaDuration);
            base.NotifyPropertyChanged(nameof(this.ImageSrc));
            base.NotifyPropertyChanged(nameof(this.MetaDataStr));
            //base.NotifyPropertyChanged<Visibility>(() => this.DescriptionVisibility);
            //base.NotifyPropertyChanged<bool>(() => this.CanPlay);
            //base.NotifyPropertyChanged<Visibility>(() => this.CanPlayVisibility);
            //base.NotifyPropertyChanged<Visibility>(() => this.CannotPlayVisibility);
            //});

            //base.NotifyPropertyChanged<Visibility>((() => this.IsLiveVisibility));
            //base.NotifyPropertyChanged<Visibility>((() => this.ShowDurationVisibility));
            //base.NotifyPropertyChanged<string>((() => this.UIDuration));
            base.NotifyPropertyChanged(nameof(this.VideoDescription));
            base.NotifyPropertyChanged(nameof(this.VideoTitle));
            base.NotifyPropertyChanged(nameof(this.SubscribersCountStr));
            base.NotifyPropertyChanged(nameof(this.SubscribersCountVisibility));



            base.NotifyPropertyChanged(nameof(this.Video));
            base.NotifyPropertyChanged(nameof(this.Likes));
            base.NotifyPropertyChanged(nameof(this.IsLiked));
        }

        public BitmapImage ImageSrc
        {
            get
            {
                if (this.Video == null)
                    return null;
                //return this.Video.image_big ?? this.Video.image_medium ?? this.Video.image;
                
                string str = this.Video.photo_320;
                if (string.IsNullOrEmpty(str))
                    str = this.Video.photo_640;
                if (string.IsNullOrEmpty(str))
                    str = this.Video.photo_800;
                if (string.IsNullOrEmpty(str))
                {
                    if (this.Video.image.IsNullOrEmpty())
                        return null;
                    var temp = this.Video.image.FirstOrDefault((i) => i.width == 320);
                    if (temp != null)
                        str = temp.url;
                }
                return new BitmapImage(new Uri(str));
            }
        }

        private void InitResolutionsCollection()
        {
            if (string.IsNullOrEmpty(this.Video.player))
            {
                this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.ReloadingFailed);
                return;
            }


            string lowerInvariant = this.Video.player.ToLowerInvariant();
            if (lowerInvariant.Contains("youtube"))
            {
                DownloadUrlResolver.GetDownloadUrls(Video.player, (result) =>
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        if (result == null)
                        {
                            this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.ReloadingFailed);
                        }

                        foreach (var videoInfo in result)
                        {
                            //if (videoInfo.VideoType == VideoType.Mp4 && videoInfo.AdaptiveType == AdaptiveType.Video && videoInfo.Resolution != 0)
                            //{
                            //this.Resolutions.Add(videoInfo.Resolution.ToString() + "p. (" + videoInfo.VideoExtension + ")");
                            this.Resolutions.Add(new VideoResolution(videoInfo.AdaptiveType == AdaptiveType.Audio ? (uint)videoInfo.AudioBitrate : (uint)videoInfo.Resolution, videoInfo.DownloadUrl, videoInfo.AdaptiveType));
                            //}

                            //this.Infos.Add(videoInfo);
                        }

                        this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Loaded);
                    });
                    return;
                });
            }
            //else if (lowerInvariant.Contains("instagram.com/"))
            //{
            //    VideoPlayerHelper.LaunchMediaPlayer(new Uri(uri, UriKind.Absolute));
            //}
            //else
            //{
            //    Navigator.Current.NavigateToWebUri(uri, false, false);
            //}
            //https://player.vimeo.com/video/210230301?__ref=vk.api
            else if (lowerInvariant.Contains("vimeo.com"))
            {
                Regex regexObj = new Regex(@"/(\d+)", RegexOptions.Singleline);
                Match match = regexObj.Match(lowerInvariant);
                if(match.Success)
                {
                    string videoId = match.Groups[1].Value;
                    JsonWebRequest.SendHTTPRequestAsync("https://player.vimeo.com/video/"+ videoId +"/config", (html, result) =>
                    {
                        if (result == false)
                        {
                            Execute.ExecuteOnUIThread(() => { this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.ReloadingFailed); });
                            return;
                        }
                        //Regex regexObj2 = new Regex("\"url\".+?\"(?<url>\\S+?)\".+?\"quality\".+?\"(?<quality>\\d+)p", RegexOptions.Singleline);

                        string pattern = "\"url\":\"(?<url>[\\d\\w:/=%~.-]+?)\",\"cdn\":\"\\S+?\",\"quality\":\"(?<quality>\\d+)p";
                        Regex regexObj2 = new Regex(pattern);
                        MatchCollection mc = regexObj2.Matches(html);
                        if (mc.Count > 0)
                        {
                            this.ProcessMatch(mc,false);
                            return;
                        }
                    });
                }
                else
                {
                    Execute.ExecuteOnUIThread(() => { this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.ReloadingFailed); });
                    //NavigatorImpl.Instance.NavigateToWebUri(Video.player, true);
                }
            }
            
            else
            {
                JsonWebRequest.SendHTTPRequestAsync(this.Video.player, (html, result) =>
                {
                    if (result == false)
                    {
                        Execute.ExecuteOnUIThread(() => { this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.ReloadingFailed); });
                        return;
                    }

                    Regex QueryStringRegex = new Regex("\"url(?<quality>\\d+)\":\"(?<url>\\S+?)\"");
                    MatchCollection mc = QueryStringRegex.Matches(html);
                    if(mc.Count>0)
                    {
                        this.ProcessMatch(mc);
                        return;
                    }

                    
                    Regex QueryStringRegexApple = new Regex("source src=\"(?<url>\\S+)\"");
                    mc = QueryStringRegexApple.Matches(html);
                    if (mc.Count > 0)
                    {
                        this.ProcessMatch(mc);
                        return;
                    }

                    Execute.ExecuteOnUIThread(() => { this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.ReloadingFailed); });
                    //NavigatorImpl.Instance.NavigateToWebUri(Video.player, true);


                }, null, null, true);
            }
        }

    private void ProcessMatch(MatchCollection mc, bool decode = true)
    {
        Execute.ExecuteOnUIThread(() =>
        {
            foreach (Match m in mc)
            {
                string url = m.Groups["url"].Value;
                if(decode)
                    url = System.Net.WebUtility.UrlDecode(url).Replace("\\/", "/").Replace("&amp;", "&"); //Windows.Data.Html.HtmlUtilities.ConvertToText(text);//на 10.0.10586 вылеты


                //
                if (url.Contains("vk.com/video_hls.php") || url.Contains("video.m3u8"))//HTTP Live Streaming (HLS-VOD) - есть варианты качества
                {
                   // this.ProcessHLS(url);
                    continue;
                }
                else if(url.Contains("playlist.m3u"))//Трансляция - нет выбора качества
                {
                    this.Resolutions.Add(new VideoResolution(1080, url, AdaptiveType.Video));
                    continue;
                }
                //
                string quality = m.Groups["quality"].Value;
                int q = 0;
                bool res = int.TryParse(quality, out q);

                if (string.IsNullOrEmpty(quality))
                {
                    //.480.mp4?extra=

                    if (url.Contains(".240.mp4?extra="))
                        q = 240;
                    if (url.Contains(".360.mp4?extra="))
                        q = 360;
                    if (url.Contains(".480.mp4?extra="))
                        q = 480;
                    if (url.Contains(".720.mp4?extra="))
                        q = 720;
                    if (url.Contains(".1080.mp4?extra="))
                        q = 1080;

                    //https://vkvd109.mycdn.me/?expires=1611057358518&srcIp=46.32.66.61&srcAg=GECKO&ms=185.226.53.153&type=0&sig=rJATaKmh_V0&ct=0&urls=45.136.21.161&clientType=14&appId=512000384397&zs=12&id=1034770582074
                }

                
                if (q == 0)
                {
                    continue;
                }
                

                this.Resolutions.Add(new VideoResolution((uint)q,url, AdaptiveType.Video));
            }

            this.LoadingStatusUpdated?.Invoke(this.Resolutions.Count > 0 ? ProfileLoadingStatus.Loaded : ProfileLoadingStatus.ReloadingFailed);

            //if (this.Resolutions.Count > 0 && this.Resolution == null)
            //    this.Resolution = this.Resolutions.First();
        });
    }

    

    private bool _isAddingRemoving;

    internal void AddRemoveToMyVideos()
    {
        if (this._isAddingRemoving)
            return;
        this._isAddingRemoving = false;
        bool add = this.CanAddToMyVideos;
        VideoService.Instance.AddRemovedToFromAlbum(add, (int)Settings.UserId, VKVideoAlbum.ADDED_ALBUM_ID, this.OwnerId, this.VideoId, null);/*, (Action<BackendResult<ResponseWithId, ResultCode>>)(res =>
        {
            this._isAddingRemoving = false;
            string successString = add ? CommonResources.VideoNew_VideoHasBeenAddedToMyVideos : CommonResources.VideoNew_VideoHasBeenRemovedFromMyVideos;
            GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, successString, null);
            if (res.ResultCode != ResultCode.Succeeded || this._likesCommentsData == null || this._likesCommentsData.Albums == null)
                return;
            if (add)
                this._likesCommentsData.Albums.Add(VideoAlbum.ADDED_ALBUM_ID);
            else
                this._likesCommentsData.Albums.Remove(VideoAlbum.ADDED_ALBUM_ID);
        })*/
            }

        public bool CanEdit
        {
            get
            {
                if (this.Video != null)
                    return this.Video.can_edit == true;
                return false;
            }
        }

        public bool CanAddToMyVideos
        {
            get
            {/*
                VideoLikesCommentsData likesCommentsData = this._likesCommentsData;
                if ((likesCommentsData != null ? likesCommentsData.Albums : null) != null && !this._likesCommentsData.Albums.Contains(VideoAlbum.ADDED_ALBUM_ID))
                    return this.Video.can_add == true;*/

                return this.Video.can_add == true;
                //return false;
            }
        }

        public bool CanRemoveFromMyVideos
        {
            get
            {/*
                VideoLikesCommentsData likesCommentsData = this._likesCommentsData;
                if ((likesCommentsData != null ? likesCommentsData.Albums : null) != null)
                    return this._likesCommentsData.Albums.Contains(VideoAlbum.ADDED_ALBUM_ID);*/

                return false;
            }
        }

        public bool CanDelete
        {
            get
            {
                if (this.Video != null)
                    return this.Video.can_edit == true;
                return false;
            }
        }

        public void Delete()
        {
            VideoService.Instance.Delete(this.OwnerId, this.VideoId, (res =>
            {
                if (res.error.error_code == VKErrors.None)
                {

                }
            }));
        }

        public string VideoUri
        {//todo:access token need?
            get { return string.Format("https://vk.com/video{0}_{1}", this.OwnerId, this.VideoId); }
        }

        public bool IsLiked
        {
            get { return this.Video !=null && this.Video.likes != null && this.Video.likes.user_likes == true; }
        }

        private bool _inSync;

        public void AddRemoveLike()
        {
            if (this._inSync)
                return;

            LikesService.Instance.AddRemoveLike(this.Video.likes.user_likes == false, this.OwnerId, this.VideoId, LikeObjectType.video, (result) =>
            {
                this._inSync = false;
                Execute.ExecuteOnUIThread(() =>
                {
                    if (result != -1)
                    {
                        this.Video.likes.count = (uint)result;
                        this.Video.likes.user_likes = !this.Video.likes.user_likes;

                        base.NotifyPropertyChanged(nameof(this.Video));
                        base.NotifyPropertyChanged(nameof(this.IsLiked));
                    }
                });
            });
        }

        public class VideoResolution
        {
            public uint Resolution { get; private set; }
            public string Url { get; private set; }
            public AdaptiveType Type { get; private set; }

            public VideoResolution(uint res, string url, AdaptiveType type)
            {
                this.Resolution = res;
                this.Url = url;
                this.Type = type;
            }

            public override string ToString()
            {
                if (this.Type == AdaptiveType.Audio)
                    return "Audio only " + "(" + this.Resolution + "k.)";
                string name = this.Resolution + "p.";
                if (this.Url.Contains("m3u8"))
                    name += " (HLS)";
                return name;
            }
        }









        public class GenericCollectionComments : GenericCollectionViewModel<VKComment>
        {
            public int OwnerId { get; private set; }
            public uint VideoId { get; private set; }

            public string TextWatermarkText
            {
                get { return LocalizedStrings.GetString("Comment"); }
            }

            public Visibility BotKeyboardVisibility
            {
                get { return Visibility.Collapsed; }
            }

            public object BotKeyboardButtons { get; private set; }

            public GenericCollectionComments(int ownerId, uint videoId)
            {
                this.OwnerId = ownerId;
                this.VideoId = videoId;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKComment>> callback)
            {
                VideoService.Instance.GetComments(this.OwnerId, this.VideoId, offset, count, (result) =>
                   {
                       if (result.error.error_code == VKErrors.None)
                       {
                           base._totalCount = result.response.count;
                           callback(result.error, result.response.items);
                       }
                       else
                       {
                           callback(result.error, null);
                       }
                   });
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("PostCommentsPage_NoComments");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "PostCommentPage_OneCommentFrm", "PostCommentPage_TwoFourCommentsFrm", "PostCommentPage_FiveCommentsFrm");
                }
            }
        }




        public class GenericCollectionRecomendations : GenericCollectionViewModel<VKVideoBase>
        {
            public int OwnerId { get; private set; }
            public uint VideoId { get; private set; }
            //StatisticsActionSource actionSource, string context

            public GenericCollectionRecomendations(int ownerId, uint videoId)
            {
                this.OwnerId = ownerId;
                this.VideoId = videoId;
                base.LoadCount = 10;
                base.ReloadCount = 10;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKVideoBase>> callback)
            {
                VideoService.Instance.GetRecommendations(this.OwnerId, this.VideoId, offset, count, (result) =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }

            //public override string GetFooterTextForCount
            //{
            //    get
            //    {
            //        if (base._totalCount <= 0)
            //            return LocalizedStrings.GetString("PostCommentsPage_NoComments");
            //        return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "PostCommentPage_OneCommentFrm", "PostCommentPage_TwoFourCommentsFrm", "PostCommentPage_FiveCommentsFrm");
            //    }
            //}
        }
    }
}
