using System.Threading.Tasks;

namespace LunaVK.Core.Library
{
    /// <summary>
    /// Указывает на поддержку подгрузки элементов в начале и в конец списка.
    /// </summary>
    public interface ISupportUpDownIncrementalLoading
    {
        /// <summary>
        /// Имеются для еще элементы для подгрузки в начало списка.
        /// </summary>
        bool HasMoreUpItems { get; }

        /// <summary>
        /// Имеются ли еще элемнты для подгрузки в конец списка.
        /// </summary>
        bool HasMoreDownItems { get; }

        /// <summary>
        /// Подгрузить следующую порцию элементов в начало списка.
        /// </summary>
        Task LoadUpAsync();

        /// <summary>
        /// Загрузить следующие элементы.
        /// </summary>
        Task LoadDownAsync(bool InReload = false);

        Task<object> Reload();
    }
}
