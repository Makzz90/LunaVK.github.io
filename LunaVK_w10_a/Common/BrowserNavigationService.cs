using LunaVK.Core;
using LunaVK.Library;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.Email;

/*
public static final a a = new a(null);
private static final Regex b = new Regex("(^|[a-z0-9.\\-]*\\.)(vk|vkontakte)\\.(com|ru|me)");
private static final Regex c = new Regex("(^|[a-z0-9.\\-]*\\.)m\\.([a-z0-9.\\-]+\\.)?(vk|vkontakte)\\.(com|ru|me)");
private static final Regex d = new Regex("^vk\\.(cc|link)");
private static final Regex e = new Regex("/(?:id)([-0-9]+)");
private static final Regex f = new Regex("/(?:club|event|public)([0-9]+)");
private static final Regex g = new Regex("/wall(([-0-9]+)_([0-9]+))");
private static final Regex h = new Regex("/(app[-0-9]+)(?:_([-0-9]+))?");
private static final Regex i = new Regex("vk.me/(join/)?[A-Za-z0-9._/]+");
private static final Regex j = new Regex("/moneysend/([A-Za-z0-9._/]+)$");
private static final Regex k = new Regex("wall(([-0-9]+)_([0-9]+))");


static final Pattern a = Pattern.compile("\\[((id|club)[0-9]+)(?::bp[-_0-9]+)?\\|([^\\]]+)\\]");
static final Pattern b = Pattern.compile("((?:(?:http|https)://)?[a-zA-Zа-яА-Я0-9-]+\\.[a-zA-Zа-яА-Я]{2,4}[a-zA-Z/?\\.=#%&-_]+)");
static final Pattern c = Pattern.compile("\\[id(\\d+):bp\\-(\\d+)_(\\d+)\\|([^\\]]+)\\]");
static final Pattern d = Pattern.compile("\\[club(\\d+):bp\\-(\\d+)_(\\d+)\\|([^\\]]+)\\]");
static final Pattern e = Pattern.compile("\\[id(\\d+)\\|([^\\]]+)\\]");
static final Pattern f = Pattern.compile("\\[club(\\d+)\\|([^\\]]+)\\]");


public static final Pattern a = k.d;
public static final Pattern b;
public static final Pattern c = Pattern.compile("\\[((?:event#)[a-z_]+)\\|([^\\]]+)\\]");
public static final Pattern d = Pattern.compile("\\[((?:id|club)[0-9]+)\\|([^\\]]+)\\]");
public static final Pattern e = Pattern.compile("'''\\[((?:id|club)[0-9]+)\\|([^\\]]+)\\]'''");
public static final Pattern f = Pattern.compile("\\[((?:id|club)[0-9]+):bp[0-9_-]+\\|([^\\]]+)\\]");
public static final Pattern g = Pattern.compile("(#[\\d\\w]{2,})(?:@([-_a-z\\d\\.]{2,}))?", 66);
public static final Pattern h = Pattern.compile("'''(.*?)'''");
public static final Pattern i = Pattern.compile("\\[(\\S+?)\\|(.+?)\\]");
public static final Pattern j = Pattern.compile("'''\\[(\\S+?)\\|(.+?)\\]'''");
public static final Pattern k = Pattern.compile("\\b(?:([0-5]?\\d):)?([0-5]?\\d):([0-5]?\\d)\\b");
*/
namespace LunaVK.Common
{
    public static class BrowserNavigationService
    {
        /// <summary>
        /// vk.com/feed?section=search&q=
        /// </summary>
        public static string _searchFeedPrefix = "vk.com/feed?section=search&q=";

        public static readonly List<string> _flagsPrefixes = new List<string>() { "D83CDDE6", "D83CDDE7", "D83CDDE8", "D83CDDE9", "D83CDDEA", "D83CDDEB", "D83CDDEC", "D83CDDED", "D83CDDEE", "D83CDDEF", "D83CDDF0", "D83CDDF1", "D83CDDF2", "D83CDDF3", "D83CDDF4", "D83CDDF5", "D83CDDF6", "D83CDDF7", "D83CDDF8", "D83CDDF9", "D83CDDFA", "D83CDDFB", "D83CDDFC", "D83CDDFD", "D83CDDFE", "D83CDDFF" };
        public static readonly List<string> _modificatableSmiles = new List<string>() { "261D", "270A", "270B", "270C", "270D", "D83CDF85", "D83CDFC3", "D83CDFC4", "D83CDFC7", "D83CDFCA", "D83DDC4A", "D83DDC4B", "D83DDC4C", "D83DDC4D", "D83DDC4E", "D83DDC4F", "D83DDC6E", "D83DDC7C", "D83DDC42", "D83DDC43", "D83DDC46", "D83DDC47", "D83DDC48", "D83DDC49", "D83DDC50", "D83DDC66", "D83DDC67", "D83DDC68", "D83DDC69", "D83DDC70", "D83DDC71", "D83DDC72", "D83DDC73", "D83DDC74", "D83DDC75", "D83DDC76", "D83DDC77", "D83DDC78", "D83DDC81", "D83DDC82", "D83DDC83", "D83DDC85", "D83DDC86", "D83DDC87", "D83DDCAA", "D83DDD90", "D83DDD95", "D83DDD96", "D83DDE4B", "D83DDE4C", "D83DDE4D", "D83DDE4E", "D83DDE4F", "D83DDE45", "D83DDE46", "D83DDE47", "D83DDEA3", "D83DDEB4", "D83DDEB5", "D83DDEB6", "D83DDEC0", "D83EDD18" };
        public static List<string> _smilesModificators = new List<string>() { "D83CDFFB", "D83CDFFC", "D83CDFFD", "D83CDFFF", "D83CDFFE" };

        /// <summary>
        /// Выражение для поиска ссылок (взято из оригинального ВК)
        /// также ищем почту, позже её исключим :\
        /// </summary>
        private static Regex _regex_Uri = new Regex(@"(((http|https)://)?[a-zA-Zа-яА-Я0-9@\.-]+\.[a-zA-Zа-яА-Я]{2,4}\b[a-zA-Z/?\.\d\-=#%&_~@]*)", RegexOptions.IgnoreCase);
        //private static Regex _regex_Uri = new Regex("(?<![A-Za-z\\$0-9А-Яа-яёЁєЄҐґЇїІіЈј\\-_@])(https?:\\/\\/)?((?:[A-Za-z\\$0-9А-Яа-яёЁєЄҐґЇїІіЈј](?:[A-Za-z\\$0-9\\-_А-Яа-яёЁєЄҐґЇїІіЈј]*[A-Za-z\\$0-9А-Яа-яёЁєЄҐґЇїІіЈј])?\\.){1,5}[A-Za-z\\$рфуконлайстбРФУКОНЛАЙСТБ\\-\\d]{2,22}(?::\\d{2,5})?)((?:\\/(?:(?:\\&|\\!|,[_%]|[A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.~=;:]+|\\[[A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.,~=;:]*\\]|\\([A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.,~=;:]*\\))*(?:,[_%]|[A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.~=;:]*[A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј_@#%?+\\/\\$~=]|\\[[A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.,~=;:]*\\]|\\([A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.,~=;:]*\\)))?)?)", RegexOptions.IgnoreCase);
        private static Regex _regex_Email = new Regex("(?<![a-zA-Z\\-_\\.0-9^#])([a-zA-Z\\-_\\.0-9]+@[a-zA-Z\\-_0-9]+\\.[a-zA-Z\\-_\\.0-9]+[a-zA-Z\\-_0-9])", RegexOptions.IgnoreCase);

        /// <summary>
        /// Упоминания вида "@id12345 (Имя Фамилия)"
        /// </summary>
        private static Regex Regex_DomainMention = new Regex("(\\*|@)((id|club|event|public)\\d+)\\s*\\((.+?)\\)", RegexOptions.IgnoreCase);

        /// <summary>
        /// Выражение для упоминаний вида "[id12345|Имя Фамилия]"
        /// </summary>
        private static Regex Regex_Mention = new Regex(@"\[(.+?)\|(.+?)\]", RegexOptions.IgnoreCase);// \[(id|club)(\d+)(?:\:([a-z0-9_\-]+))?\|([^\$]+?)\]

        private static Regex _regex_MatchedTag = new Regex(@"(#[\d\w\-]{2,})(@([_a-z\d\.\-]{2,}))?", RegexOptions.IgnoreCase);
        
        private static Regex _regex_Domain = new Regex("((?:[a-z0-9_]*[a-z0-9])?(?:(?:\\.[a-z](?:[a-z0-9_]+[a-z0-9])?)*\\.[a-z][a-z0-9_]{2,40}[a-z0-9])?)", RegexOptions.IgnoreCase);

        //private static Regex _regex_UriVK = new Regex("(?:(?:http|https)://)?(vk|vkontakte).(com|ru|me)[a-zA-Z/?\\.=#%&-_]*", RegexOptions.IgnoreCase);


        /// <summary>
        /// Возвращает InlineUIContainer с картинкой
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static InlineUIContainer GetImage(string name)
        {
            //Settings.EmojiType   
            Image image1 = new Image() { Height = 20.0, Width = 20.0 };
            image1.Source = new BitmapImage(new Uri(string.Format("ms-appx:///Assets/Emoji/{0:x}.png", name), UriKind.RelativeOrAbsolute));
            image1.Margin = new Thickness(0.0, 0.0, 0.0, -5.0);
            image1.CacheMode = new BitmapCache();//todo: этого не было, а работает ли оно?
            //if (name == "00A9" || name == "00AE")
            //    image1.Margin = new Thickness(0.0, 4.0, -4.0, -9.0);
            //else
            //    image1.Margin = new Thickness(0.0, 0.0, 4.0, -5.0);
            InlineUIContainer inlineUiContainer = new InlineUIContainer();
            inlineUiContainer.Child = image1;
            return inlineUiContainer;
        }

        public static string ConvertToHexString(byte[] bytes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < bytes.Length; ++index)
                stringBuilder = stringBuilder.Append(Convert.ToString(bytes[index], 16).PadLeft(2, '0'));
            return stringBuilder.ToString().ToUpperInvariant().Replace("FE0F", "").Replace("200D", "");
        }

        public static void CheckRelationsSmiles(ref string bytesStr, ref TextElementEnumerator textEnumerator, ref string text)
        {
            bool flag1 = true;
            int elementIndex1 = textEnumerator.ElementIndex;
            bool flag2 = bytesStr == "D83DDC68";
            if ((flag2 || bytesStr == "D83DDC69") && textEnumerator.MoveNext())
            {
                string textElement1 = textEnumerator.GetTextElement();
                string hexString1 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement1));
                if (hexString1 == "" && textEnumerator.MoveNext())
                {
                    textElement1 = textEnumerator.GetTextElement();
                    hexString1 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement1));
                }
                if (hexString1 == "2764" && textEnumerator.MoveNext())
                {
                    string textElement2 = textEnumerator.GetTextElement();
                    string hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
                    if (hexString2 == "" && textEnumerator.MoveNext())
                    {
                        textElement2 = textEnumerator.GetTextElement();
                        hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
                    }
                    string str1 = "";
                    string str2 = "";
                    if (hexString2 == "D83DDC8B" && textEnumerator.MoveNext())
                    {
                        str1 = textElement2;
                        str2 = hexString2;
                        textElement2 = textEnumerator.GetTextElement();
                        hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
                        if (hexString2 == "" && textEnumerator.MoveNext())
                        {
                            textElement2 = textEnumerator.GetTextElement();
                            hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
                        }
                    }
                    if (flag2 && hexString2 == "D83DDC68" || !flag2 && hexString2 == "D83DDC69")
                    {
                        flag1 = false;
                        bytesStr = bytesStr + hexString1 + str2 + hexString2;
                        text = text + textElement1 + str1 + textElement2;
                    }
                }
                else if ((hexString1 == "D83DDC68" || hexString1 == "D83DDC69") && (!(bytesStr == "D83DDC69") || !(hexString1 == "D83DDC68")) && textEnumerator.MoveNext())
                {
                    string textElement2 = textEnumerator.GetTextElement();
                    string hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
                    if (hexString2 == "" && textEnumerator.MoveNext())
                    {
                        textElement2 = textEnumerator.GetTextElement();
                        hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
                    }
                    if (hexString2 == "D83DDC67" || hexString2 == "D83DDC66")
                    {
                        bool flag3 = false;
                        int elementIndex2 = textEnumerator.ElementIndex;
                        flag1 = false;
                        if (textEnumerator.MoveNext())
                        {
                            string textElement3 = textEnumerator.GetTextElement();
                            string hexString3 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement3));
                            if (hexString3 == "" && textEnumerator.MoveNext())
                            {
                                textElement3 = textEnumerator.GetTextElement();
                                hexString3 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement3));
                            }
                            if ((hexString3 == "D83DDC67" || hexString3 == "D83DDC66") && (!(hexString2 == "D83DDC66") || !(hexString3 == "D83DDC67")))
                            {
                                flag3 = true;
                                bytesStr = bytesStr + hexString1 + hexString2 + hexString3;
                                text = text + textElement1 + textElement2 + textElement3;
                            }
                            else
                            {
                                textEnumerator.Reset();
                                textEnumerator.MoveNext();
                                while (textEnumerator.ElementIndex != elementIndex2)
                                    textEnumerator.MoveNext();
                            }
                        }
                        if (!flag3)
                        {
                            bytesStr = bytesStr + hexString1 + hexString2;
                            text = text + textElement1 + textElement2;
                        }
                    }
                }
            }
            if (!(textEnumerator.ElementIndex != elementIndex1 & flag1))
                return;
            textEnumerator.Reset();
            textEnumerator.MoveNext();
            while (textEnumerator.ElementIndex != elementIndex1)
                textEnumerator.MoveNext();
        }

        public static List<string> ParseText(string text)
        {
            text = text.Replace("\n", " \n ");

            
            text = BrowserNavigationService._regex_Uri.Replace(text, (m =>
            {
                /*
                string str1 = m.Groups.Count > 2 ? m.Groups[2].Value : string.Empty;
                if(m.Groups.Count <= 1 || !m.Groups[1].Value.ToLower().StartsWith("http://") && !m.Groups[1].Value.StartsWith("https://"))
                    return System.Net.WebUtility.UrlDecode(m.Value);
                string str2 = m.Groups.Count > 1 ? m.Groups[1].Value : "http://";
                if (str2 == string.Empty)
                    str2 = "http://";
                string str3 = m.Groups.Count > 2 ? m.Groups[2].Value : string.Empty;
                if (m.Groups.Count > 3)
                    str3 += m.Groups[3].Value;
                string str4 = System.Net.WebUtility.UrlDecode(m.Value);
                if (str4.Length > 38)
                    str4 = str4.Substring(0, 35) + "...";
                return string.Format("\a{0}\b{1}\a", (str2 + str3), str4).Replace("#", "\0");
                */
                if (m.Value.Contains("@"))//Почта
                    return m.Value;
                string str4 = m.Value;//Укороченная ссылка
                if (str4.Length > 38)
                    str4 = str4.Substring(0, 35) + "...";
                return string.Format("\a{0}\b{1}\a", m.Value, str4);
            }));
            
            
            //text = BrowserNavigationService._regex_Uri.Replace(text, "\a$0\b$0\a");
            text = BrowserNavigationService._regex_Email.Replace(text, "\amailto:$0\b$0\a");

            text = BrowserNavigationService.Regex_DomainMention.Replace(text, "[$2|$4]");

            text = BrowserNavigationService.Regex_Mention.Replace(text,(m=>
            {
                string value = m.Groups[1].Value;
                string title = m.Groups[2].Value;
                string[] splitted = value.Split(new char[] { '\b' });
                if(splitted.Length>1)
                {
                    value = splitted[0].Substring(1);
                }

                if (value.StartsWith("id") || value.StartsWith("club"))
                    return string.Format("\ahttps://vk.com/{0}\b{1}\a", value, title);
                return string.Format("\a{0}\b{1}\a", value, title);
            }) );//"\ahttps://vk.com/$1$2$3\b$4\b\a"

            text = BrowserNavigationService._regex_MatchedTag.Replace(text, "\ahttps://"+ BrowserNavigationService._searchFeedPrefix + "$0\b$0\a");//"$1\ahttps://{0}$3$4\b$3$4\a"

            text = text.Replace("\n ", "\n").Replace(" \n", "\n").Replace("\0", "#");
            
            //var text2 = text.ReplaceByRegex(BrowserNavigationService._regex_Email, "\amailto:$2\b$2\a").ReplaceByRegex(BrowserNavigationService.Regex_DomainMention, "[$2|$4]").ReplaceByRegex(BrowserNavigationService.Regex_Mention, "\ahttps://vk.com/$1$2$3\b$4\b\a").ReplaceByRegex(BrowserNavigationService.Regex_Tag, string.Format("$1\ahttps://{0}$3$4\b$3$4\a", BrowserNavigationService._searchFeedPrefix)).Replace("\n ", "\n").Replace(" \n", "\n").Replace("\0", "#");
            if (text.StartsWith(" "))
                text = text.Remove(0, 1);
            return Enumerable.ToList<string>(text.Split('\a'));
        }

        public static string ReplaceByRegex(this string str, Regex regex, string replace)
        {
            return regex.Replace(str, replace);
        }

        public static string PreprocessTextForGroupBoardMentions(string s)
        {
            s = Regex.Replace(s, "\\[(id|club)(\\d+):bp\\-(\\d+)_(\\d+)\\|([^\\]]+)\\]", "[$1$2|$5]");
            return s;
        }

        /// <summary>
        /// Добавляем текст, а если надо, то и эмодзи
        /// </summary>
        /// <param name="text_block"></param>
        /// <param name="par"></param>
        /// <param name="raw_text"></param>
        public static void AddRawText(RichTextBlock text_block, Paragraph par, string raw_text)
        {
            TextElementEnumerator elementEnumerator = StringInfo.GetTextElementEnumerator(raw_text);
            StringBuilder stringBuilder = new StringBuilder();
            bool flag1 = elementEnumerator.MoveNext();
            while (flag1)
            {
                string textElement1 = elementEnumerator.GetTextElement();
                string hexString1 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement1));
                if (hexString1 == "")
                {
                    flag1 = elementEnumerator.MoveNext();
                }
                else
                {
                    bool flag2 = true;
                    bool flag3 = BrowserNavigationService._flagsPrefixes.Contains(hexString1);
                    int elementIndex = elementEnumerator.ElementIndex;
                    if ((flag3 || BrowserNavigationService._modificatableSmiles.Contains(hexString1)) && elementEnumerator.MoveNext())
                    {
                        string textElement2 = elementEnumerator.GetTextElement();
                        string hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
                        if (hexString2 == "" && elementEnumerator.MoveNext())
                        {
                            textElement2 = elementEnumerator.GetTextElement();
                            hexString2 = BrowserNavigationService.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement2));
                        }
                        if (hexString2 != "" && (flag3 || BrowserNavigationService._smilesModificators.Contains(hexString2)))
                        {
                            flag2 = false;
                            hexString1 += hexString2;
                            textElement1 += textElement2;
                        }
                        else
                        {
                            elementEnumerator.Reset();
                            elementEnumerator.MoveNext();
                            while (elementEnumerator.ElementIndex != elementIndex)
                                elementEnumerator.MoveNext();
                        }
                    }
                    if (flag2)
                        BrowserNavigationService.CheckRelationsSmiles(ref hexString1, ref elementEnumerator, ref textElement1);
                    //string name;
                    if(Emoji.Dict.Contains(hexString1))//пробуем найти эмодзи
                    //if (name != null)
                    {
                        string text = stringBuilder.ToString();
                        stringBuilder = stringBuilder.Clear();
                        if (text != string.Empty)
                            par.Inlines.Add(BrowserNavigationService.GetRunWithStyle(text, text_block));
                        par.Inlines.Add(BrowserNavigationService.GetImage(hexString1));
                    }
                    else
                        stringBuilder = stringBuilder.Append(textElement1);
                    flag1 = elementEnumerator.MoveNext();
                }
            }
            string text1 = stringBuilder.ToString();
            if (text1 == string.Empty)
                return;
            par.Inlines.Add(BrowserNavigationService.GetRunWithStyle(text1, text_block));
        }

        private static Run GetRunWithStyle(string text, RichTextBlock richTextBox)
        {
            Run run = new Run();
            /*
            FontFamily fontFamily = ((Control)richTextBox).FontFamily;
            ((TextElement)run).FontFamily = fontFamily;
            double fontSize = ((Control)richTextBox).FontSize;
            ((TextElement)run).FontSize = fontSize;
            Brush foreground = ((Control)richTextBox).Foreground;
            ((TextElement)run).Foreground = foreground;*/
            run.FontSize = (double)Application.Current.Resources["FontSizeContent"];
            run.Text = text;
            return run;
        }
        
        public static Hyperlink GenerateHyperlink(string text, string tag, Action<Hyperlink, string> clickedCallback, Brush foregroundBrush = null)
        {
            Hyperlink h = new Hyperlink();
            //HyperlinkHelper.SetState(h, hyperlinkState, foregroundBrush);
            Run expr_3D = new Run();
            expr_3D.Text = (text);
            h.Inlines.Add(expr_3D);
            //
            h.Foreground = new SolidColorBrush((Windows.UI.Color)Application.Current.Resources["PhoneAccentColor"]);
            //
            h.Click += (s, a)=> { clickedCallback.Invoke(h, tag); };
            return h;
        }

        public static async void NavigateOnHyperlink(string navstr)
        {
            if (string.IsNullOrEmpty(navstr))
                return;

            if (!navstr.Contains("@") || navstr.Contains("#"))
            {
                NavigatorImpl.Instance.NavigateToWebUri(navstr);
            }
            else
            {
                if (navstr.StartsWith("http://") || navstr.StartsWith("mailto:"))
                    navstr = navstr.Substring(0, 7);

                var emailMessage = new EmailMessage();
                emailMessage.To.Add(new EmailRecipient(navstr));
                await EmailManager.ShowComposeNewEmailAsync(emailMessage);
            }
        }

    }
}
