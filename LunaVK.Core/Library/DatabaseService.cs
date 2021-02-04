using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.Library
{
    public class DatabaseService
    {
        private static DatabaseService _instance;
        private VKCountedItemsObject<VKCountry> _cachedCountries;

        public static DatabaseService Instance
        {
            get
            {
                if (DatabaseService._instance == null)
                    DatabaseService._instance = new DatabaseService();
                return DatabaseService._instance;
            }
        }

        
        public void GetNearbyCountries(Action<VKResponse<VKCountedItemsObject<VKCountry>>> callback)//используется при регистрации юзера
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["need_all"] = "0";
            parameters["count"] = "500";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKCountry>>("database.getCountries", parameters, callback);
        }
        
        public void GetCountries(Action<VKResponse<VKCountedItemsObject<VKCountry>>> callback)
        {
            if (this._cachedCountries != null)
                callback(new VKResponse<VKCountedItemsObject<VKCountry>>() { error = new VKError(), response = this._cachedCountries });
            else
                this.GetCountriesList(callback);
        }

        private void GetCountriesList(Action<VKResponse<VKCountedItemsObject<VKCountry>>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKCountry>>("database.getCountries", new Dictionary<string, string>(), (result) =>
            {
                if (result.error.error_code == Enums.VKErrors.None)
                {
                    this._cachedCountries = result.response;
                }
                callback(result);
            });
        }

        public void GetCities(string q, uint countryId, bool needAll, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKCity>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if(!string.IsNullOrEmpty(q))
                parameters["q"] = q;
            parameters["country_id"] = countryId.ToString();
            if (needAll)
                parameters["need_all"] = "1";
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKCity>>("database.getCities", parameters, callback);
        }
    }
}
