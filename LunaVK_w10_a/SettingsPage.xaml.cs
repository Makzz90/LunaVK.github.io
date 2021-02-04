using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using LunaVK.Core;
using LunaVK.ViewModels;
using LunaVK.Core.ViewModels;
using LunaVK.Library;
using LunaVK.Core.Framework;
using System.Threading.Tasks;
using LunaVK.Framework;
using LunaVK.Pages;
using LunaVK.Common;

namespace LunaVK
{
    public sealed partial class SettingsPage : PageBase
    {
        DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(250) };

        public SettingsPage()
        {
            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("Menu_Settings/Content");
            this.Loaded += SettingsPage_Loaded;
            this.Unloaded += SettingsPage_Unloaded;
            this.timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, object e)
        {
            Random rdm = new Random();
            string hexValue = string.Empty;
            int num;

            //for (int i = 0; i < 4; i++)
            //{
                num = rdm.Next(0, int.MaxValue);
                hexValue += num.ToString("X1");
            //}


            this._debug.SubTitle = "0x" + hexValue;
        }

        private void SettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE74D", Clicked = this.DeleteClicked });
            this.timer.Start();
        }

        private void Notifications_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            CustomFrame.Instance.Navigate(typeof(SettingsNotificationsPage));
        }

        private void General_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            CustomFrame.Instance.Navigate(typeof(SettingsGeneralPage));
        }

        private void Personalization_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            CustomFrame.Instance.Navigate(typeof(SettingsPersonalizationPage));
        }

        private async void DeleteClicked(object sender)
        {
            var result = await MessageBox.Show("DeleteConfirmation", "Settings_Delete");
            if (result != MessageBox.MessageBoxButton.OK)
                return;

            PushNotifications.Instance.UpdateDeviceRegistration((ret) => {
                SettingsHelper.Clear();
            }, true);


            CustomFrame.Instance.Navigate(typeof(LoginPage));
        }

        private async void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var result = await MessageBox.Show("LogOut/Text", "Settings_LogOutMessage");
            if (result !=  MessageBox.MessageBoxButton.OK)
                return;
            
            CustomFrame.Instance._shouldResetStack = true;
            CustomFrame.Instance.Navigate(typeof(LoginPage));
        }

        private void About_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            CustomFrame.Instance.Navigate(typeof(AboutPage));
        }

        private void Account_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            CustomFrame.Instance.Navigate(typeof(SettingsAccountPage));
        }

        private void Privacy_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            CustomFrame.Instance.Navigate(typeof(SettingsPrivacyPage));
        }

        private void Diagnostics_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            CustomFrame.Instance.Navigate(typeof(DiagnosticsPage));
        }

        private void Blacklist_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            CustomFrame.Instance.Navigate(typeof(BannedUsersPage));
        }
    }
}
