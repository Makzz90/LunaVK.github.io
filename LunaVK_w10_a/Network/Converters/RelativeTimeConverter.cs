using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using LunaVK.Core.Utils;

namespace LunaVK.Network.Converters
{
    /// <summary>
    /// Time converter to display elapsed time relatively to the present.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public class RelativeTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return UIStringFormatterHelper.FormatDateTimeForUI((DateTime)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
