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

using LunaVK.Core.DataObjects;
using LunaVK.Core.ViewModels;
using LunaVK.Core;
using LunaVK.ViewModels;
using LunaVK.UC;
using LunaVK.Framework;

namespace LunaVK.Pages.Audio
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class AlbumPage : PageBase
    {
        private AllAudioViewModel VM
        {
            get { return base.DataContext as AllAudioViewModel; }
        }

        public AlbumPage()
        {
            this.InitializeComponent();
            base.Title = "Альбом";
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = false;
            Dictionary<string, object> QueryString = e.Parameter as Dictionary<string, object>;
            VKPlaylist Playlist = (VKPlaylist)QueryString["Playlist"];
            base.DataContext = new AllAudioViewModel(Playlist);
        }

        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = true;
        }

        private void AudioTrackUC_CoverClick(object sender, RoutedEventArgs e)
        {
            VKAudio vm = (sender as FrameworkElement).DataContext as VKAudio;

            if (AudioPlayerViewModel2.Instance.CurrentTrack == vm)
                AudioPlayerViewModel2.Instance.PlayPause();
            else
            {
                AudioPlayerViewModel2.Instance.CurentTrackId = this.VM.Items.IndexOf(vm);
                AudioPlayerViewModel2.Instance.FillTracks(this.VM.Items, this.VM.Playlist.ToString());
                AudioPlayerViewModel2.Instance.PlayTrack(vm);
            }
        }

        private void AudioTrackUC_BackClick(object sender, RoutedEventArgs e)
        {
//           VKAudio vm = (sender as FrameworkElement).DataContext as VKAudio;

//            VKPlaylist playlist = new VKPlaylist("несохранённый плейлист", -100, this.VM.OwnerId, this.VM.Items.ToList());

//            Library.NavigatorImpl.Instance.NavigateToAudioPlayer(playlist, this.VM.Items.IndexOf(vm));
            this.AudioTrackUC_CoverClick(sender, e);
        }

        private void AudioTrackUC_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                return;

            FrameworkElement element = sender as FrameworkElement;
            this.ShowMenu(element);
        }

        private void AudioTrackUC_Holding(object sender, HoldingRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                FrameworkElement element = sender as FrameworkElement;
                this.ShowMenu(element);
            }
        }

        private void ShowMenu(FrameworkElement element)
        {
            var vm = element.DataContext as VKAudio;
            string id = vm.ToString();

            var cached = AudioCacheManager.Instance.DownloadedList.FirstOrDefault((a) => a.ToString() == id);
            if (cached!=null)
            {
                PopUP2 menu = new PopUP2();
                PopUP2.PopUpItem item = new PopUP2.PopUpItem() { Text = "Удалить", Command = new DelegateCommand((args) => {
                        this.VM.RemoveAudio(vm);
                    })
                };
                menu.Items.Add(item);
                menu.ShowAt(element);
            }

                



        }

        private void MainScroll_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }
    }
}
