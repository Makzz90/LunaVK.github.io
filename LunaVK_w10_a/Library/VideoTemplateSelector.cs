using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace LunaVK.Library
{
    public class VideoTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var vm = item as Core.Library.VideoService.VideoCatalogCategory.VideoCatalogItem;
            
            return vm.type == "video" ? this.VideoTemplate : this.VideoAlbumTemplate;
        }

        public DataTemplate VideoAlbumTemplate { get; set; }

        public DataTemplate VideoTemplate { get; set; }
    }
}
