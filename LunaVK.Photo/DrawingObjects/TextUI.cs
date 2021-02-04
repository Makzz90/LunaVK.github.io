using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Windows.Foundation;
using Windows.UI;

namespace LunaVK.Photo.DrawingObjects
{
    public class TextUI : IDrawingUI
    {
        public double X { get; set; }
        public double Y { get; set; }

        public string TagText;
        public string Font = "Arial";
        public uint FontSize = 20;



        public Color DrawColor = Colors.Orange;
        /*
        public Rect Region { get; private set; }

        /// <summary>
        /// Close
        /// </summary>
        public Rect CancelRegion
        {
            get { return new Rect(X + Width - 4, Y + Height / 2 - 4, 16, 16); }
        }
        
        /// <summary>
        /// Scale
        /// </summary>
        public Rect ScaleRegion
        {
            get { return new Rect(X + Width / 2 + 2 - 8, Y + Height / 2 + 2 - 8, 16, 16); }
        }
        */
        public void Draw(CanvasDrawingSession graphics, float scale)
        {
            var radius = 12;

            var x = X * scale;
            var y = Y * scale;

            float fontSize = this.CalculateFontSize(graphics, scale);

            var ctFormat = new CanvasTextFormat { FontSize = fontSize, WordWrapping = CanvasWordWrapping.NoWrap, FontFamily = Font };
            var ctLayout = new CanvasTextLayout(graphics, TagText, ctFormat, 0.0f, 0.0f);

            var width = ctLayout.DrawBounds.Width;
            var height = ctLayout.DrawBounds.Height;

            //this.Width = width;
            this.Height = height;

            graphics.DrawText(TagText, (float)x, (float)y, this.DrawColor, ctFormat);

            if (this.Editing)
            {
                Rect region = new Rect(x - 4, y, width + 4, height + 12);

                CanvasStrokeStyle style = new CanvasStrokeStyle();
                style.DashCap = CanvasCapStyle.Square;
                style.DashStyle = CanvasDashStyle.Dash;
                style.StartCap = CanvasCapStyle.Round;
                style.EndCap = CanvasCapStyle.Round;
                graphics.DrawRectangle(region, Colors.Orange, 2, style);

                graphics.FillCircle((float)X * scale, (float)Y * scale, radius, Colors.Orange);  //×
                graphics.DrawText("\xE106", (float)(X * scale) - 8, (float)(Y * scale) - 8, Colors.White, new CanvasTextFormat() { FontSize = 14, FontFamily = "Segoe UI Symbol" });

                graphics.FillCircle((float)(X + Width) * scale, (float)(Y + Height) * scale, radius, Colors.Orange); //Zoom
                graphics.DrawText("\xE1CA", (float)(X + Width) * scale - 9, (float)((Y + Height) * scale) - 9, Colors.White, new CanvasTextFormat() { FontSize = 14, FontFamily = "Segoe UI Symbol" });
            }
        }

        private float CalculateFontSize(CanvasDrawingSession graphics, float scale)
        {
            if (this._lastWidth == (this.Width * scale) && this._lastScale == scale)
                return this._lastFontSize;

            float ret = 12;
            for (float i = 5f; i < 100; i += 0.5f)
            {
                var ctFormat = new CanvasTextFormat { FontSize = i * scale, WordWrapping = CanvasWordWrapping.NoWrap, FontFamily = Font };
                var ctLayout = new CanvasTextLayout(graphics, TagText, ctFormat, 0.0f, 0.0f);

                var width = ctLayout.DrawBounds.Width;
                //var height = ctLayout.DrawBounds.Height;

                if ((width > (this.Width * scale) && this.Width > 0) /*|| (height > this.Height && this.Height > 0)*/)
                {
                    this._lastWidth = this.Width * scale;
                    this._lastFontSize = ret;
                    this._lastScale = scale;
                    break;
                }

                ret = i * scale;
            }
            //System.Diagnostics.Debug.WriteLine(ret+ " _lastWidth: " + _lastWidth+ " scale:" + scale);
            return ret;
        }

        private double _lastWidth;
        private float _lastFontSize;
        private float _lastScale;


        public bool Editing { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
