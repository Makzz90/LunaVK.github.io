using System;
using Newtonsoft.Json;
using LunaVK.Core.Enums;
using LunaVK.Core.Utils;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using LunaVK.Core.ViewModels;
using LunaVK.Core.Library;
using LunaVK.Core.Framework;
using System.IO;
using Windows.Media.Playback;
using Windows.Media;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Объект audio, описывающий аудиозапись
    /// https://vk.com/dev/_objects_audio
    /// </summary>
    public class VKAudio : ViewModelBase, IBinarySerializable
    {
        /// <summary>
        /// Идентификатор аудиозаписи
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// Идентификатор владельца аудиозаписи
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// Исполнитель
        /// </summary>
        public string artist { get; set; }

        /// <summary>
        /// название композиции
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// длительность аудиозаписи в секундах
        /// </summary>
        //[JsonConverter(typeof(SecondsToTimeSpanConverter))]
        public int duration { get; set; }

        /// <summary>
        /// Ссылка на MP3-файл
        /// https://vk.com/mp3/audio_api_unavailable.mp3
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// идентификатор текста аудиозаписи (если доступно)
        /// </summary>
        public int lyrics_id { get; set; }

        /// <summary>
        /// идентификатор альбома, в котором находится аудиозапись (если присвоен)
        /// </summary>
        public int album_id { get; set; }

        /// <summary>
        /// Жанр аудиозаписи.
        /// </summary>
        public VKAudioGenre genre_id { get; set; }

        /// <summary>
        /// дата добавления
        /// </summary>
        public int date { get; set; }

        /// <summary>
        /// 1, если включена опция «Не выводить при поиске». Если опция отключена, поле не возвращается
        /// </summary>
        public int no_search { get; set; }

        /// <summary>
        /// Явный
        /// </summary>
        public bool is_explicit { get; set; }


#region VM
        public string cover { get; set; }

        [JsonIgnore]
        public BitmapImage Cover
        {
            get
            {
                if(string.IsNullOrEmpty(this.cover))
                    return null;
                return new BitmapImage(new Uri(this.cover));
            }
        }

        private double _progress;

        [JsonIgnore]
        public double Progress
        {
            get
            {
                //
                if (AudioCacheManager.Instance.GetLocalFileForUniqueId(this.ToString()) != null)
                    this._progress = 100;
                //
                return this._progress;
            }
            set
            {
                this._progress = value;
                base.NotifyPropertyChanged("Progress");
            }
        }
        
        [JsonIgnore]
        public string UIDuration
        {
            get
            {
                if (this.IsUrlUnavailable || this.duration ==0 )
                    return "";
                return UIStringFormatterHelper.FormatDuration(this.duration);
            }
        }
        
#endregion
        
#region HACK
        public string actionHash { get; set; }
        public string urlHash { get; set; }
#endregion

        public void UpdateUI()
        {/*
            base.NotifyPropertyChanged(nameof(this.PlayIconVisibility));
            base.NotifyPropertyChanged(nameof(this.PauseIconVisibility));
            base.NotifyPropertyChanged(nameof(this.PlayIconForCoverVisibility));
            base.NotifyPropertyChanged(nameof(this.PauseIconForCoverVisibility));
            base.NotifyPropertyChanged(nameof(this.BackVisibility));*/
            base.NotifyPropertyChanged(nameof(this.UIDuration));
        }

        /// <summary>
        /// audio_owner_id
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "audio" + this.owner_id + "_" + this.id;
        }

        public bool IsUrlUnavailable
        {
            get
            {
                return this.url == "https://vk.com/mp3/audio_api_unavailable.mp3";
            }
        }

#region IBinarySerializable
        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.id);
            writer.Write(this.owner_id);
            writer.WriteString(this.artist);
            writer.WriteString(this.title);
            writer.Write(this.duration);
            writer.WriteString(this.url);
            writer.Write(this.album_id);
            writer.Write(this.lyrics_id);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadUInt32();
            this.owner_id = reader.ReadInt32();
            this.artist = reader.ReadString();
            this.title = reader.ReadString();
            this.duration = reader.ReadInt32();
            this.url = reader.ReadString();
            this.album_id = reader.ReadInt32();
            this.lyrics_id = reader.ReadInt32();
        }
#endregion
    }
}
