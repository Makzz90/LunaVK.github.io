using System;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Core.Framework;
using System.Collections.Generic;
using LunaVK.Core.Network;

namespace LunaVK.Core.ViewModels
{
    public class CommunitySubscribersViewModel
    {
        public CommunityManagementRole currentUserRole = CommunityManagementRole.Unknown;


        //public List<VKGroupContact> Contacts;

        public uint GroupId { get; private set; }

        public GenericCollectionAll All { get; private set; }

        public GenericCollectionUnsure Unsure { get; private set; }

        public GenericCollectionFriends Friends { get; private set; }

        public CommunitySubscribersViewModel(uint group_id, VKGroupType type)
        {
            //this.Contacts = new List<VKGroupContact>();

            this.All = new GenericCollectionAll(group_id);
            this.Unsure = new GenericCollectionUnsure(group_id);
            this.Friends = new GenericCollectionFriends(group_id, type);

            this.GroupId = group_id;
        }

        public class GenericCollectionAll : GenericCollectionViewModel<VKUser>
        {
            private uint CommunityId;

            public GenericCollectionAll(uint communityId)
            {
                this.CommunityId = communityId;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
            {
                GroupsService.Instance.GetSubscribers(this.CommunityId, offset, count, "all", offset == 0, (result) =>
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

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("NoSubscribersYet");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneSubscriberFrm", "TwoFourSubscribersFrm", "FiveSubscribersFrm");
                }
            }
            /*
             *  public string GetFooterTextForCount(GenericCollectionViewModel<CommunitySubscribers, LinkHeader> caller, int count)
        {
            if (this.CommunityType == GroupType.PublicPage)
            {
                if (count <= 0)
                    return GroupResources.NoSubscribersYet;
                return UIStringFormatterHelper.FormatNumberOfSomething(count, GroupResources.OneSubscriberFrm, GroupResources.TwoFourSubscribersFrm, GroupResources.FiveSubscribersFrm, true, (string)null, false);
            }
            if (count <= 0)
                return GroupResources.NoParticipantsYet;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, GroupResources.OneMemberFrm, GroupResources.TwoFourMembersFrm, GroupResources.FiveMembersFrm, true, (string)null, false);
        }
        */
        }

        public class GenericCollectionUnsure : GenericCollectionViewModel<VKUser>
        {
            private uint CommunityId;

            public GenericCollectionUnsure(uint communityId)
            {
                this.CommunityId = communityId;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
            {
                GroupsService.Instance.GetSubscribers(this.CommunityId, offset, count, "unsure", offset == 0, (result) =>
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

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("NoParticipantsYet");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneMemberFrm", "TwoFourMembersFrm", "FiveMembersFrm");
                }
            }
        }

        public class GenericCollectionFriends : GenericCollectionViewModel<VKUser>
        {
            private uint CommunityId;
            VKGroupType CommunityType;

            public GenericCollectionFriends(uint communityId, VKGroupType type)
            {
                this.CommunityId = communityId;
                this.CommunityType = type;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
            {
                GroupsService.Instance.GetSubscribers(this.CommunityId, offset, count, "friends", offset == 0, (result) =>
                  {
                      if (result != null && result.error.error_code == VKErrors.None)
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
                    if (this.CommunityType == VKGroupType.Page)
                    {
                        if (base._totalCount == 0)
                            return LocalizedStrings.GetString("NoSubscribersYet");
                        return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneSubscriberFrm", "TwoFourSubscribersFrm", "FiveSubscribersFrm");
                    }

                    if (base._totalCount == 0)
                        return LocalizedStrings.GetString("NoParticipantsYet");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneMemberFrm", "TwoFourMembersFrm", "FiveMembersFrm");
                }
            }
        }

        public void RemoveFromCommunity(VKUser user)
        {
            GroupsService.Instance.HandleRequest(this.GroupId, user.id, false, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {

                        if (this.All.Items.Contains(user))
                        {
                            this.All.Items.Remove(user);
                            this.All._totalCount--;
                        }

                        if (this.Unsure.Items.Contains(user))
                        {
                            this.Unsure.Items.Remove(user);
                            this.Unsure._totalCount--;
                        }

                        if (this.Friends.Items.Contains(user))
                        {
                            this.Friends.Items.Remove(user);
                            this.Friends._totalCount--;
                        }
                    });
                }
            });
        }
    }
}
