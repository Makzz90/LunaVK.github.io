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
using LunaVK.Library;
using LunaVK.Common;
using LunaVK.Core;

namespace LunaVK
{
    /// <summary>
    /// SettingsEditProfilePage
    /// </summary>
    public sealed partial class SettingsAccountPage : PageBase
    {
        public SettingsAccountPage()
        {
            base.DataContext = new SettingsAccountViewModel();
            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("NewSettings_Account/Title");

            this.VM.LoadData();
        }

        private SettingsAccountViewModel VM
        {
            get { return base.DataContext as SettingsAccountViewModel; }
        }

        private void NewsFilterTap(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToManageSources(true);
        }

        private void ChosePhotoMenuClick(object sender, RoutedEventArgs e)
        {
            this.DoChoosePhoto();
        }

        private async void DeletePhotoMenuClick(object sender, RoutedEventArgs e)
        {
            if (await MessageBox.Show("DeleteConfirmation", "DeleteOnePhoto") != MessageBox.MessageBoxButton.OK)
                return;
            this.VM.DeletePhoto();
        }

        private  void ChoosePhotoTap(object sender, TappedRoutedEventArgs e)
        {
            
        }

        private void DoChoosePhoto()
        {
            NavigatorImpl.Instance.NavigateToPhotoPickerPhotos(1, null);
        }

        private void Ellipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void PhoneNumberTap(object sender, TappedRoutedEventArgs e)
        {
            this.VM.HandlePhoneNumberTap();
        }

        private void EmailTap(object sender, TappedRoutedEventArgs e)
        {
            this.VM.HandleEmailTap();
        }

        private void ShortNameTap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void CountryPicker_OnTap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void CityPicker_OnTap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void ChangePasswordTap(object sender, RoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToChangePassword();
        }

        private void ChoosePartnerTap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void PartnerTap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void RemovePartnerTap(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
