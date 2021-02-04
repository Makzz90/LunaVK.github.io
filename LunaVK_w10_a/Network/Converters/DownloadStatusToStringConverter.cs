using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml.Data;

namespace LunaVK.Network.Converters
{
    public class DownloadStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BackgroundTransferStatus status = (BackgroundTransferStatus)value;
            if (status == BackgroundTransferStatus.Running)
                return "Downloading";
            else if (status == BackgroundTransferStatus.Error)
                return "Error";
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
