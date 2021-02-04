using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
//https://apps.timwhitlock.info/emoji/tables/unicode#block-1-emoticons
//C:\Users\Makzz\AppData\Local\Packages\Microsoft.SkypeApp_kzf8qxf38zg5c
//https://support.skype.com/en/faq/fa12330/what-is-the-full-list-of-emoticons
namespace LunaVK.UC
{
    public sealed partial class AnimatedEmojiUC : UserControl
    {
        public AnimatedEmojiUC()
        {
            this.InitializeComponent();
        }

        public AnimatedEmojiUC(string glyph):this()
        {
            this._imgBrush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/SkypeEmoji/" + glyph+"_anim.png"));///Assets/SkypeEmoji/1f621_anim.png
        }

        private void ImageBrush_ImageOpened(object sender, RoutedEventArgs e)
        {
            this._brd.Width = this.ActualWidth;
            this._brd.Height = this.ActualHeight;

            ImageBrush brush = sender as ImageBrush;
            BitmapImage img = brush.ImageSource as BitmapImage;
            int steps = img.PixelHeight / img.PixelWidth;

            for (int i = 0; i < steps; i++)
            {
                DiscreteObjectKeyFrame frame = new DiscreteObjectKeyFrame();
                frame.Value = -(this._brd.ActualWidth * i);
                frame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(43 * i));
                this.objAnim.KeyFrames.Add(frame);
            }
            this.myStoryboard.Begin();
        }
    }
}
