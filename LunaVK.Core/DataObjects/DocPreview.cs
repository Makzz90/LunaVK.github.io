using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LunaVK.Core.DataObjects
{
    public class DocPreview : IBinarySerializable
    {
        public DocPreviewPhoto photo { get; set; }

        public DocPreviewVideo video { get; set; }

        public DocPreviewGraffiti graffiti { get; set; }

        public DocPreviewVoiceMessage audio_msg { get; set; }

        public sealed class DocPreviewPhoto : IBinarySerializable
        {
            public List<DocPreviewPhotoSize> sizes { get; set; }

            public sealed class DocPreviewPhotoSize : IBinarySerializable
            {
                public string src { get; set; }

                public int width { get; set; }

                public int height { get; set; }

                public string type { get; set; }

                public void Write(BinaryWriter writer)
                {
                    writer.Write(1);
                    writer.WriteString(this.src);
                    writer.Write(this.width);
                    writer.Write(this.height);
                    writer.WriteString(this.type);
                }

                public void Read(BinaryReader reader)
                {
                    reader.ReadInt32();
                    this.src = reader.ReadString();
                    this.width = reader.ReadInt32();
                    this.height = reader.ReadInt32();
                    this.type = reader.ReadString();
                }
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write(1);
                writer.WriteList<DocPreviewPhotoSize>(this.sizes);
            }

            public void Read(BinaryReader reader)
            {
                reader.ReadInt32();
                this.sizes = reader.ReadList<DocPreviewPhotoSize>();
            }
        }

        public class DocPreviewVideo : IBinarySerializable
        {
            public string src { get; set; }

            public int width { get; set; }

            public int height { get; set; }

            public int file_size { get; set; }

            public void Write(BinaryWriter writer)
            {
                writer.Write(1);
                writer.WriteString(this.src);
                writer.Write(this.width);
                writer.Write(this.height);
                writer.Write(this.file_size);
            }

            public void Read(BinaryReader reader)
            {
                int num1 = reader.ReadInt32();
                this.src = reader.ReadString();
                this.width = reader.ReadInt32();
                this.height = reader.ReadInt32();
                this.file_size = reader.ReadInt32();
            }
        }

        public class DocPreviewGraffiti : IBinarySerializable
        {
            public string src { get; set; }

            public int width { get; set; }

            public int height { get; set; }

            public void Write(BinaryWriter writer)
            {
                writer.Write(1);
                writer.WriteString(this.src);
                writer.Write(this.width);
                writer.Write(this.height);
            }

            public void Read(BinaryReader reader)
            {
                reader.ReadInt32();
                this.src = reader.ReadString();
                this.width = reader.ReadInt32();
                this.height = reader.ReadInt32();
            }
        }

        public class DocPreviewVoiceMessage : IBinarySerializable
        {
            public int id { get; set; }

            /// <summary>
            /// Длина записи в секундах
            /// </summary>
            public int duration { get; set; }

            public List<int> waveform { get; set; }

            public string link_ogg { get; set; }

            public string link_mp3 { get; set; }

            public void Write(BinaryWriter writer)
            {
                writer.Write(1);
                writer.Write(this.duration);
                writer.WriteList(this.waveform);
                writer.WriteString(this.link_ogg);
                writer.WriteString(this.link_mp3);
            }

            public void Read(BinaryReader reader)
            {
                reader.ReadInt32();
                this.duration = reader.ReadInt32();
                this.waveform = reader.ReadListInt();
                this.link_ogg = reader.ReadString();
                this.link_mp3 = reader.ReadString();
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(4);
            writer.Write<DocPreviewPhoto>(this.photo);
            writer.Write<DocPreviewVideo>(this.video);
            writer.Write<DocPreviewGraffiti>(this.graffiti);
            writer.Write<DocPreviewVoiceMessage>(this.audio_msg);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.photo = reader.ReadGeneric<DocPreviewPhoto>();
            this.video = reader.ReadGeneric<DocPreviewVideo>();
            this.graffiti = reader.ReadGeneric<DocPreviewGraffiti>();
            this.audio_msg = reader.ReadGeneric<DocPreviewVoiceMessage>();
        }
    }
}
