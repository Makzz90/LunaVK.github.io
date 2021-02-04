using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.Library
{
    public class ProfileMediaListItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate GenericTemplate { get; set; }

        public DataTemplate PhotoAlbumTemplate { get; set; }

        public DataTemplate VideoAlbumTemplate { get; set; }

        public DataTemplate SubscriptionsTemplate { get; set; }

        public DataTemplate PhotoTemplate { get; set; }

        public DataTemplate VideoTemplate { get; set; }

        public DataTemplate AudioTemplate { get; set; }

        public DataTemplate DiscussionsTemplate { get; set; }

        public DataTemplate ProductTemplate { get; set; }

        public DataTemplate GiftsTemplate { get; set; }

        public DataTemplate EmptyDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            /*
            MediaListSectionViewModel itemViewModelBase = item as MediaListSectionViewModel;
            if (itemViewModelBase == null)
                return null;
            switch (itemViewModelBase.Type)
            {
                case ProfileMediaListItemType.Generic:
                    return this.GenericTemplate;
                case ProfileMediaListItemType.PhotoAlbum:
                    return this.PhotoAlbumTemplate;
                case ProfileMediaListItemType.VideoAlbum:
                    return this.VideoAlbumTemplate;
                case ProfileMediaListItemType.Subscriptions:
                    return this.SubscriptionsTemplate;
                case ProfileMediaListItemType.Photo:
                    return this.PhotoTemplate;
                case ProfileMediaListItemType.Video:
                    return this.VideoTemplate;
                case ProfileMediaListItemType.Audio:
                    return this.AudioTemplate;
                case ProfileMediaListItemType.Discussions:
                    return this.DiscussionsTemplate;
                case ProfileMediaListItemType.Product:
                    return this.ProductTemplate;
                case ProfileMediaListItemType.Gifts:
                    return this.GiftsTemplate;
                case ProfileMediaListItemType.EmptyData:
                    return this.EmptyDataTemplate;
                default:
                    return null;
            }*/
            if(item is VKPhoto)
            {
                return this.PhotoTemplate;
            }
            else if(item is VKMarketItem)
            {
                return this.ProductTemplate;
            }

            return null;
        }
    }
}
