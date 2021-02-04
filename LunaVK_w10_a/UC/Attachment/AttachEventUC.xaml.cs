using LunaVK.Core.DataObjects;
using LunaVK.Core.Library;
using LunaVK.Library;
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

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace LunaVK.UC.Attachment
{
    public sealed partial class AttachEventUC : UserControl
    {
        public AttachEventUC()
        {
            this.InitializeComponent();
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = base.DataContext as VKEvent;
            NavigatorImpl.Instance.NavigateToProfilePage(-(int)vm.id);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = base.DataContext as VKEvent;
            GroupsService.Instance.Join(vm.id, false);
        }
    }
}
