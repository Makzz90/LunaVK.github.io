using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.Library
{
    public class PrivacySettingTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var vm = item as Core.Library.PrivacySetting;

            return vm.type == "list" ? this.ListTemplate : this.BinaryTemplate;
        }

        public DataTemplate ListTemplate { get; set; }

        public DataTemplate BinaryTemplate { get; set; }
    }
}
