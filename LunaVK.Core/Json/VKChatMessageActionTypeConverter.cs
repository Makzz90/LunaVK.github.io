using System;
using Newtonsoft.Json;
using LunaVK.Core.Enums;

namespace LunaVK.Core.Json
{
    /// <summary>
    /// Представляет конвертер для типа <see cref="VKChatMessageActionType"/>.
    /// </summary>
    public class VKChatMessageActionTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(VKChatMessageActionType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.Value.ToString())
            {
                case "chat_photo_update": return VKChatMessageActionType.ChatPhotoUpdate;
                case "chat_photo_remove": return VKChatMessageActionType.ChatPhotoRemove;
                case "chat_create": return VKChatMessageActionType.ChatCreate;
                case "chat_title_update": return VKChatMessageActionType.ChatTitleUpdate;
                case "chat_invite_user": return VKChatMessageActionType.ChatInviteUser;
                case "chat_kick_user": return VKChatMessageActionType.ChatKickUser;

                case "chat_pin_message": return VKChatMessageActionType.ChatPinMessage;
                case "chat_unpin_message": return VKChatMessageActionType.ChatUnpinMessage;
                case "chat_invite_user_by_link": return VKChatMessageActionType.ChatInviteUserByLink;
                default: return VKChatMessageActionType.None;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch ((VKChatMessageActionType)value)
            {
                case VKChatMessageActionType.ChatPhotoUpdate:
                    writer.WriteValue("chat_photo_update");
                    break;
                case VKChatMessageActionType.ChatPhotoRemove:
                    writer.WriteValue("chat_photo_remove");
                    break;
                case VKChatMessageActionType.ChatCreate:
                    writer.WriteValue("chat_create");
                    break;
                case VKChatMessageActionType.ChatTitleUpdate:
                    writer.WriteValue("chat_title_update");
                    break;
                case VKChatMessageActionType.ChatInviteUser:
                    writer.WriteValue("chat_invite_user");
                    break;
                case VKChatMessageActionType.ChatKickUser:
                    writer.WriteValue("chat_kick_user");
                    break;


                case VKChatMessageActionType.ChatPinMessage:
                    writer.WriteValue("chat_pin_message");
                    break;
                case VKChatMessageActionType.ChatUnpinMessage:
                    writer.WriteValue("chat_unpin_message");
                    break;
                case VKChatMessageActionType.ChatInviteUserByLink:
                    writer.WriteValue("chat_invite_user_by_link");
                    break;
                default:
                    writer.WriteValue("none");
                    break;
            }
        }
    }
}
