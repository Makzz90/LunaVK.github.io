using LunaVK.Core.Utils;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using LunaVK.Core.Framework;
using System.IO;
using System.Collections.Generic;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Представляет документ ВКонтакте.
    /// </summary>
    public class VKDocument : ViewModels.ViewModelBase, IBinarySerializable
    {
        /// <summary>
        /// Идентификатор документа.
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// Идентификатор владельца документа.
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// Заголовок документа.
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// Размер документа в байтах.
        /// </summary>
        public int size { get; set; }

        /// <summary>
        /// Расширение документа.
        /// </summary>
        public string ext { get; set; }

        /// <summary>
        /// Прямая ссылка на файл.
        /// </summary>
        public string url { get; set; }
        /*
        /// <summary>
        /// Адрес изображения с размером 100x75px (если файл графический).
        /// </summary>
        [JsonProperty("photo_100")]
        public string Photo100 { get; set; }

        /// <summary>
        /// Адрес изображения с размером 130x100px (если файл графический).
        /// </summary>
        [JsonProperty("photo_130")]
        public string Photo130 { get; set; }
        */
        public Enums.VKDocumentType type { get; set; }
        public int date { get; set; }
        public string access_key { get; set; }
        public DocPreview preview { get; set; }

        /// <summary>
        /// Метки
        /// </summary>
        public List<string> tags { get; set; }

#region VM
        public bool IsGraffiti
        {
            get
            {
                DocPreview preview = this.preview;
                return (preview != null ? preview.graffiti : null) != null;
            }
        }

        public bool IsVoiceMessage
        {
            get
            {
                DocPreview preview = this.preview;
                return (preview != null ? preview.audio_msg : null) != null;
            }
        }

        public string FullDescription
        {
            get { return string.Format("{0} · {1}", UIStringFormatterHelper.BytesForUI(this.size), UIStringFormatterHelper.FormatDateTimeForUI(this.date)); }
        }

        public Visibility ExtensionVisibility
        {
            get { return this.preview == null ? Visibility.Visible : Visibility.Collapsed; }
        }

        public ImageSource ThumbnailUri
        {
            get
            {
                if (this.preview != null)
                {
                    BitmapImage bi = new BitmapImage();
                    bi.UriSource = new System.Uri(this.preview.photo.sizes[0].src);
                    return bi;
                }
                return null;
            }
        }

        public Visibility IsMenuEnabled
        {//todo: | isOwnerCommunityAdmined;
            get { return this.owner_id == Settings.UserId ? Visibility.Visible : Visibility.Collapsed; }
        }

        public void UpdateUI()
        {
            base.NotifyPropertyChanged(nameof(this.title));
            base.NotifyPropertyChanged(nameof(this.tags));
        }

        public override string ToString()
        {
            //int ownerId = this._pickedDocument.owner_id;
            //uint id = this._pickedDocument.id;
            //string accessKey = this._pickedDocument.access_key;
            string str = string.Format("doc{0}_{1}", this.owner_id, this.id);
            //if (ownerId != Settings.UserId && !string.IsNullOrEmpty(accessKey))
            //    str += string.Format("_{0}", accessKey);
            return str;
        }
#endregion

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.owner_id);
            writer.WriteString(this.title);
            writer.Write(this.size);
            writer.WriteString(this.ext);
            writer.WriteString(this.url);
            writer.Write<DocPreview>(this.preview);
            //writer.Write(this.guid.ToString());
            writer.WriteString(this.access_key);
            writer.Write((byte)this.type);
            writer.Write(this.id);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.owner_id = reader.ReadInt32();
            this.title = reader.ReadString();
            this.size = reader.ReadInt32();
            this.ext = reader.ReadString();
            this.url = reader.ReadString();
            this.preview = reader.ReadGeneric<DocPreview>();
            this.access_key = reader.ReadString();
            this.type = (Enums.VKDocumentType)reader.ReadByte();
            this.id = reader.ReadUInt32();
        }
    }
}
