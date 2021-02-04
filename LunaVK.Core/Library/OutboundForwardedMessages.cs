using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;

namespace LunaVK.Core.Library
{
    public class OutboundForwardedMessages : IOutboundAttachment
    {
        public List<VKMessage> Messages { get; private set; }

        public OutboundForwardedMessages(List<VKMessage> messages)
        {
            this.Messages = messages;
        }

        public OutboundForwardedMessages()
        {
        }

        public OutboundAttachmentUploadState UploadState { get { return OutboundAttachmentUploadState.Completed; } set{} }

        public bool IsUploadAttachment
        {
            get { return false; }
        }

        public string Title
        {
            get { return UIStringFormatterHelper.FormatNumberOfSomething(this.Messages.Count, "OneMessageFrm", "TwoFourMessagesFrm", "FiveMessagesFrm", true); }
        }
        /*
        public string Title
        {
            get
            {
                return this.Messages.Count.ToString();
            }
        }

        public string Subtitle
        {
            get
            {
                return UIStringFormatterHelper.FormatNumberOfSomething(this.Messages.Count, LocalizedStrings.GetString("OneMessageFrm"), LocalizedStrings.GetString("TwoFourMessagesFrm"), LocalizedStrings.GetString("FiveMessagesFrm"), false, null, false);
            }
        }
        */

        public VKAttachment GetAttachment()
        {
            return null;
        }

        public void Upload(Action completionCallback, Action<double> progressCallback = null)
        {
            completionCallback();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.WriteList<VKMessage>(this.Messages);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.Messages = reader.ReadList<VKMessage>();
        }
    }
}
