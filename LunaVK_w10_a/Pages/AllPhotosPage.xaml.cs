using LunaVK.Common;
using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.UC;
using LunaVK.UC.PopUp;
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
    public sealed partial class AllPhotosPage : PageBase
    {
        private double _scrollPosition;
        private OptionsMenuItem _appBarButtonCreate;

        public AllPhotosPage()
        {
            this.InitializeComponent();
            this._exGridView.Loaded2 += this._exGridView_Loaded2;
            this._appBarButtonCreate = new OptionsMenuItem() { Icon = "\xE710", Clicked = this._appBarButtonCreate_Click };
        }

        private void _exGridView_Loaded2(object sender, RoutedEventArgs e)
        {
            if (this._scrollPosition > 0)
                (sender as ScrollViewer).ChangeView(0, this._scrollPosition, 1.0f);

            if (this.VM._ownerId == 0 || this.VM._ownerId == Settings.UserId)
            {
                CustomFrame.Instance.Header.OptionsMenu.Add(this._appBarButtonCreate);
            }
        }

        private void Albums_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToPhotoAlbums(this.VM._ownerId, this.VM._ownerName);
        }

        public PhotosMainViewModel VM
        {
            get { return base.DataContext as PhotosMainViewModel; }
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                this._scrollPosition = (double)pageState["ScrollPosition"];
                this._exGridView.NeedReload = false;
            }
            else
            {
                Dictionary<string, object> QueryString = navigationParameter as Dictionary<string, object>;
                int owner_id = (int)QueryString["Id"];

                base.DataContext = new PhotosMainViewModel(owner_id);

                if (QueryString.ContainsKey("OwnerName"))
                    this.VM._ownerName = (string)QueryString["OwnerName"];
            }
            string temp = LocalizedStrings.GetString("Menu_Photos") + " " + this.VM._ownerName;
            base.Title = temp;
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this.VM;
            pageState["ScrollPosition"] = this._exGridView.GetInsideScrollViewer.VerticalOffset;
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            img.Animate(0, 1, "Opacity", 300);
            img.ImageOpened -= Image_ImageOpened;
        }

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VKPhoto photo = (sender as FrameworkElement).DataContext as VKPhoto;

            int index = this.VM.Items.IndexOf(photo);
            NavigatorImpl.Instance.NavigateToImageViewer(this.VM.PhotosCount,0,index,this.VM.Items.ToList(), ImageViewerViewModel.ViewerMode.Photos,this.GetImageFunc);
        }

        private Border GetImageFunc(int index)
        {
            GridViewItem item = this._exGridView.GetGridView.ContainerFromIndex(index) as GridViewItem;
            if (item == null)
                return null;
            UIElement ee = item.ContentTemplateRoot;
            if (ee == null)
                return null;
            Border brd = ee as Border;
            if (brd == null)
            {
                if(ee is ImageFadeInUC fade)
                {
                    return fade.Brd;
                }
                return null;
            }
            return brd;
        }

        private void PhotoAlbumUC_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VKAlbumPhoto vm = (sender as FrameworkElement).DataContext as VKAlbumPhoto;
            NavigatorImpl.Instance.NavigateToPhotosOfAlbum(vm.owner_id,vm.id,vm.title);
        }

        private void FillRowView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 600)
            {
                this.VM.UpdateRowItemsCount(2);
            }
            else if (e.NewSize.Width >= 600 && e.NewSize.Width < 900)
            {
                this.VM.UpdateRowItemsCount(3);
            }
            else
            {
                this.VM.UpdateRowItemsCount(4);
            }
        }

        private void PhotoAlbumUC_Holding(object sender, HoldingRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                FrameworkElement element = sender as FrameworkElement;
                this.ShowMenu(element);
            }
        }

        private void PhotoAlbumUC_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                return;

            FrameworkElement element = sender as FrameworkElement;
            this.ShowMenu(element);
        }

        private void ShowMenu(FrameworkElement element)
        {
            var vm = element.DataContext as VKAlbumPhoto;

            if (vm.owner_id != Settings.UserId || vm.id < 0)
                return;

            PopUP2 menu = new PopUP2();
            PopUP2.PopUpItem item = new PopUP2.PopUpItem();

            item.Text = LocalizedStrings.GetString("Delete");

            item.Command = new DelegateCommand((args) =>
            {
                this._appBarButtonDelete_Click(vm);
            });
            menu.Items.Add(item);

            PopUP2.PopUpItem item2 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("Edit") };
            item2.Command = new DelegateCommand((args) =>
            {
                this._appBarButtonEdit_Click(vm);
            });
            menu.Items.Add(item2);

            menu.ShowAt(element);
        }

        private void _appBarButtonEdit_Click(VKAlbumPhoto album)
        {
            this.ShowEditAlbum(album);
        }

        private void _appBarButtonCreate_Click(object sender)
        {
            this.ShowEditAlbum(null);
        }

        private void ShowEditAlbum(VKAlbumPhoto album)
        {
            PopUpService dc = new PopUpService() { AnimationTypeChild = PopUpService.AnimationTypes.Slide, OverrideBackKey = true };

            CreateEditAlbumViewModel editAlbumViewModel = new CreateEditAlbumViewModel(album);

            CreateAlbumUC createAlbumUc = new CreateAlbumUC(() =>
            {
                if (album == null)
                {
                    this.VM.SetInProgress(true);
                    PhotosService.Instance.CreateAlbum(editAlbumViewModel.Name, editAlbumViewModel.Description, editAlbumViewModel.PrivacyView.ToString(), editAlbumViewModel.PrivacyComment.ToString(), (result) =>
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            if (result.error.error_code == Core.Enums.VKErrors.None)
                            {
                                VKAlbumPhoto a = result.response;
                                a.sizes = new List<DocPreviewPhotoSize>();
                                
                                a.sizes.Add(new DocPreviewPhotoSize() { height = 97, width = 130, type="m", src= "https://vk.com/images/m_noalbum.png" });
                                this.VM.AlbumsVM.Items.Insert(0, result.response);
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
                    PhotosService.Instance.EditAlbum(album.id, editAlbumViewModel.Name, editAlbumViewModel.Description, editAlbumViewModel.PrivacyView.ToString(), editAlbumViewModel.PrivacyComment.ToString(), (result) =>
                    {
                        Execute.ExecuteOnUIThread(() => {
                            if (result.error.error_code == Core.Enums.VKErrors.None && result.response == 1)
                            {
                                //AddOrUpdateAlbum
                                
                                int pos = this.VM.AlbumsVM.Items.IndexOf(album);
                                this.VM.AlbumsVM.Items.Remove(album);

                                album.title = editAlbumViewModel.Name;
                                album.description = editAlbumViewModel.Description;
                                album.privacy_view = new VKAlbumPrivacy() { category = editAlbumViewModel.PrivacyView.ToString() };
                                album.privacy_comment = new VKAlbumPrivacy() { category = editAlbumViewModel.PrivacyComment.ToString() };

                                this.VM.AlbumsVM.Items.Insert(pos,album);
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

        private async void _appBarButtonDelete_Click(VKAlbumPhoto album)
        {
            if (await MessageBox.Show("DeleteConfirmation", "Delete", MessageBox.MessageBoxButton.OKCancel) != MessageBox.MessageBoxButton.OK)
                return;
            this.VM.DeleteAlbum(album);
        }

        private void ImageFadeInUC_Holding(object sender, HoldingRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                FrameworkElement element = sender as FrameworkElement;
                this.ShowMenu(element);
            }
        }

        private void ImageFadeInUC_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                return;

            FrameworkElement element = sender as FrameworkElement;
            this.ShowMenuForPhoto(element);
        }

        private void ShowMenuForPhoto(FrameworkElement element)
        {
            var vm = element.DataContext as VKPhoto;

            if (vm.owner_id != Settings.UserId)
                return;

            PopUP2 menu = new PopUP2();
            PopUP2.PopUpItem item = new PopUP2.PopUpItem();

            item.Text = LocalizedStrings.GetString("Delete");

            item.Command = new DelegateCommand((args) =>
            {
                this._appBarButtonDeletePhoto_Click(vm);
            });
            menu.Items.Add(item);

            menu.ShowAt(element);
        }

        private async void _appBarButtonDeletePhoto_Click(VKPhoto photo)
        {
            if (await MessageBox.Show("DeleteConfirmation", "Delete", MessageBox.MessageBoxButton.OKCancel) != MessageBox.MessageBoxButton.OK)
                return;
            this.VM.DeletePhoto(photo);
        }
    }
}
