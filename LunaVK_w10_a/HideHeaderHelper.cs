using System;
using System.Collections.Generic;
using System.Text;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using LunaVK.UC;
using LunaVK.Framework;

namespace LunaVK
{
    public class HideHeaderHelper
    {
        //private readonly FrameworkElement _ucHeader;
        private readonly ScrollViewer _viewportControl;
        //private readonly TranslateTransform _translateHeader;
        //private readonly TranslateTransform _translateSub;
        //private double _previousScrollPosition = 0.0;
        
        public HideHeaderHelper(ScrollViewer viewportControl)
        {
            this._viewportControl = viewportControl;
            this._viewportControl.ViewChanged += _viewportControl_ViewChanged2;
            //this.Head.Opacity = 0;
        }
        /*
        private Grid Head
        {
            get
            {
                return (Window.Current.Content as Framework.CustomFrame).HeaderWithMenu.BackBackground;
            }
        }
        */
        double temp = 250;
        double temp2 = 1.0;

        void _viewportControl_ViewChanged2(object sender, ScrollViewerViewChangedEventArgs e)
        {
            double position = _viewportControl.VerticalOffset;/*
            double diff = position - _previousScrollPosition;

            if (position > temp)
            {
                double diff_percent = diff / 100.0 * temp2;
                if (this.Head.Opacity + diff_percent > 1.0)
                {
                    this.Head.Opacity = 1.0;
                }
                else if (this.Head.Opacity + diff_percent <= 0.0)
                {
                    this.Head.Opacity = 0.0;
                }
                else
                {
                    this.Head.Opacity += diff_percent;
                }
            }
            else if (diff < 0)
            {
                double diff_percent = diff / 100.0 * temp2;
                this.Head.Opacity += diff_percent;
            }


            this._previousScrollPosition = position;*/
            if (position > temp)
                CustomFrame.Instance.Header.IsVisible = true;
            else
                CustomFrame.Instance.Header.IsVisible = false;
        }

        /*
        public void Update()
        {
            _previousScrollPosition = 0;
            this._viewportControl_ViewChanged(null, null);
        }*/
    }
}
