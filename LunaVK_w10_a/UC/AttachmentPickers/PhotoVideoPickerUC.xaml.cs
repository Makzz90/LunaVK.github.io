using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace LunaVK.UC.AttachmentPickers
{
    public sealed partial class PhotoVideoPickerUC : UserControl
    {
        public PhotoPickerAlbumsViewModel PhotosViewModel { get; set; }
        public Action<IReadOnlyList<IOutboundAttachment>> AttachmentsAction;

        /// <summary>
        /// Количество уже вложенных файлов
        /// </summary>
        byte Exists = 0;

        /// <summary>
        /// Ограничение на количество вложений
        /// </summary>
        byte Maximum = 10;

        public PhotoVideoPickerUC()
        {
            this.PhotosViewModel = new PhotoPickerAlbumsViewModel();
            base.DataContext = this.PhotosViewModel;
            this.InitializeComponent();
            this.Loaded += PhotoVideoPickerUC_Loaded;
        }

        public PhotoVideoPickerUC(byte exists, byte max):this()
        {
            this.Exists = exists;
            this.Maximum = max;
        }

        private void PhotoVideoPickerUC_Loaded(object sender, RoutedEventArgs e)
        {
        //    this.PhotosViewModel.LoadData(true);
        }

        private void _variableGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GridView lv = sender as GridView;

            foreach (var selected in e.AddedItems)
            {
                GridViewItem item = lv.ContainerFromItem(selected) as GridViewItem;
                VisualStateManager.GoToState(item, "Selected", true);
            }

            foreach (var unselected in e.RemovedItems)
            {
                GridViewItem item = lv.ContainerFromItem(unselected) as GridViewItem;
                VisualStateManager.GoToState(item, "Unselected", true);
            }
        }

        private void AttachAction_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TaskScheduler2.Clear();

            List<IOutboundAttachment> ret = new List<IOutboundAttachment>();

            foreach (var item in this.PhotosViewModel.Photos)
            {
                if (item.IsSelected)
                {
                    if (item.IsVideoVisibility == Visibility.Collapsed)
                    {
                        OutboundPhotoAttachment a = new OutboundPhotoAttachment();
                        a.sf = item.sf;
                        a.ImgWidth = item.BitmapImage.PixelWidth;
                        a.ImgHeight = item.BitmapImage.PixelHeight;
                        a.LocalUrl2 = item.BitmapImage;
                        ret.Add(a);
                    }
                    else
                    {
                        OutboundVideoAttachment a = new OutboundVideoAttachment(item.sf, true, 0);

                        ret.Add(a);
                    }

                }
            }

            this.AttachmentsAction?.Invoke(ret);
        }

        private void VariableSizedWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            VariableSizedWrapGrid grid = sender as VariableSizedWrapGrid;
            /*
            int del = 1;
            switch (CustomFrame.Instance.MenuState)
            {
                case Framework.CustomFrame.MenuStates.StateMenuCollapsedContentStretch:
                    {
                        del = 4;
                        break;
                    }
                case Framework.CustomFrame.MenuStates.StateMenuFixedContentFixed:
                    //case Framework.CustomFrame.MenuStates.StateMenuNarrowContentFixed:
                    {
                        del = 8;
                        break;
                    }
                case Framework.CustomFrame.MenuStates.StateMenuNarrowContentStretch:
                    {
                        del = 4;
                        break;
                    }
            }
            */
            int del = (int)(e.NewSize.Width / 130.0);
            grid.MaximumRowsOrColumns = del;
            grid.ItemHeight = grid.ItemWidth = e.NewSize.Width / del;
        }

        private void Photo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int selected = this.PhotosViewModel.Attached;

            FrameworkElement element = sender as FrameworkElement;
            PhotoPickerAlbumsViewModel.AlbumPhoto vm = element.DataContext as PhotoPickerAlbumsViewModel.AlbumPhoto;
            GridViewItem item = this._variableGridView.ContainerFromItem(vm) as GridViewItem;

            if (this._variableGridView.SelectedItems.Contains(vm))
            {
                if (selected + this.Exists >= this.Maximum)
                {
                    this._variableGridView.SelectedItems.Remove(vm);
                    return;
                }
                selected++;
                vm.Number = selected;
                vm.IsSelected = true;
            }
            else
            {
                vm.Number = 0;

                List<PhotoPickerAlbumsViewModel.AlbumPhoto> list = new List<PhotoPickerAlbumsViewModel.AlbumPhoto>();
                foreach (var p in PhotosViewModel.Photos)
                {
                    if (p.Number > 0)
                    {
                        list.Add(p);
                    }
                }

                var tttt = list.OrderBy((p2) => { return p2.Number; });

                int j = 1;

                foreach (var p3 in tttt)
                {
                    p3.Number = j;
                    j++;
                }

                vm.IsSelected = false;

                selected--;//для нижней кнопки
            }

            this.PhotosViewModel.UpdateUI();
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            img.Animate(0, 1, "Opacity", 600);
        }

        private async void UploadPhoto_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Допустимые форматы: JPG, PNG, GIF.
            //Допустимые форматы: AVI, MP4, 3GP, MPEG, MOV, MP3, FLV, WMV.
            //Допустимые форматы: любые форматы за исключением mp3 и исполняемых файлов. 
            Windows.Storage.Pickers.FileOpenPicker fileOpenPicker = new Windows.Storage.Pickers.FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add(".jpeg");
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.FileTypeFilter.Add(".gif");

            fileOpenPicker.FileTypeFilter.Add(".mp4");


            fileOpenPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;

            /*
            var view = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView();
            view.Activated += this.view_Activated;
            
            fileOpenPicker.PickSingleFileAndContinue();
            */

            StorageFile file = await fileOpenPicker.PickSingleFileAsync();

            if (file != null)
            {
                IOutboundAttachment a;
                if (file.ContentType.Contains("video"))
                {
                    a = new OutboundVideoAttachment(file);
                }
                else
                {
                    var photo = new OutboundPhotoAttachment();
                    photo.sf = file;

                    BitmapImage bimg = new BitmapImage();

                    using (var stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        bimg.SetSource(stream);
                    }
                    photo.LocalUrl2 = bimg;

                    a = photo;
                }
                
                List<IOutboundAttachment> ret = new List<IOutboundAttachment>();
                ret.Add(a);

                this.AttachmentsAction?.Invoke(ret);
            }
        }

        private void Cancel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            List<IOutboundAttachment> ret = new List<IOutboundAttachment>();
            this.AttachmentsAction?.Invoke(ret);
        }
    }
}
