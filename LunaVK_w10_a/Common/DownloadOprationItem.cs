using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using System.Windows.Input;
using LunaVK.Core.Library;
using LunaVK.Framework;
using LunaVK.Core.ViewModels;
using LunaVK.Core.Framework;
using LunaVK.Core;

namespace LunaVK.Common
{
    public class DownloadOprationItem : ViewModelBase, ISupportGroup
    {
        public CancellationTokenSource CancelToken { get; private set; }

        public DownloadOprationItem(DownloadOperation download)
            : this()
        {
            this.RequestUri = download.RequestedUri.ToString();
            this.ResultFileName = download.ResultFile.Name;
            this.DownloadOp = download;
        }
        /*
        public void MakeComplete()
        {
            //this.DownloadOp = null;
            //base.NotifyPropertyChanged(nameof(this.Key));
        }
        */
        /// <summary>
        /// 생성자
        /// </summary>
        public DownloadOprationItem()
        {
            this.CancelToken = new CancellationTokenSource();

            this.PauseCommand = new DelegateCommand((args) =>
            {
                this.DownloadOp.Pause();
                CheckCanExecuteChanged();
            },
            (args) =>
            {
                return this.Status == BackgroundTransferStatus.Running;
            });


            this.ResumeCommand = new DelegateCommand((args) =>
            {
                this.DownloadOp.Resume();
                CheckCanExecuteChanged();
            },
            (args) =>
            {
                return this.CurrentFileSize > 0;
            });



            this.CancelCommand = new DelegateCommand((args) =>
            {
                this.Cancel();
            },
            (args) =>
            {
                return true;
            });


            this.RestartCommand = new DelegateCommand((args) =>
            {
                this.DownloadOp.Resume();
                CheckCanExecuteChanged();
            },
            (args) =>
            {
                return this.Status == BackgroundTransferStatus.Error;
            });
        }

        public void Cancel()
        {
            if (this.CancelToken == null)
                return;//todo: надо удялть из списка
            this.CancelToken.Cancel();
            this.CancelToken.Dispose();
            this.CancelToken = null;
        }

        public DownloadOperation DownloadOp { get; private set; }

        [Column(Name = "file_name", IsPrimaryKey = true)]
        public string ResultFileName { get; private set; }

        [Column(Name = "uri")]
        public string RequestUri { get; private set; }

        public double DownloadProgressPercent
        {
            get
            {
                if (this.DownloadOp == null)
                    return 100;

                if (this.DownloadOp.Progress.TotalBytesToReceive == 0)
                    return 0;

                return this.DownloadOp.Progress.BytesReceived * 100 / this.DownloadOp.Progress.TotalBytesToReceive;
            }
        }

        public string DownloadProgressPercentString
        {
            get { return this.DownloadProgressPercent + "%"; }
        }


        //-----------------

        public ulong FinalFileSize
        {
            get
            {
                if (this.DownloadOp == null)
                    return 0;
                return this.DownloadOp.Progress.TotalBytesToReceive;
            }
        }

        /// <summary>
        /// BytesReceived
        /// </summary>
        public ulong CurrentFileSize
        {
            get
            {
                if (this.DownloadOp == null)
                    return 0;
                return this.DownloadOp.Progress.BytesReceived;
            }
        }

        public void UpdateUI()
        {
            base.NotifyPropertyChanged(nameof(this.FinalFileSize));
            base.NotifyPropertyChanged(nameof(this.CurrentFileSize));
            base.NotifyPropertyChanged(nameof(this.PercentageComplete));
            base.NotifyPropertyChanged(nameof(this.Status));


            base.NotifyPropertyChanged(nameof(this.IsPauseable));
            base.NotifyPropertyChanged(nameof(this.IsResumable));
            base.NotifyPropertyChanged(nameof(this.Speed));
            base.NotifyPropertyChanged(nameof(this.Key));

            this.CheckCanExecuteChanged();

            System.Diagnostics.Debug.WriteLine(this.Status.ToString());
        }

        public string PercentageComplete
        {
            get { return this.DownloadProgressPercent.ToString(); }
        }

        public BackgroundTransferStatus Status
        {
            get
            {
                if (this.DownloadOp == null)
                    return BackgroundTransferStatus.Completed;

                if (this.DownloadProgressPercent == 100 && this.DownloadOp.Progress.Status == BackgroundTransferStatus.Running)
                    return BackgroundTransferStatus.Completed;
                return this.DownloadOp.Progress.Status;
            }
        }


        public bool IsPauseable
        {
            get { return this.Status == BackgroundTransferStatus.Running; }
        }

        public bool IsResumable
        {
            get { return this.Status != BackgroundTransferStatus.Running && this.Status != BackgroundTransferStatus.Error && this.Status != BackgroundTransferStatus.Completed; }
        }

        private DateTime lastUIUpdate = DateTime.Now; //class variable, initialized when download starts and UI is set to 0%
        private ulong lastUiDownloaded = 0;

        public string Speed
        {
            get
            {
                if (this.DownloadOp == null)
                    return "";

                string ret = "";
                double uiUpdateSpeed = (DateTime.Now - this.lastUIUpdate).TotalMilliseconds;//Примерно каждые пол секунды (400-600)
                ulong downloaded = this.DownloadOp.Progress.BytesReceived - lastUiDownloaded;
                lastUiDownloaded = this.DownloadOp.Progress.BytesReceived;


                double scale = 1 - (uiUpdateSpeed / 1000);
                if (scale < 0)
                    scale = 1.0;


                lastUIUpdate = DateTime.Now;
                double downloadedPerSecond = (downloaded + (downloaded * scale));



                ulong size = (UInt64)downloadedPerSecond;
                if (size < 1024.0)
                    ret = string.Concat(Math.Round((double)size), " B");
                else if (size < 1048576.0)
                    ret = string.Concat((size / 1024.0).ToString("#.#"), " KB");
                else if (size < 1073741824.0)
                    ret = string.Concat((size / 1048576.0).ToString("#.#"), " MB");
                else if (size < 1099511627776.0)
                    ret = string.Concat((size / 1073741824.0).ToString("#.#"), " GB");
                else
                    ret = string.Concat((size / 1099511627776.0).ToString("#.#"), " TB");



                return string.Format("{0}/sec", ret);
            }
        }
        //-----------------
        public string DownloadedBytesString
        {
            get { return String.Format("{0:N0} / {1:N0}", this.CurrentFileSize, this.FinalFileSize); }
        }

        private string state;
        /// <summary>
        /// 상태 메시지
        /// </summary>
        public string ErrorState
        {
            get { return state; }
            set
            {
                state = value;
                NotifyPropertyChanged();
            }
        }














        /// <summary>
        /// 일시 정지
        /// </summary>
        public ICommand PauseCommand { get; private set; }

        /// <summary>
        /// 다시 시작
        /// </summary>
        public ICommand ResumeCommand { get; private set; }

        /// <summary>
        /// 취소 
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        public ICommand RestartCommand { get; private set; }

        private void CheckCanExecuteChanged()
        {
            (PauseCommand as DelegateCommand).RaiseCanExecuteChanged();
            (ResumeCommand as DelegateCommand).RaiseCanExecuteChanged();
            (CancelCommand as DelegateCommand).RaiseCanExecuteChanged();
            (RestartCommand as DelegateCommand).RaiseCanExecuteChanged();
        }

        public string Key
        {
            get { return LocalizedStrings.GetString (this.Status == BackgroundTransferStatus.Completed ? "Download_HeaderCompleted" : "Download_HeaderCurrent"); }
        }
    }
}
