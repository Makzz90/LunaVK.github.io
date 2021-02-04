using LunaVK.Core.Framework;
using LunaVK.Core.Json;
using LunaVK.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.DataObjects
{
    public class VKPodcast : IBinarySerializable
    {
        public uint id { get; set; }

        /// <summary>
        /// идентификатор владельца
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// владелец
        /// </summary>
        public string artist { get; set; }

        /// <summary>
        /// название композиции
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// длительность в секундах
        /// </summary>
        public int duration { get; set; }

        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime date { get; set; }

        /// <summary>
        /// ссылка на mp3.
        /// </summary>
        public string url { get; set; }

        public string track_code { get; set; }

        public string description { get; set; }

        /// <summary>
        /// идентификатор текста аудиозаписи (если доступно).
        /// </summary>
        public int lyrics_id { get; set; }

        /// <summary>
        /// если включена опция «Не выводить при поиске»
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool no_search { get; set; }
        
        public PodcastInfo podcast_info { get; set; }

        public class PodcastInfo
        {
            public Cover cover { get; set; }

            /// <summary>
            /// количество проигрываний
            /// </summary>
            public int plays { get; set; }

            public bool is_favorite { get; set; }

            public string description { get; set; }

            public int position { get; set; }

            public class Cover
            {
                [JsonConverter(typeof(SizesToDictionaryConverter))]
                public Dictionary<char, VKImageWithSize> sizes { get; set; }
            }
            
        }

        public override string ToString()
        {
            return "podcast" + this.owner_id + "_" + this.id;
        }

#region VM
        public string CoverImg
        {
            get
            {
                if(this.podcast_info!=null)
                {
                    if(this.podcast_info.cover!=null)
                    {
                        return this.podcast_info.cover.sizes['e'].url;
                    }
                }
                return null;
            }
        }
        #endregion

#region IBinarySerializable
        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.id);
            writer.Write(this.owner_id);
            writer.WriteString(this.artist);
            writer.WriteString(this.title);
            writer.Write(this.duration);
            writer.WriteString(this.url);
            writer.Write(this.lyrics_id);

            writer.WriteString(this.CoverImg);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadUInt32();
            this.owner_id = reader.ReadInt32();
            this.artist = reader.ReadString();
            this.title = reader.ReadString();
            this.duration = reader.ReadInt32();
            this.url = reader.ReadString();
            this.lyrics_id = reader.ReadInt32();

            string temp = reader.ReadString();
            if(!string.IsNullOrEmpty(temp))
            {
                this.podcast_info = new PodcastInfo();
                this.podcast_info.cover = new PodcastInfo.Cover();
                this.podcast_info.cover.sizes = new Dictionary<char, VKImageWithSize>();
                this.podcast_info.cover.sizes.Add('e', new VKImageWithSize() { url = temp });
            }
        }
#endregion
    }
}
