using System;
using System.Collections.Generic;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Network;

namespace LunaVK.Core.ViewModels
{
    public class ManagersViewModel : GenericCollectionViewModel<VKUser>
    {
        public List<VKGroupContact> Contacts;
        
        public uint GroupId;

        public ManagersViewModel(uint group_id)
        {
            this.GroupId = group_id;
        }


        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
        {
            GroupsService.Instance.GetManagers(this.GroupId, offset, count, true, (result) =>
            {
                if(result.error.error_code == VKErrors.None)
                {
                    foreach(var u in result.response.managers.items)
                    {
                        if (u.role == CommunityManagementRole.Creator)
                            u.is_hidden_from_feed = true;//Hack: запрещаем кликать на него
                    }
                    this.Contacts = result.response.contacts;
                    base._totalCount = result.response.managers.count;
                    callback(result.error, result.response.managers.items);
                }
                else
                {
                    callback(result.error, null);
                }
            });
        }

        //public sealed class CommunityManagers
        //{
        //    public VKCountedItemsObject<Manager> managers { get; set; }

        //    public List<VKGroupContact> contacts { get; set; }
        //}

        //public class Manager : VKUser
        //{
        //    public bool NoAction
        //    {
        //        get { return base.role == CommunityManagementRole.Creator; }
        //    }
        //}
    }
}
