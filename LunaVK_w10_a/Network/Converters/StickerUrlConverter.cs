using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace LunaVK.Network.Converters
{
    public class StickerUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //Windows.UI.Xaml.Media.Imaging.BitmapImage i = new Windows.UI.Xaml.Media.Imaging.BitmapImage()

            return "https://vk.com/images/stickers/"+value+"/128.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
