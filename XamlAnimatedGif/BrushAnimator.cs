using System;
using System.IO;
using System.Threading.Tasks;
#if WPF || SILVERLIGHT
using System.Windows.Media;
using System.Windows.Media.Animation;
#elif WINRT || WINDOWS_PHONE_APP
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
#endif
using XamlAnimatedGif.Decoding;

namespace XamlAnimatedGif
{
    public class BrushAnimator : Animator
    {
        private BrushAnimator(Stream sourceStream, Uri sourceUri, GifDataStream metadata, RepeatBehavior repeatBehavior) : base(sourceStream, sourceUri, metadata, repeatBehavior)
        {
            Brush = new ImageBrush {ImageSource = Bitmap};
            RepeatBehavior = _repeatBehavior;
        }

        protected override RepeatBehavior GetSpecifiedRepeatBehavior() { return this.RepeatBehavior; }

        protected override object ErrorSource {get{return this.Brush;}}

        public ImageBrush Brush { get;private set; }

        private RepeatBehavior _repeatBehavior;
        public RepeatBehavior RepeatBehavior
        {
            get { return _repeatBehavior; }
            set
            {
                _repeatBehavior = value;
                OnRepeatBehaviorChanged();
            }
        }

        public static Task<BrushAnimator> CreateAsync(Uri sourceUri, RepeatBehavior repeatBehavior, IProgress<int> progress = null)
        {
            return CreateAsyncCore(
                sourceUri,
                progress,
                (stream, metadata) => new BrushAnimator(stream, sourceUri, metadata, repeatBehavior));
        }

        public static Task<BrushAnimator> CreateAsync(Stream sourceStream, RepeatBehavior repeatBehavior)
        {
            return CreateAsyncCore(
                sourceStream,
                metadata => new BrushAnimator(sourceStream, null, metadata, repeatBehavior));
        }
    }
}