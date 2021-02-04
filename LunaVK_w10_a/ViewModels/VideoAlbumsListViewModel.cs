using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LunaVK.ViewModels
{
    /// <summary>
    /// Список альбомов + видео
    /// </summary>
    public class VideoAlbumsListViewModel : GenericCollectionViewModel<VKVideoBase>
    {
        public int _ownerId;

        private uint _albumsCount;
        public uint AlbumsCount
        {
            get { return this._albumsCount; }
            set
            {
                this._albumsCount = value;
                base.NotifyPropertyChanged();
            }
        }

        private uint _videosCount;
        public uint VideosCount
        {
            get { return this._videosCount; }
            set
            {
                this._videosCount = value;
                base.NotifyPropertyChanged();
            }
        }

        public Visibility AlbumsVisible
        {
            get { return this.AlbumsVM.Items.Count > 0 && base._totalCount != null ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Core.ViewModels.GroupVideosViewModel.GenericCollectionAlbums AlbumsVM { get; private set; }

        public override void OnRefresh()
        {
            base.OnRefresh();
            this.AlbumsVM.OnRefresh();
            this.AlbumsCount = 0;
            this.VideosCount = 0;
            base.NotifyPropertyChanged(nameof(this.AlbumsVisible));
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKVideoBase>> callback)
        {
            string code = "var videos = API.video.get({count:" + count + ", offset:" + offset + ", extended:1,owner_id:" + this._ownerId + "});";
            if (offset == 0)
            {
                code += ("var albums = API.video.getAlbums({count:25,extended:1,need_system:1,owner_id:" + this._ownerId + "});");
                
                code += "return {videos:videos,albums:albums};";
            }
            else
            {
                code += "return {videos:videos};";
            }

            VKRequestsDispatcher.Execute<AllVideosResponse>(code, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    base._totalCount = result.response.videos.count;
                    callback(result.error, result.response.videos.items);

                    if (offset == 0)
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            this.AlbumsVM.Items.Clear();//todo: override OnRefresh

                            foreach (var album in result.response.albums.items)
                            {
                                this.AlbumsVM.Items.Add(album);//this.Albums.Add(album);
                            }
                            base.NotifyPropertyChanged(nameof(this.AlbumsVisible));
                            this.AlbumsCount = Math.Max(result.response.albums.count, (uint)result.response.albums.items.Count);//BugFix: вк неправильное число альбомов возвращает
                            this.AlbumsVM._totalCount = this.AlbumsCount;
                            this.AlbumsVM.CurrentLoadingStatus = ProfileLoadingStatus.Loaded;
                            this.VideosCount = result.response.videos.count;
                        });
                    }
                }
                else
                {
                    callback(result.error, null);
                }
            });
        }

        public void DeleteAlbum(VKVideoAlbum album)
        {
            VideoService.Instance.DeleteAlbum(album.id, (result) =>
            {
                if (result.error.error_code == VKErrors.None && result.response == 1)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        this.AlbumsVM.Items.Remove(album);
                        this.AlbumsCount--;
                    });
                }
            });
        }

        public void DeleteVideo(VKVideoBase video)
        {
            VideoService.Instance.Delete(video.owner_id, video.id, (result) =>
            {
                if (result.error.error_code == VKErrors.None && result.response == 1)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        base.Items.Remove(video);
                        base.NotifyPropertyChanged(nameof(base.FooterText));
                        this.VideosCount--;
                    });
                }
            });
        }

        public override string GetFooterTextForCount
        {
            get
            {
                if (base._totalCount <= 0)
                    return LocalizedStrings.GetString("NoVideos");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneVideoFrm", "TwoFourVideosFrm", "FiveOrMoreVideosFrm");
            }
        }

        public class AllVideosResponse
        {
            public VKCountedItemsObject<VKVideoBase> videos { get; set; }
            public VKCountedItemsObject<VKVideoAlbum> albums { get; set; }
        }


        public VideoAlbumsListViewModel(int owner_id)
        {
            this._ownerId = owner_id;
            this.AlbumsVM = new Core.ViewModels.GroupVideosViewModel.GenericCollectionAlbums(owner_id) { IsFooterHidden = true };
        }

        public void AddRemoveToMyVideos(VKVideoBase video, bool add)
        {
            VideoService.Instance.AddRemovedToFromAlbum(add, (int)Settings.UserId, VKVideoAlbum.ADDED_ALBUM_ID, video.owner_id, video.id, (result) => {
                if (result.error.error_code == VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        if(!add)
                            base.Items.Remove(video);
                    });
                }
            });
        }
    }
}
