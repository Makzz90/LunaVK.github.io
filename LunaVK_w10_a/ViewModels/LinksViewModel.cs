using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace LunaVK.ViewModels
{
    public class LinksViewModel : GenericCollectionViewModel<VKGroupLink>
    {
        public readonly uint CommunityId;
        private bool _updatingCollection;
        //public Visibility AddButtonVisibility { get; private set; }

        public LinksViewModel(uint communityId)
        {
            this.CommunityId = communityId;
            base.Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (this._updatingCollection || e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                return;

            int index = e.NewStartingIndex;
            uint after = 0;
            if (index > 0)
                after = base.Items[index - 1].id;

            GroupsService.Instance.ReorderLink(this.CommunityId, base.Items[index].id, after, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {

                }
            });
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKGroupLink>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<List<VKGroup>>("groups.getById", new Dictionary<string, string>()
            {
                { "group_id", this.CommunityId.ToString() },
                { "fields", "links" }
            }, (result)=> {
                if(result.error.error_code== VKErrors.None)
                {
                    if (result.response[0].links != null)
                    {
                        base._totalCount = (uint)result.response[0].links.Count;

                        Execute.ExecuteOnUIThread(() =>
                        {
                            foreach (var item in result.response[0].links)
                            {
                                this.AddLink(item);
                            }
                        });

                    }
                }
                else
                {
                    /*
                    result.error.error_code = VKErrors.None;
                    List<VKGroupLink> list = new List<VKGroupLink>();
                    list.Add(new VKGroupLink() { name = "01", desc = "descr 1", id = 1 });
                    list.Add(new VKGroupLink() { name = "02", desc = "descr 2", id = 2 });

                    result.response = new List<VKGroup>();
                    result.response.Add(new VKGroup() { links = list });

                    callback(result.error, null);

                    Execute.ExecuteOnUIThread(() =>
                    {
                        //this.ClearItems();
                        foreach (var item in result.response[0].links)
                        {
                            this.AddLink(item);
                        }
                        //this.NotifyProperties();
                    });
                    */
                }

                callback(result.error, null);
            });
        }

        public override string GetFooterTextForCount
        {
            get
            {
                if (base._totalCount <= 0)
                    return LocalizedStrings.GetString("NoLinks");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneLinkFrm", "TwoFourLinksFrm", "FiveLinksFrm");
            }
        }

        public void DeleteLink(VKGroupLink item)
        {
            base.SetInProgress(true);
            GroupsService.Instance.DeleteLink(this.CommunityId, item.id, (result) =>
            {
                base.SetInProgress(false);
                if (result.error.error_code == VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        base.Items.Remove(item);
                        base._totalCount--;
                        base.NotifyPropertyChanged(nameof(base.StatusText));
                    });
                    //else
                    //    GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
                }
            });
        }

        public void AddLink(VKGroupLink item)
        {
            this._updatingCollection = true;
            base.Items.Add(item);
            base._totalCount++;
            base.NotifyPropertyChanged(nameof(base.StatusText));
            this._updatingCollection = false;
        }
    }
}
