using LunaVK.Core.Library;
using LunaVK.Framework;
using LunaVK.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;
using Windows.Storage;

namespace LunaVK.Pages
{
    //https://github.com/microsoft/Windows-universal-samples/blob/master/Samples/ShareTarget/cs/ShareTarget.xaml.cs
    public sealed partial class ShareExternalContentPage : Page
    {
        ShareOperation shareOperation;

        public ShareExternalContentPage()
        {
            

            this.InitializeComponent();
            //base.Title = "Share";
            this.Loaded += ShareExternalContentPage_Loaded;
        }

        private void ShareExternalContentPage_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.SuppressMenu = true;

            this.itemsControl.SelectionChanged += itemsControl_SelectionChanged;

            this.itemsControl.ItemsSource = DialogsViewModel.Instance.Items;
            if (DialogsViewModel.Instance.Items.Count == 0)
                DialogsViewModel.Instance.LoadDownAsync();
        }

        //protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.shareOperation = (ShareOperation)e.Parameter;
        }

        private async void itemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;

            ConversationWithLastMsg conversation = (ConversationWithLastMsg)e.AddedItems[0];
            int peer_id = conversation.conversation.peer.id;


            OutboundMessageViewModel viewModel = new OutboundMessageViewModel(peer_id,null);
            viewModel.Attachments = new System.Collections.Generic.List<IOutboundAttachment>();
            viewModel.MessageSent += ViewModel_MessageSent;
            // Retrieve the data package content.
            if (this.shareOperation.Data.Contains(StandardDataFormats.WebLink))
            {
                try
                {
                    var sharedWebLink = await this.shareOperation.Data.GetWebLinkAsync();
                    viewModel.MessageText = sharedWebLink.AbsolutePath;
                }
                catch (Exception ex)
                {
                    
                }
            }

            if (this.shareOperation.Data.Contains(StandardDataFormats.ApplicationLink))
            {
                try
                {
                    var sharedApplicationLink = await this.shareOperation.Data.GetApplicationLinkAsync();
                }
                catch (Exception ex)
                {
                    
                }
            }
            if (this.shareOperation.Data.Contains(StandardDataFormats.Text))
            {
                try
                {
                    var sharedText = await this.shareOperation.Data.GetTextAsync();
                    viewModel.MessageText = sharedText;
                }
                catch (Exception ex)
                {
                    
                }
            }
            if (this.shareOperation.Data.Contains(StandardDataFormats.StorageItems))
            {
                try
                {
                    var sharedStorageItems = await this.shareOperation.Data.GetStorageItemsAsync();
                    foreach(var item in sharedStorageItems)
                    {
                        StorageFile file = item as StorageFile;
                        IOutboundAttachment attachment = null;
                        if(file.ContentType.StartsWith("image"))
                        {
                            attachment = await OutboundPhotoAttachment.CreateForUploadNewPhoto(file, peer_id);
                        }
                        else
                        {
                            attachment = new OutboundDocumentAttachment(file);
                        }
                        
                        viewModel.Attachments.Add(attachment);
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }
            /*
            if (this.shareOperation.Data.Contains(dataFormatName))
            {
                try
                {
                    var sharedCustomData = await this.shareOperation.Data.GetTextAsync(dataFormatName);
                }
                catch (Exception ex)
                {
                    
                }
            }
            */
            if (this.shareOperation.Data.Contains(StandardDataFormats.Html))
            {
                try
                {
                    var sharedHtmlFormat = await this.shareOperation.Data.GetHtmlFormatAsync();
                }
                catch (Exception ex)
                {
                    
                }

                try
                {
                    var sharedResourceMap = await this.shareOperation.Data.GetResourceMapAsync();
                }
                catch (Exception ex)
                {
                    
                }
            }
            if (this.shareOperation.Data.Contains(StandardDataFormats.Bitmap))//не вызывается :(
            {
                try
                {
                    var sharedBitmapStreamRef = await this.shareOperation.Data.GetBitmapAsync();
                    //file = await StorageFile.CreateStreamedFileFromUriAsync(name, uri, RandomAccessStreamReference.CreateFromUri(uri)); // hangs here!!

                    //OutboundPhotoAttachment attachment = OutboundPhotoAttachment.CreateForUploadNewPhoto();
                    //attachment.ImageSrc = 
                    //viewModel.Attachments.Add(attachment);
                    int i = 0;
                }
                catch (Exception ex)
                {
                   
                }
            }
            viewModel.Send();

            
        }

        private void ViewModel_MessageSent(object sender, uint e)
        {
            shareOperation.ReportCompleted();
        }
    }
}
