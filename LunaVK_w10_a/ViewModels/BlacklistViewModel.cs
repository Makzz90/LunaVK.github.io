using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LunaVK.ViewModels
{
    public sealed class BlacklistViewModel : GenericCollectionViewModel<VKUser>
    {
        public readonly uint CommunityId;

        public BlacklistViewModel(uint communityId)
        {
            this.CommunityId = communityId;
        }
        
        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
        {
            GroupsService.Instance.GetBlacklist(this.CommunityId,offset, count, (res =>
            {
                List<VKUser> result = new List<VKUser>();

                if (res.error.error_code == VKErrors.None)
                {
                    base._totalCount = (uint)res.response.blocked_users.count;

                    foreach (var banType in res.response.blocked_users.items)
                    {
                        VKUser banner = res.response.managers.First(m => m.id == banType.ban_info.admin_id);
                        VKUser user = banType.profile;
                        //todo: group
                        banType.ban_info.Manager = banner;
                        user.ban_info = banType.ban_info;

                        result.Add(user);
                    }
                }

                callback(res.error, result);
            }));
        }

        public override string GetFooterTextForCount
        {
            get
            {
                if (base._totalCount == 0)
                    return LocalizedStrings.GetString("NoPersons");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OnePersonFrm", "TwoFourPersonsFrm", "FivePersonsFrm");
            }
        }

        public void UnblockUser(VKUser item)
        {
            //this.SetInProgress(true, "");

            VKRequestsDispatcher.DispatchRequestToVK<int>("groups.unbanUser", new Dictionary<string, string>()
            {
                { "group_id", this.CommunityId.ToString() },
                { "user_id", item.id.ToString() }
            }, (result)=> {
                if (result.error.error_code == VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() => {
                        this.Items.Remove(item);
                        this._totalCount--;
                        base.NotifyPropertyChanged(nameof(base.FooterText));
                    });
                }
            });
        }
    }
}
