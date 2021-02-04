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

using LunaVK.Core.ViewModels;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.ViewManagement;
using LunaVK.ViewModels;
using LunaVK.Core;
using LunaVK.Pages;
using Windows.UI;

namespace LunaVK.UC
{
    public sealed partial class MenuUC : UserControl
    {
        //private bool InSearch;
//        DispatcherTimer searchTimer;

        public MenuUC()
        {
            base.DataContext = MenuViewModel.Instance;
            this.InitializeComponent();
            this.Loaded += MenuUC_Loaded;
            this._menuGrid.Loaded += _menuGrid_Loaded;
            //this.userPhoto.LetsRound();

            
            

//            this.serachHints.DataContext = new MenuSearchViewModel();
//            this.SearchVM.Items.CollectionChanged += Items_CollectionChanged;
            
        }

        private void CFrame_MenuOpenChanged(object sender, bool e)
        {
//            this.SearchIcon.Visibility = e ? Visibility.Collapsed : Visibility.Visible;
            this.Birthdays.Visibility = e ? Visibility.Visible : Visibility.Collapsed;
        }
        /*
        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                this._searchResultGrid.Visibility = Visibility.Visible;
            else
                this._searchResultGrid.Visibility = Visibility.Collapsed;
        }
       
        private MenuSearchViewModel SearchVM
        {
            get { return this.serachHints.DataContext as MenuSearchViewModel; }
        }
         */
        void _menuGrid_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.MenuOpenChanged += this.CFrame_MenuOpenChanged;

            this.IconThemeSwitch.Glyph = CustomFrame.Instance.RequestedTheme == ElementTheme.Light ? "\xE793" : "\xE708";
            this.IconMute.Glyph = Settings.PushNotificationsBlockedUntil >= DateTime.UtcNow ? "\xE7ED" : "\xE767";//xE767 - есть звук
            /*
            //Мы выполняем этот код немного позднее
            double width = Math.Min((Window.Current.Content as Frame).ActualWidth, (Window.Current.Content as Frame).ActualHeight);
            
            if (CustomFrame.Instance.IsDevicePhone)
            {
                Constants.MENU_WIDE_WIDTH = width * 0.75;
            }
            */
            this._menuGrid.Loaded -= this._menuGrid_Loaded;
            /*
#if WINDOWS_PHONE_APP || WINDOWS_UWP
            if (CustomFrame.Instance.IsDevicePhone)
            {
                var vis = ApplicationView.GetForCurrentView();
                vis.VisibleBoundsChanged += App_VisibleBoundsChanged;
                this._stack.Margin = new Thickness(0, vis.VisibleBounds.Top, 0, 0);
//                this._searchResultGrid.Margin = new Thickness(0, vis.VisibleBounds.Top + SearchIcon.ActualHeight + borderSandwich.ActualHeight, 0, 0);
            }
#endif
            */
        }
        
        void MenuUC_Loaded(object sender, RoutedEventArgs e)
        {
            //            CustomFrame.Instance.MenuStateChanged += this.MenuStateChanged;
            //CustomFrame.Instance.HeaderWithMenu.HeaderHeightChanged += this.HeaderHeightChanged;


            if (Settings.DEV_AddDebugButton)
            {
                Button btn = new Button();
                btn.Style = (Style)Application.Current.Resources["MediaTransportControlFluentButtonStyle"];

                IconUC icon = new IconUC();
                icon.Glyph = "\xE964";
                icon.FontSize = 22;
                btn.Content = icon;
                btn.Tapped += Debug_Tapped;
                btn.Holding += Brd_Holding;
                //icon.Foreground = (SolidColorBrush)Application.Current.Resources["SystemControlFocusVisualPrimaryBrush"];
                this._WrapGrid.Children.Add(btn);
            }
        }

        



#region CLICK
        private void News_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.NavigateOnMenuClick(() => NavigatorImpl.Instance.NavigateToNewsFeed(), true);
        }

        private void Notifications_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.NavigateOnMenuClick(() => NavigatorImpl.Instance.NavigateToFeedback(), true);
        }

        private void Messages_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.NavigateOnMenuClick(() => NavigatorImpl.Instance.NavigateToConversations(), true);
        }

        private void Friends_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.NavigateOnMenuClick(() => NavigatorImpl.Instance.NavigateToFriends((int)Settings.UserId), true);
        }

        private void Groups_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.NavigateOnMenuClick(() => NavigatorImpl.Instance.NavigateToGroups((int)Settings.UserId), true);
        }

        private void Photos_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.NavigateOnMenuClick(() => /*NavigatorImpl.Instance.NavigateToPhotoAlbums((int)Settings.UserId,"")*/NavigatorImpl.Instance.NavigateToAllPhotos((int)Settings.UserId, ""), true);
        }

        private void Videos_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.NavigateOnMenuClick(() => NavigatorImpl.Instance.NavigateToVideoCatalog()/*NavigatorImpl.Instance.NavigateToVideos((int)Settings.UserId)*/, true);
        }

        private void Audios_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToAudio((int)Settings.UserId, "");
        }

        private void Bookmarks_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToFavorites();
        }

        private void Likes_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToLikes();
        }

        private void Documents_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.NavigateOnMenuClick(() => NavigatorImpl.Instance.NavigateToDocuments((int)Settings.UserId), true);
        }

        private void Downloads_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToDownloads();
        }

        private void Settings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.NavigateOnMenuClick(() => NavigatorImpl.Instance.NavigateToSettings(), true);
        }

        private void Debug_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(e!=null)
                CustomFrame.Instance.OpenCloseMenu(false);

            var debugUc = new DebugUC();
            //debugUc.MaxWidth = 600;
            //debugUc.Height = 400;
            debugUc.VerticalAlignment = VerticalAlignment.Bottom;
            PopUpService statusChangePopup = new PopUpService
            {
                Child = debugUc
            };
            statusChangePopup.BackgroundBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);
            statusChangePopup.OverrideBackKey = true;
            statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            statusChangePopup.Show();

            
            //this.debugUc.Visibility = Visibility.Collapsed;
            //this.debugUc.VerticalAlignment = VerticalAlignment.Bottom;
        }

        private void Brd_Holding(object sender, HoldingRoutedEventArgs e)
        {
            this.Debug_Tapped(sender, null);
        }

        private void ThemeSwitch_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (CustomFrame.Instance.RequestedTheme == ElementTheme.Light)
            {
                Settings.BackgroundType = false;
                CustomFrame.Instance.RequestedTheme = ElementTheme.Default;
                this.IconThemeSwitch.Glyph = "\xE708";
            }
            else
            {
                Settings.BackgroundType = true;
                CustomFrame.Instance.RequestedTheme = ElementTheme.Light;
                this.IconThemeSwitch.Glyph = "\xE793";
            }
            CustomFrame.Instance.OpenCloseMenu(true);
        }

        private void Mute_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UC.PopUP _pop = null;
            Point point = e.GetPosition(null);

            if (_pop == null)
            {
                _pop = new UC.PopUP();
                _pop.ItemTapped += _picker_ItemTapped;
                _pop.AddItem(-1, "Выключить уведомления на");
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
            this.IconMute.Glyph = "\xE7ED";
            //
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

        private void MyProfile_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToProfilePage((int)Settings.UserId);
        }
#endregion

        /// <summary>
        /// Сбрасываем журнал переходов перед переходом
        /// </summary>
        /// <param name="navigateAction"></param>
        /// <param name="needClearStack"></param>
        private void NavigateOnMenuClick(Action navigateAction, bool needClearStack = true)
        {
            if (needClearStack)
                CustomFrame.Instance._shouldResetStack = true;
            CustomFrame.Instance.ClearSavedPageAppBar();
            navigateAction();
        }
        
        private void MenuItemUC_Tapped_6(object sender, TappedRoutedEventArgs e)
        {
               
        }
        /*
        private void _searchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.searchTimer == null)
            {
                this.searchTimer = new DispatcherTimer();
                this.searchTimer.Interval = TimeSpan.FromSeconds(0.5);
                this.searchTimer.Tick += Timer_Tick;
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            (sender as DispatcherTimer).Stop();
            if (!string.IsNullOrEmpty(this.SearchVM.Query))
                this.SearchVM.LoadData(true);
            else
                this.SearchVM.Items.Clear();
        }
        
        
       private void SearchIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CustomFrame.Instance.OpenCloseMenu(true);
            this._searchTextBox.Focus(FocusState.Keyboard);
        }

        private void _searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TextBox box = sender as TextBox;
            if (this.searchTimer.IsEnabled)
            {
                this.searchTimer.Stop();
            }
            this.SearchVM.Query = (sender as TextBox).Text;

            if (!string.IsNullOrEmpty(this.SearchVM.Query))
                this.searchTimer.Start();
            else
            {
                this.SearchVM.Items.Clear();
            }
        }
        */
        

        private void _searchTextBox_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
      //      this._searchTextBox.IsEnabled = true;
        }

        private void _searchTextBox_PointerExited(object sender, PointerRoutedEventArgs e)
        {
      //      this._searchTextBox.IsEnabled = false;
        }
        /*
#if WINDOWS_PHONE_APP || WINDOWS_UWP
        private void App_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            if (!CustomFrame.Instance.IsDevicePhone)
                return;

            ApplicationView v = sender as ApplicationView;

            if (CustomFrame.Instance.CurrentOrientation == ApplicationViewOrientation.Portrait && v.VisibleBounds.Top == 0)
                return;


            
            this._stack.Margin = new Thickness(0, v.VisibleBounds.Top, 0, 0);
//            this._searchResultGrid.Margin = new Thickness(0, v.VisibleBounds.Top + SearchIcon.ActualHeight + borderSandwich.ActualHeight, 0, 0);
        }
#endif
        */
        private void borderSandwich_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            CustomFrame.Instance.OpenCloseMenu();
        }


        /*
        private void BaseProfileItem_BackTap(object sender, RoutedEventArgs e)
        {
            var hint = (sender as FrameworkElement).DataContext as MenuSearchViewModel.SearchHint;
            if (hint.profile != null)
            {
                NavigatorImpl.Instance.NavigateToProfilePage(hint.profile.id);
            }
            else if (hint.group != null)
            {
                NavigatorImpl.Instance.NavigateToProfilePage(-hint.group.id);
            }

            this._searchTextBox.Text = "";
            this._searchResultGrid.Visibility = Visibility.Collapsed;
            this.SearchVM.Items.Clear();
        }
        
        private void Search_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            //this.NavigateOnMenuClick((() => NavigatorImpl.Instance.NavigateToUsersSearch()));
            NavigatorImpl.Instance.NavigateToUsersSearch(this._searchTextBox.Text);

            this._searchTextBox.Text = "";
            this._searchResultGrid.Visibility = Visibility.Collapsed;
            this.SearchVM.Items.Clear();
        }
        */
        private void Games_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            //if (this.IsOnGamesPage)
            //{
            //    this.HandleSamePageNavigation(null, false);
            //}
            //else
            //{
                //bool flag = e == null;
                //MenuUC.PublishMenuItemClickedEvent("games");
                this.NavigateOnMenuClick(() => NavigatorImpl.Instance.NavigateToGames(), false);
            //}
        }

        private void Events_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //this.NavigateOnMenuClick(() => CustomFrame.Instance.Navigate(typeof(EventsPage)));
        }

        private void Search_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.NavigateOnMenuClick((() => NavigatorImpl.Instance.NavigateToUsersSearch()));
            //e.Handled = true;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }
    }
}
