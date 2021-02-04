using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.DataObjects
{
    public class CropPhoto
    {
        /// <summary>
        /// объект photo фотографии пользователя, из которой вырезается главное фото профиля
        /// </summary>
        public VKPhoto photo { get; set; }

        /// <summary>
        /// вырезанная фотография пользователя
        /// </summary>
        public CropRect crop { get; set; }

        /// <summary>
        /// миниатюрная квадратная фотография, вырезанная из фотографии crop.
        /// Содержит набор полей, аналогичный объекту crop
        /// </summary>
        public CropRect rect { get; set; }

        public class CropRect
        {
            /// <summary>
            /// координата X левого верхнего угла в процентах
            /// </summary>
            public float x { get; set; }

            /// <summary>
            /// координата Y левого верхнего угла в процентах
            /// </summary>
            public float y { get; set; }

            /// <summary>
            /// координата X правого нижнего угла в процентах
            /// </summary>
            public float x2 { get; set; }

            /// <summary>
            /// координата Y правого нижнего угла в процентах
            /// </summary>
            public float y2 { get; set; }
            /*
            public Windows.Foundation.Rect GetCroppingRectangle(double width, double height)
            {
                double num1 = this.x * width / 100.0;
                double num2 = this.x2 * width / 100.0;
                double num3 = this.y * height / 100.0;
                double num4 = this.y2 * height / 100.0;
                double num5 = num2 - num1;
                double num7 = num4 - num3;
                return new Windows.Foundation.Rect((double)num1, (double)num3 + 94, (double)num5, (double)num7);
            }*/
        }
    }
}
