using LunaVK.Core.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.Utils
{
    public static class WallPostExtensions
    {
        public static bool CanDelete(this VKWallPost wallPost, List<VKGroup> knownGroups, bool isCanEditCheck = false)
        {
            if (wallPost == null)
                return false;
            VKGroup group = knownGroups == null ? null : Enumerable.FirstOrDefault(knownGroups, (g => g.id == -wallPost.owner_id));
            if (wallPost.owner_id == Settings.UserId || wallPost.from_id == Settings.UserId || group != null && (int)group.admin_level > 1)
                return true;
            if (group != null && group.admin_level > 0 && wallPost.from_id != wallPost.owner_id)
                return !isCanEditCheck;
            return false;
        }

        public static bool CanEdit(this VKWallPost wallPost, List<VKGroup> knownGroups)
        {
            if (wallPost == null || wallPost.owner_id == Settings.UserId && wallPost.from_id != Settings.UserId)
                return false;
            bool flag = wallPost.CanDelete(knownGroups, true) && ((DateTime.Now - wallPost.date).TotalHours < 24.0 || wallPost.IsSuggestedPostponed);
            if (wallPost.IsSuggested)
                flag = wallPost.from_id == Settings.UserId;
            return flag;
        }

        public static bool CanPin(this VKWallPost wallPost, List<VKGroup> knownGroups)
        {
            if (wallPost == null)
                return false;
            long loggedInUserId = Settings.UserId;
            if (wallPost.owner_id == loggedInUserId && wallPost.from_id == loggedInUserId && wallPost.is_pinned == false)
                return true;
            VKGroup group = knownGroups == null ? null : Enumerable.FirstOrDefault(knownGroups, (g => g.id == -wallPost.owner_id));
            return wallPost.CanDelete(knownGroups, false) && group != null && (wallPost.is_pinned == false && wallPost.from_id == wallPost.owner_id);
        }

        public static bool CanUnpin(this VKWallPost wallPost, List<VKGroup> knownGroups)
        {
            if (wallPost == null)
                return false;
            long loggedInUserId = Settings.UserId;
            if (wallPost.owner_id == loggedInUserId && wallPost.from_id == loggedInUserId && wallPost.is_pinned == true)
                return true;
            VKGroup group = knownGroups == null ? null : Enumerable.FirstOrDefault(knownGroups, (g => g.id == -wallPost.owner_id));
            return wallPost.CanDelete(knownGroups, false) && group != null && (wallPost.is_pinned == true && wallPost.from_id == wallPost.owner_id);
        }

        public static bool CanReport(this VKWallPost wallPost)
        {
            return wallPost != null && wallPost.from_id != Settings.UserId && !wallPost.IsPostponed;
        }
        /*
                public static void NavigateToEditWallPost(this VKWallPost wallPost, int adminLevel)
                {
                    if (wallPost == null)
                        return;
                    ParametersRepository.SetParameterForId("EditWallPost", wallPost);
                    Navigator.Current.NavigateToNewWallPost(Math.Abs(wallPost.owner_id), wallPost.owner_id < 0, adminLevel, false, false, false);
                }

                public static void NavigateToPublishWallPost(this VKWallPost wallPost, int adminLevel)
                {
                    if (wallPost == null)
                        return;
                    ParametersRepository.SetParameterForId("PublishWallPost", wallPost);
                    Navigator.Current.NavigateToNewWallPost(Math.Abs(wallPost.owner_id), wallPost.owner_id < 0, adminLevel, false, false, false);
                }

                public static bool AskConfirmationAndDelete(this VKWallPost wallPost)
                {
                    if (wallPost == null || MessageBox.Show(CommonResources.Conversation_ConfirmDeletion, CommonResources.DeleteWallPost, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                        return false;
                    WallService.Current.DeletePost(wallPost.owner_id, wallPost.id);
                    EventAggregator.Current.Publish(new WallPostDeleted()
                    {
                        VKWallPost = wallPost
                    });
                    SocialDataManager.Instance.MarkFeedAsStale(wallPost.owner_id);
                    return true;
                }
        */
        public static bool CanRepostToCommunity(this VKWallPost wallPost)
        {
            if (wallPost == null)
                return false;
            if (wallPost.likes != null && wallPost.likes.can_publish == true || wallPost.reposts.user_reposted == true)
                return true;
            if (wallPost.friends_only == false && wallPost.from_id == Settings.UserId)
                return wallPost.owner_id == Settings.UserId;
            return false;
        }

        public static void PinUnpin(this VKWallPost wallPost, Action<bool> callback)
        {/*
            WallService.Current.PinUnpin(wallPost.is_pinned == 0, wallPost.owner_id, wallPost.id, (res =>
            {
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    EventAggregator current = EventAggregator.Current;
                    WallPostPinnedUnpinned postPinnedUnpinned = new WallPostPinnedUnpinned();
                    postPinnedUnpinned.OwnerId = wallPost.owner_id;
                    postPinnedUnpinned.PostId = wallPost.id;
                    int num = wallPost.is_pinned == 0 ? 1 : 0;
                    postPinnedUnpinned.Pinned = num != 0;
                    current.Publish(postPinnedUnpinned);
                    wallPost.is_pinned = wallPost.is_pinned != 0 ? 0 : 1;
                    SocialDataManager.Instance.MarkFeedAsStale(wallPost.owner_id);
                }
                callback(res.ResultCode == ResultCode.Succeeded);
            }));*/
        }






        public static bool IsRepost(this VKWallPost wallPost)
        {
            if (wallPost.copy_history != null)
                return wallPost.copy_history.Count > 0;
            return false;
        }

        public static bool CanGoToOriginal(this VKWallPost wallPost)
        {
            if (!wallPost.IsRepost())
                return false;
            if (!(wallPost.copy_history[0].post_type == Enums.VKNewsfeedPostType.post))
                return wallPost.copy_history[0].post_type == Enums.VKNewsfeedPostType.reply;
            return true;
        }

        public static bool AskConfirmationAndDelete(this VKWallPost wallPost)
        {/*
            if (wallPost == null || MessageBox.Show(CommonResources.Conversation_ConfirmDeletion, CommonResources.DeleteWallPost, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                return false;
            WallService.Current.DeletePost(wallPost.to_id, wallPost.id);
            EventAggregator.Current.Publish(new WallPostDeleted()
            {
                WallPost = wallPost
            });
            SocialDataManager.Instance.MarkFeedAsStale(wallPost.to_id);
            return true;*/
            return false;
        }
    }
}
