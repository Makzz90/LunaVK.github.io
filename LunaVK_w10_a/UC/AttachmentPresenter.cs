using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
using LunaVK.Core.DataObjects;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using LunaVK.UC.Attachment;
using Windows.Graphics.Display;
using LunaVK.Core.ViewModels;
using LunaVK.Core;
using LunaVK.Core.Library;
using LunaVK.Core.Enums;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.UC
{
    public class AttachmentPresenter : StackPanel
    {
        MediaPresenter mp;
        StackPanel sp;
        FrameworkElement UC;
        /*
        public MediaPresenter.NewsPhotosInfo _NewsPhotosInfo
        {
            get
            {
                return mp._newsPhotosInfo;
            }
            set
            {
                mp._newsPhotosInfo = value;
            }
        }
        */
        public AttachmentPresenter()
        {
            base.MaxWidth = 600;

            base.Children.Clear();

            this.mp = new MediaPresenter();
            this.sp = new StackPanel();
        }

        public AttachmentPresenter(List<VKPhoto> list, FrameworkElement parent)
            : this()
        {
            if (this.UC == null)
            {
                this.UC = parent;
                this.UC.SizeChanged += UC_SizeChanged;
                this.UC.Unloaded += UC_Unloaded;
            }

            foreach (var photo in list)
            {
                if (photo.height == 0 || photo.width == 0)
                {
                    //Бывает и такое
                    photo.width = 604;
                    photo.height = 403;
                }
                this.mp.Items.Add(photo);
            }

            this.Finish();
        }

        private void UC_Unloaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            element.SizeChanged -= UC_SizeChanged;
        }

        public AttachmentPresenter(List<VKVideoBase> attachments, FrameworkElement parent)
            : this()
        {
            if (this.UC == null)
            {
                this.UC = parent;
                this.UC.SizeChanged += UC_SizeChanged;
                this.UC.Unloaded += UC_Unloaded;
            }

            foreach (VKVideoBase v in attachments)
            {
                if (v.width == 0 || v.height == 0)
                {
                    //Бывает такое с ютубвидео
                    v.width = 1920;
                    v.height = 1080;
                }

                AttachVideoUC a = new AttachVideoUC(v);
                this.mp.Items.Add(a);
            }

            this.Finish();
        }

        public AttachmentPresenter(List<IOutboundAttachment> attachments, FrameworkElement parent)
            : this()
        {
            if (this.UC == null)
            {
                this.UC = parent;
                this.UC.SizeChanged += UC_SizeChanged;
                this.UC.Unloaded += UC_Unloaded;
            }

            foreach (IOutboundAttachment attachment in attachments)
            {
                //if (!(attachment is Library.OutboundPhotoAttachment) && photos.Count > 0)//if (attachment.Type != VKAttachmentType.Photo && photos.Count > 0)
                //{
                //    AttachPhotosUC a = new AttachPhotosUC(photos, width, aligment_to_right);
                //    photos.Clear();
                //    base.Children.Add(a);
                //}

                if (attachment is ThumbnailsLayoutHelper.IThumbnailSupport)
                {
                    this.mp.Items.Add(attachment as ThumbnailsLayoutHelper.IThumbnailSupport);
                    //photos.Add(attachment as Library.OutboundPhotoAttachment);
                }
                else if (attachment is OutboundGraffitiAttachment g)
                {
                    if (g.Data == null)
                        continue;

                    Image img = new Image();
                    img.Source = g.Data;

                    img.MinWidth = img.MinHeight = 150;

                    this.sp.Children.Add(img);
                }
                else if (attachment is OutboundDocumentAttachment)
                {
                    var o = attachment as OutboundDocumentAttachment;
                    AttachDocumentUC uc = new AttachDocumentUC(o._pickedDocument);

                    this.sp.Children.Add(uc);
                }

            }

            this.Finish();
        }

        private const double H = 300;

        private void Finish()
        {
            if (this.mp.Items.Count > 0)
            {
                //this.mp.MaxRectSize = new Rectangle(!double.IsInfinity(MaxWidth) ? MaxWidth :
                //        IsLandscape() ? Window.Current.Bounds.Height : Window.Current.Bounds.Width,
                //        !double.IsInfinity(MaxHeight) ? MaxHeight : H);

                this.mp.MaxRectSize = new Rectangle(double.IsInfinity(base.MaxWidth) ? Window.Current.Bounds.Width : base.MaxWidth,
                        double.IsInfinity(base.MaxHeight) ? H : base.MaxHeight);
                this.mp.Update();
                base.Children.Add(this.mp);
            }

            if (this.sp.Children.Count > 0)
                base.Children.Add(this.sp);
        }

        public AttachmentPresenter(List<VKAttachment> attachments, FrameworkElement parent)
            : this()
        {
            if (this.UC == null)
            {
                this.UC = parent;
                this.UC.SizeChanged += UC_SizeChanged;
                this.UC.Unloaded += UC_Unloaded;
            }

            foreach (VKAttachment attachment in attachments)
            {
                switch (attachment.type)
                {
                    case VKAttachmentType.Audio:
                        {
                            AttachAudioUC a = new AttachAudioUC(attachment.audio);
                            this.sp.Children.Add(a);
                            break;
                        }
                    case VKAttachmentType.Audio_Message:
                        {
                            AttachVoiceMessageUC a = new AttachVoiceMessageUC(attachment.audio_message);
                            this.sp.Children.Add(a);
                            break;
                        }
                    case VKAttachmentType.Poll:
                        {
                            AttachPollUC a = new AttachPollUC(attachment.poll);
                            a.Margin = new Thickness(10);
                            this.sp.Children.Add(a);
                            break;
                        }
                    case VKAttachmentType.Link:
                        {
                            if (attachment.link.url == null)
                                continue;

                            AttachLinkUC a = new AttachLinkUC(attachment.link);
                            this.sp.Children.Add(a);
                            break;
                        }
                    case VKAttachmentType.Photo:
                        {
                            this.mp.Items.Add(attachment.photo);
                            break;
                        }
                    case VKAttachmentType.Sticker:
                        {
                            UC.Attachment.AttachStickerUC s = new AttachStickerUC(attachment.sticker);
                            this.sp.Children.Add(s);
                            break;
                        }
                    case VKAttachmentType.Wall:
                        {
                            UC.ForwardedMessagesUC fwd = new ForwardedMessagesUC(attachment.wall, parent);
                            this.sp.Children.Add(fwd);
                            break;
                        }
                    case VKAttachmentType.Video:
                        {
                            if (attachment.video.width == 0 || attachment.video.height == 0)
                            {
                                attachment.video.width = 1920;
                                attachment.video.height = 1080;
                            }

                            AttachVideoUC a = new AttachVideoUC(attachment.video);
                            this.mp.Items.Add(a);
                            break;
                        }
                    case VKAttachmentType.Doc:
                        {
                            AttachDocumentUC a = new AttachDocumentUC(attachment.doc, attachments.Count > 1);
                            if(attachment.doc.type == VKDocumentType.IMAGE && attachment.doc.preview!=null)
                                this.mp.Items.Add(a);
                            else
                                this.sp.Children.Add(a);
                            break;
                        }
                    case VKAttachmentType.Gift:
                        {
                            AttachGiftUC a = new AttachGiftUC(attachment.gift);
                            this.sp.Children.Add(a);
                            break;
                        }
                    case VKAttachmentType.Graffiti:
                        {
                            Image img = new Image();
                            string url = attachment.graffiti.photo_200;
                            if (string.IsNullOrEmpty(url))
                                url = attachment.graffiti.url;

                            img.Source = new BitmapImage(new Uri(url));
                            img.Width = 200;
                            img.Height = 200;

                            this.sp.Children.Add(img);
                            break;
                        }
                    case VKAttachmentType.Wall_reply:
                        {
                            ForwardedMessagesUC fwd = new ForwardedMessagesUC(attachment.wall_reply, parent);
                            this.sp.Children.Add(fwd);
                            break;
                        }
                    case VKAttachmentType.Pretty_Cards:
                        {
                            AttachPrettyCardsUC a = new AttachPrettyCardsUC(attachment.pretty_cards);
                            this.sp.Children.Add(a);
                            break;
                        }

                    case VKAttachmentType.Album:
                        {
                            AttachAlbumUC a = new AttachAlbumUC(attachment.album);
                            this.sp.Children.Add(a);
                            break;
                        }
                    case VKAttachmentType.Call:
                        {
                            AttachCall a = new AttachCall(attachment.call);
                            this.sp.Children.Add(a);
                            break;
                        }
                    case VKAttachmentType.Podcast:
                        {
                            AttachPodcastUC a = new AttachPodcastUC();
                            a.DataContext = attachment.podcast;
                            this.sp.Children.Add(a);
                            break;
                        }
                    case VKAttachmentType.Emoji:
                        {
                            TextBlock a = new TextBlock() { FontSize = 50, Text = attachment.emoji };
                            this.sp.Children.Add(a);
                            break;
                        }
                    default:
                        {
#if DEBUG
                            System.Diagnostics.Debug.WriteLine("AttachmentPresenter: " + attachment.type.ToString());
#endif
                            break;
                        }
                }
            }

            this.Finish();
        }

        void UC_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double w = e.NewSize.Width;
            var element = sender as FrameworkElement;
            if (element.Margin.Left != 0 || element.Margin.Right != 0)
            {
                w -= element.Margin.Left;
                w -= element.Margin.Right;
            }
            this.mp.ReMeasure(w);
        }

        private bool IsLandscape()
        {
            var _info = DisplayInformation.GetForCurrentView();
            return _info.CurrentOrientation == DisplayOrientations.Landscape || _info.CurrentOrientation == DisplayOrientations.LandscapeFlipped;
        }
    }
}
