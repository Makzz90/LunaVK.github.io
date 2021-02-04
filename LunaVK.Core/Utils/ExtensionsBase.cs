using System;
using System.Text;
using Windows.Security.Cryptography;
using LunaVK.Core.Library;
using Windows.Data.Html;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace LunaVK.Core.Utils
{
    public static class ExtensionsBase
    {
        public static Uri ConvertToUri(this string uriStr)
        {
            return !string.IsNullOrEmpty(uriStr) ? (uriStr.StartsWith(".") || uriStr.StartsWith("/") ? new Uri(uriStr, UriKind.Relative) : new Uri(uriStr, UriKind.Absolute)) : null;
        }
        
        public static string GetShorterVersion(this string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length < 32)
                return str;

            return Framework.CacheManager2.ComputeMD5(str);//MD5Core.GetHashString(str);
        }
        
        public static string ForUI(this string backendTextString)
        {
            if (string.IsNullOrEmpty(backendTextString))
                return "";

            //backendTextString = System.Net.WebUtility.HtmlDecode(backendTextString);
            //serverUpdateData.text = Windows.Data.Html.HtmlUtilities.ConvertToText(text);//на 10.0.10586 вылеты

            return System.Net.WebUtility.HtmlDecode(backendTextString.Replace("<br>", Environment.NewLine).Replace("<br/>", Environment.NewLine).Replace("\r\n", "\n").Replace("\n", "\r\n")).Trim();
        }

        public static void CopyTo(this Object source, Object destination)
        {
            Type sourceType = source.GetType();
            Type destinationType = destination.GetType();

            //foreach (PropertyInfo sourceProperty in sourceType.GetProperties())
            //{
            //    PropertyInfo destinationProperty = destinationType.GetProperty(sourceProperty.Name);
            //    if (destinationProperty != null)
            //    {
            //        destinationProperty.SetValue(destination, sourceProperty.GetValue(this, null), null);
            //    }
            //}

            IEnumerable<PropertyInfo> propertiesS = sourceType.GetRuntimeProperties();
            IEnumerable<PropertyInfo> propertiesD = destinationType.GetRuntimeProperties();

            foreach (PropertyInfo sourceProperty in propertiesS)
            {
                PropertyInfo destProperty = destinationType.GetRuntimeProperty(sourceProperty.Name);
                if (destProperty != null && destProperty.CanWrite)
                {
                    object temp = sourceProperty.GetValue(source, null);
                    destProperty.SetValue(destination, temp,null);
                }
            }
        }
        /*
        public static string HtmlDecode(string html)
        {
            if (html == null)
            {
                return null;
            }
            if (html.IndexOf('&') < 0)
            {
                return html;
            }
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb, CultureInfo.InvariantCulture);
            int length = html.Length;
            for (int i = 0; i < length; i++)
            {
                char ch = html[i];
                if (ch == '&')
                {
                    int num3 = html.IndexOfAny(s_entityEndingChars, i + 1);
                    if ((num3 > 0) && (html[num3] == ';'))
                    {
                        string entity = html.Substring(i + 1, (num3 - i) - 1);
                        if ((entity.Length > 1) && (entity[0] == '#'))
                        {
                            try
                            {
                                if ((entity[1] == 'x') || (entity[1] == 'X'))
                                {
                                    ch = (char)int.Parse(entity.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    ch = (char)int.Parse(entity.Substring(1), CultureInfo.InvariantCulture);
                                }
                                i = num3;
                            }
                            catch (FormatException)
                            {
                                i++;
                            }
                            catch (ArgumentException)
                            {
                                i++;
                            }
                        }
                        else
                        {
                            i = num3;
                            char ch2 = HtmlEntities.Lookup(entity);
                            if (ch2 != '\0')
                            {
                                ch = ch2;
                            }
                            else
                            {
                                writer.Write('&');
                                writer.Write(entity);
                                writer.Write(';');
                                continue;
                            }
                        }
                    }
                }
                writer.Write(ch);
            }
            return sb.ToString();
        }*/
    }
}
