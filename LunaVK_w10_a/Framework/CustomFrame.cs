using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.ViewManagement;
using LunaVK.UC;
using LunaVK.Library;
using Windows.UI.Core;
using Microsoft.Toolkit.Uwp.UI.Controls;
using LunaVK.Core.Utils;
using LunaVK.Common;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;

namespace LunaVK.Framework
{
    /// <summary>
    /// Отображает экземпляры Page, поддерживает переходы на новые страницы и запоминает
    /// историю переходов для обеспечения навигации вперед и назад.
    /// </summary>
    public sealed class CustomFrame : Frame
    {
        public bool SupressTransition = true;
        public bool _shouldResetStack = false;
//        public bool _shouldDeleteLastPageFromStack = false;


        public static CustomFrame Instance;

        /// <summary>
        /// Данное устройство телефон?
        /// </summary>
        public bool IsDevicePhone { get; private set; }

        private DispatcherTimer _localTimer;

        private ApplicationView applicationView = ApplicationView.GetForCurrentView();

        private double _navBarHeight = 0;

        //private DragControlsHelper dragControlsHelper;

        public CustomFrame()
        {
            this.DefaultStyleKey = typeof(CustomFrame);
            var information = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
            this.IsDevicePhone = information.OperatingSystem.Contains("Phone");

#if WINDOWS_PHONE_APP || WINDOWS_UWP
            if(this.IsDevicePhone)
            {
                //                this.HookEvents();
                //
                //
                this._localTimer = new DispatcherTimer();
                this._localTimer.Interval = TimeSpan.FromSeconds(1);
                this._localTimer.Tick += this._localTimer_Tick;
                //
                this.Loaded += CustomFrame_Loaded;
            }
#endif
        }
        /*
        private void UpdateStatusBar()
        {
#if WINDOWS_UWP
            //this allows nav bar and status bar to overlay the app
            var view = ApplicationView.GetForCurrentView();

            view.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            view.ExitFullScreenMode();
#endif
        }
        */
        /*
        public void HookEvents()
        {
            applicationView.VisibleBoundsChanged += this.App_VisibleBoundsChanged;
            this.Loaded += CustomFrame_Loaded;//bugfix: сразу меняем оффсет
            Window.Current.VisibilityChanged += this.Current_VisibilityChanged;

            this._localTimer = new DispatcherTimer();
            this._localTimer.Interval = TimeSpan.FromSeconds(10);
            this._localTimer.Tick += this._localTimer_Tick;
        }

        public void UnHookEvents()
        {
            applicationView.VisibleBoundsChanged -= this.App_VisibleBoundsChanged;
            Window.Current.VisibilityChanged -= this.Current_VisibilityChanged;
        }
         */
        private void _localTimer_Tick(object sender, object e)
        {
            DispatcherTimer timer = sender as DispatcherTimer;
            timer.Stop();


            //
            //
            double BottomOffset = 0, LeftOffset = 0, RightOffset = 0;

            if (this.CurrentOrientation == ApplicationViewOrientation.Portrait)
            {
                BottomOffset = this._navBarHeight;
            }
            else
            {
                double l = applicationView.VisibleBounds.Left;
                if (l == 0) // Горизонтальная ориентаци, навбар справа
                {
                    RightOffset = this._navBarHeight;
                }
                else // Горизонтальная ориентаци, навбар слева
                {
                    LeftOffset = this._navBarHeight;
                }
            }

            this.Margin = new Thickness(LeftOffset, 0, RightOffset, BottomOffset);
        }
       
        private void CustomFrame_Loaded(object sender, RoutedEventArgs e)
        {
            ContactsManager.Instance.EnsureInSyncAsync();
            ContactsManager.Instance.Sync();

            //            this.App_VisibleBoundsChanged(applicationView, null);
            //this._localTimer.Start();
            this.Loaded -= CustomFrame_Loaded;

            double h = base.ActualHeight;
            double w = base.ActualWidth;

            double r = applicationView.VisibleBounds.Right;
            double b = applicationView.VisibleBounds.Bottom;
            double l = applicationView.VisibleBounds.Left;

            double BottomOffset = 0, LeftOffset = 0, RightOffset = 0;

            if (this.CurrentOrientation == ApplicationViewOrientation.Portrait)
            {
                BottomOffset = h - b;
            }
            else
            {
                if (l == 0) // Горизонтальная ориентаци, навбар справа
                {
                    RightOffset = w - r;
                }
                else // Горизонтальная ориентаци, навбар слева
                {
                    LeftOffset = l;
                }
            }

            if (BottomOffset < 0 || BottomOffset > 60)
                BottomOffset = 0;
            if (RightOffset < 0 || RightOffset > 60)
                RightOffset = 0;
            if (LeftOffset < 0 || LeftOffset > 60)
                LeftOffset = 0;

            if(BottomOffset>0 || RightOffset>0 || LeftOffset>0)
            {
                if(BottomOffset>0)
                    this._navBarHeight = BottomOffset;
                if (RightOffset > 0)
                    this._navBarHeight = RightOffset;
                if (LeftOffset > 0)
                    this._navBarHeight = LeftOffset;
                applicationView.VisibleBoundsChanged += this.App_VisibleBoundsChanged;
                Window.Current.VisibilityChanged += this.Current_VisibilityChanged;

                this.Margin = new Thickness(LeftOffset, 0, RightOffset, BottomOffset);
            }
        }

        private void Current_VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            this._localTimer.Start();
            if(e.Visible)
            {
                /*
                double BottomOffset = 0, LeftOffset = 0, RightOffset = 0;

                if (this.CurrentOrientation == ApplicationViewOrientation.Portrait)
                {
                    BottomOffset = this._navBarHeight;
                }
                else
                {
                    double l = applicationView.VisibleBounds.Left;
                    if (l == 0) // Горизонтальная ориентаци, навбар справа
                    {
                        RightOffset = this._navBarHeight;
                    }
                    else // Горизонтальная ориентаци, навбар слева
                    {
                        LeftOffset = this._navBarHeight;
                    }
                }

                this.Margin = new Thickness(LeftOffset, 0, RightOffset, BottomOffset);
                */
            }
        }

#if WINDOWS_PHONE_APP || WINDOWS_UWP
        private void App_VisibleBoundsChanged(ApplicationView v, object args)
        {
            double h = base.ActualHeight;
            double w = base.ActualWidth;

            double r = v.VisibleBounds.Right;
            double b = v.VisibleBounds.Bottom;
            double l = v.VisibleBounds.Left;

            double BottomOffset = 0, LeftOffset = 0, RightOffset = 0;

            if(this.CurrentOrientation == ApplicationViewOrientation.Portrait)
            {
                BottomOffset = h - b;
            }
            else
            {
                if(l==0) // Горизонтальная ориентаци, навбар справа
                {
                    RightOffset = w - r;
                }
                else // Горизонтальная ориентаци, навбар слева
                {
                    LeftOffset = l;
                }
            }

            if (BottomOffset < 0 || BottomOffset > 60)
                BottomOffset = 0;
            if (RightOffset < 0 || RightOffset > 60)
                RightOffset = 0;
            if (LeftOffset < 0 || LeftOffset > 60)
                LeftOffset = 0;

            //Logger.Instance.Info("VisibleBoundsChanged: ah{0} b{1} ob{2}", (int)base.ActualHeight, (int)b, (int)BottomOffset);
            this.Margin = new Thickness(LeftOffset, 0, RightOffset, BottomOffset);
        }
#endif
        

        private event EventHandler<MenuStates> _menuStateChanged;

        /// <summary>
        /// Событие изменения состаояния меню. Вызывается при подписывании.
        /// </summary>
        public event EventHandler<MenuStates> MenuStateChanged
        {
            add
            {
                this._menuStateChanged += value;
                value(this, this.MenuState);
            }
            remove
            {
                this._menuStateChanged -= value;
            }
        }

        
        private event EventHandler<bool> _menuOpenChanged;

        /// <summary>
        /// Событие открытия меню
        /// </summary>
        public event EventHandler<bool> MenuOpenChanged
        {
            add
            {
                this._menuOpenChanged += value;
                if(this.MySplitView!=null)
                    value(this, this.MySplitView.IsPaneOpen);
            }
            remove
            {
                this._menuOpenChanged -= value;
            }
        }

        public ApplicationViewOrientation CurrentOrientation;
        public EventHandler<ApplicationViewOrientation> OrientationChanged;
        
        /// <summary>
        /// Верхний контейнер для уведомлений
        /// </summary>
        public NotificationsPanel NotificationsPanel { get; private set; }

        /// <summary>
        /// Шапка
        /// Доступно после загрузки страницы
        /// </summary>
        public HeaderUC Header { get; private set; }

        private ContentPresenter _ContentPresenter { get; set; }

        public MenuStates MenuState { get; private set; }

        public MenuUC Menu { get; private set; }

        public Grid GridBack { get; private set; }

        public SplitView2 MySplitView { get; private set; }

        public Canvas OverlayDragPanel { get; private set; }
        /*
        public void DestroyDragElement(FrameworkElement element)
        {
            this.dragControlsHelper.Remove(element);
        }
        
        public void AddDragElement(FrameworkElement element)
        {
            this.dragControlsHelper = new DragControlsHelper(this.OverlayDragPanel);
            this.dragControlsHelper.Add(element);
        }
        */
        private VideoViewerElementUC _videoViewerElement;

        /// <summary>
        /// Создаём и возвращаем элемент
        /// </summary>
        public VideoViewerElementUC VideoViewerElement
        {
            get
            {
                if (this._videoViewerElement == null)
                {
                    this._videoViewerElement = new VideoViewerElementUC();
                    this.OverlayDragPanel.Children.Add(this._videoViewerElement);
                    
                    base.SizeChanged += CustomFrame_SizeChanged1;
                }

                return this._videoViewerElement;
            }
        }

        private void CustomFrame_SizeChanged1(object sender, SizeChangedEventArgs e)
        {
            var ttv = this._videoViewerElement.TransformToVisual(this.OverlayDragPanel);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            CompositeTransform compositeTransform = this._videoViewerElement.RenderTransform as CompositeTransform;
            double diff = e.NewSize.Width - (screenCoords.X + (this._videoViewerElement.ActualWidth * compositeTransform.ScaleX));
            if (diff < 0)
            {
                (this._videoViewerElement.RenderTransform as CompositeTransform).TranslateX += diff;
            }
        }

        public void DestroyVideoElement()
        {
            base.SizeChanged -= CustomFrame_SizeChanged1;
            this.OverlayDragPanel.Children.Remove(this._videoViewerElement);
            this._videoViewerElement = null;
        }

        /// <summary>
        /// Это сетка поверх всего содержимого для всплывающих элементов.
        /// Для просмотра картинок и попап сервиса
        /// </summary>
        public Grid OverlayGrid { get; private set; }

        public bool IsMenuOpened
        {
            get { return this.MySplitView.IsPaneOpen; }
        }

#if !WINDOWS_PHONE_APP || WINDOWS_UWP
//        public MediaElement MusicPlayer { get; private set; }
#endif
        
        

        public async void ShowStatusBar(bool status)
        {
#if WINDOWS_PHONE_APP
            Windows.UI.ViewManagement.StatusBar bar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            if (status)
                await bar.ShowAsync();
            else
                await bar.HideAsync();
#endif
#if WINDOWS_UWP
            
            if(this.IsDevicePhone)
            {
                var view = applicationView;

                if(view.Orientation == ApplicationViewOrientation.Landscape)
                {
                    view.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
                }
                else
                {
                    view.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
                }

                if (status)
                {
                    if (view.IsFullScreenMode && !this.ForceHideStatus)
                        view.ExitFullScreenMode();
                }
                else
                {
                    if (!view.IsFullScreenMode)
                        view.TryEnterFullScreenMode();//this hides nav bar and status bar
                }
            }
            
#endif
        }

        public bool IsBarVisible
        {
            get
            {
#if WINDOWS_PHONE_APP || WINDOWS_UWP
                if(this.IsDevicePhone)
                {
                    StatusBar bar = StatusBar.GetForCurrentView();
                    return bar.OccludedRect.Height > 0;
                }
                return false;
#else
                return false;
#endif
            }
        }

        public void OpenCloseMenu(bool? open = null)
        {
            if (this.MySplitView == null)
                return;

            if (!open.HasValue)
                open = !this.MySplitView.IsPaneOpen;
            else
            {
                if (open == false && base.ActualWidth > 1200)
                    return;
            }

            this.MySplitView.IsPaneOpen = open.Value;
        }

        public BottomMenuUC BottomMenu { get; private set; }

        public BottomPlayerUC BottomPlayer { get; private set; }


        /// <summary>
        /// Вызывается при построении макета.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.MySplitView = GetTemplateChild("MySplitView") as SplitView2;
//            this.Menu = new MenuUC();
//            this.MySplitView.Pane = this.Menu;
            this.Menu = GetTemplateChild("Menu") as MenuUC;

            this.GridBack = GetTemplateChild("GridBack") as Grid;
            this._ContentPresenter = GetTemplateChild("_ContentPresenter") as ContentPresenter;
            this.Header = GetTemplateChild("Header") as HeaderUC;
            this.NotificationsPanel = GetTemplateChild("NotificationsPanel") as NotificationsPanel;
            this.OverlayGrid = GetTemplateChild("OverlayPanel") as Grid;
            this.OverlayDragPanel = GetTemplateChild("OverlayDragPanel") as Canvas;
            this.BottomPlayer = GetTemplateChild("_bottomPlayer") as BottomPlayerUC;

            //this.BottomMenu = new BottomMenuUC();
            //this.GridBack.Children.Add(this.BottomMenu);


            this.MySplitView.PaneOpening += MySplitView_PaneOpening;
            this.MySplitView.PaneClosing += MySplitView_PaneClosing;


#if !WINDOWS_PHONE_APP || WINDOWS_UWP
            //            this.MusicPlayer = new MediaElement() { AudioCategory = AudioCategory.BackgroundCapableMedia, AutoPlay = false };
            //            this.GridBack.Children.Add(this.MusicPlayer);
#endif

            base.Navigated += (s, e) =>
            {

                if (this._shouldResetStack)
                {
                    NavigatorImpl.Instance.ClearBackStack();
                    this._shouldResetStack = false;
                }

                //
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = /*this._shouldResetStack == true*/base.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
                //

//                if (this._shouldDeleteLastPageFromStack)
//                {
//                    (Window.Current.Content as Frame).BackStack.RemoveAt((Window.Current.Content as Frame).BackStackDepth-1);
//                    this._shouldDeleteLastPageFromStack = false;
//                }
                
                this.Header.OptionsMenu.Clear();
                this.Header.HideSandwitchButton = false;
                this.Header.TitleOption = false;
            };

            base.SizeChanged += CustomFrame_SizeChanged;
        }

        

        private void MySplitView_PaneClosing(SplitView2 sender, SplitViewPaneClosingEventArgs args)
        {
            if(this.Page!=null)
                this.Page.BottomAppBar = this._savedPageAppBar;
            this._menuOpenChanged?.Invoke(sender, false);
        }

        private void MySplitView_PaneOpening(SplitView2 sender, object args)
        {
            if (this.Page != null)
            {
                this._savedPageAppBar = this.Page.BottomAppBar;
                this.Page.BottomAppBar = null;
            }
            this._menuOpenChanged?.Invoke(sender, true);
        }

        private AppBar _savedPageAppBar;

        private Page Page
        {
            get { return base.Content as Page; }
        }

        public void ClearSavedPageAppBar()
        {
            this._savedPageAppBar = null;
            this.Page.BottomAppBar = null;
        }

        private void MySplitView_PaneOpenChanged(object sender, bool e)
        {
            if(e==true)
            {
                this._savedPageAppBar = this.Page.BottomAppBar;
                this.Page.BottomAppBar = null;
            }
            else
                this.Page.BottomAppBar = this._savedPageAppBar;
            this._menuOpenChanged?.Invoke(sender, e);
        }

        public bool ForceHideStatus;

        private void CustomFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplicationView view = applicationView;

            if (this.OrientationChanged != null)
            {
                if (this.CurrentOrientation != view.Orientation)
                    this.OrientationChanged(sender, view.Orientation);
            }
            
            this.CurrentOrientation = view.Orientation;
            
            if (this.CurrentOrientation == ApplicationViewOrientation.Portrait)
            {
                if (this.ForceHideStatus == false)
                    this.ShowStatusBar(true);
            }
            else
                this.ShowStatusBar(false);
                
            if(this.IsDevicePhone)
            {
                PublishMenuState(this.CurrentOrientation == ApplicationViewOrientation.Portrait ? MenuStates.StateMenuCollapsedContentStretch : MenuStates.StateMenuNarrowContentStretch);
            }
            else
            {
                if (e.NewSize.Width > 1200)
                {
                    this.PublishMenuState(MenuStates.StateMenuFixedContentFixed);
                }
                else if (e.NewSize.Width > (double)Application.Current.Resources["WideMinWindowWidth"])
                {
                    PublishMenuState(MenuStates.StateMenuNarrowContentStretch);
                }
                else
                {
                    PublishMenuState(MenuStates.StateMenuCollapsedContentStretch);
                }
            }
        }

        private bool _suppressed;
        /// <summary>
        /// Заблокировать меню?
        /// </summary>
        /// <param name="status"></param>
        public bool SuppressMenu
        {
            get
            {
                return this._suppressed;
            }
            set
            {
                this._suppressed = value;
                this.MySplitView.ActivateSwipe(!value);
                this.MySplitView.IsPaneLocked = value;
                this.Header.HideSandwitchButton = value;
            }
        }

        private void PublishMenuState(MenuStates newState)
        {
            if (this.MenuState == newState)
                return;

            if (this._menuStateChanged != null)
                this._menuStateChanged(this, newState);

            this.MenuState = newState;
//            this.UpdateMargin(this.MenuState);
#if DEBUG
            System.Diagnostics.Debug.WriteLine("MenuState: " + newState.ToString());
#endif
        }

        public enum MenuStates : byte
        {
            NotInitialized,

            /// <summary>
            /// Меню свёрнуто, может быть развернуто.
            /// Содержимое по ширине экрана . ( 635px
            /// </summary>
            StateMenuCollapsedContentStretch,

            /// <summary>
            /// Меню компактное и может быть развёрнуто.
            /// Содержимое по ширине экрана. > 635px
            /// </summary>
            StateMenuNarrowContentStretch,

            /// <summary>
            /// Меню открыто.
            /// А содержимое шириной 900px. >1200px
            /// </summary>
            StateMenuFixedContentFixed,
        }
        
    }
}
