using LunaVK.Core.DataObjects;
using LunaVK.Core.ViewModels;
using LunaVK.ViewModels;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.UC.Attachment
{
    public sealed partial class AttachPodcastUC : UserControl
    {
        public AttachPodcastUC()
        {
            this.InitializeComponent();
        }

        private VKPodcast VM
        {
            get { return base.DataContext as VKPodcast; }
        }

        private VKAudio TempVM
        {
            get { return new VKAudio() { id = this.VM.id, owner_id = this.VM.owner_id, cover = this.VM.CoverImg, url = this.VM.url, duration = this.VM.duration, title = this.VM.title, artist = this.VM.artist }; }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (AudioPlayerViewModel2.Instance.CurrentTrack != null && AudioPlayerViewModel2.Instance.CurrentTrack.ToString() == this.TempVM.ToString())
                AudioPlayerViewModel2.Instance.PlayPause();
            else
            {
                
                AudioPlayerViewModel2.Instance.FillTracks(new List<VKAudio>() { TempVM }, this.VM.ToString());
                AudioPlayerViewModel2.Instance.PlayTrack(TempVM);
            }
        }
    }
}
