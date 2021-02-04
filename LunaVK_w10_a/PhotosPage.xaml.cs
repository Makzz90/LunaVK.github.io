using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using LunaVK.Core.DataObjects;
using LunaVK.Core.ViewModels;
using LunaVK.Core.Utils;
using LunaVK.Library;
using Windows.UI.Xaml.Media.Animation;
using LunaVK.UC;
using LunaVK.Framework;
//https://stackoverflow.com/questions/23107920/animate-image-when-loaded-in-image-control
namespace LunaVK
{
    /// <summary>
    /// Фотографии альбома PhotoAlbumPage
    /// </summary>
    public sealed partial class PhotosPage : PageBase
    {
        private double _scrollPosition;
        private OptionsMenuItem _appBarIconButtonAddPhoto;
        private OptionsMenuItem _appBarIconButtonEdit;

        public PhotosPage()
        {
            this.InitializeComponent();
            this._gridView.Loaded2 += this._exGridView_Loaded2;
            this.Loaded += PhotosPage_Loaded;

            this._appBarIconButtonAddPhoto = new OptionsMenuItem() { Icon = "\xE898", Clicked = this._appBarIconButtonAddPhoto_Click };
            this._appBarIconButtonEdit = new OptionsMenuItem() { Icon = "\xE70F", Clicked = this._appBarIconButtonEdit_Click };
        }

        private void PhotosPage_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.BackGroundGrid.Opacity = 0;

            if(this.VM.CanAddPhotos)
                CustomFrame.Instance.Header.OptionsMenu.Add(this._appBarIconButtonAddPhoto);
            if (this.VM.CanEditAlbum)
                CustomFrame.Instance.Header.OptionsMenu.Add(this._appBarIconButtonEdit);
        }

        public PhotosViewModel VM
        {
            get { return base.DataContext as PhotosViewModel; }
        }

        private void _appBarIconButtonAddPhoto_Click(object sender)
        {
            //NavigatorImpl.Instance.NavigateToAddNewVideo("", this.VM._ownerId);
        }

        private void _appBarIconButtonEdit_Click(object sender)
        {

        }
        

        private void _exGridView_Loaded2(object sender, RoutedEventArgs e)
        {
            (sender as ScrollViewer).ViewChanged += GetInsideScrollViewer_ViewChanged;

            if (this._scrollPosition > 0)
            {
                (sender as ScrollViewer).ChangeView(0, this._scrollPosition, 1.0f);
            }
            //
            //
            //_gridView.SizeChanged += FillRowView_SizeChanged;
            
        }
        
        private void GetInsideScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            
                this.UpdateHeaderOpacity(sv.VerticalOffset);
            
        }

        private void UpdateHeaderOpacity(double ScrollPosition)
        {
            //if (this.IsInEditMode)
            //{
            //    ((UIElement)this.Header).Opacity = (1.0);
            // }
            // else
            // {
            //   if (this.itemsControlPhotos.LockedBounds)
            //       return;
            CustomFrame.Instance.Header.BackGroundGrid.Opacity = (this.CalculateOpacityFadeAwayBasedOnScroll(ScrollPosition + 88.0));
                //if (this.VM == null)
               //     return;
               // this.VM.HeaderOpacity = 1.0 - this.CalculateOpacityFadeAwayBasedOnScroll(this.itemsControlPhotos.ScrollPosition + 88.0 + 44.0);
           // }
        }

        private double CalculateOpacityFadeAwayBasedOnScroll(double sp)
        {
            return sp >= 232.0 ? (sp <= 320.0 ? 1.0 / 88.0 * sp - 29.0 / 11.0 : 1.0) : 0.0;
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                this._scrollPosition = (double)pageState["ScrollPosition"];
                this._gridView.NeedReload = false;
                //todo:title
            }
            else
            {
                Dictionary<string, object> QueryString = navigationParameter as Dictionary<string, object>;
                int ownerId = (int)QueryString["OwnerId"];
                int albumId = (int)QueryString["AlbumId"];
                string albumName = (string)QueryString["AlbumName"];
                base.DataContext = new PhotosViewModel(albumId, ownerId);
                base.Title = "Фотоальбом "+albumName;
            }

            //CustomFrame.Instance.Header.IsVisible = false;
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this.VM;
            pageState["ScrollPosition"] = this._gridView.GetInsideScrollViewer.VerticalOffset;
            //CustomFrame.Instance.Header.IsVisible = true;
            CustomFrame.Instance.Header.BackGroundGrid.Opacity = 1.0;
        }

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VKPhoto photo = (sender as FrameworkElement).DataContext as VKPhoto;

            int index = this.VM.Items.IndexOf(photo);
            //NavigatorImpl.Instance.NavigateToImageViewer(this.VM.maximum, 0, index, this.VM.Photos.ToList(), /*"PhotosByIds", false, this._friendsOnlyfalse,*/ this.GetImageFunc, false);
            NavigatorImpl.Instance.NavigateToImageViewer(this.VM._albumId.ToString(), ViewModels.ImageViewerViewModel.AlbumType.NormalAlbum, this.VM._ownerId, this.VM._totalCount.Value, index, this.VM.Items.ToList(), this.GetImageFunc);
        }

        private Border GetImageFunc(int index)
        {
            var item = this._gridView.GetGridView.ContainerFromIndex(index) as GridViewItem;
            //var item = this._gridView.GetListView.ContainerFromIndex(index) as ListViewItem;
            if (item == null)
                return null;
            UIElement ee = item.ContentTemplateRoot;
            if (ee == null)
                return null;
            Border brd = ee as Border;
            if (brd == null)
            {
                if (ee is ImageFadeInUC fade)
                {
                    return fade.Brd;
                }
                return null;
            }
            return brd;//.Child as Image;
        }
        
        private void Border_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement fr = sender as FrameworkElement;
            fr.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };
        }

        private void UpdateAppBar()
        {
            /*
            if (this.ImageViewerDecorator != null && this.ImageViewerDecorator.IsShown || this.IsMenuOpen)
                return;
            if (!this._isInEditMode)
            {
                if (this.PhotoAlbumVM.CanAddPhotos || this.PhotoAlbumVM.CanEditAlbum)
                {
                    this.ApplicationBar = ((IApplicationBar)this._mainAppBar);
                }
                else
                {
                    this.ApplicationBar = (null);
                    ExtendedLongListSelector itemsControlPhotos = this.itemsControlPhotos;
                    Thickness margin = ((FrameworkElement)this.itemsControlPhotos).Margin;
                    // ISSUE: explicit reference operation
                    double left = ((Thickness)@margin).Left;
                    margin = ((FrameworkElement)this.itemsControlPhotos).Margin;
                    // ISSUE: explicit reference operation
                    double top = ((Thickness)@margin).Top;
                    margin = ((FrameworkElement)this.itemsControlPhotos).Margin;
                    // ISSUE: explicit reference operation
                    double right = ((Thickness)@margin).Right;
                    double num = -72.0;
                    Thickness thickness = new Thickness(left, top, right, num);
                    ((FrameworkElement)itemsControlPhotos).Margin = (thickness);
                }
            }
            else
                this.ApplicationBar = ((IApplicationBar)this._editAppBar);
            ApplicationBarIconButton appBarButtonDelete = this._appBarButtonDelete;
            bool flag;
            this._appBarButtonMoveToAlbum.IsEnabled = (flag = this.listBoxPhotos.SelectedItems.Count > 0);
            int num1 = flag ? 1 : 0;
            appBarButtonDelete.IsEnabled = (num1 != 0);
            this._appBarIconButtonEdit.IsEnabled = (this.PhotoAlbumVM.PhotosCount > 0);
            if (this.PhotoAlbumVM.CanAddPhotos)
            {
                if (!this._mainAppBar.Buttons.Contains(this._appBarIconButtonAddPhoto))
                    this._mainAppBar.Buttons.Add(this._appBarIconButtonAddPhoto);
            }
            else if (this._mainAppBar.Buttons.Contains(this._appBarIconButtonAddPhoto))
                this._mainAppBar.Buttons.Remove(this._appBarIconButtonAddPhoto);
            if (this.PhotoAlbumVM.CanEditAlbum)
            {
                if (this._mainAppBar.Buttons.Contains(this._appBarIconButtonEdit))
                    return;
                this._mainAppBar.Buttons.Add(this._appBarIconButtonEdit);
            }
            else
            {
                if (!this._mainAppBar.Buttons.Contains(this._appBarIconButtonEdit))
                    return;
                this._mainAppBar.Buttons.Remove(this._appBarIconButtonEdit);
            }
            */
        }







        private void FillRowView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int rows = (int)e.NewSize.Width / 125;
            this.VM.UpdateRowItemsCount(rows);
        }
    }
}
