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

//using LunaVK.Network.DataVM;
using LunaVK.Framework;
using LunaVK.Core.DataObjects;

namespace LunaVK.UC
{
    public sealed partial class ItemFriendUC : UserControl
    {
        public ItemFriendUC()
        {
            this.InitializeComponent();
        }

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var vm = base.DataContext as VKUser;
            Library.NavigatorImpl.Instance.NavigateToConversation(vm.Id);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var vm = base.DataContext as VKUser;
            Library.NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }
    }
}
