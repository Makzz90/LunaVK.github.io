using System.Collections.Generic;
using System.Linq;
using LunaVK.Core.DataObjects;
using System.Threading.Tasks;
using LunaVK.Core.Network;


namespace LunaVK.Core.Emoji
{
    public class StickersSettings
    {
        private static StickersSettings _instance;
        public static StickersSettings Instance
        {
            get { return StickersSettings._instance ?? (StickersSettings._instance = new StickersSettings()); }
        }

        /*
         * return API.store.getStockItems({type:"stickers",filters:"active"});
         
         */
        public async Task<StoreCatalog> GetStickers(List<Enums.StoreProductFilter> productFilters = null)
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
                var imgNode = packNode.FirstOrDefault((img)=>img.Name=="img");
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
                item.photo_70 = "https://vk.com//images/store/stickers/"+ pack_id +"/cover_70b.png";
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
    }
}
