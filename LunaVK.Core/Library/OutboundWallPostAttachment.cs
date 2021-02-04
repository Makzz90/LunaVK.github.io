using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;

namespace LunaVK.Core.Library
{
    public class OutboundWallPostAttachment : IOutboundAttachment
    {
        public VKWallPost _wallPost;
        public VKNewsfeedPost _newsfeedPost;

        public OutboundAttachmentUploadState UploadState { get { return OutboundAttachmentUploadState.Completed; } set { } }

        public OutboundWallPostAttachment(VKWallPost post)
        {
            this._wallPost = post;
        }

        public OutboundWallPostAttachment(VKNewsfeedPost post)
        {
            //this._newsfeedPost = post;
        }

        public OutboundWallPostAttachment()
        {
        }

        public string IconSource
        {
            get { return "\xE8F3"; }
        }

        public string Title
        {
            get
            {
                return "Запись на стене";//Conversation_WallPost;
            }
        }

        public string Subtitle
        {
            get
            {
                if (this._wallPost != null)
                    return this._wallPost.text;
                return null;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}{1}_{2}", "wall", this._wallPost.OwnerId, this._wallPost.PostId);
        }

        public void Upload(Action completionCallback, Action<double> progressCallback = null)
        {
            completionCallback();
        }

        public bool IsUploadAttachment
        {
            get { return false; }
        }

        public VKAttachment GetAttachment()
        {
            return new VKAttachment() { wall = this._wallPost, type = Enums.VKAttachmentType.Wall };
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write<VKWallPost>(this._wallPost);
            //writer.Write<VKNewsfeedPost>(this._newsfeedPost);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this._wallPost = reader.ReadGeneric<VKWallPost>();
            //this._newsfeedPost = reader.ReadGeneric<VKNewsfeedPost>();
        }
    }
}
