using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Windows.Foundation;
using Windows.UI;

namespace LunaVK.Photo.DrawingObjects
{
    public class WallPaperUI : IDrawingUI
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Rotate { get; set; }
        public CanvasBitmap Image;
        public double Width { get; set; }
        public double Height { get; set; }
        public bool Editing { get; set; }
        /*
        public Rect Region
        {
            get
            {
                return new Rect(X - Width / 2, Y - Height / 2, Width, Height);
            }
        }

        /// <summary>
        /// Close
        /// </summary>
        public Rect CancelRegion
        {
            get { return new Rect(X + Width / 2 + 2 - 8, Y - Height / 2 - 2 - 8, 16, 16); }
        }

        /// <summary>
        /// Scale
        /// </summary>
        public Rect ScaleRegion
        {
            get { return new Rect(X + Width / 2 + 2 - 8, Y + Height / 2 + 2 - 8, 16, 16); }
        }
        */

        //https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.symbol?view=winrt-19041

        
        public void Draw(CanvasDrawingSession graphics, float scale)
        {
            if (this.Image != null)
            {
               //graphics.Transform = Matrix3x2.CreateRotation((float)(this._rotate * Math.PI / 180));
                graphics.DrawImage(this.Image, new Rect(X * scale, Y * scale, Width * scale, Height * scale));
            }
            else
            {
                //(X - (Width / 2)) * scale, (Y - (Height / 2)) * scale
                graphics.DrawText("loading...", (float)(X + 10) * scale, (float)(Y + (Height / 2)) * scale, Colors.Orange, new Microsoft.Graphics.Canvas.Text.CanvasTextFormat() { FontSize = 11 * scale });
            }

            if (this.Editing)
            {
                var stickness = 2;

                graphics.DrawRectangle(new Rect(X* scale, Y* scale, Width * scale, Height * scale), Colors.Orange, stickness, new CanvasStrokeStyle() { DashStyle = CanvasDashStyle.Dash });

                /*
                //Close
                graphics.FillCircle((float)((X + Width / 2) * scale) , (float)((Y - Height / 2) * scale) , 12, Colors.Orange);
                //graphics.DrawLine((float)((X + Width / 2 ) * scale) - 4, (float)((Y - Height / 2 ) * scale) - 4, (float)((X + Width / 2 ) * scale) + 4, (float)((Y - Height / 2 ) * scale) + 4, Colors.White);
                //graphics.DrawLine((float)((X + Width / 2 ) * scale) - 4, (float)((Y - Height / 2 ) * scale) + 4, (float)((X + Width / 2 )* scale) + 4, (float)((Y - Height / 2 )* scale) - 4, Colors.White);
                graphics.DrawText("\xE10A", (float)((X + Width / 2) * scale) - 9, (float)((Y - Height / 2) * scale) - 9, Colors.Blue, new Microsoft.Graphics.Canvas.Text.CanvasTextFormat() { FontSize = 16, FontFamily = "Segoe UI Symbol" });

                //Scale
                graphics.FillCircle((float)((X + Width / 2 )* scale) , (float)((Y + Height / 2 ) * scale) , 8, Colors.Orange);
                graphics.DrawLine((float)((X + Width / 2 ) * scale)  - 4, (float)((Y + Height / 2 ) * scale) - 4, (float)(X + Width / 2) * scale  + 4, (float)(Y + Height / 2) * scale  + 4, Colors.White);
                graphics.DrawLine((float)((X + Width / 2 ) * scale)  - 4, (float)((Y + Height / 2 ) * scale)  - 4, (float)(X + Width / 2) * scale  - 4, (float)(Y + Height / 2) * scale , Colors.White);
                graphics.DrawLine((float)((X + Width / 2 ) * scale)  - 4, (float)((Y + Height / 2 ) * scale)  - 4, (float)(X + Width / 2) * scale , (float)(Y + Height / 2) * scale  - 4, Colors.White);
                graphics.DrawLine((float)((X + Width / 2 ) * scale)  + 4, (float)((Y + Height / 2 ) * scale)  + 4, (float)(X + Width / 2) * scale , (float)(Y + Height / 2) * scale  + 4, Colors.Red);
                graphics.DrawLine((float)((X + Width / 2 ) * scale) + 2 + 4, (float)((Y + Height / 2 ) * scale) + 2 + 4, (float)(X + Width / 2) * scale + 2 + 4, (float)(Y + Height / 2) * scale + 2, Colors.White);
                */

                graphics.FillCircle((float)X * scale, (float)Y * scale, 12, Colors.Orange);  //×
                graphics.DrawText("\xE106", (float)(X * scale) - 8, (float)(Y * scale) - 8, Colors.White, new Microsoft.Graphics.Canvas.Text.CanvasTextFormat() { FontSize = 14, FontFamily = "Segoe UI Symbol" });

                graphics.FillCircle((float)(X + Width) * scale, (float)(Y + Height) * scale, 12, Colors.Orange); //Zoom
                graphics.DrawText("\xE1CA", (float)(X + Width) * scale - 9, (float)((Y + Height) * scale) - 9, Colors.White, new Microsoft.Graphics.Canvas.Text.CanvasTextFormat() { FontSize = 14, FontFamily = "Segoe UI Symbol" });
            }
        }
    }
}
