using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
/*
using Urho;
using Urho.UWP;
using Urho.Resources;
using Urho.Actions;
*/

using System.Diagnostics;


using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Foundation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using LunaVK.Photo;

using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.Media.Capture;
using Windows.Devices.Enumeration;
using LunaVK.Framework;
using Windows.UI.Xaml;
using Windows.Media.Devices;
using Windows.Graphics.Display;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml.Navigation;
using LunaVK.UC.AttachmentPickers;
using LunaVK.Core.Library;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using LunaVK.UC;
using LunaVK.Core.Framework;

namespace LunaVK.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class CreateStoryPage : PageBase
    {
        //        Application currentApplication;
        //        TypeInfo selectedGameType;

        DisplayInformation displayInformation;

        public CreateStoryPage()
        {
            /*
            GameTypes = new ObservableCollection<TypeInfo>();
            Urho.Application.UnhandledException += (s, e) => e.Handled = true;
            DataContext = this;
            */
            this.InitializeComponent();

            VisualStateManager.GoToState(this, "Initializing", false);

            /*
            GameTypes.Add(new TypeInfo(typeof(BasicTechniques), "BasicTechniques", ""));
            GameTypes.Add(new TypeInfo(typeof(DynamicGeometry), "DynamicGeometry", ""));*/
            //            this.controller = new CollageController(this._grid);
            this._collageCreationUC.InitWithCanvas(this._grid,this._canvasControl);

            this.displayInformation = DisplayInformation.GetForCurrentView();

            base.Loaded += CreateStoryPage_Loaded;
            base.Unloaded += CreateStoryPage_Unloaded;
            Application.Current.Resuming += Application_Resuming;
            Window.Current.VisibilityChanged += this.Current_VisibilityChanged;
            Application.Current.Suspending += Current_Suspending;

            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.CameraHalfPressed += HardwareButtons_CameraHalfPressed;
                HardwareButtons.CameraPressed += HardwareButtons_CameraPressed;
            }

            this._collageCreationUC.InEditCallback = this.InEditCallBack;

            this.DurationStrTimer.Interval = TimeSpan.FromSeconds(0.5);
            this.DurationStrTimer.Tick += DurationStr_Tick;
        }

        private DateTime _recordStartTime;

        void DurationStr_Tick(object sender, object e)
        {
            TimeSpan timeSpan = DateTime.Now - _recordStartTime;
            this._timerTextBlock.Text = timeSpan.ToString("ss");
        }

        private async void HardwareButtons_CameraHalfPressed(object sender, CameraEventArgs e)
        {
            // test if focus is supported
            if (_mediaCapture.VideoDeviceController.FocusControl.Supported)
            {
                // get the focus control from the _mediaCapture object
                var focusControl = _mediaCapture.VideoDeviceController.FocusControl;

                // try to get full range, but settle for the first supported one.
                var focusRange = focusControl.SupportedFocusRanges.Contains(AutoFocusRange.FullRange) ? AutoFocusRange.FullRange : focusControl.SupportedFocusRanges.FirstOrDefault();

                // try to get the focus mode for focussing just once, but settle for the first supported one.
                var focusMode = focusControl.SupportedFocusModes.Contains(FocusMode.Single) ? FocusMode.Single : focusControl.SupportedFocusModes.FirstOrDefault();

                // now configure the focus control with the range and mode as settings
                focusControl.Configure(
                    new FocusSettings
                    {
                        Mode = focusMode,
                        AutoFocusRange = focusRange
                    });

                // finally wait for the camera to focus
                await focusControl.FocusAsync();
            }
        }

        private void HardwareButtons_CameraPressed(object sender, CameraEventArgs e)
        {
            /*
            // This is where we want to save to.
            var storageFolder = KnownFolders.SavedPictures;

            // Create the file that we're going to save the photo to.
            var file = await storageFolder.CreateFileAsync("sample.jpg", CreationCollisionOption.ReplaceExisting);

            // Update the file with the contents of the photograph.
            await _mediaCapture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), file);
            */
            this.Action_Tapped(sender, null);
        }

        private void InEditCallBack(bool status)
        {
            VisualStateManager.GoToState(this, status ? "InPhotoEdit" : "ReadySendPhoto", false);
        }

        private void Current_Suspending(object sender, SuspendingEventArgs e)
        {
            this.Stop();
        }

        private void CreateStoryPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.displayInformation.OrientationChanged += DisplayInformation_OrientationChanged;
            CustomFrame.Instance.Header.IsVisible = false;
            CustomFrame.Instance.MySplitView.ActivateSwipe(false);

            this.InitializeCameraAsync();
        }

        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            CustomFrame.Instance.Header.IsVisible = true;
            CustomFrame.Instance.Header.HideSandwitchButton = false;
        }

        private void DisplayInformation_OrientationChanged(DisplayInformation sender, object args)
        {
            this.UpdateRotation();
        }

        private void CreateStoryPage_Unloaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.MySplitView.ActivateSwipe(true);
            CustomFrame.Instance.Header.IsVisible = true;
            CustomFrame.Instance.Header.HideSandwitchButton = false;
            this.displayInformation.OrientationChanged -= DisplayInformation_OrientationChanged;
            Application.Current.Resuming -= Application_Resuming;
            Window.Current.VisibilityChanged -= this.Current_VisibilityChanged;
            Application.Current.Suspending -= this.Current_Suspending;
            this.Stop();
        }

        private void Current_VisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            if (!e.Visible)
                this.Stop();
        }

        /// <summary>
        /// Прекращает предварительный просмотр
        /// </summary>
        private async void Stop()
        {
            if (this._mediaCapture != null)
            {
                this._mediaCapture.Failed -= _mediaCapture_Failed;
                if (this._mediaCapture.CameraStreamState != CameraStreamState.NotStreaming)
                {
                    await this._mediaCapture.StopPreviewAsync();
                    //await this._mediaCapture.StopRecordAsync();
                }
                this._mediaCapture.Dispose();
                this._mediaCapture = null;
            }

            if (this._mediaRecording != null)
            {
                this._mediaRecording.FinishAsync();
                this._mediaRecording = null;
            }

            //this.PreviewControl.Source = null;
        }

        private async void Application_Resuming(object sender, object o)
        {
            if(!this.inEdit)
                await InitializeCameraAsync(!this._backCameraUsed);
        }

        // Provides functionality to preview and capture the photograph
        private MediaCapture _mediaCapture;

        //https://jeremylindsayni.wordpress.com/2016/04/24/how-to-use-the-camera-on-your-device-with-c-in-a-uwp-application-part-4-cleaning-up-resources-and-other-bits/
        private void UpdateRotation()
        {
            switch (this._displayOrientation)
            {
                case DisplayOrientations.Landscape:
                    {
                        this._mediaCapture.SetRecordRotation(VideoRotation.None);
                        this._mediaCapture.SetPreviewRotation(VideoRotation.None);
                        break;
                    }
                case DisplayOrientations.LandscapeFlipped:
                    {
                        this._mediaCapture.SetRecordRotation(VideoRotation.Clockwise180Degrees);
                        this._mediaCapture.SetPreviewRotation(VideoRotation.Clockwise180Degrees);
                        break;
                    }
                case DisplayOrientations.Portrait:
                    {
                        this._mediaCapture.SetRecordRotation(VideoRotation.None);//this._mediaCapture.SetRecordRotation(this._backCameraUsed ? VideoRotation.Clockwise90Degrees : VideoRotation.Clockwise270Degrees);
                        this._mediaCapture.SetPreviewRotation(this._backCameraUsed ? VideoRotation.Clockwise90Degrees : VideoRotation.Clockwise270Degrees);
                        break;
                    }
                case DisplayOrientations.PortraitFlipped:
                    {
                        this._mediaCapture.SetRecordRotation(VideoRotation.Clockwise270Degrees);
                        this._mediaCapture.SetPreviewRotation(VideoRotation.Clockwise270Degrees);
                        break;
                    }
                default:
                    {
                        this._mediaCapture.SetRecordRotation(VideoRotation.None);
                        this._mediaCapture.SetPreviewRotation(VideoRotation.None);
                        break;
                    }
            }

            this.PreviewControl.FlowDirection = this._backCameraUsed ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
        }

        private void UpdateIcons()
        {
//            this.brdFlash.Visibility = this._mediaCapture.VideoDeviceController.FlashControl.Supported ? Visibility.Visible : Visibility.Collapsed;
            this.brdSwitch.Visibility = this.onlyOneCamera ? Visibility.Collapsed : Visibility.Visible;
 //           this.brdAction.Visibility = this._mediaCapture.CameraStreamState == CameraStreamState.Streaming ? Visibility.Collapsed : Visibility.Visible;
            
            if(this.inEdit)
            {
 //               this._collageCreationUC.Visibility = Visibility.Visible;
                this.brdSwitch.Visibility = Visibility.Collapsed;
 //               this.brdAction.Visibility = Visibility.Collapsed;
//                this.brdFlash.Visibility = Visibility.Collapsed;
            }
        }

        private DisplayOrientations _displayOrientation
        {
            get
            {
                return this.displayInformation.CurrentOrientation;
            }
        }

        private bool _backCameraUsed;
        private bool onlyOneCamera;
        DeviceInformation preferredDevice = null;

        private async Task InitializeCameraAsync(bool front = false)
        {
            if (this._mediaCapture != null && this._mediaCapture.CameraStreamState == CameraStreamState.Streaming)
                await this._mediaCapture.StopPreviewAsync();
            //if (this._mediaCapture == null)
            //{

            //sw.Restart();
            var cameraDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);// Get the camera devices
            //sw.Stop();
            //System.Diagnostics.Debug.WriteLine("FindAllAsync {0} ms.", sw.ElapsedMilliseconds);

            // try to get the back facing device for a phone
            DeviceInformation backFacingDevice = null;
            if (front == false)
                backFacingDevice = cameraDevices.FirstOrDefault(c => c.EnclosureLocation?.Panel == Windows.Devices.Enumeration.Panel.Back);

            // but if that doesn't exist, take the first camera device available
            preferredDevice = backFacingDevice ?? cameraDevices.FirstOrDefault();

            if (preferredDevice == null)
            {
                VisualStateManager.GoToState(this, "NoCamera", false);
                return;//no camera on PC?
            }

            this.onlyOneCamera = cameraDevices.Count == 1;


            this._backCameraUsed = backFacingDevice != null;


            this._mediaCapture = new MediaCapture();// Create MediaCapture

            this._mediaCapture.Failed += _mediaCapture_Failed;

            sw.Restart();
            await this._mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings { VideoDeviceId = preferredDevice.Id });// Initialize MediaCapture and settings
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("InitializeAsync {0} ms.", sw.ElapsedMilliseconds);

            this.UpdateRotation();
            this.UpdateIcons();


            if (this._mediaCapture.VideoDeviceController.FocusControl.Supported)
            {
                this._mediaCapture.VideoDeviceController.FocusControl.Configure(new FocusSettings() { Mode = FocusMode.Auto });
            }


            this.PreviewControl.Source = _mediaCapture;// Set the preview source for the CaptureElement

            

            // Start viewing through the CaptureElement 
            await this._mediaCapture.StartPreviewAsync();

            VisualStateManager.GoToState(this, "Ready", false);


            if (this._mediaCapture.VideoDeviceController.FocusControl.Supported)
            {
                await this._mediaCapture.VideoDeviceController.FocusControl.FocusAsync();
            }
            //}

            
        }

        private void _mediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            int i = 0;
        }



#region Focus
        private async void PreviewControl_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (!this._mediaCapture.VideoDeviceController.FocusControl.Supported)
                return;

            if (this._mediaCapture.VideoDeviceController.FocusControl.FocusState != MediaCaptureFocusState.Searching)
            {
                var smallEdge = Math.Min(Window.Current.Bounds.Width, Window.Current.Bounds.Height);

                // Choose to make the focus rectangle 1/4th the length of the shortest edge of the window
                var size = new Size(smallEdge / 4, smallEdge / 4);
                var position = e.GetPosition(sender as UIElement);

                // Note that at this point, a rect at "position" with size "size" could extend beyond the preview area. The following method will reposition the rect if that is the case
                await this.TapToFocus(position, size);
            }
            else
            {
                //await TapUnfocus();
            }
        }

        public async Task TapToFocus(Point position, Size size)
        {
            //_isFocused = true;

            var previewRect = GetPreviewStreamRectInControl();
            var focusPreview = ConvertUiTapToPreviewRect(position, size, previewRect);

            // Note that this Region Of Interest could be configured to also calculate exposure 
            // and white balance within the region
            var regionOfInterest = new RegionOfInterest
            {
                AutoFocusEnabled = true,
                BoundsNormalized = true,
                Bounds = focusPreview,
                Type = RegionOfInterestType.Unknown,
                Weight = 100,
            };


            var focusControl = _mediaCapture.VideoDeviceController.FocusControl;
            var focusRange = focusControl.SupportedFocusRanges.Contains(AutoFocusRange.FullRange) ? AutoFocusRange.FullRange : focusControl.SupportedFocusRanges.FirstOrDefault();
            var focusMode = focusControl.SupportedFocusModes.Contains(FocusMode.Single) ? FocusMode.Single : focusControl.SupportedFocusModes.FirstOrDefault();
            var settings = new FocusSettings { Mode = focusMode, AutoFocusRange = focusRange };
            focusControl.Configure(settings);

            var roiControl = _mediaCapture.VideoDeviceController.RegionsOfInterestControl;
            await roiControl.SetRegionsAsync(new[] { regionOfInterest }, true);

            await focusControl.FocusAsync();
        }
        
        public Windows.Foundation.Rect GetPreviewStreamRectInControl()
        {
            var result = new Windows.Foundation.Rect();

            var previewResolution = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

            // In case this function is called before everything is initialized correctly, return an empty result
            if (PreviewControl == null || PreviewControl.ActualHeight < 1 || PreviewControl.ActualWidth < 1 ||
                previewResolution == null || previewResolution.Height == 0 || previewResolution.Width == 0)
            {
                return result;
            }

            var streamWidth = previewResolution.Width;
            var streamHeight = previewResolution.Height;

            // For portrait orientations, the width and height need to be swapped
            if (_displayOrientation == DisplayOrientations.Portrait || _displayOrientation == DisplayOrientations.PortraitFlipped)
            {
                streamWidth = previewResolution.Height;
                streamHeight = previewResolution.Width;
            }

            // Start by assuming the preview display area in the control spans the entire width and height both (this is corrected in the next if for the necessary dimension)
            result.Width = PreviewControl.ActualWidth;
            result.Height = PreviewControl.ActualHeight;

            // If UI is "wider" than preview, letterboxing will be on the sides
            if ((PreviewControl.ActualWidth / PreviewControl.ActualHeight > streamWidth / (double)streamHeight))
            {
                var scale = PreviewControl.ActualHeight / streamHeight;
                var scaledWidth = streamWidth * scale;

                result.X = (PreviewControl.ActualWidth - scaledWidth) / 2.0;
                result.Width = scaledWidth;
            }
            else // Preview stream is "wider" than UI, so letterboxing will be on the top+bottom
            {
                var scale = PreviewControl.ActualWidth / streamWidth;
                var scaledHeight = streamHeight * scale;

                result.Y = (PreviewControl.ActualHeight - scaledHeight) / 2.0;
                result.Height = scaledHeight;
            }

            return result;
        }

        private Windows.Foundation.Rect ConvertUiTapToPreviewRect(Point tap, Size size, Windows.Foundation.Rect previewRect)
        {
            // Adjust for the resulting focus rectangle to be centered around the position
            double left = tap.X - size.Width / 2, top = tap.Y - size.Height / 2;

            // Get the information about the active preview area within the CaptureElement (in case it's letterboxed)
            double previewWidth = previewRect.Width, previewHeight = previewRect.Height;
            double previewLeft = previewRect.Left, previewTop = previewRect.Top;

            // Transform the left and top of the tap to account for rotation
            switch (_displayOrientation)
            {
                case DisplayOrientations.Portrait:
                    var tempLeft = left;

                    left = top;
                    top = previewRect.Width - tempLeft;
                    break;
                case DisplayOrientations.LandscapeFlipped:
                    left = previewRect.Width - left;
                    top = previewRect.Height - top;
                    break;
                case DisplayOrientations.PortraitFlipped:
                    var tempTop = top;

                    top = left;
                    left = previewRect.Width - tempTop;
                    break;
            }

            // For portrait orientations, the information about the active preview area needs to be rotated
            if (_displayOrientation == DisplayOrientations.Portrait || _displayOrientation == DisplayOrientations.PortraitFlipped)
            {
                previewWidth = previewRect.Height;
                previewHeight = previewRect.Width;
                previewLeft = previewRect.Top;
                previewTop = previewRect.Left;
            }

            // Normalize width and height of the focus rectangle
            var width = size.Width / previewWidth;
            var height = size.Height / previewHeight;

            // Shift rect left and top to be relative to just the active preview area
            left -= previewLeft;
            top -= previewTop;

            // Normalize left and top
            left /= previewWidth;
            top /= previewHeight;

            // Ensure rectangle is fully contained within the active preview area horizontally
            left = Math.Max(left, 0);
            left = Math.Min(1 - width, left);

            // Ensure rectangle is fully contained within the active preview area vertically
            top = Math.Max(top, 0);
            top = Math.Min(1 - height, top);

            // Create and return resulting rectangle
            return new Windows.Foundation.Rect(left, top, width, height);
        }
#endregion
        private void Switch_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this.InitializeCameraAsync(this._backCameraUsed);
        }

        private void Flash_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (this._mediaCapture.VideoDeviceController.FlashControl.PowerSupported)
                this._mediaCapture.VideoDeviceController.FlashControl.PowerPercent = 20;
            this._mediaCapture.VideoDeviceController.FlashControl.Enabled = !this._mediaCapture.VideoDeviceController.FlashControl.Enabled;
        }

        private bool inProcess;
        private bool inEdit;
        //InMemoryRandomAccessStream captureStream;
        Stopwatch sw = new Stopwatch();

        byte temppp = 0;

        private async void CapturePhoto()
        {
            VisualStateManager.GoToState(this, "CapturingPhoto", true);

            this.inProcess = true;
            this.inEdit = true;
            this.UpdateIcons();

            // Prepare and capture photo

            var temp = ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8);
            //temp.Width = 240;
            //temp.Height = 320;
            var capture = await this._mediaCapture.PrepareLowLagPhotoCaptureAsync(temp);

            //sw.Stop();
            //System.Diagnostics.Debug.WriteLine("PrepareLowLagPhotoCaptureAsync {0} ms.", sw.ElapsedMilliseconds);

            //sw.Restart();

            var photo = await capture.CaptureAsync();//2 секунды
            //sw.Stop();
            //System.Diagnostics.Debug.WriteLine("CaptureAsync {0} ms.", sw.ElapsedMilliseconds);

            //sw.Restart();
            var bitmap = photo.Frame.SoftwareBitmap;
            await capture.FinishAsync();
            //sw.Stop();
            //System.Diagnostics.Debug.WriteLine("FinishAsync {0} ms.", sw.ElapsedMilliseconds);


            //sw.Restart();
            //bitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            //var source = new SoftwareBitmapSource();
            //await source.SetBitmapAsync(bitmap);
            //sw.Stop();
            //System.Diagnostics.Debug.WriteLine("SetBitmapAsync {0} ms.", sw.ElapsedMilliseconds);

            //this.imageControl.Source = source;
            //}


            var source = new SoftwareBitmapSource();
            //
            bitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            BitmapImage bmpImage = new BitmapImage();
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                var propertySet = new BitmapPropertySet();
                //var qualityValue = new BitmapTypedValue( 0.8, PropertyType.Single );
                //propertySet.Add("ImageQuality", qualityValue);
                sw.Restart();
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream, propertySet);
                sw.Stop();
                System.Diagnostics.Debug.WriteLine("BitmapEncoder.CreateAsync {0} ms.", sw.ElapsedMilliseconds);



                //encoder.BitmapTransform.ScaledWidth = 320;
                //encoder.BitmapTransform.ScaledHeight = 240;
                //encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                //encoder.IsThumbnailGenerated = true;

                bool isFrontDevice = this.preferredDevice.EnclosureLocation?.Panel == Windows.Devices.Enumeration.Panel.Front;
                if (isFrontDevice)
                {
                    encoder.BitmapTransform.Flip = BitmapFlip.Horizontal;
                }


                switch (this._displayOrientation)
                {
                    case DisplayOrientations.Portrait:
                        {
                            encoder.BitmapTransform.Rotation = isFrontDevice ? BitmapRotation.Clockwise90Degrees : BitmapRotation.Clockwise90Degrees;
                            break;
                        }
                    case DisplayOrientations.Landscape:
                        {
                            encoder.BitmapTransform.Rotation = BitmapRotation.None;
                            break;
                        }
                    case DisplayOrientations.LandscapeFlipped:
                        {
                            encoder.BitmapTransform.Rotation = BitmapRotation.Clockwise180Degrees;
                            break;
                        }
                }

                encoder.SetSoftwareBitmap(bitmap);

                if (encoder.IsThumbnailGenerated == false)
                    await encoder.FlushAsync();

                await bmpImage.SetSourceAsync(stream);
            }

            this.imageControl.Source = bmpImage;

            this.Stop();


            this.inProcess = false;

            VisualStateManager.GoToState(this, "ReadySendPhoto", true);
            //this.UpdateIcons();


            //await source.SetBitmapAsync(bitmap);

            //this.imageControl.Source = source;
        }

        /// <summary>
        /// Таймер для обновления текста секунд в записи
        /// </summary>
        DispatcherTimer DurationStrTimer = new DispatcherTimer();

        private bool InVideoCapture;

        private async void CaptureVideo()
        {
            this.InVideoCapture = true;

            VisualStateManager.GoToState(this, "CapturingVideo", true);

            var myVideo = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            var file = await myVideo.SaveFolder.CreateFileAsync("story.mp4", CreationCollisionOption.ReplaceExisting);
            this._mediaRecording = await _mediaCapture.PrepareLowLagRecordToStorageFileAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto), file);//todo: quality selector
            await this._mediaRecording.StartAsync();

            this._recordStartTime = DateTime.Now;
            this.DurationStrTimer.Start();
        }

        private void Action_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if(this.InVideoCapture)
            {
                VisualStateManager.GoToState(this, "Initializing", true);
                this.InVideoCapture = false;
                this.DurationStrTimer.Stop();
                //this._timerTextBlock.Text = "";
                this._mediaRecording.FinishAsync();
                VisualStateManager.GoToState(this, "ReadySendVideo", true);
                //
                //this.InitializeCameraAsync();
                return;
            }

            if (this._mediaCapture == null)
                return;


            if (this.bottomPanel.SelectedIndex == 0)
                this.CapturePhoto();
            else
                this.CaptureVideo();
        }

        private LowLagMediaRecording _mediaRecording;

        private async Task<RenderTargetBitmap> CreateRenderBitmap()
        {
            BitmapImage bimg = this.imageControl.Source as BitmapImage;


            RenderTargetBitmap rtb = new RenderTargetBitmap();
            this.brdCancel.Visibility = this._collageCreationUC.Visibility = this._controlPanel.Visibility = Visibility.Collapsed;

            var temp = this._root.Background;
            var temp2 = this._grid.Background;
            this._root.Background = this._grid.Background = null;


            // you can set the size as you need.
   //         this._root.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
   //         this._root.Arrange(new Windows.Foundation.Rect(0,0, 3000,1500));
            // to affect the changes in the UI, you must call this method at the end to apply the new changes
   //         this._root.UpdateLayout();

            await rtb.RenderAsync(this._root, (int)bimg.PixelWidth/* * dpi*/, (int)bimg.PixelHeight/* * dpi*/);









            this._root.Background = temp;
            this._grid.Background = temp2;
            this.brdCancel.Visibility = this._collageCreationUC.Visibility = this._controlPanel.Visibility = Visibility.Visible;
            
            return rtb;
        }

        private void Cancel_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.videoFile = null;
            this._progressGrid.Visibility = Visibility.Collapsed;
            this._timerTextBlock.Text = "";
            this.inEdit = false;
            this.imageControl.Source = null;
            this._collageCreationUC.ClearBoard();
            VisualStateManager.GoToState(this, "Initializing", true);
            this.InitializeCameraAsync();
        }

        private void Picture_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (this.bottomPanel.SelectedIndex == 0)
                this.ApplyBackGround();
            else
                this.ApplyVideoFile();
        }

        private async void ApplyVideoFile()
        {
            this.inEdit = true;

            Windows.Storage.Pickers.FileOpenPicker fileOpenPicker = new Windows.Storage.Pickers.FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add(".mp4");

            fileOpenPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;

            videoFile = await fileOpenPicker.PickSingleFileAsync();

            if (videoFile != null)
            {
                this.inEdit = true;

                VisualStateManager.GoToState(this, "ReadySendVideo", true);
            }
            else
            {
                this.inEdit = false;
            }

            this.UpdateIcons();
        }

            StorageFile videoFile;

        private async void ApplyBackGround()
        {
            this.inEdit = true;

            Windows.Storage.Pickers.FileOpenPicker fileOpenPicker = new Windows.Storage.Pickers.FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add(".jpeg");
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".png");

            fileOpenPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;

            StorageFile file = await fileOpenPicker.PickSingleFileAsync();

            if (file != null)
            {
                this.inEdit = true;

                BitmapImage bimg = new BitmapImage();

                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    //bimg.SetSource(stream);
                    this._collageCreationUC.LoadFromStream(stream);
                }
                //this.imageControl.Source = bimg;
                
                VisualStateManager.GoToState(this, "ReadySendPhoto", true);
            }
            else
            {
                this.inEdit = false;
            }

            this.UpdateIcons();
        }

        private async void BrdSave_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            
            

            var picker = new Windows.Storage.Pickers.FileSavePicker();
            string ext = ".jpg";

            // set appropriate file types
            picker.FileTypeChoices.Add(ext + " File", new List<string> { ext });
            picker.DefaultFileExtension = ext;
            picker.SuggestedFileName = "test";


            StorageFile file = await picker.PickSaveFileAsync();
            if (file == null)
            {
                return;
            }

            /*
            var rtb = await this.CreateRenderBitmap();

            int dpi = 300;

            var pixelBuffer = await rtb.GetPixelsAsync();
            var pixels = pixelBuffer.ToArray();

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                                     (uint)rtb.PixelWidth, (uint)rtb.PixelHeight,
                                     dpi, dpi,//displayInformation.RawDpiX, displayInformation.RawDpiY,
                                     pixels);
                await encoder.FlushAsync();
            }
            */
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await this._collageCreationUC.SaveToStream(stream);
            }
        }

        private void BrdSelectUser_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            
            SharePostUC sharePostUC = new SharePostUC("историей", WallService.RepostObject.photo,0,0);
            sharePostUC.HideOptions();

            PopUpService statusChangePopup = new PopUpService
            {
                Child = sharePostUC
            };
            
            statusChangePopup.OverrideBackKey = true;
            statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            //statusChangePopup.Opened += statusChangePopup_Opened;
            statusChangePopup.Show();

            sharePostUC.SendTap = async (list) =>
            {
                statusChangePopup.Hide();

                List<int> ids = list.Select((l) => l.conversation.peer.id).ToList();
                
                if (this.bottomPanel.SelectedIndex == 0)
                {
                    var rtb = await this.CreateRenderBitmap();

                    this._progressGrid.Visibility = Visibility.Visible;

                    int dpi = 300;

                    var pixelBuffer = await rtb.GetPixelsAsync();
                    var pixels = pixelBuffer.ToArray();

                    IRandomAccessStream stream = new InMemoryRandomAccessStream();

                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                                            (uint)rtb.PixelWidth, (uint)rtb.PixelHeight,
                                            dpi, dpi,//displayInformation.RawDpiX, displayInformation.RawDpiY,
                                            pixels);
                    await encoder.FlushAsync();

                    var reader = new DataReader(stream.GetInputStreamAt(0));
                    var bytes2 = new byte[stream.Size];
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(bytes2);
                    stream.Dispose();



                    PhotosService.Instance.UploadPhotoToHistory(false, 0, ids, "", "", 0, bytes2, (error) =>
                    {
                        int i = 0;
                        Execute.ExecuteOnUIThread(() => { this.Cancel_Tapped(null, null); });
                    },(progress)=>
                    {
                        Execute.ExecuteOnUIThread(() => { this._progressbar.Value = progress; });
                    });
                }
                else
                {
                    this._progressGrid.Visibility = Visibility.Visible;

                    
                    StorageFile file;

                    if (this.videoFile!=null)
                    {
                        file = this.videoFile;
                    }
                    else
                    {
                        var myVideo = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
                        file = await myVideo.SaveFolder.CreateFileAsync("story.mp4", CreationCollisionOption.OpenIfExists);
                    }
                    
                    var stream = await file.OpenReadAsync();

                    var reader = new DataReader(stream.GetInputStreamAt(0));
                    var bytes2 = new byte[stream.Size];
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(bytes2);
                    stream.Dispose();

                    VideoService.Instance.UploadVideoToHistory(false, 0, ids, "", "", 0, bytes2, (error) =>
                    {
                        int i = 0;
                        Execute.ExecuteOnUIThread(() => { this.Cancel_Tapped(null, null); });
                    }, (progress) =>
                    {
                        Execute.ExecuteOnUIThread(() => { this._progressbar.Value = progress/2; });
                    });
                }
                
            };
            
        }
        
        private async void BrdSend_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var rtb = await this.CreateRenderBitmap();

            this._progressGrid.Visibility = Visibility.Visible;

            int dpi = 300;

            var pixelBuffer = await rtb.GetPixelsAsync();
            var pixels = pixelBuffer.ToArray();


            IRandomAccessStream stream = new InMemoryRandomAccessStream();

            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                                    (uint)rtb.PixelWidth, (uint)rtb.PixelHeight,
                                    dpi, dpi,//displayInformation.RawDpiX, displayInformation.RawDpiY,
                                    pixels);
            await encoder.FlushAsync();

            var reader = new DataReader(stream.GetInputStreamAt(0));
            var bytes2 = new byte[stream.Size];
            await reader.LoadAsync((uint)stream.Size);
            reader.ReadBytes(bytes2);
            stream.Dispose();



            PhotosService.Instance.UploadPhotoToHistory(true, 0, null, "", "", 0, bytes2, (error) => {
                int i = 0;
                Execute.ExecuteOnUIThread(()=> { this.Cancel_Tapped(null, null); });
            }, (progress) =>
            {
                Execute.ExecuteOnUIThread(() => { this._progressbar.Value = progress; });
            });
        }
        /*
public ObservableCollection<TypeInfo> GameTypes { get; set; }

public TypeInfo SelectedGameType
{
get { return selectedGameType; }
set { RunGame(value); selectedGameType = value; }
}

public class TypeInfo
{
public Type Type { get; }
public string Name { get; }
public string Description { get; }

public TypeInfo(Type type, string name, string description)
{
Type = type;
Name = name;
Description = description;
}
}

public async void RunGame(TypeInfo value)
{
currentApplication?.Exit();
//at this moment, UWP supports assets only in pak files (see PackageTool)
//            currentApplication = UrhoSurface.Run(value.Type, new ApplicationOptions("Data") { Width = (int)UrhoSurface.ActualWidth, Height = (int)UrhoSurface.ActualHeight });
}
*/











        /*

    public class Sample : Application
    {
        UrhoConsole console;
        DebugHud debugHud;
        ResourceCache cache;
        Urho.Gui.Sprite logoSprite;
        Urho.Gui.UI ui;

        protected const float TouchSensitivity = 2;
        protected float Yaw { get; set; }
        protected float Pitch { get; set; }
        protected bool TouchEnabled { get; set; }
        protected Node CameraNode { get; set; }
        protected MonoDebugHud MonoDebugHud { get; set; }

        [Preserve]
        protected Sample(ApplicationOptions options = null) : base(options) { }

        static Sample()
        {
            Urho.Application.UnhandledException += Application_UnhandledException1;
        }

        static void Application_UnhandledException1(object sender, UnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached && !e.Exception.Message.Contains("BlueHighway.ttf"))
                Debugger.Break();
            e.Handled = true;
        }

        protected bool IsLogoVisible
        {
            get { return logoSprite.Visible; }
            set { logoSprite.Visible = value; }
        }

        static readonly Random random = new Random();
        /// Return a random float between 0.0 (inclusive) and 1.0 (exclusive.)
        public static float NextRandom() { return (float)random.NextDouble(); }
        /// Return a random float between 0.0 and range, inclusive from both ends.
        public static float NextRandom(float range) { return (float)random.NextDouble() * range; }
        /// Return a random float between min and max, inclusive from both ends.
        public static float NextRandom(float min, float max) { return (float)((random.NextDouble() * (max - min)) + min); }
        /// Return a random integer between min and max - 1.
        public static int NextRandom(int min, int max) { return random.Next(min, max); }

        /// <summary>
        /// Joystick XML layout for mobile platforms
        /// </summary>
        protected virtual string JoystickLayoutPatch => string.Empty;

        protected override void Start()
        {
            Log.LogMessage += e => System.Diagnostics.Debug.WriteLine($"[{e.Level}] {e.Message}");
            base.Start();
            if (Platform == Platforms.Android ||
                Platform == Platforms.iOS ||
                Options.TouchEmulation)
            {
                InitTouchInput();
            }
            Input.Enabled = true;
            MonoDebugHud = new MonoDebugHud(this);
            MonoDebugHud.Show();

            CreateLogo();
            SetWindowAndTitleIcon();
            CreateConsoleAndDebugHud();
            Input.SubscribeToKeyDown(HandleKeyDown);
        }

        protected override void OnUpdate(float timeStep)
        {
            MoveCameraByTouches(timeStep);
            base.OnUpdate(timeStep);
        }

        /// <summary>
        /// Move camera for 2D samples
        /// </summary>
        protected void SimpleMoveCamera2D(float timeStep)
        {
            // Do not move if the UI has a focused element (the console)
            if (UI.FocusElement != null)
                return;

            // Movement speed as world units per second
            const float moveSpeed = 4.0f;

            // Read WASD keys and move the camera scene node to the corresponding direction if they are pressed
            if (Input.GetKeyDown(Key.W)) CameraNode.Translate(Vector3.UnitY * moveSpeed * timeStep);
            if (Input.GetKeyDown(Key.S)) CameraNode.Translate(-Vector3.UnitY * moveSpeed * timeStep);
            if (Input.GetKeyDown(Key.A)) CameraNode.Translate(-Vector3.UnitX * moveSpeed * timeStep);
            if (Input.GetKeyDown(Key.D)) CameraNode.Translate(Vector3.UnitX * moveSpeed * timeStep);

            if (Input.GetKeyDown(Key.PageUp))
            {
                Camera camera = CameraNode.GetComponent<Camera>();
                camera.Zoom = camera.Zoom * 1.01f;
            }

            if (Input.GetKeyDown(Key.PageDown))
            {
                Camera camera = CameraNode.GetComponent<Camera>();
                camera.Zoom = camera.Zoom * 0.99f;
            }
        }

        /// <summary>
        /// Move camera for 3D samples
        /// </summary>
        protected void SimpleMoveCamera3D(float timeStep, float moveSpeed = 10.0f)
        {
            const float mouseSensitivity = .1f;

            if (UI.FocusElement != null)
                return;

            var mouseMove = Input.MouseMove;
            Yaw += mouseSensitivity * mouseMove.X;
            Pitch += mouseSensitivity * mouseMove.Y;
            Pitch = MathHelper.Clamp(Pitch, -90, 90);

            CameraNode.Rotation = new Quaternion(Pitch, Yaw, 0);

            if (Input.GetKeyDown(Key.W)) CameraNode.Translate(Vector3.UnitZ * moveSpeed * timeStep);
            if (Input.GetKeyDown(Key.S)) CameraNode.Translate(-Vector3.UnitZ * moveSpeed * timeStep);
            if (Input.GetKeyDown(Key.A)) CameraNode.Translate(-Vector3.UnitX * moveSpeed * timeStep);
            if (Input.GetKeyDown(Key.D)) CameraNode.Translate(Vector3.UnitX * moveSpeed * timeStep);
        }

        protected void MoveCameraByTouches(float timeStep)
        {
            if (!TouchEnabled || CameraNode == null)
                return;

            var input = Input;
            for (uint i = 0, num = input.NumTouches; i < num; ++i)
            {
                TouchState state = input.GetTouch(i);
                if (state.TouchedElement != null)
                    continue;

                if (state.Delta.X != 0 || state.Delta.Y != 0)
                {
                    var camera = CameraNode.GetComponent<Camera>();
                    if (camera == null)
                        return;

                    var graphics = Graphics;
                    Yaw += TouchSensitivity * camera.Fov / graphics.Height * state.Delta.X;
                    Pitch += TouchSensitivity * camera.Fov / graphics.Height * state.Delta.Y;
                    CameraNode.Rotation = new Quaternion(Pitch, Yaw, 0);
                }
                else
                {
                    var cursor = UI.Cursor;
                    if (cursor != null && cursor.Visible)
                        cursor.Position = state.Position;
                }
            }
        }

        protected void SimpleCreateInstructionsWithWasd(string extra = "")
        {
            SimpleCreateInstructions("Use WASD keys and mouse/touch to move" + extra);
        }

        protected void SimpleCreateInstructions(string text = "")
        {
            var textElement = new Urho.Gui.Text()
            {
                Value = text,
                HorizontalAlignment = Urho.Gui.HorizontalAlignment.Center,
                VerticalAlignment = Urho.Gui.VerticalAlignment.Center
            };
            textElement.SetFont(ResourceCache.GetFont("Fonts/Anonymous Pro.ttf"), 15);
            UI.Root.AddChild(textElement);
        }

        void CreateLogo()
        {
            cache = ResourceCache;
            var logoTexture = cache.GetTexture2D("Textures/LogoLarge.png");

            if (logoTexture == null)
                return;

            ui = UI;
            logoSprite = ui.Root.CreateSprite();
            logoSprite.Texture = logoTexture;
            int w = logoTexture.Width;
            int h = logoTexture.Height;
            logoSprite.SetScale(256.0f / w);
            logoSprite.SetSize(w, h);
            logoSprite.SetHotSpot(0, h);
            logoSprite.SetAlignment(Urho.Gui.HorizontalAlignment.Left, Urho.Gui.VerticalAlignment.Bottom);
            logoSprite.Opacity = 0.75f;
            logoSprite.Priority = -100;
        }

        void SetWindowAndTitleIcon()
        {
            var icon = cache.GetImage("Textures/UrhoIcon.png");
            Graphics.SetWindowIcon(icon);
            Graphics.WindowTitle = "UrhoSharp Sample";
        }

        void CreateConsoleAndDebugHud()
        {
            var xml = cache.GetXmlFile("UI/DefaultStyle.xml");
            console = Engine.CreateConsole();
            console.DefaultStyle = xml;
            console.Background.Opacity = 0.8f;

            debugHud = Engine.CreateDebugHud();
            debugHud.DefaultStyle = xml;
        }

        void HandleKeyDown(KeyDownEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Esc:
                    Exit();
                    return;
                case Key.F1:
                    console.Toggle();
                    return;
                case Key.F2:
                    debugHud.ToggleAll();
                    return;
            }

            var renderer = Renderer;
            switch (e.Key)
            {
                case Key.N1:
                    var quality = renderer.TextureQuality;
                    ++quality;
                    if (quality > 2)
                        quality = 0;
                    renderer.TextureQuality = quality;
                    break;

                case Key.N2:
                    var mquality = renderer.MaterialQuality;
                    ++mquality;
                    if (mquality > 2)
                        mquality = 0;
                    renderer.MaterialQuality = mquality;
                    break;

                case Key.N3:
                    renderer.SpecularLighting = !renderer.SpecularLighting;
                    break;

                case Key.N4:
                    renderer.DrawShadows = !renderer.DrawShadows;
                    break;

                case Key.N5:
                    var shadowMapSize = renderer.ShadowMapSize;
                    shadowMapSize *= 2;
                    if (shadowMapSize > 2048)
                        shadowMapSize = 512;
                    renderer.ShadowMapSize = shadowMapSize;
                    break;

                // shadow depth and filtering quality
                case Key.N6:
                    var q = (int)renderer.ShadowQuality++;
                    if (q > 3)
                        q = 0;
                    renderer.ShadowQuality = (ShadowQuality)q;
                    break;

                // occlusion culling
                case Key.N7:
                    var o = !(renderer.MaxOccluderTriangles > 0);
                    renderer.MaxOccluderTriangles = o ? 5000 : 0;
                    break;

                // instancing
                case Key.N8:
                    renderer.DynamicInstancing = !renderer.DynamicInstancing;
                    break;

                case Key.N9:
                    var screenshot = new Urho.Resources.Image();
                    Graphics.TakeScreenShot(screenshot);
                    screenshot.SavePNG(FileSystem.ProgramDir + $"Data/Screenshot_{GetType().Name}_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture)}.png");
                    break;
            }
        }

        void InitTouchInput()
        {
            TouchEnabled = true;
            var layout = ResourceCache.GetXmlFile("UI/ScreenJoystick_Samples.xml");
            if (!string.IsNullOrEmpty(JoystickLayoutPatch))
            {
                XmlFile patchXmlFile = new XmlFile();
                patchXmlFile.FromString(JoystickLayoutPatch);
                layout.Patch(patchXmlFile);
            }
            var screenJoystickIndex = Input.AddScreenJoystick(layout, ResourceCache.GetXmlFile("UI/DefaultStyle.xml"));
            Input.SetScreenJoystickVisible(screenJoystickIndex, true);
        }
    }





    public class BasicTechniques : Application
    {
        float yaw;
        float pitch;
        Node cameraNode;
        Scene scene;

        public BasicTechniques(ApplicationOptions options = null) : base(options) { }

        protected override void Start()
        {
            Urho.Application.UnhandledException += Application_UnhandledException;
            base.Start();

            // Create the scene content
            CreateScene();

            // Setup the viewport for displaying the scene
            SetupViewport();
        }

        void Application_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Exception);
            e.Handled = true;
        }

        void CreateScene()
        {
            Renderer.HDRRendering = true;
            scene = new Scene();
            scene.CreateComponent<Octree>();
            var zone = scene.CreateComponent<Zone>();
            zone.AmbientColor = new Color(0.3f, 0.3f, 0.3f);

            const float stepX = 0.23f;
            const float stepY = 0.3f;

            //by enabling this flag, we are able to edit assets via external editors (e.g. VS Code) and see changes immediately.
            ResourceCache.AutoReloadResources = true;

            cameraNode = scene.CreateChild();
            cameraNode.CreateComponent<Camera>();
            cameraNode.Position = new Vector3(stepX, -stepY, 0);

            Node lightNode = scene.CreateChild();
            lightNode.SetDirection(new Vector3(-1, -1, 1));
            Light light = lightNode.CreateComponent<Light>();
            light.LightType = LightType.Directional;
            light.Brightness = 1.1f;
            lightNode.Position = new Vector3(0, 0, 0);

            //Add a skybox
            //var skyNode = scene.CreateChild("Sky");
            //var skybox = skyNode.CreateComponent<Skybox>();
            //skybox.Model = CoreAssets.Models.Sphere;
            //skybox.SetMaterial(ResourceCache.GetMaterial("Materials/Skybox2.xml"));

            //see /FeatureSamples/Assets/Data/Sample43
            var materials = new string[,]
            {
            { "NoTexture", "NoTextureUnlit", "NoTextureNormal", "NoTextureAdd", "NoTextureMultiply" },
            { "Diff", "DiffUnlit", "DiffNormal", "DiffAlpha", "DiffAdd" },
            { "DiffEmissive", "DiffSpec", "DiffNormalSpec", "DiffAO", "DiffEnvCube" },
            { "Water", "Terrain", "NoTextureVCol", "Default", "CustomShader" },
            };

            for (int i = 0; i < materials.GetLength(1); i++)
            {
                for (int j = 0; j < materials.GetLength(0); j++)
                {
                    var sphereNode = scene.CreateChild();
                    var earthNode = sphereNode.CreateChild();
                    var textNode = sphereNode.CreateChild();
                    var material = materials[j, i];

                    Urho.Gui.Text3D text = textNode.CreateComponent<Urho.Gui.Text3D>();
                    text.Text = material;
                    text.SetFont(CoreAssets.Fonts.AnonymousPro, 13);
                    text.TextAlignment = Urho.Gui.HorizontalAlignment.Center;
                    text.VerticalAlignment = Urho.Gui.VerticalAlignment.Bottom;
                    text.HorizontalAlignment = Urho.Gui.HorizontalAlignment.Center;
                    textNode.Position = new Vector3(0, -0.75f, 0);

                    sphereNode.Position = new Vector3(i * stepX, -j * stepY, 1);
                    sphereNode.SetScale(0.2f);

                    var earthModel = earthNode.CreateComponent<StaticModel>();
                    //for VCol we have a special model:
                    //if (material.Contains("VCol"))
                        earthModel.Model = ResourceCache.GetModel("Models/Mushroom.mdl");//("Sample43/SphereVCol.mdl");
                    //else

                    //    earthModel.Model = CoreAssets.Models.Sphere;//built-in sphere model (.mdl):

                    earthModel.SetMaterial(ResourceCache.GetMaterial($"Sample43/Mat{material}.xml", sendEventOnFailure: false));
                    var backgroundNode = sphereNode.CreateChild();
                    backgroundNode.Scale = new Vector3(1, 1, 0.001f) * 1.1f;
                    backgroundNode.Position = new Vector3(0, 0, 0.55f);
                    var backgroundModel = backgroundNode.CreateComponent<StaticModel>();
                    backgroundModel.Model = CoreAssets.Models.Box;
                    backgroundModel.SetMaterial(Material.FromImage("Sample43/Background.png"));

                    earthNode.RunActions(new RepeatForever(new RotateBy(1f, 0, 5, 0)));
                }
            }
        }

        protected override void OnUpdate(float timeStep)
        {
            // rotate & move camera by mouse and WASD:

            const float mouseSensitivity = .1f;
            const float moveSpeed = 2;

            var mouseMove = Input.MouseMove;
            yaw += mouseSensitivity * mouseMove.X;
            pitch += mouseSensitivity * mouseMove.Y;
            pitch = MathHelper.Clamp(pitch, -90, 90);

            cameraNode.Rotation = new Quaternion(pitch, yaw, 0);

            if (Input.GetKeyDown(Key.W)) cameraNode.Translate(Vector3.UnitZ * moveSpeed * timeStep);
            if (Input.GetKeyDown(Key.S)) cameraNode.Translate(-Vector3.UnitZ * moveSpeed * timeStep);
            if (Input.GetKeyDown(Key.A)) cameraNode.Translate(-Vector3.UnitX * moveSpeed * timeStep);
            if (Input.GetKeyDown(Key.D)) cameraNode.Translate(Vector3.UnitX * moveSpeed * timeStep);
        }

        void SetupViewport()
        {
            Viewport viewport = new Viewport(scene, cameraNode.GetComponent<Camera>(), null);
            viewport.SetClearColor(Color.Black);
            Renderer.SetViewport(0, viewport);

            var rp = viewport.RenderPath.Clone();
            rp.Append(ResourceCache.GetXmlFile("PostProcess/BloomHDR.xml"));
            rp.Append(ResourceCache.GetXmlFile("PostProcess/FXAA2.xml"));
            //rp.Append(ResourceCache.GetXmlFile("PostProcess/GammaCorrection.xml"));

            viewport.RenderPath = rp;
        }
    }

    public class PBRMaterials : Sample
    {
        Scene scene;

        public PBRMaterials(ApplicationOptions options = null) : base(options) { }

        protected override void Start()
        {
            Application.UnhandledException += Application_UnhandledException;

            base.Start();

            // Create the scene content
            CreateScene();

            // Setup the viewport for displaying the scene
            SetupViewport();
        }

        void Application_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
        }

        void CreateScene()
        {
            scene = new Scene();

            // Load scene content prepared in the editor (XML format). GetFile() returns an open file from the resource system
            // which scene.LoadXML() will read
            scene.LoadXml(FileSystem.ProgramDir + "Data/Scenes/PBRExample.xml");

            // Create the camera (not included in the scene file)
            CameraNode = scene.CreateChild("Camera");
            CameraNode.CreateComponent<Camera>();

            // Set an initial position for the camera scene node above the plane
            CameraNode.Position = new Vector3(0.0f, 4.0f, 0.0f);
        }

        protected override void OnUpdate(float timeStep)
        {
            SimpleMoveCamera3D(timeStep);
        }

        void SetupViewport()
        {
            Viewport viewport = new Viewport(scene, CameraNode.GetComponent<Camera>(), null);
            Renderer.SetViewport(0, viewport);

            var effectRenderPath = viewport.RenderPath.Clone();
            effectRenderPath.Append(ResourceCache.GetXmlFile("PostProcess/BloomHDR.xml"));
            effectRenderPath.Append(ResourceCache.GetXmlFile("PostProcess/FXAA2.xml"));
            effectRenderPath.Append(ResourceCache.GetXmlFile("PostProcess/GammaCorrection.xml"));

            viewport.RenderPath = effectRenderPath;
        }
    }

    public class DynamicGeometry : Sample
    {
        Scene scene;
        float time;
        bool animate = true;
        uint[] vertexDuplicates;
        readonly List<Vector3> originalVertices = new List<Vector3>();
        readonly List<VertexBuffer> animatingBuffers = new List<VertexBuffer>();

        public DynamicGeometry(ApplicationOptions options = null) : base(options) { }

        protected override void Start()
        {
            base.Start();
            CreateScene();
            SimpleCreateInstructionsWithWasd("\nSpace to toggle animation");
            SetupViewport();
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
            SimpleMoveCamera3D(timeStep);
            if (Input.GetKeyPress(Key.Space))
                animate = !animate;

            if (animate)
                AnimateObjects(timeStep);
        }

        void AnimateObjects(float timeStep)
        {
            time += timeStep * 5.0f;

            // Repeat for each of the cloned vertex buffers
            for (int i = 0; i < animatingBuffers.Count; ++i)
            {
                float startPhase = time + i * 30.0f;
                VertexBuffer buffer = animatingBuffers[i];

                IntPtr vertexRawData = buffer.Lock(0, buffer.VertexCount, false);
                if (vertexRawData != IntPtr.Zero)
                {
                    uint numVertices = buffer.VertexCount;
                    uint vertexSize = buffer.VertexSize;
                    // Copy the original vertex positions
                    for (int j = 0; j < numVertices; ++j)
                    {
                        float phase = startPhase + vertexDuplicates[j] * 10.0f;
                        var src = originalVertices[j];

                    //    unsafe
                    //    {
                    //        //TODO: avoid unsafe
                    //        Vector3* dest = (Vector3*)IntPtr.Add(vertexRawData, j * (int)vertexSize);

                    //        dest->X = src.X * (1.0f + 0.1f * (float)Math.Sin(phase));
                    //        dest->Y = src.Y * (1.0f + 0.1f * (float)Math.Sin(phase + 60.0f));
                    //        dest->Z = src.Z * (1.0f + 0.1f * (float)Math.Sin(phase + 120.0f));
                    //    }
                    }
                    buffer.Unlock();
                }
            }
        }

        void SetupViewport()
        {
            var renderer = Renderer;
            renderer.SetViewport(0, new Viewport(Context, scene, CameraNode.GetComponent<Camera>(), null));
        }

        void CreateScene()
        {
            var cache = ResourceCache;

            scene = new Scene();

            // Create the Octree component to the scene so that drawable objects can be rendered. Use default volume
            // (-1000, -1000, -1000) to (1000, 1000, 1000)
            scene.CreateComponent<Octree>();

            // Create a Zone for ambient light & fog control
            Node zoneNode = scene.CreateChild("Zone");
            Zone zone = zoneNode.CreateComponent<Zone>();
            zone.SetBoundingBox(new BoundingBox(-1000.0f, 1000.0f));
            zone.FogColor = new Color(0.6f, 0.2f, 0.2f);
            zone.FogStart = 200.0f;
            zone.FogEnd = 300.0f;

            // Create a directional light
            Node lightNode = scene.CreateChild("DirectionalLight");
            lightNode.SetDirection(new Vector3(-0.6f, -1.0f, -0.8f)); // The direction vector does not need to be normalized
            Light light = lightNode.CreateComponent<Light>();
            light.LightType = LightType.Directional;
            light.Color = new Color(0.4f, 1.0f, 0.4f);
            light.SpecularIntensity = (1.5f);

            // Get the original model and its unmodified vertices, which are used as source data for the animation
            Model originalModel = cache.GetModel("Models/Jack.mdl");
            if (originalModel == null)
            {
                System.Diagnostics.Debug.WriteLine("Model not found, cannot initialize example scene");
                return;
            }
            // Get the vertex buffer from the first geometry's first LOD level
            VertexBuffer buffer = originalModel.GetGeometry(0, 0).GetVertexBuffer(0);
            IntPtr vertexRawData = buffer.Lock(0, buffer.VertexCount, false);

            if (vertexRawData != IntPtr.Zero)
            {
                uint numVertices = buffer.VertexCount;
                uint vertexSize = buffer.VertexSize;
                // Copy the original vertex positions
                for (int i = 0; i < numVertices; ++i)
                {
                    var src = (Vector3)System.Runtime.InteropServices.Marshal.PtrToStructure(IntPtr.Add(vertexRawData, i * (int)vertexSize), typeof(Vector3));
                    originalVertices.Add(src);
                }
                buffer.Unlock();

                // Detect duplicate vertices to allow seamless animation
                vertexDuplicates = new uint[originalVertices.Count];
                for (int i = 0; i < originalVertices.Count; ++i)
                {
                    vertexDuplicates[i] = (uint)i; // Assume not a duplicate
                    for (int j = 0; j < i; ++j)
                    {
                        if (originalVertices[i].Equals(originalVertices[j]))
                        {
                            vertexDuplicates[i] = (uint)j;
                            break;
                        }
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Failed to lock the model vertex buffer to get original vertices");
                return;
            }

            // Create StaticModels in the scene. Clone the model for each so that we can modify the vertex data individually
            for (int y = -1; y <= 1; ++y)
            {
                for (int x = -1; x <= 1; ++x)
                {
                    Node node = scene.CreateChild("Object");
                    node.Position = (new Vector3(x * 2.0f, 0.0f, y * 2.0f));
                    StaticModel sm = node.CreateComponent<StaticModel>();
                    Model cloneModel = originalModel.Clone();
                    sm.Model = (cloneModel);
                    // Store the cloned vertex buffer that we will modify when animating
                    animatingBuffers.Add(cloneModel.GetGeometry(0, 0).GetVertexBuffer(0));
                }
            }

            // Finally create one model (pyramid shape) and a StaticModel to display it from scratch
            // Note: there are duplicated vertices to enable face normals. We will calculate normals programmatically
            {
                const uint numVertices = 18;
                float[] vertexData =
                {
                // Position             Normal
                0.0f, 0.5f, 0.0f,       0.0f, 0.0f, 0.0f,
                0.5f, -0.5f, 0.5f,      0.0f, 0.0f, 0.0f,
                0.5f, -0.5f, -0.5f,     0.0f, 0.0f, 0.0f,

                0.0f, 0.5f, 0.0f,       0.0f, 0.0f, 0.0f,
                -0.5f, -0.5f, 0.5f,     0.0f, 0.0f, 0.0f,
                0.5f, -0.5f, 0.5f,      0.0f, 0.0f, 0.0f,

                0.0f, 0.5f, 0.0f,       0.0f, 0.0f, 0.0f,
                -0.5f, -0.5f, -0.5f,    0.0f, 0.0f, 0.0f,
                -0.5f, -0.5f, 0.5f,     0.0f, 0.0f, 0.0f,

                0.0f, 0.5f, 0.0f,       0.0f, 0.0f, 0.0f,
                0.5f, -0.5f, -0.5f,     0.0f, 0.0f, 0.0f,
                -0.5f, -0.5f, -0.5f,    0.0f, 0.0f, 0.0f,

                0.5f, -0.5f, -0.5f,     0.0f, 0.0f, 0.0f,
                0.5f, -0.5f, 0.5f,      0.0f, 0.0f, 0.0f,
                -0.5f, -0.5f, 0.5f,     0.0f, 0.0f, 0.0f,

                0.5f, -0.5f, -0.5f,     0.0f, 0.0f, 0.0f,
                -0.5f, -0.5f, 0.5f,     0.0f, 0.0f, 0.0f,
                -0.5f, -0.5f, -0.5f,    0.0f, 0.0f, 0.0f
            };

                short[] indexData =
                {
                0, 1, 2,
                3, 4, 5,
                6, 7, 8,
                9, 10, 11,
                12, 13, 14,
                15, 16, 17
            };

                Model fromScratchModel = new Model();
                VertexBuffer vb = new VertexBuffer(Context, false);
                IndexBuffer ib = new IndexBuffer(Context, false);
                var geom = new Urho.Geometry();

                // Shadowed buffer needed for raycasts to work, and so that data can be automatically restored on device loss
                vb.Shadowed = true;
                vb.SetSize(numVertices, ElementMask.Position | ElementMask.Normal, false);
                vb.SetData(vertexData);

                ib.Shadowed = true;
                ib.SetSize(numVertices, false, false);
                ib.SetData(indexData);

                geom.SetVertexBuffer(0, vb);
                geom.IndexBuffer = ib;
                geom.SetDrawRange(PrimitiveType.TriangleList, 0, numVertices, true);

                fromScratchModel.NumGeometries = 1;
                fromScratchModel.SetGeometry(0, 0, geom);
                fromScratchModel.BoundingBox = new BoundingBox(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));

                Node node = scene.CreateChild("FromScratchObject");
                node.Position = (new Vector3(0.0f, 3.0f, 0.0f));
                StaticModel sm = node.CreateComponent<StaticModel>();
                sm.Model = fromScratchModel;
            }

            // Create the camera
            CameraNode = new Node();
            CameraNode.Position = (new Vector3(0.0f, 2.0f, -20.0f));
            Camera camera = CameraNode.CreateComponent<Camera>();
            camera.FarClip = 300.0f;
        }
    }
*/

        /*
    private Ellipse el = new Ellipse() { Width=10, Height=10,Fill=new SolidColorBrush(Windows.UI.Colors.Green), HorizontalAlignment= Windows.UI.Xaml.HorizontalAlignment.Left, VerticalAlignment= Windows.UI.Xaml.VerticalAlignment.Top };
    private Ellipse el2 = new Ellipse() { Width = 10, Height = 10, Fill = new SolidColorBrush(Windows.UI.Colors.Blue), HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left, VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top };
    TextBox tb = new TextBox() { FontSize = 20, Padding = new Windows.UI.Xaml.Thickness(), TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap, AcceptsReturn = true  };

    private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        //StickerOverlayShape.CreateNew()
        //StickerAdorner s = new StickerAdorner()

//        Photo.UC.TextOverlayShape shape = new Photo.UC.TextOverlayShape();
//        var ad = shape.ParentContainer as Photo.UC.AdornerElementBaseUC;
        //ad._mousePosition = Mouse;
        //ad._elementPosition = Element;

//        this.controller.AddShape(shape);

        //Photo.UC.AdornerElementBaseUC temp = new Photo.UC.AdornerElementBaseUC(100,22, tb);

        //temp._mousePosition = Mouse;
        //temp._elementPosition = Element;
        //temp._scaleChanged = FontSized;

        //this._grid.Children.Add(temp);

        //if (!this._grid.Children.Contains(el))
        //{
        //    this._grid.Children.Add(el);
        //    this._grid.Children.Add(el2);
        //}
    }

    private void FontSized(double p)
    {
        tb.FontSize = 20 * p;
    }

    private void Mouse(Point p)
    {
        el.Margin = new Windows.UI.Xaml.Thickness(p.X, p.Y-90, 0, 0);
    }

    private void Element(Point p)
    {
        el2.Margin = new Windows.UI.Xaml.Thickness(p.X, p.Y-90, 0, 0);
    }*/
    }
}
