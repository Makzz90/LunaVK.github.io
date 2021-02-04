using LunaVK.Framework;
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

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestMasterDetils : Page
    {
        public TestMasterDetils()
        {
            this.InitializeComponent();

            this.Loaded += (s, e) =>
            {
    //            this.ucPullToRefresh.TrackListBox(this.DetailsView.MasterList);//CustomFrame.Instance.HeaderWithMenu.PullToRefresh.TrackListBox(this.DetailsView.MasterList);
                if (CustomFrame.Instance.Header.IsVisible == true)
                    CustomFrame.Instance.Header.IsVisible = false;

     //           this.LetsLoad(SelectPeer);
                base.DataContext = DialogsViewModel.Instance;
            };
            this.detailed.BackCall = () => {
                this._detailsView.SelectedItem = false;
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this._detailsView.SelectedItem = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this._detailsView.SelectedItem = false;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this._detailsView.SelectedItem = false;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this._detailsView.SelectedItemFast();
        }

        private void ItemDialogUC_Holding(object sender, HoldingRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                FrameworkElement element = sender as FrameworkElement;
            //    this.ShowMenu(element);
            }
        }

        private void ItemDialogUC_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                return;

            FrameworkElement element = sender as FrameworkElement;
         //   this.ShowMenu(element);
        }

        private void ItemDialogUC_BackTap(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as ConversationWithLastMsg;
            this._detailsView.SelectedItem = true;
//            DialogsViewModel.Instance.CurrentConversation = vm;
            this.detailed.DataContext = vm;
        }

        private void ItemDialogUC_AvatrTap(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as ConversationWithLastMsg;
            if (vm.conversation.peer.type == Core.Enums.VKConversationPeerType.User)
                NavigatorImpl.Instance.NavigateToProfilePage(vm.conversation.peer.local_id);
        }

        private void Burger_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CustomFrame.Instance.OpenCloseMenu();
        }
    }
}
