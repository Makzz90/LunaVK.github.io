using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using LunaVK.Core.DataObjects;
using LunaVK.Core;
using LunaVK.Core.Library;
using LunaVK.Core.Enums;
using LunaVK.Core.Utils;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public class UsersSearchParamsViewModel : GenericCollectionViewModel<VKUser>
    {
        public List<int> AgesTo { get; private set; }
        public List<int> AgesFrom { get; private set; }
        private int _ageToSelected = -1;
        private int _ageFromSelected = -1;
        private readonly Dictionary<string, object> _searchParams = new Dictionary<string, object>();
        public string SearchName { get; set; }

        private List<VKCountry> _countrys;
        public List<VKCountry> Countrys
        {
            get
            {
                if(this._countrys==null)
                {
                    DatabaseService.Instance.GetCountries((result) =>
                    {
                        if (result.error.error_code == VKErrors.None)
                        {
                            this._countrys = result.response.items;
                            base.NotifyPropertyChanged(nameof(this.Countrys));
                            base.NotifyPropertyChanged(nameof(this.CountrySelectorVisibility));
                        }
                    });
                }
                return this._countrys;
            }
        }

        public List<VKCity> Citys { get; private set; }

        public UsersSearchParamsViewModel()
        {
            this.InitAges();
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
        {
            if (offset == 0)
            {
                base._totalCount = null;
                base.NotifyPropertyChanged(nameof(this.UsersFoundCountStr));
            }

            UsersService.Instance.Search(this._searchParams, this.SearchName, offset, count, (result)=>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    if(!string.IsNullOrEmpty( this.SearchName))
                        base._totalCount = result.response.count;
                    callback(result.error, result.response.items);
                }
                else
                    callback(result.error, null);

                base.NotifyPropertyChanged(nameof(this.UsersFoundCountStr));
            });
        }



        private void UpdateAgesTo()
        {
            int age = this._ageFromSelected == -1 ? 14 : this._ageFromSelected;
            this.AgesTo = new List<int>();
            for (int i = age; i <= 80; i++)
            {
                this.AgesTo.Add(i);
            }
            
            base.NotifyPropertyChanged(nameof(this.AgesTo));
            base.NotifyPropertyChanged(nameof(this.AgeToSelected));
        }

        public int AgeFromSelected
        {
            get
            {

                //if (!this.AgesFrom.Contains(this._ageFromSelected))
                //    return this._ageFromSelected = this.AgesFrom.First();
                return this._ageFromSelected;
            }
            set
            {
                this._ageFromSelected = value;
                //base.NotifyPropertyChanged(nameof(this.AgeFromSelected));
                //if (value != -1)
                    this._searchParams["age_from"]= value;
                this.UpdateAgesTo();
                this.NotifyUIProperties();
            }
        }

        public int AgeToSelected
        {
            get
            {


                //if (!this.AgesTo.Contains(this._ageToSelected))
                //    return this._ageToSelected = this.AgesTo.First();
                return this._ageToSelected;
            }
            set
            {
                this._ageToSelected = value;
                //base.NotifyPropertyChanged(nameof(this.AgeToSelected));
                //if (value == null)
                //    return;
                this._searchParams["age_to"]= value;

                this.NotifyUIProperties();
            }
        }
        
        public bool IsWithPhoto
        {
            get
            {
                if (!this._searchParams.ContainsKey("has_photo"))
                    return false;

                return (bool)this._searchParams["has_photo"];
            }
            set
            {
                if (value == false)
                    this._searchParams.Remove("has_photo");
                else
                    this._searchParams["has_photo"] = value;

                this.NotifyUIProperties();
            }
        }

        public bool IsOnlineNow
        {
            get
            {
                if (!this._searchParams.ContainsKey("online"))
                    return false;

                return (bool)this._searchParams["online"];
            }
            set
            {
                if (value == false)
                    this._searchParams.Remove("online");
                else
                    this._searchParams["online"]= value;

                this.NotifyUIProperties();
            }
        }

        public bool IsSafe
        {
            get
            {
                if (!this._searchParams.ContainsKey("safe"))
                    return false;

                return (bool)this._searchParams["safe"];
            }
            set
            {
                if (value == false)
                    this._searchParams.Remove("safe");
                else
                    this._searchParams["safe"] = value;

                this.NotifyPropertyChanged(nameof(this.IsSafe));
                this.NotifyUIProperties();
            }
        }

        private void InitAges()
        {
            this.AgesFrom = new List<int>();
            for (int age = 16; age <= 60; ++age)
                this.AgesFrom.Add(age);
            //this.AgeFromSelected = Enumerable.FirstOrDefault<int>(this.AgesFrom, (a) => a == minAge);
            //base.NotifyPropertyChanged(nameof(this.AgesFrom));
            //base.NotifyPropertyChanged(nameof(this.AgeFromSelected));
            this.UpdateAgesTo();
            //this.AgeToSelected = Enumerable.First<int>(this.AgesTo, (a) => a == maxAge);
        }

        public Visibility AnySetVisibility
        {
            get
            {
                if (this._searchParams.Count==0)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }
        
        public string ParamsStr
        {
            get { return this.ToPrettyString(); }
        }

        public int Sex
        {
            get
            {
                if (!this._searchParams.ContainsKey("sex"))
                    return 0;
                return (int)this._searchParams["sex"];
            }
            set
            {
                this._searchParams["sex"]= value;
                this.UpdateRelationshipTypes();
                this.NotifyUIProperties();
            }
        }

        public int RelationshipType
        {
            get
            {
                if (!this._searchParams.ContainsKey("status"))
                    return -1;
                return (int)this._searchParams["status"];
            }
            set
            {
                if(value==-1)
                {
                    this._searchParams.Remove("status");
                }
                else
                {
                    this._searchParams["status"] = value;
                }
                
                base.NotifyPropertyChanged(nameof(this.RelationshipType));
                this.NotifyUIProperties();
            }
        }

        public int SortType
        {
            get
            {
                if (!this._searchParams.ContainsKey("sort"))
                    return -1;
                return (int)this._searchParams["sort"];
            }
            set
            {
                if (value == -1)
                {
                    this._searchParams.Remove("sort");
                }
                else
                {
                    this._searchParams["sort"] = value;
                }

                base.NotifyPropertyChanged(nameof(this.SortType));
                this.NotifyUIProperties();
            }
        }

        public int GroupType
        {
            get
            {
                if (!this._searchParams.ContainsKey("group_type"))
                    return -1;
                return (int)this._searchParams["group_type"];
            }
            set
            {
                if (value == -1)
                {
                    this._searchParams.Remove("group_type");
                }
                else
                {
                    this._searchParams["group_type"] = value;
                }

                base.NotifyPropertyChanged(nameof(this.GroupType));
                this.NotifyUIProperties();
            }
        }

        public void Clear()
        {
            this._searchParams.Clear();
            this._ageFromSelected = -1;
            this._ageToSelected = -1;
            this.Country = null;
            this.NotifyUIProperties();
        }

        private void NotifyUIProperties()
        {
            this.NotifyPropertyChanged(nameof(this.ParamsStr));
            this.NotifyPropertyChanged(nameof(this.AnySetVisibility));

            this.NotifyPropertyChanged(nameof(this.IsWithPhoto));
            this.NotifyPropertyChanged(nameof(this.IsOnlineNow));
            this.NotifyPropertyChanged(nameof(this.RelationshipType));
            this.NotifyPropertyChanged(nameof(this.AgeFromSelected));
            this.NotifyPropertyChanged(nameof(this.AgeToSelected));
            this.NotifyPropertyChanged(nameof(this.Sex));
        }

        private void UpdateRelationshipTypes()
        {
            int relTypeId = this.RelationshipType;
            base.NotifyPropertyChanged(nameof(this.RelationshipTypes));

            if(relTypeId!=-1)
                this.RelationshipType = relTypeId;
            //base.NotifyPropertyChanged(nameof(this.RelationshipType));
        }

        private string ToPrettyString()
        {
            List<string> stringList = new List<string>();
            if(_searchParams.ContainsKey("sex"))
            {
                int sex1 = (int)_searchParams["sex"];
                string sex = "";
                switch (sex1)
                {
                    case 0:
                        {
                            sex = "Sex_Any";
                            break;
                        }
                    case 1:
                        {
                            sex = "Sex_Female";
                            break;
                        }
                    case 2:
                        {
                            sex = "Sex_Male";
                            break;
                        }
                }
                stringList.Add(LocalizedStrings.GetString(sex));
            }

            if (_searchParams.ContainsKey("age_from"))
            {
                int number1 = (int)_searchParams["age_from"];
                stringList.Add(string.Format(LocalizedStrings.GetString("OneFromAgeFrm"), number1));
            }

            if (_searchParams.ContainsKey("age_to"))
            {
                int number1 = (int)_searchParams["age_to"];
                stringList.Add(string.Format(LocalizedStrings.GetString("OneToAgeFrm"), number1));
            }

            if (_searchParams.ContainsKey("status"))
            {
                int status = (int)_searchParams["status"];
                stringList.Add(this.RelationshipTypes[status]);
            }

            if (_searchParams.ContainsKey("country"))
            {
                int country = (int)_searchParams["country"];
                stringList.Add(this.Countrys[country].title);
            }

            if (_searchParams.ContainsKey("city"))
            {
                int city = (int)_searchParams["city"];
                stringList.Add(this.Citys[city].title);
            }
            
            if (_searchParams.ContainsKey("has_photo"))
            {
                stringList.Add(LocalizedStrings.GetString("UsersSearch_WithPhoto/Content"));
            }

            if (_searchParams.ContainsKey("online"))
            {
                stringList.Add(LocalizedStrings.GetString("UsersSearch_OnlineNow/Content"));
            }
            
            return string.Join(", ", stringList)/*.Capitalize()*/;
        }

        public List<string> RelationshipTypes
        {
            get
            {
                List<string> bgTypeList1 = new List<string>();
                bgTypeList1.Add("Не выбрано");
                if (this.IsFemale)
                {
                    bgTypeList1.Add(LocalizedStrings.GetString("ProfilePage_Info_NotMarriedFemale"));
                    bgTypeList1.Add(LocalizedStrings.GetString("ProfilePage_Info_InARelationship"));
                    bgTypeList1.Add(LocalizedStrings.GetString("ProfilePage_Info_EngagedFemale"));
                    bgTypeList1.Add(LocalizedStrings.GetString("ProfilePage_Info_MarriedFemale"));
                    bgTypeList1.Add(LocalizedStrings.GetString("ProfilePage_Info_ItIsComplicated"));
                    bgTypeList1.Add(LocalizedStrings.GetString("ProfilePage_Info_ActivelySearching"));
                    bgTypeList1.Add(LocalizedStrings.GetString("ProfilePage_Info_InLoveFemale"));
                    bgTypeList1.Add(LocalizedStrings.GetString("InCivilUnion"));
                }
                else
                {
                    bgTypeList1.Add(LocalizedStrings.GetString("ProfilePage_Info_NotMarriedMale"));
                    bgTypeList1.Add(LocalizedStrings.GetString("ProfilePage_Info_InARelationship"));
                    bgTypeList1.Add(LocalizedStrings.GetString("ProfilePage_Info_EngagedMale"));
                    bgTypeList1.Add(LocalizedStrings.GetString("ProfilePage_Info_MarriedMale"));
                    bgTypeList1.Add(LocalizedStrings.GetString("ProfilePage_Info_ItIsComplicated"));
                    bgTypeList1.Add(LocalizedStrings.GetString("ProfilePage_Info_ActivelySearching"));
                    bgTypeList1.Add(LocalizedStrings.GetString("ProfilePage_Info_InLoveMale"));
                    bgTypeList1.Add(LocalizedStrings.GetString("InCivilUnion"));
                }
                return bgTypeList1;
            }
        }

        public bool IsFemale
        {
            get
            {
                return this.Sex == 1;
            }
            set
            {
                if (!value)
                    return;
                this.Sex = 1;
            }
        }
        
        public string UsersFoundCountStr
        {
            get
            {
                //if (!this._searchParamsViewModel.SearchParams.IsAnySet)
                //    return CommonResources.UsersSearch_OnlineNow.ToUpperInvariant();
                if (!this._totalCount.HasValue || (string.IsNullOrEmpty(this.SearchName) && this._searchParams.Count==0 ))
                    return "";

                return UIStringFormatterHelper.FormatNumberOfSomething((int)this._totalCount, "UsersSearch_OnePersonFoundFrm", "UsersSearch_TwoFourPersonsFoundFrm", "UsersSearch_FivePersonsFoundFrm").ToUpperInvariant();
            }
        }

        public VKCountry Country
        {
            get
            {
                if (!this._searchParams.ContainsKey("country"))
                    return null;
                int country = (int)this._searchParams["country"];
                return this.Countrys[country];
            }
            set
            {
                if (value == null)
                {
                    this._searchParams.Remove("country");
                }
                else
                {
                    this._searchParams["country"] = this.Countrys.IndexOf(value);
                }

                base.NotifyPropertyChanged(nameof(this.Country));
                this.City = null;
                this.Citys = null;
                base.NotifyPropertyChanged(nameof(this.CitySelectorVisibility));
                base.NotifyPropertyChanged(nameof(this.Citys));
                this.NotifyUIProperties();

                if (value != null)
                {
                    DatabaseService.Instance.GetCities("", value.id, false, 0, 30, (result) =>
                    {
                        if (result.error.error_code == VKErrors.None)
                        {
                            this.Citys = result.response.items;
                            base.NotifyPropertyChanged(nameof(this.Citys));
                            base.NotifyPropertyChanged(nameof(this.CitySelectorVisibility));
                            this.NotifyUIProperties();
                        }
                    });
                }
            }
        }

        public bool CitySelectorVisibility
        {
            get { return this.Country != null && this.Citys != null; }
        }

        public bool CountrySelectorVisibility
        {
            get { return this._countrys!=null; }
        }

        public VKCity City
        {
            get
            {
                if (!this._searchParams.ContainsKey("city"))
                    return null;
                int city = (int)this._searchParams["city"];
                return this.Citys[city];
            }
            set
            {
                if (value == null)
                {
                    this._searchParams.Remove("city");
                }
                else
                {
                    this._searchParams["city"] = this.Citys.IndexOf(value);
                }
                base.NotifyPropertyChanged(nameof(this.City));
                this.NotifyUIProperties();
            }
        }
    }
}
