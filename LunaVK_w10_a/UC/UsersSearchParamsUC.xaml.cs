using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LunaVK.UC
{
    public sealed partial class UsersSearchParamsUC : UserControl
    {
        public Action Done;

        public UsersSearchParamsUC()
        {
            this.InitializeComponent();
        }

        private SearchParamsViewModel.UsersSearchParamsViewModel VM
        {
            get { return base.DataContext as SearchParamsViewModel.UsersSearchParamsViewModel; }
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
