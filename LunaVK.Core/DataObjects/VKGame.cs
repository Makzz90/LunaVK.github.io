using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using LunaVK.Core.Json;
using LunaVK.Core.Enums;

namespace LunaVK.Core.DataObjects
{
    public class VKGame
    {
        /// <summary>
        /// идентификатор приложения. 
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// название приложения.
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// url-адрес обложки приложения шириной 75px. 
        /// </summary>
        public string icon_75 { get; set; }

        /// <summary>
        /// url-адрес обложки приложения шириной 139px. 
        /// </summary>
        public string icon_139 { get; set; }

        /// <summary>
        /// url-адрес обложки приложения шириной 150px. 
        /// </summary>
        public string icon_150 { get; set; }

        /// <summary>
        /// url-адрес обложки приложения шириной 278px. 
        /// </summary>
        public string icon_278 { get; set; }
        
        /// <summary>
        /// url-адрес баннера шириной 560px. 
        /// </summary>
        public string banner_560 { get; set; }

        /// <summary>
        /// url-адрес баннера шириной 1120px. 
        /// </summary>
        public string banner_1120 { get; set; }

        /// <summary>
        /// тип приложения. Возможные значения: •app — социальное приложение; 
        /// •game — игра; 
        /// •site — подключаемый сайт; 
        /// •standalone — отдельное приложение (для мобильного устройства). 
        /// html5_game
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// категория приложения. 
        /// </summary>
        public string section { get; set; }

        /// <summary>
        /// адрес страницы автора приложения. 
        /// </summary>
        public string author_url { get; set; }

        /// <summary>
        /// идентификатор автора приложения. 
        /// </summary>
        public int author_id { get; set; }

        /// <summary>
        /// идентификатор официальной группы приложения. 
        /// </summary>
        public int author_group { get; set; }

        public int members_count { get; set; }//no in documentation

        /// <summary>
        /// дата размещения в Unixtime. 
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime published_date { get; set; }

//        public string install_url { get; set; }

        /// <summary>
        /// позиция в каталоге. 
        /// </summary>
        public int catalog_position { get; set; }//больше не возвращается?

        /// <summary>
        /// является ли приложение мультиязычным 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool international { get; set; }

        /// <summary>
        /// тип турнирной таблицы. Возможные значения: •0 — не поддерживается; 
        /// •1 — по уровню; 
        /// •2 — по очкам. 
        /// </summary>
        public int leaderboard_type { get; set; }

        /// <summary>
        /// идентификатор жанра 
        /// </summary>
        public int genre_id { get; set; }

        /// <summary>
        /// название жанра (например, Головоломка)
        /// </summary>
        public string genre { get; set; }

        /// <summary>
        /// идентификатор приложения в магазине приложений 
        /// </summary>
        public string platform_id { get; set; }

        /// <summary>
        /// доступно ли приложение в мобильном каталоге
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_in_catalog { get; set; }

        /// <summary>
        /// список идентификаторов друзей текущего пользователя,
        /// которые установили приложение
        /// (если был передан параметр return_friends = 1. 
        /// </summary>
        public List<int> friends { get; set; }

        /// <summary>
        /// если приложение установлено у текущего пользователя
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool installed { get; set; }

        /// <summary>
        /// если приложение — html5 игра
        /// </summary>
        //[JsonConverter(typeof(VKBooleanConverter))]
        //public bool is_html5_app { get; set; }//больше не возвращается

        /// <summary>
        /// поддерживаемая ориентация экрана. Возможные значения: •0 — альбомная и портретная; 
        /// •1 — только альбомная; 
        /// •2 — только портретная. 
        /// </summary>
        public int screen_orientation { get; set; }



        /////////////////////Объект содержит опциональные поля, если в методе был передан параметр extended = 1. 

        /// <summary>
        /// описание. 
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// короткий адрес приложения
        /// (или строка idXXXXXXX, если короткий адрес не задан). 
        /// </summary>
        public string screen_name { get; set; }

        /// <summary>
        /// url-адрес обложки приложения шириной 16px. 
        /// </summary>
        public string icon_16 { get; set; }

//        public int is_new { get; set; }

        /// <summary>
        /// массив объектов фотографий, описывающих скриншоты приложения. 
        /// </summary>
        //List<VKPhoto> screenshots { get; set; }//иная структура

        /// <summary>
        /// если у пользователя включены уведомления из этого приложения.
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool push_enabled { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool hide_tabbar { get; set; }//no in documentation



        public string mobile_iframe_url { get; set; }//no in documentation
    }
}
