using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.ViewModels
{
    /// <summary>
    /// Это модель содержимого плейлиста
    /// </summary>
    public class AllAudioViewModel : GenericCollectionViewModel<VKAudio>
    {
        public static readonly short SAVED_ALBUM_ID = -99;
        public static readonly short RECOMMENDED_ALBUM_ID = -100;
        public static readonly short POPULAR_ALBUM_ID = -101;
        

        #region VM
        public BitmapImage Cover
        {
            get
            {
                if (string.IsNullOrEmpty(this.Playlist.photo_135))
                    return null;
                return new BitmapImage(new Uri(this.Playlist.photo_135));
            }
        }

        public string Title
        {
            get { return this.Playlist.title; }
        }

        public string SubTitle
        {
            get { return this.Playlist.owner_name; }
        }
        #endregion

        public int OwnerId
        {
            get { return this.Playlist.owner_id; }
        }

        public VKPlaylist Playlist { get; private set; }
        public AllAudioViewModel(VKPlaylist playlist)
        {
            this.Playlist = playlist;
        }
        
        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKAudio>> callback)
        {
            if (this.Playlist.id == 0)
            {

            }
            else if (this.Playlist.id == RECOMMENDED_ALBUM_ID)
            {

            }
            else if (this.Playlist.id == POPULAR_ALBUM_ID)
            {
                
            }
            else if (this.Playlist.id == SAVED_ALBUM_ID)
            {
                /*
                foreach(var track in AudioCacheManager.Instance.DownloadedList)
                {
                    this.Items.Add(track);
                }
                
                return;
                */
                callback(new VKError(), AudioCacheManager.Instance.DownloadedList);
            }
            else
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["owner_id"] = this.OwnerId.ToString();
                //parameters["need_owner"] = "1";
                //parameters["need_playlist"] = "1";
                parameters["audio_offset"] = offset.ToString();
                parameters["audio_count"] = count.ToString();
                parameters["id"] = this.Playlist.id.ToString();
                //access_key
                //v=5.68&lang=ru&https=1&owner_id=-119418792&id=5&need_owner=1&need_playlist=1&audio_count=100&access_key=&
                
                VKRequestsDispatcher.DispatchRequestToVK<VKPlaylist>("execute.getPlaylist", parameters,(result)=> {
                    if(result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = (uint)result.response.audios.Count;
                        callback(result.error, result.response.audios);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }
        }

        
        public override string GetFooterTextForCount
        {
            get
            {
                if (base._totalCount <= 0)
                    return LocalizedStrings.GetString("NoTracks");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneTrackFrm", "TwoFourTracksFrm", "FiveTracksFrm");
            }
        }

        public void RemoveAudio(VKAudio audio)
        {
            this.Items.Remove(audio);
            AudioCacheManager.Instance.RemoveLocalFile(audio);
        }
    }
}
