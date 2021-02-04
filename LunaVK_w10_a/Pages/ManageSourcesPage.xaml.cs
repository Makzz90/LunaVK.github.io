using LunaVK.Core.DataObjects;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.Pages
{
    public sealed partial class ManageSourcesPage : PageBase
    {
        private List<VKUser> listUsers = new List<VKUser>();
        private List<VKGroup> listGroups = new List<VKGroup>();

        public ManageSourcesPage()
        {
            this.InitializeComponent();
            this.Loaded += this.ManageSourcesPage_Loaded;
        }

        private void ManageSourcesPage_Loaded(object sender, RoutedEventArgs e)
        {
            base.Title = this.VM.Title;
            CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE74D", Clicked = this._appBarButtonDelete_Click });

        }

        private ManageSourcesViewModel VM
        {
            get { return base.DataContext as ManageSourcesViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
                ManageSourcesViewModel sourcesViewModel = new ManageSourcesViewModel((bool)e.Parameter);
                base.DataContext = sourcesViewModel;


                //sourcesViewModel.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
                //sourcesViewModel.FriendsVM.LoadData(false, false, null, false);
                //sourcesViewModel.GroupsVM.LoadData(false, false, null, false);
                //this.BuildAppBar();
            //this.UpdateAppBar();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var user = (sender as FrameworkElement).DataContext as VKUser;
            if(user != null)
                this.listUsers.Add(user);
            else
            {
                var group = (sender as FrameworkElement).DataContext as VKGroup;
                if (group != null)
                    this.listGroups.Add(group);
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var user = (sender as FrameworkElement).DataContext as VKUser;
            if (user != null)
                this.listUsers.Remove(user);
            else
            {
                var group = (sender as FrameworkElement).DataContext as VKGroup;
                if (group != null)
                    this.listGroups.Remove(group);
            }
        }

        private void _appBarButtonDelete_Click(object sender)
        {
            if (this._pivot.SelectedIndex == 0)
            {
                if (this.listUsers.Count == 0)
                    return;
                this.VM.UsersVM.DeleteSelected(this.listUsers);
            }
            else
            {
                if (this.listGroups.Count == 0)
                    return;
                this.VM.GroupsVM.DeleteSelected(this.listGroups);
            }
        }

        private void BaseProfileItem_BackTap(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKBaseDataForGroupOrUser;
            NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }

        private void ItemGroupUC_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.BaseProfileItem_BackTap(sender, null);
        }

        private void ExtendedListView3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }
    }
}
