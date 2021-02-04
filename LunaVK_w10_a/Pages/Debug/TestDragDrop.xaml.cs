using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestDragDrop : Page
    {
        public TestDragDrop()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Происходит, когда система ввода сообщает об основном событии перетаскивания, в котором данный элемент является местом назначения.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DropArea_Drop(object sender, DragEventArgs e)
        {
            Point point = e.GetPosition(null);
            var elements = VisualTreeHelper.FindElementsInHostCoordinates(point, this.root);
            foreach(var element in elements)
            {
                if(element!= this.root)
                    System.Diagnostics.Debug.WriteLine((element as FrameworkElement).Name);
            }

            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Any())
                {
                    var storeFile = items[0] as StorageFile;
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(await storeFile.OpenAsync(FileAccessMode.Read));
                    //dragedImage.Source = bitmapImage;
                }
            }

            this.ShowDropArea(false, false);
            //(sender as Panel).Background = new SolidColorBrush(Color.FromArgb(255, 216, 216, 216));
        }

        /// <summary>
        /// Происходит, когда система ввода сообщает об основном событии перетаскивания, в котором данный элемент является потенциальным местом назначения.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropArea_DragOver(object sender, DragEventArgs e)
        {
           // var items = await e.DataView.GetStorageItemsAsync();
            //var i3 = await e.DataView.GetResourceMapAsync();
            //var storeFile = items[0] as StorageFile;

            //System.Diagnostics.Debug.WriteLine("DropArea_DragOver");
            e.AcceptedOperation = DataPackageOperation.Copy;
        //    e.DragUIOverride.Caption = "You are dragging a image";
            e.DragUIOverride.IsCaptionVisible = false;
            //e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsGlyphVisible = false;

            //(sender as Panel).Background = new SolidColorBrush(Color.FromArgb(255, 168, 168, 168));
        }

        private void DropArea_DragLeave(object sender, DragEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("DropArea_DragLeave");
            //(sender as Panel).Background = new SolidColorBrush(Color.FromArgb(255, 216, 216, 216));
            this.ShowDropArea(false, false);
        }

        private void DropArea_DragEnter(object sender, DragEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("DropArea_DragEnter");
            this.temp(e);
            //(sender as Panel).Background = new SolidColorBrush(Color.FromArgb(255, 168, 255, 168));
        }

        private void DropArea_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("DropArea_DragStarting");
            
            //(sender as Panel).Background = new SolidColorBrush(Color.FromArgb(255, 255, 168, 168));
        }

        private async void temp(DragEventArgs args)
        {
            var items = await args.DataView.GetStorageItemsAsync();
            var storeFile = items[0] as StorageFile;
            //if (string.IsNullOrEmpty(storeFile.ContentType))
            //{
            //    DropArea2.Opacity = 1;
            //}
            //else
            //{
            //    DropArea.Opacity = 1;
            //    DropArea2.Opacity = 1;
            //}
            if (storeFile.ContentType.StartsWith("image"))
            {
                this.ShowDropArea(true, false);
            }
            else
            {
                this.ShowDropArea(true, true);
            }
        }

        private void DropArea2_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            (sender as Panel).Background = new SolidColorBrush(Color.FromArgb(255, 168, 168, 255));
        }

        private void DropArea2_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            (sender as Panel).Background = new SolidColorBrush(Color.FromArgb(255, 255, 168, 168));
        }

        private void DropArea2_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            (sender as Panel).Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 100));
        }

        private void ShowDropArea(bool show, bool documentOnly)
        {
            if(documentOnly)
            {
                Grid.SetRow(this.DropAreaDocument, 0);
                Grid.SetRowSpan(this.DropAreaDocument, 2);
            }
            else
            {
                Grid.SetRow(this.DropAreaDocument, 1);
                Grid.SetRowSpan(this.DropAreaDocument, 1);
            }

            if(!show)
            {
                this.DropAreaDocument.Visibility = Visibility.Collapsed;
                this.DropAreaImage.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.DropAreaDocument.Visibility = Visibility.Visible;

                if(!documentOnly)
                    this.DropAreaImage.Visibility = Visibility.Visible;
            }
        }
    }
}
