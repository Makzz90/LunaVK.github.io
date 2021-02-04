using LunaVK.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.Common
{
    public class ImageStore
    {
        public static BitmapImage GetStockIcon(string path)
        {
            BitmapImage r = null;
            Execute.ExecuteOnUIThread(()=>
            {
                BitmapImage bitmapImage = new BitmapImage();
                //bitmapImage.set_CreateOptions((BitmapCreateOptions)0);
                bitmapImage.UriSource=new Uri(path, UriKind.Relative);
                r = bitmapImage;
            });
            
            return r;
        }
    }
}
