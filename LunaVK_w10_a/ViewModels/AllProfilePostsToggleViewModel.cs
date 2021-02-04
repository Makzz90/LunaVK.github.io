using LunaVK.Core;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using LunaVK.Core.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using LunaVK.Library;
using LunaVK.Core.Enums;

namespace LunaVK.ViewModels
{
    public class AllProfilePostsToggleViewModel : ViewModelBase
    {
        private readonly bool _hidePostsUnderline;
        private readonly VKBaseDataForGroupOrUser _profileData;
        private bool _isAllPosts = true;

        public string AllPostsText { get; private set; }
        
        public Visibility AllPostsUnderlineVisibility
        {
            get { return (this._isAllPosts && !this._hidePostsUnderline).ToVisiblity(); }
        }

        private ProfileLoadingStatus _status;

        public void UpdateState(ProfileLoadingStatus loadingStatus)
        {
            this._status = loadingStatus;
            base.NotifyPropertyChanged(nameof(this.PostsVisibility));
        }

        public Visibility PostsVisibility
        {
            get
            {
                if (this._wallPostsCount == 0)
                    return Visibility.Collapsed;

                switch (this._status)
                {
                    case ProfileLoadingStatus.Deleted:
                    case ProfileLoadingStatus.Banned:
                    case ProfileLoadingStatus.Blacklisted:
                    case ProfileLoadingStatus.Private:
                    case ProfileLoadingStatus.Service:
                        return Visibility.Collapsed;
                    default:
                        return Visibility.Visible;
                }
            }
        }


        public Visibility ProfilePostsUnderlineVisibility
        {
            get
            {
                return (!this._isAllPosts).ToVisiblity();
            }
        }

        public string ProfilePostsText { get; set; }

        public bool IsAllPosts
        {
            get
            {
                return this._isAllPosts;
            }
            set
            {
                if (this._isAllPosts == value)
                    return;
                this._isAllPosts = value;
                //this.NotifyPropertyChanged<Brush>((System.Linq.Expressions.Expression<Func<Brush>>)(() => this.AllPostsForeground));
                this.NotifyPropertyChanged(nameof(this.AllPostsUnderlineVisibility));
                //this.NotifyPropertyChanged<Brush>((System.Linq.Expressions.Expression<Func<Brush>>)(() => this.ProfilePostsForeground));
                this.NotifyPropertyChanged(nameof(this.ProfilePostsUnderlineVisibility));
                
                if (this.StateChangedCallback == null)
                    return;
                this.StateChangedCallback(value);
            }
        }

        public Visibility PostsToggleVisibility { get; private set; }

        public Visibility PostsCountVisibility { get; private set; }

        public Action<bool> StateChangedCallback { get; set; }

        private string NameGen;

        private int _wallPostsCount;

        public AllProfilePostsToggleViewModel(VKBaseDataForGroupOrUser profileData, int wallPostsCount, string nameGen)
        {
            this.AllPostsText = LocalizedStrings.GetString("Group_AllPosts");
            //
            this._profileData = profileData;
            this.PostsCountVisibility = Visibility.Collapsed;
            this.NameGen = nameGen;
            this._wallPostsCount = wallPostsCount;

            if (this._profileData is VKGroup group)
            {
                if (group != null && (group.type == VKGroupType.Page || !group.can_see_all_posts))
                {
                    this._hidePostsUnderline = true;
                    this.UpdateAllPostsText((uint)wallPostsCount);
                    this.PostsCountVisibility = Visibility.Visible;
                    this.PostsToggleVisibility = Visibility.Collapsed;
                }
                else
                    this.ProfilePostsText = LocalizedStrings.GetString("Group_CommunityPosts");
            }
            else
                this.ProfilePostsText = Settings.UserId == this._profileData.Id ? LocalizedStrings.GetString("User_MyPosts") : string.Format(LocalizedStrings.GetString("User_ProfilePostsFrm"), nameGen);
        }

        public void NavigateToSearch()
        {
            NavigatorImpl.Instance.NavigateToPostsSearch(this._profileData.Id, this.NameGen);
        }

        public void UpdateAllPostsText(uint count)
        {
            this._wallPostsCount = (int)count;
            if (this._wallPostsCount == 0)
                this.AllPostsText = LocalizedStrings.GetString("NoWallPosts");
            else
                this.AllPostsText = UIStringFormatterHelper.FormatNumberOfSomething(this._wallPostsCount, "OneWallPostFrm", "TwoWallPostsFrm", "FiveWallPostsFrm");
            base.NotifyPropertyChanged(nameof(this.AllPostsText));
            base.NotifyPropertyChanged(nameof(this.PostsVisibility));
        }
    }
}

