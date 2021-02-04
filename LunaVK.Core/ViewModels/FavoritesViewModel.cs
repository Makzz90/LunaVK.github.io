using System;
using System.Collections.Generic;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Core.Network;

namespace LunaVK.Core.ViewModels
{
    /*
     * WP vk
     * fave.getPhotos
     * fave.getVideos
     * fave.getPosts
     * API.fave.getUsers
     * fave.getLinks
     * fave.getMarketItems
     * */
    /*
     * Android vk
     * fave.getPages
     * fave.getPhotos
     * fave.getTags
     * execute.getFaveWithPages
     * */
    public class FavoritesViewModel
    {
        //wp: photo,video,post,user,link,product
        //android:user,groups,записи,статьи,limks,podcast,video,сюжеты,product
        public GenericCollectionTags TagsVM { get; private set; }

        public GenericCollectionUsers UsersVM { get; private set; }
        public GenericCollectionGroups GroupsVM { get; private set; }
        public GenericCollectionPosts PostsVM { get; private set; }
        public GenericCollectionArticles ArticlesVM { get; private set; }
        public GenericCollectionLinks LinksVM { get; private set; }
        public GenericCollectionPodcasts PodcastsVM { get; private set; }
        public GenericCollectionVideos VideosVM { get; private set; }
        public GenericCollectionProducts NarrativeVM { get; private set; }
        public GenericCollectionProducts ProductsVM { get; private set; }

        private uint _tagId;
        public uint TagId
        {
            get
            {
                return this._tagId;
            }
            set
            {

            }
        }

        public FavoritesViewModel()
        {
            this.TagsVM = new GenericCollectionTags();

            this.UsersVM = new GenericCollectionUsers();
            this.GroupsVM = new GenericCollectionGroups();
            this.PostsVM = new GenericCollectionPosts();
            this.ArticlesVM = new GenericCollectionArticles();
            this.LinksVM = new GenericCollectionLinks();
            this.PodcastsVM = new GenericCollectionPodcasts();
            this.VideosVM = new GenericCollectionVideos();
            this.NarrativeVM = new GenericCollectionProducts();
            this.ProductsVM = new GenericCollectionProducts();

        }


        public void Remove(VKBaseDataForGroupOrUser page)
        {
            //todo: обновить текст с общим числом узбранных
            if(page is VKUser user)
            {
                FavoritesService.Instance.FaveAddRemoveUser((uint)user.id, false, (result) => {
                    if (result.error.error_code == VKErrors.None && result.response == 1)
                    {
                        Execute.ExecuteOnUIThread(() => {
                            this.UsersVM.Items.Remove(user);
                        });
                    }
                });
            }
            else if (page is VKGroup group)
            {
                FavoritesService.Instance.FaveAddRemoveGroup((uint)group.id, false, (result) => {
                    if (result.error.error_code == VKErrors.None && result.response == 1)
                    {
                        Execute.ExecuteOnUIThread(() => {
                            this.GroupsVM.Items.Remove(group);
                        });
                    }
                });
            }
        }

        public class GenericCollectionTags : GenericCollectionViewModel<FavoritesService.FaveTag>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<FavoritesService.FaveTag>> callback)
            {
                FavoritesService.Instance.GetFaveTags((result) => {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.count;

                        //
                        result.response.items.Insert(0, new FavoritesService.FaveTag() { id = 0, name = "Не выбрано" });
                        //
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }
            /*
            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("NoVideos");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneVideoFrm", "TwoFourVideosFrm", "FiveOrMoreVideosFrm");
                }
            }
            */
        }

        public class GenericCollectionUsers : GenericCollectionViewModel<VKUser>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
            {
                FavoritesService.Instance.GetFaveUsers(offset, count, (result) => {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("NoPersons");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OnePersonFrm", "TwoFourPersonsFrm", "FivePersonsFrm");
                }
            }
        }

        public class GenericCollectionGroups : GenericCollectionViewModel<VKGroup>
        {
            /*
            public int itemsLoaded = 0;

            public override bool HasMoreDownItems
            {
                get { return base.Items.Count == 0 || this.itemsLoaded < base._totalCount; }
            }
            */
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKGroup>> callback)
            {
                FavoritesService.Instance.GetFaveGroups(offset, count, 0, (result) => {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount == 0)
                        return LocalizedStrings.GetString("NoPages");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneGroup", "TwoFourGroupsFrm", "FiveMoreGroupsFrm");
                }
            }
        }

        public class GenericCollectionVideos : GenericCollectionViewModel<VKVideoBase>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKVideoBase>> callback)
            {
                FavoritesService.Instance.GetFaveVideos(offset, count, (result) => {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("NoVideos");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneVideoFrm", "TwoFourVideosFrm", "FiveOrMoreVideosFrm");
                }
            }
        }

        public class GenericCollectionPosts : GenericCollectionViewModel<VKWallPost>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKWallPost>> callback)
            {
                FavoritesService.Instance.GetFavePosts(offset, count, (result) => {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("NoWallPosts");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneWallPostFrm", "TwoWallPostsFrm", "FiveWallPostsFrm");
                }
            }
        }

        

        

        public class GenericCollectionLinks : GenericCollectionViewModel<VKLink>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKLink>> callback)
            {
                FavoritesService.Instance.GetFaveLinks(offset, count, (result) => {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("NoLinks");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneLinkFrm", "TwoFourLinksFrm", "FiveLinksFrm");
                }
            }
        }

        public class GenericCollectionProducts : GenericCollectionViewModel<VKMarketItem>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKMarketItem>> callback)
            {
                FavoritesService.Instance.GetFaveProducts(offset, count, (result) => {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("NoProducts");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneProductItemFrm", "TwoFourProductItemsFrm", "FiveProductItemsFrm");
                }
            }
        }

        public class GenericCollectionPodcasts : GenericCollectionViewModel<VKPodcast>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKPodcast>> callback)
            {
                FavoritesService.Instance.GetFavePodcasts(offset, count, (result) => {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }
            /*
            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("NoProducts");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneProductFrm", "TwoFourProductsFrm", "FiveProductsFrm");
                }
            }
            */
        }

        public class GenericCollectionArticles : GenericCollectionViewModel<VKArticle>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKArticle>> callback)
            {
                FavoritesService.Instance.GetFaveArticle(offset, count, (result) => {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }
            /*
            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("NoProducts");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneProductFrm", "TwoFourProductsFrm", "FiveProductsFrm");
                }
            }
            */
        }

        
    }
}
