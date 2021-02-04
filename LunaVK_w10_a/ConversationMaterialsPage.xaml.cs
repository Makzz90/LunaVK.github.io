using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using LunaVK.ViewModels;
using LunaVK.Core.DataObjects;
using Windows.Storage.Pickers;
using Windows.Storage;
using LunaVK.Library;
using LunaVK.Core;

namespace LunaVK
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ConversationMaterialsPage : PageBase
    {
        public ConversationMaterialsPage()
        {
            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("Messenger_Materials");
        }

        private ConversationMaterialsViewModel VM
        {
            get { return base.DataContext as ConversationMaterialsViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            int peerId = (int)e.Parameter;
            base.DataContext = new ConversationMaterialsViewModel(peerId);
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot pivot = sender as Pivot;
//            this.VM.SubPage = pivot.SelectedIndex;
//            this.VM.LoadData();
        }
        
        private Border GetImageFunc(int index)
        {
            var temp = this._gridView.GetGridView.ContainerFromIndex(index);
            var temp2 = this._gridView.ContainerFromIndex(index);

            GridViewItem item = this._gridView.GetGridView.ContainerFromIndex(index) as GridViewItem;
            if (item == null)
                return null;
            UIElement ee = item.ContentTemplateRoot;
            if (ee == null)
                return null;
            Border brd = ee as Border;
            if (brd == null)
                return null;
            return brd;//.Child as Image;
        }

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            var photo = element.DataContext as VKPhoto;
        
            var item0 = this.VM.PhotosVM.Items.First((item) => (item as ConversationMaterialsViewModel.ConversationMaterial).attachment.photo == photo);

            int index = this.VM.PhotosVM.Items.IndexOf(item0);
            //NavigatorImpl.Instance.NavigateToImageViewer((uint)this.VM.Photos.Count, 0, index, this.VM.Photos.ToList(), /*"PhotosByIds", false, this._friendsOnlyfalse,*/ this.GetImageFunc, false);
            NavigatorImpl.Instance.NavigateToImageViewer((uint)this.VM.PhotosVM.Items.Count, 0, index, this.VM.PhotosVM.Items.Select((p)=>(p as ConversationMaterialsViewModel.ConversationMaterial).attachment.photo).ToList(), ImageViewerViewModel.ViewerMode.PhotosByIds, this.GetImageFunc);
        }

        private void Video_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Grid grid = sender as Grid;

            VKVideoBase vm = grid.DataContext as VKVideoBase;
            NavigatorImpl.Instance.NavigateToVideoWithComments(vm.owner_id, vm.id, vm.access_key, vm, grid.Children[0]);
        }

        private void Audio_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VKAudio vm = (sender as FrameworkElement).DataContext as VKAudio;/*
            string title = ViewModels.DialogsViewModel.Instance.CurrentConversation.Title;

            PlaylistViewModel p = new PlaylistViewModel("аудиозаписи беседы " + title, -1, this.VM.PeerId, this.VM.Audios.ToList());

            PlaylistManager.AddPlaylist(p);
            //            PlaylistManager.CurentPlaylistId = p.PlaylistId;
            Library.NavigatorImpl.Instance.NavigateToAudioPlayer();*/
        }

        private async void Document_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VKDocument vm = (sender as FrameworkElement).DataContext as VKDocument;

            var picker = new FileSavePicker();

            // set appropriate file types
            picker.FileTypeChoices.Add(vm.ext.ToUpper(), new List<string> { "." + vm.ext });
            picker.DefaultFileExtension = "." + vm.ext;
            
            picker.SuggestedFileName = vm.title;

            /*
            picker.ContinuationData.Add("Url", vm.url);
            var view = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView();
            view.Activated += this.view_Activated;
            picker.PickSaveFileAndContinue();
            */
            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    var client = new System.Net.Http.HttpClient();
                    var httpStream = await client.GetStreamAsync(new Uri(vm.url));
                    await httpStream.CopyToAsync(fileStream);
                    fileStream.Dispose();
                }
            }
        }

        async void view_Activated(Windows.ApplicationModel.Core.CoreApplicationView sender, Windows.ApplicationModel.Activation.IActivatedEventArgs args)
        {
            var fargs = args as Windows.ApplicationModel.Activation.FileSavePickerContinuationEventArgs;
            sender.Activated -= this.view_Activated;

            StorageFile file = fargs.File;
            if (file != null)
            {
                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    var client = new System.Net.Http.HttpClient();
                    var httpStream = await client.GetStreamAsync(new Uri((string)fargs.ContinuationData["Url"]));
                    await httpStream.CopyToAsync(fileStream);
                    fileStream.Dispose();
                }
            }
        }


    }
}
