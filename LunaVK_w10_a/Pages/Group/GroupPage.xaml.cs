using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.UC;
using LunaVK.UC.PopUp;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LunaVK.Pages.Group
{
    public sealed partial class GroupPage : PageBase
    {
        private PopUpService dialogService;
        private ProfileInfoFullViewModel fullInfoViewModel;
        private double VerticalOffset = 0;
        private OptionsMenuItem _manageItem;
        private OptionsMenuItem _moreItem;

        public GroupPage()
        {
            this.InitializeComponent();
            base.Loaded += this.GroupPage_Loaded;
            base.Unloaded += this.GroupPage_Unloaded;
            this.MainScroll.Loaded2 += this.InsideScrollViewerLoaded;

            this._manageItem = new OptionsMenuItem() { Icon= "\xE713", Clicked = this._appBarButtonManage_Click };
            this._moreItem = new OptionsMenuItem() { Icon = "\xE712", Clicked = this._appBarButtonMore_Click };

            this._root.SizeChanged += MainScroll_SizeChanged;
        }

        private void MainScroll_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateCoverSize();
        }

        private void UpdateCoverSize()
        {
            double w = this._root.ActualWidth;
            int span = Grid.GetColumnSpan(this.MainScroll);
            if(span==2)
            {
                w = Math.Min(600, this._root.ActualWidth);
            }
            
            this._imgCover.Height = w / 4;
        }

        private void GroupPage_Unloaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.HeaderGrid.Tapped -= this._header_Tapped;
            this.MainScroll.GetInsideScrollViewer.ViewChanged -= this.GetInsideScrollViewer_ViewChanged;
        }

        private void GroupPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.borderOffset.MinHeight = CustomFrame.Instance.Header.HeaderHeight;

            //BugFix: возвращаемся обратно на страницу
            if (this.VM != null && this.VM.Group != null)
            {
                if (this.VM.Group.cover != null && this.VM.Group.cover.enabled == false)
                    this.UpdateHeaderOpacity(1.0);
            }
            else
                CustomFrame.Instance.Header.BackGroundGrid.Opacity = 0;

            if (this.VM.Group != null)
            {
                base.Title = this.VM.Group.Title;
            }
            else
            {
                base.Title = "club" + this.VM.Id;
            }
            //CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE712", Clicked = this._appBarButtonSearch_Click });
            CustomFrame.Instance.Header.HeaderGrid.Tapped += this._header_Tapped;
            this.UpdateCoverSize();
        }

        public GroupViewModel2 VM
        {
            get { return base.DataContext as GroupViewModel2; }
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
                this._gridCover.Opacity = 1.0;

                if (this.VerticalOffset == 0)
                    this.UpdateHeaderOpacity(0);
                else
                    this.UpdateHeaderOpacityWithScrollPosition(this.VerticalOffset);
                    
            }
            else
            {
                uint id = (uint)navigationParameter;
                base.DataContext = new GroupViewModel2(id);
            }

            this.VM.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            CustomFrame.Instance.Header.BackGroundGrid.Opacity = 1.0;

            pageState["Data"] = this.VM;
            pageState["ScrollOffset"] = this.MainScroll.GetInsideScrollViewer.VerticalOffset;

            this.VM.LoadingStatusUpdated -= this.HandleLoadingStatusUpdated;
        }

        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {
            if (status == ProfileLoadingStatus.Loaded && this.VM.PostsToggleViewModel == null)
            {
                if (this.VM.Group != null)
                {
                    base.Title = this.VM.Group.Title;

                    if (this.VM.Group.cover != null && this.VM.Group.cover.enabled == false)
                        this.UpdateHeaderOpacity(1.0);
                }
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
                    //block = false;
                    this.CreateAppBar();
                    break;
                case ProfileLoadingStatus.LoadingFailed:
                    str = "LoadingFailed";
                    break;
                case ProfileLoadingStatus.Deleted:
                case ProfileLoadingStatus.Banned:
                case ProfileLoadingStatus.Blacklisted:
                    str = "Blocked";//todo: в Лоадед
                    //this.CreateAppBar();//hack: я хочу блокировать мёртвых собачек
                    break;
                default:
                    return;
            }

            VisualStateManager.GoToState(this, str, false);
        }


        private async void SendEmailButton_Click(string email)
        {
            var emailMessage = new EmailMessage();
            emailMessage.To.Add(new EmailRecipient(email));
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        private void BorderFullInformation_OnTap(object sender, TappedRoutedEventArgs e)
        {
            this.ShowFullInfoPopup();
        }

        private void ShowFullInfoPopup()
        {
            if (this.fullInfoViewModel == null)
            {
                this.fullInfoViewModel = this.VM.GetFullInfoViewModel(true);
            }

            FullInfoUC uc = new FullInfoUC();
            uc.DataContext = this.fullInfoViewModel;
            this.dialogService = new PopUpService();
            this.dialogService.Child = uc;
            this.dialogService.OverrideBackKey = true;
            this.dialogService.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            this.dialogService.Show();
        }


        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }

        private void Join_Tapped(object sender, TappedRoutedEventArgs e)
        {
            switch (this.VM.Group.member_status)
            {
                case VKGroupMembershipType.Member:
                    {
                        switch (this.VM.Group.type)
                        {
                            case VKGroupType.Group:
                                {
                                    this.VM.GroupLeave();
                                    break;
                                }
                            case VKGroupType.Page:
                                {
                                    this.VM.GroupLeave();
                                    break;
                                }
                            case VKGroupType.Event:
                                {
                                    this.VM.GroupLeave();
                                    break;
                                }
                        }
                        break;
                    }
                case VKGroupMembershipType.NotSure:
                    {
                        this.VM.GroupJoin(false);
                        break;
                    }
                case VKGroupMembershipType.RequestSent:
                    {
                        this.VM.GroupLeave();
                        break;
                    }
                case VKGroupMembershipType.InvitationReceived:
                case VKGroupMembershipType.NotAMember:
                    {
                        if (this.VM.Group.is_closed != VKGroupIsClosed.Opened)
                        {
                            this.VM.GroupJoin(null);

                            break;
                        }
                        switch (this.VM.Group.type)
                        {
                            case VKGroupType.Group:
                                {
                                    this.VM.GroupJoin(null);
                                    break;
                                }
                            case VKGroupType.Page:
                                {
                                    this.VM.GroupJoin(null);
                                    break;
                                }
                            case VKGroupType.Event:
                                {
                                    this.VM.GroupJoin(null);
                                    break;
                                }
                        }
                        break;
                    }
            }
            }

        private void Notifications_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.VM.SubscribeUnsubscribe();
        }

        private void Favorite_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.VM.FaveUnfave();
        }

        private void MediaElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height != e.PreviousSize.Height)
            {
                this.borderOffset.Height = e.NewSize.Height / 2.0;

                //
                this.transformCover2.Y = this.transformCover.Y - (this._mediaElement.ActualHeight / 4.0);
            }
        }

        private void Cover_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //           this.transformCoverScale.CenterX = e.NewSize.Width / 2.0;

            if (e.NewSize.Height != e.PreviousSize.Height)
            {
                this.borderOffset.Height = this.borderOffset2.Height = e.NewSize.Height;
            }
        }

        private void Command_Tapped(object sender, TappedRoutedEventArgs e)
        {
        //    GroupViewModel.CommandVM vm = (sender as FrameworkElement).DataContext as GroupViewModel.CommandVM;
        //    vm.Callback?.Invoke();
        }

        private void InsideScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            sv.ViewChanged += this.GetInsideScrollViewer_ViewChanged;

            if (this.VerticalOffset != 0)
                this.MainScroll.GetInsideScrollViewer.ChangeView(0, this.VerticalOffset, 1.0f);
        }

        private void GetInsideScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            this.transformCover.Y = -(sv.VerticalOffset / 2.0);
            double h = this.SecondContent.ActualHeight;
            double h2 = base.ActualHeight;
            double dif = h2 - h;

            double to = sv.VerticalOffset;
            if (dif>0)
            {
                if(to> this.borderOffset2.ActualHeight)
                    to = this.borderOffset2.ActualHeight;
            }
            else if ((-to) < dif)
                to = -dif;
            this.trSecondContent.Y = -to;
            this.transformCover2.Y = this.transformCover.Y - (this._mediaElement.ActualHeight / 4.0);
            
            if (this.VM.Group != null)
            {
                if (this.VM.Group.cover != null && this.VM.Group.cover.enabled)
                    this.UpdateHeaderOpacityWithScrollPosition(sv.VerticalOffset);
            }
            else
            {
                this.UpdateHeaderOpacityWithScrollPosition(sv.VerticalOffset);
            }
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaElement element = sender as MediaElement;
            element.Position = new TimeSpan();
            element.Play();
        }

        private void Msg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToConversation((int)-this.VM.Id);
        }

        private void Action_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.VM.Group.action_button != null)
            {
                if (this.VM.Group.action_button.action_type == VKGroupActionBtnType.open_group_app)
                {
                    NavigatorImpl.Instance.NavigateToProfileAppPage(this.VM.Group.action_button.target.app_id, (int)-this.VM.Id, "");
                }
                else if (this.VM.Group.action_button.action_type == VKGroupActionBtnType.send_email)
                {
                    this.SendEmailButton_Click(this.VM.Group.action_button.target.email);
                }
                else if (this.VM.Group.action_button.action_type == VKGroupActionBtnType.call_phone)
                {
                    PhoneCallManager.ShowPhoneCallUI(this.VM.Group.action_button.target.phone, this.VM.Title);
                }
                return;
            }
        }

        private void Share_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SharePostUC share = new SharePostUC("ссылкой", WallService.RepostObject.photo, 0, 0, "","club"+this.VM.Id);
            share.HideOptions();
            PopUpService popUp = new PopUpService { Child = share, OverrideBackKey = true, AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed };
            share.Done = popUp.Hide;
            popUp.Show();

            e.Handled = true;
        }

        private void InfoListItemUC_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            ProfileInfoItem dataContext = (sender as FrameworkElement).DataContext as ProfileInfoItem;
            if (dataContext == null || dataContext.NavigationAction == null)
                return;
            dataContext.NavigationAction();
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

            this._gridCover.Opacity = reversed;

            if (this._mediaElement.Source != null)
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

        private void _header_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.MainScroll.GetInsideScrollViewer.ChangeView(0, 0, 1);
        }

        private void CreateAppBar()
        {
            if (CustomFrame.Instance.Header.OptionsMenu.Contains(this._manageItem))
                CustomFrame.Instance.Header.OptionsMenu.Remove(this._manageItem);
            if (CustomFrame.Instance.Header.OptionsMenu.Contains(this._moreItem))
                CustomFrame.Instance.Header.OptionsMenu.Remove(this._moreItem);

            CustomFrame.Instance.Header.OptionsMenu.Add(this._moreItem);

            if (this.VM.CanManageCommunity)
                CustomFrame.Instance.Header.OptionsMenu.Add(this._manageItem);
        }

        private void _appBarButtonManage_Click(object sender)
        {
            NavigatorImpl.Instance.NavigateToCommunityManagement(this.VM.Id, this.VM.Group.type, this.VM.Group.admin_level);
        }

        private void _appBarButtonMore_Click(object sender)
        {
            //пригласить друзей groups.invite не для страниц, а для групп ("type": "group")
            //запретить соощения
            //сохр в закладках
            //скопировать ссылку
            //откр в браузере
            //откр qr код
            //пожаловаться
            PopUP2 menu = new PopUP2();



            //if (this._viewModel.CanPinToStart) _appBarMenuItemPinToStart AppBarMenuItemPinToStart_OnClick

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
                temp += "vk.com/club";
                temp += Math.Abs(this.VM.Id);
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
                temp += "vk.com/club";
                temp += Math.Abs(this.VM.Id);
                NavigatorImpl.Instance.NavigateToWebUri(temp, true);
            });
            menu.Items.Add(item4);
            /*
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
            */





            //if (sender == null)
            //     sender = this._gridCover;
            menu.ShowAt(sender as FrameworkElement);
        }

        private void AppBarMenuItemPinToStart_OnClick(/*object sender, EventArgs e*/)
        {
            this.VM.PinToStart();
        }

        private void ReportGroup(object args)
        {
            GroupsService.Instance.Report(this.VM.Id,(ReportReason)args, null);
        }

        //_appBarButtonAddNews AppBarButtonAddNews_OnClick
        private void NewPost_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToNewWallPost(WallPostViewModel.Mode.NewWallPost, -(int)this.VM.Id, this.VM.Group.admin_level, this.VM.Group.type == VKGroupType.Page);
        }


        private void WikiPage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToGroupWikiPages(this.VM.Id, this.VM.WikiPageText);

        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            img.Animate(0, 1, "Opacity", 300);
            img.ImageOpened -= this.Image_ImageOpened;
        }

        private void Conversations_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToConversations(this.VM.Id);
        }
    }
}
