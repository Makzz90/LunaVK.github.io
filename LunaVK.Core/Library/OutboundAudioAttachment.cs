using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.Library
{
    public class OutboundAudioAttachment : IOutboundAttachment
    {
        private VKAudio _audio;
        public OutboundAttachmentUploadState UploadState { get; set; }

        public OutboundAudioAttachment(VKAudio audio)
        {
            this._audio = audio;
        }

        public OutboundAudioAttachment()
        {
        }

        public string Title
        {
            get { return this._audio.artist; }
        }

        public string Subtitle
        {
            get { return this._audio.title; }
        }

        public bool IsUploadAttachment
        {
            get { return false; }
        }

        public string IconSource
        {
            get { return "\xE8D6"; }
        }

        public VKAttachment GetAttachment()
        {
            return new VKAttachment() { audio = this._audio, type = Enums.VKAttachmentType.Audio };
        }

        public void Upload(Action callback, Action<double> progressCallback = null)
        {
            callback();
        }

        public override string ToString()
        {
            return "audio" + this._audio.owner_id + "_" + this._audio.id;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);//VKConstants.SerializationVersion
            writer.Write<VKAudio>(this._audio, false);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this._audio = reader.ReadGeneric<VKAudio>();
        }
    }
}
