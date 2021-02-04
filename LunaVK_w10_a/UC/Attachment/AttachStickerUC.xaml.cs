using System;
using System.IO;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using LunaVK.Core.DataObjects;
using System.Threading.Tasks;
using LunaVK.Framework;
using LottieUWP;
using LunaVK.Core;
using LunaVK.Core.Framework;
using System.IO.IsolatedStorage;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas;
using System.Collections.Generic;

namespace LunaVK.UC.Attachment
{
    public sealed partial class AttachStickerUC : UserControl
    {
        DispatcherTimer timerOffset;
        LottieDrawable lottieDrawable;
        CanvasControl _canvasControl;
        DispatcherTimer _dispatcherTimer;

        public AttachStickerUC()
        {
            this.InitializeComponent();            

            

            this.Loaded += AttachStickerUC_Loaded;
            this.Unloaded += AttachStickerUC_Unloaded;
        }

        private VKSticker VM
        {
            get { return base.DataContext as VKSticker; }
        }

        private void AttachStickerUC_Loaded(object sender, RoutedEventArgs e)
        {
            if (Windows.System.Power.PowerManager.EnergySaverStatus == Windows.System.Power.EnergySaverStatus.On)
                return;

            if (Settings.AnimatedStickers && !string.IsNullOrEmpty(this.VM.animation_url) && this.VM.product_id>0)//без продукта это наши графити
            {
                this._canvasControl = new CanvasControl() { Width = 180, Height = 180 };
                this._canvasControl.Loaded += _canvasControl_Loaded;
                this._canvasControl.Draw += _canvasControl_Draw;
                this.grid.Children.Add(this._canvasControl);
            }
        }

        private void _canvasControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Load();
        }

        void AttachStickerUC_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.timerOffset != null)
            {
                this.timerOffset.Tick -= this.timerOffset_Tick;
                this.timerOffset.Stop();
                this.timerOffset = null;
            }

            if (this._dispatcherTimer != null)
            {
                this._dispatcherTimer.Tick -= this._dispatcherTimer_Tick;
                this._dispatcherTimer.Stop();
                this._dispatcherTimer = null;
            }

            if (this.lottieDrawable != null)
            {
                this.lottieDrawable.CancelAnimation();
                this.lottieDrawable.ClearComposition();
            }

            // Explicitly remove references to allow the Win2D controls to get garbage collected
            if (_canvasControl != null)
            {
                _canvasControl.RemoveFromVisualTree();
                _canvasControl = null;
            }
        }

        public AttachStickerUC(VKSticker sticker)
            : this()
        {
            this.DataContext = sticker;


            ImageExtensions.ImageFromCache3(sticker.photo_256, (s) =>
            {
                img.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(s));
            });
        }

        public async void Load()
        {
            /*
            VKSticker sticker = this.VM;
            string filePath = "Cache/Animated Stickers/" + sticker.sticker_id + ".json";
            
            using (Stream s = await CacheManager2.GetStreamOfCachedFile(filePath))
            {
                if (s == null)
                {
                    await CacheManager2.WriteToCache(new Uri(sticker.animation_url), filePath);
                }
                await this.lottieDrawable.SetAnimationFromUrlAsync(sticker.animation_url);

                var composition = LottieComposition.Factory.FromInputStreamSync(s);

                FrameRate = composition.FrameRate;

                lottieDrawable = new LottieDrawable();
                this.lottieDrawable.SetComposition(composition);

                _dispatcherTimer = new DispatcherTimer();
                _dispatcherTimer.Tick += _dispatcherTimer_Tick;
                _dispatcherTimer.Interval = this.GetTimerInterval;
            }
            */
            lottieDrawable = new LottieDrawable();
            await this.lottieDrawable.SetAnimationFromUrlAsync(this.VM.animation_url);

            if (lottieDrawable.Composition == null)
                return;//todo: нет сети

            FrameRate = lottieDrawable.Composition.FrameRate;

            this._dispatcherTimer = new DispatcherTimer();
            this._dispatcherTimer.Tick += _dispatcherTimer_Tick;
            this._dispatcherTimer.Interval = this.GetTimerInterval;

            this.timerOffset = new DispatcherTimer();
            this.timerOffset.Interval = TimeSpan.FromSeconds(2);
            this.timerOffset.Tick += timerOffset_Tick;

            this.timerOffset.Start();
        }


        void timerOffset_Tick(object sender, object e)
        {
            var ttv = this.TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));

            double ScreenH = (Window.Current.Content as Frame).ActualHeight;
            double y = screenCoords.Y + (base.ActualHeight / 2.0);
            double top = ScreenH * 0.2;
            double bottom = ScreenH * 0.8;

            if (y > top && y < ScreenH)
            {
                if (!this._dispatcherTimer.IsEnabled)
                {
                    this._dispatcherTimer.Start();
                    
                }
            }
            else
            {
                if (this._dispatcherTimer.IsEnabled)
                {
                    this.lottieDrawable.PauseAnimation();
                    this.lottieDrawable.Frame = this.lottieDrawable.MaxFrame;
                }
            }
        }

        private void Grid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            e.Handled = true;
            VKSticker sticker = this.DataContext as VKSticker;
            StickersPackViewUC.Show(sticker.sticker_id, "message");
        }







        private void _dispatcherTimer_Tick(object sender, object e)
        {
            if(this._canvasControl!=null)
                this._canvasControl.Invalidate();
        }

        public float FrameRate;

        private List<CanvasBitmap> CacheLottieFrames;

        private TimeSpan GetTimerInterval
        {
            get { return TimeSpan.FromTicks((long)Math.Floor(TimeSpan.TicksPerSecond / (FrameRate*2))); }
        }

        private int CurrentFrame = 0;

        private void _canvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (this.lottieDrawable == null || this.lottieDrawable.Composition==null)
                return;

            if (CacheLottieFrames != null && CurrentFrame < CacheLottieFrames.Count)
            {
                CanvasBitmap bitmap = this.CacheLottieFrames[CurrentFrame];
                args.DrawingSession.DrawImage(bitmap);

                CurrentFrame += 1;
                if (CurrentFrame >= CacheLottieFrames.Count)
                    CurrentFrame = 0;
            }
            else
            {
                this.lottieDrawable.Progress = CurrentFrame / this.lottieDrawable.Composition.Duration;

                CanvasBitmap effectImg = lottieDrawable.GetCurrentFrame(sender.Device, (float)this._canvasControl.ActualWidth, (float)this._canvasControl.ActualHeight);
                if (effectImg != null)
                {
                    args.DrawingSession.DrawImage(effectImg);
                    if (CacheLottieFrames == null)
                        CacheLottieFrames = new List<CanvasBitmap>();
                    CacheLottieFrames.Add(effectImg);
                }

                CurrentFrame += (int)FrameRate;
                if (CurrentFrame >= lottieDrawable.Composition.Duration)
                    CurrentFrame = 0;
                /*
                if (CurrentFrame == lottieDrawable.Composition.Duration)
                    CurrentFrame = 0;
                else if (CurrentFrame > lottieDrawable.Composition.Duration)
                    CurrentFrame = (int)lottieDrawable.Composition.Duration;
                else
                    CurrentFrame += (int)FrameRate;
                    */
            }

            if (this.img.Visibility == Visibility.Visible)
                this.img.Visibility = Visibility.Collapsed;
        }


        /*
         * protected override Size MeasureOverride(Size availableSize)
        {
            if (this.CacheLottieFrames != null)
            {
                this.CacheLottieFrames.Clear();
                this.CacheLottieFrames = null;
            }

            if (_canvasControl != null)
            {
                _canvasControl.Invalidate();
            }

            return base.MeasureOverride(availableSize);
        }
        */
    }
}
