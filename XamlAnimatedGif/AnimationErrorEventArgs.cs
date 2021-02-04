using System;
#if WPF || SILVERLIGHT
using System.Windows;
#elif WINRT || WINDOWS_PHONE_APP
using Windows.UI.Xaml;
#endif

namespace XamlAnimatedGif
{
#if WPF
    public delegate void AnimationErrorEventHandler(DependencyObject d, AnimationErrorEventArgs e);

    public class AnimationErrorEventArgs : RoutedEventArgs
    {
        public AnimationErrorEventArgs(object source, Exception exception, AnimationErrorKind kind)
            : base(AnimationBehavior.ErrorEvent, source)
        {
#elif WINRT || WINDOWS_PHONE_APP || SILVERLIGHT
    public class AnimationErrorEventArgs : EventArgs
    {
        public AnimationErrorEventArgs(object source, Exception exception, AnimationErrorKind kind)
        {
            Source = source;
#endif
            Exception = exception;
            Kind = kind;
        }

        public Exception Exception { get; private set; }

        public AnimationErrorKind Kind { get; private set; }

#if WINRT || SILVERLIGHT || WINDOWS_PHONE_APP
        public object Source { get; private set; }
#endif
    }

    public enum AnimationErrorKind
    {
        Loading,
        Rendering
    }
}
