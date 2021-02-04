using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using LunaVK.Core.Enums;

namespace LunaVK.Core.Network
{
    /// <summary>
    /// Представляет собой базовый класс ответа 
    /// сервера ВКонтакте в формате Json.
    /// </summary>
    /// <typeparam name="T">Тип содержимого ответа.</typeparam>
    public class VKResponse<T>
    {
        /// <summary>
        /// Объект ответа сервера ВКонтакте.
        /// </summary>
        public T response { get; set; }

        public List<VKError> execute_errors { get; set; }

        /// <summary>
        /// Объект ошибки ВКонтакте.
        /// </summary>
        public VKError error { get; set; }

        public VKResponse()
        {
            error = new VKError() { error_code = VKErrors.None };
        }
    }

    /// <summary>
    /// Представляет собой ошибку ВКонтакте.
    /// </summary>
    public sealed class VKError
    {
        public string method { get; set; }

        /// <summary>
        /// Тип ошибки. 
        /// Имеет значение None, если ошибки не произошло.
        /// </summary>
        public VKErrors error_code { get; set; }

        public string error_msg { get; set; }
    }
}
