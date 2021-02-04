using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;
using LunaVK.ViewModels;
using LunaVK.Library;
using LunaVK.UC;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LunaVK.Core;
using System.Linq;
using LunaVK.Framework;

namespace LunaVK.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MyLikesPage : PageBase
    {
        public MyLikesPage()
        {
            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("Menu_Likes/Content");
        }

        private MyLikesViewModel VM
        {
            get { return base.DataContext as MyLikesViewModel; }
        }
        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            img.Animate(0, 1, "Opacity", 300);
            img.ImageOpened -= Image_ImageOpened;
        }

        private void _list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }

        private void Video_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CatalogItemUC item = sender as CatalogItemUC;
            var vm = (sender as FrameworkElement).DataContext as VKVideoBase;
            NavigatorImpl.Instance.NavigateToVideoWithComments(vm.owner_id, vm.id, vm.access_key, vm, item.Img);
        }

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            VKPhoto photo = element.DataContext as VKPhoto;

            int index = this.VM.PhotosVM.Items.IndexOf(photo);
            NavigatorImpl.Instance.NavigateToImageViewer(this.VM.PhotosVM._totalCount.Value, this.VM.PhotosVM.Items.Count, index, this.VM.PhotosVM.Items.ToList(), ViewModels.ImageViewerViewModel.ViewerMode.PhotosByIdsForFavorites, this.GetImageFunc);
        }

        private Border GetImageFunc(int index)
        {
            GridViewItem item = this._gridPhotos.GetGridView.ContainerFromIndex(index) as GridViewItem;
            if (item == null)
                return null;
            UIElement ee = item.ContentTemplateRoot;
            if (ee == null)
                return null;
            Border brd = ee as Border;
            if (brd == null)
                return null;
            return brd;//.Child as Image;
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                
                this._listPost.NeedReload = this.VM.PostsVM.Items.Count == 0;
                this._gridPhotos.NeedReload = this.VM.PhotosVM.Items.Count == 0;
                this._listVideo.NeedReload = this.VM.VideosVM.Items.Count == 0;
                this._listProducts.NeedReload = this.VM.ProductsVM.Items.Count == 0;
            }
            else
            {
                base.DataContext = new MyLikesViewModel();
                //this._pivot.SelectedIndex = Settings.FavoritesDefaultSection;
            }
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this.VM;
            /*
           Settings.FavoritesDefaultSection = (byte)this._pivot.SelectedIndex;

           CustomFrame.Instance.Header.SubTitle = string.Empty;

           if (this._pivot.SelectedIndex == 0)
               this.VerticalOffset = this._listPeople.GetInsideScrollViewer.VerticalOffset;
           else if (this._pivot.SelectedIndex == 1)
               this.VerticalOffset = this._listGroup.GetInsideScrollViewer.VerticalOffset;
           else if (this._pivot.SelectedIndex == 2)
               this.VerticalOffset = this._listPost.GetInsideScrollViewer.VerticalOffset;
           else if (this._pivot.SelectedIndex == 3)
               this.VerticalOffset = this._listArticle.GetInsideScrollViewer.VerticalOffset;
           else if (this._pivot.SelectedIndex == 4)
               this.VerticalOffset = this._listLinks.GetInsideScrollViewer.VerticalOffset;
           else if (this._pivot.SelectedIndex == 5)
               this.VerticalOffset = this._listPodcasts.GetInsideScrollViewer.VerticalOffset;
           else if (this._pivot.SelectedIndex == 6)
               this.VerticalOffset = this._listVideo.GetInsideScrollViewer.VerticalOffset;
           else if (this._pivot.SelectedIndex == 7)
               this.VerticalOffset = this._listNarative.GetInsideScrollViewer.VerticalOffset;
           else if (this._pivot.SelectedIndex == 8)
               this.VerticalOffset = this._listProducts.GetInsideScrollViewer.VerticalOffset;

           pageState["ScrollOffset"] = this.VerticalOffset;*/
        }
    }
}
