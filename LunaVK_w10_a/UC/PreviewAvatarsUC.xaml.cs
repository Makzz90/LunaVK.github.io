using LunaVK.Core.DataObjects;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace LunaVK.UC
{
    public sealed partial class PreviewAvatarsUC : UserControl
    {
        public PreviewAvatarsUC()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty PreviewUrlsProperty = DependencyProperty.Register("PreviewUrls", typeof(List<string>), typeof(PreviewAvatarsUC), new PropertyMetadata(null, PreviewAvatarsUC.OnPreviewUrlsChanged));

        public List<string> PreviewUrls
        {
            get { return (List<string>)base.GetValue(PreviewAvatarsUC.PreviewUrlsProperty); }
            set { base.SetValue(PreviewAvatarsUC.PreviewUrlsProperty, value); }
        }

        private static void OnPreviewUrlsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IReadOnlyList<string> previews = e.NewValue as IReadOnlyList<string>;
            PreviewAvatarsUC infoListItemUc = (PreviewAvatarsUC)d;

            for (int i = 0; i < previews.Count; i++)
            {
                var p = previews[i];
                Border brd = new Border();
                brd.Style = (Style)infoListItemUc.Resources["BorderThemeItem"];
                brd.CornerRadius = new CornerRadius(brd.Width / 2);
                brd.Margin = new Thickness(0, 0, 20 * i, 0);
                brd.Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri(p)) };
                infoListItemUc.gridPreviews.Children.Add(brd);
            }
        }







        public static readonly DependencyProperty UsersProperty = DependencyProperty.Register("Users", typeof(List<VKUser>), typeof(PreviewAvatarsUC), new PropertyMetadata(null, PreviewAvatarsUC.OnUsersChanged));

        public List<VKUser> Users
        {
            get { return (List<VKUser>)base.GetValue(PreviewAvatarsUC.UsersProperty); }
            set { base.SetValue(PreviewAvatarsUC.UsersProperty, value); }
        }

        private static void OnUsersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IReadOnlyList<VKUser> previews = e.NewValue as IReadOnlyList<VKUser>;
            if (previews == null)
                return;//происходит при удалении?
            PreviewAvatarsUC infoListItemUc = (PreviewAvatarsUC)d;

            for (int i = 0; i < previews.Count; i++)
            {
                var p = previews[i];
                Border brd = new Border();
                brd.Style = (Style)infoListItemUc.Resources["BorderThemeItem"];
                brd.CornerRadius = new CornerRadius(brd.Width / 2);
                brd.Margin = new Thickness(0, 0, 20 * i, 0);
                brd.Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri(p.MinPhoto)) };
                infoListItemUc.gridPreviews.Children.Add(brd);
            }
        }
    }
}
