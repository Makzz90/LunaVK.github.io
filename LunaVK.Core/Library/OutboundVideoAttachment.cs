using System;
using System.IO;
using System.IO.IsolatedStorage;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.Core.Library
{
    public class OutboundVideoAttachment : ViewModelBase, IOutboundAttachment
    {
        VKVideoBase video;
        public Windows.Storage.StorageFile _sf;
        private bool _isPrivate;
        private int _groupId;
        private string _localUri;
        private TimeSpan _duration;

        public BitmapImage ResourceUri { get; set; }
        private string _localThumbPath;

        public bool IsUploadAttachment
        {
            get { return true; }
        }

        private double _uploadProgress;
        public double UploadProgress
        {
            get
            {
                return this._uploadProgress;
            }
            set
            {
                this._uploadProgress = value;
                base.NotifyPropertyChanged();
            }
        }

        public Visibility IsUploadingVisibility
        {
            get
            {
                if (this.UploadState != OutboundAttachmentUploadState.Uploading)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility IsFailedUploadVisibility
        {
            get
            {
                if (this._uploadState != OutboundAttachmentUploadState.Failed)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        private OutboundAttachmentUploadState _uploadState;
        public OutboundAttachmentUploadState UploadState
        {
            get { return this._uploadState; }
            set
            {
                this._uploadState = value;
                base.NotifyPropertyChanged(nameof(this.UploadState));
                base.NotifyPropertyChanged(nameof(this.IsUploadingVisibility));
                base.NotifyPropertyChanged(nameof(this.IsFailedUploadVisibility));
            }
        }



        //OutboundUploadVideoAttachment
        public OutboundVideoAttachment(StorageFile file, bool isPrivate = true, int groupId = 0)
        {
            this._sf = file;
            this._localUri = file.Path;
            this._isPrivate = isPrivate;
            this._groupId = groupId;
            this.PrepareThumbnail();
        }

        public OutboundVideoAttachment(VKVideoBase video)
        {
            this.UploadState = OutboundAttachmentUploadState.Completed;
            this.video = video;
            this.ResourceUri = new BitmapImage(new Uri(video.ImageUri));
        }

        public OutboundVideoAttachment()
        {
        }

        private async void PrepareThumbnail()
        {
            try
            {
                this._duration = (await this._sf.Properties.GetVideoPropertiesAsync()).Duration;
                var thumbnailAsync = await this._sf.GetThumbnailAsync( Windows.Storage.FileProperties.ThumbnailMode.VideosView,190);

                if (thumbnailAsync != null)
                {
                    this.ResourceUri = new BitmapImage();
                    await this.ResourceUri.SetSourceAsync(thumbnailAsync);
                    //ImageCache.Current.TrySetImageForUri(this._localThumbPath, thumbnailAsync.AsStream());

                    base.NotifyPropertyChanged(nameof(this.ResourceUri));
                }

                //this._guid = Guid.NewGuid();
                //this._localThumbPath = "/" + (object)this._guid;
                //ImageCache.Current.TrySetImageForUri(this._localThumbPath, ((IRandomAccessStream)thumbnailAsync).AsStream());
                //this.NotifyPropertyChanged("ResourceUri");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Failed to prepare video data", ex);
            }
        }

        public override string ToString()
        {
            if (this.video == null)
                return null;

            int ownerId = this.video.owner_id;
            uint id = this.video.id;
            string accessKey = this.video.access_key;
            string str = string.Format("{0}{1}_{2}", "video", ownerId, this.video.id);
            if (ownerId != Settings.UserId && !string.IsNullOrEmpty(accessKey))
                str += string.Format("_{0}", accessKey);
            return str;
        }

        public VKAttachment GetAttachment()
        {
            if(this.video==null)
            {
                this.video = new VKVideoBase()
                {
                    //photo_320 = this.ResourceUri.UriSource.AbsoluteUri,
                    duration = this._duration
                };
            }
            VKAttachment attachment = new VKAttachment();
            attachment.type = Enums.VKAttachmentType.Video;
            attachment.video = this.video;
            return attachment;
        }

        public void Upload(Action completionCallback, Action<double> progressCallback = null)
        {
            if (this.video != null && this._sf == null)
            {
                this.UploadState = OutboundAttachmentUploadState.Completed;
                //this.MediaId = this._photo.id;
                completionCallback();
                return;
            }


            if (this.UploadState == OutboundAttachmentUploadState.Completed)
            {
                completionCallback();
            }
            else
            {
                this.UploadState = OutboundAttachmentUploadState.Uploading;

                DocumentsService.Instance.ReadFully(this._sf, (bytes) =>
                {
                    VideoService.Instance.UploadVideo(bytes, this._isPrivate, 0, this._groupId, "", "", (response, code) =>
                    {
                        this.UploadState = code == Enums.VKErrors.None ? OutboundAttachmentUploadState.Completed : OutboundAttachmentUploadState.Failed;

                        video = new VKVideoBase();
                        video.id = response.video_id;
                        video.owner_id = response.owner_id;
                        completionCallback();
                    }, (progress =>
                    {
                        this.UploadProgress = progress;
                        progressCallback?.Invoke(progress);
                    }));
                });
            }

        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(2);
            writer.Write((byte)this.UploadState);
            writer.Write<VKVideoBase>(this.video);

            writer.WriteString(this._localUri);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.UploadState = (OutboundAttachmentUploadState)reader.ReadByte();
            this.video = reader.ReadGeneric<VKVideoBase>();

            this._localUri = reader.ReadString();

            if (!string.IsNullOrEmpty(this._localUri))
                this.RestoreStorageFile();
            else
            {
                this.ResourceUri = new BitmapImage(new Uri(this.video.ImageUri));
                base.NotifyPropertyChanged(nameof(this.ResourceUri));
            }
        }

        private async void RestoreStorageFile()
        {
            this._sf = await StorageFile.GetFileFromPathAsync(this._localUri);
            this.PrepareThumbnail();
            //base.NotifyPropertyChanged(nameof(this.ImageSrc));
        }
    }
}
