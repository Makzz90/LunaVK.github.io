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

namespace LunaVK.UC
{
    public sealed partial class MediaItemsHorizontalUC : UserControl
    {
        public MediaItemsHorizontalUC()
        {
            this.InitializeComponent();
        }

        private ProfileMediaViewModelFacade VM
        {
            get { return base.DataContext as ProfileMediaViewModelFacade; }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MediaListSectionViewModel vm = (sender as FrameworkElement).DataContext as MediaListSectionViewModel;
            vm.TapAction?.Invoke();
        }

        private void MediaHorizontalListItem_OnTap(object sender, TappedRoutedEventArgs e)
        {
            this.VM.MediaHorizontalItemsViewModel.ItemTapAction((sender as FrameworkElement).DataContext);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }

        private void MediaHorizontalAllItemsHeader_OnTap(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (this.VM.MediaHorizontalItemsViewModel == null)
                return;

            this.VM.MediaHorizontalItemsViewModel.HeaderTapAction();
        }
    }
}
