using LunaVK.Common;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.Photo;
using LunaVK.UC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.ViewModels
{
    public class RegistrationViewModel : ViewModelBase, IBinarySerializable
    {
        private RegistrationProfileViewModel _registrationProfileVM = new RegistrationProfileViewModel();
        private RegistrationPhoneNumberViewModel _registrationPhoneNumberVM = new RegistrationPhoneNumberViewModel();
        private RegistrationPasswordViewModel _registrationPasswordVM = new RegistrationPasswordViewModel();
        private RegistrationAddFriendsViewModel _registrationAddFriendsVM = new RegistrationAddFriendsViewModel();
        private RegistrationInterestingPagesViewModel _registrationInterestingPagesVM = new RegistrationInterestingPagesViewModel();
        private string _sid = "";
        private Action _onMovedForward = (() => { });
        private RegistrationConfirmationCodeViewModel _registrationConfirmationCodeVM;
        private object _currentVM;//ICompleteable
        private bool _friendsSearchDataLoaded;

        public Action OnMovedForward
        {
            get
            {
                return this._onMovedForward;
            }
            set
            {
                if (value == null)
                    return;
                this._onMovedForward = value;
            }
        }

        public string Title
        {
            get
            {
                if (this.Step1Visibility == Visibility.Visible)
                    return "Registration_Title_Registration";
                if (this.Step2Visibility == Visibility.Visible)
                    return "Registration_Title_PhoneNumber";
                if (this.Step3Visibility == Visibility.Visible)
                    return "Registration_Title_Confirmation";
                if (this.Step4Visibility == Visibility.Visible)
                    return "Registration_Title_Password";
                if (this.Step5Visibility == Visibility.Visible)
                    return "Registration_Title_FindFriends";
                if (this.Step6Visibility == Visibility.Visible)
                    return "Registration_Title_InterestingPages";
                return "";
            }
        }
        
        public object CurrentVM
        {
            get
            {
                return this._currentVM;
            }
            private set
            {
                if (this._currentVM != null)
                    (this._currentVM as INotifyPropertyChanged).PropertyChanged -= this.ChildViewModel_PropertyChanged;
                this._currentVM = value;
                if (this._currentVM != null)
                    (this._currentVM as INotifyPropertyChanged).PropertyChanged += this.ChildViewModel_PropertyChanged;
                if (this._currentVM == this._registrationInterestingPagesVM)
                    this._registrationInterestingPagesVM.EnsureLoadData();
                if (this._currentVM == this._registrationAddFriendsVM && !this._friendsSearchDataLoaded)
                {
//                    this._registrationAddFriendsVM.FriendsSearchVM.LoadData();
                    this._friendsSearchDataLoaded = true;
                }
                base.NotifyPropertyChanged(nameof(this.CurrentVM));
                base.NotifyPropertyChanged<bool>(() => this.CanCompleteCurrentStep);
                base.NotifyPropertyChanged<Visibility>(() => this.Step1Visibility);
                base.NotifyPropertyChanged<Visibility>(() => this.Step2Visibility);
                base.NotifyPropertyChanged<Visibility>(() => this.Step3Visibility);
                base.NotifyPropertyChanged<Visibility>(() => this.Step4Visibility);
                base.NotifyPropertyChanged<Visibility>(() => this.Step5Visibility);
                base.NotifyPropertyChanged<Visibility>(() => this.Step6Visibility);
                base.NotifyPropertyChanged<string>(() => this.Title);
            }
        }
        
        public bool CanCompleteCurrentStep
        {
            get
            {
                return false;//return this.CurrentVM.IsCompleted;
            }
        }

        public RegistrationProfileViewModel RegistrationProfileVM
        {
            get
            {
                return this._registrationProfileVM;
            }
        }

        public RegistrationPhoneNumberViewModel RegistrationPhoneNumberVM
        {
            get
            {
                return this._registrationPhoneNumberVM;
            }
        }

        public RegistrationAddFriendsViewModel RegistrationAddFriendsVM
        {
            get
            {
                return this._registrationAddFriendsVM;
            }
        }

        public RegistrationInterestingPagesViewModel RegistrationInterestingPagesVM
        {
            get
            {
                return this._registrationInterestingPagesVM;
            }
        }

        public RegistrationConfirmationCodeViewModel RegistrationConfirmationCodeVM
        {
            get
            {
                return this._registrationConfirmationCodeVM;
            }
            private set
            {
                this._registrationConfirmationCodeVM = value;
                base.NotifyPropertyChanged<RegistrationConfirmationCodeViewModel>(() => this.RegistrationConfirmationCodeVM);
            }
        }

        public RegistrationPasswordViewModel RegistrationPasswordVM
        {
            get
            {
                return this._registrationPasswordVM;
            }
        }

        public Visibility Step1Visibility
        {
            get
            {
                if (this.CurrentStep != 1)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility Step2Visibility
        {
            get
            {
                if (this.CurrentStep != 2)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility Step3Visibility
        {
            get
            {
                if (this.CurrentStep != 3)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility Step4Visibility
        {
            get
            {
                if (this.CurrentStep != 4)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility Step5Visibility
        {
            get
            {
                if (this.CurrentStep != 5)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility Step6Visibility
        {
            get
            {
                if (this.CurrentStep != 6)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public int CurrentStep
        {
            get
            {
                if (this.CurrentVM == this.RegistrationProfileVM)
                    return 1;
                if (this.CurrentVM == this.RegistrationPhoneNumberVM)
                    return 2;
                if (this.CurrentVM is RegistrationConfirmationCodeViewModel)
                    return 3;
                if (this.CurrentVM == this.RegistrationPasswordVM)
                    return 4;
                if (this.CurrentVM == this.RegistrationAddFriendsVM)
                    return 5;
                return this.CurrentVM == this.RegistrationInterestingPagesVM ? 6 : 0;
            }
            private set
            {
                switch (value)
                {
                    case 1:
                        this.CurrentVM = this.RegistrationProfileVM;
                        break;
                    case 2:
                        this.CurrentVM = this.RegistrationPhoneNumberVM;
                        break;
                    case 3:
                        this.CurrentVM = this.RegistrationConfirmationCodeVM;
                        break;
                    case 4:
                        this.CurrentVM = this.RegistrationPasswordVM;
                        break;
                    case 5:
                        this.CurrentVM = this.RegistrationAddFriendsVM;
                        break;
                    case 6:
                        this.CurrentVM = this.RegistrationInterestingPagesVM;
                        break;
                }
            }
        }

        public RegistrationViewModel()
        {
            this.CurrentVM = this._registrationProfileVM;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write<RegistrationProfileViewModel>(this._registrationProfileVM, false);
            writer.Write<RegistrationPhoneNumberViewModel>(this._registrationPhoneNumberVM, false);
            writer.Write<RegistrationConfirmationCodeViewModel>(this._registrationConfirmationCodeVM, false);
            writer.Write<RegistrationPasswordViewModel>(this._registrationPasswordVM, false);
            writer.WriteString(this._sid);
            writer.Write(this.CurrentStep);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this._registrationProfileVM = reader.ReadGeneric<RegistrationProfileViewModel>();
            this._registrationPhoneNumberVM = reader.ReadGeneric<RegistrationPhoneNumberViewModel>();
            this._registrationConfirmationCodeVM = reader.ReadGeneric<RegistrationConfirmationCodeViewModel>();
            if (this._registrationConfirmationCodeVM != null)
                this._registrationConfirmationCodeVM.RequestVoiceCallAction = new Action<Action<bool>>(this.RequestVoiceCall);
            this._registrationPasswordVM = reader.ReadGeneric<RegistrationPasswordViewModel>();
            this._sid = reader.ReadString();
            this.CurrentStep = reader.ReadInt32();
        }

        private void ChildViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "IsCompleted"))
                return;
            // ISSUE: method reference
            base.NotifyPropertyChanged<bool>(() => this.CanCompleteCurrentStep);
        }

        internal bool HandleBackKey()
        {
            bool flag = false;
            if (this.Step2Visibility == Visibility.Visible)
            {
                this.CurrentVM = this.RegistrationProfileVM;
                flag = true;
            }
            else if (this.Step3Visibility == Visibility.Visible)
            {
                this.CurrentVM = this.RegistrationPhoneNumberVM;
                flag = true;
            }
            else if (this.Step4Visibility == Visibility.Visible)
            {
                this.CurrentVM = this.RegistrationConfirmationCodeVM;
                flag = true;
            }
            else if (this.Step6Visibility == Visibility.Visible)
            {
                this.CurrentVM = this.RegistrationAddFriendsVM;
                flag = true;
            }
            return flag;
        }

        public void CompleteCurrentStep()
        {
            if (this.IsInProgress/* || !this.CurrentVM.IsCompleted*/)
                return;
            this.SetInProgress(true, "");


            if (this.CurrentVM == this._registrationProfileVM)
            {
                this._registrationPhoneNumberVM.EnsureInitialized((res => Execute.ExecuteOnUIThread((() =>
                {
                    if (res)
                    {
                        this.CurrentVM = this._registrationPhoneNumberVM;
                        this.OnMovedForward();
                    }
                    this.SetInProgress(false, "");
                }))));
            }
            else if (this.CurrentVM == this._registrationPhoneNumberVM)
            {
 //               SignUpService.Instance.SignUp(this._registrationPhoneNumberVM.PhoneNumberString, this._registrationProfileVM.FirstName, this._registrationProfileVM.LastName, false, this._registrationProfileVM.IsMale, "", (Action<BackendResult<string, ResultCode>>)(res => Execute.ExecuteOnUIThread((() =>
//                {
                    this.SetInProgress(false, "");
 //                   GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", res.Error);
 //                   if (res.ResultCode != ResultCode.Succeeded)
 //                       return;
 //                   this.CurrentVM = this.CreateRegistrationConfirmationCodeViewModel();
                    this.OnMovedForward();
//                    this._sid = res.ResultData;
//                }))));
            }
            else if (this.CurrentVM == this._registrationConfirmationCodeVM)
            {
 //               SignUpService.Instance.ConfirmSignUp(this._registrationPhoneNumberVM.PhoneNumberString, this._registrationConfirmationCodeVM.ConfirmationCode, "", (res => Execute.ExecuteOnUIThread((() =>
 //               {
                    this.SetInProgress(false, "");
 //                   if (res.ResultCode == ResultCode.BadPassword)
//                    {
                        this.CurrentVM = this._registrationPasswordVM;
                        this.OnMovedForward();
//                    }
//                    else
//                        GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", res.Error);
//                }))));
            }
            else if (this.CurrentVM == this._registrationPasswordVM)
            {
                if ((this._registrationPasswordVM.PasswordStr).Contains(" "))
                {
                    new GenericInfoUC().ShowAndHideLater("PasswordCannotContainWhitespaces");
                    this.SetInProgress(false, "");
                }
                else
                {
 //                   SignUpService.Instance.ConfirmSignUp(this._registrationPhoneNumberVM.PhoneNumberString, this._registrationConfirmationCodeVM.ConfirmationCode, this._registrationPasswordVM.PasswordStr, (Action<BackendResult<SignupConfirmation, ResultCode>>)(res => Execute.ExecuteOnUIThread((() =>
 //                   {
                        this.SetInProgress(false, "");
//                        GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", res.Error);
//                        if (res.ResultCode != ResultCode.Succeeded || res.ResultData.success != 1)
//                            return;
                        this.PerformLogin((loginResult =>
                        {
                            if (!loginResult)
                                return;
                            Execute.ExecuteOnUIThread((() =>
                            {
                                this.CurrentVM = this._registrationAddFriendsVM;
                                this.OnMovedForward();
                            }));
                        }));
 //                   }))));
                }
            }
            else if (this.CurrentVM == this._registrationAddFriendsVM)
            {
                this.CurrentVM = this._registrationInterestingPagesVM;
                this.SetInProgress(false, "");
                this.OnMovedForward();
            }
            else
            {
                if (this.CurrentVM != this._registrationInterestingPagesVM)
                    return;
                //                NavigatorImpl.Instance.NavigateToMainPage();//newspage
            }

            
        }

        private void PerformLogin(Action<bool> resultCallback)
        {
            if (this.IsInProgress)
                return;
            this.SetInProgress(true, "");
            /*
            LoginService.Instance.GetAccessToken(this._registrationPhoneNumberVM.PhoneNumberString, this._registrationPasswordVM.PasswordStr, (result =>
            {
                this.SetInProgress(false, "");
                Execute.ExecuteOnUIThread((() =>
                {
                    if (result.ResultCode == ResultCode.Succeeded)
                    {
                        this.HandleLogin(result);
                        resultCallback(true);
                    }
                    else
                    {
                        GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", (VKRequestsDispatcher.Error)null);
                        resultCallback(false);
                    }
                }));
            }));
            */
            //
            //
            resultCallback(true);//BUG удалить строку
        }
        /*
        private void HandleLogin(BackendResult<AutorizationData, ResultCode> result)
        {
            ServiceLocator.Resolve<IAppStateInfo>().HandleSuccessfulLogin(result.ResultData, false);
            if (string.IsNullOrEmpty(this._registrationProfileVM.FullAvatarUri))
                return;
            SettingsEditProfileViewModel profileViewModel = new SettingsEditProfileViewModel();
            IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication();
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(this._registrationProfileVM.FullAvatarUri, (FileMode)3, (FileAccess)1);
                try
                {
                    memoryStream = StreamUtils.ReadFully((Stream)storageFileStream);
                }
                finally
                {
                    if (storageFileStream != null)
                        ((IDisposable)storageFileStream).Dispose();
                }
                memoryStream.Position = 0L;
                profileViewModel.UploadUserPhoto((Stream)memoryStream, this._registrationProfileVM.CropPhotoRect);
            }
            finally
            {
                if (storeForApplication != null)
                    ((IDisposable)storeForApplication).Dispose();
            }
        }
        
        private ICompleteable CreateRegistrationConfirmationCodeViewModel()
        {
            this.RegistrationConfirmationCodeVM = new RegistrationConfirmationCodeViewModel(this._registrationPhoneNumberVM.PhonePrefix, this._registrationPhoneNumberVM.PhoneNumber, new Action<Action<bool>>(this.RequestVoiceCall));
            return this.RegistrationConfirmationCodeVM;
        }
        */
        private void RequestVoiceCall(Action<bool> resultCallback)
        {
            if (this.IsInProgress)
                return;
            this.SetInProgress(true, "");
            /*
            SignUpService.Instance.SignUp(this._registrationPhoneNumberVM.PhoneNumberString, this._registrationProfileVM.FirstName, this._registrationProfileVM.LastName, true, this._registrationProfileVM.IsMale, this._sid, (Action<BackendResult<string, ResultCode>>)(res => Execute.ExecuteOnUIThread((() =>
            {
                this.SetInProgress(false, "");
                GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", null);
                resultCallback(res.ResultCode == ResultCode.Succeeded);
            }))));
            */
        }

        internal void SetUserPhoto(Stream stream, Rect rect)
        {
            this._registrationProfileVM.SetUserPhoto(stream, rect);
        }
















































        public class RegistrationProfileViewModel : ViewModelBase, IBinarySerializable//, ICompleteable
        {
            private string _firstName = "";
            private string _lastName = "";
            private string _fullAvatarUri = "";
            private bool _isMale;
            private bool _isGenderSet;
            private bool _havePhoto;
            private Rect _photoCropRect;

            public bool HavePhoto
            {
                get
                {
                    return this._havePhoto;
                }
                set
                {
                    this._havePhoto = value;
                    this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.AvatarUri));
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.HavePhoto));
                }
            }

            public Rect CropPhotoRect
            {
                get
                {
                    return this._photoCropRect;
                }
            }

            public string FullAvatarUri
            {
                get
                {
                    return this._fullAvatarUri;
                }
            }

            public bool IsCompleted
            {
                get
                {
                    if (!string.IsNullOrWhiteSpace(this.FirstName) && !string.IsNullOrWhiteSpace(this.LastName))
                        return this._isGenderSet;
                    return false;
                }
            }

            public string AvatarUri
            {
                get
                {
                    if (this._havePhoto)
                        return string.Concat("cropped", this._fullAvatarUri);
                    return "";
                }
            }

            public string FirstName
            {
                get
                {
                    return this._firstName;
                }
                set
                {
                    this._firstName = value;
                    this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.FirstName));
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsCompleted));
                }
            }

            public string LastName
            {
                get
                {
                    return this._lastName;
                }
                set
                {
                    this._lastName = value;
                    this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.LastName));
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsCompleted));
                }
            }

            public bool IsMale
            {
                get
                {
                    if (this._isGenderSet)
                        return this._isMale;
                    return false;
                }
                set
                {
                    this._isMale = value;
                    this._isGenderSet = true;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsMale));
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsFemale));
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsCompleted));
                }
            }

            public bool IsFemale
            {
                get
                {
                    if (this._isGenderSet)
                        return !this._isMale;
                    return false;
                }
                set
                {
                    this._isMale = !value;
                    this._isGenderSet = true;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsMale));
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsFemale));
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsCompleted));
                }
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write(1);
                writer.WriteString(this._firstName);
                writer.WriteString(this._lastName);
                writer.Write(this._isMale);
                writer.Write(this._isGenderSet);
                writer.Write(this._havePhoto);
                writer.Write(this._fullAvatarUri);
            }

            public void Read(BinaryReader reader)
            {
                reader.ReadInt32();
                this._firstName = reader.ReadString();
                this._lastName = reader.ReadString();
                this._isMale = reader.ReadBoolean();
                this._isGenderSet = reader.ReadBoolean();
                this._havePhoto = reader.ReadBoolean();
                this._fullAvatarUri = reader.ReadString();
            }

            internal void DeletePhoto()
            {
                this.HavePhoto = false;
            }

            internal void SetUserPhoto(Stream stream, Rect rect)
            {
                this._photoCropRect = rect;
                ImagePreprocessor.PreprocessImage(stream, 1500000, false, (imPR => Execute.ExecuteOnUIThread((() =>
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    Stream stream1 = imPR.Stream;
//                    bitmapImage.SetSource(stream1);
                    WriteableBitmap bmp = new WriteableBitmap(200,200/*bitmapImage*/);
                    /*
                    WriteableBitmap bitmap = bmp.Crop(new Rect()
                    {
                        Width = (double)bmp.PixelWidth * rect.Width,
                        Height = (double)bmp.PixelHeight * rect.Height,
                        Y = rect.Top * (double)bmp.PixelHeight,
                        X = rect.Left * (double)bmp.PixelWidth
                    });
                    */
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(this._fullAvatarUri))
                            File.Delete(this._fullAvatarUri);
                        using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            MemoryStream memoryStream = new MemoryStream();
//                            bitmap.SaveJpeg(memoryStream, bitmap.PixelWidth, bitmap.PixelHeight, 0, 90);
                            memoryStream.Position = 0L;
                            this._fullAvatarUri = Guid.NewGuid().ToString();
                            ImageCache.Current.TrySetImageForUri("cropped" + this._fullAvatarUri, memoryStream);
                            stream.Position = 0L;
                            using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(this._fullAvatarUri, FileMode.Create, FileAccess.Write))
                                StreamUtils.CopyStream(stream, storageFileStream);
                            this.HavePhoto = true;
                        }
                    }
                    catch
                    {
                    }
                }))));
            }
        }










































        public class RegistrationPhoneNumberViewModel : ViewModelBase, IBinarySerializable//, ICompleteable
        {
            private string _phonePrefix = "";
            private string _phoneNumber = "";
            private VKCountry _country;
            private bool _initialized;

            public string PhoneNumberString
            {
                get
                {
                    return this.PhonePrefix + this.PhoneNumber;
                }
            }

            public VKCountry VKCountry
            {
                get
                {
                    return this._country;
                }
                set
                {
                    this._country = value;
                    this.NotifyPropertyChanged(nameof(this.VKCountry));
                    if (this._country == null)
                        return;
                    Dictionary<string, long>.Enumerator enumerator = CountriesPhoneCodes.CodeToCountryDict.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<string, long> current = enumerator.Current;
                            if (current.Value == this._country.id)
                            {
                                this._phonePrefix = current.Key;
                                this.NotifyPropertyChanged(nameof(this.PhonePrefix));
                                this.NotifyPropertyChanged(nameof(this.PhoneNumberString));
                                this.NotifyPropertyChanged(nameof(this.IsCompleted));
                            }
                        }
                    }
                    finally
                    {
                        enumerator.Dispose();
                    }
                }
            }

            public string PhonePrefix
            {
                get
                {
                    return this._phonePrefix;
                }
                set
                {
                    this._phonePrefix = value;
                    this.NotifyPropertyChanged(nameof(this.PhonePrefix));
                    this.NotifyPropertyChanged(nameof(this.PhoneNumberString));
                    this.NotifyPropertyChanged(nameof(this.IsCompleted));
                }
            }

            public string PhoneNumber
            {
                get
                {
                    return this._phoneNumber;
                }
                set
                {
                    this._phoneNumber = value;
                    this.NotifyPropertyChanged(nameof(this.PhoneNumber));
                    this.NotifyPropertyChanged(nameof(this.PhoneNumberString));
                    this.NotifyPropertyChanged(nameof(this.IsCompleted));
                }
            }

            public bool IsCompleted
            {
                get
                {
                    if (!string.IsNullOrWhiteSpace(this.PhoneNumber))
                        return !string.IsNullOrWhiteSpace(this.PhonePrefix);
                    return false;
                }
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write(1);
                writer.WriteString(this._phonePrefix);
                writer.WriteString(this._phoneNumber);
                writer.Write<VKCountry>(this._country, false);
                writer.Write(this._initialized);
            }

            public void Read(BinaryReader reader)
            {
                reader.ReadInt32();
                this._phonePrefix = reader.ReadString();
                this._phoneNumber = reader.ReadString();
                this._country = reader.ReadGeneric<VKCountry>();
                this._initialized = reader.ReadBoolean();
            }

            public void EnsureInitialized(Action<bool> resultCallback)
            {
                if (this._initialized)
                {
                    resultCallback.Invoke(true);
                    return;
                }
                DatabaseService.Instance.GetNearbyCountries((result)=>
                {
                    Execute.ExecuteOnUIThread(()=>
                    {
                        if (result.error.error_code == Core.Enums.VKErrors.None)
                        {
                            this._initialized = true;
                            this.VKCountry = Enumerable.FirstOrDefault<VKCountry>(result.response.items);
                        }
                        else
                        {
                            new GenericInfoUC().ShowAndHideLater(result.error.error_msg);
                        }
                        resultCallback.Invoke(true/*result.error.error_code == Core.Enums.VKErrors.None*/);
                    });
                });
            }
        }

















        public class RegistrationPasswordViewModel : ViewModelBase, IBinarySerializable//, ICompleteable
        {
            private string _passwordStr;

            public bool IsCompleted
            {
                get
                {
                    return !string.IsNullOrEmpty(this.PasswordStr);
                }
            }

            public string PasswordStr
            {
                get
                {
                    return this._passwordStr;
                }
                set
                {
                    this._passwordStr = value;
                    this.NotifyPropertyChanged<string>(() => this.PasswordStr);
                    this.NotifyPropertyChanged<bool>(() => this.IsCompleted);
                }
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write(1);
                writer.WriteString(this._passwordStr);
            }

            public void Read(BinaryReader reader)
            {
                reader.ReadInt32();
                this._passwordStr = reader.ReadString();
            }
        }








        public class RegistrationAddFriendsViewModel : ViewModelBase//, ICompleteable
        {
/*
            private FriendsSearchViewModel _friendsSearchVM = new FriendsSearchViewModel(FriendsSearchMode.Register);

            public FriendsSearchViewModel FriendsSearchVM
            {
                get
                {
                    return this._friendsSearchVM;
                }
            }
*/
            public bool IsCompleted
            {
                get
                {
                    return true;
                }
            }
        }












        public class RegistrationInterestingPagesViewModel : ViewModelBase//, ICompleteable
        {
            private NewsFeedSuggestedSourcesViewModel _suggestedSourcesVM;
            private bool _isLoadedData;

            public NewsFeedSuggestedSourcesViewModel SuggestedSourcesVM
            {
                get
                {
                    return this._suggestedSourcesVM;
                }
            }

            public bool IsCompleted
            {
                get
                {
                    return true;
                }
            }

            public RegistrationInterestingPagesViewModel()
            {
                this._suggestedSourcesVM = new NewsFeedSuggestedSourcesViewModel();
            }

            internal void EnsureLoadData()
            {
                if (this._isLoadedData)
                    return;
//                this._suggestedSourcesVM.SuggestedSourcesVM.LoadData(true, false, null, false);
                this._isLoadedData = true;
            }
        }














        public class RegistrationConfirmationCodeViewModel : ViewModelBase, IBinarySerializable//, ICompleteable
        {
            private static readonly int _waitingTimeBeforeSecondAttempt = 60;
            private int _currentStep = 1;
            private Action<Action<bool>> _requestVoiceCallAction;
            private string _confirmationCode;
            private string _phoneNumber;
            private string _phonePrefix;
            private DispatcherTimer _localTimer;
            private DateTime _createdDateTime;
            private bool _isRequestingCall;

            public Action<Action<bool>> RequestVoiceCallAction
            {
                get
                {
                    return this._requestVoiceCallAction;
                }
                set
                {
                    this._requestVoiceCallAction = value;
                }
            }

            private int CurrentStep
            {
                get
                {
                    return this._currentStep;
                }
                set
                {
                    this._currentStep = value;
                    base.NotifyPropertyChanged<Visibility>(() => this.FirstAttemptVisibility);
                    base.NotifyPropertyChanged<Visibility>(() => this.SecondAttemptVisibility);
                    base.NotifyPropertyChanged<Visibility>(() => this.ThirdAttemptVisibility);
                }
            }

            public Visibility FirstAttemptVisibility
            {
                get
                {
                    if (this._currentStep != 1)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }

            public Visibility SecondAttemptVisibility
            {
                get
                {
                    if (this._currentStep != 2)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }

            public Visibility ThirdAttemptVisibility
            {
                get
                {
                    if (this._currentStep != 3)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }

            public int TotalSecondsFromCreatedTime
            {
                get
                {
                    return (int)(DateTime.Now - this._createdDateTime).TotalSeconds;
                }
            }

            public string CountdownStr
            {
                get
                {
                    return TimeSpan.FromSeconds((double)Math.Max(0, RegistrationConfirmationCodeViewModel._waitingTimeBeforeSecondAttempt - this.TotalSecondsFromCreatedTime)).ToString("mm\\:ss");
                }
            }

            public string ConfirmationCode
            {
                get
                {
                    return this._confirmationCode;
                }
                set
                {
                    this._confirmationCode = value;
                    base.NotifyPropertyChanged<string>(() => this.ConfirmationCode);
                    base.NotifyPropertyChanged<bool>(() => this.IsCompleted);
                }
            }

            public string PhoneNumberFormatted
            {
                get
                {
                    return string.Concat("+", this._phonePrefix, this._phoneNumber);
                }
            }

            public bool IsCompleted
            {
                get
                {
                    return !string.IsNullOrWhiteSpace(this.ConfirmationCode);
                }
            }

            public RegistrationConfirmationCodeViewModel()
            {
                this.InitTimer();
            }

            public RegistrationConfirmationCodeViewModel(string phonePrefix, string phoneNumber, Action<Action<bool>> requestVoiceCallAction)
            {
                this._phonePrefix = phonePrefix;
                this._phoneNumber = phoneNumber;
                this._requestVoiceCallAction = requestVoiceCallAction;
                this._createdDateTime = DateTime.Now;
                this.InitTimer();
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write(1);
                writer.WriteString(this._confirmationCode);
                writer.WriteString(this._phonePrefix);
                writer.WriteString(this._phoneNumber);
                BinarySerializerExtensions.Write(writer, this._createdDateTime);
            }

            public void Read(BinaryReader reader)
            {
                reader.ReadInt32();
                this._confirmationCode = reader.ReadString();
                this._phonePrefix = reader.ReadString();
                this._phoneNumber = reader.ReadString();
                this._createdDateTime = BinarySerializerExtensions.ReadDateTime(reader);
            }

            private void InitTimer()
            {
                this._localTimer = new DispatcherTimer();
                this._localTimer.Interval = TimeSpan.FromSeconds(0.5);
                this._localTimer.Tick += this._localTimer_Tick;
                this._localTimer.Start();
            }

            public void RequestCall()
            {
                if (this._isRequestingCall)
                {
                    return;
                }
                this._isRequestingCall = true;
                this._requestVoiceCallAction.Invoke(delegate (bool res)
                {
                    Execute.ExecuteOnUIThread(delegate
                    {
                        this._isRequestingCall = false;
                        if (res)
                        {
                            this.CurrentStep = 3;
                        }
                    });
                });
            }


            private void _localTimer_Tick(object sender, object e)
            {
                base.NotifyPropertyChanged<string>(() => this.CountdownStr);
                if (this.TotalSecondsFromCreatedTime >= RegistrationConfirmationCodeViewModel._waitingTimeBeforeSecondAttempt)
                {
                    this._localTimer.Stop();
                    this.CurrentStep = 2;
                }
            }
        }

































    }
}