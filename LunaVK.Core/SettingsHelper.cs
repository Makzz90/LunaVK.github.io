#define USE_NEWMETHODE

using Newtonsoft.Json;
using System.IO;
using Windows.Storage;
using System.Runtime.CompilerServices;
using System;

//uint8 int16 uint16 int32 uint32 int64 uint64 single double char16 string datetime(не поддерживается) timespn guid point size rect

namespace LunaVK.Core
{
    public static class SettingsHelper
    {
        private const string Settings = "Settings";
        
        public static bool IsExists
        {
            get { return ApplicationData.Current.LocalSettings.Containers.ContainsKey(SettingsHelper.Settings); }
        }

        public static void Clear()
        {
            ApplicationData.Current.LocalSettings.DeleteContainer(SettingsHelper.Settings);
        }

        public static void Set<T>(T value, [CallerMemberName]string settingName = "")
        {
            try
            {
                var container = ApplicationData.Current.LocalSettings.CreateContainer(Settings, ApplicationDataCreateDisposition.Always);

                if (value == null)
                {
                    container.Values.Remove(settingName);
                    return;
                }
#if USE_NEWMETHODE
                if (value is int || value is uint || value is double || value is string || value is bool || value is short || value is ushort || value is byte || value is TimeSpan)
                {
                    container.Values[settingName] = value;
                }
                else
                {
                    using (var writter = new StringWriter())
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(writter, value);
                        container.Values[settingName] = writter.GetStringBuilder().ToString();
                    }
                }
#else
                using (var writter = new StringWriter())
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writter, value);
                    container.Values[settingName] = writter.GetStringBuilder().ToString();
                }
#endif
            }
            catch
            {
            }
        }

        public static T Get<T>([CallerMemberName]string settingName = "", T defaultValue = default(T))
        {
            try
            {
                var container = ApplicationData.Current.LocalSettings.CreateContainer(SettingsHelper.Settings, ApplicationDataCreateDisposition.Always);

                object value;
                if (container.Values.TryGetValue(settingName, out value))
                {
#if USE_NEWMETHODE
                    if (value is int || value is uint || value is double || value is bool || value is short || value is ushort || value is byte || value is TimeSpan)
                        return (T)value;


                    if (value is string str)
                    {
                        if (str == "" || ( str[0] != '{' && str[0] != '\"' ))
                            return (T)value;
                        else
                        {
                            using (StringReader reader = new StringReader((string)value))
                            {
                                using (JsonTextReader jsonReader = new JsonTextReader(reader))
                                {
                                    JsonSerializer serializer = new JsonSerializer();
                                    T result = serializer.Deserialize<T>(jsonReader);
                                    return result;
                                }
                            }
                        }
                    }
#else
                    using (StringReader reader = new StringReader((string)value))
                    {
                        using (JsonTextReader jsonReader = new JsonTextReader(reader))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            T result = serializer.Deserialize<T>(jsonReader);
                            return result;
                        }
                    }
#endif
                }
            }
            catch
            {
            }

            //if (defaultValue != null)
                return defaultValue;

            //return default(T);
        }

        public static T GetOld<T>([CallerMemberName]string settingName = "", T defaultValue = default(T))
        {
            try
            {
                var container = ApplicationData.Current.LocalSettings.CreateContainer(SettingsHelper.Settings, ApplicationDataCreateDisposition.Always);

                object value;
                if (container.Values.TryGetValue(settingName, out value))
                {

                    using (StringReader reader = new StringReader((string)value))
                    {
                        using (JsonTextReader jsonReader = new JsonTextReader(reader))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            T result = serializer.Deserialize<T>(jsonReader);
                            return result;
                        }
                    }
                }
            }
            catch
            {
            }

            //if (defaultValue != null)
            return defaultValue;

            //return default(T);
        }
    }
}
