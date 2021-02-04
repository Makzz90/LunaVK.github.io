using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Opusfile;
using LunaVK.Core;

namespace LunaVK.UC
{
    public sealed partial class AudioRecorderUC : UserControl
    {
        private MediaCapture Media;
        private AudioAmplitudeStream stream;
        public bool InRecord;
        private bool _isSending;
        private bool _isAnimating;
        public event EventHandler<VoiceMessageSentEvent> RecordDone;

        public int RecordDuration
        {
            get
            {
                TimeSpan timeSpan = DateTime.Now - _recordStartTime;
                return (int)timeSpan.TotalSeconds;
            }
        }

        /// <summary>
        /// Таймер для обновления текста секунд в записи
        /// </summary>
        DispatcherTimer DurationStrTimer = new DispatcherTimer();

        DispatcherTimer WaveTimer = new DispatcherTimer();
        private DateTime _recordStartTime;
        private readonly short[] _recordSamples = new short[1024];

        public AudioRecorderUC()
        {
            this.InitializeComponent();

            this.WaveTimer.Interval = TimeSpan.FromSeconds(0.5);
            this.WaveTimer.Tick += WaveTimer_Tick;
            this.DurationStrTimer.Interval = TimeSpan.FromSeconds(0.5);
            this.DurationStrTimer.Tick += DurationStr_Tick;
        }
        private double m = 20;
        private void WaveTimer_Tick(object sender, object e)
        {
            Border border = new Border();
            border.Width = 5;
            border.Background = new SolidColorBrush(Windows.UI.Colors.White);

            double percentage = DB / 20;
            double p2 = percentage/2  * 100;
            border.Height = Math.Max(1.0,m* p2/100);
//            System.Diagnostics.Debug.WriteLine(percentage + " => "+ p2);


            this.icontrol.Items.Insert(0,border);
        }

        void DurationStr_Tick(object sender, object e)
        {
            TimeSpan timeSpan = DateTime.Now - _recordStartTime;
            RecordDurationStr.Text = timeSpan.ToString("m\\:ss");
        }

        public void HandleManipulationCompleted()
        {
            if (this._isSending)
                return;//BugFix для ПК - у него дважды это происходит

            if (this.GridTr.TranslateX < -160.0)
            {
                this.StopRecordingData();
                this.IsOpened = false;
            }
            else
            {
                this.StopRecordingAndSend();
            }
        }

        public void HandleManipulationDelta(ManipulationDeltaRoutedEventArgs e)
        {
            double tr = e.Delta.Translation.X;
            if (this.GridTr.TranslateX + tr > 0.0)
                tr = 0.0;

            this.GridTr.TranslateX += tr;
            this.translateRecordDuration.X = this.GridTr.TranslateX * 0.5;

            if (this.GridTr.TranslateX < -160.0)
            {
                //  this.ShowHideCancelOverlay(false);
                this._iconRec.Glyph = "\xE711";
            }
            else
            {
                this._iconRec.Glyph = "\xF12E";
            }

            this.panelSlideToCancel.Opacity = GetSlideToCancelOpacity(this.GridTr.TranslateX);
        }

        public void HandleManipulationStarted()
        {
            this.IsOpened = true;
        }
        
        private double LastDB;
        private double DB;

        void stream_AmplitudeReading(object sender, double reading)
        {
            Execute.ExecuteOnUIThread(() =>
            {
                DB = this.ToDb(reading);
                LastDB = DB / 46.0;//делим на высоту элемента?
                double percentage = 1.5 * LastDB;
                this.scaleVolume.ScaleX = this.scaleVolume.ScaleY = percentage;
            });
        }

        double ToDb(double value)
        {
            if (value == 0)
                return 0;
            return 20 * Math.Log10(Math.Sqrt(value * 2));
        }

        private double ConvertAmplitudeToDb(double amplitude)
        {
            return 20.0 * Math.Log10(amplitude);
        }

        private double GetSlideToCancelOpacity(double translateX)
        {
            return translateX > -160.0 ? 1.0 - translateX / -160.0 : 0.0;
        }

        private async void StopRecordingData()
        {
            //if (this.InRecord && !this._isAnimating)
            //{
                this.DurationStrTimer.Stop();
                this.WaveTimer.Stop();

            if (this.InRecord)
            {
                await this.Media.StopRecordAsync();
                this.Media.Dispose();
            }

            this.InRecord = false;
            this.AnimScale(false);

            //if (this.GridTr.TranslateX < -160.0)
            //    this.Cancel();
            //else
            //{
            //    if (!_isSending)
            //        this.Send();
            //}
            //}
        }

        private async void Send()
        {
            this._isSending = true;

            StorageFile audioFileAsync = await GetAudioFileAsync();
            if (audioFileAsync == null)
                return;

            VoiceMessageSentEvent temp = new VoiceMessageSentEvent(audioFileAsync,this.RecordDuration, this.Waveform);
            this.RecordDone?.Invoke(this, temp);
            
            this._isSending = false;
            
            this.IsOpened = false;
        }

        private void AnimScale(bool show)
        {
            this._isAnimating = true;
            double num = show ? 2.0 : 0.0;

            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = this.GridTr,
                propertyPath = "ScaleX",
                from = this.GridTr.ScaleX,
                to = num,
                duration = 200
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = this.GridTr,
                propertyPath = "ScaleY",
                from = this.GridTr.ScaleY,
                to = num,
                duration = 200
            });
            AnimationUtils.AnimateSeveral(animInfoList, null, () => {
                this._isAnimating = false;
                if (!show)
                    this.UpdateVisibilityState();
            });
        }

        private List<int> GetWaveform(short[] sampleBuffer)
        {
            int[] numArray1 = new int[256];
            long num1 = 0;
            int num2 = 0;
            int num3 = Math.Max(1, sampleBuffer.Length / 128);
            int num4 = 0;
            int num5 = 0;
            foreach (short num6 in sampleBuffer)
            {
                num4 += (int)num6;
                if (num1++ % num3 == 0)
                {
                    int num7 = (int)Math.Round((double)Math.Abs(num4) / (double)num3);
                    numArray1[num5++] = num7;
                    num4 = 0;
                    if (num2 < num7)
                        num2 = num7;
                }
            }
            int[] numArray2 = new int[128];
            int num8 = num2 / 31;
            if (num8 > 0)
            {
                for (int index = 0; index < 128; ++index)
                    numArray2[index] = (int)Math.Min(31.0, Math.Round((double)numArray1[index] / (double)num8));
            }
            return Enumerable.ToList<int>(numArray2);
        }

        private List<int> Waveform
        {
            get
            {
                return this.GetWaveform(this._recordSamples);
            }
        }

        private async Task<StorageFile> GetAudioFileAsync()
        {
            this.InRecord = false;

            byte[] bytes = new byte[stream.Size];
            Stream ss = stream.AsStreamForRead();
            ss.Read(bytes, 0, (int)stream.Size);

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string new_path = CacheManager.NewGuid("temp.ogg");
            StorageFile storageFile = await localFolder.CreateFileAsync(new_path, CreationCollisionOption.ReplaceExisting);


            Microphone_OnBufferReady(bytes, storageFile.Path);
            return storageFile;
        }

        private void Microphone_OnBufferReady(byte[] _buffer, string path)
        {
            Opusfile.WindowsRuntime.OggOpusFile _opusComponent = new Opusfile.WindowsRuntime.OggOpusFile();

            int ii = _opusComponent.initRecorder(path);

            int data = _buffer.Length;
            int num1 = data / 1920;

            for (int index = 0; index < num1; ++index)
            {
                int length = 1920 * (index + 1) > _buffer.Length ? _buffer.Length - 1920 * index : 1920;
                var temp = SubArray<byte>(_buffer, 1920 * index, length);
                _opusComponent.writeFrame(temp, (uint)length);
            }

            _opusComponent.cleanupRecorder();







            double num2 = 0.0;
            long _samplesCount = 0;
            try
            {
                long num3 = _samplesCount + (long)(data / 2);
                int num4 = (int)((double)_samplesCount / (double)num3 * (double)this._recordSamples.Length);
                int num5 = this._recordSamples.Length - num4;
                if (num4 != 0)
                {
                    float num6 = (float)this._recordSamples.Length / (float)num4;
                    float num7 = 0.0f;
                    for (int index = 0; index < num4; ++index)
                    {
                        this._recordSamples[index] = this._recordSamples[(int)num7];
                        num7 += num6;
                    }
                }
                int index1 = num4;
                float num8 = 0.0f;
                float num9 = (float)data / 2f / (float)num5;
                int startIndex2 = 0;
                for (int index2 = 0; index2 < data / 2; ++index2)
                {
                    short int16 = BitConverter.ToInt16(_buffer, startIndex2);
                    double num6 = (double)int16 / 32768.0;
                    num2 += num6 * num6;
                    if (index2 == (int)num8 && index1 < this._recordSamples.Length)
                    {
                        this._recordSamples[index1] = int16;
                        num8 += num9;
                        ++index1;
                    }
                    startIndex2 += 2;
                }
                _samplesCount = num3;
            }
            catch (Exception ex)
            {
                //           Logger.Instance.Error("Audio record failure", ex);
            }
        }

        private T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] objArray = new T[length];
            Array.Copy(data, index, objArray, 0, length);
            return objArray;
        }
        
        public class AudioAmplitudeStream : IRandomAccessStream
        {
            private Stream m_InternalStream;

            public AudioAmplitudeStream()
            {
                MemoryStream stream = new MemoryStream();
                this.m_InternalStream = stream;
            }

            public bool CanRead
            {
                get { return true; }
            }

            public bool CanWrite
            {
                get { return true; }
            }

            public IRandomAccessStream CloneStream()
            {
                throw new NotImplementedException();
            }

            public IInputStream GetInputStreamAt(ulong position)
            {
                this.m_InternalStream.Seek((long)position, SeekOrigin.Begin);
                return this.m_InternalStream.AsInputStream();
            }

            public IOutputStream GetOutputStreamAt(ulong position)
            {
                this.m_InternalStream.Seek((long)position, SeekOrigin.Begin);
                return this.m_InternalStream.AsOutputStream();
            }

            public ulong Position
            {
                get { return (ulong)this.m_InternalStream.Position; }
            }

            public void Seek(ulong position)
            {
                this.m_InternalStream.Seek((long)position, 0);
            }

            public ulong Size
            {
                get { return (ulong)this.m_InternalStream.Length; }
                set { this.m_InternalStream.SetLength((long)value); }
            }

            public void Dispose()
            {
                this.m_InternalStream.Dispose();
            }

            public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer targetBuffer, uint count, InputStreamOptions options)
            {
                var inputStream = this.GetInputStreamAt(0);
                return inputStream.ReadAsync(targetBuffer, count, options);
            }

            public IAsyncOperation<bool> FlushAsync()
            {
                var outputStream = this.GetOutputStreamAt(0);
                return outputStream.FlushAsync();
            }

            public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
            {
                var outputStream = this.GetOutputStreamAt(this.Position);

                try
                {
                    if (AmplitudeReading != null)
                    {
                        var byteArray = buffer.ToArray();
                        var amplitude = Decode(byteArray).Select(Math.Abs).Average(x => x);
                        this.AmplitudeReading(this, amplitude);
                    }

                }
                catch (Exception e)
                {

                }

                return outputStream.WriteAsync(buffer);
            }

            private IEnumerable<Int16> Decode(byte[] byteArray)
            {
                for (var i = 0; i < byteArray.Length - 1; i += 2)
                {
                    yield return (BitConverter.ToInt16(byteArray, i));
                }
            }

            public delegate void AmplitudeReadingEventHandler(object sender, double reading);

            public event AmplitudeReadingEventHandler AmplitudeReading;

        }

        public class VoiceMessageSentEvent
        {
            public StorageFile File { get; set; }

            public int Duration { get; private set; }

            public List<int> Waveform { get; private set; }
            
            public VoiceMessageSentEvent(StorageFile file, int duration, List<int> waveform)
            {
                this.File = file;
                this.Duration = duration;
                this.Waveform = waveform;
            }
        }

        private void Stop_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.StopRecordingData();
        }

        private void Cancel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.StopRecordingData();
            this.IsOpened = false;
        }

        private void Send_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.RecordDuration > 1)
                this.StopRecordingAndSend();
            else
                this.IsOpened = false;
        }












        private void StopRecordingAndSend()
        {
            this.StopRecordingData();
            this.Send();
        }

        public async Task<bool> CanRecord()
        {
            bool ret = false;
            this.Media = new MediaCapture();
            try
            {
                //bug: если пользователь не дал доступ, то приложение крашится при развороте :(
                await this.Media.InitializeAsync(new MediaCaptureInitializationSettings() { StreamingCaptureMode = StreamingCaptureMode.Audio });
                this.Media.Dispose();
                this.Media = null;
                ret = true;
            }
            catch
            {
                this.Media = null;
            }
            return ret;
        }

        public bool IsHoldMode
        {
            get { return this.gridMobile.Visibility == Visibility.Visible; }
            set
            {
                if (value)
                {
                    this.gridDesktop.Visibility = Visibility.Collapsed;
                    this.gridMobile.Visibility = Visibility.Visible;
                }
                else
                {
                    this.gridDesktop.Visibility = Visibility.Visible;
                    this.gridMobile.Visibility = Visibility.Collapsed;
                }
            }
        }

        private bool _isOpened;
        public bool IsOpened
        {
            get
            {
                return this._isOpened;
            }
            set
            {
                if (value == this._isOpened)
                    return;

                this._isOpened = value;
                if (value)
                {
                    this.Open();
                }
                else
                {
                    this.Hide();
                }
            }
        }

        private void Open()
        {
            this.UpdateVisibilityState();
            this.ResetValues();
            this.StartRecording();
            
            
//            this.AnimScale(true); //this.AnimateOverlay(true);
        }

        private void Hide(bool animated = true)
        {
            if (animated)
                this.AnimScale(false);//this.AnimateOverlay(false);
            else
                this.UpdateVisibilityState();
        }
        
        private async void StartRecording()
        {
            this.InRecord = true;
            this.AnimScale(true);

            this.stream = new AudioAmplitudeStream();
            this.stream.AmplitudeReading += stream_AmplitudeReading;
            this._recordStartTime = DateTime.Now;
            this.DurationStrTimer.Start();
            this.WaveTimer.Start();
            


            var encodingProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Low);
            encodingProfile.Video = null;

            this.Media = new MediaCapture();
            await this.Media.InitializeAsync(new MediaCaptureInitializationSettings() { StreamingCaptureMode = StreamingCaptureMode.Audio });
            await this.Media.StartRecordToStreamAsync(encodingProfile, stream);
        }

        private void UpdateVisibilityState()
        {
            base.Visibility = (this._isOpened ? Visibility.Visible : Visibility.Collapsed);
        }

        private void ResetValues()
        {
            this.icontrol.Items.Clear();//удаляем ячейки
            this.InRecord = false;
            this._isSending = false;
            this.GridTr.TranslateX = 0;
            this.translateRecordDuration.X = 0;
            this.panelSlideToCancel.Opacity = 1.0;
            this.RecordDurationStr.Text = "0:00";
            this._iconRec.Glyph = "\xF12E";
        }
    }
}
