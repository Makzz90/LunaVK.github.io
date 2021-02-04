using LunaVK.Core;
using System;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System.Globalization;

namespace LunaVK.Common
{
    public class ThemeManager
    {
        private static bool _initialized;
        public static void ApplyColors()
        {
            ResourceDictionary temp = Application.Current.Resources.MergedDictionaries[0];
            ResourceDictionary lightDictionary = temp.ThemeDictionaries["Light"] as ResourceDictionary;
            ResourceDictionary darkDictionary = temp.ThemeDictionaries["Default"] as ResourceDictionary;

            

            if (!ThemeManager._initialized)
            {
                Color systemColor = (Color)Application.Current.Resources["SystemAccentColor"];
                Application.Current.Resources["AccentColor0"] = systemColor;
            }

            Color acc = (Color)Application.Current.Resources["AccentColor" + Settings.AccentColor];

            //systemColor.R = acc.R;
            //systemColor.G = acc.G;
            //systemColor.B = acc.B;
            //systemColor.A = acc.A;//Такое можно провернуть пока Фрейм не инициализирован
            //Application.Current.Resources["SystemAccentColor"] = acc;




            //var uiSettings = new UISettings();

            //Color AccentColor = uiSettings.GetColorValue(UIColorType.Accent);
            //var cssColorString = "rgba(" + rgba.R + "," + rgba.G + "," + rgba.B + ", " + rgba.A + ")";



            
            darkDictionary["SystemControlHighlightAccentBrush"] = new SolidColorBrush(acc);
            lightDictionary["SystemControlHighlightAccentBrush"] = new SolidColorBrush(acc);

            darkDictionary["SystemControlHighlightListAccentHighBrush"] = new SolidColorBrush(acc) { Opacity = 0.9 };
            lightDictionary["SystemControlHighlightListAccentHighBrush"] = new SolidColorBrush(acc) { Opacity = 0.7 };

            darkDictionary["SystemControlHighlightAltListAccentMediumBrush"] = new SolidColorBrush(acc) { Opacity = 0.8 };
            lightDictionary["SystemControlHighlightAltListAccentMediumBrush"] = new SolidColorBrush(acc) { Opacity = 0.6 };

            darkDictionary["SystemControlHighlightListAccentMediumBrush"] = new SolidColorBrush(acc) { Opacity = 0.8 };
            lightDictionary["SystemControlHighlightListAccentMediumBrush"] = new SolidColorBrush(acc) { Opacity = 0.6 };

            darkDictionary["SystemControlHighlightListAccentLowBrush"] = new SolidColorBrush(acc) { Opacity = 0.6 };
            lightDictionary["SystemControlHighlightListAccentLowBrush"] = new SolidColorBrush(acc) { Opacity = 0.4 };

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5) && !ThemeManager._initialized)
            {
                //RevealBrush, AcrylicBrush Windows 10 Fall Creators Update (introduced in 10.0.16299.0)
                Uri resources = new Uri("ms-appx:///Styles/rs3_Resources.xaml", UriKind.Absolute);
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = resources });
            }
            //SystemControlHighlightAccentBrush
            //
            //SystemControlHighlightAltListAccentMediumBrush
            //SystemControlHighlightListAccentMediumBrush

            //SystemControlForegroundBaseMediumBrush - кисть текста


            /*
                        TintColor — слой наложения цвета/оттенка. Можно задать RGB-значение цвета и прозрачность альфа-канала.
                        TintOpacity — непрозрачность цветного слоя. Рекомендуем начинать с 80% непрозрачности, хотя разные цвета могут выглядеть привлекательнее при других значениях.
                        BackgroundSource — флаг, указывающий тип акриловой поверхности (акрил фона или приложения).
                        FallbackColor — резервный сплошной цвет, заменяющий акрил в режиме экономии заряда. Акрил фона также сменяется резервным цветом, если окно приложения на рабочем столе неактивно или приложение запущено на телефоне или Xbox.
            */



#if WINDOWS_UWP

            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush") && ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                // Системный акрил
                
                if (!Settings.DisableAcrylicHeader)
                {
                    var lightHeader = new AcrylicBrush() { TintColor = acc, FallbackColor = acc, TintOpacity = 0.8 };
                    var lightHeader2 = new AcrylicBrush() { TintColor = acc, FallbackColor = acc, TintOpacity = 0.8 };
                    /*
                    lightHeader.BlurAmount = VKConstants.BLUR_AMOUNT;
                    lightHeader.BackdropFactor = VKConstants.BACKDROP_FACTOR;

                    lightHeader2.BlurAmount = VKConstants.BLUR_AMOUNT;
                    lightHeader2.BackdropFactor = VKConstants.BACKDROP_FACTOR;

                    lightHeader.BackdropFactor = 0.1f;
                    lightHeader2.BackdropFactor = 0.1f;
                    lightHeader.TintColorFactor = 1.0f;
                    lightHeader2.TintColorFactor = 1.0f;
                    */
                    lightDictionary["HeaderBrush"] = lightHeader;
                    lightDictionary["PivotHeaderBackground"] = lightHeader2;

                    var blackHeader = new AcrylicBrush() { TintColor = Colors.Black, FallbackColor = (Color)Application.Current.Resources["HeaderColor"], TintOpacity = 0.6 };
                    var blackHeader2 = new AcrylicBrush() { TintColor = Colors.Black, FallbackColor = (Color)Application.Current.Resources["HeaderColor"], TintOpacity = 0.6 };
                    //blackHeader.BlurAmount = VKConstants.BLUR_AMOUNT;
                    //blackHeader2.BlurAmount = VKConstants.BLUR_AMOUNT;
                    darkDictionary["HeaderBrush"] = blackHeader;
                    darkDictionary["PivotHeaderBackground"] = blackHeader2;

                }
                else
                {
                    lightDictionary["HeaderBrush"] = new SolidColorBrush(acc);
                    lightDictionary["PivotHeaderBackground"] = new SolidColorBrush(acc);
                    darkDictionary["HeaderBrush"] = new SolidColorBrush((Color)Application.Current.Resources["HeaderColor"]);
                    darkDictionary["PivotHeaderBackground"] = new SolidColorBrush((Color)Application.Current.Resources["HeaderColor"]);
                }

                if (!Settings.DisableAcrylicMenu)
                {
                    var whiteMenu = new AcrylicBrush() { TintColor = Windows.UI.Color.FromArgb(100, 255, 255, 255), FallbackColor = Colors.White, TintOpacity = 0.6 };
                    XamlCompositionBrushBase blackMenu;

                    if (Settings.UseAcrylicHost)
                        blackMenu = new AcrylicBrush() { BackgroundSource = AcrylicBackgroundSource.HostBackdrop, TintColor = Windows.UI.Colors.Black, FallbackColor = (Color)Application.Current.Resources["MenuColor"], TintOpacity = 0.7 };
                    else
                        blackMenu = new AcrylicBrush() { TintColor = Windows.UI.Colors.Black, FallbackColor = (Color)Application.Current.Resources["MenuColor"], TintOpacity = 0.7 };

                    /*
                    whiteMenu.BlurAmount = blackMenu.BlurAmount = VKConstants.BLUR_AMOUNT;
                    blackMenu.BackdropFactor = VKConstants.BACKDROP_FACTOR;
                    whiteMenu.BackdropFactor = 0.7f;
                    */
                    lightDictionary["BrushMenu"] = whiteMenu;
                    darkDictionary["BrushMenu"] = blackMenu;
                }
                else
                {
                    lightDictionary["BrushMenu"] = new SolidColorBrush(Colors.White);
                    darkDictionary["BrushMenu"] = new SolidColorBrush((Color)Application.Current.Resources["MenuColor"]);
                }
            }
            else if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase") && ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4))
            {
                if (!Settings.DisableAcrylicHeader)
                {
                    var lightHeader = new LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush() { TintColor = acc, FallbackColor = acc };
                    var lightHeader2 = new LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush() { TintColor = acc, FallbackColor = acc };
                    
                    lightHeader.BlurAmount = VKConstants.BLUR_AMOUNT;
                    lightHeader.BackdropFactor = VKConstants.BACKDROP_FACTOR;

                    lightHeader2.BlurAmount = VKConstants.BLUR_AMOUNT;
                    lightHeader2.BackdropFactor = VKConstants.BACKDROP_FACTOR;

                    lightHeader.BackdropFactor = 0.1f;
                    lightHeader2.BackdropFactor = 0.1f;
                    lightHeader.TintColorFactor = 1.0f;
                    lightHeader2.TintColorFactor = 1.0f;

                    lightDictionary["HeaderBrush"] = lightHeader;
                    lightDictionary["PivotHeaderBackground"] = lightHeader2;

                    var blackHeader = new LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush() { TintColor = Colors.Black, FallbackColor = (Color)Application.Current.Resources["HeaderColor"] };
                    var blackHeader2 = new LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush() { TintColor = Colors.Black, FallbackColor = (Color)Application.Current.Resources["HeaderColor"] };
                    blackHeader.BlurAmount = VKConstants.BLUR_AMOUNT;
                    blackHeader2.BlurAmount = VKConstants.BLUR_AMOUNT;
                    darkDictionary["HeaderBrush"] = blackHeader;
                    darkDictionary["PivotHeaderBackground"] = blackHeader2;
                    
                }
                else
                {
                    lightDictionary["HeaderBrush"] = new SolidColorBrush(acc);
                    lightDictionary["PivotHeaderBackground"] = new SolidColorBrush(acc);
                    darkDictionary["HeaderBrush"] = new SolidColorBrush((Color)Application.Current.Resources["HeaderColor"]);
                    darkDictionary["PivotHeaderBackground"] = new SolidColorBrush((Color)Application.Current.Resources["HeaderColor"]);
                }

                if (!Settings.DisableAcrylicMenu)
                {
                    LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrushBase whiteMenu = new LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush() { TintColor = Windows.UI.Color.FromArgb(100, 255, 255, 255), FallbackColor = Colors.White };
                    LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrushBase blackMenu;

                    if (Settings.UseAcrylicHost)
                        blackMenu = new LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicHostBrush() { TintColor = Windows.UI.Colors.Black, FallbackColor = (Color)Application.Current.Resources["MenuColor"] };
                    else
                        blackMenu = new LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush() { TintColor = Windows.UI.Colors.Black, FallbackColor = (Color)Application.Current.Resources["MenuColor"] };

                    whiteMenu.BlurAmount = blackMenu.BlurAmount = VKConstants.BLUR_AMOUNT;
                    blackMenu.BackdropFactor = VKConstants.BACKDROP_FACTOR;
                    whiteMenu.BackdropFactor = 0.7f;
                    //whiteMenu.TintColorFactor = 1;

                    lightDictionary["BrushMenu"] = whiteMenu;
                    darkDictionary["BrushMenu"] = blackMenu;
                }
                else
                {
                    lightDictionary["BrushMenu"] = new SolidColorBrush(Colors.White);
                    darkDictionary["BrushMenu"] = new SolidColorBrush((Color)Application.Current.Resources["MenuColor"]);
                }
            }
            else
            {
                lightDictionary["HeaderBrush"] = new SolidColorBrush(acc);
                lightDictionary["PivotHeaderBackground"] = new SolidColorBrush(acc);
                darkDictionary["HeaderBrush"] = new SolidColorBrush((Color)Application.Current.Resources["HeaderColor"]);
                darkDictionary["PivotHeaderBackground"] = new SolidColorBrush((Color)Application.Current.Resources["HeaderColor"]);
            }
#endif

            ThemeManager._initialized = true;
        }



        
    }










    /// <summary>
    /// List of Windows 10 UWP system colors. See <see href="https://msdn.microsoft.com/en-us/windows/uwp/style/color"/> for more information.
    /// </summary>
    public enum SystemColor
    {
        Accent, AccentLight3, AccentLight2, AccentLight1, AccentDark1, AccentDark2, AccentDark3,
        BaseLow, BaseMediumLow, BaseMedium, BaseMediumHigh, BaseHigh,
        AltLow, AltMediumLow, AltMedium, AltMediumHigh, AltHigh,
        ListLow, ListMedium, ListAccentLow, ListAccentMedium, ListAccentHigh,
        ChromeLow, ChromeMediumLow, ChromeMedium, ChromeHigh, ChromeAltLow, ChromeDisabledLow, ChromeDisabledHigh, ChromeBlackLow, ChromeBlackMediumLow, ChromeBlackMedium, ChromeBlackHigh, ChromeBlackWhite
    }

    /// <summary>
    /// Extension class to convert <see cref="SystemColor"/> to <see cref="Windows.UI.Color"/> or <see cref="Windows.UI.Xaml.Media.Brush"/>.
    /// </summary>
    public static class SystemColorExtension
    {
        private static string Name(this SystemColor color)
        {
            return color.ToString().StartsWith("Accent") ?
                string.Format("SystemAccentColor{0}", color.ToString().Substring("Accent".Length)) :
                string.Format("System{0}Color", color.ToString());
        }

        /// <example> <code> SystemColor.Accent.Hex(); </code> </example>
        private static string Hex(this SystemColor color)
        {
            return Application.Current.Resources[color.Name()].ToString();
        }

        /// <example> <code> SystemColor.Accent.Color(); </code> </example>
        public static Color Color(this SystemColor color)
        {
            var hex = Hex(color);
            var a = byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
            var r = byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(7, 2), NumberStyles.HexNumber);
            return Windows.UI.Color.FromArgb(a, r, g, b);
        }

        /// <example> <code> SystemColor.Accent.Brush(); </code> </example>
        public static SolidColorBrush Brush(this SystemColor color)
        {
            return new SolidColorBrush(Color(color));
        }
    }
}

/*
 * <SolidColorBrush x:Key="SystemControlBackgroundAccentBrush" Color="{ThemeResource SystemAccentColor}"/>
<SolidColorBrush x:Key="SystemControlDisabledAccentBrush" Color="{ThemeResource SystemAccentColor}"/>
<SolidColorBrush x:Key="SystemControlForegroundAccentBrush" Color="{ThemeResource SystemAccentColor}"/>
<SolidColorBrush x:Key="SystemControlHighlightAccentBrush" Color="{ThemeResource SystemAccentColor}"/>
<SolidColorBrush x:Key="SystemControlHighlightAltAccentBrush" Color="{ThemeResource SystemAccentColor}"/>
<SolidColorBrush x:Key="SystemControlHighlightAltListAccentHighBrush" Color="{ThemeResource SystemAccentColor}" Opacity="0.9" />
<SolidColorBrush x:Key="SystemControlHighlightAltListAccentLowBrush" Color="{ThemeResource SystemAccentColor}" Opacity="0.6" />
<SolidColorBrush x:Key="SystemControlHighlightAltListAccentMediumBrush" Color="{ThemeResource SystemAccentColor}" Opacity="0.8" />
<SolidColorBrush x:Key="SystemControlHighlightListAccentHighBrush" Color="{ThemeResource SystemAccentColor}" Opacity="0.9" />
<SolidColorBrush x:Key="SystemControlHighlightListAccentLowBrush" Color="{ThemeResource SystemAccentColor}" Opacity="0.6" />
<SolidColorBrush x:Key="SystemControlHighlightListAccentMediumBrush" Color="{ThemeResource SystemAccentColor}" Opacity="0.8" />
<SolidColorBrush x:Key="SystemControlHyperlinkTextBrush" Color="{ThemeResource SystemAccentColor}"/>
<SolidColorBrush x:Key="ContentDialogBorderThemeBrush" Color="{ThemeResource SystemAccentColor}" />
<SolidColorBrush x:Key="JumpListDefaultEnabledBackground" Color="{ThemeResource SystemAccentColor}" />

<SolidColorBrush x:Key="SystemControlBackgroundAccentBrush" Color="{ThemeResource SystemAccentColor}"/>
<SolidColorBrush x:Key="SystemControlDisabledAccentBrush" Color="{ThemeResource SystemAccentColor}"/>
<SolidColorBrush x:Key="SystemControlForegroundAccentBrush" Color="{ThemeResource SystemAccentColor}"/>
<SolidColorBrush x:Key="SystemControlHighlightAccentBrush" Color="{ThemeResource SystemAccentColor}"/>
<SolidColorBrush x:Key="SystemControlHighlightAltAccentBrush" Color="{ThemeResource SystemAccentColor}"/>
<SolidColorBrush x:Key="SystemControlHighlightAltListAccentHighBrush" Color="{ThemeResource SystemAccentColor}" Opacity="0.7" />
<SolidColorBrush x:Key="SystemControlHighlightAltListAccentLowBrush" Color="{ThemeResource SystemAccentColor}" Opacity="0.4" />
<SolidColorBrush x:Key="SystemControlHighlightAltListAccentMediumBrush" Color="{ThemeResource SystemAccentColor}" Opacity="0.6" />
<SolidColorBrush x:Key="SystemControlHighlightListAccentHighBrush" Color="{ThemeResource SystemAccentColor}" Opacity="0.7" />
<SolidColorBrush x:Key="SystemControlHighlightListAccentLowBrush" Color="{ThemeResource SystemAccentColor}" Opacity="0.4" />
<SolidColorBrush x:Key="SystemControlHighlightListAccentMediumBrush" Color="{ThemeResource SystemAccentColor}" Opacity="0.6" />
<SolidColorBrush x:Key="SystemControlHyperlinkTextBrush" Color="{ThemeResource SystemAccentColor}"/>
<SolidColorBrush x:Key="ContentDialogBorderThemeBrush" Color="{ThemeResource SystemAccentColor}" />
<SolidColorBrush x:Key="JumpListDefaultEnabledBackground" Color="{ThemeResource SystemAccentColor}" />

<Color x:Key="SystemAltHighColor">#FF000000</Color>
<Color x:Key="SystemAltLowColor">#33000000</Color>
<Color x:Key="SystemAltMediumColor">#99000000</Color>
<Color x:Key="SystemAltMediumHighColor">#CC000000</Color>
<Color x:Key="SystemAltMediumLowColor">#66000000</Color>
<Color x:Key="SystemBaseHighColor">#FFFFFFFF</Color>
<Color x:Key="SystemBaseLowColor">#33FFFFFF</Color>
<Color x:Key="SystemBaseMediumColor">#99FFFFFF</Color>
<Color x:Key="SystemBaseMediumHighColor">#CCFFFFFF</Color>
<Color x:Key="SystemBaseMediumLowColor">#66FFFFFF</Color>
<Color x:Key="SystemChromeAltLowColor">#FFF2F2F2</Color>
<Color x:Key="SystemChromeBlackHighColor">#FF000000</Color>
<Color x:Key="SystemChromeBlackLowColor">#33000000</Color>
<Color x:Key="SystemChromeBlackMediumLowColor">#66000000</Color>
<Color x:Key="SystemChromeBlackMediumColor">#CC000000</Color>
<Color x:Key="SystemChromeDisabledHighColor">#FF333333</Color>
<Color x:Key="SystemChromeDisabledLowColor">#FF858585</Color>
<Color x:Key="SystemChromeHighColor">#FF767676</Color>
<Color x:Key="SystemChromeLowColor">#FF171717</Color>
<Color x:Key="SystemChromeMediumColor">#FF1F1F1F</Color>
<Color x:Key="SystemChromeMediumLowColor">#FF2B2B2B</Color>
<Color x:Key="SystemChromeWhiteColor">#FFFFFFFF</Color>
<Color x:Key="SystemListLowColor">#19FFFFFF</Color>
<Color x:Key="SystemListMediumColor">#33FFFFFF</Color>

    */