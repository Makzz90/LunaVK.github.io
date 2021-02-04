using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public class VideosSearchViewModel : GenericSearchViewModelBase<object>
    {
        private readonly Dictionary<string, object> _searchParams = new Dictionary<string, object>();

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<object>> callback)
        {
            if (offset == 0)
                base.SetInProgress(true);

            VideoService.Instance.Search(this._searchParams, this._searchString, offset, count, (result) =>
            {
                base.SetInProgress(false);
                if (result.error.error_code == VKErrors.None)
                {
                    base._totalCount = result.response.count;
                    callback(result.error, result.response.items);
                }
                else
                {
                    callback(result.error, null);
                }
            });
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

        public bool IsHD
        {
            get
            {
                if (!this._searchParams.ContainsKey("hd"))
                    return false;

                return (bool)this._searchParams["hd"];
            }
            set
            {
                if (value == false)
                    this._searchParams.Remove("hd");
                else
                    this._searchParams["hd"] = value;

                this.NotifyUIProperties();
            }
        }

        public bool IsAdult
        {
            get
            {
                if (!this._searchParams.ContainsKey("adult"))
                    return false;

                return (bool)this._searchParams["adult"];
            }
            set
            {
                if (value == false)
                    this._searchParams.Remove("adult");
                else
                    this._searchParams["adult"] = value;

                this.NotifyUIProperties();
            }
        }

        public bool IsOnlyOwn
        {
            get
            {
                if (!this._searchParams.ContainsKey("search_own"))
                    return false;

                return (bool)this._searchParams["search_own"];
            }
            set
            {
                if (value == false)
                    this._searchParams.Remove("search_own");
                else
                    this._searchParams["search_own"] = value;

                this.NotifyUIProperties();
            }
        }

        public bool IsMP4
        {
            get
            {
                if (!this._searchParams.ContainsKey("is_mp4"))
                    return false;

                return (bool)this._searchParams["is_mp4"];
            }
            set
            {
                if (value == false)
                    this._searchParams.Remove("is_mp4");
                else
                    this._searchParams["is_mp4"] = value;

                this.NotifyUIProperties();
            }
        }

        public bool IsYouTuBe
        {
            get
            {
                if (!this._searchParams.ContainsKey("is_youtube"))
                    return false;

                return (bool)this._searchParams["is_youtube"];
            }
            set
            {
                if (value == false)
                    this._searchParams.Remove("is_youtube");
                else
                    this._searchParams["is_youtube"] = value;

                this.NotifyUIProperties();
            }
        }

        public bool IsVimeo
        {
            get
            {
                if (!this._searchParams.ContainsKey("is_vimeo"))
                    return false;

                return (bool)this._searchParams["is_vimeo"];
            }
            set
            {
                if (value == false)
                    this._searchParams.Remove("is_vimeo");
                else
                    this._searchParams["is_vimeo"] = value;

                this.NotifyUIProperties();
            }
        }

        public bool IsShort
        {
            get
            {
                if (!this._searchParams.ContainsKey("is_short"))
                    return false;

                return (bool)this._searchParams["is_short"];
            }
            set
            {
                if (value == false)
                    this._searchParams.Remove("is_short");
                else
                    this._searchParams["is_short"] = value;

                this.NotifyUIProperties();
            }
        }

        public bool IsLong
        {
            get
            {
                if (!this._searchParams.ContainsKey("is_long"))
                    return false;

                return (bool)this._searchParams["is_long"];
            }
            set
            {
                if (value == false)
                    this._searchParams.Remove("is_long");
                else
                    this._searchParams["is_long"] = value;

                this.NotifyUIProperties();
            }
        }

        public string ParamsStr
        {
            get { return this.ToPrettyString(); }
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

        private string ToPrettyString()
        {
            List<string> stringList = new List<string>();
            /*
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
                int country_id = (int)_searchParams["country"];
                stringList.Add(this.Countrys.First((c) => c.id == country_id).title);
            }

            if (_searchParams.ContainsKey("city"))
            {
                int city_id = (int)_searchParams["city"];
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
            */
            return string.Join(", ", stringList);
        }

        public void Clear()
        {
            base.Items.Clear();
            this._searchParams.Clear();
            this.NotifyUIProperties();
        }

        private void NotifyUIProperties()
        {
            this.NotifyPropertyChanged(nameof(this.ParamsStr));
            this.NotifyPropertyChanged(nameof(this.AnySetVisibility));
            this.NotifyPropertyChanged(nameof(this.SortType));
            this.NotifyPropertyChanged(nameof(this.IsHD));
            this.NotifyPropertyChanged(nameof(this.IsAdult));
            this.NotifyPropertyChanged(nameof(this.IsMP4));
            this.NotifyPropertyChanged(nameof(this.IsYouTuBe));
            this.NotifyPropertyChanged(nameof(this.IsVimeo));
            this.NotifyPropertyChanged(nameof(this.IsShort));
            this.NotifyPropertyChanged(nameof(this.IsLong));
            this.NotifyPropertyChanged(nameof(this.IsOnlyOwn));
        }

        public Visibility AlbumsVisible
        {
            get { return Visibility.Collapsed; }
        }

        public int AlbumsCount
        {
            get { return 0; }
        }

        public int VideosCount
        {
            get { return 0; }
        }

        public object AlbumsVM { get; private set; }

        private string _searchString;
        public override string SearchString
        {
            get
            {
                return this._searchString;
            }
            set
            {
                if (this._searchString == value)
                    return;
                this._searchString = value;
                base.Reload();
            }
        }
    }
}
