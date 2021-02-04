using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.ViewModels
{
    public class PostsSearchViewModel : GenericCollectionViewModel<VKWallPost>
    {
        private readonly int _ownerId;
        private readonly string _domain;
        public string _query = string.Empty;

        public PostsSearchViewModel(int ownerId,string domain)
        {
            this._ownerId = ownerId;
            this._domain = domain;
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKWallPost>> callback)
        {
            WallService.Instance.Search(this._query, this._domain, this._ownerId, count, offset, (result) =>
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
}
