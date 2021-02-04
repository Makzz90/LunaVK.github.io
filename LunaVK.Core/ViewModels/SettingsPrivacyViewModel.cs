using System;
using System.Collections.Generic;
using LunaVK.Core.Library;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using System.Linq;
using Windows.UI.Xaml;
using LunaVK.Core.Utils;
using LunaVK.Core.DataObjects;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LunaVK.Core.Network;

namespace LunaVK.Core.ViewModels
{
    public class SettingsPrivacyViewModel : GenericCollectionViewModel<PrivacySettingItem>
    {
        /// <summary>
        /// Моя страница, Записи на странице
        /// </summary>
        public static List<PrivacySection> sections;

        /// <summary>
        /// Друзья и друзья друзей, Все пользователи
        /// </summary>
        public static List<PrivacySettingsInfo.ValueTitle> supported_categories;

        public static List<FriendsList> lists;


        public ObservableGroupingCollection<PrivacySettingItem> GroupedItems { get; private set; }

        public SettingsPrivacyViewModel()
        {
            this.GroupedItems = new ObservableGroupingCollection<PrivacySettingItem>(base.Items);
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<PrivacySettingItem>> callback)
        {
            AccountService.Instance.GetPrivacySettings((result)=>
            {
                if(result.error.error_code == VKErrors.None)
                {
                    SettingsPrivacyViewModel.sections = result.response.sections;
                    SettingsPrivacyViewModel.supported_categories = result.response.supported_categories;
                    SettingsPrivacyViewModel.lists = result.response.lists.items;

                    base._totalCount = (uint)result.response.sections.Count;
                    //callback(result.error, result.response.settings);

                    List<PrivacySettingItem> items = new List<PrivacySettingItem>();
                    foreach(PrivacySetting setting in result.response.settings)
                    {
                        string keyString = "";
                        /*
                        var ss = result.response.sections.Find((s) => s.name == setting.section);
                        if (ss != null)
                            keyString = ss.title;
                        else
                        {
#if DEBUG
                            keyString = setting.section;                        
#endif
                            continue;//БАГ в апи: онлайн секция? ШТА?
                        }
                        */
                        if(setting.key == "online")
                        {
                            setting.title = "Видимость моего онлайна";
                        }

                        PrivacySettingItem item = new PrivacySettingItem(setting);

                        if (setting.value.owners != null)
                            item.Profiles = result.response.profiles;

                        if (setting.value.lists != null && setting.value.lists.allowed != null)
                        {
                            setting.value.category = "some";
                        }
                        else if (setting.value.owners != null && setting.value.owners.allowed != null)
                        {
                            setting.value.category = "some";
                        }

                        items.Add(item);
                    }
                    callback(result.error, items);
                }
                else
                {
                    callback(result.error, null);
                }
            });

        }        
    }

    public class PrivacySettingItem : PrivacySetting, ISupportGroup, INotifyPropertyChanged
    {
#region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.RaisePropertyChanged(propertyName);
        }

        private void RaisePropertyChanged(string property)
        {
            if (this.PropertyChanged == null)
                return;
            //Надо вызывать на ветке интерфейса
            Execute.ExecuteOnUIThread(() =>
            {
                if (this.PropertyChanged == null)
                    return;//В оригинале есть эта перепроверка
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            });
        }
#endregion

        public PrivacySettingItem(PrivacySetting data)
        {
            base.type = data.type;
            base.title = data.title;
            base.key = data.key;
            base.supported_categories = data.supported_categories;
            base.value = data.value;
            base.section = data.section;
        }

        public string Key
        {
            get
            {
                var temp = SettingsPrivacyViewModel.sections.Find((s) => s.name == base.section);
                if (temp == null)
                    return base.section;

                return temp.title;
            }
        }

        public void RefreshUI()
        {
            this.NotifyPropertyChanged(nameof(this.UserFriendlyDesc));
            this.NotifyPropertyChanged(nameof(this.PrivateVisibility));
            this.NotifyPropertyChanged(nameof(this.value));
        }

        public string UserFriendlyDesc { get { return this.BuildUserFriendlyDesc(); } }

        private string BuildUserFriendlyDesc()
        {
            string lowerInvariant = "";
            
            var temp = SettingsPrivacyViewModel.supported_categories.Find((s) => s.value == base.value.category);
            if (temp == null)
                temp = SettingsPrivacyViewModel.supported_categories.First();

            lowerInvariant = temp.title;
            string str2 = "";
            if (base.value.lists != null)
            {
                if (base.value.lists.allowed!=null)
                {
                    lowerInvariant += ":";
                    var source1 = base.value.lists.allowed.Select((l)=> SettingsPrivacyViewModel.lists.Find((sl)=>sl.id==l).name);
                    str2 = source1.ToList().GetCommaSeparated(", ");
                }
                else if (base.value.lists.excluded != null)
                {
                    lowerInvariant += ",";
                    var source2 = base.value.lists.excluded.Select((l) => SettingsPrivacyViewModel.lists.Find((sl) => sl.id == l).name);
                    str2 = LocalizedStrings.GetString("Privacy_Excluding") + " " + source2.ToList().GetCommaSeparated(", ");
                }
            }
            string str3 = "";
            if(base.value.owners!=null)
            {
                if (base.value.owners.allowed != null)
                {
                    lowerInvariant += ":";
                    var source1 = base.value.owners.allowed.Select((l) => this.Profiles.Find((sl) => sl.id == l).Title);
                    str3 = source1.ToList().GetCommaSeparated(", ");
                }
                else if (base.value.owners.excluded != null)
                {
                    lowerInvariant += ",";
                    var source2 = base.value.owners.excluded.Select((l) => this.Profiles.Find((sl) => sl.id == l).Title);
                    str3 = LocalizedStrings.GetString("Privacy_Excluding") + " " + source2.ToList().GetCommaSeparated(", ");
                }
            }
            
            return string.Join(" ", new List<string>() { lowerInvariant, str2, str3 }.Where((s => !string.IsNullOrEmpty(s))));
        }

        public Visibility PrivateVisibility
        {
            get { return (base.value.category == "only_me" || base.value.category == "friends_of_friends" || base.value.category == "nobody" || base.value.category == "some").ToVisiblity(); }
        }

        public string Description
        {
            get
            {
                var temp = SettingsPrivacyViewModel.sections.Find((ss) => ss.name == base.section);
                if (temp == null)
                    return "";
                return temp.description;
            }
        }

        public List<VKUser> Profiles;
    }
}
