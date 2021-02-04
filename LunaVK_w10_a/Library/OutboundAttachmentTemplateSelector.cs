using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using LunaVK.Core.Library;

namespace LunaVK.Library
{
    public class OutboundAttachmentTemplateSelector : ContentControl
    {
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            this.ContentTemplate = this.SelectTemplate(newContent, this);
        }

        private DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is OutboundPhotoAttachment)
                return this.PhotoTemplate;

            
            if (item is OutboundVideoAttachment)
                return this.VideoTemplate;/*
            OutboundDocumentAttachment documentAttachment = item as OutboundDocumentAttachment;
            if (item is OutboundAlbumAttachment || item is OutboundProductAttachment || (item is OutboundMarketAlbumAttachment || item is OutboundUploadDocumentAttachment) || !string.IsNullOrEmpty(documentAttachment != null ? documentAttachment.Thumb : null))
                return this.GenericThumbTemplate;*/
            if (item is OutboundAddAttachment)
                return this.AddAttachmentTemplate;
            if (item is OutboundForwardedMessages)
                return this.ForwardedMessageTemplate;
            return this.GenericIconTemplate;
        }

        public DataTemplate PhotoTemplate { get; set; }

        public DataTemplate GeoTemplate { get; set; }

        public DataTemplate VideoTemplate { get; set; }

        public DataTemplate AudioTemplate { get; set; }

        public DataTemplate DocumentTemplate { get; set; }

        public DataTemplate GenericThumbTemplate { get; set; }

        public DataTemplate AddAttachmentTemplate { get; set; }

        public DataTemplate WallPostTemplate { get; set; }

        public DataTemplate GenericIconTemplate { get; set; }

        public DataTemplate ForwardedMessageTemplate { get; set; }
    }
}
