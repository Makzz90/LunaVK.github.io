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

using LunaVK.Core.Utils;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using Windows.Storage.Pickers;
using Windows.Storage;
using LunaVK.Core;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using LunaVK.Common;

namespace LunaVK.UC.Attachment
{
    public sealed partial class AttachDocumentUC : UserControl, ThumbnailsLayoutHelper.IThumbnailSupport
    {
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(object), typeof(AttachDocumentUC), new PropertyMetadata(default(object), OnDataChanged));

        private event TappedEventHandler _backClick;
        public event TappedEventHandler OnTap
        {
            add { this._backClick += value; }
            remove { this._backClick -= value; }
        }

        /// <summary>
        /// Данные.
        /// </summary>
        public object Data
        {
            get { return GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        private static void OnDataChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((AttachDocumentUC)obj).ProcessData();
        }

        private DocPreview.DocPreviewPhoto VM
        {
            get
            {
                if (this.Data == null)
                    return null;

                VKDocument doc = this.Data as VKDocument;

                if (doc.type != VKDocumentType.IMAGE && doc.type != VKDocumentType.GIF)
                    return null;

                if (doc.preview == null)
                    return null;

                return doc.preview.photo;
            }
        }

        public bool IsCompact { get; set; }

        private void ProcessData()
        {
            this.Main.Children.Clear();

            if (this.Data == null)
                return;

            VKDocument doc = this.Data as VKDocument;
            if (doc.type == VKDocumentType.IMAGE)
            {
                if (doc.preview == null)
                {
                    //EPS file
                    BaseAttachment ba = new BaseAttachment(doc.title, doc.size, "\xEB9F", "", this.topOffset);
                    ba.OnTap = () =>
                    {
                        if (this._backClick != null)
                            this._backClick(this, null);
                        else
                        {
                            BatchDownloadManager.Instance.DownloadByIndex(doc.url, "(" + doc.ToString() + ")" + doc.title);
                            //this.SaveToDevice(doc.url, doc.title, doc.ext);
                        }
                    };
                    this.Main.Children.Add(ba);
                    return;
                }

                if (doc.preview.graffiti != null)
                {
                    Image img = new Image();
                    img.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(doc.preview.graffiti.src));
                    img.Width = doc.preview.graffiti.width;
                    img.Height = doc.preview.graffiti.height;
                    //
                    img.MaxHeight = 300;
                    img.MaxWidth = 300;
                    //
                    img.Stretch = Stretch.Uniform;
                    this.Main.Children.Add(img);
                }
                else
                {
                    if (this.IsCompact)
                    {
                        BaseAttachment ba = new BaseAttachment(doc.title, doc.size, "", doc.preview.photo.sizes[0].src, this.topOffset);
                        //ba.OnTap += delegate { };
                        this.Main.Children.Add(ba);

                    }
                    else
                    {
                        Image img = new Image();
                        img.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(doc.preview.photo.sizes[0].src));
                        img.Tapped += (s, a) => {
                            if (this._backClick != null)
                                this._backClick(this, null);
                            else
                            {
                                //this.SaveToDevice(doc.url, doc.title, doc.ext);
                                BatchDownloadManager.Instance.DownloadByIndex(doc.url, "(" + doc.ToString() + ")" + doc.title);
                            }
                        };
                        this.Main.Children.Add(img);
                        this.AddSizeAndType(doc.ext, doc.size);
                    }
                    
                }
            }
            else if (doc.type == VKDocumentType.UNKNOWN || doc.type == VKDocumentType.TEXT || doc.type == VKDocumentType.ARCHIVE || doc.type == VKDocumentType.EBOOK)
            {
                BaseAttachment ba = new BaseAttachment(doc.title, doc.size, "\xE8FF", "", this.topOffset);
                ba.OnTap = () =>
                {
                    if (this._backClick != null)
                        this._backClick(this, null);
                    else
                    {
                        //this.SaveToDevice(doc.url, doc.title, doc.ext);
                        BatchDownloadManager.Instance.DownloadByIndex(doc.url, "(" + doc.ToString() + ")" + doc.title);
                    }
                };
                this.Main.Children.Add(ba);
            }
            else if (doc.type == VKDocumentType.VIDEO)
            {
                BaseAttachment ba = new BaseAttachment(doc.title, doc.size, "\xE714");
                //ba.OnTap += delegate { };
                this.Main.Children.Add(ba);
            }
            else if (doc.type == VKDocumentType.AUDIO)
            {
                if (doc.preview != null)
                {
                    DocPreview.DocPreviewVoiceMessage audio_msg = doc.preview.audio_msg;
                    AttachVoiceMessageUC voice = new AttachVoiceMessageUC(audio_msg);
                    this.Main.Children.Add(voice);
                }
                else
                {
                    VKAudio audio = new VKAudio();
                    audio.title = doc.title;
                    audio.url = doc.url;
                    AttachAudioUC uc = new AttachAudioUC(audio);
                    this.Main.Children.Add(uc);
                }
            }
            else if (doc.type == VKDocumentType.GIF)
            {
                if (doc.preview.photo != null && doc.preview.photo.sizes.Count > 2)
                {
                    if(this.IsCompact)
                    {
                        BaseAttachment ba = new BaseAttachment(doc.title, doc.size, "",doc.preview.photo.sizes[2].src);
                        //ba.OnTap += delegate { };
                        this.Main.Children.Add(ba);

                    }
                    else
                    {
                        Image img = new Image();
                        img.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(doc.preview.photo.sizes[2].src));
                        //img.Width = doc.preview.photo.sizes[2].width;
                        //img.Height = doc.preview.photo.sizes[2].height;
                        img.Stretch = Stretch.Uniform;
                        this.Main.Children.Add(img);



                        GifViewerUC g = new GifViewerUC(doc.preview.photo.sizes[2].src, doc);
                        this.Main.Children.Add(g);
                    }
                }
            }
            else
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(doc.type.ToString());
#endif
            }
        }

        private void AddSizeAndType(string type, double size)
        {
            Border border = new Border() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom };
            border.Margin = new Thickness(5);
            border.Padding = new Thickness(10,5,10,5);
            border.CornerRadius = new CornerRadius(8);

            TextBlock textBlock1 = new TextBlock();
            textBlock1.Text = string.Format("{0} · {1}", type.ToUpper(), UIStringFormatterHelper.BytesForUI(size));
            textBlock1.FontSize = 12;
            textBlock1.Foreground = new SolidColorBrush(Windows.UI.Colors.White);

            border.Background = new SolidColorBrush( Windows.UI.Color.FromArgb(150,0,0,0));
            border.Child = textBlock1;

            this.Main.Children.Add(border);
        }

        public AttachDocumentUC()
        {
            this.InitializeComponent();
        }

        private bool topOffset = true;

        public AttachDocumentUC(VKDocument doc, bool top_offset = true) :this()
        {
            this.topOffset = top_offset;
            this.Data = doc;
            //this.ProcessData(top_offset);
        }





        /*
        private async void SaveToDevice(string url, string name, string extension)
        {
            string path = Settings.SaveFolderDoc;
            string fileName = name;// + "." + extension;

            StorageFile file = null;
            
            try
            {
                file = await DownloadsFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            }
            catch (Exception ex)
            {
                //todo: спрашивать если существует, дописывать ошибку
                this.MakeToast(url, false);
            }

            if (file != null)
            {
                if (string.IsNullOrEmpty(path))
                    Settings.SaveFolderDoc = file.Path.Replace("\\"+fileName, "");

                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    var client = new System.Net.Http.HttpClient();
                    var httpStream = await client.GetStreamAsync(new Uri(url));
                    await httpStream.CopyToAsync(fileStream);
                    fileStream.Dispose();
                    this.MakeToast(file.Name, true);
                }
            }
        }
        
        private void MakeToast(string fileName, bool status)
        {
            var splitted = fileName.Split('?');
            if(splitted.Count() > 1)
            {
                fileName = splitted[0];
            }
            else
            {
                fileName = fileName.Substring(0, 15) + "...";
            }
            // template to load for showing Toast Notification
            string xmlToastTemplate = "<toast launch=\"app-defined-string\">" +
                                     "<visual>" +
                                       "<binding template =\"ToastGeneric\">" +
                                         (status ? "<text>Скачивание завершено</text>" : "<text>Ошибка скачивания</text>") +
                                         "<text>" +
                                           (status ? "Документ загружен " : "Документ не загружен ") + fileName +
                                         "</text>" +
                                        "</binding>" +
                                     "</visual>" +
                                   "</toast>";

            // load the template as XML document
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlToastTemplate);

            // create the toast notification and show to user
            var toastNotification = new ToastNotification(xmlDocument);
            var notification = ToastNotificationManager.CreateToastNotifier();
            notification.Show(toastNotification);
        }
        */

        #region IThumbnailSupport
        /// <summary>
        /// Данные для визуализации миниатюры.
        /// </summary>
        public ThumbnailsLayoutHelper.ThumbnailSize ThumbnailSize { get; set; }

        /// <summary>
        /// Ширина исходного изображения.
        /// </summary>
        double ThumbnailsLayoutHelper.IThumbnailSupport.Width { get { return this.VM.sizes[0].width; } }

        /// <summary>
        /// Высота исходного изображения.
        /// </summary>
        double ThumbnailsLayoutHelper.IThumbnailSupport.Height { get { return this.VM.sizes[0].height; } }

        /// <summary>
        /// Источник изображения миниатюры.
        /// </summary>
        public string ThumbnailSource
        {
            get { return ThumbnailSize.Width <= 130 ? this.VM.sizes[0].src : this.VM.sizes[1].src; }
        }

        /// <summary>
        /// Возвращает соотношение ширины к высоте исходного изображения.
        /// </summary>
        public double GetRatio() { return (double)this.VM.sizes[0].width / (double)this.VM.sizes[0].height; }
#endregion
    }
}
