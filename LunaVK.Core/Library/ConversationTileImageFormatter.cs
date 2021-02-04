using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace LunaVK.Core.Library
{
    public static class ConversationTileImageFormatter
    {
        public static int DIMENSION = 336;

        public static double LogicalDim
        {
            get
            {
                return (double)ConversationTileImageFormatter.DIMENSION;
            }
        }

        public static void CreateTileImage(List<string> localUris, int userOrChatId, Action<string> callback)
        {
            //Execute.ExecuteOnUIThread((Action)(() =>
            //{
            try
            {
                WriteableBitmap wb = new WriteableBitmap(ConversationTileImageFormatter.DIMENSION, ConversationTileImageFormatter.DIMENSION);
                localUris = Enumerable.ToList(Enumerable.Where<string>(localUris, (u => !string.IsNullOrWhiteSpace(u))));
                List<Rect> map1 = ConversationTileImageFormatter.CreateMap(localUris.Count);
                ConversationTileImageFormatter.ProcessImages(wb, localUris, map1, callback, 0, userOrChatId);
            }
            catch (Exception ex)
            {
                callback("");
                //Logger.Instance.Error(string.Concat("CreateTileImage failed. ", ex));
            }
            //}));
        }

        private static void ProcessImages(WriteableBitmap wb, List<string> localUris, List<Rect> map, Action<string> callback, int ind, int userOrChatId)
        {
            if (ind >= localUris.Count || ind >= map.Count)
            {
                Rectangle rectangle = new Rectangle();
                rectangle.Width = ConversationTileImageFormatter.DIMENSION;
                rectangle.Height = ConversationTileImageFormatter.DIMENSION;
                rectangle.Fill = ((Brush)new SolidColorBrush(Colors.Black));
                rectangle.Opacity = 0.2;
                //wb.Render(rectangle, null);

                wb.Invalidate();
                IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication();
                try
                {
                    string str = string.Concat(new object[] { "/Shared/ShellContent/conversationTileImg", userOrChatId, ".jpg" });
                    if (!storeForApplication.DirectoryExists("/Shared/ShellContent"))
                        storeForApplication.CreateDirectory("/Shared/ShellContent");
                    IsolatedStorageFileStream storageFileStream = new IsolatedStorageFileStream(str, FileMode.Create, FileAccess.Write, storeForApplication);
                    try
                    {
                        int dimension1 = ConversationTileImageFormatter.DIMENSION;
                        int dimension2 = ConversationTileImageFormatter.DIMENSION;
                        //System.Windows.Media.Imaging.Extensions.SaveJpeg(wb, (Stream)storageFileStream, dimension1, dimension2, 0, 80);
                    }
                    finally
                    {
                        if (storageFileStream != null)
                            ((IDisposable)storageFileStream).Dispose();
                    }
                    callback(str);
                }
                finally
                {
                    if (storeForApplication != null)
                        ((IDisposable)storeForApplication).Dispose();
                }
            }
            else
            {
                string localUri = localUris[ind];
                Rect rect = map[ind];
                // ISSUE: explicit reference operation
                double width = rect.Width;
                rect = map[ind];
                // ISSUE: explicit reference operation
                double height = rect.Height;
                Image image1 = ConversationTileImageFormatter.CreateImage(localUri, width, height);
                WriteableBitmap writeableBitmap = wb;
                Image image2 = image1;
                TranslateTransform translateTransform = new TranslateTransform();
                rect = map[ind];
                // ISSUE: explicit reference operation
                double x = rect.X;
                translateTransform.X = x;
                rect = map[ind];
                // ISSUE: explicit reference operation
                double y = rect.Y;
                translateTransform.Y = y;
                //writeableBitmap.Render(image2, translateTransform);
                ConversationTileImageFormatter.ProcessImages(wb, localUris, map, callback, ind + 1, userOrChatId);
            }
        }

        private static Image CreateImage(string uri, double width, double height)
        {
            Image image = new Image();
            BitmapImage bitmapImage = new BitmapImage();
            image.Stretch = Stretch.UniformToFill;
            image.Width = width;
            image.Height = height;
            using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(uri, FileMode.Open, FileAccess.Read))
                {
                    bitmapImage.SetSource(storageFileStream.AsRandomAccessStream());
                }
            }
            image.Source = bitmapImage;
            return image;
        }

        private static List<Rect> CreateMap(int count)
        {
            List<Rect> rectList = new List<Rect>();

            if (count == 1)
                rectList.Add(new Rect(0.0, 0.0, ConversationTileImageFormatter.LogicalDim, ConversationTileImageFormatter.LogicalDim));
            else if (count == 2)
            {
                rectList.Add(new Rect(0.0, 0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim));
                rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 2.0, 0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim));
            }
            else if (count == 3)
            {
                rectList.Add(new Rect(0.0, 0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim));
                rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 2.0, 0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
                rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
            }
            else if (count == 4)
            {
                rectList.Add(new Rect(0.0, 0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
                rectList.Add(new Rect(0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
                rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 2.0, 0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
                rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
            }
            else if (count == 5)
            {
                rectList.Add(new Rect(0.0, 0.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim));
                rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 3.0, 0.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim));
                rectList.Add(new Rect(2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, 0.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0));
                rectList.Add(new Rect(2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0));
                rectList.Add(new Rect(2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, 2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0));
            }
            else if (count == 6)
            {
                rectList.Add(new Rect(0.0, 0.0, 2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, 2.0 * ConversationTileImageFormatter.LogicalDim / 3.0));
                rectList.Add(new Rect(0.0, 2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0));
                rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 3.0, 2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0));
                rectList.Add(new Rect(2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, 0.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0));
                rectList.Add(new Rect(2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0));
                rectList.Add(new Rect(2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, 2.0 * ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0, ConversationTileImageFormatter.LogicalDim / 3.0));
            }
            else if (count >= 7)
            {
                rectList.Add(new Rect(0.0, 0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
                rectList.Add(new Rect(0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
                rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 2.0, 0.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0));
                rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 4.0, ConversationTileImageFormatter.LogicalDim / 4.0));
                rectList.Add(new Rect(ConversationTileImageFormatter.LogicalDim / 2.0, 3.0 * ConversationTileImageFormatter.LogicalDim / 4.0, ConversationTileImageFormatter.LogicalDim / 4.0, ConversationTileImageFormatter.LogicalDim / 4.0));
                rectList.Add(new Rect(3.0 * ConversationTileImageFormatter.LogicalDim / 4.0, ConversationTileImageFormatter.LogicalDim / 2.0, ConversationTileImageFormatter.LogicalDim / 4.0, ConversationTileImageFormatter.LogicalDim / 4.0));
                rectList.Add(new Rect(3.0 * ConversationTileImageFormatter.LogicalDim / 4.0, 3.0 * ConversationTileImageFormatter.LogicalDim / 4.0, ConversationTileImageFormatter.LogicalDim / 4.0, ConversationTileImageFormatter.LogicalDim / 4.0));
            }
            
            return rectList;
        }
    }
}
