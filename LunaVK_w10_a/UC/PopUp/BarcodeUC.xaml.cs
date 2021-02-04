using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ZXing;

namespace LunaVK.UC.PopUp
{
    public sealed partial class BarcodeUC : UserControl
    {
        private string _data;
        public BarcodeUC(string data)
        {
            this._data = data;
            this.InitializeComponent();

            this.Loaded += BarcodeUC_Loaded;
        }

        private void BarcodeUC_Loaded(object sender, RoutedEventArgs e)
        {
            IBarcodeWriter writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions { Height = 400, Width = 400, Margin = 1 },
                //Renderer = new ZXing.Rendering.PixelDataRenderer() { Foreground = Windows.UI.Colors.Black }//Adding color QR code
            };

            var result = writer.Write(this._data);

            //add to image component
            imgQRCode.Source = result;
        }
    }
}
