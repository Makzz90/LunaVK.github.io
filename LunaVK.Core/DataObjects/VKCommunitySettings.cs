using LunaVK.Core.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.DataObjects
{
    public sealed class VKCommunitySettings
    {
        /// <summary>
        /// название сообщества.
        /// (set)
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// описание сообщества.
        /// (set)
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// короткий адрес. 
        /// </summary>
        public string address { get; set; }

        /// <summary>
        /// объект, описывающий место. 
        /// </summary>
        public VKPlace place { get; set; }

        /// <summary>
        /// настройки стены.
        /// 0 — выключена;
        /// 1 — открытая;
        /// 2 — ограниченная (доступно только для групп и событий);
        /// 3 — закрытая (доступно только для групп и событий).
        /// </summary>
        public int wall { get; set; }

        /// <summary>
        /// настройки фотографий.
        /// 0 — выключены;
        /// 1 — открытые;
        /// 2 — ограниченные (доступно только для групп и событий).
        /// </summary>
        public int photos { get; set; }

        /// <summary>
        /// настройки видеозаписей.
        /// 0 — выключены;
        /// 1 — открытые;
        /// 2 — ограниченные (доступно только для групп и событий).
        /// </summary>
        public int video { get; set; }

        /// <summary>
        /// настройки аудиозаписей.
        /// 0 — выключены;
        /// 1 — открытые;
        /// 2 — ограниченные (доступно только для групп и событий).
        /// </summary>
        public int audio { get; set; }

        /// <summary>
        /// настройки документов.
        /// 0 — выключены;
        /// 1 — открытые;
        /// 2 — ограниченные (доступно только для групп и событий).
        /// </summary>
        public int docs { get; set; }

        /// <summary>
        /// настройки обсуждений.
        /// 0 — выключены;
        /// 1 — открытые;
        /// 2 — ограниченные (доступно только для групп и событий).
        /// </summary>
        public int topics { get; set; }

        /// <summary>
        /// настройки вики-страниц.
        /// 0 — выключены;
        /// 1 — открытые;
        /// 2 — ограниченные (доступно только для групп и событий). todo: ХМ, этого похоже нет
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool wiki { get; set; }

        /// <summary>
        /// сообщения сообщества
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool messages { get; set; }

        /// <summary>
        /// фильтр нецензурных выражений в комментариях
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool obscene_filter { get; set; }

        /// <summary>
        /// настройки фильтра комментариев по ключевым словам. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool obscene_stopwords { get; set; }

        /// <summary>
        /// список стоп-слов. 
        /// </summary>
        public string[] obscene_words { get; set; }

        /// <summary>
        /// категория публичной страницы. 
        /// </summary>
        public int public_category { get; set; }

        /// <summary>
        /// подкатегория публичной страницы. 
        /// </summary>
        public int public_subcategory { get; set; }

        /// <summary>
        /// список возможных категорий для публичных страниц. 
        /// </summary>
        public List<Section> public_category_list { get; set; }

        /// <summary>
        /// тип сообщества.
        /// 0 — открытая;
        /// 1 — закрытая;
        /// 2 — частная.
        /// </summary>
        public int access { get; set; }

        /// <summary>
        /// идентификатор тематики.
        /// 1 — авто/мото;
        /// 2 — активный отдых;
        /// 3 — бизнес;
        /// 4 — домашние животные;
        /// 5 — здоровье;
        /// 6 — знакомство и общение;
        /// 7 — игры;
        /// 8 — ИТ (компьютеры и софт);
        /// 9 — кино;
        /// 10 — красота и мода;
        /// 11 — кулинария;
        /// 12 — культура и искусство;
        /// 13 — литература;
        /// 14 — мобильная связь и интернет;
        /// 15 — музыка;
        /// 16 — наука и техника;
        /// 17 — недвижимость;
        /// 18 — новости и СМИ;
        /// 19 — безопасность;
        /// 20 — образование;
        /// 21 — обустройство и ремонт;
        /// 22 — политика;
        /// 23 — продукты питания;
        /// 24 — промышленность;
        /// 25 — путешествия;
        /// 26 — работа;
        /// 27 — развлечения;
        /// 28 — религия;
        /// 29 — дом и семья;
        /// 30 — спорт;
        /// 31 — страхование;
        /// 32 — телевидение;
        /// 33 — товары и услуги;
        /// 34 — увлечения и хобби;
        /// 35 — финансы;
        /// 36 — фото;
        /// 37 — эзотерика;
        /// 38 — электроника и бытовая техника;
        /// 39 — эротика;
        /// 40 — юмор;
        /// 41 — общество, гуманитарные науки;
        /// 42 — дизайн и графика.
        /// </summary>
        public int subject { get; set; }

        /// <summary>
        /// список возможных тематик. Массив объектов, каждый из которых содержит поля:
        /// id (integer) — идентификатор тематики;
        /// name (string) — название тематики. 
        /// </summary>
        public List<Section> subject_list { get; set; }

        /// <summary>
        /// адрес RSS-ленты. 
        /// </summary>
        public string rss { get; set; }

        /// <summary>
        /// адрес веб-сайта.
        /// (set)
        /// </summary>
        public string website { get; set; }

        /// <summary>
        /// возрастные ограничения
        ///1 — нет ограничений;
        ///2 — 16+;
        ///3 — 18+.
        /// </summary>
        public int age_limits { get; set; }

        /// <summary>
        /// настройки блока товаров.
        /// </summary>
        public Market market { get; set; }


        //////// Нет в документации: ------------------------------

        /// <summary>
        /// контакты (доступно только для публичных страниц)
        /// </summary>
        //            [JsonConverter(typeof(VKBooleanConverter))]
        public int contacts { get; set; }

        /// <summary>
        /// ссылки (доступно только для публичных страниц)
        /// </summary>
        //            [JsonConverter(typeof(VKBooleanConverter))]
        public int links { get; set; }

        /// <summary>
        /// события (доступно только для публичных страниц).
        /// </summary>
        //            [JsonConverter(typeof(VKBooleanConverter))]
        public int events { get; set; }

        /// <summary>
        /// места (доступно только для публичных страниц)
        /// </summary>
        //            [JsonConverter(typeof(VKBooleanConverter))]
        public int places { get; set; }

        public string public_date { get; set; }
        public string public_date_label { get; set; }

        /// <summary>
        /// Статьи
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool articles { get; set; }

        /*

        

        public long event_group_id { get; set; }

        public string email { get; set; }
        */

        public string phone { get; set; }
        /*
        public User event_creator { get; set; }

        public List<Group> event_available_organizers { get; set; }

        public int? start_date { get; set; }

        public int? finish_date { get; set; }
        */
        public List<object> sections_list { get; set; }

        public int main_section { get; set; }

        public int secondary_section { get; set; }

        //
        //
        



        public class Section
        {
            public int id { get; set; }

            public string name { get; set; }

            //subtypes_list
            public List<Section> subcategories { get; set; }
        }

        public class Market
        {
            /// <summary>
            /// включен ли блок товаров
            /// </summary>
            [JsonConverter(typeof(VKBooleanConverter))]
            public bool enabled { get; set; }

            /// <summary>
            /// включены ли комментарии к товарам
            /// </summary>
            [JsonConverter(typeof(VKBooleanConverter))]
            public bool comments_enabled { get; set; }

            //country_ids (array) — идентификаторы стран; 
            //city_ids (array) — идентификаторы городов; 

            /// <summary>
            /// идентификатор контактного лица; 
            /// </summary>
            public int contact_id { get; set; }

            //public VKGroupMarket.VKCurrency currency { get; set; }//объект, описывающий валюту.
        }
        
        
    }
}
