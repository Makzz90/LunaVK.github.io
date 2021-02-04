using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using LunaVK.Core.DataObjects;
using Windows.UI.Xaml.Shapes;
using LunaVK.Framework;

using Windows.Storage;
using Windows.Storage.Streams;
using System.Text;
using LunaVK.Core.Framework;
using Opusfile;
using LunaVK.Core.Network;
using LunaVK.Core;

namespace LunaVK.UC.Attachment
{
    public sealed partial class AttachVoiceMessageUC : UserControl
    {
        //private int max = 0;
        //private double wave_width = 0;
        private DispatcherTimer _timerPlayback;

        public AttachVoiceMessageUC()
        {
            this.InitializeComponent();
            this.media.CurrentStateChanged += media_CurrentStateChanged;

            this._timerPlayback = new DispatcherTimer();
            this._timerPlayback.Interval = TimeSpan.FromMilliseconds(10.0);
            this._timerPlayback.Tick += _timerPlayback_Tick;

            this.Loaded += AttachVoiceMessageUC_Loaded;
        }

        void AttachVoiceMessageUC_Loaded(object sender, RoutedEventArgs e)
        {
            this._waveformControl.Waveform = this.VM.waveform;
            this._waveformControl.Width = this._waveformControl.WaveformWidth = 200;

            this._waveformControl.Minimum = 0;
            this._waveformControl.Value = this._waveformControl.Maximum = this.VM.duration * 1000;
            

            this._waveformControl.ManipulationMode = ManipulationModes.TranslateX;
            this._waveformControl.ManipulationCompleted += SliderPosition_OnManipulationCompleted;
            this._waveformControl.ManipulationDelta += _waveformControl_ManipulationDelta;
            //this._waveformControl.ValueChanged += _waveformControl_ValueChanged;

            this.ResetDurationString();
        }

        void _waveformControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var pos = e.Position;
            FrameworkElement element = sender as FrameworkElement;
            double percent = pos.X / element.ActualWidth;
            double num = this.VM.duration * percent;
            this.media.Position = TimeSpan.FromMilliseconds(num);
        }

        void SliderPosition_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            double num = this._waveformControl.Value;
            this.media.Position = TimeSpan.FromMilliseconds(num);
        }
        /*
        void _waveformControl_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            double percent = e.NewValue / this._waveformControl.Maximum;
            this.media.Position = TimeSpan.FromMilliseconds(this.VM.duration * percent);
        }
        */
        private DocPreview.DocPreviewVoiceMessage VM
        {
            get { return this.DataContext as DocPreview.DocPreviewVoiceMessage; }
        }

        public AttachVoiceMessageUC(DocPreview.DocPreviewVoiceMessage a)
            : this()
        {
            this.DataContext = a;
        }

        void media_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            MediaElement m = sender as MediaElement;
            if (m.CurrentState == MediaElementState.Playing)
            {
                this._playIcon.Glyph = "\xE769";
                this._timerPlayback.Start();
            }
            else if (m.CurrentState == MediaElementState.Paused || m.CurrentState == MediaElementState.Stopped)
            {
                this._playIcon.Glyph = "\xEDDA";
                this._timerPlayback.Stop();
            }
            else if (m.CurrentState == MediaElementState.Buffering || m.CurrentState == MediaElementState.Opening)
            {
                this._playIcon.Glyph = "\xF16A";
            }
        }

        void _timerPlayback_Tick(object sender, object e)
        {
            this.UpdatePlayerPosition();
        }

        private void SetDurationString(TimeSpan timeSpan)
        {
            this._textBlockDuration.Text = this.GetDurationString(timeSpan);
        }

        private void ResetDurationString()
        {
            DocPreview.DocPreviewVoiceMessage a = this.VM;
            this._textBlockDuration.Text = this.GetDurationString(TimeSpan.FromSeconds(a.duration));
        }

        private string GetDurationString(TimeSpan timeSpan)
        {
            return timeSpan.ToString(timeSpan.Hours > 0 ? "h\\:m\\:ss" : "m\\:ss");
        }

        private void UpdatePlayerPosition()
        {
            //if (!this.IsCurrentPlayer)
            //    return;
            //if (this.Source == null)
            //    return;
            try
            {
                TimeSpan position = this.media.Position;
                if (position.TotalMilliseconds <= 0.0)
                    return;
                //Action<TimeSpan> positionUpdated = this.PositionUpdated;
                //if (positionUpdated == null)
                //    return;
                //TimeSpan timeSpan = position;
                //positionUpdated(timeSpan);
                this._waveformControl.Value = position.TotalMilliseconds;
                this.SetDurationString(position);
            }
            catch
            {
            }
        }

        int maximum = 0;

        /// <summary>
        /// Нажата кнопка воспроизведения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DocPreview.DocPreviewVoiceMessage a = this.VM;
            //this.media.Source = new Uri(a.link_mp3);
            //this.media.Play();
            /*
             * Сохраняем огг
             * открываем опусом
             * сохраняем вав
             * открываем вав
             * */
            if (!string.IsNullOrEmpty(a.link_ogg))
                PlayPause(a.link_ogg);
        }

        public void PlayPause(string uri)
        {
            if (this.media.CurrentState == MediaElementState.Playing)
            {
                this.media.Pause();
                return;
            }

            if (this.media.PlayToSource != null)
            {
                this.media.Play();
                return;
            }

            JsonWebRequest.Download(uri, CacheManager.NewGuid(uri), async (s, res) =>
            {
                if (res == true)
                {

                    Opusfile.WindowsRuntime.OggOpusFile temp = new Opusfile.WindowsRuntime.OggOpusFile();
                    var stream = await CacheManager.GetStreamInCurrentUserCacheFolder(uri);
                    temp.Open(stream);
                    Opusfile.WindowsRuntime.OpusHead head = temp.Head();

                    bool num1;
                    MemoryStream data = new MemoryStream();

                    do
                    {
                        byte[] buffer0 = new byte[16384];

                        var t = temp.Read(buffer0.Length);

                        if (t.Length == 0)
                            break;

                        t.CopyTo(buffer0);
                        data.Write(buffer0, 0, (int)t.Length);
                        num1 = t.Length <= 0;
                    }
                    while (num1 == false);

                    MemoryStream str = GetWavAsMemoryStream(data, 44000, head.ChannelCount, 16);
                    StorageFile outFile = await CacheManager.GetStorageFileInCurrentUserCacheFolder(uri + ".wav");

                    await FileIO.WriteBytesAsync(outFile, str.ToArray());

                    var outStream = await CacheManager.GetStreamInCurrentUserCacheFolder(uri + ".wav");

                    Execute.ExecuteOnUIThread(() =>
                    {
                        this.media.SetSource(outStream, outFile.ContentType);
                        this.media.Play();
                    });

                }

            }, this.Progress);
        }

        private void Progress(object sender, double percent)
        {
            Execute.ExecuteOnUIThread(() => { this.ring.Progress = percent; });
        }

        private MemoryStream GetWavAsMemoryStream(Stream data, int sampleRate, int audioChannels, int bitsPerSample)
        {
            MemoryStream memoryStream = new MemoryStream();
            WriteHeader(memoryStream, sampleRate, audioChannels, bitsPerSample);
            SeekPastHeader(memoryStream);
            data.Position = 0;
            data.CopyTo(memoryStream);
            UpdateHeader(memoryStream);
            return memoryStream;
        }

        private void WriteHeader(Stream stream, int sampleRate, int audioChannels = 1, int bitsPerSample = 16)
        {
            int num = bitsPerSample / 8;
            Encoding utF8 = Encoding.UTF8;
            long position = stream.Position;
            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(utF8.GetBytes("RIFF"), 0, 4);
            stream.Write(BitConverter.GetBytes(0), 0, 4);
            stream.Write(utF8.GetBytes("WAVE"), 0, 4);
            stream.Write(utF8.GetBytes("fmt "), 0, 4);
            stream.Write(BitConverter.GetBytes(16), 0, 4);
            stream.Write(BitConverter.GetBytes(1), 0, 2);
            stream.Write(BitConverter.GetBytes((short)audioChannels), 0, 2);
            stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);
            stream.Write(BitConverter.GetBytes(sampleRate * num * audioChannels), 0, 4);
            stream.Write(BitConverter.GetBytes((short)num), 0, 2);
            stream.Write(BitConverter.GetBytes((short)bitsPerSample), 0, 2);
            stream.Write(utF8.GetBytes("data"), 0, 4);
            stream.Write(BitConverter.GetBytes(0), 0, 4);
            UpdateHeader(stream);
            stream.Seek(position, SeekOrigin.Begin);
        }

        private void SeekPastHeader(Stream stream)
        {
            if (!stream.CanSeek)
                throw new Exception("Can't seek stream to update wav header");
            stream.Seek(44L, SeekOrigin.Begin);
        }

        private void UpdateHeader(Stream stream)
        {
            if (!stream.CanSeek)
                throw new Exception("Can't seek stream to update wav header");
            long position = stream.Position;
            stream.Seek(4L, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes((int)stream.Length - 8), 0, 4);
            stream.Seek(40L, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes((int)stream.Length - 44), 0, 4);
            stream.Seek(position, SeekOrigin.Begin);
        }
    }
}
