using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using LunaVK.Core.Utils;
using LunaVK.Core.DataObjects;
using System.Net.Http;
using System.Net;
using System.IO.IsolatedStorage;

namespace LunaVK.Core.Framework
{
    public static class CacheManager2
    {
        /// <summary>
        /// CachedData_id
        /// </summary>
        private static string _cacheFolderName = "CachedData_id";

        /// <summary>
        /// Возвращает поток файла в кеше
        /// </summary>
        /// <param name="uri">Путь к файлу вида "Cache/Animated Stickers/"</param>
        /// <returns></returns>
        public static async Task<Stream> GetStreamOfCachedFile(string uri)
        {
            
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string new_path = uri.Replace("/", "\\");

            try
            {

                Stream p = await localFolder.OpenStreamForReadAsync(new_path);
                //p.Dispose();
                //return localFolder.Path + "\\" + new_path;
                return p;
            }
            catch (FileNotFoundException)
            {

            }

            return null;
            //await CacheManager2.MakeFolders(localFolder, uri);
        }

        /// <summary>
        /// Сохраняем в кеш файл из интернета
        /// </summary>
        /// <param name="url">Ссылка на файл в интернете</param>
        /// <param name="file_name">Путь к файлу вида "Cache/Animated Stickers/"</param>
        /// <returns>Файл в кеше</returns>
        public static async Task<StorageFile> WriteToCache(Uri url, string file_name)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            await CacheManager2.MakeFolders(localFolder, file_name);

            file_name = file_name.Replace("/", "\\");
            StorageFile storageFile = await localFolder.CreateFileAsync(file_name, CreationCollisionOption.ReplaceExisting);

            using (var fileStream = await storageFile.OpenStreamForWriteAsync())
            {
                HttpClientHandler handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };

                HttpClient client = new HttpClient(handler);
                /*
                var tt = await client.GetAsync(url);
                //var tt2 = tt.Content.ReadAsStringAsync();

                var bytes = await client.GetByteArrayAsync(url);
                var u1 = Encoding.Unicode.GetString(bytes);
                var u2 = Encoding.UTF32.GetString(bytes);
                var u3 = Encoding.UTF8.GetString(bytes);
                var u4 = Encoding.ASCII.GetString(bytes);
                var u5 = Encoding.BigEndianUnicode.GetString(bytes);

                


                string textToWrite = await client.GetStringAsync(url);



                await FileIO.WriteTextAsync(storageFile, textToWrite);
                */
                var httpStream = await client.GetStreamAsync(url);
                await httpStream.CopyToAsync(fileStream);

                fileStream.Dispose();
            }

            return storageFile;
        }
        /*
        public static async void WriteToCacheCallBack(Uri url, string file_name, Action<IRandomAccessStream> ret)
        {
            var cached_file = await OpenCachedFileCacheCallBack(file_name);
            if(cached_file!=null)
            {
                IRandomAccessStream new_stream0 = await cached_file.OpenReadAsync();
                ret(new_stream0);
                return;
            }

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            await CacheManager2.MakeFolders(localFolder, file_name);

            file_name = file_name.Replace("/", "\\");
            StorageFile storageFile = await localFolder.CreateFileAsync(file_name, CreationCollisionOption.ReplaceExisting);

            var fileStream = await storageFile.OpenStreamForWriteAsync();
            var refer = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(url);
            var rstream = await refer.OpenReadAsync();
            IRandomAccessStream new_stream = rstream.CloneStream();
            ret(new_stream);
            Stream stream = rstream.AsStreamForRead();
            
            StreamUtils.CopyStream(stream, fileStream, (p) =>
            {
                //Debug.WriteLine("Downloading: " +p+"%");
                //EventAggregator.Instance.PublishDownloadProgress()
            });

            
            fileStream.Dispose();
        }

        public static async void GetAudioCacheCallBack(VKAudio audio, Action<IRandomAccessStream> callbackStream)
        {
            string file_name = "Cache/Audio/" + ComputeMD5(audio.url) + ".mp3";
            var cached_file = await OpenCachedFileCacheCallBack(file_name);
            if (cached_file != null)
            {
                try
                {
                    IRandomAccessStream new_stream0 = await cached_file.OpenReadAsync();
                    callbackStream(new_stream0);
                    return;
                }
                catch
                {

                }
                
            }
            
            callbackStream(null);
            
        }

        public static async void WriteAudioToCacheCallBack(VKAudio audio, Action<double> callbackPercent)
        {
            Uri url = new Uri(audio.url);
            string file_name = "Cache/Audio/" + ComputeMD5(audio.url) + ".mp3";
            var cached_file = await OpenCachedFileCacheCallBack(file_name);
            if (cached_file != null)
            {
                try
                {
                    //IRandomAccessStream new_stream0 = await cached_file.OpenReadAsync();
                    //
                    //if (new_stream0.Size == 0)
                    //    await cached_file.DeleteAsync();
                    //
                    //callbackStream(new_stream0);
                    callbackPercent(99);
                    return;
                }
                catch
                {
                    
                }
                
                
            }

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            file_name = file_name.Replace("/", "\\");
            await CacheManager2.MakeFolders(localFolder, file_name);

            try
            {
                StorageFile storageFile = await localFolder.CreateFileAsync(file_name, CreationCollisionOption.FailIfExists);

                var fileStream = await storageFile.OpenStreamForWriteAsync();
                var refer = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(url);
                var rstream = await refer.OpenReadAsync();
                IRandomAccessStream new_stream = rstream.CloneStream();

                Stream stream = rstream.AsStreamForRead();

                StreamUtils.CopyStream(stream, fileStream, (p) =>
                {
                    callbackPercent(p);
                    if(p>=99)
                    {
                        fileStream.Dispose();
                    }
                });


                
            }
            catch
            {
                int i = 0;
            }
            
        }
        */
        
        public static async Task<StorageFile> OpenCachedFileCacheCallBack(string file_name)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            file_name = file_name.Replace("/", "\\");
            try
            {
                StorageFile storageFile = await localFolder.GetFileAsync(file_name);
                return storageFile;
            }
            catch
            {
                
            }

            return null;
        }


        public static async Task MakeFolders(StorageFolder localFolder, string path)
        {
            //pics/thumbnail/050/197/50197442.jpg
            int slash = path.IndexOf("/");
            if (slash <= 0) // -1 Not found
                return;

            string new_path = path.Substring(0, slash);
            StorageFolder opened_folder = await localFolder.CreateFolderAsync(new_path, CreationCollisionOption.OpenIfExists);
            string very_new_path = path.Remove(0, new_path.Length + 1);
            await MakeFolders(opened_folder, very_new_path);
        }

        public static string ComputeMD5(string str)
        {
            var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
            return CryptographicBuffer.EncodeToHexString(alg.HashData(buff));
        }

        /// <summary>
        /// Записывает в файл пользователя сериализованные данные
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName">Имя файла</param>
        /// <param name="obj">Данные для сериализации</param>
        public static /*async Task*/void WriteTextToFile<T>(string fileName, T obj)
        {
            

            
            try
            {
                /*
                StorageFile file = await CreateUserFile(fileName);
                string textToWrite = SerializeToJson(obj);
                await FileIO.WriteTextAsync(file, textToWrite);
                */
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string filePath = CacheManager.GetFilePath(fileName);
                    if (!storeForApplication.FileExists(filePath))
                        storeForApplication.CreateDirectory(Path.GetDirectoryName(filePath));
                    using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(filePath, FileMode.Create))
                    {
                        BinaryWriter writer = new BinaryWriter(storageFileStream);
                        string textToWrite = SerializeToJson(obj);
                        writer.WriteString(textToWrite);
                    }
                }
            }
            catch (Exception)
            {
            }
            
        }

        /// <summary>
        /// Считывает текст из переданного файла.
        /// </summary>
        /// <param name="file">Файл для считывания.</param>
        public static /*async Task<T>*/T ReadTextFromFile<T>(string fileName)
        {
            try
            {
                /*
                var folder = await GetCurrentUserCacheFolder();
                var file = await folder.GetFileAsync(fileName);
                if (file != null)
                {
                    string temp = await FileIO.ReadTextAsync(file);
                    T ret = DeserializeFromJson<T>(temp);
                    return ret;
                }
                */
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string filePath = CacheManager.GetFilePath(fileName);
                    if (storeForApplication.FileExists(filePath))
                    {
                        using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(filePath, FileMode.Open, FileAccess.Read))
                        {
                            BinaryReader reader = new BinaryReader(storageFileStream);
                            string temp = reader.ReadString();
                            T ret = DeserializeFromJson<T>(temp);
                            return ret;
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            return default(T);
        }

        private static async Task<StorageFile> CreateUserFile(string fileName)
        {
            try
            {
                var folder = await GetCurrentUserCacheFolder();
                return await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// LocalFolder + CachedData_id + ID
        /// </summary>
        /// <returns></returns>
        private static async Task<StorageFolder> GetCurrentUserCacheFolder()
        {
            StorageFolder f = await ApplicationData.Current.LocalFolder.CreateFolderAsync(_cacheFolderName + Settings.UserId, CreationCollisionOption.OpenIfExists);
            return f;
        }

        public static async void RemoveUserFile(string fileName)
        {
            try
            {
                var folder = await GetCurrentUserCacheFolder();
                var file = await folder.GetItemAsync(fileName);
                if (file != null)
                {
                    await file.DeleteAsync();
                }
            }
            catch (Exception)
            {
                int i = 0;
            }
        }

        /// <summary>
        /// Возвращает сириализованный объект
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string SerializeToJson<T>(T obj)
        {
            using (StringWriter writter = new StringWriter())
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writter, obj);
                return writter.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Десериализует Json-строку в объект указанного типа.
        /// </summary>
        /// <param name="json">Json-строка для десериализации.</param>
        private static T DeserializeFromJson<T>(string json)
        {
            using (StringReader reader = new StringReader(json))
            {
                using (JsonTextReader jsonReader = new JsonTextReader(reader))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    T result = serializer.Deserialize<T>(jsonReader);
                    return result;
                }
            }
        }
    }
}
