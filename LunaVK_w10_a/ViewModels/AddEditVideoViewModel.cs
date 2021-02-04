using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using LunaVK.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace LunaVK.ViewModels
{
    public class AddEditVideoViewModel : ViewModelBase
    {
        private bool _isInEditMode;
        private uint _videoId;
        private int _ownerId;
        private string _filePath;
        private string _name;
        private string _description;
        private bool _autoReplay;
        private bool _isSaving;
        private double _progress;
        private StorageFile _sf;
        //private AccessType _accessType;
        //private AccessType _accessTypeComments;
        private string _localThumbPath;
        public static StorageFile PickedExternalFile;
        private EditPrivacyViewModel _viewVideoPricacyVM;
        private EditPrivacyViewModel _commentVideoPrivacyVM;

        public CancellationTokenSource C { get; private set; }

        public string LocalThumbPath
        {
            get
            {
                return this._localThumbPath;
            }
        }

        public EditPrivacyViewModel ViewVideoPrivacyVM
        {
            get
            {
                return this._viewVideoPricacyVM;
            }
            set
            {
                this._viewVideoPricacyVM = value;
                base.NotifyPropertyChanged();
            }
        }

        public EditPrivacyViewModel CommentVideoPrivacyVM
        {
            get
            {
                return this._commentVideoPrivacyVM;
            }
            set
            {
                this._commentVideoPrivacyVM = value;
                base.NotifyPropertyChanged();
            }
        }

        public bool IsSaving
        {
            get
            {
                return this._isSaving;
            }
            private set
            {
                this._isSaving = value;
                this.SetInProgress(value, "");
                this.NotifyPropertyChanged<bool>((() => this.IsSaving));
                this.NotifyPropertyChanged<bool>((() => this.CanEdit));
                this.NotifyPropertyChanged<Visibility>((() => this.IsUploadingVisibility));
            }
        }

        public bool CanEdit
        {
            get
            {
                return !this.IsSaving;
            }
        }

        public Visibility IsUploadingVisibility
        {
            get
            {
                if (!this._isSaving || this._isInEditMode)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public double Progress
        {
            get
            {
                return this._progress;
            }
            private set
            {
                this._progress = value;
                this.NotifyPropertyChanged<double>((() => this.Progress));
            }
        }

        public Visibility IsUserVideo
        {
            get
            {
                if (this._ownerId <= 0L)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public string Title
        {
            get
            {
                if (!this._isInEditMode)
                    return "AddEditVideo_Add";
                return "AddEditVideo_Edit";
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
                base.NotifyPropertyChanged();
            }
        }

        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
                base.NotifyPropertyChanged();
            }
        }

        public bool AutoReplay
        {
            get
            {
                return this._autoReplay;
            }
            set
            {
                this._autoReplay = value;
                base.NotifyPropertyChanged();
            }
        }

        private AddEditVideoViewModel()
        {
        }

        public static AddEditVideoViewModel CreateForNewVideo(string filePath, int ownerId)
        {
            AddEditVideoViewModel editVideoViewModel = new AddEditVideoViewModel();
            editVideoViewModel._ownerId = ownerId;
            editVideoViewModel._filePath = filePath;
            editVideoViewModel.ViewVideoPrivacyVM = new EditPrivacyViewModel(new PrivacySettingItem(new PrivacySetting() { title = "AddEditVideo_WhoCanView" })); //new EditPrivacyViewModel(CommonResources.AddEditVideo_WhoCanView, new PrivacyInfo(), "", null);
            editVideoViewModel.CommentVideoPrivacyVM = new EditPrivacyViewModel(new PrivacySettingItem(new PrivacySetting() { title = "AddEditVideo_WhoCanComment" })); //new EditPrivacyViewModel(CommonResources.AddEditVideo_WhoCanComment, new PrivacyInfo(), "", null);
            editVideoViewModel.PrepareVideo();
            return editVideoViewModel;
        }

        public static AddEditVideoViewModel CreateForEditVideo(int ownerId, uint videoId, VKVideoBase video = null)
        {
            AddEditVideoViewModel vm = new AddEditVideoViewModel();
            vm._ownerId = ownerId;
            vm._videoId = videoId;
            vm._isInEditMode = true;
            if (video != null)
            {
                vm.InitializeWithVideo(video);
            }
            else
            {
                VideoService.Instance.GetVideoById(ownerId, videoId, "", (result)=>
                {
                    if (result.error.error_code == Core.Enums.VKErrors.None)
                    {
                        Execute.ExecuteOnUIThread(delegate
                        {
                            vm.InitializeWithVideo(result.response[0]);
                        });
                    }
                });
            }
            return vm;
        }

        private void InitializeWithVideo(VKVideoBase video)
        {
            this.Name = video.title;
            this.Description = video.description;
            this.ViewVideoPrivacyVM = new EditPrivacyViewModel(new PrivacySettingItem(new PrivacySetting() { title = "AddEditVideo_WhoCanView" }));//new EditPrivacyViewModel(CommonResources.AddEditVideo_WhoCanView, video.PrivacyViewInfo, "", null);
            this.CommentVideoPrivacyVM = new EditPrivacyViewModel(new PrivacySettingItem(new PrivacySetting() { title = "AddEditVideo_WhoCanComment" }));//new EditPrivacyViewModel(CommonResources.AddEditVideo_WhoCanComment, video.PrivacyCommentInfo, "", null);
            this._localThumbPath = video.photo_320;
        }

        private async void PrepareVideo()
        {
            try
            {
                if (this._filePath != "")
                {
                    AddEditVideoViewModel addEditVideoViewModel = this;
                    StorageFile arg_5D_0 = addEditVideoViewModel._sf;
                    StorageFile sf = await StorageFile.GetFileFromPathAsync(this._filePath);
                    addEditVideoViewModel._sf = sf;
                    addEditVideoViewModel = null;
                }
                else
                {
                    this._sf = AddEditVideoViewModel.PickedExternalFile;
                }
                await this._sf.Properties.GetVideoPropertiesAsync();
                StorageItemThumbnail windowsRuntimeStream = await this._sf.GetThumbnailAsync(ThumbnailMode.VideosView);
                this._localThumbPath = "/" + Guid.NewGuid().ToString();
                ImageCache.Current.TrySetImageForUri(this._localThumbPath, windowsRuntimeStream.AsStream());
                this.NotifyPropertyChanged<string>(() => this.LocalThumbPath);
            }
            catch (Exception var_7_261)
            {
                Logger.Instance.Error("Failed to prepare video data", var_7_261);
            }
        }

        public async void Save(Action<bool> resultCallback)
        {
            if (!this._isSaving)
            {
                this.IsSaving = true;
                if (!this._isInEditMode)
                {
                    try
                    {
                        if (this._sf == null)
                        {
                            if (this._filePath != "")
                            {
                                AddEditVideoViewModel addEditVideoViewModel = this;
                                StorageFile arg_B8_0 = addEditVideoViewModel._sf;
                                StorageFile sf = await StorageFile.GetFileFromPathAsync(this._filePath);
                                addEditVideoViewModel._sf = sf;
                                addEditVideoViewModel = null;
                            }
                            else
                            {
                                this._sf = AddEditVideoViewModel.PickedExternalFile;
                                AddEditVideoViewModel.PickedExternalFile = null;
                            }
                        }
                        var stream = (await this._sf.OpenAsync(FileAccessMode.Read));


                        byte[] fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }



                        this.C = new CancellationTokenSource();
                        VideoService.Instance.UploadVideo(fileBytes, false, 0, this._ownerId, this.Name, this.Description, (result, error)=>
                        {
                            this.IsSaving = false;
                            if (error == Core.Enums.VKErrors.None)
                            {
                                /*
                                EventAggregator.Current.Publish(new VideoAddedDeleted
                                {
                                    IsAdded = true,
                                    VideoId = res.ResultData.video_id,
                                    OwnerId = res.ResultData.owner_id
                                });
                                */
                                resultCallback.Invoke(true);
                                return;
                            }
                            this.Progress = 0.0;
                            resultCallback.Invoke(false);
                        }, (progress) =>
                        {
                            this.Progress = progress;
                        }, this.C.Token, /*this.ViewVideoPrivacyVM.GetAsPrivacyInfo()*/null, /*this.CommentVideoPrivacyVM.GetAsPrivacyInfo()*/null);
                        return;
                    }
                    catch (Exception)
                    {
                        this.IsSaving = false;
                        resultCallback.Invoke(false);
                        return;
                    }
                }

                VideoService.Instance.EditVideo(this._videoId, this._ownerId, this.Name, this.Description, /*this.ViewVideoPrivacyVM.GetAsPrivacyInfo()*/null, /*this.CommentVideoPrivacyVM.GetAsPrivacyInfo()*/null, (result)=>
                {
                    this.IsSaving = false;
                    if (result.error.error_code == Core.Enums.VKErrors.None)
                    {
                        this.FireEditedEvent();
                        resultCallback.Invoke(true);
                        return;
                    }
                    resultCallback.Invoke(false);
                });
            
            }
        }


        private void FireEditedEvent()
        {
            VKVideoBase basedOnCurrentState = this.CreateVideoBasedOnCurrentState();
            /*
            EventAggregator.Current.Publish(new VideoEdited()
            {
                Video = basedOnCurrentState
            });
            */
        }

        private VKVideoBase CreateVideoBasedOnCurrentState()
        {
            return new VKVideoBase()
            {
                id = this._videoId,
                owner_id = this._ownerId,
                title = this.Name,
                description = this.Description,
                //privacy_view = this.ViewVideoPrivacyVM.GetAsPrivacyInfo().ToStringList(),
                //privacy_comment = this.CommentVideoPrivacyVM.GetAsPrivacyInfo().ToStringList()
            };
        }

        public void Cancel()
        {
            if (this.C == null)
                return;
            this.C.Cancel(true);
        }
    }
}
