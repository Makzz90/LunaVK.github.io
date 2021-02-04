using System;
using Windows.UI.Xaml.Data;

namespace LunaVK.Network.Converters
{
    public class ByteToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToInt32((byte)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            int v = (int)value;
            return System.Convert.ToByte(v);
        }
    }
}
