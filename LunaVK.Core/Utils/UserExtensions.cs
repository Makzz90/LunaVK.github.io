using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunaVK.Core.DataObjects;

namespace LunaVK.Core.Utils
{
    public static class UserExtensions
    {
        public static bool IsBirthdayToday(this VKUser user)
        {
            DateTime now = DateTime.Now;
            return UserExtensions.IsBirthdayOnDate(user, now.Day, now.Month);
        }

        public static bool IsBirthdayTomorrow(this VKUser user)
        {
            DateTime dateTime = DateTime.Now.AddDays(1);
            return UserExtensions.IsBirthdayOnDate(user, dateTime.Day, dateTime.Month);
        }

        private static bool IsBirthdayOnDate(VKUser user, int day, int month)
        {
            if (user == null || user.bdate == null)
                return false;
            string[] strArray = user.bdate.Split('.');
            int result1 = 0;
            int result2 = 0;
            if (strArray.Length >= 2 && int.TryParse(strArray[0], out result1) && (int.TryParse(strArray[1], out result2) && day == result1))
                return month == result2;
            return false;
        }

        public static int GetBDateYear(this VKUser user)
        {
            if (user == null || user.bdate == null)
                return 0;
            string[] strArray = user.bdate.Split('.');
            if (strArray.Length < 3)
                return 0;
            int result = 0;
            if (int.TryParse(strArray[2], out result))
                return result;
            return 0;
        }

        public static int GetBDateMonth(this VKUser user)
        {
            if (user == null || user.bdate == null)
                return 0;
            string[] strArray = user.bdate.Split('.');
            if (strArray.Length < 2)
                return 0;

            int result = 0;
            if (int.TryParse(strArray[1], out result))
                return result;
            return 0;
        }

        public static int GetBDateDay(this VKUser user)
        {
            if (user == null || user.bdate == null)
                return 0;
            string[] strArray = user.bdate.Split('.');
            if (strArray.Length < 1)
                return 0;
            int result = 0;
            if (int.TryParse(strArray[0], out result))
                return result;
            return 0;
        }

        public static string GetBDateString(this VKUser user)
        {
            if (user == null || user.bdate == null)
                return "";

            string[] strArray = user.bdate.Split('.');
            

            int day = 0;
            int month = 0;
            int year = 0;

            int.TryParse(strArray[0], out day);
            int.TryParse(strArray[1], out month);

            if(strArray.Length > 2)
                int.TryParse(strArray[2], out year);

            string str = string.Format("{0}.{1}.", day, month);
            DateTime result;
            if (DateTime.TryParse(year == 0 ? str + "1970" : str + year.ToString(), new CultureInfo("ru-RU"), DateTimeStyles.None, out result))
                return result.ToString(year>0 ? "d MMMM yyyy" : "M");
            return "";
        }
    }
}
