using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.Common
{
    public class GraffitiEncoder
    {
        private const int RENDER_IMAGE_DPI = 72;
        private readonly RenderTargetBitmap _bitmap;

        public GraffitiEncoder(RenderTargetBitmap bitmap)
        {
            this._bitmap = bitmap;
        }

        public async Task<Stream> Encode()
        {
            try
            {
                //Stopwatch stopwatchEncode = new Stopwatch();
                //stopwatchEncode.Start();
                IRandomAccessStream imageStream = new InMemoryRandomAccessStream();
                BitmapEncoder bitmapEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, imageStream); // await GraffitiEncoder.BuildEncoder(imageStream);

                //byte[] pixels = await GraffitiEncoder.GetImageBinaryData1(this._bitmap);
                IBuffer buf = await this._bitmap.GetPixelsAsync();
                byte[] pixels = buf.ToArray();

                double dpi = GraffitiEncoder.RENDER_IMAGE_DPI;
                
                bitmapEncoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)this._bitmap.PixelWidth, (uint)this._bitmap.PixelHeight, dpi, dpi, pixels);
                await bitmapEncoder.FlushAsync(); //await WindowsRuntimeSystemExtensions.AsTask(bitmapEncoder.FlushAsync()).ConfigureAwait(false);
                //long size = (long)imageStream.Size;
                //stopwatchEncode.Stop();
                //Execute.ExecuteOnUIThread((Action)(() => { }));
                return WindowsRuntimeStreamExtensions.AsStreamForRead(imageStream);
            }
            catch
            {
                return null;
            }
        }
        /*
        private static async Task<BitmapEncoder> BuildEncoder(IRandomAccessStream outputStream)
        {
            //return await (WindowsRuntimeSystemExtensions.AsTask(BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, outputStream))).ConfigureAwait(false);
            return await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, outputStream);
        }
        
        private static async Task<byte[]> GetImageBinaryData1(RenderTargetBitmap bitmap)
        {
            //pixelFormat = BitmapPixelFormat.Bgra8;
            //return bitmap.ToByteArray();
            IBuffer pixels = await bitmap.GetPixelsAsync();
            return pixels.ToArray();
        }
        
        private static byte[] GetImageBinaryData2(WriteableBitmap bitmap, out BitmapPixelFormat pixelFormat)
        {
            pixelFormat = BitmapPixelFormat.Rgba8;
            int pixelWidth = ((BitmapSource)bitmap).PixelWidth;
            int pixelHeight = ((BitmapSource)bitmap).PixelHeight;
            int[] pixels = bitmap.Pixels;
            byte[] numArray = new byte[4 * pixelWidth * pixelHeight];
            int index1 = 0;
            int index2 = 0;
            while (index1 < pixels.Length)
            {
                int num = pixels[index1];
                numArray[index2] = (byte)(num >> 16);
                numArray[index2 + 1] = (byte)(num >> 8);
                numArray[index2 + 2] = (byte)num;
                numArray[index2 + 3] = (byte)(num >> 24);
                ++index1;
                index2 += 4;
            }
            return numArray;
        }
        */
    }
}
