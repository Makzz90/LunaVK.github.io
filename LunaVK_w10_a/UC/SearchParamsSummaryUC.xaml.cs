using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LunaVK.UC
{
    public sealed partial class SearchParamsSummaryUC : UserControl
    {
        public event EventHandler ClearButtonTap;

        public SearchParamsSummaryUC()
        {
            this.InitializeComponent();
        }

        private void Clear_OnTap(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this.ClearButtonTap?.Invoke(this, EventArgs.Empty);
        }
    }
}
