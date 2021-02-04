using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

using LunaVK.Core.ViewModels;
using LunaVK.Framework;
using Windows.UI.Xaml.Controls;
using LunaVK.Core.Library;
using LunaVK.Core.Framework;
using LunaVK.Core;

namespace LunaVK
{
    public sealed partial class SettingsPrivacyPage : PageBase
    {
        public SettingsPrivacyPage()
        {
            this.DataContext = new SettingsPrivacyViewModel();
            base.Title = LocalizedStrings.GetString("NewSettings_Privacy/Title");
            this.InitializeComponent();
        }

        public SettingsPrivacyViewModel VM
        {
            get { return this.DataContext as SettingsPrivacyViewModel; }
        }

        private void PrivacyTap(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as PrivacySettingItem;

            UC.EditPrivacyUC editStatusUC = new UC.EditPrivacyUC(vm);

            PopUpService statusChangePopup = new PopUpService
            {
                Child = editStatusUC
            };
            statusChangePopup.OverrideBackKey = true;
            statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            statusChangePopup.Show();
        }

        private void ExtendedListView3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }

        private void CloseProfile_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as PrivacySettingItem;
            AccountService.Instance.SetPrivacy(vm.key, (!vm.value.is_enabled).ToString().ToLower(), (result) =>
            {
                if( result.error.error_code== Core.Enums.VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() => {
                        vm.value.is_enabled = result.response.is_enabled;
                        vm.RefreshUI();
                    });
                }
            });
        }
    }
}
