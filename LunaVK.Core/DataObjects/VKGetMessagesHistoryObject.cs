using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using LunaVK.Core.Network;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Представляет ответ сервера ВКонтакте на запрос получения истории сообщений диалога или чата.
    /// </summary>
    public class VKGetMessagesHistoryObject : VKCountedItemsObject<VKMessage>
    {
        /// <summary>
        /// Количество непрочитанных сообщений.
        /// </summary>
        //public uint unread { get; set; }

        public List<VKConversation> conversations { get; set; }

        /// <summary>
        /// Количество пропущенных элементов (если применимо).
        /// </summary>
        public uint skipped { get; set; }
    }
}
