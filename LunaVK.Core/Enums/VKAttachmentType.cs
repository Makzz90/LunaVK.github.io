using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.Enums
{
    /// <summary>
    /// Перечисление типов вложений.
    /// </summary>
    public enum VKAttachmentType : byte
    {
        /// <summary>
        /// Фотография из альбома.
        /// </summary>
        Photo = 0,

        /// <summary>
        /// Видеозапись.
        /// </summary>
        Video,

        /// <summary>
        /// Аудиозапись.
        /// </summary>
        Audio,

        /// <summary>
        /// Документ.
        /// </summary>
        Doc,

        /// <summary>
        /// Ссылка на Web-страницу.
        /// </summary>
        Link,

        Market,

        Market_Album,

        /// <summary>
        /// Запись со стены.
        /// </summary>
        Wall,

        /// <summary>
        /// Комментарий к записи на стене.
        /// </summary>
        Wall_reply,

        /// <summary>
        /// Стикер.
        /// </summary>
        Sticker,

        /// <summary>
        /// Подарок.
        /// </summary>
        Gift,

        /// <summary>
        /// Опрос.
        /// </summary>
        Poll,

        /// <summary>
        /// Граффити.
        /// </summary>
        Graffiti,
        
        
        /// <summary>
        /// Заметка.
        /// </summary>
        Note,
        /*
        /// <summary>
        /// Фотография, загруженная сторонним приложением.
        /// </summary>
        App,
        /// <summary>
        /// Опрос.
        /// </summary>
        Poll,
        /// <summary>
        /// Вики-страница.
        /// </summary>
        Page,
        /// <summary>
        /// Альбом с фотографиями.
        /// </summary>
        Album,*/

        /// <summary>
        /// Список фотографий, размещенных в одном посте.
        /// </summary>
        Photos_list,

        /// <summary>
        /// Альбом с фотографиями.
        /// </summary>
        Album,

        /// <summary>
        /// Вики-страница.
        /// </summary>
        Page,

        Posted_Photo,//no in doc

        Pretty_Cards,

        Audio_Message,

        Podcast,

        Call,

        /// <summary>
        /// Сюжет
        /// </summary>
        Narrative,

        Event,

        Story,

        Situational_Theme,

        //============CUSTOM==========
        Geo,

        Repost,

        Emoji
    }
}
