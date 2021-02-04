namespace YoutubeExtractor
{
    public enum AudioType
    {
        /// <summary>
        /// Нет звука
        /// </summary>
        None,
        Aac,
        Mp3,
        Vorbis,

        /// <summary>
        /// The audio type is unknown. This can occur if YoutubeExtractor is not up-to-date.
        /// </summary>
        Unknown
    }
}