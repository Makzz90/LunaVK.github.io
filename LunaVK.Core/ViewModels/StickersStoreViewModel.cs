using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;

namespace LunaVK.Core.ViewModels
{
    public class StickersStoreViewModel
    {
        public ObservableCollection<StoreCatalog.StoreBanner> Banners { get; set; }
        public ObservableCollection<StoreCatalog.StoreSection> Sections { get; set; }


        //bool InLoading = false;

        public StickersStoreViewModel()
        {
            this.Banners = new ObservableCollection<StoreCatalog.StoreBanner>();
            this.Sections = new ObservableCollection<StoreCatalog.StoreSection>();
        }

        public async void LoadData(bool reload = false, Action<bool> calback = null)
        {

            if (reload)
            {
                //offset = 0;
            }


            //List<StoreProductFilter> l = new List<StoreProductFilter>();
            //l.Add(StoreProductFilter.Free);
            //l.Add(StoreProductFilter.New);
            //l.Add(StoreProductFilter.Promoted);

            var temp = await StoreService.Instance.GetStickersStoreCatalog();


            if (temp == null)
            {
                calback?.Invoke(false);


                return;
            }

            foreach (var banner in temp.banners)
            {
                this.Banners.Add(banner);
            }

            foreach(var section in temp.sections)
            {
                this.Sections.Add(section);
            }
            }
    }
}
