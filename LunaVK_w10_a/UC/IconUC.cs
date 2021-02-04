using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;

namespace LunaVK.UC
{
    public class IconUC : FontIcon
    {
        public IconUC()
        {
            base.FontFamily = new Windows.UI.Xaml.Media.FontFamily("ms-appx:///Assets/Fonts/segmdl2.ttf#Segoe MDL2 Assets");
        }
    }
}
