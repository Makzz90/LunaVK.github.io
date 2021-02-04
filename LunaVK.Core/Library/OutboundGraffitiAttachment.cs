using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;

namespace LunaVK.Core.Library
{
    //GraffitiAttachmentItem
    public class OutboundGraffitiAttachment : IOutboundAttachment
    {
        public Windows.UI.Xaml.Media.Imaging.RenderTargetBitmap Data;
        public VKGraffiti Doc;
        public VKSticker Sticker;

        public bool IsUploadAttachment
        {
            get { return true; }
        }

        public OutboundAttachmentUploadState UploadState { get; set; }
        
        public override string ToString()
        {
            long ownerId = this.Doc.owner_id;
            long id = this.Doc.id;
            string str = string.Format("{0}{1}_{2}", "doc", ownerId, id);
            if (ownerId != Settings.UserId && !string.IsNullOrEmpty(this.Doc.access_key))
                str += string.Format("_{0}", this.Doc.access_key);
            return str;
        }

        public async void Upload(Action completionCallback, Action<double> progressCallback = null)
        {
            this.UploadState = OutboundAttachmentUploadState.Uploading;

            if (this.Data != null)
            {
                IBuffer pixels = await this.Data.GetPixelsAsync();
                IRandomAccessStream stream = new InMemoryRandomAccessStream();

                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                byte[] bytes = pixels.ToArray();

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                    (uint)this.Data.PixelWidth, (uint)this.Data.PixelHeight,
                    72, 72, bytes);

                await encoder.FlushAsync();

                var reader = new DataReader(stream.GetInputStreamAt(0));
                var bytes2 = new byte[stream.Size];
                await reader.LoadAsync((uint)stream.Size);
                reader.ReadBytes(bytes2);
                stream.Dispose();

                DocumentsService.Instance.UploadGraffitiDocument("graffiti", ".png", bytes2, (response) =>
                {
                    this.Doc = response.graffiti;
                    this.UploadState = response != null ? OutboundAttachmentUploadState.Completed : OutboundAttachmentUploadState.Failed;
                    completionCallback();
                }, progressCallback);
            }
            else
            {
                var refer = RandomAccessStreamReference.CreateFromUri(new Uri(Sticker.photo_256, UriKind.Absolute));
                IRandomAccessStreamWithContentType rstream = await refer.OpenReadAsync();
                
                var reader0 = new DataReader(rstream.GetInputStreamAt(0));
                var bytes3 = new byte[rstream.Size];
                await reader0.LoadAsync((uint)rstream.Size);
                reader0.ReadBytes(bytes3);
                rstream.Dispose();

                DocumentsService.Instance.UploadGraffitiDocument("graffiti", ".png", bytes3, (response) =>
                {
                    this.Doc = response.graffiti;
                    this.UploadState = response != null ? OutboundAttachmentUploadState.Completed : OutboundAttachmentUploadState.Failed;
                    completionCallback();
                }, progressCallback);
            }
        }

        public VKAttachment GetAttachment()
        {
            VKAttachment attachment = new VKAttachment();
            attachment.type = Enums.VKAttachmentType.Doc;

            VKDocument doc = new VKDocument();
            doc.preview = new DocPreview();
            doc.preview.graffiti = new DocPreview.DocPreviewGraffiti() { src = this.Doc.photo_586, width = this.Doc.width, height = this.Doc.height  };
            attachment.doc = doc;
            return attachment;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            //writer.WriteString(this._uri);
            //writer.Write(this._width);
            //writer.Write(this._height);
            writer.Write<VKGraffiti>(this.Doc);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            //this._uri = reader.ReadString();
            //this._width = reader.ReadInt32();
            //this._height = reader.ReadInt32();
            this.Doc = reader.ReadGeneric<VKGraffiti>();
        }
    }
}
