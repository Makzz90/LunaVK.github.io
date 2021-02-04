using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace LunaVK.Network.Converters
{
    public class TimeSpanToUIStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //if (!(value is TimeSpan))
            //    throw new ArgumentException();

            int durationSeconds = (int)value;

            if (durationSeconds < 3600)
                return TimeSpan.FromSeconds(durationSeconds).ToString("m\\:ss");
            return TimeSpan.FromSeconds(durationSeconds).ToString("h\\:mm\\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
