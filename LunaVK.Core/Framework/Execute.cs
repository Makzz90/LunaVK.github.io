using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;

namespace LunaVK.Core.Framework
{
    public class Execute
    {
        public static void ExecuteOnUIThread(Action action)
        {
            if (Window.Current != null && Window.Current.Content is Frame && (Window.Current.Content as Frame).Dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                Execute.DO(action);
            }
        }

        private static async void DO(Action action)
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                return;

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                action();
            });
        }

        public static async void ExecuteOnBackgroundThread(Action action)
        {
            // Perform background work here.
            // Don't directly access UI elements from this method.
            await System.Threading.Tasks.Task.Run(() => action());
        }
    }
}
