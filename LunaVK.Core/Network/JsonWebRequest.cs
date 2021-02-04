using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


using Windows.Storage.Streams;
using Windows.Storage;


using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Windows.Web.Http;

using Windows.Foundation;
using System.Threading;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;
using System.Diagnostics;

namespace LunaVK.Core.Network
{
    public class JsonWebRequest
    {
        private static int _currentNumberOfRequests;
        private static DelayedExecutorWithQueue _lowPriorityQueue = new DelayedExecutorWithQueue(600, (ei =>
        {
            if (JsonWebRequest._currentNumberOfRequests > 1)
                return (DateTime.Now - ei.TimestampAdded).TotalMilliseconds > 2000.0;
            return true;
        }));

        /// <summary>
        /// Используется для получения ответа в виде json-текста, для дальнейшей сериализации в класс
        /// </summary>
        /// <param name="baseUri"></param>
        /// <param name="parameters"></param>
        /// <param name="resultCallback"></param>
        /// <param name="usePost"></param>
        /// <param name="lowPriority"></param>
        /// <param name="cancellationToken"></param>
        public static void SendHTTPRequestAsync(string baseUri, Dictionary<string, string> parameters, Action<string, bool> resultCallback, bool usePost = true, bool lowPriority = false, CancellationToken? cancellationToken = null)
        {
            if (lowPriority)
            {
                /*
                Action<Action> action = (Action<Action>)(a => JsonWebRequest.SendHTTPRequestAsync(baseUri, parameters, (res =>
                {
                    a();
                    resultCallback(res);
                }), usePost, false));
                */
                JsonWebRequest._lowPriorityQueue.AddToDelayedExecutionQueue(()=>
                {
                    JsonWebRequest.SendHTTPRequestAsync(baseUri, parameters, ((res, IsSucceeded) => { resultCallback(res, IsSucceeded); }), usePost, false);
                });
            }
            else
            {
                //Logger.Instance.Info(">>>>>>>>>>>>>>>Starting GETAsync concurrentRequestsNo = {0} ; baseUri = {1}; parameters = {2}", JsonWebRequest._currentNumberOfRequests, baseUri, JsonWebRequest.GetAsLogString(parameters));

                
                /*
                string queryString = JsonWebRequest.ConvertDictionaryToQueryString(parameters, true);

                string requestUriString = baseUri;
                if (!usePost && queryString.Length > 0)
                    requestUriString = requestUriString + "?" + queryString;

                JsonWebRequest.RequestState myRequestState = new JsonWebRequest.RequestState();
                myRequestState.resultCallback = resultCallback;
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
                //myHttpWebRequest.UserAgent = AppInfo.AppVersionForUserAgent;
                myRequestState.request = myHttpWebRequest;
                if (usePost)
                {
                    myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
                    myHttpWebRequest.Method = "POST";
                    myHttpWebRequest.BeginGetRequestStream((ar =>
                    {
                        using (StreamWriter streamWriter = new StreamWriter(myHttpWebRequest.EndGetRequestStream(ar)))
                            streamWriter.Write(queryString);
                        myHttpWebRequest.BeginGetResponse(new AsyncCallback(JsonWebRequest.RespCallback), myRequestState);
                    }), null);
                }
                else
                    myHttpWebRequest.BeginGetResponse(new AsyncCallback(JsonWebRequest.RespCallback), myRequestState);
                */
                
                Action action = (async () =>
                {
                    try
                    {
                        HttpClient client = new HttpClient();
                        //Stopwatch stopwatch = new Stopwatch();
                        //stopwatch.Start();
                        var temp = client.PostAsync(new Uri(baseUri), new HttpFormUrlEncodedContent(parameters));
                        HttpResponseMessage resp = await temp.AsTask(cancellationToken.GetValueOrDefault());

                        cancellationToken?.Register(() =>
                        {
                            temp.Cancel();
#if DEBUG
                            Debug.WriteLine("AbortED0");
#endif
                        });

                        if (!resp.IsSuccessStatusCode)
                        {
                            resultCallback?.Invoke(null, false);
                            return;
                        }

                        string content = await resp.Content.ReadAsStringAsync();
                        //stopwatch.Stop();

                        //Debug.WriteLine(string.Format("{0} in {1} ms.", baseUri, stopwatch.ElapsedMilliseconds));

                        resultCallback?.Invoke(content, true);
                    }
                    catch (Exception ex)
                    {
                        resultCallback?.Invoke(null, false);
                    }
                });

                if(Windows.UI.Xaml.Window.Current!=null && Windows.UI.Xaml.Window.Current.Visible)
                    Task.Run(action);
                else
                    action();
            }
        }

        /// <summary>
        /// Используется для получения ответа в виде чистого текста
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="resultCallback"></param>
        /// <param name="postData"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="includeCookie"></param>
        public static void SendHTTPRequestAsync(string uri, Action<string, bool> resultCallback, Dictionary<string, string> postData = null, CancellationToken? cancellationToken = null, bool includeCookie = false, bool AllowAutoRedirect = true, bool includeUserAgent = false)
        {
            /*
            //Logger.Instance.Info("Starting GetJsonAsync for uri = {0}", uri);
            Interlocked.Increment(ref JsonWebRequest._currentNumberOfRequests);
            JsonWebRequest.RequestState myRequestState = new JsonWebRequest.RequestState();
            try
            {
                myRequestState.resultCallback = resultCallback;
                HttpWebRequest myHttpWebRequest = WebRequest.Create(uri) as HttpWebRequest;
                myRequestState.ctr = cancellationToken?.Register(() =>
                {
                    myHttpWebRequest.Abort();
                    //System.Diagnostics.Debug.WriteLine("AbortED0");
                });
                //Logger.Instance.Info("SendHTTPRequestAsync 6 {0}", postData == null);
                //myHttpWebRequest.UserAgent = AppInfo.AppVersionForUserAgent;
                
                myRequestState.request = myHttpWebRequest;
                myHttpWebRequest.AllowReadStreamBuffering = false;
                myHttpWebRequest.ContinueTimeout = 25000;
                
                if (includeCookie)
                {
                    myHttpWebRequest.CookieContainer = new CookieContainer();
                    using (var protocolFilter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter())
                    {
                        var cookieManager = protocolFilter.CookieManager;
                        var cookies = cookieManager.GetCookies(new Uri("https://vk.com"));
                        foreach (HttpCookie cookie in cookies)
                        {
                            string ccc = cookie.ToString();
#if DEBUG
                            if (Debugger.IsAttached)
                            {
                                Debug.WriteLine(ccc);
                            }
#endif
                            myHttpWebRequest.CookieContainer.SetCookies(new Uri("https://vk.com/"), ccc);//Работает, но куки сбиваются :(
                        }
                        
                    }
                    
                
                }
                
                if (postData != null)
                {
                    string boundary = string.Format("----------{0:N}", Guid.NewGuid());
                    string str = "multipart/form-data; boundary=" + boundary;
                    byte[] formData = JsonWebRequest.GetMultipartFormData(postData, boundary);
                    myHttpWebRequest.Method = "POST";
                    myHttpWebRequest.ContentType = str;
                    //myHttpWebRequest.CookieContainer = new CookieContainer();
                    myHttpWebRequest.BeginGetRequestStream((ar =>
                        {
                            Stream requestStream = myHttpWebRequest.EndGetRequestStream(ar);
                            requestStream.Write(formData, 0, formData.Length);
                            requestStream.Dispose();//requestStream.Close();
                            myHttpWebRequest.BeginGetResponse(new AsyncCallback(JsonWebRequest.RespCallback), myRequestState);
                        }), null);
                }
                else
                {
                    myHttpWebRequest.BeginGetResponse(new AsyncCallback(JsonWebRequest.RespCallback), myRequestState);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("GetJsonAsync failed.", ex);
                //JsonWebRequest.SafeClose(myRequestState);
                JsonWebRequest.SafeInvokeCallback(myRequestState.resultCallback, false, null);
            }
            finally
            {
                myRequestState.ctr?.Dispose();
            }
            */
            Task.Run(async () =>
            {
                try
                {
                    HttpClient client = null;
                    if (includeCookie)
                    {
                        var protocolFilter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
                        
                        var cookieManager = protocolFilter.CookieManager;
                        var cookies = cookieManager.GetCookies(new Uri("https://vk.com"));
                        foreach (HttpCookie cookie in cookies)
                        {
                            string ccc = cookie.ToString();
                            /*
#if DEBUG
                            if (Debugger.IsAttached)
                            {
                                Debug.WriteLine(ccc);
                            }
#endif
                            */
                            //myHttpWebRequest.CookieContainer.SetCookies(new Uri("https://vk.com/"), ccc);//Работает, но куки сбиваются :(
                        }

                        client = new HttpClient(protocolFilter);

                        if(includeUserAgent)
                            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:83.0) Gecko/20100101 Firefox/83.0");//для парсера видео полной версии страницы важно
                    }
                    else
                    {
                        var f = new Windows.Web.Http.Filters.HttpBaseProtocolFilter() { AllowAutoRedirect = AllowAutoRedirect };
                        client = new HttpClient(f);
                    }

                    //Stopwatch stopwatch = new Stopwatch();
                    //stopwatch.Start();
                    IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> temp;

                    if (postData != null)
                        temp = client.PostAsync(new Uri(uri), new HttpFormUrlEncodedContent(postData));
                    else
                        temp = client.GetAsync(new Uri(uri));
                    HttpResponseMessage resp = await temp.AsTask(cancellationToken.GetValueOrDefault());

                    cancellationToken?.Register(() =>
                    {
                        temp.Cancel();
#if DEBUG
                        Debug.WriteLine("AbortED0");
#endif
                    });

                    if (!resp.IsSuccessStatusCode)
                    {
                        if (AllowAutoRedirect == false)
                        {
                            if (resp.Headers.Location != null)
                            {
                                resultCallback?.Invoke(resp.Headers.Location.OriginalString, true);
                                return;
                            }
                        }
                        resultCallback?.Invoke(null, false);
                        return;
                    }

                    

                    string content = await resp.Content.ReadAsStringAsync();
                    //stopwatch.Stop();

                    //Debug.WriteLine(string.Format("{0} in {1} ms.", baseUri, stopwatch.ElapsedMilliseconds));

                    resultCallback?.Invoke(content, true);
                }
                catch(TaskCanceledException ex)
                {
                    int i = 0;
                    //todo: а надо делать Callback, если таск отменён?
                }
                catch (Exception ex)
                {
                    resultCallback?.Invoke(null, false);
                }
            });
        }

        private static void RespCallback(IAsyncResult asynchronousResult)
        {
            JsonWebRequest.RequestState asyncState = null;
            try
            {
                //Logger.Instance.Info("<<<<<<<<<<<<RespCallback 1 {0}", asynchronousResult != null);
                asyncState = (JsonWebRequest.RequestState)asynchronousResult.AsyncState;
                HttpWebRequest request = asyncState.request;
                var response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                //
                //Stream compressedResponseStream = asyncState.response.GetResponseStream();
                //asyncState.streamResponse = compressedResponseStream;
                //compressedResponseStream.BeginRead(asyncState.BufferRead, 0, 5000, new AsyncCallback(JsonWebRequest.ReadCallBack), asyncState);

                //Logger.Instance.Info("<<<<<<<<<<<<JSONWebRequest duration {0} ms. URI {1} ---->>>>> {2}", (DateTime.Now - asyncState.startTime).TotalMilliseconds, ((WebRequest)asyncState.request).RequestUri.OriginalString, stringContent);

                string stringContent = "";
                using (Stream compressedResponseStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(compressedResponseStream))
                    {
                        stringContent = reader.ReadToEnd();
                    }
                }
                
                asyncState.ctr?.Dispose();
                JsonWebRequest.SafeInvokeCallback(asyncState.resultCallback, true, stringContent);
                response.Dispose();//JsonWebRequest.SafeClose(asyncState);
            }
            catch (Exception ex)
            {
                //Logger.Instance.Info("<<<<<<<<<<<<RespCallback Exception {0}", ex.Message);
                //JsonWebRequest.SafeClose(asyncState);
                asyncState.ctr?.Dispose();
                JsonWebRequest.SafeInvokeCallback(asyncState.resultCallback, false, null);
            }
            }
        
        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream stream = new MemoryStream();
            bool flag = false;
            Encoding utF8 = Encoding.UTF8;
            foreach (KeyValuePair<string, object> postParameter in postParameters)
            {
                if (flag)
                    stream.Write(utF8.GetBytes("\r\n"), 0, utF8.GetByteCount("\r\n"));
                flag = true;
                /*
                if (postParameter.Value is JsonWebRequest.FileParameter)
                {
                    JsonWebRequest.FileParameter fileParameter = (JsonWebRequest.FileParameter)postParameter.Value;
                    string s = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n", boundary, postParameter.Key, (fileParameter.FileName ?? postParameter.Key), (fileParameter.ContentType ?? "application/octet-stream"));
                    stream.Write(utF8.GetBytes(s), 0, utF8.GetByteCount(s));
                    stream.Write(fileParameter.File, 0, fileParameter.File.Length);
                }
                else
                {*/
                    string s = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, postParameter.Key, postParameter.Value);
                    stream.Write(utF8.GetBytes(s), 0, utF8.GetByteCount(s));
                //}
            }
            string s1 = "\r\n--" + boundary + "--\r\n";
            stream.Write(utF8.GetBytes(s1), 0, utF8.GetByteCount(s1));
            stream.Position = 0L;
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Dispose();//stream.Close();
            return buffer;
        }

        private static void SafeInvokeCallback(Action<string, bool> action, bool p, string stringContent)
        {
            //Interlocked.Decrement(ref JsonWebRequest._currentNumberOfRequests);
            try
            {
                action(stringContent, p);
            }
            catch (Exception ex)
            {
                //Logger.Instance.Error("SafeInvokeCallback failed.", ex);
            }
        }

        private static string ConvertDictionaryToQueryString(Dictionary<string, string> parameters, bool escapeString = true)
        {
            if (parameters == null || parameters.Count == 0)
                return string.Empty;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                if (parameter.Key != null && parameter.Value != null)
                {
                    if (stringBuilder.Length > 0)
                        stringBuilder = stringBuilder.Append("&");
                    string str = escapeString ? Uri.EscapeDataString(parameter.Value) : parameter.Value;
                    stringBuilder = stringBuilder.AppendFormat("{0}={1}", new object[2] { parameter.Key, str });
                }
            }
            return stringBuilder.ToString();
        }

        private static string GetAsLogString(Dictionary<string, string> parameters)
        {
            string str = "";
            foreach (var parameter in parameters)
                str = str + parameter.Key + " = " + parameter.Value + Environment.NewLine;
            return str;
        }

        private class RequestState
        {
            //public StringBuilder requestData;
            //public List<byte> readBytes;
            //public byte[] BufferRead;
            public HttpWebRequest request;
            //public HttpWebResponse response;
            //public Stream streamResponse;
            public Action<string, bool> resultCallback;
            //public DateTime startTime;
            //
            public CancellationTokenRegistration? ctr;

            public RequestState()
            {
                //this.BufferRead = new byte[5000];
                //this.requestData = new StringBuilder("");
                //this.readBytes = new List<byte>(1024);
                this.request = null;
                //this.streamResponse = null;
                //this.startTime = DateTime.Now;
            }
        }

      





































        public static void DownloadToStream(string uri, IRandomAccessStream destinationStream, EventHandler<bool> resultCallback, EventHandler<double> progressCallback)
        {
            if (string.IsNullOrWhiteSpace(uri) || !uri.StartsWith("http"))
            {
                resultCallback(null, false);
            }
            else
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                request.BeginGetResponse((asyncRes =>
                {
                    bool flag = true;
                    try
                    {
                        var response = request.EndGetResponse(asyncRes);

                        using (Stream responseStream = response.GetResponseStream())
                        {
                            JsonWebRequest.CopyStream(responseStream, destinationStream, progressCallback, response.ContentLength);
                        }
                    }
                    catch (Exception)
                    {
                        flag = false;
                    }

                    resultCallback(null, flag);
                }), null);
            }
        }

        public static void Download(string uri, string destinationFile, EventHandler<bool> resultCallback, EventHandler<double> progressCallback)
        {
            //Stream destinationStream
            if (string.IsNullOrWhiteSpace(uri))
            {
                resultCallback?.Invoke(null, false);
            }
            else
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AllowReadStreamBuffering = false;
                request.BeginGetResponse((async asyncRes =>
                {
                    bool flag = true;
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asyncRes);
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                            StorageFile file = await localFolder.CreateFileAsync(destinationFile, CreationCollisionOption.ReplaceExisting);

                            Stream destinationStream = await file.OpenStreamForWriteAsync();
                            JsonWebRequest.CopyStream(responseStream, destinationStream, progressCallback, response.ContentLength);
                            if (destinationStream.CanSeek)
                                destinationStream.Position = 0;
                            destinationStream.Dispose();
                        }
                    }
                    catch (Exception)
                    {
                        flag = false;
                    }

                    resultCallback?.Invoke(null, flag);
                }), null);
            }
        }

        public static void Upload(string uri, byte[] data, string paramName, string uploadContentType, Action<string, bool> resultCallback, string fileName = null, Action<double> progressCallback = null, CancellationToken? cancellationToken = null)
        {
            
            JsonWebRequest.RequestState rState = new JsonWebRequest.RequestState() { resultCallback = resultCallback };
            
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                rState.ctr = cancellationToken?.Register(() =>
                {
                    request.Abort();
                    //System.Diagnostics.Debug.WriteLine("AbortED0");
                });

                //request.AllowWriteStreamBuffering = false;
                //request.UserAgent = AppInfo.AppVersionForUserAgent;
                rState.request = request;
                request.Method = "POST";
                string str1 = string.Format("----------{0:N}", Guid.NewGuid());
                string str2 = "multipart/form-data; boundary=" + str1;
                request.ContentType = str2;
                request.CookieContainer = new CookieContainer();
                string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n", str1, paramName, (fileName ?? "myDataFile"), uploadContentType);
                string footer = string.Format("\r\n--{0}--\r\n", str1);
                //request.ContentLength = ((long)Encoding.UTF8.GetByteCount(header) + data.Length + (long)Encoding.UTF8.GetByteCount(footer));
                request.BeginGetRequestStream((ar =>
                {
                    try
                    {
                        using (Stream requestStream = request.EndGetRequestStream(ar))
                        {
                            requestStream.Write(Encoding.UTF8.GetBytes(header), 0, Encoding.UTF8.GetByteCount(header));
                            StreamUtils.CopyStream(data, requestStream, progressCallback);
                            requestStream.Write(Encoding.UTF8.GetBytes(footer), 0, Encoding.UTF8.GetByteCount(footer));
                            //requestStream.Close();
                            request.BeginGetResponse(new AsyncCallback(JsonWebRequest.RespCallback), rState);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Error("Upload failed to write data to request stream.", ex);
                        //JsonWebRequest.SafeClose(rState);
                        JsonWebRequest.SafeInvokeCallback(rState.resultCallback, false, null);
                    }
                }), null);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Upload failed.", ex);
                //JsonWebRequest.SafeClose(rState);
                JsonWebRequest.SafeInvokeCallback(rState.resultCallback, false, null);
            }
        }
        static int b = 32768;//original
                             //static int b = 512;

        public static void UploadVoiceMessage(string uri, byte[] data, string paramName, string uploadContentType, List<int> waveform, Action<string, bool> resultCallback, string fileName = null, Action<double> progressCallback = null, CancellationToken? cancellationToken = null)
        {
            JsonWebRequest.RequestState rState = new JsonWebRequest.RequestState()
            {
                resultCallback = resultCallback
            };
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                //request.AllowWriteStreamBuffering = false;
                //request.UserAgent = AppInfo.AppVersionForUserAgent;
                rState.request = request;
                request.Method = "POST";
                string str1 = string.Format("----------{0:N}", Guid.NewGuid());
                string str2 = "multipart/form-data; boundary=" + str1;
                request.ContentType = str2;
                request.CookieContainer = new CookieContainer();
                string headerWaveform = "";
                if (waveform != null && waveform.Count > 0)
                {
                    headerWaveform += string.Format("--{0}\r\nContent-Disposition: form-data; name=\"waveform\"\r\n\r\n", str1);
                    headerWaveform += string.Format("[{0}]\r\n", string.Join<int>(",", waveform));
                }
                string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n", str1, paramName, (fileName ?? "myDataFile"), uploadContentType);
                string footer = string.Format("\r\n--{0}--\r\n", str1);
                //request.ContentLength = ((long)Encoding.UTF8.GetByteCount(header) + data.Length + (long)Encoding.UTF8.GetByteCount(footer));
                if (!string.IsNullOrEmpty(headerWaveform))
                {
                    HttpWebRequest httpWebRequest = request;
                    //long num = httpWebRequest.ContentLength + (long)Encoding.UTF8.GetByteCount(headerWaveform);
                    //httpWebRequest.ContentLength = num;
                }
                request.BeginGetRequestStream((ar =>
                {
                    try
                    {
                        using (Stream requestStream = request.EndGetRequestStream(ar))
                        {
                            if (!string.IsNullOrEmpty(headerWaveform))
                                requestStream.Write(Encoding.UTF8.GetBytes(headerWaveform), 0, Encoding.UTF8.GetByteCount(headerWaveform));
                            requestStream.Write(Encoding.UTF8.GetBytes(header), 0, Encoding.UTF8.GetByteCount(header));
                            StreamUtils.CopyStream(data, requestStream, progressCallback);
                            requestStream.Write(Encoding.UTF8.GetBytes(footer), 0, Encoding.UTF8.GetByteCount(footer));
                            //requestStream.Close();
                            request.BeginGetResponse(new AsyncCallback(JsonWebRequest.RespCallback), rState);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Error("Upload failed to write data to request stream.", ex);
                        //JsonWebRequest.SafeClose(rState);
                        JsonWebRequest.SafeInvokeCallback(rState.resultCallback, false, null);
                    }
                }), null);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Upload failed.", ex);
                //JsonWebRequest.SafeClose(rState);
                JsonWebRequest.SafeInvokeCallback(rState.resultCallback, false, null);
            }
        }

        private static void CopyStream(Stream input, IRandomAccessStream outputRandom, EventHandler<double> progressCallback = null, long inputLength = 0)
        {
            if (inputLength == 0)
            {
                try
                {
                    inputLength = input.Length;
                }
                catch (Exception)
                {
                }
            }

            Stream output = outputRandom.AsStreamForWrite();

            //
            byte[] buffer = new byte[b];
            int num = 0;
            int count;

            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (!output.CanWrite)
                    throw new Exception("failed to copy stream");
                //if (c != null && c.IsSet)
                //    throw new Exception("CopyStream cancelled");
                output.Write(buffer, 0, count);
                //outputRandom.WriteAsync(buffer.AsBuffer());
                num += count;
                if (progressCallback != null && inputLength > 0L)
                    progressCallback(null, (double)num * 100.0 / (double)inputLength);
            }
            output.Position = 0;
        }

        private static void CopyStream(Stream input, Stream output, EventHandler<double> progressCallback = null, long inputLength = 0)
        {
            if (inputLength == 0)
            {
                try
                {
                    inputLength = input.Length;
                }
                catch (Exception)
                {
                }
            }
            byte[] buffer = new byte[b];
            int num = 0;
            int count;
            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (!output.CanWrite)
                    throw new Exception("failed to copy stream");
                //if (c != null && c.IsSet)
                //    throw new Exception("CopyStream cancelled");
                output.Write(buffer, 0, count);
                num += count;
                if (progressCallback != null && inputLength > 0L)
                    progressCallback(null, (double)num * 100.0 / (double)inputLength);
            }
        }
    }
}
