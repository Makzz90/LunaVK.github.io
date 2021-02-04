using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using LunaVK.Core;

namespace LunaVK.Network.Converters
{
    public class ForUIShortTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is DateTime))
                throw new ArgumentException();

            string result="";

            DateTime given = ((DateTime)value).ToLocalTime();
            result = FormatDateForUIShort(given);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private string FormatDateForUIShort(DateTime dateTime)
        {
            DateTime now = DateTime.Now;
            DateTime dateTime1 = new DateTime(now.Year, now.Month, now.Day);
            DateTime dateTime2 = new DateTime(now.Year, 1, 1);
            if (dateTime.Year == now.Year && dateTime.Month == now.Month && dateTime.Day == now.Day)
                return dateTime.ToString("HH:mm");
            DateTime dateTime3 = dateTime.AddDays(-1.0);
            DateTime dateTime4 = now.AddDays(-1.0);
            if (dateTime3.Year == dateTime4.Year && dateTime3.Month == dateTime4.Month && dateTime3.Day == dateTime4.Day)
                return LocalizedStrings.GetString("Yesterday");
            if (dateTime.Year == now.Year)
                return dateTime.ToString("dd.MM");
            return dateTime.ToString("dd.MM.yy");
        }
    }
}
