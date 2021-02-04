using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Photo
{
    public class TextSelectedMenu
    {



        private static void AddEditTextMenuItem(
      CollageController collageController,
      NestedSlideMenuItem menu)
        {
            SlideMenuItem slideMenuItem1 = new SlideMenuItem();
            slideMenuItem1.Tag = "EditText";
            slideMenuItem1.Name = AppResources.SlideMenuDescription_AddTextEditorMenu_Edit;
            slideMenuItem1.IconPath = ResolutionScaleNameConverter.GetScaleName("/Assets/icons/menu/text-edit.png");
            slideMenuItem1.ClickAction = (Action<SlideMenuItemBase>)(_ =>
            {
                if (collageController.SelectedText == null)
                    return;
                collageController.SelectedText.ToggleEditMode();
            });
            SlideMenuItem slideMenuItem2 = slideMenuItem1;
            ((Collection<SlideMenuItemBase>)menu.SubMenuItems).Add((SlideMenuItemBase)slideMenuItem2);
        }

        private static void AddChangeFontMenuItem(
      CollageController collageController,
      NestedSlideMenuItem menu)
        {
            SlideMenuItemWithDataTemplate withDataTemplate1 = new SlideMenuItemWithDataTemplate();
            withDataTemplate1.Tag = "text Font";
            withDataTemplate1.IsNew = true;
            withDataTemplate1.Name = AppResources.SlideMenuDescription_AddTextEditorMenu_Font;
            withDataTemplate1.IconPath = ResolutionScaleNameConverter.GetScaleName("/Assets/icons/menu/text-fonts.png");
            withDataTemplate1.SecondaryControlDataTemplate = SlideMenuHelpers.GetTemplate("FontSelectorTemplate");
            withDataTemplate1.GetDataContextFunc = (Func<object>)(() => (object)new FontSelectorSlideMenuItemViewModel(collageController));
            SlideMenuItemWithDataTemplate withDataTemplate2 = withDataTemplate1;
            ((Collection<SlideMenuItemBase>)menu.SubMenuItems).Add((SlideMenuItemBase)withDataTemplate2);
        }

        public static void AddSetForegroundColorMenuItem(
          CollageController collageController,
          NestedSlideMenuItem menu)
        {
            Action<IBrushProvider> selectedAssetChanged = (Action<IBrushProvider>)(v => collageController.SelectedText.ForegroundColor = v);
            SlideMenuItemWithDataTemplate withDataTemplate1 = new SlideMenuItemWithDataTemplate();
            withDataTemplate1.Tag = "text foreground";
            withDataTemplate1.Name = AppResources.SlideMenuDescription_Foreground;
            withDataTemplate1.IconPath = ResolutionScaleNameConverter.GetScaleName("/Assets/icons/menu/text-foreground.png");
            withDataTemplate1.Background = (Brush)Application.get_Current().get_Resources().get_Item((object)"SecondLevelMenuBrush");
            withDataTemplate1.ShowAsPopup = true;
            withDataTemplate1.GetDataContextFunc = (Func<object>)(() => (object)new SelectColorControlViewModel(selectedAssetChanged, false));
            withDataTemplate1.SecondaryControlDataTemplate = SlideMenuHelpers.GetTemplate("SelectColorTemplate");
            SlideMenuItemWithDataTemplate withDataTemplate2 = withDataTemplate1;
            ((Collection<SlideMenuItemBase>)menu.SubMenuItems).Add((SlideMenuItemBase)withDataTemplate2);
        }

        public static void AddSetBackgroundColorMenuItem(
          CollageController collageController,
          NestedSlideMenuItem menu)
        {
            SlideMenuItemWithDataTemplate withDataTemplate1 = new SlideMenuItemWithDataTemplate();
            withDataTemplate1.Tag = "background";
            withDataTemplate1.Name = AppResources.SlideMenuDescription_Background;
            withDataTemplate1.IconPath = ResolutionScaleNameConverter.GetScaleName("/Assets/icons/menu/text-background.png");
            withDataTemplate1.Background = (Brush)Application.get_Current().get_Resources().get_Item((object)"SecondLevelMenuBrush");
            withDataTemplate1.ShowAsPopup = true;
            withDataTemplate1.GetDataContextFunc = (Func<object>)(() => (object)new SetTextBackgroundViewModel(collageController));
            withDataTemplate1.SecondaryControlDataTemplate = SlideMenuHelpers.GetTemplate("SelectFromNamedAssetTemplate");
            SlideMenuItemWithDataTemplate withDataTemplate2 = withDataTemplate1;
            ((Collection<SlideMenuItemBase>)menu.SubMenuItems).Add((SlideMenuItemBase)withDataTemplate2);
        }
    }


}
