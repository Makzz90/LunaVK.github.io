using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using LunaVK.Core.ViewModels;
using LunaVK.Library;
using LunaVK.UC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LunaVK.ViewModels
{
    public class ProfilePhotosViewModel : ViewModelBase, ProfileMediaViewModelFacade.IMediaHorizontalItemsViewModel
    {
        public ObservableCollection<VKPhoto> Items { get; private set; }
        private uint _count;
        private int _owner;

        public ProfilePhotosViewModel(int ownerId)
        {
            this.Items = new ObservableCollection<VKPhoto>();
            this._owner = ownerId;
        }

        public string Title
        {
            get { return LocalizedStrings.GetString("Menu_Photos"); }
        }

        public string Count
        {
            get
            {
                if (this._count == 0)
                    return "";
                return this._count.ToString();
            }
        }
       
        public void HeaderTapAction()
        {
                    //this.PublishProfileBlockClickEvent(ProfileBlockType.photos);
                    NavigatorImpl.Instance.NavigateToPhotoAlbums(this._owner,"");
        }
        /*
       public Action<MediaListItemViewModelBase> ItemTapAction
       {
           get
           {
               return (Action<MediaListItemViewModelBase>)(vm =>
               {
                   List<Photo> list = (List<Photo>)Enumerable.ToList<Photo>(Enumerable.Select<MediaListItemViewModelBase, Photo>(this._items, (item => ((PhotoMediaListItemViewModel)item).Photo)));
                   int val1 = this._items.IndexOf(vm);
                   this.PublishProfileBlockClickEvent(ProfileBlockType.photos);
                   ImageViewerDecoratorUC.ShowPhotosFromProfile(this._profileData.id, this._isGroup, Math.Max(val1, 0), list, true);
               });
           }
       }
       */

        public void ItemTapAction(object item)
        {
            List<VKPhoto> list = this.Items.ToList();
            int val1 = this.Items.IndexOf(item as VKPhoto);
            ImageViewerDecoratorUC.ShowPhotosFromProfile(this._owner, Math.Max(val1, 0), list, this._count/* true*/);
            ///Navigator.Current.NavigateToPhotoAlbums(false, this._profileData.id, this._isGroup, this._profileData.admin_level);

        }

        public void Init(object data)
        {
            this.Items.Clear();

            if (data is VKCountedItemsObject<VKPhoto> c)
            {
                this._count = c.count;
                foreach (var item in c.items)
                    this.Items.Add(item);

                base.NotifyPropertyChanged(nameof(this.Count));
            }
            else
            {
                throw new Exception("type failed");
            }
        }
    }
}
