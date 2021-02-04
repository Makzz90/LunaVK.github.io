using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

using LunaVK.Core.ViewModels;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;
using LunaVK.Core;
using LunaVK.Library;
using LunaVK.UC;
using LunaVK.Framework;
using LunaVK.Network;
using LunaVK.Core.Library;
using LunaVK.Core.Framework;

namespace LunaVK
{
    public sealed partial class FavoritesPage : PageBase
    {
        private double VerticalOffset = 0;
        PopUpService popService;

        public FavoritesPage()
        {
            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("Menu_Bookmarks/Content");
            this.Loaded += FavoritesPage_Loaded;
        }

        private void FavoritesPage_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE8EC", Clicked = this._appBarButtonTag_Click });

            int _selected = this._pivot.SelectedIndex;

            if (_selected == 0)
                this._listPeople.Loaded2 += InsideScrollViewerLoaded;
            else if (_selected == 1)
                this._listGroup.Loaded2 += InsideScrollViewerLoaded;
            else if (_selected == 2)
                this._listPost.Loaded2 += InsideScrollViewerLoaded;
            else if (_selected == 3)
                this._listArticle.Loaded2 += InsideScrollViewerLoaded;
            else if (_selected == 4)
                this._listLinks.Loaded2 += InsideScrollViewerLoaded;
            else if (_selected == 5)
                this._listPodcasts.Loaded2 += InsideScrollViewerLoaded;
            else if(_selected == 6)
                this._listVideo.Loaded2 += InsideScrollViewerLoaded;
            else if (_selected == 7)
                this._listNarative.Loaded2 += InsideScrollViewerLoaded;
            else if (_selected == 8)
                this._listProducts.Loaded2 += InsideScrollViewerLoaded;
            
            



            //todo: проверятьпо спискам, есть ли непросмотренные
            var c = LongPollServerService.Instance._counters;
            if (c.faves > 0)
            {
                FavoritesService.Instance.MarkSeen(null);
                c.faves = 0;
                EventAggregator.Instance.PublishCounters(c);
            }
        }

        private void _appBarButtonTag_Click(object sender)
        {
            //CustomFrame.Instance.HeaderWithMenu.SubTitle = "Посмотреть позже";
            var popUC = new UC.PopUp.FaveTagsUC();
            popUC.Loaded += EditStatusUC_Loaded;
            popUC.DataContext = this.VM.TagsVM;
            popService = new PopUpService { Child = popUC };

            popService.OverrideBackKey = true;
            popService.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            popService.Show();

            popUC.SelectTap = (number) =>
            {
                this.SetSource(number);
                popService.Hide();
            };
        }

        private void SetSource(int i)
        {
            if(i==0)
            {
                CustomFrame.Instance.Header.SubTitle = "";
                return;
            }
            string name = this.VM.TagsVM.Items[i].name;
            CustomFrame.Instance.Header.SubTitle = name;
        }

        private void EditStatusUC_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            element.Loaded -= this.EditStatusUC_Loaded;
            if (this.VM.TagsVM._totalCount == null)
                this.VM.TagsVM.Reload();
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                int selected = Settings.FavoritesDefaultSection;
                this.VerticalOffset = (double)pageState["ScrollOffset"];
                this._pivot.SelectedIndex = selected;

                this._listPeople.NeedReload = this.VM.UsersVM.Items.Count == 0;
                this._listGroup.NeedReload = this.VM.GroupsVM.Items.Count == 0;
                this._listPost.NeedReload = this.VM.PostsVM.Items.Count == 0;
                this._listArticle.NeedReload = this.VM.ArticlesVM.Items.Count == 0;
                this._listLinks.NeedReload = this.VM.LinksVM.Items.Count == 0;
                this._listPodcasts.NeedReload = this.VM.PodcastsVM.Items.Count == 0;
                this._listVideo.NeedReload = this.VM.VideosVM.Items.Count == 0;
                this._listNarative.NeedReload = this.VM.NarrativeVM.Items.Count == 0;
                this._listProducts.NeedReload = this.VM.ProductsVM.Items.Count == 0;
            }
            else
            {
                base.DataContext = new FavoritesViewModel();

                if (Settings.FavoritesDefaultSection >= this._pivot.Items.Count)//BugFix: на всякий случай
                    Settings.FavoritesDefaultSection = 0;
                this._pivot.SelectedIndex = Settings.FavoritesDefaultSection;
            }
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this.VM;
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

            pageState["ScrollOffset"] = this.VerticalOffset;
        }

        private void InsideScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            if (this.VerticalOffset != 0)
                (sender as ScrollViewer).ChangeView(0, this.VerticalOffset, 1.0f);
        }

        private FavoritesViewModel VM
        {
            get { return base.DataContext as FavoritesViewModel; }
        }
        
        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gv = sender as GridView;
            var panel = (ItemsWrapGrid)gv.ItemsPanelRoot;
            panel.Orientation = Orientation.Horizontal;
            double colums = e.NewSize.Width / 130.0;
            panel.MaximumRowsOrColumns = (int)colums;

            panel.ItemHeight = panel.ItemWidth = e.NewSize.Width / (int)colums;

            //panel.ItemHeight = panel.ItemWidth = e.NewSize.Width / 4;
        }

        private void Video_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CatalogItemUC item = sender as CatalogItemUC;
            var vm = (sender as FrameworkElement).DataContext as VKVideoBase;
            NavigatorImpl.Instance.NavigateToVideoWithComments(vm.owner_id, vm.id, vm.access_key, vm, item.Img);
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            img.Animate(0, 1, "Opacity", 300);
            img.ImageOpened -= Image_ImageOpened;
        }


        private void Link_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKLink;
           
            if (string.IsNullOrWhiteSpace(vm.url))
                return;

            NavigatorImpl.Instance.NavigateToWebUri(vm.url.Trim((char[])new char[1] { '/' }));
        }


        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = e.AddedItems[0] as VKBaseDataForGroupOrUser;
            NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }










        private void ItemGroupUC_Holding(object sender, HoldingRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                FrameworkElement element = sender as FrameworkElement;
                this.ShowMenu(element);
            }
        }

        private void ItemGroupUC_RightTapped(object sender, RightTappedRoutedEventArgs e)
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

            MenuFlyoutItem item = new MenuFlyoutItem() { Text = "Убрать из избранного" };
            item.Command = new DelegateCommand((arg) =>
            {
                var vm = element.DataContext as VKBaseDataForGroupOrUser;
                this.VM.Remove(vm);
            });
            menu.Items.Add(item);
            menu.ShowAt(element);
        }

        private void _list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }
    }
}
