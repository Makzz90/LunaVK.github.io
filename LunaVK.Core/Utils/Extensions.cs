using System;
using System.Collections.Generic;
using System.Text;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using Windows.UI;
using Windows.UI.Xaml;

namespace LunaVK.Core.Utils
{
    public static class Extensions
    {
        public static string GetUserStatusString(this VKUser user)
        {
            if (user == null)
                return "";

            if (user.last_seen == null)
                return "";

            string str;

            if (user.online == true)
            {
                str = LocalizedStrings.GetString("Online");//в сети с
            }
            else
            {
                DateTime dateTime = user.last_seen.time;
                int int32 = Convert.ToInt32(Math.Floor((DateTime.Now - dateTime).TotalMinutes));

                if (int32 > 0 && int32 < 60)
                {
                    if (int32 < 2)
                    {
                        str = LocalizedStrings.GetString(user.sex == VKUserSex.Female ? "LastSeenAMomentAgoFemale" : "LastSeenAMomentAgoMale");
                    }
                    else
                    {
                        int num = int32 % 10;
                        str = user.sex == VKUserSex.Female ? (num != 1 || int32 >= 10 && int32 <= 20 ? (num >= 5 || num == 0 || int32 >= 10 && int32 <= 20 ? string.Format(LocalizedStrings.GetString("LastSeenXFiveMinutesAgoFemaleFrm"), int32) : string.Format(LocalizedStrings.GetString("LastSeenXTwoFourMinutesAgoFemaleFrm"), int32)) : string.Format(LocalizedStrings.GetString("LastSeenX1MinutesAgoFemaleFrm"), int32)) : (num != 1 || int32 >= 10 && int32 <= 20 ? (num >= 5 || num == 0 || int32 >= 10 && int32 <= 20 ? string.Format(LocalizedStrings.GetString("LastSeenXFiveMinutesAgoMaleFrm"), int32) : string.Format(LocalizedStrings.GetString("LastSeenXTwoFourMinutesAgoMaleFrm"), int32)) : string.Format(LocalizedStrings.GetString("LastSeenX1MinutesAgoMaleFrm"), int32));
                    }
                }
                else
                    str = !(DateTime.Now.Date == dateTime.Date) ? (!(DateTime.Now.AddDays(-1.0).Date == dateTime.Date) ? (DateTime.Now.Year != dateTime.Year ? (user.sex == VKUserSex.Female ? string.Format(LocalizedStrings.GetString("LastSeenOnFemaleFrm"), dateTime.ToString("dd.MM.yyyy"), dateTime.ToString("HH:mm")) : string.Format(LocalizedStrings.GetString("LastSeenOnMaleFrm"), dateTime.ToString("dd.MM.yyyy"), dateTime.ToString("HH:mm"))) : (user.sex == VKUserSex.Female ? string.Format(LocalizedStrings.GetString("LastSeenOnFemaleFrm"), dateTime.ToString("dd.MM"), dateTime.ToString("HH:mm")) : string.Format(LocalizedStrings.GetString("LastSeenOnMaleFrm"), dateTime.ToString("dd.MM"), dateTime.ToString("HH:mm")))) : (user.sex == VKUserSex.Female ? string.Format(LocalizedStrings.GetString("LastSeenYesterdayFemaleFrm"), dateTime.ToString("HH:mm")) : string.Format(LocalizedStrings.GetString("LastSeenYesterdayMaleFrm"), dateTime.ToString("HH:mm")))) : (user.sex == VKUserSex.Female ? string.Format(LocalizedStrings.GetString("LastSeenTodayFemaleFrm"), dateTime.ToString("HH:mm")) : string.Format(LocalizedStrings.GetString("LastSeenTodayMaleFrm"), dateTime.ToString("HH:mm")));
            }

            if (!string.IsNullOrEmpty(user.online_app))
            {
                string tttt = "";
                switch (user.online_app)
                {
                    case "3502561":
                        {
                            tttt = "Windows Phone";
                            break;
                        }
                    case "3140623":
                        {
                            tttt = "iPhone";
                            break;
                        }
                    case "2274003":
                        {
                            tttt = "Android";
                            break;
                        }
                    case "2685278":
                        {
                            tttt = "KateMobile";
                            break;
                        }
                    case "3265802":
                        {
                            tttt = "Api console";
                            break;
                        }
                    case "3682744":
                        {
                            tttt = "iPad";
                            break;
                        }
                    case "5674548":
                        {
                            tttt = "ВКонтакте Pro";
                            break;
                        }
                    case "5316500":
                        {
                            tttt = "VFeed pro";
                            break;
                        }
                    case "4542624":
                        {
                            tttt = "Black VK";
                            break;
                        }
                    case "5632485":
                        {
                            tttt = "Space VK";
                            break;
                        }
                    case "6244854":
                        {
                            tttt = "Luna VK";
                            break;
                        }
                }

                if (string.IsNullOrEmpty(tttt))
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("unknown app -> " + user.online_app);
#endif
                    str += (" (" + user.last_seen.platform.ToString() + ")");
                }
                else
                {
                    str += (" (" + tttt + ")");
                }
            }
            else
            {
                str += " с ";

                if (user.last_seen.platform == VKPlatform.iPad || user.last_seen.platform == VKPlatform.iPhone)
                {
                    str += user.last_seen.platform.ToString();
                }
                else if (user.last_seen.platform == VKPlatform.Mobile)
                {
                    str += "телефона";
                }
                else if (user.last_seen.platform == VKPlatform.Web)
                {
                    str += "компьютера";
                }
                else if (user.last_seen.platform == VKPlatform.Android)
                {
                    str += "Android устройства";
                }
                else if (user.last_seen.platform == VKPlatform.WindowsPhone)
                {
                    str += "Windows Phone";
                }
            }

            return str;
        }

        public static int DateTimeToUnixTimestamp(DateTime dt, bool includeTimeDiff = true)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0);
            int num = (int)((dt.Ticks - dateTime.Ticks) / 10000000L);
            if (includeTimeDiff)
            {
                int minusLocalTimeDelta = Settings.ServerMinusLocalTimeDelta;
                num += minusLocalTimeDelta;
            }
            return num;
        }

        public static Color ToColor(this string colorHex)
        {
            return Color.FromArgb(Convert.ToByte(colorHex.Substring(1, 2), 16), Convert.ToByte(colorHex.Substring(3, 2), 16), Convert.ToByte(colorHex.Substring(5, 2), 16), Convert.ToByte(colorHex.Substring(7, 2), 16));
        }

        /// <summary>
        /// True -> Visible
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Visibility ToVisiblity(this bool value)
        {
            return value == true ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
