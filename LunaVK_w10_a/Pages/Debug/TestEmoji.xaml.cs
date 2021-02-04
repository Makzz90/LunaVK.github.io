using LunaVK.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Linq;
namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestEmoji : Page
    {
        public TestEmoji()
        {
            this.InitializeComponent();
            /*
            StringBuilder builder = new StringBuilder();
            //foreach (KeyValuePair<string, string> keyValuePair in Emoji.Dict)
            //{
            //    builder.AppendLine(this.Format2(keyValuePair.Key));
            //}
            
            builder.Append("public static readonly List<uint> WeatherIndex = new List<uint>() { ");
            foreach (KeyValuePair<string, string> keyValuePair in Smiles.Weather)
            {
                int index = Emoji.Dict.IndexOf(keyValuePair.Value);
                builder.Append(index + ", ");
            }
            builder.Append("};");
            builder.AppendLine();

            builder.Append("public static readonly List<uint> GamesIndex = new List<uint>() { ");
            foreach (KeyValuePair<string, string> keyValuePair in Smiles.Games)
            {
                int index = Emoji.Dict.IndexOf(keyValuePair.Value);
                builder.Append(index + ", ");
            }
            builder.Append("};");
            builder.AppendLine();

            builder.Append("public static readonly List<uint> PictureIndex = new List<uint>() { ");
            foreach (KeyValuePair<string, string> keyValuePair in Smiles.Pictures)
            {
                int index = Emoji.Dict.IndexOf(keyValuePair.Value);
                builder.Append(index + ", ");
            }
            builder.Append("};");
            builder.AppendLine();

            this._outText.Text = builder.ToString();*/
        }


        private void ImageBrush_ImageOpened(object sender, RoutedEventArgs e)
        {
            ImageBrush brush = sender as ImageBrush;
            BitmapImage img = brush.ImageSource as BitmapImage;
            int steps = img.PixelHeight / img.PixelWidth;

            for(int i=0;i<steps;i++)
            {
                /*
                 * <DiscreteObjectKeyFrame Value="-80" KeyTime="0:0:0.3"/>
                 */
                DiscreteObjectKeyFrame frame = new DiscreteObjectKeyFrame();
                frame.Value = -(this._brd.ActualWidth * i);
                frame.KeyTime = KeyTime.FromTimeSpan( TimeSpan.FromMilliseconds(43 * i) );
                this.objAnim.KeyFrames.Add(frame);
            }
            this.myStoryboard.Begin();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string temp = "<a class=\"wk_vk_link\".*?>(.*?)</a>";
            Regex regex = new Regex(temp);
            TextBox tb = sender as TextBox;
            MatchCollection matches =  regex.Matches(tb.Text);
            StringBuilder builder = new StringBuilder();
            
            foreach (Match match in matches)
            {
                string val = match.Groups[1].Value;
                builder.AppendLine(this.Format(val));
            }

            System.Diagnostics.Debug.WriteLine(matches.Count);

            this._outText.Text = builder.ToString();
        }

        private string Format(string input)
        {
            //{ "D83CDDF0D83CDDFF", "\xD83C\xDDF0\xD83C\xDDFF" }
            string ret = "";
            string code = input;
            while (true)
            {
                string temp = input.Substring(0, 4);
                ret += "\\x";
                ret += temp;
                if (input.Length > 4)
                    input = input.Substring(4, input.Length-4);
                else
                    break;
            }
            return "{ \"" + ret + "\", \"" + code + "\" },";
        }

        private string Format2(string input)
        {
            //_dict.Add("D83DDC69D83DDC69D83DDC67D83DDC67");
            
            return "_dict.Add(\""+ input + "\");";
        }
    }
}
