using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.Enums
{
    /// <summary>
    /// Перечисление возможных состояний группы.
    /// </summary>
    public enum VKGroupIsClosed : byte
    {
        /// <summary>
        /// Публичная группа
        /// </summary>
        Opened,
        Closed,
        Private
    }
}
