using System.Collections.Generic;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;

namespace LunaVK.Core
{
    /*
    public static class LocalizedStrings0
    {
        private static ResourceLoader resourceLoader = new ResourceLoader();
 
        
        /// <summary>
        /// Returns the localized value of the specified key.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        /// <returns>The appropriate string value of the localized resources.</returns>
        public static string GetString(string key)
        {
#if DEBUG
            string temp = resourceLoader.GetString(key);
            Debug.Assert(!string.IsNullOrEmpty(temp));
#endif
            return resourceLoader.GetString(key);
        }
    }
    */




    public class LocalizedStrings
    {
        private static ResourceLoader resourceLoader = new ResourceLoader();
        public string this[string key]
        {
            get
            {
                return resourceLoader.GetString(key);
            }
        }

        public static string GetString(string key)
        {
#if DEBUG
            string temp = resourceLoader.GetString(key);
            Debug.Assert(!string.IsNullOrEmpty(temp));
#endif
            return resourceLoader.GetString(key);
        }
    }

}
