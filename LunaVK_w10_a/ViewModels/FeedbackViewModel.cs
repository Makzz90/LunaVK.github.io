using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Network;
using System;
using System.Collections.Generic;

namespace LunaVK.ViewModels
{
    public class FeedbackViewModel
    {
        public GenericCollectionFeedback FeedbackVM { get; private set; }

        public GenericCollectionComments CommentsVM { get; private set; }

        public FeedbackViewModel()
        {
            this.FeedbackVM = new GenericCollectionFeedback();
            this.CommentsVM = new GenericCollectionComments();
        }

        public class GenericCollectionFeedback : GenericCollectionViewModel<VKNotification>
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

        public class GenericCollectionComments : GenericCollectionViewModel<VKNewsfeedPost>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKNewsfeedPost>> callback)
            {
                NewsFeedService.Instance.GetNewsComments(0,0, count,base._nextFrom,result =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._nextFrom = result.response.next_from;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }
        }
    }
}
