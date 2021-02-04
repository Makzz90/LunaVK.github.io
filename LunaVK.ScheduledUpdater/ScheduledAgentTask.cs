using LunaVK.Core;
using LunaVK.Core.DataObjects;
using System;
using Windows.ApplicationModel.Background;

namespace ScheduledUpdater
{
    public sealed class ScheduledAgentTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            if (Settings.IsAuthorized)
            {
                BackgroundTaskDeferral deferral = taskInstance.GetDeferral(); // Get a deferral, to prevent the task from closing prematurely while asynchronous code is still running.

                SecondaryTileManager.Instance.UpdateAllExistingTiles( (resSecondary)=>
                {
                    CountersService.Instance.GetCountersWithLastMessage((res)=>
                    {
                        if (res.error.error_code == LunaVK.Core.Enums.VKErrors.None)
                        {
                            PrimaryTileManager.Instance.SetCounter(res.response.Counters.TotalCount);

                            PrimaryTileManager.Instance.ResetContent();

                            byte count = 0;

                            foreach (var conversation in res.response.Convs.items)
                            {
                                VKBaseDataForGroupOrUser owner = null;
                                if (conversation.last_message.from_id > 0)
                                    owner = res.response.Convs.profiles.Find((u) => u.id == conversation.last_message.from_id);
                                else
                                    owner = res.response.Convs.groups.Find((u) => u.id == -conversation.last_message.from_id);
                                PrimaryTileManager.Instance.AddContent(owner.Title, conversation.last_message.text, "Conversation_" + conversation.conversation.peer.id, owner.MinPhoto);
                                count++;
                            }

                            if (count >= 5)
                                return;
                            //Основная плитка поддерживает только 5 содержимых
                            foreach (var friend in res.response.Friends.items)
                            {
                                PrimaryTileManager.Instance.AddContent("Заявка в друзья", friend.Title, "Friend_" + friend.id, friend.MinPhoto);
                                count++;
                                if (count >= 5)
                                    break;
                            }
                        }

                        deferral.Complete(); // Inform the system that the task is finished.
                    });
                });
            }
        }
    }
}
