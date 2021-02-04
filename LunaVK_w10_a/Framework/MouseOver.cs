using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Core;

namespace LunaVK.Framework
{
    public static class MouseOver
    {
        public static readonly DependencyProperty CursorProperty = DependencyProperty.RegisterAttached("Cursor", typeof(CoreCursorType), typeof(MouseOver), new PropertyMetadata(1.0, new PropertyChangedCallback(OnCursorChanged)));

        public static CoreCursorType GetCursor(DependencyObject obj)
        {
            return (CoreCursorType)obj.GetValue(CursorProperty);
        }

        public static void SetCursor(DependencyObject obj, CoreCursorType value)
        {
            obj.SetValue(CursorProperty, value);
        }

        private static void OnCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement element = (FrameworkElement)d;
            
            element.PointerEntered += OnPointerEntered;
            element.PointerExited += OnPointerExited;
            element.PointerCaptureLost += OnPointerExited;
            element.Unloaded += Element_Unloaded;
        }

        private static void Element_Unloaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            
            element.PointerEntered -= OnPointerEntered;
            element.PointerExited -= OnPointerExited;
            element.PointerCaptureLost -= OnPointerExited;
            element.Unloaded -= Element_Unloaded;

            OnPointerExited(sender, null);
        }

        static void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
           Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }

        static void OnPointerEntered(object sender, PointerRoutedEventArgs args)
        {
            CoreCursorType cursor = MouseOver.GetCursor(sender as DependencyObject);
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(cursor, 1);
        }
    }
}
