using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;
using System;
using System.IO;

namespace LunaVK.Core.Library
{
    public class OutboundPollAttachment : IOutboundAttachment
    {
        private VKPoll _poll;

        public OutboundPollAttachment()
        {
        }

#region IOutboundAttachment
        public OutboundAttachmentUploadState UploadState { get { return OutboundAttachmentUploadState.Completed; } set { } }

        public void Upload(Action completionCallback, Action<double> progressCallback = null)
        {
            completionCallback();
        }

        public bool IsUploadAttachment
        {
            get { return false; }
        }

        public string Title
        {
            get
            {
                return LocalizedStrings.GetString("Poll");
            }
        }

        public string Subtitle
        {
            get
            {
                if (this._poll != null)
                    return this._poll.question;
                return "";
            }
        }

        public VKAttachment GetAttachment()
        {
            VKAttachment attachment = new VKAttachment();
            attachment.type= Enums.VKAttachmentType.Poll;
            attachment.poll = this._poll;
            return attachment;
        }
        #endregion

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this._poll);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this._poll = reader.ReadGeneric<VKPoll>();
        }
    }
}
