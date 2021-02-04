using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using LunaVK.Core.DataObjects;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using LunaVK.Common;
using Windows.UI.Xaml.Input;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media.Animation;
using Windows.System.Threading;
using LunaVK.Core.Framework;

namespace LunaVK.UC
{
    public class SpriteListControl : Grid
    {
        private readonly int _columns = 4;
        private readonly int _emojiColumns = 6;
        private readonly EasingFunctionBase ANIMATION_EASING;

        /// <summary>
        /// 8
        /// </summary>
        private readonly double _margin = 8;

        public SpriteListControl()
        {

            base.RenderTransform = new TranslateTransform();

            base.ManipulationDelta += SpriteListControl_ManipulationDelta;
            base.ManipulationCompleted += SpriteListControl_ManipulationCompleted;
            base.ManipulationStarted += SpriteListControl_ManipulationStarted;
            base.ManipulationMode = ManipulationModes.TranslateY | ManipulationModes.TranslateX;
            base.Tag = "CantTouchThis";
            //base.PointerEntered += SpriteListControl_PointerEntered;

            base.PointerWheelChanged += SpriteListControl_PointerWheelChanged;

            base.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
            //base.VerticalAlignment = VerticalAlignment.Stretch;
            this.Loaded += SpriteListControl_Loaded;

            this.ANIMATION_EASING = new CubicEase() { EasingMode = EasingMode.EaseOut };
        }


        private void SpriteListControl_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            foreach (var child in base.Children)
                child.IsHitTestVisible = false;
        }

        private void SpriteListControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (base.DataContext != null)
            {
                if (this.VM.IsEmoji)
                    this.CreateAndAddSprites();
                else
                    this.CreateAndAddStickersItems();
            }
            base.DataContextChanged += SpriteListControl_DataContextChanged;
            base.SizeChanged += SpriteListControl_SizeChanged;
        }

        private void SpriteListControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.VM == null || this.VM.IsEmoji)//не задействованные контролы
                return;

            double width = (e.NewSize.Width / (this.VM.IsEmoji ? this._emojiColumns : this._columns));
            int columns = this.VM.IsEmoji ? this._emojiColumns :this._columns;
            if (width > 125)
            {
                columns = (int)(e.NewSize.Width / 125);
                width = base.ActualWidth / columns;
            }

            int row = 0, column = 0;

            foreach (var child in base.Children)
            {
                FrameworkElement brd = child as FrameworkElement;

                brd.Height = brd.Width = width;
                brd.Margin = new Thickness(column * width, row * width, 0, 0);

                column++;
                if (column >= columns)
                {
                    column = 0;
                    row++;
                }
            }

            base.Height = (row + ((base.Children.Count % columns) > 0 ? 1 : 0)) * width;
        }

        private void SpriteListControl_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            foreach (var child in base.Children)
                child.IsHitTestVisible = true;

            double velocity = e.Velocities.Linear.Y;

            //System.Diagnostics.Debug.WriteLine("SpriteListControl_ManipulationCompleted " + velocity);

            double parentHeight = ((sender as FrameworkElement).Parent as FrameworkElement).ActualHeight;

            /*
             * если velocity < 0  значить палец шёл вверх
             * */
            double newTr = velocity * 200.0;//На какое расстояние палец говорит переместиться

            double offs = parentHeight - base.ActualHeight;

            //System.Diagnostics.Debug.WriteLine(string.Format("newTr:{0} offs:{1} ++:{2} y:{3}", newTr, offs, this._tr.Y + newTr, this._tr.Y));


            if (this._tr.Y > 0)
            {
                this._tr.Animate(this._tr.Y, 0, "Y", 200);
            }
            else if (this._tr.Y < offs)
            {
                this._tr.Animate(this._tr.Y, offs, "Y", 200);
            }
            else
            {
                if (newTr != 0)
                {
                    double to = this._tr.Y + newTr;
                    if (to > 0)
                        to = 0;
                    else if (to < offs)
                        to = offs;
                    this._tr.Animate(this._tr.Y, to, "Y", 300, 0, this.ANIMATION_EASING);
                }
            }
        }

        private TranslateTransform _tr
        {
            get { return base.RenderTransform as TranslateTransform; }
        }

        private void SpriteListControl_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint((UIElement)sender).Properties.MouseWheelDelta;
            delta /= 2;



            double parentHeight = ((sender as FrameworkElement).Parent as FrameworkElement).ActualHeight;
            double newTr = delta;//На какое расстояние палец говорит переместиться

            double offs = parentHeight - base.ActualHeight;

            //System.Diagnostics.Debug.WriteLine(string.Format("newTr:{0} offs:{1} ++:{2} y:{3}", newTr, offs, this._tr.Y + newTr, this._tr.Y));


            if (this._tr.Y > 0)
            {
                this._tr.Animate(this._tr.Y, 0, "Y", 200);
            }
            else if (this._tr.Y < offs)
            {
                this._tr.Animate(this._tr.Y, offs, "Y", 200);
            }
            else
            {
                if (newTr != 0)
                {
                    double to = this._tr.Y + newTr;
                    if (to > 0)
                        to = 0;
                    else if (to < offs)
                        to = offs;
                    this._tr.Animate(this._tr.Y, to, "Y", 300, 0, this.ANIMATION_EASING);
                }
            }
        }



        private void SpriteListControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (Math.Abs(e.Delta.Translation.X) > 1)
            {
                (sender as FrameworkElement).CancelDirectManipulations();
                return;
            }
            double y = e.Delta.Translation.Y;
            double parentHeight = ((sender as FrameworkElement).Parent as FrameworkElement).ActualHeight;
            double offs = parentHeight - base.ActualHeight;
            if (this._tr.Y + y >= 0 || this._tr.Y + y < offs)
            {
                y /= 3;
            }
            else if (this._tr.Y + y <= -base.Height)
            {
                this._tr.Y = -base.Height;
                return;
            }
            this._tr.Y += y;

            //System.Diagnostics.Debug.WriteLine(string.Format("y:{0}", this._tr.Y));

            e.Handled = true;
        }

        private void SpriteListControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            this.Children.Clear();
            this._tr.Y = 0;

            if (args.NewValue != null)
            {
                if (this.VM.IsEmoji)
                    this.CreateAndAddSprites();
                else
                    this.CreateAndAddStickersItems();
            }
        }
        //StickersItem

        //private double _totlaHeight = 0;

        private SpriteListItemData VM
        {
            get { return base.DataContext as SpriteListItemData; }
        }

        private void CreateAndAddSprites()
        {
            base.Children.Add(new EmojiControlUC(this.HandleEmojiClick));
        }

        private bool BuildUri(string str, out Uri uri)
        {
            bool ret = false;
            uri = null;
            switch (str.Length)
            {
                case 2:
                    uri = new Uri(string.Format("ms-appx:///Assets/Emoji/{0:X}.png", ((uint)(0 | str[0] << 16) | str[1])));
                    ret = true;
                    break;
                case 4:
                    uri = new Uri(string.Format("ms-appx:///Assets/Emoji/{0:X}{1:X}.png", ((uint)(0 | str[0] << 16) | str[1]), (0 | (ulong)(str[2] << 16) | str[3])));
                    ret = true;
                    break;
                    /*
                default:
                    uri = new Uri(string.Format("ms-appx:///Assets/Emoji/{0:X}.png", (short)str[0]));
                    ret = true;
                    break;*/
            }

            return ret;
        }

        private void CreateAndAddStickersItems()
        {
            var vm = this.VM.StickerProduct;
            if (vm == null)
                return;

            double width = (base.ActualWidth / this._columns);
            int columns = this._columns;

            if (width > 125)
            {
                columns = (int)(base.ActualWidth / 125);
                width = base.ActualWidth / columns;
            }

            int row = 0, column = 0;

            foreach (var sticker in vm.stickers)
            {
                Border brd = new Border() { Padding = new Thickness(this._margin) };
                brd.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
                brd.Loaded += Brd_Loaded;
                brd.Unloaded += Brd_Unloaded;
                /*
                if (Application.Current.Resources.ContainsKey("SystemControlTransparentRevealListLowBorderBrush"))
                {
                   
                    var _blinkBrush = Application.Current.Resources["SystemControlTransparentRevealListLowBorderBrush"] as RevealBorderBrush;
                    if (_blinkBrush != null)
                    {
                        brd.BorderThickness = new Thickness(1);
                        brd.BorderBrush = _blinkBrush;
                    }

                }
                */
                brd.Height = brd.Width = width;
                brd.DataContext = sticker;

                Image img = new Image();
                img.Source = new BitmapImage(new Uri(sticker.photo_128));
                img.Stretch = Stretch.Uniform;
                img.Opacity = sticker.ImageOpacity;
                brd.HorizontalAlignment = HorizontalAlignment.Left;
                brd.VerticalAlignment = VerticalAlignment.Top;
                brd.Margin = new Thickness(column * width, row * width, 0, 0);
                img.Tag = "CantTouchThis";

                //image1.Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.image_Tap));
                PreviewBehavior.SetPreviewUri(img, new Uri(sticker.photo_512));

                brd.Child = img;
                base.Children.Add(brd);

                column++;
                if (column >= columns)
                {
                    column = 0;
                    row++;
                }
            }

            base.Height = (row + ((vm.stickers.Count % columns) > 0 ? 1 : 0)) * width;
        }

        private void Brd_Unloaded(object sender, RoutedEventArgs e)
        {
            (sender as FrameworkElement).Tapped -= Brd_Tapped;
        }

        private void Brd_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as FrameworkElement).Tapped += Brd_Tapped;
        }

        private void Brd_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this._stickerClick?.Invoke(sender, null);
        }

        private event RoutedEventHandler _stickerClick;

        /// <summary>
        /// Стикер нажат
        /// </summary>
        public event RoutedEventHandler StickerClick
        {
            add { this._stickerClick += value; }
            remove { this._stickerClick -= value; }
        }

        private void HandleEmojiClick(object sender)
        {
            this._emojiClick?.Invoke(sender, null);
        }

        private event RoutedEventHandler _emojiClick;
        public event RoutedEventHandler EmojiClick
        {
            add { this._emojiClick += value; }
            remove { this._emojiClick -= value; }
        }

    }//StoreProduct
}
/*
 * <RevealBorderBrush x:Key="SystemControlTransparentRevealBorderBrush" TargetTheme="Dark" Color="Transparent" FallbackColor="Transparent" />
<RevealBorderBrush x:Key="SystemControlBackgroundTransparentRevealBorderBrush" TargetTheme="Dark" Color="Transparent" FallbackColor="Transparent" />
<RevealBorderBrush x:Key="SystemControlHighlightTransparentRevealBorderBrush" TargetTheme="Dark" Color="Transparent" FallbackColor="Transparent" />
<RevealBorderBrush x:Key="SystemControlHighlightAltTransparentRevealBorderBrush" TargetTheme="Dark" Color="Transparent" FallbackColor="Transparent" />
<RevealBorderBrush x:Key="SystemControlTransparentRevealListLowBorderBrush" TargetTheme="Dark" Color="{StaticResource SystemRevealListLowColor}" FallbackColor="Transparent" />
<RevealBorderBrush x:Key="SystemControlTransparentRevealBorderBrush" TargetTheme="Light" Color="Transparent" FallbackColor="Transparent" />
<RevealBorderBrush x:Key="SystemControlBackgroundTransparentRevealBorderBrush" TargetTheme="Light" Color="Transparent" FallbackColor="Transparent" />
<RevealBorderBrush x:Key="SystemControlHighlightTransparentRevealBorderBrush" TargetTheme="Light" Color="Transparent" FallbackColor="Transparent" />
<RevealBorderBrush x:Key="SystemControlHighlightAltTransparentRevealBorderBrush" TargetTheme="Light" Color="Transparent" FallbackColor="Transparent" />
<RevealBorderBrush x:Key="SystemControlTransparentRevealListLowBorderBrush" TargetTheme="Light" Color="{StaticResource SystemRevealListLowColor}" FallbackColor="Transparent" />

    */
