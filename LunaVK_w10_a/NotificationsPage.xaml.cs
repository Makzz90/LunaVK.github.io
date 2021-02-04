using System;
using LunaVK.Core;
using LunaVK.Core.ViewModels;
using LunaVK.ViewModels;
using Windows.UI.Xaml;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Library;
using System.Collections.Generic;
using Windows.UI.Xaml.Input;

namespace LunaVK
{
    //CommentItem
    //FeedbackPage
    public sealed partial class NotificationsPage : PageBase
    {
        public NotificationsPage()
        {
            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("Menu_Notifications");
        }
        
        private FeedbackViewModel VM
        {
            get { return base.DataContext as FeedbackViewModel; }
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                int selected = (int)pageState["Page"];
                this._pivot.SelectedIndex = selected;

                if (this.VM.FeedbackVM.Items.Count > 0)
                    this._listFeedBack.NeedReload = false;
                if (this.VM.CommentsVM.Items.Count > 0)
                    this._listComments.NeedReload = false;
            }
            else
            {
                base.DataContext = new FeedbackViewModel();
            }
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this.VM;
            pageState["Page"] = this._pivot.SelectedIndex;
        }

        private void _view_Tap(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKNewsfeedPost;
            VKNewsfeedFilters postType = vm.type;
            switch(postType)
            {
                case VKNewsfeedFilters.post:
                    {
                        NavigatorImpl.Instance.NavigateToWallPostComments(vm.source_id, vm.post_id);
                        break;
                    }
                case VKNewsfeedFilters.topic:
                    {
                        NavigatorImpl.Instance.NavigateToGroupDiscussion((uint)Math.Abs(vm.source_id), vm.post_id, vm.text, vm.comments.can_post);
                        break;
                    }
                case VKNewsfeedFilters.photo:
                    {
                        //long aid = this._newsItemData.NewsItem.aid;
                        //Photo photo = new Photo() { pid = this._newsItemData.NewsItem.pid, aid = aid, src_big = this._newsItemData.NewsItem.src_big, src_small = this._newsItemData.NewsItem.src_small, src_xbig = this._newsItemData.NewsItem.src_xbig, src_xxbig = this._newsItemData.NewsItem.src_xxbig, width = this._newsItemData.NewsItem.width, height = this._newsItemData.NewsItem.height, owner_id = this._newsItemData.NewsItem.owner_id };
                        //NavigatorImpl.Instance.NavigateToPhotoWithComments(photo, null, this._newsItemData.NewsItem.owner_id, photo.pid, "", false, false);
                        break;
                    }
                case VKNewsfeedFilters.video:
                    {
                        //VKClient.Common.Backend.DataObjects.Video video = new VKClient.Common.Backend.DataObjects.Video() { vid = this._newsItemData.NewsItem.vid, duration = this._newsItemData.NewsItem.duration, image_big = this._newsItemData.NewsItem.image_big, owner_id = this._newsItemData.NewsItem.owner_id };
                        //NavigatorImpl.Instance.NavigateToVideoWithComments(video, video.owner_id, video.vid, "");
                        break;
                    }
            }
            e.Handled = true;
        }
    }
}
