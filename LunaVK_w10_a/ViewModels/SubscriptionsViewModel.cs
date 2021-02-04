using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public class SubscriptionsViewModel
    {
        //private readonly uint _userId;

        public PagesCollection PagesVM { get; private set; }
               
        public GroupsCollection GroupsVM { get; private set; }

        public class PagesCollection : GenericCollectionViewModel<VKGroup>
        {
            private readonly uint _userId;

            public PagesCollection(uint userId)
            {
                this._userId = userId;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKGroup>> callback)
            {
                UsersService.Instance.GetSubscriptions(this._userId, (result) =>
                {
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

        public class GroupsCollection : GenericCollectionViewModel<VKGroup>
        {
            private readonly uint _userId;

            public GroupsCollection(uint userId)
            {
                this._userId = userId;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKGroup>> callback)
            {
                UsersService.Instance.GetSubscriptions(this._userId, (result) =>
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
        }

        public SubscriptionsViewModel(uint userId)
        {
            this.PagesVM = new PagesCollection(userId);
            this.GroupsVM = new GroupsCollection(userId);

        }
    }
}
