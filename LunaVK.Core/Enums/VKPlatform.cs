using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.Enums
{
    /// <summary>
    /// Платформа, через которую зашел пользователь.
    /// </summary>
    public enum VKPlatform : byte
    {
        /// <summary>
        /// Неизвестно.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Мобильная версия сайта.
        /// </summary>
        Mobile = 1,

        /// <summary>
        /// Приложение для iPhone.
        /// </summary>
        iPhone,

        /// <summary>
        /// Приложение для iPad.
        /// </summary>
        iPad,

        /// <summary>
        /// Приложение для Android.
        /// </summary>
        Android,

        /// <summary>
        /// Приложение для Windows Phone.
        /// </summary>
        WindowsPhone,

        /// <summary>
        /// Приложение для Windows.
        /// </summary>
        Windows,

        /// <summary>
        /// Полная версия сайта.
        /// </summary>
        Web,//7

        VK_Mobile
    }
}
