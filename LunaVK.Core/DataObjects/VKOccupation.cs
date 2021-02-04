using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// информация о текущем роде занятия пользователя
    /// </summary>
    public class VKOccupation
    {
        /// <summary>
        /// тип
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public OccupationType type { get; set; }

        /// <summary>
        /// идентификатор школы, вуза, сообщества компании
        /// </summary>
        public long id { get; set; }

        /// <summary>
        /// название школы, вуза или места работы
        /// </summary>
        public string name { get; set; }

        public enum OccupationType
        {
            /// <summary>
            /// работа
            /// </summary>
            work,

            /// <summary>
            /// среднее образование;
            /// </summary>
            school,

            /// <summary>
            ///  высшее образование. 
            /// </summary>
            university,
        }
    }
}
