using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunaVK.Core.DataObjects;
using System.Collections.ObjectModel;

namespace LunaVK.Core.ViewModels
{
    public class PlaylistViewModel
    {
        public string Name { get; set; }

        public short Id { get; set; }

        public int Owner { get; set; }

        public uint TracksCount;

        public ObservableCollection<VKAudio> Tracks { get; set; }

        public uint LastNumber;

        public VKAudio LastTrack
        {
            get
            {
                return this.Tracks[(int)this.LastNumber];
            }
        }

        public PlaylistViewModel()
        {
            this.Tracks = new ObservableCollection<VKAudio>();
        }

        public PlaylistViewModel(string name, short id, int owner, IReadOnlyList<VKAudio> tracks)
        {
            this.Tracks = new ObservableCollection<VKAudio>(tracks);
            this.Name = name;
            this.TracksCount = (uint)tracks.Count;
            this.Id = id;
            this.Owner = owner;
        }

        public string PlaylistId
        {
            get
            {
                return "audio_playlist" + this.Owner + "_" + this.Id;
            }
        }
    }
}
