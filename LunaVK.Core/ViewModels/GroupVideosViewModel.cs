using System;
using System.Collections.Generic;
using LunaVK.Core.Library;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Utils;
using LunaVK.Core.Network;

namespace LunaVK.Core.ViewModels
{
    public class GroupVideosViewModel
    {
        public GenericCollectionVideos VideosAddedVM { get; private set; }

        public GenericCollectionVideos VideosLoadedVM { get; private set; }

        public GenericCollectionAlbums AlbumsVM { get; private set; }

        public GroupVideosViewModel(int owner_id)
        {
            this.VideosAddedVM = new GenericCollectionVideos(owner_id, VKVideoAlbum.ADDED_ALBUM_ID);
            this.VideosLoadedVM = new GenericCollectionVideos(owner_id, VKVideoAlbum.UPLOADED_ALBUM_ID);
            this.AlbumsVM = new GenericCollectionAlbums(owner_id);
        }



        

        public class GenericCollectionVideos : GenericCollectionViewModel<VKVideoBase>
        {
            private int _ownerId;
            private int _albumId;

            public GenericCollectionVideos(int owner_id, int album_id = 0)
            {
                this._ownerId = owner_id;
                this._albumId = album_id;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKVideoBase>> callback)
            {

                VideoService.Instance.GetVideos(this._ownerId, offset, count, (result) =>
                {
                    if (result.error.error_code == VKErrors.None)
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
                    if (base._totalCount == 0)
                        return LocalizedStrings.GetString("NoVideos");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneVideoFrm", "TwoFourVideosFrm", "FiveOrMoreVideosFrm");
                }
            }

            
        }

        public class GenericCollectionAlbums : GenericCollectionViewModel<VKVideoAlbum>
        {
            private int _ownerId;
            public bool IsFooterHidden;

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKVideoAlbum>> callback)
            {
                VideoService.Instance.GetAlbums(this._ownerId, false, offset, count, (result) => {
                    if(result.error.error_code == VKErrors.None)
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
                    if (this.IsFooterHidden)
                        return "";

                    if (base._totalCount == 0)
                        return LocalizedStrings.GetString("NoAlbums");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneAlbumFrm", "TwoFourAlbumsFrm", "FiveAlbumsFrm");
                }
            }

            public GenericCollectionAlbums(int owner_id)
            {
                this._ownerId = owner_id;
            }
        }
    }
}
