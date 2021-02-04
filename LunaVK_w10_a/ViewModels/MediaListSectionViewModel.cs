using LunaVK.Core.Enums;
using LunaVK.Core.Utils;
using System;

namespace LunaVK.ViewModels
{
    public class MediaListSectionViewModel
    {
        public string Title { get; private set; }

        public string TitleCounter { get; private set; }

        public ProfileMediaListItemType Type { get; private set; }

        public object Data { get; private set; }

        public Action TapAction { get; private set; }

        public MediaListSectionViewModel(string title, int titleCounter, ProfileMediaListItemType type, object data = null, Action tapAction = null)
        {
            this.Title = title;
            this.Data = data;
            this.TitleCounter = UIStringFormatterHelper.CountForUI(titleCounter);//FormatForUIVeryShort
            //this.ListItemViewModel = listItemViewModel;
            this.TapAction = tapAction;
        }
    }
}
