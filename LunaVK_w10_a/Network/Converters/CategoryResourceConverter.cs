using System;
using Windows.UI.Xaml.Data;

namespace LunaVK.Network.Converters
{
    public class CategoryResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string code = "";

            switch((string)value)
            {
                case "Emotion":
                    {
                        code = "D83DDE0A";
                        break;
                    }
                case "Gestures":
                    {
                        code = "D83DDC4D";
                        break;
                    }
                case "Animals":
                    {
                        code = "D83DDC13";
                        break;
                    }
                case "Plants":
                    {
                        code = "D83CDF40";
                        break;
                    }
                case "Love":
                    {
                        code = "2764";
                        break;
                    }
                case "Transport":
                    {
                        code = "D83DDE98";
                        break;
                    }
                case "People":
                    {
                        code = "D83DDC71";
                        break;
                    }
                case "Food":
                    {
                        code = "D83CDF4F";
                        break;
                    }
                case "Music":
                    {
                        code = "D83CDFB5";
                        break;
                    }
                case "Events":
                    {
                        code = "D83CDF81";
                        break;
                    }
                case "Sport":
                    {
                        code = "26BD";
                        break;
                    }
                case "Gadgets":
                    {
                        code = "D83DDCBB";
                        break;
                    }
                case "Signs":
                    {
                        code = "2139";
                        break;
                    }
                case "Numbers":
                    {
                        code = "003520E3";
                        break;
                    }
                case "Arrow":
                    {
                        code = "2B07";
                        break;
                    }
                case "Zodiac":
                    {
                        code = "264C";
                        break;
                    }
                case "Office":
                    {
                        code = "D83DDCCC";
                        break;
                    }
                case "Flags":
                    {
                        code = "D83CDDF7D83CDDFA";
                        break;
                    }
                case "Wear":
                    {
                        code = "D83DDC55";
                        break;
                    }
                case "Space":
                    {
                        code = "D83CDF17";
                        break;
                    }
                case "Building":
                    {
                        code = "D83CDFE2";
                        break;
                    }
                case "Weather":
                    {
                        code = "26C5";
                        break;
                    }
                case "Games":
                    {
                        code = "D83CDFAE";
                        break;
                    }
                case "Pictures":
                    {
                        code = "D83CDF04";
                        break;
                    }
            }
            return new Uri("ms-appx:///Assets/Emoji/" + code + ".png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
