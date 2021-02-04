using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using LunaVK.Core.DataObjects;
using LunaVK.Core;
using LunaVK.Core.Enums;
using LunaVK.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using LunaVK.Core.Library;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace LunaVK.UC
{
    //NewsFeedbackItem
    public sealed partial class ItemNotificationUC : UserControl
    {
        public ItemNotificationUC()
        {
            this.InitializeComponent();
            //
            //if(Windows.ApplicationModel.DesignMode.DesignModeEnabled==true)
            //{
            //    img_from.ImageSource = new BitmapImage(new Uri("https://pp.userapi.com/c845520/v845520850/d6911/aTBAhpzF3eo.jpg?ava=1"));
            //}
            //
        }

        private VKBaseDataForGroupOrUser user = null;
#region Data
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(object), typeof(ItemNotificationUC), new PropertyMetadata(default(object), OnDataChanged));

        /// <summary>
        /// Данные
        /// </summary>
        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        private static void OnDataChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((ItemNotificationUC)obj).ProcessData();
        }
#endregion
        private VKNotification Notification
        {
            get { return this.Data as VKNotification;}
        }

        private void ProcessData()
        {
            this.ContentGrid.Children.Clear();
            this.ContentGrid.ColumnDefinitions.Clear();

            if (this.Data == null)
                return;

            this.icon.Glyph = this.SetIcon();

            this.GenerateLayout();

            string highlightedText = this.GetHighlightedText();

            this.date.Text = UIStringFormatterHelper.FormatDateTimeForUI(this.Notification.date);

            ScrollableTextBlock tb = new ScrollableTextBlock();
            string text = "";
            if(this.Notification.type == VKNotification.NotificationType.reply_comment)
            {
                text = string.Format("[id{0}|{1}]\n{2} {3} {4}", user.Id, user.Title, (this.Notification.ParsedFeedback as VKComment).text , this.GetLocalizableText(), highlightedText);
            }
            else if (this.Notification.type == VKNotification.NotificationType.comment_post)
            {
                text = string.Format("[id{0}|{1}] оставил комментарий:\n{2} {3} от {4}", user.Id, user.Title, (this.Notification.ParsedFeedback as VKComment).text, this.GetLocalizableText(), (this.Notification.ParsedFeedback as VKComment).date.ToString("d MMM yyyy"));
            }
            else
            {
                if (this.user != null)
                    text = string.Format("[id{0}|{1}] {2} {3}", user.Id, user.Title, this.GetLocalizableText(), highlightedText);
            }
            
            tb.Text = text;

            ContentGrid.Children.Add(tb);

            string thumb = this.GetThumb();
            if(!string.IsNullOrEmpty(thumb))
            {
                ContentGrid.ColumnDefinitions.Add(new ColumnDefinition());
                ContentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(64) });

                Image img = new Image();
                img.Source = new BitmapImage(new Uri(thumb));
                
               // img.Margin = new Thickness(0,0,15,0);
                Grid.SetColumn(img, 1);

                ContentGrid.Children.Add(img);
            }
        }

        /// <summary>
        /// Возвращаем ссылку на изображение
        /// </summary>
        /// <returns></returns>
        private string GetThumb()
        {
            if (this.Notification.ParsedParent is VKWallPost parsedParent)
            {
                if (parsedParent.attachments != null && parsedParent.attachments.Count > 0)
                {
                    if(parsedParent.attachments[0].type == VKAttachmentType.Photo)
                    {
                        return parsedParent.attachments[0].photo.photo_130;
                    }
                    else if (parsedParent.attachments[0].type == VKAttachmentType.Video)
                    {
                        return parsedParent.attachments[0].video.photo_130;
                    }
                }
            }

            if (this.Notification.ParsedParent is VKPhoto parsedParent2)
            {
                return parsedParent2.photo_130;
            }

            if (this.Notification.ParsedParent is VKVideoBase parsedParent3)
            {
                return parsedParent3.photo_130;
            }

            if (this.Notification.ParsedParent is VKComment parsedParent4)
            {
                if (parsedParent4.attachments != null && parsedParent4.attachments.Count > 0)
                {
                    if (parsedParent4.attachments[0].type == VKAttachmentType.Photo)
                    {
                        return parsedParent4.attachments[0].photo.photo_130;
                    }
                    else if (parsedParent4.attachments[0].type == VKAttachmentType.Video)
                    {
                        return parsedParent4.attachments[0].video.photo_130;
                    }
                    else if (parsedParent4.attachments[0].type == VKAttachmentType.Market)
                    {
                        return parsedParent4.attachments[0].market.thumb_photo;//.photo.photo_130;
                    }
                }

            }

            return "";
        }

        /// <summary>
        /// Задаём аватарку и запоминаем пользователя
        /// </summary>
        private void GenerateLayout()
        {
            string str = "";

            //if (this.Notification.ParsedFeedback is List<FeedbackUser> list)
            //{
            //    //todo:больше идов?

            //    Debug.Assert(list[0].from_id > 0);
            //    user = UsersService.Instance.GetCachedUser((uint)list[0].from_id);
            //    if(user == null)
            //        user = UsersService.Instance.GetCachedUser((uint)list[0].owner_id);
            //}
            //else if(this.Notification.ParsedFeedback is VKComment comment)
            //{
            //    user = UsersService.Instance.GetCachedUser((uint)comment.from_id);
            //}
            //else if(this.Notification.ParsedFeedback is VKWallPost post)
            //{
            //    user = UsersService.Instance.GetCachedUser((uint)post.from_id);
            //}
            //else if (this.Notification.ParsedFeedback is List<FeedbackCopyInfo>)
            //{
            //    int i = 0;
            //}
            user = this.Notification.Owner;
            Debug.Assert(user != null);
            if (user != null)
            {
                //todo:get user
                str = user.MinPhoto;
//                this.from.Text = user.Title;
            }
            else
            {
                int i = 0;
            }

            if (!string.IsNullOrEmpty(str))
                img_from.ImageSource = new BitmapImage(new Uri(str));

//            this.action.Text = this.GetLocalizableText();
        }

        

        private VKUserSex GetGender()
        {
            //VKBaseDataForGroupOrUser user = null;
            if (this.Notification.ParsedFeedback is VKCountedItemsObject<FeedbackUser> list)
            {
                if (list.count > 1)
                    return VKUserSex.Unknown;
//                user = (VKBaseDataForGroupOrUser)Enumerable.FirstOrDefault<User>(this._users, (Func<User, bool>)(u => u.uid == list[0].owner_id));
            }
//            else if (this.Notyfication.ParsedFeedback is VKComment)
//                user = (VKBaseDataForGroupOrUser)Enumerable.FirstOrDefault<User>(this._users, (Func<User, bool>)(u => u.uid == (this.Notyfication.ParsedFeedback as VKComment).from_id));
            else if (this.Notification.ParsedFeedback is List<FeedbackCopyInfo> info)
            {
                if (info.Count > 1)
                    return VKUserSex.Unknown;
                //user =(VKBaseDataForGroupOrUser)Enumerable.FirstOrDefault<VKBaseDataForGroupOrUser>(this._users, (Func<User, bool>)(u => u.uid == list[0].owner_id));
            }
            //else if (this.Notyfication.ParsedFeedback is MoneyTransfer)
            //{
            //    MoneyTransfer moneyTransfer = (MoneyTransfer)this.Notyfication.ParsedFeedback;
            //    user = moneyTransfer.IsOutbox ? (User)Enumerable.FirstOrDefault<User>(this._users, (Func<User, bool>)(u => u.uid == moneyTransfer.to_id)) : (User)Enumerable.FirstOrDefault<User>(this._users, (Func<User, bool>)(u => u.uid == moneyTransfer.from_id));
            //}
            //Logger.Instance.Assert(user != null, "User is null in GetGender");
            if (user == null || user is VKGroup)
                return VKUserSex.Unknown;
            return ((VKUser)user).sex;
        }
        

            /// <summary>
            /// Возвращаем текст действия на основе типа уведомления
            /// </summary>
            /// <returns></returns>
        private string GetLocalizableText()
        {
            VKUserSex gender = this.GetGender();
            string str = "";
            switch (this.Notification.type)
            {
                case  VKNotification.NotificationType.follow:
                    switch (gender)
                    {
                        case VKUserSex.Male:
                            str = "Notification_FollowMale";
                            break;
                        case VKUserSex.Female:
                            str = "Notification_FollowFemale";
                            break;
                        case VKUserSex.Unknown:
                            str = "Notification_FollowPlural";
                            break;
                    }
                    break;
                case  VKNotification.NotificationType.friend_accepted:
                    switch (gender)
                    {
                        case VKUserSex.Male:
                            str = "Notification_FriendAcceptedMale";
                            break;
                        case VKUserSex.Female:
                            str = "Notification_FriendAcceptedFemale";
                            break;
                        case VKUserSex.Unknown:
                            str = "Notification_FriendAcceptedPlural";
                            break;
                    }
                    break;
                case  VKNotification.NotificationType.mention_comments:
                    if (gender != VKUserSex.Male)
                    {
                        if (gender == VKUserSex.Female)
                        {
                            str = "Notification_MentionCommentsFemale";
                            break;
                        }
                        break;
                    }
                    str = "Notification_MentionCommentsMale";
                    break;
                case  VKNotification.NotificationType.comment_post:
                    str = "Notification_CommentPost";
                    break;
                case  VKNotification.NotificationType.comment_photo:
                    if (this.Notification.ParsedFeedback is VKComment comment)
                    {
                        if (comment.reply_to_user == Settings.UserId)
                        {
                            str = "Notification_ReplyCommentOrTopic";
                            break;
                        }
                    }

                    if (gender != VKUserSex.Male)
                    {
                        
                        if (gender == VKUserSex.Female)
                        {
                            str = "Notification_CommentPhotoFemale";
                            break;
                        }
                        break;
                    }
                    str = "Notification_CommentPhotoMale";
                    break;
                case  VKNotification.NotificationType.comment_video:
                    if (gender != VKUserSex.Male)
                    {
                        if (gender == VKUserSex.Female)
                        {
                            str = "Notification_CommentVideoFemale";
                            break;
                        }
                        break;
                    }
                    str = "Notification_CommentVideoMale";
                    break;
                case  VKNotification.NotificationType.reply_comment:
                case  VKNotification.NotificationType.reply_topic:
                case  VKNotification.NotificationType.reply_comment_photo:
                case  VKNotification.NotificationType.reply_comment_video:
                case  VKNotification.NotificationType.reply_comment_market:
                    str = "Notification_ReplyCommentOrTopic";
                    break;
                case  VKNotification.NotificationType.like_post:
                    switch (gender)
                    {
                        case VKUserSex.Male:
                            str = "Notification_LikePostMale";
                            break;
                        case VKUserSex.Female:
                            str = "Notification_LikePostFemale";
                            break;
                        case VKUserSex.Unknown:
                            str = "Notification_LikePostPlural";
                            break;
                    }
                    break;
                case  VKNotification.NotificationType.like_comment:
                case  VKNotification.NotificationType.like_comment_photo:
                case  VKNotification.NotificationType.like_comment_video:
                case  VKNotification.NotificationType.like_comment_topic:
                    switch (gender)
                    {
                        case VKUserSex.Male:
                            str = "Notification_LikeCommentMale";
                            break;
                        case VKUserSex.Female:
                            str = "Notification_LikeCommentFemale";
                            break;
                        case VKUserSex.Unknown:
                            str = "Notification_LikeCommentPlural";
                            break;
                    }
                    break;
                case  VKNotification.NotificationType.like_photo:
                    switch (gender)
                    {
                        case VKUserSex.Male:
                            str = "Notification_LikePhotoMale";
                            break;
                        case VKUserSex.Female:
                            str = "Notification_LikePhotoFemale";
                            break;
                        case VKUserSex.Unknown:
                            str = "Notification_LikePhotoPlural";
                            break;
                    }
                    break;
                case VKNotification.NotificationType.like_video:
                    switch (gender)
                    {
                        case VKUserSex.Male:
                            str = "Notification_LikeVideoMale";
                            break;
                        case VKUserSex.Female:
                            str = "Notification_LikeVideoFemale";
                            break;
                        case VKUserSex.Unknown:
                            str = "Notification_LikeVideoPlural";
                            break;
                    }
                    break;//
                case VKNotification.NotificationType.copy_post:
                    switch (gender)
                    {
                        case VKUserSex.Male:
                            str = "Notification_CopyPostMale";
                            break;
                        case VKUserSex.Female:
                            str = "Notification_CopyPostFemale";
                            break;
                        case VKUserSex.Unknown:
                            str = "Notification_CopyPostPlural";
                            break;
                    }
                    break;
                case  VKNotification.NotificationType.copy_photo:
                    switch (gender)
                    {
                        case VKUserSex.Male:
                            str = "Notification_CopyPhotoMale";
                            break;
                        case VKUserSex.Female:
                            str = "Notification_CopyPhotoFemale";
                            break;
                        case VKUserSex.Unknown:
                            str = "Notification_CopyPhotoPlural";
                            break;
                    }
                    break;
                case  VKNotification.NotificationType.copy_video:
                    switch (gender)
                    {
                        case VKUserSex.Male:
                            str = "Notification_CopyVideoMale";
                            break;
                        case VKUserSex.Female:
                            str = "Notification_CopyVideoFemale";
                            break;
                        case VKUserSex.Unknown:
                            str = "Notification_CopyVideoPlural";
                            break;
                    }
                    break;
                case  VKNotification.NotificationType.mention_comment_photo:
                    str = "Notification_MentionInPhotoComment";
                    break;
                case  VKNotification.NotificationType.mention_comment_video:
                    str = "Notification_MentionInVideoComment";
                    break;



                case VKNotification.NotificationType.wall_publish:
                    {
                        return "опубликована ваша новость";
                    }
                    /*
                case  VKNotification.NotificationType.money_transfer_received:
                    MoneyTransfer parsedFeedback1 = (MoneyTransfer)this.Notyfication.ParsedFeedback;
                    str = string.Format(gender == VKUserSex.Male ? "MoneyTransferSentMale : "MoneyTransferSentFemale, ((string)parsedFeedback1.amount.text).Replace(' ', ' '));
                    break;
                case  VKNotification.NotificationType.money_transfer_accepted:
                    MoneyTransfer parsedFeedback2 = (MoneyTransfer)this.Notyfication.ParsedFeedback;
                    str = string.Format(gender == VKUserSex.Male ? "MoneyTransferAcceptedMale : "MoneyTransferAcceptedFemale, ((string)parsedFeedback2.amount.text).Replace(' ', ' '));
                    break;
                case  VKNotification.NotificationType.money_transfer_declined:
                    MoneyTransfer parsedFeedback3 = (MoneyTransfer)this.Notyfication.ParsedFeedback;
                    str = string.Format(gender == VKUserSex.Male ? "MoneyTransferDeclinedMale : "MoneyTransferDeclinedFemale, ((string)parsedFeedback3.amount.text).Replace(' ', ' '));
                    break;*/
            }
            if (string.IsNullOrEmpty(str))
                return "";

            return LocalizedStrings.GetString(str);
        }

        private string GetHighlightedText()
        {
            VKWallPost parsedParent1 = this.Notification.ParsedParent as VKWallPost;
            if (parsedParent1 != null)
            {
                if (!string.IsNullOrEmpty(parsedParent1.text))
                    return parsedParent1.text;
                if (IsRepost(parsedParent1))
                    return parsedParent1.copy_history[0].text;
                return "";
            }
            VKComment parsedParent2 = this.Notification.ParsedParent as VKComment;
            if (parsedParent2 != null)
                return parsedParent2.text;//todo null
            VKTopic parsedParent3 = this.Notification.ParsedParent as VKTopic;
            if (parsedParent3 != null)
                return parsedParent3.title;//todo null
            return "";
        }

        private string CutText(string text)
        {
            text = ((string)text).Replace(Environment.NewLine, " ");
            text = UIStringFormatterHelper.SubstituteMentionsWithNames(text);
            if (((string)text).Length > 50)
                text = ((string)text).Substring(0, 50);
            return text;
        }

        private bool IsRepost(VKWallPost wallPost)
        {
            if (wallPost.copy_history != null)
                return wallPost.copy_history.Count > 0;
            return false;
        }

        /// <summary>
        /// Задаём иконку и цвет иконки
        /// </summary>
        /// <returns></returns>
        private string SetIcon()
        {
            switch (this.Notification.type)
            {
                case VKNotification.NotificationType.comment_post:
                case VKNotification.NotificationType.comment_photo:
                case VKNotification.NotificationType.comment_video:
                    {
                        //https://vk.com/images/svg_icons/feedback/comment.svg
                        
                        return "\xED63";
                    }
                case VKNotification.NotificationType.friend_accepted:
                    {
                        this.FeedBackIconBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 75, 179, 75));
                        return "\xE73E";
                    }
                case VKNotification.NotificationType.like_comment:
                case VKNotification.NotificationType.like_photo:
                case VKNotification.NotificationType.like_comment_photo:
                case VKNotification.NotificationType.like_comment_topic:
                case VKNotification.NotificationType.like_comment_video:
                case VKNotification.NotificationType.like_post:
                case VKNotification.NotificationType.like_video:
                    {
                        this.FeedBackIconBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 230, 70, 70));
                        return "\xEB52";
                    }
                case VKNotification.NotificationType.reply_comment:
                case VKNotification.NotificationType.reply_topic:
                case VKNotification.NotificationType.reply_comment_photo:
                case VKNotification.NotificationType.reply_comment_video:
                case VKNotification.NotificationType.reply_comment_market:
                    {
                        this.FeedBackIconBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255,75,179,75));
                        return "\xEA21";
                    }
                case VKNotification.NotificationType.follow:
                    {
                        this.FeedBackIconBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255,92,156,230));
                        return "\xE9AF";
                    }
                case VKNotification.NotificationType.wall:
                case VKNotification.NotificationType.wall_publish:
                    {
                        this.FeedBackIconBorder.Background = new SolidColorBrush(Windows.UI.Colors.Orange);
                        return "\xE874";
                    }
                case VKNotification.NotificationType.mention_comments:
                    {
                        this.FeedBackIconBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 92, 156, 230));
                        return "\xE910";
                    }
                case VKNotification.NotificationType.copy_post:
                    {
                        this.FeedBackIconBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 75, 179, 75));
                        return "\xE97A";
                    }
            }
            return "";
        }

        private void Avatar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Library.NavigatorImpl.Instance.NavigateToProfilePage(user.Id);
        }

        private void Content_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.ProcessNavigationTap(sender);
        }

        private void ProcessNavigationTap(object sender)
        {
            if (this.Notification.ParsedParent is VKWallPost || this.Notification.ParsedParent is VKComment)
            {
                VKWallPost wallPost = this.Notification.ParsedParent as VKWallPost;
                if (wallPost == null)
                {
                    VKComment parsedParent = this.Notification.ParsedParent as VKComment;
                    if (parsedParent.post != null)
                    {
                        wallPost = parsedParent.post;
                    }
                    else
                    {
                        if (parsedParent.photo != null)
                        {
                            NavigatorImpl.Instance.NavigateToPhotoWithComments(parsedParent.photo.owner_id, parsedParent.photo.id,"", parsedParent.photo);
                            return;
                        }
                        if (parsedParent.video != null)
                        {
                            NavigatorImpl.Instance.NavigateToVideoWithComments(parsedParent.video.owner_id, parsedParent.video.id, parsedParent.video.access_key, parsedParent.video);
                            return;
                        }
                        if (parsedParent.topic != null)
                        {
                            NavigatorImpl.Instance.NavigateToGroupDiscussion((uint)-parsedParent.topic.owner_id, parsedParent.topic.id, parsedParent.topic.title, parsedParent.topic.is_closed == false);
                            return;
                        }
                        //if (parsedParent.market != null)
                        //{
                        //    CurrentMarketItemSource.Source = MarketItemSource.feed;
                        //    NavigatorImpl.Instance.NavigateToProduct(parsedParent.market.owner_id, parsedParent.market.id);
                        //}
                    }
                }
                if (wallPost == null)
                    return;
                NavigatorImpl.Instance.NavigateToWallPostComments(wallPost.owner_id == 0 ? wallPost.from_id : wallPost.owner_id, wallPost.id);
            }
            else if (this.Notification.ParsedParent is VKPhoto)
            {
                VKPhoto parsedParent = this.Notification.ParsedParent as VKPhoto;
                NavigatorImpl.Instance.NavigateToPhotoWithComments(parsedParent.owner_id, parsedParent.id, "", parsedParent);
            }
            else if (this.Notification.ParsedParent is VKVideoBase)
            {
                VKVideoBase parsedParent = this.Notification.ParsedParent as VKVideoBase;
                NavigatorImpl.Instance.NavigateToVideoWithComments(parsedParent.owner_id, parsedParent.id, "", parsedParent, sender);
            }
            else if (this.Notification.ParsedParent is VKTopic)
            {
                VKTopic parsedParent = this.Notification.ParsedParent as VKTopic;
                VKComment parsedFeedback = this.Notification.ParsedFeedback as VKComment;
                uint commentId = 0;
                if (parsedFeedback != null)
                    commentId = parsedFeedback.id;
                NavigatorImpl.Instance.NavigateToGroupDiscussion((uint)-parsedParent.owner_id, parsedParent.id, parsedParent.title, parsedParent.is_closed == false, commentId);
            }
            else
            {
                //if (!(this.Notification.ParsedFeedback is MoneyTransfer))
                //    return;
                //MoneyTransfer parsedFeedback = (MoneyTransfer)this.Notification.ParsedFeedback;
                //TransferCardView.Show(parsedFeedback.id, parsedFeedback.from_id, parsedFeedback.to_id);
            }
        }
    }
}
