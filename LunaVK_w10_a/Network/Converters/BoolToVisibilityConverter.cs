using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;

namespace LunaVK.Network.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Visibility ret = Visibility.Collapsed;

            bool _val;

            _val = (bool)value;

            if (parameter != null)
                _val = !_val;

            if (_val == true)
                ret = Visibility.Visible;
            else
                ret = Visibility.Collapsed;

            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
