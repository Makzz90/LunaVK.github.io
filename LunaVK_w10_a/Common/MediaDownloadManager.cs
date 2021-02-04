using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace LunaVK.Common
{
    public class MediaDownloadManager
    {
        private static MediaDownloadManager _instance;
        public static MediaDownloadManager Instance
        {
            get
            {
                if (MediaDownloadManager._instance == null)
                    MediaDownloadManager._instance = new MediaDownloadManager();
                return MediaDownloadManager._instance;
            }
        }

        public async void DiscoverActiveDownloadsAsync(Action<IReadOnlyList<DownloadOperation>> callback)
        {
            IReadOnlyList<DownloadOperation> downloads = null;
            try
            {
                downloads = (await BackgroundDownloader.GetCurrentDownloadsAsync()).ToList();

            }
            catch (Exception ex)
            {
                Logger.Instance.Info("Md_Mgr: Exception getting list of downloads: {0}", ex.Message);
                callback?.Invoke(downloads);
            }

            callback?.Invoke(downloads);
        }

        private async void BackgroundTransfer(string url, string filename)
        {
            try
            {
                CancellationTokenSource cancelSource = new CancellationTokenSource();
                var destinationFile = await StorageFile.GetFileFromPathAsync(filename);
                DownloadOperation download = new BackgroundDownloader().CreateDownload(new Uri(url), destinationFile);
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(new Action<DownloadOperation>(MediaDownloadManager.BackgroundDownloadProgress));
                DownloadOperation downloadOperation1 = await WindowsRuntimeSystemExtensions.AsTask<DownloadOperation, DownloadOperation>(download.StartAsync(), cancelSource.Token, progressCallback);
                
            }
            catch
            {

            }
        }

        private static void BackgroundDownloadProgress(DownloadOperation download)
        {
            
        }
    }
}
