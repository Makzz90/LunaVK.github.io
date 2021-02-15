using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.ViewModels;
using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using LunaVK.UC;
using LunaVK.Core.Library;
using LunaVK.UC.PopUp;
using Windows.UI.Xaml.Media;
using LunaVK.Common;
using Windows.UI.Xaml.Controls.Primitives;

namespace LunaVK.Pages
{
    public sealed partial class ProfilePage : PageBase
    {
        //        private HideHeaderHelper _hideHelper;
        private OptionsMenuItem _moreItem;
        private PopUpService dialogService;
        private ProfileInfoFullViewModel fullInfoViewModel;

        public ProfilePage()
        {
            this.InitializeComponent();
            this.Loaded += ProfilePage_Loaded;
            this.Unloaded += ProfilePage_Unloaded;
            this.MainScroll.Loaded2 += InsideScrollViewerLoaded;
            this.MainScroll.OnPullPercentageChanged+= this.OnPullPercentageChanged;

            //           this._gridCovers.MinHeight = CustomFrame.Instance.Header.HeaderHeight;
            this._moreItem = new OptionsMenuItem() { Icon = "\xE712", Clicked = this._appBarButtonMore_Click };
        }

        private void ProfilePage_Unloaded(object sender, RoutedEventArgs e)
        {
//            CustomFrame.Instance.MenuStateChanged -= CFrame_MenuStateChanged;
            CustomFrame.Instance.Header.HeaderGrid.Tapped -= _header_Tapped;
        }

        private void OnPullPercentageChanged(double value)
        {
 //           this.transformCoverScale.ScaleX = this.transformCoverScale.ScaleY = 1 + value / 300;
        }

        private void ProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(base.Title) &&  this.VM.User!=null)
            {
                base.Title =  this.VM.User.Title;
            }
            else
            {
                base.Title = "id"+this.VM.Id;
            }
            CustomFrame.Instance.Header.HeaderGrid.Tapped += _header_Tapped;
        }

        private void InsideScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
//            (sender as ScrollViewer).ViewChanged += GetInsideScrollViewer_ViewChanged;

            if (this.VerticalOffset != 0)
                (sender as ScrollViewer).ChangeView(0, this.VerticalOffset, 1.0f);
        }
        /*
        private void GetInsideScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
 //           this.transformCover2.Y = this.transformCover.Y = -(sv.VerticalOffset / 2.0);

            
                this.UpdateHeaderOpacityWithScrollPosition(sv.VerticalOffset);
            
        }
        
        private void UpdateHeaderOpacityWithScrollPosition(double scrollPosition)
        {
 //           this.UpdateHeaderOpacity(this.CalculateOpacity(scrollPosition, this.borderOffset.ActualHeight/2.0, this.borderOffset.ActualHeight)); //this.UpdateHeaderOpacity(this.CalculateOpacity(scrollPosition, 200.0, 224.0));
            //this.ucProfileInfoHeader.SetOverlayOpacity(this.CalculateOpacity(scrollPosition, 96.0, 200.0));
        }
        
        private void UpdateHeaderOpacity(double opacity)
        {
//            CustomFrame.Instance.Header.BackGroundGrid.Opacity = opacity;

 //           this.imgUserCover.Opacity = 1.0 - opacity;
            //this.ucHeader.rectBackground.Opacity = opacity;
            //this.ucHeader.textBlockTitle.Opacity = opacity;
            //this.ucHeader.borderCounter.Opacity = opacity;
        }
        */
        private double CalculateOpacity(double verticalPos, double minSP, double maxSP)
        {
            double num1;
            if (verticalPos < minSP)
                num1 = 0.0;
            else if (verticalPos > maxSP)
            {
                num1 = 1.0;
            }
            else
            {
                double num2 = maxSP - minSP;
                num1 = 1.0 / num2 * verticalPos - minSP / num2;
            }
            return num1;
        }

        public ProfileViewModel VM
        {
            get { return base.DataContext as ProfileViewModel; }
        }
        
        private double VerticalOffset = 0;

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                this.VerticalOffset = (double)pageState["ScrollOffset"];
                this.MainScroll.NeedReload = false;
                this.CreateAppBar();
                this.HandleLoadingStatusUpdated(ProfileLoadingStatus.Loaded);
                /*
                if (this.VerticalOffset == 0)
                    this.UpdateHeaderOpacity(0);
                else
                    this.UpdateHeaderOpacityWithScrollPosition(this.VerticalOffset);
                    */


                base.Title = this.VM.User.Title;
                //CustomFrame.Instance.HeaderWithMenu.UpdateTitleBinding();
            }
            else
            {
                uint id = (uint)navigationParameter;
                this.DataContext = new ProfileViewModel(id);
                
            }
            this.VM.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;
            this.VM.CoverLoaded = this.Image_Loaded;
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
 //           CustomFrame.Instance.MenuStateChanged -= CFrame_MenuStateChanged;
            //    if (CustomFrame.Instance.HeaderWithMenu != null)
            //        CustomFrame.Instance.HeaderWithMenu.IsVisible = true;
//            CustomFrame.Instance.Header.BackGroundGrid.Opacity = 1.0;

            pageState["Data"] = this.VM;
            pageState["ScrollOffset"] = this.MainScroll.GetInsideScrollViewer.VerticalOffset;

            this.VM.LoadingStatusUpdated -= this.HandleLoadingStatusUpdated;
        }

        private void Counter_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ProfileViewModel.CounterVM vm = (sender as FrameworkElement).DataContext as ProfileViewModel.CounterVM;
            vm.Callback?.Invoke();
        }

        private void Command_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ProfileViewModel.CommandVM vm = (sender as FrameworkElement).DataContext as ProfileViewModel.CommandVM;
            vm.Callback?.Invoke();
        }

        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {
            if (status == ProfileLoadingStatus.Loaded && this.VM.PostsToggleViewModel == null)
            {
 //               CustomFrame.Instance.MenuStateChanged += CFrame_MenuStateChanged;

                base.Title = string.IsNullOrEmpty( this.VM.User.domain) ? ("id"+ this.VM.Id) : this.VM.User.domain;
//                CustomFrame.Instance.HeaderWithMenu.UpdateTitleBinding();

                
            }


            string str;

            switch (status)
            {
                case ProfileLoadingStatus.Loading:
                    str = "Loading";
                    break;
                case ProfileLoadingStatus.Reloading:
                    str = "Reloading";
                    break;
                case ProfileLoadingStatus.Loaded:
                    str = "LoadedState";
                    //
                    if(this.VM.Id == Settings.UserId)
                    {
                        EventAggregator.Instance.PublishProfileAvatarChangedEvent(this.VM.User.photo_100);
                        EventAggregator.Instance.PublishProfileNameChangedEvent(this.VM.User.Title);
                        EventAggregator.Instance.PublishProfileStatusChangedEvent(this.VM.User.status);
                    }
                    
                    if(this.VM.User.deactivated != VKIsDeactivated.None)
                    {
                        str = "Banned";
                    }
                    else if(this.VM.User.blacklisted)
                    {
                        str = "Blocked";
                    }
                    else if (this.VM.Id != Settings.UserId && this.VM.User.is_closed && (byte)this.VM.User.friend_status < 2)
                    {
                        str = "Private";
                    }
                    else if (this.VM.IsServiceProfile)
                    {
                        str = "Service";
                    }

                    this.CreateAppBar();
                    break;
                case ProfileLoadingStatus.LoadingFailed:
                    str = "LoadingFailed";
                    break;
                default:
                    return;
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine(str);
#endif
            VisualStateManager.GoToState(this, str, false);
        }
        
        private Border GetImageFunc(int index)
        {
            /*
            ListViewItem item = this._photosListView.ContainerFromIndex(index) as ListViewItem;

            if (item == null)//из-за виртуализации получается такая шляпа?
                return null;

            UIElement ee = item.ContentTemplateRoot;
            Border brd = ee as Border;
            return brd;//.Child as Image;
            */
            return null;
        }

        private void _appBarButtonMore_Click(object sender)
        {
            PopUP2 menu = new PopUP2();

            PopUP2.PopUpItem item1 = new PopUP2.PopUpItem();
            item1.Text = LocalizedStrings.GetString("PinToStart");
            item1.Icon = new SymbolIcon(Symbol.Pin);
            item1.Command = new DelegateCommand((args) =>
            {
                this.AppBarMenuItemPinToStart_OnClick();
            });
            menu.Items.Add(item1);


            PopUP2.PopUpItem item3 = new PopUP2.PopUpItem();
            item3.Text = LocalizedStrings.GetString("CopyLink");
            item3.Icon = new SymbolIcon(Symbol.Copy);
            item3.Command = new DelegateCommand((args) =>
            {
                var dataPackage = new DataPackage();
                string temp = "https://";
                if (CustomFrame.Instance.IsDevicePhone)
                    temp += "m.";
                temp += "vk.com/id";
                temp += this.VM.Id;
                dataPackage.SetText(temp);
                Clipboard.SetContent(dataPackage);
            });
            menu.Items.Add(item3);



            PopUP2.PopUpItem item4 = new PopUP2.PopUpItem();
            item4.Text = LocalizedStrings.GetString("OpenInBrowser");
            item4.Icon = new SymbolIcon(Symbol.Link);
            item4.Command = new DelegateCommand((args) =>
            {
                string temp = "https://";
                if (CustomFrame.Instance.IsDevicePhone)
                    temp += "m.";
                temp += "vk.com/id";
                temp += this.VM.Id;
                NavigatorImpl.Instance.NavigateToWebUri(temp, true);
            });
            menu.Items.Add(item4);

            if (this.VM.Id != Settings.UserId)
            {
                PopUP2.PopUpSubItem item2 = new PopUP2.PopUpSubItem();
                item2.Text = LocalizedStrings.GetString("Report") + "...";

                PopUP2.PopUpItem subitem = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("ReportReasonSpam"), CommandParameter = ReportReason.Spam };//1
                subitem.Command = new DelegateCommand((args) => { this.ReportGroup(args); });
                item2.Items.Add(subitem);
                PopUP2.PopUpItem subitem2 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("ReportReasonChildPorn"), CommandParameter = ReportReason.ChildPorn };//5
                subitem2.Command = new DelegateCommand((args) => { this.ReportGroup(args); });
                item2.Items.Add(subitem2);
                PopUP2.PopUpItem subitem3 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("ReportReasonExtremism"), CommandParameter = ReportReason.Extremism };//6
                subitem3.Command = new DelegateCommand((args) => { this.ReportGroup(args); });
                item2.Items.Add(subitem3);
                PopUP2.PopUpItem subitem4 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("ReportReasonViolence"), CommandParameter = ReportReason.Violence };//7
                subitem4.Command = new DelegateCommand((args) => { this.ReportGroup(args); });
                item2.Items.Add(subitem4);
                PopUP2.PopUpItem subitem5 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("ReportReasonDrug"), CommandParameter = ReportReason.Drugs };//4
                subitem5.Command = new DelegateCommand((args) => { this.ReportGroup(args); });
                item2.Items.Add(subitem5);
                PopUP2.PopUpItem subitem6 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("ReportReasonAdult"), CommandParameter = ReportReason.Adult };//3
                subitem6.Command = new DelegateCommand((args) => { this.ReportGroup(args); });
                item2.Items.Add(subitem6);
                PopUP2.PopUpItem subitem7 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("ReportReasonInsult"), CommandParameter = ReportReason.Abuse };//2
                subitem7.Command = new DelegateCommand((args) => { this.ReportGroup(args); });
                item2.Items.Add(subitem7);

                menu.Items.Add(item2);
            }


            if (this.VM.CanSubscribeUnsubscribe)
            {
                PopUP2.PopUpItem item6 = new PopUP2.PopUpItem(); 
                item6.Text = LocalizedStrings.GetString(this.VM.IsSubscribed ? "UnsubscribeFromNews" : "SubscribeToNews");
                item6.Icon = new SymbolIcon(Symbol.Account);
                item6.Command = new DelegateCommand((args) =>
                {
                    this.VM.SubscribeUnsubscribe();
                });
                menu.Items.Add(item6);
            }

            if (this.VM.CanFaveUnfave)
            {
                PopUP2.PopUpItem item7 = new PopUP2.PopUpItem();
                item7.Text = LocalizedStrings.GetString(this.VM.IsFavorite ? "RemoveFromBookmarks" : "AddToBookmarks");
                item7.Icon = new SymbolIcon(Symbol.Favorite);
                item7.Command = new DelegateCommand((args) =>
                {
                    this.VM.FaveUnfave();
                });
                menu.Items.Add(item7);
            }

            if(this.VM.Id!= Settings.UserId)
            {
                PopUP2.PopUpItem item5 = new PopUP2.PopUpItem();
                item5.Text = LocalizedStrings.GetString(this.VM.IsBlacklistedByMe ? "BannedUsers_UnbanUser" : "BannedUsers_BanUser");
                item5.Icon = new SymbolIcon(Symbol.BlockContact);
                item5.Command = new DelegateCommand((args) =>
                {
                    this.AppBarMenuItemBanUnban_OnClick(sender, null);
                });
                menu.Items.Add(item5);
            }
            
            menu.ShowAt(sender as FrameworkElement);
        }

        private void AppBarMenuItemBanUnban_OnClick(object sender, TappedRoutedEventArgs e)
        {
            this.VM.BanUnban();
        }

        private void AppBarMenuItemPinToStart_OnClick(/*object sender, EventArgs e*/)
        {
            this.VM.PinToStart();
        }

        private void ReportGroup(object args)
        {
            GroupsService.Instance.Report(this.VM.Id, (ReportReason)args, null);
        }

        private void CreateAppBar()
        {
            /*
            CommandBar applicationBar = new CommandBar();

            applicationBar.PrimaryCommands.Clear();
            applicationBar.SecondaryCommands.Clear();
            this.VM.Commands.Clear();

            applicationBar.Visibility = CustomFrame.Instance.MenuState == CustomFrame.MenuStates.StateMenuCollapsedContentStretch ? Visibility.Visible : Visibility.Collapsed;

            //
            
            if (this.VM.CanSubscribeUnsubscribe)
            {
                AppBarButton btn2 = new AppBarButton();
                btn2.Label = this.VM.IsSubscribed ? "не уведомлять о новых записях" : "уведомлять о новых записях";
                btn2.Icon = new SymbolIcon(Symbol.Account);
                btn2.Command = new DelegateCommand((args) =>
                {
                    this.VM.SubscribeUnsubscribe((ret) =>
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            this.VM.IsSubscribed = !this.VM.IsSubscribed;
                            this.CreateAppBar();
                        });
                    });
                });
                applicationBar.SecondaryCommands.Add(btn2);

                this.VM.Commands.Add(new ProfileViewModel.CommandVM("\xEA8F", this.VM.IsSubscribed ? "не уведомлять о новых записях" : "уведомлять о новых записях", () =>
                {
                    this.VM.SubscribeUnsubscribe((ret) =>
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            this.VM.IsSubscribed = !this.VM.IsSubscribed;
                            this.CreateAppBar();
                        });
                    });
                }));
            }
            //if (this._viewModel.CanPinToStart)

            AppBarButton btn3 = new AppBarButton();
            btn3.Label = "копировать ссылку";
            btn3.Icon = new SymbolIcon(Symbol.Copy);
            btn3.Command = new DelegateCommand((args) =>
            {
                var dataPackage = new DataPackage();
                string temp = "https://";
                if (CustomFrame.Instance.IsDevicePhone)
                    temp += "m.";
                temp += "vk.com/id";
                temp += this.VM.Id;
                dataPackage.SetText(temp);
                Clipboard.SetContent(dataPackage);
            });
            applicationBar.SecondaryCommands.Add(btn3);

            this.VM.Commands.Add(new ProfileViewModel.CommandVM("\xE71B", "копировать ссылку", () =>
            {
                var dataPackage = new DataPackage();
                string temp = "https://";
                if (CustomFrame.Instance.IsDevicePhone)
                    temp += "m.";
                temp += "vk.com/id";
                temp += this.VM.Id;
                dataPackage.SetText(temp);
                Clipboard.SetContent(dataPackage);
            }));

            AppBarButton btn4 = new AppBarButton();
            btn4.HorizontalAlignment = HorizontalAlignment.Stretch;
            btn4.Label = "открыть в браузере";
            btn4.Icon = new SymbolIcon(Symbol.Link);
            btn4.Command = new DelegateCommand((args) =>
            {
                string temp = "https://";
                if (CustomFrame.Instance.IsDevicePhone)
                    temp += "m.";
                temp += "vk.com/id";
                temp += this.VM.Id;
                NavigatorImpl.Instance.NavigateToWebUri(temp, true);
            });
            applicationBar.SecondaryCommands.Add(btn4);

            this.VM.Commands.Add(new ProfileViewModel.CommandVM("\xE774", "открыть в браузере", () =>
            {
                string temp = "https://";
                if (CustomFrame.Instance.IsDevicePhone)
                    temp += "m.";
                temp += "vk.com/id";
                temp += this.VM.Id;
                NavigatorImpl.Instance.NavigateToWebUri(temp, true);
            }));

            if (this.VM.CanFaveUnfave)
            {
                AppBarButton btn5 = new AppBarButton();
                btn5.Label = LocalizedStrings.GetString(this.VM.IsFavorite ? "RemoveFromBookmarks":"AddToBookmarks" );
                btn5.Icon = new SymbolIcon(Symbol.Favorite);
                btn5.Command = new DelegateCommand((args) =>
                {
                    this.VM.FaveUnfave((ret) =>
                    {
                        if (ret)
                        {
                            this.VM.IsFavorite = !this.VM.IsFavorite;
                            this.CreateAppBar();
                        }
                    });
                });
                applicationBar.SecondaryCommands.Add(btn5);

                this.VM.Commands.Add(new ProfileViewModel.CommandVM("\xE728", LocalizedStrings.GetString(this.VM.IsFavorite ? "RemoveFromBookmarks" : "AddToBookmarks"), () =>
                {
                    this.VM.FaveUnfave((ret) =>
                    {
                        if (ret)
                        {
                            this.VM.IsFavorite = !this.VM.IsFavorite;
                            this.CreateAppBar();
                        }
                    });
                }));
            }

            if (this.VM.CanBanUnban)
            {
                AppBarButton btn6 = new AppBarButton();
                btn6.Label = LocalizedStrings.GetString(this.VM.IsBlacklistedByMe ? "BannedUsers_UnbanUser" : "BannedUsers_BanUser");
                btn6.Icon = new SymbolIcon(Symbol.BlockContact);
                btn6.Command = new DelegateCommand((args) =>
                {
                    this.VM.BanUnban((ret) =>
                    {
                        if (ret)
                        {
                            this.VM.IsBlacklistedByMe = !this.VM.IsBlacklistedByMe;
                            this.CreateAppBar();
                        }
                    });
                });
                applicationBar.SecondaryCommands.Add(btn6);

                this.VM.Commands.Add(new ProfileViewModel.CommandVM("\xF140", LocalizedStrings.GetString(this.VM.IsBlacklistedByMe ? "BannedUsers_UnbanUser" : "BannedUsers_BanUser"), () =>
                {
                    this.VM.BanUnban((ret) =>
                    {
                        if (ret)
                        {
                            this.VM.IsBlacklistedByMe = !this.VM.IsBlacklistedByMe;
                            this.CreateAppBar();
                        }
                    });
                }));
            }
            */

            if (CustomFrame.Instance.Header.OptionsMenu.Contains(this._moreItem))
                CustomFrame.Instance.Header.OptionsMenu.Remove(this._moreItem);

            CustomFrame.Instance.Header.OptionsMenu.Add(this._moreItem);
        }

        private void Cover_SizeChanged(object sender, SizeChangedEventArgs e)
        {
//            this.transformCoverScale.CenterX = e.NewSize.Width / 2.0;

            if (e.NewSize.Height != e.PreviousSize.Height)
            {
                if(this.VM.Id < 0)
                {
//                    this.borderOffset.Height = e.NewSize.Height;
                    return;
                }

                if(CustomFrame.Instance.MenuState == CustomFrame.MenuStates.StateMenuCollapsedContentStretch )
                {
 //                   this.borderOffset.Height = e.NewSize.Height;
                }
                else
                {
 //                   this.borderOffset.Height = 80;
                }
                
            }
        }

        private void InfoItem_OnTap(object sender, TappedRoutedEventArgs e)
        {
            ProfileInfoItem dataContext = (sender as FrameworkElement).DataContext as ProfileInfoItem;
            if (dataContext == null || dataContext.NavigationAction == null)
                return;
            e.Handled = true;
            dataContext.NavigationAction();
        }

        private void BorderFullInformation_OnTap(object sender, TappedRoutedEventArgs e)
        {
            if (this.fullInfoViewModel == null)
            {
                this.VM.ShowFullInfoPopup((result) =>
                {
                    if (result)
                    {
                        FullInfoUC uc = new FullInfoUC();
                        this.fullInfoViewModel = this.VM.GetFullInfoViewModel(true);
                        uc.DataContext = this.fullInfoViewModel;
                        this.dialogService = new PopUpService();
                        this.dialogService.Child = uc;
                        this.dialogService.OverrideBackKey = true;
                        this.dialogService.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
                        this.dialogService.Show();
                        OnUpdateVisibilityList.Invoke();
                    }
                });
            }
            else
            {
                FullInfoUC uc = new FullInfoUC();
                uc.DataContext = this.fullInfoViewModel;
                this.dialogService = new PopUpService();
                this.dialogService.Child = uc;
                this.dialogService.OverrideBackKey = true;
                this.dialogService.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
                this.dialogService.Show();
                OnUpdateVisibilityList.Invoke();
            }
        }

        public delegate void UpdateVisibilityList();
        public static event UpdateVisibilityList OnUpdateVisibilityList;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToConversation((int)this.VM.Id);
        }

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            Button element = sender as Button;

            if (this.VM.Id > 0)
            {
                if (this.VM.Id == Settings.UserId)
                {
                    NavigatorImpl.Instance.NavigateToEditProfile();
                    return;
                }

                MenuFlyout menu = new MenuFlyout();

                switch (this.VM.User.friend_status)
                {
                    case VKUsetMembershipType.RequestSent:
                        { 
                        MenuFlyoutItem menuItem1 = new MenuFlyoutItem();
                        menuItem1.Text = LocalizedStrings.GetString(this.VM.User.can_send_friend_request ? "GroupPage_CancelRequest" : "GroupPage_Unfollow");
                        menuItem1.Command = new DelegateCommand((args) =>
                        {
                            this.VM.FriendRemove();
                        });
                        menu.Items.Add(menuItem1);
                        break;
                        }
                    case VKUsetMembershipType.RequestReceived:
                        {
                            /*
                            if (this.VM.User.read_state == 1)
                            {
                                if (this.VM.User.Id != AppGlobalStateManager.Current.LoggedInUserId)
                                {
                                    MenuFlyoutItem menuItem3 = new MenuFlyoutItem();
                                    string lowerInvariant = CommonResources.Profile_AddToFriends.ToLowerInvariant();
                                    menuItem3.Text = lowerInvariant;
                                    MenuFlyoutItem menuItem4 = menuItem3;
                                    menuItem4.Click += delegate (object sender, RoutedEventArgs args)
                                    {
                                        this.Add();
                                    };
                                    menu.Items.Add(menuItem4);
                                    break;
                                }
                                break;
                            }*/
                            MenuFlyoutItem menuItem5 = new MenuFlyoutItem();
                            menuItem5.Text = LocalizedStrings.GetString("Profile_AcceptRequest");
                            menuItem5.Command = new DelegateCommand((args) =>
                            {
                                this.VM.FriendAdd(false);
                            });

                            MenuFlyoutItem menuItem7 = new MenuFlyoutItem();
                            menuItem7.Text = LocalizedStrings.GetString("Profile_KeepAsFollower");
                            menuItem7.Command = new DelegateCommand((args) =>
                            {
                                //todo: Что должно быть?
                                this.VM.FriendRemove();//this.VM.FriendAdd(true);
                            });
                            menu.Items.Add(menuItem5);
                            menu.Items.Add(menuItem7);
                            break;
                        }
                    case VKUsetMembershipType.Friends:
                        {
                            MenuFlyoutItem menuItem9 = new MenuFlyoutItem();
                            menuItem9.Text = LocalizedStrings.GetString("Profile_RemoveFromFriends");
                            menuItem9.Command = new DelegateCommand((args) =>
                            {
                                this.VM.FriendRemove();
                            });
                            menu.Items.Add(menuItem9);
                            break;
                        }
                    case VKUsetMembershipType.No:
                        {
                            this.VM.FriendAdd(false);
                            return;
                        }
                }

                menu.ShowAt(element);
            }
            else
            {
                /*
                if(this.VM.GroupData.action_button!=null)
                {
                    if(this.VM.GroupData.action_button.action_type== VKGroupActionBtnType.open_group_app)
                    {
                        NavigatorImpl.Instance.NavigateToProfileAppPage(this.VM.GroupData.action_button.target.app_id, this.VM.Id,"");
                    }
                    else if (this.VM.GroupData.action_button.action_type == VKGroupActionBtnType.send_email)
                    {
                        this.SendEmailButton_Click(this.VM.GroupData.action_button.target.email);
                    }
                    return;
                }

                this.ShowSecondaryMenu(element);*/
            }
        }

        private async void SendEmailButton_Click(string email)
        {
            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
            emailMessage.To.Add(new Windows.ApplicationModel.Email.EmailRecipient(email));            
            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }
        /*
        private void ShowSecondaryMenu(FrameworkElement element)
        {
            MenuFlyout menu = new MenuFlyout();

            switch (this.VM.GroupData.member_status)
            {
                case VKGroupMembershipType.Member:
                    {
                        switch (this.VM.GroupData.type)
                        {
                            case VKGroupType.Group:
                                {
                                    MenuFlyoutItem item = new MenuFlyoutItem();
                                    item.Text = LocalizedStrings.GetString("GroupPage_LeaveCommunity");
                                    item.Command = new DelegateCommand((args) => {
                                        this.VM.GroupLeave();
                                    });
                                    menu.Items.Add(item);
                                    break;
                                }
                            case VKGroupType.Page:
                                {
                                    MenuFlyoutItem item = new MenuFlyoutItem();
                                    item.Text = LocalizedStrings.GetString("GroupPage_Unfollow");
                                    item.Command = new DelegateCommand((args) => {
                                        this.VM.GroupLeave();
                                    });
                                    menu.Items.Add(item);
                                    break;
                                }
                            case VKGroupType.Event:
                                {
                                    MenuFlyoutItem item = new MenuFlyoutItem();
                                    item.Text = LocalizedStrings.GetString("EventMaybe");
                                    item.Command = new DelegateCommand((args) => {
                                        this.VM.GroupJoin(true);
                                    });
                                    menu.Items.Add(item);

                                    MenuFlyoutItem item2 = new MenuFlyoutItem();
                                    item2.Text = LocalizedStrings.GetString("EventNotAttend");
                                    item2.Command = new DelegateCommand((args) => {
                                        this.VM.GroupLeave();
                                    });
                                    menu.Items.Add(item2);
                                    break;
                                }
                        }
                        break;
                    }
                case VKGroupMembershipType.NotSure:
                    {
                        MenuFlyoutItem item = new MenuFlyoutItem();
                        item.Text = LocalizedStrings.GetString("EventAttend");//точно пойду
                        item.Command = new DelegateCommand((args) => {
                            this.VM.GroupJoin(false);
                        });
                        menu.Items.Add(item);

                        MenuFlyoutItem item2 = new MenuFlyoutItem();
                        item2.Text = LocalizedStrings.GetString("EventNotAttend");
                        item2.Command = new DelegateCommand((args) => {
                            this.VM.GroupLeave();
                        });
                        menu.Items.Add(item2);
                        //
                        break;
                    }
                case VKGroupMembershipType.RequestSent:
                    {
                        MenuFlyoutItem item = new MenuFlyoutItem();
                        item.Text = LocalizedStrings.GetString("GroupPage_CancelRequest");
                        item.Command = new DelegateCommand((args) => {
                            this.VM.GroupLeave();
                        });
                        menu.Items.Add(item);

                        break;
                    }
                case VKGroupMembershipType.InvitationReceived:
                case VKGroupMembershipType.NotAMember:
                    {
                        if (this.VM.GroupData.is_closed != VKGroupIsClosed.Opened)
                        {
                            MenuFlyoutItem item = new MenuFlyoutItem();
                            item.Text = LocalizedStrings.GetString("GroupPage_SendRequest");
                            menu.Items.Add(item);

                            break;
                        }
                        switch (this.VM.GroupData.type)
                        {
                            case VKGroupType.Group:
                                {
                                    MenuFlyoutItem item = new MenuFlyoutItem();
                                    item.Text = LocalizedStrings.GetString("Group_Join");
                                    item.Command = new DelegateCommand((args) => {
                                        this.VM.GroupJoin(null);
                                    });
                                    menu.Items.Add(item);
                                    break;
                                }
                            case VKGroupType.Page:
                                {
                                    MenuFlyoutItem item = new MenuFlyoutItem();
                                    item.Text = LocalizedStrings.GetString("GroupPage_Follow");
                                    item.Command = new DelegateCommand((args) => {
                                        this.VM.GroupJoin(null);
                                    });
                                    menu.Items.Add(item);
                                    break;
                                }
                            case VKGroupType.Event:
                                {
                                    MenuFlyoutItem item = new MenuFlyoutItem();
                                    item.Text = LocalizedStrings.GetString("Event_Join");
                                    item.Command = new DelegateCommand((args) => {
                                        this.VM.GroupJoin(null);
                                    });
                                    menu.Items.Add(item);

                                    MenuFlyoutItem item2 = new MenuFlyoutItem();
                                    item2.Text = LocalizedStrings.GetString("EventAttend");
                                    item2.Command = new DelegateCommand((args) => {
                                        this.VM.GroupJoin(false);
                                    });
                                    menu.Items.Add(item2);

                                    MenuFlyoutItem item3 = new MenuFlyoutItem();
                                    item3.Text = LocalizedStrings.GetString("EventMaybe");
                                    item3.Command = new DelegateCommand((args) => {
                                        this.VM.GroupJoin(true);
                                    });
                                    menu.Items.Add(item3);
                                    break;
                                }
                        }
                        break;
                    }
            }


            menu.ShowAt(element);
        }
        */
        private void UcProfileInfoHeader_OnTap(object sender, TappedRoutedEventArgs e)
        {
            
            if (this.VM.HasAvatar)
            {
                if (this.VM.CanChangePhoto)
                    FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
                else
                    this.VM.OpenProfilePhotos();
            }
            else
            {
                if (this.VM.CanChangePhoto)
                    this.VM.PickNewPhoto();
            }
        }

        private void SupportHyperlink_OnClicked(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            NavigatorImpl.Instance.NavigateToConversation(333);            
        }


        private void Image_Loaded()
        {
  //          this._imgUserCover.Animate(0, 1, "Opacity", 300);
        }

        private void Third_Click(object sender, RoutedEventArgs e)
        {
//            this.ShowSecondaryMenu(sender as FrameworkElement);
        }

        private void _header_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.MainScroll.GetInsideScrollViewer.ChangeView(0, 0, 1);
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            ImageBrush img = sender as ImageBrush;
            this._avaEllipse.Animate(0, 1, "Opacity", 300);
            img.ImageOpened -= this.Image_ImageOpened;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }

        private void Share_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SharePostUC share = new SharePostUC("ссылкой", WallService.RepostObject.photo, 0, 0, "", "id" + this.VM.Id);
            share.HideOptions();
            PopUpService popUp = new PopUpService { Child = share, OverrideBackKey = true, AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed };
            share.Done = popUp.Hide;
            popUp.Show();

            e.Handled = true;
        }

        private void Msg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToConversation((int)this.VM.Id);
        }

        private void Notifications_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.VM.SubscribeUnsubscribe();
        }

        private void Favorite_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.VM.FaveUnfave();
        }

        private void Status_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.VM.Id == Settings.UserId)
                this.VM.OpenSetStatusPopup();
        }

        //if (this._viewModel.CanPost || this._viewModel.CanSuggestAPost)
        //_appBarButtonAddNews AppBarButtonAddNews_OnClick
        private void NewPost_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToNewWallPost(WallPostViewModel.Mode.NewWallPost, (int)this.VM.Id);
        }

        
        private void ChoosePhotoMenuClick(object sender, RoutedEventArgs e)
        {
            this.VM.PickNewPhoto();
        }

        private async void DeletePhotoMenuClick(object sender, RoutedEventArgs e)
        {
            if (await MessageBox.Show("DeleteConfirmation", "DeleteOnePhoto")!= MessageBox.MessageBoxButton.OK)
                return;
            this.VM.DeletePhoto();
        }

        private void OpenPhotoMenuClick(object sender, RoutedEventArgs e)
        {
            this.VM.OpenProfilePhotos();
        }
	}
}
