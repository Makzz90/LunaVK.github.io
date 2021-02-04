using System;
using System.Collections.Generic;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Network;
using LunaVK.Core.Network;

namespace LunaVK.Core.ViewModels
{
    //FeedbackViewModel
    public class NotificationsViewModel : GenericCollectionViewModel<VKNotification>
    {
        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKNotification>> callback)
        {
            NewsFeedService.Instance.GetNotifications(offset, base._nextFrom, count, (res =>
            {
                if (res.error.error_code == VKErrors.None)
                {
                    base._nextFrom = res.response.next_from;
                    base._totalCount = (uint)res.response.items.Count;

                    var c = LongPollServerService.Instance._counters;
                    if (c.notifications > 0)
                    {
                        NewsFeedService.Instance.MarkAsViewed();
                        c.notifications = 0;
                        EventAggregator.Instance.PublishCounters(c);
                    }

                    callback(res.error, res.response.items);
                }
                else
                {
                    callback(res.error, null);
                }
            }));
        }
    }
}
