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
using LunaVK.Core.ViewModels;
using LunaVK.Core;
using LunaVK.Core.Enums;
using LunaVK.Library;
using LunaVK.Framework;
using Windows.ApplicationModel.DataTransfer;
using LunaVK.Core.Network;
using LunaVK.Core.Library;
using LunaVK.UC;
using LunaVK.ViewModels;
using LunaVK.UC.PopUp;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;

namespace LunaVK
{
    public sealed partial class VideoCatalogPage : PageBase
    {
        private VideosSearchViewModel searchViewModel = null;

        private OptionsMenuItem _appBarButtonCreate;
        private OptionsMenuItem _appBarButtonUpload;
        private OptionsMenuItem _appBarButtonSearch;

        private PopUpService popService;

        private VideoCatalogViewModel2 VM
        {
            get { return base.DataContext as VideoCatalogViewModel2; }
        }

        public VideoCatalogPage()
        {
            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("Menu_Videos");
            
            this.eListView.Loaded += this.HandleInsideScrollViewerLoaded;
            this.Loaded += this.VideoCatalogPage_Loaded;

            this._appBarButtonCreate = new OptionsMenuItem() { Icon = "\xE710", Clicked = this._appBarButtonCreate_Click };
            this._appBarButtonSearch = new OptionsMenuItem() { Icon = "\xE721", Clicked = this._appBarButtonSearch_Click };
            this._appBarButtonUpload = new OptionsMenuItem() { Icon = "\xE898", Clicked = this._addVideoButton_Click };
        }

        private void VideoCatalogPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (CustomFrame.Instance == null)
                return;

            CustomFrame.Instance.Header.OptionsMenu.Add(this._appBarButtonSearch);
//            CustomFrame.Instance.Header.MoreSearchClicked += this.MoreOptionsClicked;
//            CustomFrame.Instance.Header.SearchClosed = this.SearchClosed;
//            CustomFrame.Instance.Header.ServerSearch = this.OnServerSearch;

            if (this.searchViewModel != null)
                this._appBarButtonSearch_Click(null);
        }
        /*
        private void OnServerSearch(string text)
        {
            this.searchViewModel.SearchString = text;
            this.searchViewModel.Items.Clear();
            this.eListViewMy.NeedReload = true;
            this.eListViewMy.Reload();
        }
        */
        private void _addVideoButton_Click(object sender)
        {

        }

        private void _appBarButtonSearch_Click(object sender)
        {
            /*
            if (this.searchViewModel != null)
            {
                CustomFrame.Instance.Header.ActivateSearch(true, true, this.searchViewModel.q);
            }
            else
            {
                CustomFrame.Instance.Header.ActivateSearch(true, false);
                CustomFrame.Instance.Header.ActivateMoreOptionsInSearch(true);
                this.searchViewModel = new VideosSearchViewModel();
            }


            this.eListViewMy.DataContext = this.searchViewModel;

            this._pivot.Items.Remove(this.pivotItemCatalog);//BugFix
            this._pivot.Items.Remove(this.pivotItemMyVideos);
            this._root.Children.Remove(this._pivot);
            this._root.Children.Add(this.pivotItemMyVideos);

            this._lvHeader.Visibility = Visibility.Collapsed;
            //
            base.InitializeProgressIndicator(this.searchViewModel);
            */
            this.searchViewModel = new VideosSearchViewModel();

            PopUpService expr_12 = new PopUpService();
            expr_12.OverlayGrid = this._root;
            //expr_12.OverrideBackKey = true;

            expr_12.AnimationTypeChild = PopUpService.AnimationTypes.None;
            
            DataTemplate itemTemplate = (DataTemplate)base.Resources["ItemTemplate"];
            GenericSearchUC searchUC = new GenericSearchUC(this.searchViewModel, itemTemplate, this._pivot, true);
            searchUC.Close = expr_12.Hide;
            searchUC.MoreSearchClicked += this.MoreOptionsClicked;
            expr_12.Child = searchUC;
            expr_12.Show();
            Grid.SetRow(expr_12.PopupContainer, 1);
            Grid.SetRow(expr_12.BackGroundGrid, 1);
            Grid.SetRowSpan(expr_12.PopupContainer, 2);
            Grid.SetRowSpan(expr_12.BackGroundGrid, 2);
        }
        /*
        private void SearchClosed()
        {
            this.eListViewMy.DataContext = this.VM.AllVideosVM;
            this.searchViewModel = null;

            this._root.Children.Remove(this.pivotItemMyVideos);
            this._pivot.Items.Insert(0, this.pivotItemMyVideos);
            this._pivot.Items.Add(this.pivotItemCatalog);
            this._root.Children.Add(this._pivot);

            this._lvHeader.Visibility = Visibility.Visible;
        }
        */
        private void _appBarButtonCreate_Click(object sender)
        {
 //           CreateEditVideoAlbumUC sharePostUC = new CreateEditVideoAlbumUC();

//            this.popService = new PopUpService { Child = sharePostUC };

            this.popService.OverrideBackKey = true;
            this.popService.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            //this.popUp.Show();
        }

        private void HandleInsideScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            if (this.ScrollOffset != 0 && this.eListView.GetInsideScrollViewer != null)
                this.eListView.GetInsideScrollViewer.ScrollToVerticalOffset(this.ScrollOffset);
        }

        void MenuStateChanged(object sender, CustomFrame.MenuStates e)
        {
            VisualStateManager.GoToState(this, e.ToString(), false);
        }

        private double ScrollOffset;

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                if(pageState.ContainsKey("ScrollOffset"))
                    this.ScrollOffset = (double)pageState["ScrollOffset"];
            }
            else
            {
                base.DataContext = new VideoCatalogViewModel2();
            }
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this.VM;
            if(this.eListView.GetInsideScrollViewer!=null)//странно, но бывает
                pageState["ScrollOffset"] = this.eListView.GetInsideScrollViewer.VerticalOffset;

//            CustomFrame.Instance.Header.MoreSearchClicked -= this.MoreOptionsClicked;
//            CustomFrame.Instance.Header.SearchClosed = null;
//            CustomFrame.Instance.Header.ServerSearch = null;
        }
        
        private void VariableSizedWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            VariableSizedWrapGrid grid = sender as VariableSizedWrapGrid;
            int del = 1;
            switch(CustomFrame.Instance.MenuState)
            {
                case Framework.CustomFrame.MenuStates.StateMenuCollapsedContentStretch:
                    {
                        del = 1;
                        break;
                    }
                //case Framework.CustomFrame.MenuStates.StateMenuNarrowContentFixed:
                //    {
                //        del = 4;
                //        break;
                //    }
                case Framework.CustomFrame.MenuStates.StateMenuFixedContentFixed:
                case Framework.CustomFrame.MenuStates.StateMenuNarrowContentStretch:
                    {
                        del = 3;
                        break;
                    }
            }

            grid.MaximumRowsOrColumns = del;
            grid.ItemWidth = e.NewSize.Width / del;
            grid.ItemHeight = grid.ItemWidth / 2;
            //grid.Arrange();
        }

        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gv = sender as GridView;
            var panel = (ItemsWrapGrid)gv.ItemsPanelRoot;
            //panel.Orientation = Orientation.Horizontal;

            double colums = e.NewSize.Width / 250.0;

            panel.MaximumRowsOrColumns = (int)colums;

            panel.ItemWidth = e.NewSize.Width / (int)colums;
            panel.ItemHeight = panel.ItemWidth / 2.0;
        }


        private void AttachVideoUC_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CatalogItemUC elemnt = sender as CatalogItemUC;

            if (elemnt.DataContext is VideoService.VideoCatalogCategory.VideoCatalogItem catlog_item)
            {
                //todo: а если ИД отрицательный?
                NavigatorImpl.Instance.NavigateToVideoWithComments(catlog_item.owner_id, (uint)catlog_item.id, catlog_item.access_key, null, elemnt.Img);
            }
            else if (elemnt.DataContext is VKVideoBase video)
            {
                NavigatorImpl.Instance.NavigateToVideoWithComments(video.owner_id, video.id, video.access_key, video, elemnt.Img);
            }
            else if (elemnt.DataContext is VKVideoAlbum album)
            {
                NavigatorImpl.Instance.NavigateToVideoAlbum(album.id, album.title, album.owner_id);
            }
         }

        private void CatalogItemUC_Holding(object sender, HoldingRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                FrameworkElement element = sender as FrameworkElement;
                this.ShowMenu(element);
            }
        }

        private void CatalogItemUC_RightTapped(object sender, RightTappedRoutedEventArgs e)
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

            

            if (element.DataContext is VKVideoAlbum album)
            {
                //AllowEditVisibility Edit_Click HandleEdit
                //AllowDeleteVisibility
                MenuFlyoutItem item = new MenuFlyoutItem();
                item.Text = LocalizedStrings.GetString("Edit");
                item.Command = new DelegateCommand((args) => {
                    //Navigator.Current.NavigateToCreateEditVideoAlbum(this._va.id, this._va.owner_id < 0L ? -this._va.owner_id : 0, this._va.title, this._va.PrivacyInfo);
                });
                //menu.Items.Add(item);

                if (!album.is_system)
                {
                    MenuFlyoutItem item2 = new MenuFlyoutItem();
                    item2.Text = LocalizedStrings.GetString("Delete");
                    item2.Command = new DelegateCommand((args) =>
                    {
                        VideoService.Instance.DeleteAlbum(album.id, (result) => {
                            Execute.ExecuteOnUIThread(() =>
                            {
                                if (result.error.error_code == VKErrors.None && result.response == 1)
                                    this.VM.AllVideosVM.AlbumsVM.Items.Remove(album);
                            });
                        });
                    });
                    menu.Items.Add(item2);
                }
            }
            else if (element.DataContext is VKVideoBase video)
            {
                if(this._pivot.SelectedIndex==0 && this.searchViewModel==null)
                {
                    MenuFlyoutItem item = new MenuFlyoutItem();
                    item.Text = LocalizedStrings.GetString("VideoNew_RemovedFromMyVideos");
                    item.Command = new DelegateCommand((args) => {
                        this.VM.AllVideosVM.AddRemoveToMyVideos(video,false);//mItemAddToMyVideos_Click
                    });
                    menu.Items.Add(item);
                }

                MenuFlyoutItem item2 = new MenuFlyoutItem();
                item2.Text = LocalizedStrings.GetString("VideoNew_AddToAlbum");
                item2.Command = new DelegateCommand((args) => {
                    mItemAddToAlbum_Click(video);
                });
                menu.Items.Add(item2);

                MenuFlyoutItem item3 = new MenuFlyoutItem();
                item3.Text = LocalizedStrings.GetString("CopyLink");
                item3.Command = new DelegateCommand((args) => {
                    //mItemCopyLink_Click
                    var dataPackage = new DataPackage();
                    dataPackage.SetText(string.Format("https://{0}vk.com/video{1}_{2}", CustomFrame.Instance.IsDevicePhone ? "m." : "", video.owner_id, video.id));
                    Clipboard.SetContent(dataPackage);
                });
                menu.Items.Add(item3);



                MenuFlyoutItem item4 = new MenuFlyoutItem();
                item4.Text = LocalizedStrings.GetString("ShareWallPost_Share/Text");
                item4.Command = new DelegateCommand((args) => {
                    //mItemAddToAlbum_Click
                });
                menu.Items.Add(item4);

                MenuFlyoutItem _appBarMenuItemFaveUnfave = new MenuFlyoutItem();
                _appBarMenuItemFaveUnfave.Text = LocalizedStrings.GetString(video.is_favorite ? "RemoveFromBookmarks" : "AddToBookmarks");
                _appBarMenuItemFaveUnfave.Command = new DelegateCommand((args) =>
                {
                    FavoritesService.Instance.FaveAddRemoveVideo(video.owner_id, video.id, video.access_key, !video.is_favorite, (result) =>
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            if (result.error.error_code == VKErrors.None && result.response == 1)
                                video.is_favorite = !video.is_favorite;
                        });
                    });
                });

                menu.Items.Add(_appBarMenuItemFaveUnfave);

                /*
                MenuFlyoutSubItem item5 = new MenuFlyoutSubItem();
                item5.Text = LocalizedStrings.GetString("Report") + "...";

                MenuFlyoutItem subitem = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonSpam"), CommandParameter = "0" };//1
                subitem.Command = new DelegateCommand((args) => { this.ReportPost(video,args); });
                item5.Items.Add(subitem);
                MenuFlyoutItem subitem2 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonChildPorn"), CommandParameter = "1" };//5
                subitem2.Command = new DelegateCommand((args) => { this.ReportPost(video, args); });
                item5.Items.Add(subitem2);
                MenuFlyoutItem subitem3 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonExtremism"), CommandParameter = "2" };//6
                subitem3.Command = new DelegateCommand((args) => { this.ReportPost(video, args); });
                item5.Items.Add(subitem3);
                MenuFlyoutItem subitem4 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonViolence"), CommandParameter = "3" };//7
                subitem4.Command = new DelegateCommand((args) => { this.ReportPost(video, args); });
                item5.Items.Add(subitem4);
                MenuFlyoutItem subitem5 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonDrug"), CommandParameter = "4" };//4
                subitem5.Command = new DelegateCommand((args) => { this.ReportPost(video, args); });
                item5.Items.Add(subitem5);
                MenuFlyoutItem subitem6 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonAdult"), CommandParameter = "5" };//3
                subitem6.Command = new DelegateCommand((args) => { this.ReportPost(video, args); });
                item5.Items.Add(subitem6);
                MenuFlyoutItem subitem7 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonInsult"), CommandParameter = "6" };//2
                subitem7.Command = new DelegateCommand((args) => { this.ReportPost(video, args); });
                item5.Items.Add(subitem7);
                

                menu.Items.Add(item5);
                */
            }

            if(menu.Items.Count>0)
                menu.ShowAt(element);
        }

        private void mItemAddToAlbum_Click(VKVideoBase video)
        {
            //Navigator.Current.NavigateToAddVideoToAlbum(this._ownerId, this._videoId);
            var popUC = new UC.PopUp.AddToAlbumUC(video.owner_id, video.id);
            popService = new PopUpService { Child = popUC };

            popService.OverrideBackKey = true;
            popService.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            popService.Show();

            popUC.Done = popService.Hide;
        }

        private void ReportPost(VKVideoBase video, object args)
        {
            //VideoService.Instance.Report(video.owner_id, video.id,);
        }

        private void EListViewMy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }

        private void MoreOptionsClicked()
        {
            PopUpService statusChangePopup = new PopUpService();

            FrameworkElement element = null;
            
            VideosSearchParamsUC sharePostUC = new VideosSearchParamsUC();
            sharePostUC.DataContext = this.searchViewModel;
            sharePostUC.Done = () =>
            {
                statusChangePopup.Hide();
                this.searchViewModel.Reload();
            };
            element = sharePostUC;
            

            statusChangePopup.Child = element;

            statusChangePopup.OverrideBackKey = true;
            statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            statusChangePopup.Show();
        }
        /*
        private void gridAdded_Tap(object sender, TappedRoutedEventArgs e)
        {
            this.eListViewMy.DataContext = this.VM.AllVideosVM;
            this.eListViewMy.NeedReload = true;
            if(this.VM.AllVideosVM.Items.Count==0 && this.VM.AllVideosVM._totalCount == null)
                this.eListViewMy.Reload();
        }

        private void gridUploaded_Tap(object sender, TappedRoutedEventArgs e)
        {
            this.eListViewMy.DataContext = this.VM.UploadedVideosVM;
            this.eListViewMy.NeedReload = true;
            if (this.VM.UploadedVideosVM.Items.Count == 0 && this.VM.UploadedVideosVM._totalCount == null)
                this.eListViewMy.Reload();
        }

        private void gridAlbums_Tap(object sender, TappedRoutedEventArgs e)
        {
            this.eListViewMy.DataContext = this.VM.AlbumsVM;
            this.eListViewMy.NeedReload = true;
            if (this.VM.AlbumsVM.Items.Count == 0 && this.VM.AlbumsVM._totalCount == null)
                this.eListViewMy.Reload();
        }
        */
        private void _lvHeader_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this._lvHeader.SelectedIndex==0)
            {
                if(!CustomFrame.Instance.Header.OptionsMenu.Contains(this._appBarButtonUpload))
                    CustomFrame.Instance.Header.OptionsMenu.Insert(0,this._appBarButtonUpload);
                if (!CustomFrame.Instance.Header.OptionsMenu.Contains(this._appBarButtonCreate))
                    CustomFrame.Instance.Header.OptionsMenu.Insert(0,this._appBarButtonCreate);
                //if (!CustomFrame.Instance.Header.OptionsMenu.Contains(this._appBarButtonSearch))
                //    CustomFrame.Instance.Header.OptionsMenu.Add(this._appBarButtonSearch);
            }
            else
            {
                if (CustomFrame.Instance.Header.OptionsMenu.Contains(this._appBarButtonUpload))
                    CustomFrame.Instance.Header.OptionsMenu.Remove(this._appBarButtonUpload);
                if (CustomFrame.Instance.Header.OptionsMenu.Contains(this._appBarButtonCreate))
                    CustomFrame.Instance.Header.OptionsMenu.Remove(this._appBarButtonCreate);
                //if (CustomFrame.Instance.Header.OptionsMenu.Contains(this._appBarButtonSearch))
                //    CustomFrame.Instance.Header.OptionsMenu.Remove(this._appBarButtonSearch);
            }
        }





        private void Album_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CatalogItemUC item = sender as CatalogItemUC;
            var vm = item.DataContext as VKVideoAlbum;
            NavigatorImpl.Instance.NavigateToVideoAlbum(vm.id, vm.title, vm.owner_id);
        }

        private void Albums_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //NavigatorImpl.Instance.NavigateToVideoAlbum(this.VM._ownerId, this.VM._ownerName);
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
    }
}

//https://xamlbrewer.wordpress.com/2017/02/27/creating-a-fluid-adaptive-ui-with-variablesizedwrapgrid-and-implicit-animations/