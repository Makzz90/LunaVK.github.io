using System;
using System.Collections.Generic;
using LunaVK.Core.DataObjects;

using LunaVK.Core.Json;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Core.Framework;
using LunaVK.Core.Network;

namespace LunaVK.Core.ViewModels
{
    public class VideoCatalogViewModel
    {
        public GenericCollectionVideos AllVideosVM { get; private set; }

        public GenericCollectionVideos UploadedVideosVM { get; private set; }

        public GenericCollectionAlbums AlbumsVM { get; private set; }

        public GenericCollectionCatalog CategoriesVM { get; private set; }

        public VideoCatalogViewModel()
        {
            this.AllVideosVM = new GenericCollectionVideos(0);
            this.UploadedVideosVM = new GenericCollectionVideos(VKVideoAlbum.UPLOADED_ALBUM_ID);
            this.AlbumsVM = new GenericCollectionAlbums();

            this.CategoriesVM = new GenericCollectionCatalog();
        }

        

        

        public void AddRemoveToMyVideos(VKVideoBase video)
        {
            VideoService.Instance.AddRemovedToFromAlbum(false, (int)Settings.UserId, VKVideoAlbum.ADDED_ALBUM_ID, video.owner_id, video.id, (result) => {
                if(result.error.error_code == VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(()=> {
                        this.AllVideosVM.Items.Remove(video);
                    });
                }
            });
        }












        public class GenericCollectionVideos : GenericCollectionViewModel<VKVideoBase>
        {
            private int _albumId = 0;

            public GenericCollectionVideos(int albumId)
            {
                this._albumId = albumId;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKVideoBase>> callback)
            {
                VideoService.Instance.GetVideos(0, offset, count, (result) => {
                    if(result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                }, this._albumId);
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

        public class GenericCollectionCatalog : GenericCollectionViewModel<VideoService.VideoCatalogCategory>
        {
            public GenericCollectionCatalog()
            {
                base.LoadCount = 15;//API больше не вернёт :(
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VideoService.VideoCatalogCategory>> callback)
            {
                if (offset==0)
                {
                    base._nextFrom = "";
                }

                VideoService.Instance.GetVideoCatalog(count, 4, base._nextFrom, (result) =>
                {
                    if(result.error.error_code == VKErrors.None)
                    {
                        base._nextFrom = result.response.next;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
                
            }
        }

        public class GenericCollectionAlbums : GenericCollectionViewModel<VKVideoAlbum>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKVideoAlbum>> callback)
            {
                VideoService.Instance.GetAlbums(0, false, offset, count, (result) => {
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
                        return LocalizedStrings.GetString("NoAlbums");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneAlbumFrm", "TwoFourAlbumsFrm", "FiveAlbumsFrm");
                }
            }
        }














        private string GetGlyph(string type)
        {
            switch (type)
            {
                case "top":
                    {
                        return "\xE735";
                    }
                case "ugc":
                    {
                        return "\xECAD";
                    }
                case "series":
                    {
                        return "\xE714";
                    }
            }
            return "";
        }
    }
}
