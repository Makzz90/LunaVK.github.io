using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using LunaVK.UC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;

//MainViewModel
namespace LunaVK.ViewModels
{
    public sealed class GroupManagementViewModel : ViewModelBase
    {
        public readonly uint Id;
        public VKGroupType Type;
        VKAdminLevel adminLevel;
        private VKCommunitySettings Information { get; set; }

        public bool IsLoaded;

        public string StatusText { get; }

        //public InformationViewModel InformationVM { get; private set; }
        
        public CommonFieldsViewModel CommonFieldsVM { get; private set; }
        public FoundationDateViewModel FoundationDateVM { get; private set; }
        public AgeLimitsViewModel AgeLimitsVM { get; private set; }
        public CommunityTypeSelectionViewModel CommunityTypeSelectionVM { get; private set; }
        public EventOrganizerViewModel EventOrganizerVM { get; private set; }
        public CommunityPlacementViewModel CommunityPlacementVM { get; private set; }

        public ServicesViewModel ServicesVM { get; private set; }

        public ManagersViewModel ManagersVM { get; private set; }

        public GroupStatsViewModel StatsVM { get; private set; }
        public RequestsViewModel RequestsVM { get; private set; }
        public CommunitySubscribersViewModel CommunitySubscribersVM { get; private set; }
        public BlacklistViewModel BlacklistVM { get; private set; }
        public LinksViewModel LinksVM { get; private set; }
        public InvitationsViewModel InvitationsVM { get; private set; }

        public GroupCallbackServerViewModel CallbackServerViewModel { get; private set; }

        public GroupManagementViewModel(uint id, VKGroupType type, VKAdminLevel isAdministrator)
        {
            this.Id = id;
            this.Type = type;
            this.adminLevel = isAdministrator;

            //this.InformationVM = new InformationViewModel(id);
            
            this.CommonFieldsVM = new CommonFieldsViewModel();
            this.FoundationDateVM = new FoundationDateViewModel();
            this.AgeLimitsVM = new AgeLimitsViewModel();
            this.CommunityTypeSelectionVM = new CommunityTypeSelectionViewModel();
            this.EventOrganizerVM = new EventOrganizerViewModel();
            this.CommunityPlacementVM = new CommunityPlacementViewModel(id,null);

            this.ServicesVM = new ServicesViewModel(id, type);

            this.ManagersVM = new ManagersViewModel(id);

            this.StatsVM = new GroupStatsViewModel(id);
            this.RequestsVM = new RequestsViewModel(id);
            this.CommunitySubscribersVM = new CommunitySubscribersViewModel(id, type);
            this.BlacklistVM = new BlacklistViewModel(id);
            this.LinksVM = new LinksViewModel(id);
            this.InvitationsVM = new InvitationsViewModel(id);
            this.CallbackServerViewModel = new GroupCallbackServerViewModel(id);
        }

        public Visibility RequestsVisibility
        {
            get
            {
                if (this.Type != VKGroupType.Group)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility InvitationsVisibility
        {
            get
            {
                if (this.Type != VKGroupType.Page)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility AdministrationSectionsVisibility
        {
            get { return (this.adminLevel == VKAdminLevel.Admin).ToVisiblity(); }
        }

        public string MembersTitle
        {
            get { return LocalizedStrings.GetString(this.Type == VKGroupType.Page ? "Management_Followers" : "Management_Members"); }
        }

        public Action<ProfileLoadingStatus> LoadingStatusUpdated { get; set; }



        public void LoadData()
        {
            this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Reloading);

//#if DEBUG
            //RequestsDispatcher.GetResponseFromDump<VKCommunitySettings>(500, "admin155775051.json", (result) =>
            //RequestsDispatcher.GetResponseFromDump<VKCommunitySettings>(500, "admin_private_group196072411.json", (result) =>
//#else
            GroupsService.Instance.GetCommunitySettings(this.Id, (result) =>
//#endif

            {
                Execute.ExecuteOnUIThread(() =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        //this.InformationVM.Information = result.response;
                        this.Information = result.response;
                        this.CommonFieldsVM.Read(result.response, this.Type);
                        this.FoundationDateVM.Read(result.response, this.Type);
                        this.AgeLimitsVM.Read(result.response);
                        this.CommunityTypeSelectionVM.Read(result.response, this.Type);
                        this.EventOrganizerVM.Read(result.response, this.Type);
                        this.CommunityPlacementVM.Read(result.response, this.Type);

                        this.ServicesVM.Read(result.response);


                        this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Loaded);
                        this.IsLoaded = true;
                    }
                    else
                    {
                        this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.ReloadingFailed);
                    }
                });
//            }, (jsonStr)=> {
//                return VKRequestsDispatcher.FixArrayToObject(jsonStr, "sections_list");
            });
        }

        public void SaveChanges()
        {
            Dictionary<string, string> infoParams = new Dictionary<string, string>();

            string domain = this.CommonFieldsVM.Domain != this.CommonFieldsVM.CurrentDomain ? this.CommonFieldsVM.Domain : null;
            if (string.IsNullOrEmpty( domain))
                domain = "club" + this.Id;

            infoParams["title"] = this.CommonFieldsVM.Name;//1
            infoParams["description"] = this.CommonFieldsVM.Description;//2
            //address
            infoParams["website"] = this.CommonFieldsVM.Site;//4
            //wall
            //photos
            //video
            //audio
            //docs
            //topics
            //wiki
            //events
            //places
            //contacts
            //links
            infoParams["access"] = this.CommunityTypeSelectionVM.AccessLevel.ToString();

            infoParams["subject"] = this.CommonFieldsVM.Category.id.ToString();
            infoParams["public_category"] = this.CommonFieldsVM.Category.id.ToString();

            //if(this.CommonFieldsVM.Category.id>0)
                infoParams["public_subcategory"] = (this.CommonFieldsVM.Subcategory == null ? 0 : this.CommonFieldsVM.Subcategory.id).ToString();

            //start_date
            //finish_date
            //place
            infoParams["screen_name"] = domain;

            
            
            
            infoParams["age_limits"] = this.AgeLimitsVM.AgeLimits.ToString();
            
            if (this.Type == VKGroupType.Page)
            {
                string str3 = this.FoundationDateVM.Day;
                string str4 = this.FoundationDateVM.Month.ToString();
                string str5 = this.FoundationDateVM.Year;

                if (str3.Length > 2 || str4.Length > 2 || str5.Length > 4)
                {
                    str3 = "00";
                    str4 = "00";
                    str5 = "0000";
                }

                if (str3.Length == 1)
                    str3 = "0" + str3;
                if (str4.Length == 1)
                    str4 = "0" + str4;
                if (str5.Length == 1)
                    str5 = "0000";
                string foundationDate = string.Format("{0}.{1}.{2}", str3, str4, str5);
                infoParams["public_date"] = foundationDate;
            }
            
            
            if (this.Type == VKGroupType.Event)
            {
                int? eventOrganizerId = new int?(this.EventOrganizerVM.Organizer.user_id < 0 ? -this.EventOrganizerVM.Organizer.user_id : 0);
                if (eventOrganizerId.HasValue)
                {
                    DateTime startDate = this.EventOrganizerVM.StartDate;
                    DateTime startTime = this.EventOrganizerVM.StartTime;
                    DateTime eventStartDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, startTime.Hour, startTime.Minute, 0, DateTimeKind.Local);
                    DateTime finishDate = this.EventOrganizerVM.FinishDate;
                    DateTime finishTime = this.EventOrganizerVM.FinishTime;
                    DateTime eventFinishDate = new DateTime(finishDate.Year, finishDate.Month, finishDate.Day, finishTime.Hour, finishTime.Minute, 0, DateTimeKind.Local);


                    infoParams["event_group_id"]= eventOrganizerId.Value.ToString();
                    infoParams["phone"] = this.EventOrganizerVM.Phone;
                    infoParams["email"] = this.EventOrganizerVM.Email;
                    infoParams["event_start_date"] = eventStartDate.ToString();
                    infoParams["event_finish_date"] = eventFinishDate.ToString();
                }


            }

            this.SetInProgress(true);
            //this.IsFormEnabled = false;

            GroupsService.Instance.SetCommunityInformation(this.Id, infoParams, (result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    GenericInfoUC.ShowBasedOnResult("Saved", result.error);

                    //this.IsFormEnabled = true;
                    this.SetInProgress(false);
                });
            });
        }


















































        /*
        public class InformationViewModel : ViewModelBase
        {
            public readonly uint CommunityId;
            public VKCommunitySettings Information { get; set; }

            public InformationViewModel(uint communityId)
            {
                this.CommunityId = communityId;
                //this.Information = new VKCommunitySettings();
            }



            public void SaveChanges()
            {
                GroupsService.Instance.SetCommunityInformation(this.CommunityId, this.Information.title, this.Information.description, this.Information.public_category, this.Information.public_subcategory, this.Information.website, this.Information.access, "", 0, "", null, "", "", 0, 0, (result) => { });
            }




            //CommonFieldsViewModel FoundationDateViewModel AgeLimitsViewModel CommunityTypeSelectionViewModel EventOrganizerViewModel EventDatesViewModel CommunityPlacementViewModel
        }
        */
        
        public sealed class CommonFieldsViewModel : ViewModelBase
        {
            public string CurrentDomain { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Domain { get; set; }
            public string Site { get; set; }
            public string Phone { get; set; }
            public string CategoryTitle { get; set; }
            public string CategoryPlaceholder { get; set; }

            //private int _publicSubcategory;

            public List<VKCommunitySettings.Section> AvailableCategories { get; private set; }


            public List<VKCommunitySettings.Section> AvailableSubcategories { get; private set; }

            private VKCommunitySettings.Section _subcategory;
            public VKCommunitySettings.Section Subcategory
            {
                get
                {
                    /*
                    if (this.AvailableSubcategories == null)
                        return null;

                    return this.AvailableSubcategories.FirstOrDefault((a)=>a.id == this._publicSubcategory);
                    */
                    return this._subcategory;
                }
                set
                {
                    /*
                    if (value == null)
                        this._publicSubcategory = 0;
                    else
                        this._publicSubcategory = value.id;
                    */
                    this._subcategory = value;
                    base.NotifyPropertyChanged();
                }
            }

            private VKCommunitySettings.Section _category;
            public VKCommunitySettings.Section Category
            {
                get
                {
                    return this._category;
                }
                set
                {
                    this._category = value;




                    //                   List<VKCommunitySettings.Section> subtypesList = (this._publicPageCategories != null && value != null) ? this._publicPageCategories.First((category => category.id == value.id)).subcategories : null;
                    //                   this.AvailableSubcategories = subtypesList != null ? subtypesList : null;
                    this.AvailableSubcategories = value.subcategories;

                    base.NotifyPropertyChanged();
                    base.NotifyPropertyChanged(nameof(this.AvailableSubcategories));
                    base.NotifyPropertyChanged(nameof(this.SubcategoryVisibility));

                    //base.NotifyPropertyChanged(nameof(this.Subcategory));
                    //this.Subcategory = this.AvailableSubcategories.First();
                }
            }

            public Visibility SubcategoryVisibility
            {
                get { return (this.AvailableSubcategories!=null).ToVisiblity(); }
            }

            public void Read(VKCommunitySettings information, VKGroupType type)
            {
                this.Name = ExtensionsBase.ForUI(information.title);
                this.Description = ExtensionsBase.ForUI(information.description);
                this.Domain = this.CurrentDomain = information.address;
                this.Site = ExtensionsBase.ForUI(information.website);
                this.Phone = information.phone;

                if (type != VKGroupType.Page)
                {
                    this.CategoryTitle = LocalizedStrings.GetString( "CommunitySubject");
                    this.CategoryPlaceholder = LocalizedStrings.GetString("SelectSubject");
                    /*
                    if (information.subject_list == null)
                        information.subject_list = new List<VKCommunitySettings.Section>();

                    information.subject_list.Insert(0, new VKCommunitySettings.Section() { id = 0, name = LocalizedStrings.GetString("NoneSelected") });

                    this.AvailableCategories = information.subject_list;
                    */
                    this.AvailableCategories = information.public_category_list;
                    //information.public_category_list.Add(new VKCommunitySettings.Section() { name = LocalizedStrings.GetString("NoneSelected") });//Этого быть не может!



                    this.Category = this.AvailableCategories.First((c => c.id == information.public_category));

                    //this._publicSubcategory = information.public_subcategory;




                    VKCommunitySettings.Section customListPickerItem1;
                    if (this.AvailableSubcategories == null)
                    {
                        customListPickerItem1 = null;
                    }
                    else
                    {
                        customListPickerItem1 = this.AvailableSubcategories.First((s => s.id == information.public_subcategory));
                    }
                    this.Subcategory = customListPickerItem1;
                }
                else
                {
                    this.CategoryTitle = LocalizedStrings.GetString("PublicPageCategory");
                    this.CategoryPlaceholder = LocalizedStrings.GetString("SelectCategory");

                    if (information.public_category_list == null)
                        information.public_category_list = new List<VKCommunitySettings.Section>();
                    /*
                    for (int index = 1; index < information.public_category_list.Count; ++index)
                    {
                        var subtypesList = information.public_category_list[index].subcategories;
                        if (subtypesList != null && subtypesList.Any())
                            subtypesList.First().name = LocalizedStrings.GetString("NoneSelected");
                    }
                    
                    information.public_category_list.First().name = LocalizedStrings.GetString("NoneSelected");
                    */
                    //information.public_category_list.Add(new VKCommunitySettings.Section() { name = LocalizedStrings.GetString("NoneSelected") });//Этого быть не может!

                    //                   this._publicPageCategories = information.public_category_list;
                    this.AvailableCategories = information.public_category_list;
                    this.Category = this.AvailableCategories.First((c => c.id == information.public_category));
                    if (this.Category.id == 0)
                        return;

                    VKCommunitySettings.Section customListPickerItem1;
                    if (this.AvailableSubcategories == null)
                    {
                        customListPickerItem1 = null;
                    }
                    else
                    {
                        customListPickerItem1 = this.AvailableSubcategories.First((s => s.id == information.public_subcategory));
                    }
                    this.Subcategory = customListPickerItem1;
                }

                base.NotifyPropertyChanged(nameof(this.Name));
                base.NotifyPropertyChanged(nameof(this.Description));
                base.NotifyPropertyChanged(nameof(this.Domain));
                base.NotifyPropertyChanged(nameof(this.Site));
                base.NotifyPropertyChanged(nameof(this.Phone));

                base.NotifyPropertyChanged(nameof(this.CategoryTitle));
                base.NotifyPropertyChanged(nameof(this.CategoryPlaceholder));
                base.NotifyPropertyChanged(nameof(this.AvailableCategories));
            }
        }

        public class FoundationDateViewModel : ViewModelBase
        {
            public FoundationDateViewModel()
            {
                List<string> customListPickerItemList = new List<string>();
                customListPickerItemList.Add(LocalizedStrings.GetString("NotDefined"));
                for (int i = 1; i <= 12; i++)
                {
                    customListPickerItemList.Add(UIStringFormatterHelper.GetOfMonthStr(i));
                }

                this.AvailableMonths = customListPickerItemList;











                List<string> customListPickerItemList2 = new List<string>();
                customListPickerItemList2.Add(LocalizedStrings.GetString("NotDefined"));

                for (int year = DateTime.Now.Year; year >= 1900; year--)
                {
                    customListPickerItemList2.Add(year.ToString());
                }

                this.AvailableYears = customListPickerItemList2;
            }

            private Visibility _visibility;
            public Visibility Visibility
            {
                get
                {
                    return this._visibility;
                }
                set
                {
                    this._visibility = value;
                    base.NotifyPropertyChanged();
                }
            }

            public string Title { get; set; }

            public List<string> AvailableYears { get; set; }

            public List<string> AvailableMonths { get; set; }

            private List<string> _availableDays;
            public List<string> AvailableDays
            {
                get
                {
                    return this._availableDays;
                }
                set
                {
                    this._availableDays = value;
                    this.NotifyPropertyChanged();
                }
            }

            private int _month;
            public int Month
            {
                get
                {
                    return this._month;
                }
                set
                {
                    this._month = value;
                    base.NotifyPropertyChanged();
                    this.UpdateAvailableDays();
                }
            }

            private string _day;
            public string Day
            {
                get
                {
                    return this._day;
                }
                set
                {
                    this._day = value;
                    base.NotifyPropertyChanged();
                }
            }

            private string _year;
            public string Year
            {
                get
                {
                    return this._year;
                }
                set
                {
                    this._year = value;
                    base.NotifyPropertyChanged();
                    this.UpdateAvailableDays();
                }
            }

            private void UpdateAvailableDays()
            {


                List<string> customListPickerItemList = new List<string>();
                customListPickerItemList.Add(LocalizedStrings.GetString("NotDefined"));
                List<string> source = customListPickerItemList;
                int num2 = 31;

                int year, day;


                if (int.TryParse(this.Year, out year) && this.Month > 0)
                    num2 = DateTime.DaysInMonth(year, this.Month);

                for (int index = 1; index <= num2; ++index)
                    source.Add(index.ToString());

                bool res = int.TryParse(this.Day, out day);

                this.AvailableDays = source;

                this.Day = !res || day > num2 ? source.Last() : source[day];
            }

            public void Read(VKCommunitySettings information, VKGroupType type)
            {
                if (type != VKGroupType.Page)
                {
                    this.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.Title = information.public_date_label;
                    if (this.Title.EndsWith(":"))
                        this.Title = this.Title.Substring(0, this.Title.Length - 1);



                    string[] date = information.public_date.Split('.');
                    this.Month = int.Parse(date[1]);
                    if (date.Length > 2)
                    {
                        string year = int.Parse(date[2]).ToString();
                        this.Year = this.AvailableYears.FirstOrDefault((y)=> y == year);
                    }

                    if (this.Year == null)
                        this.Year = this.AvailableYears.First();

                    this.UpdateAvailableDays();
                    this.Day = this.AvailableDays[int.Parse(date[0])];


                    base.NotifyPropertyChanged(nameof(this.Title));
                }
            }
        }

        public sealed class AgeLimitsViewModel : ViewModelBase
        {
            public int AgeLimits;

            public Visibility FullFormVisibility
            {
                get
                {
                    return (this.AgeLimits != 1).ToVisiblity();
                }
            }

            public Visibility SetAgeLimitsButtonVisibility
            {
                get
                {
                    return (this.FullFormVisibility == Visibility.Collapsed).ToVisiblity();
                }
            }

            public void Read(VKCommunitySettings information)
            {
                this.AgeLimits = information.age_limits;

                switch (information.age_limits)
                {
                    case 1:
                        this.IsNoLimits = true;
                        break;
                    case 2:
                        this.From16Only = true;
                        break;
                    case 3:
                        this.From18Only = true;
                        break;
                }

                base.NotifyPropertyChanged(nameof(this.FullFormVisibility));
                base.NotifyPropertyChanged(nameof(this.SetAgeLimitsButtonVisibility));
            }

            public bool IsNoLimits
            {
                get
                {
                    return this.AgeLimits == 1;
                }
                set
                {
                    if (value == true)
                        this.AgeLimits = 1;

                    this.NotifyPropertyChanged(nameof(this.FullFormVisibility));
                    base.NotifyPropertyChanged(nameof(this.SetAgeLimitsButtonVisibility));
                    base.NotifyPropertyChanged();
                }
            }

            public bool From16Only
            {
                get
                {
                    return this.AgeLimits == 2;
                }
                set
                {
                    if (value == true)
                        this.AgeLimits = 2;

                    this.NotifyPropertyChanged(nameof(this.FullFormVisibility));
                    base.NotifyPropertyChanged(nameof(this.SetAgeLimitsButtonVisibility));
                    base.NotifyPropertyChanged();
                }
            }

            public bool From18Only
            {
                get
                {
                    return this.AgeLimits == 3;
                }
                set
                {
                    if (value == true)
                        this.AgeLimits = 3;

                    this.NotifyPropertyChanged(nameof(this.FullFormVisibility));
                    base.NotifyPropertyChanged(nameof(this.SetAgeLimitsButtonVisibility));
                    base.NotifyPropertyChanged();
                }
            }
        }

        public sealed class CommunityTypeSelectionViewModel : ViewModelBase
        {
            private Visibility _visibility;
            public Visibility Visibility
            {
                get
                {
                    return this._visibility;
                }
                set
                {
                    this._visibility = value;
                    base.NotifyPropertyChanged();
                }
            }

            private string _title;
            public string Title
            {
                get
                {
                    return this._title;
                }
                set
                {
                    this._title = value;
                    base.NotifyPropertyChanged();
                }
            }

            private Visibility _privateVisibility;
            public Visibility PrivateVisibility
            {
                get
                {
                    return this._privateVisibility;
                }
                set
                {
                    this._privateVisibility = value;
                    base.NotifyPropertyChanged();
                }
            }

            private string _openedTitle;
            private string _closedTitle;
            private string _openedDescription;
            private string _closedDescription;

            public string OpenedTitle
            {
                get
                {
                    return this._openedTitle;
                }
                set
                {
                    this._openedTitle = value;
                    base.NotifyPropertyChanged();
                }
            }

            public string ClosedTitle
            {
                get
                {
                    return this._closedTitle;
                }
                set
                {
                    this._closedTitle = value;
                    base.NotifyPropertyChanged();
                }
            }

            public string OpenedDescription
            {
                get
                {
                    return this._openedDescription;
                }
                set
                {
                    this._openedDescription = value;
                    base.NotifyPropertyChanged();
                }
            }

            public string ClosedDescription
            {
                get
                {
                    return this._closedDescription;
                }
                set
                {
                    this._closedDescription = value;
                    base.NotifyPropertyChanged();
                }
            }

            public int AccessLevel;

            private bool _isOpenedSelected;
            private bool _isClosedSelected;
            private bool _isPrivateSelected;

            public bool IsOpenedSelected
            {
                get
                {
                    return this._isOpenedSelected;
                }
                set
                {
                    this._isOpenedSelected = value;
                    base.NotifyPropertyChanged();

                    if (value == true)
                        this.AccessLevel = 0;
                }
            }

            public bool IsClosedSelected
            {
                get
                {
                    return this._isClosedSelected;
                }
                set
                {
                    this._isClosedSelected = value;
                    base.NotifyPropertyChanged();

                    if (value == true)
                        this.AccessLevel = 1;
                }
            }

            public bool IsPrivateSelected
            {
                get
                {
                    return this._isPrivateSelected;
                }
                set
                {
                    this._isPrivateSelected = value;
                    base.NotifyPropertyChanged();

                    if (value == true)
                        this.AccessLevel = 2;
                }
            }

            public void Read(VKCommunitySettings information, VKGroupType type)
            {
                if (type == VKGroupType.Page)
                    this.Visibility = Visibility.Collapsed;
                else if (type == VKGroupType.Group)
                {
                    this.Title = LocalizedStrings.GetString("GroupType").ToUpper();
                    this.PrivateVisibility = Visibility.Visible;
                    this.OpenedTitle = LocalizedStrings.GetString("GroupType_Opened");
                    this.ClosedTitle = LocalizedStrings.GetString("GroupType_Closed");
                    this.OpenedDescription = LocalizedStrings.GetString("GroupType_Opened_Description");
                    this.ClosedDescription = LocalizedStrings.GetString("GroupType_Closed_Description");
                    switch (information.access)
                    {
                        case 0:
                            this.IsOpenedSelected = true;
                            break;
                        case 1:
                            this.IsClosedSelected = true;
                            break;
                        case 2:
                            this.IsPrivateSelected = true;
                            break;
                    }
                }
                else
                {
                    this.Title = LocalizedStrings.GetString("EventType").ToUpper();
                    this.PrivateVisibility = Visibility.Collapsed;
                    this.OpenedTitle = LocalizedStrings.GetString("EventType_Opened");
                    this.ClosedTitle = LocalizedStrings.GetString("EventType_Closed");
                    this.OpenedDescription = LocalizedStrings.GetString("EventType_Opened_Description");
                    this.ClosedDescription = LocalizedStrings.GetString("EventType_Closed_Description");
                    if (information.access == 0)
                        this.IsOpenedSelected = true;
                    else
                        this.IsClosedSelected = true;
                }
            }
        }

        public sealed class EventOrganizerViewModel : ViewModelBase
        {
            private string _phone = "";
            private string _email = "";
            private Visibility _visibility;
            private List<VKGroupContact> _availableOrganizers;
            private VKGroupContact _organizer;
            private Visibility _contactsFieldsVisibility;

            public Visibility Visibility
            {
                get
                {
                    return this._visibility;
                }
                set
                {
                    this._visibility = value;
                    base.NotifyPropertyChanged();
                }
            }

            public List<VKGroupContact> AvailableOrganizers
            {
                get
                {
                    return this._availableOrganizers;
                }
                set
                {
                    this._availableOrganizers = value;
                    base.NotifyPropertyChanged();
                }
            }

            public VKGroupContact Organizer
            {
                get
                {
                    return this._organizer;
                }
                set
                {
                    this._organizer = value;
                    base.NotifyPropertyChanged();
                }
            }

            public Visibility ContactsFieldsVisibility
            {
                get
                {
                    return this._contactsFieldsVisibility;
                }
                set
                {
                    this._contactsFieldsVisibility = value;
                    base.NotifyPropertyChanged();
                    this.NotifyPropertyChanged(nameof(this.SetContactsButtonVisibility));
                }
            }

            public Visibility SetContactsButtonVisibility
            {
                get
                {
                    return (this.ContactsFieldsVisibility == Visibility.Collapsed).ToVisiblity();
                }
            }

            public string Phone
            {
                get
                {
                    return this._phone;
                }
                set
                {
                    this._phone = value;
                    base.NotifyPropertyChanged();
                }
            }

            public string Email
            {
                get
                {
                    return this._email;
                }
                set
                {
                    this._email = value;
                    base.NotifyPropertyChanged();
                }
            }

            public void Read(VKCommunitySettings information, VKGroupType type)
            {
                if (type != VKGroupType.Event)
                {
                    this.Visibility = Visibility.Collapsed;
                }
                else
                {
                    //VKGroupContact
                    /*
                    List<CustomListPickerItem> list = information.event_available_organizers.Where<Group>((Func<Group, bool>)(o => o.type != "event")).Select<Group, CustomListPickerItem>((Func<Group, CustomListPickerItem>)(o => new CustomListPickerItem()
                    {
                        Id = -o.id,
                        Name = o.name
                    })).ToList<CustomListPickerItem>();
                    list.Insert(0, new CustomListPickerItem()
                    {
                        Id = information.event_creator.id,
                        Name = information.event_creator.Name
                    });
                    this.AvailableOrganizers = list;
                    this.Organizer = this.AvailableOrganizers.First<CustomListPickerItem>((Func<CustomListPickerItem, bool>)(o =>
                    {
                        if (information.event_group_id != 0L)
                            return information.event_group_id == -o.Id;
                        return true;
                    }));
                    this.Phone = information.phone;
                    this.Email = information.email;
                    this.ContactsFieldsVisibility = (!string.IsNullOrWhiteSpace(information.phone) || !string.IsNullOrWhiteSpace(information.email)).ToVisiblity();
                    */

                    //this.StartDate = this.StartTime = Extensions.UnixTimeStampToDateTime((double)information.start_date.Value, false);
                    //this.FinishDate = this.FinishTime = Extensions.UnixTimeStampToDateTime((double)information.finish_date.Value, false);
                    if (this.FinishTime.Second != 1)
                        return;
                    this.FinishDate = this.FinishTime = this.StartDate.AddHours(4.0);
                    this.FinishFieldsVisibility = Visibility.Collapsed;
                }
            }




















            private Visibility _finishFieldsVisibility;
            private DateTime _startDate;
            private DateTime _startTime;
            private DateTime _finishDate;
            private DateTime _finishTime;

            public DateTime StartDate
            {
                get
                {
                    return this._startDate;
                }
                set
                {
                    this._startDate = value;
                    base.NotifyPropertyChanged();
                    this.NotifyPropertyChanged<string>((() => this.StartDateString));
                }
            }

            public DateTime StartTime
            {
                get
                {
                    return this._startTime;
                }
                set
                {
                    this._startTime = value;
                    base.NotifyPropertyChanged();
                    this.NotifyPropertyChanged<string>((() => this.StartTimeString));
                }
            }

            public DateTime FinishDate
            {
                get
                {
                    return this._finishDate;
                }
                set
                {
                    this._finishDate = value;
                    base.NotifyPropertyChanged();
                    this.NotifyPropertyChanged<string>((() => this.FinishDateString));
                }
            }

            public DateTime FinishTime
            {
                get
                {
                    return this._finishTime;
                }
                set
                {
                    this._finishTime = value;
                    base.NotifyPropertyChanged();
                    this.NotifyPropertyChanged<string>((() => this.FinishTimeString));
                }
            }

            public string StartTimeString
            {
                get
                {
                    return this.StartTime.ToString("HH:mm");
                }
            }

            public string FinishTimeString
            {
                get
                {
                    return this.FinishTime.ToString("HH:mm");
                }
            }

            public string StartDateString
            {
                get
                {
                    string str = this.StartDate.ToString("dd MMM yyyy");
                    if (str.StartsWith("0"))
                        str = str.Substring(1);
                    return str;
                }
            }

            public string FinishDateString
            {
                get
                {
                    string str = this.FinishDate.ToString("dd MMM yyyy");
                    if (str.StartsWith("0"))
                        str = str.Substring(1);
                    return str;
                }
            }

            public Visibility FinishFieldsVisibility
            {
                get
                {
                    return this._finishFieldsVisibility;
                }
                set
                {
                    this._finishFieldsVisibility = value;
                    base.NotifyPropertyChanged();
                    this.NotifyPropertyChanged<Visibility>((() => this.SetFinishTimeButtonVisibility));
                }
            }

            public Visibility SetFinishTimeButtonVisibility
            {
                get
                {
                    return (this.FinishFieldsVisibility == Visibility.Collapsed).ToVisiblity();
                }
            }
        }





        public sealed class CommunityPlacementViewModel : ViewModelBase//, IHandle<CommunityPlacementEdited>, IHandle
        {
            private string _descriptionText = "";
            private VKPlace _place;
            private Visibility _visibility;
            //private SolidColorBrush _descriptionForeground;
            private Visibility _editButtonVisibility;
            //private SolidColorBrush _pinForeground;
            //private double _panelTilt;





            private readonly uint _communityId;
            private int _countryId;
            private int _cityId;
            private string _country;
            private string _city;
            public Geocoordinate GeoCoordinate { get; set; }




            public CommunityPlacementViewModel(uint communityId, VKPlace place)
            {
                this._communityId = communityId;
                if (place == null)
                    return;
                if (string.IsNullOrEmpty( place.country))
                {
                   // this._countryId = place.country_id;
                   // this.Country = place.country_name;
                }
                if (string.IsNullOrEmpty(place.city))
                {
                  //  this._cityId = place.city_id;
                  //  this.City = place.city_name;
                }
             //   this.Address = place.address;
             //   this.Place = place.title;
             //   this.GeoCoordinate = new GeoCoordinate(place.latitude, place.longitude);
            }

            public Visibility Visibility
            {
                get
                {
                    return this._visibility;
                }
                set
                {
                    this._visibility = value;
                    base.NotifyPropertyChanged();
                }
            }

            public string DescriptionText
            {
                get
                {
                    return this._descriptionText;
                }
                set
                {
                    this._descriptionText = value;
                    base.NotifyPropertyChanged();
                }
            }
            
            public Visibility EditButtonVisibility
            {
                get
                {
                    return this._editButtonVisibility;
                }
                set
                {
                    this._editButtonVisibility = value;
                    base.NotifyPropertyChanged();
                }
            }
            

            public void Read(VKCommunitySettings information, VKGroupType type)
            {
                this.Visibility = Visibility.Collapsed;
                /*
                if (type == VKGroupType.Page)
                    this.Visibility = Visibility.Collapsed;
                else
                    this.SetPlacement(information.place);
                    */
            }

            public void SetPlacement(VKPlace place)
            {
                this._place = place;
                this.DescriptionText = "";
                if (place == null || string.IsNullOrWhiteSpace(place.country) && string.IsNullOrWhiteSpace(place.city) && (string.IsNullOrWhiteSpace(place.address) && string.IsNullOrWhiteSpace(place.title)))
                {
                    this.DescriptionText = LocalizedStrings.GetString("ChoosePlacement");
                    //this.DescriptionForeground = this.PinForeground = (SolidColorBrush)Application.Current.Resources["PhoneBlue300Brush"];
                    this.EditButtonVisibility = Visibility.Collapsed;
                    //this.PanelTilt = 2.5;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(place.title))
                        this.DescriptionText = place.title;
                    if (!string.IsNullOrWhiteSpace(place.address))
                        this.DescriptionText = !(this.DescriptionText == "") ? this.DescriptionText + ", " + place.address : place.address;
                    if (this.DescriptionText == "")
                    {
                        if (!string.IsNullOrWhiteSpace(place.city))
                            this.DescriptionText = ExtensionsBase.ForUI(place.city);
                        else if (!string.IsNullOrWhiteSpace(place.country))
                            this.DescriptionText = ExtensionsBase.ForUI(place.country);
                    }
                    //this.DescriptionForeground = (SolidColorBrush)Application.Current.Resources["PhoneContrastTitleBrush"];
                    //this.PinForeground = (SolidColorBrush)Application.Current.Resources["PhoneGray300Brush"];
                    this.EditButtonVisibility = Visibility.Visible;
                    // this.PanelTilt = 0.0;
                }
            }

            public void NavigateToPlacementSelection()
            {
                //NavigatorImpl.Current.NavigateToCommunityManagementPlacementSelection(this.ParentViewModel.CommunityId, this._place);
            }
            /*
            public void Handle(CommunityPlacementEdited message)
            {
                this.SetPlacement(message.Place);
            }
            */


            public void SaveChanges()
            {
                this.SetInProgress(true, "");
                //this.IsFormEnabled = false;
                //GroupsService current = GroupsService.Current;
                uint communityId = this._communityId;
                int countryId = this._countryId;
                int cityId = this._cityId;
                string address = "";// this.Address;
                string place1 = "";// this.Place;
                Geocoordinate geoCoordinate1 = this.GeoCoordinate;
                double latitude = (object)geoCoordinate1 != null ? geoCoordinate1.Latitude : 0.0;
                //GeoCoordinate geoCoordinate2 = this.GeoCoordinate;
                double longitude = (object)geoCoordinate1 != null ? geoCoordinate1.Longitude : 0.0;
                
                GroupsService.Instance.SetCommunityPlacement(communityId, countryId, cityId, address, place1, latitude, longitude, (result)=>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        /*
                        VKClient.Common.Backend.DataObjects.Place place2 = new VKClient.Common.Backend.DataObjects.Place()
                        {
                            country_id = this._countryId,
                            country_name = this.Country,
                            city_id = this._cityId,
                            city_name = this.City,
                            address = this.Address,
                            title = this.Place,
                            latitude = this.GeoCoordinate.Latitude,
                            longitude = this.GeoCoordinate.Longitude,
                            group_id = this._communityId
                        };
                        */
                        //Navigator.Current.GoBack();
                        //EventAggregator.Current.Publish((object)new CommunityPlacementEdited()
                        //{
                        //    Place = place2
                        //});
                    }
                    else
                    {
                        this.SetInProgress(false, "");
                        //this.IsFormEnabled = true;
                        //GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
                    }
                });
            }

        }

        public sealed class InvitationsViewModel : GenericCollectionViewModel<VKUser>
        {
            public readonly uint CommunityId;

            public InvitationsViewModel(uint communityId)
            {
                this.CommunityId = communityId;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
            {
                GroupsService.Instance.GetInvitations(this.CommunityId, offset, count, (result) =>
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
                    
                });
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount == 0)
                        return LocalizedStrings.GetString("NoPersons");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OnePersonFrm", "TwoFourPersonsFrm", "FivePersonsFrm");
                }
            }
        }

    }
}
