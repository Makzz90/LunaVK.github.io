using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace LunaVK.Photo
{
    public static class PredefinedColorPalettes
    {
        public static List<SolidColorBrush> GetSolidColorBrushProviders(string colorFileName = "")
        {
            List<SolidColorBrush> brushProviderList = new List<SolidColorBrush>() { new SolidColorBrush( Colors.Black ), new SolidColorBrush( Colors.Gray ), new SolidColorBrush( Colors.White ) };
            double[] numArray = new double[3] { 0.3, 0.5, 0.7 };
            double sat = 0.7;
            if (colorFileName.Contains("neon"))
                sat = 1.0;
            if (colorFileName.Contains("pastel"))
                sat = 0.4;
            for (int i = 0; i < 360; i += 15)
            {
                //brushProviderList.AddRange(((IEnumerable<double>)numArray).Select<double, HslColor>((Func<double, HslColor>)(l => HslColor.FromHsl(i, sat, l))).Select<HslColor, SerializableSolidColorBrush>((Func<HslColor, SerializableSolidColorBrush>)(hsl => new SerializableSolidColorBrush()
                //{
                //    Color = hsl.ToPlatformColor()
                //})).Cast<IBrushProvider>());
                brushProviderList.Add(new SolidColorBrush(PredefinedColorPalettes.FromHsl(i,sat,0.5)));
            }
            return brushProviderList;
        }

        #region FromHsl()
        /// <summary>
        /// Returns a Color struct based on HSL model.
        /// </summary>
        /// <param name="hue">0..360 range hue</param>
        /// <param name="saturation">0..1 range saturation</param>
        /// <param name="lightness">0..1 range lightness</param>
        /// <param name="alpha">0..1 alpha</param>
        /// <returns></returns>
        private static Color FromHsl(double hue, double saturation, double lightness, double alpha = 1.0)
        {
            //Debug.Assert(hue >= 0);
            //Debug.Assert(hue <= 360);

            double chroma = (1 - Math.Abs(2 * lightness - 1)) * saturation;
            double h1 = hue / 60;
            double x = chroma * (1 - Math.Abs(h1 % 2 - 1));
            double m = lightness - 0.5 * chroma;
            double r1, g1, b1;

            if (h1 < 1)
            {
                r1 = chroma;
                g1 = x;
                b1 = 0;
            }
            else if (h1 < 2)
            {
                r1 = x;
                g1 = chroma;
                b1 = 0;
            }
            else if (h1 < 3)
            {
                r1 = 0;
                g1 = chroma;
                b1 = x;
            }
            else if (h1 < 4)
            {
                r1 = 0;
                g1 = x;
                b1 = chroma;
            }
            else if (h1 < 5)
            {
                r1 = x;
                g1 = 0;
                b1 = chroma;
            }
            else //if (h1 < 6)
            {
                r1 = chroma;
                g1 = 0;
                b1 = x;
            }

            byte r = (byte)(255 * (r1 + m));
            byte g = (byte)(255 * (g1 + m));
            byte b = (byte)(255 * (b1 + m));
            byte a = (byte)(255 * alpha);

            return Color.FromArgb(a, r, g, b);
        }
        #endregion
    }
}
