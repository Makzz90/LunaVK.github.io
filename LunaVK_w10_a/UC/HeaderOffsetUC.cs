using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using LunaVK.Framework;

namespace LunaVK.UC
{
    /// <summary>
    /// Сетка высотой с шапку + статус бар
    /// </summary>
    public sealed class HeaderOffsetUC : Grid
    {
        public HeaderOffsetUC()
        {
            if (CustomFrame.Instance == null)
                return;

            base.Unloaded += HeaderOffsetUC_Unloaded;
            if(CustomFrame.Instance.Header==null)
                base.Loaded += HeaderOffsetUC_Loaded;
            else
                CustomFrame.Instance.Header.HeaderHeightChanged += this.HeaderWithMenu_HeaderHeightChanged;

            WindowTitleUC.OnFullScreenMode += WindowTitleUC_OnFullScreenMode;
            WindowTitleUC.OnWindowedMode += WindowTitleUC_OnWindowedMode;
        }

        private void WindowTitleUC_OnWindowedMode()
        {
            Height = 80;
        }

        private void WindowTitleUC_OnFullScreenMode()
        {
            Height = 48;
        }

        private void HeaderOffsetUC_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.HeaderHeightChanged += this.HeaderWithMenu_HeaderHeightChanged;
            base.Loaded -= HeaderOffsetUC_Loaded;
        }

        void HeaderWithMenu_HeaderHeightChanged(object sender, double e)
        {
            base.Height = e;
        }

        void HeaderOffsetUC_Unloaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.HeaderHeightChanged -= this.HeaderWithMenu_HeaderHeightChanged;
        }
    }
}
