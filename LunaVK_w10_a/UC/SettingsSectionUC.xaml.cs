using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.UC
{
    public sealed partial class SettingsSectionUC : UserControl
    {
        public SettingsSectionUC()
        {
            this.InitializeComponent();
        }

#region Title
        public string Title
        {
            get { return (string)base.GetValue(SettingsSectionUC.TitleProperty); }
            set { base.SetValue(SettingsSectionUC.TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(SettingsSectionUC), new PropertyMetadata("", SettingsSectionUC.OnTitleChanged));
        
        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SettingsSectionUC)d).titleBlock.Text = (string)e.NewValue;
        }
#endregion

        public string Icon
        {
            get { return this.fIcon.Glyph; }
            set
            {
                this.fIcon.Glyph = value;
            }
        }

#region SubTitle
        public string SubTitle
        {
            get { return (string)base.GetValue(SettingsSectionUC.SubTitleProperty); }
            set { base.SetValue(SettingsSectionUC.SubTitleProperty, value); }
        }

        public static readonly DependencyProperty SubTitleProperty = DependencyProperty.Register(nameof(SubTitle), typeof(string), typeof(SettingsSectionUC), new PropertyMetadata("", SettingsSectionUC.OnSubTitleChanged));
        
        private static void OnSubTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string value = (string)e.NewValue;
            ((SettingsSectionUC)d).tSubTitle.Text = value;
            ((SettingsSectionUC)d).tSubTitle.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
        }
#endregion
    }
}
