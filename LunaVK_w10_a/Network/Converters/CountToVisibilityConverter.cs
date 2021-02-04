using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;

namespace LunaVK.Network.Converters
{
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is int) && !(value is uint))
                return Visibility.Visible;
            int result = 0;
            if (parameter is string)
                int.TryParse(parameter.ToString(), out result);
            int num = int.Parse(value.ToString());//(int)value;
            //if (this.IsInverted)
            //    return (Visibility)(num > result ? 1 : 0);
            return (Visibility)(num > result ? 0 : 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
