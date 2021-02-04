using LunaVK.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.UC
{
    public sealed partial class DownloadItemUC : UserControl
    {
        public DownloadItemUC()
        {
            this.InitializeComponent();
        }

        private async void M_downloadVisualRoot_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var vm = (sender as FrameworkElement).DataContext as DownloadOprationItem;
            if (vm.Status != Windows.Networking.BackgroundTransfer.BackgroundTransferStatus.Completed)
                return;

            var folder = KnownFolders.DocumentsLibrary;
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(Path.Combine(folder.Path, vm.ResultFileName));
                await Launcher.LaunchFileAsync(file);
                file = null;
            }
            catch
            {

            }
        }

        private async void HyperlinkButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var vm = (sender as FrameworkElement).DataContext as DownloadOprationItem;

            if (vm.Status != Windows.Networking.BackgroundTransfer.BackgroundTransferStatus.Completed)
                return;

            var folder = KnownFolders.DocumentsLibrary;//await StorageFolder.GetFolderFromPathAsync(vm.DownloadOp.ResultFile.Path);
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(Path.Combine(folder.Path, vm.ResultFileName));
                FolderLauncherOptions folderLauncher = new FolderLauncherOptions();
                folderLauncher.ItemsToSelect.Add(file);
                await Launcher.LaunchFolderAsync(folder, folderLauncher);
                file = null;
            }
            catch (FileNotFoundException ex)
            {
                await Launcher.LaunchFolderAsync(folder);
            }
            catch
            {

            }
        }
    }
}
