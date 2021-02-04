using LunaVK.Core.Enums;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Общие для пользователя и сообщества данные
    /// </summary>
    public interface VKBaseDataForGroupOrUser
    {
        /// <summary>
        /// П.С. У группы вернётся минус
        /// </summary>
        int Id { get; }

        string Title { get; }
        
        string MinPhoto { get; }

        bool IsVerified { get; }

        VKIsDeactivated Deactivated { get; }

        VKCounters Counters { get; }

        bool IsSubscribed { get; set; }

        bool IsFavorite { get; set; }

        VKGroupMainSection MainSectionType { get; }
    }
}
