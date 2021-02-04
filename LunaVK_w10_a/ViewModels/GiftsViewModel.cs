using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;

namespace LunaVK.ViewModels
{
    public class GiftsViewModel : GenericCollectionViewModel<GiftItem>
    {
        private readonly uint _userId;
        private readonly string _firstName;
        private string _firstNameGen;

        public GiftsViewModel(uint userId, string firstName, string firstNameGen)
        {
            this._userId = userId;
            this._firstName = firstName;
            this._firstNameGen = firstNameGen;
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<GiftItem>> callback)
        {
            GiftsService.Instance.Get(this._userId, count, offset, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    base._totalCount = result.response.count;

                    List<GiftItem> list = new List<GiftItem>();

                    foreach (var item in result.response.items)
                    {
                        GiftItem gift = new GiftItem();

                        gift.id = item.gift.id;
                        gift.thumb_256 = item.gift.thumb_256;
                        gift.date = item.date;
                        gift.message = item.message;
                        gift.privacy = item.privacy;
                        gift.from_id = item.from_id;

                        if (item.from_id > 0)
                            gift.Owner = result.response.profiles.First((i) => i.id == item.from_id);
                        else
                            gift.Owner = result.response.groups.First((j) => j.id == (-item.from_id));

                        list.Add(gift);

                    }

                    callback(result.error, list);
                }
                else
                {
                    callback(result.error, null);
                }
            });
        }


        /*
        public override string GetFooterTextForCount
        {
            get
            {
                if (base._totalCount == 0)
                    return LocalizedStrings.GetString("NoPersons");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OnePersonFrm", "TwoFourPersonsFrm", "FivePersonsFrm");
            }
        }
        */
    }

    public class GiftItem : VKGift
    {
        public VKBaseDataForGroupOrUser Owner { get; set; }

        public int date { get; set; }

        public string message { get; set; }
        public int privacy { get; set; }
        public int from_id { get; set; }

        public Visibility MessageOrPrivacyDescriptionVisibility
        {
            get
            {
                return (this.MessageVisiblity == Visibility.Visible || this.PrivacyDescriptionVisibility == Visibility.Visible).ToVisiblity();
            }
        }

        public Visibility MessageVisiblity
        {
            get
            {
                return string.IsNullOrEmpty(this.message).ToVisiblity();
            }
        }

        public Visibility PrivacyDescriptionVisibility
        {
            get
            {
                return (this.Owner.Id == Settings.UserId || this.privacy == 1).ToVisiblity();
            }
        }

        public Visibility SendBackVisibility
        {
            get
            {
                return (this.Owner.Id == Settings.UserId || from_id > 1).ToVisiblity();
            }
        }

        public string Date
        {
            get
            {
                return UIStringFormatterHelper.FormatDateTimeForUI(this.date);
            }
        }

        public Visibility MoreActionsVisibility
        {
            get
            {
                /*
                 * this.CanSeeGifts = profile != null && profile.can_see_gifts == 1;
      this.IsMoreActionsVisible = this.IsCurrentUser || this.CanSeeGifts;
                 * */
                return (this.Owner.Id == Settings.UserId || from_id > 1).ToVisiblity();
            }
        }
    }
}
