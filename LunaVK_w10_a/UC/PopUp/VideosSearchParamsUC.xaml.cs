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

namespace LunaVK.UC.PopUp
{
    public sealed partial class VideosSearchParamsUC : UserControl
    {
        public Action Done;

        public VideosSearchParamsUC()
        {
            this.InitializeComponent();
        }

        private VideosSearchViewModel VM
        {
            get { return base.DataContext as VideosSearchViewModel; }
        }

        private void Clear_OnTap(object sender, TappedRoutedEventArgs e)
        {
            this.VM.Clear();
        }

        private void Apply_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Done?.Invoke();
        }
    }
}
