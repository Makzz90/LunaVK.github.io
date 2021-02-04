using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestPhotoEditor : Page
    {
        public TestPhotoEditor()
        {
            this.InitializeComponent();

            //this._collageCreationUC.InitWith(this._grid);
            //this._collageCreationUC.Invalidated += this.Update;
            this._collageCreationUC.InitWithCanvas(this._grid, this._canvasControl);
        }

        /*
        CanvasBitmap image;

        private void Canvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }
        
        private async Task CreateResourcesAsync(CanvasControl sender)
        {
            StorageFile inputFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/1-3330-256.png"));

            InMemoryRandomAccessStream inMemoryRandomAccessStream = new InMemoryRandomAccessStream();
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, inMemoryRandomAccessStream);

            image = await CanvasBitmap.LoadAsync(sender, inputFile.Path);
        }

        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            using (var session = args.DrawingSession)
            {
                var blur = new GaussianBlurEffect();
                blur.BlurAmount = 5.0f;
                blur.Source = image;

                session.DrawImage(blur, new Rect(0, 0, sender.ActualWidth, sender.ActualHeight),
                    new Rect(0, 0, image.SizeInPixels.Width, image.SizeInPixels.Height), 1f);

                //session.DrawImage(image, new Rect(0,0, sender.ActualWidth, sender.ActualHeight));

                //session.DrawText(lblName.Text, 150, 150, Colors.Black, new Microsoft.Graphics.Canvas.Text.CanvasTextFormat() { FontSize = 40, FontWeight = Windows.UI.Text.FontWeights.Bold });

                foreach(var shape in this._collageCreationUC.Shapes())
                {
                    if(shape.Control is TextBox tb)
                    {
                        var adorner = shape.ParentContainer as Photo.UC.AdornerElementBaseUC;
                        var angle = adorner.Rotation;

                        var startAngle = angle * Math.PI / 180;
                        var directionTransform = Matrix3x2.CreateRotation((float)startAngle);
                        //session.Transform = directionTransform * Matrix3x2.CreateTranslation(new Vector2(150,150));


                        

                        var temp = tb.TransformToVisual(this._grid).TransformPoint(new Point());

                        session.Transform = Matrix3x2.CreateRotation((float)startAngle, new Vector2((float)temp.X, (float)temp.Y));

                        session.DrawText(tb.Text, (float)temp.X, (float)temp.Y, Colors.Green, new Microsoft.Graphics.Canvas.Text.CanvasTextFormat() { FontSize = (float)tb.FontSize, FontWeight = tb.FontWeight, FontFamily = tb.FontFamily.Source });
                    }
                }
            }
        }

        public void Update()
        {
            this.Canvas.Invalidate();
        }
        */
    }
}
