using System;
using System.Collections.Generic;
using LunaVK.Core.Library;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using LunaVK.Core.Enums;
using LunaVK.Core.Utils;
using LunaVK.Core;

namespace LunaVK.ViewModels
{
    public class PhotoPickerMyPhotosViewModel : GenericCollectionViewModel<VKPhoto>
    {
        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKPhoto>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if(offset>0)
                parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            //type
            //owner_id
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKPhoto>>("photos.getAll", parameters, (result) =>
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
                    return LocalizedStrings.GetString("NoPhotos");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OnePhotoFrm", "TwoFourPhotosFrm", "FivePhotosFrm");
            }
        }
    }
}
