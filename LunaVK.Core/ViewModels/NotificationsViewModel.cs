using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using System.Threading.Tasks;

namespace LunaVK.Core.ViewModels
{
    public class NotificationsViewModel : ISupportUpDownIncrementalLoading
    {
        public ObservableCollection<VKNotification> Notifications { get; private set; }

        public bool HasMoreUpItems
        {
            get { return false; }
        }

        public bool HasMoreDownItems
        {
            get
            {
                if (this.nextFrom == null && this.Notifications.Count == 0)
                    return true;

                return !string.IsNullOrEmpty(this.nextFrom);
            }
        }

//        public bool IsReadyForAutoLoad { get; set; }

        private string nextFrom = null;

        public NotificationsViewModel()
        {
            this.Notifications = new ObservableCollection<VKNotification>();
        }

        public async Task LoadUpAsync()
        {

        }

        public Action<ProfileLoadingStatus> LoadingStatusUpdated;

        public async Task<object> Reload()
        {
            this.nextFrom = "";
            this.Notifications.Clear();
            this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Reloading);
            await LoadDownAsync(true);

            //todo:
            //            if(MenuViewModel.Instance.NotificationsItem.Count>0)
            //            {
            await Task.Delay(1000);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters = new Dictionary<string, string>();
            var temp2 = await RequestsDispatcher.GetResponse<int>("notifications.markAsViewed", parameters);
            //if(temp2.error.error_code== VKErrors.None)
            //{
            //    EventAggregator.Instance.PublishCounters();
            //}
            //            }


            return null;
        }

        public async Task LoadDownAsync(bool InReload = false)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            if (!String.IsNullOrEmpty(this.nextFrom))
            {
                parameters["start_from"] = this.nextFrom;
                parameters["count"] = "30";
            }
            else
            {
                parameters["count"] = "15";
            }

            VKResponse<NotificationData> temp = await RequestsDispatcher.GetResponse<NotificationData>("notifications.get", parameters);

            if (temp.error.error_code == VKErrors.None)
            {
                this.nextFrom = temp.response.next_from;

                UsersService.Instance.SetCachedUsers(temp.response.groups);
                UsersService.Instance.SetCachedUsers(temp.response.profiles);

                foreach (VKNotification p in temp.response.items)
                {
                    this.Notifications.Add(p);
                }
                
            }

//            this.IsReadyForAutoLoad = true;

        }

        public class NotificationData
        {
            /// <summary>
            /// массив оповещений для текущего пользователя. 
            /// </summary>
            public List<VKNotification> items { get; set; }

            /// <summary>
            /// информация о пользователях, которые находятся в списке оповещений. 
            /// </summary>
            public List<VKUser> profiles { get; set; }

            /// <summary>
            /// информация о сообществах, которые находятся в списке оповещений. 
            /// </summary>
            public List<VKGroup> groups { get; set; }

            /// <summary>
            /// время последнего просмотра пользователем раздела оповещений в формате Unixtime. 
            /// </summary>
            //public int last_viewed { get; set; }

            public string next_from { get; set; }
        }
    }
}
