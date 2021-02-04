using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LunaVK.ViewModels
{
    public class StickersManageViewModel
    {
        private bool _updatingCollection;

        public ObservableCollection<StockItem> ActiveStickers { get; private set; }

        public ObservableCollection<StockItem> HiddenStickers { get; private set; }

        public StickersManageViewModel()
        {
            this.ActiveStickers = new ObservableCollection<StockItem>();
            this.HiddenStickers = new ObservableCollection<StockItem>();

            this.ActiveStickers.CollectionChanged += ActiveStickers_OnCollectionChanged;
        }

        private void ActiveStickers_OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (this._updatingCollection || e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                return;
            int before = 0;
            int after = 0;
            int index = e.NewStartingIndex;
            int id = this.ActiveStickers[index].product.id;
            if (index > 0)
                after = this.ActiveStickers[index - 1].product.id;
            if (index < this.ActiveStickers.Count - 1)
                before = this.ActiveStickers[index + 1].product.id;
            //this.SetProgress(true);
            StoreService.Instance.ReorderProducts(id, after, before, (result) =>
            {
                //this.SetProgress(false);
                //if (result.ResultCode != ResultCode.Succeeded)
                //    GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", (VKRequestsDispatcher.Error)null);
                ///else
                //    EventAggregator.Current.Publish(new StickersPacksReorderedEvent(((Collection<StockItemHeader>)this.ActiveStickers)[index].StockItem, index));
            });
        }

        private void AddActiveStickers(StockItem stockItemHeader)
        {
            this._updatingCollection = true;
            this.ActiveStickers.Add(stockItemHeader);
            this._updatingCollection = false;
        }

        public void Load()
        {
            List<StoreProductFilter> productFilters = new List<StoreProductFilter>();
            productFilters.Add(StoreProductFilter.Purchased);
            
            StoreService.Instance.GetStockItems(productFilters, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        //this.ClearItems();
                        foreach (StockItem stockItem in result.response.items)
                        {
                            if (stockItem.product.active)
                                this.AddActiveStickers(stockItem);
                            else
                                this.HiddenStickers.Add(stockItem);
                        }
                        //this.NotifyProperties();
                    });
                }
                //callback(resultCode);
            });
        }

        public void Activate(StockItem stockItemHeader)
        {
            //this.SetProgress(true);
            //StorePurchaseManager.ActivateStickersPack(stockItemHeader, (Action<bool>)(activated => this.SetProgress(false)));
            StoreService.Instance.ActivateProduct(stockItemHeader.product.id, (result) =>
            {
                if (result.error.error_code == VKErrors.None && result.response == 1)
                {
                    stockItemHeader.product.active = true;
                    this.ActivateDeactivate(stockItemHeader.product.id, true);
                    //EventAggregator.Current.Publish(new StickersPackActivatedDeactivatedEvent(stockItemHeader, true));
                }
                //else
                //    GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
            });
        }

        public void Deactivate(StockItem stockItemHeader)
        {
            //this.SetProgress(true);
            //StorePurchaseManager.DeactivateStickersPack(stockItemHeader, (Action<bool>)(deactivated => this.SetProgress(false)));
            StoreService.Instance.DeactivateProduct(stockItemHeader.product.id, (result) =>
            {
                if (result.error.error_code == VKErrors.None && result.response == 1)
                {
                    stockItemHeader.product.active = false;
                    this.ActivateDeactivate(stockItemHeader.product.id, false);
                    //EventAggregator.Current.Publish(new StickersPackActivatedDeactivatedEvent(stockItemHeader, false));
                }
                //else
                //    GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
            });
        }

        private void ActivateDeactivate(int productId, bool activate)
        {
            Execute.ExecuteOnUIThread(() =>
            {
                StockItem stockItemHeader = activate ? this.HiddenStickers.FirstOrDefault((item => item.product.id == productId)) : this.ActiveStickers.FirstOrDefault((item => item.product.id == productId));
                if (stockItemHeader == null)
                    return;
                if (activate)
                {
                    this.HiddenStickers.Remove(stockItemHeader);
                    this.AddActiveStickers(stockItemHeader);
                }
                else
                {
                    this.ActiveStickers.Remove(stockItemHeader);
                    this.HiddenStickers.Insert(0, stockItemHeader);
                }
                //this.NotifyProperties();
            });
        }
    }
}
