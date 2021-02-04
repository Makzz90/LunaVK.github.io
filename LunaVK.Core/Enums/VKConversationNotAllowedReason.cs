using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.Enums
{
    public enum VKConversationNotAllowedReason : short
    {
        /// <summary>
        /// пользователь заблокирован или удален
        /// </summary>
        UserDisabled = 18,

        /// <summary>
        /// нельзя отправить сообщение пользователю, который в чёрном списке
        /// </summary>
        UserBlackListed = 900,

        /// <summary>
        /// пользователь запретил сообщения от сообщества
        /// </summary>
        CommunityChatDisabled,

        /// <summary>
        /// пользователь запретил присылать ему сообщения с помощью настроек приватности
        /// </summary>
        PrivacySettings,

        /// <summary>
        /// в сообществе отключены сообщения
        /// </summary>
        CommunityMessagesDisabled = 915,

        /// <summary>
        ///  в сообществе заблокированы сообщения
        /// </summary>
        CommunityMessagesBlocked,

        /// <summary>
        /// нет доступа к чату; // нас исключили
        /// </summary>
        NoAccessToChat,

        /// <summary>
        /// нет доступа к e-mail
        /// </summary>
        NoAccessToEMail,

        /// <summary>
        /// нет доступа к сообществу
        /// </summary>
        NoAccessToCommunity = 203
    }
}