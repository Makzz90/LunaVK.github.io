using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Photo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.Photo.UC
{
    public class StickerOverlayShape : ContainedShapeControlContainer
    {
        private List<SlideMenuItemBase> _menuItems;
        public Image Image
        {
            get { return base.Control as Image; }
        }

        public StickerOverlayShape()
            : base(new Image(), 90, 90)
        {
            this._menuItems = new List<SlideMenuItemBase>();
            this._menuItems.Add(new SlideMenuItemBase()
            {
                Name = "Изображение",
                IconPath = "ms-appx:///Assets/PhotoEditor/add-menu-stickers.png",
                SecondaryControlDataTemplate = (DataTemplate)Application.Current.Resources["SelectStickerTemplate"],
                GetDataContextFunc = (Func<object>)(() => (object)new SelectStickerViewModel(this.Image))
            });
        }

        public override List<SlideMenuItemBase> MenuItems
        {
            get
            {
                return this._menuItems;
            }
        }
    }
}
