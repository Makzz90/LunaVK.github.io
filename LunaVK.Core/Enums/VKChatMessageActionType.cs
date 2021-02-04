using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using LunaVK.Core.Json;

namespace LunaVK.Core.Enums
{
    /// <summary>
    /// Тип служебного сообщения чата.
    /// </summary>
    [JsonConverter(typeof(VKChatMessageActionTypeConverter))]
    public enum VKChatMessageActionType : byte
    {
        /// <summary>
        /// Это не служебное сообщение.
        /// </summary>
        None = 0,

        /// <summary>
        /// Обновлена фотография беседы.
        /// </summary>
        ChatPhotoUpdate,

        /// <summary>
        /// Удалена фотография беседы.
        /// </summary>
        ChatPhotoRemove,

        /// <summary>
        /// Беседа создана.
        /// </summary>
        ChatCreate,

        /// <summary>
        /// Заголовок беседы обновлен.
        /// </summary>
        ChatTitleUpdate,

        /// <summary>
        /// В беседу добавлен новый пользователь.
        /// </summary>
        ChatInviteUser,

        /// <summary>
        /// Пользователь исключен из беседы.
        /// </summary>
        ChatKickUser,

        /// <summary>
        /// закреплено сообщение
        /// </summary>
        ChatPinMessage,

        /// <summary>
        /// откреплено сообщение
        /// </summary>
        ChatUnpinMessage,

        /// <summary>
        /// пользователь присоединился к беседе по ссылке. 
        /// </summary>
        ChatInviteUserByLink,




        /// <summary>
        /// Это непрочитанное сообщение
        /// </summary>
        UNREAD_ITEM_ACTION
    }
}
