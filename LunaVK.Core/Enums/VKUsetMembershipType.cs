using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.Enums
{
    public enum VKUsetMembershipType : byte
    {
        /// <summary>
        /// не является другом
        /// </summary>
        No,

        /// <summary>
        /// отправлена заявка/подписка пользователю
        /// </summary>
        RequestSent,

        /// <summary>
        /// имеется входящая заявка/подписка от пользователя
        /// </summary>
        RequestReceived,

        /// <summary>
        /// является другом
        /// </summary>
        Friends,
    }
}
