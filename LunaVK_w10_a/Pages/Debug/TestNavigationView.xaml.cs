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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestNavigationView : Page
    {
        UIElement last;

        public TestNavigationView()
        {
            this.InitializeComponent();
            //last = _stp.Children[0];
            /*
            foreach (var element in _stp.Children)
            {
                element.Tapped += Border_Tapped;
            }

            if(_stp.Orientation == Orientation.Vertical)
            {
                Storyboard.SetTargetProperty(_bottomFrames, "Y");
                Storyboard.SetTargetProperty(_topFrames, "Y");
                _brd.HorizontalAlignment = HorizontalAlignment.Left;
                _brd.VerticalAlignment = VerticalAlignment.Stretch;
                _rect.Width = 3;
                _rect.Height = double.NaN;
            }
            else
            {
                Storyboard.SetTargetProperty(_bottomFrames, "X");
                Storyboard.SetTargetProperty(_topFrames, "X");

                _brd.HorizontalAlignment = HorizontalAlignment.Stretch;
                _brd.VerticalAlignment = VerticalAlignment.Top;
                _brd.Margin = new Thickness(0, 38, 0, 0);

                _rect.Height = 3;
                _rect.Width = double.NaN;
            }
            */
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            this.Set_Click(sender, e);
            this.Storyboard1.Begin();
        }

        private void Set_Click(object sender, RoutedEventArgs e)
        {
            this.Storyboard1.Stop();

            Storyboard.SetTargetProperty(_bottomFrames, this._type.Text);
            Storyboard.SetTargetProperty(_topFrames, this._type.Text);

            _topFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(0);
            _topFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(600);
            _bottomFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(0);
            _bottomFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(600);

            _bottomFrames.KeyFrames[0].Value = double.Parse(this._number1.Text);
            _bottomFrames.KeyFrames[1].Value = double.Parse(this._number2.Text);

            _topFrames.KeyFrames[0].Value = double.Parse(this._number3.Text);
            _topFrames.KeyFrames[1].Value = double.Parse(this._number4.Text);
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
 //           _trBottom.Y = -950;
  //          _trTop.Y = 0;
            last = null;
        }
        //https://github.com/microsoft/microsoft-ui-xaml/issues/833



        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            if (last == element)
                return;
            /*
            if (last==null)
            {
                _topFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(0);
                _topFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(600);
                _bottomFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(0);
                _bottomFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(600);
                
                 //* -1 > 1
                 //* -975
                 //* -950
                 //* 25
                 //* 0
                 //* 

                double newY = element.TransformToVisual(element.Parent as FrameworkElement).TransformPoint(new Point(0, 0)).Y;
                double h = this._rectGeometry.Rect.Height;

                _bottomFrames.KeyFrames[0].Value = -(h - newY - (element.ActualHeight / 2));
                _bottomFrames.KeyFrames[1].Value = -(h - newY - element.ActualHeight);
                _topFrames.KeyFrames[0].Value = newY + (element.ActualHeight / 2);
                _topFrames.KeyFrames[1].Value = newY;

                this.Storyboard1.Begin();

                last = element;

                return;
            }
            
             //* 1 > 2
             //* -950
             //* -900
             //* 0
             //* 50
             //* 
             //* 1 > 3
             //* -950
             //* -850
             //* 0
             //* 100
             //* 

            int d800 = 150;// int.Parse(this._number1.Text);//150
            int d1000 = 700;// int.Parse(this._number2.Text);//700
            int d600 = 150;// int.Parse(this._number3.Text);//150

            if (_stp.Orientation == Orientation.Vertical)
            {
                double newY = element.TransformToVisual(element.Parent as FrameworkElement).TransformPoint(new Point(0, 0)).Y;
                double prevY = last.TransformToVisual(element.Parent as FrameworkElement).TransformPoint(new Point(0, 0)).Y;

                double h = this._rectGeometry.Rect.Height;

                _bottomFrames.KeyFrames[0].Value = -(h - prevY - element.ActualHeight);
                _bottomFrames.KeyFrames[1].Value = -(h - newY - element.ActualHeight);
                _topFrames.KeyFrames[0].Value = prevY;
                _topFrames.KeyFrames[1].Value = newY;

                if (newY > prevY)//сверху вниз
                {
                    _topFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(d800);
                    _topFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(d1000);
                    _bottomFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(0);
                    _bottomFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(d600);
                }
                else
                {
                    _topFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(0);
                    _topFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(d600);
                    _bottomFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(d800);
                    _bottomFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(d1000);
                }
            }
            else
            {

                double newX = element.TransformToVisual(element.Parent as FrameworkElement).TransformPoint(new Point(0, 0)).X;
                double prevX = last.TransformToVisual(element.Parent as FrameworkElement).TransformPoint(new Point(0, 0)).X;
                
                double w = this._rectGeometry.Rect.Width;

                _bottomFrames.KeyFrames[0].Value = -(w - prevX - element.ActualWidth);
                _bottomFrames.KeyFrames[1].Value = -(w - newX - element.ActualWidth);
                _topFrames.KeyFrames[0].Value = prevX;
                _topFrames.KeyFrames[1].Value = newX;

                if (newX > prevX)//слева направо
                {
                    _topFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(d800);
                    _topFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(d1000);
                    _bottomFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(0);
                    _bottomFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(d600);
                }
                else
                {
                    _topFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(0);
                    _topFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(d600);
                    _bottomFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(d800);
                    _bottomFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(d1000);
                }
            }

            _tbOunt.Text = string.Format("{0} {1} {2} {3}", (int)_bottomFrames.KeyFrames[0].Value, (int)_bottomFrames.KeyFrames[1].Value, (int)_topFrames.KeyFrames[0].Value, (int)_topFrames.KeyFrames[1].Value);
            this.Storyboard1.Begin();


            last = element;
            */
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _navView.SelectedIndex = int.Parse(this._Index.Text);
        }

        private void Button_Click_Remove(object sender, RoutedEventArgs e)
        {
            this._navView.Items.RemoveAt(0);
        }

        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            this._navView.Items.Add("Lol"+ this._navView.Items.Count);
        }
    }
}
