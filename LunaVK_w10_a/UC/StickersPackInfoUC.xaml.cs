using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.UC
{
    public sealed partial class StickersPackInfoUC : UserControl
    {
        public StickersPackInfoUC()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty NewIndicatorEnabledProperty = DependencyProperty.Register("NewIndicatorEnabled", typeof(bool), typeof(StickersPackInfoUC), new PropertyMetadata(true, new PropertyChangedCallback(StickersPackInfoUC.NewIndicatorEnabled_OnChanged)));
        public bool NewIndicatorEnabled
        {
            get
            {
                return (bool)base.GetValue(StickersPackInfoUC.NewIndicatorEnabledProperty);
            }
            set
            {
                base.SetValue(StickersPackInfoUC.NewIndicatorEnabledProperty, value);
            }
        }

        private static void NewIndicatorEnabled_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (((StickersPackInfoUC)d).borderNewIndicator).Visibility = (((bool)e.NewValue).ToVisiblity());
        }
    }
}
