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
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LunaVK.ViewModels
{
    /// <summary>
    /// Альбомы + фотографии
    /// </summary>
    public class PhotosMainViewModel : GenericCollectionViewModel<VKPhoto>
    {
        public readonly int _ownerId;
        public string _ownerName;
        public PhotoAlbumViewModel AlbumsVM { get; private set; }

        //
        //
        public ObservableCollection<List<VKPhoto>> SortedItems { get; private set; }
        public int RowItemsCount { get; private set; }
        //
        //
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

        private uint _photosCount;
        public uint PhotosCount
        {
            get { return this._photosCount; }
            set
            {
                this._photosCount = value;
                base.NotifyPropertyChanged();
            }
        }

        public Visibility AlbumsVisible
        {
            get { return this.AlbumsVM.Items.Count > 0 && base._totalCount!=null ? Visibility.Visible : Visibility.Collapsed; }
        }

        public PhotosMainViewModel(int owner_id)
        {
            this._ownerId = owner_id;
            this.AlbumsVM = new PhotoAlbumViewModel(owner_id) { IsFooterHidden = true };//this.Albums = new ObservableCollection<VKAlbumPhoto>();
            base.ReloadCount = 40;
            base.LoadCount = 40;
            //
            //
            //
            this.RowItemsCount = 2;
            this.SortedItems = new ObservableCollection<List<VKPhoto>>();
        }

        public void UpdateRowItemsCount(int rowItemsCount)
        {
            if (rowItemsCount != this.RowItemsCount)
            {
                this.RowItemsCount = rowItemsCount;
                this.UpdateRowItems();
            }
        }

        public override void OnRefresh()
        {
            base.OnRefresh();
            this.AlbumsVM.OnRefresh();
            this.AlbumsCount = 0;
            this.PhotosCount = 0;
            base.NotifyPropertyChanged(nameof(this.AlbumsVisible));
        }

        private void UpdateRowItems()
        {
            int i = 0;
            var rowItems = base.Items.Take(RowItemsCount);
            while (rowItems != null && rowItems.Count() != 0)
            {
                var rowItemsCount = rowItems.Count();
                var item = this.SortedItems.ElementAtOrDefault(i);
                if (item == null)
                {
                    item = new List<VKPhoto>();
                    this.SortedItems.Insert(i, item);
                }

                for (int j = 0; j < rowItemsCount; j++)
                {
                    var rowItem = rowItems.ElementAt(j);
                    var temp = item.ElementAtOrDefault(j);
                    if (temp == null || !temp.Equals(rowItem))
                    {
                        item.Insert(j, rowItem);
                    }
                }

                while (item.Count > rowItemsCount)
                {
                    item.RemoveAt(item.Count - 1);
                }
                i++;
                rowItems = base.Items.Skip(i * RowItemsCount).Take(RowItemsCount);
            }

            int rowCount = base.Items.Count / RowItemsCount + 1;
            while (this.SortedItems.Count > rowCount)
            {
                this.SortedItems.RemoveAt(this.SortedItems.Count - 1);
            }
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKPhoto>> callback)
        {
            string code = "var photos = API.photos.getAll({count:"+ count+ ", need_covers:1,offset:"+ offset+", photo_sizes:1,need_system:1,owner_id:" + this._ownerId + "});";
            if (offset==0)
            {
                code += ("var albums = API.photos.getAlbums({count:25,need_covers:1,photo_sizes:1,need_system:1,owner_id:" + this._ownerId + "});");
                code += "albums.count = API.photos.getAlbumsCount({";
                if (this._ownerId > 0)
                    code += ("user_id:" + this._ownerId);
                else
                    code += ("group_id:" + (-this._ownerId));
                code += "});";
                code += "return {photos:photos,albums:albums};";
            }
            else
            {
                code += "return {photos:photos};";
            }
            

            VKRequestsDispatcher.Execute<AllPhotosResponse>(code, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    base._totalCount = result.response.photos.count;
                    callback(result.error, result.response.photos.items);

                    if(offset==0)
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            this.AlbumsVM.Items.Clear();//todo: override OnRefresh
                            
                            foreach (var album in result.response.albums.items)
                            {
                                this.AlbumsVM.Items.Add(album);//this.Albums.Add(album);
                            }
                            base.NotifyPropertyChanged(nameof(this.AlbumsVisible));
                            this.AlbumsCount = Math.Max( result.response.albums.count, (uint)result.response.albums.items.Count);//BugFix: вк неправильное число альбомов возвращает
                            this.AlbumsVM._totalCount = this.AlbumsCount;
                            this.AlbumsVM.CurrentLoadingStatus = ProfileLoadingStatus.Loaded;
                            this.PhotosCount = result.response.photos.count;
                        });
                    }
                    //
                    //
                    //
                    Execute.ExecuteOnUIThread(() =>
                    {
                        this.UpdateRowItems();
                    });
                    }
                else
                {
                    callback(result.error, null);
                }
            });
        }

        public void DeleteAlbum(VKAlbumPhoto album)
        {
            PhotosService.Instance.DeleteAlbum(album.id, (result) =>
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

        public void DeletePhoto(VKPhoto photo)
        {
            PhotosService.Instance.DeletePhoto(photo.id, photo.owner_id,(result) =>
            {
                if (result.error.error_code == VKErrors.None && result.response == 1)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        base.Items.Remove(photo);
                        base.NotifyPropertyChanged(nameof(base.FooterText));
                        this.PhotosCount--;
                    });
                }
            });
        }

        public override string GetFooterTextForCount
        {
            get
            {
                if (base._totalCount <= 0)
                    return LocalizedStrings.GetString("NoPhotos");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OnePhotoFrm", "TwoFourPhotosFrm", "FivePhotosFrm");
            }
        }

        public class AllPhotosResponse
        {
            public VKCountedItemsObject<VKPhoto> photos { get; set; }
            public VKCountedItemsObject<VKAlbumPhoto> albums { get; set; }
        }
    }
}
