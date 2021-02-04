using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public class LikesViewModel
    {
        public GenericCollectionAll AllVM { get; private set; }

        public GenericCollectionShared SharedVM { get; private set; }

        public GenericCollectionFriends FriendsVM { get; private set; }

        public LikesViewModel(int ownerId, uint itemId, LikeObjectType type, int knownCount)
        {
            this.AllVM = new GenericCollectionAll(ownerId,itemId,type);
            this.SharedVM = new GenericCollectionShared(ownerId, itemId, type);
            this.FriendsVM = new GenericCollectionFriends(ownerId, itemId, type);

            this._allCount = knownCount;
        }

        private int _allCount = -1;

        public string Title
        {
            get
            {
                if (this._allCount < 0)
                    return "";
                return UIStringFormatterHelper.FormatNumberOfSomething(this._allCount, "LikesPage_OneLikedFrm", "LikesPage_TwoFourLikedFrm", "LikesPage_FiveLikedFrm");
            }
        }

        public class GenericCollectionAll : GenericCollectionViewModel<VKUser>
        {
            private int _ownerId;
            private uint _itemId;
            private LikeObjectType _type;

            public GenericCollectionAll(int ownerId, uint itemId, LikeObjectType type)
            {
                this._ownerId = ownerId;
                this._itemId = itemId;
                this._type = type;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
            {
                LikesService.Instance.GetLikesList(this._type, this._ownerId, this._itemId, count, offset, false, false, (result =>
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
                }));
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount == 0)
                        return LocalizedStrings.GetString("NoPersons");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OnePersonFrm", "TwoFourPersonsFrm", "FivePersonsFrm");
                }
            }
        }

        public class GenericCollectionShared : GenericCollectionViewModel<VKUser>
        {
            private int _ownerId;
            private uint _itemId;
            private LikeObjectType _type;

            public GenericCollectionShared(int ownerId, uint itemId, LikeObjectType type)
            {
                this._ownerId = ownerId;
                this._itemId = itemId;
                this._type = type;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
            {
                LikesService.Instance.GetLikesList(this._type, this._ownerId, this._itemId, count, offset, true, false, (result =>
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
                }));
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount == 0)
                        return LocalizedStrings.GetString("NoPersons");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OnePersonFrm", "TwoFourPersonsFrm", "FivePersonsFrm");
                }
            }
        }

        public class GenericCollectionFriends : GenericCollectionViewModel<VKUser>
        {
            private int _ownerId;
            private uint _itemId;
            private LikeObjectType _type;

            public GenericCollectionFriends(int ownerId, uint itemId, LikeObjectType type)
            {
                this._ownerId = ownerId;
                this._itemId = itemId;
                this._type = type;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
            {
                LikesService.Instance.GetLikesList(this._type, this._ownerId, this._itemId, count, offset, false, true, (result =>
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
                }));
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount == 0)
                        return LocalizedStrings.GetString("NoPersons");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OnePersonFrm", "TwoFourPersonsFrm", "FivePersonsFrm");
                }
            }
        }
    }
}
