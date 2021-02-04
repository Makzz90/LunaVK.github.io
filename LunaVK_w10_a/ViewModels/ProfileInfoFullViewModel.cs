using LunaVK.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.ViewModels
{
    public class ProfileInfoFullViewModel
    {
        public ObservableCollection<ProfileInfoItem> InfoSections { get; private set; }

        public ProfileInfoFullViewModel()
        {
            this.InfoSections = new ObservableCollection<ProfileInfoItem>();
        }
    }
    
    public abstract class ProfileInfoItem
    {
        /// <summary>
        /// Название секции, например, контакты/образование
        /// </summary>
        public string Key { get; private set; }

//        public string Icon { get; private set; }

        public string Data { get; private set; }

//        public string Title { get; private set; }

        public ProfileInfoItemType Type { get; private set; }

        //        public string photo;
        //        public BitmapImage GroupImage
        //        {
        //            get
        //            {
        //                if (string.IsNullOrEmpty(this.photo))
        //                    return null;

        //                return new BitmapImage(new Uri(this.photo));
        //            }
        //        }



        //        public List<string> Previews { get; set; }

        public Action NavigationAction;



        public ProfileInfoItem(string key, string data, ProfileInfoItemType type = ProfileInfoItemType.IconRichText)
        {
            if(!string.IsNullOrEmpty(key))
                this.Key = LocalizedStrings.GetString(key);
            this.Data = data;
            this.Type = type;
        }

    }

    public class CustomProfileInfoItem : ProfileInfoItem
    {
        public string Icon { get; private set; }

        public CustomProfileInfoItem(string icon, string data)
            : base("", data, ProfileInfoItemType.IconRichText)
        {
            this.Icon = icon;
        }
    }

    public class CustomProfileInfoItemTitled : ProfileInfoItem
    {
        public string Title { get; private set; }

        public CustomProfileInfoItemTitled(string title, string data)
            : base("", data, ProfileInfoItemType.TitleSubtitle)
        {
            this.Title = LocalizedStrings.GetString( title);
        }
    }

    public class InfoListItem : ProfileInfoItem
    {
        public string Icon { get; private set; }

        public IReadOnlyList<string> Previews { get; set; }

        public InfoListItem(string icon, string data)
            : base("", data, ProfileInfoItemType.IconTextPreviews)
        {
            this.Icon = icon;
        }
    }

    public class LinkItem : ProfileInfoItem
    {
        public string Title { get; private set; }
        private string _photo;
        public BitmapImage GroupImage
        {
            get
            {
                if (string.IsNullOrEmpty(this._photo))
                    return null;

                return new BitmapImage(new Uri(this._photo));
            }
        }

        public LinkItem(string title, string data, string photo) :base("",data, ProfileInfoItemType.TitleSubtitleImage)
        {
            this.Title = title;
            this._photo = photo;
        }
    }


    public enum ProfileInfoItemType
    {
        IconSimpleText,
        IconRichText,
        TitleSubtitle,
        TitleSubtitleImage,
        IconTextPreviews,

        //RichText,
        //SimpleText,
        //Full,
        //Contact,
        //Previews
    }
}
