using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using LunaVK.Library;
using LunaVK.ViewModels;
using LunaVK.Core;
using LunaVK.Core.Library;
using Windows.ApplicationModel.Background;
using LunaVK.UC;
using LunaVK.Core.Framework;

namespace LunaVK
{
    public sealed partial class SettingsNotificationsPage : PageBase
    {
        UC.PopUP _pop = null;
        private bool NeedSync;

        public SettingsNotificationsPage()
        {
            base.DataContext = new SettingsViewModel();

            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("SettingsNotifications");
            this.Loaded += SettingsNotificationsPage_Loaded;
        }

        private async void SettingsNotificationsPage_Loaded(object sender, RoutedEventArgs e)
        {
            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if (backgroundAccessStatus == BackgroundAccessStatus.Denied)
            {
                this._backgroudError.Visibility = Visibility.Visible;
            }

            base.InitializeProgressIndicator();
        }

        private SettingsViewModel VM
        {
            get { return base.DataContext as SettingsViewModel; }
        }

        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (this.NeedSync)
            {
                AccountService.Instance.SavePushSettings(PushNotifications.Instance.GetHardwareID, (result)=> {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        GenericInfoUC.ShowBasedOnResult(LocalizedStrings.GetString("Saved"), result.error);
                    });
                });
            }
        }

        private void CancelDNDClick(object sender, RoutedEventArgs e)
        {
            this.VM.Disable(0);
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Point point = e.GetPosition(null);

            if (_pop == null)
            {
                _pop = new UC.PopUP();
                _pop.ItemTapped += _picker_ItemTapped;
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

            this.VM.Disable(hour * 3600);
        }

        private void _Checked(object sender, RoutedEventArgs args)
        {
            this.NeedSync = true;
        }

        private void PushNotifications_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            this._silentBtn.Visibility = cb.SelectedIndex > 0 ? Visibility.Visible : Visibility.Collapsed;
            this._panelCustomPushNotificationsServer.Visibility = cb.SelectedIndex > 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RestoreServer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.VM.CustomPushNotificationsServer = "http://www.xn-----nlcaiebdb9andydgfuq5v.xn--p1ai/wns.php?channel=";
        }

        private void ConfigureNewsSourcesTap(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToManageSources(false);
        }
    }
}
