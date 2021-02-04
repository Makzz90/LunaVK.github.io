using LunaVK.Core;
using LunaVK.Core.Enums;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.Web.Http;
using Windows.UI.Xaml.Input;
using Windows.UI.ViewManagement;

namespace LunaVK.UC
{
    public sealed partial class VideoViewerElementUC : UserControl
    {
        private TimeSpan? _newPos;
        private Flyout _settingFlyout;
        private bool _volumeOpened;
//        private AppBarButton _skipForwardButton;
//        private AppBarButton _skipBackwardButton;


        public Action DockedAction;
        private bool IsCompact;

        public VideoViewerElementUC()
        {
            this.InitializeComponent();

            this.Loaded += VideoViewerElementUC_Loaded;
            this.Unloaded += VideoViewerElementUC_Unloaded;

            VisualStateManager.GoToState(this._mediaElement.TransportControls, "Loading", true);

            this._mediaElement.MediaOpened += _mediaElement_MediaOpened;

            Window.Current.VisibilityChanged += this.Current_VisibilityChanged;



            base.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            base.ManipulationDelta += DragItem_ManipulationDelta;

            Window.Current.CoreWindow.KeyDown += this.CoreWindow_KeyDown;
        }
        
        private void Current_VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            if(!e.Visible)
            {
                if(this.VM!=null && this._mediaElement.Source!=null)
                {
                    this._mediaElement.Pause();
                }
            }
        }

        private void VideoViewerElementUC_Unloaded(object sender, RoutedEventArgs e)
        {
            this.VM.LoadingStatusUpdated = null;
            this.SizeChanged -= this.P_SizeChanged;

            Window.Current.CoreWindow.KeyDown -= this.CoreWindow_KeyDown;
            Window.Current.VisibilityChanged -= this.Current_VisibilityChanged;

            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == Windows.System.VirtualKey.Escape && this._mediaElement.IsFullWindow)
            {
                this._mediaElement.IsFullWindow = false;
                //this._mediaElement.PlaybackRate//The playback rate ratio for the media. A value of 1.0 is the normal playback speed. Value can be negative to play backwards.
                args.Handled = true;
            }
        }

        private void _mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if(this._newPos.HasValue)
            {
                Debug.Assert((sender as MediaElement).CanSeek==true);
                (sender as MediaElement).Position = this._newPos.Value;
                this._newPos = null;
            }
        }
        
        private void P_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RectangleGeometry rectangleGeometry = new RectangleGeometry();
            rectangleGeometry.Rect = new Rect(0.0, 0.0, e.NewSize.Width, e.NewSize.Height);
            this.Clip = rectangleGeometry;//BugFix
        }
        
        private VideoCommentsViewModel VM
        {
            get { return base.DataContext as VideoCommentsViewModel; }
        }

        public void MakeCompact()
        {
            if (this.IsCompact)
                return;

            VisualStateManager.GoToState(this._mediaElement.TransportControls, "CompactMode", true);
            this._mediaElement.IsHitTestVisible = false;
            this._brdClose.Visibility = Visibility.Visible;
            this.IsCompact = true;









            Size childSize = new Size(this.ActualWidth, this.ActualHeight);

            double ratio = this.ActualHeight / this.ActualWidth;

            double w = 200;//будущая ширина проигрывателя
            double h = w * ratio;//будущая высота проигрывателя для пропорции

            Rect target = new Rect(CustomFrame.Instance.ActualWidth - w - 10, CustomFrame.Instance.ActualHeight - h - 10, w, h);
            CompositeTransform compositeTransform1 = RectangleUtils.TransformRect(new Rect(new Point(), childSize), target, false);//позиционирует и вычисляет масштаб
            //CustomFrame.Instance.VideoViewerElement.RenderTransform = tr;

            CompositeTransform renderTransform = this.RenderTransform as CompositeTransform;
            //Debug.Assert(renderTransform != null);
            if (renderTransform != null)
            {
                renderTransform.Animate(renderTransform.TranslateX, renderTransform.TranslateX + compositeTransform1.TranslateX, "TranslateX", 600, 0, null);
                renderTransform.Animate(renderTransform.TranslateY, renderTransform.TranslateY + compositeTransform1.TranslateY, "TranslateY", 600, 0, null);
                renderTransform.Animate(renderTransform.ScaleX, compositeTransform1.ScaleX, "ScaleX", 600, 0, null, null);
                renderTransform.Animate(renderTransform.ScaleY, compositeTransform1.ScaleY, "ScaleY", 600, 0, null, null);
            }
        }

        public void MakeNormal()
        {
            VisualStateManager.GoToState(this._mediaElement.TransportControls, "NormalMode", true);
            this._mediaElement.IsHitTestVisible = true;
            this._brdClose.Visibility = Visibility.Collapsed;
            this.IsCompact = false;

            CompositeTransform renderTransform = this.RenderTransform as CompositeTransform;

            renderTransform.Animate(renderTransform.TranslateX, 0, "TranslateX", 600, 0, null);
            renderTransform.Animate(renderTransform.TranslateY, 0, "TranslateY", 600, 0, null);
            renderTransform.Animate(renderTransform.ScaleX, 1, "ScaleX", 600, 0, null, null);
            renderTransform.Animate(renderTransform.ScaleY, 1, "ScaleY", 600, 0, null, null);
        }

        private void VideoViewerElementUC_Loaded(object sender, RoutedEventArgs e)
        {
            if (CustomFrame.Instance == null || this.VM == null)
                return;//для тестов

            this.VM.LoadingStatusUpdated = this.HandleLoadingStatusUpdated;
            
            

            this.SizeChanged += this.P_SizeChanged;
        }

        public void PlayPause()
        {
            if (this._mediaElement.CurrentState == MediaElementState.Playing)
                this._mediaElement.Pause();
            else
                this._mediaElement.Play();
        }

        public void InitViewModel(object vm)
        {
            VisualStateManager.GoToState(this._mediaElement.TransportControls, "Loading", true);

            this._newPos = null;
            this._mediaElement.Stop();
            //this._mediaElement.Position = new TimeSpan();
            this._mediaElement.Source = null;

            if(this.VM != null)
                this.VM.LoadingStatusUpdated = null;
            
            base.DataContext = vm;
            this.VM.LoadingStatusUpdated = this.HandleLoadingStatusUpdated;

            if(vm is System.ComponentModel.INotifyPropertyChanged notify)
            {
                notify.PropertyChanged += Notify_PropertyChanged;
            }

            if(AudioPlayerViewModel.Instance.PlaybackState == Windows.Media.MediaPlaybackStatus.Playing)
                AudioPlayerViewModel.Instance.PlayPause();
        }

        private void Notify_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Resolution")
            {
                if (this.VM.Resolution == null)
                    return;
                //this.SetNewSource(this.VM.Resolution.Url);
                if (this._mediaElement.Source != null)
                    this._newPos = this._mediaElement.Position;
                this._mediaElement.Source = new Uri(this.VM.Resolution.Url, UriKind.Absolute);
                
                if (this.VM.Resolution.Type == YoutubeExtractor.AdaptiveType.Video)
                {
                    var audio = this.VM.Resolutions.FirstOrDefault((i) => i.Type == YoutubeExtractor.AdaptiveType.Audio);
                    if (audio != null)
                    {
                        this._soundStream.Position = this._mediaElement.Position;
                        this._soundStream.Source = new Uri(audio.Url);
                        return;
                    }
                }
                
                this._soundStream.Source = null;
            }
        }

        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {
            if(status == ProfileLoadingStatus.Reloading)
            {
                this._soundStream.Source = null;
                VisualStateManager.GoToState(this._mediaElement.TransportControls, "Loading", true);
            }
            else if (status == ProfileLoadingStatus.ReloadingFailed)
            {
                VisualStateManager.GoToState(this._mediaElement.TransportControls, "Error", true);
            }
            else if (status == ProfileLoadingStatus.Loaded)
            {
                if (this._mediaElement.Source != null)
                    return;

                VisualStateManager.GoToState(this._mediaElement.TransportControls, "Normal", true);
                if (this.VM.Resolutions.Count > 0)
                {
                    var resolution = this.VM.Resolutions.FirstOrDefault((r) => r.Resolution==Settings.DefaultVideoResolution);
                    if (resolution == null)
                        resolution = this.VM.Resolutions.First();
                    this.VM.Resolution = resolution;
                }
            }
        }
        /*
        private void SetNewSource(string url)
        {
            if(this._mediaElement.Source!=null)
                this._newPos = this._mediaElement.Position;
            this._mediaElement.Source = new Uri(url);
            if(this._mediaElement.IsAudioOnly)
            {
                var audio = this.VM.Resolutions.FirstOrDefault((i) => i.Resolution == 0);
                this._soundStream.Source = new Uri(audio.Url);
            }
            else
            {
                this._soundStream.Source = null;
            }
        }
        */
        /*
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //VisualStateManager.GoToState(this._mediaElement.TransportControls, "NormalMode", true);
            AppBarButton btn = sender as AppBarButton;
            int index = this.VM.Resolutions.IndexOf(btn.DataContext as string);
            var info = this.VM.Infos[index];
            this.SetNewSource(info.DownloadUrl);
        }
        */
        
        private void MediaControlsCommandBar2_Loaded(object sender, RoutedEventArgs e)
        {
            CommandBar bar = sender as CommandBar;
            bar.MinWidth = bar.ActualWidth + 1;
            bar.MinWidth = 400;
        }
        
        private void _brdClose_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            CustomFrame.Instance.DestroyVideoElement();
        }
        
        private void Back_Tapped(object sender, TappedRoutedEventArgs e)
        {
//            VideoViewerUC.Show(this.VM.OwnerId, this.VM.VideoId, "", this.VM.Video);
            
            /*
            UIElement temp = CustomFrame.Instance.VideoViewerElement;
            
            CompositeTransform renderTransform = temp.RenderTransform as CompositeTransform;
            
            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                from = renderTransform.TranslateX,
                to = 0,
                propertyPath = "TranslateX",
                duration = 300,
                target = renderTransform,
                easing = this.ANIMATION_EASING
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                from = renderTransform.TranslateY,
                to = 0,
                propertyPath = "TranslateY",
                duration = 300,
                target = renderTransform,
                easing = this.ANIMATION_EASING
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                from = renderTransform.ScaleX,
                to = 1,
                propertyPath = "ScaleX",
                duration = 300,
                target = renderTransform,
                easing = this.ANIMATION_EASING
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                from = renderTransform.ScaleY,
                to = 1,
                propertyPath = "ScaleY",
                duration = 300,
                target = renderTransform,
                easing = this.ANIMATION_EASING
            });
            AnimationUtils.AnimateSeveral(animInfoList);
            */
        }

        private void ControlPanelVisibilityStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("Video element state: " + e.NewState.Name.ToString());
#endif
        }

        /// <summary>
        /// Разворачиваем
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!this.IsCompact)
                return;

            
            VideoViewerUC.Show();
            e.Handled = true;
        }


        private void _mediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (this._soundStream.Source == null)
                return;

            MediaElement element = sender as MediaElement;
            if (element.CurrentState == MediaElementState.Playing)
                this._soundStream.Play();
            else if (element.CurrentState == MediaElementState.Paused)
                this._soundStream.Pause();
            else if (element.CurrentState == MediaElementState.Stopped)
                this._soundStream.Stop();

            this._soundStream.Position = element.Position;
        }

        private void ProgressSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if(this._soundStream.Source!=null)
            {
                if (Math.Abs(this._soundStream.Position.TotalMilliseconds - this._mediaElement.Position.TotalMilliseconds) > 300)
                    this._soundStream.Position = this._mediaElement.Position;
            }
        }

        private void SettingFlyout_Opened(object sender, object e)
        {
            Flyout flyout = sender as Flyout;
            object vm = (flyout.Content as FrameworkElement).DataContext;
            if (vm == null)
                (flyout.Content as FrameworkElement).DataContext = this.VM;
            this._settingFlyout = flyout;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this._settingFlyout == null)
                return;

            this._settingFlyout.Hide();
            this._settingFlyout = null;
        }

        private void StackPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this._mediaElement.TransportControls, "VolumeStateOpen", true);
        }

        private void StackPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (this._volumeOpened)
                return;

            VisualStateManager.GoToState(this._mediaElement.TransportControls, "VolumeStateClose", true);
        }

        private void Volume_Click(object sender, RoutedEventArgs e)
        {
            if(this._volumeOpened)
            {
                this._volumeOpened = false;
                this.StackPanel_PointerExited(sender, null);
            }
            else
            {
                this.StackPanel_PointerEntered(sender, null);
                this._volumeOpened = true;
            }
        }
        /*
        private void SkipForwardButton_Loaded(object sender, RoutedEventArgs e)
        {
            this._skipForwardButton = sender as AppBarButton;
            this._skipForwardButton.IsEnabled = true;
            this._skipForwardButton.Visibility = Visibility.Visible;
        }

        private void SkipBackwardButton_Loaded(object sender, RoutedEventArgs e)
        {
            this._skipBackwardButton = sender as AppBarButton;
            this._skipBackwardButton.IsEnabled = true;
            this._skipBackwardButton.Visibility = Visibility.Visible;
        }
        */
        private void SkipBackwardButton_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

        }

        private void SkipForwardButton_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

        }

        private async void ButtonMini_Click(object sender, RoutedEventArgs e)
        {
            //Enter CompactOverlay mode and set the form to 200 x 200 pixels
            //var preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            //preferences.CustomSize = new Windows.Foundation.Size(200, 200);
            //await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, preferences);

            //Return to default mode
            //var preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.Default);
            //await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default, preferences);



            var myView = Windows.ApplicationModel.Core.CoreApplication.CreateNewView();
            int newViewId = 0;


            await myView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var frame = new Frame();
                frame.Navigate(typeof(AboutPage));
                Window.Current.Content = frame;
                // This is a change from 8.1: In order for the view to be displayed later it needs to be activated.
                Window.Current.Activate();
                ApplicationView.GetForCurrentView().Title = "About";
                newViewId = ApplicationView.GetForCurrentView().Id;
            });

            

            

            bool viewShown = await ApplicationViewSwitcher.TryShowAsViewModeAsync(newViewId, ApplicationViewMode.CompactOverlay);
        }

        /*
        private void ButtonDock_Click(object sender, RoutedEventArgs e)
        {
            this.DockedAction?.Invoke();
            this.MakeCompact();
            CustomFrame.Instance.AddDragElement(this);
        }
        */
        /*
        private void ButtonDock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this.DockedAction?.Invoke();
            this.MakeCompact();
            //CustomFrame.Instance.AddDragElement(this);
        }
        */
        private void _border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if(this.IsCompact)
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeAll, 1);
        }

        private void _border_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }


        private void DragItem_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (!this.IsCompact)
                return;

            FrameworkElement element = (sender as FrameworkElement);
            CompositeTransform transform = element.RenderTransform as CompositeTransform;

            if (transform.TranslateX + e.Delta.Translation.X + (element.ActualWidth * transform.ScaleX) > CustomFrame.Instance.ActualWidth)
                return;

            if (transform.TranslateX + e.Delta.Translation.X < 0)
            {
                transform.TranslateX = 0;
                return;
            }

            if (transform.TranslateY + e.Delta.Translation.Y + (element.ActualHeight * transform.ScaleY) > CustomFrame.Instance.ActualHeight)
                return;

            if (transform.TranslateY + e.Delta.Translation.Y < 0)
            {
                transform.TranslateY = 0;
                return;
            }

            transform.TranslateX += e.Delta.Translation.X;
            transform.TranslateY += e.Delta.Translation.Y;
        }

        public MediaElement ME
        {
            get { return this._mediaElement; }
        }
    }
}
/*
Disabled
NormalMode
PlayState
VolumeState
NonFullWindowState
AudioSelectionUnavailable
CCSelectionUnavailable
Normal
Disabled
Loading
Buffering
ControlPanelFadeOut
Normal
PauseState
ControlPanelFadeIn
ControlPanelFadeOut
ControlPanelFadeIn
PlayState
ControlPanelFadeOut
PauseState
ControlPanelFadeIn
PlayState
ControlPanelFadeOut
PauseState
ControlPanelFadeIn
ControlPanelFadeOut
ControlPanelFadeIn

    */