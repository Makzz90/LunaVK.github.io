using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LunaVK.Core.ViewModels
{
    public class GroupsListViewModel
    {
        public GenericCollectionAll AllVM { get; private set; }

        public GenericCollectionEvents EventsVM { get; private set; }

        public GenericCollectionManaged ManagedVM { get; private set; }

        public GroupsListViewModel()
        {
            this.AllVM = new GenericCollectionAll();
            this.EventsVM = new GenericCollectionEvents();
            this.ManagedVM = new GenericCollectionManaged();
        }

        public class GenericCollectionAll : GenericCollectionViewModel<VKGroup>
        {
            public ObservableCollection<VKGroup> Invites { get; private set; }

            private bool _invitationsVisible;
            public bool InvitationsVisible
            {
                get
                {
                    return this._invitationsVisible;
                }
                set
                {
                    this._invitationsVisible = value;
                    base.NotifyPropertyChanged("InvitationsVisible");
                }
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKGroup>> callback)
            {
                string code = "var items = API.groups.get({extended:1,fields:\"name,verified,photo_100,activity,members_count\",count:" + count + ", offset:" + offset + "});";

                if (offset == 0)
                {
                    code += "var invitations = API.groups.getInvites({fields:\"members_count,city\"});";
                    code += "if (invitations.items.length>0){";
                    code += "var userOrGroupIds = invitations.items@.invited_by;";
                    code += "var userIds=[];var groupIds = []; var i =0;";
                    code += "while (i < invitations.items.length){";
                    code += "var id = parseInt(userOrGroupIds[i]);";
                    code += "if (id > 0)";
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

                VKRequestsDispatcher.Execute<VKGroupsGetObject>(code, (result) =>
                {

                    if (result.error.error_code == VKErrors.None)
                    {
                        this._totalCount = result.response.count;

                        if (result.response.invites != null && result.response.invites.count > 0)
                        {
                            Execute.ExecuteOnUIThread(() =>
                            {
                                this.InvitationsVisible = true;

                                foreach (VKGroup group in result.response.invites.items)
                                {
                                    VKBaseDataForGroupOrUser inviter = null;
                                    if (group.invited_by > 0)
                                        inviter = result.response.profiles.Find((u) => u.id == group.invited_by);
                                    else
                                        inviter = result.response.groups.Find((u) => u.id == (-group.invited_by));
                                    if (inviter != null)
                                        group.Inviter = inviter.Title;
                                    this.Invites.Add(group);
                                }
                            });
                        }

                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }

                }, (jsonStr) =>
                {
                    jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "groups");
                    return VKRequestsDispatcher.FixFalseArray(jsonStr, "inviters");
                });



            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount == 0)
                        return LocalizedStrings.GetString("NoPages");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneGroup", "TwoFourGroupsFrm", "FiveMoreGroupsFrm");
                }
            }

            public async Task<object> Reload()
            {
                this.Items.Clear();
                this.Invites.Clear();

                //this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Reloading);
                //await LoadDownAsync(true);
                return null;
            }

            public GenericCollectionAll()
            {
                this.Invites = new ObservableCollection<VKGroup>();
            }
        }

        public class VKGroupsGetObject : VKCountedItemsObject<VKGroup>
        {
            public VKCountedItemsObject<VKGroup> invites { get; set; }
        }

        public class GenericCollectionEvents : GenericCollectionViewModel<VKGroup>
        {
            //EventsCountVisibility
            //FooterText
            //GroupHeaderTemplate Title HasTitleVisibility

            public ObservableGroupingCollection<VKGroup> GroupedItems { get; private set; }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKGroup>> callback)
            {
                GroupsService.Instance.GetUserGroups(0, offset, count, "events", (result) =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        this._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }

            public GenericCollectionEvents()
            {
                this.GroupedItems = new ObservableGroupingCollection<VKGroup>(base.Items);
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount == 0)
                        return LocalizedStrings.GetString("NoPages");
                    return "";// UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneEventFrm", "TwoFourEventsFrm", "FiveEventsFrm");
                }
            }
        }

        public class GenericCollectionManaged : GenericCollectionViewModel<VKGroup>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKGroup>> callback)
            {
                GroupsService.Instance.GetUserGroups(0, offset, count, "moder", (result) =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        this._totalCount = result.response.count;
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
                    if (base._totalCount == 0)
                        return LocalizedStrings.GetString("NoPages");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneGroupFrm", "TwoFourGroupsFrm", "FiveMoreGroupsFrm");
                }
            }

            public async Task<object> Reload()
            {
                this.Items.Clear();

                //this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Reloading);
                //await LoadDownAsync(true);
                return null;
            }
        }

        public void Join(VKGroup group, Action<bool> callback)
        {
            GroupsService.Instance.Join((uint)group.id, null, (result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    if (result == true)
                    {
                        if (this.AllVM.Invites.Contains(group))
                        {
                            this.AllVM.Invites.Remove(group);

                            if (this.AllVM.Invites.Count == 0)
                                this.AllVM.InvitationsVisible = false;
                            //todo:добавить в список Всех
                        }
                        else if(this.EventsVM.Items.Contains(group))
                        {
                            this.EventsVM.Items.Remove(group);
                        }
                        else if (this.ManagedVM.Items.Contains(group))
                        {
                            this.ManagedVM.Items.Remove(group);
                        }
                    }

                    callback?.Invoke(result);
                });
            });

        }

        public void Leave(VKGroup group, Action<bool> callback)
        {
            GroupsService.Instance.Leave((uint)group.id, (result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    if (result == true)
                    {
                        if (this.AllVM.Items.Contains(group))
                        {
                            this.AllVM.Items.Remove(group);

                            this.AllVM._totalCount--;
                        }

                        if (this.AllVM.Invites.Contains(group))
                        {
                            this.AllVM.Invites.Remove(group);

                            if (this.AllVM.Invites.Count == 0)
                                this.AllVM.InvitationsVisible = false;
                        }
                        else if (this.EventsVM.Items.Contains(group))
                        {
                            this.EventsVM.Items.Remove(group);
                            this.EventsVM._totalCount--;
                        }
                        else if (this.ManagedVM.Items.Contains(group))
                        {
                            this.ManagedVM.Items.Remove(group);
                            this.ManagedVM._totalCount--;
                        }
                    }
                    callback?.Invoke(result);
                });
            });
        }
    }
}
