using System;

using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LunaVK.UC
{
	public sealed partial class WindowTitleUC : Page
    {
        public delegate void ScreenModeEvent();
        public static event ScreenModeEvent OnFullScreenMode;
        public static event ScreenModeEvent OnWindowedMode;

        public WindowTitleUC()
		{
			InitializeComponent();

			Window.Current.SetTitleBar(this.trikibar);
		}

        private void _fullScreenBtn_Tapped(object sender, TappedRoutedEventArgs e)
		{
            ApplicationView view = ApplicationView.GetForCurrentView();

            if(view.IsFullScreenMode)
            {
                Height = 32;
                view.ExitFullScreenMode();
                this.trikibarRoot.Visibility = Visibility.Visible;
            }
            else
            {
                if(view.TryEnterFullScreenMode())
                {
                    Height = 0;
                    trikibarRoot.Visibility = Visibility.Collapsed;

                    ApplicationView.GetForCurrentView().VisibleBoundsChanged += FullScreenModeTrigger_VisibleBoundsChanged;
                }
                else
                {
                    throw new Exception("Не удалось перевести приложение в полноэкранный режим");
                }
            }
            OnFullScreenMode.Invoke();
        }
        private void FullScreenModeTrigger_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            if(!sender.IsFullScreenMode)
            {
                sender.VisibleBoundsChanged -= FullScreenModeTrigger_VisibleBoundsChanged;
                this.trikibarRoot.Visibility = Visibility.Visible;
                this.Height = 32;
                OnWindowedMode.Invoke();
            }
        }
    }
}
