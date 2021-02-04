using System;
using System.Collections.Generic;
using System.Text;

using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using LunaVK.Core.DataObjects;
using Windows.UI.Xaml.Media;
using Windows.Storage.Streams;
using LunaVK.Core.Utils;
using LunaVK.Core.Library;
using LunaVK.Library;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;

namespace LunaVK.UC
{
    public class MediaPresenter : Canvas
    {
        public NewsPhotosInfo _newsPhotosInfo = null;

        private List<VKPhoto> _list;

        /// <summary>
        /// Коллекция медиаэлементов.
        /// </summary>
        public ObservableCollection<ThumbnailsLayoutHelper.IThumbnailSupport> Items { get; private set; }

        private const double ThumbnailsMargin = 4;
        /// <summary>
        /// Максимальный размер прямоугольной области, которую может занять сетка с элементами.
        /// </summary>
        public Rectangle MaxRectSize { get; set; }

        public byte CornerRadius;

        /// <summary>
        /// Инициализирует новый экземрляр класса.
        /// </summary>
        public MediaPresenter()
        {
            this.Items = new ObservableCollection<ThumbnailsLayoutHelper.IThumbnailSupport>();
            this._list = new List<VKPhoto>();
        }

        public void ReMeasure(double width)
        {
            if (this.MaxRectSize.Width == width && this.Items.Count == 1)
                return;
            /*
             * Если вложено несколько видео, то лучше пересчитать,
             * а инчае плохо выглядит
             * */



            this.MaxRectSize = new Rectangle(width, this.MaxRectSize.Height);
            ThumbnailsLayoutHelper.CalculateThumbnailSizes(this.MaxRectSize, this.Items, ThumbnailsMargin);

            double currentWidth = 0;
            double currentHeight = 0;

            for (int i = 0; i < this.Items.Count; i++)
            {
                var size = this.Items[i].ThumbnailSize;
                //
                if (size.Width <= 0 || size.Height<=0)
                    continue;//todo: размер по-умолчанию надо выставить?
                //
                FrameworkElement u = base.Children[i] as FrameworkElement;
                //
                if (u==null)
                    continue;//todo: такого не должно происходить!
                //
                u.Width = size.Width;
                u.Height = size.Height;
                
                Canvas.SetLeft(u, currentWidth);
                Canvas.SetTop(u, currentHeight);
                
                if (size.LastRow)
                {
                    currentHeight += (size.Height + ThumbnailsMargin);
                    if (i == Items.Count - 1)
                        currentWidth += (size.Width + ThumbnailsMargin);
                }
                else
                    currentWidth += (size.Width + ThumbnailsMargin);

                if (size.LastColumn)
                {
                    currentHeight += (size.Height + ThumbnailsMargin);
                    currentWidth = 0;
                }
            }

            currentHeight = 0;
            currentWidth = 0;

            for (int i = 0; i < base.Children.Count; i++)
            {
                var child = base.Children[i] as FrameworkElement;
                if (child == null) continue;

                double h = Canvas.GetTop(child) + child.Height;
                double w = Canvas.GetLeft(child) + child.Width;
                if (h > currentHeight) currentHeight = h;
                if (w > currentWidth) currentWidth = w;
            }


            if (this.CornerRadius > 0)
            {
                for (int j = 0; j < base.Children.Count; j++)
                {
                    var child = base.Children[j] as Border;

                    if (child == null)
                        continue;

                    var border = child.Child as Border;

                    if (border == null)
                        continue;

                    double offsetX = Canvas.GetLeft(child);
                    double offsetY = Canvas.GetTop(child);

                    bool flagX = (currentWidth - (child.Width + offsetX)) < 0.5;
                    bool flagY = (currentHeight - (child.Height + offsetY)) < 0.5;

                    //System.Diagnostics.Debug.WriteLine(string.Format("{6}->offsetX:{0} h:{1} all_size:({2}*{3}) {4}*{5}  {7}_{8}", offsetX, offsetY, currentWidth, currentHeight, child.Width, child.Height, j, flagX, flagY));

                    bool roundTopLeft = offsetX == 0 && offsetY == 0;
                    bool roundTopRight = flagX == true && offsetY == 0;
                    bool roundBottomLeft = flagY == true && offsetX == 0;
                    bool roundBottomRight = flagY == true && flagX == true;

                    child.CornerRadius = border.CornerRadius = new CornerRadius(roundTopLeft ? this.CornerRadius : 0, roundTopRight ? this.CornerRadius : 0, roundBottomRight ? this.CornerRadius : 0, roundBottomLeft ? this.CornerRadius : 0);
                }
            }



            base.Height = currentHeight;
            base.Width = currentWidth;
        }

        /// <summary>
        /// Обновить элемент управления.
        /// </summary>
        public void Update()
        {
            if (this.MaxRectSize.Height == 0 && this.MaxRectSize.Width == 0)
                this.MaxRectSize = new Rectangle(600, 450);//этого не должно происходить :)

            base.Children.Clear();

            ThumbnailsLayoutHelper.CalculateThumbnailSizes(this.MaxRectSize, this.Items, ThumbnailsMargin);

            double currentWidth = 0;
            double currentHeight = 0;

            int i = 0;
            foreach(var item in this.Items)
            //for (int i = 0; i < this.Items.Count; i++)
            {
                var size = this.Items[i].ThumbnailSize;
                
                //if (size.Width==0 || size.Height==0)
                //{
                //    int im = 0;
                //}

                if (item is VKPhoto photo)
                {
                    this._list.Add(photo);
                }
                //
                if (item is UserControl uc)
                {
                    uc.Width = size.Width;
                    uc.Height = size.Height;
                    Canvas.SetLeft(uc, currentWidth);
                    Canvas.SetTop(uc, currentHeight);
                    base.Children.Add(uc);
                }
                else if (item is OutboundPhotoAttachment outboundPhoto)
                {
                    Border brd = new Border()
                    {
                        Width = size.Width,
                        Height = size.Height
                    };
                    Image img = new Image();
                    img.Tag = i;



                    img.Source = outboundPhoto.ImageSrc;
                    img.Opacity = 0;
                    img.Loaded += img_Loaded;

//                    this._list.Add(o.server_photo);
//                    img.Tapped += img_Tapped;

                    img.Stretch = Stretch.UniformToFill;
                    img.VerticalAlignment = VerticalAlignment.Center;
                    img.HorizontalAlignment = HorizontalAlignment.Center;
                    brd.Child = img;
                    //
                    Canvas.SetLeft(brd, currentWidth);
                    Canvas.SetTop(brd, currentHeight);
                    base.Children.Add(brd);
                }/*
                else if (this.Items[i] is Library.OutboundGraffitiAttachment) оно здесь не бывает
                {
                    var o = this.Items[i] as Library.OutboundGraffitiAttachment;
                    Border item = new Border()
                    {
                        Width = size.Width,
                        Height = size.Height
                    };
                    Image img = new Image();
                    //img.Source = o.LocalUrl2;

                    img.Stretch = Windows.UI.Xaml.Media.Stretch.UniformToFill;
                    img.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                    img.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                    item.Child = img;
                    //
                    Canvas.SetLeft(item, currentWidth);
                    Canvas.SetTop(item, currentHeight);
                    base.Children.Add(item);
                }*/
                
                else
                {
                    //VirtualizableImage

                    Border brdPlaceHolder = new Border()
                    {
                        Width = size.Width,//
                        Height = size.Height,//
                        Style = (Style)Application.Current.Resources["BorderTheme"]
                    };

#if DEBUG
                    //brd.Background = new SolidColorBrush(Windows.UI.Colors.Yellow);
                    //brd.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                    //brd.BorderThickness = new Thickness(2);


                    //TextBlock tb = new TextBlock() { Text = i.ToString(), FontSize=25, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush(Windows.UI.Colors.Red) };
                    //brd.Child = tb;
#endif

                    
                    //var brush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(item.ThumbnailSource)), Stretch = Stretch.UniformToFill };
                    
                    Border brd = new Border();
                    //brd.Background = brush;
                    brd.Tag = i;
                    brd.Tapped += Brd_Tapped;
                    brd.Opacity = 0;
                    this.TrySetImageForUri(item.ThumbnailSource, brd);
                    //brush.ImageOpened += delegate
                    //{
                    //    brd.Animate(0, 1, "Opacity", 300);
                    //};

                    brdPlaceHolder.Child = brd;
                    /*
                    Image img = new Image();
                    img.Tag = i;
                    img.Tapped += img_Tapped;
                    img.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(item.ThumbnailSource));
                    img.Stretch = Stretch.UniformToFill;//original
             //       img.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
             //       img.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                    img.Opacity = 0;
                    img.ImageOpened += img_ImageOpened;

                    brd.Child = img;
                    */
                    //
                    Canvas.SetLeft(brdPlaceHolder, currentWidth);
                    Canvas.SetTop(brdPlaceHolder, currentHeight);
                    base.Children.Add(brdPlaceHolder);
                }

                i++;

                if (size.LastRow)
                {
                    currentHeight += (size.Height + ThumbnailsMargin);
                    if (i == Items.Count - 1)
                        currentWidth += (size.Width + ThumbnailsMargin);
                }
                else
                    currentWidth += (size.Width + ThumbnailsMargin);

                if (size.LastColumn)
                {
                    currentHeight += (size.Height + ThumbnailsMargin);
                    currentWidth = 0;
                }               
            }

            currentHeight = 0;
            currentWidth = 0;

            for (int j = 0; j < base.Children.Count; j++)
            {
                var child = base.Children[j] as FrameworkElement;

                if (child == null)
                    continue;


                double h = Canvas.GetTop(child) + child.Height;
                double w = Canvas.GetLeft(child) + child.Width;

                if (h > currentHeight)
                    currentHeight = h;
                if (w > currentWidth)
                    currentWidth = w;
            }

            
            

            base.Height = currentHeight;
            base.Width = currentWidth;
        }

        public async void TrySetImageForUri(string uriString, Border border)
        {
            if (uriString.StartsWith("http"))
            {
                var brush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(uriString)), Stretch = Stretch.UniformToFill };

                border.Background = brush;

                brush.ImageOpened += delegate
                {
                    border.Animate(0, 1, "Opacity", 300);
                };
            }
            else if (uriString.StartsWith("{"))
            {
                border.Opacity = 1.0;

                try
                {
                    StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(uriString);
                    bool r = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.CheckAccess(file);
                    if (r)
                    {
                        using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                        {

                            BitmapImage _bitmap = new BitmapImage();// create a new bitmap, coz the old one must be done for...

                            await _bitmap.SetSourceAsync(fileStream);// And get that bitmap sucked in from stream.

                            var brush = new ImageBrush() { ImageSource = _bitmap, Stretch = Stretch.UniformToFill };
                            border.Background = brush;
                        }
                    }
                }
                catch
                {
                    int i = 0;
                }
            }
            else
            {
                border.Opacity = 1.0;

                try
                {

                    StorageFile file = await StorageFile.GetFileFromPathAsync(uriString);
                    bool r = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.CheckAccess(file);
                    if (r)
                    {
                        using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                        {

                            BitmapImage _bitmap = new BitmapImage();// create a new bitmap, coz the old one must be done for...

                            await _bitmap.SetSourceAsync(fileStream);// And get that bitmap sucked in from stream.

                            var brush = new ImageBrush() { ImageSource = _bitmap, Stretch = Stretch.UniformToFill };
                            border.Background = brush;
                        }
                    }
                }
                catch
                {
                    int i = 0;
                }

               
            }
        }

        private void Brush_ImageOpened(object sender, RoutedEventArgs e)
        {
            ImageBrush img = sender as ImageBrush;
            img.Animate(0, 1, "Opacity", 300);
            img.ImageOpened -= Brush_ImageOpened;
        }

        void img_Loaded(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            img.Animate(0, 1, "Opacity", 300);
            img.Loaded -= img_Loaded;
        }

        void img_ImageOpened(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            img.Animate(0, 1, "Opacity", 300);
            img.ImageOpened -= img_ImageOpened;
        }
        
        private Border GetImageFunc(int index)
        {
            var children = base.Children.Where( (element)=> { return element is Border; } ).ToList();
            Border brdPlaceHolder = children[index] as Border;
            return brdPlaceHolder.Child as Border;
        }

        private void Brd_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.img_Tapped(sender, e);
        }

        private void img_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FrameworkElement img = sender as FrameworkElement;
            int index = (int)img.Tag;

            if(this._newsPhotosInfo!=null)
            {
                NavigatorImpl.Instance.NaviateToImageViewerPhotoFeed(this._newsPhotosInfo.SourceId, this._list[0].album_id.ToString(), (uint)this._list.Count, index, this._newsPhotosInfo.Date, this._list, this._newsPhotosInfo.NewsType == NewsPhotosInfo.NewsPhotoType.Photo ? "Photos" : "PhotoTags", this.GetImageFunc);
            }
            else
            {
                //                NavigatorImpl.Instance.NavigateToImageViewer((uint)this.PhotosCount, 0, index, this.PhotosListFromAttachments, "PhotosByIds", false, /*this._friendsOnly*/false, this.GetImageFunc, false);
                NavigatorImpl.Instance.NavigateToImageViewer((uint)this.PhotosCount, 0, index, this.PhotosListFromAttachments, ViewModels.ImageViewerViewModel.ViewerMode.PhotosByIds, this.GetImageFunc);
            }
            
        }



        private int PhotosCount
        {
            get
            {
                /*
                switch (this.ItemType)
                {
                    case ThumbsItem.ItemDataType.Attachment:
                        return Enumerable.Count<Attachment>(this._attachments, (Func<Attachment, bool>)(a => a.type == "photo"));
                    case ThumbsItem.ItemDataType.NewsPhotosInfo:
                        return this._newsPhotosInfo.Photos.Count;
                    default:
                        return 0;
                }*/
                return this.Items.Count(a => a is VKPhoto);
            }
        }

        private List<VKPhoto> PhotosListFromAttachments
        {
            get
            {
                return this.Items.Where((a => a is VKPhoto)).Select<object, VKPhoto>((a => (VKPhoto)a)).ToList();
            }
        }

        public class NewsPhotosInfo
        {
            public int SourceId { get; set; }

            public DateTime Date { get; set; }

            public NewsPhotosInfo.NewsPhotoType NewsType { get; set; }

            public uint Count { get; set; }

            public List<VKPhoto> Photos { get; set; }

            public enum NewsPhotoType
            {
                Photo,
                PhotoTag,
            }
        }
    }
}
