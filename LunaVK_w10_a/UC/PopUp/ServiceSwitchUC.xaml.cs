using LunaVK.Core;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using LunaVK.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.UC.PopUp
{
    public sealed partial class ServiceSwitchUC : UserControl
    {
        public Action<int> SelectTap;

        public ServiceSwitchUC()
        {
            this.InitializeComponent();
        }
        
        public ServiceSwitchUC(ServicesViewModel.CommunityService service, ServicesViewModel.CommunityServiceState currentState):this()
        {
            base.DataContext = new ServiceSwitchViewModel(service,currentState);
        }
        
        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionChanged += Lv_SelectionChanged;
        }

        private void Lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            this.SelectTap?.Invoke(lv.SelectedIndex);
        }

        public sealed class ServiceSwitchViewModel : ViewModelBase
        {
            private readonly ServicesViewModel.CommunityService _service;
            private ServicesViewModel.CommunityServiceState _currentState;

            public string PageTitle
            {
                get
                {
                    switch (this._service)
                    {
                        case ServicesViewModel.CommunityService.Wall:
                            return LocalizedStrings.GetString("SectionWall/Title").ToUpper();
                        case ServicesViewModel.CommunityService.Photos:
                            return LocalizedStrings.GetString("SectionPhotos/Title").ToUpper();
                        case ServicesViewModel.CommunityService.Videos:
                            return LocalizedStrings.GetString("SectionVideos/Title").ToUpper();
                        case ServicesViewModel.CommunityService.Audios:
                            return LocalizedStrings.GetString("SectionAudios/Title").ToUpper();
                        case ServicesViewModel.CommunityService.Documents:
                            return LocalizedStrings.GetString("SectionDocuments/Title").ToUpper();
                        case ServicesViewModel.CommunityService.Discussions:
                            return LocalizedStrings.GetString("SectionDiscussions/Title").ToUpper();
                        default:
                            return "";
                    }
                }
            }

            public string DisabledTitle
            {
                get { return LocalizedStrings.GetString(this._service == ServicesViewModel.CommunityService.Wall ? "Disabled_Form1" : "Disabled_Form2"); }
            }
            
            public string OpenedTitle
            {
                get { return LocalizedStrings.GetString(this._service == ServicesViewModel.CommunityService.Wall ? "Opened_Form1" : "Opened_Form2"); }
            }
            
            public string OpenedDescription
            {
                get
                {
                    switch (this._service)
                    {
                        case ServicesViewModel.CommunityService.Wall:
                            return LocalizedStrings.GetString("OpenedWallDescription");
                        case ServicesViewModel.CommunityService.Photos:
                            return LocalizedStrings.GetString("OpenedPhotosDescription");
                        case ServicesViewModel.CommunityService.Videos:
                            return LocalizedStrings.GetString("OpenedVideosDescription");
                        case ServicesViewModel.CommunityService.Audios:
                            return LocalizedStrings.GetString("OpenedAudiosDescription");
                        case ServicesViewModel.CommunityService.Documents:
                            return LocalizedStrings.GetString("OpenedDocumentsDescription");
                        case ServicesViewModel.CommunityService.Discussions:
                            return LocalizedStrings.GetString("OpenedDiscussionsDescription");
                        default:
                            return "";
                    }
                }
            }

            public string LimitedTitle
            {
                get { return LocalizedStrings.GetString(this._service == ServicesViewModel.CommunityService.Wall ? "Limited_Form1" : "Limited_Form2"); }
            }
            
            public string LimitedDescription
            {
                get
                {
                    switch (this._service)
                    {
                        case ServicesViewModel.CommunityService.Wall:
                            return LocalizedStrings.GetString("LimitedWallDescription");
                        case ServicesViewModel.CommunityService.Photos:
                            return LocalizedStrings.GetString("LimitedPhotosDescription");
                        case ServicesViewModel.CommunityService.Videos:
                            return LocalizedStrings.GetString("LimitedVideosDescription");
                        case ServicesViewModel.CommunityService.Audios:
                            return LocalizedStrings.GetString("LimitedAudiosDescription");
                        case ServicesViewModel.CommunityService.Documents:
                            return LocalizedStrings.GetString("LimitedDocumentsDescription");
                        case ServicesViewModel.CommunityService.Discussions:
                            return LocalizedStrings.GetString("LimitedDiscussionsDescription");
                        default:
                            return "";
                    }
                }
            }

            public Visibility ClosedVisibility
            {
                get { return (this._service == ServicesViewModel.CommunityService.Wall).ToVisiblity(); }
            }
            
            public ServiceSwitchViewModel(ServicesViewModel.CommunityService service, ServicesViewModel.CommunityServiceState currentState)
            {
                this._service = service;
                this._currentState = currentState;
            }
            /*
            public void SaveResult(ServicesViewModel.CommunityServiceState newState)
            {
                ParametersRepository.SetParameterForId("CommunityManagementService", this._service);
                ParametersRepository.SetParameterForId("CommunityManagementServiceNewState", newState);
                Navigator.Current.GoBack();
            }
            */
            public int SelectedIndex
            {
                get { return (int)this._currentState; }
                set
                {
                    this._currentState = (ServicesViewModel.CommunityServiceState)value;
                }
            }
        }

        
    }
}
