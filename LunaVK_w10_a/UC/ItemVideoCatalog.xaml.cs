using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LunaVK.Core.DataObjects;

using LunaVK.Library;
using LunaVK.Core.ViewModels;

namespace LunaVK.UC
{
    public sealed partial class ItemVideoCatalog : UserControl
    {
        public ItemVideoCatalog()
        {
            this.InitializeComponent();
        }

        private Core.Library.VideoService.VideoCatalogCategory VM
        {
            get { return base.DataContext as Core.Library.VideoService.VideoCatalogCategory; }
        }
        /*
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKVideoBase;
            NavigatorImpl.Instance.NavigateToVideoWithComments(vm.owner_id, vm.id, vm.access_key, vm);
        }
        */
        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.VM == null)
                return;//из-за виртуализации?

            GridView gv = sender as GridView;
            var panel = (ItemsWrapGrid)gv.ItemsPanelRoot;
            //panel.Orientation = Orientation.Horizontal;

            double colums = Math.Max(2, e.NewSize.Width / 250.0);
            //
            if (this.VM.view != "vertical_compact")
            {
                colums = 1;
            }
            //

            panel.MaximumRowsOrColumns = (int)colums;

            panel.ItemWidth = e.NewSize.Width / (int)colums;
            panel.ItemHeight = colums == 1 ? 120 : panel.ItemWidth * 0.5;
        }

        private void LoadMore_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as Core.Library.VideoService.VideoCatalogCategory;
            vm.LoadMore();
        }

        private void Border_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement fr = sender as FrameworkElement;
            fr.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };
        }

        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as Core.Library.VideoService.VideoCatalogCategory.VideoCatalogItem;
            if(vm.type == "album")
            {
                NavigatorImpl.Instance.NavigateToVideos(vm.owner_id);
            }
            else if (vm.type == "video")
            {
                Grid grid = sender as Grid;
                Border border = grid.Children[0] as Border;

                NavigatorImpl.Instance.NavigateToVideoWithComments(vm.owner_id, (uint)vm.id, vm.access_key, null, border.Child);
            }
        }

        private void CatalogItemUC_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CatalogItemUC element = sender as CatalogItemUC;
            var vm = element.DataContext as Core.Library.VideoService.VideoCatalogCategory.VideoCatalogItem;
            if (vm.type == "album")
            {
                NavigatorImpl.Instance.NavigateToVideos(vm.owner_id);
            }
            else if (vm.type == "video")
            {
                NavigatorImpl.Instance.NavigateToVideoWithComments(vm.owner_id, (uint)vm.id, vm.access_key, null, element.Img);
            }
        }

        private void CatalogName_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var vm = (sender as FrameworkElement).DataContext as Core.Library.VideoService.VideoCatalogCategory;
            if(vm.type == "channel")
            {
                NavigatorImpl.Instance.NavigateToProfilePage(vm.OwnerId);
            }
        }
    }
}
