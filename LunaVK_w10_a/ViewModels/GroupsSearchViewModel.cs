using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public class GroupsSearchViewModel : GenericCollectionViewModel<VKGroup>, IBinarySerializable
    {
        private readonly Dictionary<string, object> _searchParams = new Dictionary<string, object>();

        public string q = string.Empty;
        
        public bool InvitationsVisible
        {
            get
            {
                return false;
            }
        }
        
        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKGroup>> callback)
        {
            GroupsService.Instance.Search(this.q, offset, count, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    base._totalCount = result.response.count;
                    callback(result.error, result.response.items);
                }
                else
                {
                    callback(result.error, null);
                }
            }, this._searchParams);
        }

        private List<VKCountry> _countrys;
        public List<VKCountry> Countrys
        {
            get
            {
                if (this._countrys == null)
                {
                    //CacheManager.TryDeserialize(this, "Countrys", false);
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

        public Visibility AnySetVisibility
        {
            get
            {
                if (this._searchParams.Count == 0)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public bool CitySelectorVisibility
        {
            get
            {
                return this.Country != null && this.Citys != null;
            }
        }

        public VKCountry Country
        {
            get
            {
                if (!this._searchParams.ContainsKey("country"))
                    return null;
                uint country = (uint)this._searchParams["country"];
                return this.Countrys[(int)country];
            }
            set
            {
                if (value == null)
                {
                    this._searchParams.Remove("country");
                }
                else
                {
                    this._searchParams["country"] = (uint)this.Countrys.IndexOf(value);
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

        public List<VKCity> Citys { get; private set; }

        public VKCity City
        {
            get
            {
                if (!this._searchParams.ContainsKey("city"))
                    return null;
                uint city = (uint)this._searchParams["city"];
                return this.Citys[(int)city];
            }
            set
            {
                if (value == null)
                {
                    this._searchParams.Remove("city");
                }
                else
                {
                    this._searchParams["city"] = (uint)this.Citys.IndexOf(value);
                }
                base.NotifyPropertyChanged(nameof(this.City));
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
                base.NotifyPropertyChanged(nameof(this.FutureVisibility));
                this.NotifyUIProperties();
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
                int sex1 = (int)_searchParams["group_type"];
                string result = "";
                switch (sex1)
                {
                    case 0:
                        {
                            result = "Group";
                            break;
                        }
                    case 1:
                        {
                            result = "Page";
                            break;
                        }
                    case 2:
                        {
                            //result = "Event";
                            if (this._searchParams.ContainsKey("future"))
                            {
                                result="Предстоящие мероприятие";
                            }
                            else
                            {
                                result = "Мероприятие";
                            }
                          
                            break;
                        }
                }
                stringList.Add(result);
            }

            if (_searchParams.ContainsKey("country"))
            {
                uint country = (uint)_searchParams["country"];
                stringList.Add(this.Countrys[(int)country].title);
            }

            if (_searchParams.ContainsKey("city"))
            {
                uint city = (uint)_searchParams["city"];
                stringList.Add(this.Citys[(int)city].title);
            }

            

            return string.Join(", ", stringList);
        }

        private void NotifyUIProperties()
        {
            this.NotifyPropertyChanged(nameof(this.ParamsStr));
            this.NotifyPropertyChanged(nameof(this.AnySetVisibility));

            
        }

        public void Clear()
        {
            this._searchParams.Clear();
            this.Country = null;
            
            base.NotifyPropertyChanged(nameof(this.SortType));
            base.NotifyPropertyChanged(nameof(this.GroupType));
            base.NotifyPropertyChanged(nameof(this.IsFuture));
            this.NotifyUIProperties();
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

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.WriteList<VKCountry>(this._countrys);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this._countrys = reader.ReadList<VKCountry>();
        }
    }
}
