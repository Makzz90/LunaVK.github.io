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

namespace LunaVK.Framework
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
        /// Возвращает CachedData_id000\\md5
        /// </summary>
        /// <param name="fileUri"></param>
        /// <returns></returns>
        public static string NewGuid(string fileUri)
        {
            string md5 = ComputeMD5(fileUri);

            int dot = fileUri.LastIndexOf('.');
            if(dot<=0)
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
        public static async Task<StorageFile> GetStorageFileInCurrentUserCacheFolder(string fileUri)
        {
            string file = NewGuid(fileUri);
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            StorageFile f = await localFolder.CreateFileAsync(file, CreationCollisionOption.OpenIfExists);
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
        public static bool TrySerialize(IBinarySerializable obj, string fileId, /*bool trim = false,*/ bool isUserData = true)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(CacheManager.GetFilePath(fileId, isUserData), FileMode.Create))
                    {
                        BinaryWriter writer = new BinaryWriter(storageFileStream);
                        //if (trim && obj is IBinarySerializableWithTrimSupport)
                        //    (obj as IBinarySerializableWithTrimSupport).WriteTrimmed(writer);
                        //else
                            obj.Write(writer);
                    }
                }
                stopwatch.Stop();
                Logger.Instance.Info("CacheManager.TrySerialize succeeded for fileId = {0}, in {1} ms.", fileId, stopwatch.ElapsedMilliseconds);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("CacheManager.TrySerialize failed.", ex);
            }
            return false;
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
                Logger.Instance.Error("CacheManager.TryDeserialize failed.", ex);
                CacheManager.TryDelete(fileId, isUserData);
            }
            return false;
        }

        public static bool TryDelete(string fileId, bool isUserData = true)
        {
            try
            {
                //new Stopwatch().Start();
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
    }


    /*
    public static class CacheManager
    {
        private static string _cacheFolderName = "CachedDataV4";
        private static string _stateFolderName = "CachedData";

        public static async void EnsureCacheFolderExists()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder CachedDataV4 = await localFolder.CreateFolderAsync(CacheManager._cacheFolderName, CreationCollisionOption.OpenIfExists);
            StorageFolder CachedData = await localFolder.CreateFolderAsync(CacheManager._stateFolderName, CreationCollisionOption.OpenIfExists);
        }

        public static async void EraseAll()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            await localFolder.DeleteAsync();
            //IsolatedStorageFile.GetUserStoreForApplication().Remove();
        }

        /// <summary>
        /// Возварщает путь для локального файла
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="dataType"></param>
        /// <param name="pathSeparator"></param>
        /// <returns></returns>
        public static string GetFilePath(string fileId, CacheManager.DataType dataType = CacheManager.DataType.CachedData, string pathSeparator = "/")
        {
            return CacheManager.GetFolderNameForDataType(dataType) + pathSeparator + fileId;
        }

        public static string GetFullFilePath(string fileId, CacheManager.DataType dataType = CacheManager.DataType.CachedData)
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, CacheManager.GetFilePath(fileId, dataType, "\\"));
        }

        public static string TrySerializeToString(IBinarySerializable obj)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BinaryWriter writer = new BinaryWriter((Stream)memoryStream);
                    obj.Write(writer);
                    memoryStream.Position = 0L;
                    return CacheManager.AsciiToString(new BinaryReader((Stream)memoryStream).ReadBytes((int)memoryStream.Length));
                }
            }
            catch (Exception ex)
            {
//                Logger.Instance.Error("TrySerializeToString.TryDeserialize failed.", ex);
            }
            return "";
        }

        public static void TryDeserializeFromString(IBinarySerializable obj, string serStr)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(CacheManager.StringToAscii(serStr)))
                {
                    BinaryReader reader = new BinaryReader((Stream)memoryStream);
                    obj.Read(reader);
                }
            }
            catch (Exception ex)
            {
//                Logger.Instance.Error("TrySerializeToString.TryDeserialize failed.", ex);
            }
        }

        public static byte[] StringToAscii(string s)
        {
            byte[] numArray = new byte[s.Length];
            for (int index = 0; index < s.Length; ++index)
            {
                char ch = s[index];
                numArray[index] = (int)ch > (int)sbyte.MaxValue ? (byte)63 : (byte)ch;
            }
            return numArray;
        }

        public static string AsciiToString(byte[] bytes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte num in bytes)
                stringBuilder = stringBuilder.Append((char)num);
            return stringBuilder.ToString();
        }

        public static async Task<bool> TryDeserialize(IBinarySerializable obj, string fileId, CacheManager.DataType dataType = CacheManager.DataType.CachedData)
        {
                try
                {
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    string filePath = CacheManager.GetFilePath(fileId, dataType, "/");

                    Stream storageFileStream = await localFolder.OpenStreamForReadAsync(filePath);
                    BinaryReader reader = new BinaryReader(storageFileStream);
                    obj.Read(reader);
                }
                catch
                {
                    return false;
                }
//                stopwatch.Stop();
//                Logger.Instance.Info("CacheManager.TryDeserialize succeeded for fileId = {0}, in {1} ms.", fileId, stopwatch.ElapsedMilliseconds);
                return true;
        }
       
        public static async Task<bool> TryDeleteAsync(string fileId)
        {
            bool result;
            try
            {
                await (await (await ApplicationData.Current.LocalFolder.GetFolderAsync(CacheManager.GetFolderNameForDataType(CacheManager.DataType.CachedData))).GetFileAsync(fileId)).DeleteAsync();
            }
            catch// (Exception var_5_16D)
            {
//                Logger.Instance.Error("CacheManager.TryDeleteAsync failed. File Id = " + fileId, var_5_16D);
                result = false;
                return result;
            }
            result = true;
            return result;
        }

        public static string GetFolderNameForDataType(CacheManager.DataType dataType)
        {
            if (dataType == CacheManager.DataType.CachedData)
                return CacheManager._cacheFolderName;
            if (dataType == CacheManager.DataType.StateData)
                return CacheManager._stateFolderName;
            throw new Exception("Unknown data type");
        }

        public static async Task<bool> TrySerializeAsync(IBinarySerializable obj, string fileId, bool trim = false, CacheManager.DataType dataType = CacheManager.DataType.CachedData)
        {
            bool result;
            try
            {
                Stream arg_FC_0 = await (await ApplicationData.Current.LocalFolder.GetFolderAsync(CacheManager.GetFolderNameForDataType(dataType))).OpenStreamForWriteAsync(fileId, CreationCollisionOption.ReplaceExisting);
                BinaryWriter writer = new BinaryWriter(arg_FC_0);
                if (trim && obj is IBinarySerializableWithTrimSupport)
                {
                    (obj as IBinarySerializableWithTrimSupport).WriteTrimmed(writer);
                }
                else
                {
                    obj.Write(writer);
                }

                arg_FC_0.Dispose(); //arg_FC_0.Close();
                result = true;
            }
            catch// (Exception var_5_140)
            {
//                Logger.Instance.Error("CacheManager.TrySerializeAsync failed.", var_5_140);
                result = false;
            }
            return result;
        }
       
        //
        //
        public static async Task<string> TrySaveRawCachedData(byte[] bytes, string fileId)
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                string new_path = CacheManager.GetFilePath(fileId, CacheManager.DataType.CachedData, "\\");
                await MakeFolders(localFolder, new_path);
                StorageFile storageFile = await localFolder.CreateFileAsync(new_path, CreationCollisionOption.ReplaceExisting);

                Stream outputStream = await storageFile.OpenStreamForWriteAsync();
                outputStream.Write(bytes, 0, bytes.Length);
                outputStream.Position = 0;
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Write file done {0}", outputStream.Length);
#endif
                outputStream.Dispose();
                return localFolder.Path + "\\" + new_path;
            }
            catch (Exception ex)
            {
                //                Logger.Instance.Error("CacheManager.TrySaveRawCachedData failed.", ex);
                int i = 0;
            }
            return null;
        }
        //
        //
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

        public static async Task<bool> MakeFile(string file)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            await MakeFolders(localFolder, file);
            StorageFile storageFile = await localFolder.CreateFileAsync(file, CreationCollisionOption.ReplaceExisting);
            return true;
        }

        public static async Task<bool> TrySerialize(IBinarySerializable obj, string fileId, bool trim = false, CacheManager.DataType dataType = CacheManager.DataType.CachedData)
        {
            try
            {

                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                string new_path = CacheManager.GetFilePath(fileId, CacheManager.DataType.CachedData, "/");
                StorageFile storageFile = await localFolder.CreateFileAsync(new_path, CreationCollisionOption.ReplaceExisting);
                Stream storageFileStream = await storageFile.OpenStreamForWriteAsync();
                BinaryWriter writer = new BinaryWriter(storageFileStream);
                if (trim && obj is IBinarySerializableWithTrimSupport)
                    (obj as IBinarySerializableWithTrimSupport).WriteTrimmed(writer);
                else
                    obj.Write(writer);

//                stopwatch.Stop();
//                Logger.Instance.Info("CacheManager.TrySerialize succeeded for fileId = {0}, in {1} ms.", fileId, stopwatch.ElapsedMilliseconds);
                return true;
            }
            catch (Exception ex)
            {
//                Logger.Instance.Error("CacheManager.TrySerialize failed.", ex);
            }
            return false;
        }
        
        /// <summary>
        /// Создаёт файл в кеше и возвращает поток для записи содержимого вовнутрь
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public static async Task<Stream> GetStreamForWrite(string fileId)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string new_path = CacheManager.GetFilePath(fileId, CacheManager.DataType.CachedData, "/");
            StorageFile storageFile = await localFolder.CreateFileAsync(new_path, CreationCollisionOption.ReplaceExisting);
            Stream storageFileStream = await storageFile.OpenStreamForWriteAsync();
            return storageFileStream;
            //using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
            //    return (Stream)storeForApplication.OpenFile(CacheManager.GetFilePath(fileId, CacheManager.DataType.CachedData, "/"), FileMode.Create);
        }

        public static async Task<Stream> GetStreamForRead(string fileId)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string new_path = fileId;//CacheManager.GetFilePath(fileId, CacheManager.DataType.CachedData, "/");
            Stream storageFileStream = await localFolder.OpenStreamForReadAsync(new_path);
            return storageFileStream;
        }
        
        public static async Task<StorageFile> GetFileAsync(string fileId)
        {
            StorageFile result;
            try
            {
                result = await StorageFile.GetFileFromPathAsync(CacheManager.GetFullFilePath(fileId, CacheManager.DataType.CachedData));
                return result;
            }
            catch// (Exception var_3_7B)
            {
 //               Logger.Instance.Error("CacheManager.GetFileAsync failed.", var_3_7B);
            }
            result = null;
            return result;
        }


        public static async Task<bool> TryDelete(string fileId, CacheManager.DataType dataType = CacheManager.DataType.CachedData)
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                string filePath = CacheManager.GetFilePath(fileId, dataType, "/");
                StorageFile f = await localFolder.GetFileAsync(filePath);
                if(f==null)
                    return false;
                await f.DeleteAsync();
                return true;
            }
            catch (Exception ex)
            {
//                Logger.Instance.Error("CacheManager.TryDelete failed.", ex);
            }
            return false;
        }

        public enum DataType
        {
            CachedData,
            StateData,
        }
    }
    */
}
