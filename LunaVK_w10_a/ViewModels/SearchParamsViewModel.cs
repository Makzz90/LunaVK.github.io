using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Xaml;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public class SearchParamsViewModel
    {
        public SearchParamsViewModel()
        {
            this.FastVM = new MenuSearchViewModel();
            this.UsersVM = new UsersSearchParamsViewModel();
            this.GroupsVM = new GroupsSearchParamsViewModel();
            this.PhotosVM = new PhotosSearchParamsViewModel();
        }

        public MenuSearchViewModel FastVM { get; set; }
        public UsersSearchParamsViewModel UsersVM { get; set; }
        public GroupsSearchParamsViewModel GroupsVM { get; set; }
        public PhotosSearchParamsViewModel PhotosVM { get; set; }
        

        private string _searchName;
        public string SearchName
        {
            get
            {
                return this._searchName;
            }
            set
            {
                this._searchName = value;
                this.FastVM.SearchName = value;
                this.UsersVM.SearchName = value;
                this.GroupsVM.SearchName = value;
                this.PhotosVM.SearchName = value;
            }
        }



        public class UsersSearchParamsViewModel : GenericCollectionViewModel<VKUser>
        {
            private readonly Dictionary<string, object> _searchParams = new Dictionary<string, object>();
            public string SearchName;
            public List<int> AgesTo { get; private set; }
            public List<int> AgesFrom { get; private set; }
            private int _ageToSelected = -1;
            private int _ageFromSelected = -1;

            public UsersSearchParamsViewModel()
            {
                this.InitAges();
            }

            public string FoundCountStr
            {
                get
                {
                    if (!this._totalCount.HasValue || (string.IsNullOrEmpty(this.SearchName) && this._searchParams.Count == 0))
                        return "";

                    return UIStringFormatterHelper.FormatNumberOfSomething((int)this._totalCount, "UsersSearch_OnePersonFoundFrm", "UsersSearch_TwoFourPersonsFoundFrm", "UsersSearch_FivePersonsFoundFrm").ToUpperInvariant();
                }
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
            {
                if (offset == 0)
                {
                    base._totalCount = null;
                    base.NotifyPropertyChanged(nameof(this.FoundCountStr));
                }

                UsersService.Instance.Search(this._searchParams, this.SearchName, offset, count, (result) =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        if (!string.IsNullOrEmpty(this.SearchName))
                            base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                        callback(result.error, null);

                    base.NotifyPropertyChanged(nameof(this.FoundCountStr));
                });
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

            private List<VKCountry> _countrys;
            public List<VKCountry> Countrys
            {
                get
                {
                    if (this._countrys == null)
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

            public Visibility AnySetVisibility
            {
                get
                {
                    if (this._searchParams.Count == 0)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }

            public string ParamsStr
            {
                get { return this.ToPrettyString(); }
            }

            private string ToPrettyString()
            {
                List<string> stringList = new List<string>();
                if (this._searchParams.ContainsKey("sex"))
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
                    uint country_id = (uint)_searchParams["country"];
                    stringList.Add(this.Countrys.First((c)=>c.id== country_id).title);
                }

                if (_searchParams.ContainsKey("city"))
                {
                    uint city_id = (uint)_searchParams["city"];
                    stringList.Add(this.Citys.First((c) => c.id == city_id).title);
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
                    this._searchParams["sex"] = value;
                    this.UpdateRelationshipTypes();
                    this.NotifyUIProperties();
                }
            }

            public bool CitySelectorVisibility
            {
                get { return this.Country != null && this.Citys != null; }
            }

            public bool CountrySelectorVisibility
            {
                get { return this._countrys != null; }
            }

            public VKCountry Country
            {
                get
                {
                    if (!this._searchParams.ContainsKey("country"))
                        return null;
                    uint country_id = (uint)this._searchParams["country"];
                    return this.Countrys.First((c)=>c.id== country_id);
                }
                set
                {
                    if (value == null)
                    {
                        this._searchParams.Remove("country");
                    }
                    else
                    {
                        int pos = this.Countrys.IndexOf(value);
                        this._searchParams["country"] = this.Countrys[pos].id;
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

            public VKCity City
            {
                get
                {
                    if (!this._searchParams.ContainsKey("city"))
                        return null;
                    uint city_id = (uint)this._searchParams["city"];
                    return this.Citys.First((c) => c.id == city_id);
                }
                set
                {
                    if (value == null)
                    {
                        this._searchParams.Remove("city");
                    }
                    else
                    {
                        int pos = this.Citys.IndexOf(value);
                        this._searchParams["city"] = this.Citys[pos].id;
                    }
                    base.NotifyPropertyChanged(nameof(this.City));
                    this.NotifyUIProperties();
                }
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
                        this._searchParams["online"] = value;

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
                    if (value == -1)
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
                    this._searchParams["age_from"] = value;
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
                    this._searchParams["age_to"] = value;

                    this.NotifyUIProperties();
                }
            }

            private void UpdateRelationshipTypes()
            {
                int relTypeId = this.RelationshipType;
                base.NotifyPropertyChanged(nameof(this.RelationshipTypes));

                if (relTypeId != -1)
                    this.RelationshipType = relTypeId;
                //base.NotifyPropertyChanged(nameof(this.RelationshipType));
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

            public void Clear()
            {
                base.Items.Clear();
                //
                this._searchParams.Clear();
                this._ageFromSelected = -1;
                this._ageToSelected = -1;
                this.Country = null;
                this.NotifyUIProperties();
            }
        }

        public class GroupsSearchParamsViewModel : GenericCollectionViewModel<VKGroup>
        {
            private readonly Dictionary<string, object> _searchParams = new Dictionary<string, object>();
            public string SearchName;

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKGroup>> callback)
            {
                if (offset == 0)
                {
                    base._totalCount = null;
                    base.NotifyPropertyChanged(nameof(this.FoundCountStr));
                }

                GroupsService.Instance.Search(this.SearchName, offset, count, (result) =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        //if (!string.IsNullOrEmpty(this.SearchName))
                            base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                        callback(result.error, null);

                    base.NotifyPropertyChanged(nameof(this.FoundCountStr));
                }, this._searchParams);
            }

            public Visibility AnySetVisibility
            {
                get
                {
                    if (this._searchParams.Count == 0)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }

            public string ParamsStr
            {
                get { return this.ToPrettyString(); }
            }

            private string ToPrettyString()
            {
                List<string> stringList = new List<string>();
                if (_searchParams.ContainsKey("group_type"))
                {
                    int type1 = (int)_searchParams["group_type"];
                    string typeStr = "";
                    switch (type1)
                    {
                        case 0:
                            {
                                typeStr = "Group";
                                break;
                            }
                        case 1:
                            {
                                typeStr = "Page";
                                break;
                            }
                        case 2:
                            {
                                //typeStr = "Event";
                                if (this._searchParams.ContainsKey("future"))
                                {
                                    typeStr = "Предстоящие мероприятие";
                                }
                                else
                                {
                                    typeStr = "Мероприятие";
                                }
                                break;
                            }
                    }
                    stringList.Add(typeStr);
                }

                if (_searchParams.ContainsKey("country"))
                {
                    uint country_id = (uint)_searchParams["country"];
                    stringList.Add(this.Countrys.First((c) => c.id == country_id).title);
                }

                if (_searchParams.ContainsKey("city"))
                {
                    uint city_id = (uint)_searchParams["city"];
                    stringList.Add(this.Citys.First((c) => c.id == city_id).title);
                }

                return string.Join(", ", stringList);
            }

            private List<VKCountry> _countrys;
            public List<VKCountry> Countrys
            {
                get
                {
                    if (this._countrys == null)
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

            public bool CountrySelectorVisibility
            {
                get { return this._countrys != null; }
            }

            public VKCountry Country
            {
                get
                {
                    if (!this._searchParams.ContainsKey("country"))
                        return null;
                    uint country_id = (uint)this._searchParams["country"];
                    return this.Countrys.First((c) => c.id == country_id);
                }
                set
                {
                    if (value == null)
                    {
                        this._searchParams.Remove("country");
                    }
                    else
                    {
                        int pos = this.Countrys.IndexOf(value);
                        this._searchParams["country"] = this.Countrys[pos].id;
                    }

                    base.NotifyPropertyChanged(nameof(this.Country));
                    this.City = null;
                    this.Citys = null;
                    base.NotifyPropertyChanged(nameof(this.CitySelectorVisibility));
                    base.NotifyPropertyChanged(nameof(this.Citys));
                    //this.NotifyUIProperties();

                    if (value != null)
                    {
                        DatabaseService.Instance.GetCities("", value.id, false, 0, 30, (result) =>
                        {
                            if (result.error.error_code == VKErrors.None)
                            {
                                this.Citys = result.response.items;
                                base.NotifyPropertyChanged(nameof(this.Citys));
                                base.NotifyPropertyChanged(nameof(this.CitySelectorVisibility));
                                //this.NotifyUIProperties();
                            }
                        });
                    }
                }
            }


            public VKCity City
            {
                get
                {
                    if (!this._searchParams.ContainsKey("city"))
                        return null;
                    uint city_id = (uint)this._searchParams["city"];
                    return this.Citys.First((c) => c.id == city_id);
                }
                set
                {
                    if (value == null)
                    {
                        this._searchParams.Remove("city");
                    }
                    else
                    {
                        int pos = this.Citys.IndexOf(value);
                        this._searchParams["city"] = this.Citys[pos].id;
                    }
                    //base.NotifyPropertyChanged(nameof(this.City));
                    this.NotifyUIProperties();
                }
            }

            public List<VKCity> Citys { get; private set; }

            public bool CitySelectorVisibility
            {
                get { return this.Country != null && this.Citys != null; }
            }

            public string FoundCountStr
            {
                get
                {
                    if (!this._totalCount.HasValue || (string.IsNullOrEmpty(this.SearchName) && this._searchParams.Count == 0))
                        return "";

                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneGroupFrm", "TwoFourGroupsFrm", "FiveMoreGroupsFrm").ToUpperInvariant();
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

                    if (value != 2)
                    {
                        this.IsFuture = false;
                        base.NotifyPropertyChanged(nameof(this.IsFuture));
                    }

                    base.NotifyPropertyChanged(nameof(this.GroupType));
                    base.NotifyPropertyChanged(nameof(this.FutureVisibility));
                    this.NotifyUIProperties();
                }
            }

            public bool IsFuture
            {
                get
                {
                    if (!this._searchParams.ContainsKey("future"))
                        return false;

                    return (bool)this._searchParams["future"];
                }
                set
                {
                    if (value == false)
                        this._searchParams.Remove("future");
                    else
                        this._searchParams["future"] = value;

                    this.NotifyUIProperties();
                }
            }

            public Visibility FutureVisibility
            {
                get
                {
                    return (this.GroupType == 2).ToVisiblity();
                }
            }

            private void NotifyUIProperties()
            {
                this.NotifyPropertyChanged(nameof(this.ParamsStr));
                this.NotifyPropertyChanged(nameof(this.AnySetVisibility));

                //this.NotifyPropertyChanged(nameof(this.IsWithPhoto));
                //this.NotifyPropertyChanged(nameof(this.IsOnlineNow));
                //this.NotifyPropertyChanged(nameof(this.RelationshipType));
                //this.NotifyPropertyChanged(nameof(this.AgeFromSelected));
                //this.NotifyPropertyChanged(nameof(this.AgeToSelected));
                //this.NotifyPropertyChanged(nameof(this.Sex));
            }

            public void Clear()
            {
                base.Items.Clear();
                //
                this._searchParams.Clear();
                this.Country = null;

                base.NotifyPropertyChanged(nameof(this.SortType));
                base.NotifyPropertyChanged(nameof(this.GroupType));
                base.NotifyPropertyChanged(nameof(this.IsFuture));
                this.NotifyUIProperties();
            }
        }

        public class PhotosSearchParamsViewModel : GenericCollectionViewModel<VKPhoto>
        {
            private readonly Dictionary<string, object> _searchParams = new Dictionary<string, object>();
            public string SearchName;

            public PhotosSearchParamsViewModel()
            {
                base.ReloadCount = 60;
                base.LoadCount = 50;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKPhoto>> callback)
            {
                if (offset == 0)
                {
                    base._totalCount = null;
                    base.NotifyPropertyChanged(nameof(this.FoundCountStr));
                }

                PhotosService.Instance.Search(this.SearchName, offset, count, (result) =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        //if (!string.IsNullOrEmpty(this.SearchName))
                        base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                        callback(result.error, null);

                    base.NotifyPropertyChanged(nameof(this.FoundCountStr));
                }, this._searchParams);
            }

            public Visibility AnySetVisibility
            {
                get
                {
                    if (this._searchParams.Count == 0)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }


            public string ParamsStr
            {
                get { return ""; }
            }

            public void Clear()
            {
                base.Items.Clear();
                //
                this._searchParams.Clear();
                //this.Country = null;
                //this.NotifyUIProperties();
            }

            public string FoundCountStr
            {
                get
                {
                    if (!this._totalCount.HasValue || (string.IsNullOrEmpty(this.SearchName) && this._searchParams.Count == 0))
                        return "";

                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OnePhoto", "TwoFourPhotosFrm", "FiveOrMorePhotosFrm").ToUpperInvariant();
                }
            }
        }
    }
}
