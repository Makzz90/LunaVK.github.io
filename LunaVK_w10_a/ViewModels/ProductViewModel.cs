using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Library;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public class ProductViewModel : GenericCollectionViewModel<VKComment>
    {
        private int OwnerId;
        private uint ItemId;
        private VKMarketItem _product;
        private VKBaseDataForGroupOrUser _owner;
        public ObservableCollection<string> Photos { get; set; }

        public ProductViewModel(int ownerId, uint itemId)
        {
            this.OwnerId = ownerId;
            this.ItemId = itemId;

            this.Photos = new ObservableCollection<string>();
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKComment>> callback)
        {
            if (this._product == null)
            {
                MarketService.Instance.GetProduct(this.OwnerId, this.ItemId, (result) =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        this._product = result.response.items[0];
                        if(this._product.owner_id>0)
                            this._owner = result.response.profiles[0];
                        else
                            this._owner = result.response.groups[0];

                        callback(result.error, null);
                        base._totalCount = 0;

                        Execute.ExecuteOnUIThread(() =>
                        {
                            if(this._product.photos!= null && this._product.photos.Count>0)
                            {
                                foreach(var photo in this._product.photos)
                                {
                                    this.Photos.Add(photo.photo_320);
                                }
                            }

                            base.NotifyPropertyChanged(nameof(this.NavDotsVisibility));
                            base.NotifyPropertyChanged(nameof(this.IsSlideViewCycled));
                            base.NotifyPropertyChanged(nameof(this.ProductTitle));
                            base.NotifyPropertyChanged(nameof(this.Price));
                            base.NotifyPropertyChanged(nameof(this.Description));
                            base.NotifyPropertyChanged(nameof(this.GroupName));
                            base.NotifyPropertyChanged(nameof(this.Category));
                            base.NotifyPropertyChanged(nameof(this.GroupImage));
                            base.NotifyPropertyChanged(nameof(this.WikiPageName));
                            base.NotifyPropertyChanged(nameof(this.WikiPageVisibility));
                            base.NotifyPropertyChanged(nameof(this.ContactSellerButtonVisibility));
                            base.NotifyPropertyChanged(nameof(this.ProductUnavailableVisibility));
                            base.NotifyPropertyChanged(nameof(this.MetaData));
                        });
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }
        }

        public Visibility NavDotsVisibility
        {
            get { return (this.Photos.Count > 1).ToVisiblity(); }
        }

        public bool IsSlideViewCycled
        {
            get { return this.Photos.Count > 1; }
        }

        public string ProductTitle
        {
            get { return this._product == null ? "" : this._product.title; }
        }

        public string Price
        {
            get { return this._product == null ? "" : this._product.PriceString; }
        }

        public string Description
        {
            get { return this._product == null ? "" : this._product.description; }
        }

        public string GroupName
        {
            get { return this._owner == null ? "" : this._owner.Title; }
        }

        public string Category
        {
            get { return this._product == null ? "" : this._product.CategoryString; }
        }

        public string GroupImage
        {
            get { return this._owner == null ? "" : this._owner.MinPhoto; }
        }

        public string WikiPageName
        {
            get
            {
                if (this._owner == null)
                    return "";

                //if (this._owner.market == null)
                //    return "";

                //if (this._owner.market.wiki == null)
                //    return "";
                return "";
            }
        }

        public Visibility WikiPageVisibility
        {
            get
            {
                return (!string.IsNullOrEmpty(this.WikiPageName)).ToVisiblity();
            }
        }

        public Visibility ContactSellerButtonVisibility
        {
            get { return this._product == null ? Visibility.Collapsed : (this._product.availability == 0).ToVisiblity(); }
        }

        public Visibility ProductUnavailableVisibility
        {
            get { return this._product == null ? Visibility.Collapsed : (this._product.availability > 0).ToVisiblity(); }
        }

        public string MetaData
        {
            get
            {
                if (this._product == null)
                    return "";

                string str = UIStringFormatterHelper.FormateDateForEventUI(this._product.date);
                int viewsCount = this._product.views_count;
                if (viewsCount > 0)
                    str = str + " · " + UIStringFormatterHelper.FormatNumberOfSomething(viewsCount, "OneViewFrm", "TwoFourViewsFrm", "FiveViewsFrm");

                return str;
            }
        }

        public void ContactSeller()
        {
            if(this._owner is VKGroup group)
            {
                if (group.market == null || group.market.contact_id == 0)
                    return;

                NavigatorImpl.Instance.NavigateToConversation(group.market.contact_id);
            }
            else
            {
                NavigatorImpl.Instance.NavigateToConversation(this.OwnerId);
            }
            //EventAggregator.Current.Publish((object)new MarketContactEvent(string.Format("{0}_{1}", (object)this._product.owner_id, (object)this._product.id), MarketContactAction.start));
            //this.PrepareProductForSharing(CommonResources.ContactSellerMessage);
            
        }

        public void NavigateToGroup()
        {
            NavigatorImpl.Instance.NavigateToProfilePage(this.OwnerId);
        }
    }
}
