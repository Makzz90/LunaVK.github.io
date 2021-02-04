using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.Enums
{
    /// <summary>
    /// Перечисление возможных состояний деактивации.
    /// </summary>
    public enum VKIsDeactivated : byte
    {
        /// <summary>
        /// Не деактивировано.
        /// </summary>
        None = 0,

        /// <summary>
        /// Удалено.
        /// </summary>
        Deleted,

        /// <summary>
        /// Забанено.
        /// </summary>
        Banned
    }
}
