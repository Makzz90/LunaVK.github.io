namespace LunaVK.Core.DataObjects
{
    public enum ReportReason : byte
    {
        /// <summary>
        /// это спам
        /// </summary>
        Spam,

        /// <summary>
        /// детская порнография
        /// </summary>
        ChildPorn,

        /// <summary>
        /// экстремизм
        /// </summary>
        Extremism,

        /// <summary>
        /// насилие
        /// </summary>
        Violence,

        /// <summary>
        /// пропаганда наркотиков
        /// </summary>
        Drugs,

        /// <summary>
        /// материал для взрослых
        /// </summary>
        Adult,

        /// <summary>
        /// оскорбление
        /// </summary>
        Abuse,
    }
}
