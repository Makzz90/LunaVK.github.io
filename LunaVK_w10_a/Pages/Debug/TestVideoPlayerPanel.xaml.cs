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

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestVideoPlayerPanel : Page
    {
        public TestVideoPlayerPanel()
        {
            this.InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SettingFlyout_Opened(object sender, object e)
        {

        }

        private void ControlPanelVisibilityStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            int i = 0;
        }

        private void ProgressSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {

        }

        private void Back_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {

        }

        private void MediaControlsCommandBar2_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void StackPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
        }

        private void StackPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
        }

        private void Volume_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
