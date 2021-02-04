using System;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LunaVK.Photo.UC
{
    public sealed partial class AdornerElementBaseUC : UserControl, ICompositeShape
    {
        private Point _startDragPoint;
        private Point _currentDragPoint;
        private double _adornedControlInitialScale;
        private double _initialDist;
        private double _adornedControlInitialAngle;

        public Action<Point> _mousePosition;
        public Action<Point> _elementPosition;
        public Action<double> _scaleChanged;

        double _initialWidth;
        double _initialHeight;

        private FrameworkElement _child;
        public FrameworkElement Control
        {
            get { return this._child; }
        }

        public Action<ICompositeShape> OnTap { get; set; }
        public Action<ICompositeShape> OnDelete { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return this._isSelected;
            }
            set
            {
                this._isSelected = value;
                this._brd.Visibility = this.AdornerRotateButton.Visibility = this.AdornerDeleteButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                this.AdornerRotateButton.IsHitTestVisible = this.AdornerDeleteButton.IsHitTestVisible = value;
            }
        }

        public AdornerElementBaseUC(double initialWidth, double initialHeight, FrameworkElement element)
        {
            this.InitializeComponent();

            this.AdornerRotateButton.ManipulationMode = ManipulationModes.All;
            this.AdornerRotateButton.ManipulationStarted += AdornerRotateButton_ManipulationStarted;
            this.AdornerRotateButton.ManipulationDelta += AdornerRotateButton_ManipulationDelta;


            this._gridContent.MinWidth = this._initialWidth = initialWidth;
            this._gridContent.MinHeight = this._initialHeight = initialHeight;

            this._child = element;

            if(element!=null)
                this._gridContent.Children.Add(element);
        }

        public void UpdateSize(double w, double h)
        {
            this._gridContent.Width = w;
            this._gridContent.Height = h;

            this._initialWidth = w / this.Scale;
            this._initialHeight = h / this.Scale;

        }

        private void AdornerRotateButton_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.IsInertial)
                return;

            double x = e.Delta.Translation.X;
            double y = e.Delta.Translation.Y;
            this._currentDragPoint.X = this._currentDragPoint.X + x;
            this._currentDragPoint.Y = this._currentDragPoint.Y + y;

            if (_mousePosition != null)
                _mousePosition(this._currentDragPoint);

            /*
            double x = e.Delta.Translation.X;
            double y = e.Delta.Translation.Y;
            //Point local1 = this._currentDragPoint;
            //local1.X = local1.X + x1;
            //local1.Y = local1.Y + y;
            this._currentDragPoint.X = this._currentDragPoint.X + x;
            this._currentDragPoint.Y = this._currentDragPoint.Y + y;

            System.Diagnostics.Debug.WriteLine("x:{0} y:{1} dis:{2}", x, y, this._initialDist);

            double scale = this._adornedControlInitialScale * this.VectorLen(new Point(this.Center.X - this._currentDragPoint.X, this.Center.Y - this._currentDragPoint.Y)) / this._initialDist;
            if (scale < this.MinScale)
                scale = this.MinScale;
            double x2 = this._currentDragPoint.X - this.Center.X;
            double rotation = Math.Atan2(this._currentDragPoint.Y - this.Center.Y, x2) * 180.0 / Math.PI + this._adornedControlInitialAngle;
            if ((int)Math.Abs(rotation) % 360 < 7)
                rotation = 0.0;
            this.SetScaleAndRotation(scale, rotation);
            */

            if (this.Center.X == 0)
                return;

            double dist = this.VectorLen(new Point(this.Center.X - this._currentDragPoint.X, this.Center.Y - this._currentDragPoint.Y));
            double scale = this._adornedControlInitialScale * dist / this._initialDist;

            if (scale < 1.0)
                scale = 1.0;
            if (scale > 5.0)
                scale = 5.0;
            

            //this._transformContent.ScaleX = this._transformContent.ScaleY = scale;
            this._gridContent.Width = this._initialWidth * scale;
            this._gridContent.Height = this._initialHeight * scale;
            this.Scale = scale;

            if (_scaleChanged != null)
                _scaleChanged(scale);


            double rotation = this.CalcAngle(this._currentDragPoint, this.Center) + this._adornedControlInitialAngle;

            if ((int)Math.Abs(rotation) % 360 < 6)
                rotation = 0.0;

            //this._text.Text = string.Format("cur x:{0},y:{1}", (int)this._currentDragPoint.X, (int)this._currentDragPoint.Y);
            //this._text2.Text = string.Format("scale:{0} rotation:{1} dist:{2}", (int)scale, (int)rotation,(int)dist);
            this._btnTransform2.Rotation = -rotation;
            this._btnTransform.Rotation = -rotation;
            this._transform.Rotation = rotation;

            e.Handled = true;
        }

        private void AdornerRotateButton_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            this._currentDragPoint = this._startDragPoint = (sender as FrameworkElement).TransformToVisual(Window.Current.Content).TransformPoint(e.Position);//позиция на странице
            this._initialDist = this.VectorLen(new Point(this.Center.X - this._startDragPoint.X, this.Center.Y - this._startDragPoint.Y));//начальная дистация
            this._adornedControlInitialScale = this.Scale;

            double num = this.CalcAngle(this._startDragPoint, new Point(this.Center.X, this.Center.Y));
            this._adornedControlInitialAngle = this.Rotation - num;

            if (_mousePosition != null)
                _mousePosition(this._currentDragPoint);

            if(this.Center!=null)
            {
                int i = 0;
            }
            /*
            //this._currentDragPoint = this._startDragPoint = e.Position;
            //this._currentDragPoint = this._startDragPoint = this.TransformToParentContainer(element, e.Position);
            this._currentDragPoint = this._startDragPoint = (sender as FrameworkElement).TransformToVisual(Window.Current.Content).TransformPoint(e.Position);

            this._initialDist = this.VectorLen(new Point(this.Center.X - this._startDragPoint.X, this.Center.Y - this._startDragPoint.Y));
            this._adornedControlInitialScale = this.Scale;
            double num = this.CalcAngle(this._startDragPoint, new Point(this.Center.X, this.Center.Y));
            this._adornedControlInitialAngle = this.Rotation - num;
            */
            e.Handled = true;
        }

        public Point Center
        {
            get
            {
                var temp = this._gridContent.TransformToVisual(Window.Current.Content).TransformPoint(new Point());
            //    temp.X += (this._gridContent.ActualWidth / 2.0);
             //   temp.Y += (this._gridContent.ActualHeight / 2.0);

                //if (_elementPosition != null)
                //    _elementPosition(temp);

                //this._text.Text = string.Format("center x:{0},y:{1}", (int)temp.X, (int)temp.Y);
                //this._text3.Text = string.Format("actual w:{0},h:{1}", (int)(Window.Current.Content as Frame).ActualWidth, (int)(Window.Current.Content as Frame).ActualHeight);
                return temp;
                //return this.AdornerRotateButton.ActualHeight / 2.0;
            }
        }

        /*
        public virtual void Resize(double scaleX, double scaleY)
        {
            double num = (scaleX + scaleY) / 2.0;
            this.Scale *= num;
            this.CenterX *= num;
            this.CenterY *= num;
        }

            public virtual void SetCenterPosition(double centerX, double centerY)
    {
      if (double.IsNaN(centerX))
        centerX = 0.0;
      if (double.IsNaN(centerY))
        centerY = 0.0;
      this.State.CenterX = centerX;
      this.State.CenterY = centerY;
      this.UpdateCenterPosition();
    }

            public void CenterInParentShape()
    {
      ICompositeShape parentShape = this.ParentShape;
      this.SetCenterPosition(parentShape.Width / 2.0, parentShape.Height / 2.0);
    }
        */
        private double MinScale
        {
            get { return 0.5; }
        }

        

        public double Rotation
        {
            get
            {
                return this._transform.Rotation;
            }
        }

        private double Scale = 1.0;/*
        {
            get
            {
                //return this._transform.ScaleX;
                return this._gridContent.Width / 200;
            }
        }*/

        

        private double VectorLen(Point p)
        {
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        private double CalcAngle(Point p1, Point p2)
        {
            return Math.Atan2(p1.Y - p2.Y, p1.X - p2.X) * 180.0 / Math.PI;
        }

        public Point TransformToParentContainer(FrameworkElement element, Point p)
        {
            return element.TransformToVisual(this._parent).TransformPoint(p);
            //return element.TransformToVisual(Window.Current.Content).TransformPoint(p);
        }
        /*
        internal void SetScaleAndRotation(double scale, double rotation)
        {
            //System.Diagnostics.Debug.WriteLine("scale:{0} rotation:{1}", scale, rotation);
            this._text2.Text = string.Format("scale:{0},rotation:{1}", (int)scale, (int)rotation);

            if (double.IsNaN(scale) || double.IsInfinity(scale))
                scale = 1.0;
            this._transform.Rotation = rotation;
            if (scale < 5.0)
            {
                this._transform.ScaleX = this._transform.ScaleY= scale;
                //_gridContent.Width = 200 * scale;
                //_gridContent.Height = 150 * scale;
                //Scale = scale;
            }
            //this.RaiseChangedEvent();
        }
        */
        private void _parent_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double x = e.Delta.Translation.X;
            double y = e.Delta.Translation.Y;

            _transform.TranslateX += x;
            _transform.TranslateY += y;

            if (this._elementPosition != null)
                this._elementPosition(new Point(x,y));
        }

        private void Delete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (this.OnDelete != null)
                this.OnDelete(this);
        }

        private void _gridContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this._brd.Width = e.NewSize.Width;
            this._brd.Height = e.NewSize.Height;
        }

        private void _gridContent_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (this.OnTap != null)
                this.OnTap(this);
        }
    }
}
