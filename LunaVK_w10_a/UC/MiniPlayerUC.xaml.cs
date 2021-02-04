using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

using LunaVK.Core.ViewModels;
using LunaVK.ViewModels;



#if WINDOWS_PHONE_APP
//using BackgroundAudio;
using Windows.Media.Playback;
#endif

namespace LunaVK.UC
{
    public sealed partial class MiniPlayerUC : UserControl
    {
        

        public MiniPlayerUC()
        {
            base.DataContext = null;
            this.InitializeComponent();

#if WINDOWS_PHONE_APP
//            ForegroundPlaylistManager.Instance.CurrentStateChanged += this.CurrentStateChanged;
#endif
            this.Loaded += MiniPlayerUC_Loaded;
        }

        void MiniPlayerUC_Loaded(object sender, RoutedEventArgs e)
        {
            //            this.VM.LoadCurrentPlaylist();
            base.DataContext = AudioPlayerViewModel.Instance;//todo:оно точно тут?
        }
        
        private void PlayPause_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AudioPlayerViewModel.Instance.PlayPause();
        }

        private void Grid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {

            if(e.Cumulative.Translation.X>100)
            {
//                ForegroundPlaylistManager.Instance.PlayNext();
            }
            else if(e.Cumulative.Translation.X<-100)
            {
//                ForegroundPlaylistManager.Instance.PlayPrev();
            }

        }

        private void Panel_Tapped(object sender, TappedRoutedEventArgs e)
        {
//            Library.NavigatorImpl.Instance.NavigateToAudioPlayer();
        }
    }
}
