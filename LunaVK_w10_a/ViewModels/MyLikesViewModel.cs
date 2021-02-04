using System;
using System.Collections.Generic;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Core;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public class MyLikesViewModel
    {
        public GenericCollectionPhotos PhotosVM { get; private set; }

        public GenericCollectionVideos VideosVM { get; private set; }

        public GenericCollectionPosts PostsVM { get; private set; }

        public GenericCollectionProducts ProductsVM { get; private set; }

        public MyLikesViewModel()
        {
            this.PhotosVM = new GenericCollectionPhotos();
            this.VideosVM = new GenericCollectionVideos();
            this.PostsVM = new GenericCollectionPosts();
            this.ProductsVM = new GenericCollectionProducts();
        }

        public class GenericCollectionPhotos : GenericCollectionViewModel<VKPhoto>
        {
            public GenericCollectionPhotos()
            {
                base.ReloadCount = 60;
                base.LoadCount = 50;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKPhoto>> callback)
            {
                FavoritesService.Instance.GetFavePhotos(offset, count, (result) => {
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
                        return LocalizedStrings.GetString("NoPhotos");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OnePhotoFrm", "TwoFourPhotosFrm", "FivePhotosFrm");
                }
            }
        }

        public class GenericCollectionVideos : GenericCollectionViewModel<VKVideoBase>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKVideoBase>> callback)
            {
                FavoritesService.Instance.GetLikedVideos(offset, count, (result) => {
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
                FavoritesService.Instance.GetLikedPosts(offset, count, (result) => {
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


        public class GenericCollectionProducts : GenericCollectionViewModel<VKMarketItem>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKMarketItem>> callback)
            {
                FavoritesService.Instance.GetLikedProducts(offset, count, (result) => {
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
    }
}
