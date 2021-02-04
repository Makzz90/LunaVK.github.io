using LunaVK.Core.DataObjects;
using LunaVK.Library;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace LunaVK.Pages
{
    public sealed partial class LikesPage : PageBase
    {
        public LikesPage()
        {
            this.InitializeComponent();
        }

        private LikesViewModel VM
        {
            get { return base.DataContext as LikesViewModel; }
        }

        private void BaseProfileItem_BackTap(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKUser;
            NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                //this.listBoxBanned.NeedReload = false;
                if (this.VM.AllVM.Items.Count > 0)
                    this.listBoxAll.NeedReload = false;
                if (this.VM.FriendsVM.Items.Count > 0)
                    this.listBoxFriends.NeedReload = false;
                if (this.VM.SharedVM.Items.Count > 0)
                    this.listBoxShared.NeedReload = false;
            }
            else
            {
                Dictionary<string, object> QueryString = navigationParameter as Dictionary<string, object>;
                int ownerId = (int)QueryString["OwnerId"];
                uint itemId = (uint)QueryString["ItemId"];
                byte type = (byte)QueryString["Type"];
                int knownCount = (int)QueryString["KnownCount"];
                bool selectFriendLikes = (bool)QueryString["SelectFriendLikes"];

                base.DataContext = new LikesViewModel(ownerId,itemId, (Core.Enums.LikeObjectType)type, knownCount);
            }

            base.Title = this.VM.Title;
        }
        
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this.VM;
        }
    }
}
