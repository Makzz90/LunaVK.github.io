using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.DataObjects
{
    public class VKAd
    {
        /// <summary>
        /// тип объявления, всегда равен post
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// возрастная метка
        /// </summary>
        public string age_restriction { get; set; }

        /// <summary>
        /// строка для регистрации событий методом adsint.registerAdEvents
        /// и для действий в методах adsint.hideAd и adsint.reportAd.
        /// Подробнее в разделе 4. Регистрация событий. 
        /// </summary>
        public string ad_data { get; set; }

        /// <summary>
        /// строка для регистрации событий методом adsint.registerAdEvents
        /// </summary>
        public string ad_data_impression { get; set; }

        //        public long time_to_live { get; set; }

        /// <summary>
        ///  объект записи на стене.
        ///  В поле post_type содержится значение post_ads.
        ///  Дополнительно объект может содержать поле track_code (string),
        ///  необходимое для регистрации событий. 
        /// </summary>
        public VKWallPost post { get; set; }

        /// <summary>
        /// массив ссылок на пиксели для регистрации событий
        /// </summary>
        public List<AdStatistics> statistics { get; set; }
        /*
        public VKAd()
        {
            this.statistics = new List<AdStatistics>();
        }
        */
        public class AdStatistics
        {
            /// <summary>
            /// тип события
            /// load — данные об объявлении получены через API; 
            /// impression —объявление показано пользователю;
            /// click_post_owner — переход по заголовку поста (в группу);
            /// click_post_link — переход по ссылке в посте (включая хэштег, упоминание, внутренние ссылки ВК и внешние);
            /// </summary>
            public string type { get; set; }

            /// <summary>
            /// ссылка на пиксель, возможно с промежуточными редиректами.
            /// </summary>
            public string url { get; set; }
        }
    }
}
