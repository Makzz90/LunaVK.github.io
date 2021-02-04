using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using LunaVK.Core.DataObjects;
using System.Collections.Generic;

namespace LunaVK.Library
{
    public class MusicTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is List<VKPlaylist>)
            {
                return this.AlbumTemplate;
            }
            else if (item is VKBaseDataForGroupOrUser)
            {
                return this.ArtistTemplate;
            }
            return this.TrackTemplate;
        }

        public DataTemplate ArtistTemplate { get; set; }

        public DataTemplate TrackTemplate { get; set; }

        public DataTemplate AlbumTemplate { get; set; }
    }

    //public interface IMusicItem { }
}
