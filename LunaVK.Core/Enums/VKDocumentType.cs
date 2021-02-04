using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.Enums
{
    /// <summary>
    /// Типы вложенного документа в сообщении
    /// </summary>
    public enum VKDocumentType : byte
    {
        TEXT = 1,
        ARCHIVE,
        GIF,
        IMAGE,

        /// <summary>
        /// Это голосовое сообщение DOC_TYPE_AUDIO
        /// </summary>
        AUDIO,
        VIDEO,
        EBOOK,
        UNKNOWN,
    }
}
