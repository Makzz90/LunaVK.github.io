using System;
using Windows.UI.Xaml.Media.Imaging;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;
using Windows.UI.Xaml;
using LunaVK.Core.ViewModels;
using LunaVK.Core.Framework;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace LunaVK.Core.Library
{
    public class OutboundPhotoAttachment : ViewModelBase, IOutboundAttachment, ThumbnailsLayoutHelper.IThumbnailSupport
    {
        public VKPhoto _photo;
        private StorageFile _sf;
        public uint MediaId;
        public bool IsForWallPost;
        public int _userOrGroupId;
        public string token;

        public bool IsUploadAttachment
        {
            get { return true; }
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

                base.NotifyPropertyChanged("UploadProgress");
            }
        }

        public /*override*/ Visibility IsFailedUploadVisibility
        {
            get
            {
                if (this._uploadState != OutboundAttachmentUploadState.Failed)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        private string _localUri;

        private uint _width;
        private uint _height;
        /*
        public string FileName
        {
            get
            {
                if (this._photo != null)
                    return "Image";
                return this.sf.Name;
            }
        }
        */
        
        
        private double _uploadProgress;
        public double UploadProgress
        {
            get
            {
                return this._uploadProgress;
            }
            set
            {
                //Это нужно для вложения в поле ввода текста
                this._uploadProgress = value;
                //Execute.ExecuteOnUIThread(() =>
                //{
                    base.NotifyPropertyChanged();
                //});
            }
        }
        
        public void Upload(Action completionCallback, Action<double> progressCallback = null)
        {
            if (this._photo != null)
            {
                this.UploadState = OutboundAttachmentUploadState.Completed;
                this.MediaId = this._photo.id;
                //this.ImgWidth = this._photo.TrueWidth;
                //this.ImgHeight = this._photo.TrueHeight;
                completionCallback();
                return;
            }

            this.UploadState = OutboundAttachmentUploadState.Uploading;
            DocumentsService.Instance.ReadFully(this._sf, (bytes) =>
            {
                /*
                DocumentsService.Instance.UploadPhoto(bytes, (photo, code) =>
                {
                    this._photo = photo;
                    this.MediaId = photo.id;
                    this.UploadState = code == Enums.VKErrors.None ? OutboundAttachmentUploadState.Completed : OutboundAttachmentUploadState.Failed;

                    completionCallback();
                }, (progress =>
                {
                    this.UploadProgress = progress;
                    progressCallback?.Invoke(progress);
                }));*/
                this.UploadImpl(bytes, () =>
                {
                    //if (this._uploadState == OutboundAttachmentUploadState.Completed && this._photo != null)
                    //{
                    //    this.CleanupCache();
                    //    this._localUri = this._photo.src_big;
                    //}
                    completionCallback?.Invoke();
                }, (progress =>
                {
                    this.UploadProgress = progress;
                    progressCallback?.Invoke(progress);
                }));
            });
        }

        private void UploadImpl(byte[] bytes, Action completionCallback, Action<double> progressCallback)
        {
            if (this.IsForWallPost)
            {
                //this.UploadToImpl((Action<byte[], Action<BackendResult<Photo, ResultCode>>, Action<double>>)((b, res, p, c) => PhotosService.Instance.UploadPhotoToWall(this._userOrGroupId, b, res, p, c)), bytes, completionCallback, progressCallback);
                PhotosService.Instance.UploadPhotoToWall(this._userOrGroupId, bytes, (photo) =>
                {
                    if(photo!=null)
                    {
                        this._photo = photo;
                        this.UploadState = OutboundAttachmentUploadState.Completed;
                        this.MediaId = photo.id;
                    }
                    else
                    {
                        this.UploadState = OutboundAttachmentUploadState.Failed;
                    }
                    completionCallback.Invoke();

                }, progressCallback);
            }
            else
            {
                //this.UploadToImpl(new Action<byte[], Action<BackendResult<Photo, ResultCode>>, Action<double>>(MessagesService.Instance.UploadPhoto), bytes, completionCallback, progressCallback);
                MessagesService.Instance.UploadPhoto(bytes, (photo) =>
                {
                    if (photo != null)
                    {
                        this._photo = photo;
                        this.UploadState = OutboundAttachmentUploadState.Completed;
                        this.MediaId = photo.id;
                    }
                    else
                    {
                        this.UploadState = OutboundAttachmentUploadState.Failed;
                    }
                    completionCallback.Invoke();

                }, progressCallback);
            }
        }
        /*
        private void UploadToImpl(Action uploadAction, byte[] bytes, Action completionCallback, Action<double> progressCallback)
        {
            //this._c = new Cancellation();
            this.UploadProgress = 0.0;
            uploadAction.Invoke(bytes, (res =>
            {
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    Photo resultData = res.ResultData;
                    //this._attachmentId = OutboundPhotoAttachment.ComposeAttachmentId(resultData.owner_id, resultData.pid, resultData.access_key);
                    this._photo = res.ResultData;
                   // MemoryStream memoryStream = new MemoryStream(bytes);
                    //ImageCache.Current.TrySetImageForUri(this._localUri, (Stream)memoryStream);
                    //memoryStream.Close();
                    this.UploadState = OutboundAttachmentUploadState.Completed;
                    completionCallback.Invoke();
                }
                else
                {
                    //Logger.Instance.Info("!!!!!!!!!!!!FAILED TO UPLOAD", new object[0]);
                    this.UploadState = OutboundAttachmentUploadState.Failed;
                    if (this._retryFlag)
                    {
                        this._retryFlag = false;
                        this.Upload(completionCallback, null);
                    }
                    else
                        completionCallback.Invoke();
                }
            }), (Action<double>)(progress =>
            {
                this.UploadProgress = progress;
                if (progressCallback == null)
                    return;
                progressCallback(progress);
            }));
        }*/

        public Visibility IsUploadingVisibility
        {
            get
            {
                if (this.UploadState != OutboundAttachmentUploadState.Uploading)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public static async Task<OutboundPhotoAttachment> CreateForUploadNewPhoto(StorageFile file, int userOrGroupId = 0, bool forPost = false)
        {
            var properties = await file.Properties.GetImagePropertiesAsync();
            
            OutboundPhotoAttachment outboundPhotoAttachment = new OutboundPhotoAttachment();
            outboundPhotoAttachment._userOrGroupId = userOrGroupId;
            outboundPhotoAttachment.UploadState = OutboundAttachmentUploadState.NotStarted;
            outboundPhotoAttachment.IsForWallPost = forPost;
            outboundPhotoAttachment._width = properties.Width;
            outboundPhotoAttachment._height = properties.Height;
            outboundPhotoAttachment._localUri = file.Path;
            outboundPhotoAttachment._sf = file;

            BitmapImage bimg = new BitmapImage();

            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                bimg.SetSource(stream);
            }
            outboundPhotoAttachment._imageSrc = bimg;
            /*
            outboundPhotoAttachment._isForUpload = true;
            Guid guid = Guid.NewGuid();
            outboundPhotoAttachment._localUri = string.Concat("/", guid.ToString());
            if (!ImageCache.Current.TrySetImageForUri(outboundPhotoAttachment.LocalUrlBig, stream))
            {
                stream.Close();
                previewStream.Close();
                throw new Exception("Failed to save local attachment");
            }
            if (previewStream != null)
            {
                if (!ImageCache.Current.TrySetImageForUri(outboundPhotoAttachment._localUri, previewStream))
                {
                    previewStream.Close();
                    throw new Exception("Failed to save local attachment");
                }
                previewStream.Close();
            }
            else
            {
                stream.Position = 0L;
                ImageCache.Current.TrySetImageForUri(outboundPhotoAttachment._localUri, stream);
            }
            stream.Close();
            */
            return outboundPhotoAttachment;
        }

        public static OutboundPhotoAttachment CreateForChoosingExistingPhoto(VKPhoto photo, int userOrGroupId = 0, bool forPost = true)
        {
            OutboundPhotoAttachment outboundPhotoAttachment = new OutboundPhotoAttachment();
            outboundPhotoAttachment._userOrGroupId = userOrGroupId;
            
            //outboundPhotoAttachment._attachmentId = OutboundPhotoAttachment.ComposeAttachmentId(photo.owner_id, photo.pid, photo.access_key);
            //outboundPhotoAttachment._localUri = srcBig;
            outboundPhotoAttachment.UploadState = OutboundAttachmentUploadState.Completed;
            outboundPhotoAttachment.IsForWallPost = forPost;
            outboundPhotoAttachment._photo = photo;
            
            return outboundPhotoAttachment;
        }
        /*
        private static string ComposeAttachmentId(int ownerId, int id, string accessKey)
        {
            string str0 = string.Format("photo{0}_{1}", ownerId, id);
            if (ownerId != Settings.UserId && !string.IsNullOrEmpty(accessKey))
                str0 = string.Concat(str0, string.Format("_{0}", accessKey));
            return str0;
        }
        */
        public VKAttachment GetAttachment()
        {
            if (this._sf != null)
                this._localUri = this._sf.Path;

            if (this._photo == null)
            {
                return new VKAttachment()
                {
                    type = Enums.VKAttachmentType.Photo,
                    photo = new VKPhoto()
                    {
                        photo_2560 = string.IsNullOrEmpty( this.token) ? this._localUri : this.token,//this._localUri,
                        date = DateTime.Now,
                        owner_id = (int)Settings.UserId,
                        user_id = (int)Settings.UserId,
                        album_id = -3,
                        can_comment = false,
                        width = (int)this._width,
                        height = (int)this._height,
                        likes = new VKLikes()
                        {
                            can_publish = false,
                            can_like = false
                        },
                        comments = new VKComments()
                        {
                            can_post = false
                        }
                    }
                };
            }

            VKAttachment attachment = new VKAttachment();
            attachment.type = Enums.VKAttachmentType.Photo;
            attachment.photo = this._photo;
            return attachment;
        }

        public override string ToString()
        {
            if (this._photo == null)
                return null;

            int ownerId = this._photo.owner_id;
            uint id = this._photo.id;
            string accessKey = this._photo.access_key;
            string str = string.Format("{0}{1}_{2}", "photo", ownerId, this._photo.id);
            if (ownerId != Settings.UserId && !string.IsNullOrEmpty(accessKey))
                str += string.Format("_{0}", accessKey);
            return str;
        }

        #region VM
        private BitmapImage _imageSrc;
        public BitmapImage ImageSrc
        {
            get
            {
                if (this._imageSrc != null)
                    return _imageSrc;

                string temp = "";

                if (string.IsNullOrEmpty(this._localUri))
                {
                    if (this._photo != null)
                    {
                        temp = this._photo.photo_2560;
                    }
                }
                else
                {
                    temp = this._localUri;
                }

                if (string.IsNullOrEmpty(temp))
                    return null;

                return new BitmapImage(new Uri(temp));
            }
        }
#endregion

#region IThumbnailSupport
        /// <summary>
        /// Данные для визуализации миниатюры.
        /// </summary>
        public ThumbnailsLayoutHelper.ThumbnailSize ThumbnailSize { get; set; }

        /// <summary>
        /// Ширнина исходного изображения.
        /// </summary>
        double ThumbnailsLayoutHelper.IThumbnailSupport.Width { get { return this._width; } }

        /// <summary>
        /// Высота исходного изображения.
        /// </summary>
        double ThumbnailsLayoutHelper.IThumbnailSupport.Height { get { return this._height; } }

        /// <summary>
        /// Источник изображения миниатюры.
        /// </summary>
        public string ThumbnailSource
        {
            get
            {
                return this._sf.Path;
            }
        }

        /// <summary>
        /// Возвращает соотношение ширины к высоте исходного изображения.
        /// </summary>
        public double GetRatio()
        {
            return (double)this._width / (double)this._height;
        }
#endregion

        public void Write(BinaryWriter writer)
        {
            writer.Write(3);
            writer.WriteString(this._localUri);
            writer.Write(this._userOrGroupId);
            writer.Write((byte)this._uploadState);
            //writer.WriteString(this._attachmentId);
            writer.Write(this.IsForWallPost);//writer.Write((int)this._postType);
            writer.Write(this._photo);
            //writer.Write(this._isForUpload);

            writer.Write(this._width);
            writer.Write(this._height);
        }

        public void Read(BinaryReader reader)
        {
            int num1 = reader.ReadInt32();
            this._localUri = reader.ReadString();
            this._userOrGroupId = reader.ReadInt32();
            this.UploadState = (OutboundAttachmentUploadState)reader.ReadByte();
            //this._attachmentId = reader.ReadString();
            this.IsForWallPost = reader.ReadBoolean();//this._postType = (PostType)reader.ReadInt32();
            this._photo = reader.ReadGeneric<VKPhoto>();
            if (this._uploadState == OutboundAttachmentUploadState.Uploading)
                this.UploadState = OutboundAttachmentUploadState.Failed;
            //this._isForUpload = reader.ReadBoolean();

            this._width = reader.ReadUInt32();
            this._height = reader.ReadUInt32();

            if (!string.IsNullOrEmpty(this._localUri))
                this.RestoreStorageFile();
        }

        private async void RestoreStorageFile()
        {
            try
            {
                this._sf = await StorageFile.GetFileFromPathAsync(this._localUri);
            }
            catch (Exception ex)
            {
                int i = 0;
            }
            //base.NotifyPropertyChanged(nameof(this.ImageSrc));
        }
    }
}
