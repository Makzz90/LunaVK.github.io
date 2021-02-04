using LunaVK.Core.Enums;
using System;

namespace LunaVK.Core.Library
{
    /// <summary>
    /// Указывает на поддержку подгрузки элементов в конец списка. Для фонововй ветки!
    /// </summary>
    public interface ISupportDownIncrementalLoading
    {

        /// <summary>
        /// Имеются ли еще элемнты для подгрузки в конец списка.
        /// </summary>
        bool HasMoreDownItems { get; }


        /// <summary>
        /// Загрузить следующие элементы.
        /// </summary>
        void LoadDownAsync(bool InReload = false);

        Action<ProfileLoadingStatus> LoadingStatusUpdated { get; set; }

        ProfileLoadingStatus CurrentLoadingStatus { get; }
        void Reload();
    }
}
