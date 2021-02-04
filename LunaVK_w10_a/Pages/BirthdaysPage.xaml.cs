using LunaVK.Core;
using LunaVK.Library;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.Pages
{
    public sealed partial class BirthdaysPage : PageBase
    {
        public BirthdaysPage()
        {
            this.InitializeComponent();
            base.DataContext = new BirthdaysViewModel();
            base.Title = LocalizedStrings.GetString("Birthdays_Title");

            this.ZoomedInGrid.ItemsSource = this.BDSource.View.CollectionGroups;
        }

        private void User_OnTap(object sender, TappedRoutedEventArgs e)
        {
            Birthday vm = (sender as FrameworkElement).DataContext as Birthday;
            NavigatorImpl.Instance.NavigateToProfilePage(vm.UserId);
        }

        private void Gift_OnTap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void ExtendedListView3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.ZoomedOutGrid.SemanticZoomOwner.ToggleActiveView();
        }
    }
}
