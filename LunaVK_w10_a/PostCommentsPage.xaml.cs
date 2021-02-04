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

using LunaVK.Framework;
using LunaVK.Core.ViewModels;
using LunaVK.Core;
using LunaVK.Core.Enums;
using LunaVK.Core.DataObjects;
using LunaVK.UC.AttachmentPickers;
using LunaVK.Core.Library;
using LunaVK.UC;
using LunaVK.Library;

namespace LunaVK
{
    public sealed partial class PostCommentsPage : PageBase
    {
        private bool likesAdded;
        //UC.PopUP popAttach = null;
        private PopUpService _flyout;
        private uint _replyToCid;

        private PostCommentsViewModel VM
        {
            get { return base.DataContext as PostCommentsViewModel; }
        }

        public PostCommentsPage()
        {
            this.InitializeComponent();

            base.Title = LocalizedStrings.GetString("Conversation_WallPost");

            //            this._post.IsForComments = true;
            this.ucNewMessage.OnAddAttachTap = this.AddAttachTap;
            this.ucNewMessage.OnSendTap = this.BorderSend_Tapped;
            this.ucNewMessage.TextBoxNewComment.TextChanged += TextBoxNewComment_TextChanged;
            this.ucNewMessage.ReplyNameTapped = this.ReplyUser_Tapped;
            //this.ucNewMessage.PanelControl.StickerTapped += PanelControl_StickerTapped;
            this.ucNewMessage.StickerTapped += PanelControl_StickerTapped;//this.ucNewMessage.StickersAutoSuggest.StickerTapped += PanelControl_StickerTapped;
            this.ucNewMessage.OnImageDeleteTap = this.ImageDeleteTap;
        }

        void TextBoxNewComment_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            this.ucNewMessage.ActivateSendButton(this.VM.Attachments.Count > 0 || !string.IsNullOrEmpty(tb.Text));
        }

        private void ImageDeleteTap(IOutboundAttachment attachment)
        {
            this.VM.Attachments.Remove(attachment);
        }

        private void ReplyUser_Tapped()
        {
            this.ucNewMessage.ReplyToUserName = "";
            this._replyToCid = 0;
        }

        private void BorderSend_Tapped()
        {
            string temp = this.ucNewMessage.TextBoxNewComment.Text;

            if (!this.VM.CanPostComment(temp, this.VM.Attachments.ToList()))
                return;

            this.ucNewMessage.TextBoxNewComment.Text = "";//Очищаем поле

            this.VM.PostComment(temp, this._replyToCid, this.ucNewMessage.IsFromGroupChecked, this.VM.Attachments.ToList());
            this._replyToCid = 0;
            this.ucNewMessage.ReplyToUserName = "";
            this.VM.Attachments.Clear();
        }

        void PanelControl_StickerTapped(object sender, VKSticker sticker)
        {
            //this.VM.PostComment("", 0, this.ucNewMessage.IsFromGroupChecked, null, null, sticker.sticker_id);

            if (sticker.is_allowed)
                this.VM.PostComment("", 0, this.ucNewMessage.IsFromGroupChecked, null, null, sticker.sticker_id);
        }

        void AddAttachTap(FrameworkElement sender)
        {
            /*
             * public static readonly List<NamedAttachmentType> AttachmentTypesWithPhotoFromGalleryAndLocation = new List<NamedAttachmentType>()
             * { new NamedAttachmentType() { AttachmentType = AttachmentType.Photo, Name = CommonResources.AttachmentType_Photo }, 
             * new NamedAttachmentType() { AttachmentType = AttachmentType.Audio, Name = CommonResources.AttachmentType_Audio },
             * new NamedAttachmentType() { AttachmentType = AttachmentType.Video, Name = CommonResources.AttachmentType_Video },
             * new NamedAttachmentType() { AttachmentType = AttachmentType.Document, Name = CommonResources.AttachmentType_Document }, 
             * new NamedAttachmentType() { AttachmentType = AttachmentType.Location, Name = CommonResources.AttachmentType_Location } };
             */

            if (this.VM.Attachments.Count >= 2)
                return;

            this._flyout = new PopUpService();
            this._flyout.OverrideBackKey = true;
            this._flyout.AnimationTypeChild = PopUpService.AnimationTypes.Slide;

            PopUP2 menu = new PopUP2();

            PopUP2.PopUpItem item = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("AttachmentType_PhotoVideo") };
            item.Command = new DelegateCommand((args) =>
            {
                //               PhotoVideoPickerUC uc = new PhotoVideoPickerUC((byte)this.VM.Attachments.Count,2);
                //               uc.AttachmentsAction = this.HandleAttachmentsAction;
                //               this._flyout.Child = uc;
                this._flyout.Show();
            });
            menu.Items.Add(item);

            PopUP2.PopUpItem item2 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("AttachmentType_Audio") };
            menu.Items.Add(item2);

            PopUP2.PopUpItem item5 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("AttachmentType_Document") };
            item5.Command = new DelegateCommand((args) =>
            {
                DocumentsPickerUC uc = new DocumentsPickerUC();
                uc.AttachmentsAction = this.HandleAttachmentsAction;
                uc.DocumentAction = this.HandleDocumentAction;
                this._flyout.Child = uc;
                this._flyout.Show();
            });
            menu.Items.Add(item5);

            PopUP2.PopUpItem item6 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("AttachmentType_Location") };
            item6.Command = new DelegateCommand((args) =>
            {
                LocationPickerUC uc = new LocationPickerUC();
                uc.AttachmentsAction = this.HandleAttachmentsAction;
                this._flyout.Child = uc;
                this._flyout.Show();
            });
            menu.Items.Add(item6);

            menu.ShowAt(sender);
        }

        private void HandleDocumentAction(VKDocument doc)
        {
            OutboundDocumentAttachment o = new OutboundDocumentAttachment(doc);
            this.VM.Attachments.Add(o);
            this.VM.UploadAttachments();
            this._flyout.Hide();
        }

        private void HandleAttachmentsAction(IReadOnlyList<IOutboundAttachment> list)
        {
            foreach (var attach in list)
            {
                if (attach is OutboundPhotoAttachment p)
                    p.IsForWallPost = true;
                this.VM.Attachments.Add(attach);
            }
            this.VM.UploadAttachments();
            this._flyout.Hide();
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            IDictionary<string, object> QueryString = e.Parameter as IDictionary<string, object>;

            uint postId = (uint)QueryString["PostId"];
            int ownerId = (int)QueryString["PostOwnerId"];
            uint commentId = (uint)QueryString["CommentId"];

            VKBaseDataForPostOrNews postData = null;
            if (QueryString.Keys.Contains("WallPost"))
            {
                postData = (VKBaseDataForPostOrNews)QueryString["WallPost"];

            }

            base.DataContext = new PostCommentsViewModel(postId, ownerId, commentId, this.ReplyToComment, postData);

            Binding binding = new Binding() { Source = this.VM.Attachments, Mode = BindingMode.OneTime };
            this.ucNewMessage.ItemsControlAttachments.SetBinding(ItemsControl.ItemsSourceProperty, binding);

            //var content = (base.DataContext as PostCommentsViewModel).WallPostData;
            //var binding = new Binding() { Mode = BindingMode.OneWay, Path = new PropertyPath("DataContext"), Source = postData };
            //this._post.SetBinding(FrameworkElement.DataContextProperty, binding);

            if (postData is VKWallPost wallPost)
            {
                wallPost.IsRepost = true;//скрываем подвал
                this._post.DataContext = wallPost;
                this._stack.DataContext = wallPost;
            }

            this.VM.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;
            this.VM.Attachments.CollectionChanged += Attachments_CollectionChanged;
            // this.VM.LoadData(true);
        }

        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (this._post.DataContext is VKWallPost wallPost)
            {
                wallPost.IsRepost = false;//открываем подвал
            }
        }

        private void Attachments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.ucNewMessage.ActivateSendButton(this.VM.Attachments.Count > 0 || !string.IsNullOrEmpty(this.ucNewMessage.TextBoxNewComment.Text));
        }

        private void ReplyToComment(VKComment commentItem)
        {
            VKBaseDataForGroupOrUser user = commentItem.User;//UsersService.Instance.GetCachedUser(commentItem.from_id);
            string temp = user.Title;
            if (user is VKUser u)
            {
                if (!string.IsNullOrEmpty(u.first_name_dat))
                {
                    temp = u.first_name_dat + " " + u.last_name_dat;
                }
            }
            this.ucNewMessage.ReplyToUserName = temp;
            this._replyToCid = commentItem.id;
        }

        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {
            if (status == ProfileLoadingStatus.Loaded)
            {
                //this._post.DataContext = this.VM.WallPostData;
                if (this.VM.WallPostData is VKNewsfeedPost)
                    this._post.DataContext = this.VM.WallPostData as VKNewsfeedPost;
                else
                {
                    this._post.DataContext = this.VM.WallPostData as VKWallPost;
                    this.ucNewMessage.SetAdminLevel((int)(this.VM.WallPostData as VKWallPost).AdminLevel);
                }

                if (_likes.ItemsSource == null)
                    _likes.ItemsSource = this.VM.Likes;
                //

                //System.Diagnostics.Debug.WriteLine(string.Format("RealOffset {0} offsetFoPrev {1} items.Count {2}", this.VM.RealOffset, this.VM.offsetFoPrev, this.VM.Items.Count));
                //

                /*
                long ostalos = this.VM.RealOffset + this.VM.offsetFoPrev;

                this.prevCommentBorder.Visibility = ostalos > 0 ? Visibility.Visible : Visibility.Collapsed;

                this.prevCommentText.Text = "Загрузить предыдущие " + (Math.Min(20,ostalos)).ToString() + " комментариев";

                if(!likesAdded)
                {
                    foreach(var like in this.VM.Likes)
                    {
                        Image img = new Image();
                        img.Height = img.Width = 30;
                        img.Margin = new Thickness(5,0,5,0);
                        img.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(like.photo_50));
                        this._post.LikesPanel.Children.Insert(1, img);
                    }
                    
                    likesAdded = true;
                }*/
            }

        }



        /*
        void CallBck(bool result)
        {
            if(result==true)
            {
                this._post.DataContext = this.VM.WallPostData;
                this._post.DataPost = this.VM.WallPostData as Network.DataObjects.VKWallPost;

                Network.DataObjects.VKComment c = this.VM.Items.LastOrDefault();

                if (c != null)
                    this.MainScroll.GetListView.ScrollIntoView(c);
            }
        }
        */
        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //           this.VM.LoadPrev();
        }

        private void _likes_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToLikesPage(this.VM.WallPostData.OwnerId, this.VM.WallPostData.PostId, LikeObjectType.post);
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
        /*
private void ucNewMessage_SizeChanged(object sender, SizeChangedEventArgs e)
{
this.Offset.Height = e.NewSize.Height + 20;//todo:binding
}
*/
    }
}
