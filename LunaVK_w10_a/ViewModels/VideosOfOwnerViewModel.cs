using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.ViewModels
{
    public class VideosOfOwnerViewModel : GenericCollectionViewModel<VKVideoBase>
    {
        private int _ownerId;
        private int _albumId;

        public VideosOfOwnerViewModel(int owner_id, int album_id)
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
                if (base._totalCount <= 0)
                    return LocalizedStrings.GetString("NoVideos");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneVideoFrm", "TwoFourVideosFrm", "FiveOrMoreVideosFrm");
            }
        }

        public void Delete(VKVideoBase item)
        {
            VideoService.Instance.Delete(this._ownerId, item.id, (result) =>
            {
                if (result.error.error_code == VKErrors.None && result.response == 1)
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
