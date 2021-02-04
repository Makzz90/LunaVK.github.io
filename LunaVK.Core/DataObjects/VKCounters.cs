using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.DataObjects
{
    public class VKCounters
    {
        /// <summary>
        /// количество фотоальбомов; 
        /// </summary>
        public int albums { get; set; }//g

        /// <summary>
        /// количество видеозаписей; 
        /// </summary>
        public int videos { get; set; }//g

        /// <summary>
        /// количество аудиозаписей; 
        /// </summary>
        public int audios { get; set; }//g

        /// <summary>
        /// Количество заметок.
        /// </summary>
        public int notes { get; set; }

        /// <summary>
        /// количество сообществ; 
        /// </summary>
        public int groups { get; set; }

        public int gifts { get; set; }

        /// <summary>
        /// количество фотографий; 
        /// </summary>
        public int photos { get; set; }//g

        /// <summary>
        /// Количество друзей.
        /// </summary>
        public int friends { get; set; }

        /// <summary>
        /// Количество друзей в сети.
        /// </summary>
        public int online_friends { get; set; }

        /// <summary>
        /// количество общих друзей; 
        /// </summary>
        public int mutual_friends { get; set; }

        /// <summary>
        /// количество видеозаписей с пользователем; 
        /// </summary>
        public int user_videos { get; set; }

        /// <summary>
        /// количество фотографий с пользователем;
        /// нет в документации
        /// </summary>
        public int user_photos { get; set; }

        /// <summary>
        /// количество подписчиков; 
        /// </summary>
        public int followers { get; set; }

        public int subscriptions { get; set; }

        public int topics { get; set; }//g

        /// <summary>
        /// Документы
        /// </summary>
        public int docs { get; set; }//g

        /// <summary>
        /// количество объектов в блоке «Интересные страницы». 
        /// </summary>
        public int pages { get; set; }

        public int market { get; set; }

        public int podcasts { get; set; }

        /// <summary>
        /// Статьи
        /// </summary>
        public int articles { get; set; }

        public int addresses { get; set; }

        /// <summary>
        /// Клипы
        /// </summary>
        public int clips { get; set; }

        public int clips_followers { get; set; }
    }
}
