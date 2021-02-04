using LunaVK.Core.Utils;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.UC
{
    public sealed partial class PhotoAlbumUC : UserControl
    {
        public PhotoAlbumUC()
        {
            this.InitializeComponent();
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            img.Animate(0, 1, "Opacity", 300);
            img.ImageOpened -= Image_ImageOpened;
        }
    }
}
