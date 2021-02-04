using LunaVK.Library;
using LunaVK.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.UC
{
    public sealed partial class CommunityCreationUC : UserControl
    {
        public CommunityCreationUC()
        {
            this.InitializeComponent();
            base.DataContext = new CommunityCreationViewModel();
        }

        private CommunityCreationViewModel VM
        {
            get { return base.DataContext as CommunityCreationViewModel; }
        }

        private void TermsLink_OnClicked(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            NavigatorImpl.Instance.NavigateToWebUri("https://m.vk.com/terms", true);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.VM.CreateCommunity();
        }
    }
}
