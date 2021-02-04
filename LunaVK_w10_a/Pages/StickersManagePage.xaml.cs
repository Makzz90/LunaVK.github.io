using LunaVK.Core.DataObjects;
using LunaVK.UC.Controls;
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
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class StickersManagePage : PageBase
    {
        private StickersManageViewModel VM
        {
            get { return base.DataContext as StickersManageViewModel; }
        }

        public StickersManagePage()
        {
            base.DataContext = new StickersManageViewModel();

            this.InitializeComponent();

            this.Loaded += StickersManagePage_Loaded;
        }

        private void StickersManagePage_Loaded(object sender, RoutedEventArgs e)
        {
            base.Title = "Мои стикеры";
            this.VM.Load();
        }

        private void Deactivate_OnTap(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as StockItem;
            this.VM.Deactivate(vm);
        }

        private void Activate_OnTap(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as StockItem;
            this.VM.Activate(vm);
        }
    }
}
