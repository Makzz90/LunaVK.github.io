using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace LunaVK.Network
{
    public class WNS_Helper
    {
        private OAuthToken accessToken = new OAuthToken();

        // Post to WNS
        public async Task<string> PostToWns(string secret, string sid, string uri, string xml, string notificationType, string contentType)
        {
            try
            {
                // You should cache this access token.
                if(string.IsNullOrEmpty( accessToken.access_token))
                {
                    GetAccessToken(secret, sid);
                    return "In auth";
                }
                

                byte[] contentInBytes = Encoding.UTF8.GetBytes(xml);

                //string url = System.Net.WebUtility.UrlDecode(uri);//декодировать нельзя т.к. сервер майков не найдёт канал
                HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
                request.Method = "POST";
                request.Headers["X-Wns-Type"] = notificationType;
                request.Headers[HttpRequestHeader.Authorization] = String.Format("Bearer {0}", accessToken.access_token );

                if(!string.IsNullOrEmpty(contentType))
                    request.ContentType = contentType;

                using (Stream requestStream = await request.GetRequestStreamAsync())
                    requestStream.Write(contentInBytes, 0, contentInBytes.Length);

                using (HttpWebResponse webResponse = (HttpWebResponse)await request.GetResponseAsync())
                {
                    Stream s = webResponse.GetResponseStream();
                    byte[] buffer = new byte[webResponse.ContentLength];
                    s.Read(buffer, 0, (int)webResponse.ContentLength);
                    string str0 = Encoding.UTF8.GetString(buffer, 0, (int)webResponse.ContentLength);

                    //return webResponse.StatusCode.ToString();
                    string[] debugOutput = {
                                       webResponse.StatusCode.ToString(),
                                       webResponse.Headers["X-WNS-Debug-Trace"],
                                       webResponse.Headers["X-WNS-Error-Description"],
                                       webResponse.Headers["X-WNS-Msg-ID"],
                                       webResponse.Headers["X-WNS-Status"],
                                   };
                    


                    return string.Join(" | ", debugOutput);
                }
            }

            catch (WebException webException)
            {
                HttpStatusCode status = ((HttpWebResponse)webException.Response).StatusCode;

                if (status == HttpStatusCode.Unauthorized)
                {
                    // The access token you presented has expired. Get a new one and then try sending
                    // your notification again.

                    // Because your cached access token expires after 24 hours, you can expect to get 
                    // this response from WNS at least once a day.

                    this.GetAccessToken(secret, sid);

                    // We recommend that you implement a maximum retry policy.
                    //              return PostToWns(uri, xml, secret, sid, notificationType, contentType);
                    return status.ToString();
                }
                else if (status == HttpStatusCode.Gone || status == HttpStatusCode.NotFound)
                {
                    // The channel URI is no longer valid.

                    // Remove this channel from your database to prevent further attempts
                    // to send notifications to it.

                    // The next time that this user launches your app, request a new WNS channel.
                    // Your app should detect that its channel has changed, which should trigger
                    // the app to send the new channel URI to your app server.

                    return status.ToString();
                }
                else if (status == HttpStatusCode.NotAcceptable)
                {
                    // This channel is being throttled by WNS.

                    // Implement a retry strategy that exponentially reduces the amount of
                    // notifications being sent in order to prevent being throttled again.

                    // Also, consider the scenarios that are causing your notifications to be throttled. 
                    // You will provide a richer user experience by limiting the notifications you send 
                    // to those that add true value.

                    return status.ToString();
                }
                else
                {
                    // WNS responded with a less common error. Log this error to assist in debugging.

                    // You can see a full list of WNS response codes here:
                    // http://msdn.microsoft.com/en-us/library/windows/apps/hh868245.aspx#wnsresponsecodes

                    string[] debugOutput = {
                                       status.ToString(),
                                       webException.Response.Headers["X-WNS-Debug-Trace"],
                                       webException.Response.Headers["X-WNS-Error-Description"],
                                       webException.Response.Headers["X-WNS-Msg-ID"],
                                       webException.Response.Headers["X-WNS-Status"]
                                   };
                    return string.Join(" | ", debugOutput);
                }
            }

            catch (Exception ex)
            {
                return "EXCEPTION: " + ex.Message;
            }
        }


        

        private OAuthToken GetOAuthTokenFromJson(string jsonString)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonString)))
            {
                var ser = new DataContractJsonSerializer(typeof(OAuthToken));
                var oAuthToken = (OAuthToken)ser.ReadObject(ms);
                return oAuthToken;
            }
        }

        protected async void GetAccessToken(string secret, string sid)
        {
            var urlEncodedSecret = WebUtility.UrlEncode(secret);
            var urlEncodedSid = WebUtility.UrlEncode(sid);

            var body = String.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=notify.windows.com",
                                     urlEncodedSid,
                                     urlEncodedSecret);

            string ret = "";
            using (var client = new HttpClient())
            {
                ByteArrayContent byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes(body));

                byteArrayContent.Headers.Remove("Content-Type");
                byteArrayContent.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                byteArrayContent.Headers.ContentLength = ((long)Encoding.UTF8.GetByteCount(body));

                HttpResponseMessage response = await client.PostAsync(new Uri("https://login.live.com/accesstoken.srf"), byteArrayContent);
                byte[] o = await response.Content.ReadAsByteArrayAsync();
                ret = Encoding.UTF8.GetString(o, 0, o.Length);

                //response = client.UploadString("https://login.live.com/accesstoken.srf", body);
            }

            this.accessToken = GetOAuthTokenFromJson(ret);
            //return GetOAuthTokenFromJson(ret);
        }



        // Authorization
        [DataContract]
        public class OAuthToken
        {
            [DataMember]
            public string access_token { get; set; }

            [DataMember]
            public string token_type { get; set; }
        }
    }
}
