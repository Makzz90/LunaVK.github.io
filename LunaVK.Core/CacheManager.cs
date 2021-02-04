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
using LunaVK.Core;
using System.IO.IsolatedStorage;
using LunaVK.Core.Utils;
using LunaVK.Core.Framework;

namespace LunaVK.Core
{
    public static class CacheManager
    {
        /*
         * кеш сообщений в папки ИД
         * кеш аватарок в ОБЩУЮ
         * кеш стикеров в ОБЩУЮ
         * кеш диалогов в папку ИД
         * настройки в ОБЩУЮ
         * кеш списка друзей в ИД
         * кеш списка музыки в ИД
         * кеш диалогов в ИД
         * кеш анимированных стикеров в ОБЩУЮ
         * кеш голосовых сообщений в ИД
         * */

        /// <summary>
        /// CachedData_id
        /// </summary>
        private static string _cacheFolderName = "CachedData_id";

        /// <summary>
        /// CachedData
        /// </summary>
        private static string _stateFolderName = "CachedData";
        /*
        public async Task<StorageFolder> GetCurrentUserCacheFolder()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            StorageFolder f = await localFolder.GetFolderAsync(_cacheFolderName+Settings.UserId);
            return f;
        }
        */
        /// <summary>
        /// Возвращает CachedData_id{000}\\{md5}
        /// </summary>
        /// <param name="fileUri"></param>
        /// <returns></returns>
        public static string NewGuid(string fileUri)
        {
            string md5 = ComputeMD5(fileUri);

            int dot = fileUri.LastIndexOf('.');
            if (dot <= 0)
            {
                return _cacheFolderName + Settings.UserId + "\\" + md5;

            }

            string ext = fileUri.Substring(dot);
            return _cacheFolderName + Settings.UserId + "\\" + md5 + ext;

        }

        public static async void SaveFileToCurrentUserCacheFolder(byte[] bytes, string fileUri)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string new_path = _cacheFolderName + Settings.UserId;
            await MakeFolders(localFolder, new_path);


            string md5 = ComputeMD5(fileUri);

            StorageFile storageFile = await localFolder.CreateFileAsync(new_path + "\\" + md5, CreationCollisionOption.ReplaceExisting);

            Stream outputStream = await storageFile.OpenStreamForWriteAsync();
            outputStream.Write(bytes, 0, bytes.Length);
            outputStream.Position = 0;
#if DEBUG
            System.Diagnostics.Debug.WriteLine("Write file done {0}", outputStream.Length);
#endif
            outputStream.Dispose();
            // return localFolder.Path + "\\" + new_path;
        }

        public static async Task<IRandomAccessStream> GetStreamInCurrentUserCacheFolder(string fileUri)
        {
            string file = NewGuid(fileUri);
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile f = await localFolder.GetFileAsync(file);
            var stream = await f.OpenAsync(Windows.Storage.FileAccessMode.Read);//IRandomAccessStream stream = await f.OpenReadAsync();
            return stream;
        }

        /// <summary>
        /// Создам файл и возврааем его
        /// но если он есть, просто возвращаем его
        /// </summary>
        /// <param name="fileUri"></param>
        /// <returns></returns>
        public static async Task<StorageFile> GetStorageFileInCurrentUserCacheFolder(string fileUri, CreationCollisionOption option = CreationCollisionOption.OpenIfExists)
        {
            string file = NewGuid(fileUri);
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            StorageFile f = await localFolder.CreateFileAsync(file, option);
            return f;
        }

        private static async Task MakeFolders(StorageFolder localFolder, string path)
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

        private static string ComputeMD5(string str)
        {
            var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
            var hashed = alg.HashData(buff);
            var res = CryptographicBuffer.EncodeToHexString(hashed);
            return res;
        }











        /// <summary>
        /// Возвращает строку вида "CachedData/файл" или "CachedData_id000/файл"
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="isUserData"></param>
        /// <param name="pathSeparator"></param>
        /// <returns></returns>
        public static string GetFilePath(string fileId, bool isUserData = true, string pathSeparator = "/")
        {
            //CachedData / ""
            //return CacheManager.GetFolderNameForDataType(dataType) + pathSeparator + fileId;
            return string.Format("{0}{1}{2}", isUserData ? (CacheManager._cacheFolderName + Settings.UserId) : CacheManager._stateFolderName, pathSeparator, fileId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj">Объект</param>
        /// <param name="fileId"></param>
        /// <param name="trim"></param>
        /// <param name="isUserData"></param>
        /// <returns></returns>
        public static bool TrySerialize(IBinarySerializable obj, string fileId, bool isUserData = true)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string filePath = CacheManager.GetFilePath(fileId, isUserData);
                    if (!storeForApplication.FileExists(filePath))
                        storeForApplication.CreateDirectory(Path.GetDirectoryName(filePath));
                    using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(filePath, FileMode.Create))
                    {
                        BinaryWriter writer = new BinaryWriter(storageFileStream);
                        obj.Write(writer);
                    }
                }
                stopwatch.Stop();
                Logger.Instance.Info("CacheManager.TrySerialize succeeded for fileId = {0}, in {1} ms.", fileId, stopwatch.ElapsedMilliseconds);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("CacheManager.TrySerialize failed. (" + obj.ToString() + "/" + fileId + ")", ex);
            }
            return false;
        }

        public static void TrySerializeAsync(IBinarySerializable obj, string fileId, bool isUserData = true)
        {
            //bool result = false;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Task.Run(async () =>
            {
                try
                {
                    StorageFolder rootDirectory = await ApplicationData.Current.LocalFolder.GetFolderAsync(isUserData ? (CacheManager._cacheFolderName + Settings.UserId) : CacheManager._stateFolderName);
                    using (Stream arg_FC_0 = await rootDirectory.OpenStreamForWriteAsync(fileId, CreationCollisionOption.ReplaceExisting))
                    {
                        BinaryWriter writer = new BinaryWriter(arg_FC_0);
                        obj.Write(writer);
                        //result = true;
                    }


                    Logger.Instance.Info("CacheManager.TrySerializeAsync succeeded for fileId = {0}, in {1} ms.", fileId, stopwatch.ElapsedMilliseconds);

                }
                catch (Exception ex)
                {
                    Logger.Instance.Error("CacheManager.TrySerializeAsync failed. (" + obj.ToString() + "/" + fileId + ")", ex);
                }
                finally
                {
                    stopwatch.Stop();
                }
            });
            //return result;
        }

        public static bool TryDeserialize(IBinarySerializable obj, string fileId, bool isUserData = true)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string filePath = CacheManager.GetFilePath(fileId, isUserData);
                    if (!storeForApplication.FileExists(filePath))
                        return false;
                    using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(filePath, FileMode.Open, FileAccess.Read))
                    {
                        BinaryReader reader = new BinaryReader(storageFileStream);
                        obj.Read(reader);
                    }
                }
                stopwatch.Stop();
                Logger.Instance.Info("CacheManager.TryDeserialize succeeded for fileId = {0}, in {1} ms.", fileId, stopwatch.ElapsedMilliseconds);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("CacheManager.TryDeserialize failed. (" + obj.ToString() + "/" + fileId + ")", ex);
                CacheManager.TryDelete(fileId, isUserData);
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fileId"></param>
        /// <param name="isUserData"></param>
        /// <returns>Файл существует</returns>
        public static async Task<bool> TryDeserializeAsync(IBinarySerializable obj, string fileId, bool isUserData = true)
        {
            bool result = false;
            //await Task.Run(async() =>//не было этой строки
            //{
            try
            {
                //C:\Users\Makzz\AppData\Local\Packages\132728F202473.LunaVK_tthxwnw0xv2tg\LocalState\CachedData_id460389
                StorageFolder rootDirectory = await ApplicationData.Current.LocalFolder.GetFolderAsync(isUserData ? (CacheManager._cacheFolderName + Settings.UserId) : CacheManager._stateFolderName);
                using (Stream arg_143_0 = await rootDirectory.OpenStreamForReadAsync(fileId))
                {
                    BinaryReader reader = new BinaryReader(arg_143_0);
                    obj.Read(reader);
                    result = true;
                }
            }
            catch (Exception var_8_15F)
            {
                Logger.Instance.Error("CacheManager.TryDeserializeAsync failed.", var_8_15F);
                result = false;
            }
            //});
            return result;
        }

        public static bool TryDelete(string fileId, bool isUserData = true)
        {
            try
            {
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string filePath = CacheManager.GetFilePath(fileId, isUserData);
                    if (!storeForApplication.FileExists(filePath))
                        return false;
                    storeForApplication.DeleteFile(filePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("CacheManager.TryDelete failed.", ex);
            }
            return false;
        }

        public static void EraseAll()
        {
            IsolatedStorageFile.GetUserStoreForApplication().Dispose();//remove
        }

        public static bool TrySaveRawCachedData(byte[] bytes, string fileId, FileMode fileMode, bool isUserData = true)
        {
            try
            {
                //new Stopwatch().Start();
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(CacheManager.GetFilePath(fileId, isUserData), fileMode))
                        storageFileStream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("CacheManager.TrySaveRawCachedData failed.", ex);
            }
            return false;
        }
    }
}
