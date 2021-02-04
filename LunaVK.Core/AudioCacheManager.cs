using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Xaml;

namespace LunaVK.Core
{
    public class AudioCacheManager
    {
        /// <summary>
        /// Сопоставление трека с загруженным файлом
        /// </summary>
        public Dictionary<string, string> DownloadedDict { get; private set; }

        /// <summary>
        /// Информация о треках
        /// </summary>
        public List<VKAudio> DownloadedList { get; private set; }

        /// <summary>
        /// Пользовательские плейлисты
        /// </summary>
        public List<VKPlaylist> PlayLists { get; private set; }

        [JsonIgnore]
        ThreadPoolTimer timerSave;

        private static AudioCacheManager _instance;
        public static AudioCacheManager Instance
        {
            get
            {
                if (AudioCacheManager._instance == null)
                    AudioCacheManager._instance = new AudioCacheManager();
                
                return AudioCacheManager._instance;
            }
        }

        public AudioCacheManager()
        {
            DownloadedDict = new Dictionary<string, string>();
            DownloadedList = new List<VKAudio>();
            PlayLists = new List<VKPlaylist>();
        }

        public async Task Save()
        {
            /*await*/ CacheManager2.WriteTextToFile("AudioCacheManager", AudioCacheManager._instance);
        }

        public string GetLocalFileForUniqueId(string uniqueId)
        {
            //if (!AppGlobalStateManager.Current.GlobalState.IsMusicCachingEnabled)
            //    return null;
            if (this.DownloadedDict.ContainsKey(uniqueId))
                return this.DownloadedDict[uniqueId];
            return null;
        }

        public void SetLocalFileForUniqueId(VKAudio audio, string path)
        {
            this.DownloadedDict[audio.ToString()] = path;
            if (!this.DownloadedList.Contains(audio))
                this.DownloadedList.Add(audio);
        }

        public /*async*/ void RemoveLocalFile(VKAudio audio)
        {
            string id = audio.ToString();
            if(this.DownloadedDict.ContainsKey(id))
            {
                string path = this.DownloadedDict[id];
                //var file = await StorageFile.GetFileFromPathAsync(path);
                //await file.DeleteAsync();
                string file_name = CacheManager2.ComputeMD5(audio.ToString()) + ".mp3";//audio2000090396_456243309
                var cached_file = CacheManager.GetFilePath(file_name, false);

                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    try
                    {
                        if (storeForApplication.FileExists(cached_file))
                        {
                            storeForApplication.DeleteFile(cached_file);
                        }
                    }
                    catch
                    {

                    }
                }
                this.DownloadedDict.Remove(id);
                this.DownloadedList.Remove(audio);
                StartTimer();
            }
        }
        
        public async Task Init()
        {
            AudioCacheManager inst = /*await*/ CacheManager2.ReadTextFromFile<AudioCacheManager>("AudioCacheManager");
            if (inst != null)
                AudioCacheManager._instance = inst;
        }
        
        private void StartTimer()
        {
            if(this.timerSave!=null)
                this.timerSave.Cancel();
            this.timerSave = ThreadPoolTimer.CreatePeriodicTimer(_clockTimer_Tick, TimeSpan.FromSeconds(10));
        }

        private void _clockTimer_Tick(ThreadPoolTimer timer)
        {
            this.Save();
            timer.Cancel();
        }

        public async void WriteAudioToCacheCallBackString(VKAudio audio, Action<IRandomAccessStream> callbackFileName, Action<double> callbackPercent)
        {
            string file_name = CacheManager2.ComputeMD5(audio.ToString()) + ".mp3";//audio2000090396_456243309
            var cached_file = CacheManager.GetFilePath(file_name, false);

            using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    IsolatedStorageFileStream file = null;

                    if (storeForApplication.FileExists(cached_file))
                    {//Есть в кеше
                        file = storeForApplication.OpenFile(cached_file, FileMode.Open);
                        if(file.Length> 50)
                        {
                            callbackFileName(file.AsRandomAccessStream());
                            callbackPercent(100);
                            audio.UpdateUI();
                            this.SetLocalFileForUniqueId(audio, cached_file);
                            return;
                        }
                        
                    }

                    EventAggregator.Instance.PublishDownloadProgress(audio.ToString(),0.5);

                    storeForApplication.CreateDirectory("CachedData");
                    if(file==null)
                        file = storeForApplication.CreateFile(cached_file);

                    //callbackFileName(file.AsRandomAccessStream());

                    if(audio.IsUrlUnavailable)
                    {
                        AudioService.Instance.GetAudio(audio.owner_id, audio.id, async ( result) => {
                            if (result != null && result.error.error_code == Enums.VKErrors.None)
                            {
                                VKAudio a = result.response;

                                var refer = RandomAccessStreamReference.CreateFromUri(new Uri(a.url));
                                var rstream = await refer.OpenReadAsync();
                                callbackFileName(rstream);


                                JsonWebRequest.DownloadToStream(a.url, file.AsRandomAccessStream(), (s, result2) => { }, (s, progress) =>
                                {
                                    EventAggregator.Instance.PublishDownloadProgress(audio.ToString(), progress);
                                    callbackPercent(progress);
                                    if (progress == 100)
                                    {
                                        //fileStream.Dispose();
                                        //callbackStream(new_stream);//лучше возвращаться файл позже, т.к. плеер начинает лезть в стрим

                                        this.SetLocalFileForUniqueId(audio, cached_file);
                                        this.StartTimer();
                                        //file.Dispose();
                                    }
                                });
                            }
                            else
                            {
                                EventAggregator.Instance.PublishDownloadProgress(audio.ToString(), 0);
                                file.Dispose();
                            }

                        });
                    }
                    else
                    {
                        var refer = RandomAccessStreamReference.CreateFromUri(new Uri(audio.url));
                        var rstream = await refer.OpenReadAsync();
                        callbackFileName(rstream);

                        JsonWebRequest.DownloadToStream(audio.url, file.AsRandomAccessStream(), (s,result) => {
                            int i = 0;

                        }, (s,progress) => {
                            callbackPercent(progress);
                            if (progress == 100)
                            {
                                //fileStream.Dispose();
                                //callbackStream(new_stream);//лучше возвращаться файл позже, т.к. плеер начинает лезть в стрим

                                this.SetLocalFileForUniqueId(audio, cached_file);
                                this.StartTimer();

                                //file.Dispose();
                            }
                        });
                    }
                }
                catch
                {

                }
            }
        }
        
        /*
        public async void WriteAudioToCacheCallBackString(VKAudio audio, Action<string> callbackFileName, Action<double> callbackPercent)
        {


            //ШИКАРНЫЙ ВАРИАНТ

            //string file_name = "Cache/Audio/" + CacheManager2.ComputeMD5(audio.url) + ".mp3";
            string file_name = "Cache/Audio/" + CacheManager2.ComputeMD5(  audio.ToString()   ) + ".mp3";//audio2000090396_456243309
            var cached_file = await CacheManager2.OpenCachedFileCacheCallBack(file_name);
            if (cached_file != null)//Есть в кеше
            {
                try
                {
                    callbackFileName(cached_file.Path);
                    callbackPercent(100);

                    this.SetLocalFileForUniqueId(audio, cached_file.Path);

                    return;
                }
                catch
                {

                }
            }

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            file_name = file_name.Replace("/", "\\");
            await CacheManager2.MakeFolders(localFolder, file_name);

            StorageFile storageFile = null;

            try
            {
                Uri url = new Uri(audio.url);
                storageFile = await localFolder.CreateFileAsync(file_name, CreationCollisionOption.ReplaceExisting);
                var fileStream = await storageFile.OpenStreamForWriteAsync();
                var refer = RandomAccessStreamReference.CreateFromUri(url);
                var rstream = await refer.OpenReadAsync();
                callbackFileName(storageFile.Path);
                Stream stream = rstream.AsStreamForRead();

                StreamUtils.CopyStream(stream, fileStream, (p) =>
                {
                    callbackPercent(p);
                    if (p == 100)
                    {
                        fileStream.Dispose();
                        //callbackStream(new_stream);//лучше возвращаться файл позже, т.к. плеер начинает лезть в стрим

                        this.SetLocalFileForUniqueId(audio, storageFile.Path);
                        this.StartTimer();
                    }
                });



            }
            catch
            {
                if (storageFile != null)
                    storageFile.DeleteAsync();
                callbackFileName(null);
            }
        }
        */
    }
}
