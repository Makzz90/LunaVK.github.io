using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using LunaVK.Core.Json;
using LunaVK.Core.Enums;
using Windows.UI.Xaml.Media;
using LunaVK.Core.Utils;
using LunaVK.Core.Framework;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    public class VKConversation : IBinarySerializable
    {
        /// <summary>
        /// информация о собеседнике
        /// </summary>
        public ConversationPeer peer { get; set; }

        /// <summary>
        /// последнее сообщения, которое прочитал текущий пользователь
        /// </summary>
        public uint in_read { get; set; }

        /// <summary>
        /// последнее сообщения, которое прочитал собеседник
        /// </summary>
        public uint out_read { get; set; }

        /// <summary>
        /// число непрочитанных сообщений. 
        /// </summary>
        public uint unread_count { get; set; }


        public uint last_message_id { get; set; }
        
        public bool is_marked_unread { get; set; }

        public ConversationSortId sort_id { get; set; }
        
        /// <summary>
        /// диалог помечен как важный
        /// (только для сообщений сообществ). 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool important { get; set; }

        /// <summary>
        /// диалог помечен как неотвеченный
        /// (только для сообщений сообществ). 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool unanswered { get; set; }

        /// <summary>
        /// настройки Push-уведомлений
        /// не передаются, если ничего не настроено
        /// </summary>
        public VKPushSettings push_settings { get; set; }

        /// <summary>
        /// может ли пользователь писать в диалог
        /// </summary>
        public ConversationCanWrite can_write { get; set; }

        /// <summary>
        /// настройки чата
        /// </summary>
        public ConversationSettings chat_settings { get; set; }

        public VKBotKeyboard current_keyboard { get; set; }

        public bool AreDisabledNow
        {
            get
            {
                if (this.push_settings == null)
                    return false;
                if (this.push_settings.disabled_forever)
                    return true;
                if (this.push_settings.disabled_until == -1)
                    return true;
                if (this.push_settings.disabled_until > 0)
                    return Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, true) < this.push_settings.disabled_until;
                return false;
            }
        }

        public class ConversationPeer : IBinarySerializable
        {
            /// <summary>
            /// идентификатор назначения.
            /// </summary>
            public int id { get; set; }

            /// <summary>
            /// тип. Возможные значения: user, chat, group, email
            /// </summary>
            [JsonConverter(typeof(VKConversationPeerTypeConverter))]
            public VKConversationPeerType type { get; set; }

            /// <summary>
            /// локальный идентификатор назначения.
            /// Для чатов это id - 2000000000,
            /// для сообществ — -id,
            /// для e-mail это -(id+2000000000). 
            /// </summary>
            public int local_id { get; set; }


            public void Write(BinaryWriter writer)
            {
                writer.Write(1);
                writer.Write(this.id);
                writer.Write((byte)this.type);
                writer.Write(this.local_id);
            }

            public void Read(BinaryReader reader)
            {
                reader.ReadInt32();
                this.id = reader.ReadInt32();
                this.type = (VKConversationPeerType)reader.ReadByte();
                this.local_id = reader.ReadInt32();
            }
        }

        public class ConversationSettings : IBinarySerializable
        {
            /// <summary>
            ///  число участников;
            /// </summary>
            public uint members_count { get; set; }

            /// <summary>
            /// название
            /// </summary>
            public string title { get; set; }

            public VKPinnedMessage pinned_message { get; set; }

            /// <summary>
            /// статус текущего пользователя.
            /// in — состоит в чате;
            /// kicked — исключён из чата;
            /// left — покинул чат.
            /// </summary>
            public string state { get; set; }

            /// <summary>
            /// изображение-обложка чата
            /// </summary>
            public VKMessage.MsgActionPhoto photo { get; set; }

            /// <summary>
            /// идентификаторы последних пользователей, писавших в чат.
            /// </summary>
            public List<int> active_ids { get; set; }




            public void Write(BinaryWriter writer)
            {
                writer.Write(1);
                writer.Write(this.members_count);
                writer.WriteString(this.title);
                writer.WriteString(this.state);
                writer.Write(this.photo);
                writer.WriteList(this.active_ids);
            }

            public void Read(BinaryReader reader)
            {
                reader.ReadInt32();
                this.members_count = reader.ReadUInt32();
                this.title = reader.ReadString();
                this.state = reader.ReadString();
                this.photo = reader.ReadGeneric<VKMessage.MsgActionPhoto>();
                this.active_ids = reader.ReadListInt();
            }
        }

        public class ConversationCanWrite
        {
            /// <summary>
            ///  пользователь может писать в диалог
            /// </summary>
            [JsonConverter(typeof(VKBooleanConverter))]
            public bool allowed { get; set; }

            /// <summary>
            /// код ошибки для allowed = false. Возможные значения:
            /// 18 — пользователь заблокирован или удален;
            /// 900 — нельзя отправить сообщение пользователю, который в чёрном списке;
            /// 901 — пользователь запретил сообщения от сообщества;
            /// 902 — пользователь запретил присылать ему сообщения с помощью настроек приватности;
            /// 915 — в сообществе отключены сообщения;
            /// 916 — в сообществе заблокированы сообщения;
            /// 917 — нет доступа к чату; // нас исключили
            /// 918 — нет доступа к e-mail;
            /// 203 — нет доступа к сообществу. 
            /// </summary>
            public VKConversationNotAllowedReason reason { get; set; }
        }

        public class ConversationSortId
        {
            /// <summary>
            /// 0 - нет сортировки, 16 - закреплено
            /// </summary>
            public uint major_id { get; set; }

            public uint minor_id { get; set; }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.unread_count);
            writer.Write(this.in_read);
            writer.Write(this.out_read);
            if(this.can_write==null)
                writer.Write(true);
            else
                writer.Write(this.can_write.allowed);
            writer.Write(this.peer);
            writer.Write(this.chat_settings);
            writer.Write(this.push_settings);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.unread_count = reader.ReadUInt32();
            this.in_read = reader.ReadUInt32();
            this.out_read = reader.ReadUInt32();
            bool _canWrite = reader.ReadBoolean();
            this.peer = reader.ReadGeneric<ConversationPeer>();
            this.chat_settings = reader.ReadGeneric<ConversationSettings>();
            this.push_settings = reader.ReadGeneric<VKPushSettings>();

            this.can_write = new ConversationCanWrite() { allowed = _canWrite };
        }
    }
}
