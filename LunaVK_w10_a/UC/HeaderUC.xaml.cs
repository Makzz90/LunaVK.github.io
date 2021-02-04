using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

using Windows.UI.ViewManagement;
using LunaVK.Library;
using LunaVK.Core.Utils;
using System.Collections.ObjectModel;
using LunaVK.Framework;
using LunaVK.Core.Library;
using LunaVK.ViewModels;
using LunaVK.Core;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.Foundation.Metadata;

namespace LunaVK.UC
{
    public sealed partial class HeaderUC : UserControl
    {        
        //private EasingFunctionBase _menuEasing;
        public ObservableCollection<OptionsMenuItem> OptionsMenu { get; set; }
        
        /// <summary>
        /// Полная высота шапки = её высота + высота статусбара
        /// </summary>
        public double HeaderHeight
        {
            get { return this.Offset.Height + this._headerGrid.ActualHeight; }
        }
        
        private bool _isVisible = true;

        /// <summary>
        /// Виден ли задний фон шапки?
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return this._isVisible;
            }
            set
            {
                if(this._isVisible != value)
                {
                    this._isVisible = value;
                    //VisualStateManager.GoToState(this, value ? "Visible" : "IntermediateFull", true);
                    this.root.IsHitTestVisible = value;//root0
                    this.TopBarInterBackground.Opacity = value ? 1.0 : 0;
                }
            }
        }

        public Grid BackGroundGrid
        {
            get { return this.TopBarInterBackground; }
        }

        public UIElement FullScreenBtn
        {
            get { return this._fullScreenBtn; }
        }
        /*
        public void ShowProgress(bool show)
        {
            this._progressBar.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }
        */

        public ProgressBar Progress
        {
            get { return this._progressBar; }
        }

        private bool _hideSandwitchButton;
        public bool HideSandwitchButton
        {
            get
            {
                return this._hideSandwitchButton;
            }
            set
            {
                if (value != this._hideSandwitchButton)
                {
                    this._hideSandwitchButton = value;
                    this.borderSandwich.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
                }
            }
        }

        public bool HideMoreOptions
        {
            get
            {
                return this._moreOptionsPanel.Visibility == Visibility.Collapsed;
            }
            set
            {
                this._moreOptionsPanel.Visibility = (!value).ToVisiblity();
            }
        }


        /*
        /// <summary>
        /// Установка текста шапки
        /// Надо делать после загрузки страницы
        /// </summary>
        /// <param name="text"></param>
        public void SetTitle(string text)
        {
            this._title.Text = text;
        }
        */
        /*
        public string Title
        {
            get { return this._title.Text; }
            set { this._title.Text = value; }
        }
        */

        public string Title
        {
            get { return this._ucTitle.Title; }
            set { this._ucTitle.Title = value; }
        }

        public string SubTitle
        {
            get { return this._ucTitle.SubTitle; }
            set { this._ucTitle.SubTitle = value; }
        }
        /*
        /// <summary>
        /// Происходит позже, чем загрузка страницы
        /// </summary>
        public void UpdateTitleBinding()
        {
            if ((Window.Current.Content as Frame).Content is PageBase page)
            {
                var binding = new Binding() { Mode = BindingMode.OneWay, Path = new PropertyPath("Title"), Source = page };

                this._title.SetBinding(CustomTextBlock.TextProperty, binding);
            }
        }
        */
        public PullToRefreshUC PullToRefresh
        {
            get { return this.ucPullToRefresh; }
        }

        public Grid HeaderGrid
        {
            get { return this._headerGrid; }
        }

        /// <summary>
        /// Показываем стрелочку дополнительных пунктов у текста шапки
        /// </summary>
        public bool TitleOption
        {
            get { return this._ucTitle.TitleOption; }
            set { this._ucTitle.TitleOption = value; }
        }


        private event EventHandler<double> _headerHeightChanged;

        /// <summary>
        /// Высота статусбара + высота шапки. Вызывается при подписи.
        /// </summary>
        public event EventHandler<double> HeaderHeightChanged
        {
            add
            {
                this._headerHeightChanged += value;
                value(this, this.HeaderHeight);
            }
            remove
            {
                this._headerHeightChanged -= value;
            }
        }

        public HeaderUC()
        {
            this.OptionsMenu = new ObservableCollection<OptionsMenuItem>();
            base.DataContext = MenuViewModel.Instance;
            this.InitializeComponent();
#if WINDOWS_PHONE_APP
            ApplicationView.GetForCurrentView().VisibleBoundsChanged += App_VisibleBoundsChanged;
#endif
#if WINDOWS_UWP
            if (CustomFrame.Instance.IsDevicePhone)
                ApplicationView.GetForCurrentView().VisibleBoundsChanged += App_VisibleBoundsChanged;
#endif

            //QuadraticEase quadraticEase = new QuadraticEase();
            //quadraticEase.EasingMode = EasingMode.EaseOut;
            //this._menuEasing = quadraticEase;
            this.Loaded += this.HeaderWithMenuUC_Loaded;
//            (Window.Current.Content as Frame).SizeChanged += HeaderWithMenuUC_SizeChanged;
            
            this.itemsControlOptionsMenu.ItemsSource = this.OptionsMenu;

            if(CustomFrame.Instance.IsDevicePhone)
            {
                this.Offset.Height = 27;
            }
            else
            {
                this.Offset.Height = 32;
            }
        }

        void HeaderWithMenuUC_Loaded(object sender, RoutedEventArgs e)
        {
            this._headerHeightChanged?.Invoke(this, this.HeaderHeight);
            base.Loaded -= this.HeaderWithMenuUC_Loaded;

            if (!CustomFrame.Instance.IsDevicePhone)
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

                CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.LayoutMetricsChanged += this.TitleBar_LayoutMetricsChanged;
                //coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;


                this.UpdateApplictionFrame();

                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
                {
                    this.ActualThemeChanged += HeaderUC_ActualThemeChanged;
                }             
            }
        }

        private void HeaderUC_ActualThemeChanged(FrameworkElement sender, object args)
        {
            this.UpdateTitleColors();
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (sender.IsVisible)
                this.Offset.Height = sender.Height;
            else
                this.Offset.Height = 0;
            this._headerHeightChanged?.Invoke(this, this.HeaderHeight);
        }

        private void UpdateTitleColors()
        {
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

            if (Settings.UI_HideApplicationFrame)
            {
                titleBar.ButtonForegroundColor = Colors.White;
            }
            else
            {
                titleBar.ButtonForegroundColor = base.ActualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
            }
        }

        public void UpdateApplictionFrame()
        {
            // customize title area
            //CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = Settings.UI_HideApplicationFrame;

            // customize buttons' colors
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

            //ApplicationView.GetForCurrentView().Title = "LunaVK";

            this.UpdateTitleColors();

            titleBar.ButtonBackgroundColor = Colors.Transparent;

            if (Settings.UI_HideApplicationFrame)
            {

                this.trikibarRoot.Visibility = Visibility.Collapsed;
                this._fullScreenBtn.Visibility = Visibility.Collapsed;

                //titleBar.ButtonBackgroundColor = Colors.Transparent;
                //titleBar.ButtonForegroundColor = Colors.White;
                //titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                //titleBar.ButtonInactiveForegroundColor = Colors.White;

                Window.Current.SetTitleBar(null);
            }
            
            else
            {

                this.trikibarRoot.Visibility = Visibility.Visible;
                this._fullScreenBtn.Visibility = Visibility.Visible;

                //titleBar.ButtonBackgroundColor = null;
                //titleBar.ButtonForegroundColor = null;
                //titleBar.ButtonInactiveBackgroundColor = null;
                //titleBar.ButtonInactiveForegroundColor = null;

                Window.Current.SetTitleBar(this.trikibar);
            }
        }

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            this._fullScreenBtn.Margin = new Thickness(0, 0, sender.SystemOverlayRightInset, 0);
            //this.CoreTitleBar_IsVisibleChanged(sender, args);
        }

#if WINDOWS_PHONE_APP || WINDOWS_UWP
        private void App_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            ApplicationView v = sender as ApplicationView;

            if (CustomFrame.Instance.CurrentOrientation == ApplicationViewOrientation.Portrait && v.VisibleBounds.Top == 0)
                return;

            this.Offset.Height = v.VisibleBounds.Top;
            
            this._headerHeightChanged?.Invoke(this, this.HeaderHeight);
        }
#endif

        public UIElement TitlePanel
        {
            //get { return this._titlePanel; }
            get { return this._ucTitle; }
        }
        
        private void OptionsBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            OptionsMenuItem dataContext = (sender as FrameworkElement).DataContext as OptionsMenuItem;


            /*
            // ISSUE: reference to a compiler-generated field
            EventHandler<OptionsMenuItemType> menuItemSelected = this.OptionsMenuItemSelected;
            if (menuItemSelected == null)
                return;
            int type = (int)dataContext.Type;
            menuItemSelected(this, (OptionsMenuItemType)type);*/
            dataContext.Clicked?.Invoke(sender);

            e.Handled = true;
        }




        /// <summary>
        /// После загрузки страницы вызывается
        /// </summary>
        public void BackAndRefreshApply()
        {
            this._refreshBtn.Visibility = ((CustomFrame.Instance.Content as Page).DataContext is ISupportDownIncrementalLoading) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Refresh_Tpped(object sender, TappedRoutedEventArgs e)
        {
            var gv = (CustomFrame.Instance.Content as PageBase).FindChild<Controls.ExtendedGridView2>();
            if (gv is Controls.ExtendedGridView2 ex)
            {
                ex.NeedReload = true;
                ex.Reload();
                return;//нам надо заблокировать дальнейший поиск, т.к. в шапках сеток бывают и списки
            }

            var lv = (CustomFrame.Instance.Content as PageBase).FindChild<Controls.ExtendedListView3>();
            if (lv is Controls.ExtendedListView3 ex2)
            {
                ex2.NeedReload = true;
                ex2.Reload();
            }
            else if((CustomFrame.Instance.Content as PageBase).DataContext is ISupportReload reload)
            {
                reload.Reload();
            }

        }

        private void Back_Tapped(object sender, TappedRoutedEventArgs e)
        {
         //   if (CustomFrame.Instance.CanGoBack)
                CustomFrame.Instance.GoBack();
        }

        DispatcherTimer searchTimer;

        public void ActivateMoreOptionsInSearch(bool status)
        {
            this._moreSearchBrd.Visibility = status.ToVisiblity();
        }

        public void ActivateSearch(bool status, bool noCallback = false, /*bool moreOptions = false,*/ string searchQ = "", bool hideCloseBtn = false)
        {
            this._searchCloseBtn.Visibility = (!hideCloseBtn).ToVisiblity();
            this._ucTitle.Visibility = (!status).ToVisiblity();

            if (status)
            {
                if (hideCloseBtn)
                {
                    this.searchTextBox.Margin = new Thickness(0, 0, 12, 0);
                    this._moreSearchBrd.Margin = new Thickness(0,0,20,0);
                }

                this.searchPanel.Visibility = Visibility.Visible;
                this._moreOptionsPanel.Visibility = Visibility.Collapsed;

                if(string.IsNullOrEmpty(searchQ))
                    this.searchTextBox.Focus(FocusState.Keyboard);

                if (this.searchTimer == null)
                {
                    this.searchTimer = new DispatcherTimer();
                    this.searchTimer.Interval = TimeSpan.FromSeconds(1);
                    this.searchTimer.Tick += Timer_Tick;
                }
                this.searchTextBox.Text = searchQ;
                this.searchTextBox.TextChanged += this.searchTextBox_TextChanged;
            }
            else
            {
                this.searchTextBox.Margin = new Thickness(0, 0, 0, 0);
                this._moreSearchBrd.Margin = new Thickness(0, 0, 8, 0);
                
                this._moreSearchBrd.Visibility = this.searchPanel.Visibility = Visibility.Collapsed;
                this._moreOptionsPanel.Visibility = Visibility.Visible;

                if (this.SearchClosed != null && !noCallback)
                    this.SearchClosed();

                if (this.searchTimer != null)
                {
                    if (this.searchTimer.IsEnabled)
                        this.searchTimer.Stop();

                    this.searchTimer.Tick -= Timer_Tick;
                    this.searchTimer = null;
                }
                
                this.searchTextBox.TextChanged -= this.searchTextBox_TextChanged;
                this.searchTextBox.Text = "";
            }
        }

        public Action<string> LocalSearch;

        /// <summary>
        /// Вызывается по таймеру после изменения текста в поле поиска
        /// </summary>
        public Action<string> ServerSearch;

        /// <summary>
        /// Происходит при закрытии поля поиска
        /// </summary>
        public Action SearchClosed;

        public Action MoreSearchClicked;

        private void Timer_Tick(object sender, object e)
        {
            (sender as DispatcherTimer).Stop();
            this.ServerSearch?.Invoke(this.searchTextBox.Text);
        }

        private void CloseSearch_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.ActivateSearch(false);
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.searchTimer.IsEnabled)
                this.searchTimer.Stop();
            
            string text = (sender as TextBox).Text;

            if (!string.IsNullOrEmpty(text))
                this.searchTimer.Start();

            this.LocalSearch?.Invoke(text);
        }

        private void Sandwich_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            CustomFrame.Instance.OpenCloseMenu();
        }

        private void SearchTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (e.Key == Windows.System.VirtualKey.Enter /*&& !string.IsNullOrEmpty(textBox.Text)*/)
            {
                if (this.searchTimer.IsEnabled)
                    this.searchTimer.Stop();

                this.ServerSearch?.Invoke(textBox.Text);
            }
        }

        private void MoreSearch_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this.MoreSearchClicked?.Invoke();
        }

        private void _fullScreenBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            
            if (view.IsFullScreenMode)
            {
                this.Offset.Height = 32;
                view.ExitFullScreenMode();
                this.trikibarRoot.Visibility = Visibility.Visible;
                //this._fullScreenBtn.Icon = new SymbolIcon(Symbol.FullScreen);
                //this._headerGrid.Margin = new Thickness(0, 0, Settings.UI_HideApplicationFrame ? 150 : 0, 0);
            }
            else
            {
                bool succeeded = view.TryEnterFullScreenMode();
                if (succeeded)
                {
                    this.Offset.Height = 0;
                    this.trikibarRoot.Visibility = Visibility.Collapsed;
                    //this._fullScreenBtn.Icon = new SymbolIcon(Symbol.BackToWindow);
                    //this._headerGrid.Margin = new Thickness();

                    ApplicationView.GetForCurrentView().VisibleBoundsChanged += FullScreenModeTrigger_VisibleBoundsChanged;
                }
                else
                {
                    // cannot maximize
                }
            }

            this._headerHeightChanged?.Invoke(this, this.HeaderHeight);
        }

        private void FullScreenModeTrigger_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            if (!sender.IsFullScreenMode)
            {
                sender.VisibleBoundsChanged -= FullScreenModeTrigger_VisibleBoundsChanged;
                this.trikibarRoot.Visibility = Visibility.Visible;
                this.Offset.Height = 32;
                this._headerGrid.Margin = new Thickness(0, 0, Settings.UI_HideApplicationFrame ? 180 : 0, 0);
                this._headerHeightChanged?.Invoke(this, this.HeaderHeight);
            }
        }
    }
}
