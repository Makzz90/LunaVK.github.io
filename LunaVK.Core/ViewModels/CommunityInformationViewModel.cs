using System;
using LunaVK.Core.Enums;
using LunaVK.Core.Json;
using System.Threading.Tasks;
using System.Linq;
using LunaVK.Core.Network;
using LunaVK.Core.Library;
using LunaVK.Core.Framework;

namespace LunaVK.Core.ViewModels
{
    //InformationViewModel
    public class CommunityInformationViewModel : ViewModelBase
    {
        public readonly uint CommunityId;
        public GroupsService.CommunitySettings Information { get; private set; }
        /*
        public CommonFieldsViewModel CommonFieldsViewModel { get; private set; }

        public FoundationDateViewModel FoundationDateViewModel { get; private set; }

        public AgeLimitsViewModel AgeLimitsViewModel { get; private set; }

        public CommunityTypeSelectionViewModel CommunityTypeSelectionViewModel { get; private set; }

        public EventOrganizerViewModel EventOrganizerViewModel { get; private set; }

        public EventDatesViewModel EventDatesViewModel { get; private set; }

        public CommunityPlacementViewModel CommunityPlacementViewModel { get; private set; }
        */
        public CommunityInformationViewModel(uint group_id)
        {
            this.CommunityId = group_id;
            this.Information = new GroupsService.CommunitySettings();
        }

        public Action<ProfileLoadingStatus> LoadingStatusUpdated { get; set; }

        

        public void LoadData()
        {
            this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Reloading);

            GroupsService.Instance.GetCommunitySettings(this.CommunityId, (result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                { 
                    if (result.error.error_code == VKErrors.None)
                    {
                        this.Information = result.response;
                        base.NotifyPropertyChanged(nameof(this.Information));

                    
                        this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Loaded);
                    }
                    else
                    {
                        this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.ReloadingFailed);
                    }
                });
            });
        }

        public void SaveChanges()
        {
            GroupsService.Instance.SetCommunityInformation(this.CommunityId, this.Information.title, this.Information.description, this.Information.public_category, this.Information.public_subcategory, this.Information.website, this.Information.access, "",0,"",null,"","",0,0,(result)=> { });
        }




        //CommonFieldsViewModel FoundationDateViewModel AgeLimitsViewModel CommunityTypeSelectionViewModel EventOrganizerViewModel EventDatesViewModel CommunityPlacementViewModel
    }
}
