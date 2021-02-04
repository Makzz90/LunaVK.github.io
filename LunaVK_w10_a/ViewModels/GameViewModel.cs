using LunaVK.Core.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public class GameViewModel : GenericCollectionViewModel<VKWallPost>
    {
        public VKGame Game { get; private set; }

        public GameViewModel(VKGame game)
        {
            this.Game = game;

        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKWallPost>> callback)
        {
            if (offset == 0)
            {
                /*
                AppsService.Instance.GetApp(this.Game.id, (res) =>
                {
                    if(res.error.error_code== VKErrors.None)
                    {

                    }
                });*/
                callback(new VKError(), null);
            }
            else
            {
                /*
                AccountService.Instance.GetBannedUsers(offset, count, (res =>
                {
                    List<VKBaseDataForGroupOrUser> list = new List<VKBaseDataForGroupOrUser>();

                    if (res.error.error_code == VKErrors.None)
                    {
                        base._totalCount = res.response.count;

                        foreach (var i in res.response.items)
                        {
                            var profile = res.response.profiles.Find(p => p.id == i);
                            list.Add(profile);
                        }
                    }

                    callback(res.error.error_code, list);
                }));*/
            }
        }
    }
}
