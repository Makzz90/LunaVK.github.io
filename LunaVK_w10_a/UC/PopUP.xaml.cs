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

using LunaVK.Framework;

namespace LunaVK.UC
{
    public sealed partial class PopUP : UserControl
    {
        public event EventHandler<int> ItemTapped;
        private List<string> Titles;
        PopUpService dialogService;//DialogService dialogService;
        public bool Showing = false;
        public double _MaxWidth = 0.0;
        public object Argument = null;

        public PopUP()
        {
            this.InitializeComponent();

            this.Titles = new List<string>();
        }

        /// <summary>
        /// Добавляем пункт во всплывающее меню
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content">Текст</param>
        /// <param name="icon">Иконка</param>
        /// <param name="active">Доступность</param>
        public void AddItem(int id, string content, string icon = "", bool active = true)
        {
            this.Titles.Add(content);
            double font_size = (double)Application.Current.Resources["FontSizeContent"];

            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            sp.Margin = new Thickness(0, font_size / 3.0, 0, font_size / 3.0);
            sp.Height = 25.0;
            sp.Tag = id;
            if (id != -1)
            {
                sp.Tapped += sp_Tapped;
                sp.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
            }

            TextBlock t = new TextBlock();
            t.Text = content;
            t.Margin = new Thickness(10, 0, 10, 0);
            t.VerticalAlignment = VerticalAlignment.Center;
            t.Style = (Style)Application.Current.Resources["TextBlockThemeContent"];//t.Foreground = (SolidColorBrush)Application.Current.Resources["TextBrushMediumHigh"];
            t.FontSize = font_size;
            t.Measure(new Size(1000, 1000));
            

            double icon_width = 0.0;

            if(!string.IsNullOrEmpty(icon))
            {
                IconUC iconUC = new IconUC();
                iconUC.Glyph = icon;
                iconUC.VerticalAlignment = VerticalAlignment.Center;
                iconUC.Style = (Style)Application.Current.Resources["FontIconTheme"];////iconUC.Foreground = (SolidColorBrush)Application.Current.Resources["TextBrushMediumHigh"];
                iconUC.FontSize = t.FontSize;
                iconUC.Measure(new Size(1000, 1000));
                iconUC.Margin = new Thickness(10, 0, 0, 0);
                sp.Children.Add(iconUC);
                icon_width = iconUC.DesiredSize.Width + iconUC.Margin.Left;
            }

            this._MaxWidth = Math.Max(this._MaxWidth, t.DesiredSize.Width + icon_width + t.Margin.Left + t.Margin.Right + 2.0);//2 - это толщина рамки

            sp.Children.Add(t);

            if (!active)
                sp.Opacity = 0.5;

            this.main.Children.Add(sp);
        }

        public void ClearItems()
        {
            this.main.Children.Clear();
            this._MaxWidth = 0.0;
        }

        public string GetTitle(int index)
        {
            if (index < this.Titles.Count)
                return this.Titles[index];

            int ind = 0;
            this.main.Children.FirstOrDefault((c) => {
                if(c is StackPanel)
                {
                    int tag = (int)(c as StackPanel).Tag;
                    if (tag==index)
                    {
                        return true;
                    }
                    ind++;
                }
                
                return false;
            });

            return this.Titles[ind];
        }

        void sp_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (this.ItemTapped != null)
            {
                FrameworkElement element = sender as FrameworkElement;
                this.ItemTapped(this.Argument ?? this, (int)element.Tag);
                dialogService.Hide();
            }
        }

        public void AddSpace()
        {
            //Windows.UI.Popups.PopupMenu m = new Windows.UI.Popups.PopupMenu();
            //m.Commands.Add(new Windows.UI.Popups.UICommand() { Id=1 });
            //m.ShowAsync(new Point(50, 50));
            Windows.UI.Xaml.Shapes.Rectangle r = new Windows.UI.Xaml.Shapes.Rectangle();
            r.Height = 1;
            r.StrokeThickness = 1;
            r.Stroke = (SolidColorBrush)Application.Current.Resources["AccentBrushHigh"];
            r.Margin = new Thickness(0, 5, 0, 5);
            r.Opacity = 0.5;

            this.main.Children.Add(r);
        }

        public void Show(Point position)
        {
            if (this.Showing)
                return;

            this.VerticalAlignment = VerticalAlignment.Top;
            this.HorizontalAlignment = HorizontalAlignment.Left;

            double total_height = 0.0;
            foreach (FrameworkElement element in this.main.Children)
            {
                total_height += element.Height;
                total_height += element.Margin.Top;
                total_height += element.Margin.Bottom;
            }
            
            if (total_height + position.Y + 20.0 > ((Window.Current.Content as Frame).Content as Page).ActualHeight)
            {
                position.Y -= total_height;
            }

            if (position.X + this._MaxWidth /*+ 20.0*/ > ((Window.Current.Content as Frame).Content as Page).ActualWidth)
            {
                double diff = position.X + this._MaxWidth /*+ 20.0*/ - ((Window.Current.Content as Frame).Content as Page).ActualWidth;
                //System.Diagnostics.Debug.WriteLine(diff);
                position.X -= diff;
                //position.X -= 20;
            }


            if (dialogService == null)
                dialogService = new PopUpService();
            dialogService.VerticalOffset = position.Y;
            dialogService.HorizontalOffset = position.X;
            dialogService.Child = this;
                        
            dialogService.Closed += dialogService_Closed;

            dialogService.Show();


            this.Showing = true;
        }

        void dialogService_Closed(object sender, EventArgs e)
        {
            this.Showing = false;
        }
    }
}
