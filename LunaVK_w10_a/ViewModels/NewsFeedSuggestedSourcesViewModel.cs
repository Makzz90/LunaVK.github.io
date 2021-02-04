using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.ViewModels
{
    public class NewsFeedSuggestedSourcesViewModel : GenericCollectionViewModel<VKUserOrGroupSource>
    {
        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUserOrGroupSource>> callback)
        {
            NewsFeedService.Instance.GetSuggestedSources(offset, count, true, (res =>
            {
                if (res.error.error_code == VKErrors.None)
                {
                    base._totalCount = (uint)res.response.items.Count;
                    callback(res.error, res.response.items);
                }
                else
                {
                    callback(res.error, null);
                }
            }));
        }
    }
}
