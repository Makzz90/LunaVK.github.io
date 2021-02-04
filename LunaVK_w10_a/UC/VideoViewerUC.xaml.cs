using LunaVK.Core.DataObjects;
using LunaVK.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using Windows.UI.Core;
using Windows.System.Display;
using System.Diagnostics;
using LunaVK.Core;
using LunaVK.Library;
using Windows.ApplicationModel.DataTransfer;
using LunaVK.Core.Library;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using Windows.UI.Xaml.Media.Animation;
using System;
using LunaVK.Common;
using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace LunaVK.UC
{
    public sealed partial class VideoViewerUC : UserControl
    {
        private DisplayRequest _displayRequest;
        PopUpService popService;
        private static PopUpService _flyout;
        private DateTime lastKeyDown = DateTime.MinValue;

        public VideoViewerUC()
        {
            this.InitializeComponent();

            this._pivot.Opacity = 0;

            this.Loaded += this.VideoViewerUC_Loaded;
            this.Unloaded += this.VideoViewerUC_Unloaded;

            this._fakeViewerElement.SizeChanged += this._fakeViewerElement_SizeChanged1;
            
            Window.Current.CoreWindow.KeyDown += this.CoreWindow_KeyDown;

            if(CustomFrame.Instance.IsDevicePhone)
                CustomFrame.Instance.OrientationChanged += this.Instance_OrientationChanged;
        }

        private void _fakeViewerElement_SizeChanged1(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width == 0)
                return;

            this._fakeViewerElement.SizeChanged -= this._fakeViewerElement_SizeChanged1;
            this.UpdateMediaSize();
        }
        
        private void VideoViewerUC_Unloaded(object sender, RoutedEventArgs e)
        {
            this._displayRequest.RequestRelease();//to release request of keep display on
            this._displayRequest = null;
            Window.Current.CoreWindow.KeyDown -= this.CoreWindow_KeyDown;

            Window.Current.SizeChanged -= Current_SizeChanged;
            CustomFrame.Instance.OrientationChanged -= this.Instance_OrientationChanged;
        }
        
        private VideoCommentsViewModel VM
        {
            get { return base.DataContext as VideoCommentsViewModel; }
        }
        
        private void VideoViewerUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateMediaSize();

            if (this.VM.Resolutions.Count == 0)
                this.VM.Load();

            this._displayRequest = new DisplayRequest();
            this._displayRequest.RequestActive();//to request keep display on
            
            this._pivot.Animate(0, 1, "Opacity", 1000);
            
            Window.Current.SizeChanged += Current_SizeChanged;
        }
        
        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == Windows.System.VirtualKey.Space)
            {
                //System.Diagnostics.Debug.WriteLine("VirtualKey.Space");

                if ((DateTime.Now - this.lastKeyDown).TotalMilliseconds < 500)
                    return;

                //System.Diagnostics.Debug.WriteLine("VideoViewerElement.PlayPause()");
                CustomFrame.Instance.VideoViewerElement.PlayPause();
                args.Handled = true;

                this.lastKeyDown = DateTime.Now;
            }
        }
        /*
        private readonly double _videoElementWidth = 200;
        
        private void Closed()
        {
            Size childSize = new Size(CustomFrame.Instance.VideoViewerElement.ActualWidth, CustomFrame.Instance.VideoViewerElement.ActualHeight);

            double w = this._videoElementWidth;//будущая ширина проигрывателя
            double h = w * 0.56;//будущая высота проигрывателя для пропорции 16:9

            Rect target = new Rect(CustomFrame.Instance.ActualWidth - w - 10, CustomFrame.Instance.ActualHeight - h - 10, w, h);
            CompositeTransform compositeTransform1 = RectangleUtils.TransformRect(new Rect(new Point(), childSize), target, false);//позиционирует и вычисляет масштаб
            //CustomFrame.Instance.VideoViewerElement.RenderTransform = tr;

            CompositeTransform renderTransform = CustomFrame.Instance.VideoViewerElement.RenderTransform as CompositeTransform;
            Debug.Assert(renderTransform != null);
            if (renderTransform != null)
            {
                EasingFunctionBase ease = new QuarticEase() { EasingMode = EasingMode.EaseIn };
                renderTransform.Animate(renderTransform.TranslateX, renderTransform.TranslateX + compositeTransform1.TranslateX, "TranslateX", 600, 0, ease);
                renderTransform.Animate(renderTransform.TranslateY, renderTransform.TranslateY + compositeTransform1.TranslateY, "TranslateY", 600, 0, ease);
                renderTransform.Animate(renderTransform.ScaleX, compositeTransform1.ScaleX, "ScaleX", 600, 0, ease, null);
                renderTransform.Animate(renderTransform.ScaleY, compositeTransform1.ScaleY, "ScaleY", 600, 0, ease, null);
            }
            CustomFrame.Instance.VideoViewerElement.MakeCompact();
        }
        
        public void OnBackRequested(object sender, BackRequestedEventArgs args)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= this.OnBackRequested;
//            this.Closed();
        }
        */
        private void Options_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.VM == null || this.VM.Video == null)
                return;

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


            MenuFlyoutItem subitem = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonSpam"), CommandParameter = ReportReason.Spam };
            subitem.Command = new DelegateCommand((args) => { this.ReportPost(args); });
            item2.Items.Add(subitem);
            MenuFlyoutItem subitem2 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonChildPorn"), CommandParameter = ReportReason.ChildPorn };
            subitem2.Command = new DelegateCommand((args) => { this.ReportPost(args); });
            item2.Items.Add(subitem2);
            MenuFlyoutItem subitem3 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonExtremism"), CommandParameter = ReportReason.Extremism };
            subitem3.Command = new DelegateCommand((args) => { this.ReportPost(args); });
            item2.Items.Add(subitem3);
            MenuFlyoutItem subitem4 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonViolence"), CommandParameter = ReportReason.Violence };
            subitem4.Command = new DelegateCommand((args) => { this.ReportPost(args); });
            item2.Items.Add(subitem4);
            MenuFlyoutItem subitem5 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonDrug"), CommandParameter = ReportReason.Drugs };
            subitem5.Command = new DelegateCommand((args) => { this.ReportPost(args); });
            item2.Items.Add(subitem5);
            MenuFlyoutItem subitem6 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonAdult"), CommandParameter = ReportReason.Adult };
            subitem6.Command = new DelegateCommand((args) => { this.ReportPost(args); });
            item2.Items.Add(subitem6);
            MenuFlyoutItem subitem7 = new MenuFlyoutItem() { Text = LocalizedStrings.GetString("ReportReasonInsult"), CommandParameter = ReportReason.Abuse };
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
                menuItem1.Click += this.mItemDelete_Click;
                menuItems.Items.Add(menuItem1);
            }

            MenuFlyoutItem _appBarMenuItemFaveUnfave = new MenuFlyoutItem();
            _appBarMenuItemFaveUnfave.Text = LocalizedStrings.GetString(this.VM.Video.is_favorite ? "RemoveFromBookmarks" : "AddToBookmarks");
            _appBarMenuItemFaveUnfave.Command = new DelegateCommand((args) =>
            {
                FavoritesService.Instance.FaveAddRemoveVideo(this.VM.Video.owner_id, this.VM.Video.id, this.VM.Video.access_key, !this.VM.Video.is_favorite, (result) =>
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        if (result.error.error_code == VKErrors.None && result.response == 1)
                            this.VM.Video.is_favorite = !this.VM.Video.is_favorite;
                    });
                });
            });

            menuItems.Items.Add(_appBarMenuItemFaveUnfave);
            
            if(this.VM.Resolutions.Count>0)
            {
                menuItems.Items.Add(new MenuFlyoutSeparator());

                MenuFlyoutSubItem itemDownload = new MenuFlyoutSubItem();
                itemDownload.Text = LocalizedStrings.GetString("Download") + "...";

                foreach(var res in this.VM.Resolutions)
                {
                    MenuFlyoutItem subDownload = new MenuFlyoutItem() { Text = (res.Resolution+"p"), CommandParameter = res.Url };
                    subDownload.Command = new DelegateCommand((args) => { this.Download(args); });
                    itemDownload.Items.Add(subDownload);
                }

                menuItems.Items.Add(itemDownload);
            }
            



            menuItems.ShowAt(sender as FrameworkElement);
        }

        private void mItemEdit_Click(object sender, RoutedEventArgs e)
        {
            //Navigator.Current.NavigateToEditVideo(this.VM.OwnerId, this.VM.VideoId, this.VM.Video);
        }

        private void _appBarButtonShare_Click(object sender, RoutedEventArgs e)
        {
            SharePostUC share = new SharePostUC("видеозаписью", WallService.RepostObject.video, this.VM.Video.owner_id, this.VM.Video.id, this.VM.Video.access_key);
            PopUpService popUp = new PopUpService { Child = share, OverrideBackKey = true, AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed };
            share.Done = popUp.Hide;
            popUp.Show();
        }
        
        private void mItemAddToMyVideos_Click(object sender, RoutedEventArgs e)
        {
            this.VM.AddRemoveToMyVideos();
        }

        private void mItemAddToAlbum_Click(object sender, RoutedEventArgs e)
        {
            //Navigator.Current.NavigateToAddVideoToAlbum(this._ownerId, this._videoId);
            var popUC = new UC.PopUp.AddToAlbumUC(this.VM.OwnerId, this.VM.VideoId);
            popService = new PopUpService { Child = popUC };

            popService.OverrideBackKey = true;
            popService.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            popService.Show();

            popUC.Done = popService.Hide;
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

        private void Download(object args)
        {
            string url = (string)args;
            BatchDownloadManager.Instance.DownloadByIndex(url, "(" + this.VM.Video.ToString() + ") " + this.VM.VideoTitle);
        }

        private void ReportPost(object args)
        {
            var video = this.VM.Video;
            VideoService.Instance.Report(video.owner_id, video.id, (ReportReason)args, null);
        }

        private void Owner_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            NavigatorImpl.Instance.NavigateToProfilePage(this.VM.OwnerId);
        }

        private void Like_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this.VM.AddRemoveLike();
        }

        private void Publish_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SharePostUC share = new SharePostUC("видеозаписью", WallService.RepostObject.video, this.VM.OwnerId, this.VM.VideoId, this.VM.Video.access_key);
            PopUpService popUp = new PopUpService { Child = share, OverrideBackKey = true, AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed };
            share.Done = popUp.Hide;
            popUp.Show();

            e.Handled = true;
        }

        private void _likes_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToLikesPage(this.VM.OwnerId, this.VM.VideoId, LikeObjectType.video, (int)this.VM.Video.likes.count);
        }

        public static void Show()
        {
            VideoCommentsViewModel vm = CustomFrame.Instance.VideoViewerElement.DataContext as VideoCommentsViewModel;

            VideoViewerUC uc = new VideoViewerUC() { DataContext = vm };
            var dialogService = new PopUpService();
            dialogService.Child = uc;
            dialogService.OverrideBackKey = true;
            dialogService.AnimationTypeChild = PopUpService.AnimationTypes.Fade;
            //dialogService.Closed += DialogService_Closed;
            //dialogService.BackgroundBrush = null;
            
            dialogService.OnClosingAction = (callback =>
            {
                //uc.AnimateOut(callback);
                CustomFrame.Instance.VideoViewerElement.MakeCompact();
                callback.Invoke();
            });
            

            CustomFrame.Instance.VideoViewerElement.DockedAction = dialogService.Hide;//uc.ViewerElementUC.DockedAction = dialogService.Hide;
            CustomFrame.Instance.VideoViewerElement.MakeNormal();

            VideoViewerUC._flyout = dialogService;
            VideoViewerUC._flyout.Show();
        }

        public static void Show(int ownerId, uint videoId, string accessKey = "", VKVideoBase video = null, object sender = null)
        {
            if(CustomFrame.Instance.VideoViewerElement.DataContext != null)
            {
                VideoCommentsViewModel temp_vm = CustomFrame.Instance.VideoViewerElement.DataContext as VideoCommentsViewModel;
                if(temp_vm!=null)
                {
                    if(temp_vm.OwnerId == ownerId && temp_vm.VideoId == videoId)
                    {
                        VideoViewerUC.Show();
                        
                        return;
                    }
                }
            }

            VideoCommentsViewModel vm = new VideoCommentsViewModel(ownerId, videoId, accessKey);

            VideoViewerUC uc = new VideoViewerUC() { DataContext = vm };
            var dialogService = new PopUpService();
            dialogService.Child = uc;
            dialogService.OverrideBackKey = true;
            dialogService.AnimationTypeChild = PopUpService.AnimationTypes.None;
            
            dialogService.OnClosingAction = (callback =>
            {
                //uc.AnimateOut(callback);
                CustomFrame.Instance.VideoViewerElement.MakeCompact();
                callback.Invoke();
            });
            
            
            CustomFrame.Instance.VideoViewerElement.InitViewModel(vm);
            CustomFrame.Instance.VideoViewerElement.MakeNormal();
            uc.AnimateIn(sender);

            CustomFrame.Instance.VideoViewerElement.DockedAction = dialogService.Hide;


            VideoViewerUC._flyout = dialogService;
            VideoViewerUC._flyout.Show();
        }

        private static void DialogService_Closed(object sender, EventArgs e)
        {
            CustomFrame.Instance.VideoViewerElement.MakeCompact();
        }
        
        public void AnimateIn(object sender)
        {
            this._pivot.Animate(0, 1, "Opacity", 500);

            if (sender != null)
            {
                Common.ImageAnimator imageAnimator = new Common.ImageAnimator(200, null);
                imageAnimator.AnimateIn(sender as FrameworkElement, CustomFrame.Instance.VideoViewerElement);
            }
        }

        public void AnimateOut(Action callback)
        {
            this._pivot.Animate(1, 0, "Opacity", 300, 0, null, callback);
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            this.UpdateMediaSize();
        }

        private void _fakeViewerElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateMediaSize();
        }

        private void UpdateMediaSize()
        {
            double w = this._fakeViewerElement.ActualWidth;
            double h = this._fakeViewerElement.ActualHeight;
            if (w == 0)
            {
                if (this._rootGrid.ActualWidth == this._pivot.ActualWidth)
                    w = this._pivot.ActualWidth;
                else
                    w = this._rootGrid.ActualWidth - this._pivot.ActualWidth;
                
                h = w * 0.56;
            }
            else
            {
                var ttv = this._fakeViewerElement.TransformToVisual(Window.Current.Content);
                Point screenCoords = ttv.TransformPoint(new Point(0, 0));
                (CustomFrame.Instance.VideoViewerElement.RenderTransform as CompositeTransform).TranslateX = screenCoords.X;
            }

            CustomFrame.Instance.VideoViewerElement.Width = w;
            CustomFrame.Instance.VideoViewerElement.Height = h;
        }

        private void Instance_OrientationChanged(object sender, ApplicationViewOrientation e)
        {
            if(e == ApplicationViewOrientation.Landscape)
            {
                if (!CustomFrame.Instance.VideoViewerElement.ME.IsFullWindow)
                    CustomFrame.Instance.VideoViewerElement.ME.IsFullWindow = true;
            }
            else
            {
                if (CustomFrame.Instance.VideoViewerElement.ME.IsFullWindow)
                    CustomFrame.Instance.VideoViewerElement.ME.IsFullWindow = false;
            }
        }
    }
}
