using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using LunaVK.Core.ViewModels;
using LunaVK.Library;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.ViewModels
{
    public class ProfileMarketViewModel : ViewModelBase, ProfileMediaViewModelFacade.IMediaHorizontalItemsViewModel
    {
        public ObservableCollection<VKMarketItem> Items { get; private set; }
        private uint _count;
        private int _owner;

        public ProfileMarketViewModel(int ownerId)
        {
            this.Items = new ObservableCollection<VKMarketItem>();
            this._owner = ownerId;
        }

        public string Title
        {
            get { return LocalizedStrings.GetString("Menu_Market/Content"); }
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
            NavigatorImpl.Instance.NavigateToMarket(this._owner);
        }

        public void ItemTapAction(object item)
        {
            VKMarketItem listItemViewModel = item as VKMarketItem;
            NavigatorImpl.Instance.NavigateToProduct(listItemViewModel.owner_id, listItemViewModel.id);
        }

        public void Init(object data)
        {
            if (data is VKCountedItemsObject<VKMarketItem> c)
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
