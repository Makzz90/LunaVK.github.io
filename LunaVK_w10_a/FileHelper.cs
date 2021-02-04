using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;

namespace LunaVK
{
    public static class FileHelper
    {
        /// <summary>
        /// Записывает указанный текст в файл и возвращает успешность операции.
        /// </summary>
        /// <param name="file">Файл для записи.</param>
        /// <param name="textToWrite">Текст для записи.</param>
        public static async Task<bool> WriteTextToFile<T>(string fileName, T obj)
        {
            try
            {
                StorageFile file = await CreateLocalFile(fileName);
                string textToWrite = SerializeToJson(obj);
                await FileIO.WriteTextAsync(file, textToWrite);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Считывает текст из переданного файла.
        /// </summary>
        /// <param name="file">Файл для считывания.</param>
        public static async Task<T> ReadTextFromFile<T>(string fileName)
        {
            try
            {
                StorageFile file = await GetLocalFileFromName(fileName);
                if (file != null)
                {
                    string temp = await FileIO.ReadTextAsync(file);
                    T ret = DeserializeFromJson<T>(temp);
                    return ret;
                }
            }
            catch (Exception)
            {
                
            }
            return default(T);
        }

        public static async Task ReadTextFromFile<T>(string fileName, Action<T> callback)
        {
            try
            {
                StorageFile file = await GetLocalFileFromName(fileName);
                if (file != null)
                {
                    string temp = await FileIO.ReadTextAsync(file);
                    T ret = DeserializeFromJson<T>(temp);
                    if (callback != null)
                        callback(ret);
                }
            }
            catch (Exception)
            {

            }
        }

        public static async void Remove(string fileName)
        {
            try
            {
                StorageFile file = await GetLocalFileFromName(fileName);
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
        /// Возвращает файл по его имени или null, если файл отсутствует.
        /// </summary>
        /// <param name="fileName">Имя файла, включая расширение.</param>
        public static async Task<StorageFile> GetLocalFileFromName(string fileName)
        {
            StorageFile ret = null;
            try
            {
                ret = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
            }
            catch(FileNotFoundException e)
            {
            }
            return ret;
        }

        /// <summary>
        /// Создает указанный файл в локальной папке приложения и возвращает его.
        /// Возвращает null при ошибке.
        /// </summary>
        /// <param name="fileName">Имя файла, включая расширение.</param>
        public static async Task<StorageFile> CreateLocalFile(string fileName)
        {
            try
            {
                return await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string SerializeToJson<T>(T obj)
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
        public static T DeserializeFromJson<T>(string json)
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
