using LunaVK.Core.Enums;

namespace LunaVK.Core.Library
{
    public interface ISupportUpIncrementalLoading
    {
        /// <summary>
        /// Имеются для еще элементы для подгрузки в начало списка.
        /// </summary>
        bool HasMoreUpItems { get; }

        /// <summary>
        /// Подгрузить следующую порцию элементов в начало списка.
        /// </summary>
        void LoadUpAsync();

        ProfileLoadingStatus CurrentLoadingStatus { get; }
    }
}
