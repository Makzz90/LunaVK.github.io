using System;
using Newtonsoft.Json;
using LunaVK.Core.Enums;

namespace LunaVK.Core.Json
{
    public class VKConversationPeerTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(VKConversationPeerType);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.Value.ToString())
            {
                case "group": return VKConversationPeerType.Group;
                case "chat": return VKConversationPeerType.Chat;
                case "email": return VKConversationPeerType.Email;
                default: return VKConversationPeerType.User;
            }
        }
        //Возможные значения: user, chat, group, email
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch ((VKConversationPeerType)value)
            {
                case VKConversationPeerType.Group:
                    writer.WriteValue("group");
                    break;
                case VKConversationPeerType.Chat:
                    writer.WriteValue("chat");
                    break;
                case VKConversationPeerType.Email:
                    writer.WriteValue("email");
                    break;
                default:
                    writer.WriteValue("user");
                    break;
            }
        }
    }
}
