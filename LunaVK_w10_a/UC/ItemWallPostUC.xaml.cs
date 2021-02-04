using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.ViewModels;
using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LunaVK.UC
{
    public sealed partial class ItemWallPostUC : UserControl
    {
        private bool _isPinning;

        public ItemWallPostUC()
        {
            this.InitializeComponent();
        }

        #region HideFooter
        /// <summary>
        /// Список прикреплений, которые требуется отобразить.
        /// </summary>
        public bool HideFooter
        {
            get { return (bool)GetValue(HideFooterProperty); }
            set { SetValue(HideFooterProperty, value); }
        }

        public static readonly DependencyProperty HideFooterProperty = DependencyProperty.Register("HideFooter", typeof(bool), typeof(ItemWallPostUC), new PropertyMetadata(false, ItemWallPostUC.OnHideFooterChanged));

        private static void OnHideFooterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var presenter = (ItemWallPostUC)obj;
            presenter._footerGrid.Visibility = (!(bool)e.NewValue).ToVisiblity();
        }
        #endregion

        public VKWallPost VM
        {
            get { return base.DataContext as VKWallPost; }
        }
        
        private void _headerTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            int to;

            var vm = this.VM;

            if (vm.from_id != 0)
                to = vm.from_id;
            else if (vm.OwnerId == 0)
                to = vm.owner_id;
            else
                to = vm.OwnerId;

            NavigatorImpl.Instance.NavigateToProfilePage(to);
        }

        private void action_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this.GenerateMenuItems_Post(sender);
        }











        private void editMenuItem_Click(object args)
        {
            //ParametersRepository.SetParameterForId("WallRepostInfo", this._wallRepostInfo);
            //this._wallPost.NavigateToEditWallPost(this.AdminLevel);
            NavigatorImpl.Instance.NavigateToNewWallPost(WallPostViewModel.Mode.EditWallPost, this.VM.owner_id, this.VM.AdminLevel, false, this.VM);
        }

        private async void _deleteMenuItem_Click(object args)
        {
            MessageDialog dialog = new MessageDialog(LocalizedStrings.GetString("Conversation_ConfirmDeletion"), LocalizedStrings.GetString("DeleteWallPost"));
            dialog.Commands.Add(new UICommand { Label = LocalizedStrings.GetString("Yes"), Id = 0 });
            dialog.Commands.Add(new UICommand { Label = LocalizedStrings.GetString("No"), Id = 1 });
            IUICommand res = await dialog.ShowAsync();
            if ((int)res.Id == 1)
                return;

            this.VM._deletedItemCallback?.Invoke();
        }

        private void unpinMenuItem_Click(object args)
        {
            this.PinUnpin();
        }

        private void pinMenuItem_Click(object args)
        {
            this.PinUnpin();
        }

        private void PinUnpin()
        {
            if (this._isPinning)
                return;
            this._isPinning = true;

            //this._wallPost.PinUnpin((res => Execute.ExecuteOnUIThread((() =>
            //{
            //    this._isPinning = false;
            //    this.ReleaseMenu();
            //    this.CreateMenu();
            //}))));
        }

        private void CopyLinkMI_OnClick(object args)
        {
            //Clipboard.SetText(string.Format("https://vk.com/wall{0}_{1}", this.WallPost.to_id, this.WallPost.id));
            var dataPackage = new DataPackage();
            string temp = "https://";
            if (CustomFrame.Instance.IsDevicePhone)
                temp += "m.";
            temp += "vk.com/wall";
            temp += this.VM.OwnerId;
            temp += "_";
            temp += this.VM.PostId;
            dataPackage.SetText(temp);
            Clipboard.SetContent(dataPackage);
        }

        private void goToOriginal_Click(object args)
        {
            NavigatorImpl.Instance.NavigateToWallPostComments(this.VM.copy_history[0].owner_id, this.VM.copy_history[0].id, 0, this.VM.copy_history[0]);
            //StatsEventsTracker.Instance.Handle(new PostActionEvent()
            //{
            //    PostId = this.WallPost.to_id.ToString() + "_" + this.WallPost.id,
            //    ActionType = PostActionType.Expanded
            //});
        }

        public void GenerateMenuItems_Post(object sender)
        {
            PopUP2 menuItemList = new PopUP2();

            if (this.VM.can_edit)
            {
                PopUP2.PopUpItem menuItem1 = new PopUP2.PopUpItem();
                menuItem1.Text = LocalizedStrings.GetString("Edit");
                menuItem1.Command = new DelegateCommand(this.editMenuItem_Click);
                menuItemList.Items.Add(menuItem1);
            }
            if (this.VM.can_publish == true || this.VM.IsPostponed)
            {
                PopUP2.PopUpItem menuItem = new PopUP2.PopUpItem();
                if (this.VM.IsPostponed)
                {
                    menuItem.Command = new DelegateCommand(this.publishNowMenuItem_Click);
                    menuItem.Text = LocalizedStrings.GetString("PublishNow");
                }
                else
                {
                    menuItem.Command = new DelegateCommand(this.publishMenuItem_Click);
                    menuItem.Text = LocalizedStrings.GetString("SuggestedNews_Publish");
                }
                menuItemList.Items.Add(menuItem);
            }
            if (/*this.AllowPinUnpin && this.CanPin*/this.VM.can_pin && !this.VM.is_pinned)
            {
                PopUP2.PopUpItem menuItem1 = new PopUP2.PopUpItem();
                menuItem1.Text = LocalizedStrings.GetString("PinPost");
                menuItem1.Command = new DelegateCommand(this.pinMenuItem_Click);
                menuItemList.Items.Add(menuItem1);
            }
            if (/*this.AllowPinUnpin && this.CanUnpin*/this.VM.can_pin && this.VM.is_pinned)
            {
                PopUP2.PopUpItem menuItem1 = new PopUP2.PopUpItem();
                menuItem1.Text = LocalizedStrings.GetString("UnpinPost");
                menuItem1.Command = new DelegateCommand(this.unpinMenuItem_Click);
                menuItemList.Items.Add(menuItem1);
            }
            if (/*this.CanDelete*/this.VM.can_delete)
            {
                PopUP2.PopUpItem menuItem1 = new PopUP2.PopUpItem();
                menuItem1.Text = LocalizedStrings.GetString("Delete");
                menuItem1.Command = new DelegateCommand(this._deleteMenuItem_Click);
                menuItemList.Items.Add(menuItem1);
            }
            /*
            if (!string.IsNullOrWhiteSpace(this.TextToCopy))
            {
                PopUP2.PopUpItem menuItem1 = new PopUP2.PopUpItem();
                string conversationCopy = CommonResources.Conversation_Copy;
                menuItem1.Text = conversationCopy;
                PopUP2.PopUpItem menuItem1 = menuItem1;
                // ISSUE: method pointer
                menuItem1.Command += new RoutedEventHandler(this.copyMenuItem_Click);
                menuItemList.Items.Add(menuItem1);
            }
            */
            if (this.VM.post_type != VKNewsfeedPostType.reply)
            {
                PopUP2.PopUpItem menuItem1 = new PopUP2.PopUpItem();
                menuItem1.Text = LocalizedStrings.GetString("CopyLink");
                menuItem1.Command = new DelegateCommand(this.CopyLinkMI_OnClick);
                menuItemList.Items.Add(menuItem1);
            }

            if (this.VM.CanGoToOriginal)
            {
                PopUP2.PopUpItem menuItem1 = new PopUP2.PopUpItem();
                menuItem1.Text = LocalizedStrings.GetString("GoToOriginal");
                menuItem1.Command = new DelegateCommand(this.goToOriginal_Click);
                menuItemList.Items.Add(menuItem1);
            }

            if (this.VM.IgnoreNewsfeedItemCallback != null /*&& this.GetIgnoreItemData() != null*/)
            {
                PopUP2.PopUpItem menuItem1 = new PopUP2.PopUpItem();
                menuItem1.Text = LocalizedStrings.GetString("HideThisPost");
                //menuItem1.Command += new RoutedEventHandler(this.HidePostItem_OnClick);
                menuItemList.Items.Add(menuItem1);
            }
            /*
            if (!this.CanDelete && this.HideSourceItemsCallback != null!this.VM.can_delete)
            {
                PopUP2.PopUpItem menuItem1 = new PopUP2.PopUpItem();
                menuItem1.Text = "HideFromNews";
                //menuItem1.Command += new RoutedEventHandler(this.HideFromNewsMenuItem_OnClick);
                menuItemList.Items.Add(menuItem1);
            }
            */

            //
            
            PopUP2.PopUpItem _appBarMenuItemFaveUnfave = new PopUP2.PopUpItem();
            _appBarMenuItemFaveUnfave.Text = LocalizedStrings.GetString(this.VM.is_favorite ? "RemoveFromBookmarks" : "AddToBookmarks");
            _appBarMenuItemFaveUnfave.Command = new DelegateCommand((args) =>
            {
                FavoritesService.Instance.AddRemovePost(this.VM.OwnerId, this.VM.PostId, !this.VM.is_favorite, (result) =>
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        if (result.error.error_code == VKErrors.None && result.response == 1)
                            this.VM.is_favorite = !this.VM.is_favorite;
                    });
                });
            });
            menuItemList.Items.Add(_appBarMenuItemFaveUnfave);
            

            if (this.VM.CanReport)
            {
                PopUP2.PopUpSubItem item2 = new PopUP2.PopUpSubItem();
                item2.Text = LocalizedStrings.GetString("Report") + "...";

                PopUP2.PopUpItem subitem = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("ReportReasonSpam"), CommandParameter = "0" };//1
                subitem.Command = new DelegateCommand((args) => { this.ReportPost(args); });
                item2.Items.Add(subitem);
                PopUP2.PopUpItem subitem2 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("ReportReasonChildPorn"), CommandParameter = "1" };//5
                subitem2.Command = new DelegateCommand((args) => { this.ReportPost(args); });
                item2.Items.Add(subitem2);
                PopUP2.PopUpItem subitem3 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("ReportReasonExtremism"), CommandParameter = "2" };//6
                subitem3.Command = new DelegateCommand((args) => { this.ReportPost(args); });
                item2.Items.Add(subitem3);
                PopUP2.PopUpItem subitem4 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("ReportReasonViolence"), CommandParameter = "3" };//7
                subitem4.Command = new DelegateCommand((args) => { this.ReportPost(args); });
                item2.Items.Add(subitem4);
                PopUP2.PopUpItem subitem5 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("ReportReasonDrug"), CommandParameter = "4" };//4
                subitem5.Command = new DelegateCommand((args) => { this.ReportPost(args); });
                item2.Items.Add(subitem5);
                PopUP2.PopUpItem subitem6 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("ReportReasonAdult"), CommandParameter = "5" };//3
                subitem6.Command = new DelegateCommand((args) => { this.ReportPost(args); });
                item2.Items.Add(subitem6);
                PopUP2.PopUpItem subitem7 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("ReportReasonInsult"), CommandParameter = "6" };//2
                subitem7.Command = new DelegateCommand((args) => { this.ReportPost(args); });
                item2.Items.Add(subitem7);

                menuItemList.Items.Add(item2);
            }




            menuItemList.ShowAt(sender as FrameworkElement);

        }

        private void publishNowMenuItem_Click(object args)
        {

            WallPostViewModel wallPostViewModel = new WallPostViewModel(this.VM, /*this.AdminLevel*/ VKAdminLevel.Moderator);
            //wallPostViewModel.WMMode = WallPostViewModel.Mode.PublishWallPost;
            //wallPostViewModel.IsPublishSuggestedSuppressed = true;
            IOutboundAttachment timerAttachment = wallPostViewModel.Attachments.FirstOrDefault((a => a.ToString() == "timestamp"));
            if (timerAttachment != null)
                wallPostViewModel.Attachments.Remove(timerAttachment);
            wallPostViewModel.Publish((result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    /*
                    if (result == VKErrors.None)
                    {
                        if (this._wallPost.IsFromGroup())
                        {
                            long groupId = -this._wallPost.owner_id;
                            Group group = this._groups.FirstOrDefault<Group>((Func<Group, bool>)(g => g.id == groupId));
                            if (group == null)
                                return;
                            GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.WallPost, groupId, group.name);
                        }
                        else if (this._wallPost.owner_id >= 0L)
                        {
                            GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.WallPost, 0, "");
                        }
                        else
                        {
                            long communityId = -this._wallPost.owner_id;
                            Group group = this._groups.FirstOrDefault<Group>((Func<Group, bool>)(g => g.id == communityId));
                            if (group == null)
                                return;
                            GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.WallPost, communityId, group.name);
                        }
                    }
                    else if (result == ResultCode.PostsLimitOrAlreadyScheduled)
                    {
                        if (timerAttachment != null)
                            new GenericInfoUC(2000).ShowAndHideLater(CommonResources.ScheduledForExistingTime, null);
                        else
                            new GenericInfoUC(2000).ShowAndHideLater(CommonResources.PostsLimitReached, null);
                    }
                    else
                        new GenericInfoUC(2000).ShowAndHideLater(CommonResources.Error, null);
                    */
                });
            });
        }

        private void publishMenuItem_Click(object args)
        {
            //this._wallPost.NavigateToPublishWallPost(this.AdminLevel);
        }

        private void CopyLinkMI_OnClick(object sender, RoutedEventArgs e)
        {
            //           Clipboard.SetText(string.Format("https://vk.com/wall{0}_{1}", this.WallPost.to_id, this.WallPost.id));
            var dataPackage = new DataPackage();
            string temp = "https://";
            //if (CustomFrame.Instance.IsDevicePhone)
            //    temp += "m.";
            temp += "vk.com/wall";
            temp += this.VM.OwnerId;
            temp += "_";
            temp += this.VM.PostId;
            dataPackage.SetText(temp);
            Clipboard.SetContent(dataPackage);
        }

        private async void ReportPost(object args)
        {
            /*
            VKPhoto photo = this.CurrentPhotoVM.Photo;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = photo.owner_id.ToString();
            parameters["photo_id"] = photo.id.ToString();
            parameters["reason"] = (string)args;
            var temp = await RequestsDispatcher.GetResponse<int>("photos.report", parameters);*/
            int i = 0;
        }

        private void Publish_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int ownerId =  this.VM.OwnerId;
            uint postId = this.VM.PostId;
            SharePostUC share = new SharePostUC( "постом", WallService.RepostObject.wall, ownerId, postId);
            PopUpService popUp = new PopUpService { Child = share, OverrideBackKey = true, AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed };
            share.Done = popUp.Hide;
            popUp.Show();
            e.Handled = true;
        }

        private void Comments_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VKNewsfeedPost vm = (sender as FrameworkElement).DataContext as VKNewsfeedPost;
            VKWallPost vm2 = (sender as FrameworkElement).DataContext as VKWallPost;
            //Library.NavigatorImpl.Instance.NavigateToWallPostComments(post.post_id, post.source_id, false, post);

            if (vm != null)
            {
                switch (vm.post_type)
                {
                    case VKNewsfeedPostType.post:
                        {
                            NavigatorImpl.Instance.NavigateToWallPostComments(vm.OwnerId, vm.PostId, 0, vm);
                            break;
                        }
                    //case Network.Enums.VKNewsfeedPostType.photo:
                    //    {
                    //        break;
                    //    }
                    case VKNewsfeedPostType.video:
                        {

                            //NavigatorImpl.Instance.NavigateToVideoWithComments(post.video.,);
                            e.Handled = true;
                            break;
                        }
                }
            }
            else if (vm2 != null)
            {
                switch (vm2.post_type)
                {
                    case VKNewsfeedPostType.post:
                        {
                            NavigatorImpl.Instance.NavigateToWallPostComments(vm2.OwnerId, vm2.PostId, 0, vm2);
                            break;
                        }
                    case VKNewsfeedPostType.video:
                        {
                            e.Handled = true;
                            break;
                        }
                }
            }

        }
        /*
        private LikeObjectType LikeObjType
        {
            get
            {
                VKNewsfeedPost vm = base.DataContext as VKNewsfeedPost;
                if (vm != null)
                {
                    switch (vm.post_type)
                    {
                        case VKNewsfeedPostType.photo:
                            return LikeObjectType.photo;
                        case VKNewsfeedPostType.video:
                            return LikeObjectType.video;
                        default:
                            return LikeObjectType.post;
                    }
                }
                else
                {
                    VKWallPost vm2 = base.DataContext as VKWallPost;
                    if (vm2 != null)
                    {
                        switch (vm2.post_type)
                        {
                            case VKNewsfeedPostType.photo:
                                return LikeObjectType.photo;
                            case VKNewsfeedPostType.video:
                                return LikeObjectType.video;
                            default:
                                return LikeObjectType.post;
                        }
                    }
                }
            }
        }
        */
        private void Like_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //(sender as FrameworkElement).IsHitTestVisible = false;//bug: если быстро крутить то станет навсегда недоступно
            LikeObjectType type;
            VKNewsfeedPost vmNewsPost = (sender as FrameworkElement).DataContext as VKNewsfeedPost;
            if (vmNewsPost != null)
            {
                switch (vmNewsPost.post_type)
                {
                    case VKNewsfeedPostType.photo:
                        type = LikeObjectType.photo;
                        break;
                    case VKNewsfeedPostType.video:
                        type = LikeObjectType.video;
                        break;
                    default:
                        type = LikeObjectType.post;
                        break;
                }

                LikesService.Instance.AddRemoveLike(vmNewsPost.likes.user_likes == false, vmNewsPost.OwnerId, vmNewsPost.PostId, type, (result) =>
                {

                    //(sender as FrameworkElement).IsHitTestVisible = true;

                    if (result != -1)
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            vmNewsPost.likes.count = (uint)result;
                            vmNewsPost.likes.user_likes = !vmNewsPost.likes.user_likes;

                            this.VM.UpdateUI();
                        });
                    }
                });

                return;
            }

            VKWallPost vmWallPost = (sender as FrameworkElement).DataContext as VKWallPost;
            if (vmWallPost != null)
            {
                switch (vmWallPost.post_type)
                {
                    case VKNewsfeedPostType.photo:
                        type = LikeObjectType.photo;
                        break;
                    case VKNewsfeedPostType.video:
                        type = LikeObjectType.video;
                        break;
                    default:
                        type = LikeObjectType.post;
                        break;
                }

                LikesService.Instance.AddRemoveLike(vmWallPost.likes.user_likes == false, vmWallPost.OwnerId, vmWallPost.PostId, type, (result) =>
                {

                    //(sender as FrameworkElement).IsHitTestVisible = true;

                    if (result != -1)
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            vmWallPost.likes.count = (uint)result;
                            vmWallPost.likes.user_likes = !vmWallPost.likes.user_likes;

                            this.VM.UpdateUI();
                        });
                    }
                });
            }


        }

        private void Signer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToProfilePage(this.VM.signer_id);
        }

        private void Copyrights_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToWebUri(this.VM.copyright.link);
        }
    }
}
