using System;
using Windows.UI.Xaml.Data;
using LunaVK.Core.Utils;

namespace LunaVK.Network.Converters
{
    public class UIStringTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is DateTime))
                throw new ArgumentException();

            DateTime given = (DateTime)value;//((DateTime)value).ToLocalTime();
            string result = UIStringFormatterHelper.FormatDateTimeForUIShort(given);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
