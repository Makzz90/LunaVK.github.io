using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using LunaVK.Core.DataObjects;
using System.Threading.Tasks;
using LunaVK.Core.Network;
using LunaVK.Core.Library;
using LunaVK.Core.Enums;

namespace LunaVK.Core.ViewModels
{
    public class GroupsViewModel : ViewModelBase, ISupportUpDownIncrementalLoading
    {
        public ObservableCollection<VKGroup> Groups { get; private set; }
        public ObservableCollection<VKGroup> Invites { get; private set; }
        
        public bool InvitationsVisible { get; private set; }

        public async Task LoadUpAsync()
        {
            throw new NotImplementedException();
        }

        public bool HasMoreUpItems
        {
            get { return false; }
        }

        public bool HasMoreDownItems { get { return this.Groups.Count == 0 || this.Groups.Count < this.maximum; } }

        public Action<ProfileLoadingStatus> LoadingStatusUpdated { get; set; }

        public async Task<object> Reload()
        {
            this.Groups.Clear();
            this.Invites.Clear();

            this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Reloading);
            await LoadDownAsync(true);
            return null;
        }

        uint maximum = 0;

        public GroupsViewModel()
        {
            this.Groups = new ObservableCollection<VKGroup>();
            this.Invites = new ObservableCollection<VKGroup>();
        }

        public async Task LoadDownAsync(bool InReload = false)
        {
            string code = "var items = API.groups.get({\"extended\":1,\"fields\":\"name,verified,photo_100,activity,members_count\",\"count\":20,\"offset\":" + this.Groups.Count + "});";

            if (InReload)
            {
                code += "var invitations = API.groups.getInvites({fields:\"members_count,city\"});";
                code += "if (invitations.items.length>0){";
                code += "var userOrGroupIds = invitations.items@.invited_by;";
                code += "var userIds=[];var groupIds = []; var i =0;";
                code += "while (i < invitations.items.length){";
                code += "var id = parseInt(userOrGroupIds[i]);";
                code += "if (id > 0) ";
                code += "userIds.push(id);";
                code += "else ";
                code += "groupIds.push(-id);";
                code += "i=i+1;";
                code += "}";
                
                code += "if (userIds.length > 0) { items.profiles  = API.users.get({\"user_ids\":userIds,fields:\"sex\"}); }";
                code += "if (groupIds.length > 0) { items.groups = API.groups.getById({\"group_ids\":groupIds}); }";
                code += "items.invites = invitations;";
                code += "}";
            }

            code += "return items;";

            var temp = await RequestsDispatcher.Execute<VKGroupsGetObject>(code,(jsonStr) =>
                {
                    jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "groups");
                    return VKRequestsDispatcher.FixFalseArray(jsonStr, "inviters");
                });


            if (temp.error.error_code != VKErrors.None)
            {
                return;
            }

            if (InReload)
                maximum = temp.response.count;

            foreach (VKGroup group in temp.response.items)
            {
                this.Groups.Add(group);
            }

            if (temp.response.invites != null && temp.response.invites.count > 0)
            {
                this.InvitationsVisible = true;
                base.NotifyPropertyChanged("InvitationsVisible");

                foreach (VKGroup group in temp.response.invites.items)
                {
                    VKBaseDataForGroupOrUser inviter = null;
                    if(group.invited_by>0)
                        inviter = temp.response.profiles.Find((u) => u.id == group.invited_by);
                    else
                        inviter = temp.response.groups.Find((u) => u.id == (-group.invited_by));
                    if (inviter != null)
                        group.Inviter = inviter.Title;
                    this.Invites.Add(group);
                }
            }
        }

        public class VKGroupsGetObject : VKCountedItemsObject<VKGroup>
        {
            public VKCountedItemsObject<VKGroup> invites { get; set; }
        }

        public async Task Join(VKGroup group)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = group.id.ToString();
            var result = await RequestsDispatcher.GetResponse<int>("groups.join", parameters);

            if (result.error.error_code != VKErrors.None)
                return;

            if (result != null && result.response == 1)
            {
                this.Invites.Remove(group);

                if (this.Invites.Count == 0)
                {
                    this.InvitationsVisible = false;
                    base.NotifyPropertyChanged("InvitationsVisible");
                }
            }
        }

        public async Task Leave(VKGroup group)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = group.id.ToString();
            var result = await RequestsDispatcher.GetResponse<int>("groups.leave", parameters);

            if (result.error.error_code != VKErrors.None)
                return;

            if (result != null && result.response == 1)
            {
                this.Invites.Remove(group);

                if (this.Invites.Count == 0)
                {
                    this.InvitationsVisible = false;
                    base.NotifyPropertyChanged("InvitationsVisible");
                }
            }
        }
    }
}
