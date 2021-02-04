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

using LunaVK.Core.DataObjects;
using LunaVK.Framework;
using LunaVK.Core.ViewModels;
using LunaVK.Core;

namespace LunaVK.Pages.Group.Management
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class RequestsPage : PageBase
    {
        public RequestsPage()
        {
            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("Management_Requests");
        }

        public RequestsViewModel VM
        {
            get { return base.DataContext as RequestsViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            uint Id = (uint)e.Parameter;
            this.DataContext = new RequestsViewModel(Id);

//            this.VM.LoadData();
        }

        private void BaseProfileItem_PrimaryClick(object sender, RoutedEventArgs e)
        {
            Button element = sender as Button;
            VKUser vm = element.DataContext as VKUser;
            element.IsEnabled = false;
            this.VM.AddUser(vm);//todo:callback
            element.IsEnabled = true;
        }

        private void BaseProfileItem_SecondaryClick(object sender, RoutedEventArgs e)
        {
            Button element = sender as Button;
            VKUser vm = element.DataContext as VKUser;
            element.IsEnabled = false;
            this.VM.DeleteUser(vm);//todo:callback
            element.IsEnabled = true;
        }

        private void BaseProfileItem_Tapped(object sender, RoutedEventArgs e)
        {
            VKUser vm = (sender as FrameworkElement).DataContext as VKUser;
            Library.NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }
    }
}
