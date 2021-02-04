using LunaVK.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using LunaVK.Core.DataObjects;
using LunaVK.Photo;
using LunaVK.Photo.ViewModels;
using Windows.System.Threading;
using Windows.Foundation;
using System.ComponentModel;
using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System.IO;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.FileProperties;

namespace LunaVK.Photo.ViewModels
{
    public class PhotoPickerPhotosViewModel : ViewModelBase//, IPhotoPickerPhotosViewModel
    {
        private ObservableCollection<AlbumPhotoHeaderFourInARow> _photos = new ObservableCollection<AlbumPhotoHeaderFourInARow>();
        private ObservableCollection<AlbumPhoto> _albumPhotos = new ObservableCollection<AlbumPhoto>();
        private List<AlbumPhoto> _selectedPhotos = new List<AlbumPhoto>();
        private HashSet<long> _timestamps = new HashSet<long>();
        private int _recentlyAddedImageInd = -1;
        private int _countToLoad = 100;
        private string _albumId;
        private bool _isLoaded;
        private int _maxAllowedToSelect;
        private ImageEditorViewModel _imageEditor;
        private bool _ownPhotoPick;
        private int _totalCount;
        private bool _isLoading;

        public List<AlbumPhoto> SelectedPhotos
        {
            get
            {
                return this._selectedPhotos;
            }
        }

        public ObservableCollection<AlbumPhotoHeaderFourInARow> Photos
        {
            get
            {
                return this._photos;
            }
        }

        public ObservableCollection<AlbumPhoto> AlbumPhotos
        {
            get
            {
                return this._albumPhotos;
            }
            set
            {
                this._albumPhotos = value;
                base.NotifyPropertyChanged();
            }
        }

        public int MaxAllowedToSelect
        {
            get
            {
                return this._maxAllowedToSelect;
            }
        }

        public int SelectedCount
        {
            get
            {
                return this._selectedPhotos.Count;
            }
        }

        public bool CanTakePicture
        {
            get
            {
                return this._albumId == "Camera Roll";
            }
        }

        private bool IsLoaded
        {
            get
            {
                return this._isLoaded;
            }
            set
            {
                if (this._isLoaded == value)
                    return;
                this._isLoaded = value;
                base.NotifyPropertyChanged();
                base.NotifyPropertyChanged(nameof( this.PhotosCountStr));
            }
        }

        public string PhotosCountStr
        {
            get
            {
//                if (this._isLoaded)
//                    return CommonUtils.FormatPhotosCountString(this._photos.Sum<AlbumPhotoHeaderFourInARow>((Func<AlbumPhotoHeaderFourInARow, int>)(p => p.GetItemsCount())));
                return "";
            }
        }

        public int PhotosCount
        {
            get
            {
                return this._photos.Sum<AlbumPhotoHeaderFourInARow>((Func<AlbumPhotoHeaderFourInARow, int>)(p => p.GetItemsCount()));
            }
        }

        public string Title
        {
            get
            {
                return this.AlbumName.ToUpperInvariant();
            }
        }

        public string AlbumName
        {
            get
            {
                string albumId = this._albumId;
                if (albumId == "Camera Roll")
                    return "AlbumCameraRoll".ToLowerInvariant();
                if (albumId == "Saved Pictures")
                    return "AlbumSavedPictures".ToLowerInvariant();
                if (albumId == "Sample Pictures")
                    return "AlbumSamplePictures".ToLowerInvariant();
                if (albumId == "Screenshots")
                    return "AlbumScreenshots".ToLowerInvariant();
                return this._albumId;
            }
        }

        public string AlbumId
        {
            get
            {
                return this._albumId;
            }
            set
            {
                if (!(this._albumId != value))
                    return;
                this._albumId = value;
                base.NotifyPropertyChanged();
                base.NotifyPropertyChanged(nameof( this.AlbumName));
                base.NotifyPropertyChanged(nameof(this.Title));
                base.NotifyPropertyChanged(nameof(this.CanTakePicture));
                this.LoadData(true);
            }
        }

        public ImageEditorViewModel ImageEditor
        {
            get
            {
                return this._imageEditor;
            }
        }

        public bool OwnPhotoPick
        {
            get
            {
                return this._ownPhotoPick;
            }
        }

        public int TotalCount
        {
            get
            {
                return this._totalCount;
            }
        }

        public int RecentlyAddedImageInd
        {
            get
            {
                return this._recentlyAddedImageInd;
            }
        }

        public int CountToLoad
        {
            get
            {
                return this._countToLoad;
            }
            set
            {
                this._countToLoad = value;
            }
        }

        public bool SuppressEXIFFetch
        {
            get
            {
                return this.ImageEditor.SuppressParseEXIF;
            }
            set
            {
                this.ImageEditor.SuppressParseEXIF = value;
            }
        }

        public PhotoPickerPhotosViewModel(int maxAllowedToSelect, bool ownPhotoPick)
        {
            this._albumId = "Camera Roll";
            this._maxAllowedToSelect = maxAllowedToSelect;
            this._ownPhotoPick = ownPhotoPick;
            this._imageEditor = new ImageEditorViewModel();
        }

        public void CleanupSession()
        {
            this._imageEditor.CleanupSession();
        }

        public void LoadData(bool refresh = true, Action callback = null)
        {
            if (this._isLoading)
                return;
            this._isLoading = true;
            if (refresh)
            {
                this._imageEditor.ResetCachedMediaLibrary();
                this._photos.Clear();
                this._albumPhotos.Clear();
            }
            this.IsLoaded = false;
            this.SetInProgress(true, "");
            IAsyncAction asyncAction = ThreadPool.RunAsync((async o =>
            {
                try
                {
                    List<AlbumPhoto> photoHeaders = new List<AlbumPhoto>();
                    HashSet<long> longSet1 = new HashSet<long>();
                    //using (MediaLibrary mediaLibrary = new MediaLibrary())
                    //{
                    var mediaLibrary = KnownFolders.PicturesLibrary;
                    //using (PictureAlbum pictureAlbum = (mediaLibrary.RootPictureAlbum.Albums).FirstOrDefault((a => a.Name == this._albumId)))
                    var pictureAlbum = await mediaLibrary.GetFoldersAsync();
                   // {
                        this._recentlyAddedImageInd = -1;
                        if (pictureAlbum != null)
                        {
                        int count = 1;// pictureAlbum.Pictures.Count;
                            this._totalCount = count;
                            int num1;









                        var queryOptions = new QueryOptions
                        {
                            FolderDepth = FolderDepth.Deep,
                            IndexerOption = IndexerOption.UseIndexerWhenAvailable
                        };
                        queryOptions.FileTypeFilter.Add(".jpg");
                        queryOptions.FileTypeFilter.Add(".png");


                        // clear all existing sorts
                        queryOptions.SortOrder.Clear();

                        // add descending sort by date modified
                        SortEntry se = new SortEntry() { PropertyName = "System.DateModified", AscendingOrder = false };
                        queryOptions.SortOrder.Add(se);
                        StorageFileQueryResult query = mediaLibrary.CreateFileQueryWithOptions(queryOptions);
                        var fileList = await query.GetFilesAsync((uint)this.Photos.Count, 40);












                        for (int i = count - 1 - this._albumPhotos.Count; i >= 0; i = num1 - 1)
                            {
                                double width;
                                double height;
                            //using (Picture picture = pictureAlbum.Pictures[i])
                            StorageFile file = fileList[i];
                            ImageProperties picture = await file.Properties.GetImagePropertiesAsync();
                            




                            //{
                            HashSet<long> longSet2 = longSet1;
                                    DateTime date = new DateTime( picture.DateTaken.ToUnixTimeMilliseconds());
                                    long ticks1 = date.Ticks;
                                    longSet2.Add(ticks1);
                                    if (this._timestamps != null)
                                    {
                                        HashSet<long> timestamps = this._timestamps;
                                        date = new DateTime(picture.DateTaken.ToUnixTimeMilliseconds());
                                        long ticks2 = date.Ticks;
                                        if (!timestamps.Contains(ticks2))
                                            this._recentlyAddedImageInd = count - 1 - this._albumPhotos.Count - i;
                                    }
                                    width = (double)picture.Width;
                                    height = (double)picture.Height;
                                //}
                                AlbumPhoto albumPhoto = new AlbumPhoto(this._albumId, i, ((ap, preview) => this._imageEditor.GetImageStream(ap.AlbumId, ap.SeqNo, preview)), width, height);
                                if (this._selectedPhotos.Any<AlbumPhoto>((p =>
                                {
                                    if (p.SeqNo == i)
                                        return p.AlbumId == this._albumId;
                                    return false;
                                })))
                                    albumPhoto.IsSelected = true;
                                albumPhoto.PropertyChanged += new PropertyChangedEventHandler(this.albumPhoto_PropertyChanged);
                                photoHeaders.Add(albumPhoto);
                                if (photoHeaders.Count != this._countToLoad)
                                    num1 = i;
                                else
                                    break;
                            }
                            if (refresh)
                            {
                                this._timestamps = longSet1;
                            }
                            else
                            {
                                foreach (long num2 in longSet1)
                                {
                                    if (!this._timestamps.Contains(num2))
                                        this._timestamps.Add(num2);
                                }
                            }
                            Execute.ExecuteOnUIThread(() =>
                            {
                                foreach (AlbumPhoto albumPhoto in photoHeaders)
                                    this.AlbumPhotos.Add(albumPhoto);
                                foreach (IEnumerable<AlbumPhoto> photos in photoHeaders.Partition<AlbumPhoto>(4))
                                    this._photos.Add(new AlbumPhotoHeaderFourInARow(photos)
                                    {
                                        SelectionOpacity = this._ownPhotoPick ? 0.0 : 1.0
                                    });
                                if (this._albumPhotos.Count == this.PhotosCount)
                                    this.IsLoaded = true;
                                this.SetInProgress(false, "");
                                if (callback != null)
                                    callback();
                                this._isLoading = false;
                            });
                        }
                        else
                        {
                            this.SetInProgress(false, "");
                            if (callback != null)
                                callback();
                            this._isLoading = false;
                        }
                   // }
                    }
                //}
                catch (Exception ex)
                {
                    //Logger.Instance.Error("Failed to read gallery photos ", ex);
                }
            }));
        }

        private void albumPhoto_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "IsSelected"))
                return;
            AlbumPhoto ap = sender as AlbumPhoto;
            if (ap == null)
                return;
            if (ap.IsSelected && !this._selectedPhotos.Any<AlbumPhoto>((Func<AlbumPhoto, bool>)(p =>
            {
                if (p.AlbumId == ap.AlbumId)
                    return p.SeqNo == ap.SeqNo;
                return false;
            })))
            {
                if (this._selectedPhotos.Count == this._maxAllowedToSelect)
                {
                    ap.IsSelected = false;
                }
                else
                {
                    this._selectedPhotos.Add(ap);
                    this.NotifySelectionChanged();
                }
            }
            if (ap.IsSelected)
                return;
            AlbumPhoto albumPhoto = this._selectedPhotos.FirstOrDefault<AlbumPhoto>((Func<AlbumPhoto, bool>)(p =>
            {
                if (p.AlbumId == ap.AlbumId)
                    return p.SeqNo == ap.SeqNo;
                return false;
            }));
            if (albumPhoto == null)
                return;
            this._selectedPhotos.Remove(albumPhoto);
            this.NotifySelectionChanged();
        }

        private void NotifySelectionChanged()
        {
            base.NotifyPropertyChanged(nameof(this.SelectedCount));
            base.NotifyPropertyChanged(nameof(this.Title));
        }

        private AlbumPhoto GetAlbumPhotoBySeqNo(int seqNo)
        {
            int index = (this._totalCount - seqNo - 1) / 4;
            if (index < this._photos.Count)
                return this._photos[index].GetAsAlbumPhotos().FirstOrDefault<AlbumPhoto>((Func<AlbumPhoto, bool>)(p => p.SeqNo == seqNo));
            return (AlbumPhoto)null;
        }

        internal void HandleEffectUpdate(string albumId, int seqNo)
        {
            if (!(this._albumId == albumId))
                return;
            AlbumPhoto albumPhotoBySeqNo = this.GetAlbumPhotoBySeqNo(seqNo);
            if (albumPhotoBySeqNo == null)
                return;
            albumPhotoBySeqNo.NotifyUpdateThumbnail();
        }

















































        public class AlbumPhotoHeaderFourInARow// : IItemsCount
        {
            public AlbumPhoto Photo1 { get; set; }

            public AlbumPhoto Photo2 { get; set; }

            public AlbumPhoto Photo3 { get; set; }

            public AlbumPhoto Photo4 { get; set; }

            public bool AllowEdit { get; private set; }

            public Visibility AllowEditVisibility
            {
                get
                {
                    if (!this.AllowEdit)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }

            public bool AllowRemove { get; private set; }

            public Visibility AllowRemoveVisibility
            {
                get
                {
                    if (!this.AllowRemove)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }

            public Visibility AllowEditOrRemoveVisibility
            {
                get
                {
                    if (!this.AllowEdit && !this.AllowRemove)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }

            public double SelectionOpacity { get; set; }

            public Visibility Photo1IsSet
            {
                get
                {
                    if (this.Photo1 == null)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }

            public Visibility Photo2IsSet
            {
                get
                {
                    if (this.Photo2 == null)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }

            public Visibility Photo3IsSet
            {
                get
                {
                    if (this.Photo3 == null)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }

            public Visibility Photo4IsSet
            {
                get
                {
                    if (this.Photo4 == null)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }

            public int PhotosCount
            {
                get
                {
                    return this.IsSet(this.Photo1) + this.IsSet(this.Photo2) + this.IsSet(this.Photo3) + this.IsSet(this.Photo4);
                }
            }

            public AlbumPhotoHeaderFourInARow(bool allowEdit, bool allowRemove)
            {
                this.AllowEdit = allowEdit;
                this.AllowRemove = allowRemove;
            }

            public AlbumPhotoHeaderFourInARow(IEnumerable<LunaVK.Core.DataObjects.VKPhoto> photos, IEnumerable<long> messageIds = null)
            {
                IEnumerator<LunaVK.Core.DataObjects.VKPhoto> enumerator = photos.GetEnumerator();
                List<long> longList = messageIds != null ? Enumerable.ToList<long>(messageIds) : null;
                if (enumerator.MoveNext())
                {
                    // ISSUE: explicit non-virtual call
                    this.Photo1 = new AlbumPhoto(enumerator.Current, longList != null ? longList[0] : 0L);
                }
                if (enumerator.MoveNext())
                {
                    // ISSUE: explicit non-virtual call
                    this.Photo2 = new AlbumPhoto(enumerator.Current, longList != null ? longList[1] : 0L);
                }
                if (enumerator.MoveNext())
                {
                    // ISSUE: explicit non-virtual call
                    this.Photo3 = new AlbumPhoto(enumerator.Current, longList != null ? longList[2] : 0L);
                }
                if (!enumerator.MoveNext())
                    return;
                // ISSUE: explicit non-virtual call
                this.Photo4 = new AlbumPhoto(enumerator.Current, longList != null ? longList[3] : 0L);
            }

            public AlbumPhotoHeaderFourInARow(IEnumerable<AlbumPhoto> photos)
            {
                IEnumerator<AlbumPhoto> enumerator = photos.GetEnumerator();
                if (enumerator.MoveNext())
                    this.Photo1 = enumerator.Current;
                if (enumerator.MoveNext())
                    this.Photo2 = enumerator.Current;
                if (enumerator.MoveNext())
                    this.Photo3 = enumerator.Current;
                if (!enumerator.MoveNext())
                    return;
                this.Photo4 = enumerator.Current;
            }

            public IEnumerable<LunaVK.Core.DataObjects.VKPhoto> GetAsPhotos()
            {
                if (this.Photo1 != null)
                    yield return this.Photo1.Photo;
                if (this.Photo2 != null)
                    yield return this.Photo2.Photo;
                if (this.Photo3 != null)
                    yield return this.Photo3.Photo;
                if (this.Photo4 != null)
                    yield return this.Photo4.Photo;
            }

            public IEnumerable<AlbumPhoto> GetAsAlbumPhotos()
            {
                if (this.Photo1 != null)
                    yield return this.Photo1;
                if (this.Photo2 != null)
                    yield return this.Photo2;
                if (this.Photo3 != null)
                    yield return this.Photo3;
                if (this.Photo4 != null)
                    yield return this.Photo4;
            }

            private int IsSet(AlbumPhoto obj)
            {
                return obj == null || obj.Photo != null && string.IsNullOrEmpty(obj.Src) ? 0 : 1;
            }

            public AlbumPhoto GetPhotoByTag(string tag)
            {
                AlbumPhoto albumPhoto = null;
                if (!(tag == "1"))
                {
                    if (!(tag == "2"))
                    {
                        if (!(tag == "3"))
                        {
                            if (tag == "4")
                                albumPhoto = this.Photo4;
                        }
                        else
                            albumPhoto = this.Photo3;
                    }
                    else
                        albumPhoto = this.Photo2;
                }
                else
                    albumPhoto = this.Photo1;
                return albumPhoto;
            }

            public int GetItemsCount()
            {
                return this.PhotosCount;
            }
        }
    }
}
