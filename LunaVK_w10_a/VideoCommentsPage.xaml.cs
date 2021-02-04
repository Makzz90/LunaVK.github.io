using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using LunaVK.Core.DataObjects;
using Windows.UI.Xaml.Media.Imaging;
using LunaVK.Network;
using LunaVK.Core;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.ViewModels;
using YoutubeExtractor;
using LunaVK.Library;
using LunaVK.Core.Enums;
using LunaVK.Framework;
using Windows.ApplicationModel.DataTransfer;

namespace LunaVK
{
    public sealed partial class VideoCommentsPage : PageBase
    {
        WebView web = null;

        private VideoCommentsViewModel VM
        {
            get { return base.DataContext as VideoCommentsViewModel; }
        }

        public VideoCommentsPage()
        {
            this.InitializeComponent();
            //this.Resolutions = new ObservableCollection<Resolution>();
            //this._resolutions.ItemsSource = this.Resolutions;

            this.Loaded += (s, e) =>
            {
                CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE712", Clicked = this.ShowContextMenu });
            };
            base.Title = LocalizedStrings.GetString("Video_Title");
        }

        private void ShowContextMenu()
        {
            MenuFlyout menuItems = new MenuFlyout();
            if (this.VM.CanEdit)
            {
                MenuFlyoutItem menuItem1 = new MenuFlyoutItem();
                menuItem1.Text = LocalizedStrings.GetString("Edit");
                menuItem1.Click += new RoutedEventHandler(this.mItemEdit_Click);
                menuItems.Items.Add(menuItem1);
            }
            if (this.VM.CanAddToMyVideos)
            {
                MenuFlyoutItem menuItem1 = new MenuFlyoutItem();
                menuItem1.Text = LocalizedStrings.GetString("VideoNew_AddToMyVideos");
                menuItem1.Click += new RoutedEventHandler(this.mItemAddToMyVideos_Click);
                menuItems.Items.Add(menuItem1);
            }

            MenuFlyoutItem menuItem3 = new MenuFlyoutItem();
            menuItem3.Text = LocalizedStrings.GetString("VideoNew_AddToAlbum");
            menuItem3.Click += new RoutedEventHandler(this.mItemAddToAlbum_Click);
            menuItems.Items.Add(menuItem3);

            MenuFlyoutItem menuItem5 = new MenuFlyoutItem();
            menuItem5.Text = LocalizedStrings.GetString("OpenInBrowser");
            menuItem5.Click += new RoutedEventHandler(this.mItemOpenInBrowser_Click);
            menuItems.Items.Add(menuItem5);

            MenuFlyoutItem menuItem7 = new MenuFlyoutItem();
            menuItem7.Text = LocalizedStrings.GetString("CopyLink");
            menuItem7.Click += new RoutedEventHandler(this.mItemCopyLink_Click);
            menuItems.Items.Add(menuItem7);




            MenuFlyoutSubItem item2 = new MenuFlyoutSubItem();
            item2.Text = LocalizedStrings.GetString("Report") + "...";


            MenuFlyoutItem subitem = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonSpam"), CommandParameter = "0" };//1
            subitem.Command = new DelegateCommand((args) => { this.ReportPost(args); });
            item2.Items.Add(subitem);
            MenuFlyoutItem subitem2 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonChildPorn"), CommandParameter = "1" };//5
            subitem2.Command = new DelegateCommand((args) => { this.ReportPost(args); });
            item2.Items.Add(subitem2);
            MenuFlyoutItem subitem3 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonExtremism"), CommandParameter = "2" };//6
            subitem3.Command = new DelegateCommand((args) => { this.ReportPost(args); });
            item2.Items.Add(subitem3);
            MenuFlyoutItem subitem4 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonViolence"), CommandParameter = "3" };//7
            subitem4.Command = new DelegateCommand((args) => { this.ReportPost(args); });
            item2.Items.Add(subitem4);
            MenuFlyoutItem subitem5 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonDrug"), CommandParameter = "4" };//4
            subitem5.Command = new DelegateCommand((args) => { this.ReportPost(args); });
            item2.Items.Add(subitem5);
            MenuFlyoutItem subitem6 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonAdult"), CommandParameter = "5" };//3
            subitem6.Command = new DelegateCommand((args) => { this.ReportPost(args); });
            item2.Items.Add(subitem6);
            MenuFlyoutItem subitem7 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonInsult"), CommandParameter = "6" };//2
            subitem7.Command = new DelegateCommand((args) => { this.ReportPost(args); });
            item2.Items.Add(subitem7);

            menuItems.Items.Add(item2);



            if (this.VM.CanRemoveFromMyVideos)
            {
                MenuFlyoutItem menuItem1 = new MenuFlyoutItem();
                menuItem1.Text = LocalizedStrings.GetString("VideoNew_RemovedFromMyVideos");
                menuItem1.Click += new RoutedEventHandler(this.mItemAddToMyVideos_Click);
                menuItems.Items.Add(menuItem1);
            }
            if (this.VM.CanDelete)
            {
                MenuFlyoutItem menuItem1 = new MenuFlyoutItem();
                menuItem1.Text = LocalizedStrings.GetString("Delete");
                menuItem1.Click += new RoutedEventHandler(this.mItemDelete_Click);
                menuItems.Items.Add(menuItem1);
            }

            menuItems.ShowAt(this.MainScroll);
        }

        private void ReportPost(object args)
        {
            var video = this.VM.Video;
            //VideoService.Instance.Report(video.owner_id, video.id,)
            
        }

        private void mItemEdit_Click(object sender, RoutedEventArgs e)
        {
            //Navigator.Current.NavigateToEditVideo(this.VM.OwnerId, this.VM.VideoId, this.VM.Video);
        }

        private void mItemAddToMyVideos_Click(object sender, RoutedEventArgs e)
        {
            this.VM.AddRemoveToMyVideos();
        }

        private void mItemAddToAlbum_Click(object sender, RoutedEventArgs e)
        {
            //Navigator.Current.NavigateToAddVideoToAlbum(this._ownerId, this._videoId);
        }

        private void mItemOpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToWebUri(this.VM.VideoUri, true);
        }

        private void mItemCopyLink_Click(object sender, RoutedEventArgs e)
        {
            //Clipboard.SetText(this.VM.VideoUri);

            var dataPackage = new DataPackage();
            dataPackage.SetText(this.VM.VideoUri);
            Clipboard.SetContent(dataPackage);
        }

        private void mItemDelete_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (MessageBox.Show(CommonResources.DeleteConfirmation, VideoResources.DeleteVideo, (MessageBoxButton)1) != MessageBoxResult.OK)
                return;
            this.VM.Delete((Action<ResultCode>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (res == ResultCode.Succeeded)
                    Navigator.Current.GoBack();
                else
                    GenericInfoUC.ShowBasedOnResult((int)res, "", (VKRequestsDispatcher.Error)null);
            }))));*/
        }


        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            IDictionary<string, object> QueryString = e.Parameter as IDictionary<string, object>;

            int ownerId = (int)QueryString["OwnerId"];
            uint videoId = (uint)QueryString["VideoId"];
            string accessKey = "";
            VKVideoBase video = null;

            if (QueryString.Keys.Contains("AccessKey"))
                accessKey = (string)QueryString["AccessKey"];

            if (QueryString.Keys.Contains("VideoContext"))
            {
                video = (VKVideoBase)QueryString["VideoContext"];

                this.VideoTitle.Text = video.title;
                this.VideoDescription.Text = video.description;


                string str = video.photo_320;
                if (string.IsNullOrEmpty(str))
                    str = video.photo_640;
                if (string.IsNullOrEmpty(str))
                    str = video.photo_800;

                this._preview.Source = new BitmapImage(new Uri(str));
            }

            base.DataContext = new VideoCommentsViewModel(ownerId, videoId, accessKey, video);
            this.VM.LoadingStatusUpdated+= this.HandleLoadingStatusUpdated;
        }

        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {
            if(status== ProfileLoadingStatus.Loaded)
            {
                if(this.VM.Resolutions.Count==0)
                {
                    this.web = new WebView();
                    this.ContentGrid.Children.Add(this.web);


//                    if(this.VM.Video.player.Contains("vk.com/video_ext.php"))
//                    {
//                        this.web.NavigateToString("<html><body><iframe src=\"" + this.VM.Video.player + "\" width=\"607\" height=\"360\" frameborder=\"0\"></iframe></body></html>");
//                    }
//                    else
//                    {
                        this.web.Navigate(new Uri(this.VM.Video.player));
//                    }
                    
                }
            }
        }

        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (this.web != null)
            {
                this.web.NavigateToString("");
                this.ContentGrid.Children.Remove(this.web);
            }
        }

        private void Resolutions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            var index = cb.SelectedIndex;
            var videoInfo = this.VM.Infos[index];
            
            this.me.Source = new Uri(videoInfo.DownloadUrl);
            me.AreTransportControlsEnabled = true;
        }

        private void Grid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToProfilePage(this.VM.OwnerId);
        }
    }
}
