using LunaVK.Common;
using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Framework;
using LunaVK.UC;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LunaVK
{
    public sealed partial class VideoPage : PageBase
    {
        /// <summary>
        /// Список видео в альбоме
        /// </summary>
        public VideoPage()
        {
            this.InitializeComponent();
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            IDictionary<string, object> QueryString = e.Parameter as IDictionary<string, object>;
            int owner = (int)QueryString["OwnerId"];
            int album = (int)QueryString["AlbumId"];
            string albumName = (string)QueryString["AlbumName"];


            base.DataContext = new VideosOfOwnerViewModel(owner, album);
            //this.VM.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;

            base.Title = LocalizedStrings.GetString("Profile_Videos") + " " + albumName;
        }

        private VideosOfOwnerViewModel VM
        {
            get { return base.DataContext as VideosOfOwnerViewModel; }
        }

        private void CatalogItemUC_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKVideoBase;
            CatalogItemUC item = sender as CatalogItemUC;
            Library.NavigatorImpl.Instance.NavigateToVideoWithComments(vm.owner_id, vm.id, vm.access_key, vm, item.Img);
        }

        private void _list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }

        private void Album_Holding(object sender, HoldingRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                FrameworkElement element = sender as FrameworkElement;
                this.ShowMenu(element);
            }
        }

        private void Album_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                return;

            FrameworkElement element = sender as FrameworkElement;
            this.ShowMenu(element);
        }














        private void ShowMenu(FrameworkElement element)
        {
            MenuFlyout menu = new MenuFlyout();

            var video = element.DataContext as VKVideoBase;

            if (video.can_edit)
            {
                MenuFlyoutItem item1 = new MenuFlyoutItem();
                item1.Text = LocalizedStrings.GetString("Delete");
                item1.Command = new DelegateCommand(async (args) =>
                {
                    if (await MessageBox.Show("DeleteConfirmation", "Delete", MessageBox.MessageBoxButton.OKCancel) != MessageBox.MessageBoxButton.OK)
                        return;
                    this.VM.Delete(video);
                });
                menu.Items.Add(item1);
            }


            MenuFlyoutItem item3 = new MenuFlyoutItem();
            item3.Text = LocalizedStrings.GetString("CopyLink");
            item3.Command = new DelegateCommand((args) =>
            {
                //mItemCopyLink_Click
                var dataPackage = new DataPackage();
                dataPackage.SetText(string.Format("https://{0}vk.com/video{1}_{2}", CustomFrame.Instance.IsDevicePhone ? "m." : "", video.owner_id, video.id));
                Clipboard.SetContent(dataPackage);
            });
            menu.Items.Add(item3);



            MenuFlyoutItem item4 = new MenuFlyoutItem();
            item4.Text = LocalizedStrings.GetString("ShareWallPost_Share/Text");
            item4.Command = new DelegateCommand((args) =>
            {
                //mItemAddToAlbum_Click
            });
            menu.Items.Add(item4);




            if (menu.Items.Count > 0)
                menu.ShowAt(element);
        }

    }
}
