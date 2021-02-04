using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace LunaVK.UC
{
    public sealed partial class ManagerEditingUC : UserControl
    {/*
        public ManagerEditingUC()
        {
            this.InitializeComponent();
        }
        */
        public ManagerEditingUC(uint communityId, VKUser user, bool isEditing, VKGroupContact contact)// : this()
        {
            base.DataContext = new ManagerEditingViewModel(communityId,user, isEditing, contact);

            this.InitializeComponent();
        }

        

        public sealed class ManagerEditingViewModel : ViewModelBase
        {
            public ManagerEditingViewModel(uint communityId, VKUser manager, bool isEditing, VKGroupContact contact)
            {
                this._communityId = communityId;
                this._manager = manager;
                this._isEditing = isEditing;

                this._contact = contact;

                if (contact != null)
                {
                    this.Position = contact.desc;
                    this.Email = contact.email;
                    this.Phone = contact.phone;
                    this.IsContact = true;
                }

                switch (manager.role)
                {
                    case Core.Enums.CommunityManagementRole.Moderator:
                        this.IsModeratorSelected = new bool?(true);
                        break;
                    case Core.Enums.CommunityManagementRole.Editor:
                        this.IsEditorSelected = new bool?(true);
                        break;
                    case Core.Enums.CommunityManagementRole.Administrator:
                        this.IsAdministratorSelected = new bool?(true);
                        break;
                }
            }

            public string Position { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }

            private readonly uint _communityId;
            private readonly VKUser _manager;
            private readonly bool _isEditing;
            private readonly VKGroupContact _contact;

            private bool? _isModeratorSelected;
            private bool? _isEditorSelected;
            private bool? _isAdministratorSelected;

            public bool IsFormCompleted
            {
                get
                {
                    //if (this._isEditing)
                    //    return true;
                    bool? nullable = this.IsModeratorSelected;
                    
                    if ((nullable.GetValueOrDefault() == true ? (nullable.HasValue ? true : false) : false) == false)
                    {
                        nullable = this.IsEditorSelected;
                        
                        if ((nullable.GetValueOrDefault() == true ? (nullable.HasValue ? true : false) : false) == false)
                        {
                            nullable = this.IsAdministratorSelected;
                            
                            if (nullable.GetValueOrDefault() != true)
                                return false;
                            return nullable.HasValue;
                        }
                    }
                    return true;
                }
            }

            public bool? IsModeratorSelected
            {
                get { return this._isModeratorSelected; }
                set
                {
                    this._isModeratorSelected = value;
                    this.NotifyPropertyChanged<bool?>((() => this.IsModeratorSelected));
                    this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
                }
            }

            public bool? IsEditorSelected
            {
                get { return this._isEditorSelected; }
                set
                {
                    this._isEditorSelected = value;
                    this.NotifyPropertyChanged<bool?>((() => this.IsEditorSelected));
                    this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
                }
            }

            public bool? IsAdministratorSelected
            {
                get { return this._isAdministratorSelected; }
                set
                {
                    this._isAdministratorSelected = value;
                    this.NotifyPropertyChanged<bool?>((() => this.IsAdministratorSelected));
                    this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
                }
            }

            public string ManagerPhoto
            {
                get { return this._manager.MinPhoto; }
            }

            public string ManagerName
            {
                get
                {
                    return this._manager.first_name;
                }
            }

            public string Case
            {
                get
                {
                    if (!this._isEditing)
                        return string.Format(LocalizedStrings.GetString("WillBeCommunityManager"), this._manager.first_name_acc);
                    return LocalizedStrings.GetString("IsCommunityManager");
                }
            }

            private bool _isContact;

            public bool IsContact
            {
                get
                {
                    return this._isContact;
                }
                set
                {
                    this._isContact = value;
                    base.NotifyPropertyChanged();
                }
            }

            public Visibility RemoveButtonVisibility
            {
                get { return this._isEditing.ToVisiblity(); }
            }
        }
    }
}
