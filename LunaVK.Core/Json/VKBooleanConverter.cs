using Newtonsoft.Json;
using System;

namespace LunaVK.Core.Json
{
    /// <summary>
    /// Представляет Json конвертер 1-0 то <see cref="bool"/>.
    /// </summary>
    public sealed class VKBooleanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) { return objectType == typeof(bool); }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string temp = reader.Value.ToString();
            return temp == "1" || temp == "True";
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((bool)value) ? 1 : 0);
        }
    }
}
