using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using System.Threading.Tasks;
using LunaVK.Core;
using LunaVK.Framework;
using LunaVK.Core.Network;
using System.Globalization;
using LunaVK.Core.Framework;
using System.Linq;


namespace LunaVK.ViewModels
{
    public class BirthdaysViewModel : GenericCollectionViewModel<Birthday>
    {
        DelayedExecutor _de = new DelayedExecutor(5000);
        public ObservableGroupingCollection<Birthday> GroupedItems { get; private set; }

        private bool _isDataUpdating;
        //public ObservableCollection<Birthday> Birthdays { get; private set; }

        public Visibility BlockVisibility
        {
            get { return base.Items.Count > 0 ? Visibility.Visible : Visibility.Collapsed; }
        }
        
        public BirthdaysViewModel()
        {
            //this.Birthdays = new ObservableCollection<Birthday>();
            this.GroupedItems = new ObservableGroupingCollection<Birthday>(base.Items);
        }
        
        private void Process()
        {
            var friends = /*await*/ FriendsCache.Instance.GetFriends();
            if (friends == null)
                return;

            Execute.ExecuteOnUIThread(() => { 
            base.Items.Clear();
            foreach (VKUser friend in friends.Take(3))//todo:optimize
            {
                if (friend.IsBirthdayToday())
                {
                    base.Items.Add(new Birthday(friend, "", true));

                    //
                    string str = LocalizedStrings.GetString("HasABirthdayToday");
                    CustomFrame.Instance.NotificationsPanel.AddAndShowNotification(friend.MinPhoto, friend.Title, str/*, () =>
                    {
                        if (user_id != 0)
                            Library.NavigatorImpl.Instance.NavigateToConversation(user_id);
                    }*/);
                }
                else if (friend.IsBirthdayTomorrow())
                    base.Items.Add(new Birthday(friend, LocalizedStrings.GetString("Tomorrow"), false));

            }

            // ISSUE: method reference
            this.NotifyPropertyChanged(nameof(this.BlockVisibility));
            this._isDataUpdating = false;
            });
        }

        public /*async*/ void UpdateData()
        {
            if (this._isDataUpdating )
                return;
            this._isDataUpdating = true;
            this._de.AddToDelayedExecution(this.Process);
        }









        private static int GetDiffFromDate(DateTime baseDate, int month, int day)
        {
            int cur = baseDate.Month * 100 + baseDate.Day;
            int num2 = month * 100 + day;
            
            return num2 >= cur ? (num2 - cur) : (num2 - cur + 100000);
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<Birthday>> callback)
        {
            base._totalCount = 0;
            var friends = FriendsCache.Instance.GetFriends();
            if (friends == null)
            {
                callback(new VKError(), null);
                return;
            }

            DateTime dateTime = DateTime.Now;

            List <VKUser> list0 = friends.ToList();
            var enumerator1 = Enumerable.OrderBy<VKUser, int>(list0.Where(u => { return u.GetBDateDay() > 0 && u.GetBDateMonth() > 0; }), (u =>
            {
                DateTime baseDate = dateTime;
                int month = u.GetBDateMonth();
                int day = u.GetBDateDay();
                return BirthdaysViewModel.GetDiffFromDate(baseDate, month, day);
            })).ToList();

            List<Birthday> list = new List<Birthday>();
            foreach (VKUser friend in enumerator1)
            {
                int day = friend.GetBDateDay();
                int month = friend.GetBDateMonth();
                int year = friend.GetBDateYear();

                if(day>0)
                {
                    if(month>0)
                    {
                        if (friend.IsBirthdayToday())
                        {
                            string subtitle;

                            if (year > 0)
                            {
                                subtitle = (UIStringFormatterHelper.FormatNumberOfSomething(DateTime.Now.Year - year, "TurnsOneYearTodayFrm", "TurnsTwoFourYearsTodayFrm", "TurnsFiveYearsTodayFrm"));
                            }
                            else
                            {
                                subtitle = LocalizedStrings.GetString("HasABirthdayToday");
                                
                            }

                            list.Add(new Birthday(friend, subtitle,true,"Today"));
                        }
                        else if (friend.IsBirthdayTomorrow())
                        {
                            list.Add(new Birthday(friend, friend.GetBDateString(), false, "Tomorrow"));
                        }
                        else
                        {
                            string key = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
                            list.Add(new Birthday(friend, friend.GetBDateString(), false, key));
                        }
                    }
                }

                
                

            }

            callback(new VKError(), list);
        }







        
    }

    public class Birthday : ISupportGroup
    {
        public VKBaseDataForGroupOrUser User { get; private set; }

        public string Key { get; private set; }

        public int UserId
        {
            get { return this.User.Id; }
        }

        public string UserPhoto
        {
            get { return this.User.MinPhoto; }
        }

        public string UserName
        {
            get { return this.User.Title; }
        }

        public string Description { get; private set; }

        public Visibility DescriptionVisibility
        {
            get { return string.IsNullOrEmpty(this.Description) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility GiftVisibility { get; private set; }

        public Birthday(VKBaseDataForGroupOrUser user, string subtitle = "", bool canSendGift = false, string key = "")
        {
            this.User = user;
            this.Description = subtitle;
            this.GiftVisibility = canSendGift ? Visibility.Visible : Visibility.Collapsed;

            this.Key = key;
        }
    }
}
