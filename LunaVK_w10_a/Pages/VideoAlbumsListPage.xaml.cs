using LunaVK.Common;
using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.UC;
using LunaVK.UC.PopUp;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LunaVK.Pages
{
    public sealed partial class VideoAlbumsListPage : PageBase
    {
        private double _scrollPosition;
        private OptionsMenuItem _appBarButtonCreate;
        private OptionsMenuItem _appBarButtonSearch;
        private OptionsMenuItem _appBarButtonAddVideo;

        private VideosSearchViewModel searchViewModel = null;
        private VideoAlbumsListViewModel _videosViewModel;
        private PopUpService popService;

        public VideoAlbumsListPage()
        {
            this.InitializeComponent();
            this._appBarButtonCreate = new OptionsMenuItem() { Icon = "\xE710", Clicked = this._appBarButtonCreate_Click };
            this._appBarButtonSearch = new OptionsMenuItem() { Icon = "\xE721", Clicked = this._appBarButtonSearch_Click };
            this._appBarButtonAddVideo = new OptionsMenuItem() { Icon = "\xE898", Clicked = this._appBarButtonAddVideo_Click };
            this._exLv.Loaded2 += this._exGridView_Loaded2;
        }

        private void _exGridView_Loaded2(object sender, RoutedEventArgs e)
        {
            if (this._scrollPosition > 0)
                (sender as ScrollViewer).ChangeView(0, this._scrollPosition, 1.0f);

            if (this._videosViewModel._ownerId == 0 || this._videosViewModel._ownerId == Settings.UserId)
            {
                CustomFrame.Instance.Header.OptionsMenu.Add(this._appBarButtonAddVideo);

                CustomFrame.Instance.Header.OptionsMenu.Add(this._appBarButtonCreate);
                CustomFrame.Instance.Header.OptionsMenu.Add(this._appBarButtonSearch);

                CustomFrame.Instance.Header.MoreSearchClicked += this.MoreOptionsClicked;
                CustomFrame.Instance.Header.SearchClosed = this.SearchClosed;
                CustomFrame.Instance.Header.ServerSearch = this.OnServerSearch;

                if (this.searchViewModel != null)
                    this._appBarButtonSearch_Click(null);
            }
        }

        private VideoAlbumsListViewModel VM
        {
            get { return base.DataContext as VideoAlbumsListViewModel; }
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                this._videosViewModel = (VideoAlbumsListViewModel)pageState["Data"];

                if(pageState.ContainsKey("SearchData"))
                {
                    this.searchViewModel = (VideosSearchViewModel)pageState["SearchData"];
                    base.DataContext = this.searchViewModel;
                }
                else
                {
                    base.DataContext = this._videosViewModel;
                }
                
                this._scrollPosition = (double)pageState["ScrollPosition"];
                this._exLv.NeedReload = false;
            }
            else
            {
                IDictionary<string, object> QueryString = navigationParameter as IDictionary<string, object>;
                int owner = (int)QueryString["OwnerId"];
                string ownerName = (string)QueryString["OwnerName"];

                this._videosViewModel = new VideoAlbumsListViewModel(owner);
                base.DataContext = this._videosViewModel;
            }
            string temp = LocalizedStrings.GetString("Menu_Videos");// + " " + this.VM._ownerName;
            base.Title = temp;
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this._videosViewModel;
            pageState["ScrollPosition"] = this._exLv.GetInsideScrollViewer.VerticalOffset;

            if (this.searchViewModel != null)
            {
                pageState["SearchData"] = this.searchViewModel;
            }

                CustomFrame.Instance.Header.MoreSearchClicked -= this.MoreOptionsClicked;
            CustomFrame.Instance.Header.SearchClosed = null;
            CustomFrame.Instance.Header.ServerSearch = null;
        }

        private void Albums_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //NavigatorImpl.Instance.NavigateToVideoAlbum(this.VM._ownerId, this.VM._ownerName);
        }

        private void ExtendedListView3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }












        private void _appBarButtonCreate_Click(object sender)
        {
            this.ShowEditAlbum(null);
        }

        private void _appBarButtonAddVideo_Click(object sender)
        {
            NavigatorImpl.Instance.NavigateToAddNewVideo("", this.VM._ownerId);
        }

        private void ShowEditAlbum(VKVideoAlbum album)
        {
            PopUpService dc = new PopUpService() { AnimationTypeChild = PopUpService.AnimationTypes.Slide, OverrideBackKey = true };

            CreateEditAlbumViewModel editAlbumViewModel = new CreateEditAlbumViewModel(album);
            editAlbumViewModel.DescriptionVisibility = Visibility.Collapsed;

            CreateAlbumUC createAlbumUc = new CreateAlbumUC(() =>
            {
                if (album == null)
                {
                    this.VM.SetInProgress(true);
                    VideoService.Instance.AddAlbum(editAlbumViewModel.Name, editAlbumViewModel.PrivacyView.ToString(), (result) =>
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            if (result.error.error_code == Core.Enums.VKErrors.None && result.response>0)
                            {
                                VKVideoAlbum a = new VKVideoAlbum();
                                a.id = result.response;
                                a.title = editAlbumViewModel.Name;
                                a.privacy = new VKAlbumPrivacy() { category = editAlbumViewModel.PrivacyView.ToString() };
                                a.updated_time = DateTime.Now;
                                this.VM.AlbumsVM.Items.Add(a);
                                this.VM.AlbumsCount++;
                                dc.Hide();
                            }
                            else
                            {
                                //ExtendedMessageBox.ShowSafe(CommonResources.Error);
                                MessageBox.Show("Error", "", MessageBox.MessageBoxButton.OK);
                            }
                        });
                    },
                    editAlbumViewModel._groupId);
                }
                else
                {
                    this.VM.SetInProgress(true);
                    VideoService.Instance.EditAlbum(editAlbumViewModel.Name, album.id, editAlbumViewModel.PrivacyView.ToString(), (result) =>
                    {
                        Execute.ExecuteOnUIThread(() => {
                            if (result.error.error_code == Core.Enums.VKErrors.None && result.response == 1)
                            {
                                //AddOrUpdateAlbum

                                int pos = this.VM.AlbumsVM.Items.IndexOf(album);
                                this.VM.AlbumsVM.Items.Remove(album);

                                album.title = editAlbumViewModel.Name;
                                album.privacy = new VKAlbumPrivacy() { category = editAlbumViewModel.PrivacyView.ToString() };

                                this.VM.AlbumsVM.Items.Insert(pos, album);
                                dc.Hide();
                            }
                            else
                            {
                                //ExtendedMessageBox.ShowSafe(CommonResources.Error);
                                MessageBox.Show("Error", "", MessageBox.MessageBoxButton.OK);
                            }
                        });
                    });
                }

            });


            createAlbumUc.DataContext = editAlbumViewModel;
            //((UIElement)createAlbumUc).Visibility = Visibility.Visible;

            dc.Child = createAlbumUc;
            dc.Show();
        }

        private async void _appBarButtonDelete_Click(VKVideoAlbum album)
        {
            if (await MessageBox.Show("DeleteConfirmation", "Delete", MessageBox.MessageBoxButton.OKCancel) != MessageBox.MessageBoxButton.OK)
                return;
            this.VM.DeleteAlbum(album);
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

        private void _appBarButtonEdit_Click(VKVideoAlbum album)
        {
            this.ShowEditAlbum(album);
        }

        private void ShowMenu(FrameworkElement element)
        {
            PopUP2 menu = new PopUP2();

            if (element.DataContext is VKVideoAlbum album)
            {
                if (album.owner_id != Settings.UserId || album.id < 0)
                    return;

                PopUP2.PopUpItem item = new PopUP2.PopUpItem();

                item.Text = LocalizedStrings.GetString("Delete");

                item.Command = new DelegateCommand((args) =>
                {
                    this._appBarButtonDelete_Click(album);
                });
                menu.Items.Add(item);

                PopUP2.PopUpItem item2 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("Edit") };
                item2.Command = new DelegateCommand((args) =>
                {
                    this._appBarButtonEdit_Click(album);
                });
                menu.Items.Add(item2);

                menu.ShowAt(element);
            }
            else if (element.DataContext is VKVideoBase video)
            {
                if (this.searchViewModel == null)
                {
                    PopUP2.PopUpItem item = new PopUP2.PopUpItem();
                    item.Text = LocalizedStrings.GetString("VideoNew_RemovedFromMyVideos");
                    item.Command = new DelegateCommand((args) => {
                        this._videosViewModel.AddRemoveToMyVideos(video,false);//mItemAddToMyVideos_Click
                    });
                    menu.Items.Add(item);
                }


                if (video.can_add)
                {
                    PopUP2.PopUpItem menuItem1 = new PopUP2.PopUpItem();
                    menuItem1.Text = LocalizedStrings.GetString("VideoNew_AddToMyVideos");
                    menuItem1.Command = new DelegateCommand((args) =>
                    {
                        this.VM.AddRemoveToMyVideos(video,true);//mItemAddToMyVideos_Click
                    });
                    menu.Items.Add(menuItem1);
                }




                PopUP2.PopUpItem item2 = new PopUP2.PopUpItem();
                item2.Text = LocalizedStrings.GetString("VideoNew_AddToAlbum");
                item2.Command = new DelegateCommand((args) => {
                    mItemAddToAlbum_Click(video);
                });
                menu.Items.Add(item2);

                PopUP2.PopUpItem item3 = new PopUP2.PopUpItem();
                item3.Text = LocalizedStrings.GetString("CopyLink");
                item3.Command = new DelegateCommand((args) => {
                    //mItemCopyLink_Click
                    var dataPackage = new DataPackage();
                    dataPackage.SetText(string.Format("https://{0}vk.com/video{1}_{2}", CustomFrame.Instance.IsDevicePhone ? "m." : "", video.owner_id, video.id));
                    Clipboard.SetContent(dataPackage);
                });
                menu.Items.Add(item3);



                PopUP2.PopUpItem item4 = new PopUP2.PopUpItem();
                item4.Text = LocalizedStrings.GetString("ShareWallPost_Share/Text");
                item4.Command = new DelegateCommand((args) => {
                    //mItemAddToAlbum_Click
                });
                menu.Items.Add(item4);

                PopUP2.PopUpItem _appBarMenuItemFaveUnfave = new PopUP2.PopUpItem();
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

            if (menu.Items.Count > 0)
                menu.ShowAt(element);
        }

        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CatalogItemUC item = sender as CatalogItemUC;
            var vm = item.DataContext as VKVideoBase;
            NavigatorImpl.Instance.NavigateToVideoWithComments(vm.owner_id, vm.id, vm.access_key, vm, item.Img);
        }

        private void Album_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CatalogItemUC item = sender as CatalogItemUC;
            var vm = item.DataContext as VKVideoAlbum;
            NavigatorImpl.Instance.NavigateToVideoAlbum(vm.id, vm.title, vm.owner_id);
        }




        private void _appBarButtonSearch_Click(object sender)
        {
            if (this.searchViewModel != null)
            {
                CustomFrame.Instance.Header.ActivateSearch(true, true, this.searchViewModel.SearchString);
            }
            else
            {
                CustomFrame.Instance.Header.ActivateSearch(true, false);
                CustomFrame.Instance.Header.ActivateMoreOptionsInSearch(true);
                this.searchViewModel = new VideosSearchViewModel();
            }


            this._exLv.DataContext = this.searchViewModel;

            base.InitializeProgressIndicator(this.searchViewModel);
        }

        private void SearchClosed()
        {
            this._exLv.DataContext = this._videosViewModel;
            this.searchViewModel = null;
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
                this.searchViewModel.Items.Clear();

                this._exLv.NeedReload = true;
                this._exLv.Reload();
            };
            element = sharePostUC;


            statusChangePopup.Child = element;

            statusChangePopup.OverrideBackKey = true;
            statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            statusChangePopup.Show();
        }

        private void OnServerSearch(string text)
        {
            this.searchViewModel.SearchString = text;
            this.searchViewModel.Items.Clear();
            this._exLv.NeedReload = true;
            this._exLv.Reload();
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


    }
}
