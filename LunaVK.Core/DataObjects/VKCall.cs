using LunaVK.Core.Framework;
using LunaVK.Core.Json;
using LunaVK.Core.Utils;
using Newtonsoft.Json;
using System;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    public class VKCall : IBinarySerializable
    {
        /// <summary>
        /// инициатор звонка. 
        /// </summary>
        public int initiator_id { get; set; }

        /// <summary>
        /// получатель звонка. 
        /// </summary>
        public int receiver_id { get; set; }

        /// <summary>
        /// состояние. 
        /// canceled_by_initiator — сброшен инициатором 
        /// canceled_by_receiver — сброшен получателем 
        /// reached — состоялся 
        /// </summary>
        public string state { get; set; }
        
        /// <summary>
        /// длительность звонка в секундах.
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime time { get; set; }

        public void Write(BinaryWriter writer)
        {
            writer.Write(2);
            writer.Write(this.initiator_id);
            writer.Write(this.receiver_id);
            writer.WriteString(this.state);
            writer.Write(this.time);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.initiator_id = reader.ReadInt32();
            this.receiver_id = reader.ReadInt32();
            this.state = reader.ReadString();
            this.time = reader.ReadDateTime();
        }
    }
}
