using LunaVK.Core.DataObjects;
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

namespace LunaVK.Pages.Debug
{
    public sealed partial class TestDownload : Page
    {
        public TestDownload()
        {
            this.InitializeComponent();
            //BatchDownloadManager
            this._control2.DataContext = new VKDocument() { title = "Title", ext = ".test", size = 12345 };
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            this._progress.Value = e.NewValue;
        }

        private void Button_ClickNormalState(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this._control, "NormalState", true);
        }

        private void Button_ClickDownloadingState(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this._control, "DownloadingState", true);
        }
    }
}
