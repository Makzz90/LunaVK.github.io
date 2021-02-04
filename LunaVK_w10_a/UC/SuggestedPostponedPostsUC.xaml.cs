using LunaVK.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LunaVK.UC
{
    public sealed partial class SuggestedPostponedPostsUC : UserControl
    {
        public SuggestedPostponedPostsUC()
        {
            this.InitializeComponent();
        }

        private SuggestedPostponedPostsViewModel VM
        {
            get { return base.DataContext as SuggestedPostponedPostsViewModel; }
        }

        private void BorderSuggested_OnTap(object sender, TappedRoutedEventArgs e)
        {
            this.VM.OpenSuggestedPostsPage();
        }

        private void BorderPostponed_OnTap(object sender, TappedRoutedEventArgs e)
        {
            this.VM.OpenPostponedPostsPage();
        }
    }
}
