using System;
using System.Collections.Generic;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Library;
using LunaVK.Core.Enums;
using LunaVK.Core.Network;

namespace LunaVK.Core.ViewModels
{
    public class MarketViewModel : GenericCollectionViewModel<VKMarketItem>
    {
        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKMarketItem>> callback)
        {
            MarketService.Instance.GetProducts(0, count, offset, (result) => {
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
    }
}
