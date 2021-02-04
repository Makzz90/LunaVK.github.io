using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using LunaVK.UC;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;

namespace LunaVK.ViewModels
{
    public class ServicesViewModel : ViewModelBase
    {
        private VKGroupType Type;

        public ServicesViewModel(uint communityId, VKGroupType type)
        {
            this._communityId = communityId;
            this.Type = type;
        }



        private bool _isFormEnabled = true;
        private string _keyWords = "";
        private readonly uint _communityId;
        private VKCommunitySettings _information;
        private CommunityServiceState _wallOrComments;
        private CommunityServiceState _photos;
        private CommunityServiceState _videos;
        private CommunityServiceState _audios;
        private CommunityServiceState _documents;
        private CommunityServiceState _discussions;
        private bool _market;
        private bool _messages;
        private bool _links;
        private bool _events;
        private bool _contacts;
        private bool _articles;
        private bool _isStrongLanguageFilterEnabled;
        private bool _isKeyWordsFilterEnabled;

        public List<VKCommunitySettings.Section> AvailableMainSections { get; private set; }
        public List<VKCommunitySettings.Section> AvailableSecondarySections { get; private set; }

        private VKCommunitySettings.Section _mainSection;
        public VKCommunitySettings.Section MainSection
        {
            get
            {
                return this._mainSection;
            }
            set
            {
                if (this._mainSection == value)
                    return;

                this._information.main_section = value.id;
                this._mainSection = value;
                base.NotifyPropertyChanged();
                base.NotifyPropertyChanged(nameof(this.SecondarySectionVisibility));

                this.AvailableSecondarySections = this.AvailableMainSections.Where((s) => s.id != this._information.main_section).ToList();

                VKCommunitySettings.Section temp = null;
                if (this._information.main_section == this._information.secondary_section || value.id == 0)
                    temp = this.AvailableSecondarySections[0];
                else
                    temp = this.SecondarySection;

                base.NotifyPropertyChanged(nameof(this.AvailableSecondarySections));
                this.SecondarySection = temp;
                base.NotifyPropertyChanged(nameof(this.SecondarySection));
            }
        }

        private VKCommunitySettings.Section _secondarySection;
        public VKCommunitySettings.Section SecondarySection
        {
            get
            {
                return this._secondarySection;
            }
            set
            {
                if (this._secondarySection == value)
                    return;

                this._secondarySection = value;
                

                if (value == null)
                {
                    return;
                }
                
                this._information.secondary_section = value.id;
                

                base.NotifyPropertyChanged();
            }
        }

        public Visibility SecondarySectionVisibility
        {
            get
            {
                return (this._information.main_section > 0).ToVisiblity();
            }
        }

        public bool IsFormEnabled
        {
            get
            {
                return this._isFormEnabled;
            }
            set
            {
                this._isFormEnabled = value;
                base.NotifyPropertyChanged();
            }
        }

        public CommunityServiceState WallOrComments
        {
            get
            {
                return this._wallOrComments;
            }
            set
            {
                this._wallOrComments = value;
                base.NotifyPropertyChanged();
                this.NotifyPropertyChanged<bool>((() => this.IsCommentsChecked));
                this.NotifyPropertyChanged<string>((() => this.WallStateString));
            }
        }

        public CommunityServiceState Photos
        {
            get
            {
                return this._photos;
            }
            set
            {
                this._photos = value;
                base.NotifyPropertyChanged();
                this.NotifyPropertyChanged<bool>((() => this.IsPhotosChecked));
                this.NotifyPropertyChanged<string>((() => this.PhotosStateString));
            }
        }

        public CommunityServiceState Videos
        {
            get
            {
                return this._videos;
            }
            set
            {
                this._videos = value;
                base.NotifyPropertyChanged();
                this.NotifyPropertyChanged<bool>((() => this.IsVideosChecked));
                this.NotifyPropertyChanged<string>((() => this.VideosStateString));
            }
        }

        public CommunityServiceState Audios
        {
            get
            {
                return this._audios;
            }
            set
            {
                this._audios = value;
                base.NotifyPropertyChanged();
                this.NotifyPropertyChanged<bool>((() => this.IsAudiosChecked));
                this.NotifyPropertyChanged<string>((() => this.AudiosStateString));
            }
        }

        public CommunityServiceState Documents
        {
            get
            {
                return this._documents;
            }
            set
            {
                this._documents = value;
                base.NotifyPropertyChanged();
                this.NotifyPropertyChanged<string>((() => this.DocumentsStateString));
            }
        }

        public CommunityServiceState Discussions
        {
            get
            {
                return this._discussions;
            }
            set
            {
                this._discussions = value;
                base.NotifyPropertyChanged();
                this.NotifyPropertyChanged<bool>((() => this.IsDiscussionsChecked));
                this.NotifyPropertyChanged<string>((() => this.DiscussionsStateString));
            }
        }

        public bool Market
        {
            get
            {
                return this._market;
            }
            set
            {
                this._market = value;
                base.NotifyPropertyChanged();
                //this.NotifyPropertyChanged<string>((() => this.MarketStateString));
            }
        }

        public bool Messages
        {
            get
            {
                return this._messages;
            }
            set
            {
                this._messages = value;
                base.NotifyPropertyChanged();
            }
        }

        public bool Links
        {
            get
            {
                return this._links;
            }
            set
            {
                this._links = value;
                base.NotifyPropertyChanged();
            }
        }

        public bool Events
        {
            get
            {
                return this._events;
            }
            set
            {
                this._events = value;
                base.NotifyPropertyChanged();
            }
        }

        public bool Contacts
        {
            get
            {
                return this._contacts;
            }
            set
            {
                this._contacts = value;
                base.NotifyPropertyChanged();
            }
        }

        public bool Articles
        {
            get
            {
                return this._articles;
            }
            set
            {
                this._articles = value;
                base.NotifyPropertyChanged();
            }
        }

        public Visibility DetailedFormVisibility
        {
            get
            {
                return (this._information == null || this.Type != VKGroupType.Page).ToVisiblity();
            }
        }

        public Visibility SimpleFormVisibility
        {
            get
            {
                return (this._information != null && this.Type == VKGroupType.Page).ToVisiblity();
            }
        }

        public bool IsCommentsChecked
        {
            get
            {
                return this.WallOrComments == CommunityServiceState.Opened;
            }
            set
            {
                this.WallOrComments = value ? CommunityServiceState.Opened : CommunityServiceState.Disabled;
            }
        }

        public bool IsPhotosChecked
        {
            get
            {
                return this.Photos == CommunityServiceState.Opened;
            }
            set
            {
                this.Photos = value ? CommunityServiceState.Opened : CommunityServiceState.Disabled;
            }
        }

        public bool IsVideosChecked
        {
            get
            {
                return this.Videos == CommunityServiceState.Opened;
            }
            set
            {
                this.Videos = value ? CommunityServiceState.Opened : CommunityServiceState.Disabled;
            }
        }

        public bool IsAudiosChecked
        {
            get
            {
                return this.Audios == CommunityServiceState.Opened;
            }
            set
            {
                this.Audios = value ? CommunityServiceState.Opened : CommunityServiceState.Disabled;
            }
        }

        public bool IsDiscussionsChecked
        {
            get { return this.Discussions == CommunityServiceState.Opened; }
            set { this.Discussions = value ? CommunityServiceState.Opened : CommunityServiceState.Disabled; }
        }

        public string WallStateString
        {
            get { return ServicesViewModel.GetStateString(this.WallOrComments, true); }
        }

        public string PhotosStateString
        {
            get { return ServicesViewModel.GetStateString(this.Photos, false); }
        }

        public string VideosStateString
        {
            get { return ServicesViewModel.GetStateString(this.Videos, false); }
        }

        public string AudiosStateString
        {
            get { return ServicesViewModel.GetStateString(this.Audios, false); }
        }

        public string DocumentsStateString
        {
            get { return ServicesViewModel.GetStateString(this.Documents, false); }
        }

        public string DiscussionsStateString
        {
            get { return ServicesViewModel.GetStateString(this.Discussions, false); }
        }


        public bool IsStrongLanguageFilterEnabled
        {
            get
            {
                return this._isStrongLanguageFilterEnabled;
            }
            set
            {
                this._isStrongLanguageFilterEnabled = value;
                this.NotifyPropertyChanged<bool>((() => this.IsStrongLanguageFilterEnabled));
            }
        }

        public bool IsKeyWordsFilterEnabled
        {
            get
            {
                return this._isKeyWordsFilterEnabled;
            }
            set
            {
                this._isKeyWordsFilterEnabled = value;
                this.NotifyPropertyChanged<bool>((() => this.IsKeyWordsFilterEnabled));
                this.NotifyPropertyChanged<Visibility>((() => this.KeyWordsFieldVisibility));
            }
        }

        public string KeyWords
        {
            get
            {
                return this._keyWords;
            }
            set
            {
                this._keyWords = value;
                this.NotifyPropertyChanged<string>((() => this.KeyWords));
            }
        }

        public Visibility KeyWordsFieldVisibility
        {
            get
            {
                return this.IsKeyWordsFilterEnabled.ToVisiblity();
            }
        }

        public void Read(VKCommunitySettings information)
        {
            this._information = information;
            this.WallOrComments = (CommunityServiceState)this._information.wall;
            if (this.Type != VKGroupType.Page)
            {
                this.Documents = (CommunityServiceState)this._information.docs;
            }
            else
            {
                this.Links = this._information.links == 1;
                this.Events = this._information.events == 1;
                this.Contacts = this._information.contacts == 1;
            }
            this.Photos = (CommunityServiceState)information.photos;
            this.Videos = (CommunityServiceState)information.video;
            this.Audios = (CommunityServiceState)information.audio;
            this.Discussions = (CommunityServiceState)this._information.topics;
            base.NotifyPropertyChanged<Visibility>((() => this.DetailedFormVisibility));
            base.NotifyPropertyChanged<Visibility>((() => this.SimpleFormVisibility));
            this.IsStrongLanguageFilterEnabled = information.obscene_filter;
            this.IsKeyWordsFilterEnabled = information.obscene_stopwords;

            this.Market = information.market == null ? false : information.market.enabled;
            this.Messages = information.messages;
            this.Articles = information.articles;


            if (this._information.obscene_words != null)
            {
                foreach (string obsceneWord in this._information.obscene_words)
                    this.KeyWords = !(this.KeyWords == "") ? this.KeyWords + ", " + obsceneWord : obsceneWord;
            }
            
            this.AvailableMainSections = information.sections_list.Select((i )=>new VKCommunitySettings.Section() { id = (int)(i as JArray)[0], name = (string)(i as JArray)[1] }).ToList();
            base.NotifyPropertyChanged(nameof(this.AvailableMainSections));

            this.MainSection = this.AvailableMainSections.First((s)=>s.id == information.main_section);
            this.SecondarySection = this.AvailableMainSections.First((s) => s.id == information.secondary_section);

            
           

        }

        public void SaveChanges()
        {
            this.SetInProgress(true);
            this.IsFormEnabled = false;

            Dictionary<string, string> infoParams = new Dictionary<string, string>();

            infoParams["wall"] = ((int)this.WallOrComments).ToString();
            infoParams["photos"] = ((int)this.Photos).ToString();
            infoParams["video"] = ((int)this.Videos).ToString();
            infoParams["audio"] = ((int)this.Audios).ToString();
            infoParams["docs"] = ((int)this.Documents).ToString();
            infoParams["topics"] = ((int)this.Discussions).ToString();
            infoParams["links"] = this.Links ? "1" : "0";
            infoParams["events"] = this.Events ? "1" : "0";
            infoParams["contacts"] = this.Contacts ? "1" : "0";
            infoParams["obscene_filter"] = this.IsStrongLanguageFilterEnabled ? "1" : "0";
            infoParams["obscene_stopwords"] = this.IsKeyWordsFilterEnabled ? "1" : "0";
            infoParams["obscene_words"] = this.KeyWords;

            infoParams["articles"] = this.Articles ? "1" : "0";
            infoParams["messages"] = this.Messages ? "1" : "0";

            infoParams["market"] = this.Market ? "1" : "0";

            infoParams["main_section"] = this.MainSection.id.ToString();
            
                infoParams["secondary_section"] = this.SecondarySection.id.ToString();
            


            //GroupsService.Instance.SetCommunityServices(this._communityId, (int)this.WallOrComments, (int)this.Photos, (int)this.Videos, (int)this.Audios, (int)this.Documents, (int)this.Discussions, this.Links ? 1 : 0, this.Events ? 1 : 0, this.Contacts ? 1 : 0, this.IsStrongLanguageFilterEnabled ? 1 : 0, this.IsKeyWordsFilterEnabled ? 1 : 0, this.KeyWords, (result) =>
            GroupsService.Instance.SetCommunityInformation(this._communityId, infoParams, (result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    GenericInfoUC.ShowBasedOnResult("Saved", result.error);

                    this.IsFormEnabled = true;
                    this.SetInProgress(false);
                });
            });
        }

        private static string GetStateString(CommunityServiceState state, bool isWall)
        {
            switch (state)
            {
                case CommunityServiceState.Disabled:
                    return LocalizedStrings.GetString(isWall ? "Disabled_Form1" : "Disabled_Form2");
                case CommunityServiceState.Opened:
                    return LocalizedStrings.GetString(isWall ? "Opened_Form1" : "Opened_Form2");
                case CommunityServiceState.Limited:
                    return LocalizedStrings.GetString(isWall ? "Limited_Form1" : "Limited_Form2");
                case CommunityServiceState.Closed:
                    return LocalizedStrings.GetString("Closed/Text");
                default:
                    return "";
            }
        }









        public enum CommunityServiceState
        {
            Disabled,
            Opened,
            Limited,
            Closed,
        }

        public enum CommunityService
        {
            Wall,
            Photos,
            Videos,
            Audios,
            Documents,
            Discussions,
        }
    }
}
