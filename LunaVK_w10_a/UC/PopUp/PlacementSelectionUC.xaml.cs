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

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236
//PlacementSelectionPage
namespace LunaVK.UC.PopUp
{
    public sealed partial class PlacementSelectionUC : UserControl
    {
#if WINDOWS_PHONE_APP || WINDOWS_UWP
        Windows.UI.Xaml.Controls.Maps.MapControl map;
#else
        Bing.Maps.Map map;
#endif

        public PlacementSelectionUC()
        {
            this.InitializeComponent();

#if WINDOWS_PHONE_APP || WINDOWS_UWP
            map = new Windows.UI.Xaml.Controls.Maps.MapControl() { Height = 240, Margin = new Thickness(0,10,0,0) };
            try
            {
                map.MapServiceToken = "ArEQxjMYhiZPLLAh74-gaBNvbvv9GPxCFKqv-7i9HHaHmxet0TMb6wheIxUB4Rd5";
            }
            catch
            {

            }

            // map.MapTapped += map_MapTapped;
#else
            map = new Bing.Maps.Map();
            map.Credentials = "AmOFJiHtBeytoES_ZZxm9yiLe8hYQaaC7GUO-Fi-yX7RhhZ8n7pyn-17I9SyDlG7";
            //map.Tapped += map_Tapped;
#endif
        }

        private void ViewerContent_Loaded(object sender, RoutedEventArgs e)
        {
            //MapsSettings.ApplicationContext.ApplicationId = ("55677f7c-3dab-4a57-95b2-4efd44a0e692");
            //MapsSettings.ApplicationContext.AuthenticationToken = ("1jh4FPILRSo9J1ADKx2CgA");
            StackPanel st = sender as StackPanel;
            st.Children.Add(this.map);
        }
    }
}
