using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.DataObjects
{
    public class LongPollServerResponse
    {
        /// <summary>
        /// Секретный ключ сессии
        /// </summary>
        public string key { get; set; }

        /// <summary>
        /// Адрес сервера
        /// </summary>
        public string server { get; set; }

        /// <summary>
        /// Номер последнего события, начиная с которого надо получать данные
        /// </summary>
        public int ts { get; set; }
    }
}
