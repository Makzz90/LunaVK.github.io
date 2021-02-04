using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.UC
{
    public sealed partial class ItemGroupInvitationUC : UserControl
    {
        /// <summary>
        /// CommunityInvitationUC
        /// </summary>
        public ItemGroupInvitationUC()
        {
            this.InitializeComponent();
        }

        public event RoutedEventHandler JoinClick
        {
            add { this.JoinBtn.Click += value; }
            remove { this.JoinBtn.Click -= value; }
        }

        public event RoutedEventHandler HideClick
        {
            add { this.HideBtn.Click += value; }
            remove { this.HideBtn.Click -= value; }
        }
    }
}
