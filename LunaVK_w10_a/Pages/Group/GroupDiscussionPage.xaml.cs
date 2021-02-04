using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Framework;
using LunaVK.UC;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.Pages.Group
{
    public sealed partial class GroupDiscussionPage : PageBase
    {
        private uint _replyToCid;
        private PopUpService _flyout;
        private bool _isAddingComment;

        public GroupDiscussionPage()
        {
            this.InitializeComponent();
#if DEBUG
            if(Debugger.IsAttached)
                this._listPages.Visibility = Visibility.Visible;
#endif
            this.ucNewMessage.IsVoiceMessageButtonEnabled = false;
            this.ucNewMessage.ReplyNameTapped = this.ReplyUser_Tapped;
            this.ucNewMessage.OnSendTap = this._appBarButtonAddComment_Click;
            this.ucNewMessage.TextBoxNewComment.TextChanged += this.TextBoxNewComment_TextChanged;
            this.ucNewMessage.OnAddAttachTap = this.AddAttachTap;
            this.ucNewMessage.StickerTapped += this.PanelControl_StickerTapped;

        }

        public GroupDiscussionViewModel VM
        {
            get { return base.DataContext as GroupDiscussionViewModel; }
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                //               this.VM.LoadingStatusUpdated(ProfileLoadingStatus.Loaded);
            }
            else
            {
                Dictionary<string, object> QueryString = navigationParameter as Dictionary<string, object>;
                uint groupId = (uint)QueryString["GroupId"];//уже плюсовая
                uint topicId = (uint)QueryString["TopicId"];
                string topicName = (string)QueryString["TopicName"];
                bool canComment = (bool)QueryString["CanComment"];
                uint commentId = (uint)QueryString["CommentId"];

                this.DataContext = new GroupDiscussionViewModel(groupId, topicId, this.ReplyToComment, canComment, commentId);
                base.Title = topicName;
                
            }

            this.VM.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;

            Binding binding = new Binding() { Source = this.VM.Attachments, Mode = BindingMode.OneTime };
            this.ucNewMessage.ItemsControlAttachments.SetBinding(ItemsControl.ItemsSourceProperty, binding);
        }

        private void ReplyUser_Tapped()
        {
            this.ucNewMessage.ReplyToUserName = "";
            this._replyToCid = 0;
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

        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            this.VM.LoadingStatusUpdated -= this.HandleLoadingStatusUpdated;
        }

        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {
            if (status == ProfileLoadingStatus.Loaded)
            {
            }
        }

        private void _appBarButtonAddComment_Click()
        {
            if (this._isAddingComment)
                return;
            string text = this.ucNewMessage.TextBoxNewComment.Text;
            //if (text.Length < 2 && this.VM.Attachments.Count == 0)
            //    return;
            this._isAddingComment = true;
            this.VM.AddComment(text.Replace("\r\n", "\r").Replace("\r", "\r\n"), this.VM.Attachments.ToList(), (res =>
            {
                this._isAddingComment = false;
                
                    if (res)
                    {
                    this.ucNewMessage.TextBoxNewComment.Text = string.Empty;
                        //this.InitializeCommentVM();
                        //this.UpdateAppBar();
                    }
                   //else
                  //      ExtendedMessageBox.ShowSafe(CommonResources.Error);
                
            }), 0, this.ucNewMessage.IsFromGroupChecked, "");
        }

        public bool ReadyToSend
        {
            get
            {
                string text = this.ucNewMessage.TextBoxNewComment.Text;
                
                if (text.Length > 2 && this.VM.Attachments.Count == 0)
                    return true;
                if (this.VM.Attachments.Count > 0)
                    return this.VM.Attachments.All((a => a.UploadState == OutboundAttachmentUploadState.Completed));
                return false;
            }
        }

        private void TextBoxNewComment_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            this.ucNewMessage.ActivateSendButton(this.ReadyToSend && this.VM.CanComment);
        }

        private void AddAttachTap(FrameworkElement sender)
        {
            if (this.VM.Attachments.Count >= 10)
                return;

            if (this._flyout == null)
            {
                this._flyout = new PopUpService();
                this._flyout.OverrideBackKey = true;
                this._flyout.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            }

            AttachmentPickerUC uc = new AttachmentPickerUC((byte)this.VM.Attachments.Count, 10);
            uc.AttachmentsAction = this.HandleAttachmentsAction;
            //uc.DrawGraffiti = this.HandleStartGraffitiAction;
            this._flyout.Child = uc;
            this._flyout.Show();
        }

        private void HandleAttachmentsAction(IReadOnlyList<IOutboundAttachment> list)
        {
            foreach (var attach in list)
                this.VM.Attachments.Add(attach);

            if (list.Count == 1)
            {
                if (list[0] is OutboundDocumentAttachment doc)
                {
                    if (doc._pickedDocument != null && doc._pickedDocument.IsGraffiti)
                    {
                        //this.BorderSend_Tapped();
                    }
                }
            }

            this._flyout.Hide();
        }

        void PanelControl_StickerTapped(object sender, VKSticker sticker)
        {
            if (sticker.is_allowed)
                this.VM.AddComment("",null,null, sticker.sticker_id);
            //else
            //    this.VM.SendStickerAsGraffiti(sticker);
        }
    }
}
