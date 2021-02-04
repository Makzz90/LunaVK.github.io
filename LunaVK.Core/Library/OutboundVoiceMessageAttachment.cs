using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using Windows.Storage.Streams;
using System.IO;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;

namespace LunaVK.Core.Library
{
    public class OutboundVoiceMessageAttachment : IOutboundAttachment
    {
        private StorageFile _file;
        private List<int> _waveform;
        private string _filePath;
        private int _duration;
        private DocPreview.DocPreviewVoiceMessage _savedDoc;

        public OutboundAttachmentUploadState UploadState { get; set; }

        public OutboundVoiceMessageAttachment(StorageFile file, int duration, List<int> waveform)
        {
            this._file = file;
            this._filePath = file.Path;
            this._duration = duration;
            this._waveform = waveform;
        }

        public OutboundVoiceMessageAttachment()
        {
        }

        public void Upload(Action completionCallback, Action<double> progressCallback = null)
        {
            DocumentsService.Instance.ReadFully(this._file, (bytes) =>
            {
                DocumentsService.Instance.UploadVoiceMessageDocument(bytes, this._waveform, (doc) =>
                {
                    this.UploadState = doc!=null ? OutboundAttachmentUploadState.Completed : OutboundAttachmentUploadState.Failed;

                    this._savedDoc = doc.audio_message;
                    completionCallback();
                }, progressCallback);
            });
        }
        
        public bool IsUploadAttachment
        {
            get { return true; }
        }

        public override string ToString()
        {
            uint owner = Settings.UserId;
            return string.Format("{0}{1}_{2}", "audio_message", owner, this._savedDoc == null ? 0 : this._savedDoc.id);//todo:было doc, а где правда?
        }

        public VKAttachment GetAttachment()
        {
            VKAttachment attachment = new VKAttachment();
            attachment.type = Enums.VKAttachmentType.Doc;
            VKDocument doc = new VKDocument() { type = Enums.VKDocumentType.AUDIO };
            doc.preview = new DocPreview();
            doc.preview.audio_msg = this._savedDoc ?? new DocPreview.DocPreviewVoiceMessage();
            attachment.doc = doc;
            return attachment;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.WriteString(this._filePath);
            writer.Write(this._duration);
            BinarySerializerExtensions.WriteList(writer, this._waveform);
            writer.Write((byte)this.UploadState);
            writer.Write<DocPreview.DocPreviewVoiceMessage>(this._savedDoc);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this._filePath = reader.ReadString();
            this._duration = reader.ReadInt32();
            this._waveform = BinarySerializerExtensions.ReadListInt(reader);
            this.UploadState = (OutboundAttachmentUploadState)reader.ReadByte();
            if (this.UploadState == OutboundAttachmentUploadState.Uploading)
                this.UploadState = OutboundAttachmentUploadState.Failed;
            this._savedDoc = reader.ReadGeneric<DocPreview.DocPreviewVoiceMessage>();
        }
    }
}
