using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using LunaVK.Network;
using LunaVK.Core;
using LunaVK.Core.Network;
using LunaVK.Core.Enums;
using LunaVK.ViewModels;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.ViewManagement;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using LunaVK.Library;
using LunaVK.Framework;
using LunaVK.Core.Framework;
using LunaVK.Pages;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using Windows.Storage;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Metadata;
using LunaVK.Common;

namespace LunaVK
{
    public sealed partial class LoginPage : Page
    {
        /// <summary>
        /// Если есть континум анимация
        /// </summary>
        bool needAnimation;
        //private bool Accessed;
        private bool _isCompleted;

        public LoginPage()
        {
            this.InitializeComponent();
            this.Loaded += this.LoginPage_Loaded;
            this.Unloaded += this.LoginPage_Unloaded;

            InputPane.GetForCurrentView().Showing += this.Keyboard_Showing;
            InputPane.GetForCurrentView().Hiding += this.Keyboard_Hiding;
        }

        private void LoginPage_Unloaded(object sender, RoutedEventArgs e)
        {
            InputPane.GetForCurrentView().Showing -= this.Keyboard_Showing;
            InputPane.GetForCurrentView().Hiding -= this.Keyboard_Hiding;
        }

        private void Keyboard_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            //this.tr.Y = sender.OccludedRect.Height / (-2);
            this.ShowingMoveSpline.Value = (sender.OccludedRect.Height / (-2));
            this.MoveMiddleOnShowing.Begin();
        }

        private void Keyboard_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            //this.tr.Y = 0;
            this.ShowingMoveSpline.Value = 0;
            this.MoveMiddleOnShowing.Begin();
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.SuppressMenu = true;
            CustomFrame.Instance.Header.Visibility = Visibility.Collapsed;

            if (this.needAnimation)
                this.StoryName.Begin();
            else
            {
                this.RootBack.Opacity = 1.0;
                Transform1.Y = Transform2.Y = 0;
                gridLogin.Opacity = gridPass.Opacity = 1;
            }


            if (Settings.IsAuthorized)
            {
                PushNotifications.Instance.UpdateDeviceRegistration((ret) =>
                {
                    Settings.UserId = 0;
                    Settings.AccessToken = Settings.LoggedInUserName = Settings.LoggedInUserPhoto = Settings.LoggedInUserStatus = "";
                }, true);
            }

            EventAggregator.Instance.PublishCounters(new Core.DataObjects.CountersArgs());
            DialogsViewModel.Instance.Items.Clear();
            CacheManager.TryDelete("News");
            LongPollServerService.Instance.Stop();
            LongPollServerService.Instance.SetUnreadMessages(0);
            //PushNotificationsManager.Instance.EnsureTheChannelIsClosed();
            ContactsManager.Instance.DeleteAllContactsAsync();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            this.PerformLoginAttempt();
        }

        private void PerformLoginAttempt()
        {
            this._error.Text = "";
            this._error.Opacity = 0;
            this._progressRing.IsActive = true;
            this.passwordBox.IsEnabled = this.textBoxUsername.IsEnabled = this.LoginBtn.IsEnabled = false;
            
            VKRequestsDispatcher.DispatchLoginRequest(this.textBoxUsername.Text, this.passwordBox.Password, this.Callback);
        }

        private void Callback(VKErrors error, string description)
        {
            if (error == VKErrors.None)
            {
                Regex QueryStringRegex = new Regex("access_token=(?<access_token>.+)&.+user_id=(?<user_id>\\d+)");
                Match m = QueryStringRegex.Match(description);
                string access_token = m.Groups["access_token"].Value;
                uint user_id = uint.Parse(m.Groups["user_id"].Value);
                
                Settings.AccessToken = access_token;
                Settings.UserId = user_id;
                LongPollServerService.Instance.Restart();
                PushNotifications.Instance.UpdateDeviceRegistration();

                CustomFrame.Instance._shouldResetStack = true;

                MenuViewModel.Instance.GetBaseData((res) =>
                {
                    Execute.ExecuteOnUIThread(() =>
                    { 
                        if (res == true)
                            NavigatorImpl.Instance.NavigateToNewsFeed();
                        else
                        {
                            this._error.Text = "Авторизация выполнена, но не удалось получить данные пользователя.";
                            this._error.Opacity = 1;
                            this._progressRing.IsActive = false;
                            this.passwordBox.IsEnabled = this.textBoxUsername.IsEnabled = this.LoginBtn.IsEnabled = true;
                        }
                    });
                });
                //this.Accessed = true;
            }
            else
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    if(!string.IsNullOrEmpty(description))
                    {
                        this._error.Text = description;
                        //this._error.Animate(1, 0, "Opacity", 400, 3000);
                        this._error.Opacity = 1;
                        this._errorStoryBoard.Begin();
                    }
                    this._progressRing.IsActive = false;
                    this.passwordBox.IsEnabled = this.textBoxUsername.IsEnabled = this.LoginBtn.IsEnabled = true;
                });
            }
        }


        










        

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToWebUri("https://m.vk.com/terms", true);
        }
        
        private void Settings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CustomFrame.Instance.Navigate(typeof(SettingsGeneralPage), true);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            
            CustomFrame.Instance.SuppressMenu = false;
            CustomFrame.Instance.Header.Visibility = Visibility.Visible;
            //CustomFrame.Instance.MySplitView.ActivateSwipe(true);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
            {
                ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("image");
                if (imageAnimation != null)
                {
                    this.needAnimation = true;
                    imageAnimation.TryStart(this.HeaderImage);
                }
            }
            else
            {
                this.needAnimation = true;
            }

        }

        private void UpdateLoginButtonState()
        {
            this.LoginBtn.IsEnabled = !string.IsNullOrWhiteSpace(this.textBoxUsername.Text) && !string.IsNullOrEmpty(this.passwordBox.Password);
        }

        private void textBoxUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.UpdateLoginButtonState();
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.UpdateLoginButtonState();
        }

        private void textBoxUsername_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter || string.IsNullOrEmpty(this.textBoxUsername.Text))
                return;
            this.passwordBox.Focus(FocusState.Keyboard);
        }

        private void passwordBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter || string.IsNullOrEmpty(this.textBoxUsername.Text) || string.IsNullOrEmpty(this.passwordBox.Password))
                return;
            this.PerformLoginAttempt();
        }














        private void OAuthButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.Web.Http.Filters.HttpBaseProtocolFilter myFilter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
            var cookieManager = myFilter.CookieManager;

            Windows.Web.Http.HttpCookieCollection myCookieJar = cookieManager.GetCookies(new Uri("https://www.vk.com/"));
            foreach (Windows.Web.Http.HttpCookie cookie in myCookieJar)
            {
                cookieManager.DeleteCookie(cookie);
            }
            //Windows.UI.Xaml.Controls.WebView.ClearTemporaryWebDataAsync();
            //this.browser.InvokeScriptAsync("eval", new string[] { "location.reload(true)" });
            string url = string.Format("https://{0}/authorize?client_id={1}&scope={2}&redirect_uri={3}&display=mobile&v={4}&response_type=token", this._cbProxy.IsChecked.Value ? "vk-oauth-proxy.xtrafrancyz.net" : "oauth.vk.com",
                VKConstants.ApplicationID, VKConstants.Scope, VKConstants.Redirect, VKConstants.API_VERSION);
            this.browser.Navigate(new Uri(url));

        }

        private void _browser_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            VisualStateManager.GoToState(this, "Loading", true);
            _isCompleted = false;
            if (args.Uri.AbsoluteUri.Contains("access_token="))
            {
                _isCompleted = true;

                var parts = args.Uri.Fragment.Substring(1).Split('&').ToArray();
                string access_token = parts[0].Split('=')[1];
                uint user_id = uint.Parse(parts[2].Split('=')[1]);


                Settings.AccessToken = access_token;
                Settings.UserId = user_id;
                LongPollServerService.Instance.Restart();
                PushNotifications.Instance.UpdateDeviceRegistration();

                CustomFrame.Instance._shouldResetStack = true;

                sender.NavigationCompleted -= this.browser_NavigationCompleted;//bugfix: таким образом после успешного входа уберём показ поля для ввода пароля

                MenuViewModel.Instance.GetBaseData((res) =>
                {
                    if (res == true)
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            Settings.UseProxy = this._cbProxy.IsChecked.Value;
                            NavigatorImpl.Instance.NavigateToNewsFeed();
                        });
                    }
                });
            }
            else if (args.Uri.AbsoluteUri.Contains("error="))
            {
                this._isCompleted = true;
                VisualStateManager.GoToState(this, "Normal", true);
            }
        }

        private void TextBlock_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToRegistrationPage();
        }














        
        
        
        private void browser_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            VisualStateManager.GoToState(this, this._isCompleted ? "Normal" : "Auth", true);
        }
        
        private void browser_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            /*
            ServiceHelper.DialogService.ShowMessage(
                        "Не удалось подключиться к ВКонтакте. Авторизация не удалась.",
                        "Ошибка соединения");*/
            _isCompleted = true;
            //_isLoading = false;
            VisualStateManager.GoToState(this, "Normal", true);
        }
    }
}
