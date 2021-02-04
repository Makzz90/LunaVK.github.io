using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace LunaVK.UC
{
    public sealed partial class TitleWithSubtitleUC : UserControl
    {
        public TitleWithSubtitleUC()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(TitleWithSubtitleUC), new PropertyMetadata("", IsPullEnabledChanged));

        public string Title
        {
            get { return (string)base.GetValue(TitleProperty); }
            set { base.SetValue(TitleProperty, value); }
        }

        private static void IsPullEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TitleWithSubtitleUC control = (TitleWithSubtitleUC)d;
            string val = (string)e.NewValue;
            

            if (string.IsNullOrEmpty(val))
            {
                control._tbTitle.Text = val;
                control.TitleFadeIn.Begin();
            }
            else
            {
                control.TitleFadeOut.Begin();
            }
        }
        /*
        private string _title;
        public string Title
        {
            get
            {
                return this._title;
            }
            set
            {
                if (this._title == value)
                    return;

                if (string.IsNullOrEmpty(this._title))
                {
                    this._tbTitle.Text = value;
                    this.TitleFadeIn.Begin();
                }
                else
                {
                    this.TitleFadeOut.Begin();
                }

                this._title = value;
            }
        }
        */
        private string _subTitle;
        public string SubTitle
        {
            get
            {
                return this._subTitle;
            }
            set
            {
                if (this._subTitle == value)
                    return;

                if (string.IsNullOrEmpty(value))
                {
                    this.SubTitleFadeOut.Begin();
                    this._subTitle = value;
                    return;
                }

                if (string.IsNullOrEmpty(this._subTitle))
                {
                    this._tbSubTitle.Text = value;
                    this.SubTitleFadeIn.Begin();
                }
                else
                {
                    this.SubTitleChangeStart.Begin();
                }

                this._subTitle = value;
            }
        }

#region Foreground
        public Brush ForegroundColor
        {
            get { return (Brush)base.GetValue(ScrollableTextBlock.BrushProperty); }
            set { base.SetValue(ScrollableTextBlock.BrushProperty, value); }
        }

        public static readonly DependencyProperty ForegroundColorProperty = DependencyProperty.Register("ForegroundColor", typeof(Brush), typeof(TitleWithSubtitleUC), new PropertyMetadata(null, TitleWithSubtitleUC.OnForegroundPropertyChanged));
        private static void OnForegroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TitleWithSubtitleUC uc = (TitleWithSubtitleUC)d;
            uc._tbTitle.Foreground = (Brush)e.NewValue;
            uc._tbSubTitle.Foreground = (Brush)e.NewValue;
        }
#endregion

        private void TitleFadeOut_Completed(object sender, object e)
        {
            this._tbTitle.Text = this.Title;
            if (!string.IsNullOrEmpty(this.Title))
                this.TitleFadeIn.Begin();            
        }

        private void SubTitleChangeStart_Completed(object sender, object e)
        {
            this._tbSubTitle.Text = this._subTitle;
            this.SubTitleChangeEnd.Begin();
        }

        /// <summary>
        /// Показываем стрелочку дополнительных пунктов у текста шапки
        /// </summary>
        public bool TitleOption
        {
            get
            {
                return this.iconMenuOpen.Visibility == Visibility.Visible;
            }
            set
            {
                this.iconMenuOpen.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
