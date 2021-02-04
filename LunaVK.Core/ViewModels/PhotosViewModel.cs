using System;
using System.Collections.Generic;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using System.Collections.ObjectModel;
using System.Linq;
using LunaVK.Core.Framework;
using Windows.UI.Xaml;

namespace LunaVK.Core.ViewModels
{
    /// <summary>
    /// PhotoAlbumViewModel
    /// </summary>
    public class PhotosViewModel : GenericCollectionViewModel<VKPhoto>
    {
        public int _albumId = 0;
        public int _ownerId = 0;

        private VKAlbumPhoto _album;

        public PhotosViewModel(int albumId, int ownerId)
        {
            this._albumId = albumId;
            this._ownerId = ownerId;

            this.RowItemsCount = 4;
            this.SortedItems = new ObservableCollection<List<VKAttachment>>();
            base.Items.CollectionChanged += Items_CollectionChanged;
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKPhoto>> callback)
        {
            /*
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["count"] = count.ToString();
            parameters["rev"] = "1";
            parameters["owner_id"] = this._ownerId.ToString();
            parameters["album_id"] = this._albumId.ToString();

            if (offset > 0)
                parameters["offset"] = offset.ToString();
            
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKPhoto>>("photos.get", parameters,(result)=>
            {
                if (result.error.error_code != VKErrors.None)
                {
                    callback(result.error, null);
                    //Execute.ExecuteOnUIThread(this.UpdateRowItems);
                    return;
                }

                base._totalCount = result.response.count;
                callback(result.error, result.response.items);
            });
            */
            string code = string.Format("var albums=null;var photos=API.photos.get({{count:{2},owner_id:{0},album_id:{1},rev:1,offset:{3}}});", this._ownerId, this._albumId, count,offset);
            if (offset == 0)
            {
                code += string.Format("albums = API.photos.getAlbums({{need_covers:1, count:1, owner_id:{0}, album_ids:{1}}});", this._ownerId, this._albumId);
            }
            code += "return {photos:photos,albums:albums};";

            VKRequestsDispatcher.Execute<GetPhotosResp>(code, (result) => {
                if (result.error.error_code != VKErrors.None)
                {
                    callback(result.error, null);
                    return;
                }

                
                base._totalCount = result.response.photos.count;

                if (offset == 0)
                {
                    VKAlbumPhoto album = result.response.albums.items[0];

                    if (result.response.photos.items.Count > 0)
                        album.thumb_src = result.response.photos.items[0].photo_320;

                    this._album = album;
                    base.NotifyPropertyChanged(nameof(this.AlbumName));
                    base.NotifyPropertyChanged(nameof(this.AlbumDescription));
                    base.NotifyPropertyChanged(nameof(this.HaveAlbumDescVisibility));
                    base.NotifyPropertyChanged(nameof(this.PhotosCountStr));
                    base.NotifyPropertyChanged(nameof(this.ThumbSrc));

                }


                callback(result.error, result.response.photos.items);
            });
        }

        private class GetPhotosResp
        {
            public VKCountedItemsObject<VKPhoto> photos { get; set; }
            public VKCountedItemsObject<VKAlbumPhoto> albums { get; set; }
        }

        public override string GetFooterTextForCount
        {
            get
            {
                if (!base._totalCount.HasValue || base._totalCount <= 0)
                    return LocalizedStrings.GetString("NoPhotos");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OnePhoto", "TwoFourPhotosFrm", "FiveOrMorePhotosFrm");
            }
        }
        
        public string AlbumName
        {
            get
            {
                if (this._album == null)
                    return null;
                return this._album.title;
            }
        }
        
        public string AlbumDescription
        {
            get
            {
                if (this._album == null)
                    return null;
                return this._album.description;
            }
        }

        public Visibility HaveAlbumDescVisibility
        {
            get
            {
                if (!string.IsNullOrEmpty(this.AlbumDescription))
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public string ThumbSrc
        {
            get
            {
                if (this._album == null)
                    return null;
                return this._album.thumb_src;
            }
        }

        public string PhotosCountStr
        {
            get
            {
                return this.GetFooterTextForCount;
            }
        }

        public bool CanEditAlbum
        {
            get
            {
                /*
                if (this._albumType == AlbumType.NormalAlbum && (this._ownerId == Settings.UserId && !this._isGroup || this.EditableGroupAlbum))
                    return true;
                if (this._albumType == AlbumType.SavedPhotos)
                    return this._userOrGroupId == AppGlobalStateManager.Current.LoggedInUserId;
                */
                
                if (this._albumId == VKAlbumPhoto.SavedPhotos)
                    return this._ownerId == Settings.UserId;
                return false;
            }
        }

        public bool CanAddPhotos
        {
            get
            {
                if (this._albumId == VKAlbumPhoto.SavedPhotos)
                    return false;
                //if (!this.CanEditAlbum)
                //    return this._forceCanUpload;
                return true;
            }
        }

        public bool CanRemovePhoto
        {
            get
            {
                //if (this._albumId == VKAlbumPhoto.PhotosWithUser)
                //    return false;
                if (this.CanEditAlbum)
                    return true;
                //if (!this._isGroup)
                //    return this._userOrGroupId == AppGlobalStateManager.Current.LoggedInUserId;
                return false;
            }
        }








        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.UpdateRowItems();
        }

        public int RowItemsCount { get; private set; }

        public void UpdateRowItemsCount(int rowItemsCount)
        {
            if (rowItemsCount != this.RowItemsCount)
            {
                System.Diagnostics.Debug.WriteLine("UpdateRowItemsCount: " + rowItemsCount);
                this.RowItemsCount = rowItemsCount;
                this.UpdateRowItems();
            }
        }

        public ObservableCollection<List<VKAttachment>> SortedItems { get; private set; }

        private void UpdateRowItems()
        {
            this.SortedItems.Clear();
            int i = 0;
            while (true)
            {
                var rowItems = this.Items.Skip(i * this.RowItemsCount).Take(this.RowItemsCount);
                if (rowItems.Count() == 0)
                    break;
                //this.SortedItems.Add(new ClassImgItem(rowItems.ToList()));
                this.SortedItems.Add(rowItems.Select((p => new VKAttachment() { type = VKAttachmentType.Photo, photo = p })).ToList());
                i++;
            }
        }
    }
}
