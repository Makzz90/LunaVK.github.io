using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.Enums
{
    /// <summary>
    /// Пол пользователя ВКонтакте.
    /// </summary>
    public enum VKUserSex : byte
    {
        /// <summary>
        /// Пол не указан.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Женский пол.
        /// </summary>
        Female = 1,
        /// <summary>
        /// Мужской пол.
        /// </summary>
        Male = 2
    }
}
