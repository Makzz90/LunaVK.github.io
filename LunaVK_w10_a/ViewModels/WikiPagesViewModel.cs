using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.ViewModels
{
    public class WikiPagesViewModel : GenericCollectionViewModel<WikiPageSectionViewModel>
    {
        private uint _groupId;
        public string Title { get; private set; }

        //public List<string> Headers { get; private set; }

        public WikiPagesViewModel(uint groupId, string title)
        {
            this._groupId = groupId;
            this.Title = title;
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<WikiPageSectionViewModel>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = this._groupId.ToString();

            VKRequestsDispatcher.DispatchRequestToVK<List<VKWiki>>("pages.getTitles", parameters, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    base._totalCount = (uint)result.response.Count;
                    List<WikiPageSectionViewModel> list = new List<WikiPageSectionViewModel>();
                    foreach (var item in result.response)
                    {
                        list.Add(new WikiPageSectionViewModel(this._groupId, item.id, item.title));
                    }

                    //this.Headers = result.response.Select((i)=>i.title).ToList();
                    //base.NotifyPropertyChanged(nameof(this.Headers));
                    callback(result.error, list);
                }
                else
                {
                    callback(result.error, null);
                }
            });
        }
    }

    public sealed class WikiPageSectionViewModel : ViewModelBase
    {
        private readonly uint GroupId;
        private readonly uint Id;
        public string Title { get; private set; }
        public VKWiki wiki;
        public bool IsLoaded { get; private set; }

        public string ViewUrl
        {
            get
            {
                if (this.wiki == null)
                    return null;

                return this.wiki.view_url;
            }
        }

        public WikiPageSectionViewModel(uint groupId, uint id, string title)
        {
            this.GroupId = groupId;
            this.Id = id;
            this.Title = title;
        }

        public void LoadData()
        {
            if (this.IsLoaded)
                return;

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = (-this.GroupId).ToString();
            parameters["page_id"] = this.Id.ToString();

            base.SetInProgress(true);

            VKRequestsDispatcher.DispatchRequestToVK<VKWiki>("pages.get", parameters, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    base.SetInProgress(false);
                    this.IsLoaded = true;
                    this.wiki = result.response;
                    base.NotifyPropertyChanged(nameof(this.ViewUrl));
                }
            });
        }

        //Заглушка для PivotItem.Header
        public override string ToString()
        {
            return this.Title;
        }
    }
}
