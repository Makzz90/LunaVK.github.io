using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Library;
using LunaVK.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class SubscriptionsPage : PageBase
    {
        public SubscriptionsPage()
        {
            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("SubscriptionsPage_OwnSubscriptionsTitle");
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.DataContext = new SubscriptionsViewModel((uint)e.Parameter);
        }

        private void BaseProfileItem_BackTap(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKGroup;
            NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }
    }
}
