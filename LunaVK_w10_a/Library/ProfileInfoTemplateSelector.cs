using LunaVK.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.Library
{
    public class ProfileInfoTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// StackPanel
        /// TextBlock Text="{Binding Title}"
        /// ScrollableTextBlock Text="{Binding Data}"
        /// </summary>
        public DataTemplate RichTextTemplate { get; set; }

        /// <summary>
        /// StackPanel
        /// TextBlock Text="{Binding Title}"
        /// TextBlock Text="{Binding Data}"
        /// </summary>
        public DataTemplate SimpleTextTemplate { get; set; }

        /// <summary>
        /// StackPanel
        /// TextBlock Text="{Binding Title}"
        /// TextBlock Description={Binding Data}"
        /// Ellipse
        /// </summary>
        public DataTemplate TextAndImageTemplate { get; set; }

        /// <summary>
        /// StackPanel Orientation="Horizontal"
        /// "{Binding Icon}"
        /// TextBlock Text="{Binding Data}"
        /// </summary>
        public DataTemplate TextAndIconTemplate { get; set; }

        /// <summary>
        /// InfoListItemUC->
        /// StackPanel Orientation="Horizontal"
        /// "{Binding Icon}"
        /// ScrollableTextBlock Text="{Binding Data}"
        /// gridPreviews
        /// </summary>
        public DataTemplate PreviewsTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            ProfileInfoItem profileInfoItem = item as ProfileInfoItem;
            if (profileInfoItem == null)
                return null;
            switch (profileInfoItem.Type)
            {
                case ProfileInfoItemType.TitleSubtitle:
                    return this.RichTextTemplate;
                case ProfileInfoItemType.IconSimpleText:
                    return this.SimpleTextTemplate;
                case ProfileInfoItemType.TitleSubtitleImage:
                    return this.TextAndImageTemplate;
                case ProfileInfoItemType.IconRichText:
                    return this.TextAndIconTemplate;
                case ProfileInfoItemType.IconTextPreviews:
                    return this.PreviewsTemplate;
                default:
                    return null;
            }
        }
    }
}