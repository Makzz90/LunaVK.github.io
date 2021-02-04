using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using LunaVK.Core.Library;

namespace LunaVK.Network.Converters
{
    public class BoolToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (int)((bool)value ? 1 : 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (int)value == 1 ? true : false;
        }
    }
}
