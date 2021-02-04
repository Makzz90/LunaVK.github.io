using LunaVK.Common;
using LunaVK.Core;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.Pages
{
    public sealed partial class SettingsEditProfilePage : PageBase
    {
        private OptionsMenuItem applicationBarIconButton1;

        public SettingsEditProfilePage()
        {
            this.InitializeComponent();
            this.Loaded += this.SettingsEditProfilePage_Loaded;

            this.applicationBarIconButton1 = new OptionsMenuItem() { Icon = "\xE73E", Clicked = this._appBarButtonCheck_Click };

        }

        private void SettingsEditProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            base.Title = LocalizedStrings.GetString("Settings_EditProfile_Title");
        }

        private SettingsEditProfileViewModel VM
        {
            get { return base.DataContext as SettingsEditProfileViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            
                base.DataContext = new SettingsEditProfileViewModel();
            this.VM.Load();
            this.VM.PropertyChanged += this.vm_PropertyChanged;
        }

        private void vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CanSave")
                this.UpdateAppBar();
            else if(e.PropertyName == "IsLoaded")
            {
                VisualStateManager.GoToState(this._loading, this.VM.IsLoaded ? "Loaded" : "Reloading", true);
            }

        }
        
        private void _appBarButtonCheck_Click(object sender)
        {
            this.VM.Save();
        }

        private void UpdateAppBar()
        {
            if (this.VM.CanSave)
            {
                if (!CustomFrame.Instance.Header.OptionsMenu.Contains(this.applicationBarIconButton1))
                    CustomFrame.Instance.Header.OptionsMenu.Add(this.applicationBarIconButton1);
            }
            else
            {
                if (CustomFrame.Instance.Header.OptionsMenu.Contains(this.applicationBarIconButton1))
                    CustomFrame.Instance.Header.OptionsMenu.Remove(this.applicationBarIconButton1);
            }
        }
        
        private void CancelNameRequestButtonTap(object sender, TappedRoutedEventArgs e)
        {
            this.VM.CancelNameRequest();
        }

        private void ChoosePhotoTap(object sender, TappedRoutedEventArgs e)
        {
            this.DoChoosePhoto();
        }

        private void GridPhotoTap(object sender, TappedRoutedEventArgs e)
        {
            this.PhotoMenu.ShowAt(sender as FrameworkElement);
        }

        private void BirthdayTap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void ChoosePartnerTap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void PartnerTap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void RemovePartnerTap(object sender, TappedRoutedEventArgs e)
        {
            this.VM.Partner = null;
        }

        private void CountryPicker_OnTap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void CityPicker_OnTap(object sender, TappedRoutedEventArgs e)
        {

        }
        
        

        private void TextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {

        }


        private void DoChoosePhoto()
        {
            //Navigator.Current.NavigateToPhotoPickerPhotos(1, true, false);
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
    }
}
