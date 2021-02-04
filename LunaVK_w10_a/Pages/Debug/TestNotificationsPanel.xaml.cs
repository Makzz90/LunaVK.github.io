using System;
using System.Collections.Generic;
using System.IO;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using LunaVK.Core.DataObjects;

namespace LunaVK
{
    public sealed partial class TestNotificationsPanel : PageBase
    {
        private string[] _names;
        private DispatcherTimer[] _timers;

        public TestNotificationsPanel()
        {
            
            this.InitializeComponent();

            this._names = new string[(int)this.slUsers.Maximum];
            this._timers = new DispatcherTimer[(int)this.slUsers.Maximum];

            for(int i =0;i<(int)this.slUsers.Maximum;i++)
            {
                this._names[i] = "User #"+(i+1);
                this._timers[i] = new DispatcherTimer();
                this._timers[i].Tick += TestNotificationsPanel_Tick;
                this._timers[i].Interval = TimeSpan.FromMilliseconds(this.slInterv.Value * 1000 + i * 10);

                if (this.slUsers.Value < i)
                    this._timers[i].Start();
            }
            this.Unloaded += TestNotificationsPanel_Unloaded;
            this.Loaded += TestNotificationsPanel_Loaded;
            

        }


        void TestNotificationsPanel_Loaded(object sender, RoutedEventArgs e)
        {
//            this.UpdateProfilePhoto(490, 1.3);
            /*
            //
            //
            double crop_x = 9.4;
            double crop_y = 5.4;
            double crop_x2 = 80.9;
            double crop_y2 = 77.5;

            double img_width = 1620;
            double img_height = 2160;
            double ratio = img_width / img_height;

            double width = (Window.Current.Content as Framework.CustomFrame).ActualWidth;
            double height = width / 2;

            this.BackGrid.Width = width;
            this.BackGrid.Height = height;

            Rect croppingRectangle1 = this.GetCroppingRectangle(width, height, crop_x, crop_y, crop_x2, crop_y2);
            this.ProfileImageClipRect.Rect = croppingRectangle1;

            this.image.Width = width + croppingRectangle1.Left + croppingRectangle1.Right;
            this.image.Height = (1 - ratio) * this.image.Width + this.image.Width;
            int i = 0;*/
            //
            //
        }


        int ProfileImageWidth;
        int ProfileImageHeight;
        Thickness ProfileImageMargin;


        private void UpdateProfilePhoto(double width, double ratio)
        {
            double requiredHeight = width / ratio;
            CropPhoto cropPhoto = new CropPhoto();

            cropPhoto.crop = new CropPhoto.CropRect() { x=9.4F,y=5.4F,x2=80.9F,y2=77.5F };
            cropPhoto.photo = new VKPhoto() { width = 1620, height = 2160 };
            cropPhoto.rect = new CropPhoto.CropRect() {  x=14.5F,y=13,x2=93.5F,y2=71.9F};


            
      //      string avatarUrl;
            if (cropPhoto != null)
            {
                bool flag = true;
//                string appropriateForScaleFactor = cropPhoto.photo.GetAppropriateForScaleFactor(requiredHeight, 1);
                VKPhoto photo = cropPhoto.photo;
                double num1 = photo.height > 0 ? (double)photo.width / (double)photo.height : 1.0;
                double width1 = width;
                double height = width1 / num1;
                if (num1 > ratio)
                {
                    height = requiredHeight;
                    width1 = requiredHeight * num1;
                    flag = false;
                }
                this.ProfileImageWidth = (int)width1;
                this.ProfileImageHeight = (int)height;
                Rect croppingRectangle1 = new Rect();// cropPhoto.crop.GetCroppingRectangle(width1, height);
                Rect croppingRectangle2 = new Rect();//cropPhoto.rect.GetCroppingRectangle(croppingRectangle1.Width, croppingRectangle1.Height);
                croppingRectangle2.X = croppingRectangle2.X + croppingRectangle1.X;
                croppingRectangle2.Y = croppingRectangle2.Y + croppingRectangle1.Y;
                double num4 = croppingRectangle2.X + croppingRectangle2.Width / 2.0;
                double num5 = croppingRectangle2.Y + croppingRectangle2.Height;
                if (flag)
                {
                    double num6 = croppingRectangle2.Height <= requiredHeight ? 2.0 : 2.56;
                    double num7 = num5 - croppingRectangle2.Height - croppingRectangle2.Height / num6;
                    double val1 = requiredHeight / 2.0 - num7;
                    if (croppingRectangle2.Height > requiredHeight && num7 - croppingRectangle2.Height / 2.0 >= 0.0)
                    {
                        val1 = -croppingRectangle2.Y;
                    }
                    double num8 = Math.Min(0.0, Math.Max(val1, requiredHeight - height));
                    this.ProfileImageMargin = new Thickness(0.0, num8, 0.0, 0.0);
//                    this.ProfileImageClipRect.Rect = new Rect(0.0, -(num8 + 1.0), width, requiredHeight + 1.0);
                }
                else
                {
                    double num6 = Math.Min(0.0, Math.Max(width / 2.0 - num4, width - width1));
                    this.ProfileImageMargin = new Thickness(num6, 0.0, 0.0, 0.0);
 //                   this.ProfileImageClipRect.Rect = new Rect(-(num6 + 1.0), 0.0, width + 1.0, requiredHeight);
                }
              //  this.NotifyPropertyChanged<Thickness>(() => this.ProfileImageMargin);
             //   this.NotifyPropertyChanged<Rect>(() => this.ProfileImageClipRect);
             //   avatarUrl = appropriateForScaleFactor;
            }
            else
            {
    //            avatarUrl = this._userData.photo_max ?? this._userData.photo_big;
                this.ProfileImageWidth = (int)width;
                this.ProfileImageHeight = (int)requiredHeight;
            }
            //if (!ProfileHeaderViewModelBase.IsValidAvatarUrl(avatarUrl))
            //    return;
            //this.ProfileImageUrl = avatarUrl;
            //this.NotifyPropertyChanged<string>(() => this.ProfileImageUrl);
            //this.NotifyPropertyChanged<int>(() => this.ProfileImageWidth);
            //this.NotifyPropertyChanged<int>(() => this.ProfileImageHeight);
            

            /*
            VKPhoto photo = cropPhoto.photo;

            double num1 = photo.height > 0 ? (double)photo.width / (double)photo.height : 1.0;
            double width1 = width;
            double height = requiredHeight;// width1 / num1;

            this.ProfileImageWidth = (int)width1;
            this.ProfileImageHeight = (int)height;



            this.BackGrid.Width = ProfileImageWidth;
            this.BackGrid.Height = requiredHeight;//ProfileImageHeight;


          //  this.image.Width = ProfileImageWidth;
            this.image.Height = requiredHeight;
            

            Rect croppingRectangle1 = cropPhoto.crop.GetCroppingRectangle(width1, height);
            
            this.ProfileImageClipRect.Rect = croppingRectangle1;

            // на сколько процентов изображение меньше экрана
            double mul = 100 - (cropPhoto.crop.x2 - cropPhoto.crop.x * 2) + 1;
          //  double add = width + width * mul / 100;

            tr.ScaleX = tr.ScaleY = 1 + mul/100;
            double offs = 0;// requiredHeight / 4;
            this.image.Margin = new Thickness(-(croppingRectangle1.Left * tr.ScaleX), -((croppingRectangle1.Top+offs) * tr.ScaleX), 0, 0);
            */
            int i = 0;


//            this.BackGrid.Width = ProfileImageWidth;
//            this.BackGrid.Height = requiredHeight;
//            this.image.Margin = ProfileImageMargin;
            this.Temp.Text = string.Format("ProfileImageWidth {0} ProfileImageHeight {1} requiredHeight {2}", ProfileImageWidth, ProfileImageHeight, requiredHeight);

//            this.Temp2.Text = string.Format("Rect.Left {0} Rect.Top {1} Rect.X {2} Rect.Y {3}", this.ProfileImageClipRect.Rect.Left, this.ProfileImageClipRect.Rect.Top, this.ProfileImageClipRect.Rect.X, this.ProfileImageClipRect.Rect.Y);
        }
        /*
        public Rect GetCroppingRectangle(double width, double height,   double x,double y, double x2, double y2)
        {
            double num1 = x * width / 100.0;
            double num2 = x2 * width / 100.0;
            double num3 = y * height / 100.0;
            double num4 = y2 * height / 100.0;
            double num5 = num2 - num1;
            double num7 = num4 - num3;
            return new Rect((double)(int)num1, (double)(int)num3, (double)(int)num5, (double)(int)num7);
        }
        */
        void TestNotificationsPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < (int)this.slUsers.Maximum; i++)
            {
                this._timers[i].Stop();
            }
        }

        void TestNotificationsPanel_Tick(object sender, object e)
        {
            DispatcherTimer t = sender as DispatcherTimer;
            int i = GetTimerIndex(t);

            Library.MessengerStateManager.Instance.HandleInAppNotification(this._names[i], this.GenerateText(), i, "https://vk.com/images/camera_50.png");
            //UC.NotificationsPanel n = (Window.Current.Content as Framework.CustomFrame).NotificationsPanel;
            //n.AddAndShowNotification("https://vk.com/images/camera_50.png", this._names[i], this.GenerateText(), null);

            t.Start();
        }
        
        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            //UC.NotificationsPanel n = UC.NotificationsPanel.Instance;
            //n.AddAndShowNotification("https://vk.com/images/camera_50.png", "Test", "LOL2", null);

            //this.pop.AddSpace();
        }


        // собеседники
        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (this._timers == null)
                return;

            Slider s = sender as Slider;
            for (int i = 0; i < (int)s.Maximum; i++)
            {
                if (i > (int)s.Value)
                    this._timers[i].Stop();
                else
                {
                    if (!this._timers[i].IsEnabled)
                        this._timers[i].Start();
                }
            }
        }

        // интервал
        private void Slider_ValueChanged2(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (this.slUsers == null)
                return;

            Slider s = sender as Slider;
            for (int i = 0; i < (int)this.slUsers.Maximum; i++)
            {
                this._timers[i].Interval = TimeSpan.FromMilliseconds(s.Value*1000 + i*10);
            }
        }

        // длина
        private void Slider_ValueChanged3(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider s = sender as Slider;
        }

        private string GenerateText()
        {
            string ret="";
            for(int i=0;i<this.slLen.Value;i++)
            {
                ret += i.ToString();
            }
            return ret;
        }

        private int GetTimerIndex(DispatcherTimer t)
        {
            int i =0;
            foreach(DispatcherTimer timer in this._timers)
            {
                if(timer == t)
                {
                    break;
                }
                i++;
            }
            return i;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //tr.ScaleX = tr.ScaleY = (tr.ScaleX+0.1);
            //Thickness m = this.image.Margin;
            //m.Left = m.Left + m.Left * 0.1;
            //this.image.Margin = m;
        }
    }
}
