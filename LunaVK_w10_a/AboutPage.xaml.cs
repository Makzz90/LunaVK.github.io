using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using LunaVK.Core;
using LunaVK.Library;
using Windows.Foundation.Metadata;
using Windows.System.Profile;
using Windows.System;
using LunaVK.Core.Framework;

namespace LunaVK
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class AboutPage : PageBase
    {

        public AboutPage()
        {
            this.InitializeComponent();
            
            this.resolution.Text = string.Format("Разрешение окна: {0}x{1}epx", (int)(Window.Current.Content as Frame).ActualWidth, (int)(Window.Current.Content as Frame).ActualHeight);
            

            

            Windows.ApplicationModel.PackageVersion version = Windows.ApplicationModel.Package.Current.Id.Version;
#if WINDOWS_UWP
            string os = "UWP";
#elif WINDOWS_PHONE_APP
            string os = "WP8.1";
#else
            string os = "WIN8.1";
#endif

#if DEBUG
            string type = "Debug";
#else
            string type = "Release";
#endif

            this.ver.Text = string.Format("Версия: {0}.{1}.{2}.{3} {4} {5}", version.Major, version.Minor, version.Build, version.Revision, type, os);
            this.api.Text = string.Format("Версия API: {0}", VKConstants.API_VERSION);

            Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation information = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
            
            this.manufacturer.Text = string.Format("Производитель системы: {0}", information.SystemManufacturer);
            this.product.Text = string.Format("Название системы: {0}", information.SystemProductName);




            string sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong v = ulong.Parse(sv);
            ulong v1 = (v & 0xffff000000000000L) >> 48;
            ulong v2 = (v & 0x0000ffff00000000L) >> 32;
            ulong v3 = (v & 0x00000000ffff0000L) >> 16;
            ulong v4 = (v & 0x000000000000ffffL);
            


            this.os.Text = string.Format("ОС: {0} build {1}.{2}.{3}.{4}", information.OperatingSystem,v1,v2,v3,v4);

            var scale_ = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().ResolutionScale;
            this.scale.Text = string.Format("Масштаб в системе: {0}", scale_.ToString());

            this.family.Text = "Семейство: " + Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues["DeviceFamily"];

            int ver = 0;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 1))
            {
                ver = 1;
            }
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 2))
            {
                ver = 2;
            }
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
            {
                ver = 3;
            }
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4))
            {
                ver = 4;
            }
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                ver = 5;
            }
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 6))
            {
                ver = 6;
            }
            this.apicontract.Text = "This device supports all APIs in UniversalApiContract version " + ver;

            Window.Current.SizeChanged += Current_SizeChanged;

            this.Loaded += AboutPage_Loaded;
            this.Unloaded += AboutPage_Unloaded;
        }

        private void AboutPage_Loaded(object sender, RoutedEventArgs e)
        {
            base.Title = LocalizedStrings.GetString("NewSettings_About/Title");
            Core.Library.AppsService.Instance.GetApp(int.Parse(VKConstants.ApplicationID), (result)=>
            {
                if(result.error.error_code == Core.Enums.VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        this.users.Text = "Users count: " + result.response.items[0].members_count;
                        this.users.Visibility = Visibility.Visible;
                    });
                }
            });
        }

        private void AboutPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.resolution.Text = string.Format("Разрешение окна: {0}x{1}epx", (int)(Window.Current.Content as Frame).ActualWidth, (int)(Window.Current.Content as Frame).ActualHeight);
        }
        
        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToProfilePage(-154148777);
        }

        private void Border_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToWebUri("https://m.vk.com/privacy", true);
        }

        private void Border_Tapped_2(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToWebUri("https://m.vk.com/terms", true);
        }

        private void CustomIconUC_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var icon = sender as UC.CustomIconUC;
            icon.Glyph = "\xE94C";
            this.info.Visibility = Visibility.Visible;

            this.description.Text = "на самом деле, данное приложение является портом официального клиента ВКонтакте для Windows Phone 8.1, написанного [id19187792|Сергеем Москвиным]";
        }

        private void Me_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToProfilePage(460389);
        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            LauncherOptions options = new LauncherOptions();

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
            {
                options.IgnoreAppUriHandlers = true;
            }

            Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9P230PPRTDV4"), options);

        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }
    }
}
