using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.ViewModels
{
    public class VideoCatalogViewModel2
    {
        public VideoAlbumsListViewModel AllVideosVM { get; private set; }

        public Core.ViewModels.VideoCatalogViewModel.GenericCollectionCatalog CategoriesVM { get; private set; }

        public VideoLivesListViewModel LivesVM { get; private set; }

        public VideoCatalogViewModel2()
        {
            this.AllVideosVM = new VideoAlbumsListViewModel((int)Settings.UserId);
            this.CategoriesVM = new Core.ViewModels.VideoCatalogViewModel.GenericCollectionCatalog();
            this.LivesVM = new VideoLivesListViewModel();
        }

        public class VideoLivesListViewModel : GenericCollectionViewModel<VKVideoBase>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKVideoBase>> callback)
            {
                VideoService.Instance.GetRecommendedLiveVideos((result) =>
                {
                    if (result.error.error_code == Core.Enums.VKErrors.None)
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
        }
    }
}
