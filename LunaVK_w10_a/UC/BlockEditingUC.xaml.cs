using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.UC
{
    public sealed partial class BlockEditingUC : UserControl
    {
        public BlockEditingUC()
        {
            this.InitializeComponent();
        }

        public BlockEditingUC(uint communityId, VKUser user, VKUser manager) : this()
        {
            base.DataContext = new BlockEditingViewModel(communityId,user, manager);
        }

        public sealed class BlockEditingViewModel : ViewModelBase
        {
            public BlockEditingViewModel(uint communityId, VKUser user, VKUser manager)
            {
                this._communityId = communityId;
                this._manager = manager;
                this._user = user;

                if (user != null)
                    this.BanReason = user.ban_info.reason;
            }

            private readonly uint _communityId;
            private readonly VKUser _user;
            private readonly VKUser _manager;

            public string UserPhoto
            {
                get { return this._user.MinPhoto; }
            }

            public string UserName
            {
                get { return this._user.Title; }
            }

            public bool IsCommentVisible { get; set; }


            public Visibility AddedByVisibility
            {
                get { return (this._manager != null).ToVisiblity();  }
            }

            public string UserMembership
            {
                get
                {
                    if (this._manager == null)
                        return LocalizedStrings.GetString("IsCommunityMemberOrFollower");
                    if (this._user.sex == Core.Enums.VKUserSex.Female)
                        return LocalizedStrings.GetString("BlockedInCommunityFemale");
                    return LocalizedStrings.GetString("BlockedInCommunityMale");
                }
            }

            public string AddByForm
            {
                get
                {
                    if (this._manager == null)
                        return "";
                    if (this._manager.sex == Core.Enums.VKUserSex.Female)
                        return LocalizedStrings.GetString("AddedFemale");
                    return LocalizedStrings.GetString("AddedMale");
                }
            }

            public string ManagerName
            {
                get
                {
                    return (this._manager != null ? this._manager.Title : null) ?? "";
                }
            }

            public string BlockStartDate
            {
                get
                {
                    return UIStringFormatterHelper.FormateDateForEventUI(this._user.ban_info.date);
                }
            }

            public string Comment
            {
                get
                {
                    return ExtensionsBase.ForUI(this._user.ban_info.comment);
                }
            }

            public int BanReason { get; set; }
        }
    }
}
