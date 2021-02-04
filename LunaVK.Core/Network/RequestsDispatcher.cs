using System;
using System.Collections.Generic;
using System.Text;
using LunaVK.Core.DataObjects;
using System.Net;
using System.IO;
using System.Threading;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.UI.Popups;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using LunaVK.Core.Enums;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LunaVK.Core.Network
{
    public static class RequestsDispatcher
    {
        static HttpClient httpClient;
        //static string Referer = "";

        public static async Task<string> DoWebRequestString(string url, List<KeyValuePair<string, string>> post = null, CancellationToken ct = default(CancellationToken))
        {
            string ret = "";

            var f = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
            var manager = f.CookieManager;

            using (var client = new Windows.Web.Http.HttpClient(f))
            {
                try
                {
                    Windows.Web.Http.HttpResponseMessage response = null;
                    if (post == null)
                    {
                        response = await client.GetAsync(new Uri(url));
                    }
                    else
                    {
                        response = await client.PostAsync(new Uri(url), new Windows.Web.Http.HttpFormUrlEncodedContent(post));
                    }
                    //if (AllowAutoRedirect == false)
                    //{
                    //    if (response.Headers.Location != null)
                    //        return response.Headers.Location.OriginalString;
                    //}
                    ret = await response.Content.ReadAsStringAsync();
                }
                catch (Exception) { }
            }
            /*
            if (RequestsDispatcher.httpClient == null)
            {
                CookieContainer cookies = new CookieContainer();

                HttpClientHandler handler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    CookieContainer = cookies,
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                };

                httpClient = new HttpClient(handler);

#if !WINDOWS_UWP
                //return string.Format("Mozilla/5.0 ({0}; ARM; Touch; NOKIA; Lumia 1520) like Gecko", AppInfo.OSTypeAndVersion);
                return "Mozilla/5.0 (Windows Phone 8.1; ARM; Trident/8.0; Touch; WebView/2.0; rv:11.0; IEMobile/11.0; NOKIA; Lumia 1520) like Gecko";
                httpClient.DefaultRequestHeaders.Add("User-Agent", AppInfo.AppVersionForUserAgent);
                
#endif
            }
            try
            {
                HttpResponseMessage response = null;
                if (post == null)
                {
                    response = await httpClient.GetAsync(url, ct);
                }
                else
                {
                    HttpContent httpContent = new FormUrlEncodedContent(post);
                    response = await httpClient.PostAsync(url, httpContent, ct);
                }
                ret = await response.Content.ReadAsStringAsync();
            }
            catch
            {

            }
            */

            return ret;
        }

        public static void GetResponseFromDump<T>(int millisecondsDelay,string file_name, Action< VKResponse<T>> callback, Func<string, string> customDeserializationFunc = null)
        {
            Task.Run(async () =>
            {
                await Task.Delay(millisecondsDelay);

                var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets\\Json");
                Windows.Storage.StorageFile file = await folder.GetFileAsync(file_name);

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
                string jsonResp = Encoding.UTF8.GetString(fileBytes);//Convert.ToString(fileBytes);

                if (customDeserializationFunc != null)
                    jsonResp = customDeserializationFunc(jsonResp);

                VKResponse<T> response = JsonConvert.DeserializeObject<VKResponse<T>>(jsonResp);
                callback?.Invoke(response);
            });
        }

        /// <summary>
        /// Делает POST-запрос указанному ресурсу указанных параметров и 
        /// асинхронно возвращает результат.
        /// </summary>
        /// <param name="requestURL">URL запроса.</param>
        /// <param name="parameters">Словарь передаваемых параметров.</param>
        /// <param name="timeout">Таймаут операции.</param>
        /// <param name="ct">Токен отмены.</param>
        public static async Task<string> PostAsync(string requestURL, Dictionary<string, string> parameters, bool AllowAutoRedirect = true)
        {
            string result = String.Empty;
            
            var f = new Windows.Web.Http.Filters.HttpBaseProtocolFilter() { AllowAutoRedirect = AllowAutoRedirect };
            var manager = f.CookieManager;
            
            using (var client = new Windows.Web.Http.HttpClient(f))
            {
                try
                {
                    var response = await client.PostAsync(new Uri(requestURL), new Windows.Web.Http.HttpFormUrlEncodedContent(parameters));
                    if (AllowAutoRedirect == false)
                    {
                        if (response.Headers.Location != null)
                            return response.Headers.Location.OriginalString;
                    }
                    result = await response.Content.ReadAsStringAsync();
                }
                catch (Exception) { }
            }
            return result;
        }

        
        public async static void DispatchLoginRequest(string login, string password, Action<VKErrors, string> callback)
        {
#region Delete cookie
            Windows.Web.Http.Filters.HttpBaseProtocolFilter myFilter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
            var cookieManager = myFilter.CookieManager;

            Windows.Web.Http.HttpCookieCollection myCookieJar = cookieManager.GetCookies(new Uri("https://www.vk.com/"));
            foreach (Windows.Web.Http.HttpCookie cookie in myCookieJar)
                cookieManager.DeleteCookie(cookie);
#endregion



            //GET https://oauth.vk.com/authorize?client_id=5674548&scope=audio,friends,docs,groups,messages,notes,notifications,notify,offline,pages,photos,stats,status,video,wall&redirect_uri=https://oauth.vk.com/blank.html&display=mobile&v=5.54&response_type=token

            /*
            // Шаг 1: получаем страницу входа (на ней важны скрытые поля у кнопки входа)
            string requestUriString = string.Format("https://oauth.vk.com/authorize?client_id={0}&scope={1}&redirect_uri={2}&display=mobile&v={3}&response_type=token", Constants.ApplicationID, Constants.Scope, Constants.Redirect, Constants.API_VERSION);
            string html = await RequestsDispatcher.DoWebRequestString(requestUriString);
            
                if(string.IsNullOrEmpty(html))
                {
                    callback(VKErrors.NoNetwork, "Нет доступа к интернету");
                    return;
                }

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                //if (doc.DocumentNode == null)
                //    return;

                HtmlNode c1 = doc.DocumentNode.ChildNodes["html"];
                HtmlNode body = c1.ChildNodes["body"];

                IEnumerable<HtmlNode> bodyNode = body.Descendants().ToList();

                // Шаг 2: парсим скрытые поля

                //<input type="hidden" name="_origin" value="https://oauth.vk.com">
                var hidden_items = bodyNode.Where((d) =>
                {
                    if (d.Attributes.Contains("type") && d.Name == "input")
                    {
                        return d.Attributes["type"].Value == "hidden";
                    }
                    return false;
                });

                Dictionary<string, string> parameters = new Dictionary<string, string>();

                foreach (HtmlNode item in hidden_items)
                {

                    //<div class="audio_item ai_has_btn" id="audio460389_456239093_audios460389"
                    string name = item.Attributes["name"].Value;
                    string value = item.Attributes["value"].Value;
                    parameters.Add(name, value);
                }

                //<div class="iwrap"><input type="text" class="textfield" name="email" value="" /></div>
                //<div class="iwrap"><input type="password" class="textfield" name="pass" /></div>
                var textbox_items = bodyNode.Where((d) =>
                {
                    if (d.Attributes.Contains("type") && d.Name == "input")
                    {
                        return d.Attributes["type"].Value == "text" || d.Attributes["type"].Value == "password";
                    }
                    return false;
                });

                if (textbox_items.Count() == 2)
                {
                    parameters.Add("email", login);
                    parameters.Add("pass", password);
                }

                // Отправляем наш логин и пароль
                html = await RequestsDispatcher.PostAsync("https://login.vk.com/?act=login&soft=1&utf8=1", parameters);
                //JsonWebRequest.SendHTTPRequestAsync("https://login.vk.com/?act=login&soft=1&utf8=1", (html2, result2) =>
                //{
                    //Возможны варианты: пользователь ввёл данные неверно
                    //Приложению будут доступны:
                    //if (result2 == false)
                    if(string.IsNullOrEmpty(html))
                    {
                        callback(VKErrors.NoNetwork, string.Empty);
                        return;
                    }

                    doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    //if (doc.DocumentNode == null)
                    //    return;

                    c1 = doc.DocumentNode.ChildNodes["html"];
                    body = c1.ChildNodes["body"];

                    bodyNode = body.Descendants().ToList();

                    //<div class="service_msg service_msg_warning">Указан неверный логин или пароль.</div>
                    var service_msg = bodyNode.FirstOrDefault((d) =>
                    {
                        if (d.Attributes.Contains("class") && d.Name == "div")
                        {
                            return d.Attributes["class"].Value.StartsWith("service_msg");
                        }
                        return false;
                    });

                    if (service_msg != null)
                    {
                        callback(VKErrors.AccessDenied, service_msg.InnerText.Trim());
                        return;
                    }

                    // Действие: разрешить приложению
                    //<form method="post" action="https://login.vk.com/?act=grant_access
                    var action_items = bodyNode.Where((d) =>
                    {
                        if (d.Attributes.Contains("action") && d.Name == "form")
                        {
                            return true;
                        }
                        return false;
                    });

                    //ИЛИ <form method="post" action="/login?act=authcheck_code
                    if (action_items.Count() != 1)
                        return;
                    
                    string action = action_items.ElementAt(0).Attributes["action"].Value;
                    if (action.Contains("grant_access"))
                    {
                        parameters = new Dictionary<string, string>();
                        html = await RequestsDispatcher.PostAsync(action, parameters, false);

                        if (string.IsNullOrEmpty(html))
                        {
                            callback(VKErrors.NoNetwork, string.Empty);
                            return;
                        }

                        Regex QueryStringRegex = new Regex("access_token=(?<access_token>.+)&.+user_id=(?<user_id>\\d+)");
                        Match m = QueryStringRegex.Match(html);
                        string access_token = m.Groups["access_token"].Value;
                        uint user_id = uint.Parse(m.Groups["user_id"].Value);
                        //callback(new AutorizationData() { AccessToken = access_token, UserId = user_id }, VKErrors.None);
                        callback(VKErrors.None, html);
                    }
                    else if (action.Contains("authcheck_code"))
                    {
                        //_ajax=1&code=037216
                        string text = await InputTextDialogAsync("Код подтверждения входа:");
                        parameters = new Dictionary<string, string>();
                        //parameters.Add("_ajax","1");
                        parameters.Add("code", text);
                        html = await RequestsDispatcher.PostAsync("https://m.vk.com" + action, parameters);

                        doc = new HtmlDocument();
                        doc.LoadHtml(html);

                        if (doc.DocumentNode == null)
                        {
                            callback(VKErrors.AccessDenied, string.Empty);
                            return;
                        }

                        c1 = doc.DocumentNode.ChildNodes["html"];
                        body = c1.ChildNodes["body"];

                        bodyNode = body.Descendants().ToList();



                        // Действие: разрешить приложению
                        //<form method="post" action="https://login.vk.com/?act=grant_access
                        action_items = bodyNode.Where((d) =>
                        {
                            if (d.Attributes.Contains("action") && d.Name == "form")
                            {
                                return true;
                            }
                            return false;
                        });

                        if (action_items.Count() != 1)
                        {
                            callback(VKErrors.InternalServerError, string.Empty);
                            return;
                        }

                        action = action_items.ElementAt(0).Attributes["action"].Value;
                        if (action.Contains("grant_access"))
                        {
                            parameters = new Dictionary<string, string>();
                            html = await RequestsDispatcher.PostAsync(action, parameters, false);

                            if (string.IsNullOrEmpty(html))
                                return;

                            Regex QueryStringRegex = new Regex("access_token=(?<access_token>.+)&.+user_id=(?<user_id>\\d+)");
                            Match m = QueryStringRegex.Match(html);
                            string access_token = m.Groups["access_token"].Value;
                            uint user_id = uint.Parse(m.Groups["user_id"].Value);
                            //callback(new AutorizationData() { AccessToken = access_token, UserId = user_id }, VKErrors.None);
                            callback(VKErrors.None, html);
                        }
                    }
                    */
            
        }

        private static async Task<string> InputTextDialogAsync(string title)
        {
            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Ok";
            dialog.SecondaryButtonText = "Cancel";
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return inputTextBox.Text;
            else
                return "";
        }
    }
}
