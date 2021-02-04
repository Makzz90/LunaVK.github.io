using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
//https://github.com/Tyrrrz/YoutubeExplode/blob/55483163c23e8115144d2887029641399e9b6e4d/YoutubeExplode/YoutubeClient.Video.cs
namespace YoutubeExtractor
{
    internal static class Decipherer
    {

        public static string DecipherWithVersion(string cipher, string cipherVersion)
        {
            //string jsUrl = string.Format("http://s.ytimg.com/yts/jsbin/player_{0}.js", cipherVersion);//_
            string jsUrl = string.Format("https://www.youtube.com{0}", cipherVersion);
            string js = HttpHelper.DownloadString(jsUrl);

            //Find "C" in this: var A = B.sig||C (B.s)
            //string functNamePattern = @"\""signature"",\s?([a-zA-Z0-9\$]+)\("; //Regex Formed To Find Word or DollarSign
            string functNamePattern = @"(\w+)=function\(\w+\){(\w+)=\2\.split\(\x22{2}\);.*?return\s+\2\.join\(\x22{2}\)}";

            var funcName = Regex.Match(js, functNamePattern).Groups[1].Value;

            if(string.IsNullOrEmpty(funcName))
                throw new NotImplementedException("Couldn't find signature function.");

            if (funcName.Contains("$"))
                funcName = "\\" + funcName; //Due To Dollar Sign Introduction, Need To Escape

            string funcPattern = @"(?!h\.)" + @funcName + @"=function\(\w+\)\{.*?\}"; //Escape funcName string
            var funcBody = Regex.Match(js, funcPattern, RegexOptions.Singleline).Value; //Entire sig function
            var lines = funcBody.Split(';'); //Each line in sig function

            string functionIdentifier = "";
            string operations = "";

            foreach (var line in lines) //Matches the funcBody with each cipher method. Only runs till all three are defined.
            {
                Match m;

                //
                // Get the name of the function called in this statement
                functionIdentifier = Regex.Match(line, @"\w+(?:.|\[)(\""?\w+(?:\"")?)\]?\(").Groups[1].Value;
                if (string.IsNullOrWhiteSpace(functionIdentifier))
                    continue;
                //



                string reReverse = string.Format(@"{0}:\bfunction\b\(\w+\)", functionIdentifier); //Regex for reverse (one parameter)
                string reSlice = string.Format(@"{0}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\.", functionIdentifier); //Regex for slice (return or not)
                string reSwap = string.Format(@"{0}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b", functionIdentifier); //Regex for the char swap.
                
                if ((m=Regex.Match(js, reSlice)).Success)
                {
                    if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success)
                        operations += "s" + m.Groups["index"].Value + " "; //operation is a slice
                }
                else if ((m=Regex.Match(js, reSwap)).Success)
                {
                    if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success)
                        operations += "w" + m.Groups["index"].Value + " "; //operation is a swap (w)
                }
                else if((m = Regex.Match(js, reReverse)).Success)
                {
                    operations += "r "; //operation is a reverse
                }
            }
            
            operations = operations.Trim();

            return DecipherWithOperations(cipher, operations);
        }

        private static string ApplyOperation(string cipher, string op)
        {
            switch (op[0])
            {
                case 'r'://reverse
                    return new string(cipher.ToCharArray().Reverse().ToArray());

                case 'w'://Swap
                    {
                        int index = GetOpIndex(op);
                        return SwapFirstChar(cipher, index);
                    }

                case 's'://Slice
                    {
                        int index = GetOpIndex(op);
                        return cipher.Substring(index);
                    }

                default:
                    throw new NotImplementedException("Couldn't find cipher operation.");
            }
        }

        private static string DecipherWithOperations(string cipher, string operations)
        {
            return operations.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Aggregate(cipher, ApplyOperation);
        }
        
        private static int GetOpIndex(string op)
        {
            string parsed = new Regex(@".(\d+)").Match(op).Result("$1");
            int index = Int32.Parse(parsed);

            return index;
        }

        private static string SwapFirstChar(string cipher, int index)
        {
            var builder = new StringBuilder(cipher);
            builder[0] = cipher[index];
            builder[index] = cipher[0];

            return builder.ToString();
        }
    }
}