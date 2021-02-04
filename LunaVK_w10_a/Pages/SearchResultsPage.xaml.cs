using LunaVK.Core.DataObjects;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.UC;
using LunaVK.ViewModels;
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

namespace LunaVK.Pages
{
    public sealed partial class SearchResultsPage : PageBase
    {
        private int _selected;
        private double VerticalOffset = 0;

        public SearchResultsPage()
        {
            this.InitializeComponent();
            this.Loaded += SearchResultsPage_Loaded;
            base.Title = "";
        }

        private void SearchResultsPage_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.ActivateSearch(true, false, this.VM.SearchName, true);
            
            CustomFrame.Instance.Header.ServerSearch = this.HandleSearch;
            CustomFrame.Instance.Header.MoreSearchClicked += this.MoreOptionsClicked;

            this._pivot.SelectionChanged += _pivot_SelectionChanged;

            if (this._selected != 0)
                this._pivot.SelectedIndex = this._selected;

            if (this._selected == 0)
                this.serachHints.Loaded2 += InsideScrollViewerLoaded;
            else if (this._selected == 1)
                this.listBoxUsers.Loaded2 += InsideScrollViewerLoaded;
            else if (this._selected == 2)
                this.listBoxGroups.Loaded2 += InsideScrollViewerLoaded;
            else if (this._selected == 3)
                this._gridViewPhotos.Loaded2 += InsideScrollViewerLoaded;


        }

        private void _pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot pivot = sender as Pivot;
            CustomFrame.Instance.Header.ActivateMoreOptionsInSearch(pivot.SelectedIndex == 1 || pivot.SelectedIndex == 2);
        }

        private void InsideScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            if (this.VerticalOffset != 0)
                (sender as ScrollViewer).ChangeView(0, this.VerticalOffset, 1.0f);
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                this._selected = (int)pageState["Page"];
                this.VerticalOffset = (double)pageState["ScrollOffset"];

                if (this.VM.FastVM.Items.Count > 0)
                    this.serachHints.NeedReload = false;
                if (this.VM.UsersVM.Items.Count > 0)
                    this.listBoxUsers.NeedReload = false;
                if (this.VM.GroupsVM.Items.Count > 0)
                    this.listBoxGroups.NeedReload = false;
                if (this.VM.PhotosVM.Items.Count > 0)
                    this._gridViewPhotos.NeedReload = false;
            }
            else
            {
                string q = (string)navigationParameter;
                this.DataContext = new SearchParamsViewModel() { SearchName = q };
            }
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this.VM;
            pageState["Page"] = this._pivot.SelectedIndex;
            CustomFrame.Instance.Header.ActivateSearch(false,true);//выключаем таймер

            if (this._pivot.SelectedIndex == 0)
                this.VerticalOffset = this.serachHints.GetInsideScrollViewer.VerticalOffset;
            else if (this._pivot.SelectedIndex == 1)
                this.VerticalOffset = this.listBoxUsers.GetInsideScrollViewer.VerticalOffset;
            else if (this._pivot.SelectedIndex == 2)
                this.VerticalOffset = this.listBoxGroups.GetInsideScrollViewer.VerticalOffset;
            else if (this._pivot.SelectedIndex == 3)
                this.VerticalOffset = this._gridViewPhotos.GetInsideScrollViewer.VerticalOffset;
            pageState["ScrollOffset"] = this.VerticalOffset;
        }

        private void BaseProfileItem_BackTap(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKUser;
            NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }

        private void BaseProfileItemHint_BackTap(object sender, RoutedEventArgs e)
        {
            var hint = (sender as FrameworkElement).DataContext as SearchHint;
            if (hint.profile != null)
            {
                NavigatorImpl.Instance.NavigateToProfilePage(hint.profile.Id);
            }
            else if (hint.group != null)
            {
                NavigatorImpl.Instance.NavigateToProfilePage(hint.group.Id);
            }
        }

        private SearchParamsViewModel VM
        {
            get { return base.DataContext as SearchParamsViewModel; }
        }

        private void MoreOptionsClicked()
        {
            PopUpService statusChangePopup = new PopUpService();

            FrameworkElement element = null;

            if (this._pivot.SelectedIndex == 1)
            {
                UsersSearchParamsUC sharePostUC = new UsersSearchParamsUC();
                sharePostUC.DataContext = this.VM.UsersVM;
                sharePostUC.Done = () =>
                {
                    statusChangePopup.Hide();
                    this.VM.UsersVM.Items.Clear();

                    this.listBoxUsers.NeedReload = true;
                    this.listBoxUsers.Reload();
                };
                element = sharePostUC;
            }
            else if (this._pivot.SelectedIndex == 2)
            {
                GroupsSearchParamsUC sharePostUC = new GroupsSearchParamsUC();
                sharePostUC.DataContext = this.VM.GroupsVM;
                sharePostUC.Done = () =>
                {
                    statusChangePopup.Hide();
                    this.VM.GroupsVM.Items.Clear();

                    this.listBoxGroups.NeedReload = true;
                    this.listBoxGroups.Reload();
                };
                element = sharePostUC;
            }
            else
            {
                return;
            }

            statusChangePopup.Child = element;

            statusChangePopup.OverrideBackKey = true;
            statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            statusChangePopup.Show();
        }

        private void HandleSearch(string q)
        {
            this.VM.SearchName = q;

            this.VM.FastVM.Items.Clear();
            this.VM.UsersVM.Items.Clear();
            this.VM.GroupsVM.Items.Clear();
            this.VM.PhotosVM.Items.Clear();

            this.serachHints.NeedReload = true;
            this.listBoxUsers.NeedReload = true;
            this.listBoxGroups.NeedReload = true;
            this._gridViewPhotos.NeedReload = true;

            if (this._pivot.SelectedIndex == 0)
                this.serachHints.Reload();
            else if (this._pivot.SelectedIndex == 1)
                this.listBoxUsers.Reload();
            else if (this._pivot.SelectedIndex == 2)
                this.listBoxGroups.Reload();
            else if (this._pivot.SelectedIndex == 3)
                this._gridViewPhotos.Reload();
        }

        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            CustomFrame.Instance.Header.MoreSearchClicked -= this.MoreOptionsClicked;
            CustomFrame.Instance.Header.SearchClosed = null;
            CustomFrame.Instance.Header.ServerSearch = null;
        }

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VKPhoto photo = (sender as FrameworkElement).DataContext as VKPhoto;

            int index = this.VM.PhotosVM.Items.IndexOf(photo);
            //UC.ImageViewerDecoratorUC imageViewer = new UC.ImageViewerDecoratorUC();
            //imageViewer.Initialize(this.VM.Photos.ToList(), (i) => { return this.GetImageFunc(i); });
            //imageViewer.Show(index);

            NavigatorImpl.Instance.NavigateToImageViewer(this.VM.PhotosVM._totalCount.Value, 0, index, this.VM.PhotosVM.Items.ToList(), ImageViewerViewModel.ViewerMode.AlbumPhotos, this.GetImageFunc);
            //NavigatorImpl.Instance.NavigateToImageViewer(this.VM._albumId.ToString(), ViewModels.ImageViewerViewModel.AlbumType.NormalAlbum, this.VM._ownerId, this.VM._totalCount.Value, index, this.VM.Items.ToList(), this.GetImageFunc);
        }

        private Border GetImageFunc(int index)
        {
            GridViewItem item = this._gridViewPhotos.GetGridView.ContainerFromIndex(index) as GridViewItem;
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

        private void ItemGroupUC_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKGroup;
            NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }

        private void SearchParamsSummaryUC_OnClearButtonTap(object sender, EventArgs e)
        {
            if (this._pivot.SelectedIndex == 0)
            {
                //this.VM.FastVM.
             }
            if (this._pivot.SelectedIndex == 1)
                this.VM.UsersVM.Clear();
            else if (this._pivot.SelectedIndex == 2)
                this.VM.GroupsVM.Clear();
            else if (this._pivot.SelectedIndex == 3)
                this.VM.PhotosVM.Clear();
        }
    }
}
