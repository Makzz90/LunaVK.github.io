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
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LunaVK.UC
{
    public sealed partial class ItemNewsFeedUC : UserControl
    {
        private bool _isPinning;

        public ItemNewsFeedUC()
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

        public static readonly DependencyProperty HideFooterProperty = DependencyProperty.Register("HideFooter", typeof(bool), typeof(ItemNewsFeedUC), new PropertyMetadata(false, ItemNewsFeedUC.OnHideFooterChanged));

        private static void OnHideFooterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var presenter = (ItemNewsFeedUC)obj;
            presenter._footerGrid.Visibility = (!(bool)e.NewValue).ToVisiblity();
        }
        #endregion


        private VKNewsfeedPost VM
        {
            get { return base.DataContext as VKNewsfeedPost; }
        }

        private void _headerTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            NavigatorImpl.Instance.NavigateToProfilePage(this.VM.OwnerId);
        }

        private void action_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this.GenerateMenuItems_News(sender);
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

        public void GenerateMenuItems_News(object sender)
        {
            PopUP2 menuItemList = new PopUP2();

            //if (this.VM.post_type != VKNewsfeedPostType.reply )
            //{
            PopUP2.PopUpItem menuItem3 = new PopUP2.PopUpItem();
            menuItem3.Text = LocalizedStrings.GetString("CopyLink");
            menuItem3.Command = new DelegateCommand((args) => { this.CopyLinkMI_OnClick(sender, null); });//Click += new RoutedEventHandler(this.CopyLinkMI_OnClick);
            menuItemList.Items.Add(menuItem3);
            //}

            if (this.VM.can_delete)
            {
                PopUP2.PopUpItem menuItem2 = new PopUP2.PopUpItem();
                menuItem2.Text = "Delete";
                menuItem2.Command = new DelegateCommand((args) => { this._deleteMenuItem_Click(); });
                menuItemList.Items.Add(menuItem2);
            }

            //if (this._IgnoreNewsfeedItemCallback != null && this.GetIgnoreItemData() != null)
            if (this.VM.IgnoreNewsfeedItemCallback != null)
            {
                PopUP2.PopUpItem menuItem1 = new PopUP2.PopUpItem();
                menuItem1.Text = LocalizedStrings.GetString("HideThisPost");//Это не интересно
                menuItem1.Command = new DelegateCommand((args) => { this.VM.IgnoreNewsfeedItemCallback(); });
                menuItemList.Items.Add(menuItem1);
            }
            if (/*!this.VM.CanDelete &&*/ this.VM.HideSourceItemsCallback != null)
            {
                PopUP2.PopUpItem menuItem1 = new PopUP2.PopUpItem();
                menuItem1.Text = LocalizedStrings.GetString("HideFromNews");
                menuItem1.Command = new DelegateCommand((args) => { this.VM.HideSourceItemsCallback(); });
                menuItemList.Items.Add(menuItem1);
            }
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

            menuItemList.ShowAt(sender as FrameworkElement);
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
            int ownerId = this.VM.OwnerId;
            uint postId = this.VM.PostId;
            SharePostUC share = new SharePostUC("новостью", WallService.RepostObject.wall, ownerId, postId);
            PopUpService popUp = new PopUpService { Child = share, OverrideBackKey = true, AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed };
            share.Done = popUp.Hide;
            popUp.Show();
            e.Handled = true;
        }
        
        private void Comments_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VKNewsfeedPost vm = (sender as FrameworkElement).DataContext as VKNewsfeedPost;
 //           VKWallPost vm2 = (sender as FrameworkElement).DataContext as VKWallPost;

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
            /*
            else if(vm2!=null)
            {
                switch(vm2.post_type)
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
            */
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
            VKNewsfeedPost vmNewsPost = this.VM;
            if(vmNewsPost!=null)
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
                //
                Debug.Assert(vmNewsPost.likes!=null);
                //
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
            /*
            VKWallPost vmWallPost = (sender as FrameworkElement).DataContext as VKWallPost;
            if(vmWallPost!=null)
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
            */
            
        }

        private void Signer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToProfilePage(this.VM.signer_id);
        }

        private void Copyrights_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToWebUri(this.VM.copyright.link);
        }

        private void _deleteMenuItem_Click()
        {
            //this.InitiateDelete();
            //if (!this._wallPost.AskConfirmationAndDelete() || this._deletedItemCallback == null)
            //    return;
            //this._deletedItemCallback(this);
            this.VM._deletedItemCallback?.Invoke();
        }
    }
}
