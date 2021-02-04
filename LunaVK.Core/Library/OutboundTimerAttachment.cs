using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;

namespace LunaVK.Core.Library
{
    public class OutboundTimerAttachment : IOutboundAttachment
    {
        public DateTime Timer { get; private set; }

        public OutboundAttachmentUploadState UploadState
        {
            get
            {
                return OutboundAttachmentUploadState.Completed;
            }
            set
            {
            }
        }

        public override string ToString()
        {
            return "timestamp";
        }

        public bool IsUploadAttachment
        {
            get
            {
                return false;
            }
        }

        public void Upload(Action callback, Action<double> progressCallback = null)
        {
            callback();
        }

        public VKAttachment GetAttachment()
        {
            throw new NotImplementedException();
        }

        public OutboundTimerAttachment()
        {
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.Timer);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.Timer = reader.ReadDateTime();
        }
    }
}
