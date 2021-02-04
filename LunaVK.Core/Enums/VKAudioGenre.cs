using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.Enums
{
    /// <summary>
    /// Перечисление жанров аудиозаписей.
    /// </summary>
    public enum VKAudioGenre : byte
    {
        /// <summary>
        /// Жанр неизвестен.
        /// </summary>
        Unknown = 0,
        Rock = 1,
        Pop = 2,
        RapAndHipHop = 3,
        EasyListening = 4,
        DanceAndHouse = 5,
        Instrumental = 6,
        Metal = 7,
        Dubstep = 8,
        JazzAndBlues = 9,
        DrumAndBass = 10,
        Trance = 11,
        Chanson = 12,
        Ethnic = 13,
        AcousticAndVocal = 14,
        Reggae = 15,
        Classical = 16,
        IndiePop = 17,
        Other = 18,
        Speech = 19,
        Alternative = 21,
        ElectropopAndDisco = 22
    }
}
