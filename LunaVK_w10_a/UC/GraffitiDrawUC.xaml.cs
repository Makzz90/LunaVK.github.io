using System;
using System.Collections.Generic;
//using System.IO;
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
using Windows.UI.Xaml.Navigation;

using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media.Animation;
using System.Runtime.Serialization;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using LunaVK.Core.ViewModels;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Threading.Tasks;


using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Graphics.Imaging;
using App1uwp.ViewModels;

namespace App1uwp.UC
{
    public sealed partial class GraffitiDrawUC : UserControl
    {
        private readonly GraffitiDrawService _graffitiDrawService = new GraffitiDrawService();
        public event EventHandler<Color> ColorSelected;

        public GraffitiDrawUC()
        {
            this.InitializeComponent();
            this.ColorSelected += GraffitiDrawUC_ColorSelected;
            this.ThicknessSelected += GraffitiDrawUC_ThicknessSelected;

            this.GraffitiDrawUC_ColorSelected(null, this.VM.CurrentColor);
            this._graffitiDrawService.StrokeThickness = this.VM.CurrentThickness;

            //base.SizeChanged += (delegate(object sender, SizeChangedEventArgs args)
            //{
            //    this.drawCanvas.Height = (args.NewSize.Height - this.panelControls.Height/* - FramePageUtils.SoftNavButtonsCurrentSize*/);
            //    this.drawCanvas.Width = (args.NewSize.Width);
            //});

            this.drawCanvas.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
        }

        void GraffitiDrawUC_ThicknessSelected(object sender, int thickness)
        {
            this._graffitiDrawService.StrokeThickness = thickness;
            this.ShowHideThicknessPopup(false);
        }

        void GraffitiDrawUC_ColorSelected(object sender, Color color)
        {
            this._graffitiDrawService.StrokeBrush = new SolidColorBrush(color);
            //this.ucBrushThickness.SetFillColor(color);
            foreach(var thick in this.VM.ThicknessItems)
            {
                thick.FillBrush = new SolidColorBrush(color);
            }
        }

        private void borderThickness_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.ShowHideThicknessPopup(this.ucBrushThickness.Visibility != 0);
        }

        private void ShowHideThicknessPopup(bool show)
        {
//            if (this._isThicknessPopupAnimating)
//                return;
//            this._isThicknessPopupAnimating = true;
            TranslateTransform translateTransform = (TranslateTransform)this.ucBrushThickness.RenderTransform;
            CubicEase cubicEase = new CubicEase();
            int num1 = !show ? 1 : 0;
            cubicEase.EasingMode = (EasingMode)num1;
            EasingFunctionBase easingFunction1 = cubicEase;
            int num2 = show ? -8 : 0;
            int num3 = show ? 1 : 0;
            if (show)
            {
                this.ucBrushThickness.Visibility = Visibility.Visible;
                this.borderThicknessPopupOverlay.Visibility = Visibility.Visible;
//                Touch.FrameReported -= new TouchFrameEventHandler(this.Touch_OnFrameReported);
            }
            else
            {
                this.ucBrushThickness.IsHitTestVisible = false;
                this.borderThicknessPopupOverlay.Visibility = Visibility.Collapsed;
//                Touch.FrameReported += new TouchFrameEventHandler(this.Touch_OnFrameReported);
            }
            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
            AnimationUtils.AnimationInfo animationInfo = new AnimationUtils.AnimationInfo();
            animationInfo.target = (DependencyObject)translateTransform;
            animationInfo.propertyPath = "Y";
            double y = translateTransform.Y;
            animationInfo.from = y;
            double num4 = (double)num2;
            animationInfo.to = num4;
            int num5 = 200;
            animationInfo.duration = num5;
            EasingFunctionBase easingFunction2 = easingFunction1;
            animationInfo.easing = easingFunction2;
            animInfoList.Add(animationInfo);
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = (DependencyObject)this.ucBrushThickness,
                propertyPath = "Opacity",
                from = this.ucBrushThickness.Opacity,
                to = (double)num3,
                duration = 200,
                easing = easingFunction1
            });
            int? startTime = new int?();
            Action completed = (Action)(() =>
            {
//                this._isThicknessPopupAnimating = false;
                if (show)
                    return;
                this.ucBrushThickness.IsHitTestVisible = true;
                this.ucBrushThickness.Visibility = Visibility.Collapsed;
            });
            AnimationUtils.AnimateSeveral(animInfoList, startTime, completed);
        }

        private void borderThicknessPopupOverlay_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            this.ShowHideThicknessPopup(false);
        }

        private bool _isDrawing;

        private void HandleTouchPoint(Point touchPointPosition, bool isLastPoint = false)
        {
            Path path = this._graffitiDrawService.HandleTouchPoint(touchPointPosition, isLastPoint);
            if (path == null)
                return;
            this.drawCanvas.Children.Add(path);
        }

        private void drawCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (this._isDrawing)
            {
                Canvas canvs = sender as Canvas;
                var ttv = canvs.TransformToVisual(Window.Current.Content);
                Point screenCoords = ttv.TransformPoint(new Point(0, 0));
                var point = e.GetCurrentPoint(null);
                Point position = point.Position;
                position.Y -= screenCoords.Y;
                position.X -= screenCoords.X;
                this.HandleTouchPoint(position, false);
            }
        }

        

        private void drawCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this._isDrawing = true;
            e.Handled = true;
        }

        private void drawCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Canvas canvs = sender as Canvas;
            var ttv = canvs.TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            var point = e.GetCurrentPoint(null);
            Point position = point.Position;
            position.Y -= screenCoords.Y;
            position.X -= screenCoords.X;
            this.HandleTouchPoint(position, true);
            this._isDrawing = false;
            this.UpdateUndoOpacity();
        }

        private void drawCanvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;

            if (this._isDrawing && !e.IsInertial && e.OriginalSource is Canvas)
            {
                //Canvas canvs = sender as Canvas;
                //var ttv = canvs.TransformToVisual(Window.Current.Content);
                //Point screenCoords = ttv.TransformPoint(new Point(0, 0));
                //var point = e.GetCurrentPoint(null);
                //Point position = point.Position;
                //position.Y -= screenCoords.Y;
                //position.X -= screenCoords.X;
                Point position = e.Position;
                this.HandleTouchPoint(position, false);
            }
        }

        private void drawCanvas_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            this._isDrawing = true;
            e.Handled = true;
        }

        private void drawCanvas_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            this.drawCanvas.CancelDirectManipulations();
            Point position = e.Position;
            e.Handled = true;
            this.HandleTouchPoint(position, true);
            this._isDrawing = false;
            this.UpdateUndoOpacity();
        }

        public class GraffitiDrawService
        {
            private readonly List<Point> _currentPoints = new List<Point>();
            private readonly List<Curve> _curves = new List<Curve>();
            private readonly Curve _currentCurve = new Curve();
            private readonly List<PathFigure> _currentCurvePathFigures = new List<PathFigure>();
            private readonly CurveData _currentCurveData = new CurveData();
            private const int MAX_POINTS_COUNT = 10;
            private const int MIN_POINTS_COUNT = 5;
            private bool _canAddPoints;

            public GraffitiCacheData _graffitiCacheData = new GraffitiCacheData();

            public List<CurveData> CurvesData = new List<CurveData>();

            public int StrokeThickness { get; set; }

            public SolidColorBrush StrokeBrush { get; set; }

            public bool CanUndo
            {
                get
                {
                    if (this._curves.Count <= 0)
                        return this._currentPoints.Count > 0;
                    return true;
                }
            }

            public Path HandleTouchPoint(Point point, bool isLastPoint = false)
            {
                if (this._currentPoints.Count > MAX_POINTS_COUNT)
                {
                    this._canAddPoints = false;
                    while (this._currentPoints.Count > MIN_POINTS_COUNT)
                        this._currentPoints.RemoveAt(0);
                }
                this._currentPoints.Add(point);
                this._currentCurveData.Points.Add(point);
                Path path = null;
                if (!this._canAddPoints)
                {
                    PathFigure pathFigure;
                    this.InitNewPath(out path, out pathFigure);
                    this._currentCurveData.StrokeBrush = (Brush)this.StrokeBrush;
                    this._currentCurveData.StrokeThickness = this.StrokeThickness;
                    this._graffitiCacheData.SetCurveStrokeThickness(this.StrokeThickness);
                    this._graffitiCacheData.SetCurveColorHex(this.StrokeBrush.Color.ToString());
                    this._currentCurve.Add(path);
                    this._currentCurvePathFigures.Add(pathFigure);
                    this._canAddPoints = true;
                }
                this._graffitiCacheData.AddPoint(point, isLastPoint);
                Enumerable.Last<PathFigure>(this._currentCurvePathFigures).Segments = GraffitiDrawService.GetSegments(this._currentPoints);
                if (isLastPoint)
                    this.Checkpoint();
                return path;
            }

            private void InitNewPath(out Path path, out PathFigure pathFigure)
            {
                pathFigure = PathDataBuilder.CreatePathFigure(this._currentPoints[0]);
                path = PathDataBuilder.CreatePath(pathFigure, (double)this.StrokeThickness, (Brush)this.StrokeBrush);
            }

            public static PathSegmentCollection GetSegments(IReadOnlyList<Point> controlPoints)
            {
                if (controlPoints.Count == 0)
                    return null;
                PathSegmentCollection segmentCollection = new PathSegmentCollection();
                Point point1 = controlPoints[0];
                //point1 = controlPoints[0];
                if (controlPoints.Count <= 3)
                {
                    LineSegment lineSegment1 = new LineSegment();
                    Point point2 = new Point(point1.X + 1.0, point1.Y);
                    lineSegment1.Point = point2;
                    LineSegment lineSegment2 = lineSegment1;
                    segmentCollection.Add(lineSegment2);
                }
                else
                {
                    for (int index = 1; index < ((IReadOnlyCollection<Point>)controlPoints).Count; ++index)
                    {
                        point1 = controlPoints[index - 1];
                        double x2 = point1.X;
                        point1 = controlPoints[index - 1];
                        double y2 = point1.Y;
                        point1 = controlPoints[index];
                        double x3 = point1.X;
                        point1 = controlPoints[index];
                        double y3 = point1.Y;
                        if (Math.Sqrt(Math.Pow(x3 - x2, 2.0) + Math.Pow(y3 - y2, 2.0)) < 2.0)
                        {
                            LineSegment lineSegment1 = new LineSegment();
                            Point point2 = new Point(x3, y3);
                            lineSegment1.Point = point2;
                            LineSegment lineSegment2 = lineSegment1;
                            segmentCollection.Add(lineSegment2);
                        }
                        else
                        {
                            PolyQuadraticBezierSegment quadraticBezierSegment1 = new PolyQuadraticBezierSegment();
                            PointCollection pointCollection = new PointCollection();
                            Point point2 = new Point(x2, y2);
                            pointCollection.Add(point2);
                            Point point3 = new Point((x2 + x3) / 2.0, (y2 + y3) / 2.0);
                            pointCollection.Add(point3);
                            quadraticBezierSegment1.Points = pointCollection;
                            PolyQuadraticBezierSegment quadraticBezierSegment2 = quadraticBezierSegment1;
                            segmentCollection.Add(quadraticBezierSegment2);
                        }
                    }
                }
                return segmentCollection;
            }

            private void Checkpoint()
            {
                this._canAddPoints = false;
                this._currentPoints.Clear();
                this.CurvesData.Add(CurveData.BuildFrom(this._currentCurveData));
                this._currentCurveData.Points.Clear();
                this._curves.Add(new Curve(Enumerable.ToList<Path>(this._currentCurve)));
                this._currentCurve.Clear();
            }

            public Curve Undo()
            {
                Curve curve = Enumerable.LastOrDefault<Curve>(this._curves);
                if (curve == null)
                    return null;
                CurveData curveData = Enumerable.LastOrDefault<CurveData>(this.CurvesData);
                if (curveData != null)
                    this.CurvesData.Remove(curveData);
                this._curves.RemoveAt(this._curves.Count - 1);
                this._graffitiCacheData.RemoveLastCurve();
                return curve;
            }

            public void Clear()
            {
                this._canAddPoints = false;
                this._currentPoints.Clear();
                this.CurvesData.Clear();
                this._curves.Clear();
                this._graffitiCacheData.RemoveAllCurves();
            }

            public class Curve : List<Path>
            {
                public Curve()
                {
                }

                public Curve(IEnumerable<Path> curvePaths)
                {
                    this.AddRange(curvePaths);
                }
            }

            public class CurveData
            {
                public List<Point> Points { get; private set; }

                public Brush StrokeBrush { get; set; }

                public int StrokeThickness { get; set; }

                public CurveData()
                {
                    this.Points = new List<Point>();
                }

                public static CurveData BuildFrom(CurveData data)
                {
                    CurveData curveData = new CurveData();
                    curveData.StrokeBrush = data.StrokeBrush;
                    curveData.StrokeThickness = data.StrokeThickness;
                    curveData.Points.AddRange((IEnumerable<Point>)Enumerable.ToList<Point>(data.Points));
                    return curveData;
                }
            }

            [DataContract]
            public class GraffitiCacheData
            {
                private GraffitiCacheDataCurve _currentCurve = new GraffitiCacheDataCurve();

                [DataMember]
                public List<GraffitiCacheDataCurve> Curves { get; set; }

                [DataMember]
                public int SelectedStrokeThickness { get; set; }

                [DataMember]
                public string SelectedColorHex { get; set; }

                public GraffitiCacheData()
                {
                    this.Curves = new List<GraffitiCacheDataCurve>();
                }

                public void AddPoint(Point point, bool isLastPoint = false)
                {
                    this._currentCurve.AddPoint(point);
                    if (!isLastPoint)
                        return;
                    this.Curves.Add(this._currentCurve);
                    this._currentCurve = new GraffitiCacheDataCurve();
                }

                public void RemoveLastCurve()
                {
                    if (this.Curves.Count == 0)
                        return;
                    this.Curves.RemoveAt(this.Curves.Count - 1);
                }

                public void RemoveAllCurves()
                {
                    this.Curves.Clear();
                }

                public void SetCurveStrokeThickness(int strokeThickness)
                {
                    this._currentCurve.StrokeThickness = strokeThickness;
                }

                public void SetCurveColorHex(string colorHex)
                {
                    this._currentCurve.ColorHex = colorHex;
                }
            }

            [DataContract]
            public class GraffitiCacheDataCurve
            {
                [DataMember]
                public List<GraffitiCacheDataPoint> Points { get; set; }

                [DataMember]
                public int StrokeThickness { get; set; }

                [DataMember]
                public string ColorHex { get; set; }

                public GraffitiCacheDataCurve()
                {
                    this.Points = new List<GraffitiCacheDataPoint>();
                }

                public void AddPoint(Point point)
                {
                    // ISSUE: explicit reference operation
                    // ISSUE: explicit reference operation
                    this.Points.Add(new GraffitiCacheDataPoint(point.X, point.Y));
                }

                public List<Point> GetPoints()
                {
                    return Enumerable.ToList<Point>(Enumerable.Select<GraffitiCacheDataPoint, Point>(this.Points, (point => new Point(point.X, point.Y))));
                }
            }

            [DataContract]
            public class GraffitiCacheDataPoint
            {
                [DataMember]
                public double X { get; set; }

                [DataMember]
                public double Y { get; set; }

                public GraffitiCacheDataPoint()
                {
                }

                public GraffitiCacheDataPoint(double x, double y)
                {
                    this.X = x;
                    this.Y = y;
                }
            }

            public static class PathDataBuilder
            {
                public static PathFigure CreatePathFigure(Point startPoint)
                {
                    PathFigure pathFigure = new PathFigure();
                    Point point = startPoint;
                    pathFigure.StartPoint = point;
                    PathSegmentCollection segmentCollection = new PathSegmentCollection();
                    pathFigure.Segments = segmentCollection;
                    int num1 = 0;
                    pathFigure.IsClosed = (num1 != 0);
                    int num2 = 0;
                    pathFigure.IsFilled = (num2 != 0);
                    return pathFigure;
                }

                public static Path CreatePath(PathFigure pathFigure, double lineStrokeThickness, Brush strokeBrush)
                {
                    PathGeometry pathGeometry1 = new PathGeometry();
                    pathGeometry1.Figures.Add(pathFigure);
                    Path path = new Path();
                    PathGeometry pathGeometry2 = pathGeometry1;
                    path.Data = ((Geometry)pathGeometry2);
                    double num1 = lineStrokeThickness;
                    ((Shape)path).StrokeThickness = num1;
                    Brush brush = strokeBrush;
                    ((Shape)path).Stroke = brush;
                    int num2 = 2;
                    ((Shape)path).StrokeStartLineCap = ((PenLineCap)num2);
                    int num3 = 2;
                    ((Shape)path).StrokeEndLineCap = ((PenLineCap)num3);
                    int num4 = 2;
                    ((Shape)path).StrokeLineJoin = ((PenLineJoin)num4);
                    return path;
                }
            }
        }

        private GraffitiDrawViewModel VM
        {
            get
            {
                return base.DataContext as GraffitiDrawViewModel;
            }
        }

        private void SelectColor(GraffitiDrawViewModel.ColorViewModel colorVM)
        {
            foreach (var color in this.VM.Colors)
            {
                color.IsSelected = (color.Color == colorVM.Color);
                if (color.IsSelected)
                {
                    if (this.ColorSelected != null)
                        this.ColorSelected(this, color.Color);
                }
            }
        }

        private void Color_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            GraffitiDrawViewModel.ColorViewModel colorVM = ((FrameworkElement)sender).DataContext as GraffitiDrawViewModel.ColorViewModel;
            if (colorVM.IsSelected)
                return;

            this.SelectColor(colorVM);
        }

        private void Thickness_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.SelectThickness(((FrameworkElement)sender).DataContext as GraffitiDrawViewModel.BrushThicknessViewModel);
        }

        public event EventHandler<int> ThicknessSelected;

        private void SelectThickness(GraffitiDrawViewModel.BrushThicknessViewModel thicknessVM)
        {
            if (thicknessVM == null)
                return;

            foreach (var thick in this.VM.ThicknessItems)
            {
                thick.IsSelected = (thick.Thickness == thicknessVM.Thickness);
                if (thick.IsSelected)
                {
                    if (this.ThicknessSelected != null)
                        this.ThicknessSelected(this, thick.Thickness);
                }
            }
        }

        private void UpdateUndoOpacity()
        {
            double num = this._graffitiDrawService.CanUndo ? 1.0 : 0.4;
            this.borderUndo.Opacity = num;
            this.gridAttach.Opacity = num;
        }

        private bool _isUndoing;

        private void Undo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this._isUndoing)
                return;
            this._isUndoing = true;
            var curve = this._graffitiDrawService.Undo();
            if (curve == null)
            {
                this._isUndoing = false;
            }
            else
            {
                foreach (Path path in curve)
                {
                    if (this.drawCanvas.Children.Contains(path))
                        this.drawCanvas.Children.Remove(path);
                }
                this.UpdateUndoOpacity();
                this._isUndoing = false;
            }
        }

        private bool _isSaving;
        public Action<RenderTargetBitmap> SendAction;

        private async void Action_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!this._graffitiDrawService.CanUndo || this._isSaving)
                return;
            this._isSaving = true;
            RenderTargetBitmap bitmap = await this.CreateRenderBitmap();
            
            if (this.SendAction != null)
                this.SendAction(bitmap);
        }

        private async Task<RenderTargetBitmap> CreateRenderBitmap()
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(this.drawCanvas, (int)this.drawCanvas.ActualWidth, (int)this.drawCanvas.ActualHeight);
            return renderTargetBitmap;
        }
        
        private void Clear_Tapped(object sender, TappedRoutedEventArgs e)
        {         
            this._graffitiDrawService.Clear();
            this.drawCanvas.Children.Clear();
            this.drawCanvas.Background = new SolidColorBrush(Colors.Transparent);
            this.UpdateUndoOpacity();
        }

        private async void ApplyBackGround()
        {
            Windows.Storage.Pickers.FileOpenPicker fileOpenPicker = new Windows.Storage.Pickers.FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add(".jpeg");
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".png");

            fileOpenPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;

            StorageFile file = await fileOpenPicker.PickSingleFileAsync();

            if (file != null)
            {
                BitmapImage bimg = new BitmapImage();

                using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    bimg.SetSource(stream);
                }
                ImageBrush imgb = new ImageBrush();
                imgb.Stretch = Stretch.Uniform;
                imgb.ImageSource = bimg;
                this.drawCanvas.Background = imgb;
            }
        }

        private void drawCanvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Canvas canvs = sender as Canvas;
            var ttv = canvs.TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            
            

            Point position = e.GetPosition(null);

            position.Y -= screenCoords.Y;
            position.X -= screenCoords.X;

            this.HandleTouchPoint(position, true);
            this._isDrawing = false;
            this.UpdateUndoOpacity();
        }

        private void ApplyBack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.ApplyBackGround();
        }

       
    }
}
