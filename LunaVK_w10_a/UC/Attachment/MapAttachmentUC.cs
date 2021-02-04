using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
using LunaVK.Core.DataObjects;
using Windows.Devices.Geolocation;

namespace LunaVK.UC.Attachment
{
    public class MapAttachmentUC : Grid
    {
#if WINDOWS_PHONE_APP || WINDOWS_UWP
        Windows.UI.Xaml.Controls.Maps.MapControl map;
#else
        Bing.Maps.Map map;
#endif

        public MapAttachmentUC()
        {
#if WINDOWS_PHONE_APP || WINDOWS_UWP
            map = new Windows.UI.Xaml.Controls.Maps.MapControl();
            try
            {
                map.MapServiceToken = "ArEQxjMYhiZPLLAh74-gaBNvbvv9GPxCFKqv-7i9HHaHmxet0TMb6wheIxUB4Rd5";
            }
            catch
            {

            }
            
           // map.MapTapped += map_MapTapped;
#else
            map = new Bing.Maps.Map();
            map.Credentials = "AmOFJiHtBeytoES_ZZxm9yiLe8hYQaaC7GUO-Fi-yX7RhhZ8n7pyn-17I9SyDlG7";
            //map.Tapped += map_Tapped;
#endif
        }

        public MapAttachmentUC(VKGeo geo)
            :this()
        {
            //Bing.Maps
            //Windows.UI.Xaml.Controls.Maps.MapControl map = new Windows.UI.Xaml.Controls.Maps.MapControl();
            int i = 0;
        }

        public MapAttachmentUC(VKGeoInMsg geo)
            : this()
        {
            map.Width = 300;
            map.Height = 200;

            BasicGeoposition b = new BasicGeoposition() { Latitude = geo.coordinates.latitude, Longitude = geo.coordinates.longitude };
            var point = new Geopoint(b);
#if WINDOWS_PHONE_APP
            map.Center = point;
#endif
            map.ZoomLevel = 16.0;

            base.Children.Add(map);
            int i = 0;
        }
    }
}
