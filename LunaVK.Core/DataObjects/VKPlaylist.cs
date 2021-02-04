using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.Core.DataObjects
{
    //AudioAlbum
    public class VKPlaylist
    {
        public int id { get; set; }
        public int owner_id { get; set; }
        public string title { get; set; }
        public string photo_135 { get; set; }
        public ObservableCollection<VKAudio> audios { get; set; }
        public string owner_name { get; set; }

        public VKPlaylist()
        {
            this.audios = new ObservableCollection<VKAudio>();
        }

        public VKPlaylist(string name, int _id, int owner, IReadOnlyList<VKAudio> tracks)
        {
            this.audios = new ObservableCollection<VKAudio>(tracks);
            this.title = name;
            this.id = _id;
            this.owner_id = owner;
        }

        [JsonIgnore]
        public BitmapImage Cover
        {
            get
            {
                if (string.IsNullOrEmpty(this.photo_135))
                    return null;
                BitmapImage bi = new BitmapImage(new Uri(this.photo_135));
                return bi;

            }
        }

        /// <summary>
        /// audio_owner_id
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "audio_playlist" + this.owner_id + "_" + this.id;
        }
    }
}
