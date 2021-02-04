using System;
using System.Collections.Generic;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Core.Network;

namespace LunaVK.Core.ViewModels
{
    public class GamesMainViewModel : GenericCollectionViewModel<VKGame>
    {
        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKGame>> callback)
        {
            
            AppsService.Instance.GetCatalog(offset, count, (result) => {
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
            /*
            AppsService.Instance.GetDashboard(offset, count, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    base._totalCount = 0;
                    //callback(result.error, result.response.items);
                }
                else
                {
                    //callback(result.error, result.response.items);
                }
            });*/
        }

        public override string GetFooterTextForCount
        {
            get
            {
                if (base._totalCount <= 0)
                    return LocalizedStrings.GetString("NoGames");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneGameFrm", "TwoFourGamesFrm", "FiveGamesFrm");
            }
        }
    }
}
