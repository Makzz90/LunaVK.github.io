using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using LunaVK.Framework;
using LunaVK.Core.Enums;
using LunaVK.Core.Utils;
using LunaVK.Core;
using LunaVK.ViewModels;
using LunaVK.Library;
using LunaVK.Core.DataObjects;
using LunaVK.Common;

namespace LunaVK
{
    public sealed partial class GroupManagementPage : PageBase
    {
        PopUpService popService;
        

        public GroupManagementPage()
        {
            this.InitializeComponent();
            this.Loaded += GroupManagementPage_Loaded;
        }

        private GroupManagementViewModel VM
        {
            get { return base.DataContext as GroupManagementViewModel; }
        }

        private void GroupManagementPage_Loaded(object sender, RoutedEventArgs e)
        {
            base.Title = LocalizedStrings.GetString("GroupsListPage_Manage/Content");

            this.VM.LoadData();
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            Dictionary<string, object> QueryString = e.Parameter as Dictionary<string, object>;
            uint groupId = (uint)QueryString["CommunityId"];
            VKGroupType type = (VKGroupType)QueryString["CommunityType"];
            VKAdminLevel adminLevel = (VKAdminLevel)QueryString["AdminLevel"];

            if(type != VKGroupType.Group)
            {
                this._nav.Items.Remove(this._navItemRequests);
                this._pivot.Items.Remove(this._pivotItemRequests);
            }

            if(adminLevel != VKAdminLevel.Admin)
            {
                this._nav.Items.Remove(this._navInformation);
                this._nav.Items.Remove(this._navServices);
                this._nav.Items.Remove(this._navManagers);
                this._nav.Items.Remove(this._navLinks);

                this._pivot.Items.Remove(this._pivotItemInformation);
                this._pivot.Items.Remove(this._pivotItemServices);
                this._pivot.Items.Remove(this._pivotItemManagers);
                this._pivot.Items.Remove(this._pivotItemLinks);
            }

            base.DataContext = new GroupManagementViewModel(groupId,type,adminLevel);

            base.HandleOnNavigatedTo(e);

        }

        private void Requests_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            Library.NavigatorImpl.Instance.NavigateToCommunityManagementRequests(this.VM.Id);
        }

        private void Members_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            Library.NavigatorImpl.Instance.NavigateToCommunitySubscribers(this.VM.Id,this.VM.Type);
        }

        private void Managers_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            Library.NavigatorImpl.Instance.NavigateToCommunityManagementManagers(this.VM.Id);
        }

        private void Information_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            Library.NavigatorImpl.Instance.NavigateToCommunityManagementInformation(this.VM.Id);
        }

        private void Blacklist_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            Library.NavigatorImpl.Instance.NavigateToCommunityManagementBlacklist(this.VM.Id);
        }

        private void Links_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            Library.NavigatorImpl.Instance.NavigateToCommunityManagementLinks(this.VM.Id);
        }

        private void Services_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            Library.NavigatorImpl.Instance.NavigateToCommunityManagementServices(this.VM.Id);
        }

        private void Invitations_OnClicked(object sender, TappedRoutedEventArgs e)
        {

        }





        

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }

        private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//bug
            //this.VM.Information.RefreshUI();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            this.VM.SaveChanges();
        }

        private void SaveServices_Click(object sender, RoutedEventArgs e)
        {
            this.VM.ServicesVM.SaveChanges();
        }

        private void SetAgeLimitsButton_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            //if (!this.ViewModel.ParentViewModel.IsFormEnabled)
            //    return;
            this.VM.AgeLimitsVM.From16Only = true;
        }

        private void EditButton_OnClicked(object sender, TappedRoutedEventArgs e)
        {
           // if (!this.ViewModel.ParentViewModel.IsFormEnabled)
           //     return;
            e.Handled = true;
            //Navigator.Current.NavigateToCommunityManagementPlacementSelection(this.ParentViewModel.CommunityId, this._place);
        }

        private void WallOption_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            var popUC = new UC.PopUp.ServiceSwitchUC(ServicesViewModel.CommunityService.Wall, this.VM.ServicesVM.WallOrComments);
            popService = new PopUpService { Child = popUC };

            popService.OverrideBackKey = true;
            popService.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            popService.Show();

            popUC.SelectTap = (number) =>
            {
                this.VM.ServicesVM.WallOrComments = (ServicesViewModel.CommunityServiceState)number;
                popService.Hide();
            };

            //Navigator.Current.NavigateToCommunityManagementServiceSwitch(ServicesViewModel.CommunityService.Wall, this.VM.ServicesVM.WallOrComments);
        }

        private void PhotosOption_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            var popUC = new UC.PopUp.ServiceSwitchUC(ServicesViewModel.CommunityService.Photos, this.VM.ServicesVM.Photos);
            popService = new PopUpService { Child = popUC };

            popService.OverrideBackKey = true;
            popService.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            popService.Show();

            popUC.SelectTap = (number) =>
            {
                this.VM.ServicesVM.Photos = (ServicesViewModel.CommunityServiceState)number;
                popService.Hide();
            };

            //Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Photos, this.ViewModel.Photos);
        }

        private void VideosOption_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            var popUC = new UC.PopUp.ServiceSwitchUC(ServicesViewModel.CommunityService.Videos, this.VM.ServicesVM.Videos);
            popService = new PopUpService { Child = popUC };

            popService.OverrideBackKey = true;
            popService.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            popService.Show();

            popUC.SelectTap = (number) =>
            {
                this.VM.ServicesVM.Videos = (ServicesViewModel.CommunityServiceState)number;
                popService.Hide();
            };

            //Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Videos, this.ViewModel.Videos);
        }

        private void AudiosOption_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            var popUC = new UC.PopUp.ServiceSwitchUC(ServicesViewModel.CommunityService.Audios, this.VM.ServicesVM.Audios);
            popService = new PopUpService { Child = popUC };

            popService.OverrideBackKey = true;
            popService.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            popService.Show();

            popUC.SelectTap = (number) =>
            {
                this.VM.ServicesVM.Audios = (ServicesViewModel.CommunityServiceState)number;
                popService.Hide();
            };

            // Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Audios, this.ViewModel.Audios);
        }

        private void DocumentsOption_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            var popUC = new UC.PopUp.ServiceSwitchUC(ServicesViewModel.CommunityService.Documents, this.VM.ServicesVM.Documents);
            popService = new PopUpService { Child = popUC };

            popService.OverrideBackKey = true;
            popService.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            popService.Show();

            popUC.SelectTap = (number) =>
            {
                this.VM.ServicesVM.Documents = (ServicesViewModel.CommunityServiceState)number;
                popService.Hide();
            };

            // Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Documents, this.ViewModel.Documents);
        }

        private void DiscussionsOption_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            var popUC = new UC.PopUp.ServiceSwitchUC(ServicesViewModel.CommunityService.Discussions, this.VM.ServicesVM.Discussions);
            popService = new PopUpService { Child = popUC };

            popService.OverrideBackKey = true;
            popService.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            popService.Show();

            popUC.SelectTap = (number) =>
            {
                this.VM.ServicesVM.Discussions = (ServicesViewModel.CommunityServiceState)number;
                popService.Hide();
            };

            //  Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Discussions, this.ViewModel.Discussions);
        }

        private void ArticlesOption_OnClicked(object sender, TappedRoutedEventArgs e)
        {

        }

        private void MarketOption_OnClicked(object sender, TappedRoutedEventArgs e)
        {

        }












        private void MoreInformation_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            string uri = "https://"+ (CustomFrame.Instance.IsDevicePhone ? "m." : "") +"vk.com/agelimits?api_view=1";
            //string lang = LangHelper.GetLang();
            //if (!string.IsNullOrEmpty(lang))
            //    uri += string.Format("&lang={0}", lang);
            NavigatorImpl.Instance.NavigateToWebUri(uri, true);
        }

















        private void BaseProfileItem_Tapped(object sender, RoutedEventArgs e)
        {
            //e.Handled = true;
            VKUser vm = (sender as FrameworkElement).DataContext as VKUser;
            Library.NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }
        

        private void BaseProfileItem_PrimaryClick(object sender, RoutedEventArgs e)
        {
            Button element = sender as Button;
            VKUser vm = element.DataContext as VKUser;
            element.IsEnabled = false;
            this.VM.RequestsVM.AddUser(vm);//todo:callback
            element.IsEnabled = true;
        }

        private void BaseProfileItem_SecondaryClick(object sender, RoutedEventArgs e)
        {
            Button element = sender as Button;
            VKUser vm = element.DataContext as VKUser;
            element.IsEnabled = false;
            this.VM.RequestsVM.DeleteUser(vm);//todo:callback
            element.IsEnabled = true;
        }

        private void BaseProfileItem_ThirdClick(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKUser;

            VKGroupContact groupContact = this.VM.ManagersVM.Contacts == null ? null : this.VM.ManagersVM.Contacts.FirstOrDefault(c => c.user_id == vm.id);

            UC.ManagerEditingUC control = new UC.ManagerEditingUC(this.VM.Id, vm, true, groupContact);

            PopUpService statusChangePopup = new PopUpService
            {
                Child = control
            };
            statusChangePopup.OverrideBackKey = true;
            statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            statusChangePopup.Show();
        }

        private void OnEditClicked(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKUser;

            UC.BlockEditingUC control = new UC.BlockEditingUC(this.VM.Id, vm, vm.ban_info.Manager);

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

            this.VM.BlacklistVM.UnblockUser(vm);
        }




        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot pivot = sender as Pivot;
            if (pivot.SelectedIndex==2)
            {
                if(!this.VM.StatsVM.IsLoaded)
                    this.VM.StatsVM.LoadData(0);
            }
            else if((pivot.SelectedItem as PivotItem).DataContext is GroupCallbackServerViewModel vm)
            {
                vm.LoadData();
            }
        }
        
        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            var vm = element.DataContext as VKGroupLink;
            /*
            PopUP2 menu = new PopUP2();
            PopUP2.PopUpItem item = new PopUP2.PopUpItem() { Text = "Изменить" };

            //ContextMenu_OnEditClicked
            item.Command = new DelegateCommand(this.ContextMenu_OnEditClicked);
            menu.Items.Add(item);



            PopUP2.PopUpItem item2 = new PopUP2.PopUpItem() { Text = "Удалить" };
            //ContextMenu_OnDeleteClicked
            item2.Command = new DelegateCommand((args) =>
            {
                //if (linkHeader == null || MessageBox.Show(CommonResources.GenericConfirmation, CommonResources.LinkRemoving, (MessageBoxButton)1) != MessageBoxResult.OK)
                //    return;
                this.VM.DeleteLink(vm);
            });
            menu.Items.Add(item2);



            menu.ShowAt(element);*/
        }

        private void PeriodComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            cb.SelectionChanged += Cb_SelectionChanged;
        }

        private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            this.VM.StatsVM.LoadData((uint)cb.SelectedIndex);
        }

        private void AddLink_Click(object sender, RoutedEventArgs e)
        {
            var popUC = new UC.PopUp.LinkCreationUC(this.VM.Id);
            popService = new PopUpService { Child = popUC };

            popService.OverrideBackKey = true;
            popService.AnimationTypeChild = PopUpService.AnimationTypes.Slide;
            popService.Show();

            popUC.Done = (link) =>
            {
                this.VM.LinksVM.AddLink(link);
                popService.Hide();
            };
            
        }

        private async void ContextMenu_OnDeleteClicked(object sender, RoutedEventArgs e)
        {
            if(await MessageBox.Show("DeleteConfirmation", "LinkRemoving", MessageBox.MessageBoxButton.OKCancel) != MessageBox.MessageBoxButton.OK)
                return;

            var vm = (sender as FrameworkElement).DataContext as VKGroupLink;
            this.VM.LinksVM.DeleteLink(vm);
        }

        private void ContextMenu_OnEditClicked(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKGroupLink;

            var popUC = new UC.PopUp.LinkCreationUC(this.VM.Id, vm);
            popService = new PopUpService { Child = popUC };

            popService.OverrideBackKey = true;
            popService.AnimationTypeChild = PopUpService.AnimationTypes.Slide;
            popService.Show();

            popUC.Done = (link) =>
            {
                this.VM.LinksVM.AddLink(link);
                popService.Hide();
            };
        }

        private void Link_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKGroupLink;
            NavigatorImpl.Instance.NavigateToWebUri(vm.url);
        }

        private void Button_ClickAdd(object sender, RoutedEventArgs e)
        {
//            this._nav.Items.Insert(0,this._nav01);
            //this._pivot.Items.Insert(0,this._item01);
        }

        private void Button_ClickRemove(object sender, RoutedEventArgs e)
        {
 //           this._nav.Items.Remove(this._nav01);
            //this._pivot.Items.Remove(this._item01);
        }
    }
}
