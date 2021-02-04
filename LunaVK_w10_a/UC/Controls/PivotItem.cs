using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace LunaVK.UC.Controls
{
    public class PivotItem : ContentControl
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(PivotItem), new PropertyMetadata(null, HeaderPropertyCallback));

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        private static void HeaderPropertyCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null)
                return;
            
            if (args.NewValue == args.OldValue)
                return;
        }
    }
}
