using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace LunaVK.UC.AttachmentPickers
{
    public sealed partial class DocumentsPickerUC : UserControl
    {
        public Action<IReadOnlyList<IOutboundAttachment>> AttachmentsAction;
        public Action<VKDocument> DocumentAction;

        public DocumentsPickerUC()
        {
            this.DataContext = new PickerDocumentsViewModel();
            this.InitializeComponent();
            //this.Loaded += DocumentsPickerUC_Loaded;
        }

        //private void DocumentsPickerUC_Loaded(object sender, RoutedEventArgs e)
        //{
        //    this.VM.LoadData(true);
        //}

        private PickerDocumentsViewModel VM
        {
            get { return this.DataContext as PickerDocumentsViewModel; }
        }

        private void Document_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(e!=null)
                e.Handled = true;

            VKDocument vm = (sender as FrameworkElement).DataContext as VKDocument;

            this.DocumentAction?.Invoke(vm);
        }

        private async void UploadFile_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileOpenPicker fileOpenPicker = new Windows.Storage.Pickers.FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add("*");

            fileOpenPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;

            StorageFile file = await fileOpenPicker.PickSingleFileAsync();

            if (file != null)
            {
                OutboundDocumentAttachment a = new OutboundDocumentAttachment(file);
                a._sf = file;

                BitmapImage bimg = new BitmapImage();

                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    bimg.SetSource(stream);
                }
                a.LocalUrl2 = bimg;

                List<IOutboundAttachment> ret = new List<IOutboundAttachment>();
                ret.Add(a);

                this.AttachmentsAction?.Invoke(ret);
            }
        }

        private void Cancel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            List<IOutboundAttachment> ret = new List<IOutboundAttachment>();
            this.AttachmentsAction?.Invoke(ret);
        }

        public class PickerDocumentsViewModel : ISupportUpDownIncrementalLoading
        {
            public ObservableCollection<VKDocument> Documents { get; private set; }

            public async Task LoadUpAsync()
            {
                throw new NotImplementedException();
            }

            public bool HasMoreUpItems
            {
                get { return false; }
            }

            uint _maximum;

            public bool HasMoreDownItems
            {
                get
                {
                    return this.Documents.Count == 0 || this._maximum - this.Documents.Count > 0;
                }
            }

            public PickerDocumentsViewModel()
            {
                this.Documents = new ObservableCollection<VKDocument>();
            }

            public async Task<object> Reload()
            {
                this.Documents.Clear();
                //this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Reloading);
                await LoadDownAsync(true);
                return null;
            }

            public async Task LoadDownAsync(bool InReload = false)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["offset"] = this.Documents.Count.ToString();
                parameters["count"] = "30";
                //type
                //owner_id
                /*
                var temp = await RequestsDispatcher.GetResponse<VKCountedItemsObject<VKDocument>>("docs.get", parameters);
                if (temp.error.error_code == VKErrors.None)
                {
                    this._maximum = temp.response.count;
                    foreach (var file in temp.response.items)
                    {
                        this.Documents.Add(file);
                    }
                }*/
            }
        }

        private void AttachDocumentUC_OnTap(object sender, RoutedEventArgs e)
        {

        }












        /*
        private PopUpService _dialogService;
        private int _maxSelectionCount;
        public static void Show(int maxSelectionCount)
        {
            DocumentsPickerUC documentsPickerUc = new DocumentsPickerUC();
            documentsPickerUc._maxSelectionCount = maxSelectionCount;
            documentsPickerUc._dialogService = new PopUpService()
            {
                //AnimationType = DialogService.AnimationTypes.None,
                AnimationTypeChild = PopUpService.AnimationTypes.Swivel,
                //HideOnNavigation = true,
                OverrideBackKey = true,
                Child = documentsPickerUc
            };
            documentsPickerUc._dialogService.Show();
        }
        */
    }
}
