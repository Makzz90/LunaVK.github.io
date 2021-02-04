using System;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.Common
{
    public class ImageInfo
    {
        public string Uri { get; set; }

        public Func<bool, BitmapSource> GetSourceFunc { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }
    }
}
