using System;
using System.Collections.Generic;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Core.Framework;

namespace LunaVK.Core.ViewModels
{
    /// <summary>
    /// Альбомы
    /// </summary>
    public class PhotoAlbumViewModel : GenericCollectionViewModel<VKAlbumPhoto>
    {
        public readonly int _ownerId;
        public string _ownerName;
        public bool IsFooterHidden;

        public PhotoAlbumViewModel(int owner_id)
        {
            this._ownerId = owner_id;
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKAlbumPhoto>> callback)
        {
            string code = "var albums = API.photos.getAlbums({count:"+ count+", need_covers:1,photo_sizes:1,need_system:1";
            if (this._ownerId != Settings.UserId)
            {
                code+=(",owner_id:"+ this._ownerId);
            }

            if (base.Items.Count>0)
                code += (",offset:" + base.Items.Count);
            code+="});";

            code+="var count = API.photos.getAlbumsCount({";
            if(this._ownerId>0)
                code+=("user_id:"+this._ownerId);
            else
                code+=("group_id:"+(-this._ownerId));
            code += "});return {items:albums.items,count:count};";

            VKRequestsDispatcher.Execute<VKCountedItemsObject<VKAlbumPhoto>>(code,(result)=>
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

        public void DeleteAlbum(VKAlbumPhoto album)
        {
            PhotosService.Instance.DeleteAlbum(album.id, (result) =>
            {
                if(result.error.error_code== VKErrors.None && result.response==1)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        base.Items.Remove(album);
                    });
                }
            });
        }

        public void UpdateAlbumsCount()
        {
            base.NotifyPropertyChanged(nameof(base.FooterText));
        }
    }
}
