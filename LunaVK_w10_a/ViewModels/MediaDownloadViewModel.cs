using LunaVK.Common;
using LunaVK.Core;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;


//BatchDownloadManager
namespace LunaVK.ViewModels
{
    public sealed class MediaDownloadViewModel : GenericCollectionViewModel<DownloadOprationItem>
    {
        public ObservableGroupingCollection<DownloadOprationItem> GroupedItems { get; private set; }

        public MediaDownloadViewModel()
        {
            this.GroupedItems = new ObservableGroupingCollection<DownloadOprationItem>(base.Items);
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<DownloadOprationItem>> callback)
        {
            if (offset == 0)
            {
                BatchDownloadManager.Instance.DiscoverActiveDownloadsAsync((ret, total) =>
                {
                    if (ret == null)
                    {
                        callback(new VKError() { error_code = Core.Enums.VKErrors.UnknownError }, ret);
                    }
                    else
                    {
                        base._totalCount = total;
                        callback(new VKError() { error_code = Core.Enums.VKErrors.None }, ret);


                        foreach (var item in ret)
                        {
                            item.CancelToken.Token.Register(() =>
                            {
                                base.Items.Remove(item);
                                base._totalCount--;
                                base.NotifyPropertyChanged(nameof(base.FooterText));
                            });
                        }
                    }
                });
            }
        }

        public override string GetFooterTextForCount
        {
            get
            {
                if (base._totalCount == 0)
                    return LocalizedStrings.GetString("Download_Empty");
                return base._totalCount + " downloads";//UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OnePersonFrm", "TwoFourPersonsFrm", "FivePersonsFrm");
            }
        }
    }
}
