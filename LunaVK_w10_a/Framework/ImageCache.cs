using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
//using Windows.Security.Cryptography.Core.HashAlgorithmNames;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using LunaVK.Core.Utils;
using System.IO.IsolatedStorage;

namespace LunaVK.Framework
{
    public class ImageCache// : IBinarySerializable
    {
        public static string COMMUNITY_IMAGE = "https://vk.com/images/community";
        private Dictionary<string, string> _uriToLocalPathDict = new Dictionary<string, string>();
        private object _lockObj = new object();
//        private readonly SHA1 _hasher = (SHA1)new SHA1Managed();
        private static ImageCache _current;

        public static ImageCache Current
        {
            get
            {
                if (ImageCache._current == null)
                    ImageCache._current = new ImageCache();
                return ImageCache._current;
            }
        }

        public bool HasImageInCache(string uri)
        {
            return this._uriToLocalPathDict.ContainsKey(uri);
        }

        public async Task<IRandomAccessStream> GetCachedImageStream(string uriString)
        {
            if (uriString == null)
                return null;
            string empty = string.Empty;
//            if (uriString.EndsWith("gif"))
//                return (!uriString.StartsWith(ImageCache.COMMUNITY_IMAGE) ? (!uriString.StartsWith("https://vk.com/images/deactivated") ? (!uriString.StartsWith("https://vk.com/images/contact") ? (!uriString.Contains("_null") ? Application.GetResourceStream(new Uri("/VKClient.Common;component/Resources/Photo_Placeholder.png", UriKind.RelativeOrAbsolute)) : Application.GetResourceStream(new Uri("/VKClient.Common;component/Resources/Empty1x1.png", UriKind.RelativeOrAbsolute))) : Application.GetResourceStream(new Uri("/VKClient.Common;component/Resources/EmailUser.png", UriKind.RelativeOrAbsolute))) : Application.GetResourceStream(new Uri("/VKClient.Common;component/Resources/deactivatedUser.png", UriKind.RelativeOrAbsolute))) : Application.GetResourceStream(new Uri("/VKClient.Common;component/Resources/community_100.png", UriKind.RelativeOrAbsolute))).Stream;
            lock (this._lockObj)
            {
                if (this._uriToLocalPathDict.ContainsKey(uriString))
                    empty = this._uriToLocalPathDict[uriString];
            }
            if (!string.IsNullOrEmpty(empty))
            {
                //IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication();
                //if (storeForApplication.FileExists(empty))
                //    return (Stream)storeForApplication.OpenFile(empty, FileMode.Open, FileAccess.Read);
                try
                {
                    var localFile = await ApplicationData.Current.LocalFolder.GetFileAsync(empty);
                    return await localFile.OpenReadAsync();
                }
                catch
                {

                }
            }
            return null;
        }

        public async void TryRemoveUri(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return;
            try
            {
                string localPathBy = this.GetLocalPathBy(uri);
                //IsolatedStorageFile.GetUserStoreForApplication().DeleteFile(localPathBy);
                //
                var localFile = await ApplicationData.Current.LocalFolder.GetFileAsync(localPathBy);
                await localFile.DeleteAsync();
                //
                lock (this._lockObj)
                    this._uriToLocalPathDict.Remove(localPathBy);
            }
            catch (Exception ex)
            {
 //               Logger.Instance.Error("TryRemoveUri failed", ex);
            }
        }

        public bool TrySetImageForUri(string uriString, Stream responseStream)
        {
            if (responseStream != null)
            {
                if (!string.IsNullOrWhiteSpace(uriString))
                {
                    try
                    {
                        string localPathBy = uriString;//this.GetLocalPathBy(uriString);
                        using (IsolatedStorageFileStream storageFileStream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile(localPathBy, FileMode.Create, FileAccess.Write))
                            responseStream.CopyTo(storageFileStream);
                        lock (this._lockObj)
                            this._uriToLocalPathDict[uriString] = localPathBy;
                        return true;
                    }
                    catch (Exception ex)
                    {
//                        Logger.Instance.Error("TrySetImageForUri", ex);
                    }
                    return false;
                }
            }
            return false;
        }

        private string GetLocalPathBy(string uriString)
        {/*
            byte[] bytes = Encoding.UTF8.GetBytes(uriString);
            byte[] hash;
            lock (this._hasher)
                hash = this._hasher.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "");*/
            return uriString.GetShorterVersion();
        }

        public void Write(BinaryWriter writer)
        {
//            writer.WriteDictionary(this._uriToLocalPathDict);
        }

        public void Read(BinaryReader reader)
        {
//            this._uriToLocalPathDict = reader.ReadDictionary();
        }
    }
}
