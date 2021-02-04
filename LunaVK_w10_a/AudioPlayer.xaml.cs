using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using LunaVK.ViewModels;
using LunaVK.Core.Utils;
using LunaVK.Core.DataObjects;
using LunaVK.Core.ViewModels;
using LunaVK.Framework;

namespace LunaVK
{
    /// <summary>
    /// Страница музыкального плеера
    /// </summary>
    public sealed partial class AudioPlayer : PageBase
    {
        public AudioPlayer()
        {
            base.DataContext = AudioPlayerViewModel2.Instance;

            this.InitializeComponent();
            this.Loaded += AudioPlayer_Loaded;
            base.SizeChanged += AudioPlayer_SizeChanged;
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            Dictionary<string, object> QueryString = e.Parameter as Dictionary<string, object>;

            if (QueryString.ContainsKey("Playlist"))
            {
                VKPlaylist playlist = (VKPlaylist)QueryString["Playlist"];
                //AudioPlayerViewModel2.Instance.FillTracks(playlist.audios);

                
                AudioPlayerViewModel2.Instance.FillTracks(playlist.audios, playlist.ToString());
                
            }

            if (QueryString.ContainsKey("TrackNumber"))
            {
                int trackNumber = (int)QueryString["TrackNumber"];
                AudioPlayerViewModel2.Instance.PlayTrack(trackNumber);
            }

            
        }

        void AudioPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.IsVisible = false;
            
            //this.UpdateSize();

            this._root.ManipulationMode |= ManipulationModes.TranslateY;
            //           this._root.ManipulationMode |= ManipulationModes.TranslateX;

            //AudioPlayerViewModel2.Instance.TrackChanged += PlaylistMabager_TrackChanged;
            AudioPlayerViewModel2.Instance.FetchArtwork();

            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = false;
        }

        void PlaylistMabager_TrackChanged(object sender, int e)
        {
            //           this.listView.ScrollIntoView(this.VM.Tracks[e], ScrollIntoViewAlignment.Default);
//            AudioPlayerViewModel2.Instance.TrackChanged -= PlaylistMabager_TrackChanged;//нам же надо это всего раз?
        }

        void AudioPlayer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateSize();
        }

        private void UpdateSize()
        {
            this.gridArtWork.Height = base.ActualHeight - this.panelControls.Height;
            this.listView.Height = this.gridArtWork.Height;
            this.panelControls.Margin = new Thickness(0, this.gridArtWork.Height, 0, 0);
            this.listView.Margin = new Thickness(0, this.gridArtWork.Height + this.panelControls.Height, 0, 0);
            this.gridArtWork.Width = this.panelControls.Width = this.listView.Width = base.ActualWidth;
        }
        
        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            CustomFrame.Instance.Header.IsVisible = true;
            CustomFrame.Instance.Header.HideSandwitchButton = false;

            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = true;
        }
        
        private void Playlist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.OpenCloseMenu();
        }

        private void Track_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKAudio;

            if (AudioPlayerViewModel2.Instance.CurrentTrack == vm)
                AudioPlayerViewModel2.Instance.PlayPause();
            else
                AudioPlayerViewModel2.Instance.PlayTrack(vm);
        }

        private void Prev_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AudioPlayerViewModel2.Instance.PrevTrack();
        }

        private void PausePlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AudioPlayerViewModel2.Instance.PlayPause();
        }

        private void Next_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AudioPlayerViewModel2.Instance.NextTrack();
        }

        private void Shuffle_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void Repeat_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void More_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }


        public void Back_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            //if (e.IsInertial)
            //    return;

            FrameworkElement el = sender as FrameworkElement;

            double y = e.Delta.Translation.Y;
            this.transformPanel.Y += y;

            if (this.transformPanel.Y > 0)
            {
                this.transformPanel.Y = 0;
                e.Handled = true;

                el.CancelDirectManipulations();
            }

            if ((-this.transformPanel.Y) > this.gridArtWork.Height)
            {
                this.transformPanel.Y = -this.gridArtWork.Height;
                e.Handled = true;

                el.CancelDirectManipulations();
            }
        }

        private bool _isMenuOpen = true;

        private void Back_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            double vel = e.Velocities.Linear.Y;
            double y = e.Cumulative.Translation.Y;
            bool need_change = (this._isMenuOpen ? (y < -base.ActualHeight / 2.0 || vel < -1.5) : (y > base.ActualHeight / 2.0 || vel > 1.5));
            if (need_change)
                this._isMenuOpen = !this._isMenuOpen;
            this.OpenCloseMenu(this._isMenuOpen);
        }

        public void OpenCloseMenu(bool? open = null, bool force_action = false)
        {
            if (!open.HasValue)
            {
                open = !this._isMenuOpen;
            }
            this._isMenuOpen = open.Value;
            this.AnimateMenu(this._isMenuOpen, 150);
        }

        private void AnimateMenu(bool open, int duration = 200)
        {
            double menuY = open ? 0.0 : -this.gridArtWork.Height;
            this.transformPanel.Animate(this.transformPanel.Y, menuY, "Y", duration);
            this.PlaylistArrowTransform.Animate(PlaylistArrowTransform.Rotation, open ? 180 : 0, "Rotation", 300);

            CustomFrame.Instance.Header.HideSandwitchButton = !open;
        }
        
        protected override void HandleOnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if(this._isMenuOpen == false)
            {
                this.OpenCloseMenu(true);
                e.Cancel = true;
            }
        }
    }
}
