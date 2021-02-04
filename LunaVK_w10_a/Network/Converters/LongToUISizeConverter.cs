using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using LunaVK.Core.Utils;

namespace LunaVK.Network.Converters
{
    public class LongToUISizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //todo:localize
            if (value is ulong)
            {
                double size = (double)System.Convert.ToDouble(value);
                return UIStringFormatterHelper.BytesForUI(size);
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
