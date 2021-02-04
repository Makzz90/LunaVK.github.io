using System;
using System.Collections.Generic;
using System.Text;
using LunaVK.Core.Network;
using Windows.Storage.Streams;
using LunaVK.Core.DataObjects;
using System.IO;
using Newtonsoft.Json;
using LunaVK.Core.Enums;

namespace LunaVK.Core.Library
{
    public class DocumentsService
    {
        private static DocumentsService _instance;
        public static DocumentsService Instance
        {
            get
            {
                if (DocumentsService._instance == null)
                    DocumentsService._instance = new DocumentsService();
                return DocumentsService._instance;
            }
        }

        public void GetDocuments(Action<VKResponse<VKCountedItemsObject<VKDocument>>> callback, int offset, int count, int ownerId = 0, VKDocumentType type = 0, bool return_tags = false)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["count"] = count.ToString();
            parameters["type"] = ((byte)type).ToString();
            
            if (offset > 0)
                parameters["offset"] = offset.ToString();
            if(return_tags)
                parameters["return_tags"] = "1";

            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKDocument>>("docs.get", parameters, callback);
        }

        public void UploadVoiceMessageDocument(byte[] Data, List<int> waveform, Action<VKAttachment> callback, Action<double> progressCallback = null)
        {
            DocumentsService.Instance.GetDocumentUploadServerCallback(0, "audio_message", (upload_url =>
            {
                if (upload_url == null)
                    callback(null);
                else
                {
                    JsonWebRequest.UploadVoiceMessage(upload_url, Data, "file", "file", waveform, (JsonString, result) =>
                    {
                        if (result)
                        {
                            UploadDocResponseData uploadData = JsonConvert.DeserializeObject<UploadDocResponseData>(JsonString);
                            if (string.IsNullOrEmpty(uploadData.error))
                                this.SaveDocument(uploadData, callback);
                            else
                                callback(null);
                        }
                        else
                            callback(null);
                        //SaveDocument(resp, callback);
                    }, "Voice Message", progressCallback);//todo: filename

                }
            }));
        }

        public void UploadGraffitiDocument(string fileName, string fileExtension, byte[] data, Action<VKAttachment> callback, Action<double> progressCallback = null)
        {
            this.GetDocumentUploadServer(0, "graffiti", (uploadUrl) =>
            {
                if (!string.IsNullOrEmpty(uploadUrl))
                {
                    string str = fileName.EndsWith(fileExtension) ? fileName : fileName + fileExtension;

                    JsonWebRequest.Upload(uploadUrl, data, "file", "file", (JsonString, jsonResult) =>
                    {
                        /*UploadDocResponseData
                        if (string.IsNullOrEmpty( jsonResult.error) )
                        {
                            this.SaveDocument(jsonResult, callback);
                        }
                        else
                            callback(null);*/
                        if (jsonResult)
                        {
                            UploadDocResponseData uploadData = JsonConvert.DeserializeObject<UploadDocResponseData>(JsonString);
                            if (string.IsNullOrEmpty(uploadData.error))
                                this.SaveDocument(uploadData, callback);
                            else
                                callback(null);
                        }
                        else
                            callback(null);
                    }, str, progressCallback);
                }
                else
                    callback(null);
            });
        }

        public void UploadDocument(int groupId, string fileName, string fileExtension, byte[] data, Action<VKAttachment> callback, Action<double> progressCallback = null)
        {
            this.GetDocumentWallUploadServer(groupId, (uploadUrlResponse) =>
            {
                if (string.IsNullOrEmpty(uploadUrlResponse))
                    callback(null);
                else
                {
                    string str = fileName.EndsWith(fileExtension) ? fileName : fileName + fileExtension;

                    JsonWebRequest.Upload(uploadUrlResponse, data, "file", "file", (JsonString, jsonResult) =>
                    {
                        if (jsonResult)
                        {
                            UploadDocResponseData uploadData = JsonConvert.DeserializeObject<UploadDocResponseData>(JsonString);
                            if (string.IsNullOrEmpty(uploadData.error))
                                this.SaveDocument(uploadData, callback);
                            else
                                callback(null);
                        }
                        else
                            callback(null);
                        //if (string.IsNullOrEmpty(uploadRes.error))
                        //{
                        //    this.SaveDocument(uploadRes, callback);
                        //}
                        //else
                        //    callback(null);
                    }, str, progressCallback);
                }
            });
        }

        private void GetDocumentUploadServer(string type, Action<string> callback, int optionalGroupId = 0)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (optionalGroupId != 0)
                parameters["group_id"] = optionalGroupId.ToString();

            if (!string.IsNullOrWhiteSpace(type))
                parameters["type"] = type;

            VKRequestsDispatcher.DispatchRequestToVK<UploadServerAddress>("docs.getUploadServer", parameters,(result)=> {
                callback(result.error.error_code == VKErrors.None ? result.response.upload_url : null);
            });
            
        }

        private void GetDocumentUploadServer(int optionalGroupId, string type, Action<string> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (optionalGroupId != 0L)
                parameters["group_id"] = optionalGroupId.ToString();
            if (!string.IsNullOrWhiteSpace(type))
                parameters["type"] = type;
            VKRequestsDispatcher.DispatchRequestToVK<UploadServerAddress>("docs.getUploadServer", parameters, (result) => {
                callback(result.error.error_code == VKErrors.None ? result.response.upload_url : null);
            });
        }

        public void GetDocumentUploadServerCallback(long optionalGroupId, string type, Action<string> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (optionalGroupId != 0)
                parameters["group_id"] = optionalGroupId.ToString();
            if (!string.IsNullOrWhiteSpace(type))
                parameters["type"] = type;
            VKRequestsDispatcher.DispatchRequestToVK<UploadServerAddress>("docs.getUploadServer", parameters, (result) => {
                callback(result.error.error_code == VKErrors.None ? result.response.upload_url : null);
            });
        }

        private void GetDocumentWallUploadServer(int optionalGroupId, Action<string> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (optionalGroupId != 0)
                parameters["group_id"] = optionalGroupId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<UploadServerAddress>("docs.getWallUploadServer", parameters, (result) => {
                callback(result.error.error_code == VKErrors.None ? result.response.upload_url : null);
            });
        }

        private void SaveDocument(UploadDocResponseData uploadData, Action<VKAttachment> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["file"] = uploadData.file;
            VKRequestsDispatcher.DispatchRequestToVK<VKAttachment>("docs.save", parameters, (result) => {
                callback(result.error.error_code == VKErrors.None ? result.response : null);
            });
        }

        public void Search(string q, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKDocument>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["q"] = q;
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKDocument>>("docs.search", parameters, callback);
        }

        public void Delete(int ownerId, uint documentId, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["doc_id"] = documentId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("docs.delete", parameters, callback);
        }

        public void Add(int ownerId, uint docId, string accessKey, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["doc_id"] = docId.ToString();
            if (!string.IsNullOrWhiteSpace(accessKey))
                parameters["access_key"] = accessKey;
            VKRequestsDispatcher.DispatchRequestToVK<int>("docs.add", parameters, callback);
        }

        public void Edit(int ownerId, uint id, string title, string tags, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["doc_id"] = id.ToString();
            parameters["title"] = title;
            parameters["tags"] = tags;
            VKRequestsDispatcher.DispatchRequestToVK<int>("docs.edit", parameters, callback);
        }


        public async void ReadFully(Windows.Storage.StorageFile file, Action<byte[]> callback)
        {//StreamUtils
            byte[] fileBytes = null;
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }

            callback(fileBytes);
        }
    }
}
