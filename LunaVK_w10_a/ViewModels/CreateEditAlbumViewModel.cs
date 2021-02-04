using LunaVK.Core;
using LunaVK.Core.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
//CreateEditVideoAlbumViewModel
namespace LunaVK.ViewModels
{
    public class CreateEditAlbumViewModel
    {
        private int _albumId;
        public uint _groupId;
        public PrivacyInfo PrivacyView { get; set; }
        public PrivacyInfo PrivacyComment { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<PrivacyInfo> AccessTypes { get; private set; }
        public Visibility DescriptionVisibility { get; set; }

        private CreateEditAlbumViewModel(uint groupId = 0, PrivacyInfo piV = null, PrivacyInfo piC = null)
        {
            this._groupId = groupId;
            this.AccessTypes = new List<PrivacyInfo>() { new PrivacyInfo("all"), new PrivacyInfo("friends"), new PrivacyInfo("friends_of_friends"), new PrivacyInfo("only_me") };
            this.PrivacyView = piV ?? this.AccessTypes.First();
            this.PrivacyComment = piC ?? this.AccessTypes.First();
            this.DescriptionVisibility = Visibility.Visible;
        }

        public CreateEditAlbumViewModel(int albumId = 0, uint groupId = 0, string name = "", string description = "", PrivacyInfo piV = null, PrivacyInfo piC = null) : this(groupId, piV, piC)
        {
            this._albumId = albumId;
            
            this.Name = name;
            this.Description = description;
        }

        public CreateEditAlbumViewModel(VKAlbumPhoto album, uint groupId = 0)
            :this(groupId)
        {
            if(album!=null)
            {
                this._albumId = album.id;
                this.Name = album.title;
                this.Description = album.description;

                if (album.privacy_view != null)
                    this.PrivacyView = this.AccessTypes.Find((p)=>p.ToString() == album.privacy_view.category);
                if (album.privacy_comment != null)
                    this.PrivacyComment = this.AccessTypes.Find((p) => p.ToString() == album.privacy_comment.category);
            }
        }

        public CreateEditAlbumViewModel(VKVideoAlbum album, uint groupId = 0)
            : this(groupId)
        {
            if (album != null)
            {
                this._albumId = album.id;
                this.Name = album.title;
                this.DescriptionVisibility = Visibility.Collapsed;

                if (album.privacy != null)
                    this.PrivacyView = this.AccessTypes.Find((p) => p.ToString() == album.privacy.category);
            }
        }


        public string Caption
        {
            get { return LocalizedStrings.GetString(this._isNewMode ? "CreateAlbumUC_CreateAlbum" : "CreateAlbumUC_EditAlbum"); }
        }

        public string ButtonText
        {
            get { return LocalizedStrings.GetString(this._isNewMode ? "CreateAlbumUC_Create" : "CreateAlbumUC_Save"); }
        }

        private bool _isNewMode { get { return this._albumId == 0; } }

        public Visibility IsUserAlbumVisibility
        {
            get
            {
                if (this._groupId != 0)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility IsGroupAlbumVisibility
        {
            get
            {
                if (this._groupId == 0)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }
    }
}
