using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace LunaVK.UC.Controls
{
    public partial class Chart : Grid
    {
        private PathSegmentCollection pathSegments = new PathSegmentCollection();
        private PathFigure figure = new PathFigure();

        /// <summary>
        /// Initializes a new instance of the Chart class.
        /// </summary>
        public Chart()
        {
            Path path = new Path();
            path.VerticalAlignment = VerticalAlignment.Bottom;

            PathGeometry pathGeometry = new PathGeometry();
            PathFigureCollection pathFigures = new PathFigureCollection();

            
            
            this.figure.Segments = this.pathSegments;

            pathFigures.Add(this.figure);

            pathGeometry.Figures = pathFigures;
            path.Data = pathGeometry;

            base.Children.Add(path);

            base.SizeChanged += Chart_SizeChanged;
        }

        private void Chart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ProcessData();
        }


#region DataSource
        /// <summary>
        /// Identifies <see cref="DataSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(
            nameof(DataSource), typeof(IEnumerable), typeof(Chart),
            new PropertyMetadata(null, new PropertyChangedCallback(Chart.OnDataSourcePropertyChanged)));

        /// <summary>
        /// Gets or sets data source for the chart.
        /// This is a dependency property.
        /// </summary>
        public IEnumerable DataSource
        {
            get { return (IEnumerable)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        private static void OnDataSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Chart chart = d as Chart;
            DetachOldDataSourceCollectionChangedListener(chart, e.OldValue);
            AttachDataSourceCollectionChangedListener(chart, e.NewValue);
            chart.ProcessData();
        }
#endregion

#region Stroke
        /// <summary>
        /// Gets or sets stroke (outline) Brush for the indicator.
        /// This is a dependency property.
        /// </summary>
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register( nameof(Stroke), typeof(Brush), typeof(Chart), new PropertyMetadata(null, new PropertyChangedCallback(Chart.OnStrokePropertyChanged)));

        private static void OnStrokePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Chart chart = d as Chart;
            Path path = chart.Children[0] as Path;
            path.Stroke = e.NewValue as Brush;
        }
#endregion

#region StrokeThickness
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register( nameof(StrokeThickness), typeof(double), typeof(Chart), new PropertyMetadata(null, new PropertyChangedCallback(Chart.OnStrokeThicknessPropertyChanged)));

        private static void OnStrokeThicknessPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Chart chart = d as Chart;
            Path path = chart.Children[0] as Path;
            path.StrokeThickness = (double)e.NewValue;
        }
#endregion



        private static void DetachOldDataSourceCollectionChangedListener(Chart chart, object dataSource)
        {
            if (dataSource != null && dataSource is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged -= chart.OnDataSourceCollectionChanged;
            }
        }

        private static void AttachDataSourceCollectionChangedListener(Chart chart, object dataSource)
        {
            if (dataSource != null && dataSource is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged += chart.OnDataSourceCollectionChanged;
            }
        }
        

        private void OnDataSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.ProcessData();
        }

        private void ProcessData()
        {
            this.pathSegments.Clear();

            if (this.DataSource != null)
            {
                PolyLineSegment polyBezierSegment = new PolyLineSegment();//PolyBezierSegment


                double mutiplierX = base.ActualWidth / ((this.DataSource as IList).Count - 1 );

                double max = 0;

                for (int i = 0; i < (this.DataSource as IList).Count; i++)
                {
                    double dataItem = (double)(this.DataSource as IList)[i];
                    if (max < dataItem)
                        max = dataItem;
                }

                double mutiplierY = max == 0 ? 1 : (base.ActualHeight / max);
                for (int i=0;i< (this.DataSource as IList).Count;i++)
                {
                    double dataItem = (double)(this.DataSource as IList)[i] * mutiplierY;
                    polyBezierSegment.Points.Add(new Point(i * mutiplierX, -dataItem));
                    
                    if (i == 0)
                    {
                        this.figure.StartPoint = new Point(i * mutiplierX, -dataItem);
                    }
                }
                this.pathSegments.Add(polyBezierSegment);
            }
        }
    }
}
