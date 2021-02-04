using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.ViewModels
{
    public class GroupCallbackServerViewModel : ViewModelBase
    {
        private readonly uint _communityId;
        public List<GroupsService.CallbackServer> CallbackServers { get; private set; }

        public GroupCallbackServerViewModel(uint communityId)
        {
            this._communityId = communityId;
            this.CallbackServers = new List<GroupsService.CallbackServer>();
        }

        public void LoadData()
        {
            GroupsService.Instance.GetCallbackServers(this._communityId, (result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    if (result.error.error_code == Core.Enums.VKErrors.None)
                    {
                        this.CallbackServers = result.response.items;
                        base.NotifyPropertyChanged(nameof(this.CallbackServers));
                    }
                });
            });
        }
    }
}
