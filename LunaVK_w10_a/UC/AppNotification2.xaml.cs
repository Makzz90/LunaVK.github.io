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
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.UC
{
    public sealed partial class AppNotification2 : UserControl
    {
        private DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        private Action _tapCallback;
        private const double MAX_MANIPULATION_X = 100;
        public bool ToRemove = false;

        /// <summary>
        /// Действие по окончанию времени
        /// </summary>
        public Action<UserControl> TimeToDelete;

        public AppNotification2()
        {
            this.InitializeComponent();
            //this._brd.LetsRound();
            //this.Loaded += AppNotification2_Loaded;
        }
        /*
        void AppNotification2_Loaded(object sender, RoutedEventArgs e)
        {
            this.ScaleStoryboard.Begin();
        }
        */
        public string Title
        {
            get
            {
                if(timer!=null && !timer.IsEnabled)
                    return "";//эта панель удаляется, значит надо сделать новую

                return this.title.Text;
            }
        }

        public AppNotification2(string image_src, string title, string content, Action tapCallback)
            : this()
        {
            this._tapCallback = tapCallback;

            Uri uri = new Uri(image_src, UriKind.RelativeOrAbsolute);
            BitmapImage bitmapImage = new BitmapImage() { UriSource = uri };
            this.imgBrush.ImageSource = bitmapImage;
            this.title.Text = title;
            this.AddContent(content);

            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(object sender, object e)
        {
            if (this.content.Children.Count == 1)
            {
                //timer.Tick -= timer_Tick;
                //timer.Stop();
                this.Hide();

                //if (this.TimeToDelete != null)
                //    this.TimeToDelete(this);
            }
            else
            {
                this.content.Children.RemoveAt(0);
            }
        }

        /// <summary>
        /// Добавляем текст
        /// </summary>
        /// <param name="content"></param>
        public void AddContent(string content)
        {
            if(this.content.Children.Count>=3)
            {
                this.content.Children.RemoveAt(0);
            }
            TextBlock t = new TextBlock();//todo:scrolable textblock for emodgi?
            t.FontSize = (double)Application.Current.Resources["FontSizeContent"]; ;
            t.Text = content;
            t.Style = (Style)Application.Current.Resources["TextBlockThemeContent"];
            t.Margin = new Thickness(0, 0, 10, 10);
            t.TextWrapping = TextWrapping.Wrap;

            this.content.Children.Add(t);

            if(!timer.IsEnabled)
            {
                timer.Start();
            }
        }

        public void Hide()
        {
            this.ToRemove = true;
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
            CollapsedStoryboard.Completed += CollapsedStoryboard_Completed;
            CollapsedStoryboard.Begin();
        }

        private void CollapsedStoryboard_Completed(object sender, object e)
        {
            CollapsedStoryboard.Completed -= CollapsedStoryboard_Completed;
            this.TimeToDelete?.Invoke(this);
        }
        
        private void main_grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (this._tapCallback != null)
                _tapCallback();

            this.TimeToDelete?.Invoke(this);
        }

        private CompositeTransform rootGridTransform
        {
            get { return this.RenderTransform as CompositeTransform; }
        }

        private void UserControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
            double resultTransform = rootGridTransform.TranslateX + e.Delta.Translation.X;
            if (resultTransform > 0)
                rootGridTransform.TranslateX += e.Delta.Translation.X;
            else
                rootGridTransform.TranslateX = 0;
        }

        private void UserControl_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;

            if (rootGridTransform.TranslateX < MAX_MANIPULATION_X)
                ManipulationResetStoryboard.Begin();
            else if (rootGridTransform.TranslateX >= MAX_MANIPULATION_X)
            {
                this.IsHitTestVisible = false;
                //                parent.SizeChanged -= Parent_SizeChanged;
                this.ManipulationDelta -= UserControl_ManipulationDelta;
                this.ManipulationCompleted -= UserControl_ManipulationCompleted;
                //timer.Stop();
                //timer = null;

                ManipulationCompletedStoryboard.Completed += delegate
                {
                    this.Hide();
                };
                ManipulationCompletedStoryboard.Begin();
            }
        }

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this.TimeToDelete?.Invoke(this);
        }
    }
}
