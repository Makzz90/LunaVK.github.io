using System;
using System.Collections.Generic;
using System.Text;
using LunaVK.Core.Enums;
using LunaVK.Core.DataObjects;
using Newtonsoft.Json;
using LunaVK.Core.Json;

namespace LunaVK.Core.DataObjects
{
    public class UpdatesResponse
    {
        public int ts { get; set; }

        public List<LongPollServerUpdateData> Updates { get; set; }

        public class LongPollServerUpdateData
        {
            public LongPollServerUpdateType UpdateType { get; set; }


            /// <summary>
            /// Для события UserBecameOnline это нужно
            /// </summary>
            public int user_id { get; set; }

            public uint message_id { get; set; }

            public int peer_id { get; set; }

            public int timestamp { get; set; }

            public string text { get; set; }

            //public VKMessageType @out { get; set; }

            public VKMessage message { get; set; }

            public VKUser user { get; set; }

            public int Platform { get; set; }

            public int Counter { get; set; }

            //public bool hasAttachOrForward { get; set; }

            public List<attach> attachments { get; set; }

            public List<fwd> fwds { get; set; }

            [JsonConverter(typeof(VKChatMessageActionTypeConverter))]
            public VKChatMessageActionType source_act { get; set; }

            /// <summary>
            /// Новое название беседы
            /// </summary>
            public string source_text { get; set; }

            /// <summary>
            /// Старое название беседы
            /// </summary>
            public string source_old_text { get; set; }

            /// <summary>
            /// идентификатор пользователя,
            /// к которому относится сервисное действие
            /// (для source_act=chat_invite_user и source_act=chat_kick_user). 
            /// </summary>
            public int source_mid { get; set; }

            public flag flags { get; set; }

            /// <summary>
            /// идентификатор администратора или редактора,
            /// отправившего сообщение. Возвращается
            /// для сообщений, отправленных от имени сообщества 
            /// </summary>
            public int from_admin { get; set; }

            /// <summary>
            ///  дополнительная информация об изменениях в чате
            ///  type_id = 1, 2 — $info = "0"; 
            ///  type_id = 3 — $info = "admin_id"; 
            ///  type_id = 5 — $info = "conversation_message_id"; 
            ///  type_id = 6, 7, 8, 9 — $info = "user_id"; 
            /// </summary>
            public int info { get; set; }

            /// <summary>
            ///  идентификатор типа измения в чате. 
            /// </summary>
            public type type_id { get; set; }

            public string extended { get; set; }

            public override string ToString()
            {
                return string.Format("UpdateType:{0} message_id:{1} peer_id:{2}", this.UpdateType, this.message_id, this.peer_id);
            }

            public class attach
            {
                public int id;

                public int owner_id;

                public int item_id;

                /// <summary>
                /// photo, video, audio, doc, wall, sticker, link, money
                /// </summary>
                public string type;

                public int product_id;

                public string title;

                public string description;

                public string url;
            }

            public class fwd
            {
                public int userId;
                public int msgId;

                public fwd(int user_id, int msg_id)
                {
                    userId = user_id;
                    msgId = msg_id;
                }
            }

            public class flag
            {
                /// <summary>
                /// сообщение не прочитано 1
                /// </summary>
                public bool UNREAD;

                /// <summary>
                /// исходящее сообщение 2
                /// </summary>
                public bool OUTBOX;

                /// <summary>
                /// на сообщение был создан ответ 4
                /// </summary>
                public bool REPLIED;

                /// <summary>
                /// помеченное сообщение 8
                /// </summary>
                public bool IMPORTANT;

                /// <summary>
                /// сообщение отправлено через чат. 16
                /// Обратите внимание, этот флаг устаревший и вскоре перестанет поддерживаться.
                /// </summary>
                public bool CHAT;

                /// <summary>
                /// сообщение отправлено другом. 32
                /// Не применяется для сообщений из групповых бесед.
                /// </summary>
                public bool FRIENDS;

                /// <summary>
                /// сообщение помечено как "Спам" 64
                /// </summary>
                public bool SPAM;

                /// <summary>
                /// сообщение удалено (в корзине) 128
                /// </summary>
                public bool DELЕTЕD;

                /// <summary>
                /// сообщение проверено пользователем на спам. 256
                /// Обратите внимание, этот флаг устаревший и вскоре перестанет поддерживаться.
                /// </summary>
                public bool FIXED;

                /// <summary>
                /// сообщение содержит медиаконтент. 512
                /// Обратите внимание, этот флаг устаревший и вскоре перестанет поддерживаться.
                /// </summary>
                public bool MEDIA;

                /// <summary>
                /// приветственное сообщение от сообщества. 65536
                /// Диалог с таким сообщением не нужно поднимать в списке
                /// (отображать его только при открытии диалога напрямую). Флаг недоступен для версий <2.
                /// </summary>
                public bool HIDDEN;

                /// <summary>
                /// сообщение удалено для всех получателей. 131072
                /// Флаг недоступен для версий <3. 
                /// </summary>
                public bool DELЕTЕD_FOR_ALL;
            }

            public enum type : byte
            {
                /// <summary>
                /// Изменилось название беседы; 
                /// </summary>
                NameChanged = 1,

                /// <summary>
                /// Сменилась обложка беседы; 
                /// </summary>
                CoverChanged,

                /// <summary>
                /// Назначен новый администратор; 
                /// </summary>
                NewAdmin,

                /// <summary>
                /// Закреплено сообщение 
                /// </summary>
                MessagePinned = 5,

                /// <summary>
                ///  Пользователь присоединился к беседе 
                /// </summary>
                UserEnter,

                /// <summary>
                /// Пользователь покинул беседу 
                /// </summary>
                UserLeave,

                /// <summary>
                /// Пользователя исключили из беседы 
                /// </summary>
                UserKicked,

                /// <summary>
                /// С пользователя сняты права администратора 
                /// </summary>
                AdminEmpty
            }
        }
    }
}
