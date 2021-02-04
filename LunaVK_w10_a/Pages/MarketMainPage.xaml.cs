using LunaVK.Core;
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
    public sealed partial class MarketMainPage : PageBase
    {
        public MarketMainPage()
        {
            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("Menu_Market/Content");
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.DataContext = new MarketMainViewModel((int)e.Parameter);
        }
        
    }
}
