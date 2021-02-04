using System.IO;

namespace LunaVK.Core.Framework
{
    public interface IBinarySerializable
    {
        void Write(BinaryWriter writer);

        void Read(BinaryReader reader);
    }

    public interface IBinarySerializableWithTrimSupport : IBinarySerializable
    {
        void WriteTrimmed(BinaryWriter writer);
    }
}
