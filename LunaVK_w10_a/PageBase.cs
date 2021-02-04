using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media.Animation;
using System.Collections.ObjectModel;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.ViewModels;
using LunaVK.Core.Library;
using LunaVK.Core;

namespace LunaVK
{
    public class PageBase : Page
    {
        //private bool _isLoadedAtLeastOnce;
        public object NavigationParameter;
        //public bool CancelBack;
        public string Title
        {
            get
            {
                return CustomFrame.Instance.Header.Title;
            }
            set
            {
                if (CustomFrame.Instance.Header.Title == value)
                    return;
                CustomFrame.Instance.Header.Title = value;
            }
        }

        public PageBase()
        {
            this.Loaded += this.PageBase_Loaded;
            this.Unloaded += this.PageBase_Unloaded;
            
            if (CustomFrame.Instance!=null)
            {
                if (!CustomFrame.Instance.SupressTransition)
                    this.SetUpPageAnimation();
            }
            
        }

        
        

        protected void SetUpPageAnimation()
        {

            TransitionCollection trc = new TransitionCollection();

            //Windows.UI.Xaml.Media.Animation.NavigationThemeTransition n = new Windows.UI.Xaml.Media.Animation.NavigationThemeTransition();
            //Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo s = new Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo();
            //n.DefaultNavigationTransitionInfo = s;

            //PaneThemeTransition tr = new PaneThemeTransition() { Edge = EdgeTransitionLocation.Right };
            //ContentThemeTransition tr = new ContentThemeTransition() { HorizontalOffset=500, VerticalOffset=0 };

            NavigationThemeTransition tr = new NavigationThemeTransition();

            //ContinuumNavigationTransitionInfo info = new ContinuumNavigationTransitionInfo();
            DrillInNavigationTransitionInfo info = new DrillInNavigationTransitionInfo();
            tr.DefaultNavigationTransitionInfo = info;
            


            trc.Add(tr);
            base.Transitions = trc;
        }

        void PageBase_Loaded(object sender, RoutedEventArgs e)
        {
            //
            if (CustomFrame.Instance == null)
                return;
            //
            CustomFrame.Instance.SupressTransition = false;

            //if (!this._isLoadedAtLeastOnce)
            //{
            //    this._isLoadedAtLeastOnce = true;
            //}
#if WINDOWS_PHONE_APP
            if (LunaVK.Core.Settings.MenuSwipe == true)
            {
                if (base.Content.FindChild<Pivot>() == null)
                {
                    if (base.Content is Grid)
                    {
                        Grid grid = base.Content as Grid;
                        if (!(grid.Children[0] is ScrollViewer))
                        {
                            grid.ManipulationMode |= ManipulationModes.TranslateX;//BUG: на wp8.1 эта строка вызывает множество падений, видимо когда делаем хук на ScrollViewer
                            grid.ManipulationDelta += CustomFrame.Instance.Menu.Rectangle_ManipulationDelta;
                            grid.ManipulationStarted += CustomFrame.Instance.Menu._menuCallout_ManipulationStarted;
                            grid.ManipulationCompleted += CustomFrame.Instance.Menu.Rectangle_ManipulationCompleted;
                        }
                    }
                }
            }
#endif
            Window.Current.VisibilityChanged += Current_VisibilityChanged;
            //
            //
            //            if (!CustomFrame.Instance.IsDevicePhone)
            //                CustomFrame.Instance.HeaderWithMenu.BackAndRefreshApply();

            //

//            if (CustomFrame.Instance.OverlayGrid.Children.Count > 0)
//                CustomFrame.Instance.OverlayGrid.Children.Clear();

            MenuViewModel.Instance.RefreshBaseDataIfNeeded();

            if (!CustomFrame.Instance.IsDevicePhone)
                CustomFrame.Instance.Header.BackAndRefreshApply();

            if(CustomFrame.Instance.Header.RenderTransform is TranslateTransform renderTransform)
            {
                renderTransform.Y = 0;
            }
            

//            CustomFrame.Instance.HeaderWithMenu.UpdateTitleBinding();
            CustomFrame.Instance.Header.ActivateSearch(false,true);
            //this.InitializeProgressIndicator();


            if (Settings.DEV_IsLogsAutoSending && Logger.Instance.IsLogFileExists)
            {
                AppsService.Instance.SendLog(Logger.Instance.ReadLogFromStorage(), (result) =>
                {
                    if (result == true)
                    {
                        Logger.Instance.DeleteLogFromIsolatedStorage();
                    }
                });
            }
        }

        void PageBase_Unloaded(object sender, RoutedEventArgs e)
        {/*
            if (base.Content is Grid)
            {
                Grid grid = base.Content as Grid;

                grid.ManipulationDelta -= CustomFrame.Instance.Menu.Rectangle_ManipulationDelta;
                grid.ManipulationStarted -= CustomFrame.Instance.Menu._menuCallout_ManipulationStarted;
                grid.ManipulationCompleted -= CustomFrame.Instance.Menu.Rectangle_ManipulationCompleted;
            }
            */
            Window.Current.VisibilityChanged -= this.Current_VisibilityChanged;
        }

        /// <summary>
        /// Инициализация индикатора прогресса
        /// </summary>
        /// <param name="source">ViewModel, а иначе base.DataContext</param>
        internal void InitializeProgressIndicator(object source = null)
        {
            if (CustomFrame.Instance.Header != null)
            {
                var binding = new Binding() { Mode = BindingMode.OneWay, Path = new PropertyPath("IsInProgressVisibility"), Source = (source == null ? base.DataContext : source) };
                BindingOperations.SetBinding(CustomFrame.Instance.Header.Progress, UIElement.VisibilityProperty, binding);
            }
        }

        void Current_VisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            this.HandleOnVisibilityChanged(e.Visible);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.BottomAppBar = null;
            this.NavigationParameter = e.Parameter;
            MenuViewModel.Instance.UpdateSelectedItem();
            base.OnNavigatedTo(e);
            this.HandleOnNavigatedTo(e);
            //this.InitializeProgressIndicator();

#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#elif WINDOWS_UWP
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
#endif













#region Page state
            // Returning to a cached page through navigation shouldn't trigger state loading 
            if (this._pageKey != null)
                return; 


            var frameState = SuspensionManager.SessionStateForFrame(base.Frame);
            this._pageKey = "Page-" + this.Frame.BackStackDepth;

            if (e.NavigationMode == NavigationMode.New)
            {
                // Очистка существующего состояния для перехода вперед при добавлении новой страницы в
                // стек навигации
                var nextPageKey = this._pageKey;
                int nextPageIndex = this.Frame.BackStackDepth;
                while (frameState.Remove(nextPageKey))
                {
                    nextPageIndex++;
                    nextPageKey = "Page-" + nextPageIndex;
                }

                // Передача параметра навигации на новую страницу
                this.LoadState(e.Parameter, null);
            }
            else
            {
                // Передача на страницу параметра навигации и сохраненного состояния страницы с использованием
                // той же стратегии загрузки приостановленного состояния и повторного создания страниц, удаленных
                // из кэша
                this.LoadState(e.Parameter, (Dictionary<String, Object>)frameState[this._pageKey]);
            }
#endregion
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            this.HandleOnNavigatingFrom(e);

#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
#elif WINDOWS_UWP
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
#endif
            //
            if (MemoryDiagnosticsHelper.IsLowMemDevice)
                GC.Collect();
            //
        }

#if WINDOWS_PHONE_APP
        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
#elif WINDOWS_UWP
        private void OnBackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
#endif

#if WINDOWS_PHONE_APP || WINDOWS_UWP
            if (e.Handled == true)
                return;

            if (CustomFrame.Instance.IsMenuOpened && CustomFrame.Instance.MenuState!= CustomFrame.MenuStates.StateMenuFixedContentFixed)
            {
                CustomFrame.Instance.OpenCloseMenu(false);
                e.Handled = true;
                return;
            }

            if (CustomFrame.Instance.OverlayGrid.Children.Count > 0)
            {
                e.Handled = true;
                return;
            }

            //if(this.CancelBack)
            //{
            //    this.CancelBack = false;
            //    e.Handled = true;
            //    return;
            //}

            var args = new System.ComponentModel.CancelEventArgs();
            this.HandleOnBackKeyPress(args);
            if (args.Cancel)
            {
                e.Handled = true;
            }
            else
            {
                bool ret = NavigatorImpl.GoBack();
                e.Handled = ret;
            }
        }
#endif



        protected virtual void HandleOnNavigatedTo(NavigationEventArgs e) { }

        protected virtual void HandleOnNavigatingFrom(NavigatingCancelEventArgs e) { }

        protected virtual void HandleOnBackKeyPress(System.ComponentModel.CancelEventArgs e) { }

        protected virtual void HandleOnVisibilityChanged(bool is_visible) { }







#region Process lifetime management
        //https://github.com/Q42/Q42.WinRT/blob/master/Q42.WinRT.SampleApp/Common/LayoutAwarePage.cs

        private String _pageKey;

        /// <summary> 
        /// Invoked when this page will no longer be displayed in a Frame. 
        /// </summary> 
        /// <param name="e">Event data that describes how this page was reached.  The Parameter 
        /// property provides the group to be displayed.</param> 
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
            var pageState = new Dictionary<String, Object>();
            this.SaveState(pageState);
            frameState[_pageKey] = pageState;
        }

        /// <summary> 
        /// Populates the page with content passed during navigation.  Any saved state is also 
        /// provided when recreating a page from a prior session. 
        /// </summary> 
        /// <param name="navigationParameter">The parameter value passed to 
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested. 
        /// </param> 
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier 
        /// session.  This will be null the first time a page is visited.</param> 
        protected virtual void LoadState(Object navigationParameter, Dictionary<String, Object> pageState) { }

        /// <summary> 
        /// Preserves state associated with this page in case the application is suspended or the 
        /// page is discarded from the navigation cache.  Values must conform to the serialization 
        /// requirements of <see cref="SuspensionManager.SessionState"/>. 
        /// </summary> 
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param> 
        protected virtual void SaveState(Dictionary<String, Object> pageState) { }
#endregion
    }
}
