using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System.Collections.Generic;
using System.IO;

namespace LunaVK.Common
{
    public class ContactsCache : IBinarySerializable
    {
        public List<string> PhoneNumbers { get; set; }

        public ContactsCache()
        {
            this.PhoneNumbers = new List<string>();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.WriteList(this.PhoneNumbers);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.PhoneNumbers = reader.ReadList();
        }
    }
}
