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
using LunaVK.Core.DataObjects;
using LunaVK.Core;
using LunaVK.UC;
using LunaVK.Framework;
using LunaVK.Common;
using LunaVK.UC.PopUp;
using LunaVK.Library;
using LunaVK.Core.Framework;
using LunaVK.ViewModels;
using LunaVK.Core.Library;

namespace LunaVK
{
    /// <summary>
    /// Альбомы
    /// </summary>
    public sealed partial class PhotoAlbumPage : PageBase
    {
        private double _scrollPosition;
        private OptionsMenuItem _appBarButtonCreate;

        public PhotoAlbumPage()
        {
            this.InitializeComponent();

            this._appBarButtonCreate = new OptionsMenuItem() { Icon = "\xE710", Clicked = this._appBarButtonCreate_Click };

            this._exGridView.Loaded2 += _exGridView_Loaded2;
        }

        private void _exGridView_Loaded2(object sender, RoutedEventArgs e)
        {
            base.InitializeProgressIndicator();

            if (this._scrollPosition > 0)
            {
                this._exGridView.GetInsideScrollViewer.ChangeView(0, this._scrollPosition, 1.0f);
            }

            if (this.VM._ownerId == 0 || this.VM._ownerId == Settings.UserId)
            {
                CustomFrame.Instance.Header.OptionsMenu.Add(this._appBarButtonCreate);
            }
        }

        public PhotoAlbumViewModel VM
        {
            get { return base.DataContext as PhotoAlbumViewModel; }
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

                base.DataContext = new PhotoAlbumViewModel(owner_id);

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

        //Navigator.Current.NavigateToImageViewer
        private void PhotoAlbumUC_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKAlbumPhoto;

            NavigatorImpl.Instance.NavigateToPhotosOfAlbum(vm.owner_id, vm.id, vm.title);
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

            if (vm.owner_id != Settings.UserId)
                return;

            PopUP2 menu = new PopUP2();
            PopUP2.PopUpItem item = new PopUP2.PopUpItem();

            item.Text = "EditAlbumPage_AppBar_Delete";

            item.Command = new DelegateCommand((args) =>
            {
                this._appBarButtonDelete_Click(vm);
            });
            menu.Items.Add(item);

            PopUP2.PopUpItem item2 = new PopUP2.PopUpItem() { Text = "PhotoAlbumPage_AppBar_Edit" };
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

            CreateAlbumUC createAlbumUc = new CreateAlbumUC(()=> 
            {
                if(album==null)
                {
                    this.VM.SetInProgress(true);
                    PhotosService.Instance.CreateAlbum(editAlbumViewModel.Name, editAlbumViewModel.Description, editAlbumViewModel.PrivacyView.ToString(), editAlbumViewModel.PrivacyComment.ToString(), (result) =>
                    {
                        Execute.ExecuteOnUIThread(() =>
                        { 
                        if(result.error.error_code == Core.Enums.VKErrors.None)
                        {
                                this.VM.Items.Insert(0, result.response);
                                this.VM.UpdateAlbumsCount();
                                dc.Hide();
                            }
                        else
                            {
                                //ExtendedMessageBox.ShowSafe(CommonResources.Error);
                                MessageBox.Show("Error","", MessageBox.MessageBoxButton.OK);
                            }
                        });
                    },
                    editAlbumViewModel._groupId);
                }
                else
                {
                    this.VM.SetInProgress(true);
                    PhotosService.Instance.EditAlbum(album.id, album.title, album.description, editAlbumViewModel.PrivacyView.ToString(), editAlbumViewModel.PrivacyComment.ToString(), (result) =>
                    {
                        Execute.ExecuteOnUIThread(() => {
                            if (result.error.error_code == Core.Enums.VKErrors.None && result.response == 1)
                            {
                                //this.VM.Items.Insert(0, result.response);
                                //AddOrUpdateAlbum
                                int pos = this.VM.Items.IndexOf(album);
                                this.VM.Items.Remove(album);
                                this.VM.Items.Insert(pos,album);
                                this.VM.UpdateAlbumsCount();
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
            if (await MessageBox.Show("GenericConfirmation", "DeleteAlbum", MessageBox.MessageBoxButton.OKCancel) != MessageBox.MessageBoxButton.OK)
                return;
            this.VM.DeleteAlbum(album);
        }

    }
}
