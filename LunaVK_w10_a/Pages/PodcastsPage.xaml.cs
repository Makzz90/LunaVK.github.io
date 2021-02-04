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
    public sealed partial class PodcastsPage : PageBase
    {
        public PodcastsPage()
        {
            this.InitializeComponent();
            base.Title = "Podcasts";
        }

        private PodcastsViewModel VM
        {
            get { return base.DataContext as PodcastsViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            int owner_id = (int)e.Parameter;
            base.DataContext = new PodcastsViewModel(owner_id);
        }

        private void ExtendedListView3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }
    }
}
