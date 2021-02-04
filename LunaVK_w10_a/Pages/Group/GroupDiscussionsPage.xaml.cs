using LunaVK.Core.Enums;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

namespace LunaVK.Pages.Group
{
    /// <summary>
    /// Это страница со списком обсуждений
    /// </summary>
    public sealed partial class GroupDiscussionsPage : PageBase
    {
        public GroupDiscussionsPage()
        {
            this.InitializeComponent();
            this.Loaded += GroupDiscussionsPage_Loaded;
        }

        private void GroupDiscussionsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.VM.CanCreateDiscussion)
            {
                CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE710", Clicked = this._appBarButtonAdd_Click });
            }
        }

        public GroupDiscussionsViewModel VM
        {
            get { return base.DataContext as GroupDiscussionsViewModel; }
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                if (this.VM.Items.Count > 0)
                    this._exListView.NeedReload = false;
            }
            else
            {
                Dictionary<string, object> QueryString = navigationParameter as Dictionary<string, object>;
                int GroupId = (int)QueryString["GroupId"];
                string Name = (string)QueryString["Name"];
                VKAdminLevel AdminLevel = (VKAdminLevel)QueryString["AdminLevel"];
                bool IsPublicPage = (bool)QueryString["IsPublicPage"];
                bool CanCreateTopic = (bool)QueryString["CanCreateTopic"];
                base.Title = string.Format("Обсуждения | {0}", Name);

                this.DataContext = new GroupDiscussionsViewModel((uint)-GroupId, AdminLevel, IsPublicPage, CanCreateTopic);
            }
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this.VM;
        }

        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ThemeHeader vm = (sender as FrameworkElement).DataContext as ThemeHeader;
            if (vm == null)
                return;
            this.VM.NavigateToDiscusson(false, vm);
        }

        private void _appBarButtonAdd_Click(object sender)
        {
            NavigatorImpl.Instance.NavigateToNewWallPost( WallPostViewModel.Mode.NewTopic, (int)-this.VM._gid, this.VM._adminLevel, this.VM._isPublicPage);
        }

        private void _exListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }
    }
}
