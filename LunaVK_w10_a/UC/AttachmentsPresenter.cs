using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.UC.Attachment;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using LunaVK.Core.Utils;
using Windows.Graphics.Display;
using System.Diagnostics;
using LunaVK.Core;
/*
* текст + влож = отступы со всех сторон
* только текст = отступы со всех сторон
* картинка вложена = отступов нет
* статья/ссылка/документ = отступы со всех сторон
*/
namespace LunaVK.UC
{
    public class AttachmentsPresenter : StackPanel
    {
        public AttachmentsPresenter()
        {
            base.Loaded += AttachmentsPresenter_Loaded;
            base.Unloaded += AttachmentsPresenter_Unloaded;
        }

        private ScrollableTextBlock _textBlock;
        private MediaPresenter _mediaPresenter;
        private StackPanel _listPresenter;

        public ScrollableTextBlock TextPresenter
        {
            get
            {
                if (this._textBlock == null)
                    this._textBlock = new ScrollableTextBlock() { SelectionEnabled = true, FullOnly = this.IsMessage };

                return this._textBlock;
            }
            private set
            {
                this._textBlock = value;
            }
        }

        /// <summary>
        /// Представляет медиавложения.
        /// </summary>
        public MediaPresenter MediaPresenter
        {
            get
            {
                if (this._mediaPresenter == null)
                    this._mediaPresenter = new MediaPresenter();

                return this._mediaPresenter;
            }
            private set
            {
                this._mediaPresenter = value;
            }
        }

        /// <summary>
        /// Представляет элементы в виде списка.
        /// </summary>
        public StackPanel ListPresenter
        {
            get
            {
                if (this._listPresenter == null)
                {
                    this._listPresenter = new StackPanel();
                    if (this.IsMessage)
                        this._listPresenter.CornerRadius = new CornerRadius(15);
                }
                return this._listPresenter;
            }
            private set
            {
                _listPresenter = value;
            }
        }

#region Attachments
        /// <summary>
        /// Список прикреплений, которые требуется отобразить.
        /// </summary>
        public IReadOnlyList<VKAttachment> Attachments
        {
            get { return (IReadOnlyList<VKAttachment>)GetValue(AttachmentsProperty); }
            set { SetValue(AttachmentsProperty, value); }
        }

        public static readonly DependencyProperty AttachmentsProperty = DependencyProperty.Register(nameof(Attachments), typeof(IReadOnlyList<VKAttachment>), typeof(AttachmentsPresenter), new PropertyMetadata(null, AttachmentsPresenter.OnAttachmentsChanged));

        private static void OnAttachmentsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            
            var presenter = (AttachmentsPresenter)obj;
            presenter.ProcessAttachments();
        }
#endregion

        /// <summary>
        /// Если в числе вложений только картинки, то мы можем отступы убрать потом.
        /// </summary>
        public bool IsOnlyImages = false;

        public bool ForceMediaPresenterMargin = false;

        //после пересланных срабатывает
        private void ProcessAttachments()
        {
            this.IsOnlyImages = true;

            if (!this.Attachments.IsNullOrEmpty() &&     this.ForwardedMessages.IsNullOrEmpty())
            {
                if (this._mediaPresenter != null)
                {
                    this._mediaPresenter.Items.Clear();
                    if (base.Children.Contains(this._mediaPresenter))
                        base.Children.Remove(this._mediaPresenter);
                }

                if (this._listPresenter != null)
                {
                    this._listPresenter.Children.Clear();
                    if (base.Children.Contains(this._listPresenter))
                        base.Children.Remove(this._listPresenter);
                }
            }
            //
            //
            if(!this.IsMessage)
            {
                if(this.Attachments.IsNullOrEmpty())
                {
                    if (this._mediaPresenter != null)
                    {
                        //Debug.Assert(this._mediaPresenter.Items.Count == 0);
                        this._mediaPresenter.Items.Clear();
                        if (base.Children.Contains(this._mediaPresenter))
                            base.Children.Remove(this._mediaPresenter);
                    }

                    if (this._listPresenter != null)
                    {
                        //Debug.Assert(this._listPresenter.Children.Count == 0);
                        this._listPresenter.Children.Clear();
                        if (base.Children.Contains(this._listPresenter))
                            base.Children.Remove(this._listPresenter);
                    }
                }
            }
            //
            //
            /*но чистит переылаемые сообщения
            if (this._listPresenter != null)
            {
                if(this._listPresenter.Children.Count>0)
                {
                    this._listPresenter.Children.Clear();
                }
            }
            */
            /*
#if DEBUG
            if (this._listPresenter != null)
            {
                Debug.Assert(this._listPresenter.Children.Count==0);
            }

            if (this._mediaPresenter != null)
            {
                Debug.Assert(this._mediaPresenter.Items.Count == 0);
            }
#endif
*/
            if (this.Attachments != null)
            {
                //System.Diagnostics.Debug.WriteLine("ProcessAttachments" + this.Attachments.Count);

                foreach (VKAttachment attachment in this.Attachments)
                {
                    if (attachment.type != VKAttachmentType.Photo)
                        this.IsOnlyImages = false;

                    switch (attachment.type)
                    {
                        case VKAttachmentType.Audio:
                            {
                                AttachAudioUC a = new AttachAudioUC(attachment.audio);
                                if (this.ListPresenter.Children.Count > 0 || (this._mediaPresenter!=null && this._mediaPresenter.Items.Count > 0))
                                    a.Margin = new Thickness(0,15,0,0);
                                this.ListPresenter.Children.Add(a);
                                break;
                            }
                        case VKAttachmentType.Audio_Message:
                            {
                                AttachVoiceMessageUC a = new AttachVoiceMessageUC(attachment.audio_message);
                                this.ListPresenter.Children.Add(a);
                                break;
                            }
                        case VKAttachmentType.Poll:
                            {
                                AttachPollUC a = new AttachPollUC(attachment.poll);
                                a.Margin = new Thickness(10);
                                this.ListPresenter.Children.Add(a);
                                break;
                            }
                        case VKAttachmentType.Link:
                            {
                                FrameworkElement a = null;

                                if (attachment.link.IsAMP)
                                {
                                    a = new AttachArticleUC();
                                }
                                else
                                {
                                    if (this.IsMessage)
                                    {
                                        a = new NewsLinkMediumUC();
                                    }
                                    else
                                    {
                                        a = new AttachLinkUC();
                                    }
                                }

                                a.DataContext = attachment.link;

                                this.ListPresenter.Children.Add(a);
                                break;
                            }
                        case VKAttachmentType.Photo:
                            {
                                this.MediaPresenter.Items.Add(attachment.photo);
                                //imagesExists = true;
                                break;
                            }
                        case VKAttachmentType.Sticker:
                            {
                                AttachStickerUC s = new AttachStickerUC(attachment.sticker);
                                this.ListPresenter.Children.Add(s);
                                //imagesExists = true;
                                break;
                            }
                        case VKAttachmentType.Wall:
                            {
                                ForwardedMessagesUC fwd = new ForwardedMessagesUC(attachment.wall, this.IsMessage);
                                this.ListPresenter.Children.Add(fwd);
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
                                this.MediaPresenter.Items.Add(a);
                                //imagesExists = true;
                                break;
                            }
                        case VKAttachmentType.Doc:
                            {
                                AttachDocumentUC a = new AttachDocumentUC(attachment.doc, Attachments.Count > 1 && this.Attachments[0] != attachment);
                                if (attachment.doc.type == VKDocumentType.IMAGE && attachment.doc.preview != null)
                                    this.MediaPresenter.Items.Add(a);
                                else if(attachment.doc.type == VKDocumentType.GIF && attachment.doc.preview != null)
                                    this.MediaPresenter.Items.Add(a);
                                else
                                    this.ListPresenter.Children.Add(a);
                                break;
                            }
                        case VKAttachmentType.Gift:
                            {
                                AttachGiftUC a = new AttachGiftUC(attachment.gift);
                                this.ListPresenter.Children.Add(a);
                                break;
                            }
                        case VKAttachmentType.Graffiti:
                            {
                                Image img = new Image();
                                string url = attachment.graffiti.photo_200;
                                if (string.IsNullOrEmpty(url))
                                    url = attachment.graffiti.url;

                                img.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(url));
                                img.Width = 200;
                                img.Height = 200;

                                this.ListPresenter.Children.Add(img);
                                break;
                            }
                        case VKAttachmentType.Wall_reply:
                            {
                                ForwardedMessagesUC fwd = new ForwardedMessagesUC(attachment.wall_reply, null);
                                this.ListPresenter.Children.Add(fwd);
                                break;
                            }
                        case VKAttachmentType.Pretty_Cards:
                            {
                                AttachPrettyCardsUC a = new AttachPrettyCardsUC(attachment.pretty_cards);
                                this.ListPresenter.Children.Add(a);
                                break;
                            }

                        case VKAttachmentType.Album:
                            {
                                AttachAlbumUC a = new AttachAlbumUC(attachment.album);
                                this.ListPresenter.Children.Add(a);
                                break;
                            }
                        case VKAttachmentType.Call:
                            {
                                AttachCall a = new AttachCall(attachment.call);
                                this.ListPresenter.Children.Add(a);
                                break;
                            }
                        case VKAttachmentType.Podcast:
                            {
                                AttachPodcastUC a = new AttachPodcastUC();
                                a.DataContext = attachment.podcast;
                                this.ListPresenter.Children.Add(a);
                                break;
                            }
                        case VKAttachmentType.Repost:
                            {
                                //ItemWallPostUC a = new ItemWallPostUC() { DataContext = attachment.newsfeed_post };
                                ItemNewsFeedUC a = new ItemNewsFeedUC() { DataContext = attachment.newsfeed_post };
                                this.ListPresenter.Children.Add(a);
                                break;
                            }
                        case VKAttachmentType.Emoji:
                            {
                                TextBlock a = new TextBlock() { FontSize = 50, Text = attachment.emoji };
                                this.ListPresenter.Children.Add(a);
                                break;
                            }
                        case VKAttachmentType.Geo:
                            {
                                /*
                                 * this._geoItem = !this._isMessage ? (!(this._geo.type == "place") ? (showMap ? this.CreateMapPointFull(topMargin) : this.CreateMapPointSmall(topMargin)) : (showMap ? this.CreateMapPlaceFull(topMargin) : this.CreateMapPlaceSmall(topMargin))) : this.CreateMapPointSimple(topMargin);
                    this.VirtualizableChildren.Add((IVirtualizable)this._geoItem);
                    */
                                if (attachment.geo.type == "place")
                                {

                                }
                                else if (attachment.geo.type == "point")
                                {

                                }
                                MapPointSmallAttachmentUC a = new MapPointSmallAttachmentUC(attachment.geo);
                                this.ListPresenter.Children.Add(a);
                                break;
                            }
                        case VKAttachmentType.Page:
                            {
                                /*
                                if (this._attachment.type == "page" && this._attachment.Page != null)
                                {
                                    this._title = this._attachment.Page.title ?? "";
                                    this._subtitle = CommonResources.WikiPage;
                                    this._navigateUri = string.Format("https://vk.com/club{0}?w=page-{0}_{1}", this._attachment.Page.gid, this._attachment.Page.pid);
                                    this._iconSrc = "/Resources/WallPost/AttachLink.png";
                                }
                                */
                                FrameworkElement a = new AttachLinkUC();
                                VKLink link = new VKLink();
                                link.caption = LocalizedStrings.GetString("WikiPage");
                                link.url = string.Format("https://vk.com/club{0}?w=page-{0}_{1}", attachment.page.group_id, attachment.page.id);
                                link.title = attachment.page.title;
                                a.DataContext = link;
                                this.ListPresenter.Children.Add(a);
                                break;
                            }
                        case VKAttachmentType.Market:
                            {
                                NewsLinkMediumUC a = new NewsLinkMediumUC();
                                VKLink link = new VKLink();
                                link.button = new VKPrettyCard.Button();
                                link.button.title = LocalizedStrings.GetString("ViewProduct");
                                link.button.action = new VKPrettyCard.Action();
                                link.button.action.url = string.Format("https://vk.com/product{0}_{1}", attachment.market.owner_id, attachment.market.id);
                                link.caption = attachment.market.description;
                                link.title = attachment.market.title;
                                link.description = attachment.market.PriceString;
                                link.photo = new VKPhoto();
                                link.photo.sizes = new Dictionary<char, VKImageWithSize>();
                                link.photo.sizes.Add('m', new VKImageWithSize() { height = 400, width = 400, type = 'm', url = attachment.market.thumb_photo });
                                a.DataContext = link;
                                this.ListPresenter.Children.Add(a);
                                break;
                            }
                        case VKAttachmentType.Event:
                            {
                                FrameworkElement a = new AttachEventUC();
                                a.DataContext = attachment.@event;
                                this.ListPresenter.Children.Add(a);
                                break;
                            }
                        case VKAttachmentType.Story:
                            {
                                FrameworkElement a = new AttachStory(attachment.story);
                                this.ListPresenter.Children.Add(a);
                                break;
                            }
                        default:
                            {
#if DEBUG
                                Debug.WriteLine("AttachmentSPresenter: " + attachment.type.ToString());
#endif
                                break;
                            }
                    }
                }
            }

            










            

            if (this._mediaPresenter != null && !base.Children.Contains(this._mediaPresenter))
            {
                if (this.IsMessage)
                    _mediaPresenter.CornerRadius = 6;

                this._mediaPresenter.MaxRectSize = this.GetRectangle();

                base.Children.Add(this._mediaPresenter);
                this._mediaPresenter.Update();
            }

            if (this._listPresenter != null && !base.Children.Contains(this._listPresenter))
            {
                base.Children.Add(this._listPresenter);
            }
            //
            this.UpdateMargin();
        }

        private Rectangle GetRectangle()
        {
            double w = base.ActualWidth == 0 ? 600 : base.ActualWidth;
            //double w = 600;
            double maxH = this.IsMessage ? 300 : 400;
            double h = double.IsInfinity(base.MaxHeight) ? maxH : base.MaxHeight;

            if(double.IsInfinity(base.MaxWidth))
            {
                bool isLandscape = this.IsLandscape;
                object parent = base.Parent;
                while (parent != null)
                {
                    FrameworkElement element = parent as FrameworkElement;
                    double value = isLandscape ? 600 : element.MaxWidth;
                    if (!double.IsInfinity(value))
                    {
                        w = value;
                        break;
                    }
                    else
                    {
                        parent = element.Parent;
                    }
                }
            }
            else
            {
                w = base.MaxWidth;
                //w = this.IsLandscape ? Window.Current.Bounds.Height : Window.Current.Bounds.Width;
            }
            //Rectangle ret = new Rectangle(!double.IsInfinity(base.MaxWidth) ? base.MaxWidth :
            //        this.IsLandscape() ? Window.Current.Bounds.Height : Window.Current.Bounds.Width,
            //        !double.IsInfinity(base.MaxHeight) ? base.MaxHeight : 300);

            return new Rectangle(w,h);
        }

        private void UpdateMargin()
        {
            if(!string.IsNullOrEmpty(this.Text))//Есть текст
            {
                if (this.IsMessage)
                {
                    if (!this.Attachments.IsNullOrEmpty() || !this.ForwardedMessages.IsNullOrEmpty())//Есть документы или есть пересылаемые сообщения
                    {
                        base.Margin = new Thickness(10);
                        this._textBlock.Margin = new Thickness(0, 0, 0, 10);
                    }
                    //else if (!this.ForwardedMessages.IsNullOrEmpty())//Есть пересылаемые сообщения
                    //{
                    //    base.Margin = new Thickness(10);
                    //    this._textBlock.Margin = new Thickness(0, 0, 0, 5);
                    //}
                    else
                    {
                        base.Margin = new Thickness();
                        this._textBlock.Margin = new Thickness(14, 7, 14, 7);
                    }
                }
                else
                {
                    base.Margin = new Thickness();
                    this._textBlock.Margin = new Thickness(10, 0, 10, 10);
                    if (this._listPresenter!=null)
                        this._listPresenter.Margin = new Thickness(10,0,10,0);
                }
            }
            else
            {
                //Нет текта
                if (this.IsMessage)
                {
                    if (this.Attachments != null && this.Attachments.Count > 0)
                    {
                        if (this.IsOnlyImages == false)
                            base.Margin = new Thickness(10);
                        else
                            base.Margin = new Thickness();
                    }
                    else if (this.ForwardedMessages != null && this.ForwardedMessages.Count > 0)
                    {
                        base.Margin = new Thickness(10);
                    }
                    else
                    {
                        base.Margin = new Thickness();
                    }
                }
                else
                {
                    base.Margin = new Thickness();
                    if (this._listPresenter != null)
                        this._listPresenter.Margin = new Thickness(10, 0, 10, 0);
                }
            }
            
            if(this.ForceMediaPresenterMargin)
            {
                if (this._mediaPresenter != null)
                    this._mediaPresenter.Margin = new Thickness(10, 0, 10, 0);
            }

            base.InvalidateMeasure();
        }

#region IsMessage
        public static readonly DependencyProperty IsMessageProperty = DependencyProperty.Register("IsMessage", typeof(bool), typeof(AttachmentsPresenter), new PropertyMetadata(default(bool)));
        public bool IsMessage
        {
            get { return (bool)GetValue(IsMessageProperty); }
            set { SetValue(IsMessageProperty, value); }
        }
#endregion

#region ForwardedMessages
        public IReadOnlyList<VKMessage> ForwardedMessages
        {
            get { return (IReadOnlyList<VKMessage>)GetValue(ForwardedMessagesProperty); }
            set { SetValue(ForwardedMessagesProperty, value); }
        }

        public static readonly DependencyProperty ForwardedMessagesProperty = DependencyProperty.Register("ForwardedMessages", typeof(IReadOnlyList<VKMessage>), typeof(AttachmentsPresenter), new PropertyMetadata(null, AttachmentsPresenter.OnForwardedMessagesChanged));

        private static void OnForwardedMessagesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var presenter = (AttachmentsPresenter)obj;
            presenter.ProcessForwardedMessages();
        }

        private void ProcessForwardedMessages()
        {
            if (this._mediaPresenter != null)
            {
                this._mediaPresenter.Items.Clear();
                if (base.Children.Contains(this._mediaPresenter))
                    base.Children.Remove(this._mediaPresenter);
            }

            if (this._listPresenter != null)
            {
                this._listPresenter.Children.Clear();
                if (base.Children.Contains(this._listPresenter))
                    base.Children.Remove(this._listPresenter);
            }

            

            if (this.ForwardedMessages != null)
            {
                //System.Diagnostics.Debug.WriteLine("ProcessForwardedMessages" + this.ForwardedMessages.Count);

                foreach (VKMessage msg in this.ForwardedMessages)
                {
                    //
                    if (msg.text == "" && msg.fwd_messages == null && msg.attachments == null && msg.geo == null)
                        msg.text = "(контент удалён)";
                    //
                    //double offs = 0;
                    //if (this.DataVM.UserThumbVisibility == Visibility.Visible)
                    //    offs = 64;
                    ForwardedMessagesUC fwd = new ForwardedMessagesUC(msg, this, 0);
                    this.ListPresenter.Children.Add(fwd);
                }
            }




            if (this._mediaPresenter != null)
            {
                if (this.IsMessage)
                    _mediaPresenter.CornerRadius = 6;

                this._mediaPresenter.MaxRectSize = this.GetRectangle();

                base.Children.Add(this._mediaPresenter);
                this._mediaPresenter.Update();
            }

            if (this._listPresenter != null)
                base.Children.Add(this._listPresenter);

            //
            this.UpdateMargin();
        }
#endregion

#region Text
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(AttachmentsPresenter), new PropertyMetadata(null, AttachmentsPresenter.OnTextChanged));

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var presenter = (AttachmentsPresenter)obj;
            presenter.ProcessText();
        }

        private void ProcessText()
        {
            if (string.IsNullOrEmpty(this.Text))
            {
                if (base.Children.Contains(this._textBlock))
                    base.Children.Remove(this._textBlock);
                this.TextPresenter = null;
            }
            else
            {
                this.TextPresenter.Text = this.Text;
            }





            if (this._textBlock != null)
            {
                if (!base.Children.Contains(this._textBlock))
                    base.Children.Insert(0, this._textBlock);
            }

            this.UpdateMargin();
        }
#endregion





        private bool IsLandscape
        {
            get
            {
                var _info = DisplayInformation.GetForCurrentView();
                return _info.CurrentOrientation == DisplayOrientations.Landscape || _info.CurrentOrientation == DisplayOrientations.LandscapeFlipped;
            }
        }

        private void AttachmentsPresenter_Unloaded(object sender, RoutedEventArgs e)
        {
            base.SizeChanged -= AttachmentsPresenter_SizeChanged;
        }

        private void AttachmentsPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            base.SizeChanged += AttachmentsPresenter_SizeChanged;
            //
            if (this._mediaPresenter != null)
                this._mediaPresenter.ReMeasure(this.ActualWidth);
        }

        private void AttachmentsPresenter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this._mediaPresenter != null)
                this._mediaPresenter.ReMeasure(e.NewSize.Width);
        }
    }
}
