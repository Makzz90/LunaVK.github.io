using ExifLib;
using LunaVK.Core;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.Framework
{
    public static class ImagePreprocessor
    {
        public static Rect GetThumbnailRect(double actualWidth, double actualHeight, Rect relativeThumbRect)
        {
            Size childSize = new Size(actualWidth, actualHeight);
            return RectangleUtils.CalculateRelative(RectangleUtils.GetSize(RectangleUtils.ResizeToFitIfNotContained(new Size(VKConstants.MAX_PHOTO_WIDTH, VKConstants.MAX_PHOTO_HEIGHT), childSize)), relativeThumbRect);
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16384];
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int count;
                while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                    memoryStream.Write(buffer, 0, count);
                return memoryStream.ToArray();
            }
        }

        public static void PreprocessImage(Stream stream, int desiredSizeInPx, bool closeStream, Action<ImagePreprocessResult> completionCallback)
        {
            Stopwatch.StartNew();
            try
            {
                MemoryStream exifStream;
                ImagePreprocessor.PatchAwayExif(stream, out exifStream);
                long position = stream.Position;
                bool keepExif = Settings.SaveLocationDataOnUpload;
                stream.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
//                bitmapImage.SetSource(stream);
                WriteableBitmap wb = new WriteableBitmap(200,200/*bitmapImage*/);
                
                ExifReader reader = null;
                stream.Position = 0L;
                reader = new ExifReader(stream);
                bool flag = false;
                switch (reader.info.Orientation)
                {
                    case ExifOrientation.BottomRight:
//                        wb = wb.Rotate(180);
                        flag = true;
                        break;
                    case ExifOrientation.TopRight:
//                        wb = wb.Rotate(90);
                        flag = true;
                        break;
                    case ExifOrientation.BottomLeft:
//                        wb = wb.Rotate(270);
                        flag = true;
                        break;
                }
                int pixelWidth = ((BitmapSource)wb).PixelWidth;
                int pixelHeight = ((BitmapSource)wb).PixelHeight;
                int num1 = pixelWidth * pixelHeight;
                if (desiredSizeInPx > num1)
                    desiredSizeInPx = num1;
                if (num1 > desiredSizeInPx | flag)
                {
                    MemoryStream ms = new MemoryStream();
                    double num2 = Math.Sqrt(num1 / desiredSizeInPx);
                    int resizedWidth = (int)Math.Round(pixelWidth / num2);
                    int resizedHeight = (int)Math.Round(pixelHeight / num2);
                    MemoryStream resultStream;
                    IAsyncAction asyncAction = ThreadPool.RunAsync((o =>
                    {
                        try
                        {
//                            System.Windows.Media.Imaging.Extensions.SaveJpeg(wb, ms, resizedWidth, resizedHeight, 0, VKConstants.JPEGQUALITY);
                            ms.Position = 0L;
                            if (keepExif)
                            {
                                exifStream = new MemoryStream(ImagePreprocessor.ResetOrientation(reader.info.OrientationOffset, exifStream.ToArray(), reader.info.LittleEndian));
                                resultStream = ImagePreprocessor.MergeExif(ms, exifStream);
                                Logger.Instance.Info("RESIZED JPEG SIZE: " + resultStream.Length);
                                ms.Dispose();
                            }
                            else
                                resultStream = ms;
                            if (closeStream)
                                stream.Dispose();
                            exifStream.Dispose();
                            resultStream.Position = 0L;
                            GC.Collect();
                            completionCallback(new ImagePreprocessResult((Stream)resultStream, resizedWidth, resizedHeight));
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Error("Failed to resize image", ex);
                        }
                    }));
                }
                else
                {
                    MemoryStream resultStream = new MemoryStream();
                    if (keepExif)
                    {
                        stream.Position = 0L;
                        stream.CopyTo((Stream)resultStream);
                        if (closeStream)
                            stream.Dispose();
                    }
                    else
                    {
                        stream.Position = 0L;
                        resultStream.WriteByte((byte)stream.ReadByte());
                        resultStream.WriteByte((byte)stream.ReadByte());
                        stream.Position = position;
                        stream.CopyTo((Stream)resultStream);
                    }
                    if (closeStream)
                        stream.Dispose();
                    exifStream.Dispose();
                    resultStream.Position = 0L;
                    GC.Collect();
                    completionCallback(new ImagePreprocessResult((Stream)resultStream, pixelWidth, pixelHeight));
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Failed to resize image", ex);
            }
        }

        public static byte[] ResetOrientation(long p, byte[] exifData, bool littleEndian)
        {
            byte[] array = new List<byte>(exifData).ToArray();
            long num = p - 2L;
            if (num >= 0L && num < (long)(array.Length - 1))
                ExifIO.WriteUShort(array, (int)num, littleEndian, (ushort)1);
            return array;
        }

        public static MemoryStream MergeExif(Stream ms, MemoryStream exifStream)
        {
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.WriteByte((byte)ms.ReadByte());
            memoryStream.WriteByte((byte)ms.ReadByte());
            exifStream.WriteTo(memoryStream);
            ms.CopyTo(memoryStream);
            memoryStream.Position = 0L;
            return memoryStream;
        }

        public static void PatchAwayExif(Stream inStream, out MemoryStream exifStream)
        {
            exifStream = new MemoryStream();
            byte[] numArray = new byte[2] { (byte)inStream.ReadByte(), (byte)inStream.ReadByte() };
            if ((int)numArray[0] != (int)byte.MaxValue || (int)numArray[1] != 216)
                return;
            ImagePreprocessor.SkipAppHeaderSection(inStream, out exifStream);
        }

        private static byte[] SkipAppHeaderSection(Stream inStream, out MemoryStream exifStream)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            byte[] numArray = new byte[2] { (byte)inStream.ReadByte(), (byte)inStream.ReadByte() };
            exifStream = new MemoryStream();
            while ((int)numArray[0] == (int)byte.MaxValue && (int)numArray[1] >= 224 && (int)numArray[1] <= 239)
            {
                exifStream.WriteByte(numArray[0]);
                exifStream.WriteByte(numArray[1]);
                byte num1 = (byte)inStream.ReadByte();
                byte num2 = (byte)inStream.ReadByte();
                exifStream.WriteByte(num1);
                exifStream.WriteByte(num2);
                byte[] buffer = new byte[((int)num1 << 8 | (int)num2) - 2];
                inStream.Read(buffer, 0, buffer.Length);
                exifStream.Write(buffer, 0, buffer.Length);
                numArray[0] = (byte)inStream.ReadByte();
                numArray[1] = (byte)inStream.ReadByte();
            }
            inStream.Position -= 2L;
            exifStream.Position = 0L;
            stopwatch.Stop();
            return numArray;
        }














        public class ImagePreprocessResult
        {
            public Stream Stream { get; set; }

            public int Width { get; set; }

            public int Height { get; set; }

            public ImagePreprocessResult(Stream stream, int width, int height)
            {
                this.Stream = stream;
                this.Width = width;
                this.Height = height;
            }
        }
    }
}
