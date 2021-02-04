using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.ViewModels
{
    public class ImageViewerViewModel
    {
        private string _aid;
        private AlbumType _albumType;
        private ViewerMode _mode;
        //private bool _isLocked;
        public uint _photosCount;
        private int _userOrGroupId;
        private bool _isLoading;
        private DateTime _date;
        //private bool _canLoadMoreProfileListPhotos;
        //private int _albumId;
        //private List<string> _accessKeys;
        private List<int> _ownerIds;
        private int _initialOffset;
        private List<VKDocument> _gifDocs;
        public readonly ObservableCollection<PhotoViewModel> PhotosCollection = new ObservableCollection<PhotoViewModel>();

        public ImageViewerViewModel(string aid, AlbumType albumType, int userOrGroupId, uint photosCount, List<VKPhoto> photos)
        {
            this._aid = aid;
            this._userOrGroupId = userOrGroupId;
            this._albumType = albumType;
            this._photosCount = photosCount;/* <= 0 ? -1 : photosCount;*/
            this._mode = ViewerMode.AlbumPhotos;
            if (photos == null || photos.Count == 0)
                return;
            this.ReadPhotos(photos);
        }

        public ImageViewerViewModel(uint photosCount, int initialOffset, List<VKPhoto> photos, ViewerMode mode)
        {
            this._mode = mode;
            this._photosCount = photosCount;
            //this._accessKeys = accessKeys;
            //this._ownerIds = ownerIds;
            this._initialOffset = initialOffset;
            if (photos != null && photos.Count > 0)
                this.ReadPhotos(photos);
            //else
            //    this.LoadPhotosByIds(photoIds);
        }

        public ImageViewerViewModel(int userOrGroupId, string aid, uint photosCount, DateTime date, List<VKPhoto> photos, ViewerMode mode)
        {
            this._aid = aid;
            this._userOrGroupId = userOrGroupId;
            this._mode = mode;
            this._photosCount = photosCount;
            if (photos != null)
                this.ReadPhotos(photos);
            this._date = date;
        }

        public ImageViewerViewModel(int userOrGroupId, List<VKPhoto> photos, bool canLoadMorePhotos, int albumId = 0)
        {
            //this._canLoadMoreProfileListPhotos = canLoadMorePhotos;
            this._mode = ViewerMode.ProfilePhotosList;
            this._userOrGroupId = userOrGroupId;
            this._photosCount = 0;// -1;
            //this._albumId = albumId;
            if (photos == null || photos.Count <= 0)
                return;
            this.ReadPhotos(photos);
        }

        private void ReadPhotos(List<VKPhoto> photos)
        {
            this.PhotosCollection.Clear();/*
            List<Photo>.Enumerator enumerator = photos.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Photo current = enumerator.Current;
                    PhotoWithFullInfo photoWithFullInfo1 = null;
                    if (current.owner_id == AppGlobalStateManager.Current.LoggedInUserId && current.album_id == -3L)
                    {
                        PhotoWithFullInfo photoWithFullInfo2 = new PhotoWithFullInfo();
                        photoWithFullInfo2.Photo = current;
                        photoWithFullInfo2.Users = new List<GroupOrUser>()
                        {
                            AppGlobalStateManager.Current.GlobalState.LoggedInUser
                        };
                        photoWithFullInfo2.Groups = new List<GroupOrUser>();
                        photoWithFullInfo2.PhotoTags = new List<PhotoVideoTag>();
                        photoWithFullInfo2.Comments = new List<Comment>();
                        photoWithFullInfo2.LikesAllIds = new List<long>();
                        photoWithFullInfo2.Users2 = new List<GroupOrUser>();
                        photoWithFullInfo2.Users3 = new List<GroupOrUser>();
                        photoWithFullInfo1 = photoWithFullInfo2;
                    }
                    this._photosCollection.Add(new PhotoViewModel(current, photoWithFullInfo1));
                }
            }
            finally
            {
                enumerator.Dispose();
            }*/
            foreach(var photo in photos)
            {
                this.PhotosCollection.Add(new PhotoViewModel(photo, null));
            }
            
        }

        private void LoadPhotosByIds(List<uint> photoIds)
        {
            for (int index = 0; index < photoIds.Count; ++index)
                this.PhotosCollection.Add(new PhotoViewModel(this._ownerIds[0], photoIds[index], /*index < this._accessKeys.Count ? this._accessKeys[index] :*/ ""));
        }

        

        public void LoadPhotosFromFeed(Action<bool> callback)
        {
            if (this._isLoading)
                return;
            this._isLoading = true;
            PhotosService.Instance.GetPhotos(this._userOrGroupId, this._aid, null, this._date.ToBinary(), this._mode == ViewerMode.Photos ? "photo" : "photo_tag", (res =>
            {
                this._isLoading = false;
                if (res.error.error_code == Core.Enums.VKErrors.None)
                {

                    int num;
                    for (int i = 0; i < res.response.Count; i = num + 1)
                    {
                        PhotoViewModel photoViewModel = Enumerable.FirstOrDefault<PhotoViewModel>(this.PhotosCollection, (p =>
                        {
                            if (p.Photo != null)
                                return p.Photo.id == res.response[i].id;
                            return false;
                        }));
                        if (photoViewModel != null)
                            this.PhotosCollection[(this.PhotosCollection).IndexOf(photoViewModel)].Photo = res.response[i];
                        else
                            this.PhotosCollection.Add(new PhotoViewModel(res.response[i]));
                        num = i;
                    }
                    if (this._photosCount < this.PhotosCollection.Count)
                        this._photosCount = (uint)this.PhotosCollection.Count;
                    callback(true);
                }
                else
                    callback(false);
            }));
        }

        public void LoadMorePhotos(Action<bool> callback = null)
        {
            if (this._isLoading)
                return;
            this._isLoading = true;
            //if (this._photosCount == this.PhotosCollection.Count || this._mode == ViewerMode.ProfilePhotosList && !this._canLoadMoreProfileListPhotos)
            if (this._photosCount == this.PhotosCollection.Count && this._photosCount != 0)
                return;

            switch (this._mode)
            {
                case ViewerMode.AlbumPhotos:
                    ImageViewerViewModel.GetAlbumPhotos(this._albumType, this._aid, this._userOrGroupId, this.PhotosCollection.Count, 50, (result)=>
                    {
                        if(result.error.error_code == Core.Enums.VKErrors.None)
                        {
                            foreach(var photo in result.response.items)
                                this.PhotosCollection.Add(new PhotoViewModel(photo));
                            
                            this._photosCount = result.response.count;
                            Execute.ExecuteOnUIThread(() => { 
                                callback?.Invoke(true);
                            });
                        }
                        else
                            callback?.Invoke(false);
                        this._isLoading = false;

                    });
                    break;
                case ViewerMode.PhotosByIdsForFavorites:
                    FavoritesService.Instance.GetFavePhotos(this._initialOffset + this.PhotosCollection.Count, 50, (result) =>
                    {
                        if (result.error.error_code == Core.Enums.VKErrors.None)
                        {
                            foreach (var photo in result.response.items)
                                this.PhotosCollection.Add(new PhotoViewModel(photo));

                            this._photosCount = result.response.count;
                            Execute.ExecuteOnUIThread(() => {
                                callback?.Invoke(true);
                            });
                        }
                        else
                            callback?.Invoke(false);
                        this._isLoading = false;
                    });
                    break;
            }
        }


        public static void GetAlbumPhotos(AlbumType albumType, string albumId, int userOrGroupId, int offset, int count, Action<VKResponse< VKCountedItemsObject<VKPhoto>>> callback)
        {
            PhotosService current = PhotosService.Instance;
            switch (albumType)
            {
                case AlbumType.AllPhotos:
                    current.GetAllPhotos(userOrGroupId,  offset, count, callback);
                    break;
                case AlbumType.ProfilePhotos:
                    current.GetProfilePhotos(userOrGroupId, offset, count, callback);
                    break;
                case AlbumType.PhotosWithUser:
                    current.GetUserPhotos(userOrGroupId, offset, count, callback);
                    break;
                case AlbumType.WallPhotos:
                    current.GetWallPhotos(userOrGroupId, offset, count, callback);
                    break;
                case AlbumType.SavedPhotos:
                    current.GetSavedPhotos(userOrGroupId, offset, count, callback);
                    break;
                case AlbumType.NormalAlbum:
                    current.GetAlbumPhotos(userOrGroupId, albumId, offset, count, callback);
                    break;
            }
        }

        public enum AlbumType
        {
            AllPhotos,
            ProfilePhotos,
            PhotosWithUser,
            WallPhotos,
            SavedPhotos,
            NormalAlbum,
        }

        public enum ViewerMode
        {
            AlbumPhotos,
            PhotosByIds,
            Photos,
            PhotoTags,
            PhotosByIdsForFavorites,
            ProfilePhotosList,
            Gifs,
        }
    }
}
