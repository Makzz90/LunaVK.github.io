using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Library
{
    public class RGB
    {
        public byte R;
        public byte G;
        public byte B;

        public RGB(byte r,byte g, byte b)
        {
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public RGB(Windows.UI.Color color)
        {
            this.R = color.R;
            this.G = color.G;
            this.B = color.B;
        }

        public Windows.UI.Color AsColor
        {
            get
            {
                Windows.UI.Color ret = new Windows.UI.Color();
                ret.R = this.R;
                ret.G = this.G;
                ret.B = this.B;
                ret.A = byte.MaxValue;
                return ret;
            }
        }
    }
}
