using LunaVK.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestVirtualBar : Page
    {
        private ApplicationView applicationView;

        public TestVirtualBar()
        {
            this.InitializeComponent();
            this.Loaded += TestVirtualBar_Loaded;

           
        }

        private void TestVirtualBar_Loaded(object sender, RoutedEventArgs e)
        {
            applicationView = ApplicationView.GetForCurrentView();
            applicationView.VisibleBoundsChanged += this.App_VisibleBoundsChanged;

            this.App_VisibleBoundsChanged(applicationView, null);

            Window.Current.VisibilityChanged += this.Current_VisibilityChanged;
        }

        private void Current_VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            this.App_VisibleBoundsChanged(applicationView, null);
        }

        private void App_VisibleBoundsChanged(ApplicationView v, object args)
        {
            double h = base.ActualHeight;
            double w = base.ActualWidth;

            double r = v.VisibleBounds.Right;
            double b = v.VisibleBounds.Bottom;
            double l = v.VisibleBounds.Left;
            double t = v.VisibleBounds.Top;

            double BottomOffset = 0, LeftOffset = 0, RightOffset = 0;

            if (CustomFrame.Instance.CurrentOrientation == ApplicationViewOrientation.Portrait)
            {
                BottomOffset = h - b;
            }
            else
            {
                if (l == 0) // Горизонтальная ориентаци, навбар справа
                {
                    RightOffset = w - r;
                }
                else // Горизонтальная ориентаци, навбар слева
                {
                    LeftOffset = l;
                }
            }

            if (BottomOffset < 0 || BottomOffset > 60)
                BottomOffset = 0;
            if (RightOffset < 0 || RightOffset > 60)
                RightOffset = 0;
            if (LeftOffset < 0 || LeftOffset > 60)
                LeftOffset = 0;

            //Logger.Instance.Info("VisibleBoundsChanged: ah{0} b{1} ob{2}", (int)base.ActualHeight, (int)b, (int)BottomOffset);
            //this.Margin = new Thickness(LeftOffset, 0, RightOffset, BottomOffset);

            this._tbLeft.Text = "Left: " + LeftOffset;
            this._tbRight.Text = "Right: " + RightOffset;
            this._tbBottom.Text = "Bottom: " + BottomOffset;
            //this._tbTop.Text = "Top: " + TopOffset;
            this._tbInfo.Text = string.Format("ah{0} aw{1} r{2} b{3} l{4} t{5}", (int)base.ActualHeight, (int)base.ActualWidth, (int)r, (int)b, (int)l, (int)t);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
 //           CustomFrame.Instance.HookEvents();
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
 //           CustomFrame.Instance.UnHookEvents();
        }
    }
}
