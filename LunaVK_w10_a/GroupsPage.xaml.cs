using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

using LunaVK.Core.ViewModels;
using LunaVK.Core.DataObjects;
using LunaVK.Core;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.UC;
using LunaVK.UC.Controls;
using LunaVK.ViewModels;
using Windows.UI.Xaml.Navigation;

namespace LunaVK
{
    /// <summary>
    /// GroupsListPage
    /// </summary>
    public sealed partial class GroupsPage : PageBase
    {
        private GroupsSearchViewModel searchViewModel = null;

        private int _selected;
        private double VerticalOffset = 0;

        public GroupsPage()
        {
            this.InitializeComponent();

            base.Title = LocalizedStrings.GetString("Menu_Communities/Content");

            this.communitiesListBox.Loaded += this.CommunitiesListBox_Loaded;
            this.eventsListBox.Loaded += this.EventsListBox_Loaded;
            this.manageListBox.Loaded += this.ManageListBox_Loaded;

            this.Loaded += this.GroupsPage_Loaded;
            CustomFrame.Instance.Header.SearchClosed = this.SearchClosed;
            CustomFrame.Instance.Header.ServerSearch = this.OnServerSearch;
            CustomFrame.Instance.Header.MoreSearchClicked += this.MoreOptionsClicked;
        }

        public GroupsListViewModel VM
        {
            get { return base.DataContext as GroupsListViewModel; }
        }

        private void InsideScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            if (this.VerticalOffset != 0)
                (sender as ScrollViewer).ChangeView(0, this.VerticalOffset, 1.0f);
        }

        private void OnServerSearch(string text)
        {
            this.searchViewModel.q = text;
            this.searchViewModel.Items.Clear();
            this.communitiesListBox.NeedReload = true;
            this.communitiesListBox.Reload();
        }

        private void GroupsPage_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE721", Clicked = this._appBarButtonSearch_Click });
            CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE774", Clicked = NavigatorImpl.Instance.NavigateToGroupRecommendations });//_appBarButtonGlobe_Click
//            CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xF22C", Clicked = NavigatorImpl.Instance.NavigateToSuggestedSourcesPage });
            CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE710", Clicked = this._appBarButtonCreate_Click });

            if (this.searchViewModel != null)
                this._appBarButtonSearch_Click(null);


            if (this._selected != 0)
                this._pivot.SelectedIndex = this._selected;

            if (this._selected == 0)
                this.communitiesListBox.Loaded2 += this.InsideScrollViewerLoaded;
            else if (this._selected == 1)
                this.eventsListBox.Loaded2 += this.InsideScrollViewerLoaded;
            else if (this._selected == 2)
                this.manageListBox.Loaded2 += this.InsideScrollViewerLoaded;
        }

        private void _appBarButtonSearch_Click(object sender)
        {
            if (this.searchViewModel != null)
            {
                CustomFrame.Instance.Header.ActivateSearch(true, true, this.searchViewModel.q);
            }
            else
            {
                CustomFrame.Instance.Header.ActivateSearch(true,false);

                this.searchViewModel = new GroupsSearchViewModel();
                this._topPanel.DataContext = this.searchViewModel;
            }

            CustomFrame.Instance.Header.ActivateMoreOptionsInSearch(true);

            this.communitiesListBox.DataContext = this.searchViewModel;

            this._pivot.Items.Remove(this.pivotItemAll);
            this._pivot.Visibility = Visibility.Collapsed;//this._root.Children.Remove(this._pivot);
            this._navView.Visibility = Visibility.Collapsed;
            //В этот раз делаем невидимым, т.к. пивот почему-то выгружает список событий :(
            this._root.Children.Add(this.pivotItemAll);
        }

        private void SearchClosed()
        {
            this.communitiesListBox.DataContext = this.VM.AllVM;
            this.searchViewModel = null;
            this._topPanel.DataContext = null;

            this._root.Children.Remove(this.pivotItemAll);
            this._pivot.Items.Insert(0, this.pivotItemAll);
            this._pivot.Visibility = Visibility.Visible;//this._root.Children.Add(this._pivot);

            this._navView.Visibility = Visibility.Visible;
        }

        private void _appBarButtonCreate_Click(object sender)
        {
            CommunityCreationUC sharePostUC = new CommunityCreationUC();

            PopUpService statusChangePopup = new PopUpService
            {
                Child = sharePostUC
            };
            //sharePostUC.SendTap = (users, title) => {
            //    statusChangePopup.Hide();
            //    this.CreateChat(users, title);
            //};
            statusChangePopup.OverrideBackKey = true;
            statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            statusChangePopup.Show();
        }

        private void ManageListBox_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.PullToRefresh.TrackListBox(sender as ExtendedListView3);
        }

        private void EventsListBox_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.PullToRefresh.TrackListBox(sender as ExtendedListView3);
        }

        private void CommunitiesListBox_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.PullToRefresh.TrackListBox(sender as ExtendedListView3);
        }



        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                this._selected = (int)pageState["Page"];
                this.VerticalOffset = (double)pageState["ScrollOffset"];

                this.manageListBox.NeedReload = this.VM.ManagedVM.Items.Count == 0;
                this.eventsListBox.NeedReload = this.VM.EventsVM.Items.Count == 0;

                if (!pageState.ContainsKey("SearchVM"))
                {
                    this.communitiesListBox.NeedReload = this.VM.AllVM.Items.Count == 0;
                }
                else
                {
                    this.searchViewModel = (GroupsSearchViewModel)pageState["SearchVM"];
                    this._topPanel.DataContext = this.searchViewModel;
                    this.communitiesListBox.NeedReload = false;
                }

                
            }
            else
            {
                if (base.DataContext == null)
                    base.DataContext = new GroupsListViewModel();
            }
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this.VM;
            pageState["Page"] = this._pivot.SelectedIndex;
            if (this.searchViewModel != null)
                pageState["SearchVM"] = this.searchViewModel;

            CustomFrame.Instance.Header.SearchClosed = null;
            CustomFrame.Instance.Header.ServerSearch = null;
            CustomFrame.Instance.Header.MoreSearchClicked -= this.MoreOptionsClicked;

            CustomFrame.Instance.Header.ActivateSearch(false);//выключаем таймер

            if (this._pivot.SelectedIndex == 0)
                this.VerticalOffset = this.communitiesListBox.GetInsideScrollViewer.VerticalOffset;
            else if (this._pivot.SelectedIndex == 1)
                this.VerticalOffset = this.eventsListBox.GetInsideScrollViewer.VerticalOffset;
            else if (this._pivot.SelectedIndex == 2)
                this.VerticalOffset = this.manageListBox.GetInsideScrollViewer.VerticalOffset;
            pageState["ScrollOffset"] = this.VerticalOffset;
        }

        private void ItemGroupUC_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKGroup;
            NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }

        private void GroupInvitation_JoinClick(object sender, RoutedEventArgs e)
        {
            Button element = sender as Button;
            VKGroup vm = element.DataContext as VKGroup;
            element.IsEnabled = false;
            this.VM.Join(vm, (result) =>
            {
                element.IsEnabled = true;
            });

        }

        private void GroupInvitation_HideClick(object sender, RoutedEventArgs e)
        {
            Button element = sender as Button;
            VKGroup vm = element.DataContext as VKGroup;
            element.IsEnabled = false;
            this.VM.Leave(vm, (result) =>
            {
                element.IsEnabled = true;
            });
        }

        private void ItemGroupInvitationUC_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKGroup;
            NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }

        private void ItemGroupUC_Holding(object sender, HoldingRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                FrameworkElement element = sender as FrameworkElement;
                this.ShowMenu(element);
            }
        }

        private void ItemGroupUC_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                return;

            FrameworkElement element = sender as FrameworkElement;
            this.ShowMenu(element);
        }

        private void ShowMenu(FrameworkElement element)
        {
            MenuFlyout menu = new MenuFlyout();

            MenuFlyoutItem item = new MenuFlyoutItem() { Text = "Выйти" };
            item.Command = new DelegateCommand((arg) =>
            {
                var vm = element.DataContext as VKGroup;
                //NavigatorImpl.Instance.NavigateToFriends(vm.id, vm.first_name_gen);
                this.VM.Leave(vm, null);
            });
            menu.Items.Add(item);
            menu.ShowAt(element);
        }

        private void MoreOptionsClicked()
        {
            PopUpService statusChangePopup = new PopUpService();

            FrameworkElement element = null;

            GroupsSearchParamsUC sharePostUC = new GroupsSearchParamsUC();
            sharePostUC.DataContext = this.searchViewModel;
            sharePostUC.Done = () => {
                statusChangePopup.Hide();
                this.searchViewModel.Items.Clear();//this.VM.Items.Clear();
                this.communitiesListBox.NeedReload = true;
                this.communitiesListBox.Reload();
            };
            element = sharePostUC;

            statusChangePopup.Child = element;

            statusChangePopup.OverrideBackKey = true;
            statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            statusChangePopup.Show();
        }

        private void Clear_OnTap(object sender, TappedRoutedEventArgs e)
        {
            this.searchViewModel.Clear();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var vm = e.AddedItems[0] as VKGroup;
            //NavigatorImpl.Instance.NavigateToProfilePage(-vm.id);
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }
    }
}
