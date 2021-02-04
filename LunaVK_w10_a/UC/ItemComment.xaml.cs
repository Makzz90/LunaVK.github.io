using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using LunaVK.Core.Network;
using LunaVK.ViewModels;
using LunaVK.Core.Enums;
using LunaVK.Library;
using LunaVK.Core.Library;
using LunaVK.Core.Framework;
using LunaVK.Core;

namespace LunaVK.UC
{
    //RepostHeaderUC
    //CommentItem
    public sealed partial class ItemComment : UserControl
    {
        public ItemComment()
        {
            this.InitializeComponent();
            this.Loaded += ItemComment_Loaded;
        }

        private void ItemComment_Loaded(object sender, RoutedEventArgs e)
        {
            if(this.IsIncluded)
            {
                //Margin="0 10 10 10"
                this._stackPanel.Margin = new Thickness(0,10,0,10);

                //Width="30" Height="30" Margin="10"
                this._ellipse.Margin = new Thickness(0,10,10,10);
                this._ellipse.Width = this._ellipse.Height = 20;
            }
            else
            {
                this._stackPanel.Margin = new Thickness(0, 10, 10, 10);

                this._ellipse.Margin = new Thickness(10);
                this._ellipse.Width = this._ellipse.Height = 35;
            }
        }

        private VKComment VM
        {
            get { return base.DataContext as VKComment; }
        }

        public static readonly DependencyProperty IsIncludedProperty = DependencyProperty.Register("IsIncluded", typeof(bool), typeof(ItemComment), new PropertyMetadata(false));

        public bool IsIncluded
        {
            get { return (bool)GetValue(IsIncludedProperty); }
            set { SetValue(IsIncludedProperty, value); }
        }
        /*
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(object), typeof(ItemComment), new PropertyMetadata(default(object), OnDataChanged));

        /// <summary>
        /// Данные.
        /// </summary>
        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        private static void OnDataChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((ItemComment)obj).ProcessData();
        }
        
        private VKComment DataVM
        {
            get { return this.Data as VKComment; }
        }
        
        private void ProcessData()
        {
            this.MainContent.Children.Clear();
            //
            if (this.Data == null)
                return;

            this.optionsBorder.Visibility = Visibility.Visible;
            this.reportBorder.Visibility = Visibility.Collapsed;
            this.shareBorder.Visibility = Visibility.Collapsed;
            this.deleteBorder.Visibility = Visibility.Collapsed;

            this._state.IsActive = this.DataVM.Marked;
            //

            bool isMale;
            string pic;
            string name;
            this.GetNamePicAndSex(this.DataVM.from_id, out name, out pic, out isMale);

            if (!string.IsNullOrEmpty(this.DataVM.text))
            {
                ScrollableTextBlock t = new ScrollableTextBlock();
                t.FullOnly = true;
                //t.Foreground = (SolidColorBrush)Application.Current.Resources["TextBrushMediumHigh"];
                t.Text = this.DataVM.text;
                t.FontSize = (double)Application.Current.Resources["FontSizeContent"];

                this.MainContent.Children.Add(t);
            }

//#if !DEBUG
            if (this.DataVM.attachments != null)
            {
                AttachmentPresenter ap = new AttachmentPresenter(this.DataVM.attachments,this.MainContent);//20-margin from StackPanel
                this.MainContent.Children.Add(ap);
            }
//#endif
            this.textBlockDate.Text = UIStringFormatterHelper.FormatDateTimeForUI(this.DataVM.date);

            if (this.DataVM.likes.count > 0)
                this._textblockLikes.Text = this.DataVM.likes.count.ToString();
            else
                this._textblockLikes.Visibility = Visibility.Collapsed;
            //
            this._id.Text = this.DataVM.id.ToString();
            //

            //if (this.DataVM.reply_to_user == 0)
            //    this.textBlockUserOrGroupName.Text = this.DataVM.User.Title;
            //else
            //    this.textBlockUserOrGroupName.Text = this.DataVM.User.Title + " ответил " + this.DataVM._replyToUserDat;
            //

            string str = this.CreateUserNameText(this.DataVM.User,false);
            
            if (this.DataVM.reply_to_user != 0)
            {
                str += " ответил " + this.DataVM._replyToUserDat;
            }

            this.textBlockUserOrGroupName.Text = str;
            //

            this.imageUserOrGroup.ImageSource = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(this.DataVM.User.MinPhoto));//Source

            
        }
        */
        private string CreateUserNameText(VKBaseDataForGroupOrUser user, bool isDat)
        {
            if (user.Id > 0)
            {
                string str = isDat ? (user as VKUser).NameDat : user.Title;
                return string.Format("[id{0}|{1}]", user.Id, str);
            }
            return string.Format("[club{0}|{1}]", -user.Id, user.Title);
        }

        private void GetNamePicAndSex(long userOrGroupId, out string name, out string pic, out bool isMale)
        {
            name = "";
            pic = "";
            isMale = false;/*
            if (userOrGroupId > 0L)
            {
                GroupOrUser user = Enumerable.FirstOrDefault<GroupOrUser>(this._users, (Func<GroupOrUser, bool>)(p => p.id == userOrGroupId));//uid
                if (user == null)
                    return;
                name = user.Name;
                pic = user.photo_max;
                isMale = user.sex != 1;
            }
            else
            {
                GroupOrUser group = Enumerable.FirstOrDefault<GroupOrUser>(this._groups, (Func<GroupOrUser, bool>)(g => g.id == -userOrGroupId));
                if (group == null || group.name == null)
                    return;
                name = group.name;
                pic = group.photo_200;
                isMale = true;
            }*/
        }

        private void Share_Tapped()
        {
            SharePostUC share = new SharePostUC("комментарием", WallService.RepostObject.wall, this.VM.owner_id, this.VM.id);
            PopUpService popUp = new PopUpService { Child = share, OverrideBackKey = true, AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed };
            share.Done = popUp.Hide;
            popUp.Show();
        }
        
        private void Options_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MenuFlyout menu = new MenuFlyout();

            MenuFlyoutItem item = new MenuFlyoutItem() { Text = "Поделится" };
            item.Command = new DelegateCommand((args) =>
            {
                this.Share_Tapped();
            });
            menu.Items.Add(item);

            if(this.VM.CanEdit)
            {
                MenuFlyoutItem item4 = new MenuFlyoutItem() { Text = "Редактировать" };
                item4.Command = new DelegateCommand((args) =>
                {
                    this._appBarMenuItemEdit_Click();
                });
                menu.Items.Add(item4);
            }

            if (this.VM.CanDelete)
            {
                MenuFlyoutItem item3 = new MenuFlyoutItem() { Text = "Удалить" };
                item3.Command = new DelegateCommand((args) =>
                {
                    this.Delete_Tapped();//_appBarMenuItemDelete
                });
                menu.Items.Add(item3);
            }

            MenuFlyoutSubItem item2 = new MenuFlyoutSubItem() { Text = LocalizedStrings.GetString("Report") };
            MenuFlyoutItem subitem = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonSpam"), CommandParameter = ReportReason.Spam };
            subitem.Command = new DelegateCommand((args) => { this.ReportPhoto(args); });
            item2.Items.Add(subitem);
            MenuFlyoutItem subitem2 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonChildPorn"), CommandParameter = ReportReason.ChildPorn };
            subitem2.Command = new DelegateCommand((args) => { this.ReportPhoto(args); });
            item2.Items.Add(subitem2);
            MenuFlyoutItem subitem3 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonExtremism"), CommandParameter = ReportReason.Extremism };
            subitem3.Command = new DelegateCommand((args) => { this.ReportPhoto(args); });
            item2.Items.Add(subitem3);
            MenuFlyoutItem subitem4 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonViolence"), CommandParameter = ReportReason.Violence };
            subitem4.Command = new DelegateCommand((args) => { this.ReportPhoto(args); });
            item2.Items.Add(subitem4);
            MenuFlyoutItem subitem5 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonDrug"), CommandParameter = ReportReason.Drugs };
            subitem5.Command = new DelegateCommand((args) => { this.ReportPhoto(args); });
            item2.Items.Add(subitem5);
            MenuFlyoutItem subitem6 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonAdult"), CommandParameter = ReportReason.Adult };
            subitem6.Command = new DelegateCommand((args) => { this.ReportPhoto(args); });
            item2.Items.Add(subitem6);
            MenuFlyoutItem subitem7 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonInsult"), CommandParameter = ReportReason.Abuse };
            subitem7.Command = new DelegateCommand((args) => { this.ReportPhoto(args); });
            item2.Items.Add(subitem7);
            menu.Items.Add(item2);


            if (menu.Items.Count > 0)
                menu.ShowAt(sender as FrameworkElement);
        }

        private void ReportPhoto(object args)
        {
            WallService.Instance.Report(this.VM.owner_id, this.VM.id, (ReportReason)args, null);
        }

        private void Like_Tapped(object sender, TappedRoutedEventArgs e)
        {
            (sender as FrameworkElement).IsHitTestVisible = false;

            if (this.VM.likes == null)
                this.VM.likes = new VKLikes();
            //todo:LikeObjectType = photo_comment
            LikesService.Instance.AddRemoveLike(this.VM.likes.user_likes == false, this.VM.owner_id == 0 ? this.VM.from_id : this.VM.owner_id, this.VM.id, LikeObjectType.comment, (result) => {
                Execute.ExecuteOnUIThread(() => {
                    (sender as FrameworkElement).IsHitTestVisible = true;
                    if(result!=-1)
                    {
                        this.VM.likes.count = (uint)result;
                        this.VM.likes.user_likes = !this.VM.likes.user_likes;
                        this.VM.RefreshUI();
                    }
                });
            });
        }
        

        

        private void Reply_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VKComment comment = (sender as FrameworkElement).DataContext as VKComment;
            comment.ReplyTapped?.Invoke();
        }

        private void _appBarMenuItemEdit_Click()
        {
            /*
            if (this.PostCommentsVM.WallPostData == null || this.PostCommentsVM.WallPostData.WallPost == null)
                return;
            this.PostCommentsVM.WallPostData.WallPost.NavigateToEditWallPost(this.PostCommentsVM.WallPostItem == null ? 3 : this.PostCommentsVM.WallPostItem.AdminLevel);


            
             public static void NavigateToEditWallPost(this WallPost wallPost, int adminLevel)
        {
            if (wallPost == null)
                return;
            ParametersRepository.SetParameterForId("EditWallPost", wallPost);
            Navigator.Current.NavigateToNewWallPost(Math.Abs(wallPost.to_id), wallPost.to_id < 0, adminLevel, false, false, false);
        }
        
        */
           // NavigatorImpl.Instance.NavigateToNewWallPost(WallPostViewModel.Mode.EditWallComment,this.VM.owner_id,);
        }

        private void Delete_Tapped()
        {
            VKComment comment = this.VM;
            comment.DeleteTapped?.Invoke();
        }

        private void LoadComments_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VKComment comment = (sender as FrameworkElement).DataContext as VKComment;
            comment.LoadMoreInThread();
        }

        private void Avatar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VKComment comment = (sender as FrameworkElement).DataContext as VKComment;
            if(comment.from_id!=0)
                NavigatorImpl.Instance.NavigateToProfilePage(comment.from_id);
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width<300)
            {
                this._btnReply.Visibility = Visibility.Visible;
                this._textReply.Visibility = Visibility.Collapsed;
            }
            else
            {
                this._btnReply.Visibility = Visibility.Collapsed;
                this._textReply.Visibility = Visibility.Visible;
            }
        }
    }
}
