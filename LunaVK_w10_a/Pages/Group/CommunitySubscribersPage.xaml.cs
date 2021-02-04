using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using LunaVK.Core.DataObjects;
using LunaVK.Framework;
using LunaVK.Core.ViewModels;
using LunaVK.Core;
using LunaVK.Core.Enums;
using LunaVK.Library;
using System.Collections.Generic;

namespace LunaVK.Pages.Group
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class CommunitySubscribersPage : PageBase
    {
        public CommunitySubscribersPage()
        {
            this.InitializeComponent();
            //base.Title = LocalizedStrings.GetString("Management_Members");
        }

        public CommunitySubscribersViewModel VM
        {
            get { return base.DataContext as CommunitySubscribersViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            Dictionary<string, object> QueryString = e.Parameter as Dictionary<string, object>;
            uint groupId = (uint)QueryString["CommunityId"];
            VKGroupType type = (VKGroupType)QueryString["CommunityType"];
            this.DataContext = new CommunitySubscribersViewModel(groupId, type);
            //SUBSCRIBERS
            base.Title = LocalizedStrings.GetString(type != VKGroupType.Page ? "Management_Members" : "Management_Subscribers");

            if(type != VKGroupType.Event)
            {
                this._pivot.Items.Remove(this.PivotItemUnsure);
            }
        }

        private void BaseProfileItem_ThirdClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            this.ShowMenu(element);
        }

        private void ShowMenu(FrameworkElement element)
        {
            var vm = element.DataContext as VKUser;

            if (vm.id == Settings.UserId)
                return;

            MenuFlyout menu = new MenuFlyout();

            if (vm.role == CommunityManagementRole.Unknown && (this.VM.currentUserRole == CommunityManagementRole.Administrator || this.VM.currentUserRole == CommunityManagementRole.Creator))
            {
                MenuFlyoutItem item = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("AddToManagers") };
                item.Command = new DelegateCommand((args) =>
                {
                    //NavigatorImpl.Instance.NavigateToCommunityManagementManagerAdding(this.VM.GroupId, this.VM.CommunityType, dataContext.User, false);

                    UC.ManagerEditingUC control = new UC.ManagerEditingUC(this.VM.GroupId,vm,false,null);

                    PopUpService statusChangePopup = new PopUpService
                    {
                        Child = control
                    };
                    statusChangePopup.OverrideBackKey = true;
                    statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
                    statusChangePopup.Show();
                });
                menu.Items.Add(item);
            }

            if (vm.role != CommunityManagementRole.Unknown && (this.VM.currentUserRole == CommunityManagementRole.Administrator || this.VM.currentUserRole == CommunityManagementRole.Creator))
            {
                /*
                MenuFlyoutItem item = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("Edit") };
                item.Command = new DelegateCommand((args) =>
                {
                    var groupContact = this.VM.Contacts.FirstOrDefault(c => c.user_id == vm.id);

                    UC.ManagerEditingUC control = new UC.ManagerEditingUC(this.VM.GroupId, vm, true, groupContact);

                    PopUpService statusChangePopup = new PopUpService
                    {
                        Child = control
                    };
                    statusChangePopup.OverrideBackKey = true;
                    statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
                    statusChangePopup.Show();
                });
                menu.Items.Add(item);
                */
            }
            else
            {
                MenuFlyoutItem item = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("BannedUsers_BanUser") };
                item.Command = new DelegateCommand((args) =>
                {
                    //NavigatorImpl.Instance.NavigateToCommunityManagementBlockAdding(this.VM.GroupId, vm, true);

                    UC.BlockEditingUC control = new UC.BlockEditingUC(this.VM.GroupId, vm, null);

                    PopUpService statusChangePopup = new PopUpService
                    {
                        Child = control
                    };
                    statusChangePopup.OverrideBackKey = true;
                    statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
                    statusChangePopup.Show();
                });
                menu.Items.Add(item);
            }

            if ((vm.role == CommunityManagementRole.Unknown || (this.VM.currentUserRole == CommunityManagementRole.Administrator && vm.id != Settings.UserId) || this.VM.currentUserRole == CommunityManagementRole.Creator))
            {
                MenuFlyoutItem item = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("RemoveFromCommunity")/*, Icon = new SymbolIcon(Symbol.List)*/ };
                item.Command = new DelegateCommand((args) =>
                {
                    /*
                    MessageBox.Show(CommonResources.GenericConfirmation, CommonResources.RemovingFromCommunity, (MessageBoxButton)1) != MessageBoxResult.OK)
                        return;
                    */
                    this.VM.RemoveFromCommunity(vm);
                });
                menu.Items.Add(item);
            }

            menu.ShowAt(element);
        }

        private void BaseProfileItem_BackTap(object sender, RoutedEventArgs e)
        {
            VKUser vm = (sender as FrameworkElement).DataContext as VKUser;
            NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }
    }
}
