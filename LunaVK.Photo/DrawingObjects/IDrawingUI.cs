using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.Foundation;

namespace LunaVK.Photo.DrawingObjects
{
    interface IDrawingUI
    {
        void Draw(CanvasDrawingSession graphics, float scale);

        double X { get; set; }

        double Y { get; set; }

        double Width { get; set; }

        double Height { get; set; }

        bool Editing { get; set; }

        //Rect Region { get; }

        //Rect CancelRegion { get; }

        //Rect ScaleRegion { get; }
    }
}
