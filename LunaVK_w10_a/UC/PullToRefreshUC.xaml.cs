using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace LunaVK.UC
{
    public sealed partial class PullToRefreshUC : UserControl
    {
#if WINDOWS_PHONE_APP
//        StatusBar bar;
#endif
        public PullToRefreshUC()
        {
            this.InitializeComponent();
            this.Loaded += PullToRefreshUC_Loaded;
            this.SizeChanged += PullToRefreshUC_SizeChanged;
        }

        private void PullToRefreshUC_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateWidth(e.NewSize.Width);
        }

        void PullToRefreshUC_Loaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
//            bar = StatusBar.GetForCurrentView();
#endif
            this.UpdateWidth(base.ActualWidth);
            this.Update(0.0);
        }

        private void UpdateWidth(double width)
        {
            this.rectProgress.Width = width;
            (this.rectProgress.RenderTransform as ScaleTransform).CenterX = width / 2.0;
        }
        /*
        public void TrackListBox(Framework.ExtendedListView2 lv)
        {
            lv.OnPullPercentageChanged += this.OnPullPercentageChanged;
            lv.Unloaded += Lv_Unloaded;
            this.Update(0.0);
        }
        */
        public void TrackListBox(Controls.ExtendedListView3 lv)
        {
            lv.OnPullPercentageChanged += this.OnPullPercentageChanged;
            lv.Unloaded += Lv_Unloaded;
            this.Update(0.0);
        }

        private void Lv_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Update(0.0);
            var lv = sender as Controls.ExtendedListView3;
            if (lv != null)
            {
                lv.OnPullPercentageChanged -= this.OnPullPercentageChanged;
                return;
            }
            var lv2 = sender as Controls.ExtendedListView3;
            lv2.OnPullPercentageChanged -= this.OnPullPercentageChanged;
        }

        private void OnPullPercentageChanged(double value)
        {
            this.Update(value);
        }

        private void Update(double p)
        {
            //1-100%
            //0-0%
            double new_val = 1.0/100.0*p;
            (this.rectProgress.RenderTransform as ScaleTransform).ScaleX = new_val;
            this.textBlockTip.Opacity = new_val;

#if WINDOWS_PHONE_APP
            /*
            if( p < 2 )
            {
                bar.ShowAsync();
            }
            else
            {
                bar.HideAsync();
            }
            */
#endif
        }
    }
}
