using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using LunaVK.Core.DataObjects;
using Newtonsoft.Json.Linq;

namespace LunaVK.Core.Json
{
    public class SizesToDictionaryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type != JTokenType.Array)
                return null;

            var temp = token.ToObject<List<VKImageWithSize>>();
            
            Dictionary<char, VKImageWithSize> ret = new Dictionary<char, VKImageWithSize>();

            foreach (VKImageWithSize img in temp)
                ret.Add(img.type, img);

            //BugFix: Если этих полей вдруг не будет
            //if(!ret.ContainsKey('s'))
            //    ret.Add('s', new VKImageWithSize());
            //if (!ret.ContainsKey('m'))
            //    ret.Add('m', new VKImageWithSize());
            //if (!ret.ContainsKey('x'))
            //    ret.Add('x', new VKImageWithSize());
            //if (!ret.ContainsKey('y'))
            //    ret.Add('y', new VKImageWithSize());
            //if (!ret.ContainsKey('z'))
            //    ret.Add('z', new VKImageWithSize());
            //if (!ret.ContainsKey('w'))
            //    ret.Add('w', new VKImageWithSize());

            return ret;
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
