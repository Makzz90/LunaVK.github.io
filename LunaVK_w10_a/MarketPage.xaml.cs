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
using LunaVK.Core;

namespace LunaVK
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MarketPage : PageBase
    {
        public MarketPage()
        {
            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("Menu_Market/Title");
            base.DataContext = new MarketViewModel();
        }

        public MarketViewModel VM
        {
            get { return base.DataContext as MarketViewModel; }
        }
    }
}
