using System.Collections.Generic;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using Windows.Foundation;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Brushes;

namespace LunaVK.Photo.DrawingObjects
{
    public class DoodleUI : IDrawingUI
    {
        CanvasBitmap _brush_image;
        public double X { get; set; }
        public double Y { get; set; }
        public Color DrawingColor { get; set; }
        public int DrawingSize { get; set; }
        private List<Point> _points;
        public List<Point> Points
        {
            get
            {
                if (_points == null)
                {
                    _points = new List<Point>();
                }
                return _points;
            }
        }

        public void InitImageBrush(CanvasBitmap cb)
        {
            if (DrawingColor == Colors.Transparent)
            {
                if (_brush_image == null)
                    _brush_image = cb;
            }
        }

        public void Draw(CanvasDrawingSession graphics, float scale)
        {
            if (_points != null && _points.Count>0)
            {
                ICanvasBrush brush;
                if (DrawingColor == Colors.Transparent)
                {
                    if (_brush_image == null)
                        return;
                    brush = new CanvasImageBrush(graphics, _brush_image);
                }
                else
                {
                    brush = new CanvasSolidColorBrush(graphics, DrawingColor);
                }
                if (_points.Count == 1)
                {
                    graphics.FillEllipse((float)_points[0].X * scale, (float)_points[0].Y * scale, DrawingSize * scale / 2, DrawingSize * scale / 2, brush);
                }
                else
                {
                    var style = new CanvasStrokeStyle();
                    style.DashCap = CanvasCapStyle.Round;
                    style.StartCap = CanvasCapStyle.Round;
                    style.EndCap = CanvasCapStyle.Round;
                    for (int i = 0; i < _points.Count - 1; ++i)
                    {
                        graphics.DrawLine((float)_points[i].X * scale, (float)_points[i].Y * scale, (float)_points[i + 1].X * scale, (float)_points[i + 1].Y * scale, brush, DrawingSize * scale, style);
                    }
                }
            }
        }

        public bool Editing { get; set; }
        public Rect Region { get; }
        public Rect CancelRegion { get; }
        public Rect ScaleRegion { get; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
