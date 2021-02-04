using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Shapes;

namespace LunaVK.UC.Attachment
{
    public class BaseAttachment : Grid
    {
        public Action OnTap { get; set; }

        public BaseAttachment(string title, long size, string icon, string img_url = "", bool top_offset = true)
        {
            base.HorizontalAlignment = HorizontalAlignment.Left;
            base.Height = 50;

            base.Margin = new Thickness(0, top_offset ? 10 : 0, 0,0);

            base.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(50) });
            base.ColumnDefinitions.Add(new ColumnDefinition());
            base.RowDefinitions.Add(new RowDefinition() /* Height = new GridLength(50) }*/);
            base.RowDefinitions.Add(new RowDefinition() /*{ Height = GridLength.Auto }*/);
            base.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
            base.Tapped += BaseAttachment_Tapped;

            if (string.IsNullOrEmpty(img_url))
            {
                IconUC iconUC = new IconUC();
                iconUC.FontSize = 20;
                //iconUC.Margin = new Thickness(0, 0, 10, 0);
                iconUC.Glyph = icon;
                iconUC.Foreground = (SolidColorBrush)Application.Current.Resources["AccentBrushHigh"];

                Ellipse ellipse = new Ellipse() { Height = 50, Width = 50 };
                //ellipse.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(byte.MaxValue, 227, 234, 242));
                //ellipse.HorizontalAlignment = HorizontalAlignment.Left;
                ellipse.Style = (Style)Application.Current.Resources["EllipseTheme"];

                

                Grid.SetRowSpan(ellipse, 2);
                Grid.SetRowSpan(iconUC, 2);

                base.Children.Add(ellipse);

                base.Children.Add(iconUC);
            }
            else
            {
                Image img = new Image();
                img.Margin = new Thickness(10, 5, 10, 5);
                img.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(img_url));
                img.Stretch = Stretch.UniformToFill;
                Grid.SetRowSpan(img, 2);
                base.Children.Add(img);
            }

            TextBlock titleBlock = new TextBlock();
            titleBlock.Text = title;
            titleBlock.Style = (Style)App.Current.Resources["TextBlockThemeSubContent"];//titleBlock.Foreground = (SolidColorBrush)Application.Current.Resources["AccentBrushHigh"];
            titleBlock.FontSize = (double)Application.Current.Resources["FontSizeContent"];
            titleBlock.VerticalAlignment = VerticalAlignment.Center;
            titleBlock.Margin = new Thickness(10, 0, 0, 0);

            TextBlock sizeBlock = new TextBlock();
            sizeBlock.Text = UIStringFormatterHelper.BytesForUI(size);
            sizeBlock.Style = (Style)App.Current.Resources["TextBlockThemeSubContent"];//sizeBlock.Foreground = (SolidColorBrush)Application.Current.Resources["TextBrushMediumHigh"];
            sizeBlock.FontSize = (double)Application.Current.Resources["FontSizeSmall"];
            sizeBlock.VerticalAlignment = VerticalAlignment.Center;
            sizeBlock.Margin = new Thickness(10, 0, 0, 0);
            sizeBlock.Opacity = 0.5;



            Grid.SetColumn(titleBlock, 1);
            Grid.SetColumn(sizeBlock, 1);
            Grid.SetRow(sizeBlock, 1);

            base.Children.Add(titleBlock);
            base.Children.Add(sizeBlock);
        }

        void BaseAttachment_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (this.OnTap != null)
            {
                this.OnTap();
                e.Handled = true;
            }
        }
    }
}
