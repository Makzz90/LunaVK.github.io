using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using LunaVK.UC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;


namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestControls_Part2 : Page
    {
        public TestControls_Part2()
        {
            this.InitializeComponent();
            //this.Loaded += TestControls_Part2_Loaded;
        }

        /*
         * Overlay

Панель скрыта до тех пор, пока не будет открыта. После открытия панель перекрывает область содержимого.

Inline

Панель всегда видима и не перекрывает область содержимого. Доступное пространство экрана делится между областью панели и областью содержимого.

CompactOverlay

В этом режиме отображается узкая часть панели, ширины которой хватает лишь для отображения значков. Ширина закрытой панели по умолчанию составляет 48 пикселей и может быть изменена с помощью свойства CompactPaneLength. При открытии панель перекрывает область содержимого.

CompactInline

В этом режиме отображается узкая часть панели, ширины которой хватает лишь для отображения значков. Ширина закрытой панели по умолчанию составляет 48 пикселей и может быть изменена с помощью свойства CompactPaneLength. Когда панель открыта, уменьшается пространство, доступное для содержимого, так как панель занимает его место.
*/
        private async void TestControls_Part2_Loaded(object sender, RoutedEventArgs e)
        {

            string html = await this.GetResponseFromDump("music_search.txt");
            /*
            HttpResponseMessage resp = null;
            var f = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
            var client2 = new HttpClient(f);
            resp = await client2.GetAsync(new Uri("https://m.vk.com/audio"));
            if (!resp.IsSuccessStatusCode)
                return;
            string html = await resp.Content.ReadAsStringAsync();
            */
        html = System.Net.WebUtility.HtmlDecode(html);

            Regex artistsR = new Regex("OwnerRow__title.>(?<title>.+?)</div>.+?OwnerRow__subtitle.>(?<subtitle>.*?)</div>", RegexOptions.Singleline);
            var matches = artistsR.Matches(html);
            foreach (Match match in matches)
            {
                string title = match.Groups["title"].Value;
                string subtitle = match.Groups["subtitle"].Value;
            }

            Regex albumsR = new Regex(@"audioPlaylists__item.+?audio_playlist(?<owner_id>[\d\-]+)_(?<id>\d+).+?al_playlist.>(?<title>.+?)</.+?audioPlaylists__itemCover.+?style=""(?<style>.*?)"".+?(audioPlaylists__itemSubtitle|audioPlaylists__itemAuthor).>(?<owner>.+?)</", RegexOptions.Singleline);
            matches = albumsR.Matches(html);
            foreach (Match match in matches)
            {
                int owner_id = int.Parse(match.Groups["owner_id"].Value);
                int id = int.Parse( match.Groups["id"].Value);
                string title = match.Groups["title"].Value;
                string owner = match.Groups["owner"].Value;
                string style = match.Groups["style"].Value;

                if(!string.IsNullOrEmpty(style))
                {
                    Regex reg_urls = new Regex("(https:.+?)\"");
                    var url = reg_urls.Match(style);
                    if(url.Success)
                    {
                        string cover = url.Groups[1].Value;
                    }
                }
            }

            /*
            Regex data = new Regex(@"data-audio=""\[(?<id>\d+),(?<owner_id>\d+),""(?<subtitle>.*?)"",""(?<title>.*?)"",""(?<performer>.*?)"",(?<duration>\d+),\d+,\d+,"".*?"",\d+,\d+,"".*?"","".*?"",""(?<addHash>.*?)\\/(?<editHash>.*?)\\/(?<actionHash>.*?)\\/(?<deleteHash>.*?)\\/(?<replaceHash>.*?)\\/(?<urlHash>.*?)\\/(?<restoreHash>.*?)"",""(?<cover>.*?)""");
            var matches = data.Matches(html);

            List<string> list = new List<string>();

            foreach(Match match in matches)
            {
                uint id = uint.Parse(match.Groups["id"].Value);
                uint owner_id = uint.Parse(match.Groups["owner_id"].Value);

                string addHash = match.Groups["addHash"].Value;
                string editHash = match.Groups["editHash"].Value;
                string actionHash = match.Groups["actionHash"].Value;
                string deleteHash = match.Groups["deleteHash"].Value;
                string replaceHash = match.Groups["replaceHash"].Value;
                string urlHash = match.Groups["urlHash"].Value;
                string restoreHash = match.Groups["restoreHash"].Value;
                bool canEdit = !string.IsNullOrEmpty(editHash);
                bool canDelete = !string.IsNullOrEmpty(deleteHash);

                list.Add(owner_id + "_" + id + "_" + actionHash + "_" + urlHash);
            }

            Regex count = new Regex("audioPage__count.+?>(\\d+)");
            Match matchCount = count.Match(html);
            if (matchCount.Success)
            {
                int countm = int.Parse( matchCount.Groups[1].Value);
            }

            string ids = string.Join(",", list.GetRange(0,3));

            await Task.Delay(700);

            Dictionary<string, string> postParams = new Dictionary<string, string>();
            postParams["act"] = "reload_audio";
            postParams["ids"] = ids;
            resp = await client2.PostAsync(new Uri("https://m.vk.com/audio"), new HttpFormUrlEncodedContent(postParams));

            if (!resp.IsSuccessStatusCode)
                return;

            html = await resp.Content.ReadAsStringAsync();

            Regex reg_urls = new Regex("(https:.+?)\"");
            MatchCollection urls = reg_urls.Matches(html);

            foreach (Match match in urls)
            {
                string encodedUrl = match.Groups[1].Value;
                int iii = 0;

            }
            */
        }
        
        /*
         var a = g[e.fullId];
         if (a >= 0)
          for (var o = a; o < a + 3; o++) {
           var r = m[o];
           !r || r.url || d[r.fullId] || (i.push(r.fullId + "_" + r.actionHash + "_" + r.urlHash), d[r.fullId] = !0)
          }
         ajax.post("/audio", {
          act: "reload_audio",
          ids: i.join(",")
         }, {
         */

        private async Task<string> GetResponseFromDump(string file_name)
        {
            var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets\\Json");
            Windows.Storage.StorageFile file = await folder.GetFileAsync(file_name);

            byte[] fileBytes = null;
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }
            string json = Encoding.UTF8.GetString(fileBytes);//Convert.ToString(fileBytes);
            //VKResponse<T> response = JsonConvert.DeserializeObject<VKResponse<T>>(json);
            return json;
        }

        public class DataAudio
        {
            /*
            [
	            456239021,
	            375988312,
	            "",
	            "Улети",//u = i.title,
	            "T-Fest",//d = i.subTitle,
	            209,
	            0,
	            0,
	            "",
	            0,
	            66,
	            "",
	            "[]",
	            "d298464c044efedac7\/\/c27b7c48e37d487266\/6f9dfb43f82518b6e0\/\/a4de0ca0eba78dff34\/1d9918f2b1a8c96bb7",
	            "https:\/\/sun9-55.userapi.com\/c853424\/v853424754\/bb211\/wVfw-TwMNnY.jpg,https:\/\/sun9-63.userapi.com\/c853424\/v853424754\/bb20e\/K4w_fGFirYQ.jpg",
	            {
		            "duration":209,
		            "content_id":"375988312_456239021",
		            "puid22":14,
		            "account_age_type":3,
		            "_SITEID":276,
		            "vk_id":375988312,
		            "ver":251116
	            },
	            "",
	            [
		            {
			            "id":"1605419359760420133",
			            "name":"T-Fest"
		            }
	            ],
	            "",
	            [
		            -2000428260,
		            1428260,
		            "63df635caffb0f0e65"
	            ],
	            "bccdde6dfD8cp8zXEQwGyx6Ig89ZKu69WWKISaWjKoiDfTI",
	            0,
	            0,
	            true,
	            ""
            ]
            */
    }

    int i = 0;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _ucTitle.Title = "Title"+i;
            i++;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _ucTitle.SubTitle = "Subtitle" + i;
            i++;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _ucTitle.Title = string.Empty;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _ucTitle.SubTitle = string.Empty;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            SharePostUC share = new SharePostUC("записью", Core.Library.WallService.RepostObject.wall,0,0);
            
            PopUpService statusChangePopup = new PopUpService { Child = share, OverrideBackKey = true, AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed };
            share.Done = statusChangePopup.Hide;
            statusChangePopup.Show();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            this._split.DisplayMode = (SplitViewDisplayMode)cb.SelectedIndex;
        }

        private void ComboBox_SelectionChanged0(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            this._split0.DisplayMode = (SplitViewDisplayMode)cb.SelectedIndex;
        }

        private void StackPanel_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            this._trClip.Animate(this._trClip.X, 0, "X", 300);
            this._trText.Animate(this._trText.X, 0, "X", 300);
        }

        private void StackPanel_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            this._trClip.Animate(this._trClip.X, -200, "X", 300);
            this._trText.Animate(this._trText.X, -190, "X", 300);
        }
    }
}
