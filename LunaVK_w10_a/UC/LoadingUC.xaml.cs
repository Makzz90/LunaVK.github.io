using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System;

namespace LunaVK.UC
{
    public sealed partial class LoadingUC : UserControl
    {
        public Action TryAgainCmd;

        public LoadingUC()
        {
            this.InitializeComponent();
        }
        
        private void buttonTryAgain_Click(object sender, RoutedEventArgs e)
        {
            this.TryAgainCmd?.Invoke();
        }

        private void _tryAgain_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this.TryAgainCmd?.Invoke();
        }
    }
}
