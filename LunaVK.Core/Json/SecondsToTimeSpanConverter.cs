using System;
using Newtonsoft.Json;

namespace LunaVK.Core.Json
{
    /// <summary>
    /// Json-конвертер. Конвертирует длительность в секундах в структуру TimeSpan.
    /// </summary>
    public class SecondsToTimeSpanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Возвращает новый объект TimeSpan, полученный из
        /// значения длительности в секундах.
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(int)) return TimeSpan.FromSeconds((int)reader.Value);
            else if (reader.ValueType == typeof(long)) return TimeSpan.FromSeconds((long)reader.Value);
            else return TimeSpan.FromSeconds(long.Parse(reader.Value.ToString()));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((TimeSpan)value).TotalSeconds);
        }
    }
}
