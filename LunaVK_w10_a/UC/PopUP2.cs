using LunaVK.Core;
using LunaVK.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace LunaVK.UC
{
    public class PopUP2
    {
        public IList<PopUpItemBase> Items { get; }
        public event EventHandler<object> Opened;


        public PopUP2()
        {
            this.Items = new List<PopUpItemBase>();
        }

        public void ShowAt(FrameworkElement placementTarget)
        {
            if (Settings.CustomPopUpMenu)
            {
                PopUpService dialogService = new PopUpService();
                var transform = placementTarget.TransformToVisual(Window.Current.Content);
                var position = transform.TransformPoint(new Point(0, 0));
                if (position.Y < 0)
                    position.Y = 10;

                Border brd = new Border() { BorderThickness = new Thickness(1), BorderBrush = (SolidColorBrush)Application.Current.Resources["PhoneAccentColorBrush"] };
                brd.CornerRadius = new CornerRadius(2);
                brd.VerticalAlignment = VerticalAlignment.Top;
                brd.HorizontalAlignment = HorizontalAlignment.Left;
                brd.Style = (Style)Application.Current.Resources["BorderTheme"];//brd.Background = new SolidColorBrush(Windows.UI.Colors.White);
                StackPanel main = new StackPanel();
                main.Margin = new Thickness(2, 4, 2, 4); 
                double _MaxWidth = 0.0;

                double font_size = (double)Application.Current.Resources["FontSizeContent"];

                foreach (var item in this.Items)
                {
                    if (item is PopUpItem popUpItem)
                    {
                        StackPanel sp = new StackPanel();
                        sp.Orientation = Orientation.Horizontal;
                        sp.Margin = new Thickness(0, font_size / 3.0, 0, font_size / 3.0);
                        sp.Height = 25.0;
                        sp.Tag = item;
                        sp.Tapped += (sender,e) => {
                            PopUpItem _base = (sender as FrameworkElement).Tag as PopUpItem;
                            dialogService.Hide();
                            _base.Command.Execute(_base.CommandParameter);
                        };
                        sp.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
                        
                        TextBlock t = new TextBlock();
                        t.Text = popUpItem.Text;
                        t.Margin = new Thickness(10, 0, 10, 0);
                        t.VerticalAlignment = VerticalAlignment.Center;
                        t.Style = (Style)Application.Current.Resources["TextBlockThemeContent"];//t.Foreground = (SolidColorBrush)Application.Current.Resources["TextBrushMediumHigh"];
                        t.FontSize = font_size;
                        t.Measure(new Size(1000, 1000));

                        double icon_width = 0.0;

                        if (popUpItem.Icon != null)
                        {
                            var iconUC = popUpItem.Icon;
                            
                            iconUC.VerticalAlignment = VerticalAlignment.Center;
                            //iconUC.Style = (Style)Application.Current.Resources["FontIconTheme"];////iconUC.Foreground = (SolidColorBrush)Application.Current.Resources["TextBrushMediumHigh"];
                            //iconUC.FontSize = t.FontSize;
                            iconUC.Measure(new Size(1000, 1000));
                            iconUC.Margin = new Thickness(10, 0, 0, 0);
                            sp.Children.Add(iconUC);
                            icon_width = iconUC.DesiredSize.Width + iconUC.Margin.Left;
                        }

                        _MaxWidth = Math.Max(_MaxWidth, t.DesiredSize.Width + icon_width + t.Margin.Left + t.Margin.Right + 2.0);//2 - это толщина рамки

                        sp.Children.Add(t);

                        main.Children.Add(sp);
                        //MenuFlyoutItem menuItem = new MenuFlyoutItem();
                        //menuItem.Text = popUpItem.Text;
                        //menuItem.Icon = popUpItem.Icon;
                        //menuItem.Command = popUpItem.Command;
                        //menuItem.CommandParameter = popUpItem.CommandParameter;
                        //menuItemList.Items.Add(menuItem);
                    }
                    else if (item is PopUpSubItem popUpSubItem)
                    {
                        //MenuFlyoutSubItem subItem = new MenuFlyoutSubItem();
                        //subItem.Text = popUpSubItem.Text;
                        //foreach (var s in popUpSubItem.Items)
                        //{
                        //    MenuFlyoutItem subitem = new MenuFlyoutItem();

                        //    PopUpItem p = (PopUpItem)s;
                        //    subitem.Text = p.Text;
                        //    subitem.Icon = p.Icon;
                        //    subitem.Command = p.Command;
                        //    subitem.CommandParameter = p.CommandParameter;
                        //    subItem.Items.Add(subitem);
                        //}
                        //menuItemList.Items.Add(subItem);
                    }
                }



                double total_height = this.Items.Count * 25;

                if (total_height + position.Y + 20.0 > ((Window.Current.Content as Frame).Content as Page).ActualHeight)
                {
                    position.Y -= total_height;
                }

                if (position.X + _MaxWidth /*+ 20.0*/ > ((Window.Current.Content as Frame).Content as Page).ActualWidth)
                {
                    double diff = position.X + _MaxWidth /*+ 20.0*/ - ((Window.Current.Content as Frame).Content as Page).ActualWidth;
                    //System.Diagnostics.Debug.WriteLine(diff);
                    position.X -= diff;
                    //position.X -= 20;
                }



                brd.Child = main;
                
                dialogService.VerticalOffset = position.Y;
                dialogService.HorizontalOffset = position.X;
                dialogService.Child = brd;
                dialogService.BackgroundBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);

                //dialogService.Closed += dialogService_Closed;

                dialogService.Show();
            }
            else
            {
                try
                {
                    MenuFlyout menuItemList = new MenuFlyout();
                    if (this.Opened != null)
                        menuItemList.Opened += this.Opened;

                    foreach (var item in this.Items)
                    {
                        if (item is PopUpItem popUpItem)
                        {
                            MenuFlyoutItem menuItem = new MenuFlyoutItem();
                            menuItem.Text = popUpItem.Text;
 //                           menuItem.Icon = popUpItem.Icon;//BUG
                            menuItem.Command = popUpItem.Command;
                            menuItem.CommandParameter = popUpItem.CommandParameter;
                            menuItemList.Items.Add(menuItem);
                        }
                        else if (item is PopUpSubItem popUpSubItem)
                        {
                            MenuFlyoutSubItem subItem = new MenuFlyoutSubItem();
                            subItem.Text = popUpSubItem.Text;
//                            subItem.Icon = popUpSubItem.Icon;//BUG
                            foreach (var s in popUpSubItem.Items)
                            {
                                MenuFlyoutItem subitem = new MenuFlyoutItem();

                                PopUpItem p = (PopUpItem)s;
                                subitem.Text = p.Text;
//                                subitem.Icon = p.Icon;//BUG
                                subitem.Command = p.Command;
                                subitem.CommandParameter = p.CommandParameter;
                                subItem.Items.Add(subitem);
                            }
                            menuItemList.Items.Add(subItem);
                        }
                    }

                    menuItemList.ShowAt(placementTarget);
                }
                catch
                {
                    Settings.CustomPopUpMenu = true;
                }
            }
        }



        public class PopUpItem : PopUpItemBase
        {
            public ICommand Command { get; set; }
            public object CommandParameter { get; set; }
        }

        public sealed class PopUpSubItem : PopUpItemBase
        {
            public IList<PopUpItemBase> Items { get; }

            public PopUpSubItem()
            {
                this.Items = new List<PopUpItemBase>();
            }
        }

        public class PopUpItemBase
        {
            public string Text { get; set; }
            public IconElement Icon { get; set; }
        }
    }
}
