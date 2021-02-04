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

using LunaVK.Core.ViewModels;

namespace LunaVK
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class StickersStorePage : PageBase
    {
        public StickersStorePage()
        {
            base.DataContext = new StickersStoreViewModel();
            this.InitializeComponent();
            this.Loaded += StickersStorePage_Loaded;
            
        }

        private void StickersStorePage_Loaded(object sender, RoutedEventArgs e)
        {
            (base.DataContext as StickersStoreViewModel).LoadData(true);
        }

        private void _header_Tapped(object sender, TappedRoutedEventArgs e)
        {
            (base.DataContext as StickersStoreViewModel).LoadData(true);
        }
    }
}
