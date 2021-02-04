using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace LunaVK.Framework
{
    public class StoryboardHelper : DependencyObject
    {
        public static bool GetBeginIf(DependencyObject obj)
        {
            return (bool)obj.GetValue(BeginIfProperty);
        }

        public static void SetBeginIf(DependencyObject obj, bool value)
        {
            obj.SetValue(BeginIfProperty, value);
        }

        public static readonly DependencyProperty BeginIfProperty = DependencyProperty.RegisterAttached("BeginIf", typeof(bool), typeof(StoryboardHelper), new PropertyMetadata(false, BeginIfPropertyChangedCallback));

        private static void BeginIfPropertyChangedCallback(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var storyboard = s as Storyboard;
            if (storyboard == null)
                throw new InvalidOperationException("This attached property only supports Storyboards.");

            var begin = (bool)e.NewValue;
            if (begin) storyboard.Begin();
            else storyboard.Stop();
        }
    }
}
