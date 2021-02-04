using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using LunaVK.Library;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;

namespace LunaVK.ViewModels
{
    public class ProfileMediaViewModelFacade : ViewModelBase
    {
        private VKBaseDataForGroupOrUser _profileData;

        /// <summary>
        /// Это секция из однотипных элементов, например, "фотографии"
        /// </summary>
        public IMediaHorizontalItemsViewModel MediaHorizontalItemsViewModel { get; private set; }

        /// <summary>
        /// Это секция из разных элементов с заголовком вида "Подписки 41", "Документы 2"
        /// </summary>
        public IMediaVerticalItemsViewModel MediaVerticalItemsViewModel { get; private set; }

        /// <summary>
        /// Секции вида "1 видео", "200 фотографий"
        /// </summary>
        public ObservableCollection<MediaListSectionViewModel> MediaSectionsViewModel { get; private set; }

        private bool _canDisplayHorizontalItems;
        private bool _canDisplayVerticalItems;

        private VKUser UserData { get { return this._profileData as VKUser; } }
        private VKGroup GroupData { get { return this._profileData as VKGroup; } }

        private VKGroupMainSection _mainSectionType;

        public ProfileMediaViewModelFacade()
        {
            this.MediaSectionsViewModel = new ObservableCollection<MediaListSectionViewModel>();
        }

        public Visibility MediaSectionsVisibility
        {
            get
            {
                return (this.MediaSectionsViewModel.Count>0).ToVisiblity();
            }
        }

        public Visibility MediaHorizontalItemsVisibility
        {
            get
            {
                return (this.MediaHorizontalItemsViewModel != null && !string.IsNullOrEmpty( this.MediaHorizontalItemsViewModel.Count)).ToVisiblity();
            }
        }

        public Visibility MediaVerticalItemsVisibility
        {
            get
            {
                return (this.MediaVerticalItemsViewModel != null).ToVisiblity();
            }
        }

        public void PreInit(VKBaseDataForGroupOrUser profileData)
        {
            if (this._profileData == null || profileData.MainSectionType != this._profileData.MainSectionType)
            {
                this._canDisplayHorizontalItems = false;
                this._canDisplayVerticalItems = false;
                if (profileData.MainSectionType == VKGroupMainSection.None)
                {
                    this._canDisplayHorizontalItems = false;
                    this._canDisplayVerticalItems = false;
                }
                else
                {
                    switch (profileData.MainSectionType)
                    {
                        case VKGroupMainSection.Photos:
                            this.MediaHorizontalItemsViewModel = new ProfilePhotosViewModel(profileData.Id);
                            this._canDisplayHorizontalItems = true;
                            break;
                        case VKGroupMainSection.Discussions:
                            //this.MediaVerticalItemsViewModel = (IMediaVerticalItemsViewModel)new ProfileDiscussionsViewModel();
                            break;
                        case VKGroupMainSection.Audios:
                            //this.MediaVerticalItemsViewModel = (IMediaVerticalItemsViewModel)new ProfileAudiosViewModel();
                            break;
                        case VKGroupMainSection.Videos:
                            //this.MediaHorizontalItemsViewModel = (IMediaHorizontalItemsViewModel)new ProfileVideosViewModel();
                            this._canDisplayHorizontalItems = true;
                            break;
                        case VKGroupMainSection.Market:
                            this.MediaHorizontalItemsViewModel = new ProfileMarketViewModel(profileData.Id);
                            this._canDisplayHorizontalItems = true;
                            break;
                    }
                    this._canDisplayVerticalItems = !this._canDisplayHorizontalItems;
                }
            }
            this._profileData = profileData;
            /*
            if (this._canDisplayHorizontalItems)
            {
                base.NotifyPropertyChanged(nameof(this.MediaHorizontalItemsViewModel));
                this.MediaHorizontalItemsViewModel.Init(this._profileData);
            }
            else
            {
                base.NotifyPropertyChanged(nameof(this.MediaVerticalItemsViewModel));
                this.MediaVerticalItemsViewModel.Init(this._profileData);
            }
            
            base.NotifyPropertyChanged(nameof(this.MediaHorizontalItemsVisibility));
            base.NotifyPropertyChanged(nameof(this.MediaVerticalItemsVisibility));
            */
            this.ProfileMediaSectionsViewModel_Init(this._profileData, this._profileData.MainSectionType);//this._mediaSectionsViewModel.Init(this._profileData, this._profileData.MainSectionType);
            base.NotifyPropertyChanged(nameof(this.MediaSectionsVisibility));
            //base.NotifyPropertyChanged(() => this.CanDisplay);
        }

        public void Init(object data)
        {
            //
            if (this.MediaHorizontalItemsViewModel == null || data == null)
            {
                //this.MediaHorizontalItemsViewModel = null;//BugFix: при возврате назад
                return;
            }
            //
            if (this._canDisplayHorizontalItems)
            {
                base.NotifyPropertyChanged(nameof(this.MediaHorizontalItemsViewModel));
                this.MediaHorizontalItemsViewModel.Init(data);//this.MediaHorizontalItemsViewModel.Init(this._profileData);
            }
            else
            {
                base.NotifyPropertyChanged(nameof(this.MediaVerticalItemsViewModel));
                //this.MediaVerticalItemsViewModel.Init(this._profileData);
            }

            base.NotifyPropertyChanged(nameof(this.MediaHorizontalItemsVisibility));
            base.NotifyPropertyChanged(nameof(this.MediaVerticalItemsVisibility));
        }


        private void ProfileMediaSectionsViewModel_Init(VKBaseDataForGroupOrUser profileData, VKGroupMainSection mainSectionType)
        {
            this.MediaSectionsViewModel.Clear();
            this._mainSectionType = mainSectionType;
            var counters = profileData.Counters;


#region Counters
            if (counters != null)
            {
                int profileId = this._profileData.Id;
                //if (this.UserData == null)
                //    profileId = (-profileId);
                if (counters.followers > 0)
                {
                    string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(counters.followers, "OneFollowerFrm", "TwoFourFollowersFrm", "FiveFollowersFrm", false);
                    this.MediaSectionsViewModel.Add(new MediaListSectionViewModel(temp_str, counters.followers, ProfileMediaListItemType.Generic));
                }

                if (counters.podcasts > 0)
                {
                    string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(counters.podcasts, "OnePodcastFrm", "TwoFourPodcastsFrm", "FiveOrMorePodcastsFrm", false);
                    this.MediaSectionsViewModel.Add(new MediaListSectionViewModel(temp_str, counters.podcasts, ProfileMediaListItemType.Generic, null, () => { NavigatorImpl.Instance.NavigateToPodcasts(profileId, this._profileData.Title); }));
                }

                if (counters.articles > 0)
                {
                    string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(counters.articles, "OneArticleFrm", "TwoFourArticlesFrm", "FiveArticlesFrm", false);
                    this.MediaSectionsViewModel.Add(new MediaListSectionViewModel(temp_str, counters.articles, ProfileMediaListItemType.Generic, null, () => { NavigatorImpl.Instance.NavigateToArticles(profileId, this._profileData.Title); }));
                }
                
                if (counters.albums > 0)
                {
                    string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(counters.albums, "OneAlbumFrm", "TwoFourAlbumsFrm", "FiveAlbumsFrm", false);
                    this.MediaSectionsViewModel.Add(new MediaListSectionViewModel(temp_str, counters.albums, ProfileMediaListItemType.Generic,null, () =>
                    {
                        string str = "";
                        if (this.UserData != null)
                            str = this.UserData.first_name_gen;
                        else
                            str = this._profileData.Title;
                        NavigatorImpl.Instance.NavigateToPhotoAlbums(profileId, str);
                    }));
                }

                if (counters.topics > 0)
                {
                    string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(counters.topics, "OneTopicFrm", "TwoFourTopicsFrm", "FiveTopicsFrm", false);
                    this.MediaSectionsViewModel.Add(new MediaListSectionViewModel(temp_str, counters.topics, ProfileMediaListItemType.Generic,null, () => { NavigatorImpl.Instance.NavigateToGroupDiscussions(profileId, this._profileData.Title, this.GroupData.admin_level, this.GroupData.is_closed == VKGroupIsClosed.Opened, this.GroupData.can_create_topic); }));
                }

                if (counters.photos > 0)
                {
                    string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(counters.photos, "AlbumsPhotosCountOneFrm", "AlbumsPhotosCountTwoFrm", "AlbumsPhotosCountFiveFrm", false);
                    this.MediaSectionsViewModel.Add(new MediaListSectionViewModel(temp_str, counters.photos, ProfileMediaListItemType.Generic,null, () =>
                    {
                        string str = "";
                        if (this.UserData != null)
                            str = this.UserData.first_name_gen;
                        else
                            str = this._profileData.Title;
                        //NavigatorImpl.Instance.NavigateToPhotoAlbums(profileId, str);
                        NavigatorImpl.Instance.NavigateToAllPhotos(profileId, str);
                    }));
                }

                if (counters.docs > 0)
                {
                    string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(counters.docs, "OneDocument", "TwoFourDocumentsFrm", "FiveMoreDocumentsFrm", false);
                    this.MediaSectionsViewModel.Add(new MediaListSectionViewModel(temp_str, counters.docs, ProfileMediaListItemType.Generic,null, () => { NavigatorImpl.Instance.NavigateToDocuments(profileId); }));
                }

                if (counters.gifts > 0)
                {
                    string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(counters.gifts, "OneGift", "TwoFourGiftsFrm", "FiveMoreGiftsFrm", false);
                    this.MediaSectionsViewModel.Add(new MediaListSectionViewModel(temp_str, counters.gifts, ProfileMediaListItemType.Generic, null, ()=>
                    {
                        string str = "";
                        if (this.UserData != null)
                            str = this.UserData.first_name_gen;
                        
                        NavigatorImpl.Instance.NavigateToGifts((uint)profileId, this._profileData.Title, str);
                    }));
                }
                if (counters.audios > 0)
                {
                    string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(counters.audios, "OneAudio", "TwoFourAudioFrm", "FiveOrMoreAudioFrm", false);
                    this.MediaSectionsViewModel.Add(new MediaListSectionViewModel(temp_str, counters.audios, ProfileMediaListItemType.Generic, null, () =>
                    {
                        string str = "";
                        if (this.UserData != null)
                            str = this.UserData.first_name_gen;
                        else
                            str = this._profileData.Title;
                        NavigatorImpl.Instance.NavigateToAudio(profileId,str);

                    }));
                }
                if (counters.groups > 0)
                {
                    string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(counters.groups, "OneGroup", "TwoFourGroupsFrm", "FiveMoreGroupsFrm", false);
                    this.MediaSectionsViewModel.Add(new MediaListSectionViewModel(temp_str, counters.groups, ProfileMediaListItemType.Generic, null, () => { NavigatorImpl.Instance.NavigateToSubscriptions((uint)profileId); }));
                }
                if (counters.pages > 0)
                {
                    string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(counters.pages, "OnePageFrm", "TwoFourPageFrm", "FiveMorePageFrm", false);
                    this.MediaSectionsViewModel.Add(new MediaListSectionViewModel(temp_str, counters.pages, ProfileMediaListItemType.Generic, null, () => { NavigatorImpl.Instance.NavigateToSubscriptions((uint)profileId); }));

                    //int titleCounter = counters.pages + counters.groups + counters.subscriptions;
                    //this.MediaItems.Add(new MediaListSectionViewModel("Profile_Subscriptions", titleCounter, ProfileMediaListItemType.Subscriptions, null, null));
                }
                if (counters.subscriptions > 0)
                {
                    //string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(counters.pages, "OnePageFrm", "TwoFourPageFrm", "FiveMorePageFrm", false);
                    this.MediaSectionsViewModel.Add(new MediaListSectionViewModel("подписки", counters.subscriptions, ProfileMediaListItemType.Generic, null, () => { NavigatorImpl.Instance.NavigateToSubscriptions((uint)profileId); }));
                }
                
                if (counters.videos > 0)
                {
                    string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(counters.videos, "OneVideoFrm", "TwoFourVideosFrm", "FiveOrMoreVideosFrm", false);
                    this.MediaSectionsViewModel.Add(new MediaListSectionViewModel(temp_str, counters.videos, ProfileMediaListItemType.Generic, null, () =>
                    {
                        string str = "";
                        if (this.UserData != null)
                            str = this.UserData.first_name_gen;
                        else
                            str = this._profileData.Title;
                        NavigatorImpl.Instance.NavigateToVideos(profileId, str);
                    }));
                }

                if (counters.user_photos > 0)
                {
                    string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(counters.user_photos, "OneMark", "TwoFourMarksFrm", "FiveMoreMarksFrm", false);
                    this.MediaSectionsViewModel.Add(new MediaListSectionViewModel(temp_str, counters.user_photos, ProfileMediaListItemType.Generic, null));
                }

                if(counters.clips > 0)
                {
                    string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(counters.clips, "OneClip", "TwoFourClipsFrm", "FiveMoreClipsFrm", false);
                    this.MediaSectionsViewModel.Add(new MediaListSectionViewModel(temp_str, counters.clips, ProfileMediaListItemType.Generic, null));
                }

                if (counters.clips_followers > 0)
                {
                    //string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(counters.clips_followers, "OneClip", "TwoFourClipsFrm", "FiveMoreClipsFrm", false);
                    this.MediaSectionsViewModel.Add(new MediaListSectionViewModel("clips_followers", counters.clips_followers, ProfileMediaListItemType.Generic, null));
                }
            }

            //this.CountersVisibility = this.MediaSectionsViewModel.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
#endregion
        }

        public interface IMediaHorizontalItemsViewModel
        {
            string Title { get; }

            string Count { get; }

            void HeaderTapAction();

           void ItemTapAction(object item);

            //ObservableCollection<object> Items { get; }

            void Init(object data);//void Init(VKBaseDataForGroupOrUser profileData);

            //void Unload();

            //void Reload();

            //void LoadMoreItems(object linkedItem);
        }

        public interface IMediaVerticalItemsViewModel
        {
            string Title { get; }

            int Count { get; }

            bool CanDisplay { get; }

            //List<MediaListItemViewModelBase> Items { get; }

            bool IsAllItemsVisible { get; }

            Action HeaderTapAction { get; }

            //Action<MediaListItemViewModelBase> ItemTapAction { get; }

            void Init(VKBaseDataForGroupOrUser profileData);
        }
    }
}
