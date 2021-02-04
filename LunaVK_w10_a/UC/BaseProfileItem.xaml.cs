using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using LunaVK.Core.Utils;

namespace LunaVK.UC
{
    public sealed partial class BaseProfileItem : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(BaseProfileItem), new PropertyMetadata(null, OnTitleChanged));
        public static readonly DependencyProperty SubTitleProperty = DependencyProperty.Register("SubTitle", typeof(string), typeof(BaseProfileItem), new PropertyMetadata(null, OnSubTitleChanged));
        public static readonly DependencyProperty SubSubTitleProperty = DependencyProperty.Register("SubSubTitle", typeof(string), typeof(BaseProfileItem), new PropertyMetadata(null, OnSubSubTitleChanged));

        public static readonly DependencyProperty Preview1UrlProperty = DependencyProperty.Register("Preview1Url", typeof(string), typeof(BaseProfileItem), new PropertyMetadata(null, OnPreview1UrlChanged));

        public static readonly DependencyProperty PrimaryContentProperty = DependencyProperty.Register("PrimaryContent", typeof(string), typeof(BaseProfileItem), new PropertyMetadata(null, OnPrimaryContentChanged));
        public static readonly DependencyProperty SecondaryContentProperty = DependencyProperty.Register("SecondaryContent", typeof(string), typeof(BaseProfileItem), new PropertyMetadata(null, OnSecondaryContentChanged));
        public static readonly DependencyProperty ThirdContentProperty = DependencyProperty.Register("ThirdContent", typeof(string), typeof(BaseProfileItem), new PropertyMetadata(null, OnThirdContentChanged));

        public static readonly DependencyProperty HideThirdCommandProperty = DependencyProperty.Register("HideThirdCommand", typeof(bool), typeof(BaseProfileItem), new PropertyMetadata(false, HideThirdCommandChanged));

        public string Title
        {
            get { return (string)base.GetValue(TitleProperty); }
            set { base.SetValue(TitleProperty, value); }
        }

        public string SubTitle
        {
            get { return (string)base.GetValue(SubTitleProperty); }
            set { base.SetValue(SubTitleProperty, value); }
        }

        public string SubSubTitle
        {
            get { return (string)base.GetValue(SubSubTitleProperty); }
            set { base.SetValue(SubSubTitleProperty, value); }
        }

        public string PrimaryContent
        {
            get { return (string)base.GetValue(PrimaryContentProperty); }
            set { base.SetValue(PrimaryContentProperty, value); }
        }
        public string SecondaryContent
        {
            get { return (string)base.GetValue(SecondaryContentProperty); }
            set { base.SetValue(SecondaryContentProperty, value); }
        }
        public string ThirdContent
        {
            get { return (string)base.GetValue(ThirdContentProperty); }
            set { base.SetValue(ThirdContentProperty, value); }
        }

        public bool HideThirdCommand
        {
            get { return (bool)GetValue(HideThirdCommandProperty); }
            set { SetValue(HideThirdCommandProperty, value); }
        }

        private static void OnPrimaryContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                ((BaseProfileItem)d).PrimaryCommandBtn.Visibility = Visibility.Visible;
                ((BaseProfileItem)d).PrimaryCommandBtn.Content = (e.NewValue as string);
            }
            else
            {
                ((BaseProfileItem)d).PrimaryCommandBtn.Visibility = Visibility.Collapsed;
            }

            ((BaseProfileItem)d).UpdateButtonsPanel();
        }

        private void UpdateButtonsPanel()
        {
            this.ButtonsPanel.Visibility = (!(string.IsNullOrEmpty(this.PrimaryContent) && string.IsNullOrEmpty(this.PrimaryContent))).ToVisiblity();
        }

        private static void OnSecondaryContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                ((BaseProfileItem)d).SecondaryCommandBtn.Visibility = Visibility.Visible;
                ((BaseProfileItem)d).SecondaryCommandBtn.Content = (e.NewValue as string);
            }
            else
            {
                ((BaseProfileItem)d).SecondaryCommandBtn.Visibility = Visibility.Collapsed;
            }

            ((BaseProfileItem)d).UpdateButtonsPanel();
        }

        private static void OnThirdContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (((BaseProfileItem)d).HideThirdCommand)
                return;

            if (e.NewValue != null)
            {
                ((BaseProfileItem)d).ThirdCommandIcon.Glyph = (e.NewValue as string);
                ((BaseProfileItem)d).ThirdCommandBtn.Visibility = Visibility.Visible;
            }
            else
            {
                ((BaseProfileItem)d).ThirdCommandBtn.Visibility = Visibility.Collapsed;
            }
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
         //   if (e.NewValue!=null)
          //      ((BaseProfileItem)d).TextBlockTitle.Text = (e.NewValue as string);
        }

        private static void OnSubTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty((e.NewValue as string)))
                ((BaseProfileItem)d).TextBlockSubTitle.Text = "";
            else
            {
                ((BaseProfileItem)d).TextBlockSubTitle.Visibility = Visibility.Visible;
                ((BaseProfileItem)d).TextBlockSubTitle.Text = (e.NewValue as string);
            }
        }

        private static void OnSubSubTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty((e.NewValue as string)))
                ((BaseProfileItem)d).TextBlockSubSubTitle.Text = "";
            else
            {
                ((BaseProfileItem)d).TextBlockSubSubTitle.Visibility = Visibility.Visible;
                ((BaseProfileItem)d).TextBlockSubSubTitle.Text = (e.NewValue as string);
            }
        }

        private static void HideThirdCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BaseProfileItem)d).ThirdCommandBtn.Visibility = (!(bool)e.NewValue).ToVisiblity();
        }

        public string Preview1Url
        {
            get { return (string)base.GetValue(Preview1UrlProperty); }
            set { base.SetValue(Preview1UrlProperty, value); }
        }

        private static void OnPreview1UrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            string newSource = e.NewValue as string;
            if (string.IsNullOrEmpty(newSource))
                return;

            BaseProfileItem infoListItemUc = (BaseProfileItem)d;

            Uri uri = new Uri(newSource, UriKind.RelativeOrAbsolute);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.UriSource = uri;
            infoListItemUc.imageBrush.ImageSource = bitmapImage;
        }

        public event RoutedEventHandler PrimaryClick
        {
            add { this.PrimaryCommandBtn.Click += value; }
            remove { this.PrimaryCommandBtn.Click -= value; }
        }

        public event RoutedEventHandler SecondaryClick
        {
            add { this.SecondaryCommandBtn.Click += value; }
            remove { this.SecondaryCommandBtn.Click -= value; }
        }

        private event RoutedEventHandler _thirdClick;
        public event RoutedEventHandler ThirdClick
        {
            add { this._thirdClick += value; }
            remove { this._thirdClick -= value; }
        }

        private event RoutedEventHandler _backClick;
        public event RoutedEventHandler BackTap
        {
            add { this._backClick += value; }
            remove { this._backClick -= value; }
        }

        /*
        public string PrimaryContent
        {
            get
            {
                return this.PrimaryCommandBtn.Content as string;
            }
            set
            {
                this.PrimaryCommandBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.PrimaryCommandBtn.Content = value;
            }
        }

        public string SecondaryContent
        {
            get
            {
                return this.SecondaryCommandBtn.Content as string;
            }
            set
            {
                this.SecondaryCommandBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.SecondaryCommandBtn.Content = value;
            }
        }

        public string ThirdContent
        {
            get
            {
                return this.ThirdCommandBtn.Content as string;
            }
            set
            {
                this.ThirdCommandBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.ThirdCommandIcon.Glyph = value;
            }
        }
        */
        public BaseProfileItem()
        {
            this.InitializeComponent();
        }

        private void ThirdCommandBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this._thirdClick?.Invoke(sender, null);
        }

        private void Back_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this._backClick?.Invoke(sender, null);
        }

        private void CommandBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
