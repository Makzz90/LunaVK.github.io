using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace LunaVK.UC
{
    public sealed partial class SearchParamsUserUC : UserControl
    {
        List<VKCountry> Countries;

        public SearchParamsUserUC()
        {
            this.InitializeComponent();
            this.Loaded += SearchParamsUserUC_Loaded;
        }

        private void SearchParamsUserUC_Loaded(object sender, RoutedEventArgs e)
        {
            for(int i=14;i<=80;i++)
            {
                this._ageFromComboBox.Items.Add("от "+i);
                this._ageToComboBox.Items.Add("до " + i);
            }
            //this._countryComboBox.Items.Add("Country");

            this.GetNearbyCountries((result) => {
                this._countryComboBox.Items.Clear();

                if (result == null)
                    return;

                this.Countries = result;

                foreach (var city in result)
                {
                    this._countryComboBox.Items.Add(city.title);
                }
                //this._countryComboBox.SelectedIndex = 0;
            });
        }

        private void GetNearbyCountries(Action<List<VKCountry>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["need_all"] = "0";
            parameters["count"] = "500";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKCountry>>("database.getCountries", parameters,(result)=> {
                if (result.error.error_code == Core.Enums.VKErrors.None)
                    callback(result.response.items);
                else
                    callback(null);
            });
        }

        public void GetCities(/*string q,*/ int countryId, bool needAll, Action<List<VKCity>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            //parameters["q"] = q;
            parameters["country_id"] = countryId.ToString();
            if (needAll)
                parameters["need_all"] = "1";
            //parameters["offset"] = offset.ToString();
            parameters["count"] = "30";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKCity>>("database.getCities", parameters, (result) =>
            {
                if (result.error.error_code == Core.Enums.VKErrors.None)
                    callback(result.response.items);
                else
                    callback(null);
            });
        }

        private void _countryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Countries == null)
                return;

            ComboBox cb = sender as ComboBox;
            //if (cb.SelectedIndex == 0)
            //    return;

            VKCountry country = this.Countries[cb.SelectedIndex];



            this.GetCities(country.id,false,(result) => {

                this._cityComboBox.Items.Clear();

                if (result == null)
                    return;


                foreach (var city in result)
                {
                    this._cityComboBox.Items.Add(city.title);
                }
                this._cityComboBox.SelectedIndex = 0;
            });
        }
    }
}
