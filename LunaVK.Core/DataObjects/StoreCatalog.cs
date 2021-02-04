using LunaVK.Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.DataObjects
{
    public class StoreCatalog// : IAccountStickersData
    {
        public List<StoreBanner> banners = new List<StoreBanner>();

        public List<StoreSection> sections = new List<StoreSection>();

        //public int? NewStoreItemsCount { get; set; }

        //public bool? HasStickersUpdates { get; set; }

        //public VKCountedItemsObject<StoreProduct> Products { get; set; }

        //public List<StockItem> StockItems { get; set; }

        //public StoreStickers RecentStickers { get; set; }


        public class StoreSection
        {
            public StoreSection(string id, string header/*, string img_296, string img_70*/ )
            {
                name = id;
                title = header;
                packs = new List<StockItem>();
            }

            /// <summary>
            /// ИД
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// Название для пользователя
            /// </summary>
            public string title { get; set; }

            public List<StockItem> packs { get; set; }




            public string Title { get { return title; } }
        }

        public class StoreBanner
        {
            public StoreBanner(string thumb, uint pack_id)
            {
                this.photo_640 = thumb;
                id = pack_id;
            }
            //public string type { get; set; }

            //public StockItem stock_item { get; set; }

            //public StoreSection section { get; set; }

            //public string photo_480 { get; set; }

            public string photo_640 { get; set; }

            //public string photo_960 { get; set; }

            //public string photo_1280 { get; set; }



            public uint id { get; set; }
        }
    }
}
