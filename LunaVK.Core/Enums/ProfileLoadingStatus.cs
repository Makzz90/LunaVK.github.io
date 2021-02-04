namespace LunaVK.Core.Enums
{
    public enum ProfileLoadingStatus
    {
        /// <summary>
        /// Перезагрузка данных
        /// </summary>
        Reloading,

        /// <summary>
        /// Подгрузка данных
        /// </summary>
        Loading,
        
        /// <summary>
        /// Данные загружены
        /// </summary>
        Loaded,

        /// <summary>
        /// Ошибка подгрузки данных
        /// </summary>
        LoadingFailed,

        /// <summary>
        /// Ошибка загрузки данных
        /// </summary>
        ReloadingFailed,

        /// <summary>
        /// Аккаунт удалён
        /// </summary>
        Deleted,

        /// <summary>
        /// Аккаунт заблокирован
        /// </summary>
        Banned,

        /// <summary>
        /// Нет содержимого
        /// </summary>
        Empty,

        /// <summary>
        /// Я в чёрном списке
        /// </summary>
        Blacklisted,

        /// <summary>
        /// Группа приватна
        /// </summary>
        Private,

        /// <summary>
        /// Сервисный аккаунт
        /// </summary>
        Service,
    }
}
