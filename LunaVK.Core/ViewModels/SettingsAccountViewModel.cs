using System;
using System.Collections.Generic;
using System.Text;
using LunaVK.Core.Network;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Library;
using LunaVK.Core.Framework;

namespace LunaVK.Core.ViewModels
{
    //SettingsEditProfileViewModel
    public class SettingsAccountViewModel : ViewModelBase
    {
        private SettingsAccountInfo _settingsAccountInfo;

        public string PhoneNumberStr {
            get
            {
                string str = this._settingsAccountInfo == null ? " " : (string.IsNullOrEmpty(this._settingsAccountInfo.Account.phone) ? LocalizedStrings.GetString("SettingsAccount_SetUp") : this._settingsAccountInfo.Account.phone);
                if (this._settingsAccountInfo != null && this._settingsAccountInfo.Account.phone_status == "waiting")
                    str = str + " (" + LocalizedStrings.GetString("SettingsAccount_PhoneNumberWaiting") + ")";
                return str;
            }
        }

        public string EmailStr
        {
            get
            {
                string str = this._settingsAccountInfo == null ? " " : (string.IsNullOrEmpty(this._settingsAccountInfo.Account.email) ? LocalizedStrings.GetString("SettingsAccount_SetUp") : this._settingsAccountInfo.Account.email);
                if (this._settingsAccountInfo != null && this._settingsAccountInfo.Account.email_status == "need_confirmation")
                    str = str + " (" + LocalizedStrings.GetString("SettingsAccount_EmailNeedsConfirmation") + ")";
                return str;
            }
        }

        public string ShortNameStr
        {
            get
            {
                if (this._settingsAccountInfo != null && !string.IsNullOrEmpty(this._settingsAccountInfo.ProfileInfo.screen_name))
                    return "@" + this._settingsAccountInfo.ProfileInfo.screen_name;
                return LocalizedStrings.GetString("SettingsAccount_SetUp");
            }
        }


        public int ShowByDefaultType
        {
            get
            {
                if (this._settingsAccountInfo == null)
                    return 0;

                return this._settingsAccountInfo.Account.own_posts_default;
            }
        }
        
        public bool PostComments
        {
            get
            {
                if (this._settingsAccountInfo == null)
                    return false;

                return this._settingsAccountInfo.Account.no_wall_replies == 0;
            }
        }

        public string FirstName
        {
            get
            {
                if (this._settingsAccountInfo == null)
                    return "";

                return this._settingsAccountInfo.ProfileInfo.first_name;
            }
        }

        public string LastName
        {
            get
            {
                if (this._settingsAccountInfo == null)
                    return "";

                return this._settingsAccountInfo.ProfileInfo.last_name;
            }
        }

        public bool IsMale
        {
            get
            {
                if (this._settingsAccountInfo == null)
                    return false;

                return this._settingsAccountInfo.ProfileInfo.sex == 2;
            }
        }

        public bool IsFemale
        {
            get
            {
                if (this._settingsAccountInfo == null)
                    return false;

                return this._settingsAccountInfo.ProfileInfo.sex == 1;
            }
        }

        public string BirthDateStr
        {
            get
            {
                if (this._settingsAccountInfo == null)
                    return "";

                return this._settingsAccountInfo.ProfileInfo.bdate;
            }
        }
        public int BirthdayShowType
        {
            get
            {
                if (this._settingsAccountInfo == null)
                    return 0;

                return this._settingsAccountInfo.ProfileInfo.bdate_visibility;
            }
        }

        public int RelationshipType
        {
            get
            {
                if (this._settingsAccountInfo == null)
                    return 0;

                return this._settingsAccountInfo.ProfileInfo.relation;
            }
        }
        public string Country
        {
            get
            {
                if (this._settingsAccountInfo == null || this._settingsAccountInfo.ProfileInfo.country == null)
                    return LocalizedStrings.GetString( "Settings_EditProfile_NoneSelected");

                return this._settingsAccountInfo.ProfileInfo.country.title;
            }
        }
        public string City
        {
            get
            {
                if (this._settingsAccountInfo == null || this._settingsAccountInfo.ProfileInfo.city == null)
                    return LocalizedStrings.GetString("Settings_EditProfile_NoneSelected");

                return this._settingsAccountInfo.ProfileInfo.city.title;
            }
        }

        public string AvatarUri
        {
            get { return Settings.LoggedInUserPhoto; }
        }

        public void LoadData()
        {
#if DEBUG
            RequestsDispatcher.GetResponseFromDump<SettingsAccountInfo>( 1000, "accountSettingsTest.json", (result) =>
#else
            AccountService.Instance.GetSettingsAccountInfo((result) =>
#endif
            {
                if (result.error.error_code == Enums.VKErrors.None)
                {
                    this._settingsAccountInfo = result.response;
                    
                    base.NotifyPropertyChanged("PhoneNumberStr");
                    base.NotifyPropertyChanged("EmailStr");
                    base.NotifyPropertyChanged("ShortNameStr");
                    base.NotifyPropertyChanged("ShowByDefaultType");
                    base.NotifyPropertyChanged("PostComments");

                    base.NotifyPropertyChanged("FirstName");
                    base.NotifyPropertyChanged("LastName");
                    base.NotifyPropertyChanged("IsMale");
                    base.NotifyPropertyChanged("IsFemale");
                    base.NotifyPropertyChanged("BirthDateStr");
                    base.NotifyPropertyChanged("BirthdayShowType");
                    base.NotifyPropertyChanged("RelationshipType");
                    base.NotifyPropertyChanged("Country");
                    base.NotifyPropertyChanged("City");
                }
            });
            
        }

        public string NewsFilterDescStr
        {
            get
            {
                //int hiddenSourcesCount = this._hiddenSourcesCount;
                //if (hiddenSourcesCount > 0)
                //    return CommonResources.SettingsAccount_NotShown + ": " + hiddenSourcesCount.ToString();
                return " ";
            }
        }

        public void DeletePhoto()
        {
            this.SetInProgress(true);
            AccountService.Instance.DeleteProfilePhoto((result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    this.SetInProgress(false);
                    if (result.error.error_code != Enums.VKErrors.None)
                        return;

                    //if (AppGlobalStateManager.Current.GlobalState.LoggedInUser == null)
                    //    return;
                    //AppGlobalStateManager.Current.GlobalState.LoggedInUser.photo_max = res.ResultData.photo_max;
                    //EventAggregator.Current.Publish(new BaseDataChangedEvent()
                    //{
                    //    IsProfileUpdateRequired = true
                    //});
                    Settings.LoggedInUserPhoto = result.response.photo_max;
                    base.NotifyPropertyChanged(nameof(this.AvatarUri));


                });
            });
        }

        public void HandlePhoneNumberTap()
        {
            if (this._settingsAccountInfo == null || string.IsNullOrEmpty(this._settingsAccountInfo.Account.change_phone_url))
                return;
            //NavigatorImpl.Current.NavigationToValidationPage(this._settingsAccountInfo.Account.change_phone_url);
        }

        public void HandleEmailTap()
        {
            if (this._settingsAccountInfo == null || string.IsNullOrEmpty(this._settingsAccountInfo.Account.change_email_url))
                return;
            //NavigatorImpl.Current.NavigationToValidationPage(this._settingsAccountInfo.Account.change_email_url);
        }
    }
}
