using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.UC;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

//WallPostViewModel

namespace LunaVK.Pages
{
    public sealed partial class CommentsPage : PageBase
    {
        private PopUpService _flyout;
        private uint _replyToCid;
        private string _title;

        public CommentsPage()
        {
            this.InitializeComponent();

            this.ucNewMessage.OnSendTap = this._appBarButtonSend_Click;
            this.ucNewMessage.StickerTapped += this.PanelControl_StickerTapped;
            this.ucNewMessage.OnAddAttachTap = this.AddAttachTap;
            this.ucNewMessage.TextBoxNewComment.TextChanged += this.TextBoxNewComment_TextChanged;
            this.ucNewMessage.ReplyNameTapped = this.ReplyUser_Tapped;
            this.ucNewMessage.OnImageDeleteTap = this.ImageDeleteTap;

            this.Loaded += this.CommentsPage_Loaded;
        }

        private void CommentsPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.ucNewMessage.ActivateSendButton(false);
            base.Title = this._title;
        }

        private CommentsViewModel VM
        {
            get { return base.DataContext as CommentsViewModel; }
        }

        private void ReplyUser_Tapped()
        {
            this.ucNewMessage.ReplyToUserName = "";
            this._replyToCid = 0;
            this.ucNewMessage.TextBoxNewComment.Text = "";
        }

        private void _appBarButtonSend_Click()
        {
            //Text.Replace("\r\n", "\r").Replace("\r", "\r\n")();//в оригинале
            string temp = this.ucNewMessage.TextBoxNewComment.Text.Trim().Replace("\r\n", "\r").Replace("\r", "\r\n").Replace("\"", "\\\"");//в оригинале без трима
            //temp = temp.Replace("\r", "\n");
            //temp = temp.Replace("\n", " ");
            if (!this.VM.CanPostComment(temp, this.VM.Attachments.ToList()))
                return;

            this.ucNewMessage.TextBoxNewComment.Text = "";//Очищаем поле

            this.VM.AddComment(temp, this._replyToCid, this.ucNewMessage.IsFromGroupChecked);
            this._replyToCid = 0;
            this.ucNewMessage.ReplyToUserName = "";
            this.VM.Attachments.Clear();
        }

        void PanelControl_StickerTapped(object sender, VKSticker sticker)
        {
            if (sticker.is_allowed)
                this.VM.AddComment("", 0, false, null, sticker.sticker_id);
            //else
            //    this.VM.SendStickerAsGraffiti(sticker);
        }

        private void ReplyToComment(VKComment commentItem)
        {
            VKBaseDataForGroupOrUser user = commentItem.User;//UsersService.Instance.GetCachedUser(commentItem.from_id);
            string temp = user.Title;
            string str1 = "";
            if (user is VKUser u)
            {
                if (!string.IsNullOrEmpty(u.first_name_dat))
                {
                    temp = u.first_name_dat + " " + u.last_name_dat;
                }
                str1 = u.first_name;
            }
            else if (user is VKGroup g)
            {
                str1 = g.name;
            }
            this.ucNewMessage.ReplyToUserName = temp;
            this._replyToCid = commentItem.id;
            this.ucNewMessage.TextBoxNewComment.Text = str1 + ", ";
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            IDictionary<string, object> QueryString = e.Parameter as IDictionary<string, object>;

            object data = null;

            int ownerId = (int)QueryString["OwnerId"];
            uint postId = (uint)QueryString["ItemId"];

            if (QueryString.Keys.Contains("Data"))
                data = QueryString["Data"];

            if (data is VKPhoto photo)
            {
                base.DataContext = new CommentsViewModel(photo, this.ReplyToComment);

                this._title = "Фотография";
            }
            else
            {
                uint commentId = 0;
                if (QueryString.Keys.Contains("CommentId"))
                    commentId = (uint)QueryString["CommentId"];

                VKWallPost forPostOrNews = data as VKWallPost;
                base.DataContext = new CommentsViewModel(ownerId, postId, commentId, this.ReplyToComment, forPostOrNews);
                this._title = LocalizedStrings.GetString("Conversation_WallPost");
            }
            
            Binding binding = new Binding() { Source = this.VM.Attachments, Mode = BindingMode.OneTime };
            this.ucNewMessage.ItemsControlAttachments.SetBinding(ItemsControl.ItemsSourceProperty, binding);
            /*
            if (data is VKWallPost wallPost)
            {
                wallPost.IsRepost = true;//скрываем подвал
                this._post.DataContext = wallPost;
                this._stack.DataContext = wallPost;
            }
            */
            this.VM.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;


            //this.VM.PropertyChanged += this.VM_PropertyChanged;
            this.VM.CanPublish = this.OnCanPublish;
            this.VM.Attachments.CollectionChanged += this.Attachments_CollectionChanged;
        }

        private void Attachments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.OnCanPublish();
        }

        private void OnCanPublish()
        {
            this.ucNewMessage.ActivateSendButton(this.ReadyToSend);
        }
        /*
        private void VM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CanPublish")
                return;
            this.ucNewMessage.ActivateSendButton(this.VM.Attachments.Count > 0 || !string.IsNullOrEmpty(this.ucNewMessage.TextBoxNewComment.Text));
            //this.UpdateAppBar();
            //    ObservableCollection<IOutboundAttachment> outboundAttachments = this.VM.Attachments;
            //    Func<IOutboundAttachment, bool> func1 = (Func<IOutboundAttachment, bool>)(a => a.UploadState == OutboundAttachmentUploadState.Uploading);
            //    if (Enumerable.Any<IOutboundAttachment>(outboundAttachments, func1))
            //        return;
            //    this.PostCommentsVM.SetInProgress(false, "");
        }
        */
        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {
            switch (status)
            {
                case ProfileLoadingStatus.Empty:
                case ProfileLoadingStatus.Loaded:
                    {
                        if (this.VM.Photo != null)
                        {
                            //this.ucNewMessage.SetAdminLevel(adminLevel);
                            if (this.VM.Photo.can_comment == false)
                            {
                                VisualStateManager.GoToState(this.ucNewMessage, "Loading", false);
                                break;
                            }
                        }
                        else if(this.VM.WallPostData!=null)
                        {
                            this.ucNewMessage.SetAdminLevel((int)this.VM.WallPostData.AdminLevel);
                            if ( this.VM.WallPostData.comments.can_post == false)
                            {
                                VisualStateManager.GoToState(this.ucNewMessage, "Loading", false);
                                break;
                            }
                        }
                        VisualStateManager.GoToState(this.ucNewMessage, "Ready", false);
                        this.ucNewMessage.UpdateVisibilityState();
                        break;
                    }
                case ProfileLoadingStatus.Reloading:
                    //case ProfileLoadingStatus.Loading:
                    {
                        VisualStateManager.GoToState(this.ucNewMessage, "Loading", false);
                        break;
                    }
                case ProfileLoadingStatus.Deleted:
                case ProfileLoadingStatus.Banned:
                    {
                        VisualStateManager.GoToState(this.ucNewMessage, "Blocked", false);
                        break;
                    }
                case ProfileLoadingStatus.LoadingFailed:
                    {
                        VisualStateManager.GoToState(this.ucNewMessage, "Ready", false);
                        break;
                    }
            }

        }

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //           this.VM.LoadPrev();
        }

        private void _likes_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToLikesPage(this.VM._ownerId, this.VM._itemId, this.VM.Photo == null ? LikeObjectType.post : LikeObjectType.photo, this.VM.Photo == null ? (int)this.VM.WallPostData.likes.count : (int)this.VM.Photo.likes.count);
        }

        private void AddAttachTap(FrameworkElement sender)
        {
            if (this.VM.Attachments.Count >= 2)
                return;

            if (this._flyout == null)
            {
                this._flyout = new PopUpService();
                this._flyout.OverrideBackKey = true;
                this._flyout.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            }

            AttachmentPickerUC uc = new AttachmentPickerUC((byte)this.VM.Attachments.Count, 2);
            uc.AttachmentsAction = this.HandleAttachmentsAction;
            //uc.DrawGraffiti = this.HandleStartGraffitiAction;
            this._flyout.Child = uc;
            this._flyout.Show();
            /*
             * AttachmentPickerUC.Show(AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation, this._commentVM.NumberOfAttAllowedToAdd, (Action) (() =>
      {
        PostCommentsPage.HandleInputParams(this._commentVM);
        this.UpdateAppBar();
      }), true, 0, 0,  null);
      */
        }

        private void HandleAttachmentsAction(IReadOnlyList<IOutboundAttachment> list)
        {


            foreach (var attach in list)
            {
                if (attach is OutboundPhotoAttachment photoAttachment)
                    photoAttachment.IsForWallPost = true;
                this.VM.Attachments.Add(attach);
            }

            if (list.Count == 1)
            {
                if (list[0] is OutboundDocumentAttachment doc)
                {
                    if (doc._pickedDocument.IsGraffiti)
                    {
                        //this.BorderSend_Tapped();
                    }
                }
            }

            this._flyout.Hide();
            this.VM.UploadAttachments();
        }

        private void Publish_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SharePostUC share = new SharePostUC("записью", WallService.RepostObject.wall, this.VM._ownerId,this.VM._itemId);
            PopUpService popUp = new PopUpService { Child = share, OverrideBackKey = true, AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed };
            share.Done = popUp.Hide;
            popUp.Show();

            e.Handled = true;
        }
        
        private void TextBoxNewComment_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TextBox tb = sender as TextBox;
            //this.ucNewMessage.ActivateSendButton(this.VM.Attachments.Count > 0 || !string.IsNullOrEmpty(tb.Text));
            this.ucNewMessage.ActivateSendButton(this.ReadyToSend);
        }

        private void Like_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this.VM.AddRemoveLike();
        }

        private void Owner_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToProfilePage(this.VM._ownerId);
        }
        
        private bool ReadyToSend
        {
            get
            {
                ObservableCollection<IOutboundAttachment> outboundAttachments = this.VM.Attachments;
                if (!string.IsNullOrWhiteSpace(this.ucNewMessage.TextBoxNewComment.Text) && outboundAttachments.Count == 0)
                    return true;
                if (outboundAttachments.Count > 0)
                    return Enumerable.All<IOutboundAttachment>(outboundAttachments, (a => a.UploadState == OutboundAttachmentUploadState.Completed));
                return false;
            }
        }

        private void ImageDeleteTap(IOutboundAttachment attachment)
        {
            this.VM.Attachments.Remove(attachment);
        }
    }
}
