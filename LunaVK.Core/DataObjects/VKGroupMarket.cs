using Newtonsoft.Json;
using LunaVK.Core.Json;

namespace LunaVK.Core.DataObjects
{
    public class VKGroupMarket
    {
        /// <summary>
        /// включен ли блок товаров в сообществе
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool enabled { get; set; }

        /// <summary>
        /// минимальная цена товаров
        /// </summary>
        public int price_min { get; set; }

        /// <summary>
        /// максимальная цена товаров
        /// </summary>
        public int price_max { get; set; }

        /// <summary>
        /// идентификатор главной подборки товаров
        /// </summary>
        public int main_album_id { get; set; }

        /// <summary>
        ///  идентификатор контактного лица для связи с продавцом
        /// </summary>
        public int contact_id { get; set; }

        /// <summary>
        /// информация о валюте
        /// </summary>
        public VKCurrency currency { get; set; }

        /// <summary>
        /// строковое обозначение
        /// </summary>
        public string currency_text { get; set; }//todo: эта строка здесь?

        public class VKCurrency
        {
            /*
            public static readonly Currency RUB = new Currency()
            {
                id = 643,
                name = "RUB"
            };
            public static readonly Currency UAH = new Currency()
            {
                id = 980,
                name = "UAH"
            };
            public static readonly Currency KZT = new Currency()
            {
                id = 398,
                name = "KZT"
            };
            public static readonly Currency EUR = new Currency()
            {
                id = 978,
                name = "EUR"
            };
            public static readonly Currency USD = new Currency()
            {
                id = 840,
                name = "USD"
            };
            */
            public int id { get; set; }

            public string name { get; set; }
        }
    }
}
