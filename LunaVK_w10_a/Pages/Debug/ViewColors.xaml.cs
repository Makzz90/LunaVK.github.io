using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.Pages.Debug
{
    
    public sealed partial class ViewColors : Page
    {
        public ObservableCollection<VKColor> Items { get; private set; }

        public ViewColors()
        {
            this.InitializeComponent();

            this.Items = new ObservableCollection<VKColor>();
            base.DataContext = this;

            this.Loaded += ViewColors_Loaded;
        }

        private async void ViewColors_Loaded(object sender, RoutedEventArgs e)
        {
            var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            Windows.Storage.StorageFile file = await folder.GetFileAsync("colors.xml");

            byte[] fileBytes = null;
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }
            string xml = Encoding.UTF8.GetString(fileBytes);//Convert.ToString(fileBytes);


            Regex regexObj = new Regex(@"<color name=""(?<name>[\D\d]+?)"">(?<color>[\D\d]+?)<", RegexOptions.Singleline);
            MatchCollection matches = regexObj.Matches(xml);

            List<VKColor> tempItems = new List<VKColor>();

            foreach (Match m in matches)
            {
                string name = m.Groups["name"].Value;
                string color = m.Groups["color"].Value;
                SolidColorBrush brush = null;
                if(color.StartsWith("#"))
                {
                    string temp = color;

                    byte R = 0, G = 0, B = 0, A = byte.MaxValue;
                    if (temp.Length==4)//#000
                    {
                        char r = color[1];
                        char g = color[2];
                        char b = color[3];

                        temp = string.Format("#{0}{0}{1}{1}{2}{2}",r,g,b);
                    }


                    if(temp.Length==9)//#ff000000
                    {
                        A = Convert.ToByte(temp.Substring(1, 2), 16);
                        R = Convert.ToByte(temp.Substring(3, 2), 16);
                        G = Convert.ToByte(temp.Substring(5, 2), 16);
                        B = Convert.ToByte(temp.Substring(7, 2), 16);
                    }
                    else if (temp.Length == 7)//#7fa87f
                    {
                        R = Convert.ToByte(temp.Substring(1, 2), 16);
                        G = Convert.ToByte(temp.Substring(3, 2), 16);
                        B = Convert.ToByte(temp.Substring(5, 2), 16);
                    }

                    brush = new SolidColorBrush(Color.FromArgb(A, R, G, B));
                }
                VKColor c = new VKColor() { ColorName = name, ColorBrush = brush, ColorText = color };
                tempItems.Add(c);
            }

            int j = 0;
            foreach(var item in tempItems)
            {
                if (item.ColorText.StartsWith("@color/"))
                {
                    string temp = item.ColorText.Substring(7);
                    var c = tempItems.Find((i) => i.ColorName == temp);
                    item.ColorBrush = c.ColorBrush;
                }
                item.Number = j.ToString();
                this.Items.Add(item);
                j++;
            }









            //
            //
            /*
            foreach( var r in Application.Current.Resources)
            {
                VKColor c = new VKColor() { ColorName = r.Key.ToString(), ColorText = r.Value.ToString() };
                this.Items.Add(c);
            }
            */
        }

        public class VKColor
        {
            public string Number { get; set; }

            public string ColorName { get; set; }//window_bg_black

            public SolidColorBrush ColorBrush { get; set; }

            public string ColorText { get; set; }//#7fa87f
        }
    }
}
