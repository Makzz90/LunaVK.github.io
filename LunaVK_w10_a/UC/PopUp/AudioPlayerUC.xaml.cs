using LunaVK.Core.ViewModels;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.UC.PopUp
{
    public sealed partial class AudioPlayerUC : UserControl
    {
        public AudioPlayerUC()
        {
            this.InitializeComponent();

            base.DataContext = AudioPlayerViewModel.Instance;
        }

        private AudioPlayerViewModel VM
        {
            get { return base.DataContext as AudioPlayerViewModel; }
        }

        private void RevButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            this.VM.PrevTrack();
        }

        private void playImage_Tap(object sender, TappedRoutedEventArgs e)
        {
            this.VM.PlayPause();
        }
        
        private void ForwardButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            this.VM.NextTrack();
        }

        private void Repeat_Tap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void Add_Tap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void Shuffle_Tap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void SongText_Tap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void Broadcast_Tap(object sender, TappedRoutedEventArgs e)
        {
            //this.VM.Broadcast = !this.VM.Broadcast;
        }
        
    }
}
