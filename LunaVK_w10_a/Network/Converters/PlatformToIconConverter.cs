using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace LunaVK.Network.Converters
{
    public class PlatformToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string str = value as string;
            /*
             * Mobile = 1,

        /// <summary>
        /// Приложение для iPhone.
        /// </summary>
        iPhone,

        /// <summary>
        /// Приложение для iPad.
        /// </summary>
        iPad,

        /// <summary>
        /// Приложение для Android.
        /// </summary>
        Android,

        /// <summary>
        /// Приложение для Windows Phone.
        /// </summary>
        WindowsPhone,

        /// <summary>
        /// Приложение для Windows.
        /// </summary>
        Windows,

        /// <summary>
        /// Полная версия сайта.
        /// </summary>
        Web,//7

        VK_Mobile
             * */
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
