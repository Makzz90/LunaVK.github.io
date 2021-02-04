using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

using Windows.UI.Xaml.Documents;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.UC
{
    public sealed partial class InfoListItemUC : UserControl
    {
        //public Action TapAction { get; set; }

#region IconUrl
        public static readonly DependencyProperty IconUrlProperty = DependencyProperty.Register("IconUrl", typeof(string), typeof(InfoListItemUC), new PropertyMetadata(null,InfoListItemUC.OnIconUrlChanged));
        public string IconUrl
        {
            get { return (string)base.GetValue(InfoListItemUC.IconUrlProperty); }
            set { base.SetValue(InfoListItemUC.IconUrlProperty, value); }
        }

        private static void OnIconUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InfoListItemUC infoListItemUc = (InfoListItemUC)d;

            infoListItemUc.borderIcon.Glyph = e.NewValue as string;
            infoListItemUc.borderIcon.Visibility = string.IsNullOrEmpty(infoListItemUc.borderIcon.Glyph) ? Visibility.Collapsed : Visibility.Visible;
        }
#endregion

#region Text
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(InfoListItemUC), new PropertyMetadata(null,InfoListItemUC.OnTextChanged));
        public string Text
        {
            get { return (string)base.GetValue(InfoListItemUC.TextProperty); }
            set { base.SetValue(InfoListItemUC.TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InfoListItemUC)d).UpdateText(e.NewValue as string);
        }

        private void UpdateText(string text)
        {
            text = text != null ? text.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Replace("  ", " ") : "";
            this.textBlockContent.Text = UIStringFormatterHelper.SubstituteMentionsWithNames(text);
        }
        #endregion

#region PreviewUrls
        public static readonly DependencyProperty PreviewUrlsProperty = DependencyProperty.Register("PreviewUrls", typeof(List<string>), typeof(InfoListItemUC), new PropertyMetadata(null,InfoListItemUC.OnPreviewUrlsChanged));

        public List<string> PreviewUrls
        {
            get { return (List<string>)base.GetValue(InfoListItemUC.PreviewUrlsProperty); }
            set { base.SetValue(InfoListItemUC.PreviewUrlsProperty, value); }
        }

        private static void OnPreviewUrlsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InfoListItemUC infoListItemUc = (InfoListItemUC)d;
            infoListItemUc.gridPreviews.Children.Clear();

            IReadOnlyList<string> previews = e.NewValue as IReadOnlyList<string>;
            if (previews != null)
            {
                for (int i = 0; i < previews.Count; i++)
                {
                    var p = previews[i];
                    Border brd = new Border();
                    brd.Style = (Style)infoListItemUc.Resources["BorderThemeItem"];
                    brd.CornerRadius = new CornerRadius(brd.Width / 2);
                    brd.Margin = new Thickness(0, 0, 20 * i, 0);
                    brd.Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri(p)) };
                    infoListItemUc.gridPreviews.Children.Add(brd);
                }
            }
        }
#endregion

        public InfoListItemUC()
        {
            this.InitializeComponent();
            this.textBlockContent.Text = "";
        }
    }
}
