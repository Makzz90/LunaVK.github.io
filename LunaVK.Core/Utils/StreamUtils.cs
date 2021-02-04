using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LunaVK.Core.Utils
{
    public static class StreamUtils
    {
        public static void CopyStream(Stream input, Stream output, Action<double> progressCallback = null, long inputLength = 0)
        {
            if (inputLength == 0L)
            {
                try
                {
                    inputLength = input.Length;
                }
                catch (Exception)
                {
                }
            }
            byte[] buffer = new byte[b];
            int num = 0;
            int count;
            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (!output.CanWrite)
                    throw new Exception("failed to copy stream");
                //if (c != null && c.IsSet)
                //    throw new Exception("CopyStream cancelled");
                output.Write(buffer, 0, count);
                num += count;
                if (progressCallback != null && inputLength > 0L)
                    progressCallback((double)num * 100.0 / (double)inputLength);
            }
        }

        private static int Read(this byte[] input, byte[] output, int offset, int output_size)
        {
            int num = 0;
            for (int i = offset; i < input.Length && num < output_size; i++)
            {
                output[num] = input[i];
                num++;
            }
            return num;
        }

        static int b = 32768;//original
        //static int b = 512;

        public static void CopyStream(byte[] input, Stream output, Action<double> progressCallback = null)
        {
            int inputLength = input.Length;
            byte[] buffer = new byte[b];
            int num = 0;
            int count;
            int offs = 0;
            while ((count = input.Read(buffer, offs, buffer.Length)) > 0)
            {
                if (!output.CanWrite)
                    throw new Exception("failed to copy stream");
                //if (c != null && c.IsSet)
                //    throw new Exception("CopyStream cancelled");
                output.Write(buffer, 0, count);
                num += count;
                offs += count;
                if (progressCallback != null && inputLength > 0)
                    progressCallback((double)num * 100.0 / (double)inputLength);
            }
            /*
            for (int i = 0; i < input.Length;i++ )
            {
                output.WriteByte(input[i]);
                if (progressCallback != null && input.Length > 0)
                    progressCallback((double)i * 100.0 / (double)input.Length);
            }*/
        }

        public static MemoryStream ReadFully(Stream input)
        {
            byte[] buffer = new byte[16384];
            MemoryStream memoryStream = new MemoryStream();
            int count;
            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                memoryStream.Write(buffer, 0, count);
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
