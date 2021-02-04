using LunaVK.Core;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.UC;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.ApplicationModel.Email;

namespace LunaVK.Pages
{
    public sealed partial class GroupPage : PageBase
    {
        private double VerticalOffset = 0;

        public GroupPage()
        {
            this.InitializeComponent();
            this.Loaded += this.GroupPage_Loaded;
            this.Unloaded += this.GroupPage_Unloaded;
            this.MainScroll.Loaded2 += this.InsideScrollViewerLoaded;

            this.borderOffset.MinHeight = CustomFrame.Instance.Header.HeaderHeight;
        }

        private void GroupPage_Unloaded(object sender, RoutedEventArgs e)
        {
            //CustomFrame.Instance.MenuStateChanged -= CFrame_MenuStateChanged;
            CustomFrame.Instance.Header.HeaderGrid.Tapped -= _header_Tapped;
            this.MainScroll.GetInsideScrollViewer.ViewChanged -= this.GetInsideScrollViewer_ViewChanged;
        }

        private void GroupPage_Loaded(object sender, RoutedEventArgs e)
        {
            //BugFix: возвращаемся обратно на страницу
            if (this.VM != null && this.VM._group != null)
            {
                if (this.VM._group.cover != null && this.VM._group.cover.enabled == false)
                    this.UpdateHeaderOpacity(1.0);
            }
            else
                CustomFrame.Instance.Header.BackGroundGrid.Opacity = 0;

            if (this.VM._group != null)
            {
                base.Title = this.VM._group.Title;
            }

            CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE712", Clicked = this._appBarButtonSearch_Click });
            CustomFrame.Instance.Header.HeaderGrid.Tapped += this._header_Tapped;
        }

        private void _appBarButtonSearch_Click(object sender)
        {
            if(this.VM._group!=null)
                this.HeaderOffsetUC_Tapped(null, null);
        }

        public GroupViewModel VM
        {
            get { return base.DataContext as GroupViewModel; }
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                
                this.VerticalOffset = (double)pageState["ScrollOffset"];
                this.MainScroll.NeedReload = false;
                
                this.CreateAppBar();
                this.HandleLoadingStatusUpdated(ProfileLoadingStatus.Loaded);
                this._main.Visibility = Visibility.Visible;//BugFix - делаем шапку видимой, а то LoadingStatusUpdated не срабатывает :(
                this.VM.UpdateCoverVisibility();
                this._groupCoverImg.Opacity = 1.0;
                
                if (this.VerticalOffset == 0)
                    this.UpdateHeaderOpacity(0);
                else
                    this.UpdateHeaderOpacityWithScrollPosition(this.VerticalOffset);
                    
            }
            else
            {
                uint Id = (uint)navigationParameter;
                this.DataContext = new GroupViewModel(Id);
            }
            this.VM.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;
            //this.VM.CoverLoaded = this.Image_Loaded;
        }

        private void Image_Loaded()
        {
            this._groupCoverImg.Animate(0, 1, "Opacity", 300);

        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            CustomFrame.Instance.Header.BackGroundGrid.Opacity = 1.0;

            pageState["Data"] = this.VM;
            pageState["ScrollOffset"] = this.MainScroll.GetInsideScrollViewer.VerticalOffset;

            this.VM.LoadingStatusUpdated -= this.HandleLoadingStatusUpdated;
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            img.Animate(0, 1, "Opacity", 300);
            img.ImageOpened -= this.Image_ImageOpened;
        }

        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(status);
#endif
            if (status == ProfileLoadingStatus.Loaded && this.VM.PostsToggleViewModel == null)
            {
                //CustomFrame.Instance.MenuStateChanged += CFrame_MenuStateChanged;

                base.Title = this.VM._group.Title;
//                CustomFrame.Instance.HeaderWithMenu.UpdateTitleBinding();

                if (this.VM._group != null)
                {
                    if (this.VM._group.cover != null && this.VM._group.cover.enabled == false)
                        this.UpdateHeaderOpacity(1.0);
                }
            }
            

            string str;
            bool block = true;

            switch (status)
            {
                case ProfileLoadingStatus.Loading:
                    //str = "Loading";
                    //break;
                    return;
                case ProfileLoadingStatus.Reloading:
                    //str = "Reloading";
                    //break;
                    return;
                case ProfileLoadingStatus.Loaded:
                    str = "LoadedState";
                    block = false;
                    this.CreateAppBar();
                    break;
                case ProfileLoadingStatus.LoadingFailed:
                    str = "LoadingFailed";
                    break;
                case ProfileLoadingStatus.Deleted:
                case ProfileLoadingStatus.Banned:
                case ProfileLoadingStatus.Blacklisted:
                case ProfileLoadingStatus.Service:
                    str = "Blocked";
                    this.CreateAppBar();//hack: я хочу блокировать мёртвых собачек
                    break;
                case ProfileLoadingStatus.Private:
                    str = "Private";
                    break;
                default:
                    return;
            }

            this.borderOffset.IsHitTestVisible = !block;
            VisualStateManager.GoToState(this, str, false);
        }

        private void Cover_SizeChanged(object sender, SizeChangedEventArgs e)
        {
 //           this.transformCoverScale.CenterX = e.NewSize.Width / 2.0;

            if (e.NewSize.Height != e.PreviousSize.Height)
            {
                    this.borderOffset.Height = e.NewSize.Height;
            }
        }

        private void InfoItem_OnTap(object sender, TappedRoutedEventArgs e)
        {
            ProfileInfoItem dataContext = ((FrameworkElement)sender).DataContext as ProfileInfoItem;
            if (dataContext == null || dataContext.NavigationAction == null)
                return;
            e.Handled = true;
            dataContext.NavigationAction();
        }

        private void BorderFullInformation_OnTap(object sender, TappedRoutedEventArgs e)
        {
            this.ring.IsActive = true;
            this.panelMoreInfo.IsHitTestVisible = false;
            this.VM.ShowFullInfoPopup((result) => {
                if (result)
                    this.panelMoreInfo.Visibility = Visibility.Collapsed;

                this.ring.IsActive = false;
                this.panelMoreInfo.IsHitTestVisible = true;
            });
        }

        private void Third_Click(object sender, RoutedEventArgs e)
        {
            this.ShowSecondaryMenu(sender as FrameworkElement);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToConversation((int)-this.VM._gid);
        }

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            Button element = sender as Button;
            
            if (this.VM._group.action_button != null)
            {
                if (this.VM._group.action_button.action_type == VKGroupActionBtnType.open_group_app)
                {
                    NavigatorImpl.Instance.NavigateToProfileAppPage(this.VM._group.action_button.target.app_id, (int)-this.VM._gid, "");
                }
                else if (this.VM._group.action_button.action_type == VKGroupActionBtnType.send_email)
                {
                    this.SendEmailButton_Click(this.VM._group.action_button.target.email);
                }
                return;
            }

            this.ShowSecondaryMenu(element);
        }
        
        private async void SendEmailButton_Click(string email)
        {
            var emailMessage = new EmailMessage();
            emailMessage.To.Add(new EmailRecipient(email));
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        private void ShowSecondaryMenu(FrameworkElement element)
        {
            MenuFlyout menu = new MenuFlyout();

            switch (this.VM._group.member_status)
            {
                case VKGroupMembershipType.Member:
                    {
                        switch (this.VM._group.type)
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
                        if (this.VM._group.is_closed != VKGroupIsClosed.Opened)
                        {
                            MenuFlyoutItem item = new MenuFlyoutItem();
                            item.Text = LocalizedStrings.GetString("GroupPage_SendRequest");
                            menu.Items.Add(item);

                            break;
                        }
                        switch (this.VM._group.type)
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

        private void HeaderOffsetUC_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PopUP2 menu = new PopUP2();
            
            if (this.VM.CanPost || this.VM.CanSuggestAPost)
            {
                PopUP2.PopUpItem item = new PopUP2.PopUpItem();
                item.Icon = new SymbolIcon(Symbol.Add);
                item.Text = LocalizedStrings.GetString(this.VM.CanSuggestAPost ? "SuggestedNews_SuggestAPost" : "MainPage_News_AddNews");
                item.Command = new DelegateCommand((args) => {
                    this.VM.NavigateToNewWallPost();
                });
                menu.Items.Add(item);
            }

            if (this.VM.CanManageCommunity)
            {
                PopUP2.PopUpSubItem item = new PopUP2.PopUpSubItem();
                item.Icon = new SymbolIcon(Symbol.Setting);
                item.Text = "управление";

                PopUP2.PopUpItem subItem = new PopUP2.PopUpItem();
                subItem.Icon = new IconUC() { Glyph = "\xE946" };
                subItem.Text = "Информация";
                subItem.Command = new DelegateCommand((args) => { NavigatorImpl.Instance.NavigateToCommunityManagementInformation(this.VM._gid); });
                item.Items.Add(subItem);

                PopUP2.PopUpItem subItem2 = new PopUP2.PopUpItem();
                subItem2.Icon = new IconUC() { Glyph = "\xE74C" };
                subItem2.Text = "Разделы";
                subItem2.Command = new DelegateCommand((args) => { NavigatorImpl.Instance.NavigateToCommunityManagementServices(this.VM._gid); });
                item.Items.Add(subItem2);

                PopUP2.PopUpItem subItem3 = new PopUP2.PopUpItem();
                subItem3.Icon = new IconUC() { Glyph = "\xE8D4" };
                subItem3.Text = "Руководители";
                subItem3.Command = new DelegateCommand((args) => { NavigatorImpl.Instance.NavigateToCommunityManagementManagers(this.VM._gid); });
                item.Items.Add(subItem3);

                if (this.VM._group.type == VKGroupType.Group)
                {
                    PopUP2.PopUpItem subItem4 = new PopUP2.PopUpItem();
                    subItem4.Icon = new IconUC() { Glyph = "\xE8FB" };
                    subItem4.Text = "Заявки";
                    subItem4.Command = new DelegateCommand((args) => { NavigatorImpl.Instance.NavigateToCommunityManagementRequests(this.VM._gid); });
                    item.Items.Add(subItem4);
                }

                if (this.VM._group.type != VKGroupType.Page)
                {
                    PopUP2.PopUpItem subItem5 = new PopUP2.PopUpItem();
                    subItem5.Icon = new IconUC() { Glyph = "\xE8FA" };
                    subItem5.Text = "Приглашения";
                    item.Items.Add(subItem5);
                }

                PopUP2.PopUpItem subItem6 = new PopUP2.PopUpItem();
                subItem6.Icon = new IconUC() { Glyph = "\xE716" };
                subItem6.Text = "Участники";
                subItem6.Command = new DelegateCommand((args) => { NavigatorImpl.Instance.NavigateToCommunitySubscribers(this.VM._gid,this.VM._group.type); });
                item.Items.Add(subItem6);

                PopUP2.PopUpItem subItem7 = new PopUP2.PopUpItem();
                subItem7.Icon = new IconUC() { Glyph = "\xE8F8" };
                subItem7.Text = "Чёрный список";
                subItem7.Command = new DelegateCommand((args) => { NavigatorImpl.Instance.NavigateToCommunityManagementBlacklist(this.VM._gid); });
                item.Items.Add(subItem7);

                PopUP2.PopUpItem subItem8 = new PopUP2.PopUpItem();
                subItem8.Icon = new IconUC() { Glyph = "\xE71B" };
                subItem8.Text = "Ссылки";
                subItem8.Command = new DelegateCommand((args) => { NavigatorImpl.Instance.NavigateToCommunityManagementLinks(this.VM._gid); });
                item.Items.Add(subItem8);
                
                menu.Items.Add(item);

                //               this.VM.Commands.Add(new GroupViewModel.CommandVM("\xE713", "управление", () => { NavigatorImpl.Instance.NavigateToCommunityManagement((uint)(-this.VM.Id), this.VM.GroupData.type); }));
            }

            if (this.VM.CanSubscribeUnsubscribe)
            {
                PopUP2.PopUpItem item = new PopUP2.PopUpItem();
                item.Text = this.VM.IsSubscribed ? "не уведомлять о новых записях" : "уведомлять о новых записях";
                item.Icon = new IconUC() { Glyph = this.VM.IsSubscribed ? "\xE7ED" : "\xE781" };//new SymbolIcon(Symbol.Account);
                item.Command = new DelegateCommand((args) =>
                {
                    this.VM.SubscribeUnsubscribe((ret) =>
                    {
                        this.CreateAppBar();
                    });
                });
                //applicationBar.SecondaryCommands.Add(btn2);
                menu.Items.Add(item);

            }
            //if (this._viewModel.CanPinToStart)

            PopUP2.PopUpItem item3 = new PopUP2.PopUpItem();
            item3.Text = LocalizedStrings.GetString("CopyLink");
            item3.Icon = new SymbolIcon(Symbol.Copy);
            item3.Command = new DelegateCommand((args) =>
            {
                var dataPackage = new DataPackage();
                string temp = "https://";
                if (CustomFrame.Instance.IsDevicePhone)
                    temp += "m.";
                temp += "vk.com/club";
                temp += Math.Abs(this.VM._gid);
                dataPackage.SetText(temp);
                Clipboard.SetContent(dataPackage);
            });
            menu.Items.Add(item3); //applicationBar.SecondaryCommands.Add(btn3);



            PopUP2.PopUpItem item4 = new PopUP2.PopUpItem();
            item4.Text = LocalizedStrings.GetString("OpenInBrowser");
            item4.Icon = new SymbolIcon(Symbol.Link);
            item4.Command = new DelegateCommand((args) =>
            {
                string temp = "https://";
                if (CustomFrame.Instance.IsDevicePhone)
                    temp += "m.";
                temp += "vk.com/club";
                temp += Math.Abs(this.VM._gid);
                NavigatorImpl.Instance.NavigateToWebUri(temp, true);
            });
            menu.Items.Add(item4); //applicationBar.SecondaryCommands.Add(btn4);



            if (this.VM.CanFaveUnfave)
            {
                PopUP2.PopUpItem item = new PopUP2.PopUpItem();
                item.Text = LocalizedStrings.GetString(this.VM.IsFavorite ? "RemoveFromBookmarks" : "AddToBookmarks");
                item.Icon = new SymbolIcon(Symbol.Favorite);
                item.Command = new DelegateCommand((args) =>
                {
                    this.VM.FaveUnfave((ret) =>
                    {
                        this.CreateAppBar();
                    });
                });
                menu.Items.Add(item); //applicationBar.SecondaryCommands.Add(btn5);


            }
            /*
            if (this.VM.CanBanUnban)
            {
                AppBarButton btn6 = new AppBarButton();
                btn6.Label = LocalizedStrings.GetString(this.VM.IsBlacklistedByMe ? "BannedUsers_UnbanUser" : "BannedUsers_BanUser");
                btn6.Icon = new SymbolIcon(Symbol.BlockContact);
                btn6.Command = new DelegateCommand((args) =>
                {
                    this.VM.BanUnban((ret) =>
                    {
                        //this.VM.IsBlacklistedByMe = !this.VM.IsBlacklistedByMe;
                        this.CreateAppBar();
                    });
                });
                applicationBar.SecondaryCommands.Add(btn6);

                //this.VM.Commands.Add(new GroupViewModel.CommandVM("\xF140", LocalizedStrings.GetString(this.VM.IsBlacklistedByMe ? "BannedUsers_UnbanUser" : "BannedUsers_BanUser"), () => { this.VM.BanUnban(); }));
            }
            */
            
            
            
            //this.VM.Commands.Add(new GroupViewModel.CommandVM("\xE728", LocalizedStrings.GetString(this.VM.GroupData.is_member ? "RemoveFromBookmarks" : "AddToBookmarks")));
            
            /*
            if (this.VM.CanSendGift)
            {
                AppBarButton btn = new AppBarButton();
                btn.Label = "отправить подарок";
                btn.Icon = new SymbolIcon(Symbol.Emoji);
                btn.Command = new DelegateCommand((args) =>
                {
                    //this.VM.FaveUnfave((ret) =>
                    //{
                    //    this.VM.UserData.is_favorite = !this.VM.UserData.is_favorite;
                    //    this.CreateAppBar();
                    //});
                });
                applicationBar.PrimaryCommands.Add(btn);
            }
            */
            if (sender == null)
                sender = this._groupCoverImg;
            menu.ShowAt(sender as FrameworkElement);
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaElement element = sender as MediaElement;
            element.Position = new TimeSpan();
            element.Play();
        }

        private void MediaElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height != e.PreviousSize.Height)
            {
                this.borderOffset.Height = e.NewSize.Height/2.0;

                //
                this.transformCover2.Y = this.transformCover.Y - (this._mediaElement.ActualHeight / 4.0);
            }
        }

        private void InsideScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            this.MainScroll.GetInsideScrollViewer.ViewChanged += this.GetInsideScrollViewer_ViewChanged;

            if (this.VerticalOffset != 0)
                this.MainScroll.GetInsideScrollViewer.ChangeView(0, this.VerticalOffset, 1.0f);
        }

        private void GetInsideScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            this.transformCover.Y = -(sv.VerticalOffset / 2.0);
            this.transformCover2.Y = this.transformCover.Y - (this._mediaElement.ActualHeight/4.0);

            if (this.VM._group != null)
            {
                if (this.VM._group.cover != null && this.VM._group.cover.enabled)
                    this.UpdateHeaderOpacityWithScrollPosition(sv.VerticalOffset);
            }
            else
            {
                this.UpdateHeaderOpacityWithScrollPosition(sv.VerticalOffset);
            }
        }

        private void UpdateHeaderOpacityWithScrollPosition(double scrollPosition)
        {
            this.UpdateHeaderOpacity(this.CalculateOpacity(scrollPosition, this.borderOffset.ActualHeight / 2.0, this.borderOffset.ActualHeight)); //this.UpdateHeaderOpacity(this.CalculateOpacity(scrollPosition, 200.0, 224.0));
            //this.ucProfileInfoHeader.SetOverlayOpacity(this.CalculateOpacity(scrollPosition, 96.0, 200.0));
        }

        private void UpdateHeaderOpacity(double opacity)
        {
            double reversed = 1.0 - opacity;
            CustomFrame.Instance.Header.BackGroundGrid.Opacity = opacity;

            this._groupCoverImg.Opacity = reversed;
            //this.ucHeader.rectBackground.Opacity = opacity;
            //this.ucHeader.textBlockTitle.Opacity = opacity;
            //this.ucHeader.borderCounter.Opacity = opacity;

            if (this._mediaElement.Source!=null)
            {
                this._mediaElement.Volume = reversed;
                this._mediaElement.Opacity = reversed;
            }
        }

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

        private void UcProfileInfoHeader_OnTap(object sender, TappedRoutedEventArgs e)
        {
            /*
            if (this.VM.HeaderViewModel.HasAvatar)
            {
                if (this.VM.CanChangePhoto)
                    this.PhotoMenu.IsOpen = true;
                else
                    this.VM.OpenProfilePhotos();
            }
            else
            {
                if (!this.VM.CanChangePhoto)
                    return;
                this.VM.PickNewPhoto();
            }*/
            this.VM.OpenProfilePhotos();
        }

        private void Command_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GroupViewModel.CommandVM vm = (sender as FrameworkElement).DataContext as GroupViewModel.CommandVM;
            vm.Callback?.Invoke();
        }

        private void CreateAppBar()
        {
            this.VM.Commands.Clear();

            
            if (this.VM.CanPost || this.VM.CanSuggestAPost)
            {
                this.VM.Commands.Add(new GroupViewModel.CommandVM("\xE710", LocalizedStrings.GetString(this.VM.CanSuggestAPost ? "SuggestedNews_SuggestAPost" : "MainPage_News_AddNews"), this.VM.NavigateToNewWallPost));
            }

            if (this.VM.CanManageCommunity)
            {
                this.VM.Commands.Add(new GroupViewModel.CommandVM("\xE713", "управление", () => { NavigatorImpl.Instance.NavigateToCommunityManagement(this.VM._gid, this.VM._group.type); }));
            }

            if (this.VM.CanSubscribeUnsubscribe)
            {
                this.VM.Commands.Add(new GroupViewModel.CommandVM("\xEA8F", this.VM.IsSubscribed ? "не уведомлять о новых записях" : "уведомлять о новых записях", () =>
                {
                    this.VM.SubscribeUnsubscribe((ret) =>
                    {
                       this.CreateAppBar();
                    });
                }));
            }
            //if (this._viewModel.CanPinToStart)

            

            this.VM.Commands.Add(new GroupViewModel.CommandVM("\xE71B", "копировать ссылку", () =>
            {
                var dataPackage = new DataPackage();
                string temp = "https://";
                if (CustomFrame.Instance.IsDevicePhone)
                    temp += "m.";
                temp += "vk.com/club";
                temp += Math.Abs(this.VM._gid);
                dataPackage.SetText(temp);
                Clipboard.SetContent(dataPackage);
            }));

            

            this.VM.Commands.Add(new GroupViewModel.CommandVM("\xE774", "открыть в браузере", () =>
            {
                string temp = "https://";
                if (CustomFrame.Instance.IsDevicePhone)
                    temp += "m.";
                temp += "vk.com/club";
                temp += Math.Abs(this.VM._gid);
                NavigatorImpl.Instance.NavigateToWebUri(temp, true);
            }));

            if (this.VM.CanFaveUnfave)
            {
                this.VM.Commands.Add(new GroupViewModel.CommandVM("\xE728", LocalizedStrings.GetString(this.VM.IsFavorite ? "RemoveFromBookmarks" : "AddToBookmarks"), () =>
                {
                    this.VM.FaveUnfave((ret) =>
                    {
                        this.CreateAppBar();
                    });
                }));
            }
            /*
            if (this.VM.CanBanUnban)
            {
                AppBarButton btn6 = new AppBarButton();
                btn6.Label = LocalizedStrings.GetString(this.VM.IsBlacklistedByMe ? "BannedUsers_UnbanUser" : "BannedUsers_BanUser");
                btn6.Icon = new SymbolIcon(Symbol.BlockContact);
                btn6.Command = new DelegateCommand((args) =>
                {
                    this.VM.BanUnban((ret) =>
                    {
                        //this.VM.IsBlacklistedByMe = !this.VM.IsBlacklistedByMe;
                        this.CreateAppBar();
                    });
                });
                applicationBar.SecondaryCommands.Add(btn6);

                this.VM.Commands.Add(new GroupViewModel.CommandVM("\xF140", LocalizedStrings.GetString(this.VM.IsBlacklistedByMe ? "BannedUsers_UnbanUser" : "BannedUsers_BanUser"), () => { this.VM.BanUnban(); }));
            }
            */


        }

        private void _header_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.MainScroll.GetInsideScrollViewer.ChangeView(0, 0, 1);
        }
    }
}
