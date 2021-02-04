using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LunaVK.UC
{
    //ConversationHeaderUC
    public sealed partial class ItemDialogUC : UserControl
    {
        public ItemDialogUC()
        {
            this.InitializeComponent();
        }

        private void Avatar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this._avaClick?.Invoke(sender, null);
        }

        private void Back_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this._backClick?.Invoke(sender, null);
        }



        private event RoutedEventHandler _backClick;
        public event RoutedEventHandler BackTap
        {
            add { this._backClick += value; }
            remove { this._backClick -= value; }
        }

        private event RoutedEventHandler _avaClick;
        public event RoutedEventHandler AvatrTap
        {
            add { this._avaClick += value; }
            remove { this._avaClick -= value; }
        }

        
    }
}
