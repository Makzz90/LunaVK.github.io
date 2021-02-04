using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;

namespace LunaVK.Core.Utils
{
    public static class ThemeUtils
    {
        public static Style GenerateThemeStyle(string target_type, string brush_name)
        {
            object temp = Windows.UI.Xaml.Markup.XamlReader.Load("<Style xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" TargetType=\""+target_type+"\" > <Setter Property=\"Foreground\" Value=\"{ThemeResource "+brush_name+"}\"/> </Style>");
            return temp as Style;
        }
    }
}
