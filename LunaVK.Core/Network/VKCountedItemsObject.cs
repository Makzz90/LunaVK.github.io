using Newtonsoft.Json;
using System.Collections.Generic;
using LunaVK.Core.DataObjects;

namespace LunaVK.Core.Network
{
    /// <summary>
    /// Представляет собой списковый ответ ВКонтакте на запрос.
    /// </summary>
    public class VKCountedItemsObject<T>
    {
        /// <summary>
        /// Общее количество элементов.
        /// </summary>
        public uint count { get; set; }

        /// <summary>
        /// Коллекция объектов.
        /// </summary>
        public List<T> items { get; set; }

        /// <summary>
        /// Список пользователей.
        /// </summary>
        public List<VKUser> profiles { get; set; }

        /// <summary>
        /// Список сообществ.
        /// </summary>
        public List<VKGroup> groups { get; set; }

        public string next_from { get; set; }
    }
}
