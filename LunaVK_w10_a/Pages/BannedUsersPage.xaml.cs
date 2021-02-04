using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace LunaVK.Pages
{
    public sealed partial class BannedUsersPage : PageBase
    {
        private List<VKBaseDataForGroupOrUser> list = new List<VKBaseDataForGroupOrUser>();

        public BannedUsersPage()
        {
            this.InitializeComponent();

            base.Title = LocalizedStrings.GetString("NewSettings_BlackList/Title");


            base.DataContext = new BannedUsersViewModel(); ;
            this.Loaded += BannedUsersPage_Loaded;
        }

        private void BannedUsersPage_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE74D", Clicked = this._appBarButtonDelete_Click });
        }

        public BannedUsersViewModel VM
        {
            get { return base.DataContext as BannedUsersViewModel; }
        }


        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                this.listBoxBanned.NeedReload = false;
            }
            else
            {
                if (base.DataContext == null)
                    base.DataContext = new BannedUsersViewModel();
            }
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this.VM;
        }
        
        private void _appBarButtonDelete_Click(object sender)
        {
            if (this.list.Count == 0)
                return;
            
            this.VM.DeleteSelected(this.list);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKBaseDataForGroupOrUser;
            this.list.Add(vm);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKBaseDataForGroupOrUser;
            this.list.Remove(vm);
        }

        private void BaseProfileItem_BackTap(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKBaseDataForGroupOrUser;
            NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }
    }
}
