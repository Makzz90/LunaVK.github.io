using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using LunaVK.Library;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestElementPosition : Page
    {
        public TestElementPosition()
        {
            this.InitializeComponent();
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            /*
            Point buttonPosition = e.GetPosition(this);
            var transform = Ball.TransformToVisual(this);
            Point ballPosition = transform.TransformPoint(new Point());
            //Point ballCenter = new Point();
            Point to = new Point(buttonPosition.X- ballPosition.X, buttonPosition.Y - ballPosition.Y);

            var animation = myStory.Children[0] as PointAnimation;
            animation.To = to;
            myStory.Begin();
            */

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VKPhoto photo = new VKPhoto();
            photo.id = 100;
            photo.album_id = 200;
            photo.owner_id = 300;
            photo.sizes = new Dictionary<char, VKImageWithSize>();


            VKImageWithSize temp = new VKImageWithSize();
            temp.height = 100;
            temp.width = 200;
            temp.url = "temp";
            temp.type = 'm';

            photo.sizes.Add('n',temp);



            VKImageWithSize temp2 = new VKImageWithSize();
            temp2.height = 100;
            temp2.width = 200;
            temp2.url = "temp";
            temp2.type = 'p';

            photo.sizes.Add('o', temp2);


            photo.width = 400;
            photo.height = 500;
            photo.text = "600";
            photo.access_key = "700";
            photo.user_id = 800;

            var res = CacheManager.TrySerialize(photo, "VKImageWithSize");
            int i = 0;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            VKPhoto photo = new VKPhoto();
            var res = CacheManager.TryDeserialize(photo, "VKImageWithSize");
            int i = 0;
        }

        private Border GetImageFunc(int index)
        {
            return this._brd;
        }

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //NavigatorImpl.Instance.NavigateToImageViewer("", ViewModels.ImageViewerViewModel.AlbumType.NormalAlbum, 0, 1, 0, null, this.GetImageFunc);

            Image imageFit = new Image();

            imageFit.Width = this._overlay.ActualWidth;
            imageFit.Height = this._overlay.ActualHeight;

            imageFit.Source = _originalImg.Source;
            //imageFit.Stretch = Stretch.UniformToFill;//для канваса не нужно
            //imageFit.HorizontalAlignment = HorizontalAlignment.Center;
            
            imageFit.Loaded += ImageFit_Loaded;
            
            this._overlay.Children.Add(imageFit);
        }

        private void ImageFit_Loaded(object sender, RoutedEventArgs e)
        {
            Image imageFit = sender as Image;

            Size childSize = new Size(imageFit.ActualWidth, imageFit.ActualHeight);
            Rect fill = RectangleUtils.ResizeToFill(new Size(_originalImg.ActualWidth, _originalImg.ActualHeight), childSize);
            //Rect fill = RectangleUtils.ResizeToFill(RectangleUtils.ResizeToFill(new Size(_originalImg.ActualWidth, _originalImg.ActualHeight), new Size(300,300)), childSize);//orig
            Rect target = _originalImg.TransformToVisual(imageFit).TransformBounds(fill);
            CompositeTransform tr = RectangleUtils.TransformRect(new Rect(new Point(), childSize), target, false);//позиционирует и вычисляет масштаб
            imageFit.RenderTransform = tr;



            
            // Возвращает объект преобразования, который может использоваться для преобразования
            // координат из UIElement в заданный объект.
            GeneralTransform visual = _originalImg.TransformToVisual(imageFit);
            
            Rect rect1;
            if (_originalImg.Parent is FrameworkElement parent)
                rect1 = new Rect(0.0, 0.0, parent.ActualWidth, parent.ActualHeight);
            else
                rect1 = new Rect(0.0, 0.0, _originalImg.ActualWidth, _originalImg.ActualHeight);//orig

            Rect source = visual.TransformBounds(rect1);
            CompositeTransform compositeTransform1 = new CompositeTransform();

            //CompositeTransform compositeTransform3 = RectangleUtils.TransformRect(source, new Rect(new Point(), childSize), false);

            RectangleGeometry rectangleGeometry = new RectangleGeometry();
            rectangleGeometry.Rect = source;
            rectangleGeometry.Transform = compositeTransform1;
            imageFit.Clip = rectangleGeometry;
            
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _overlay.Children.Clear();
        }
    }
}
