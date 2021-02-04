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

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace LunaVK.UC.PopUp
{
    public sealed partial class PostScheduleUC : UserControl
    {
        /*
        private DateTime Time
        {
            get
            {
                return new DateTime(this._date.Date.Year, this._date.Date.Month, this._date.Date.Day, this._time.Time.Hours, this._time.Time.Minutes, this._time.Time.Seconds);
            }
            set
            {
                this._time.Time = value.TimeOfDay;
                this._date.Date = DateTimeOffset.FromUnixTimeMilliseconds(value.Millisecond);
            }
        }
        */
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(DateTime), typeof(PostScheduleUC), new PropertyMetadata(null, OnDataChanged));

        /// <summary>
        /// Данные.
        /// </summary>
        public DateTime Data
        {
            //get { return (DateTime)GetValue(DataProperty); }
            
            get
            {
                DateTime ret = new DateTime(this._date.Date.Year, this._date.Date.Month, this._date.Date.Day, this._time.Time.Hours, this._time.Time.Minutes, this._time.Time.Seconds);
                //if (ret < DateTime.Now)
                //    ret = DateTime.Now;
                return ret;
            }
            
            set { SetValue(DataProperty, value); }
        }

        private static void OnDataChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            PostScheduleUC uc = (PostScheduleUC)obj;
            DateTime time = (DateTime)uc.GetValue(DataProperty);//uc.Data;
            if (time < DateTime.Now)
                time = DateTime.Now;
            uc._time.Time = time.TimeOfDay;
            uc._date.Date = new DateTimeOffset(time);
        }

        public event RoutedEventHandler CancelClick
        {
            add { this._cancelBtn.Click += value; }
            remove { this._cancelBtn.Click -= value; }
        }

        public event RoutedEventHandler SaveClick
        {
            add { this._saveBtn.Click += value; }
            remove { this._saveBtn.Click -= value; }
        }

        public PostScheduleUC()
        {
            this.InitializeComponent();
            this._date.MinYear = DateTimeOffset.Now;
            this._date.MaxYear = DateTimeOffset.Now.AddYears(3);
        }
    }
}
