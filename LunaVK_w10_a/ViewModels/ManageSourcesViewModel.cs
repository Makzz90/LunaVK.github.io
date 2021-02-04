using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public class ManageSourcesViewModel
    {
        public GenericCollectionUsers UsersVM { get; private set; }

        public GenericCollectionGroups GroupsVM { get; private set; }

        private bool _hidenSourcesMode;

        

        public ManageSourcesViewModel(bool hidenSourcesMode)
        {
            this.UsersVM = new GenericCollectionUsers(hidenSourcesMode);
            this.GroupsVM = new GenericCollectionGroups(hidenSourcesMode);

            this._hidenSourcesMode = hidenSourcesMode;
        }

        public string Title
        {
            get { return LocalizedStrings.GetString(this._hidenSourcesMode ? "HiddenSources" : "Settings_Nofications_Sources/Text"); }
        }

        public class GenericCollectionUsers : GenericCollectionViewModel<VKUser>
        {
            private bool _hidenSourcesMode;

            public GenericCollectionUsers(bool hidenSourcesMode)
            {
                this._hidenSourcesMode = hidenSourcesMode;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
            {
                if(this._hidenSourcesMode)
                {
                    NewsFeedService.Instance.GetBanned((result) =>
                    {
                        if (result.error.error_code == VKErrors.None)
                        {
                            base._totalCount = (uint)result.response.profiles.Count;
                            callback(result.error, result.response.profiles);
                        }
                        else
                        {
                            callback(result.error, null);
                        }
                    });
                }
                else
                {
                    WallService.Instance.GetWallSubscriptionsProfiles(offset, count, (result) =>
                    {
                        if (result.error.error_code == VKErrors.None)
                        {
                            base._totalCount = (uint)result.response.count;
                            callback(result.error, result.response.items);
                        }
                        else
                        {
                            callback(result.error, null);
                        }
                    });
                }
                
            }

            internal void DeleteSelected(List<VKUser> list)
            {
                if (this._hidenSourcesMode)
                {
                    NewsFeedService.Instance.DeleteBan(list.Select(fh => (uint)fh.Id).ToList(),null,(result)=>
                    //AccountService.Instance.UnbanUsers(list.Select(fh => fh.Id).ToList(), (result) =>
                    {
                        if (result==true)
                        {
                            Execute.ExecuteOnUIThread(() =>
                            {
                                foreach (var item in list)
                                {
                                    base.Items.Remove(item as VKUser);
                                    base._totalCount--;
                                }
                                list.Clear();
                                base.NotifyPropertyChanged(nameof(base.FooterText));
                            });
                        }

                    });
                }
                else
                {
                    WallService.Instance.WallSubscriptionsUnsubscribe(list.Select(fh => fh.Id).ToList(), (result) =>
                    {
                        if (result.error.error_code == VKErrors.None)
                        {
                            Execute.ExecuteOnUIThread(() =>
                            {
                                foreach (var item in list)
                                {
                                    base.Items.Remove(item as VKUser);
                                    base._totalCount--;
                                }
                                list.Clear();
                                base.NotifyPropertyChanged(nameof(base.FooterText));
                            });
                        }

                    });
                }
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("NoPersons");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OnePersonFrm", "TwoFourPersonsFrm", "FivePersonsFrm");
                }
            }
            
        }

        public class GenericCollectionGroups : GenericCollectionViewModel<VKGroup>
        {
            private bool _hidenSourcesMode;

            public GenericCollectionGroups(bool hidenSourcesMode)
            {
                this._hidenSourcesMode = hidenSourcesMode;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKGroup>> callback)
            {
                if(this._hidenSourcesMode)
                {
                    NewsFeedService.Instance.GetBanned((result) =>
                    {
                        if (result.error.error_code == VKErrors.None)
                        {
                            base._totalCount = (uint)result.response.groups.Count;
                            callback(result.error, result.response.groups);
                        }
                        else
                        {
                            callback(result.error, null);
                        }
                    });
                }
                else
                {
                    WallService.Instance.GetWallSubscriptionsGroups(offset, count, (result) =>
                    {
                        if (result.error.error_code == VKErrors.None)
                        {
                            base._totalCount = (uint)result.response.count;
                            callback(result.error, result.response.items);
                        }
                        else
                        {
                            callback(result.error, null);
                        }
                    });
                }
                
            }

            internal void DeleteSelected(List<VKGroup> list)
            {
                if (this._hidenSourcesMode)
                {
                    NewsFeedService.Instance.DeleteBan(null,list.Select(fh => (uint)fh.Id).ToList(), (result) =>
                    {
                        if (result == true)
                        {
                            Execute.ExecuteOnUIThread(() =>
                            {
                                foreach (var item in list)
                                {
                                    base.Items.Remove(item as VKGroup);
                                    base._totalCount--;
                                }
                                list.Clear();
                                base.NotifyPropertyChanged(nameof(base.FooterText));
                            });
                        }
                    });
                }
                else
                {
                    WallService.Instance.WallSubscriptionsUnsubscribe(list.Select(fh => (-fh.Id)).ToList(), (result) =>
                    {
                        if (result.error.error_code == VKErrors.None)
                        {
                            Execute.ExecuteOnUIThread(() =>
                            {
                                foreach (var item in list)
                                {
                                    base.Items.Remove(item as VKGroup);
                                    base._totalCount--;
                                }
                                list.Clear();
                                base.NotifyPropertyChanged(nameof(base.FooterText));
                            });
                        }

                    });
                }
            }
            /*
            public override string GetFooterTextForCount
            {//original
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("NoCommunites");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneCommunityFrm", "TwoFourCommunitiesFrm", "FiveCommunitiesFrm");
                }
            }
            */
            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount == 0)
                        return LocalizedStrings.GetString("NoPages");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneGroup", "TwoFourGroupsFrm", "FiveMoreGroupsFrm");
                }
            }
        }
    }
}
