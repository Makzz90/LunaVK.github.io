namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Представляет диалог ВКонтакте.
    /// </summary>
    public class VKDialog
    {
        /// <summary>
        /// Количество непрочитанных входящих сообщений в диалоге.
        /// (если все сообщения прочитаны, поле не возвращается)
        /// </summary>
        public int unread { get; set; }

        /// <summary>
        /// Личное сообщение ВКонтакте.
        /// </summary>
        public VKMessage message { get; set; }

        /// <summary>
        /// идентификатор последнего сообщения, прочитанного текущим пользователем
        /// </summary>
        public int in_read { get; set; }

        /// <summary>
        /// идентификатор последнего сообщения, прочитанного собеседником
        /// </summary>
        public int out_read { get; set; }
    }
}
