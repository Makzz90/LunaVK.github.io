using System;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using LunaVK.Core.DataObjects;
using LunaVK.Core.ViewModels;
using LunaVK.Framework;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace LunaVK.UC
{
    public sealed partial class AvatarUC : UserControl
    {
#region Data
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(object), typeof(AvatarUC), new PropertyMetadata(null, OnDataChanged));

        /// <summary>
        /// Данные.
        /// </summary>
        public object Data
        {
            get { return GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        private static void OnDataChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((AvatarUC)obj).ProcessData();
        }
#endregion

#region PlatformIcon
        public static readonly DependencyProperty PlatformIconProperty = DependencyProperty.Register("PlatformIcon", typeof(string), typeof(AvatarUC), new PropertyMetadata(null, OnPlatformIconChanged));

        /// <summary>
        /// Данные.
        /// </summary>
        public string PlatformIcon
        {
            get { return (string)GetValue(PlatformIconProperty); }
            set { SetValue(PlatformIconProperty, value); }
        }

        private static void OnPlatformIconChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var uc = ((AvatarUC)obj);
            if(string.IsNullOrEmpty(uc.PlatformIcon))
            {
                uc._icon1.Glyph = uc._icon2.Glyph = "";
                uc._icon1.Visibility = uc._icon2.Visibility = Visibility.Collapsed;
            }
            else
            {
                uc._icon1.Glyph = uc._icon2.Glyph = uc.PlatformIcon;
                uc._icon1.Visibility = uc._icon2.Visibility = Visibility.Visible;
            }
        }
#endregion

        private Ellipse Ellipse2 = null;
        private Ellipse Ellipse3 = null;
        private Ellipse Ellipse4 = null;

        private ConversationAvatarViewModel VM
        {
            get { return this.Data as ConversationAvatarViewModel; }
        }

        private void ProcessData()
        {
            this.MoreAvas.Children.Clear();

            if (this.Data == null)
                return;

            if (this.VM.Images.Count == 0)
                return;


            Debug.Assert(string.IsNullOrEmpty(this.VM.Images[0]) == false);
            this.ImageBrush1.ImageSource = new BitmapImage(new Uri(this.VM.Images[0]));
            if(this.VM.Images.Count>1)
            {
                Ellipse2 = CreateEllipse(2, this.VM.Images[1]);
                MoreAvas.Children.Add(Ellipse2);
            }
            if (this.VM.Images.Count > 2)
            {
                Ellipse3 = CreateEllipse(3, this.VM.Images[2]);
                MoreAvas.Children.Add(Ellipse3);
            }
            if (this.VM.Images.Count > 3)
            {
                Ellipse4 = CreateEllipse(4, this.VM.Images[3]);
                MoreAvas.Children.Add(Ellipse4);
            }

            UpdateSizes(base.ActualWidth);
        }


        public AvatarUC()
        {
            this.InitializeComponent();
            this.Loaded += AvatarUC_Loaded;
            this.SizeChanged += AvatarUC_SizeChanged;
        }

        private void UpdateSizes(double hw)
        {
            if (this.Data == null || hw==0)
                return;

            this._icon1.FontSize = hw / 3.0;
            this._icon2.FontSize = this._icon1.FontSize - 4;

            UpdateEllipseSize(Ellipse1, hw, 1);
            if (Ellipse2 != null)
                UpdateEllipseSize(Ellipse2, hw, 2);
            if (Ellipse3 != null)
                UpdateEllipseSize(Ellipse3, hw, 3);
            if (Ellipse4 != null)
                UpdateEllipseSize(Ellipse4, hw, 4);
        }

        private void AvatarUC_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSizes(base.ActualWidth);
        }

        private void UpdateEllipseSize(Ellipse el, double hw, byte number)
        {
            if (hw == 0)
                return;

            el.Height = el.Width = hw;
            
            ImageBrush brush = el.Fill as ImageBrush;
            TranslateTransform tr = brush.Transform as TranslateTransform;
            tr.Y = 0;

            double quad = hw / 4;
            double half = hw / 2;

            switch (this.VM.Images.Count)//total images
            {
                case 1:
                    {
                        if(number==1)
                        {
                            el.Clip.Rect = new Rect(0, 0, hw, hw);
                            tr.X = 0;
                            tr.Y = 0;
                        }
                        break;
                    }
                case 2:
                    {
                        if (CustomFrame.Instance.IsDevicePhone)
                        {
                            if (number == 1)
                            {
                                el.Clip.Rect = new Rect(0, 0, half - 0.5, hw);
                                tr.X = -quad;
                            }
                            else if (number == 2)
                            {
                                el.Clip.Rect = new Rect(quad + 0.5, 0, half, hw);
                                tr.X = quad;
                            }
                        }
                        else
                        {
                            if (number == 1)
                            {
                                el.Clip.Rect = new Rect(0, 0, half - 0.5, hw);
                                tr.X = -quad;
                            }
                            else if (number == 2)
                            {
                                el.Clip.Rect = new Rect(half + 0.5, 0, half, hw);////
                                tr.X = quad;
                            }
                        }
                        break;
                    }
                case 3:
                    {
                        if (CustomFrame.Instance.IsDevicePhone)
                        {
                            if (number == 1)
                            {
                                el.Clip.Rect = new Rect(0, 0, half - 0.5, hw);
                                tr.X = -quad;
                            }
                            else if (number == 2)
                            {
                                el.Clip.Rect = new Rect(quad + 0.5, 0, half, half - 0.5);
                                tr.X = quad;
                                tr.Y = -quad;
                            }
                            else if (number == 3)
                            {
                                el.Clip.Rect = new Rect(quad + 0.5, quad + 0.5, half, half);
                                tr.X = quad;
                                tr.Y = quad;
                            }
                        }
                        else
                        {
                            if (number == 1)
                            {
                                el.Clip.Rect = new Rect(0, 0, half - 0.5, hw);
                                tr.X = -quad;
                            }
                            else if (number == 2)
                            {
                                el.Clip.Rect = new Rect(half + 0.5, 0, half, half - 0.5);
                                tr.X = quad;
                                tr.Y = -quad;
                            }
                            else if (number == 3)
                            {
                                el.Clip.Rect = new Rect(half + 0.5, half + 0.5, half, half);
                                tr.X = quad;
                                tr.Y = half;
                            }
                        }
                        break;
                    }
                case 4:
                    {
                        if (CustomFrame.Instance.IsDevicePhone)
                        {
                            if (number == 1)
                            {
                                el.Clip.Rect = new Rect(0, 0, half - 0.5, half - 0.5);
                                tr.X = -quad;
                                tr.Y = -quad;
                            }
                            else if (number == 2)
                            {
                                el.Clip.Rect = new Rect(quad + 0.5, 0, half, half - 0.5);
                                tr.X = quad;
                                tr.Y = -quad;
                            }
                            else if (number == 3)
                            {
                                el.Clip.Rect = new Rect(quad + 0.5, quad + 0.5, half, half);
                                tr.X = quad;
                                tr.Y = quad;
                            }
                            else if (number == 4)
                            {
                                el.Clip.Rect = new Rect(0, quad + 0.5, half - 0.5, half);
                                tr.X = -quad;
                                tr.Y = quad;
                            }
                        }
                        else
                        {
                            if (number == 1)
                            {
                                el.Clip.Rect = new Rect(0, 0, half - 0.5, half - 0.5);
                                tr.X = -quad;
                                tr.Y = -quad;
                            }
                            else if (number == 2)
                            {
                                el.Clip.Rect = new Rect(half + 0.5, 0, half, half - 0.5);
                                tr.X = quad;
                                tr.Y = -quad;
                            }
                            else if (number == 3)
                            {
                                el.Clip.Rect = new Rect(half + 0.5, half + 0.5, half, half);
                                tr.X = quad;
                                tr.Y = half;
                            }
                            else if (number == 4)
                            {
                                el.Clip.Rect = new Rect(0, half + 0.5, half - 0.5, half);
                                tr.X = -quad;
                                tr.Y = half;
                            }
                        }
                        break;
                    }
            }

            /*
            половина 1 аватара  <TranslateTransform X="-10"/> <RectangleGeometry Rect="0 0 19.5 40"/>
            половина 2 аватара  <TranslateTransform X="10"/> <RectangleGeometry Rect="20 0 20 40"/>

            четверть 1 <TranslateTransform X="-10" Y="-10"/>  <RectangleGeometry Rect="0 0 20 20"/>
            четверть 2 <TranslateTransform X="10" Y="-10"/>  <RectangleGeometry Rect="20.5 0 20 20" />
            четверть 3  <TranslateTransform X="10" Y="10"/> <RectangleGeometry Rect="20 20 40 40" />
            */
        }

        private void AvatarUC_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSizes(e.NewSize.Width);
        }
        
        private Ellipse CreateEllipse(byte number, string img_uri)
        {
            Ellipse temp = new Ellipse();
            temp.Clip = new RectangleGeometry();
            ImageBrush brush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(img_uri))  };
            brush.Transform = new TranslateTransform();
            temp.Fill = brush;
//            UpdateEllipseSize(temp, base.ActualWidth, number);
            return temp;
        }
    }
}


