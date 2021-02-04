using LunaVK.Core.DataObjects;
using System;
using System.IO;

namespace LunaVK.Core.Library
{
    public class OutboundAddAttachment : IOutboundAttachment
    {
        public bool IsUploadAttachment
        {
            get { return false; }
        }

        public void Upload(Action completionCallback, Action<double> progressCallback = null)
        {
            completionCallback();
        }

        public OutboundAttachmentUploadState UploadState { get { return OutboundAttachmentUploadState.Completed; } set { } }

        public void Write(BinaryWriter writer)
        {
        }

        public void Read(BinaryReader reader)
        {
        }

        public VKAttachment GetAttachment()
        {
            return null;
        }
    }
}
