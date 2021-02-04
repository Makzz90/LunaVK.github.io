using Newtonsoft.Json;
using System;

namespace LunaVK.Core.Json
{
    /// <summary>
    /// Json-конвертер. Конвертирует Unixtime в структуру DateTime.
    /// </summary>
    public class UnixtimeToDateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        /// <summary>
        /// Возвращает новый объект DateTime, полученный из
        /// значения Unixtime.
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            long value = 0;
            if (reader.Value is String)
                value = long.Parse(reader.Value.ToString());
            else
                value = (long)reader.Value;

            if (value == 0)
                return null;//DateTime.MinValue;
            return GetDateTimeFromUnixTime(value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(GetUnixTimeFromDateTime((DateTime)value));
        }

        /// <summary>
        /// Возвращает структуру DateTime из числа UnixTime.
        /// </summary>
        /// <param name="unixTime">Время в формате UnixTime.</param>
        public static DateTime GetDateTimeFromUnixTime(long unixTime, bool includeTimeDiff = true)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            //return (new DateTime(1970, 1, 1, 0, 0, 0)).AddSeconds(unixTime);
            if (includeTimeDiff)
            {
                int minusLocalTimeDelta = Settings.ServerMinusLocalTimeDelta;
                unixTime -= (long)minusLocalTimeDelta;
            }
            dateTime = dateTime.AddSeconds(unixTime).ToLocalTime();
            return dateTime;
        }

        public static long GetUnixTimeFromDateTime(DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }
    }
}
