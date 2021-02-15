using LunaVK.Pages;
using LunaVK.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.UC.PopUp
{
    public sealed partial class FullInfoUC : UserControl
    {
        public FullInfoUC()
        {
            this.InitializeComponent();
			ProfilePage.OnUpdateVisibilityList += ProfilePage_OnUpdateVisibilityList;
        }

        private void ProfilePage_OnUpdateVisibilityList()
        {
            lv.Visibility = Visibility.Visible;
        }

		private void InfoItem_OnTap(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            ProfileInfoItem dataContext = (sender as FrameworkElement).DataContext as ProfileInfoItem;
            if (dataContext == null || dataContext.NavigationAction == null)
                return;
            dataContext.NavigationAction();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView _lv = sender as ListView;
            _lv.SelectionMode = ListViewSelectionMode.None;
            _lv.SelectionMode = ListViewSelectionMode.Single;
        }
	}
}
