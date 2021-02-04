using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using System;

namespace LunaVK.Core.Library
{
    public interface IOutboundAttachment : IBinarySerializable
    {
        void Upload(Action callback, Action<double> progressCallback = null);

        bool IsUploadAttachment { get; }

        OutboundAttachmentUploadState UploadState { get; set; }

        VKAttachment GetAttachment();
    }

    public enum OutboundAttachmentUploadState
    {
        NotStarted,
        Uploading,
        Failed,
        Completed,
    }
}
