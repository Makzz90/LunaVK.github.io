using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.ViewModels
{
    //GenericSearchViewModel
    public class DocumentsSearchDataProvider : GenericSearchViewModelBase<object>
    {
        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<object>> callback)
        {
            DocumentsService.Instance.Search(this.SearchString, offset, count,(result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    base._totalCount = result.response.count;
                    callback(result.error, result.response.items);
                }
                else
                {
                    callback(result.error, null);
                }
            });
        }

        public override string GetFooterTextForCount
        {
            get
            {
                if (base._totalCount <= 0)
                    return LocalizedStrings.GetString("NoDocuments");

                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneDocument", "TwoFourDocumentsFrm", "FiveMoreDocumentsFrm");
            }
        }

        private string _searchString;
        public override string SearchString
        {
            get
            {
                return this._searchString;
            }
            set
            {
                if (this._searchString == value)
                    return;
                this._searchString = value;
                base.Reload();
            }
        }
    }
}
