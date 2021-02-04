using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using LunaVK.ViewModels;
using LunaVK.Core.DataObjects;
using LunaVK.Core;
using LunaVK.Common;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.Core.Utils;
using LunaVK.UC.PopUp;
using LunaVK.UC.AttachmentPickers;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Collections.Generic;
using Windows.UI;
using LunaVK.UC;
using System.Net.Http;
using System.Net.Http.Headers;

namespace LunaVK
{
    /// <summary>
    /// Страница с документами
    /// </summary>
    public sealed partial class DocumentsPage : PageBase
    {
        public DocumentsPage()
        {
            this.InitializeComponent();
            this.Loaded += this.DocumentsPage_Loaded;
            base.Title = LocalizedStrings.GetString("Menu_Documents");
        }

        private void DocumentsPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.VM.Reload();
            CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE898", Clicked = this.AppBarAddButton_OnClicked });
            CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE721", Clicked = this.AppBarSearchButton_OnClicked });
        }

        private DocumentsViewModel VM
        {
            get { return base.DataContext as DocumentsViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            int ownerId = (int)e.Parameter;
            base.DataContext = new DocumentsViewModel(ownerId);
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot pivot = sender as Pivot;
            this.VM.SubPage = pivot.SelectedIndex;
            if (this.VM.Items[this.VM.SubPage].Items.Count == 0)
            {
                this.VM.Items[this.VM.SubPage].Reload();
            }
        }

        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gv = sender as GridView;
            var panel = (ItemsWrapGrid)gv.ItemsPanelRoot;
            panel.Orientation = Orientation.Horizontal;
            double colums = e.NewSize.Width / 100.0;
            panel.MaximumRowsOrColumns = (int)colums;

            panel.ItemHeight = panel.ItemWidth = e.NewSize.Width / (int)colums;
        }
        /*
        private Image GetImageFunc(int index)
        {
            GridViewItem item = this._gridView.ContainerFromIndex(index) as GridViewItem;
            UIElement ee = item.ContentTemplateRoot;
            Border brd = ee as Border;
            return brd.Child as Image;
        }
        */
        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var photo = (sender as FrameworkElement).DataContext as VKPhoto;

            //            int index = this.VM.Photos.IndexOf(photo);
            UC.ImageViewerDecoratorUC imageViewer = new UC.ImageViewerDecoratorUC();
            //           imageViewer.Initialize(this.VM.Photos.ToList(), (i) => { return this.GetImageFunc(i); });
            //           imageViewer.Show(index);
        }

        private void Video_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKVideoBase;
            Library.NavigatorImpl.Instance.NavigateToVideoWithComments(vm.owner_id, vm.id, vm.access_key, vm);
        }

        private void ExtendedListView3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;

            if(e.AddedItems.Count>0)
            {
                VKDocument doc = e.AddedItems[0] as VKDocument;
                this.StartDownload(doc);
            }
        }

        private void StartDownload(VKDocument doc)
        {
            BatchDownloadManager.Instance.DownloadByIndex(doc.url, "(" + doc.ToString() + ")" + doc.title);
        }

        private void item_OnEditButtonClicked(object sender, RoutedEventArgs e)
        {
            VKDocument vm = (sender as FrameworkElement).DataContext as VKDocument;
            //            Navigator.Current.NavigateToDocumentEditing(menuItemDataContext.Document.owner_id, menuItemDataContext.Document.id, menuItemDataContext.Document.title);

            DocumentEditingUC uc = new DocumentEditingUC(vm);

            PopUpService statusChangePopup = new PopUpService
            {
                Child = uc
            };
            uc.Done = (doc) =>
            {
                vm.title = doc.title;
                vm.tags = doc.tags;
                vm.UpdateUI();
                statusChangePopup.Hide();
            };
            statusChangePopup.OverrideBackKey = true;
            statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.Slide;
            statusChangePopup.Show();
        }

        private async void item_OnDeleteButtonClicked(object sender, RoutedEventArgs e)
        {
            VKDocument vm = (sender as FrameworkElement).DataContext as VKDocument;

            if (await MessageBox.Show("DeleteConfirmation", UIStringFormatterHelper.FormatNumberOfSomething(1, "Documents_DeleteOneFrm", "Documents_DeleteTwoFourFrm", "Documents_DeleteFiveFrm", true, null, false)) != MessageBox.MessageBoxButton.OK)
                return;
            this.VM.DeleteDocument(vm);
        }

        private async void AppBarAddButton_OnClicked(object sender)
        {
            //DocumentsPickerUC.Show(20);
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add("*");

            fileOpenPicker.SuggestedStartLocation = PickerLocationId.Downloads;

            IReadOnlyList<StorageFile> files = await fileOpenPicker.PickMultipleFilesAsync();

            if (files != null && files.Count > 0)
            {
                this.VM.UploadDocuments(files);
            }
        }

        private void AppBarSearchButton_OnClicked(object sender)
        {
            PopUpService expr_12 = new PopUpService();
            expr_12.OverlayGrid = this._backGrid;
            expr_12.OverrideBackKey = true;
            
            expr_12.AnimationTypeChild = PopUpService.AnimationTypes.None;
            DocumentsSearchDataProvider searchDataProvider = new DocumentsSearchDataProvider(/*this.VM.Items[this.pivot.SelectedIndex].Items*/);
            DataTemplate itemTemplate = (DataTemplate)base.Resources["ItemTemplate"];
            GenericSearchUC searchUC = new GenericSearchUC(searchDataProvider, itemTemplate,this.pivot);
            searchUC.Close = expr_12.Hide;
            searchUC.SelectedItemCallback = (element) =>
            {
                this.StartDownload(element as VKDocument);
            };

            expr_12.Child = searchUC;
            expr_12.Show();
            Grid.SetRow(expr_12.PopupContainer, 1);
            Grid.SetRow(expr_12.BackGroundGrid, 1);
        }
    }
}
