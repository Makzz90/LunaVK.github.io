using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using LunaVK.ViewModels;
using LunaVK.Core.DataObjects;
using LunaVK.Framework;
using LunaVK.Core;

namespace LunaVK.Pages.Group.Management
{
    public sealed partial class BlacklistPage : PageBase
    {
        public BlacklistPage()
        {
            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("NewSettings_BlackList/Title");
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            uint Id = (uint)e.Parameter;
            this.DataContext = new BlacklistViewModel(Id);
        }

        private BlacklistViewModel VM
        {
            get { return base.DataContext as BlacklistViewModel; }
        }

        private void BaseProfileItem_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            VKUser vm = (sender as FrameworkElement).DataContext as VKUser;
            Library.NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }
        

        private void OnEditClicked(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKUser;

            UC.BlockEditingUC control = new UC.BlockEditingUC(this.VM.CommunityId, vm, vm.ban_info.Manager);

            PopUpService statusChangePopup = new PopUpService
            {
                Child = control
            };
            statusChangePopup.OverrideBackKey = true;
            statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            statusChangePopup.Show();
        }

        private void OnUnblockClicked(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKUser;

            this.VM.UnblockUser(vm);
        }
    }
}
