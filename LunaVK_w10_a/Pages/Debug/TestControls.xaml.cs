using LunaVK.Common;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента пустой страницы см. по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestControls : Page
    {
        ObservableCollection<string> Temp = new ObservableCollection<string>();
        Windows.UI.Xaml.Controls.MediaElement mysong = new Windows.UI.Xaml.Controls.MediaElement();

        public ObservableCollection<IOutboundAttachment> Attachments { get; set; }






        public TestControls()
        {
            this.DataContext = this;
            this.Attachments = new ObservableCollection<IOutboundAttachment>();

            this.InitializeComponent();

            Temp.Add("1");
            Temp.Add("2");
            Temp.Add("3");

            //this.SlideV.ItemsSource = Temp;



            base.Loaded += TestControls_Loaded;


 //           this.ucNewMessage.OnImageDeleteTap = this.ImageDeleteTap;
            //this.flip.Loaded += flipViewLoaded;

        }

        private void TestControls_Loaded(object sender, RoutedEventArgs e)
        {
            //Play();

            this._sw.DataContext = null;
            this._sw.Items = new ObservableCollection<object>();
        }

        private async void Play()
        {
            
            var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            Windows.Storage.StorageFile file = await folder.GetFileAsync("7fc078f7ea.ogg.oga");
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            mysong.MediaOpened += Mysong_MediaOpened;
            mysong.MediaFailed += Mysong_MediaFailed;
            mysong.AutoPlay = false;
            //mysong.SetSource(stream, file.ContentType);
            mysong.Source = new Uri("ms-appx:///Assets/7fc078f7ea.ogg.oga");
            mysong.CurrentStateChanged += Mysong_CurrentStateChanged;
            mysong.Play();
        }

        private void Mysong_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            int i = 0;
        }

        private void Mysong_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            int i = 0;
        }

        private void Mysong_MediaOpened(object sender, RoutedEventArgs e)
        {
            int i = 0;
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider sl = sender as Slider;
            ConversationAvatarViewModel vm = new ConversationAvatarViewModel();
            vm.Images.Add("https://pp.userapi.com/c830508/v830508353/1703df/8Dc-9EV8OtQ.jpg?ava=1");
            if (sl.Value>1)
                vm.Images.Add("https://pp.userapi.com/c850232/v850232761/5ed2e/ItOBjJ7Ym5k.jpg?ava=1");
            if (sl.Value > 2)
                vm.Images.Add("https://pp.userapi.com/c830508/v830508353/1703df/8Dc-9EV8OtQ.jpg?ava=1");
            if (sl.Value > 3)
                vm.Images.Add("https://sun9-5.userapi.com/c830609/v830609394/157b9b/zDgdBWkXDjs.jpg?ava=1");



            


            this.Avatar.Data = vm;

            Load((int)sl.Value);
        }

        private async void Load(int limit)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["count"] = limit.ToString();
            //parameters["rev"] = "1";
            //parameters["owner_id"] = this._ownerId.ToString();
            //parameters["album_id"] = this._albumId.ToString();

            //if (this.Photos.Count > 0)
            //    parameters["offset"] = this.Photos.Count.ToString();


            /*
            var temp = await RequestsDispatcher.GetResponse<VKCountedItemsObject<VKPhoto>>("photos.getAll", parameters);

            if (temp.error.error_code != VKErrors.None)
            {
                return;
            }

            //this.maximum = temp.response.count;
            List<VKAttachment> list = new List<VKAttachment>();
            foreach (VKPhoto photo in temp.response.items)
            {
                list.Add(new VKAttachment() { type = VKAttachmentType.Photo, photo = photo });
            }
            */
//            this.ap.Attachments = list;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Attachments.Add(new OutboundGeoAttachment(0, 0));
        }

        private void ImageDeleteTap(IOutboundAttachment attachment)
        {
            this.Attachments.Remove(attachment);
        }







        

        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gv = sender as GridView;
            var panel = (ItemsWrapGrid)gv.ItemsPanelRoot;
            panel.Orientation = Orientation.Horizontal;

            double colums = e.NewSize.Width / 90.0;

            panel.MaximumRowsOrColumns = (int)colums;


            panel.ItemHeight = panel.ItemWidth = e.NewSize.Width / (int)colums;
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
         //   this.Closed?.Invoke(this, null);

            var vm = (sender as FrameworkElement).DataContext as VKSticker;
            //if (vm.is_allowed)
            //{
        //    this.StickerTapped?.Invoke(this, vm);
            //}
        }

        private async void _sw_Loaded(object sender, RoutedEventArgs e)
        {
            List<StoreProductFilter> l = new List<StoreProductFilter>() { StoreProductFilter.Active };
            //l.Add(StoreProductFilter.Active);
            //List<StockItem> temp = await StoreService.Instance.GetStickers(l);

            //if (temp == null)
            //    return;

            //this._sw.Items = new ObservableCollection<object>(temp);
            //foreach (var item in temp)
            //{
            //    this._sw.FooterItems.Add(item);
            //}
        }

        private void _sw_Loaded_1(object sender, RoutedEventArgs e)
        {
            /*
            this._sw.Items = new ObservableCollection<object>();
            for (int i = 0; i < 30; i++)
            {
                string temp = i.ToString();
                this._sw.Items.Add(temp);
                this._sw.FooterItems.Add(temp);
            }*/
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UC.StickersPackViewUC.Show(12691, "message");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this._sw.DataContext = null;
            this._sw.Items = new ObservableCollection<object>();
            var emoji = new SpriteListItemData() { IsEmoji = true };
            this._sw.Items.Add(emoji);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            List<StoreProductFilter> l = new List<StoreProductFilter>() { StoreProductFilter.Active };
            StoreService.Instance.GetStockItems(l, (result) =>
            {
                if (result != null && result.error.error_code == VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        foreach (var item in result.response.items)
                        {
                            //spriteListItemDataList1.Add(new SpriteListItemData() { StickerStockItemHeader = item });
                            this._sw.Items.Add(new SpriteListItemData() { StickerStockItemHeader = item });
                        }

                        //this.panelControl.Items = new ObservableCollection<object>(spriteListItemDataList1);

                        //this.panelControl.FooterItems = new ObservableCollection<object>(spriteListItemDataList1);
                    });
                    //this.panelControl.SelectedIndex = 1;
                }
               
            });
        }
    }
}
