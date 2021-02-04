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

using LunaVK.Network;
using LunaVK.Core;
using Windows.Storage.Pickers;
using Windows.Storage;

namespace LunaVK
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class SettingsGeneralPage : PageBase
    {
        public SettingsGeneralPage()
        {
            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("SettingsGeneral");
            base.DataContext = new ViewModels.SettingsViewModel();

            this._switchProxy.IsChecked = Settings.UseProxy;
        }

        private ViewModels.SettingsViewModel VM
        {
            get { return base.DataContext as ViewModels.SettingsViewModel; }
        }

        //private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    TextBox box = sender as TextBox;
        //    if (string.IsNullOrEmpty(box.Text))
        //        error.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        //    else
        //    {
        //        Uri u;
        //        bool noerror = Uri.TryCreate(box.Text, UriKind.Absolute, out u);
        //        error.Visibility = noerror ? Visibility.Collapsed : Visibility.Visible;
        //    }

        //}
        /*
        private async void Checked_Changed(object sender, RoutedEventArgs args)
        {
            
    //          public static String getProxyAddr() {
    //    return PreferenceManager.getDefaultSharedPreferences(KApplication.current).getString("proxy_addr", "proxy.katemobile.ru");
    //}

    //public static Integer getProxyPort() {
    //    return Integer.valueOf(Integer.parseInt(PreferenceManager.getDefaultSharedPreferences(KApplication.current).getString("proxy_port", "3752")));
    //}
             
            UC.ToggleSwitch t = sender as UC.ToggleSwitch;
            if (t.IsChecked)
            {
                this._panelStatus.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this._textStatus.Text = "Проверка Proxy-сервера (1)";
                //bool proxy = await RequestsDispatcher.Ping("userapi.com");
                //if(proxy)
                //{
                //    this._textStatus.Text = "(1) успешно";
                //    Settings.ProxyAdress = "userapi.com";
                //    Settings.UseProxy = true;
                //}
            }
            else
            {
                this._panelStatus.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                Settings.UseProxy = false;
            }
        }
*/
        private async void BorderDoc_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var diagFolder = new FolderPicker() { SuggestedStartLocation = PickerLocationId.Downloads };
            diagFolder.FileTypeFilter.Add("*");
            var outputFolder = await diagFolder.PickSingleFolderAsync();
            if (outputFolder == KnownFolders.DocumentsLibrary)
                outputFolder = KnownFolders.AppCaptures;
            if (outputFolder != null)
            {
                this.VM.SaveFolderDoc = outputFolder.Path;
            }
        }

        private async void BorderPhoto_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var diagFolder = new FolderPicker() { SuggestedStartLocation = PickerLocationId.PicturesLibrary };
            diagFolder.FileTypeFilter.Add("*");
            var outputFolder = await diagFolder.PickSingleFolderAsync();
            if (outputFolder != null)
            {
                this.VM.SaveFolderPhoto = outputFolder.Path;
            }
        }


        private async void BorderVoice_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var diagFolder = new FolderPicker() { SuggestedStartLocation = PickerLocationId.MusicLibrary };
            diagFolder.FileTypeFilter.Add("*");
            var outputFolder = await diagFolder.PickSingleFolderAsync();
            if (outputFolder != null)
            {
                this.VM.SaveFolderVoice = outputFolder.Path;
            }
        }

        private async void BorderVideo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var diagFolder = new FolderPicker() { SuggestedStartLocation = PickerLocationId.VideosLibrary };
            diagFolder.FileTypeFilter.Add("*");
            var outputFolder = await diagFolder.PickSingleFolderAsync();
            if (outputFolder != null)
            {
                this.VM.SaveFolderVideo = outputFolder.Path;
            }
        }
    }
}
