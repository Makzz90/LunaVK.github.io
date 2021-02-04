using LunaVK.Core.Utils;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace LunaVK.Library
{
    public class BackgroundTaskUtils
    {
        /// <summary>
        /// Register a background task with the specified taskEntryPoint, trigger.
        /// </summary>
        /// <param name="taskEntryPoint">Task entry point for the background task.</param>
        /// <param name="trigger">The trigger for the background task.</param>
        public static byte RegisterBackgroundTask(string taskEntryPoint, IBackgroundTrigger trigger)
        {
            string[] splitted = taskEntryPoint.Split('.');
            string name = splitted[1];

            foreach (var cur in BackgroundTaskRegistration.AllTasks) // Loop through all background tasks and unregister any with name
            {
                if (cur.Value.Name == name)
                    return 2; //cur.Value.Unregister(true);
            }

            BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
            builder.SetTrigger(trigger);

            builder.TaskEntryPoint = taskEntryPoint;
            builder.Name = name;

#if WINDOWS_UWP
            //builder.IsNetworkRequested = true; // из без этого работает ответ ;)
#endif
            try
            {
                BackgroundTaskRegistration task = builder.Register();

                Logger.Instance.Info("RegisterBackgroundTask OK: " + task.Name);
                task.Completed += Task_Completed;
                task.Progress += Task_Progress;
                return 1;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("RegisterBackgroundTask exception: " + taskEntryPoint, ex);
            }
            return 0;
        }

        private static void Task_Progress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            
            Logger.Instance.Info("Task_Progress: " + args.Progress + sender.Name + sender.TaskId);
        }

        private static void Task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            args.CheckResult();
            Logger.Instance.Info("Task_Completed: " + sender.Name + sender.TaskId);
        }

        public static bool UnRegisterBackgroundTask(string taskEntryPoint)
        {
            bool found = false;

            string[] splitted = taskEntryPoint.Split('.');
            string name = splitted[1];

            foreach (var cur in BackgroundTaskRegistration.AllTasks) // Loop through all background tasks and unregister any with name
            {
                if (cur.Value.Name == name)
                {
                    cur.Value.Unregister(true);
                    found = true;
                }
            }

            return found;
        }

    }
}
