using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using System;
using System.Collections.Generic;
using LunaVK.Core.DataObjects;
using LunaVK.Core;
using LunaVK.Core.Utils;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public class MarketMainViewModel : GenericCollectionViewModel<VKMarketItem>
    {
        private readonly int _ownerId;

        public MarketMainViewModel(int ownerId)
        {
            this._ownerId = ownerId;
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKMarketItem>> callback)
        {
            MarketService.Instance.GetProducts(this._ownerId, count, offset, (result) =>
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
                if (base._totalCount <= 0)
                    return LocalizedStrings.GetString("NoProducts");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneProductItemFrm", "TwoFourProductItemsFrm", "FiveProductItemsFrm");
            }
        }
    }
}
