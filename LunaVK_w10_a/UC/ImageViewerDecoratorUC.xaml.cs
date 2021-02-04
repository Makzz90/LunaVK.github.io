using System;
using System.Collections.Generic;
using System.IO;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media.Animation;
using LunaVK.Core.DataObjects;
using LunaVK.Framework;
using Windows.UI.ViewManagement;
using Windows.Storage.Pickers;
using LunaVK.Core;
using LunaVK.Core.Network;
using LunaVK.Core.Enums;
using Windows.Storage;
using LunaVK.ViewModels;
using LunaVK.Common;
using Windows.UI.Xaml.Media.Imaging;
using System.ComponentModel;
using LunaVK.Core.Library;
using LunaVK.Library;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Diagnostics;
using LunaVK.Core.Framework;

namespace LunaVK.UC
{
    public sealed partial class ImageViewerDecoratorUC : UserControl, INotifyPropertyChanged
    {
        private readonly int DURATION_BOUNCING = 170;
        private readonly double MIN_SCALE = 0.5;
        private readonly double MAX_SCALE = 4.0;
        private readonly EasingFunctionBase ANIMATION_EASING;
        private List<Image> _images;
        private bool _isInPinch;
        private double HIDE_AFTER_VERT_SWIPE_THRESHOLD = 100.0;
        private readonly int ANIMATION_DURATION_MS = 250;
        private double HIDE_AFTER_VERT_SWIPE_VELOCITY_THRESHOLD = 100.0;
        private uint _count;
        private Func<int, ImageInfo> _getImageInfoFunc;
        //private Func<int, Image> _getImageFunc;
        private Func<int, Border> _getImageByIdFunc;
        private int _currentInd = 0;
        private readonly double MOVE_TO_NEXT_VELOCITY_THRESHOLD = 100.0;
        private double MARGIN_BETWEEN_IMAGES = 12.0;
        //private List<VKPhoto> _photos;
        private int DURATION_MOVE_TO_NEXT = 200;
        private ImageAnimator _imageAnimator;

        /// <summary>
        /// 300
        /// </summary>
        public readonly int ANIMATION_INOUT_DURATION_MS = 300;
        public readonly EasingFunctionBase ANIMATION_EASING_IN_OUT;
        private readonly DispatcherTimer _timer;
        //        private bool _initialStatusBarVisibility;
        //        public Action HideCallback;
        public event PropertyChangedEventHandler PropertyChanged;

        public Action CurrentIndexChanged { get; set; }

        private PopUpService _flyout;

        /// <summary>
        /// Мы на стадии закрытия?
        /// </summary>
        private bool IsInVerticalSwipe = false;

        public ImageViewerDecoratorUC()
        {
            this.InitializeComponent();
            this._images = new List<Image>();

            this._images.Add(new Image());
            this._images.Add(new Image());
            this._images.Add(new Image());

            this.imageViewer.Children.Add(this._images[0]);
            this.imageViewer.Children.Add(this._images[1]);
            this.imageViewer.Children.Add(this._images[2]);

            this._timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(150.0) };
            this._timer.Tick += _timer_Tick;

            base.Loaded += ImageViewerDecoratorUC_Loaded;
            base.Unloaded += ImageViewerDecoratorUC_Unloaded;

            var ease = new SineEase();
            //CubicEase cubicEase = new CubicEase() { EasingMode = EasingMode.EaseOut };
            //var ease = new CircleEase() { EasingMode = EasingMode.EaseOut };
            this.ANIMATION_EASING = ease;

            QuadraticEase quadraticEase = new QuadraticEase() { EasingMode = EasingMode.EaseIn };
            //ElasticEase quadraticEase = new ElasticEase() { Oscillations = 1, Springiness = 10 };
            this.ANIMATION_EASING_IN_OUT = quadraticEase;

            this._imageAnimator = new ImageAnimator(this.ANIMATION_INOUT_DURATION_MS, this.ANIMATION_EASING_IN_OUT);
            this._blackRectangle.Opacity = 0;

            (this.gridTop.RenderTransform as CompositeTransform).TranslateY = -this.gridTop.Height;
            (this.gridBottom.RenderTransform as CompositeTransform).TranslateY = this.gridBottom.Height;

//            base.SizeChanged += ImageViewerDecoratorUC_SizeChanged;

            this.CurrentIndexChanged = this.RespondToCurrentIndexChanged;
            base.DataContext = this;

            if (!CustomFrame.Instance.IsDevicePhone)
                this._closeBtn.Visibility = Visibility.Visible;
        }

        private void ImageViewerDecoratorUC_Unloaded(object sender, RoutedEventArgs e)
        {
            ImageViewerLowProfileImageLoader.ImageDownloaded -= this.ImageViewerLowProfileImageLoader_ImageDownloaded;
            ImageViewerLowProfileImageLoader.ImageDownloadingProgress -= this.ImageViewerLowProfileImageLoader_ImageDownloadingProgress;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= this.OnBackRequested;
            Window.Current.CoreWindow.KeyDown -= this.CoreWindow_KeyDown;
        }

        private void ImageViewerLowProfileImageLoader_ImageDownloadingProgress(object sender, double e)
        {
            if (this.CurrentImage != sender)
                return;
            Core.Framework.Execute.ExecuteOnUIThread(() =>
            {
                this._progressRing.Value = e;
            });
        }

        private void ImageViewerLowProfileImageLoader_ImageDownloaded(object sender, EventArgs e)
        {
            Core.Framework.Execute.ExecuteOnUIThread(() =>
            {
                this.UpdateProgressBarVisibility();
            });
        }

        private void UpdateProgressBarVisibility()
        {
            this._progressRing.Value = this.CurrentImage.Source != null ? 0.2 : 0;
            //this._progressRing.Visibility = this._loadingTextBlock.Visibility = (this.CurrentImage.Source != null ? Visibility.Collapsed : Visibility.Visible);
        }

        void _timer_Tick(object sender, object e)
        {
            this._timer.Stop();
            //this._timer.Tick -= (new EventHandler(this._timer_Tick));
            //this._userHideDecorator = !this._userHideDecorator;
            this.ShowInformation();
            this.Update();
        }

        /// <summary>
        /// Запйскаем таймер
        /// </summary>
        private void RespondToTap()
        {
            this._timer.Start();
        }

        /// <summary>
        /// Останавливаем таймер
        /// </summary>
        private void RespondToDoubleTap()
        {
            this._timer.Stop();
        }

        void ImageViewerDecoratorUC_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ArrangeImages();
//            this.UpdateImagesSources();

            //BugFix
            RectangleGeometry rectangleGeometry = new RectangleGeometry();
            rectangleGeometry.Rect = new Rect(0.0, 0.0, e.NewSize.Width, e.NewSize.Height);
            this.CurrentImage.Clip = rectangleGeometry;

        }

#if WINDOWS_PHONE_APP
        void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            e.Handled = true;
            this.Hide();
        }
#elif WINDOWS_UWP
        private void OnBackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            e.Handled = true;
            this.Hide();
        }
#endif


        /// <summary>
        /// _images[1]
        /// </summary>
        public Image CurrentImage
        {
            get { return this._images[1]; }
        }

        private CompositeTransform CurrentImageTransform
        {
            get { return this.CurrentImage.RenderTransform as CompositeTransform; }
        }

        private double CurrentImageScale
        {
            get { return this.CurrentImageTransform.ScaleX; }
        }

        void ImageViewerDecoratorUC_Loaded(object sender, RoutedEventArgs e)
        {
            ArrangeImages();
            //

            var originalImage = this.OriginalImage;
            if (originalImage != null)
            {
                if(originalImage.Background is ImageBrush imgBrush)
                {
                    this.CurrentImage.Source = imgBrush.ImageSource;
                }
                else
                {
                    if (originalImage.Child is Image img)
                        this.CurrentImage.Source = img.Source;
                }
                //this.CurrentImage.Source = originalImage.Source;
            }
            this._imageAnimator.AnimateIn(/*this.GetImageSizeSafelyBy(this._currentInd),*/ this.OriginalImage, this.CurrentImage, (() =>
            {
                this.IsHitTestVisible = true;
                this.UpdateImagesSources(false, new bool?());

            }));
            this._blackRectangle.Animate(this._blackRectangle.Opacity, 1.0, "Opacity", this.ANIMATION_INOUT_DURATION_MS, 150, this.ANIMATION_EASING);

            Window.Current.CoreWindow.KeyDown += this.CoreWindow_KeyDown;
        }

        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            if(args.VirtualKey == Windows.System.VirtualKey.Right)
            {
                this.CurrentImageTransform.ScaleX = this.CurrentImageTransform.ScaleY = 1;
                if (this.ChevronRight.Visibility == Visibility.Visible)
                    this.ChevronRight_Tapped(null, null);
                
            }
            else if (args.VirtualKey == Windows.System.VirtualKey.Left)
            {
                this.CurrentImageTransform.ScaleX = this.CurrentImageTransform.ScaleY = 1;
                if (this.ChevronLeft.Visibility == Visibility.Visible)
                    this.ChevronLeft_Tapped(null, null);
            }
        }

        private void imageViewer_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            //System.Diagnostics.Debug.WriteLine(e.Delta.Scale);
            //this.textBlockCounter.Text = String.Format("{0}\n{1}\n{2}", e.Delta.Scale, e.Position.X, e.Position.Y);
            //double scaled_width = this.CurrentImage.ActualWidth * this.CurrentImageTransform.ScaleX;
            //double scaled_height = this.CurrentImage.ActualHeight * this.CurrentImageTransform.ScaleY;

            e.Handled = true;
            if (e.Delta.Scale == 1.0)
            {

                this.HandleDragDelta(e.Delta.Translation.X, e.Delta.Translation.Y);
            }
            else
            {
                //if (this.CurrentImageTransform.ScaleX == 1.0 && (this.CurrentImageTransform.TranslateX != 0.0 || this.CurrentImageTransform.TranslateY != 0.0))
                //    return;

                if (!this._isInPinch)
                {
                    this._isInPinch = true;
                }

                this.HandlePinch(e.Position, e.Delta.Scale);
            }
        }

        private double Clamp(double val, double min, double max)
        {
            if (val <= min)
                return min;
            if (val >= max)
                return max;
            return val;
        }

        private double TranslateInterval(double x, double a, double b, double toA, double toB)
        {
            return (toB * (x - a) + toA * (b - x)) / (b - a);
        }

        private void HandlePinch(Point pos, float scale)
        {
            //if (this._mode == ImageViewerMode.Normal && this.ForbidResizeInNormalMode)
            //    return;
            //Point point1 = pointFirst;
            //Point point2 = pointSecond;
            //double x1 = this._first.X;
            //double y1 = this._first.Y;
            //double x2 = this._second.X;
            //double y2 = this._second.Y;
            //double x3 = point1.X;
            //double y3 = point1.Y;
            //double x4 = point2.X;
            //double y4 = point2.Y;
            //double x5 = this.Clamp(Math.Sqrt(((x4 - x3) * (x4 - x3) + (y4 - y3) * (y4 - y3)) / ((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1))) * this._previousTransformation.ScaleX, VKClient.Common.ImageViewer.ImageViewer.MIN_SCALE / 2.0, this.MaxScale);
            //if (x5 < 1.0)
            //    x5 = this.TranslateInterval(x5, this.MIN_SCALE / 2.0, 1.0, this.MIN_SCALE, 1.0);
            //double num1 = x5 / this._previousTransformation.ScaleX;
            //double num2 = (x3 - num1 * x1 + (x4 - num1 * x2)) / 2.0;
            //double num3 = (y3 - num1 * y1 + (y4 - num1 * y2)) / 2.0;
            //CompositeTransform compositeTransform = this.CurrentImage.RenderTransform as CompositeTransform;
            //double num4 = num1 * this._previousTransformation.ScaleX;
            //compositeTransform.ScaleX = num4;
            //double num5 = num1 * this._previousTransformation.ScaleY;
            //compositeTransform.ScaleY = num5;
            //double num6 = num1 * this._previousTransformation.TranslateX + num2;
            //compositeTransform.TranslateX = num6;
            //double num7 = num1 * this._previousTransformation.TranslateY + num3;
            //compositeTransform.TranslateY = num7;




            CompositeTransform CompositeTransform_ = this.CurrentImage.RenderTransform as CompositeTransform;
            CompositeTransform_.ScaleX *= scale;
            CompositeTransform_.ScaleY *= scale;
            if (CompositeTransform_.ScaleX < this.MIN_SCALE)
            {
                CompositeTransform_.ScaleX = CompositeTransform_.ScaleY = this.MIN_SCALE;
                return;
            }
            else if (CompositeTransform_.ScaleX > this.MAX_SCALE)
            {
                CompositeTransform_.ScaleX = CompositeTransform_.ScaleY = this.MAX_SCALE;
                return;
            }

            double originX = pos.X;
            double originY = pos.Y;
            double dx = originX - originX * scale;
            double dy = originY - originY * scale;
            double new_tr_x = dx * CompositeTransform_.ScaleX;
            double new_tr_y = dy * CompositeTransform_.ScaleX;

            CompositeTransform_.TranslateX += new_tr_x;
            CompositeTransform_.TranslateY += new_tr_y;
        }

        private void HandleDragDelta(double hDelta, double vDelta)
        {
            double num1 = hDelta;
            double num2 = vDelta;
            if (this.CurrentImageScale == 1.0)
            {/*mos
                CompositeTransform renderTransform1 = ((UIElement)this.CurrentImage).RenderTransform as CompositeTransform;
                if (renderTransform1.TranslateX == 0.0 && this.AllowVerticalSwipe && (this.IsInVerticalSwipe || num1 == 0.0 && num2 != 0.0 || Math.Abs(num2) / Math.Abs(num1) > 1.2))
                {
                    this.IsInVerticalSwipe = true;
                    CompositeTransform compositeTransform = renderTransform1;
                    double num3 = compositeTransform.TranslateY + num2;
                    compositeTransform.TranslateY = num3;
                    ((UIElement)this._loadingTextBlock).Opacity = 0.0;
                    ((UIElement)this._blackRectangle).Opacity = (Math.Max(0.0, 1.0 - Math.Abs(renderTransform1.TranslateY) / 400.0));
                }
                else
                {
                    if (this._currentInd == 0 && num1 > 0.0 && renderTransform1.TranslateX > 0.0 || this._currentInd == this._count - 1 && num1 < 0.0 && renderTransform1.TranslateX < 0.0)
                        num1 /= 3.0;
                    using (List<Image>.Enumerator enumerator = this._images.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            CompositeTransform renderTransform2 = ((UIElement)enumerator.Current).RenderTransform as CompositeTransform;
                            double num3 = renderTransform2.TranslateX + num1;
                            renderTransform2.TranslateX = num3;
                        }
                    }
                }*/


                CompositeTransform compositeTransform = this.CurrentImage.RenderTransform as CompositeTransform;
                if (compositeTransform.TranslateX == 0.0 && (this.IsInVerticalSwipe || num1 == 0.0 && num2 != 0.0 || Math.Abs(num2) / Math.Abs(num1) > 1.2))
                {
                    this.IsInVerticalSwipe = true;
                    compositeTransform.TranslateY += num2;
                }
                else
                {
                    if (this._currentInd == 0 && num1 > 0.0 && compositeTransform.TranslateX > 0.0 || this._currentInd == this._count - 1 && num1 < 0.0 && compositeTransform.TranslateX < 0.0)
                        num1 /= 3.0;
                    foreach (UIElement image in this._images)
                        (image.RenderTransform as CompositeTransform).TranslateX += num1;
                }
            }
            else
            {
                CompositeTransform currentImageTransform = this.CurrentImageTransform;
                currentImageTransform.TranslateX = currentImageTransform.TranslateX + num1;
                currentImageTransform.TranslateY = currentImageTransform.TranslateY + num2;
            }
        }

        private void imageViewer_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;
            if (this._isInPinch)
            {
                this.HandlePinchCompleted();
                this._isInPinch = false;
            }
            else
            {
                double velX = e.Velocities.Linear.X;
                double velY = e.Velocities.Linear.Y;
                this.HandleDragCompleted(e.Cumulative.Translation.X * Math.Abs( velX), e.Cumulative.Translation.Y * Math.Abs(velY));
            }
        }
        /*
        public Size GetImageSizeSafelyBy(int ind)
        {
            if (ind >= 0 && ind < this._count)
            {
                Image imageInfo = this._getImageFunc(ind);
                if (imageInfo != null && imageInfo.ActualWidth > 0.0 && imageInfo.ActualHeight > 0.0)
                    return new Size(imageInfo.ActualWidth, imageInfo.ActualHeight);
            }
            return new Size(100.0, 100.0);
        }
        */
        public Size GetImageSizeSafelyBy(int ind)
        {
            if (ind >= 0 && ind < this._count)
            {
                ImageInfo imageInfo = this._getImageInfoFunc(ind);
                if (imageInfo != null && imageInfo.Width > 0.0 && imageInfo.Height > 0.0)
                    return new Size(imageInfo.Width, imageInfo.Height);
            }
            return new Size(100.0, 100.0);
        }

        /// <summary>
        /// Вписываем текущую картинку в окно.
        /// </summary>
        public Rect CurrentImageFitRectOriginal
        {
            get
            {
                return RectangleUtils.ResizeToFit(new Rect(new Point(), new Size(this.ActualWidth, this.ActualHeight)), this.GetImageSizeSafelyBy(this._currentInd));
            }
        }

        private void HandleDragCompleted(double hVelocity, double vVelocity)
        {
            double num1 = hVelocity;
            double num2 = vVelocity;
            if (this.CurrentImageScale == 1.0)
            {
                if (this.IsInVerticalSwipe)
                {
                    CompositeTransform currentImageTransform = this.CurrentImageTransform;
                    if (Math.Abs(currentImageTransform.TranslateY) < this.HIDE_AFTER_VERT_SWIPE_THRESHOLD && num2 < this.HIDE_AFTER_VERT_SWIPE_VELOCITY_THRESHOLD)
                        currentImageTransform.Animate(currentImageTransform.TranslateY, 0.0, "TranslateY", this.ANIMATION_DURATION_MS, 0, this.ANIMATION_EASING);
                    else
                        this.Hide(null, false);
                    this.IsInVerticalSwipe = false;
                }
                else
                {
                    bool? moveNext = new bool?();
                    double translateX = (this._images[1].RenderTransform as CompositeTransform).TranslateX;
                    
                    if ((num1 < -this.MOVE_TO_NEXT_VELOCITY_THRESHOLD && translateX < 0.0 || translateX <= -this.ActualWidth / 2.0) && this._currentInd < this._count - 1)
                        moveNext = new bool?(true);
                    else if ((num1 > this.MOVE_TO_NEXT_VELOCITY_THRESHOLD && translateX > 0.0 || translateX >= this.ActualWidth / 2.0) && this._currentInd > 0)
                        moveNext = new bool?(false);
                    double num4 = 0.0;
                    
                    bool flag1 = true;
                    if ((moveNext.GetValueOrDefault() == flag1 ? (moveNext.HasValue ? 1 : 0) : 0) != 0)
                    {
                        num4 = -this.ActualWidth - this.MARGIN_BETWEEN_IMAGES;
                    }
                    else
                    {
                        bool? nullable2 = moveNext;
                        bool flag2 = false;
                        if ((nullable2.GetValueOrDefault() == flag2 ? (nullable2.HasValue ? 1 : 0) : 0) != 0)
                            num4 = this.ActualWidth + this.MARGIN_BETWEEN_IMAGES;
                    }
                    double delta = num4 - translateX;
                    if (moveNext.HasValue && moveNext.Value)
                    {
                        this.AnimateTwoImagesOnDragComplete(this._images[1], this._images[2], delta, (() =>
                        {
                            this.MoveToNextOrPrevious(moveNext.Value);
                            this.ArrangeImages();
                        }), moveNext.HasValue);

                        this.ChangeCurrentInd(moveNext.Value);
                    }
                    else if (moveNext.HasValue && !moveNext.Value)
                    {
                        this.AnimateTwoImagesOnDragComplete(this._images[0], this._images[1], delta, (() =>
                        {
                            this.MoveToNextOrPrevious(moveNext.Value);
                            this.ArrangeImages();
                        }), moveNext.HasValue);

                        this.ChangeCurrentInd(moveNext.Value);
                    }
                    else
                    {
                        if (delta == 0.0)
                            return;
                        this.AnimateImageOnDragComplete(this._images[0], delta, null, moveNext.HasValue);
                        this.AnimateImageOnDragComplete(this._images[1], delta, null, moveNext.HasValue);
                        this.AnimateImageOnDragComplete(this._images[2], delta, this.ArrangeImages, moveNext.HasValue);
                    }
                }
            }
            else
            {
                this.EnsureBoundaries();//AnimateToEnsureBoundaries
            }
        }

        private void HandlePinchCompleted()
        {
            CompositeTransform renderTransform = ((UIElement)this.CurrentImage).RenderTransform as CompositeTransform;
            if (renderTransform.ScaleX < 1.0)
                this.AnimateImage(1.0, 1.0, 0.0, 0.0, null);
            else
                this.EnsureBoundaries();//AnimateToEnsureBoundaries

        }

        private void MoveToNextOrPrevious(bool next)
        {
            if (next)
            {
                this.Swap(this._images, 0, 1);
                this.Swap(this._images, 1, 2);
            }
            else
            {
                this.Swap(this._images, 1, 2);
                this.Swap(this._images, 0, 1);
            }
            this.UpdateImagesSources(false, new bool?(next));
        }

        private void Swap(List<Image> images, int ind1, int ind2)
        {
            Image image = images[ind1];
            images[ind1] = images[ind2];
            images[ind2] = image;
        }

        public Rect CurrentImageFitRectTransformed
        {
            get { return this.CurrentImageTransform.TransformBounds(this.CurrentImageFitRectOriginal); }
        }

        private void EnsureBoundaries()
        {
            Rect fitRectTransformed = this.CurrentImageFitRectTransformed;
            Rect target = RectangleUtils.AlignRects(new Rect(new Point(), new Size(this.ActualWidth, this.ActualHeight)), fitRectTransformed, false);
            if (target == fitRectTransformed)
                return;
            CompositeTransform compositeTransform = RectangleUtils.TransformRect(this.CurrentImageFitRectOriginal, target, false);
            this.AnimateImage(compositeTransform.ScaleX, compositeTransform.ScaleY, compositeTransform.TranslateX, compositeTransform.TranslateY, null);
        }

        private void imageViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            this.RespondToDoubleTap();

            e.Handled = true;
            if (this.CurrentImageScale == 1.0)
            {
                Point position = e.GetPosition(this.CurrentImage);
                Rect imageFitRectOriginal = this.CurrentImageFitRectOriginal;
                CompositeTransform compositeTransform1 = new CompositeTransform();

                compositeTransform1.ScaleX = compositeTransform1.ScaleY = 2.0;

                compositeTransform1.TranslateX = -position.X;
                compositeTransform1.TranslateY = -position.Y;

                Rect target = RectangleUtils.AlignRects(new Rect(new Point(), new Size(this.ActualWidth, this.ActualHeight)), compositeTransform1.TransformBounds(imageFitRectOriginal), false);
                CompositeTransform compositeTransform2 = RectangleUtils.TransformRect(imageFitRectOriginal, target, false);
                this.AnimateImage(compositeTransform2.ScaleX, compositeTransform2.ScaleY, compositeTransform2.TranslateX, compositeTransform2.TranslateY);
            }
            else
            {
                this.AnimateImage(1.0, 1.0, 0.0, 0.0, null);
            }
        }

        private void imageViewer_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;

            if (this._inAnimating)
                return;

            var temp = e.GetCurrentPoint(null);
            bool zoomIn = temp.Properties.MouseWheelDelta > 0;


            if (e.KeyModifiers == Windows.System.VirtualKeyModifiers.Control)
            {
                this.AnimateImage(1.0, 1.0, 0.0, 0.0, null);

                /*


                this.HandleDragCompleted(zoomIn ? 200 : -200, 0);

                this.ChangeCurrentInd(zoomIn);
                */

                return;
            }




            //todo: зум через таймер
            Point position = temp.Position;
            Rect imageFitRectOriginal = this.CurrentImageFitRectOriginal;
            CompositeTransform compositeTransform1 = new CompositeTransform();

            double addZoom = zoomIn ? 0.15 : -0.15;
            if (this.CurrentImageScale < MIN_SCALE)
                this.CurrentImageTransform.ScaleX = this.CurrentImageTransform.TranslateY = MIN_SCALE;
            compositeTransform1.ScaleX = compositeTransform1.ScaleY = (this.CurrentImageScale + addZoom);

            compositeTransform1.TranslateX = -position.X;
            compositeTransform1.TranslateY = -position.Y;

            Rect target = RectangleUtils.AlignRects(new Rect(new Point(), new Size(this.ActualWidth, this.ActualHeight)), compositeTransform1.TransformBounds(imageFitRectOriginal), false);
            CompositeTransform compositeTransform2 = RectangleUtils.TransformRect(imageFitRectOriginal, target, false);
            this.AnimateImage(compositeTransform2.ScaleX, compositeTransform2.ScaleY, compositeTransform2.TranslateX, compositeTransform2.TranslateY);
        }

        public void AnimateImage(double toScaleX = 1.0, double toScaleY = 1.0, double toTranslateX = 0.0, double toTranslateY = 0.0, Action completionCallback = null)
        {
            CompositeTransform renderTransform = this.CurrentImage.RenderTransform as CompositeTransform;
            renderTransform.Animate(renderTransform.ScaleX, toScaleX, "ScaleX", this.DURATION_BOUNCING, 0, this.ANIMATION_EASING, null);
            renderTransform.Animate(renderTransform.ScaleY, toScaleY, "ScaleY", this.DURATION_BOUNCING, 0, this.ANIMATION_EASING, null);
            renderTransform.Animate(renderTransform.TranslateX, toTranslateX, "TranslateX", this.DURATION_BOUNCING, 0, this.ANIMATION_EASING, null);
            renderTransform.Animate(renderTransform.TranslateY, toTranslateY, "TranslateY", this.DURATION_BOUNCING, 0, this.ANIMATION_EASING, completionCallback);
        }

        private void SetImageSource(Image image, object source)
        {
            if (source == null)
            {
                image.Source = null;
            }
            else if (source is BitmapSource bs)
            {
                image.Source = bs;
            }
            else if(source is string str)
            {
                if (str.StartsWith("{"))
                    return;

                Uri uri = new Uri(str);
                ImageViewerLowProfileImageLoader.SetUriSource(image, uri);


                /*
                BitmapImage bitmapImage = new BitmapImage();
                string uri = source.ToString();
                UC.AudioRecorderUC.AudioAmplitudeStream stream = new UC.AudioRecorderUC.AudioAmplitudeStream();
                LunaVK.Network.JsonWebRequest.DownloadToStream(uri, stream, (s, res) => {
                    if (res == true)
                    {
                        LunaVK.Core.Framework.Execute.ExecuteOnUIThread(async () =>
                        {
                            await bitmapImage.SetSourceAsync(stream);
                            image.Source = bitmapImage;
                        });
                    }

                }, null);*/
            }
        }

        private void ArrangeImages()
        {
            this._images[0].RenderTransform = new CompositeTransform() { TranslateX = -base.ActualWidth };
            this._images[1].RenderTransform = new CompositeTransform();
            this._images[2].RenderTransform = new CompositeTransform() { TranslateX = base.ActualWidth };

            this._images[0].Width = base.ActualWidth;
            this._images[1].Width = base.ActualWidth;
            this._images[2].Width = base.ActualWidth;

            this._images[0].Height = base.ActualHeight;
            this._images[1].Height = base.ActualHeight;
            this._images[2].Height = base.ActualHeight;
        }
        /*
        private Image OriginalImage
        {
            get
            {
                if (this._getImageFunc == null)
                    return null;

                return this._getImageFunc(this._currentInd);
            }
        }
        */
        private Border OriginalImage
        {
            get
            {
                if (this._getImageByIdFunc == null)
                    return null;
                
                return this._getImageByIdFunc(this._currentInd);
            }
        }

        bool _inAnimating;

        public void Hide(Action callback = null, bool leavingPageImmediately = false)
        {
            ImageViewerLowProfileImageLoader.Cancel();

            this._inAnimating = true;
            CustomFrame.Instance.ForceHideStatus = false;
            //            CustomFrame.Instance.ShowStatusBar(this._initialStatusBarVisibility);
            //
            if (CustomFrame.Instance.CurrentOrientation == ApplicationViewOrientation.Portrait)
                CustomFrame.Instance.ShowStatusBar(true);
            //
            CustomFrame.Instance.OrientationChanged -= this.Instance_OrientationChanged;
            base.SizeChanged -= ImageViewerDecoratorUC_SizeChanged;

            this.ChevronLeft.Visibility = this.ChevronRight.Visibility = Visibility.Collapsed;
            this.imageViewer.IsHitTestVisible = false;

            bool? clockwiseRotation = new bool?();
            var originalImage = this.OriginalImage;

            if (originalImage == null)//bugfix: нул от виртуализации?
                leavingPageImmediately = true;

            if (!leavingPageImmediately)
            {
                originalImage.Opacity = 0.0;
                this._imageAnimator.AnimateOut(/*this.GetImageSizeSafelyBy(this._currentInd),*/ originalImage, this.CurrentImage, clockwiseRotation, (() =>
                {
                    originalImage.Opacity = 1.0;

                    if (this._flyout != null)
                        this._flyout.Hide();
                    //
                    //                    CustomFrame.Instance.HeaderWithMenu.IsVisible = true;
                    //
                }));
                this._blackRectangle.Animate(this._blackRectangle.Opacity, 0.0, "Opacity", this.ANIMATION_INOUT_DURATION_MS);

                this.textBoxText.Animate(this.textBoxText.Opacity, 0.0, "Opacity", this.ANIMATION_INOUT_DURATION_MS);

                CompositeTransform Transform1 = (this.gridTop.RenderTransform as CompositeTransform);
                Transform1.Animate(Transform1.TranslateY, -this.gridTop.Height, "TranslateY", this.ANIMATION_DURATION_MS, 0, this.ANIMATION_EASING);

                CompositeTransform Transform2 = (this.gridBottom.RenderTransform as CompositeTransform);
                Transform2.Animate(Transform2.TranslateY, this.gridBottom.Height, "TranslateY", this.ANIMATION_DURATION_MS, 0, this.ANIMATION_EASING);

            }
            else
            {
                callback?.Invoke();

                if (this._flyout != null)
                {
                    this._flyout.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
                    this._flyout.Hide();
                }
            }
#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
#elif WINDOWS_UWP
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
#endif
        }
        
        private void UpdateImagesSources(bool keepCurrentAsIs = false, bool? movedForvard = null)
        {
            //if (!this.ShowNextPrevious)
            //{
            //    if (!keepCurrentAsIs)
            //        this.SetImageSource(this._images[1], this.GetImageSource(this._currentInd, false));
            //    this.SetImageSource(this._images[0], null);
            //    this.SetImageSource(this._images[2], null);
            //}
            //else
            //{
            if (!keepCurrentAsIs && !movedForvard.HasValue)
                this.SetImageSource(this._images[1], this.GetImageSource(this._currentInd));
            bool num = !movedForvard.HasValue ? true : (!movedForvard.HasValue ? false : (movedForvard.Value ? true : false));
            if ((!movedForvard.HasValue ? true : (!movedForvard.HasValue ? false : (!movedForvard.Value ? true : false))) != false)
            {
                object imageSource = this.GetImageSource(this._currentInd - 1);
                this.SetImageSource(this._images[0], null);
                this.SetImageSource(this._images[0], imageSource);
            }
            if (num == false)
                return;
            object imageSource1 = this.GetImageSource(this._currentInd + 1);
            this.SetImageSource(this._images[2], null);
            this.SetImageSource(this._images[2], imageSource1);
            //}
        }

        private object GetImageSource(int ind, bool allowBackgroundCreation = true)
        {
            /*
            if (ind < 0 || ind >= this._photos.Count)
                return null;

            if (!string.IsNullOrEmpty(this._photos[ind].photo_2560))
                return this._photos[ind].photo_2560;
            if (!string.IsNullOrEmpty(this._photos[ind].photo_1280))
                return this._photos[ind].photo_1280;
            if (!string.IsNullOrEmpty(this._photos[ind].photo_807))
                return this._photos[ind].photo_807;
            return this._photos[ind].photo_604;*/
            if (ind < 0 || ind >= this._count)
                return null;
            ImageInfo imageInfo = this._getImageInfoFunc(ind);
            if (imageInfo != null)
            {
                if (!string.IsNullOrEmpty(imageInfo.Uri))
                    return imageInfo.Uri;
                if (imageInfo.GetSourceFunc != null)
                    return imageInfo.GetSourceFunc(allowBackgroundCreation);
            }
            return null;
        }

        private void ChangeCurrentInd(bool next)
        {
            this._currentInd = !next ? this._currentInd - 1 : this._currentInd + 1;
            this.Update();
            this.CurrentIndexChanged?.Invoke();
        }

        private void AnimateTwoImagesOnDragComplete(Image image1, Image image2, double delta, Action completedCallback, bool movingToNextOrPrevious)
        {
            int num = movingToNextOrPrevious ? this.DURATION_MOVE_TO_NEXT : this.DURATION_BOUNCING;
            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
            CompositeTransform compositeTransform1 = image1.RenderTransform as CompositeTransform;
            CompositeTransform compositeTransform2 = image2.RenderTransform as CompositeTransform;
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                from = compositeTransform1.TranslateX,
                to = compositeTransform1.TranslateX + delta,
                propertyPath = "TranslateX",
                duration = num,
                target = compositeTransform1,
                easing = this.ANIMATION_EASING
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                from = compositeTransform2.TranslateX,
                to = compositeTransform2.TranslateX + delta,
                propertyPath = "TranslateX",
                duration = num,
                target = compositeTransform2,
                easing = this.ANIMATION_EASING
            });
            //int? startTime = new int?(0);
            
            AnimationUtils.AnimateSeveral(animInfoList, null, completedCallback);
        }
        
        private void AnimateImageOnDragComplete(Image image, double delta, Action completedCallback, bool movingToNextOrPrevious)
        {
            int duration = movingToNextOrPrevious ? this.DURATION_MOVE_TO_NEXT : this.DURATION_BOUNCING;
            CompositeTransform target = image.RenderTransform as CompositeTransform;
            target.Animate(target.TranslateX, target.TranslateX + delta, "TranslateX", duration, 0, this.ANIMATION_EASING, completedCallback);
        }

        public void Show(int ind)
        {
            CustomFrame.Instance.ForceHideStatus = true;

            this.CurrentIndexChanged?.Invoke();

            this._flyout = new PopUpService();
            this._flyout.AnimationTypeChild = PopUpService.AnimationTypes.None;
            this._flyout.BackgroundBrush = null;
            this._flyout.Child = this;//должно быть так



            this._currentInd = ind;

            this.SetImageSource(this.CurrentImage, null);

            var thumbImage = this.OriginalImage;
            if(thumbImage!=null)
                thumbImage.Opacity = 0.0;


            this._flyout.Opened += ((sender, args) =>
            {
                if (thumbImage != null)
                    thumbImage.Opacity = 1.0;

                CompositeTransform Transform1 = (this.gridTop.RenderTransform as CompositeTransform);
                Transform1.Animate(Transform1.TranslateY, 0.0, "TranslateY", this.ANIMATION_DURATION_MS, 0, this.ANIMATION_EASING);

                CompositeTransform Transform2 = (this.gridBottom.RenderTransform as CompositeTransform);
                Transform2.Animate(Transform2.TranslateY, 0.0, "TranslateY", this.ANIMATION_DURATION_MS, 0, this.ANIMATION_EASING);
                //
                ImageViewerLowProfileImageLoader.ImageDownloaded += this.ImageViewerLowProfileImageLoader_ImageDownloaded;
                ImageViewerLowProfileImageLoader.ImageDownloadingProgress += ImageViewerLowProfileImageLoader_ImageDownloadingProgress;
            });

            this._flyout.Show();
            this.Update();
#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#elif WINDOWS_UWP
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
#endif
            CustomFrame.Instance.OrientationChanged += this.Instance_OrientationChanged;

        }

        

        private void Instance_OrientationChanged(object sender, ApplicationViewOrientation e)
        {
            this.ArrangeImages();
            this.AnimateImage();
        }

        private void Update()
        {
            if (this._count > 0)
            {
                this.textBlockCounter.Text = string.Format(LocalizedStrings.GetString("ImageViewer_PhotoCounterFrm"), this._currentInd + 1, this._count);
                this.textBlockCounter.Visibility = Visibility.Visible;
            }
            else
            {
                this.textBlockCounter.Text = "";
                this.textBlockCounter.Visibility = Visibility.Collapsed;
            }


            //            this.textBoxText.Text = this.CurrentPhotoVM.Photo.text;

            //           this.ShareCountStr.Visibility = this.CommentsCountStr.Visibility = this.LikesCountStr.Visibility = Visibility.Collapsed;
            
            //if (this._currentInd == 0)
            //    this.ChevronLeft.Opacity = 0;

            //if (this._currentInd + 1 >= this._count)
            //    this.ChevronRight.Opacity = 0;

            this.ChevronLeft.Visibility = (this._currentInd == 0 || CustomFrame.Instance.IsDevicePhone) ? Visibility.Collapsed : Visibility.Visible;
            this.ChevronRight.Visibility = (this._currentInd + 1 >= this._count || CustomFrame.Instance.IsDevicePhone) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ShowInformation()
        {
            CompositeTransform Transform1 = (this.gridTop.RenderTransform as CompositeTransform);
            CompositeTransform Transform2 = (this.gridBottom.RenderTransform as CompositeTransform);

            if (Transform1.TranslateY == 0.0)
            {
                Transform1.Animate(Transform1.TranslateY, -this.gridTop.Height, "TranslateY", this.ANIMATION_DURATION_MS, 0, this.ANIMATION_EASING);
                Transform2.Animate(Transform2.TranslateY, this.gridBottom.Height, "TranslateY", this.ANIMATION_DURATION_MS, 0, this.ANIMATION_EASING);
                this.textBoxText.Animate(0.8, 0.0, "Opacity", this.ANIMATION_INOUT_DURATION_MS, 0, null, null);
                //
                CustomFrame.Instance.ForceHideStatus = true;
                CustomFrame.Instance.ShowStatusBar(false);
            }
            else
            {
                //Show
                Transform1.Animate(Transform1.TranslateY, 0.0, "TranslateY", this.ANIMATION_DURATION_MS, 0, this.ANIMATION_EASING);
                Transform2.Animate(Transform2.TranslateY, 0.0, "TranslateY", this.ANIMATION_DURATION_MS, 0, this.ANIMATION_EASING);
                this.textBoxText.Animate(0.0, 0.8, "Opacity", this.ANIMATION_INOUT_DURATION_MS, 0, null, null);
                //
                CustomFrame.Instance.ForceHideStatus = false;
                CustomFrame.Instance.ShowStatusBar(true);
            }
        }

        private void imageViewer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.RespondToTap();
        }

        private void Close_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Hide();
        }
        /*
        private VKPhoto CurrentVM
        {
            get
            {
                return this._photos[this._currentInd];
            }
        }
        */
        private void Like_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var photo = this.CurrentPhotoVM.Photo;
            //Debug.Assert(photo.likes != null);
            if (photo.likes == null)
                photo.likes = new VKLikes();

            (sender as FrameworkElement).IsHitTestVisible = false;

            LikesService.Instance.AddRemoveLike(!photo.likes.user_likes, photo.owner_id, photo.id, LikeObjectType.photo, (likes) =>
            {
                Execute.ExecuteOnUIThread(() =>
                { 
                    (sender as FrameworkElement).IsHitTestVisible = true;
                    if(likes!= -1)
                    {
                        bool temp = photo.likes.user_likes;
                        photo.likes = new VKLikes() { count = (uint)likes, user_likes = !temp };
                        this.RaisePropertyChanged("CurrentPhotoVM");
                        //this.RaisePropertyChanged("CurrentPhotoVM.UserLiked");
                    }
                });

            }, photo.access_key);
        }

        private void SaveToDevice_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.SaveToDevice(this.CurrentPhotoVM.Photo.MaxPhoto);
        }

        private async void SaveToDevice(string url)
        {
            string path = Settings.SaveFolderPhoto;
            string fileName = string.Format("photo{0}_{1}.jpg", this.CurrentPhotoVM.Photo.owner_id, this.CurrentPhotoVM.Photo.id);

            StorageFile file = null;
            if (string.IsNullOrEmpty(path))
            {
                var picker = new FileSavePicker();

                // set appropriate file types
                picker.FileTypeChoices.Add(".jpg Image", new List<string> { ".jpg" });
                picker.DefaultFileExtension = ".jpg";
                picker.SuggestedFileName = fileName;

                //picker.ContinuationData.Add("Url", url);
                //var view = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView();
                //view.Activated += this.view_Activated;
                //picker.PickSaveFileAndContinue();

                file = await picker.PickSaveFileAsync();
            }
            else
            {
                try
                {
                    var folder = await StorageFolder.GetFolderFromPathAsync(path);
                    file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                }
                catch
                {
                    this.MakeToast(url, false);
                }
            }

            if (file != null)
            {
                if (string.IsNullOrEmpty(path))
                    Settings.SaveFolderPhoto = file.Path.Replace("\\"+fileName,"");

                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    var client = new System.Net.Http.HttpClient();
                    var httpStream = await client.GetStreamAsync(new Uri(url));
                    await httpStream.CopyToAsync(fileStream);
                    fileStream.Dispose();
                    this.MakeToast(file.Path, true);
                }
            }
        }

        private void MakeToast(string fileName, bool status)
        {
            // template to load for showing Toast Notification
            string xmlToastTemplate =   "<toast launch=\"app-defined-string\">" +
                                            "<visual>" +
                                                "<binding template =\"ToastGeneric\">" +
                                                    "<text>" + (status ? "Скачивание завершено" : "Ошибка скачивания") + "</text>" +
                                                    "<text>" + (status ? "Изображение загружено " : "Изображение не загружено ") + fileName + "</text>" +
                                                    (status ? "<image placement=\"hero\" src=\"" + fileName + "\"/>" : "") +
                                                "</binding>" +
                                            "</visual>" +
                                        "</toast>";

            // load the template as XML document
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlToastTemplate);

            // create the toast notification and show to user
            var toastNotification = new ToastNotification(xmlDocument);
            var notification = ToastNotificationManager.CreateToastNotifier();
            notification.Show(toastNotification);
        }

        private void Options_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //FrameworkElement element = sender as FrameworkElement;

            MenuFlyout menu = new MenuFlyout();

            if(this.CurrentPhotoVM.Photo.album_id != VKConstants.ALBUM_TYPE_SAVED_PHOTOS)
            {
                MenuFlyoutItem item = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("AppBarMenu_SaveInAlbum") };
                item.Command = new DelegateCommand((args) =>
                {
                    this.SaveInSavedPhotosAlbum();
                });
                menu.Items.Add(item);
            }

            if (this.CurrentPhotoVM.Photo.owner_id != Settings.UserId)
            {
                MenuFlyoutSubItem item2 = new MenuFlyoutSubItem() { Text = LocalizedStrings.GetString("Report") };
                MenuFlyoutItem subitem = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonSpam"), CommandParameter = ReportReason.Spam };
                subitem.Command = new DelegateCommand((args) => { this.ReportPhoto(args); });
                item2.Items.Add(subitem);
                MenuFlyoutItem subitem2 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonChildPorn"), CommandParameter = ReportReason.ChildPorn };
                subitem2.Command = new DelegateCommand((args) => { this.ReportPhoto(args); });
                item2.Items.Add(subitem2);
                MenuFlyoutItem subitem3 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonExtremism"), CommandParameter = ReportReason.Extremism };
                subitem3.Command = new DelegateCommand((args) => { this.ReportPhoto(args); });
                item2.Items.Add(subitem3);
                MenuFlyoutItem subitem4 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonViolence"), CommandParameter = ReportReason.Violence };
                subitem4.Command = new DelegateCommand((args) => { this.ReportPhoto(args); });
                item2.Items.Add(subitem4);
                MenuFlyoutItem subitem5 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonDrug"), CommandParameter = ReportReason.Drugs };
                subitem5.Command = new DelegateCommand((args) => { this.ReportPhoto(args); });
                item2.Items.Add(subitem5);
                MenuFlyoutItem subitem6 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonAdult"), CommandParameter = ReportReason.Adult };
                subitem6.Command = new DelegateCommand((args) => { this.ReportPhoto(args); });
                item2.Items.Add(subitem6);
                MenuFlyoutItem subitem7 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonInsult"), CommandParameter = ReportReason.Abuse };
                subitem7.Command = new DelegateCommand((args) => { this.ReportPhoto(args); });
                item2.Items.Add(subitem7);
                menu.Items.Add(item2);
            }

            MenuFlyoutItem item3 = new MenuFlyoutItem() { Text = "Перейти к владельцу" };
            item3.Command = new DelegateCommand((args) =>
            {
                NavigatorImpl.Instance.NavigateToProfilePage(this.CurrentPhotoVM.Photo.owner_id);
            });
            menu.Items.Add(item3);

            MenuFlyoutItem item4 = new MenuFlyoutItem() { Text = "Перейти к альбому" };
            item4.Command = new DelegateCommand((args) =>
            {
                NavigatorImpl.Instance.NavigateToPhotosOfAlbum(this.CurrentPhotoVM.Photo.owner_id, this.CurrentPhotoVM.Photo.album_id,"");
            });
            menu.Items.Add(item4);

            if (menu.Items.Count > 0)
                menu.ShowAt(sender as FrameworkElement);
        }

        private void ReportPhoto(object args)
        {
            VKPhoto photo = this.CurrentPhotoVM.Photo;
            PhotosService.Instance.Report(photo.owner_id, photo.id, (ReportReason)args,null);
        }

        private void SaveInSavedPhotosAlbum()
        {
            VKPhoto photo = this.CurrentPhotoVM.Photo;
            PhotosService.Instance.CopyPhotos(photo.owner_id, photo.id, photo.access_key,null);
        }

        async void view_Activated(Windows.ApplicationModel.Core.CoreApplicationView sender, Windows.ApplicationModel.Activation.IActivatedEventArgs args)
        {
            var fargs = args as Windows.ApplicationModel.Activation.FileSavePickerContinuationEventArgs;
            sender.Activated -= this.view_Activated;

            StorageFile file = fargs.File;
            if (file != null)
            {
                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    var client = new System.Net.Http.HttpClient();
                    var httpStream = await client.GetStreamAsync(new Uri((string)fargs.ContinuationData["Url"]));
                    await httpStream.CopyToAsync(fileStream);
                    fileStream.Dispose();
                }
            }
        }


        public static void ShowPhotosFromAlbum(string aid, ImageViewerViewModel.AlbumType albumType, int userOrGroupId, uint photosCount, int selectedPhotoIndex, List<VKPhoto> photos, Func<int, Border> getImageByIdFunc)
        {
            ImageViewerViewModel imageViewerViewModel = new ImageViewerViewModel(aid, albumType, userOrGroupId, photosCount, photos);
            ImageViewerDecoratorUC decoratorForCurrentPage = new ImageViewerDecoratorUC();//ImageViewerDecoratorUC decoratorForCurrentPage = ImageViewerDecoratorUC.GetDecoratorForCurrentPage(null);
            decoratorForCurrentPage.InitWith(imageViewerViewModel, getImageByIdFunc);
            //decoratorForCurrentPage.Initialize(this.VM.Photos.ToList(), getImageByIdFunc );
            decoratorForCurrentPage.Show(selectedPhotoIndex);
        }

        public static void ShowPhotosFromProfile(int userOrGroupId, int selectedPhotoIndex, List<VKPhoto> photos, /*bool canLoadMoreProfileListPhotos*/uint photosCount)
        {
            ImageViewerViewModel imageViewerViewModel = new ImageViewerViewModel(userOrGroupId, photos, /*canLoadMoreProfileListPhotos*/true, 0) { _photosCount = photosCount };
            ImageViewerDecoratorUC decoratorForCurrentPage = new ImageViewerDecoratorUC();
            decoratorForCurrentPage.InitWith(imageViewerViewModel, (i => null));
            decoratorForCurrentPage.Show(selectedPhotoIndex);
        }

        public static void ShowPhotosById(uint photosCount, int initialOffset, int selectedPhotoIndex, List<VKPhoto> photos, ImageViewerViewModel.ViewerMode viewerMode = ImageViewerViewModel.ViewerMode.PhotosByIds, Func<int, Border> getImageByIdFunc = null, bool hideActions = false)
        {
            Debug.Assert(selectedPhotoIndex!= -1);
            ImageViewerViewModel ivvm = new ImageViewerViewModel(photosCount, initialOffset, photos, viewerMode);
            ImageViewerDecoratorUC decoratorForCurrentPage = new ImageViewerDecoratorUC(); //ImageViewerDecoratorUC decoratorForCurrentPage = ImageViewerDecoratorUC.GetDecoratorForCurrentPage(page);
            decoratorForCurrentPage.InitWith(ivvm, getImageByIdFunc);
            //decoratorForCurrentPage._fromDialog = fromDialog;
            //decoratorForCurrentPage._friendsOnly = friendsOnly;
            //decoratorForCurrentPage._hideActions = hideActions;
            if (hideActions)
                decoratorForCurrentPage.gridBottom.Visibility = Visibility.Collapsed;
            decoratorForCurrentPage.Show(selectedPhotoIndex);
        }

        public static void ShowPhotosFromFeed(int userOrGroupId, string aid, uint photosCount, int selectedPhotoIndex, DateTime date, List<VKPhoto> photos, string mode, Func<int, Border> getImageByIdFunc)
        {
            ImageViewerViewModel.ViewerMode mode1 = (ImageViewerViewModel.ViewerMode)Enum.Parse(typeof(ImageViewerViewModel.ViewerMode), mode);
            ImageViewerViewModel ivvm = new ImageViewerViewModel(userOrGroupId, aid, photosCount, date, photos, mode1);
            ImageViewerDecoratorUC dec = new ImageViewerDecoratorUC(); //ImageViewerDecoratorUC dec = ImageViewerDecoratorUC.GetDecoratorForCurrentPage(null);
            dec.InitWith(ivvm, getImageByIdFunc);
            ivvm.LoadPhotosFromFeed((res =>
            {
                if (!res)
                    return;
                dec.HandlePhotoUpdate(ivvm);
            }));
            dec.Show(selectedPhotoIndex);
        }


        private ImageViewerViewModel _imageViewerVM;
        private FrameworkElement _currentViewControl;
        private Action<int> _setContextOnViewControl;
        private Action<int, bool> _showHideOverlay;

        private void InitWith(ImageViewerViewModel ivvm, Func<int, Border> getImageByIdFunc, FrameworkElement currentViewControl = null, Action<int> setContextOnViewControl = null, Action<int, bool> showHideOverlay = null)
        {
            this._imageViewerVM = ivvm;
            this._getImageByIdFunc = getImageByIdFunc;//this._getImageFunc = getImageByIdFunc;
            this._currentViewControl = currentViewControl;
            this._setContextOnViewControl = setContextOnViewControl;
            this._showHideOverlay = showHideOverlay;
            this.HandlePhotoUpdate(ivvm);
            //
            base.SizeChanged += ImageViewerDecoratorUC_SizeChanged;
        }

        private void HandlePhotoUpdate(ImageViewerViewModel ivvm)
        {
            if (this._imageViewerVM != ivvm)
                return;

            this.InitializeImageViewer(ivvm._photosCount, (ind =>
            {
                Border image1;
                if (this._getImageByIdFunc == null)
                {
                    image1 = null;
                }
                else
                {
                    image1 = this._getImageByIdFunc(ind);
                }

                ImageInfo imageInfo = new ImageInfo();
                if (ind == this._currentInd && this.CurrentImage.Source is BitmapImage)
                {
                    BitmapImage source = this.CurrentImage.Source as BitmapImage;
                    imageInfo.Width = (double)source.PixelWidth;
                    imageInfo.Height = (double)source.PixelHeight;
                }
                /* omg переделка для бордера 
                 * А это вообще нужно?
                if ((image1 != null ? image1.Source : null) is BitmapImage)
                {
                    BitmapImage source = (BitmapImage)image1.Source;
                    imageInfo.Width = source.PixelWidth;
                    imageInfo.Height = source.PixelHeight;
                }*/
                






                if (ind < 0 || ind >= ivvm.PhotosCollection.Count)//BUG!!!!!!!!!!!!!
                    return null;
                PhotoViewModel photos = ivvm.PhotosCollection[ind];
                imageInfo.Uri = photos.ImageSrc;//bugfix?:replaced
                if (imageInfo.Width == 0)
                    imageInfo.Width = (double)photos.Photo.width;
                if (imageInfo.Height == 0)
                    imageInfo.Height = (double)photos.Photo.height;
                //imageInfo.Uri = photos.ImageSrc;
                return imageInfo;
            }));
            /*
            this.imageViewer.Initialize(ivvm.PhotosCollection.Count, (Func<int, ImageInfo>)(ind =>
            {
                Func<int, Image> getImageByIdFunc = this._getImageByIdFunc;
                Image image1;
                if (getImageByIdFunc == null)
                {
                    image1 = null;
                }
                else
                {
                    int num = ind;
                    image1 = getImageByIdFunc(num);
                }

                ImageInfo imageInfo = new ImageInfo();
                if (ind == this.imageViewer.CurrentInd && this.imageViewer.CurrentImage.Source is BitmapImage)
                {
                    BitmapImage source = this.imageViewer.CurrentImage.Source as BitmapImage;
                    imageInfo.Width = (double)((BitmapSource)source).PixelWidth;
                    imageInfo.Height = (double)((BitmapSource)source).PixelHeight;
                }
                if ((image1 != null ? image1.Source : null) is BitmapImage)
                {
                    BitmapImage source = (BitmapImage)image1.Source;
                    imageInfo.Width = (double)((BitmapSource)source).PixelWidth;
                    imageInfo.Height = (double)((BitmapSource)source).PixelHeight;
                }
                if (ind < 0 || ind >= ivvm.PhotosCollection.Count)
                    return null;
                PhotoViewModel photos = ivvm.PhotosCollection[ind];
                imageInfo.Uri = photos.ImageSrc;//bugfix?:replaced
                if (imageInfo.Width == 0.0)
                    imageInfo.Width = (double)photos.Photo.width;
                if (imageInfo.Height == 0.0)
                    imageInfo.Height = (double)photos.Photo.height;
                //imageInfo.Uri = photos.ImageSrc;
                return imageInfo;
            }), this._getImageByIdFunc, (Action<int, bool>)((ind, show) =>
            {
                Func<int, Image> getImageByIdFunc = this._getImageByIdFunc;
                Image image1;
                if (getImageByIdFunc == null)
                {
                    image1 = null;
                }
                else
                {
                    int num = ind;
                    image1 = getImageByIdFunc(num);
                }
                Image image2 = image1;
                if (image2 != null)
                    image2.Opacity = (show ? 1.0 : 0.0);
                Action<int, bool> showHideOverlay = this._showHideOverlay;
                if (showHideOverlay == null)
                    return;
                int num1 = ind;
                int num2 = show ? 1 : 0;
                showHideOverlay(num1, num2 != 0);
            }), this._currentViewControl, this._setContextOnViewControl);*/
        }
        /*
         * public void Initialize(int totalCount, Func<int, ImageInfo> getImageInfoFunc, Func<int, Image> getImageFunc, Action<int, bool> showHideOriginalImageAction, FrameworkElement currentViewControl = null, Action<int> setDataContextOnCurrentViewControlAction = null)
    {
      this._count = totalCount;
      this._getImageInfoFunc = getImageInfoFunc;
      this._getImageFunc = getImageFunc;
      this._showHideOriginalImageAction = showHideOriginalImageAction;
      if (this._currentViewControl != null)
        this.Children.Remove(this._currentViewControl);
      this._currentViewControl = currentViewControl;
      this._setDataContextOnCurrentViewControlAction = setDataContextOnCurrentViewControlAction;
      if (!this._isShown)
        return;
      this.UpdateImagesSources(false, new bool?());
    }
    */
        public PhotoViewModel CurrentPhotoVM
        {
            get { return this.GetVMByIndex(this._currentInd); }
        }

        private PhotoViewModel GetVMByIndex(int currentInd)
        {
            if (currentInd >= 0 && this._imageViewerVM != null && currentInd < this._imageViewerVM.PhotosCollection.Count)
                return this._imageViewerVM.PhotosCollection[currentInd];
            return null;
        }

        private void RespondToCurrentIndexChanged()
        {
            this.RaisePropertyChanged(nameof(this.CurrentPhotoVM));//обновляем счётчики
            if (this.CurrentPhotoVM != null)
            {
                if (!this.CurrentPhotoVM.IsLoadedFullInfo)
                    this.CurrentPhotoVM.LoadInfoWithComments(null);


                /*
                Action loadPreviousVM = (() => Execute.ExecuteOnUIThread((() =>
                {
                    if (this.PreviousPhotoVM == null || this.PreviousPhotoVM.IsLoadedFullInfo)
                        return;
                    this.PreviousPhotoVM.LoadInfoWithComments(((resPrevious, re) => { }));
                })));
                Action loadNextPhotoVM = (() => Execute.ExecuteOnUIThread((() =>
                {
                    this.Update();
                    if (this.NextPhotoVM == null)
                        return;
                    if (!this.NextPhotoVM.IsLoadedFullInfo)
                        this.NextPhotoVM.LoadInfoWithComments(((resNext, ress) => loadPreviousVM()));
                    else
                        loadPreviousVM();
                })));
                if (!this.CurrentPhotoVM.IsLoadedFullInfo)
                    this.CurrentPhotoVM.LoadInfoWithComments(((res, r) => loadNextPhotoVM()));
                else
                    loadNextPhotoVM();
                    */
            }

            if (this._imageViewerVM.PhotosCollection.Count - 1 - this._currentInd < 3)
            {
                this._imageViewerVM.LoadMorePhotos((res) =>
                {
                    if (!res)
                        return;

                    this.HandlePhotoUpdate(this._imageViewerVM);
                    this.RaisePropertyChanged("CurrentPhotoVM");
                    this.Update();
                    this.UpdateImagesSources(false);
                });
            }
            this.Update();
        }

        private void InitializeImageViewer(uint totalCount, Func<int, ImageInfo> getImageInfoFunc)
        {
            this._count = totalCount;
            this._getImageInfoFunc = getImageInfoFunc;
            //this._getImageFunc = getImageFunc;
            //this._showHideOriginalImageAction = showHideOriginalImageAction;
            //if (this._currentViewControl != null)
            //    this.Children.Remove(this._currentViewControl);
            //this._currentViewControl = currentViewControl;
            //this._setDataContextOnCurrentViewControlAction = setDataContextOnCurrentViewControlAction;
            //if (!this._isShown)
            //    return;

 //           this.UpdateImagesSources(false);
        }

        private void RaisePropertyChanged(string property)
        {
            // ISSUE: reference to a compiler-generated field
            if (this.PropertyChanged == null)
                return;
            Core.Framework.Execute.ExecuteOnUIThread(() =>
            {
                // ISSUE: reference to a compiler-generated field
                PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
                if (propertyChanged == null)
                    return;
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(property);
                propertyChanged(this, e);
            });
        }

        private void Share_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SharePostUC share = new SharePostUC("фотографией", WallService.RepostObject.photo, this.CurrentPhotoVM.Photo.owner_id, this.CurrentPhotoVM.Photo.id, this.CurrentPhotoVM.Photo.access_key);
            PopUpService popUp = new PopUpService { Child = share, OverrideBackKey = true, AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed };
            share.Done = popUp.Hide;
            popUp.Show();
            e.Handled = true;
        }
        
        private void ChevronRight_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(e!=null)
                e.Handled = true;

            (this._images[1].RenderTransform as CompositeTransform).TranslateX = -1;
            this.HandleDragCompleted(-(this.MOVE_TO_NEXT_VELOCITY_THRESHOLD + 10), 0);
        }

        private void ChevronLeft_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null)
                e.Handled = true;

            (this._images[1].RenderTransform as CompositeTransform).TranslateX = 1;
            this.HandleDragCompleted(this.MOVE_TO_NEXT_VELOCITY_THRESHOLD + 10, 0);
        }

        private void CommentTap(object sender, TappedRoutedEventArgs e)
        {
            if(this.CurrentPhotoVM.Photo.can_comment)
                NavigatorImpl.Instance.NavigateToPhotoWithComments(this.CurrentPhotoVM.Photo.owner_id, this.CurrentPhotoVM.Photo.id, this.CurrentPhotoVM.Photo.access_key, this.CurrentPhotoVM.Photo);
        }

        private void ChevronLeft_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            element.Opacity = 1.0;
        }

        private void ChevronLeft_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            element.Opacity = 0.5;
        }
        /*
        private void UserControl_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            //this.ChevronLeft.Visibility = (this._currentInd == 0 || this.ActualWidth < 700) ? Visibility.Collapsed : Visibility.Visible;
            //this.ChevronRight.Visibility = (this._currentInd + 1 >= this._count || this.ActualWidth < 700) ? Visibility.Collapsed : Visibility.Visible;

            if (e.Key == Windows.System.VirtualKey.Right)
            {
                if (this._currentInd + 1 < this._count)
                    this.ChevronRight_Tapped(sender, null);
            }
            else if (e.Key == Windows.System.VirtualKey.Left)
            {
                if (this._currentInd > 0)
                    this.ChevronLeft_Tapped(sender, null);
            }
        }
        */
        private void UserTap(object sender, TappedRoutedEventArgs e)
        {

        }



        /*
        
        private void SelectTaggedUser(int ind)
    {
      PhotoVideoTag photoTag = this._photoTags[ind];
      if (this._selectedTagInd == ind)
      {
        if (photoTag.uid == 0L)
          return;
        Navigator.Current.NavigateToUserProfile(photoTag.uid, photoTag.tagged_name, "", false);
      }
      else
      {
        for (int index = 0; index < this._tagHyperlinks.Count; ++index)
        {
          Hyperlink tagHyperlink = this._tagHyperlinks[index];
          if (index == ind)
          {
            if (photoTag.uid != 0L)
              HyperlinkHelper.SetState(tagHyperlink, HyperlinkState.Normal,  null);
          }
          else
            HyperlinkHelper.SetState(tagHyperlink, HyperlinkState.Accent,  null);
        }
        WriteableBitmap opacityMask = this.GenerateOpacityMask(((FrameworkElement) this.image).ActualWidth, ((FrameworkElement) this.image).ActualHeight, photoTag.x, photoTag.x2, photoTag.y, photoTag.y2);
        Image image = this.image;
        ImageBrush imageBrush = new ImageBrush();
        WriteableBitmap writeableBitmap = opacityMask;
        imageBrush.ImageSource=((ImageSource) writeableBitmap);
        int num = 1;
        ((TileBrush) imageBrush).Stretch=((Stretch) num);
        ((UIElement) image).OpacityMask=((Brush) imageBrush);
        this._selectedTagInd = ind;
      }
    }


         private WriteableBitmap GenerateOpacityMask(double totalWidth, double totalHeight, double x1, double x2, double y1, double y2)
    {
      int num1 = (int) (100.0 * (totalHeight / totalWidth));
      int num2 = (int) (100.0 * x1 / 100.0);
      int num3 = (int) (100.0 * x2 / 100.0);
      int num4 = (int) ((double) num1 * y1 / 100.0);
      int num5 = (int) ((double) num1 * y2 / 100.0);
      WriteableBitmap writeableBitmap = new WriteableBitmap(100, num1);
      for (int index = 0; index < writeableBitmap.Pixels.Length; ++index)
      {
        int num6 = index % ((BitmapSource) writeableBitmap).PixelWidth;
        int num7 = index / ((BitmapSource) writeableBitmap).PixelWidth;
        writeableBitmap.Pixels[index] = num6 < num2 || num6 > num3 || (num7 < num4 || num7 > num5) ? int.MinValue : -16777216;
      }
      return writeableBitmap;
    }
    */

    }
}
