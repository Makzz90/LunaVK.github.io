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
        public static readonly int PUSH_ANIMATION_DURATION = 200;
        public static readonly int HOLD_GESTURE_MS = 150;
        public static readonly EasingFunctionBase PUSH_ANIMATION_EASING = new CubicEase();
        public static readonly double PUSH_SCALE = 0.8;
        public static readonly DependencyProperty TopOffsetProperty = DependencyProperty.Register("TopOffset", typeof(int), typeof(PreviewBehavior), new PropertyMetadata(140));
        private static bool _isShowingPreview = false;
        private static DateTime _lastShownTime = DateTime.MinValue;
        private const int DEFAULT_TOP_OFFSET = 140;
        private static DispatcherTimer _timer;
        private static PopUpService _loader;//private FullscreenLoader _loader;
        private static PreviewImageUC _ucPreview;
        private static FrameworkElement _hoveredElement;
        //private SupportedPageOrientation _savedSupportedOrientation;
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
            if (_timer == null)
            {
                DispatcherTimer dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Interval = TimeSpan.FromMilliseconds(PreviewBehavior.HOLD_GESTURE_MS);
                _timer = dispatcherTimer;
            }




            Uri newCacheUri = (Uri)d.GetValue(PreviewUriProperty);
            Image image = (Image)d;

            image.PointerPressed += PreviewBehavior.AssociatedObject_PointerPressed;
            //image.PointerEntered += PreviewBehavior.AssociatedObject_PointerEntered;
            image.PointerCaptureLost += PreviewBehavior.AssociatedObject_PointerExited;
            image.PointerExited += PreviewBehavior.AssociatedObject_PointerExited;
            //image.PointerReleased += PreviewBehavior.AssociatedObject_PointerReleased;

            image.ManipulationStarted += AssociatedObject_ManipulationStarted;
            image.ManipulationDelta += AssociatedObject_ManipulationDelta;
            image.ManipulationCompleted += AssociatedObject_ManipulationCompleted;
            image.ManipulationMode = ManipulationModes.TranslateY | ManipulationModes.TranslateX;

            image.Holding += Image_Holding;

            image.Unloaded += PreviewBehavior.AssociatedObject_Unloaded;
            image.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        private static void AssociatedObject_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("PointerExited");
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                //var temp = Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.LeftButton);
                //if (temp.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down) || temp.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Locked))
                //{
                //}
                if (PreviewBehavior._isShowingPreview)
                {
                    e.Handled = true;
                    //PreviewBehavior.EnsureHidePreview();
                    StartTimer();
                }
                else
                    PreviewBehavior.StopTimer();
            }
        }

        private static void AssociatedObject_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (PreviewBehavior._isShowingPreview)
            {
                e.Handled = true;
                PreviewBehavior.EnsureHidePreview();
            }
            else
                PreviewBehavior.StopTimer();

        }

        private static void AssociatedObject_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (PreviewBehavior._isShowingPreview && !e.IsInertial)
            {
                Point point = e.Position;
                UIElement rootVisual = Window.Current.Content as UIElement;

                //var p = PreviewBehavior.GetHostCoordinates(e.Container.TransformToVisual(rootVisual).TransformPoint(point));
                var p = e.Container.TransformToVisual(rootVisual).TransformPoint(point);

                var newHoveredElements = VisualTreeHelper.FindElementsInHostCoordinates(p, rootVisual);
                FrameworkElement newHoveredElement = (FrameworkElement)newHoveredElements.FirstOrDefault((el) => el is Image);
                if (newHoveredElement != null)
                {
                    //BehaviorCollection behaviors1 = Interaction.GetBehaviors((DependencyObject)newHoveredElement);
                    //PreviewBehavior previewBehavior = (behaviors1 != null ? behaviors1.FirstOrDefault<Behavior>((Func<Behavior, bool>)(b => b is PreviewBehavior)) : (Behavior)null) as PreviewBehavior;
                    //if (previewBehavior == null && newHoveredElement.Parent != null)
                    //{
                    //    BehaviorCollection behaviors2 = Interaction.GetBehaviors(newHoveredElement.Parent);
                    //    previewBehavior = (behaviors2 != null ? behaviors2.FirstOrDefault<Behavior>((Func<Behavior, bool>)(b => b is PreviewBehavior)) : (Behavior)null) as PreviewBehavior;
                    //}
                    //if (previewBehavior != null)
                    //{
                    if (_hoveredElement != newHoveredElement)
                    {

                        Image image = newHoveredElement as Image;

                        Uri previewUri = PreviewBehavior.GetPreviewUri(image);
                        if (previewUri != null)
                        {
                            System.Diagnostics.Debug.WriteLine(previewUri);
                            BitmapImage originalImage = (image != null ? image.Source : null) as BitmapImage;
                            ShowPreview(previewUri.AbsoluteUri, originalImage, PreviewBehavior.DEFAULT_TOP_OFFSET);
                            SetHoveredElement(newHoveredElement);
                        }
                    }
                    //}
                }
                e.Handled = true;
            }
            else
                StopTimer();
        }

        private static void AssociatedObject_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("AssociatedObject_ManipulationStarted");
            _lastTouchFrameDate = DateTime.Now;

            if (PreviewBehavior._isShowingPreview)
            {
                e.Handled = true;
                //
                //
                StopTimer();
                //
                //
                //PreviewBehavior.EnsureHidePreview();
            }
            else
                PreviewBehavior.StartTimer();
        }

        private static void AssociatedObject_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("PointerPressed");
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var temp = Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.LeftButton);
                if (temp.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down) || temp.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Locked))
                {


                    _lastTouchFrameDate = DateTime.Now;
                    PreviewBehavior.AssociatedObject = sender as Image;

                    if (PreviewBehavior._isShowingPreview)
                    {
                        (sender as Image).ManipulationMode = ManipulationModes.TranslateY | ManipulationModes.TranslateX;
                        e.Handled = true;
                        PreviewBehavior.EnsureHidePreview();
                    }
                    else
                        PreviewBehavior.StartTimer();
                }
            }
        }

        private static void EnsureHidePreview()
        {
            if (!PreviewBehavior._isShowingPreview)
                return;
            StopTimer();
            PreviewBehavior._lastShownTime = DateTime.Now;
            HidePreview();
            SetHoveredElement(null);
            PreviewBehavior._isShowingPreview = false;

            //Touch.FrameReported -= (new TouchFrameEventHandler(this.Touch_FrameReported));
            //PageBase currentPage = FramePageUtils.CurrentPage;
            //if (currentPage != null)
            //    currentPage.SupportedOrientations = this._savedSupportedOrientation;
            //EventAggregator.Current.Publish(new PreviewCompletedEvent());
        }

        private static void StartTimer()
        {
            if (_timer.IsEnabled)
                return;
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private static void StopTimer()
        {
            if (!_timer.IsEnabled)
                return;
            _timer.Stop();
            _timer.Tick -= _timer_Tick;
        }

        private static void _timer_Tick(object sender, object e)
        {
            if (PreviewBehavior._isShowingPreview)
            {
                if ((DateTime.Now - _lastTouchFrameDate).TotalMilliseconds < 750.0)
                    return;
                EnsureHidePreview();
            }
            else
            {
                //if (string.IsNullOrEmpty(/*PreviewUri*/PreviewBehavior.GetPreviewUri(PreviewBehavior.AssociatedObject).AbsoluteUri))
                //    return;
                PreviewBehavior._isShowingPreview = true;

                //Touch.FrameReported += (new TouchFrameEventHandler(this.Touch_FrameReported));
                //PageBase currentPage = FramePageUtils.CurrentPage;
                //if (currentPage != null)
                //{
                //    this._savedSupportedOrientation = currentPage.SupportedOrientations;
                //    FramePageUtils.CurrentPage.SupportedOrientations = (currentPage.Orientation == PageOrientation.PortraitUp || currentPage.Orientation == PageOrientation.Portrait ? (SupportedPageOrientation)1 : (SupportedPageOrientation)2);
                //}
                string previewUri = PreviewBehavior.GetPreviewUri(PreviewBehavior.AssociatedObject).AbsoluteUri;
                Image associatedObject = AssociatedObject as Image;
                BitmapImage originalImage = (associatedObject != null ? associatedObject.Source : null) as BitmapImage;
                int topOffset = PreviewBehavior.DEFAULT_TOP_OFFSET;//TopOffset;
                ShowPreview(previewUri, originalImage, topOffset);
                SetHoveredElement(AssociatedObject);
            }
        }

        private static void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            Image image = (Image)sender;

            image.PointerPressed -= PreviewBehavior.AssociatedObject_PointerPressed;
            //image.PointerEntered -= PreviewBehavior.AssociatedObject_PointerEntered;
            image.PointerCaptureLost -= PreviewBehavior.AssociatedObject_PointerExited;
            image.PointerExited -= PreviewBehavior.AssociatedObject_PointerExited;
            //image.PointerReleased -= PreviewBehavior.AssociatedObject_PointerReleased;

            image.Holding -= Image_Holding;

            image.ManipulationStarted -= AssociatedObject_ManipulationStarted;
            image.ManipulationDelta -= AssociatedObject_ManipulationDelta;
            image.ManipulationCompleted -= AssociatedObject_ManipulationCompleted;
        }

        

        private static void Image_Holding(object sender, HoldingRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.HoldingState);

            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {


                
                _lastTouchFrameDate = DateTime.Now;
                PreviewBehavior.AssociatedObject = sender as Image;

                if (PreviewBehavior._isShowingPreview)
                {
                    (sender as Image).ManipulationMode = ManipulationModes.TranslateY | ManipulationModes.TranslateX;
                    e.Handled = true;
                    PreviewBehavior.EnsureHidePreview();
                }
                else
                    PreviewBehavior.StartTimer();

                e.Handled = true;
            }
        }
        #endregion

        private static void ShowPreview(string previewUri, BitmapImage originalImage = null, int topOffset = 0)
        {
            if (_ucPreview == null)
            {
                _ucPreview = new PreviewImageUC();
                _loader = new PopUpService() { IsHitTestVisible = false };
                _loader.AnimationTypeChild = PopUpService.AnimationTypes.None;
                _loader.Child = _ucPreview;
                _loader.Show();
            }
            _ucPreview.SetImageUri(previewUri, originalImage);
            _ucPreview.imagePreview.Margin = (new Thickness(0.0, (double)topOffset, 0.0, 0.0));
        }

        private static void HidePreview()
        {
            if (_ucPreview == null)
                return;
            _loader.Hide();
            _loader = null;
            _ucPreview = null;
        }

        private static void SetHoveredElement(FrameworkElement newHoveredElement)
        {
            if (_hoveredElement == newHoveredElement)
                return;
            var animInfoList = new List<AnimationUtils.AnimationInfo>();
            if (_hoveredElement != null)
                animInfoList.AddRange(PreviewBehavior.CreateAnimations(_hoveredElement, false));
            _hoveredElement = newHoveredElement;
            if (_hoveredElement != null)
                animInfoList.AddRange(PreviewBehavior.CreateAnimations(_hoveredElement, true));
            AnimationUtils.AnimateSeveral(animInfoList, new int?(0), null);
        }

        private static List<AnimationUtils.AnimationInfo> CreateAnimations(FrameworkElement element, bool push)
        {
            if (!(((UIElement)element).RenderTransform is ScaleTransform))
            {
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.CenterX = (element.ActualWidth / 2.0);
                scaleTransform.CenterY = (element.ActualHeight / 2.0);
                ((UIElement)element).RenderTransform = ((Transform)scaleTransform);
            }
            var animationInfoList = new List<AnimationUtils.AnimationInfo>();
            var animationInfo1 = new AnimationUtils.AnimationInfo();
            animationInfo1.duration = PreviewBehavior.PUSH_ANIMATION_DURATION;
            animationInfo1.easing = PreviewBehavior.PUSH_ANIMATION_EASING;
            double scaleX = (((UIElement)element).RenderTransform as ScaleTransform).ScaleX;
            animationInfo1.from = scaleX;
            double num1 = push ? PreviewBehavior.PUSH_SCALE : 1.0;
            animationInfo1.to = num1;
            Transform renderTransform1 = ((UIElement)element).RenderTransform;
            animationInfo1.target = (DependencyObject)renderTransform1;
            // ISSUE: variable of the null type
            animationInfo1.propertyPath = "ScaleX";
            animationInfoList.Add(animationInfo1);
            var animationInfo2 = new AnimationUtils.AnimationInfo();
            animationInfo2.duration = PreviewBehavior.PUSH_ANIMATION_DURATION;
            animationInfo2.easing = PreviewBehavior.PUSH_ANIMATION_EASING;
            double scaleY = (((UIElement)element).RenderTransform as ScaleTransform).ScaleY;
            animationInfo2.from = scaleY;
            double num2 = push ? PreviewBehavior.PUSH_SCALE : 1.0;
            animationInfo2.to = num2;
            Transform renderTransform2 = ((UIElement)element).RenderTransform;
            animationInfo2.target = (DependencyObject)renderTransform2;
            // ISSUE: variable of the null type
            animationInfo2.propertyPath = "ScaleY";
            animationInfoList.Add(animationInfo2);
            return animationInfoList;
        }
    }
}
