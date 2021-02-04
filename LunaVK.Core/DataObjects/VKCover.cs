using System.Collections.Generic;
using Newtonsoft.Json;
using LunaVK.Core.Json;
using Windows.UI.Xaml;
using Windows.Graphics.Display;
using System.Linq;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// обложка сообщества
    /// </summary>
    public class VKCover
    {
        /// <summary>
        /// копии изображений обложки
        /// </summary>
        public List<VKImageWithSize> images;

        /// <summary>
        /// включена ли обложка
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool enabled { get; set; }

        #region VM
        public string CurrentImage
        {
            get
            {
                if (this.images == null || this.images.Count == 0)
                    return null;
                //width *= (double)ScaleFactor.GetRealScaleFactor() / 100.0;

                var width = Window.Current.Bounds.Width * DisplayProperties.LogicalDpi;


                foreach (VKImageWithSize coverImage in this.images.OrderBy<VKImageWithSize, int>(i => i.width))
                {
                    if ((double)coverImage.width >= width)
                        return coverImage.url;
                }
                return this.images.LastOrDefault<VKImageWithSize>().url;
            }
        }
        #endregion
    }
}
