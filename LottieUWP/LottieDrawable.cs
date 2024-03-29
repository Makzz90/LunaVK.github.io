using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using LottieUWP.Animation.Content;
using LottieUWP.Manager;
using LottieUWP.Model;
using LottieUWP.Model.Layer;
using LottieUWP.Parser;
using LottieUWP.Utils;
using LottieUWP.Value;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.Foundation;
using System.Threading.Tasks;
using System.Threading;

namespace LottieUWP
{
    /// <summary>
    /// This can be used to show an lottie animation in any place that would normally take a drawable.
    /// If there are masks or mattes, then you MUST call <seealso cref="RecycleBitmaps()"/> when you are done
    /// or else you will leak bitmaps.
    /// <para>
    /// It is preferable to use <seealso cref="LottieAnimationView"/> when possible because it
    /// handles bitmap recycling and asynchronous loading
    /// of compositions.
    /// </para>
    /// </summary>
    public class LottieDrawable : UserControl, ILottieDrawable, IAnimatable, IDisposable
    {
        private Matrix3X3 _matrix = Matrix3X3.CreateIdentity();
        private LottieComposition _composition;
        private readonly LottieValueAnimator _animator = new LottieValueAnimator();
        private float _scale = 1f;

        private readonly List<Action<LottieComposition>> _lazyCompositionTasks = new List<Action<LottieComposition>>();
        private ImageAssetManager _imageAssetManager;
        private IImageAssetDelegate _imageAssetDelegate;
        private FontAssetManager _fontAssetManager;
        private FontAssetDelegate _fontAssetDelegate;
        private bool _enableMergePaths;
        private CompositionLayer _compositionLayer;
        private byte _alpha = 255;
        private bool _performanceTrackingEnabled;
        private BitmapCanvas _bitmapCanvas;
        private CanvasAnimatedControl _canvasControl;
        private bool _forceSoftwareRenderer;

        public LottieDrawable()
        {
            _animator.Update += (sender, e) =>
            {
                if (_compositionLayer != null)
                {
                    _compositionLayer.Progress = _animator.AnimatedValueAbsolute;
                }
            };
            Loaded += UserControl_Loaded;
            Unloaded += UserControl_Unloaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _canvasControl = new CanvasAnimatedControl
            {
                ForceSoftwareRenderer = _forceSoftwareRenderer,
                //UseSharedDevice = true//��� �� �������� ��� ������
            };

            _canvasControl.Paused = true;
            _canvasControl.Draw += CanvasControlOnDraw;
            _canvasControl.Loaded += (s, args) => InvalidateMeasure();
            Content = _canvasControl;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Explicitly remove references to allow the Win2D controls to get garbage collected
            if (_canvasControl != null)
            {
                _canvasControl.RemoveFromVisualTree();
                _canvasControl = null;
            }

            if (_bitmapCanvas != null)
            {
                _bitmapCanvas.Dispose();
                _bitmapCanvas = null;
            }

            ClearComposition();
        }

        public void ForceSoftwareRenderer(bool force)
        {
            _forceSoftwareRenderer = force;
            if (_canvasControl != null)
            {
                _canvasControl.ForceSoftwareRenderer = force;
            }
        }

        /// <summary>
        /// Returns whether or not any layers in this composition has masks.
        /// </summary>
        public bool HasMasks()
        {
            return _compositionLayer != null && _compositionLayer.HasMasks();
        }

        /// <summary>
        /// Returns whether or not any layers in this composition has a matte layer.
        /// </summary>
        public bool HasMatte()
        {
            return _compositionLayer != null && _compositionLayer.HasMatte();
        }

        public bool EnableMergePaths()
        {
            return _enableMergePaths;
        }

        /// <summary>
        /// Enable this to get merge path support.
        /// 
        /// Merge paths currently don't work if the the operand shape is entirely contained within the
        /// first shape. If you need to cut out one shape from another shape, use an even-odd fill type
        /// instead of using merge paths.
        /// </summary>
        public void EnableMergePaths(bool enable)
        {
            if (_enableMergePaths == enable)
            {
                return;
            }
            _enableMergePaths = enable;
            if (_composition != null)
            {
                BuildCompositionLayer();
            }
        }

        public bool IsMergePathsEnabled()
        {
            return _enableMergePaths;
        }

        /// <summary>
        /// If you use image assets, you must explicitly specify the folder in assets/ in which they are
        /// located because bodymovin uses the name filenames across all compositions (img_#).
        /// Do NOT rename the images themselves.
        /// 
        /// If your images are located in src/main/assets/airbnb_loader/ then call
        /// `setImageAssetsFolder("airbnb_loader/");`.
        /// 
        /// 
        /// If you use LottieDrawable directly, you MUST call <seealso cref="RecycleBitmaps()"/> when you
        /// are done. Calling <seealso cref="RecycleBitmaps()"/> doesn't have to be final and <seealso cref="LottieDrawable"/>
        /// will recreate the bitmaps if needed but they will leak if you don't recycle them.
        /// 
        /// Be wary if you are using many images, however. Lottie is designed to work with vector shapes 
        /// from After Effects. If your images look like they could be represented with vector shapes, 
        /// see if it is possible to convert them to shape layers and re-export your animation. Check 
        /// the documentation at http://airbnb.io/lottie for more information about importing shapes from 
        /// Sketch or Illustrator to avoid this. 
        /// </summary>
        public string ImageAssetsFolder { get; set; }

        /// <summary>
        /// If you have image assets and use <seealso cref="LottieDrawable"/> directly, you must call this yourself.
        /// 
        /// Calling recycleBitmaps() doesn't have to be final and <seealso cref="LottieDrawable"/>
        /// will recreate the bitmaps if needed but they will leak if you don't recycle them.
        /// 
        /// </summary>
        public void RecycleBitmaps()
        {
            _imageAssetManager?.RecycleBitmaps();
        }

        /// <summary>
        /// Create a composition with <see cref="LottieCompositionFactory"/>
        /// </summary>
        /// <param name="composition">The new composition.</param>
        /// <returns>True if the composition is different from the previously set composition, false otherwise.</returns>
        public bool SetComposition(LottieComposition composition)
        {
            //if (Callback == null) // TODO: needed?
            //{
            //    throw new System.InvalidOperationException("You or your view must set a Drawable.Callback before setting the composition. This " + "gets done automatically when added to an ImageView. " + "Either call ImageView.setImageDrawable() before setComposition() or call " + "setCallback(yourView.getCallback()) first.");
            //}

            if (_composition == composition)
            {
                return false;
            }

            lock (this)
            {
                ClearComposition();
                _composition = composition;
                BuildCompositionLayer();
                _animator.Composition = composition;
                Progress = _animator.AnimatedFraction;
                Scale = _scale;
                UpdateBounds();

                // We copy the tasks to a new ArrayList so that if this method is called from multiple threads, 
                // then there won't be two iterators iterating and removing at the same time. 
                foreach (var t in _lazyCompositionTasks.ToList())
                {
                    t.Invoke(composition);
                }
                _lazyCompositionTasks.Clear();
                composition.PerformanceTrackingEnabled = _performanceTrackingEnabled;
            }

            return true;
        }

        public bool PerformanceTrackingEnabled
        {
            set
            {
                _performanceTrackingEnabled = value;
                if (_composition != null)
                {
                    _composition.PerformanceTrackingEnabled = value;
                }
            }
        }

        public PerformanceTracker PerformanceTracker => _composition?.PerformanceTracker;

        private void BuildCompositionLayer()
        {
            _compositionLayer = new CompositionLayer(this, LayerParser.Parse(_composition), _composition.Layers, _composition);
        }

        public void ClearComposition()
        {
            RecycleBitmaps();
            if (_animator.IsRunning)
            {
                _animator.Cancel();
            }

            lock (this)
            {
                _composition = null;
            }

            _compositionLayer = null;
            _imageAssetManager = null;
            _animator.ClearComposition();
            InvalidateSelf();
        }

        public void InvalidateSelf()
        {
            _canvasControl?.Invalidate();
        }

        public void SetAlpha(byte alpha)
        {
            _alpha = alpha;
        }

        public int GetAlpha()
        {
            return _alpha;
        }

        //public int Opacity
        //{
        //    get
        //    {
        //        return PixelFormat.TRANSLUCENT;
        //    }
        //}

        private void CanvasControlOnDraw(ICanvasAnimatedControl canvasControl, CanvasAnimatedDrawEventArgs args)
        {
            lock (this)
            {
                if (_bitmapCanvas == null)
                {
                    return;
                }

                Draw(canvasControl.Device, _bitmapCanvas, _compositionLayer, _composition?.Bounds ?? default(Rect), _scale, _alpha, _matrix, canvasControl.Size.Width, canvasControl.Size.Height, args.DrawingSession);
            }
        }

        private static void Draw(CanvasDevice device, BitmapCanvas bitmapCanvas, CompositionLayer compositionLayer, Rect bounds, float scale, byte alpha, Matrix3X3 matrix, double width, double height, CanvasDrawingSession canvasDrawingSession)
        {
            using (bitmapCanvas.CreateSession(device, width, height, canvasDrawingSession))
            {
                bitmapCanvas.Clear(Colors.Transparent);
                LottieLog.BeginSection("Drawable.Draw");
                if (compositionLayer == null)
                {
                    return;
                }

                var localScale = scale;
                float extraScale = 1f;

                float maxScale = GetMaxScale(bitmapCanvas, bounds);
                if (localScale > maxScale)
                {
                    localScale = maxScale;
                    extraScale = scale / localScale;
                }

                if (extraScale > 1)
                {
                    // This is a bit tricky... 
                    // We can't draw on a canvas larger than ViewConfiguration.get(context).getScaledMaximumDrawingCacheSize() 
                    // which works out to be roughly the size of the screen because Android can't generate a 
                    // bitmap large enough to render to. 
                    // As a result, we cap the scale such that it will never be wider/taller than the screen 
                    // and then only render in the top left corner of the canvas. We then use extraScale 
                    // to scale up the rest of the scale. However, since we rendered the animation to the top 
                    // left corner, we need to scale up and translate the canvas to zoom in on the top left 
                    // corner. 
                    bitmapCanvas.Save();
                    float halfWidth = (float)bounds.Width / 2f;
                    float halfHeight = (float)bounds.Height / 2f;
                    float scaledHalfWidth = halfWidth * localScale;
                    float scaledHalfHeight = halfHeight * localScale;
                    bitmapCanvas.Translate(
                        scale * halfWidth - scaledHalfWidth,
                        scale * halfHeight - scaledHalfHeight);
                    bitmapCanvas.Scale(extraScale, extraScale, scaledHalfWidth, scaledHalfHeight);
                }

                matrix.Reset();
                matrix = MatrixExt.PreScale(matrix, localScale, localScale);
                compositionLayer.Draw(bitmapCanvas, matrix, alpha);
                LottieLog.EndSection("Drawable.Draw");

                if (extraScale > 1)
                {
                    bitmapCanvas.Restore();
                }
            }
        }

        public void Start()
        {
            PlayAnimation();
        }

        public void Stop()
        {
            EndAnimation();
        }

        public bool IsRunning => IsAnimating;

        /// <summary>
        /// Plays the animation from the beginning. If speed is &lt; 0, it will start at the end
        /// and play towards the beginning
        /// </summary>
        public void PlayAnimation()
        {
            if (_compositionLayer == null)
            {
                _lazyCompositionTasks.Add(c =>
                {
                    PlayAnimation();
                });
                return;
            }
            _animator.PlayAnimation();
        }

        public void EndAnimation()
        {
            _lazyCompositionTasks.Clear();
            _animator.EndAnimation();
        }

        /// <summary>
        /// Continues playing the animation from its current position. If speed &lt; 0, it will play backwards 
        /// from the current position.
        /// </summary>
        public void ResumeAnimation()
        {
            if (_compositionLayer == null)
            {
                _lazyCompositionTasks.Add(c =>
                {
                    ResumeAnimation();
                });
                return;
            }
            _animator.ResumeAnimation();
        }

        /// <summary>
        /// Gets or sets the minimum frame that the animation will start from when playing or looping.
        /// </summary>
        public float MinFrame
        {
            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(c => MinFrame = value);
                    return;
                }
                _animator.MinFrame = value;
            }

            get => _animator.MinFrame;
        }

        /// <summary>
        /// Sets the minimum progress that the animation will start from when playing or looping. 
        /// </summary>
        public float MinProgress
        {
            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(c =>
                    {
                        MinProgress = value;
                    });
                    return;
                }
                MinFrame = MiscUtils.Lerp(_composition.StartFrame, _composition.EndFrame, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum frame that the animation will end at when playing or looping.
        /// </summary>
        public float MaxFrame
        {
            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(c => MaxFrame = value);
                    return;
                }
                _animator.MaxFrame = value;
            }

            get => _animator.MaxFrame;
        }

        /// <summary>
        /// Sets the maximum progress that the animation will end at when playing or looping.
        /// </summary>
        public float MaxProgress
        {
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 1)
                    value = 1;

                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(c =>
                    {
                        MaxProgress = value;
                    });
                    return;
                }
                MaxFrame = MiscUtils.Lerp(_composition.StartFrame, _composition.EndFrame, value);
            }
        }

        /// <summary>
        /// <see cref="MinFrame"/>
        /// <see cref="MaxFrame"/>
        /// </summary>
        /// <param name="minFrame"></param>
        /// <param name="maxFrame"></param>
        public void SetMinAndMaxFrame(float minFrame, float maxFrame)
        {
            if (_composition == null)
            {
                _lazyCompositionTasks.Add(c => SetMinAndMaxFrame(minFrame, maxFrame));
                return;
            }
            _animator.SetMinAndMaxFrames(minFrame, maxFrame);
        }

        /// <summary>
        /// <see cref="MinProgress"/>
        /// <see cref="MaxProgress"/>
        /// </summary>
        /// <param name="minProgress"></param>
        /// <param name="maxProgress"></param>
        public void SetMinAndMaxProgress(float minProgress, float maxProgress)
        {
            if (minProgress < 0)
                minProgress = 0;
            if (minProgress > 1)
                minProgress = 1;
            if (maxProgress < 0)
                maxProgress = 0;
            if (maxProgress > 1)
                maxProgress = 1;

            if (_composition == null)
            {
                _lazyCompositionTasks.Add(c =>
                {
                    SetMinAndMaxProgress(minProgress, maxProgress);
                });
                return;
            }
            SetMinAndMaxFrame((int)MiscUtils.Lerp(_composition.StartFrame, _composition.EndFrame, minProgress),
                (int)MiscUtils.Lerp(_composition.StartFrame, _composition.EndFrame, maxProgress));
        }

        /// <summary>
        /// Reverses the current animation speed. This does NOT play the animation.
        /// <see cref="Speed"/>
        /// <see cref="PlayAnimation"/>
        /// <see cref="ResumeAnimation"/>
        /// </summary>
        public void ReverseAnimationSpeed()
        {
            _animator.ReverseAnimationSpeed();
        }

        /// <summary>
        /// Sets the playback speed. If speed &lt; 0, the animation will play backwards.
        /// Returns the current playback speed. This will be &lt; 0 if the animation is playing backwards.
        /// </summary>
        public float Speed
        {
            set => _animator.Speed = value;
            get => _animator.Speed;
        }

        public event EventHandler<ValueAnimator.ValueAnimatorUpdateEventArgs> AnimatorUpdate
        {
            add => _animator.Update += value;
            remove => _animator.Update -= value;
        }

        public void RemoveAllUpdateListeners()
        {
            _animator.RemoveAllUpdateListeners();
        }

        public event EventHandler ValueChanged
        {
            add => _animator.ValueChanged += value;
            remove => _animator.ValueChanged -= value;
        }

        public void RemoveAllAnimatorListeners()
        {
            _animator.RemoveAllListeners();
        }

        public float Frame
        {
            /**
            * Sets the progress to the specified frame.
            * If the composition isn't set yet, the progress will be set to the frame when
            * it is.
            */
            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(c =>
                    {
                        Frame = value;
                    });
                    return;
                }

                _animator.Frame = value;
            }
            /**
            * Get the currently rendered frame.
            */
            get => _animator.Frame;
        }

        public float Progress
        {
            get => _animator.AnimatedValueAbsolute;
            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(c =>
                    {
                        Progress = value;
                    });
                    return;
                }
                Frame = MiscUtils.Lerp(_composition.StartFrame, _composition.EndFrame, value);
            }
        }

        /// <summary>
        /// <see cref="RepeatCount"/>
        /// </summary>
        [Obsolete("Loop property is Obsolete, and will be removed on a future version. Use RepeatCount instead.")]
        public bool Looping
        {
            get => _animator.RepeatCount == LottieValueAnimator.Infinite;
            set => _animator.RepeatCount = value ? LottieValueAnimator.Infinite : 0;
        }

        /// <summary>
        /// Defines what this animation should do when it reaches the end. This
        /// setting is applied only when the repeat count is either greater than
        /// 0 or <see cref="LottieValueAnimator.Infinite"/>. Defaults to <see cref="LottieUWP.RepeatMode.Restart"/>.
        /// Return either one of <see cref="LottieUWP.RepeatMode.Reverse"/> or <see cref="LottieUWP.RepeatMode.Restart"/>
        /// </summary>
        /// <param name="value"><seealso cref="RepeatMode"/></param>
        public RepeatMode RepeatMode
        {
            set => _animator.RepeatMode = value;
            get => _animator.RepeatMode;
        }

        /// <summary>
        /// Sets how many times the animation should be repeated. If the repeat 
        /// count is 0, the animation is never repeated. If the repeat count is 
        /// greater than 0 or <see cref="LottieValueAnimator.Infinite"/>, the repeat mode will be taken 
        /// into account. The repeat count is 0 by default.
        /// 
        /// Count the number of times the animation should be repeated
        /// 
        /// Return the number of times the animation should repeat, or <see cref="LottieValueAnimator.Infinite"/>
        /// </summary>
        public int RepeatCount
        {
            set => _animator.RepeatCount = value;
            get => _animator.RepeatCount;
        }

        public float FrameRate
        {
            get => _animator.FrameRate;
            set => _animator.FrameRate = value;
        }

        public bool IsAnimating => _animator.IsRunning;

        /// <summary>
        /// Use this to manually set fonts. 
        /// </summary>
        public FontAssetDelegate FontAssetDelegate
        {
            set
            {
                _fontAssetDelegate = value;
                if (_fontAssetManager != null)
                {
                    _fontAssetManager.Delegate = value;
                }
            }
        }

        public TextDelegate TextDelegate { set; get; }

        public bool UseTextGlyphs()
        {
            return TextDelegate == null && _composition.Characters.Count > 0;
        }

        /// <summary>
        /// Set the scale on the current composition. The only cost of this function is re-rendering the
        /// current frame so you may call it frequent to scale something up or down.
        /// 
        /// The smaller the animation is, the better the performance will be. You may find that scaling an
        /// animation down then rendering it in a larger ImageView and letting ImageView scale it back up
        /// with a scaleType such as centerInside will yield better performance with little perceivable
        /// quality loss.
        /// 
        /// You can also use a fixed view width/height in conjunction with the normal ImageView 
        /// scaleTypes centerCrop and centerInside. 
        /// </summary>
        public float Scale
        {
            set
            {
                _scale = value;
                lock (this)
                {
                    UpdateBounds();
                    InvalidateMeasure();
                    InvalidateSelf();
                }
            }
            get => _scale;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            InvalidateSelf();
            return base.MeasureOverride(availableSize);
        }

        /// <summary>
        /// Use this if you can't bundle images with your app. This may be useful if you download the
        /// animations from the network or have the images saved to an SD Card. In that case, Lottie
        /// will defer the loading of the bitmap to this delegate.
        /// 
        /// Be wary if you are using many images, however. Lottie is designed to work with vector shapes 
        /// from After Effects. If your images look like they could be represented with vector shapes, 
        /// see if it is possible to convert them to shape layers and re-export your animation. Check 
        /// the documentation at http://airbnb.io/lottie for more information about importing shapes from 
        /// Sketch or Illustrator to avoid this. 
        /// </summary>
        public IImageAssetDelegate ImageAssetDelegate
        {
            set
            {
                _imageAssetDelegate = value;
                if (_imageAssetManager != null)
                {
                    _imageAssetManager.Delegate = value;
                }
            }
        }

        public LottieComposition Composition => _composition;

        private void UpdateBounds()
        {
            if (_composition == null)
            {
                return;
            }
            Width = (int)(_composition.Bounds.Width * _scale);
            Height = (int)(_composition.Bounds.Height * _scale);
            _bitmapCanvas?.Dispose();
            _bitmapCanvas = new BitmapCanvas(Width, Height);
        }

        public void CancelAnimation()
        {
            _lazyCompositionTasks.Clear();
            _animator.Cancel();
        }

        public void PauseAnimation()
        {
            _lazyCompositionTasks.Clear();
            _animator.PauseAnimation();
        }

        public int IntrinsicWidth => _composition == null ? -1 : (int)(_composition.Bounds.Width * _scale);

        public int IntrinsicHeight => _composition == null ? -1 : (int)(_composition.Bounds.Height * _scale);

        /// <summary>
        /// Takes a <see cref="KeyPath"/>, potentially with wildcards or globstars and resolve it to a list of 
        /// zero or more actual <see cref="KeyPath"/>s
        /// that exist in the current animation.
        /// 
        /// If you want to set value callbacks for any of these values, it is recommended to use the 
        /// returned <see cref="KeyPath"/> objects because they will be internally resolved to their content 
        /// and won't trigger a tree walk of the animation contents when applied. 
        /// </summary>
        /// <param name="keyPath"></param>
        /// <returns></returns>
        public List<KeyPath> ResolveKeyPath(KeyPath keyPath)
        {
            if (_compositionLayer == null)
            {
                Debug.WriteLine("Cannot resolve KeyPath. Composition is not set yet.", LottieLog.Tag);
                return new List<KeyPath>();
            }
            var keyPaths = new List<KeyPath>();
            _compositionLayer.ResolveKeyPath(keyPath, 0, keyPaths, new KeyPath());
            return keyPaths;
        }

        /// <summary>
        /// Add a property callback for the specified <see cref="KeyPath"/>. This <see cref="KeyPath"/> can resolve 
        /// to multiple contents. In that case, the callback's value will apply to all of them. 
        /// 
        /// Internally, this will check if the <see cref="KeyPath"/> has already been resolved with 
        /// <see cref="ResolveKeyPath"/> and will resolve it if it hasn't. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyPath"></param>
        /// <param name="property"></param>
        /// <param name="callback"></param>
        public void AddValueCallback<T>(KeyPath keyPath, LottieProperty property, ILottieValueCallback<T> callback)
        {
            if (_compositionLayer == null)
            {
                _lazyCompositionTasks.Add(c =>
                {
                    AddValueCallback(keyPath, property, callback);
                });
                return;
            }
            bool invalidate;
            if (keyPath.GetResolvedElement() != null)
            {
                keyPath.GetResolvedElement().AddValueCallback(property, callback);
                invalidate = true;
            }
            else
            {
                List<KeyPath> elements = ResolveKeyPath(keyPath);

                for (int i = 0; i < elements.Count; i++)
                {
                    elements[i].GetResolvedElement().AddValueCallback(property, callback);
                }
                invalidate = elements.Any();
            }
            if (invalidate)
            {
                InvalidateSelf();
                if (property == LottieProperty.TimeRemap)
                {
                    // Time remapping values are read in setProgress. In order for the new value 
                    // to apply, we have to re-set the progress with the current progress so that the 
                    // time remapping can be reapplied. 
                    Progress = Progress;
                }
            }
        }

        /// <summary>
        /// Overload of <see cref="AddValueCallback{T}(KeyPath, LottieProperty, ILottieValueCallback{T})"/> that takes an interface. This allows you to use a single abstract 
        /// method code block in Kotlin such as: 
        /// drawable.AddValueCallback(yourKeyPath, LottieProperty.Color) { yourColor } 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyPath"></param>
        /// <param name="property"></param>
        /// <param name="callback"></param>
        public void AddValueCallback<T>(KeyPath keyPath, LottieProperty property, SimpleLottieValueCallback<T> callback)
        {
            AddValueCallback(keyPath, property, new SimpleImplLottieValueCallback<T>(callback));
        }

        /// <summary>
        /// Allows you to modify or clear a bitmap that was loaded for an image either automatically
        /// 
        /// through <seealso cref="ImageAssetsFolder"/> or with an <seealso cref="ImageAssetDelegate"/>.
        /// 
        /// 
        /// </summary>
        /// <returns> the previous Bitmap or null.
        ///  </returns>
        public CanvasBitmap UpdateBitmap(string id, CanvasBitmap bitmap)
        {
            var bm = ImageAssetManager;
            if (bm == null)
            {
                Debug.WriteLine("Cannot update bitmap. Most likely the drawable is not added to a View " + "which prevents Lottie from getting a Context.", LottieLog.Tag);
                return null;
            }
            var ret = bm.UpdateBitmap(id, bitmap);
            InvalidateSelf();
            return ret;
        }

        public CanvasBitmap GetImageAsset(string id)
        {
            return ImageAssetManager?.BitmapForId(Device, id);
        }

        private ImageAssetManager ImageAssetManager
        {
            get
            {
                if (_imageAssetManager != null && !_imageAssetManager.HasSameContext(Device))
                {
                    _imageAssetManager.RecycleBitmaps();
                    _imageAssetManager = null;
                }

                if (_imageAssetManager == null)
                {
                    var clonedDict = new Dictionary<string, LottieImageAsset>();
                    foreach (var entry in _composition.Images)
                    {
                        clonedDict.Add(entry.Key, entry.Value);
                    }
                    _imageAssetManager = new ImageAssetManager(ImageAssetsFolder, _imageAssetDelegate, clonedDict, Device);
                }

                return _imageAssetManager;
            }
        }

        public CanvasDevice Device => _canvasControl?.Device;

        public Typeface GetTypeface(string fontFamily, string style)
        {
            var assetManager = FontAssetManager;
            return assetManager?.GetTypeface(fontFamily, style);
        }

        private FontAssetManager FontAssetManager => _fontAssetManager ??
            (_fontAssetManager = new FontAssetManager(_fontAssetDelegate));

        /**
        * If there are masks or mattes, we can't scale the animation larger than the canvas or else 
        * the off screen rendering for masks and mattes after saveLayer calls will get clipped. 
        */
        private static float GetMaxScale(BitmapCanvas canvas, Rect bounds)
        {
            var maxScaleX = (float)canvas.Width / (float)bounds.Width;
            var maxScaleY = (float)canvas.Height / (float)bounds.Height;
            return Math.Min(maxScaleX, maxScaleY);
        }

        private void Dispose(bool disposing)
        {
            _animator.Dispose();
            _imageAssetManager?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~LottieDrawable()
        {
            Dispose(false);
        }

        private class ColorFilterData
        {
            internal readonly string LayerName;
            internal readonly string ContentName;
            internal readonly ColorFilter ColorFilter;

            internal ColorFilterData(string layerName, string contentName, ColorFilter colorFilter)
            {
                LayerName = layerName;
                ContentName = contentName;
                ColorFilter = colorFilter;
            }

            public override int GetHashCode()
            {
                var hashCode = 17;
                if (!string.IsNullOrEmpty(LayerName))
                {
                    hashCode = hashCode * 31 * LayerName.GetHashCode();
                }

                if (!string.IsNullOrEmpty(ContentName))
                {
                    hashCode = hashCode * 31 * ContentName.GetHashCode();
                }
                return hashCode;
            }

            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }

                if (!(obj is ColorFilterData))
                {
                    return false;
                }

                var other = (ColorFilterData)obj;

                return GetHashCode() == other.GetHashCode() && ColorFilter == other.ColorFilter;
            }
        }


















        /// <summary>
        /// Gets the current progress
        /// </summary>
        /// <param name="resourceCreator"></param>
        /// <param name = "scaleX"> lateral zoom ratio </ param>
        /// <param name = "scaleY"> vertical zoom ratio </ param>
        /// <returns></returns>
        public CanvasBitmap GetCurrentFrame(ICanvasResourceCreator resourceCreator, float width, float height)
        {
            lock (this)
            {
                var commandList = new CanvasRenderTarget(resourceCreator, width, height, Utils.Utils.Dpi());
                using (CanvasDrawingSession session = commandList.CreateDrawingSession())
                {
                    if (_bitmapCanvas == null || _bitmapCanvas.Width < width || _bitmapCanvas.Height < height)
                    {
                        _bitmapCanvas?.Dispose();
                        _bitmapCanvas = new BitmapCanvas(width, height);
                    }

                    using (_bitmapCanvas.CreateSession(resourceCreator.Device, width, height, session))
                    {
                        _bitmapCanvas.Clear(Colors.Transparent);
                        //LottieLog.BeginSection("Drawable.Draw");
                        if (_compositionLayer == null)
                        {
                            return null;
                        }

                        _matrix.Reset();

                        float scaleX = width / (float)_composition.Bounds.Width;
                        float scaleY = height / (float)_composition.Bounds.Height;
                        _matrix = MatrixExt.PreScale(_matrix, scaleX, scaleY);
                        _compositionLayer.Draw(_bitmapCanvas, _matrix, _alpha);
                        //LottieLog.EndSection("Drawable.Draw");
                    }

                }

                return commandList;
            }
        }

        public async Task SetAnimationFromUrlAsync(string url, CancellationTokenSource compositionTaskCTS = null)
        {
            if (compositionTaskCTS == null)
                compositionTaskCTS = new CancellationTokenSource();
            var compositionResult = await LottieCompositionFactory.FromUrlAsync(Device, url, compositionTaskCTS.Token);
            

            compositionTaskCTS = null;
            if (compositionResult.Value != null)
            {
                this.SetComposition(compositionResult.Value);
            }
        }
    }
}
