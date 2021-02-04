using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

using Windows.UI.Xaml.Media.Imaging;
using LunaVK.Core.DataObjects;
using LunaVK.Framework;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media.Animation;
using System.Diagnostics;
using LunaVK.ViewModels;

namespace LunaVK.UC
{
    public sealed partial class StoryViewer : UserControl
    {
        public Action Done;
        private List<VKStory> Stories;
        private List<RectangleGeometry> Fills = new List<RectangleGeometry>();
        private uint CurrentIndex = 0;
        private bool Close;
        private int OwnerId;
        private Stopwatch sw;

        public StoryViewer()
        {
            this.InitializeComponent();
            this.Loaded += this.StoryViewer_Loaded;
            this.Unloaded += this.StoryViewer_Unloaded;
            this.sw = new Stopwatch();
        }

        void StoryViewer_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Close = true;
            this.Unloaded -= this.StoryViewer_Unloaded;
        }

        void StoryViewer_Loaded(object sender, RoutedEventArgs e)
        {
            this.RenderNext();
            this.Loaded -= this.StoryViewer_Loaded;
        }
        
        public StoryViewer(NewsViewModel.NewStory stories)
            :this()
        {
            this.title.Text = stories.Owner.Title;
            this.UserVerified.Visibility = stories.Owner.IsVerified ? Visibility.Visible : Visibility.Collapsed;
            this.ownerPhoto.ImageSource = new BitmapImage(new Uri(stories.Owner.MinPhoto));
            this.date.Text = UIStringFormatterHelper.FormatDateTimeForUI(stories.Stories[0].date);

            this.Stories = stories.Stories;
            this.OwnerId = stories.Owner.Id;

            if (stories.Owner is VKGroup)
                this.OwnerId *= -1;
            
            foreach (VKStory story in stories.Stories)
            {/*
                switch (story.type)
                {
                    case "photo":
                        {
                            Image img = new Image();
                            img.Opacity = 0;
                            string link = story.photo.photo_807;
                            if (string.IsNullOrEmpty(link))
                                link = story.photo.photo_604;
                            if (string.IsNullOrEmpty(link))
                                link = story.photo.photo_130;
                            img.Source = new BitmapImage(new Uri(link));
                            this.content.Children.Insert(0,img);
                            break;
                        }
                    case "video":
                        {
                            MediaElement me = new MediaElement();
                            me.Opacity = 0;
                            me.AreTransportControlsEnabled = false;
                            me.AutoPlay = false;
                            me.MediaOpened += Me_MediaOpened;
                            me.MediaFailed += Me_MediaFailed;
                            string link = story.video.files.mp4_720;
                            if (string.IsNullOrEmpty(link))
                                link = story.video.files.mp4_480;
                            if (string.IsNullOrEmpty(link))
                                link = story.video.files.mp4_360;
                            if (string.IsNullOrEmpty(link))
                                link = story.video.files.mp4_240;
                            me.Source = new Uri(link);
                            this.content.Children.Insert(0, me);
                            break;
                        }
                }*/

                this.AddRect();
            }
            
            
        }

        

        
        private void AddRect()
        {
            /*
         * <Border Background="{ThemeResource AccentBrushMedium}" CornerRadius="3" Grid.ColumnSpan="2"/>
        <Border Background="{ThemeResource AccentBrushHigh}" CornerRadius="3" Grid.ColumnSpan="2">
            <Border.Clip>
                <RectangleGeometry x:Name="clipRectangleFill">
                    <RectangleGeometry.Transform>
                        <TranslateTransform x:Name="transformRectangleFill" />
                    </RectangleGeometry.Transform>
                </RectangleGeometry>
            </Border.Clip>
        </Border>
         * */

            this.panelSlider.ColumnDefinitions.Add(new ColumnDefinition());

            Grid grid = new Grid();
            grid.Margin = new Thickness(2);
            Grid.SetColumn(grid, this.panelSlider.Children.Count);

            Border brdBack = new Border() { CornerRadius = new CornerRadius(2), Background = new SolidColorBrush(Windows.UI.Colors.White), Opacity = 0.3 };
            Border brdFront = new Border() { CornerRadius = new CornerRadius(2), Background = new SolidColorBrush(Windows.UI.Colors.White) };


            TranslateTransform tt = new TranslateTransform() { X = -900 };
            RectangleGeometry g = new RectangleGeometry() { Transform = tt };
            brdFront.SizeChanged += brdFront_SizeChanged;

            brdFront.Clip = g;
            this.Fills.Add(g);



            grid.Children.Add(brdBack);
            grid.Children.Add(brdFront);

            this.panelSlider.Children.Add(grid);
        }

        void brdFront_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            RectangleGeometry g = element.Clip;
            g.Rect = new Rect(0.0, 0.0, e.NewSize.Width, e.NewSize.Height);
        }

        Storyboard curAnimation;

        private void AnimateFill(int number, int seconds)
        {
            var g = this.Fills[number];
            
            var tt = g.Transform as TranslateTransform;

            this.curAnimation = tt.Animate(-g.Rect.Width, 0, "X", seconds * 1000, 0, null, this.RenderNext);
        }
        
        private void RenderNext()
        {
            if (this.Close)
                return;

            int number = (int)this.CurrentIndex;
            if (number>=this.Stories.Count)
            {
                this.Done();
                return;
            }

            /*
            if(number>0)
            {
                this.content.Children[number - 1].Visibility = Visibility.Collapsed;
            }

            FrameworkElement el = this.content.Children[number] as FrameworkElement;
            if (el is MediaElement element)
            {
                element.Play();
            }

            this.content.Children[number].Opacity = 1.0;
            */

            if(this.content.Children.Count>0)
            {
                FrameworkElement el = this.content.Children[0] as FrameworkElement;
                if (el is MediaElement element)
                {
                    element.MediaOpened -= Me_MediaOpened;
                    element.MediaFailed -= Me_MediaFailed;
                    //element.CurrentStateChanged -= Me_CurrentStateChanged;
                }
                else
                {
                    Image i = el as Image;
                    i.Source = null;
                    i.ImageOpened -= Img_ImageOpened;
                    i.ImageFailed -= Img_ImageFailed;
                }
                this.content.Children.Remove(el);

            }

            var story = this.Stories[number];
            if(story.link!=null)
            {
                this.msmPanel.Visibility = Visibility.Collapsed;
                this.button.Visibility = Visibility.Visible;
                this.buttonText.Text = story.link.text;
            }
            else
            {
                this.msmPanel.Visibility = Visibility.Visible;
                this.button.Visibility = Visibility.Collapsed;
            }

            switch (story.type)
            {
                case "photo":
                    {
                        Image img = new Image();
                        string link = story.photo.photo_807;
                        if (string.IsNullOrEmpty(link))
                            link = story.photo.photo_604;
                        if (string.IsNullOrEmpty(link))
                            link = story.photo.photo_130;
                        img.Source = new BitmapImage(new Uri(link));
                        img.ImageOpened += Img_ImageOpened;
                        img.ImageFailed += Img_ImageFailed;
                        this.content.Children.Insert(0, img);
                        break;
                    }
                case "video":
                    {
                        MediaElement me = new MediaElement();
                        me.AreTransportControlsEnabled = false;
                        me.AutoPlay = false;
                        me.MediaOpened += Me_MediaOpened;
                        me.MediaFailed += Me_MediaFailed;
                        //me.CurrentStateChanged += Me_CurrentStateChanged;
                        string link = story.video.files.mp4_720;
                        if (string.IsNullOrEmpty(link))
                            link = story.video.files.mp4_480;
                        if (string.IsNullOrEmpty(link))
                            link = story.video.files.mp4_360;
                        if (string.IsNullOrEmpty(link))
                            link = story.video.files.mp4_240;
                        me.Source = new Uri(link);
                        this.content.Children.Insert(0, me);
                        break;
                    }
            }
            
            this.CurrentIndex++;
        }

        private void Me_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            MediaElement me = sender as MediaElement;
            if (me.CurrentState == MediaElementState.Stopped || me.CurrentState == MediaElementState.Paused)
                me.Play();
        }

        private void Img_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            int number = (int)this.CurrentIndex-1;
            var story = this.Stories[number];
            this.AnimateFill(number, story.type == "photo" ? 4 : (int)story.video.duration.TotalSeconds);
        }

        private void Img_ImageOpened(object sender, RoutedEventArgs e)
        {
            int number = (int)this.CurrentIndex-1;
            var story = this.Stories[number];
            this.AnimateFill(number, story.type == "photo" ? 4 : (int)story.video.duration.TotalSeconds);
        }
        
        private void Me_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            int number = (int)this.CurrentIndex-1;
            var story = this.Stories[number];
            this.AnimateFill(number, story.type == "photo" ? 4 : (int)story.video.duration.TotalSeconds);
        }

        private void Me_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (sender is MediaElement element)
            {
                element.Play();
            }

            int number = (int)this.CurrentIndex-1;
            var story = this.Stories[number];
            this.AnimateFill(number, story.type == "photo" ? 4 : (int)story.video.duration.TotalSeconds);
        }
        
        private void Close_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Close = true;
            this.Done();
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var story = this.Stories[(int)this.CurrentIndex-1];
            if(story.link!=null)
            {
                this.Close = true;
                this.Done();
                Library.NavigatorImpl.Instance.NavigateToWebUri(story.link.url);
            }
        }
        
        private void Profile_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Close = true;
            this.Done();
            Library.NavigatorImpl.Instance.NavigateToProfilePage(this.OwnerId);
        }

        private void Content_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //Debug.WriteLine("Content_PointerPressed");
            this.curAnimation.Pause();

            FrameworkElement el = this.content.Children[0] as FrameworkElement;
            if (el is MediaElement element)
            {
                element.Pause();
            }

            this.sw.Restart();
        }

        private void Content_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //Debug.WriteLine("Content_PointerReleased");

            this.sw.Stop();

            if (this.sw.ElapsedMilliseconds<150)
            {
                this.curAnimation.Stop();
                int number = (int)this.CurrentIndex - 1;
                var g = this.Fills[number];
                var tt = g.Transform as TranslateTransform;
                tt.X = 0;

                this.RenderNext();
                return;
            }
            

            this.curAnimation.Resume();

            FrameworkElement el = this.content.Children[0] as FrameworkElement;
            if (el is MediaElement element)
            {
                element.Play();
            }
        }

        
    }
}
