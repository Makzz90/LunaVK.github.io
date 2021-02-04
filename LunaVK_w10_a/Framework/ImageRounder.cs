using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using System.Net.Http;
using Windows.UI.Xaml.Media;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media.Imaging;
using LunaVK.Core;

namespace LunaVK.Framework
{
    public static class ImageRounder
    {
        public static void LetsRound(this Border target)
        {
            double RadiusY = Settings.RoundAvatar * target.Width / 100.0 / 2.0;
            target.CornerRadius = new CornerRadius(RadiusY);
        }
    }
}
