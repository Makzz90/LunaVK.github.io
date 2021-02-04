using Newtonsoft.Json;
using LunaVK.Core.Json;
using LunaVK.Core.Framework;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// настройки оповещений для диалога
    /// </summary>
    public class VKPushSettings : IBinarySerializable
    {
        /// <summary>
        /// указывает, до какого времени оповещения для чата отключены
        /// -1 навсегда и только у чата
        /// </summary>
        public int disabled_until { get; set; }

        
        /// <summary>
        /// Передаётся если оповещения отключены навсегда
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool disabled_forever { get; set; }

        /// <summary>
        /// Передаётся если отключен звук уведомлений
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool no_sound { get; set; }

        
        /// <summary>
        /// указывает, вЫключен ли звук оповещений (появляется при получении информации о чате)
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool sound { get; set; }
        
        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.disabled_until);
            writer.Write(this.disabled_forever);
            writer.Write(this.no_sound);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.disabled_until = reader.ReadInt32();
            this.disabled_forever = reader.ReadBoolean();
            this.no_sound = reader.ReadBoolean();
        }
    }
}
