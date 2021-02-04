using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.ViewModels
{
    public class InfoListItem
    {
        public string IconUrl { get; set; }

        public string Text { get; set; }
        
        public string Preview1 { get; set; }

        public string Preview2 { get; set; }

        public string Preview3 { get; set; }

        public bool IsTiltEnabled { get; set; }

        public Action TapAction { get; set; }
    }
}
