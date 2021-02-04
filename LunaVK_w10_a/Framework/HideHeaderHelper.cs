using LunaVK.Common;
using LunaVK.Core.Utils;
using LunaVK.UC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace LunaVK.Framework
{
    public class HideHeaderHelper
    {
        //private const double SYSTEM_TRAY_HEIGHT = 32.0;
        //private const double FRESH_NEWS_RESET_SCROLL = 100.0;
        //private const int FRESH_NEWS_EXTRA_Y_OFFSET = 8;
        //private const int EXTRA_DELTA_WHEN_SCROLLING_UPWARDS = 530;
        private readonly FrameworkElement _ucHeader;
        private readonly NewsfeedHeaderUC _ucHeaderNews;

        private readonly ScrollViewer _viewportControl;
        private readonly TranslateTransform _translateHeader;
        private readonly TranslateTransform _translateFreshNews;
        private readonly double _minOffsetHeader;
        private readonly double _maxOffsetHeader;
        private readonly double _minOffsetFreshNews;
        private double? _previousScrollPosition;
        private bool _isAnimating;
        private double _initialScrollPositionAfterDirectionChange;
        private bool _directionDownwards;
        private FreshNewsState _freshNewsState;
        private bool _isFreshNewsShowed;

        DispatcherTimer timerIdle = new DispatcherTimer();

        ManipulationState state;
        
        public HideHeaderHelper(FrameworkElement ucHeader, ScrollViewer viewportControl, NewsfeedHeaderUC borderFreshNews = null)
        {
            this._ucHeader = ucHeader;
            this._ucHeaderNews = borderFreshNews;

            this._viewportControl = viewportControl;

            TranslateTransform renderTransform;
            if (Core.Settings.HideHeader)
            {
                renderTransform = this._ucHeader.RenderTransform as TranslateTransform;
                if (renderTransform == null)
                {
                    renderTransform = new TranslateTransform();
                    this._ucHeader.RenderTransform = renderTransform;
                }
            }
            else
            {
                renderTransform = new TranslateTransform();
            }

            this._translateHeader = renderTransform;
            
            //this._minOffsetHeader = (-this._ucHeader.Height) + 32.0;
            //
            this._minOffsetHeader = -83;//this._minOffsetHeader = (-this._ucHeader.Height);
                                   //            if (!VKClient.Common.Library.AppGlobalStateManager.Current.GlobalState.HideSystemTray)
                                   //                this._minOffsetHeader += 32;
                                   //
            this._maxOffsetHeader = 0.0;
            this._minOffsetFreshNews = 0.0;

            if(borderFreshNews!=null)
            {
                this._ucHeaderNews.Visibility = Visibility.Collapsed;
                this._translateFreshNews = this._ucHeaderNews.RenderTransform as TranslateTransform;
                this._translateFreshNews.Y = this._minOffsetFreshNews;
            }
            
//            this.Activate(true);//this._viewportControl.ViewportChanged += this.ViewportControl_OnViewportControlChanged;
            //this._viewportControl.ManipulationStateChanged += this.ViewportControl_OnManipulationStateChanged;




            timerIdle.Tick += Timer_Tick;
            timerIdle.Interval = TimeSpan.FromSeconds(1);
        }
        
        public void Activate(bool status)
        {
            if(status)
                this._viewportControl.ViewChanged += this.ViewportControl_OnViewportControlChanged;
            else
                this._viewportControl.ViewChanged -= this.ViewportControl_OnViewportControlChanged;
        }

        private void Timer_Tick(object sender, object e)
        {
            state = ManipulationState.Idle;
            this.ViewportControl_OnManipulationStateChanged();
            (sender as DispatcherTimer).Stop();
        }

        private double GetMaxFreshNewsTranslateY
        {
            get
            {
                return 75.0 + 8.0;//this._ucHeader.ActualHeight + 8.0;
            }
        }

        private void MakeIdleState()
        {
            if (timerIdle.IsEnabled)
                timerIdle.Stop();
            timerIdle.Start();
        }

        private void MakeScrollState()
        {
            if (this.state == ManipulationState.Idle)
            {
                if (timerIdle.IsEnabled)
                    timerIdle.Stop();

                this.state = ManipulationState.Scroll;
                this.ViewportControl_OnManipulationStateChanged();
            }
        }


        private void ViewportControl_OnManipulationStateChanged(/*object sender, ManipulationStateChangedEventArgs e*/)
        {
            if (this._isAnimating || this.state != ManipulationState.Idle)
                return;
            this.UpdateSystemTrayAndAppBarIfNeeded();
            this._isAnimating = true;
            if (this.ShouldHide)
            {
                double Y = this._viewportControl.VerticalOffset;
                this.Hide(Y < this._ucHeader.ActualHeight);
            }
            else
                this.Show(true);
        }
        
        public void Show(bool showFreshNews = false)
        {
            this.UpdateSystemTrayAndAppBarIfNeeded();
            this._isAnimating = true;
            EasingFunctionBase ieasingFunction = new QuadraticEase();
            object yproperty = TranslateTransform.YProperty;
            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>()
            {
                new AnimationUtils.AnimationInfo()
                {
                    target = this._translateHeader,
                    propertyPath = "Y",
                    from = this._translateHeader.Y,
                    to = this._maxOffsetHeader,
                    easing = ieasingFunction,
                    duration = 150
                }
            };

            if (showFreshNews && this._isFreshNewsShowed)
            {
                animInfoList.Add(new AnimationUtils.AnimationInfo()
                {
                    target = this._translateFreshNews,
                    propertyPath = "Y",
                    from = this._translateFreshNews.Y,
                    to = 75 + 8,//((FrameworkElement)this._ucHeader).Height + 8.0,
                    easing = ieasingFunction,
                    duration = 150
                });
            }

            AnimationUtils.AnimateSeveral(animInfoList, new int?(0), (Action)(() =>
            {
                this.UpdateExtraCrop();
                this.UpdateSystemTrayAndAppBarIfNeeded();
                this._isAnimating = false;
                if (this._isFreshNewsShowed)
                    return;
                this.ShowFreshNews();
            }));
        }

        private void Hide(bool hideFreshNews = false)
        {
            this.UpdateSystemTrayAndAppBarIfNeeded();
            this._isAnimating = true;
            EasingFunctionBase ieasingFunction = new QuadraticEase();
            
            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>()
            {
                new AnimationUtils.AnimationInfo()
                {
                    target = this._translateHeader,
                    propertyPath = "Y",
                    from = this._translateHeader.Y,
                    to = this._minOffsetHeader,
                    easing = ieasingFunction,
                    duration = 150
                }
            };

            if (hideFreshNews && this._isFreshNewsShowed)
            {
                animInfoList.Add( new AnimationUtils.AnimationInfo()
                {
                    target = this._translateFreshNews,
                    propertyPath = "Y",
                    from = this._translateFreshNews.Y,
                    to = this._minOffsetFreshNews,
                    easing = ieasingFunction,
                    duration = 150
                });
            }
            AnimationUtils.AnimateSeveral(animInfoList, null, () =>
            {
                this.UpdateExtraCrop();
                this.UpdateSystemTrayAndAppBarIfNeeded();
                this._isAnimating = false;
            });
        }

        public void UpdateFreshNewsState(FreshNewsState state)
        {
            this._freshNewsState = state;
            if (this._freshNewsState == FreshNewsState.NoNews)
                this.HideFreshNews();
            else
                this.ShowFreshNews();
        }

        public void ShowFreshNews()
        {
            if (this._freshNewsState == FreshNewsState.NoNews || this._isAnimating /*|| this._isFreshNewsShowed*/)
                return;

            this._ucHeaderNews.Visibility = Visibility.Visible;

            this._isAnimating = true;
            
            
            
            BackEase backEase = new BackEase() { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 };
            Action completed = (() =>
            {
                this._isAnimating = false;
                this._isFreshNewsShowed = true;
            });
            this._translateFreshNews.Animate(this._translateFreshNews.Y, this.GetMaxFreshNewsTranslateY, "Y", 350, 0, backEase, completed, false);
        }

        private void HideFreshNews()
        {
            if (this._isAnimating || !this._isFreshNewsShowed /*|| this._ucHeader.IsLoadingFreshNews*/)
                return;

            this._isAnimating = true;
            
            double minOffsetFreshNews = this._minOffsetFreshNews;
            QuadraticEase quadraticEase = new QuadraticEase() { EasingMode = EasingMode.EaseOut };
            Action completed = (() =>
            {
                this._isAnimating = false;
                this._isFreshNewsShowed = false;
                this._ucHeaderNews.Visibility = Visibility.Collapsed;
            });
            this._translateFreshNews.Animate(this._translateFreshNews.Y, minOffsetFreshNews, "Y", 250, 0, quadraticEase, completed, false);
        }

        private void UpdateSystemTrayAndAppBarIfNeeded()
        {
            //if (VKClient.Common.Library.AppGlobalStateManager.Current.GlobalState.HideSystemTray == false)
            //    return;
            //if (this._previousScrollPosition >= 0)
            //    Microsoft.Phone.Shell.SystemTray.IsVisible = !this.ShouldHide();
        }

        private bool ShouldHide
        {
            get
            {
                double Y = this._viewportControl.VerticalOffset;

                if (Y < this._ucHeader.ActualHeight/* - 32.0*/)
                    return false;
                return this._translateHeader.Y < this._maxOffsetHeader;
            }
        }

        private void ViewportControl_OnViewportControlChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer viewport = sender as ScrollViewer;
            //Rect viewport = this._viewportControl.Viewport;

            double y = viewport.VerticalOffset;//viewport.Y;
            if (this.state != ManipulationState.Idle && !this._isAnimating)
            {
                if (!this._previousScrollPosition.HasValue)
                {
                    this._previousScrollPosition = new double?(y);
                    this._initialScrollPositionAfterDirectionChange = y;
                    this._directionDownwards = true;
                }
                double num1 = y;
                double? previousScrollPosition = this._previousScrollPosition;
                double valueOrDefault1 = previousScrollPosition.GetValueOrDefault();
                if ((num1 < valueOrDefault1 ? (previousScrollPosition.HasValue ? 1 : 0) : 0) != 0)
                {
                    if (this._directionDownwards)
                        this._initialScrollPositionAfterDirectionChange = y;
                    this._directionDownwards = false;
                }

                previousScrollPosition = this._previousScrollPosition;
                double valueOrDefault2 = previousScrollPosition.GetValueOrDefault();
                if ((y > valueOrDefault2 ? (previousScrollPosition.HasValue ? 1 : 0) : 0) != 0)
                {
                    if (!this._directionDownwards)
                        this._initialScrollPositionAfterDirectionChange = y;
                    this._directionDownwards = true;
                }
                if (!this._directionDownwards && y < 100.0 && this._freshNewsState != FreshNewsState.Reload)
                    this.UpdateFreshNewsState(FreshNewsState.NoNews);
                if (this._directionDownwards && y >= this._initialScrollPositionAfterDirectionChange && y > 0.0)
                {
                    double num3 = this._translateHeader.Y - (y - this._previousScrollPosition.Value);
                    if (num3 < this._minOffsetHeader)
                        num3 = this._minOffsetHeader;
                    int num4 = this._translateHeader.Y != num3 ? 1 : 0;
                    this._translateHeader.Y = num3;
                    if (num4 != 0 && num3 == this._minOffsetHeader)
                        this.HideFreshNews();
                    this.UpdateExtraCrop();
                }
                else if (!this._directionDownwards && y <= Math.Max(/*(this._ucHeader).Height*/75, this._initialScrollPositionAfterDirectionChange - 530.0))
                {
                    double num3 = this._translateHeader.Y - (y - this._previousScrollPosition.Value);
                    bool flag = false;
                    if (num3 > this._maxOffsetHeader)
                    {
                        num3 = this._maxOffsetHeader;
                        flag = true;
                    }
                    this._translateHeader.Y = num3;
                    if (flag)
                        this.ShowFreshNews();
                    this.UpdateExtraCrop();
                }
            }
            if (y == 0.0)
                this.Show(false);
            this._previousScrollPosition = new double?(y);
            this.UpdateSystemTrayAndAppBarIfNeeded();

            this.MakeScrollState();
            this.MakeIdleState();
        }

        private void UpdateExtraCrop()
        {
            //AttachedProperties.SetExtraDeltaYCropWhenHidingImage(this._viewportControl, Math.Max(this._ucHeader.Height + this._translateHeader.Y, 32.0));
        }

        public void Reset()
        {
            this.Activate(false);//this._viewportControl.ViewChanged -= this.ViewportControl_OnViewportControlChanged;
            this.timerIdle.Stop();
            this._translateHeader.Y = 0;
        }
        
        public enum ManipulationState
        {
            Idle,
            Scroll,
        }
    }
}
