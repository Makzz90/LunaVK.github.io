using LunaVK.Core;
using LunaVK.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MediaDownloadPage : PageBase
    {
        public MediaDownloadPage()
        {
            this.InitializeComponent();

            base.DataContext = new MediaDownloadViewModel();

            this.MainScroll.Loaded2 += this.InsideScrollViewerLoaded;
        }

        private MediaDownloadViewModel VM
        {
            get { return base.DataContext as MediaDownloadViewModel; }
        }

        private void InsideScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            base.Title = LocalizedStrings.GetString("Menu_Downloads");
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            //this.VM.StartDownload(new Uri(this._tbUri.Text), this._tbFileName.Text);
        }
    }
}
