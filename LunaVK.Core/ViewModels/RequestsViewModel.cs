using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using LunaVK.Core.DataObjects;
using System.Threading.Tasks;
using LunaVK.Core.Network;
using LunaVK.Core.Library;
using LunaVK.Core.Enums;
using LunaVK.Core.Utils;
using LunaVK.Core.Framework;

namespace LunaVK.Core.ViewModels
{
    public class RequestsViewModel : GenericCollectionViewModel<VKUser>
    {
        uint GroupId;

        public RequestsViewModel(uint group_id)
        {
            this.GroupId = group_id;
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
        {
            GroupsService.Instance.GetRequests(this.GroupId, offset, count, (result) => {
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

        public override string GetFooterTextForCount
        {
            get
            {
                if (base._totalCount <= 0)
                    return LocalizedStrings.GetString("NoRequests");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "RequestOneForm", "RequestTwoForm", "RequestFiveForm");
            }
        }

        public void AddUser(VKUser user)
        {
            GroupsService.Instance.HandleRequest(this.GroupId, user.id, true, (result) => {
                if(result.error.error_code == VKErrors.None)
                {
                    if (result.response == 1)
                    {
                        Execute.ExecuteOnUIThread(() => {
                            base.Items.Remove(user);
                        });
                        base._totalCount--;
                    }
                }
            });
        }

        public void DeleteUser(VKUser user)
        {
            GroupsService.Instance.HandleRequest(this.GroupId, user.id, false, (result) => {
                if (result.error.error_code == VKErrors.None)
                {
                    if (result.response == 1)
                    {
                        Execute.ExecuteOnUIThread(() => {
                            base.Items.Remove(user);
                        });
                        base._totalCount--;
                    }
                }
            });
        }
    }
}
