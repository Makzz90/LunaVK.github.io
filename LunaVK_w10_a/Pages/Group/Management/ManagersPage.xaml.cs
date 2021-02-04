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
    public sealed partial class ManagersPage : PageBase
    {
        public ManagersPage()
        {
            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("Management_Managers/Title");
        }

        /*
        private void BaseProfileItem_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            VKUser vm = element.DataContext as VKUser;
            Library.NavigatorImpl.Instance.NavigateToProfilePage(vm.id);
        }
        */
        public ManagersViewModel VM
        {
            get { return base.DataContext as ManagersViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            uint Id = (uint)e.Parameter;
            this.DataContext = new ManagersViewModel(Id);
        }

        private void BaseProfileItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            VKUser vm = (sender as FrameworkElement).DataContext as VKUser;
            Library.NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }

        private void BaseProfileItem_ThirdClick(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKUser;

            var groupContact = this.VM.Contacts.FirstOrDefault(c => c.user_id == vm.id);

            UC.ManagerEditingUC control = new UC.ManagerEditingUC(this.VM.GroupId, vm, true, groupContact);

            PopUpService statusChangePopup = new PopUpService
            {
                Child = control
            };
            statusChangePopup.OverrideBackKey = true;
            statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            statusChangePopup.Show();
        }
    }
}
