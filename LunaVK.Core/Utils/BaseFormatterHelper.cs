using System;
using System.Collections.Generic;
using System.Linq;

namespace LunaVK.Core.Utils
{
    public class BaseFormatterHelper
    {
        public static List<string> ParsePhoneNumbers(string rawStr)
        {
            if (rawStr == null || rawStr == "")
                return new List<string>();
            rawStr = rawStr.Replace("+", ",+");
            string[] strArray = rawStr.Split(new char[2] { ',', ';' });
            List<string> stringList = new List<string>();
            foreach (string str1 in strArray)
            {
                string source1 = str1.Replace('-', ' ').Replace('(', ' ').Replace(')', ' ').Trim();
                bool num = source1.Length <= 0 ? false : (source1[0] == '+' ? true : false);
                string str2 = new string(source1.Where<char>((c =>
                {
                    if (!char.IsDigit(c))
                        return c == ' ';
                    return true;
                })).ToArray<char>());
                if (num)
                    str2 = "+" + str2;
                string source2 = str2;
                
                if (new string(source2.Where<char>((c) => char.IsDigit(c)).ToArray<char>()).Length >= 4)
                    stringList.Add(str2);
            }
            return stringList;
        }
    }
}
