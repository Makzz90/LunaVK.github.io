using LunaVK.Core.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.DataObjects
{
    public sealed class VKStatsResponse
    {
        /// <summary>
        /// период начала отсчёта в формате Unixtime
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime period_from { get; set; }

        /// <summary>
        /// период окончания отсчёта в формате Unixtime
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime period_to { get; set; }

        public Visitors visitors { get; set; }

        public Activity activity { get; set; }

        /// <summary>
        /// данные о посетителях и просмотрах
        /// </summary>
        public class Visitors
        {
            public int mobile_views { get; set; }

            /// <summary>
            /// число просмотров
            /// </summary>
            public int views { get; set; }
            
            /// <summary>
            /// число посетителей
            /// </summary>
            public int visitors { get; set; }



            public List<Age> age { get; set; }

            public List<City> cities { get; set; }

            public List<Country> countries { get; set; }

            public List<Sex> sex { get; set; }

            public List<SexAge> sex_age { get; set; }
        }

        /// <summary>
        /// данные об охвате
        /// </summary>
        public class Reach
        {
            /// <summary>
            /// полный охват
            /// </summary>
            public int reach { get; set; }

            /// <summary>
            /// охват с мобильных устройств
            /// </summary>
            public int mobile_reach { get; set; }

            /// <summary>
            /// охват подписчиков
            /// </summary>
            public int reach_subscribers { get; set; }




            public List<Age> age { get; set; }

            public List<City> cities { get; set; }

            public List<Sex> sex { get; set; }

            public List<SexAge> sex_age { get; set; }

            public List<Country> countries { get; set; }
        }


        /// <summary>
        /// статистика по возрасту
        /// </summary>
        public class Age
        {
            /// <summary>
            /// возрастной интервал
            /// 12-18, 18-21, 21-24, 24-27, 27-30, 30-35, 35-45, 45-100
            /// </summary>
            public string value { get; set; }

            /// <summary>
            /// число посетителей
            /// </summary>
            public int count { get; set; }
        }

        public class City
        {
            /// <summary>
            /// название города
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// идентификатор города или "other" для раздела «прочие города
            /// </summary>
            public int city_id { get; set; }

            /// <summary>
            /// число посетителей
            /// </summary>
            public int count { get; set; }
        }

        public class Sex
        {
            /// <summary>
            /// пол ("m" — мужской, "f" — женский)
            /// </summary>
            public string value { get; set; }

            /// <summary>
            /// число посетителей
            /// </summary>
            public int count { get; set; }
        }

        /// <summary>
        /// статистика по странам
        /// </summary>
        public class Country
        {
            /// <summary>
            /// название страны
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// двухбуквенный код страны (например, "RU")
            /// </summary>
            public string code { get; set; }

            /// <summary>
            /// идентификатор страны
            /// </summary>
            public int country_id { get; set; }

            /// <summary>
            /// число посетителей
            /// </summary>
            public int count { get; set; }
        }

        /// <summary>
        /// статистика по полу и возрасту.
        /// </summary>
        public class SexAge
        {
            /// <summary>
            /// пол и возрастной интервал (например, "f;12-18")
            /// </summary>
            public string value { get; set; }

            /// <summary>
            /// число посетителей
            /// </summary>
            public int count { get; set; }
        }

        public class Activity
        {
            public int comments { get; set; }

            public int likes { get; set; }

            public int subscribed { get; set; }

            public int unsubscribed { get; set; }
        }
    }
}
