using LunaVK.Core.Library;
using LunaVK.Core.ViewModels;
using LunaVK.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LunaVK.ViewModels
{
    public class CommunityCreationViewModel : ViewModelBase
    {
        private string _name = "";
        private bool _isGroupSelected;
        private bool _isPublicPageSelected;
        private bool _isEventSelected;
        private bool _isPlaceSelected;
        private bool _isCompanySelected;
        private bool _isPersonSelected;
        private bool _isProductionSelected;
        private bool _areTermsAccepted;

        public bool IsGroupSelected
        {
            get
            {
                return this._isGroupSelected;
            }
            set
            {
                this._isGroupSelected = value;
                this.NotifyPropertyChanged<Visibility>((() => this.PublicPageTypeFormPartVisibility));
                this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
                this.NotifyPropertyChanged<Visibility>((() => this.DescriptionVisibility));
            }
        }

        public bool IsPublicPageSelected
        {
            get
            {
                return this._isPublicPageSelected;
            }
            set
            {
                this._isPublicPageSelected = value;
                this.NotifyPropertyChanged<Visibility>((() => this.PublicPageTypeFormPartVisibility));
                this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
                this.NotifyPropertyChanged<Visibility>((() => this.DescriptionVisibility));
            }
        }

        public bool IsEventSelected
        {
            get
            {
                return this._isEventSelected;
            }
            set
            {
                this._isEventSelected = value;
                this.NotifyPropertyChanged<Visibility>((() => this.PublicPageTypeFormPartVisibility));
                this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
                this.NotifyPropertyChanged<Visibility>((() => this.DescriptionVisibility));
            }
        }





        public Visibility PublicPageTypeFormPartVisibility
        {
            get
            {
                if (!this.IsPublicPageSelected)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public bool IsPlaceSelected
        {
            get
            {
                return this._isPlaceSelected;
            }
            set
            {
                this._isPlaceSelected = value;
                this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
                this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
            }
        }

        public string Description { get; set; }

        public Visibility DescriptionVisibility
        {
            get
            {
                return this.IsPublicPageSelected ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool IsCompanySelected
        {
            get
            {
                return this._isCompanySelected;
            }
            set
            {
                this._isCompanySelected = value;
                this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
            }
        }

        public bool IsPersonSelected
        {
            get
            {
                return this._isPersonSelected;
            }
            set
            {
                this._isPersonSelected = value;
                this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
            }
        }

        public bool IsProductionSelected
        {
            get
            {
                return this._isProductionSelected;
            }
            set
            {
                this._isProductionSelected = value;
                this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
            }
        }

        public bool AreTermsAccepted
        {
            get
            {
                return this._areTermsAccepted;
            }
            set
            {
                this._areTermsAccepted = value;
                this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
            }
        }

        public bool IsFormCompleted
        {
            get
            {
                if (this.Name.Length >= 2 && !string.IsNullOrWhiteSpace(this.Name))
                {
                    if (this.IsGroupSelected || this.IsEventSelected)
                        return true;

                    if (this.IsPublicPageSelected && this.AreTermsAccepted)
                    {
                        if (this.IsPlaceSelected || this.IsCompanySelected || this.IsPersonSelected || this.IsProductionSelected)
                            return true;
                    }
                }

                return false;
            }
        }

        public void CreateCommunity()
        {
            string type = "group";
            int subtype = 0;
            
            if (this.IsPublicPageSelected)
            {
                type = "public";
                if (this.IsPlaceSelected)
                {
                    subtype = 1;
                }
                else if (this.IsCompanySelected)
                {

                    subtype = 2;
                }
                else if(this.IsPersonSelected)
                {
                    subtype = 3;
                }
                else
                {
                    subtype = 4;
                }
            }
            else
            {
                if (this.IsEventSelected)
                    type = "event";
            }
            //this.SetInProgress(true, "");
            //this.IsFormEnabled = false;
            GroupsService.Instance.CreateCommunity(this.Name, type, this.Description, subtype, (result => 
            {
                if (result.error.error_code== Core.Enums.VKErrors.None)
                {
                    NavigatorImpl.Instance.NavigateToProfilePage(result.response.Id);
                    //this._navigationService.RemoveBackEntry();
                }
                else
                {
                    //this.SetInProgress(false, "");
                    //this.IsFormEnabled = true;
                    //GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
                }
            }));
        }
    }
}
