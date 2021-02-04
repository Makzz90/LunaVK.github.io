using System;
using System.Collections.Generic;
using System.Text;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media.Imaging;
using LunaVK.Core.Enums;
using System.IO;
using Windows.Storage.FileProperties;

namespace LunaVK.Core.Library
{
    //OutboundUploadDocumentAttachment
    public class OutboundDocumentAttachment : IOutboundAttachment
    {
        /// <summary>
        /// Документ с сервера
        /// </summary>
        public VKDocument _pickedDocument;
        public Windows.Storage.StorageFile _sf;
        public BitmapImage LocalUrl2 { get; set; }
        private VKDocument SavedDoc;
        private string _localThumbPath;

        public OutboundAttachmentUploadState UploadState { get; set; }

        public bool IsUploadAttachment
        {
            get { return this._sf != null; }
        }

        public string Title
        {
            get { return this._sf == null ? this._pickedDocument.title : this._sf.Name; }
        }

        public string IconSource
        {
            get { return "\xE8FF"; }
        }

        public string Subtitle
        {
            get
            {
                if (this._sf == null)
                    return UIStringFormatterHelper.BytesForUI(this._pickedDocument.size);

                return "";
            }
        }

        public override string ToString()
        {
            if (this._pickedDocument == null)
                return null;
            int ownerId = this._pickedDocument.owner_id;
            uint id = this._pickedDocument.id;
            string accessKey = this._pickedDocument.access_key;
            string str = string.Format("{0}{1}_{2}", "doc", ownerId, id);
            if (ownerId != Settings.UserId && !string.IsNullOrEmpty(accessKey))
                str += string.Format("_{0}", accessKey);
            return str;
        }
        /*
        public override Attachment GetAttachment()
        {
            Attachment attachment = new Attachment();
            attachment.type = "doc";
            Doc doc = this.SavedDoc ?? new Doc();
            attachment.doc = doc;
            return attachment;
        }
        */
        public VKAttachment GetAttachment()
        {
            VKAttachment attachment = new VKAttachment();
            attachment.type = VKAttachmentType.Doc;
            if (this.SavedDoc != null)
                attachment.doc = this.SavedDoc;
            else
                attachment.doc = this._pickedDocument ?? new VKDocument();
            return attachment;
        }

        public OutboundDocumentAttachment()
        {
        }

        public OutboundDocumentAttachment(VKDocument pickedDocument)
        {
            this._pickedDocument = pickedDocument;
            this.UploadState = OutboundAttachmentUploadState.Completed;
        }

        public OutboundDocumentAttachment(Windows.Storage.StorageFile file)
        {
            this._sf = file;
            this.PrepareThumbnail();
        }

        public void Upload(Action callback, Action<double> progressCallback = null)
        {
            if (this._sf == null)
            {
                callback();
                return;
            }

            this.UploadState = OutboundAttachmentUploadState.Uploading;

            DocumentsService.Instance.ReadFully(this._sf, (bytes) =>
            {
                DocumentsService.Instance.UploadDocument(0, this._sf.Name, this._sf.FileType, bytes, (response) =>
                {
                    this._pickedDocument = response.doc;

                    this.UploadState = response == null ? OutboundAttachmentUploadState.Failed : OutboundAttachmentUploadState.Completed;

                    callback();
                });
            });
        }

        

        private async void PrepareThumbnail()
        {
            try
            {
                VKDocument savedDoc = new VKDocument();
                BasicProperties basicPropertiesAsync = await this._sf.GetBasicPropertiesAsync();
                savedDoc.title = this._sf.Name;
                savedDoc.size = (int)basicPropertiesAsync.Size;
                string fileType = this._sf.FileType;
                
                if(this._sf.ContentType.Contains("image"))
                {
                    savedDoc.type = VKDocumentType.IMAGE;
                }
                else if (this._sf.ContentType.Contains("video"))
                {
                    savedDoc.type = VKDocumentType.VIDEO;
                }
                else if (this._sf.ContentType.Contains("audio"))
                {
                    savedDoc.type = VKDocumentType.AUDIO;
                }
                else if (this._sf.ContentType.Contains("text"))
                {
                    savedDoc.type = VKDocumentType.TEXT;
                }
                else
                {
                    savedDoc.type = VKDocumentType.UNKNOWN;
                }
                /*
                 * типы передаваемых данных:

    application;
    audio;
    example;
    image;
    message;
    model;
    multipart;
    text;
    video.
    */

                savedDoc.ext = fileType.StartsWith(".") ? fileType.Substring(1) : fileType;
                //savedDoc.guid = Guid.NewGuid();
                try
                {
                    StorageItemThumbnail thumbnailAsync = await this._sf.GetThumbnailAsync(ThumbnailMode.DocumentsView);
                    this._localThumbPath = "/" + Guid.NewGuid();
                    //ImageCache.Current.TrySetImageForUri(this._localThumbPath, (thumbnailAsync).AsStream());
                    //savedDoc.PreviewUri = this._localThumbPath;
                }
                catch
                {
                }
                this.SavedDoc = savedDoc;
                savedDoc = null;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Failed to prepare doc data", ex);
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            //writer.WriteString(this._filePath);
            writer.Write((byte)this.UploadState);
            //writer.WriteString(this._localThumbPath);
            writer.Write<VKDocument>(this._pickedDocument);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            //this._filePath = reader.ReadString();
            this.UploadState = (OutboundAttachmentUploadState)reader.ReadByte();
            //this._localThumbPath = reader.ReadString();
            if (this.UploadState == OutboundAttachmentUploadState.Uploading)
                this.UploadState = OutboundAttachmentUploadState.Failed;
            this._pickedDocument = reader.ReadGeneric<VKDocument>();
        }
    }
}
