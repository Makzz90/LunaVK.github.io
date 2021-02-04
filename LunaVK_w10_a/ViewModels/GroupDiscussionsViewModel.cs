using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public class GroupDiscussionsViewModel : GenericCollectionViewModel<ThemeHeader>
    {
        public readonly uint _gid;
        public readonly VKAdminLevel _adminLevel;
        public readonly bool _isPublicPage;
        
        public GroupDiscussionsViewModel(uint gid, VKAdminLevel adminLevel, bool isPublicPage, bool canCreateTopic)
        {
            this._gid = gid;
            this._adminLevel = adminLevel;
            this._isPublicPage = isPublicPage;
            this.CanCreateDiscussion = canCreateTopic;
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<ThemeHeader>> callback)
        {
            GroupsService.Instance.GetTopics(this._gid, offset, count, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    base._totalCount = result.response.count;
                    
                    List<ThemeHeader> list = new List<ThemeHeader>();

                    foreach(var topic in result.response.items)
                    {
                        ThemeHeader h = new ThemeHeader(topic,
                            result.response.profiles == null ? null : result.response.profiles.FirstOrDefault((u => u.id == topic.updated_by)),
                            result.response.groups == null ? null : result.response.groups.FirstOrDefault((u => u.id == -topic.updated_by))
                            );
                        h.ClosedVisibility = topic.is_closed ? Visibility.Visible : Visibility.Collapsed;
                        h.FixedVisibility = topic.is_fixed ? Visibility.Visible : Visibility.Collapsed;
                            
                        list.Add(h);
                    }
                    
                    callback(result.error, list);
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
                    return LocalizedStrings.GetString("NoTopics");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount,"OneTopicFrm", "TwoFourTopicsFrm", "FiveTopicsFrm");
            }
        }

        internal void NavigateToDiscusson(bool loadFromEnd, ThemeHeader header)
        {
            NavigatorImpl.Instance.NavigateToGroupDiscussion(this._gid, header._topic.id, header._topic.title, header._topic.is_closed == false);
        }

        public bool CanCreateDiscussion { get; private set; }

        
    }

    public class ThemeHeader
    {
        public VKTopic _topic { get; private set; }
        private VKUser _user;
        private VKGroup _group;

        public ThemeHeader(VKTopic t, VKUser user, VKGroup group)
        {
            this._topic = t;
            this._user = user;
            this._group = group;
        }

        public string Header
        {
            get { return this._topic.title; }
        }

        public string MessagesCountStr
        {
            get
            {
                return UIStringFormatterHelper.FormatNumberOfSomething(this._topic.comments, "OneMessageFrm", "TwoFourMessagesFrm", "FiveMessagesFrm");
            }
        }

        public string ImageSrc
        {
            get
            {
                if (this._user != null)
                    return this._user.photo_100;
                if (this._group != null)
                    return this._group.photo_100;
                return "";
            }
        }

        public string Name
        {
            get
            {
                if (this._user != null)
                    return this._user.Title;
                if (this._group != null)
                    return this._group.name;
                return "";
            }
        }

        public string Date
        {
            get
            {
                return UIStringFormatterHelper.FormatDateTimeForUI(this._topic.updated);
            }
        }

        public string Text
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this._topic.last_comment))
                    return UIStringFormatterHelper.SubstituteMentionsWithNames(this._topic.last_comment.Replace(Environment.NewLine, " "));
                return "...";
            }
        }

        public Visibility ClosedVisibility { get; set; }

        public Visibility FixedVisibility { get; set; }


    }
}
