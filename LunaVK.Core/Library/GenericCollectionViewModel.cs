using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Network;
using LunaVK.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LunaVK.Core.Library
{
    public abstract class GenericCollectionViewModel<T> : ViewModelBase, ISupportDownIncrementalLoading, ISupportReload
        where T : class
    {
        public ObservableCollection<T> Items { get; private set; }

        /// <summary>
        /// 30
        /// </summary>
        public short LoadCount = 30;

        /// <summary>
        /// 40
        /// </summary>
        public short ReloadCount = 40;
        public uint? _totalCount;
        public string _nextFrom;

        public string FooterText
        {
            get
            {
                //if (this.StatusText != "" || !this.IsFullyLoaded && (!this.NeedCollectionCountBeforeFullyLoading || !this.IsLoaded))
                //    return "";
                //int count = this.GetCollectionCount();
                //if (this.NeedCollectionCountBeforeFullyLoading && this._totalCount > 0)

                if(this._totalCount.HasValue == false)
                {
                    return "";
                }

                //uint count = this._totalCount.Value;
                //if (count == 0 && !string.IsNullOrEmpty(this.NoItemsDescription) || count == 0 && (!string.IsNullOrEmpty(this.NoContentImage) || !string.IsNullOrEmpty(this.NoContentText)))
                //    return "";
                return this.GetFooterTextForCount;
            }
        }


        public GenericCollectionViewModel()
        {
            this.Items = new ObservableCollection<T>();
        }
        
        public virtual bool HasMoreDownItems
        {
            get
            {
                if (this._nextFrom == null)
                {
                    if (!this._totalCount.HasValue)//Ещё не загружали данные, а если конец?
                        return this.Items.Count == 0;

                    return this.Items.Count < this._totalCount.Value;
                }
                return !string.IsNullOrEmpty(this._nextFrom);
            }
        }

        /// <summary>
        /// this._totalCount = null;
        ///this._nextFrom = null;
        ///this.Items.Clear();
        /// </summary>
        public virtual void OnRefresh()
        {
            this._totalCount = null;
            this._nextFrom = null;
            this.Items.Clear();
            //
            //
            base.NotifyPropertyChanged(nameof(this.FooterText));
        }

        public void Reload()
        {
            this.OnRefresh();
            this.LoadDownAsync(true);
        }

        public ProfileLoadingStatus CurrentLoadingStatus { get; /*private*/ set; }
        public Action<ProfileLoadingStatus> LoadingStatusUpdated { get; set; }

        private void UpdateLoadingStatus(ProfileLoadingStatus status)
        {
            this.CurrentLoadingStatus = status;
            base.NotifyPropertyChanged(nameof(this.FooterText));
            base.NotifyPropertyChanged(nameof(this.StatusText));
            Execute.ExecuteOnUIThread(() => { this.LoadingStatusUpdated?.Invoke(status); });
        }

        private VKError _error;

        //public void LoadData(bool refresh = false, bool suppressLoadingMessage = false, Action<BackendResult<B, ResultCode>> callback = null, bool clearCollectionOnRefresh = false)
        public void LoadDownAsync(bool refresh = false)
        {
            if(refresh)
            {
//                if (this.Items.Count == 0)//А зачем нам показывать прогресс, если у нас есть элементы?
                    this.UpdateLoadingStatus(ProfileLoadingStatus.Reloading);
            }
            else
                this.UpdateLoadingStatus(ProfileLoadingStatus.Loading);

            this.GetData(refresh ? 0 : this.Items.Count, refresh ? this.LoadCount : this.ReloadCount, (error,items) =>
            {
                //if (_mainVM != null)
                //    _mainVM.SetInProgress(false, "");
                this._error = error;
                if (error.error_code == VKErrors.None)
                {
                    if (items != null)//попробуем здесь поставить
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            foreach (var item in items)
                                this.Items.Add(item);
                        });
                    }

                    if (this.CurrentLoadingStatus == ProfileLoadingStatus.Reloading || this.CurrentLoadingStatus == ProfileLoadingStatus.Loading)//Это было после обычной загрузки, подгрузки? Если было изменено в коде, то игнорим...
                    {
                        if (this._totalCount.HasValue)
                        {
                            if (this._totalCount.Value == 0)
                                this.UpdateLoadingStatus(ProfileLoadingStatus.Empty);
                            else
                                this.UpdateLoadingStatus(ProfileLoadingStatus.Loaded);
                        }
                        else
                        {
                            this.UpdateLoadingStatus(ProfileLoadingStatus.Loaded);//this.UpdateStatus(false, new ResultCode?());
                        }
                    }
                    /*
                    if (items != null)//так было здесь
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            foreach (var item in items)
                                this.Items.Add(item);
                        });
                    }
                    */
                }
                //else if(error== VKErrors.AccessDenied)
                //{
                //    this.UpdateLoadingStatus(ProfileLoadingStatus.Private);
                //}
                else
                {
                    if(refresh)
                    {
                            this.UpdateLoadingStatus(ProfileLoadingStatus.ReloadingFailed);
                    }
                    else
                    {
                        this.UpdateLoadingStatus(ProfileLoadingStatus.LoadingFailed);//this.UpdateStatus(false, new ResultCode?(res.ResultCode));
                    }
                    
                    
                    //this._isLoading = false;
                    //if (callback == null)
                    //    return;
                    //callback(res);
                }

                base.NotifyPropertyChanged(nameof(this.FooterText));
            });
        }

        public string StatusText
        {
            get
            {
                switch(this.CurrentLoadingStatus)
                {
                    case ProfileLoadingStatus.Reloading:
                        {
                            return LocalizedStrings.GetString("Loading/Text");
                        }
                    case ProfileLoadingStatus.Deleted:
                        {
                            return LocalizedStrings.GetString("UserDeleted");
                        }
                    case ProfileLoadingStatus.Banned:
                        {
                            return LocalizedStrings.GetString("UserBanned");
                        }
                    case ProfileLoadingStatus.Blacklisted:
                        {
                            return LocalizedStrings.GetString("UserBlacklisted");
                        }
                    case ProfileLoadingStatus.Empty:
                    case ProfileLoadingStatus.Loaded:
                    case ProfileLoadingStatus.Loading:
                        {
                            return String.Empty;
                        }
                    default:
                        {
                            if(this._error.error_code == VKErrors.NoNetwork)
                            {
                                return LocalizedStrings.GetString("FailedToConnectError").Replace("\\r\\n", Environment.NewLine);
                            }

                            //return string.Format(LocalizedStrings.GetString("Error_Generic"), this._error.error_msg).Replace("\\r\\n", Environment.NewLine);
                            return this._error.error_msg + " ("+ (int)this._error.error_code+ ")";
                        }
                }
            }
        }
        
        public virtual string GetFooterTextForCount { get; }

        public abstract void GetData(int offset, int count, Action<VKError, IReadOnlyList<T>> callback);
    }
}

/*
 * public interface ICollectionDataProvider<B, T> where B : class where T : class
  {
    Func<B, ListWithCount<T>> ConverterFunc { get; }

    void GetData(GenericCollectionViewModel<B, T> caller, int offset, int count, Action<BackendResult<B, ResultCode>> callback);

    string GetFooterTextForCount(GenericCollectionViewModel<B, T> caller, int count);
  }

    */