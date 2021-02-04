using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Core;

namespace LunaVK.Framework
{
    public class PopUpService
    {
        public PopUpService()
        {
            this.OverlayGrid = CustomFrame.Instance.OverlayGrid;
            (Window.Current.Content as Frame).Navigating += this.PopUpService_Navigating;
            
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.Handled == true)
                return;

            if(args.VirtualKey == Windows.System.VirtualKey.Escape)
            {
                this.Hide();
                args.Handled = true;
            }
        }

        private void PopUpService_Navigating(object sender, Windows.UI.Xaml.Navigation.NavigatingCancelEventArgs e)
        {
            this.Hide();
        }

        private AppViewBackButtonVisibility _previousBackButtonVisibility;
        
        private PopUpService.AnimationTypes _animationTypeOverlay = PopUpService.AnimationTypes.Fade;
        public PopUpService.AnimationTypes AnimationTypeChild = PopUpService.AnimationTypes.Slide;

        /// <summary>
        /// Это наш элемент
        /// </summary>
        public FrameworkElement Child { get; set; }

        public double VerticalOffset { get; set; }

        public double HorizontalOffset { get; set; }

        public bool IsOpen { get; private set; }

        /// <summary>
        /// Происходит перед скрытием
        /// </summary>
        public Action<Action> OnClosingAction { get; set; }

        /// <summary>
        /// Проиходит после выполнения анимации скрытия
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Происходит после проигрывания анимации
        /// </summary>
        public event EventHandler Opened;

        private AppBar _savedPageAppBar;

        /// <summary>
        /// Скрывать элемент при нажатии назад? По-умолчания - нет.
        /// </summary>
        public bool OverrideBackKey;

#if WINDOWS_PHONE_APP
        private void OnBackKeyPress(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            e.Handled = true;
            this.Hide();
        }
#endif
#if WINDOWS_UWP
        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            this.Hide();
        }
#endif

        public void Show()
        {
            if (this.OverrideBackKey)
            {
#if WINDOWS_PHONE_APP
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += OnBackKeyPress;
#endif
#if WINDOWS_UWP
                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
#endif
            }

            this._savedPageAppBar = this.Page.BottomAppBar;
            this.Page.BottomAppBar = null;
            this.IsOpen = true;
            this.InitializePopup();

            SystemNavigationManager navigationManager = SystemNavigationManager.GetForCurrentView();
            this._previousBackButtonVisibility = navigationManager.AppViewBackButtonVisibility;
            navigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private Page Page
        {
            get { return CustomFrame.Instance.Content as Page; }
        }

        /// <summary>
        /// Оверлай от фрайма - он выше всех
        /// </summary>
        public Grid OverlayGrid;


        /// <summary>
        /// Контейнер с нашим элементом
        /// </summary>
        public Grid PopupContainer { get; private set; }

        public Grid BackGroundGrid { get; private set; }

        public Brush BackgroundBrush = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));

        public bool IsHitTestVisible = true;

        private void InitializePopup()
        {
            this.PopupContainer = new Grid() { IsHitTestVisible = this.IsHitTestVisible };
            this.BackGroundGrid = new Grid() { IsHitTestVisible = this.IsHitTestVisible };

            if (this.BackgroundBrush != null)
            {
                this.BackGroundGrid.Background = this.BackgroundBrush;
                this.BackGroundGrid.Tapped += OverlayGrid_Tapped;
            }

            if (this.HorizontalOffset>0 || this.VerticalOffset>0)
                this.Child.Margin = new Thickness(this.HorizontalOffset, this.VerticalOffset, 0.0, 0.0);
            this.PopupContainer.Children.Add(this.Child);

            this.PopupContainer.Opacity = 0;
            this.BackGroundGrid.Opacity = 0;
            //
            this.Child.Loaded += this.Child_Loaded;
            this.Child.Unloaded += this.Child_Unloaded;
            //
            this.OverlayGrid.Children.Add(this.BackGroundGrid);
            this.OverlayGrid.Children.Add(this.PopupContainer);

            //this.RunShowStoryboard(this.PopupContainer, this.AnimationTypeChild);
            //this.RunShowStoryboard(this.BackGroundGrid, this._animationTypeOverlay, () =>
            //{
            //    this.Opened?.Invoke(this.Child, null);
            //});
        }

        private void Child_Unloaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= this.OnBackKeyPress;
#endif
#if WINDOWS_UWP
            SystemNavigationManager.GetForCurrentView().BackRequested -= this.OnBackRequested;
#endif
        }

        private void Child_Loaded(object sender, RoutedEventArgs e)
        {
            this.RunShowStoryboard(this.PopupContainer, this.AnimationTypeChild);
            this.RunShowStoryboard(this.BackGroundGrid, this._animationTypeOverlay, () =>
            {
                this.Opened?.Invoke(this.Child, null);
            });

            Window.Current.CoreWindow.KeyDown += this.CoreWindow_KeyDown;
        }

        void OverlayGrid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (sender != e.OriginalSource)
                return;
            this.Hide();
        }

        public void Hide()
        {
//            this.RunHideStoryboard(this.PopupContainer, this.AnimationTypeChild);
//            this.RunHideStoryboard(this.BackGroundGrid, this._animationTypeOverlay, this.HideStoryboardCompleted);

#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= this.OnBackKeyPress;
#endif
#if WINDOWS_UWP
            SystemNavigationManager.GetForCurrentView().BackRequested -= this.OnBackRequested;
#endif
//            this.Page.BottomAppBar = this._savedPageAppBar;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = this._previousBackButtonVisibility;



            Action hideAction = (() => 
            {
                this.RunHideStoryboard(this.PopupContainer, this.AnimationTypeChild);
                this.RunHideStoryboard(this.BackGroundGrid, this._animationTypeOverlay, this.HideStoryboardCompleted);
                this.Page.BottomAppBar = this._savedPageAppBar;
            });

            if (this.OnClosingAction != null)
                this.OnClosingAction(hideAction);
            else
                hideAction();

            (Window.Current.Content as Frame).Navigating -= this.PopUpService_Navigating;
            Window.Current.CoreWindow.KeyDown -= this.CoreWindow_KeyDown;
        }

        private void RunShowStoryboard(FrameworkElement element, PopUpService.AnimationTypes animation, Action completionCallback = null)
        {
            if (element == null)
            {
                completionCallback?.Invoke();
            }
            else
            {
                Storyboard storyboard = null;
                switch (animation)
                {
                    case PopUpService.AnimationTypes.Slide:
                        storyboard = XamlReader.Load("<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.Y)\"><EasingDoubleKeyFrame KeyTime=\"0\" Value=\"-50\"/><EasingDoubleKeyFrame KeyTime=\"0:0:0.35\" Value=\"0\"><EasingDoubleKeyFrame.EasingFunction><ExponentialEase EasingMode=\"EaseOut\" Exponent=\"6\"/></EasingDoubleKeyFrame.EasingFunction></EasingDoubleKeyFrame></DoubleAnimationUsingKeyFrames><DoubleAnimation Storyboard.TargetProperty=\"(UIElement.Opacity)\" From=\"0\" To=\"1\" Duration=\"0:0:0.350\"><DoubleAnimation.EasingFunction><ExponentialEase EasingMode=\"EaseOut\" Exponent=\"6\"/></DoubleAnimation.EasingFunction></DoubleAnimation></Storyboard>") as Storyboard;
                        element.RenderTransform = new TranslateTransform();
                        break;
                    case PopUpService.AnimationTypes.SlideInversed:
                        storyboard = XamlReader.Load("<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.Y)\"><SplineDoubleKeyFrame KeyTime=\"0\" Value=\"800\"/><SplineDoubleKeyFrame KeyTime=\"0:0:0.35\" Value=\"0\"><SplineDoubleKeyFrame.KeySpline><KeySpline><KeySpline.ControlPoint1><Point X=\"0.1\" Y=\"0.9\" /></KeySpline.ControlPoint1><KeySpline.ControlPoint2><Point X=\"0.2\" Y=\"1\" /></KeySpline.ControlPoint2></KeySpline></SplineDoubleKeyFrame.KeySpline></SplineDoubleKeyFrame></DoubleAnimationUsingKeyFrames><DoubleAnimation Storyboard.TargetProperty=\"(UIElement.Opacity)\" From=\"0\" To=\"1\" Duration=\"0\"/></Storyboard>") as Storyboard;
                        element.RenderTransform = new TranslateTransform();
                        break;
                    case PopUpService.AnimationTypes.SlideHorizontal:
                        storyboard = XamlReader.Load("<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.X)\"><EasingDoubleKeyFrame KeyTime=\"0\" Value=\"-150\"/><EasingDoubleKeyFrame KeyTime=\"0:0:0.35\" Value=\"0\"><EasingDoubleKeyFrame.EasingFunction><ExponentialEase EasingMode=\"EaseOut\" Exponent=\"6\"/></EasingDoubleKeyFrame.EasingFunction></EasingDoubleKeyFrame></DoubleAnimationUsingKeyFrames><DoubleAnimation Storyboard.TargetProperty=\"(UIElement.Opacity)\" From=\"0\" To=\"1\" Duration=\"0:0:0.350\" ><DoubleAnimation.EasingFunction><ExponentialEase EasingMode=\"EaseOut\" Exponent=\"6\"/></DoubleAnimation.EasingFunction></DoubleAnimation></Storyboard>") as Storyboard;
                        element.RenderTransform = new TranslateTransform();
                        break;
                    case PopUpService.AnimationTypes.Swivel:
                        storyboard = XamlReader.Load("<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.Projection).(PlaneProjection.RotationX)\"><EasingDoubleKeyFrame KeyTime=\"0\" Value=\"-45\"/><EasingDoubleKeyFrame KeyTime=\"0:0:0.35\" Value=\"0\"><EasingDoubleKeyFrame.EasingFunction><ExponentialEase EasingMode=\"EaseOut\" Exponent=\"6\"/></EasingDoubleKeyFrame.EasingFunction></EasingDoubleKeyFrame></DoubleAnimationUsingKeyFrames><DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.Opacity)\"><DiscreteDoubleKeyFrame KeyTime=\"0\" Value=\"1\" /></DoubleAnimationUsingKeyFrames></Storyboard>") as Storyboard;

                        PlaneProjection planeProjection = new PlaneProjection();
                        planeProjection.RotationX = -45.0;
                        planeProjection.CenterOfRotationX = element.ActualHeight / 2.0;
                        element.Projection = planeProjection;
                        break;
                    case PopUpService.AnimationTypes.Fade:
                        storyboard = XamlReader.Load("<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><DoubleAnimation Duration=\"0:0:0.267\" Storyboard.TargetProperty=\"(UIElement.Opacity)\" To=\"1\"/></Storyboard>") as Storyboard;
                        break;
                }
                if (storyboard != null)
                {
                    element.Opacity = 0.0;

                    storyboard.Completed += ((s, e) =>
                    {

                        completionCallback?.Invoke();
                    });
                    foreach (Timeline t in storyboard.Children)
                    {
                        Storyboard.SetTarget(t, element);
                    }
                    /*
                    IEnumerator<Timeline> enumerator = storyboard.Children.AsEnumerable < Timeline>();
                    try
                    {
                        while (((IEnumerator)enumerator).MoveNext())
                            Storyboard.SetTarget(enumerator.Current, (DependencyObject)element);
                    }
                    finally
                    {
                        if (enumerator != null)
                            ((IDisposable)enumerator).Dispose();
                    }*/
                    storyboard.Begin();
                }
                else
                {
                    element.Opacity = 1.0;

                    completionCallback?.Invoke();
                }
            }
        }

        private void RunHideStoryboard(FrameworkElement element, PopUpService.AnimationTypes animation, Action completionCallback = null)
        {
            if (element == null)
                return;
            Storyboard storyboard = null;
            switch (animation)
            {
                case PopUpService.AnimationTypes.Slide:
                    storyboard = XamlReader.Load("<Storyboard  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.Y)\"><EasingDoubleKeyFrame KeyTime=\"0\" Value=\"0\"/>        <EasingDoubleKeyFrame KeyTime=\"0:0:0.25\" Value=\"-50\">            <EasingDoubleKeyFrame.EasingFunction>                <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>            </EasingDoubleKeyFrame.EasingFunction>        </EasingDoubleKeyFrame>    </DoubleAnimationUsingKeyFrames>    <DoubleAnimation Storyboard.TargetProperty=\"(UIElement.Opacity)\" From=\"1\" To=\"0\" Duration=\"0:0:0.25\">        <DoubleAnimation.EasingFunction>            <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>        </DoubleAnimation.EasingFunction>    </DoubleAnimation></Storyboard>") as Storyboard;
                    //if(element.RenderTransform == null)
                    element.RenderTransform = new TranslateTransform();
                    break;
                case PopUpService.AnimationTypes.SlideInversed:
                    storyboard = XamlReader.Load("<Storyboard  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.Y)\">        <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"0\"/>        <EasingDoubleKeyFrame KeyTime=\"0:0:0.35\" Value=\"800\">            <EasingDoubleKeyFrame.EasingFunction>                <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>            </EasingDoubleKeyFrame.EasingFunction>        </EasingDoubleKeyFrame>    </DoubleAnimationUsingKeyFrames></Storyboard>") as Storyboard;
                    //if (element.RenderTransform == null)
                    element.RenderTransform = new TranslateTransform();
                    break;
                case PopUpService.AnimationTypes.SlideHorizontal:
                    storyboard = XamlReader.Load("<Storyboard  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.X)\">        <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"0\"/>        <EasingDoubleKeyFrame KeyTime=\"0:0:0.25\" Value=\"150\">            <EasingDoubleKeyFrame.EasingFunction>                <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>            </EasingDoubleKeyFrame.EasingFunction>        </EasingDoubleKeyFrame>    </DoubleAnimationUsingKeyFrames>    <DoubleAnimation Storyboard.TargetProperty=\"(UIElement.Opacity)\" From=\"1\" To=\"0\" Duration=\"0:0:0.25\">        <DoubleAnimation.EasingFunction>            <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>        </DoubleAnimation.EasingFunction>    </DoubleAnimation></Storyboard>") as Storyboard;
                    break;
                case PopUpService.AnimationTypes.Swivel:
                    storyboard = XamlReader.Load("<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.Projection).(PlaneProjection.RotationX)\">        <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"0\"/>        <EasingDoubleKeyFrame KeyTime=\"0:0:0.25\" Value=\"45\">            <EasingDoubleKeyFrame.EasingFunction>                <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>            </EasingDoubleKeyFrame.EasingFunction>        </EasingDoubleKeyFrame>    </DoubleAnimationUsingKeyFrames>    <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.Opacity)\">        <DiscreteDoubleKeyFrame KeyTime=\"0\" Value=\"1\" />        <DiscreteDoubleKeyFrame KeyTime=\"0:0:0.267\" Value=\"0\" />    </DoubleAnimationUsingKeyFrames></Storyboard>") as Storyboard;

                    PlaneProjection planeProjection = new PlaneProjection();
                    planeProjection.RotationX = 0.0;
                    planeProjection.CenterOfRotationX = element.ActualHeight / 2.0;
                    element.Projection = planeProjection;
                    break;
                case PopUpService.AnimationTypes.Fade:
                    storyboard = XamlReader.Load("<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><DoubleAnimation Duration=\"0:0:0.267\" Storyboard.TargetProperty=\"(UIElement.Opacity)\" To=\"0\"/></Storyboard>") as Storyboard;
                    break;
            }
            try
            {
                if (storyboard != null)
                {
                    storyboard.Completed += ((s, e) =>
                    {
                        completionCallback?.Invoke();
                    });
                    foreach (Timeline t in storyboard.Children)
                    {
                        Storyboard.SetTarget(t, element);
                    }

                    storyboard.Begin();
                }
                else
                {
                    if (completionCallback != null)
                        completionCallback();
                }
            }
            catch
            {
                if (completionCallback != null)
                    completionCallback();
            }
        }

        private void HideStoryboardCompleted()
        {
            this.IsOpen = false;

            this.PopupContainer.Children.Remove(this.Child);
            
            this.OverlayGrid.Children.Remove(this.BackGroundGrid);
            this.OverlayGrid.Children.Remove(this.PopupContainer);

            this.Child = null;
            this.BackGroundGrid = null;
            this.PopupContainer = null;

            this.OverlayGrid.Tapped -= OverlayGrid_Tapped;

            this.Closed?.Invoke(this, null);
        }

        public enum AnimationTypes
        {
            /// <summary>
            /// Появление сверху вниз
            /// </summary>
            Slide,

            /// <summary>
            /// Появление снизу вверх
            /// </summary>
            SlideInversed,

            /// <summary>
            /// Появление слева направо
            /// </summary>
            SlideHorizontal,

            /// <summary>
            /// Поворот проекции по оси Х
            /// </summary>
            Swivel,
            //SwivelHorizontal,

            /// <summary>
            /// Просто появление
            /// </summary>
            Fade,
            None,
        }
    }
}
