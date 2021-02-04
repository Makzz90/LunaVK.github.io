using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LunaVK.Library
{
    public class YoutubeExtractor
    {
        public class VideoInfo
        {

        }

        /// <summary>
        /// Normalizes the given YouTube URL to the format http://youtube.com/watch?v={youtube-id}
        /// and returns whether the normalization was successful or not.
        /// </summary>
        /// <param name="url">The YouTube URL to normalize.</param>
        /// <param name="normalizedUrl">The normalized YouTube URL.</param>
        /// <returns>
        /// <c>true</c>, if the normalization was successful; <c>false</c>, if the URL is invalid.
        /// </returns>
        public static bool TryNormalizeYoutubeUrl(string url, out string normalizedUrl)
        {
            url = url.Trim();

            url = url.Replace("youtu.be/", "youtube.com/watch?v=");
            url = url.Replace("www.youtube", "youtube");
            url = url.Replace("youtube.com/embed/", "youtube.com/watch?v=");

            if (url.Contains("/v/"))
            {
                url = "http://youtube.com" + new Uri(url).AbsolutePath.Replace("/v/", "/watch?v=");
            }

            url = url.Replace("/watch#", "/watch?");

            IDictionary<string, string> query = ParseQueryString(url);

            string v;

            if (!query.TryGetValue("v", out v))
            {
                normalizedUrl = null;
                return false;
            }

            normalizedUrl = "http://youtube.com/watch?v=" + v;

            return true;
        }

        public static string UrlDecode(string url)
        {
            return System.Net.WebUtility.UrlDecode(url);
        }

        public static IDictionary<string, string> ParseQueryString(string s)
        {
            // remove anything other than query string from url
            if (s.Contains("?"))
            {
                s = s.Substring(s.IndexOf('?') + 1);
            }

            if (s.Contains("?"))
            {
                s = s.Substring(0, s.IndexOf('?'));
            }

            var dictionary = new Dictionary<string, string>();

            foreach (string vp in Regex.Split(s, "&"))
            {
                string[] strings = Regex.Split(vp, "=");
                dictionary.Add(strings[0], strings.Length == 2 ? UrlDecode(strings[1]) : string.Empty);
            }

            return dictionary;
        }

        private static JObject LoadJson(string url)
        {
            string pageSource = DownloadString(url);

            if (IsVideoUnavailable(pageSource))
            {
                throw new Exception();
            }

            var dataRegex = new Regex(@"ytplayer\.config\s*=\s*(\{.+?\});", RegexOptions.Multiline);

            string extractedJson = dataRegex.Match(pageSource).Result("$1");

            return JObject.Parse(extractedJson);
        }

        private static bool IsVideoUnavailable(string pageSource)
        {
            const string unavailableContainer = "<div id=\"watch-player-unavailable\">";

            return pageSource.Contains(unavailableContainer);
        }

        private static string ReadStreamFromResponse(WebResponse response)
        {
            using (Stream responseStream = response.GetResponseStream())
            {
                using (var sr = new StreamReader(responseStream))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public static string DownloadString(string url)
        {
            var request = WebRequest.Create(url);
            request.Method = "GET";

            System.Threading.Tasks.Task<WebResponse> task = System.Threading.Tasks.Task.Factory.FromAsync(
                request.BeginGetResponse,
                asyncResult => request.EndGetResponse(asyncResult),
                null);

            return task.ContinueWith(t => ReadStreamFromResponse(t.Result)).Result;
        }

        internal static class Url
        {
            public static string SetQueryParameter(string url, string key, string value)
            {
                if(value==null)
                    value = string.Empty;

                // Find existing parameter
                var existingMatch = Regex.Match(url, $"[?&]({Regex.Escape(key)}=?.*?)(?:&|/|$)");

                // Parameter already set to something
                if (existingMatch.Success)
                {
                    var group = existingMatch.Groups[1];

                    // Remove existing
                    url = url.Remove(group.Index, group.Length);

                    // Insert new one
                    url = url.Insert(group.Index, $"{key}={value}");

                    return url;
                }
                // Parameter hasn't been set yet
                else
                {
                    // See if there are other parameters
                    var hasOtherParams = url.IndexOf('?') >= 0;

                    // Prepend either & or ? depending on that
                    var separator = hasOtherParams ? '&' : '?';

                    // Assemble new query string
                    return url + separator + key + '=' + value;
                }
            }

            public static string SetRouteParameter(string url, string key, string value)
            {
                if (value == null)
                    value = string.Empty;

                // Find existing parameter
                var existingMatch = Regex.Match(url, $"/({Regex.Escape(key)}/?.*?)(?:/|$)");

                // Parameter already set to something
                if (existingMatch.Success)
                {
                    var group = existingMatch.Groups[1];

                    // Remove existing
                    url = url.Remove(group.Index, group.Length);

                    // Insert new one
                    url = url.Insert(group.Index, $"{key}/{value}");

                    return url;
                }
                // Parameter hasn't been set yet
                else
                {
                    // Assemble new query string
                    return url + '/' + key + '/' + value;
                }
            }

            public static Dictionary<string, string> SplitQuery(string query)
            {
                var dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var paramsEncoded = query.TrimStart('?').Split(new char[] { '&' });
                foreach (var paramEncoded in paramsEncoded)
                {
                    var param = UrlDecode(paramEncoded);

                    // Look for the equals sign
                    var equalsPos = param.IndexOf('=');
                    if (equalsPos <= 0)
                        continue;

                    // Get the key and value
                    var key = param.Substring(0, equalsPos);
                    var value = equalsPos < param.Length
                        ? param.Substring(equalsPos + 1)
                        : string.Empty;

                    // Add to dictionary
                    dic[key] = value;
                }

                return dic;
            }
        }

        public IEnumerable<VideoInfo> GetDownloadUrls(string videoUrl, bool decryptSignature = true)
        {
            if (videoUrl == null)
                throw new ArgumentNullException("videoUrl");

            bool isYoutubeUrl = TryNormalizeYoutubeUrl(videoUrl, out videoUrl);

            if (!isYoutubeUrl)
                throw new ArgumentException("URL is not a valid youtube URL!");

            try
            {
                var requestedAt = DateTimeOffset.Now;

                JObject json = LoadJson(videoUrl);
                // Get player response JSON
                var playerResponseRaw = json.SelectToken("args.player_response").Value<string>();
                var playerResponseJson = JToken.Parse(playerResponseRaw);

                // Extract whether the video is a live stream
                var isLiveStream = playerResponseJson.SelectToken("videoDetails.isLive")?.Value<bool>() == true;

                // Extract valid until date
                var expiresIn = TimeSpan.FromSeconds(playerResponseJson.SelectToken("streamingData.expiresInSeconds").Value<double>());
                var validUntil = requestedAt + expiresIn;

                // Extract stream info
                var hlsManifestUrl = isLiveStream ? playerResponseJson.SelectToken("streamingData.hlsManifestUrl")?.Value<string>() : null;
                var dashManifestUrl = !isLiveStream ? playerResponseJson.SelectToken("streamingData.dashManifestUrl")?.Value<string>() : null;
                //var muxedStreamInfosUrlEncoded = !isLiveStream ? playerConfigJson.SelectToken("args.url_encoded_fmt_stream_map")?.Value<string>() : null;
                //var adaptiveStreamInfosUrlEncoded = !isLiveStream ? playerConfigJson.SelectToken("args.adaptive_fmts")?.Value<string>() : null;
                var muxedStreamInfosJson = !isLiveStream ? playerResponseJson.SelectToken("streamingData.formats") : null;
                var adaptiveStreamInfosJson = !isLiveStream ? playerResponseJson.SelectToken("streamingData.adaptiveFormats") : null;

                if (muxedStreamInfosJson != null)
                {
                    foreach (var streamInfoJson in muxedStreamInfosJson)
                    {
                        // Extract info
                        var itag = streamInfoJson.SelectToken("itag").Value<int>();
                        var url = streamInfoJson.SelectToken("url")?.Value<string>();

                        // Decipher signature if needed
                        if (string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(playerConfiguration.PlayerSourceUrl))
                        {
                            var cipher = streamInfoJson.SelectToken("cipher").Value<string>();
                            var cipherDic = Url.SplitQuery(cipher);

                            url = cipherDic["url"];
                            var signature = cipherDic["s"];

                            // Get cipher operations (cached)
                            var cipherOperations = await GetCipherOperationsAsync(playerConfiguration.PlayerSourceUrl).ConfigureAwait(false);

                            // Decipher signature
                            signature = cipherOperations.Decipher(signature);

                            // Set the corresponding parameter in the URL
                            var signatureParameter = cipherDic.GetValueOrDefault("sp") ?? "signature";
                            url = Url.SetQueryParameter(url, signatureParameter, signature);
                        }

                        // Try to extract content length, otherwise get it manually
                        var contentLength = streamInfoJson.SelectToken("contentLength")?.Value<long>() ?? -1;

                        if (contentLength <= 0)
                        {
                            contentLength = Regex.Match(url, @"clen=(\d+)").Groups[1].Value.ParseLongOrDefault();
                        }

                        if (contentLength <= 0)
                        {
                            // Send HEAD request and get content length
                            contentLength = await _httpClient.GetContentLengthAsync(url, false).ConfigureAwait(false) ?? 0;

                            // If content length is still not available - stream is gone or faulty
                            if (contentLength <= 0)
                                continue;
                        }

                        // Extract container
                        var containerRaw = streamInfoJson.SelectToken("mimeType").Value<string>().SubstringUntil(";").SubstringAfter("/");
                        var container = Heuristics.ContainerFromString(containerRaw);

                        // Extract audio encoding
                        var audioEncodingRaw = streamInfoJson.SelectToken("mimeType").Value<string>().SubstringAfter("codecs=\"").SubstringUntil("\"").Split(", ").Last();
                        var audioEncoding = Heuristics.AudioEncodingFromString(audioEncodingRaw);

                        // Extract video encoding
                        var videoEncodingRaw = streamInfoJson.SelectToken("mimeType").Value<string>().SubstringAfter("codecs=\"").SubstringUntil("\"").Split(", ").First();
                        var videoEncoding = Heuristics.VideoEncodingFromString(videoEncodingRaw);

                        // Determine video quality from itag
                        var videoQuality = Heuristics.VideoQualityFromItag(itag);

                        // Determine video quality label from video quality
                        var videoQualityLabel = Heuristics.VideoQualityToLabel(videoQuality);

                        // Determine video resolution from video quality
                        var resolution = Heuristics.VideoQualityToResolution(videoQuality);

                        // Add to list
                        muxedStreamInfoMap[itag] = new MuxedStreamInfo(itag, url, container, contentLength, audioEncoding, videoEncoding,
                            videoQualityLabel, videoQuality, resolution);
                    }
                }
                int i = 0;
                /*
                string videoTitle = GetVideoTitle(json);

                IEnumerable<ExtractionInfo> downloadUrls = ExtractDownloadUrls(json);

                IEnumerable<VideoInfo> infos = GetVideoInfos(downloadUrls, videoTitle).ToList();

                string htmlPlayerVersion = GetHtml5PlayerVersion(json);

                foreach (VideoInfo info in infos)
                {
                    info.HtmlPlayerVersion = htmlPlayerVersion;

                    if (decryptSignature && info.RequiresDecryption)
                    {
                        DecryptDownloadUrl(info);
                    }
                }

                return infos;*/
            }

            catch (Exception ex)
            {
                
            }

            return null; // Will never happen, but the compiler requires it
        }
    }
}
