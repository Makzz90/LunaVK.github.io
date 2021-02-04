using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.UC
{
    public sealed partial class NewsfeedHeaderUC : UserControl
    {
        public Action OnFreshNewsTap { get; set; }

        public NewsfeedHeaderUC()
        {
            this.InitializeComponent();

            //this.translateFreshNews.Y = 0.0;
            //this.borderFreshNews.Visibility = Visibility.Collapsed;
            this.UpdateFreshNewsLoadingState();
        }

        public static readonly DependencyProperty IsLoadingFreshNewsProperty = DependencyProperty.Register("IsLoadingFreshNews", typeof(bool), typeof(NewsfeedHeaderUC), new PropertyMetadata(false,NewsfeedHeaderUC.IsLoadingFreshNews_OnChanged));

        public bool IsLoadingFreshNews
        {
            get { return (bool)base.GetValue(NewsfeedHeaderUC.IsLoadingFreshNewsProperty); }
            set { base.SetValue(NewsfeedHeaderUC.IsLoadingFreshNewsProperty, value); }
        }

        private static void IsLoadingFreshNews_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NewsfeedHeaderUC)d).UpdateFreshNewsLoadingState();
        }

        private void UpdateFreshNewsLoadingState()
        {
            if (this.IsLoadingFreshNews)
            {
                this.imageArrowFreshNews.Visibility = Visibility.Collapsed;
                this.progressRingFreshNews.Visibility = Visibility.Visible;
                this.progressRingFreshNews.IsActive = true;
            }
            else
            {
                this.imageArrowFreshNews.Visibility = Visibility.Visible;
                this.progressRingFreshNews.Visibility = Visibility.Collapsed;
                this.progressRingFreshNews.IsActive = false;
            }
        }

        private void BorderFreshNews_OnTap(object sender, RoutedEventArgs e)
        {
            this.OnFreshNewsTap?.Invoke();
        }
    }
}
