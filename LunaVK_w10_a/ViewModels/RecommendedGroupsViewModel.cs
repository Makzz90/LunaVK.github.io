using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public class RecommendedGroupsViewModel : GenericCollectionViewModel<GroupsService.GroupCatalogCategoryPreview>
    {
        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<GroupsService.GroupCatalogCategoryPreview>> callback)
        {
            GroupsService.Instance.GetCatalogCategoriesPreview((result)=> {
                if(result.error.error_code == VKErrors.None)
                {
                    if(result.response.categories==null)//у свежего профиля нет рекомендаций
                    {
                        base._totalCount = 0;
                    }
                    else
                    {
                        base._totalCount = (uint)result.response.categories.Count;
                    }
                    
                    callback(result.error, result.response.categories);
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
                //return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneCommunityFrm", "TwoFourCommunitiesFrm", "FiveCommunitiesFrm");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneGroup", "TwoFourGroupsFrm", "FiveMoreGroupsFrm");
            }
        }

    }
}
