using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using LunaVK.Library;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public class SuggestedPostponedPostsViewModel : GenericCollectionViewModel<VKWallPost>
    {
        private readonly int _ownerId;
        private int _suggestedPostsCount;
        private int _postponedPostsCount;
        private bool _mode;

        public SuggestedPostponedPostsViewModel(int ownerId, int suggestedPostsCount, int postponedPostsCount)
        {
            this._ownerId = ownerId;
            this._suggestedPostsCount = suggestedPostsCount;
            this._postponedPostsCount = postponedPostsCount;
        }

        public SuggestedPostponedPostsViewModel(int ownerId, int mode)
        {
            this._ownerId = ownerId;
            this._mode = mode > 0;
        }

        public string SuggestedPostsStr
        {
            get { return UIStringFormatterHelper.FormatNumberOfSomething(this._suggestedPostsCount, "SuggestedNews_OneSuggestedPostFrm", "SuggestedNews_TwoFourSuggestedPostsFrm", "SuggestedNews_FiveSuggestedPostsFrm"); }
        }

        public string PostponedPostsStr
        {
            get { return UIStringFormatterHelper.FormatNumberOfSomething(this._postponedPostsCount, "PostponedNews_OnePostponedPostFrm", "PostponedNews_TwoFourPostponedPostsFrm", "PostponedNews_FivePostponedPostsFrm"); }
        }

        public Visibility SuggestedVisibility
        {
            get
            {
                if (this._suggestedPostsCount <= 0)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility PostponedVisibility
        {
            get
            {
                if (this._postponedPostsCount <= 0)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility SeparatorVisibility
        {
            get
            {
                //if (this.SuggestedVisibility != Visibility.Visible || this.PostponedVisibility != Visibility.Visible)
                //    return Visibility.Collapsed;
                //return Visibility.Visible;

                if (this.PostponedVisibility == Visibility.Visible && this.SuggestedVisibility == Visibility.Collapsed)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility SuggestedPostponedVisibility
        {
            get
            {
                //if (this.IsAllDataBlockerStatus || !this.CanShowSuggestedPostponed || (this._suggestedPostponedViewModel == null || !this._suggestedPostponedViewModel.CanDisplay))
                //    return Visibility.Collapsed;
                if (this._suggestedPostsCount > 0 || this._postponedPostsCount > 0)
                    return Visibility.Visible;

                return Visibility.Collapsed;//vis
            }
        }

        public void OpenSuggestedPostsPage()
        {
            NavigatorImpl.Instance.NavigateToSuggestedPostponedPostsPage(this._ownerId, 0);
        }

        public void OpenPostponedPostsPage()
        {
            NavigatorImpl.Instance.NavigateToSuggestedPostponedPostsPage(this._ownerId, 1);
        }

        private string ModeFilter
        {
            get { return this._mode ? "postponed" : "suggests"; }
        }

        public string Title
        {
            get { return LocalizedStrings.GetString(this._mode ? "PostponedNews_Title" : "SuggestedNews_Title"); }
        }


        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKWallPost>> callback)
        {
            WallService.Instance.GetWall(this._ownerId, offset, count, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    foreach (VKWallPost p in result.response.items)
                    {
                        p.likes = null;
                        p.views = null;
                        p.comments = null;
                        p.IsFooterHiden = true;

                        if(this._mode)//postponed
                        {
                            p.post_type = VKNewsfeedPostType.postpone;
                        }
                        else//suggests
                        {
                            p.post_type = VKNewsfeedPostType.suggest;
                        }
                        
                    }
                    base._totalCount = result.response.count;
                    callback(result.error, result.response.items);
                }
                else
                {
                    callback(result.error, null);
                }
            }, this.ModeFilter);
        }

        public override string GetFooterTextForCount
        {
            get
            {
                if (this._mode)
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("PostponedNews_NoPosts");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "PostponedNews_OnePostponedPostFrm", "PostponedNews_TwoFourPostponedPostsFrm", "PostponedNews_FivePostponedPostsFrm");
                }
                else
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("SuggestedPosts_NoPosts");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "SuggestedNews_OneSuggestedPostFrm", "SuggestedNews_TwoFourSuggestedPostsFrm", "SuggestedNews_FiveSuggestedPostsFrm");
                }
            }
        }

        public void DeletedCallback(VKWallPost item)
        {
            WallService.Instance.DeletePost(item.OwnerId, item.PostId, (result)=>
            {
                if(result.error.error_code == VKErrors.None && result.response==1)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        base.Items.Remove(item);
                        base._totalCount--;
                        base.NotifyPropertyChanged(nameof(base.FooterText));
                    });
                }
            });
        }

        public void PublishCallback(VKWallPost item)
        {
            WallPostViewModel wallPostViewModel = new WallPostViewModel(item, Core.Enums.VKAdminLevel.Admin);
            wallPostViewModel.WMMode = WallPostViewModel.Mode.PublishWallPost;
            wallPostViewModel.IsPublishSuggestedSuppressed = true;
            wallPostViewModel.Publish((result)=>
            {
                if (result.error_code == VKErrors.None )
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        base.Items.Remove(item);
                        base._totalCount--;
                        base.NotifyPropertyChanged(nameof(base.FooterText));
                    });
                }
            });

        }
    }
}
