using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

using LunaVK.Framework;
using System.Windows.Input;
using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Library;
using LunaVK.ViewModels;

namespace LunaVK
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class FriendsPage : PageBase
    {
        public ICommand AddFriend { get; set; }
        private FriendsViewModel3.FriendsSearchViewModel searchViewModel = null;
        private int _selected;

        public FriendsPage()
        {
            this.InitializeComponent();
            this.Loaded += FriendsPage_Loaded;
            this.Unloaded += FriendsPage_Unloaded;

            CustomFrame.Instance.Header.SearchClosed = this.SearchClosed;
            CustomFrame.Instance.Header.ServerSearch = this.OnServerSearch;
        }

        void FriendsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.TitlePanel.Tapped -= this.OpenNewsSourcePicker;
            //CustomFrame.Instance.HeaderWithMenu.SearchClosed = null;
            //CustomFrame.Instance.HeaderWithMenu.LocalSearch = null;
        }

        void FriendsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if(this.VM._userId == Settings.UserId)
                CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE710" });

            CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE721", Clicked = this._appBarButtonSearch_Click });

            if (this.searchViewModel != null)
                this._appBarButtonSearch_Click(null);
            else
            {
                if (this._selected != 0)
                    this._pivot.SelectedIndex = this._selected;
            }
        }

        private void SearchClosed()
        {
            this._allFriendsBox.DataContext = this.VM.AllFriendsVM;
            this.searchViewModel = null;

            this._root.Children.Remove(this.pivotItemAll);
            this._pivot.Items.Insert(0, this.pivotItemAll);
            //this._pivot.Items.Insert(1,this.pivotItemOnline);
            this._pivot.Visibility = Visibility.Visible;//this._root.Children.Add(this._pivot);

            this._navView.Visibility = Visibility.Visible;
        }


        private void _appBarButtonSearch_Click(object sender)
        {
            //CustomFrame.Instance.HeaderWithMenu.ActivateSearch(true);

            //            this._allFriendsBox.ItemsSource = this.VM.SearchItems;
            if (this.searchViewModel != null)
            {
                CustomFrame.Instance.Header.ActivateSearch(true, true, this.searchViewModel.q);
            }
            else
            {
                CustomFrame.Instance.Header.ActivateSearch(true);
                this.searchViewModel = new FriendsViewModel3.FriendsSearchViewModel(this.VM._userId);
            }

            this._allFriendsBox.DataContext = this.searchViewModel;




            //this._pivot.Items.Remove(this.pivotItemOnline);//BugFix
            this._pivot.Items.Remove(this.pivotItemAll);
            this._pivot.Visibility = Visibility.Collapsed;//this._root.Children.Remove(this._pivot);
            this._root.Children.Add(this.pivotItemAll);

            this._navView.Visibility = Visibility.Collapsed;
        }

        public FriendsViewModel3 VM
        {
            get { return base.DataContext as FriendsViewModel3; }
        }

        

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                this._selected = (int)pageState["Page"];

                if (this.VM.AllFriendsVM.Items.Count > 0)
                    this._allFriendsBox.NeedReload = false;
                if (this.VM.OnlineFriendsVM.Items.Count > 0)
                    this._onlineFriendsBox.NeedReload = false;
                if (this.VM.SuggestionsFriendsVM.Items.Count>0)
                    this.suggestionsListBox.NeedReload = false;
                if (this.VM.MutualFriendsVM.Items.Count > 0)
                    this.mutualFriendsListBox.NeedReload = false;
            }
            else
            {
                Dictionary<string, object> QueryString = navigationParameter as Dictionary<string, object>;
                int userId = (int)QueryString["Id"];

                this.DataContext = new FriendsViewModel3((uint)userId);

                if (QueryString.ContainsKey("UserName") && userId != Settings.UserId)
                    this.VM._userName = (string)QueryString["UserName"];
            }

            if (this.VM._userId == Settings.UserId)
            {
                CustomFrame.Instance.Header.TitlePanel.Tapped += this.OpenNewsSourcePicker;
                CustomFrame.Instance.Header.TitleOption = true;
                this._pivot.Items.Remove(this.pivotItemMutualFriends);
                this._navView.Items.Remove(this._navMutual);
            }
            else
            {
                CustomFrame.Instance.Header.TitleOption = false;

                this._pivot.Items.Remove(this.pivotItemSuggestionsFriends);
                this._pivot.Items.Remove(this.pivotItemRequestsFriends);
                this._pivot.Items.Remove(this.pivotItemRequestsOutFriends);

                this._navView.Items.Remove(this._navSuggestions);
                this._navView.Items.Remove(this._navRequests);
                this._navView.Items.Remove(this._navRequestsOut);
            }

            string temp = LocalizedStrings.GetString("Menu_Friends/Content");
            if (this.VM._userName != null)
                temp += (" " + this.VM._userName);
            base.Title = temp;

            //CustomFrame.Instance.HeaderWithMenu.SearchClosed = this.SearchClosed;
            //CustomFrame.Instance.HeaderWithMenu.LocalSearch = this.VM.LocalSearch;
//            CustomFrame.Instance.HeaderWithMenu.ServerSearch = this.VM.ServerSearch;
        }

        private void OnServerSearch(string text)
        {
            this.searchViewModel.q = text;
            this.searchViewModel.Items.Clear();
            this._allFriendsBox.NeedReload = true;
            this._allFriendsBox.Reload();
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this.VM;
            pageState["Page"] = this._pivot.SelectedIndex;
        }

        private void OpenNewsSourcePicker(object sender, TappedRoutedEventArgs e)
        {
            //if (this.pivot.Items.Contains(this.pivotItemMutualFriends))
            //{
            //    this.pivot.Items.Remove(this.pivotItemMutualFriends);
            //    return;
            //}

            /*
            Point point = e.GetPosition(null);

            if (pop == null)
            {
                pop = new UC.PopUP();
                
                //0 - друзья
                //1 - заявки
                //2 - исходящие
                //3 - предполагаемые
                
                pop.AddItem(0, LocalizedStrings.GetString("Menu_Friends/Content"));
                pop.AddItem(1, "Заявки");
                pop.AddItem(2, "Исходящие заявки");
                pop.AddItem(3, "Предполагаемые друзья");

                if(this.VM.Lists.Count>0)
                {
                    pop.AddSpace();
                    foreach(FriendsViewModel2.VKFriendsGetObject.Lists l in this.VM.Lists)
                    {
                        pop.AddItem(99+l.id, l.name);
                    }
                }
                
                pop.ItemTapped += _picker_ItemTapped;
            }

            pop.Show(point);*/
            MenuFlyout menu = new MenuFlyout();
            /*
            MenuFlyoutItem item = new MenuFlyoutItem() { Text = "Друзья" };
            item.Command = new DelegateCommand((args) =>
            {
                if (this._pivot.SelectedIndex == 0)
                {
//                    this.VM.SetSource(0, true);
                }
                else
                {
//                    this.VM.SetSource(0, false);
                    this._pivot.SelectedIndex = 0;
                }
                base.Title = item.Text;
            });
            menu.Items.Add(item);

            MenuFlyoutItem item2 = new MenuFlyoutItem() { Text = "Заявки" };
            item2.Command = new DelegateCommand((args) =>
            {
                if (this._pivot.SelectedIndex == 0)
                {
 //                   this.VM.SetSource(1, true);
                }
                else
                {
//                    this.VM.SetSource(1, false);
                    this._pivot.SelectedIndex = 0;
                }
                base.Title = item2.Text;
            });
            menu.Items.Add(item2);
            */
            MenuFlyoutItem item3 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("Birthdays_Title"), Command = new DelegateCommand((args) => { NavigatorImpl.Instance.NavigateToBirthdaysPage(); }) };
            menu.Items.Add(item3);

            if (this.VM.AllFriendsVM.Lists != null)
            {
                

                menu.Items.Add(new MenuFlyoutSeparator());

                foreach (var list in this.VM.AllFriendsVM.Lists)
                {
                    MenuFlyoutItem l = new MenuFlyoutItem() { Text = list.name };
                    l.CommandParameter = list.id;

                    menu.Items.Add(l);
                }
            }

            if(menu.Items.Count>0)
                menu.ShowAt(sender as FrameworkElement);
        }

        void _picker_ItemTapped(object sender, int i)
        {
            // if (i < 4)
            // {
            //              this.VM.SetSource((byte)i);
            //               this.VM.Title = pop.GetTitle(i);
            //                CustomFrame.Instance.HeaderWithMenu.SetTitle(this.VM.Title);
            //}
            //else
            //{
            //    this.VM.SetSource(i);
            //}
        }
        /*
        private async void BaseProfileItem_PrimaryClick(object sender, RoutedEventArgs e)
        {
            Button element = sender as Button;
            VKUser vm = element.DataContext as VKUser;
            element.IsEnabled = false;
 //           await this.VM.AddFriend(vm);
            element.IsEnabled = true;
        }

        private async void BaseProfileItem_SecondaryClick(object sender, RoutedEventArgs e)
        {
            Button element = sender as Button;
            VKUser vm = element.DataContext as VKUser;
            element.IsEnabled = false;
//            await this.VM.DeleteFriend(vm);
            element.IsEnabled = true;
        }

        private void BaseProfileItem_ThirdClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            VKUser vm = element.DataContext as VKUser;
            if(vm.Mode == 0)
            {
                Library.NavigatorImpl.Instance.NavigateToConversation(vm.id);
            }
            else if(vm.Mode==3)
            {
                this.VM.AddFriend(vm);
            }
        }
        */


        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot pi = sender as Pivot;

 //           this.VM.SetSubSource((byte)pi.SelectedIndex);
        }


        private void FriendRequestUC_AddClick(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as FriendsViewModel3.GenericCollectionAll;
            vm.AddFriend(vm.RequestsViewModel);
        }

        private void FriendRequestUC_HideClick(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as FriendsViewModel3.GenericCollectionAll;
            vm.DeleteFriend(vm.RequestsViewModel);
        }

        private void ShowMenu(FrameworkElement element)
        {
            FriendsViewModel3.VKUserEx vm = element.DataContext as FriendsViewModel3.VKUserEx;

            MenuFlyout menu = new MenuFlyout();

            MenuFlyoutItem item = new MenuFlyoutItem() { Text = "Посмотреть друзей" };
            item.Command = new DelegateCommand((arg) =>
            {
                NavigatorImpl.Instance.NavigateToFriends(vm.Id, vm.first_name_gen);
            });
            menu.Items.Add(item);

            //MenuFlyoutItem item2 = new MenuFlyoutItem() { Text = "Предложить друзей" };
            //menu.Items.Add(item2);

            MenuFlyoutSeparator item3 = new MenuFlyoutSeparator();
            menu.Items.Add(item3);

            if(this.VM._userId == Settings.UserId)
            {
                MenuFlyoutItem item4 = new MenuFlyoutItem() { Text = "Убрать из друзей" };
                item4.Command = new DelegateCommand((arg) =>
                {
                    this.VM.AllFriendsVM.DeleteFriend(vm);
                });
                menu.Items.Add(item4);
            }
            //MenuFlyoutSeparator item5 = new MenuFlyoutSeparator();
            //menu.Items.Add(item5);

            if (this.VM.AllFriendsVM.Lists != null)
            {
                MenuFlyoutSubItem sub = new MenuFlyoutSubItem() { Text = "Настроить списки" };
                foreach (var list in this.VM.AllFriendsVM.Lists)
                {
                    MenuFlyoutItem l = new MenuFlyoutItem() { Text = list.name };
                    l.CommandParameter = list.id;
                    l.Command = new DelegateCommand((arg) =>
                    {
                        this.VM.EditFriend(vm, (int)arg);
                    });
//                    if (vm.lists != null && vm.lists.Contains(list.id))
//                        l.Icon = new SymbolIcon(Symbol.Accept);//BUG
                    sub.Items.Add(l);
                }

                menu.Items.Add(sub);
            }
            
            menu.ShowAt(element);
        }

        private void Back_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(e!=null)
                e.Handled = true;

            VKUser vm = (sender as FrameworkElement).DataContext as VKUser;
            NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }

        private void BaseProfileItem_BackTap(object sender, RoutedEventArgs e)
        {
            this.Back_Tapped(sender, null);
        }

        private void Options_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            this.ShowMenu(sender as FrameworkElement);
        }

        private void WriteMessage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            VKUser vm = (sender as FrameworkElement).DataContext as VKUser;
            NavigatorImpl.Instance.NavigateToConversation(vm.Id);
        }

        private void Back_Holding(object sender, HoldingRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                FrameworkElement element = sender as FrameworkElement;
                this.ShowMenu(element);
            }
        }

        private void Back_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                return;

            FrameworkElement element = sender as FrameworkElement;
            this.ShowMenu(element);
        }

        private void pivot_Loaded(object sender, RoutedEventArgs e)
        {

            Pivot p = sender as Pivot;

            //           this.VM.SetSource(0, true);

            p.SelectionChanged += this.Pivot_SelectionChanged;
        }

        private void BaseProfileItem_PrimaryClick(object sender, RoutedEventArgs e)
        {
            FriendsViewModel3.VKUserEx vm = (sender as FrameworkElement).DataContext as FriendsViewModel3.VKUserEx;
            this.VM.RequestsFriendsVM.AddFriend(vm);
        }

        private void BaseProfileItem_SecondaryClick(object sender, RoutedEventArgs e)
        {
            FriendsViewModel3.VKUserEx vm = (sender as FrameworkElement).DataContext as FriendsViewModel3.VKUserEx;
            this.VM.RequestsFriendsVM.DeleteFriend(vm);
        }
    }
}
