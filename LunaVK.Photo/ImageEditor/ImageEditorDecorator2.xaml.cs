using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

namespace LunaVK.Photo.ImageEditor
{
    public sealed partial class ImageEditorDecorator2 : Page
    {
        private bool _inCropMode;
        private bool _filtersPanelShown;
        private bool _isInSetResetCrop;
        private bool _inSelectOwnPhotoArea;

        private ImageEditorViewModel _imageEditorVM;

        public ImageEditorDecorator2()
        {
            this.InitializeComponent();
        }

        private void SelectUnselectTap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void FilterTapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void SetCrop(object sender, TappedRoutedEventArgs e)
        {

        }

        private void ResetCrop(object sender, TappedRoutedEventArgs e)
        {

        }

        private void SendPhotoTap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void TextEffectTap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void CropEffectTap(object sender, TappedRoutedEventArgs e)
        {
            /*
            var random = new Random();
            var pixels = new byte[256 * 256 * 4];
            random.NextBytes(pixels);
            BitmapSource bitmapSource = BitmapSource.Create(256, 256, 96, 96, PixelFormats.Pbgra32, null, pixels, 256 * 4);
            var visual = new DrawingVisual();
            using (DrawingContext drawingContext = visual.RenderOpen())
            {
                drawingContext.DrawImage(bitmapSource, new Rect(0, 0, 256, 256));
                drawingContext.DrawText(
                    new FormattedText("Hi!", CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                        new Typeface("Segoe UI"), 32, Brushes.Black), new Point(0, 0));
            }
            var image = new DrawingImage(visual.Drawing);
            Image1.Source = image;
            */
            this.ToggleCropMode();
        }

        private void FilterEffectTap(object sender, TappedRoutedEventArgs e)
        {
            /*
            // Initialize the WriteableBitmap with size 512x512 and set it as source of an Image control
            WriteableBitmap writeableBmp = BitmapFactory.New(512, 512);
            ImageControl.Source = writeableBmp;
            using (writeableBmp.GetBitmapContext())
            {

                // Load an image from the calling Assembly's resources via the relative path
                writeableBmp = BitmapFactory.New(1, 1).FromResource("Data/flower2.png");

                // Clear the WriteableBitmap with white color
                writeableBmp.Clear(Colors.White);

                // Set the pixel at P(10, 13) to black
                writeableBmp.SetPixel(10, 13, Colors.Black);

                // Get the color of the pixel at P(30, 43)
                Color color = writeableBmp.GetPixel(30, 43);

                // Green line from P1(1, 2) to P2(30, 40)
                writeableBmp.DrawLine(1, 2, 30, 40, Colors.Green);

                // Line from P1(1, 2) to P2(30, 40) using the fastest draw line method 
                int[] pixels = writeableBmp.Pixels;
                int w = writeableBmp.PixelWidth;
                int h = writeableBmp.PixelHeight;
                WriteableBitmapExtensions.DrawLine(pixels, w, h, 1, 2, 30, 40, myIntColor);

                // Blue anti-aliased line from P1(10, 20) to P2(50, 70) with a stroke of 5
                writeableBmp.DrawLineAa(10, 20, 50, 70, Colors.Blue, 5);

                // Black triangle with the points P1(10, 5), P2(20, 40) and P3(30, 10)
                writeableBmp.DrawTriangle(10, 5, 20, 40, 30, 10, Colors.Black);

                // Red rectangle from the point P1(2, 4) that is 10px wide and 6px high
                writeableBmp.DrawRectangle(2, 4, 12, 10, Colors.Red);

                // Filled blue ellipse with the center point P1(2, 2) that is 8px wide and 5px high
                writeableBmp.FillEllipseCentered(2, 2, 8, 5, Colors.Blue);

                // Closed green polyline with P1(10, 5), P2(20, 40), P3(30, 30) and P4(7, 8)
                int[] p = new int[] { 10, 5, 20, 40, 30, 30, 7, 8, 10, 5 };
                writeableBmp.DrawPolyline(p, Colors.Green);

                // Cubic Beziér curve from P1(5, 5) to P4(20, 7) 
                // with the control points P2(10, 15) and P3(15, 0)
                writeableBmp.DrawBezier(5, 5, 10, 15, 15, 0, 20, 7, Colors.Purple);

                // Cardinal spline with a tension of 0.5 
                // through the points P1(10, 5), P2(20, 40) and P3(30, 30)
                int[] pts = new int[] { 10, 5, 20, 40, 30, 30 };
                writeableBmp.DrawCurve(pts, 0.5, Colors.Yellow);

                // A filled Cardinal spline with a tension of 0.5 
                // through the points P1(10, 5), P2(20, 40) and P3(30, 30) 
                writeableBmp.FillCurveClosed(pts, 0.5, Colors.Green);

                // Blit a bitmap using the additive blend mode at P1(10, 10)
                writeableBmp.Blit(new Point(10, 10), bitmap, sourceRect, Colors.White, WriteableBitmapExtensions.BlendMode.Additive);

                // Override all pixels with a function that changes the color based on the coordinate
                writeableBmp.ForEach((x, y, color) => Color.FromArgb(color.A, (byte)(color.R / 2), (byte)(x * y), 100));

            } // Invalidate and present in the Dispose call

            // Take snapshot
            var clone = writeableBmp.Clone();

            // Save to a TGA image stream (file for example)
            writeableBmp.WriteTga(stream);

            // Crops the WriteableBitmap to a region starting at P1(5, 8) and 10px wide and 10px high
            var cropped = writeableBmp.Crop(5, 8, 10, 10);

            // Rotates a copy of the WriteableBitmap 90 degress clockwise and returns the new copy
            var rotated = writeableBmp.Rotate(90);

            // Flips a copy of the WriteableBitmap around the horizontal axis and returns the new copy
            var flipped = writeableBmp.Flip(FlipMode.Horizontal);

            // Resizes the WriteableBitmap to 200px wide and 300px high using bilinear interpolation
            var resized = writeableBmp.Resize(200, 300, WriteableBitmapExtensions.Interpolation.Bilinear);
            */
        }

        private void ToggleCropMode()
        {
            if (this._isInSetResetCrop)
                return;
            if (this._inCropMode)
            {
                this._inCropMode = false;
                this._inSelectOwnPhotoArea = false;
                ((UIElement)this.gridChooseThumbnail).Visibility = Visibility.Collapsed;
                ((UIElement)this.gridCropLines).Visibility = Visibility.Collapsed;
                ((UIElement)this.gridCrop).Visibility = Visibility.Collapsed;
                this.imageViewer.Mode = ImageViewerMode.Normal;
                ((UIElement)this.stackPanelEffects).Visibility = Visibility.Visible;
                ((UIElement)this.stackPanelCrop).Visibility = Visibility.Collapsed;
                this.UpdateImageAndEllipseSelectOpacity(1);
            }
            else
            {
                this._inCropMode = true;
                Picture galleryImage = this._imageEditorVM.GetGalleryImage(this._albumId, this.CurrentPhotoSeqNo);
                bool rotated90 = false;
                Size correctImageSize = this._imageEditorVM.GetCorrectImageSize(galleryImage, this._albumId, this.CurrentPhotoSeqNo, out rotated90);
                BitmapImage bitmapImage = new BitmapImage();
                Point point = new Point();
                Size viewportSize = this._imageEditorVM.ViewportSize;
                // ISSUE: explicit reference operation
                double num1 = ((Size)@viewportSize).Width * 2.0;
                viewportSize = this._imageEditorVM.ViewportSize;
                // ISSUE: explicit reference operation
                double num2 = ((Size)@viewportSize).Height * 2.0;
                Size size = new Size(num1, num2);
                Rect fit1 = RectangleUtils.ResizeToFit(new Rect(point, size), correctImageSize);
                // ISSUE: explicit reference operation
                if (((Rect)@fit1).Height < (double)galleryImage.Height)
                {
                    // ISSUE: explicit reference operation
                    // ISSUE: explicit reference operation
                    bitmapImage.DecodePixelHeight = (rotated90 ? (int)((Rect)@fit1).Height : (int)((Rect)@fit1).Width);
                }
                ((BitmapSource)bitmapImage).SetSource(galleryImage.GetImage());
                this.imageViewer.CurrentImage.Source = ((ImageSource)this._imageEditorVM.RotateIfNeeded(this._albumId, this.CurrentPhotoSeqNo, new WriteableBitmap((BitmapSource)bitmapImage)));
                this.imageViewer.RectangleFill = ScaleFactor.GetScaleFactor() != 150 ? new Rect(12.0, 136.0, 456.0, 456.0) : new Rect(12.0, 163.0, 456.0, 456.0);
                this.imageViewer.Mode = ImageViewerMode.RectangleFill;
                ImageEffectsInfo imageEffectsInfo = this._imageEditorVM.GetImageEffectsInfo(this._albumId, this.CurrentPhotoSeqNo);
                if (imageEffectsInfo.CropRect != null)
                {
                    Rect rect1 = new Rect();
                    // ISSUE: explicit reference operation
                    rect1.X = ((double)imageEffectsInfo.CropRect.X);
                    // ISSUE: explicit reference operation
                    rect1.Y = ((double)imageEffectsInfo.CropRect.Y);
                    // ISSUE: explicit reference operation
                    rect1.Width = ((double)imageEffectsInfo.CropRect.Width);
                    // ISSUE: explicit reference operation
                    rect1.Height = ((double)imageEffectsInfo.CropRect.Height);
                    Rect rect2 = rect1;
                    Rect fit2 = RectangleUtils.ResizeToFit(new Rect(new Point(), new Size(((FrameworkElement)this.imageViewer).Width, ((FrameworkElement)this.imageViewer).Height)), correctImageSize);
                    ((UIElement)this.imageViewer.CurrentImage).RenderTransform = ((Transform)RectangleUtils.TransformRect(((GeneralTransform)RectangleUtils.TransformRect(new Rect(new Point(), correctImageSize), fit2, false)).TransformBounds(rect2), this.imageViewer.RectangleFill, false));
                }
                else
                    this.imageViewer.AnimateToRectangleFill();
                galleryImage.Dispose();
                this.ShowHideGridFilters(false);
                ((UIElement)this.gridCropLines).Visibility = Visibility.Visible;
                ((UIElement)this.gridCrop).Visibility = Visibility.Visible;
                ((UIElement)this.stackPanelEffects).Visibility = Visibility.Collapsed;
                ((UIElement)this.stackPanelCrop).Visibility = Visibility.Visible;
                this.UpdateImageAndEllipseSelectOpacity(0);
            }
        }

        private void UpdateImageAndEllipseSelectOpacity(int op)
        {
//            if (this._pickerPage.VM.OwnPhotoPick)
//                return;
            ((DependencyObject)this.elliplseSelect).Animate(((UIElement)this.elliplseSelect).Opacity, (double)op, UIElement.OpacityProperty, 150, new int?(0), null, null);
            ((DependencyObject)this.imageSelect).Animate(((UIElement)this.imageSelect).Opacity, (double)op, UIElement.OpacityProperty, 150, new int?(0), null, null);
        }

        private void ShowHideGridFilters(bool show)
        {
            TranslateTransform renderTransform = ((UIElement)this.gridFilters).RenderTransform as TranslateTransform;
            int num = show ? 0 : 221;
            this._filtersPanelShown = show;
            ((DependencyObject)renderTransform).Animate(renderTransform.Y, (double)num, TranslateTransform.YProperty, 250, new int?(0), this._easing, null, false);
        }
    }
}
