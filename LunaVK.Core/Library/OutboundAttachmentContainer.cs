using LunaVK.Core.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunaVK.Core.Utils;

namespace LunaVK.Core.Library
{
    public class OutboundAttachmentContainer : IBinarySerializable
    {
        private OutboundGeoAttachment _geoAttachment;
        private OutboundPhotoAttachment _photoAttachment;
        private OutboundVideoAttachment _videoAttachment;
        private OutboundDocumentAttachment _documentAttachment;
        //private OutboundAudioAttachment _audioAttachment;
        //private OutboundUploadVideoAttachment _uploadVideoAttachment;
        private OutboundWallPostAttachment _wallPostAttachment;
        private OutboundForwardedMessages _forwardedMessages;
        //private OutboundUploadDocumentAttachment _uploadDocumentAttachment;
        //private OutboundTimerAttachment _timerAttachment;
        //private OutboundLinkAttachment _linkAttachment;
        //private OutboundProductAttachment _productAttachment;
        //private OutboundNoteAttachment _noteAttachment;
        //private OutboundMarketAlbumAttachment _marketAlbumAttachment;
        //private OutboundAlbumAttachment _albumAttachment;
        private OutboundVoiceMessageAttachment _voiceMessageAttachment;
        //private IOutboundAttachment _outboundAttachment;
        private OutboundPollAttachment _pollAttachment;

        public IOutboundAttachment OutboundAttachment { get; private set; }
        /*
        public bool IsGeoAttachment
        {
            get
            {
                return this._geoAttachment != null;
            }
        }
        */
        public OutboundAttachmentContainer(IOutboundAttachment outboundAttachment)
        {
            //this._outboundAttachment = outboundAttachment;
            if (outboundAttachment is OutboundPhotoAttachment)
                this._photoAttachment = outboundAttachment as OutboundPhotoAttachment;
            else if (outboundAttachment is OutboundGeoAttachment)
                this._geoAttachment = outboundAttachment as OutboundGeoAttachment;
            else if (outboundAttachment is OutboundVideoAttachment)
                this._videoAttachment = outboundAttachment as OutboundVideoAttachment;
            //else if (outboundAttachment is OutboundAudioAttachment)
            //    this._audioAttachment = outboundAttachment as OutboundAudioAttachment;
            else if (outboundAttachment is OutboundDocumentAttachment)
                this._documentAttachment = outboundAttachment as OutboundDocumentAttachment;
            //else if (outboundAttachment is OutboundUploadVideoAttachment)
            //    this._uploadVideoAttachment = outboundAttachment as OutboundUploadVideoAttachment;
            else if (outboundAttachment is OutboundWallPostAttachment)
                this._wallPostAttachment = outboundAttachment as OutboundWallPostAttachment;
            else if (outboundAttachment is OutboundForwardedMessages)
                this._forwardedMessages = outboundAttachment as OutboundForwardedMessages;
            //else if (outboundAttachment is OutboundUploadDocumentAttachment)
            //    this._uploadDocumentAttachment = outboundAttachment as OutboundUploadDocumentAttachment;
            else if (outboundAttachment is OutboundPollAttachment)
                this._pollAttachment = outboundAttachment as OutboundPollAttachment;
            //else if (outboundAttachment is OutboundTimerAttachment)
            //    this._timerAttachment = outboundAttachment as OutboundTimerAttachment;
            //else if (outboundAttachment is OutboundLinkAttachment)
            //    this._linkAttachment = outboundAttachment as OutboundLinkAttachment;
            //else if (outboundAttachment is OutboundProductAttachment)
            //    this._productAttachment = outboundAttachment as OutboundProductAttachment;
            //else if (outboundAttachment is OutboundNoteAttachment)
            //    this._noteAttachment = outboundAttachment as OutboundNoteAttachment;
            //else if (outboundAttachment is OutboundMarketAlbumAttachment)
            //    this._marketAlbumAttachment = outboundAttachment as OutboundMarketAlbumAttachment;
            //else if (outboundAttachment is OutboundAlbumAttachment)
            //{
            //    this._albumAttachment = outboundAttachment as OutboundAlbumAttachment;
            //}
            else if (outboundAttachment is OutboundVoiceMessageAttachment)
            {
                this._voiceMessageAttachment = outboundAttachment as OutboundVoiceMessageAttachment;
            }
            else
            {
                throw new Exception("Unknown attachment type");
            }
        }

        public OutboundAttachmentContainer()
        {
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(9);
            writer.Write<OutboundPhotoAttachment>(this._photoAttachment);
            writer.Write<OutboundGeoAttachment>(this._geoAttachment);
            writer.Write<OutboundVideoAttachment>(this._videoAttachment);
            //writer.Write<OutboundAudioAttachment>(this._audioAttachment);
            writer.Write<OutboundDocumentAttachment>(this._documentAttachment);
            //writer.Write<OutboundUploadVideoAttachment>(this._uploadVideoAttachment);
            writer.Write<OutboundWallPostAttachment>(this._wallPostAttachment);
            writer.Write<OutboundForwardedMessages>(this._forwardedMessages);
            //writer.Write<OutboundUploadDocumentAttachment>(this._uploadDocumentAttachment);
            writer.Write<OutboundPollAttachment>(this._pollAttachment);
            //writer.Write<OutboundTimerAttachment>(this._timerAttachment);
            //writer.Write<OutboundLinkAttachment>(this._linkAttachment);
            //writer.Write<OutboundProductAttachment>(this._productAttachment);
            //writer.Write<OutboundNoteAttachment>(this._noteAttachment);
            //writer.Write<OutboundMarketAlbumAttachment>(this._marketAlbumAttachment);
            //writer.Write<OutboundAlbumAttachment>(this._albumAttachment);
            writer.Write<OutboundVoiceMessageAttachment>(this._voiceMessageAttachment);
        }

        public void Read(BinaryReader reader)
        {
            int num1 = reader.ReadInt32();
            this._photoAttachment = reader.ReadGeneric<OutboundPhotoAttachment>();
            if (this._photoAttachment != null)
                this.OutboundAttachment = this._photoAttachment;
            this._geoAttachment = reader.ReadGeneric<OutboundGeoAttachment>();
            if (this._geoAttachment != null)
                this.OutboundAttachment = this._geoAttachment;
            this._videoAttachment = reader.ReadGeneric<OutboundVideoAttachment>();
            if (this._videoAttachment != null)
                this.OutboundAttachment = this._videoAttachment;
            //this._audioAttachment = reader.ReadGeneric<OutboundAudioAttachment>();
            //if (this._audioAttachment != null)
            //    this.OutboundAttachment = this._audioAttachment;
            this._documentAttachment = reader.ReadGeneric<OutboundDocumentAttachment>();
            if (this._documentAttachment != null)
                this.OutboundAttachment = this._documentAttachment;

            //this._uploadVideoAttachment = reader.ReadGeneric<OutboundUploadVideoAttachment>();
            //if (this._uploadVideoAttachment != null)
            //    this.OutboundAttachment = this._uploadVideoAttachment;


            this._wallPostAttachment = reader.ReadGeneric<OutboundWallPostAttachment>();
            if (this._wallPostAttachment != null)
                this.OutboundAttachment = this._wallPostAttachment;
            this._forwardedMessages = reader.ReadGeneric<OutboundForwardedMessages>();
            if (this._forwardedMessages != null)
                this.OutboundAttachment = this._forwardedMessages;

            //this._uploadDocumentAttachment = reader.ReadGeneric<OutboundUploadDocumentAttachment>();
            //if (this._uploadDocumentAttachment != null)
            //    this.OutboundAttachment = this._uploadDocumentAttachment;
            this._pollAttachment = reader.ReadGeneric<OutboundPollAttachment>();
            if (this._pollAttachment != null)
                this.OutboundAttachment = this._pollAttachment;
            //this._timerAttachment = reader.ReadGeneric<OutboundTimerAttachment>();
            //if (this._timerAttachment != null)
            //    this.OutboundAttachment = this._timerAttachment;

            //this._linkAttachment = reader.ReadGeneric<OutboundLinkAttachment>();
            //if (this._linkAttachment != null)
            //    this.OutboundAttachment = this._linkAttachment;

            //this._productAttachment = reader.ReadGeneric<OutboundProductAttachment>();
            //if (this._productAttachment != null)
            //    this.OutboundAttachment = this._productAttachment;

            //this._noteAttachment = reader.ReadGeneric<OutboundNoteAttachment>();
            //if (this._noteAttachment != null)
            //    this.OutboundAttachment = this._noteAttachment;

            //this._marketAlbumAttachment = reader.ReadGeneric<OutboundMarketAlbumAttachment>();
            //if (this._marketAlbumAttachment != null)
            //    this.OutboundAttachment = this._marketAlbumAttachment;
            //this._albumAttachment = reader.ReadGeneric<OutboundAlbumAttachment>();
            //if (this._albumAttachment != null)
            //    this.OutboundAttachment = this._albumAttachment;

            this._voiceMessageAttachment = reader.ReadGeneric<OutboundVoiceMessageAttachment>();
            if (this._voiceMessageAttachment != null)
                this.OutboundAttachment = this._voiceMessageAttachment;

            if (this.OutboundAttachment == null)
                throw new Exception("Outbound Attachment is NULL");
        }
    }
}