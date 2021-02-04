using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using LunaVK.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace LunaVK.ViewModels
{
    public class SettingsEditProfileViewModel : ViewModelBase
    {
        /*
        public void Reload()
        {
            this.PageLoadInfoViewModel.LoadingState = PageLoadingState.Loading;
            this.Load((Action)(loaded => Execute.ExecuteOnUIThread((Action)(() => this.PageLoadInfoViewModel.LoadingState = loaded ? PageLoadingState.Loaded : PageLoadingState.LoadingFailed))));
        }
        */
        private ProfileInfo _profileInfo;
        private readonly Dictionary<string, string> _updateDictionary = new Dictionary<string, string>();
        private bool _isLoaded;
        private bool _isSaving;
        private static readonly VKCity _defaultCity;
        private static readonly VKCountry _defaultCountry;

        public void Load()
        {
            AccountService.Instance.GetSettingsProfileInfo((result)=>
            {
                Execute.ExecuteOnUIThread(delegate
                {
                    if (result.error.error_code == Core.Enums.VKErrors.None)
                    {
                        this._isLoaded = true;
                        base.NotifyPropertyChanged(nameof(this.IsLoaded));
                        this._profileInfo = result.response;
                        //callback.Invoke(resultCode);
                        this.NotifyProperties();
                        //return;
                    }
                    //callback.Invoke(resultCode);
                });
            });
        }

        public bool IsLoaded
        {
            get
            {
                return this._isLoaded;
            }
        }


        public string AvatarUri
        {
            get
            {
                if (this._profileInfo != null)
                    return this._profileInfo.photo_max;
                return "";
            }
        }

        public string BirthDateStr
        {
            get
            {
                if (this._profileInfo == null)
                    return "";
                if (this.IsBDateSet)
                    return this.BirthDateValue.ToString("dd.MM.yyyy");
                return LocalizedStrings.GetString("Settings_EditProfile_Birthdate_Select");
            }
        }

        public DateTime BirthDateValue
        {
            get
            {
                DateTime dateTime = new DateTime(1960, 1, 1);
                if (!this.IsBDateSet)
                    return dateTime;
                string[] strArray = this._profileInfo.bdate.Split(new char[1] { '.' });
                if (strArray.Length == 3)
                    dateTime = new DateTime(int.Parse(strArray[2]), int.Parse(strArray[1]), int.Parse(strArray[0]));
                return dateTime;
            }
            set
            {
                DateTime dateTime = value;
                if (value >= DateTime.Now.AddYears(-14))
                    dateTime = new DateTime(DateTime.Now.AddYears(-14).Year, value.Month, value.Day);
                this._profileInfo.bdate = dateTime.ToString("dd.MM.yyyy");
                this.AddToUpdateDictionary("bdate", this._profileInfo.bdate);
                this.NotifyPropertyChanged<DateTime>((System.Linq.Expressions.Expression<Func<DateTime>>)(() => this.BirthDateValue));
                this.NotifyPropertyChanged(nameof(this.BirthDateStr));
            }
        }

        public bool IsDirty
        {
            get
            {
                return this._updateDictionary.Count > 0;
            }
        }

        private bool IsBDateSet
        {
            get
            {
                return this._profileInfo != null && !string.IsNullOrWhiteSpace(this._profileInfo.bdate) && !(this._profileInfo.bdate == "0.0.0");
            }
        }

        public bool HaveNameRequestInProgress
        {
            get
            {
                if (this._profileInfo != null && this._profileInfo.name_request != null)
                    return this._profileInfo.name_request.status == "processing";
                return false;
            }
        }

        public bool CanSave
        {
            get
            {
                if (this._isLoaded && !this._isSaving && (this.IsDirty && !string.IsNullOrWhiteSpace(this.FirstName)))
                    return !string.IsNullOrWhiteSpace(this.LastName);
                return false;
            }
        }

        public bool IsSaving
        {
            get
            {
                return this._isSaving;
            }
            set
            {
                this._isSaving = value;
                this.NotifyPropertyChanged(nameof(this.IsSaving));
                this.NotifyPropertyChanged(nameof(this.CanSave));
            }
        }

        public string FirstName
        {
            get
            {
                if (this._profileInfo != null)
                    return this._profileInfo.first_name;
                return string.Empty;
            }
            set
            {
                if (this._profileInfo == null)
                    return;
                this._profileInfo.first_name = value;
                this.NotifyPropertyChanged(nameof(this.FirstName));
                this.AddToUpdateDictionary("first_name", this.FirstName);
            }
        }

        public string LastName
        {
            get
            {
                if (this._profileInfo != null)
                    return this._profileInfo.last_name;
                return string.Empty;
            }
            set
            {
                if (this._profileInfo == null)
                    return;
                this._profileInfo.last_name = value;
                this.NotifyPropertyChanged(nameof(this.LastName));
                this.AddToUpdateDictionary("last_name", this.LastName);
            }
        }

        public bool HavePhoto
        {
            get
            {
                if (!string.IsNullOrEmpty(this.AvatarUri))
                    return !this.AvatarUri.EndsWith("camera_200.png");
                return false;
            }
        }

        public bool IsMale
        {
            get
            {
                if (this._profileInfo != null)
                    return this._profileInfo.sex == 2;
                return false;
            }
            set
            {
                if (!value || this._profileInfo == null)
                    return;
                this._profileInfo.sex = 2;
                this.AddToUpdateDictionary("sex", "2");
                int saveRelTypeId = this.RelationshipType;
                base.NotifyPropertyChanged(nameof(this.RelationshipTypes));
                this.RelationshipType = saveRelTypeId;
            }
        }

        public Visibility CitySelectorVisibility
        {
            get
            {
                return this.Country.id <= 0 ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool IsFemale
        {
            get
            {
                if (this._profileInfo != null)
                    return this._profileInfo.sex == 1;
                return false;
            }
            set
            {
                if (!value || this._profileInfo == null)
                    return;
                this._profileInfo.sex = 1;
                this.AddToUpdateDictionary("sex", "1");
                int saveRelTypeId = this.RelationshipType;
                base.NotifyPropertyChanged(nameof(this.RelationshipTypes));
                this.RelationshipType = saveRelTypeId;
            }
        }

        public string RequestedName
        {
            get
            {
                if (this._profileInfo != null && this._profileInfo.name_request != null)
                    return this._profileInfo.name_request.first_name + " " + this._profileInfo.name_request.last_name;
                return "";
            }
        }

        public bool IsPartnerApplicable
        {
            get
            {
                if (this.RelationshipType != 1 && this.RelationshipType != 6)
                    return this.RelationshipType > 0;
                return false;
            }
        }

        public int RelationshipType
        {
            get
            {
                if (this._profileInfo != null)
                    return this._profileInfo.relation;
                return 0;
            }
            set
            {
                if (this._profileInfo == null)
                    return;
                int num = this.Partner == null ? 0 : (int)this.Partner.sex;
                if (value == 3 || value == 4)
                    num = this.IsMale ? 1 : 2;
                if (this.Partner != null && (int)this.Partner.sex != num)
                    this.Partner = null;
                this._profileInfo.relation = value;
                this.AddToUpdateDictionary("relation", value.ToString());
                this.NotifyPropertyChanged(nameof(this.IsPartnerApplicable));
                this.NotifyPropertyChanged(nameof(this.RelationshipType));
            }
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

        public bool HavePartner
        {
            get
            {
                if (this._profileInfo != null)
                    return this._profileInfo.relation_partner != null;
                return false;
            }
        }

        public VKUser Partner
        {
            get
            {
                if (this._profileInfo != null)
                    return this._profileInfo.relation_partner;
                return null;
            }
            set
            {
                if (this._profileInfo == null)
                    return;
                this._profileInfo.relation_partner = value;
                this.AddToUpdateDictionary("relation_partner_id", value == null ? "0" : value.id.ToString());
                this.NotifyPropertyChanged(nameof(this.Partner));
                this.NotifyPropertyChanged(nameof(this.HavePartner));
                this._profileInfo.relation_pending = 0;
                this.NotifyPropertyChanged(nameof(this.HavePendingPartner));
                this.NotifyPropertyChanged(nameof(this.PendingPartnerText));
                this.NotifyPropertyChanged(nameof(this.RelationRequestsText));
            }
        }

        public VKCountry Country
        {
            get
            {
                if (this._profileInfo != null && this._profileInfo.country != null)
                    return this._profileInfo.country;
                return SettingsEditProfileViewModel._defaultCountry;
            }
            set
            {
                if (value == null)
                    return;
                this._profileInfo.country = value;
                this.AddToUpdateDictionary("country_id", value == null ? "0" : value.id.ToString());
                this.ResetCity();
                this.NotifyPropertyChanged(nameof(this.Country));
            }
        }

        public void ResetCity()
        {
            this._profileInfo.city = null;
            this.AddToUpdateDictionary("city_id", "0");
            this.NotifyPropertyChanged(nameof(this.City));
        }

        public VKCity City
        {
            get
            {
                if (this._profileInfo != null && this._profileInfo.city != null)
                    return this._profileInfo.city;
                return SettingsEditProfileViewModel._defaultCity;
            }
            set
            {
                if (value == null)
                    return;
                this._profileInfo.city = value;
                this.AddToUpdateDictionary("city_id", value == null ? "0" : value.id.ToString());
                this.NotifyPropertyChanged(nameof(this.City));
            }
        }

        public bool HavePendingPartner
        {
            get
            {
                if (this._profileInfo != null)
                    return this._profileInfo.relation_pending == 1;
                return false;
            }
        }

        public string PendingPartnerText
        {
            get
            {
                if (this._profileInfo != null && this._profileInfo.relation_pending == 1 && this._profileInfo.relation_partner != null)
                    return LocalizedStrings.GetString("Settings_EditProfile_PendingConfirmation");
                return "";
            }
        }

        public string RelationRequestsText
        {
            get
            {
                if (this._profileInfo == null || this._profileInfo.relation_requests == null || this._profileInfo.relation_requests.Count <= 0)
                    return "";
                if (this._profileInfo.relation_requests.Count == 1)
                {
                    VKUser user = this._profileInfo.relation_requests[0];
                    return string.Format( LocalizedStrings.GetString(user.IsFemale ? "Settings_EditProfile_SomebodyIsInLoveWithYouFemaleFrm" : "CommonResources.Settings_EditProfile_SomebodyIsInLoveWithYouMaleFrm"), user.NameLink);
                }
                string commaSeparated = this._profileInfo.relation_requests.Select((u => u.NameLink)).ToList().GetCommaSeparated(", ");
                string.Format(LocalizedStrings.GetString("Settings_EditProfile_SomebodyIsInLoveWithYouManyFrm"), commaSeparated);
                return commaSeparated;
            }
        }

        public bool HaveRelationRequests
        {
            get { return !string.IsNullOrEmpty(this.RelationRequestsText); }
        }

        private void NotifyProperties()
        {
            base.NotifyPropertyChanged(nameof(this.AvatarUri));
            base.NotifyPropertyChanged(nameof(this.BirthDateStr));
            base.NotifyPropertyChanged(nameof(this.BirthdayShowType));
            //base.NotifyPropertyChanged(nameof(this.BirthdaysShowTypes));
            base.NotifyPropertyChanged(nameof(this.HaveNameRequestInProgress));
            base.NotifyPropertyChanged(nameof(this.HavePhoto));
            base.NotifyPropertyChanged(nameof(this.IsFemale));
            base.NotifyPropertyChanged(nameof(this.IsMale));
            base.NotifyPropertyChanged(nameof(this.RelationshipType));
            base.NotifyPropertyChanged(nameof(this.RelationshipTypes));
            base.NotifyPropertyChanged(nameof(this.RequestedName));
            base.NotifyPropertyChanged(nameof(this.IsPartnerApplicable));
            base.NotifyPropertyChanged(nameof(this.HavePartner));
            base.NotifyPropertyChanged(nameof(this.Partner));
            base.NotifyPropertyChanged(nameof(this.FirstName));
            base.NotifyPropertyChanged(nameof(this.LastName));
            base.NotifyPropertyChanged(nameof(this.PendingPartnerText));
            base.NotifyPropertyChanged(nameof(this.HavePendingPartner));
            base.NotifyPropertyChanged(nameof(this.RelationRequestsText));
            base.NotifyPropertyChanged(nameof(this.HaveRelationRequests));
        }

        public void AddToUpdateDictionary(string key, string value)
        {
            this._updateDictionary[key] = value;
            base.NotifyPropertyChanged(nameof(this.IsDirty));
            base.NotifyPropertyChanged(nameof(this.CanSave));
        }

        public int BirthdayShowType
        {
            get
            {
                if (this._profileInfo != null)
                    return this._profileInfo.bdate_visibility;
                return 1;
            }
            set
            {
                if (/*value == 0 ||*/ this._profileInfo == null)
                    return;
                this._profileInfo.bdate_visibility = value;
                this.AddToUpdateDictionary("bdate_visibility", value.ToString());
            }
        }

        public string StatusText { get; set; }
        public string FooterText { get; set; }

        internal void CancelNameRequest()
        {
            if (this._profileInfo == null || this._profileInfo.name_request == null || (this._profileInfo.name_request.id == 0 || this.IsSaving))
                return;
            this.IsSaving = true;
            this.SetInProgress(true);
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["cancel_request_id"] = this._profileInfo.name_request.id.ToString();
            
            AccountService.Instance.SaveSettingsAccountInfo(parameters, (result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    this.IsSaving = false;
                    this.SetInProgress(false);
                    if (result.error.error_code == Core.Enums.VKErrors.None)
                    {
                        this._profileInfo.name_request = null;
                        base.NotifyPropertyChanged(nameof(this.HaveNameRequestInProgress));
                    }
                    //else
                    //    GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", (VKRequestsDispatcher.Error)null);
                });
            });
            
        }

        internal void DeletePhoto()
        {
            //this.SetInProgress(true, "");
            AccountService.Instance.DeleteProfilePhoto((result) =>
            {
                if (result.error.error_code != Core.Enums.VKErrors.None)
                    return;

                Execute.ExecuteOnUIThread(() =>
                {
                
                //this.SetInProgress(false, "");
                

                if (this._profileInfo != null)
                {
                        this._profileInfo.photo_max = result.response.photo_max;
                        Settings.LoggedInUserPhoto = result.response.photo_max;
                        base.NotifyPropertyChanged(nameof( this.AvatarUri));
                        base.NotifyPropertyChanged(nameof( this.HavePhoto));
                        //AppGlobalStateManager.Current.GlobalState.LoggedInUser.photo_max = res.ResultData.photo_max;
                        //EventAggregator.Current.Publish(new BaseDataChangedEvent()
                        //{
                        //    IsProfileUpdateRequired = true
                        //});
                    }
                //else
                //    GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", null);
                    
                });
            });
        }

        public void Save()
        {
            if (!this.CanSave)
                return;
            this.IsSaving = true;
            this.SetInProgress(true);
            AccountService.Instance.SaveSettingsAccountInfo(this._updateDictionary, (result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    this.IsSaving = false;
                    this.SetInProgress(false);
                    if (result.error.error_code == Core.Enums.VKErrors.None)
                    {
                        if (this._updateDictionary.ContainsKey("first_name") || this._updateDictionary.ContainsKey("last_name"))
                        {
                            //int num = (int)MessageBox.Show(CommonResources.Settings_EditProfile_ChangeNameRequestDesc, CommonResources.Settings_EditProfile_ChangeNameRequest, MessageBoxButton.OK);
                            MessageDialog dialog = new MessageDialog(LocalizedStrings.GetString("Settings_EditProfile_ChangeNameRequest"),LocalizedStrings.GetString("Settings_EditProfile_ChangeNameRequestDesc") );
                            //dialog.Commands.Add(new UICommand { Label = "Ok", Id = 1 });
                            //dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 0 });
                            var res = dialog.ShowAsync();
                        }
                        //EventAggregator.Current.Publish((object)new BaseDataChangedEvent()
                        //{
                        //    IsProfileUpdateRequired = true
                        //});
                        NavigatorImpl.GoBack();
                    }
                    //else
                    //    VKClient.Common.UC.GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
                });
            });
        }
    }
}
