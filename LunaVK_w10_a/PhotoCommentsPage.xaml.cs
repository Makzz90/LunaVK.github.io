using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Library;
using LunaVK.ViewModels;
using LunaVK.Core.Enums;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class PhotoCommentsPage : PageBase
    {
        VKPhoto data = null;
        int ownerId = 0;

        public PhotoCommentsPage()
        {
            this.InitializeComponent();

            this.ucNewMessage.OnSendTap = this._appBarButtonSend_Click;
        }

        private PhotoViewModel VM
        {
            get { return base.DataContext as PhotoViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            IDictionary<string, object> QueryString = e.Parameter as IDictionary<string, object>;

            this.ownerId = (int)QueryString["OwnerId"];
            uint photoId = (uint)QueryString["PhotoId"];
            string accessKey = "";

            if (QueryString.Keys.Contains("AccessKey"))
            {
                accessKey = (string)QueryString["AccessKey"];
            }
            if (QueryString.Keys.Contains("PhotoContext"))
            {
                data = (VKPhoto)QueryString["PhotoContext"];
            }

            //this.RefreshHeader();


            base.DataContext = new PhotoViewModel(this.ownerId, photoId, accessKey);
            this.VM.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;
        }

        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {
            if (status == ProfileLoadingStatus.Loaded)
            {
                this.GenerateAuthorText();
                this.GeneratePhotoText();
                this.GenerateTextForTags();
                //this.ucCommentGeneric.ProcessLoadedComments(result);
                //this.ucNewMessage.SetAdminLevel(adminLevel);
                //this.stackPanelInfo.Visibility = (result ? Visibility.Visible : Visibility.Collapsed);
                //this.UpdateAppBar();
            }
        }

        private void GenerateAuthorText()
        {
            string date = "";
            if (this.VM.Photo != null)
                date = UIStringFormatterHelper.FormatDateTimeForUI(this.VM.Photo.date);//created
            //this.UserHeader.Initilize(this.VM.OwnerImageUri, this.VM.OwnerName ?? "", date, this.VM.AuthorId);
            this.textBlockName.Text = this.VM.OwnerName ?? "";
            this.textBlockDate.Text = date;
            this.ImageUri.ImageSource = new BitmapImage(new Uri(this.VM.OwnerImageUri));
        }

        private void GeneratePhotoText()
        {
            if (!string.IsNullOrEmpty(this.VM.Text))
            {
                this.textPhotoText.Text = this.VM.Text;
                this.textPhotoText.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                this.textPhotoText.Text = "";
                this.textPhotoText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void GenerateTextForTags()
        {

        }

        private void _appBarButtonSend_Click()
        {
            VKComment comment1 = new VKComment();
            comment1.id = 0;
            comment1.date = DateTime.UtcNow;
            //comment1.text = stickerItemData == null ? str4 : "";
            //comment1.reply_to_cid = this._replyToCid;
            //comment1.reply_to_uid = this._replyToUid;
            comment1.likes = new VKLikes() { can_like = false };
            this.VM.AddComment(comment1, false,(res =>
            {
                if (res==null)
                    return;
                //this.InitializeCommentVM();
                //this.UpdateAppBar();
            }));
        }
    }
}
