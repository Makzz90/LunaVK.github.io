using LunaVK.Library;
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

namespace LunaVK.UC
{
    public sealed partial class AllProfilePostsToggleUC : UserControl
    {
        public AllProfilePostsToggleUC()
        {
            this.InitializeComponent();
        }

        private AllProfilePostsToggleViewModel VM
        {
            get { return base.DataContext as AllProfilePostsToggleViewModel; }
        }

        private void AllPosts_OnTap(object sender, TappedRoutedEventArgs e)
        {
            if (this.VM == null)
                return;
            this.VM.IsAllPosts = true;
        }

        private void ProfilePosts_OnTap(object sender, TappedRoutedEventArgs e)
        {
            if (this.VM == null)
                return;
            this.VM.IsAllPosts = false;
        }

        private void Search_OnTap(object sender, TappedRoutedEventArgs e)
        {
            if (this.VM == null)
                return;
            this.VM.NavigateToSearch();
        }
    }
}
