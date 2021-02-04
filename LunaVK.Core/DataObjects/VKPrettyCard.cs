using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.DataObjects
{
    public class VKPrettyCards
    {
        public List<VKPrettyCard> cards { get; set; }
    }

    public class VKPrettyCard
    {
        /// <summary>
        /// идентификатор карточки
        /// </summary>
        public string card_id { get; set; }

        /// <summary>
        /// URL карточки
        /// </summary>
        public string link_url { get; set; }

        public string link_url_target { get; set; }

        /// <summary>
        /// подпись карточки
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// изображения
        /// </summary>
        public List<Image> images { get; set; }

        public Button button { get; set; }

        /// <summary>
        /// цена
        /// </summary>
        public string price { get; set; }

        /// <summary>
        /// старая цена
        /// </summary>
        public string price_old { get; set; }

        public class Button
        {
            public string title { get; set; }

            public Action action { get; set; }
        }

        public class Action
        {
            public string type { get; set; }
            public string url { get; set; }
            public string target { get; set; }
            public uint group_id { get; set; }
        }

        public class Image
        {
            public string url { get; set; }
            public uint width { get; set; }
            public uint height { get; set; }
        }
    }
}
