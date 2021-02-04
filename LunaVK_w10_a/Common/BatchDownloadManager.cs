using LunaVK.Core;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Metadata;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;

namespace LunaVK.Common
{
    public class BatchDownloadManager : GenericCollectionViewModelSql<DownloadOprationItem>
    {
        private static BatchDownloadManager _instance;
        public static BatchDownloadManager Instance
        {
            get
            {
                if (BatchDownloadManager._instance == null)
                    BatchDownloadManager._instance = new BatchDownloadManager();
                return BatchDownloadManager._instance;
            }
        }

        public BatchDownloadManager()
            : base("downloads.db", typeof(DownloadOprationItem))
        {
        }

        private List<DownloadOprationItem> Items = new List<DownloadOprationItem>();
        public uint? _totalCount;

        public async void DiscoverActiveDownloadsAsync(Action<IReadOnlyList<DownloadOprationItem>, uint> callback)
        {
            uint total = 0;

            var temp = base.GetItems("Downloads");
            if (temp != null && temp.Count > 0)
            {
                foreach (var item in temp)
                {
                    item.CancelToken.Token.Register(() =>
                    {
                        this.Items.Remove(item);
                        this.RemoveRecord(item);
                    });
                }

                total = (uint)temp.Count;
            }


            try
            {
                //백그라운드 다운로더에서 현재 다운로드 정보를 반환 받아
                IReadOnlyList<DownloadOperation> downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();

                if (downloads.Count > 0)
                {
                    foreach (DownloadOperation download in downloads)
                    {
                        HandleDownloadAsync(download, false);
                    }

                    total += (uint)downloads.Count;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Info("Md_Mgr: Exception getting list of downloads: {0}", ex.Message);
            }

            this._totalCount = total;
            callback(temp, total);
        }

        public async void DownloadStream(string mainUrl, string content, string fileName)
        {
            //https://pvv4.vkuservideo.net/c545636/video/hls/8/ec0MzMwNjg4Njww/videos/4d60b9a38a/index-f2-v1-a1.m3u8

            var pos = mainUrl.LastIndexOf('/');
            string source = mainUrl.Substring(0, pos + 1);

            var result = content.Split(new[] { '\r', '\n' });

            var list = result.Where((a)=>a.Contains(".ts"));
            int i = 0;

            var pos2 = fileName.IndexOf(')');
            string fileId = fileName.Substring(1, pos2 - 1);

            string title = fileName.Substring(pos2 + 1);

            StorageFile outTempFile = await KnownFolders.DocumentsLibrary.CreateFileAsync(fileId+".mp4", CreationCollisionOption.ReplaceExisting);
            var s = (await outTempFile.OpenAsync(FileAccessMode.ReadWrite)).AsStreamForWrite();


            foreach (var item in list)
            {
                //seg-1-f3-v1-a1.ts?extra=
                
                //https://pvv4.vkuservideo.net/c531328/video/hls/13/ec0MzMwNjg4Njww/videos/4d60b9a38a/seg-2-f3-v1-a1.ts
                /*
                BackgroundDownloader downloader = new BackgroundDownloader();

                downloader.TransferGroup = BackgroundTransferGroup.CreateGroup("Test");

                StorageFile destTempFile = await KnownFolders.DocumentsLibrary.CreateFileAsync(fileName + ".ts", CreationCollisionOption.GenerateUniqueName);

                listForMerge.Add(destTempFile.Name);

                DownloadOperation download = downloader.CreateDownload(new Uri(source + item), destTempFile);

                //Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(this.DownloadProgress);

                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                await download.StartAsync().AsTask();

                System.Diagnostics.Debug.WriteLine(">> " + i);
                i++;

                if (i > 20)
                    break;
                    */
                using (HttpClient client = new HttpClient())
                using (var response = await client.GetStreamAsync(source + item))
                {
                    response.CopyTo(s);
                }

                double percent = i * 100 / result.Count();
                CreateProgressToastAsync(title, percent, fileId);
                i++;
            }
            
            s.Dispose();
            s = null;

            CreateSuccessToast(title, fileId);
        }

        public void DownloadByIndex(string url, string fileName)
        {
            fileName = fileName.Replace('/', 'I');

            Task.Run(async () =>
            {
                try
                {
                    Uri source = null;
                    //todo: нужен UserAgent для m3u8
                    HttpClientHandler handler = new HttpClientHandler() { AllowAutoRedirect = true };

                    using (HttpClient client = new HttpClient(handler))
                    using (HttpResponseMessage response = await client.GetAsync(url))
                    {
                        // ... Read the response to see if we have the redirected url
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            source = response.RequestMessage.RequestUri;

                            if (url.Contains(".m3u8"))
                            {
                                string content = await response.Content.ReadAsStringAsync();
                                this.DownloadStream(url, content, fileName);
                                return;
                            }
                        }
                        else
                        {
                            CreateFailureToast(fileName, "");
                            return;
                        }
                    }

                    if (fileName == null || fileName == "")
                    {
                        //파일명이 없으면 원본 파일 명을 사용한다.
                        string lastSegment = source.Segments.LastOrDefault();
                        fileName = Uri.UnescapeDataString(lastSegment);
                    }
                    
                    StorageFile destTempFile = await KnownFolders.DocumentsLibrary.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

                    BackgroundDownloader downloader = new BackgroundDownloader();
                    DownloadOperation download = downloader.CreateDownload(source, destTempFile);

                    CreateToast(LocalizedStrings.GetString("Download_Started"), destTempFile.Name, download.Guid.ToString());
                    
                    await HandleDownloadAsync(download, true);// Attach progress and completion handlers.

                    if (download.Progress.Status == BackgroundTransferStatus.Completed)
                    {
                        CreateSuccessToast(destTempFile.Name, download.Guid.ToString());
                        var cur = this.Items.FirstOrDefault((item) => item.DownloadOp != null && item.DownloadOp.Guid == download.Guid);
                        if (cur != null)
                        {
                            this.Items.Remove(cur);
                            //cur.MakeComplete();
                            this.Items.Add(cur);
                            this.AddRecord(cur);
                        }
                    }

                }
                catch (Exception ex)
                {
                }
            });
        }

        /// <summary>
        /// 다운로드 관리
        /// </summary>
        /// <param name="download"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        private async Task HandleDownloadAsync(DownloadOperation download, bool start)
        {
            DownloadOprationItem downloadItem = new DownloadOprationItem(download);

            this.Items.Add(downloadItem);

            downloadItem.CancelToken.Token.Register(() =>
            {
                if (downloadItem.Status != BackgroundTransferStatus.Completed)
                    downloadItem.DownloadOp.ResultFile.DeleteAsync();
                this.Items.Remove(downloadItem);
                if (downloadItem.DownloadOp != null)
                {
                    ToastNotificationManager.History.Remove(downloadItem.DownloadOp.Guid.ToString());
                }
            });

            try
            {





                // 프로그래스 콜백 하나 만들고
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(this.DownloadProgress);
                if (start)
                {
                    // true이면 바로 시작
                    await download.StartAsync().AsTask(downloadItem.CancelToken.Token, progressCallback);
                }
                else
                {
                    // false이면 캔슬토큰과 프로그래스만 붙여 놓고
                    await download.AttachAsync().AsTask(downloadItem.CancelToken.Token, progressCallback);
                }

                //ResponseInformation response = download.GetResponseInformation();
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                downloadItem.ErrorState = ex.Message;

                CreateFailureToast(download.ResultFile.Name, download.Guid.ToString());
                return;
            }
        }

        private void DownloadProgress(DownloadOperation download)
        {
            var di = this.Items.FirstOrDefault(p => p.DownloadOp != null && p.DownloadOp.Guid == download.Guid);

            if (download.Progress.TotalBytesToReceive > 0)
            {
                if (di != null)
                {
                    di.UpdateUI();

                    this.UpdateProgress(download);
                }
            }
        }

        private static string GetFileNameFromUri(Uri sourceUri)
        {
            //https://psv4.userapi.com/c856424/u460389/docs/d16/f6989f0bac92/Pride.cdr?extra=gZCDiK-rk4J3w21SjBynT-o-0BlfNhp1F09hWUxy3yF2TTTYcESC5Lxzp6Q_0VXW2uS3NMVnBgR6FHHDm5rF_-RRp60ojCcAnsWLMNFpuCT82LuQDEh_EOR8_HlMXfrte_tFU0E_&dl=1
            string temp = System.IO.Path.GetFileName(sourceUri.PathAndQuery);

            //Pride.cdr?extra=gZCDiK-rk4J3w21SjBynT-o-0BlfNhp1F09hWUxy3yF2TTTYcESC5Lxzp6Q_0VXW2uS3NMVnBgR6FHHDm5rF_-RRp60ojCcAnsWLMNFpuCT82LuQDEh_EOR8_HlMXfrte_tFU0E_&dl=1
            var pos = temp.IndexOf('?');
            if (pos > 0)
                temp = temp.Substring(0, pos);
            return temp;
        }

        private static void CreateFailureToast(string FileName, string tag)
        {
            CreateToast(LocalizedStrings.GetString("Download_Failed"), FileName, tag);
        }

        private static void CreateSuccessToast(string FileName, string tag)
        {
            //CreateToast("Download Completed", FileName, tag);

            XmlDocument xmlDocument = new XmlDocument();
            string xml = $@"<toast launch='action=openFileDownload&amp;downloadId={FileName}&amp;folder=temp'>
                              <visual>
                                <binding template='ToastGeneric'>
                                  <text>{LocalizedStrings.GetString("Download_Completed")}</text>
                                  <text>{FileName}</text>
                                </binding>
                              </visual>
                                <audio src='ms-winsoundevent:Notification.Default' loop='false' silent='true'/>

                            <actions>

                                <action
                                  activationType='foreground'
                                  arguments='action=openFolderDownload&amp;downloadId={FileName}&amp;folder=temp'
                                  content='{LocalizedStrings.GetString("Download_Acceptance_OpenFolder_Button")}'/>



                              </actions>

                            </toast>";
            xmlDocument.LoadXml(xml);

            ToastNotification toastNotification = new ToastNotification(xmlDocument);
            toastNotification.Tag = tag;
            var notifier = ToastNotificationManager.CreateToastNotifier();
            notifier.Show(toastNotification);
        }

        private static void CreateToast(string title, string name, string tag)
        {
            /*
            // Create xml template
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            // Set elements
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            IXmlNode element0 = stringElements[0];
            element0.AppendChild(toastXml.CreateTextNode(title));

            IXmlNode element1 = stringElements[1];
            element1.AppendChild(toastXml.CreateTextNode(name));

            // Create toast
            var toast = new ToastNotification(toastXml);
            toast.Tag = tag;
            var notifier = ToastNotificationManager.CreateToastNotifier();
            notifier.Show(toast);
            */
            XmlDocument xmlDocument = new XmlDocument();
            string xml = $@"<toast launch='action=viewDownloads'>
                              <visual>
                                <binding template='ToastGeneric'>
                                  <text>{title}</text>
                                  <text>{name}</text>
                                </binding>
                              </visual>
                                <audio src='ms-winsoundevent:Notification.Default' loop='false' silent='true'/>

                            

                            </toast>";
            xmlDocument.LoadXml(xml);

            ToastNotification toastNotification = new ToastNotification(xmlDocument);
            toastNotification.Tag = tag;
            var notifier = ToastNotificationManager.CreateToastNotifier();
            notifier.Show(toastNotification);
        }

        /*
         * <toast launch="action=viewDownload&amp;downloadId=9438108">
  
  <visual>
    <binding template="ToastGeneric">
      <text>Downloading this week's new music...</text>
      <progress
        title="{progressTitle}"
        value="{progressValue}"
        valueStringOverride="{progressValueString}"
        status="{progressStatus}"/>
    </binding>
  </visual>

  <actions>

    <action
      activationType="background"
      arguments="action=pauseDownload&amp;downloadId=9438108"
      content="Pause"/>

    <action
      activationType="background"
      arguments="action=cancelDownload&amp;downloadId=9438108"
      content="Cancel"/>
    
  </actions>
  
</toast>
*/
        private void UpdateProgress(DownloadOperation downloader)
        {
            double percent = downloader.Progress.BytesReceived * 100 / downloader.Progress.TotalBytesToReceive;
            //string s_tag = downloader.Guid.ToString();
            var pos2 = downloader.ResultFile.Name.IndexOf(')');

            string fileId = downloader.ResultFile.Name.Substring(1, pos2 - 1);

            EventAggregator1.Instance.PublishEvent(new Library.Events.DownloadProgressedEvent(percent, fileId));

            string s_tag = downloader.Guid.ToString();// Unique tag to reference the toast by
            string title = downloader.ResultFile.Name;

            CreateProgressToastAsync(title, percent, s_tag);
        }









        private static void CreateProgressToastAsync(string title, double percent, string tag)
        {
            if (!ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4))
                return;

            XmlDocument xmlDocument = new XmlDocument();

            string xml = @"<toast launch='action=viewDownloads'>
                              <visual>
                                <binding template='ToastGeneric'>
                                  <text>Скачивание...</text>
                                  <progress title='{title}' status='{status}' value='{progressValue}'
                                            valueStringOverride='{progressValueStringOverride}' />
                                </binding>
                              </visual>
                                <audio src='ms-winsoundevent:Notification.Default' loop='false' silent='true'/>

                            

                            </toast>";
            /*
             * <actions>

                                <action
                                  activationType='foreground'
                                  arguments='action=pauseDownload&amp;downloadId={id}'
                                  content='Pause'/>

                                <action
                                  activationType='foreground'
                                  arguments='action=cancelDownload&amp;downloadId={id}'
                                  content='Cancel'/>


                              </actions>
                              */
            xmlDocument.LoadXml(xml);

            // Dictionary with all the elements to databind.
            var data = new Dictionary<string, string>
            {
                {"title", title},
                {"status", ""},
                {"progressValue", "0"},
                {"progressValueStringOverride", "Downloading..."},
                {"id", tag},
            };




            var toastNotification = new ToastNotification(xmlDocument)
            {
                Tag = tag,
                Data = new NotificationData(data)
            };

            var s_toastNotifier = ToastNotificationManager.CreateToastNotifier();


            var list = ToastNotificationManager.History.GetHistory();
            var exist = list.FirstOrDefault((t) => t.Tag == tag);
            if (exist == null)
            {
                s_toastNotifier.Show(toastNotification);
            }

            // Update the data, just the parts that have changed.
            data = new Dictionary<string, string>
            {
                {"progressValue", (percent/100).ToString().Replace(',','.')},
                {"progressValueStringOverride", percent.ToString() + "%"}
            };

            // Using the unique tag we created to identify toast.
            s_toastNotifier.Update(new NotificationData(data), tag);


            //SendCompletedToast(title);
        }
        /*
        private static void CreateNotifications(BackgroundDownloader downloader)
        {
            //Requires Creators Update and 1.4.0 of Notifications library: You must target SDK 15063 and be running build 15063
            //Windows.Foundation.UniversalApiContract (introduced in v4.0)
            var successToastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            successToastXml.GetElementsByTagName("text").Item(0).InnerText = "Downloads completed successfully.";
            ToastNotification successToast = new ToastNotification(successToastXml);
            downloader.SuccessToastNotification = successToast;

            var failureToastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            failureToastXml.GetElementsByTagName("text").Item(0).InnerText = "At least one download completed with failure.";
            ToastNotification failureToast = new ToastNotification(failureToastXml);
            downloader.FailureToastNotification = failureToast;
        }
        */

        protected override void CreateDatabase()
        {
            base.CreateTableSql("Downloads");
        }

        public void AddRecord(object item)
        {
            base.InsertItem("Downloads", item);
        }

        public void RemoveRecord(object item)
        {
            base.DeleteItem("Downloads", item);
        }
    }
}
