using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using LunaVK.Core.Library;

namespace LunaVK.Network.Converters
{
    public class ThreelenToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //if (value is bool)
            //    return (byte)((bool)value ? 1 : 0);
            
            return (int)((byte)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Enum.Parse(typeof(Threelen), value.ToString(), true);
        }
    }
}
