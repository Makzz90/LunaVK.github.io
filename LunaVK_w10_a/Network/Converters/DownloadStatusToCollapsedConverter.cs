using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace LunaVK.Network.Converters
{
    public class DownloadStatusToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Visibility ret = Visibility.Visible;

            BackgroundTransferStatus status = (BackgroundTransferStatus)value;
            
            string[] parameters = ((string)parameter).Split('|');
            foreach(var param in parameters)
            {
                BackgroundTransferStatus flag = (BackgroundTransferStatus)Enum.Parse(typeof(BackgroundTransferStatus), param);
                if(flag == status)
                {
                    ret = Visibility.Collapsed;
                    break;
                }
            }
            
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
