using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace LunaVK.Core.Utils
{
    public class UIStringFormatterHelper
    {
        private readonly static Regex _linksRegex = new Regex(@"(\[.*?\|.*?\])|(https?://\S+)|(#\S+)");
        //private static Regex _regex_Uri = new Regex("(?<![A-Za-z\\$0-9А-Яа-яёЁєЄҐґЇїІіЈј\\-_@])(https?:\\/\\/)?((?:[A-Za-z\\$0-9А-Яа-яёЁєЄҐґЇїІіЈј](?:[A-Za-z\\$0-9\\-_А-Яа-яёЁєЄҐґЇїІіЈј]*[A-Za-z\\$0-9А-Яа-яёЁєЄҐґЇїІіЈј])?\\.){1,5}[A-Za-z\\$рфуконлайстбРФУКОНЛАЙСТБ\\-\\d]{2,22}(?::\\d{2,5})?)((?:\\/(?:(?:\\&|\\!|,[_%]|[A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.~=;:]+|\\[[A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.,~=;:]*\\]|\\([A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.,~=;:]*\\))*(?:,[_%]|[A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.~=;:]*[A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј_@#%?+\\/\\$~=]|\\[[A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.,~=;:]*\\]|\\([A-Za-z0-9¨¸À-ÿєЄҐґЇїІіЈј\\-_@#%?+\\/\\$.,~=;:]*\\)))?)?)", RegexOptions.IgnoreCase);
        //private static Regex _regex_Email = new Regex("([^#]|^)(?<![a-zA-Z\\-_\\.0-9])([a-zA-Z\\-_\\.0-9]+@[a-zA-Z\\-_0-9]+\\.[a-zA-Z\\-_\\.0-9]+[a-zA-Z\\-_0-9])", RegexOptions.IgnoreCase);
        //private static Regex _regex_Domain = new Regex("((?:[a-z0-9_]*[a-z0-9])?(?:(?:\\.[a-z](?:[a-z0-9_]+[a-z0-9])?)*\\.[a-z][a-z0-9_]{2,40}[a-z0-9])?)", RegexOptions.IgnoreCase);
        //private static Regex _regex_MatchedTag = new Regex("(?:)?(#[a-zA-Zа-яА-ЯёЁіїєґІЄЇҐЎў’_\\d]*(?:)?[a-zA-Zа-яА-ЯёЁіїєґІЄЇҐЎў’][a-zA-Zа-яА-ЯёЁіїєґІЄЇҐЎў’_\\d]*(?:)?[a-zA-Zа-яА-ЯёЁіїєґІЄЇҐЎў’_\\d]+|#[a-zA-Zа-яА-ЯёЁіїєґІЄЇҐЎў’_\\d]*(?:)?[a-zA-Zа-яА-ЯёЁіїєґІЄЇҐЎў’_\\d]+[a-zA-Zа-яА-ЯёЁіїєґІЄЇҐЎў’](?:)?[a-zA-Zа-яА-ЯёЁіїєґІЄЇҐЎў’_\\d]*)(?:)?", RegexOptions.IgnoreCase);
        //public static Regex Regex_Mention = new Regex("\\[(id|club)(\\d+)(?:\\:([a-z0-9_\\-]+))?\\|([^\\$]+?)\\]", RegexOptions.IgnoreCase);
        //public static Regex Regex_DomainMention = new Regex("(\\*|@)((id|club|event|public)\\d+)\\s*\\((.+?)\\)", RegexOptions.IgnoreCase);
        //public static Regex Regex_Tag = new Regex("(^|[\\s.,:'\";>\\)\\(]?)(" + UIStringFormatterHelper._regex_MatchedTag + ")(@" + UIStringFormatterHelper._regex_Domain + ")?(?=$|[\\s\\.,:'\"&;?<\\)\\(]?)", RegexOptions.IgnoreCase);
        //private static List<string> _domainsList = new List<string>() { "AC", "AD", "AE", "AF", "AG", "AI", "AL", "AM", "AN", "AO", "AQ", "AR", "AS", "AT", "AU", "AW", "AX", "AZ", "BA", "BB", "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BM", "BN", "BO", "BR", "BS", "BT", "BV", "BW", "BY", "BZ", "CA", "CC", "CD", "CF", "CG", "CH", "CI", "CK", "CL", "CM", "CN", "CO", "CR", "CU", "CV", "CX", "CY", "CZ", "DE", "DJ", "DK", "DM", "DO", "DZ", "EC", "EE", "EG", "EH", "ER", "ES", "ET", "EU", "FI", "FJ", "FK", "FM", "FO", "FR", "GA", "GD", "GE", "GF", "GG", "GH", "GI", "GL", "GM", "GN", "GP", "GQ", "GR", "GS", "GT", "GU", "GW", "GY", "HK", "HM", "HN", "HR", "HT", "HU", "ID", "IE", "IL", "IM", "IN", "IO", "IQ", "IR", "IS", "IT", "JE", "JM", "JO", "JP", "KE", "KG", "KH", "KI", "KM", "KN", "KP", "KR", "KW", "KY", "KZ", "LA", "LB", "LC", "LI", "LK", "LR", "LS", "LT", "LU", "LV", "LY", "MA", "MC", "MD", "ME", "MG", "MH", "MK", "ML", "MM", "MN", "MO", "MP", "MQ", "MR", "MS", "MT", "MU", "MV", "MW", "MX", "MY", "MZ", "NA", "NC", "NE", "NF", "NG", "NI", "NL", "NO", "NP", "NR", "NU", "NZ", "OM", "PA", "PE", "PF", "PG", "PH", "PK", "PL", "PM", "PN", "PR", "PS", "PT", "PW", "PY", "QA", "RE", "RO", "RU", "RS", "RW", "SA", "SB", "SC", "SD", "SE", "SG", "SH", "SI", "SJ", "SK", "SL", "SM", "SN", "SO", "SR", "SS", "ST", "SU", "SV", "SX", "SY", "SZ", "TC", "TD", "TF", "TG", "TH", "TJ", "TK", "TL", "TM", "TN", "TO", "TP", "TR", "TT", "TV", "TW", "TZ", "UA", "UG", "UK", "UM", "US", "UY", "UZ", "VA", "VC", "VE", "VG", "VI", "VN", "VU", "WF", "WS", "YE", "YT", "YU", "ZA", "ZM", "ZW", "ARPA", "AERO", "ASIA", "BIZ", "CAT", "COM", "COOP", "INFO", "INT", "JOBS", "MEDIA", "MOBI", "MUSEUM", "NAME", "NET", "ORG", "POST", "PRO", "TEL", "TRAVEL", "XXX", "CLUB", "ACADEMY", "EDU", "GOV", "MIL", "LOCAL", "INTERNATIONAL", "XN--LGBBAT1AD8J", "XN--54B7FTA0CC", "XN--FIQS8S", "XN--FIQZ9S", "XN--WGBH1C", "XN--NODE", "XN--J6W193G", "XN--H2BRJ9C", "XN--MGBBH1A71E", "XN--FPCRJ9C3D", "XN--GECRJ9C", "XN--S9BRJ9C", "XN--XKC2DL3A5EE0H", "XN--45BRJ9C", "XN--MGBA3A4F16A", "XN--MGBAYH7GPA", "XN--80AO21A", "XN--MGBX4CD0AB", "XN--L1ACC", "XN--MGBC0A9AZCG", "XN--MGB9AWBF", "XN--MGBAI9AZGQP6J", "XN--YGBI2AMMX", "XN--WGBL6A", "XN--P1AI", "XN--MGBERP4A5D4AR", "XN--90A3AC", "XN--YFRO4I67O", "XN--CLCHC0EA0B2G2A9GCD", "XN--3E0B707E", "XN--FZC2C9E2C", "XN--XKC2AL3HYE2A", "XN--MGBTF8FL", "XN--KPRW13D", "XN--KPRY57D", "XN--O3CW4H", "XN--PGBS0DH", "XN--J1AMH", "XN--MGBAAM7A8H", "XN--MGB2DDES", "XN--80ASEHDB", "XN--80ASWG", "XN--OGBPF8FL", "рф", "РФ", "укр", "УКР", "онлайн", "ОНЛАЙН", "сайт", "САЙТ", "срб", "СРБ" };
        private static readonly Regex _userOrGroupRegEx = new Regex("\\[(id|club)\\d+.*?\\|.*?\\]");

        public static string FormatDateTimeForUI(int unixTime)
        {
            return UIStringFormatterHelper.FormatDateTimeForUI(UIStringFormatterHelper.UnixTimeStampToDateTime((double)unixTime, true));
        }

        public static string FormatDateTimeForUIShort(DateTime dateTime, bool includeTimeDiff = true)
        {
            if (includeTimeDiff)
            {
                int minusLocalTimeDelta = Settings.ServerMinusLocalTimeDelta;
                dateTime = dateTime.AddSeconds(-minusLocalTimeDelta);
            }
            string str1 = dateTime.ToString("HH:mm");
            return str1;
        }

        /// <summary>
        /// Возвращает текст вида "Вчера во столько-то"
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string FormatDateTimeForUI(DateTime dateTime)
        {
            DateTime now = DateTime.Now;
            int int32 = Convert.ToInt32(Math.Floor((now - dateTime).TotalMinutes));
            string str1 = dateTime.ToString("HH:mm");//22:05
            if (int32 >= -1 && int32 < 60)
            {
                if (int32 < 1)
                    return LocalizedStrings.GetString("MomentAgo");
                return UIStringFormatterHelper.FormatNumberOfSomething(int32, "OneMinuteFrm", "TwoFourMinutes", "FiveMinutes");
            }
            if (now.Date == dateTime.Date)
            {
                return string.Format("{0} {1} {2}", LocalizedStrings.GetString("Today"), LocalizedStrings.GetString("At"), str1);//сегодня в
            }
            if (now.AddDays(-1.0).Date == dateTime.Date)
            {
                return string.Format("{0} {1} {2}", LocalizedStrings.GetString("Yesterday"), LocalizedStrings.GetString("At"), str1);//вчера в
            }
            if (now.Year == dateTime.Year)
            {
                string ofMonthStr = UIStringFormatterHelper.GetOfMonthStr(dateTime.Month);
                return string.Format("{0} {1} {2} {3}", dateTime.Day, ofMonthStr, LocalizedStrings.GetString("At"), str1);//22 мая в 22:05
            }
            int day1 = dateTime.Day;
            string ofMonthStr1 = UIStringFormatterHelper.GetOfMonthStr(dateTime.Month);
            int year = dateTime.Year;
            
            string str2 = LocalizedStrings.GetString("At");
            return string.Format("{0} {1} {2} {3} {4}", day1, ofMonthStr1, year, str2, str1);
            
        }

        public static string FormatNumberOfSomething(int number, string oneSomethingFrm, string twoSomethingFrm, string fiveSomethingFrm, bool includeNumberInResult = true, string additionalFormatParam = null, bool includeZero = false)
        {
            if (number < 0)
                throw new Exception("Invalid number to format.");
            if (number == 0 && !includeZero)
                return "";
            int num = number % 10;
            /*
             * 1 участник
             * 2 участник а
             * 3 участник а
             * 4 участник а
             * 5 участник ОВ
             * 6 участник ОВ
             * 7 участник ОВ
             * 8 участник ОВ
             * 9 участник ОВ
             * 10 участник ОВ
             * */
            string format = !(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "en") || !(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "pt") ? (number == 1 ? oneSomethingFrm : twoSomethingFrm) : (num != 1 || number % 100 == 11 ? (num < 2 || num > 4 || number % 100 >= 12 && number % 100 <= 14 ? fiveSomethingFrm : twoSomethingFrm) : oneSomethingFrm);
            //
            format = LocalizedStrings.GetString(format);
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(format));
#endif
            //
            if (!includeNumberInResult)
                return string.Format(format, "").Trim();
            NumberFormatInfo numberFormatInfo = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            numberFormatInfo.NumberGroupSeparator = " ";
            string str = number.ToString("#,#", numberFormatInfo);
            if (additionalFormatParam == null)
                return string.Format(format, str);
            return string.Format(format, str, additionalFormatParam);
        }

        public static string GetOfMonthStr(int month)
        {
            switch (month)
            {
                case 1:
                    return LocalizedStrings.GetString("OfJanuary");
                case 2:
                    return LocalizedStrings.GetString("OfFebruary");
                case 3:
                    return LocalizedStrings.GetString("OfMarch");
                case 4:
                    return LocalizedStrings.GetString("OfApril");
                case 5:
                    return LocalizedStrings.GetString("OfMay");
                case 6:
                    return LocalizedStrings.GetString("OfJune");
                case 7:
                    return LocalizedStrings.GetString("OfJuly");
                case 8:
                    return LocalizedStrings.GetString("OfAugust");
                case 9:
                    return LocalizedStrings.GetString("OfSeptember");
                case 10:
                    return LocalizedStrings.GetString("OfOctober");
                case 11:
                    return LocalizedStrings.GetString("OfNovember");
                case 12:
                    return LocalizedStrings.GetString("OfDecember");
                default:
                    return "";
            }
        }
        
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp, bool includeTimeDiff = true)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            if (includeTimeDiff)
            {
                int minusLocalTimeDelta = Settings.ServerMinusLocalTimeDelta;
                unixTimeStamp -= minusLocalTimeDelta;
            }
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static string CutTextGently(string text, int preferredMaxLength)
        {
            if (text == null || text.Length <= preferredMaxLength)
                return text ?? "";

            //MatchCollection matchCollection1 = UIStringFormatterHelper._regex_Uri.Matches(text);
            //MatchCollection matchCollection2 = UIStringFormatterHelper._regex_Email.Matches(text);
            //MatchCollection matchCollection3 = UIStringFormatterHelper.Regex_DomainMention.Matches(text);
            //MatchCollection matchCollection4 = UIStringFormatterHelper.Regex_Mention.Matches(text);
            //MatchCollection matchCollection5 = UIStringFormatterHelper.Regex_Tag.Matches(text);


            MatchCollection matchCollection6 = UIStringFormatterHelper._linksRegex.Matches(text);

            int val1 = preferredMaxLength - 1;
            int num1 = text.IndexOf(' ', val1 + 1);
            if (num1 > 0 && num1 < preferredMaxLength + 30)
                val1 = num1;
            int num2 = text.IndexOfAny(new char[3] { '.', '!', '?' }, val1 + 1);
            if (num2 > 0 && num2 < preferredMaxLength + 200)
                val1 = num2;
            if (text.Length - val1 < 20)
                return text;
            /*
            foreach (Match match in matchCollection1)
            {
                string str = match.Groups.Count > 2 ? match.Groups[2].Value : string.Empty;
                if (UIStringFormatterHelper._domainsList.Contains((str.Contains(".") ? str.Remove(0, str.LastIndexOf(".") + 1) : string.Empty).ToUpper()) && match.Index <= val1 && match.Index + match.Length - 1 >= val1)
                    val1 = Math.Max(val1, match.Index + match.Length - 1);
            }
            foreach (Match match in matchCollection2)
            {
                if (match.Index <= val1 && match.Index + match.Length - 1 >= val1)
                    val1 = Math.Max(val1, match.Index + match.Length - 1);
            }
            foreach (Match match in matchCollection3)
            {
                if (match.Index <= val1 && match.Index + match.Length - 1 >= val1)
                    val1 = Math.Max(val1, match.Index + match.Length - 1);
            }
            foreach (Match match in matchCollection4)
            {
                if (match.Index <= val1 && match.Index + match.Length - 1 >= val1)
                    val1 = Math.Max(val1, match.Index + match.Length - 1);
            }
            foreach (Match match in matchCollection5)
            {
                if (match.Index <= val1 && match.Index + match.Length - 1 >= val1)
                    val1 = Math.Max(val1, match.Index + match.Length - 1);
            }*/
            foreach (Match match in matchCollection6)
            {
                if (match.Index <= val1 && match.Index + match.Length - 1 >= val1)
                    val1 = Math.Max(val1, match.Index + match.Length - 1);
            }

            return text.Substring(0, val1 + 1);
        }

        public static string BytesForUI(double size)
        {
            if (size < 1024.0)
                return string.Concat(Math.Round(size), " B");
            if (size < 1048576.0)
                return string.Concat((size / 1024.0).ToString("#.#"), " KB");
            if (size < 1073741824.0)
                return string.Concat((size / 1048576.0).ToString("#.#"), " MB");
            if (size < 1099511627776.0)
                return string.Concat((size / 1073741824.0).ToString("#.#"), " GB");
            return string.Concat((size / 1099511627776.0).ToString("#.#"), " TB");
        }
        
        /// <summary>
        /// Строка вида 1.1К
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string CountForUI(int count)
        {
            if(count >= 1000000)
                return string.Concat((count / 1000000.0).ToString("#.#"), "M");
            if(count >= 1000)
                return string.Concat((count / 1000.0).ToString("#.#"), "K");

            return count.ToString();
        }

        public static string FormatDuration(int durationSeconds)
        {
            if (durationSeconds < 3600)
                return TimeSpan.FromSeconds((double)durationSeconds).ToString("m\\:ss");
            return TimeSpan.FromSeconds((double)durationSeconds).ToString("h\\:mm\\:ss");
        }

        public static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalSeconds < 3600)
                return duration.ToString("m\\:ss");
            return duration.ToString("h\\:mm\\:ss");
        }

        public static string SubstituteMentionsWithNames(string text)
        {
            int startIndex = 0;
            MatchCollection matchCollection = UIStringFormatterHelper._userOrGroupRegEx.Matches(text);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Match match in matchCollection)
            {
                if (match.Index != startIndex)
                    stringBuilder = stringBuilder.Append(text.Substring(startIndex, match.Index - startIndex));
                int num = match.Value.IndexOf("|");
                string str = match.Value.Substring(num + 1, match.Value.Length - num - 2);
                stringBuilder = stringBuilder.Append(str);
                startIndex = match.Index + match.Length;
            }
            if (startIndex < text.Length)
                stringBuilder = stringBuilder.Append(text.Substring(startIndex));
            return stringBuilder.ToString();
        }

        public static string FormateDateForEventUI(DateTime dateTime)
        {
            string str;
            if (dateTime.Date != DateTime.Today)
            {
                DateTime dateTime1 = DateTime.Today.AddDays(-1.0);
                
                if (dateTime.Date == dateTime1.Date)
                {
                    str = LocalizedStrings.GetString("Yesterday");
                }
                else
                {
                    dateTime1 = DateTime.Today;
                    dateTime1 = dateTime1.AddDays(1.0);
                    
                    if (dateTime.Date == dateTime1.Date)
                    {
                        str = LocalizedStrings.GetString("Tomorrow");
                    }
                    else
                    {
                        str = dateTime.Day + " " + CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dateTime.Month);
                        
                        if (dateTime.Year != DateTime.Now.Year)
                            str = str + " " + dateTime.Year;
                    }
                }
            }
            else
                str = LocalizedStrings.GetString("Today");
            return str + " " + LocalizedStrings.GetString("At") + " " + dateTime.ToString("HH:mm");
        }

        public static string FormatDateForMessageUI(DateTime dateTime)
        {
            if (dateTime.Date != DateTime.Today)
            {
                string str = string.Empty;
                DateTime date2 = DateTime.Today.AddDays(-1.0).Date;
                if (dateTime.Date == date2)
                {
                    str = LocalizedStrings.GetString("Yesterday").ToLower();//вчера
                }
                else
                {
                    if(dateTime.Year == DateTime.Now.Year)
                    {
                        str = dateTime.ToString("d MMM");//29 май
                    }
                    else
                    {
                        str = dateTime.ToString("d MMM yyyy");//29 май 2015
                    }
                }

                return str;
            }

            return dateTime.ToString("H:mm");//5:50
        }
    }
}
