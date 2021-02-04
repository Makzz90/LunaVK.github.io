using LunaVK.Core.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace LunaVK.UC.Attachment
{
    public class MapPointSmallAttachmentUC : StackPanel
    {
        
        public MapPointSmallAttachmentUC(VKGeo geo)
        {
            base.Children.Clear();
            base.Margin = new Windows.UI.Xaml.Thickness(5);
            base.Orientation = Orientation.Horizontal;

            IconUC icon = new IconUC();
            icon.Glyph = "\xEB49";
            icon.Margin = new Windows.UI.Xaml.Thickness(5, 0, 5, 0);



            string str = "";

            if (geo.place != null)
            {
                if (!string.IsNullOrEmpty(geo.place.title))
                {
                    string[] strArray = geo.place.title.Split(',');
                    if (strArray.Length != 0)
                        str = strArray[0];
                }
                if (!string.IsNullOrEmpty(geo.place.city) && geo.place.city != str)
                {
                    if (!string.IsNullOrEmpty(str))
                        str += ", ";
                    str += geo.place.city;
                }
                else if (!string.IsNullOrEmpty(geo.place.country) && geo.place.country != str)
                {
                    if (!string.IsNullOrEmpty(str))
                        str += ", ";
                    str += geo.place.country;
                }
            }





                TextBlock tb = new TextBlock();
            tb.Text = str;





            base.Children.Add(icon);
            base.Children.Add(tb);
        }
    }
}
