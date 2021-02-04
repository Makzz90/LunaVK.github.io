using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using LunaVK.ViewModels;

namespace LunaVK.UC
{
    /// <summary>
    /// Это контейнер с диалогами.
    /// Он вечно висит в памяти.
    /// </summary>
    public sealed partial class ConversationsUC : UserControl
    {
        public event RoutedEventHandler AvatrTap;
        public event RoutedEventHandler BackTap;
        public event RoutedEventHandler ShowMenuTap;

        public ConversationsUC()
        {
            this.InitializeComponent();
            base.DataContext = DialogsViewModel.Instance;
        }

        public Controls.ExtendedListView3 ExtendedListView
        {
            get { return this._exListView; }
        }

        private void ItemDialogUC_Holding(object sender, HoldingRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                this.ShowMenuTap.Invoke(sender, e);
            }
        }

        private void ItemDialogUC_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                return;

            this.ShowMenuTap.Invoke(sender, e);
        }

        private void ItemDialogUC_BackTap(object sender, RoutedEventArgs e)
        {
            this.BackTap.Invoke(sender, e);
        }

        private void ItemDialogUC_AvatrTap(object sender, RoutedEventArgs e)
        {
            this.AvatrTap.Invoke(sender, e);
        }

    }
}
