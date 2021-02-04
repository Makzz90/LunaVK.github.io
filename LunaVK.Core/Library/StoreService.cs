using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunaVK.Core.Library
{
    public class StoreService
    {
        private static StoreService _instance;
        public static StoreService Instance
        {
            get { return StoreService._instance ?? (StoreService._instance = new StoreService()); }
        }

        public void GetStickersKeywords(Action<VKResponse< StickersAutoSuggestDictionary.StickersKeywordsData>> callback)
        {
            /*
             *  b("need_stickers", 0);
    if (paramInt >= 0) {
      b("chunk", paramInt);
    }
    if (paramString.length() > 0) {
      paramInt = i;
    } else {
      paramInt = 0;
    }
    if (paramInt != 0) {
      c("chunks_hash", paramString);
    }
             * */
            //string code = "return API.store.getStickersKeywords({aliases:1,all_products:1});";

            //VKRequestsDispatcher.Execute<StickersAutoSuggestDictionary.StickersKeywordsData>(code, callback, null, true);



            VKRequestsDispatcher.DispatchRequestToVK<StickersAutoSuggestDictionary.StickersKeywordsData>("store.getStickersKeywords", new Dictionary<string, string>()
              {
                { "aliases", "1" },//больше слов в подсказках
                //{ "all_products", "1" }// ничего?
                { "need_stickers", "1" }
              },
              callback, null, true);
        }

        public void GetStockItems(List<Enums.StoreProductFilter> productFilters = null, Action<VKResponse<VKCountedItemsObject<StockItem>>> callback = null)
        {
            /*
             * Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["type"] = productType.ToString();
            parameters["merchant"] = "microsoft";
            parameters["no_inapp"] = "1";
            if (productIds != null && productIds.Count > 0)
                parameters["product_ids"] = string.Join<long>(",", (IEnumerable<long>)productIds);
            if (productFilters != null && productFilters.Count > 0)
                parameters["filters"] = string.Join(",", productFilters.Select<StoreProductFilter, string>((Func<StoreProductFilter, string>)(filter => filter.ToString().ToLowerInvariant())));
            if (purchaseForId > 0L)
                parameters["purchase_for"] = purchaseForId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKList<StockItem>>("store.getStockItems", parameters, callback, null, false, true, new CancellationToken?(), null);
            */
            string code = "return API.store.getStockItems({type:\"stickers\",";
            if (productFilters != null && productFilters.Count > 0)
                code += ("filters:\"" + string.Join(",", productFilters.Select((filter => filter.ToString().ToLowerInvariant()))) + "\",");

            code += "merchant:\"microsoft\"});";

            VKRequestsDispatcher.Execute<VKCountedItemsObject<StockItem>>(code, callback);
        }

        public void GetStockItemByName(string name, Action<VKResponse<StockItem>> callback, CancellationToken? cancellationToken = null)
        {
            VKRequestsDispatcher.DispatchRequestToVK<StockItem>("store.getStockItemByName", new Dictionary<string, string>()
              {
                { "type", "stickers" },
                { "name", name },
                { "merchant", "microsoft" },
                { "no_inapp", "1" }
              },
              callback, null, false, cancellationToken);
        }

        public void GetStockItemByStickerId(uint stickerId, Action<VKResponse< StockItem>> callback, CancellationToken? cancellationToken = null)
        {
//#if DEBUG
            //await Task.Delay(3000);
            //var temp = await RequestsDispatcher.GetResponseFromDump<StockItem>("12691.json");
            //callback(temp);
//#else

            VKRequestsDispatcher.DispatchRequestToVK<StockItem>("store.getStockItemByStickerId", new Dictionary<string, string>()
              {
                { "sticker_id", stickerId.ToString() },
                { "merchant", "microsoft" },
                { "no_inapp", "1" }
              },
              callback, null, false, cancellationToken);
//#endif
        }

        public async Task<StoreCatalog> GetStickersStoreCatalog()
        {
            /*
            string code = "return API.store.getStockItems({type:\"stickers\",";
            if (productFilters != null && productFilters.Count > 0)
                code += ("filters:\""+string.Join(",", productFilters.Select((filter => filter.ToString().ToLowerInvariant())))+"\",");

            code += "merchant:\"microsoft\"});";
            
            var temp = await Network.RequestsDispatcher.Execute<Network.VKCountedItemsObject<StockItem>>(code);
            
            if (temp.error.error_code != Enums.VKErrors.None)
                return null;

            return temp.response.items;*/


            //VKRequestsDispatcher.DispatchRequestToVK<StoreCatalog>("execute.getStickersStoreCatalog", parameters, callback,  null, false, true, new CancellationToken?(),  null);

            /*
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("act", "stickers_store");
            parameters.Add("al", "1");
            parameters.Add("box", "1");

            string html = await RequestsDispatcher.PostAsync("https://vk.com/al_im.php", parameters);

            if (string.IsNullOrEmpty(html))//no internet
                return null;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html.Replace("<!--", "< !--"));

            StoreCatalog catlog = new StoreCatalog();

            IEnumerable<HtmlNode> bodyNode = doc.DocumentNode.Descendants().ToList();

#region Banners
            var im_stickers_banners = bodyNode.FirstOrDefault((node) => node.Attributes.Contains("class") && node.Name == "div" && node.Attributes["class"].Value == "im_stickers_banners");

            foreach (var banner in im_stickers_banners.ChildNodes)
            {
                string temp = banner.Attributes["onclick"].Value;

                string onclick = temp.Replace("Emoji.previewSticker(", "").Replace(", this, false, event);", "");

                uint pack_id = 0;
                if (!uint.TryParse(onclick, out pack_id))
                    continue;


                string src = banner.FirstChild.Attributes["src"].Value;

                catlog.banners.Add(new StoreCatalog.StoreBanner(src, pack_id));
                //     System.Diagnostics.Debug.WriteLine(pack_id + " " + src);
            }
#endregion

#region Вкладки
            //<ul class="ui_tabs clear_fix "onmouseover="uiTabs.tryInit(this)" >
            var ui_tabs = bodyNode.FirstOrDefault((node) => node.Attributes.Contains("class") && node.Name == "ul" && node.Attributes["class"].Value.StartsWith("ui_tabs"));
            if (ui_tabs == null)
                return null;

            List<StoreCatalog.StoreSection> sections = new List<StoreCatalog.StoreSection>();

            IEnumerable<HtmlNode> tabsNode = ui_tabs.ChildNodes.Descendants().ToList();

            foreach (var tab in tabsNode)
            {
                if (tab.Attributes.Contains("class") && tab.Attributes["class"].Value.StartsWith("ui_tab"))
                {
                    //curBox().tbTab(this, 'summer')
                    string temp = tab.Attributes["onclick"].Value;
                    string onclick = temp.Replace("curBox().tbTab(this, '", "").Replace("')", "");
                    string id = onclick;
                    string title = tab.InnerText.Trim();

                    sections.Add(new StoreCatalog.StoreSection(id, title));
                }
            }

            catlog.sections = sections;
#endregion

            
            //<a href="/stickers/vovchik" class="im_sticker_bl" onclick="return Emoji.previewSticker(286, this, false, event);"> 
            //<div class="im_sticker_bl_demo"> 
            //<img class="im_sticker_bl_demo_thumb" src="https://vk.com//images/store/stickers/286/preview1_296.jpg" width="296" height="188" /> 
            //</div> 
            //<div class="im_sticker_bl_info clear"> <div class="im_sticker_bl_act"><button class="flat_button _sticker_btn_286" onclick="return Emoji.buyStickers(286, event, this, '328ae5f4a361c33776', 'store')" onmouseover="showTooltip(this, {text: 'Купить набор', shift: [0,6,6], showdt: 0, black: 1, appendParentCls: curBox() ? 'box_body' : 'page_block'});">63 руб.</button></div> <div class="im_sticker_bl_name">Вовчик<span class="im_sticker_bl_promo">New</span></div> <div class="im_sticker_bl_desc">Антон Андреев</div>
            //</div>
            //</a>
            
            var im_packs = bodyNode.Where((node) => node.Attributes.Contains("class") && node.Name == "a" && node.Attributes["class"].Value == "im_sticker_bl");
            foreach (var pack in im_packs)
            {
                string link = pack.Attributes["href"].Value;

                string temp = pack.Attributes["onclick"].Value;

                string onclick = temp.Replace("return Emoji.previewSticker(", "").Replace(", this, false, event);", "");

                uint pack_id = 0;
                if (!uint.TryParse(onclick, out pack_id))
                    continue;




                IEnumerable<HtmlNode> packNode = pack.Descendants().ToList();
                var imgNode = packNode.FirstOrDefault((img) => img.Name == "img");
                if (imgNode == null)
                    continue;
                var nameNode = packNode.FirstOrDefault((n) => n.Name == "div" && n.Attributes["class"].Value.StartsWith("im_sticker_bl_name"));
                //<a href="/stickers/vovchik" class="im_sticker_bl" onclick="return Emoji.previewSticker(286, this, false, event);"> 

                //<button class="flat_button _sticker_btn_286" onclick="return Emoji.buyStickers(286, event, this, '328ae5f4a361c33776', 'store')" ">63 руб.</button>
                var buttonNode = packNode.FirstOrDefault((n) => n.Name == "button" && n.Attributes["class"].Value.StartsWith("flat_button"));

                //<div class="im_sticker_bl_desc">Антон Андреев</div>
                var authorNode = packNode.FirstOrDefault((n) => n.Name == "div" && n.Attributes["class"].Value.StartsWith("im_sticker_bl_desc"));

                string src = imgNode.Attributes["src"].Value;
                string title = nameNode.InnerText;
                string price = buttonNode.InnerText;
                string author = authorNode.InnerText;

                StockItem item = new StockItem();
                item.author = author;
                item.photo_296 = src;
                item.photo_70 = "https://vk.com//images/store/stickers/" + pack_id + "/cover_70b.png";
                item.price_str = price;
                item.product = new StoreProduct() { title = title, base_url = link };
                sections[1].packs.Add(item);
            }
            //https://vk.com//images/store/stickers/286/preview1_296.jpg большое превью
            //https://vk.com//images/store/stickers/285/cover_70b.png мини превью для телефона

            return catlog;
            */
            return null;
        }

        public void ActivateProduct(int productId, Action<VKResponse<int>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<int>("store.activateProduct", new Dictionary<string, string>()
            {
                { "type", "stickers" },
                { "product_id", productId.ToString() }
            }, callback);
        }

        public void DeactivateProduct(int productId, Action<VKResponse<int>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<int>("store.deactivateProduct", new Dictionary<string, string>()
            {
                { "type", "stickers" },
                { "product_id", productId.ToString() }
            }, callback);
        }

        public void ReorderProducts(int productId, int after = 0, int before = 0, Action<VKResponse<int>> callback = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["type"] = "stickers";
            parameters["product_id"] = productId.ToString();

            if (after > 0)
                parameters["after"] = after.ToString();
            if (before > 0)
                parameters["before"] = before.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("store.reorderProducts", parameters, callback);
        }
    }
}
