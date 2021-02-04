using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public class PodcastsViewModel : GenericCollectionViewModel<VKPodcast>
    {
        public readonly int _ownerId;

        public PodcastsViewModel(int ownerId)
        {
            this._ownerId = ownerId;
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKPodcast>> callback)
        {
            PodcastsService.Instance.GetPodcasts(offset, count, this._ownerId, (result) =>
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
                if (base._totalCount == 0)
                    return "Пусто";
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OnePodcastFrm", "TwoFourPodcastsFrm", "FiveOrMorePodcastsFrm");
            }
        }
    }
}
