using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace LunaVK.Library
{
    public class SavedContacts : IBinarySerializable
    {
        public List<VKUser> SavedUsers { get; set; }

        public DateTime SyncedDate { get; set; }

        //public FriendRequests Requests { get; set; }

        public uint CurrentUserId { get; set; }

        public SavedContacts()
        {
            this.SavedUsers = new List<VKUser>();
            this.SyncedDate = DateTime.MinValue;
            //this.Requests = null;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(3);
            writer.WriteList(this.SavedUsers);
            writer.Write(this.SyncedDate);
            writer.Write(this.CurrentUserId);
            //writer.Write<FriendRequests>(this.Requests, false);
        }

        public void Read(BinaryReader reader)
        {
            int num1 = reader.ReadInt32();
            this.SavedUsers = reader.ReadList<VKUser>();
            this.SyncedDate = reader.ReadDateTime();
            this.CurrentUserId = reader.ReadUInt32();
            //this.Requests = reader.ReadGeneric<FriendRequests>();
        }
    }
}
