using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using LunaVK.Core.Network;

namespace LunaVK.ViewModels
{
    public class NewsSearchViewModel : GenericCollectionViewModel<VKNewsfeedPost>
    {
        public string q = string.Empty;

        public Visibility StoryVisible
        {
            get { return Visibility.Collapsed; }
        }

        public Visibility StoryTitleVisible
        {
            get { return Visibility.Collapsed; }
        }

        public NewsSearchViewModel()
        {
            base.ReloadCount = 15;
            base.LoadCount = 15;
        }

        //public ObservableCollection<NewStory> Stories { get; private set; }

        public string UserPhoto
        {
            get { return Settings.LoggedInUserPhoto; }
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKNewsfeedPost>> callback)
        {
            if(offset==0)
                base.SetInProgress(true);

            NewsFeedService.Instance.Search(this.q, count, 0, 0, base._nextFrom, (result) =>
            {
                base.SetInProgress(false);
                if (result.error.error_code == VKErrors.None)
                {
                    base._totalCount = result.response.count;
                    base._nextFrom = result.response.next_from;
                    callback(result.error, result.response.items);
                }
                else
                {
                    callback(result.error, null);
                }
            });
        }
    }
}
