using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.Common
{
    public sealed class Emoji
    {
        public class EmojiChar
        {
            public int index;

            private static string[] imagePaths;

            private static string[] ImagePaths
            {
                get
                {
                    if (Emoji.EmojiChar.imagePaths == null)
                    {
                        int length = (int)Math.Ceiling(24.03);
                        Emoji.EmojiChar.imagePaths = new string[length];
                        for (int index = 0; index < length; ++index)
                            Emoji.EmojiChar.imagePaths[index] = string.Format("/Emojis/fullset/fullset-{0}.-{1}.png", index.ToString("D2"), 64);
                    }
                    return Emoji.EmojiChar.imagePaths;
                }
            }

            private string GetImagePath()
            {
                int num = this.index - 1;
                if (num < 0 || num >= 2403)
                    return null;
                return Emoji.EmojiChar.ImagePaths[num / 100];
            }

            public IReadOnlyList<Emoji.EmojiChar.Args> Image
            {
                get
                {
                    List<Emoji.EmojiChar.Args> ret = new List<Emoji.EmojiChar.Args>();
                    /*
                    return Observable.Create<Emoji.EmojiChar.Args>((observer =>
                    {
                        int imageIndex = this.GetImageIndex();
                        observer.OnNext(new Emoji.EmojiChar.Args()
                        {
                            EmojiChar = this,
                            BaseImage = ImageStore.GetStockIcon(this.GetImagePath()),
                            X = (imageIndex % 10 * 65 + 1),
                            Y = (imageIndex / 10 * 65 + 1),
                            Width = 64.0,
                            Height = 64.0
                        });
                        return (() => { });
                    }));*/
                    return null;
                }
            }

            public class Args
            {
                public Emoji.EmojiChar EmojiChar;
                public BitmapSource BaseImage;
                public double X;
                public double Y;
                public double Width;
                public double Height;
            }
        }
    }
}
