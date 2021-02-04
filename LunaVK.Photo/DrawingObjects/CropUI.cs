using Microsoft.Graphics.Canvas;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas.Geometry;

namespace LunaVK.Photo.DrawingObjects
{
    public class CropUI : IDrawingUI
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Color DrawColor { get; set; }
        /*
        public Rect Region
        {
            get { return new Rect(X, Y, Width, Height); }
        }

        /// <summary>
        /// Click the cancel area in the upper left corner
        /// </summary>
        public Rect CancelRegion
        {
            get { return new Rect(X - 8, Y - 8, 16, 16); }
        }

        /// <summary>
        /// Click on the upper right corner to confirm the area
        /// </summary>
        public Rect RightTopRegion
        {
            get { return new Rect(X + Width - 8, Y - 8, 16, 16); }
        }

        /// <summary>
        /// Enlarged area at the bottom right corner
        /// </summary>
        public Rect ScaleRegion
        {
            get { return new Rect(X + Width - 8, Y + Height - 8, 16, 16); }
        }
        */
        public void Draw(CanvasDrawingSession graphics, float scale)
        {
            var radius = 12;
            CanvasStrokeStyle style = new CanvasStrokeStyle();
            //style.DashCap = CanvasCapStyle.Round;
            style.DashStyle = CanvasDashStyle.Dash;
            //style.StartCap = CanvasCapStyle.Round;
            //style.EndCap = CanvasCapStyle.Round;

            graphics.FillRectangle(new Rect(0,0, X*scale, 9999), Color.FromArgb(100, 0, 0, 0));
            graphics.FillRectangle(new Rect((X+Width) * scale, 0, 9999, 9999), Color.FromArgb(100, 0, 0, 0));
            graphics.FillRectangle(new Rect(X * scale, 0, Width * scale, Y*scale), Color.FromArgb(100, 0, 0, 0));
            graphics.FillRectangle(new Rect(X * scale, (Y + Height) * scale, Width * scale, 9999), Color.FromArgb(100, 0, 0, 0));

            //graphics.FillRectangle(new Rect(X * scale, Y * scale, Width * scale, Height * scale), Color.FromArgb(100, 0XFF, 0XFF, 0XFF));
            graphics.DrawRectangle(new Rect(X * scale, Y * scale, Width * scale, Height * scale), DrawColor, 2, style); //rectangle
            
            if (Width > 50 && Height > 50)  //When the conditions are met, draw the nine-square grid
            {
                graphics.DrawLine((float)X * scale, (float)(Y + (Height / 3)) * scale, (float)(X + Width) * scale, (float)(Y + Height / 3) * scale, Colors.Orange, 0.3f, style);
                graphics.DrawLine((float)X * scale, (float)(Y + (Height * 2 / 3)) * scale, (float)(X + Width) * scale, (float)(Y + Height * 2 / 3) * scale, Colors.Orange, 0.3f, style);
                graphics.DrawLine((float)(X + Width / 3) * scale, (float)Y, (float)(X + Width / 3) * scale, (float)(Y + Height) * scale, Colors.Orange, 0.3f, style);
                graphics.DrawLine((float)(X + Width * 2 / 3) * scale, (float)Y * scale, (float)(X + Width * 2 / 3) * scale, (float)(Y + Height) * scale, Colors.Orange, 0.3f, style);
            }
            
            graphics.FillCircle((float)X * scale, (float)Y * scale, radius, DrawColor);  //×
            graphics.DrawText("\xE106", (float)(X * scale) - 8, (float)(Y * scale) - 8, Colors.White, new Microsoft.Graphics.Canvas.Text.CanvasTextFormat() { FontSize = 14, FontFamily = "Segoe UI Symbol" });

            graphics.FillCircle((float)(X + Width) * scale, (float)(Y + Height) * scale, radius, DrawColor); //Zoom
            graphics.DrawText("\xE1CA", (float)(X + Width) * scale - 9, (float)((Y + Height) * scale) - 9, Colors.White, new Microsoft.Graphics.Canvas.Text.CanvasTextFormat() { FontSize = 14, FontFamily = "Segoe UI Symbol" });

            //graphics.FillCircle((float)(X + Width) * scale, (float)Y * scale, radius, DrawColor); //√
            //graphics.DrawText("\xE10B", (float)((X + Width) * scale) - 9, (float)(Y * scale) - 9, Colors.White, new Microsoft.Graphics.Canvas.Text.CanvasTextFormat() { FontSize = 14, FontFamily = "Segoe UI Symbol" });

            
        }

        public bool Editing { get; set; }
    }
}
