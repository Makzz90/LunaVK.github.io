namespace LunaVK.Core.Enums
{
    /// <summary>
    /// Тип источника записи.
    /// </summary>
    public enum VKPostSourceType
    {
        /// <summary>
        /// Запись создана через интерфейс сайта.
        /// </summary>
        VK,
        /// <summary>
        /// Запись создана через виджет на стороннем сайте.
        /// </summary>
        Widget,
        /// <summary>
        /// Запись создана приложением через API.
        /// </summary>
        API,
        /// <summary>
        /// Запись создана посредством импорта RSS-ленты со стороннего сайта.
        /// </summary>
        RSS,
        /// <summary>
        /// Запись создана посредством отправки SMS-сообщения на специальный номер.
        /// </summary>
        SMS
    }
}
