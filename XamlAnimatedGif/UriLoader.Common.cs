using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;




using System.Collections.Generic;

using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;

using Windows.Web.Http;


namespace XamlAnimatedGif
{
    partial class UriLoader
    {
        //
        private static async Task DeleteTempFileAsync(string fileName)
        {
            try
            {
                var file = await ApplicationData.Current.TemporaryFolder.GetFileAsync(fileName);
                await file.DeleteAsync();
            }
            catch (FileNotFoundException)
            {
            }
        }

        private static async Task<Stream> CreateTempFileStreamAsync(string fileName)
        {
            IStorageFile file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            return await file.OpenStreamForWriteAsync();
        }

        private static async Task<Stream> GetStreamFromUriCoreAsync(Uri uri)
        {
            switch (uri.Scheme)
            {
                case "ms-appx":
                case "ms-appdata":
                    {
                        var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                        return await file.OpenStreamForReadAsync();
                    }
                case "ms-resource":
                    {
                        var rm = ResourceManager.Current;
                        var context = ResourceContext.GetForCurrentView();
                        var candidate = rm.MainResourceMap.GetValue(uri.LocalPath, context);
                        if (candidate != null && candidate.IsMatch)
                        {
                            var file = await candidate.GetValueAsFileAsync();
                            return await file.OpenStreamForReadAsync();
                        }
                        throw new Exception("Resource not found");
                    }
                case "file":
                    {
                        var file = await StorageFile.GetFileFromPathAsync(uri.LocalPath);
                        return await file.OpenStreamForReadAsync();
                    }
            }

            throw new NotSupportedException("Only ms-appx:, ms-appdata:, ms-resource:, http:, https: and file: URIs are supported");
        }

        private static string GetCacheFileName(Uri uri)
        {
            HashAlgorithmProvider sha1 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            byte[] bytes = Encoding.UTF8.GetBytes(uri.AbsoluteUri);
            IBuffer bytesBuffer = CryptographicBuffer.CreateFromByteArray(bytes);
            IBuffer hashBuffer = sha1.HashData(bytesBuffer);
            return CryptographicBuffer.EncodeToHexString(hashBuffer);
        }

        private static async Task<Stream> OpenTempFileStreamAsync(string fileName)
        {
            IStorageFile file;
            try
            {
                file = await ApplicationData.Current.TemporaryFolder.GetFileAsync(fileName);
            }
            catch (FileNotFoundException)
            {
                return null;
            }

            return await file.OpenStreamForReadAsync();
        }
        //
        public Task<Stream> GetStreamFromUriAsync(Uri uri, IProgress<int> progress)
        {
            if (uri.IsAbsoluteUri && (uri.Scheme == "http" || uri.Scheme == "https"))
                return GetNetworkStreamAsync(uri, progress);
            return GetStreamFromUriCoreAsync(uri);
        }

        private static async Task<Stream> GetNetworkStreamAsync(Uri uri, IProgress<int> progress)
        {
            string cacheFileName = GetCacheFileName(uri);
            var cacheStream = await OpenTempFileStreamAsync(cacheFileName);
            if (cacheStream == null)
            {
                //await DownloadToCacheFileAsync(uri, cacheFileName, progress);
            }
            progress.Report(100);
            return await OpenTempFileStreamAsync(cacheFileName);
        }
        /*
        private static async Task DownloadToCacheFileAsync(Uri uri, string fileName, IProgress<int> progress)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, uri);
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    long length = response.Content.Headers.ContentLength ?? 0;
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = await CreateTempFileStreamAsync(fileName))
                    {
                        IProgress<long> absoluteProgress = null;
                        if (progress != null)
                        {
                            absoluteProgress =
                                new Progress<long>(bytesCopied =>
                                {
                                    if (length > 0)
                                        progress.Report((int) (100*bytesCopied/length));
                                    else
                                        progress.Report(-1);
                                });
                        }
                        await responseStream.CopyToAsync(fileStream, absoluteProgress);
                    }
                }
            }
            catch
            {
                DeleteTempFileAsync(fileName);
                throw;
            }
        }*/
        //
        public class DownloadProgressChangedArgs
        {
            public Uri Uri { get; private set; }

            public double Percentage { get; private set; }

            public DownloadProgressChangedArgs(Uri uri, double percentage)
            {
                this.Uri = uri;
                this.Percentage = percentage;
            }
        }
        //
        public event EventHandler<DownloadProgressChangedArgs> DownloadProgressChanged;

        private async Task DownloadToCacheFileAsync(Uri uri, string fileName, CancellationToken cancellationToken)
        {
            Exception obj = null;
            int num = 0;
            try
            {
                HttpClient httpClient = new HttpClient();
                try
                {
                    IBuffer source = await httpClient.GetBufferAsync(uri).AsTask(cancellationToken, new Progress<HttpProgress>(delegate(HttpProgress progress)
                    {
                        ulong? totalBytesToReceive = progress.TotalBytesToReceive;
                        if (totalBytesToReceive.HasValue)
                        {
                            double percentage = Math.Round(progress.BytesReceived * 100.0 / totalBytesToReceive.Value, 2);
                            EventHandler<DownloadProgressChangedArgs> expr_3E = this.DownloadProgressChanged;
                            if (expr_3E == null)
                            {
                                return;
                            }
                            expr_3E.Invoke(this, new DownloadProgressChangedArgs(uri, percentage));
                        }
                    }));
                    Stream stream = source.AsStream();
                    try
                    {
                        Stream stream2 = await UriLoader.CreateTempFileStreamAsync(fileName);
                        try
                        {
                            await stream.CopyToAsync(stream2);
                        }
                        finally
                        {
                            if (stream2 != null)
                            {
                                stream2.Dispose();
                            }
                        }
                        stream2 = null;
                    }
                    finally
                    {
                        if (stream != null)
                        {
                            stream.Dispose();
                        }
                    }
                    stream = null;
                }
                finally
                {
                    if (httpClient != null)
                    {
                        httpClient.Dispose();
                    }
                }
                httpClient = null;
            }
            catch (Exception obj_0)
            {
                num = 1;
                obj = obj_0;
            }

            if (num == 1)
            {
                await UriLoader.DeleteTempFileAsync(fileName);
                Exception expr_2FC = obj as Exception;
                if (expr_2FC == null)
                {
                    throw obj;
                }
                ExceptionDispatchInfo.Capture(expr_2FC).Throw();
            }
            obj = null;
        }
    }
}
