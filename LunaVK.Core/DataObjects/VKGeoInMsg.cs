using Newtonsoft.Json;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// информация о местоположении
    /// </summary>
    public sealed class VKGeoInMsg
    {
        /// <summary>
        /// Тип места.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// координаты места
        /// Класс, если в сообщении
        /// Строка, если в посте
        /// </summary>
        public Coord coordinates { get; set; }

        /// <summary>
        /// Описание места (если добавлено).
        /// </summary>
        public VKPlace place { get; set; }

        public class Coord
        {
            /// <summary>
            /// географическая широта; 
            /// </summary>
            public long latitude { get; set; }

            /// <summary>
            /// географическая долгота. 
            /// </summary>
            public long longitude { get; set; }
        }
    }
}
