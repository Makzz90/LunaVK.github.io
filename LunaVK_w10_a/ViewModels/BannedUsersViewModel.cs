using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public sealed class BannedUsersViewModel : GenericCollectionViewModel<VKBaseDataForGroupOrUser>
    {
        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKBaseDataForGroupOrUser>> callback)
        {
            AccountService.Instance.GetBannedUsers(offset, count, (res =>
            {
                List<VKBaseDataForGroupOrUser> list = new List<VKBaseDataForGroupOrUser>();

                if (res.error.error_code == VKErrors.None)
                {
                    base._totalCount = res.response.count;

                    foreach(var i in res.response.items)
                    {
                        var profile = res.response.profiles.Find(p => p.id == i);
                        list.Add(profile);
                    }
                }

                callback(res.error, list);
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

        internal void DeleteSelected(List<VKBaseDataForGroupOrUser> list)
        {
            AccountService.Instance.UnbanUsers(list.Select(fh => fh.Id).ToList(), (result)=> {
                if(result.error.error_code == VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(()=> {
                        foreach(var item in list)
                        {
                            base.Items.Remove(item);
                            base._totalCount--;
                        }
                        list.Clear();
                        base.NotifyPropertyChanged(nameof(base.FooterText));
                    });
                }

            });
        }
    }
}
