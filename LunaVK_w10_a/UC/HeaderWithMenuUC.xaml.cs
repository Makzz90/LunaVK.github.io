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

namespace LunaVK.UC
{
    public sealed partial class HeaderWithMenuUC : UserControl
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
                    this.root0.IsHitTestVisible = value;
                    this.TopBarInterBackground.Opacity = value ? 1.0 : 0;
                }
            }
        }

        public Grid BackGroundGrid
        {
            get
            {
                return this.TopBarInterBackground;
            }
        }

        public void ShowProgress(bool show)
        {
            this.progressBar.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
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

        public string Title
        {
            get { return this._title.Text; }
            set { this._title.Text = value; }
        }

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

        public PullToRefreshUC PullToRefresh
        {
            get { return this.ucPullToRefresh; }
        }

        public Grid HeaderGrid
        {
            get { return this._headerGrid; }
        }
        //
        private DebugUC debugUc;
        //
        /*
        /// <summary>
        /// Показываем стрелочку дополнительных пунктов у текста шапки
        /// </summary>
        /// <param name="value"></param>
        public void EnableTitleOption(bool value)
        {
            this.iconMenuOpen.Visibility = value ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
        }
        */
        /// <summary>
        /// Показываем стрелочку дополнительных пунктов у текста шапки
        /// </summary>
        public bool TitleOption
        {
            get
            {
                return this.iconMenuOpen.Visibility == Visibility.Visible;
            }
            set
            {
                this.iconMenuOpen.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
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

        public HeaderWithMenuUC()
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
            this.Loaded += HeaderWithMenuUC_Loaded;
//            (Window.Current.Content as Frame).SizeChanged += HeaderWithMenuUC_SizeChanged;
            
            this.itemsControlOptionsMenu.ItemsSource = this.OptionsMenu;
#if DEBUG
            this.debugUc = new DebugUC();
            this.debugUc.Visibility = Visibility.Collapsed;
            this.debugUc.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom;
            this.root0.Children.Add(this.debugUc);

            Border brd = new Border();
            Grid.SetColumn(brd,3);
            brd.Tapped += MenuItemUC_Tapped_Debug;
            IconUC icon = new IconUC
            {
                Glyph = "\xE964",
                FontSize = 22
            };
            brd.Child = icon;
//            this.GridSubButtons.Children.Add(brd);

#endif
            if(CustomFrame.Instance.IsDevicePhone)
            {
                this.Offset.Height = 27;
            }
            //else
            //{
            //    this.Offset.Height = 28;
           // }
        }


       // private TranslateTransform _menuTransform;
        private double MenuWidth
        {
            get { return CustomFrame.Instance.Menu.Width; }
        }
        /*
        void HeaderWithMenuUC_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (CustomFrame.Instance.IsDevicePhone)
                Constants.MENU_WIDE_WIDTH = Math.Min((Window.Current.Content as Frame).ActualWidth, (Window.Current.Content as Frame).ActualHeight) * 0.75;

//            this._searchGrid.Width = Constants.MENU_WIDE_WIDTH;
            //this._headerGrid.Width = CustomFrame.Instance.ActualWidth + Constants.MENU_WIDE_WIDTH;
            //this._headerContentTransform.X = Constants.MENU_WIDE_WIDTH;
//            this._searchTransform.X = -Constants.MENU_WIDE_WIDTH;
        }
        */
        void HeaderWithMenuUC_Loaded(object sender, RoutedEventArgs e)
        {
            //            this.serachHints.Margin = new Thickness(0, this.Offset.Height + (double)Application.Current.Resources["Double55"], 0, 0);

            //           CustomFrame.Instance.MenuStateChanged += this.MenuStateChanged;
            //           this._menuTransform = CustomFrame.Instance.Menu.Transf;
            this._headerHeightChanged?.Invoke(this, this.HeaderHeight);
        }

        void MenuStateChanged(object sender, CustomFrame.MenuStates e)
        {
            VisualStateManager.GoToState(this, e.ToString(), false);
        }

#if WINDOWS_PHONE_APP || WINDOWS_UWP
        private void App_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            ApplicationView v = sender as ApplicationView;

            if (CustomFrame.Instance.CurrentOrientation == ApplicationViewOrientation.Portrait && v.VisibleBounds.Top == 0)
                return;

            //if(CustomFrame.Instance.IsDevicePhone)
                this.Offset.Height = v.VisibleBounds.Top;
            //else
            //    this.Offset.Height = 28;
            this._headerHeightChanged?.Invoke(this, this.HeaderHeight);
        }
#endif

        public StackPanel TitlePanel
        {
            get { return this._titlePanel; }
        }
        
        private void Canvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            OptionsMenuItem dataContext = ((FrameworkElement)sender).DataContext as OptionsMenuItem;


            /*
            // ISSUE: reference to a compiler-generated field
            EventHandler<OptionsMenuItemType> menuItemSelected = this.OptionsMenuItemSelected;
            if (menuItemSelected == null)
                return;
            int type = (int)dataContext.Type;
            menuItemSelected(this, (OptionsMenuItemType)type);*/
            dataContext.Clicked?.Invoke();

            e.Handled = true;
        }




        /// <summary>
        /// После загрузки страницы вызывается
        /// </summary>
        public void BackAndRefreshApply()
        {
            this._refreshBtn.Visibility = ((CustomFrame.Instance.Content as Page).DataContext is ISupportUpDownIncrementalLoading) ? Visibility.Visible : Visibility.Collapsed;
            //this._bacBtnk.Visibility = CustomFrame.Instance.CanGoBack ? Visibility.Visible : Visibility.Collapsed;

            
        }
        
        private void MenuItemUC_Tapped_Debug(object sender, TappedRoutedEventArgs e)
        {
            bool cur_vis = this.debugUc.Visibility == Visibility.Visible;
            this.debugUc.Visibility = cur_vis ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Refresh_Tpped(object sender, TappedRoutedEventArgs e)
        {
            //var temp = (CustomFrame.Instance.Content as PageBase).FindChild<ExtendedListView2>();
            //if (temp is ExtendedListView2 ex)
            //{
            //    ex.NeedReload = true;
            //    ex.Reload();
            //}

            var temp2 = (CustomFrame.Instance.Content as PageBase).FindChild<Controls.ExtendedListView3>();
            if (temp2 is Controls.ExtendedListView3 ex2)
            {
                ex2.NeedReload = true;
                ex2.Reload();
            }
        }

        private void Back_Tapped(object sender, TappedRoutedEventArgs e)
        {
         //   if (CustomFrame.Instance.CanGoBack)
                CustomFrame.Instance.GoBack();
        }

        private void ThemeSwitch_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (CustomFrame.Instance.RequestedTheme == ElementTheme.Light)
                CustomFrame.Instance.RequestedTheme = ElementTheme.Default;
            else
                CustomFrame.Instance.RequestedTheme = ElementTheme.Light;            
        }

        private void Mute_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UC.PopUP _pop = null;
            Point point = e.GetPosition(null);

            if (_pop == null)
            {
                _pop = new UC.PopUP();
                _pop.ItemTapped += _picker_ItemTapped;
                _pop.AddSpace();
                _pop.AddItem(0, "1 час");
                _pop.AddItem(1, "2 часа");
                _pop.AddItem(2, "3 часа");
                _pop.AddItem(3, "5 часов");
                _pop.AddItem(4, "8 часов");
            }

            _pop.Show(point);
        }

        private void _picker_ItemTapped(object argument, int i)
        {
            ushort hour = 0;
            switch (i)
            {
                case 0:
                    {
                        hour = 1;
                        break;
                    }
                case 1:
                    {
                        hour = 2;
                        break;
                    }
                case 2:
                    {
                        hour = 3;
                        break;
                    }
                case 3:
                    {
                        hour = 5;
                        break;
                    }
                case 4:
                    {
                        hour = 8;
                        break;
                    }
            }

            SettingsViewModel VM = new SettingsViewModel();
            VM.Disable(hour * 3600);
        }

        DispatcherTimer searchTimer;

        public void ActivateSearch(bool status, bool noCallback = false, bool moreOptions = false, string searchQ = "", bool hideCloseBtn = false)
        {
            this._searchCloseBtn.Visibility = (!hideCloseBtn).ToVisiblity();

            if (status)
            {
                if(moreOptions)
                    this._moreSearchBrd.Visibility = Visibility.Visible;

                this.searchPanel.Visibility = Visibility.Visible;
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
                this._moreSearchBrd.Visibility = this.searchPanel.Visibility = Visibility.Collapsed;
                
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
    }
}
