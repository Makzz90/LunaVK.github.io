using System;
using System.Collections.Generic;
using System.Text;
using LunaVK.Core.Library;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using LunaVK.Core.ViewModels;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using LunaVK.Framework;
using System.Linq;
using Windows.Storage.Search;
using LunaVK.Core.Enums;
using System.Diagnostics;
using Windows.System.Threading;
using Windows.Foundation;
using LunaVK.Core.Framework;

namespace LunaVK.ViewModels
{
    public class PhotoPickerAlbumsViewModel : ViewModelBase, ISupportUpDownIncrementalLoading
    {
        public ObservableCollection<AlbumPhoto> Photos { get; private set; }
        public ObservableCollection<NamedFolder> Folders { get; private set; }
        public StorageFolder CurFolder;

        public List<AlbumPhoto> SelectedPhotos = new List<AlbumPhoto>();

        public PhotoPickerAlbumsViewModel()
        {
            if (CustomFrame.Instance == null)
                return;

            this.Photos = new ObservableCollection<AlbumPhoto>();
            this.Folders = new ObservableCollection<NamedFolder>();
            this.CurFolder = KnownFolders.PicturesLibrary;
        }

        public Action<ProfileLoadingStatus> LoadingStatusUpdated { get; set; }

        public async Task LoadUpAsync()
        {
            throw new NotImplementedException();
        }

        public bool HasMoreUpItems
        {
            get { return false; }
        }

        public string StatusText
        {
            get { return String.Empty; }
        }

        public string FooterText
        {
            get { return String.Empty; }
        }

        private bool _hasMoreItems = true;
        public bool HasMoreDownItems { get { return this._hasMoreItems; } }

        List<Task<StorageItemThumbnail>> thumbnailOperations = new List<Task<StorageItemThumbnail>>();

        public async Task<object> Reload()
        {
            this.Photos.Clear();
            this.Folders.Clear();
            this._hasMoreItems = true;

            this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Reloading);
            await LoadDownAsync(true);
            return null;
        }

        public async Task LoadDownAsync(bool InReload = false)
        {
            if (CustomFrame.Instance == null)
                return;

            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //StorageFolderQueryResult queryResult = this.CurFolder.CreateFolderQuery(CommonFolderQuery.GroupByMonth);

            //IReadOnlyList<StorageFolder> folderList = await queryResult.GetFoldersAsync();
            
            //Debug.WriteLine("GetFoldersAsync: " + sw.ElapsedMilliseconds);
            //sw.Restart();

            var queryOptions = new QueryOptions { FolderDepth = FolderDepth.Deep, IndexerOption = IndexerOption.UseIndexerWhenAvailable };
            queryOptions.FileTypeFilter.Add(".jpg");
            queryOptions.FileTypeFilter.Add(".png");
            queryOptions.FileTypeFilter.Add(".mp4");
            
            queryOptions.SortOrder.Clear(); // clear all existing sorts
            
            SortEntry se = new SortEntry() { PropertyName = "System.DateModified", AscendingOrder = false };
            queryOptions.SortOrder.Add(se); // add descending sort by date modified

            StorageFileQueryResult query = this.CurFolder.CreateFileQueryWithOptions(queryOptions);
            var fileList = await query.GetFilesAsync((uint)this.Photos.Count,40);

            this._hasMoreItems = fileList.Count == 40;

            //Debug.WriteLine("query.GetFilesAsync: " + fileList.Count + " " + sw.ElapsedMilliseconds);
            //sw.Restart();
            
            //foreach (StorageFile item in fileList.OrderBy(a => a.DateCreated))
            foreach (StorageFile file in fileList)
            {

                //Допустимые форматы: JPG, PNG, GIF.
                //Допустимые форматы: AVI, MP4, 3GP, MPEG, MOV, MP3, FLV, WMV.
                //Допустимые форматы: любые форматы за исключением mp3 и исполняемых файлов. 
                if (!file.ContentType.StartsWith("image") && !file.ContentType.StartsWith("video"))
                    continue;

                if (file.Attributes.HasFlag(FileAttributes.LocallyIncomplete))
                    continue;


                
                    //ImageProperties imgProps = await file.Properties.GetImagePropertiesAsync();

                    //var task = file.GetThumbnailAsync(ThumbnailMode.PicturesView, 50).AsTask();
                //task.ContinueWith(this.Continue).GetAwaiter();
                //thumbnailOperations.Add(task);


                AlbumPhoto photo = new AlbumPhoto();
                //photo.ThumbnailTask = task;
                photo.sf = file;
                //photo.id = i;
                //i++;
                this.Photos.Add(photo);
            }

            //Debug.WriteLine("this.Photos.Add: " + sw.ElapsedMilliseconds);
            //sw.Stop();




            


            var folders = await this.CurFolder.GetFoldersAsync();
            foreach (var f in folders)
            {
                if (f.Name == "Sample Pictures")
                    continue;

                StorageFolder to_add;

                if (f.Name == "Camera Roll")
                    to_add = KnownFolders.CameraRoll;
                else if (f.Name == "Saved Pictures")
                    to_add = KnownFolders.SavedPictures;
                else
                    to_add = f;

                if (this.Folders.FirstOrDefault((ff) => ff.Folder.Path == to_add.Path) == null)
                    this.Folders.Add(new NamedFolder(to_add) /*{ th = thumb }*/);
            }
        }






        // Verify queries are supported because they are not supported in all picked locations.
         //       if (folder.IsCommonFolderQuerySupported(monthShape))





        

        public int Attached
        {
            get
            {
                //int ret = 0;
                //foreach (var p in this.Photos)
                //{
                //    if (p.IsSelected)
                //        ret++;
                //}

                //return ret;
                return this.SelectedPhotos.Count;
            }
        }

        public void UpdateUI()
        {
            base.NotifyPropertyChanged(nameof(this.Attached));
            base.NotifyPropertyChanged(nameof(this.CancelAttachVisibility));
            base.NotifyPropertyChanged(nameof(this.AttachVisibility));
        }

        public Visibility CancelAttachVisibility
        {
            get
            {
                return this.Attached > 0 ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility AttachVisibility
        {
            get
            {
                return this.Attached == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public void SetNewsSource(StorageFolder value)
        {
            this.CurFolder = value;
            TaskScheduler2.Clear();
            this.Reload();
        }

        public class NamedFolder : ViewModelBase
        {
            public string FolderName { get { return Folder.DisplayName; } }
            public StorageFolder Folder;
            public StorageItemThumbnail th;

            private BitmapImage _bitmapImage { get; set; }
            public BitmapImage BitmapImage
            {
                get
                {
                    if (_bitmapImage == null)
                        TaskScheduler2.Add(this.Load);

                    return _bitmapImage;
                }
                set
                {
                    _bitmapImage = value;
                }
            }

            public Visibility SDCardVisibility
            {
                get
                {
                    if(!CustomFrame.Instance.IsDevicePhone)
                        return Visibility.Collapsed;
                    return this.Folder.Path.StartsWith("D:") ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            public async Task Load()
            {
                _bitmapImage = new BitmapImage();
                if (th != null)
                {
                    await _bitmapImage.SetSourceAsync(th);
                    base.NotifyPropertyChanged("BitmapImage");
                }
            }

            public NamedFolder(StorageFolder folder)
            {
                this.Folder = folder;
            }
        }

        

        public class AlbumPhoto : ViewModelBase
        {
            private bool isFailed;
            private IAsyncAction m_workItem;
            //public Task<StorageItemThumbnail> ThumbnailTask { get; set; }

            private BitmapImage _bitmapImage { get; set; }
            public BitmapImage BitmapImage
            {
                get
                {
                    if(this.isFailed)
                    {
                        this.m_workItem = null;
                        this.isFailed = false;
                    }
                    //if (_bitmapImage == null)
                    //    TaskScheduler2.Add(this.Load);
                    if (this._bitmapImage == null && this.m_workItem == null)
                    {
                        this.Load2();
                        return null;
                    }

                    return this._bitmapImage;
                }
                set
                {
                    this._bitmapImage = value;
                    base.NotifyPropertyChanged("BitmapImage");
                }
            }

            public StorageFile sf;
            //public int id;

            private void Load2()
            {
                IAsyncAction asyncAction = ThreadPool.RunAsync(async (workItem) =>
                {
                    
                    try
                    {
                        var th = await this.sf.GetThumbnailAsync(ThumbnailMode.PicturesView, 200, ThumbnailOptions.None);
                        if (th != null)
                        {
                            Execute.ExecuteOnUIThread(async () =>
                            {
                                _bitmapImage = new BitmapImage();
                                await _bitmapImage.SetSourceAsync(th);
                                this.BitmapImage = _bitmapImage;
                                //base.NotifyPropertyChanged("BitmapImage");
                            });
                            //Execute.ExecuteOnUIThread(() =>
                            //{
                            //    _bitmapImage = new BitmapImage();
                            //    _bitmapImage.SetSource(th);
                            //});
                        }
                        else
                        {
                            this.isFailed = true;
                        }
                        //Debug.WriteLine(">" + id.ToString() + " " + (th == null ? "fail" : "done") );
                    }
                    catch
                    {
                        //Бывает картинки битые и превью не сделать :\
                    }
                }, WorkItemPriority.Low);

                this.m_workItem = asyncAction;

                // logic to print the result when the task is complete
                //this.m_workItem.Completed = new AsyncActionCompletedHandler( (IAsyncAction asyncInfo, AsyncStatus asyncStatus) =>
                //{
                //    base.NotifyPropertyChanged("BitmapImage");
                //    Debug.WriteLine("Notified>" + id.ToString());
                //});
            
        }

            //public int Width { get; set; }
            //public int Height { get; set; }

            public Action Loaded { get; set; }

            private bool _isSelected;
            public bool IsSelected
            {
                get
                {
                    return this._isSelected;
                }
                set
                {
                    this._isSelected = value;
                    base.NotifyPropertyChanged("IsSelected");
                }
            }

            private int _number;
            public int Number
            {
                get
                {
                    return this._number;
                }
                set
                {
                    this._number = value;
                    base.NotifyPropertyChanged("Number");
                }
            }

            public Visibility IsVideoVisibility
            {
                get
                {
                    return (this.sf != null && this.sf.ContentType.Contains("video")) ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            public async Task Load()
            {
                _bitmapImage = new BitmapImage();
                try
                {
                    var th = await this.sf.GetThumbnailAsync(ThumbnailMode.PicturesView, 100, ThumbnailOptions.None);
                    if (th != null)
                    {
                        await _bitmapImage.SetSourceAsync(th);
                        base.NotifyPropertyChanged("BitmapImage");
                    }
                }
                catch
                {
                    //Бывает картинки битые и превью не сделать :\
                }
                
            }
        }
    }
}
