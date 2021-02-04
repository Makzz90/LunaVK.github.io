using LunaVK.Core.Utils;
using LunaVK.Framework;
using LunaVK.UC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.Common
{
    public class PreviewBehavior
    {
        /// <summary>
        /// 200
        /// </summary>
        public static readonly int PUSH_ANIMATION_DURATION = 200;

        /// <summary>
        /// 150ms
        /// </summary>
        public static readonly int HOLD_GESTURE_MS = 250;
        public static readonly EasingFunctionBase PUSH_ANIMATION_EASING = new CubicEase();

        /// <summary>
        /// 0.8
        /// </summary>
        public static readonly double PUSH_SCALE = 0.8;

#region TopOffset
        public static readonly DependencyProperty TopOffsetProperty = DependencyProperty.Register("TopOffset", typeof(int), typeof(PreviewBehavior), new PropertyMetadata(140));
        public static int GetTopOffset(DependencyObject d)
        {
            return (int)d.GetValue(TopOffsetProperty);
        }

        public static void SetTopOffset(DependencyObject d, int value)
        {
            d.SetValue(TopOffsetProperty, value);
        }
#endregion

        private static bool _isShowingPreview = false;
        private static bool _needShowPreview = false;
        private const int DEFAULT_TOP_OFFSET = 140;
        private static DispatcherTimer _timer;
        //private static DispatcherTimer _hideTimer;
        private static PopUpService _loader;//private FullscreenLoader _loader;
        private static PreviewImageUC _ucPreview;
        private static DateTime _lastTouchFrameDate;
        private static Image AssociatedObject;

#region PreviewUri
        public static readonly DependencyProperty PreviewUriProperty = DependencyProperty.RegisterAttached("PreviewUri", typeof(Uri), typeof(PreviewBehavior), new PropertyMetadata(null, OnCacheUriChanged));

        /// <summary>
        /// Gets the PreviewUri property. This dependency property 
        /// WebUri that has to be cached
        /// </summary>
        public static Uri GetPreviewUri(DependencyObject d)
        {
            return (Uri)d.GetValue(PreviewUriProperty);
        }

        /// <summary>
        /// Sets the PreviewUri property. This dependency property 
        /// WebUri that has to be cached
        /// </summary>
        public static void SetPreviewUri(DependencyObject d, Uri value)
        {
            d.SetValue(PreviewUriProperty, value);
        }

        private static void OnCacheUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Uri newCacheUri = (Uri)d.GetValue(PreviewUriProperty);
            Image image = (Image)d;

            image.PointerPressed += PreviewBehavior.AssociatedObject_PointerPressed;
            image.PointerEntered += PreviewBehavior.AssociatedObject_PointerEntered;
            image.PointerCaptureLost += PreviewBehavior.AssociatedObject_PointerExited;
            image.PointerExited += PreviewBehavior.AssociatedObject_PointerExited;
            image.PointerReleased += PreviewBehavior.AssociatedObject_PointerReleased;
            //image.PointerMoved += PreviewBehavior.AssociatedObject_PointerMoved;
            image.RenderTransformOrigin = new Point(0.5, 0.5);

            image.ManipulationStarted += AssociatedObject_ManipulationStarted;
            image.ManipulationDelta += AssociatedObject_ManipulationDelta;
            image.ManipulationCompleted += AssociatedObject_ManipulationCompleted;
            image.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            image.Tag = "CantTouchThis";

            image.Unloaded += PreviewBehavior.AssociatedObject_Unloaded;
        }
        
        #endregion

        private static void AssociatedObject_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (PreviewBehavior._isShowingPreview)
            {
                e.Handled = true;
//                PreviewBehavior.EnsureHidePreview();
            }
            else
            {
                //                PreviewBehavior.AnimateScale(sender as FrameworkElement, false);

                //                PreviewBehavior._needShowPreview = false;

                //                PreviewBehavior.StartTimer();
                StopTimer();
            }

            
            /*
             * if (PreviewBehavior._isShowingPreview)
              {
                e.Handled = true;
                this.EnsureHidePreview();
              }
              else
                this.StopTimer();
        */
        }

        private static void AssociatedObject_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (PreviewBehavior._isShowingPreview)
            {
                e.Handled = true;
            }
            else
            {
                /*                if (PreviewBehavior.AssociatedObject == sender as Image)
                                    return;

                                PreviewBehavior.AssociatedObject = sender as Image;

                                PreviewBehavior.AnimateScale(sender as FrameworkElement, true);

                                PreviewBehavior.StartTimer();

                                PreviewBehavior._needShowPreview = true;
                */
                //
                //
                PreviewBehavior._needShowPreview = false;
                PreviewBehavior.StopTimer();
            }
        }

        private static void AssociatedObject_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if(PreviewBehavior._isShowingPreview)
            {
                e.Handled = true;
            }
            else
            {
//                PreviewBehavior.AssociatedObject = sender as Image;

//                PreviewBehavior.AnimateScale(sender as FrameworkElement, true);

//               PreviewBehavior.StartTimer();

                PreviewBehavior._needShowPreview = true;
                PreviewBehavior.StopTimer();
            }

            
            /*
             * if (PreviewBehavior._isShowingPreview)
              {
                e.Handled = true;
                this.EnsureHidePreview();
              }
              else
                this.StartTimer();
        */
        }
        

        private static void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            Image image = (Image)sender;

            image.PointerPressed -= PreviewBehavior.AssociatedObject_PointerPressed;
            image.PointerEntered -= PreviewBehavior.AssociatedObject_PointerEntered;
            image.PointerCaptureLost -= PreviewBehavior.AssociatedObject_PointerExited;
            image.PointerExited -= PreviewBehavior.AssociatedObject_PointerExited;
            image.PointerReleased -= PreviewBehavior.AssociatedObject_PointerReleased;
            //image.PointerMoved -= PreviewBehavior.AssociatedObject_PointerMoved;

            image.ManipulationStarted -= PreviewBehavior.AssociatedObject_ManipulationStarted;
            image.ManipulationDelta -= PreviewBehavior.AssociatedObject_ManipulationDelta;
            image.ManipulationCompleted -= PreviewBehavior.AssociatedObject_ManipulationCompleted;
        }
        
        static PreviewBehavior()
        {
            PreviewBehavior._timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(PreviewBehavior.HOLD_GESTURE_MS) };
            PreviewBehavior._timer.Tick += PreviewBehavior._timer_Tick;
        }
        
        private static void AnimateScale(FrameworkElement element, bool inc)
        {
            ScaleTransform tr = element.RenderTransform as ScaleTransform;
            if (tr == null)
            {
                element.RenderTransform = new ScaleTransform();
                tr = element.RenderTransform as ScaleTransform;
            }

            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = tr,
                propertyPath = "ScaleX",
                from = tr.ScaleX,
                to = inc ? 1.3 : 1.0,
                duration = 200
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = tr,
                propertyPath = "ScaleY",
                from = tr.ScaleY,
                to = inc ? 1.3 : 1.0,
                duration = 200
            });

            AnimationUtils.AnimateSeveral(animInfoList);
        }
        

        private static void StartTimer()
        {
            //System.Diagnostics.Debug.WriteLine("StartTimer");
            if (PreviewBehavior._timer.IsEnabled)
                return;
            PreviewBehavior._timer.Start();
        }

        private static void StopTimer()
        {
            //System.Diagnostics.Debug.WriteLine("StopTimer");
            if (PreviewBehavior._timer.IsEnabled)
                PreviewBehavior._timer.Stop();
        }

        private static void _timer_Tick(object sender, object e)
        {
            if (PreviewBehavior._needShowPreview)
            {
                Uri previewUri = PreviewBehavior.GetPreviewUri(PreviewBehavior.AssociatedObject);
                Image associatedObject = PreviewBehavior.AssociatedObject as Image;
                BitmapImage originalImage = (associatedObject != null ? associatedObject.Source : null) as BitmapImage;

                PreviewBehavior.ShowPreview(previewUri.AbsoluteUri, originalImage, PreviewBehavior.DEFAULT_TOP_OFFSET);

                PreviewBehavior.StopTimer();
            }
            else
            {
                if ((DateTime.Now - _lastTouchFrameDate).TotalMilliseconds < 750.0)
                    return;
                PreviewBehavior.EnsureHidePreview();
            }
        }
        

            private static void AssociatedObject_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("PointerReleased");

            PreviewBehavior.AnimateScale(sender as FrameworkElement, false);

            PreviewBehavior._needShowPreview = false;

            PreviewBehavior.StartTimer();
        }

        private static void AssociatedObject_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("PointerExited");
            //if (PreviewBehavior._isShowingPreview)
            //{
            //    e.Handled = true;
            //    this.EnsureHidePreview();
            //}
            //else
            //    this.StopTimer();

            PreviewBehavior.AnimateScale(sender as FrameworkElement, false);

            PreviewBehavior._needShowPreview = false;

            if (PreviewBehavior._isShowingPreview)
                PreviewBehavior.StartTimer();
        }
        
        private static void AssociatedObject_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("PointerPressed");
            PreviewBehavior.AssociatedObject = sender as Image;

            PreviewBehavior.AnimateScale(sender as FrameworkElement, true);

            PreviewBehavior.StartTimer();

            PreviewBehavior._needShowPreview = true;
        }
        
        /*
        private static void AssociatedObject_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("PointerMoved");
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var temp = Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.LeftButton);
                if (!temp.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
                    return;
            }

            //_lastTouchFrameDate = DateTime.Now;

            //PreviewBehavior.AssociatedObject = sender as Image;
            if (!PreviewBehavior._isShowingPreview)
            {
                double i = (DateTime.Now - PreviewBehavior._lastTouchFrameDate).TotalMilliseconds;
                if (i < 100.0)
                    return;
                PreviewBehavior._needShowPreview = false;

                PreviewBehavior.StopTimer();
            }
        }
        */
        private static void AssociatedObject_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
                //System.Diagnostics.Debug.WriteLine("PointerEntered");
            PreviewBehavior._lastTouchFrameDate = DateTime.Now;

            //if (PreviewBehavior._isShowingPreview)
            //{
            //    e.Handled = true;
            //    this.EnsureHidePreview();
            //}
            //else
            //    this.StartTimer();



            PreviewBehavior.AnimateScale(sender as FrameworkElement, true);

            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var temp = Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.LeftButton);
                //System.Diagnostics.Debug.WriteLine(temp.ToString());
                if (!temp.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
                    return;
            }

            PreviewBehavior.AssociatedObject = sender as Image;

            PreviewBehavior.StartTimer();

            PreviewBehavior._needShowPreview = true;
        }

        private static void EnsureHidePreview()
        {
            if (!PreviewBehavior._isShowingPreview)
                return;
            PreviewBehavior.StopTimer();
            PreviewBehavior.HidePreview();
        }

        private static void ShowPreview(string previewUri, BitmapImage originalImage = null, int topOffset = 0)
        {
            PreviewBehavior._isShowingPreview = true;
            if (PreviewBehavior._ucPreview == null)
            {
                PreviewBehavior._ucPreview = new PreviewImageUC();
                PreviewBehavior._loader = new PopUpService() { IsHitTestVisible = false };
                PreviewBehavior._loader.AnimationTypeChild = PopUpService.AnimationTypes.None;
                PreviewBehavior._loader.Child = PreviewBehavior._ucPreview;
                PreviewBehavior._loader.Show();
            }
            PreviewBehavior._ucPreview.SetImageUri(previewUri, originalImage);
            PreviewBehavior._ucPreview.imagePreview.Margin = (new Thickness(0.0, topOffset, 0.0, 0.0));
        }

        private static void HidePreview()
        {
            if (PreviewBehavior._ucPreview == null)
                return;
            PreviewBehavior._loader.Hide();
            PreviewBehavior._loader = null;
            PreviewBehavior._ucPreview = null;

            PreviewBehavior._isShowingPreview = false;
        }

        
    }
}
