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
using Windows.UI.Xaml.Navigation;

using LunaVK.Core.DataObjects;
using Windows.UI.Xaml.Shapes;
using LunaVK.Framework;

using Windows.Storage;
using Windows.Storage.Streams;
using System.Text;
using LunaVK.Core.Framework;

namespace LunaVK.UC.Attachment
{
    [TemplatePart(Name = PART_CanvasName, Type = typeof(Canvas))]
    [TemplatePart(Name = PART_CanvasMaskName, Type = typeof(Canvas))]
    public class WaveformControl : Slider
    {
        private const string PART_CanvasName = "PART_Canvas";
        private const string PART_CanvasMaskName = "PART_CanvasMask";
        private Canvas _canvas;
        private Canvas _canvasMask;
        private Grid _horizontalFill;
        private const int WAVEFORM_ITEM_MIN_HEIGHT = 3;
        //private const int WAVEFORM_ITEM_MAX_HEIGHT = 25;
        private const int WAVEFORM_ITEM_WIDTH = 3;
        private const int WAVEFORM_ITEM_BETWEEN = 1;

#region ItemsSource
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IList<int>), typeof(WaveformControl), new PropertyMetadata(null, new PropertyChangedCallback((d, e) => ((WaveformControl)d).RenderWaveform())));

        public IList<int> ItemsSource
        {
            get { return (IList<int>)base.GetValue(WaveformControl.ItemsSourceProperty); }
            set { base.SetValue(WaveformControl.ItemsSourceProperty, value); }
        }
#endregion

#region WaveformWidth
        public static readonly DependencyProperty WaveformWidthProperty = DependencyProperty.Register("WaveformWidth", typeof(double), typeof(WaveformControl), new PropertyMetadata(0, new PropertyChangedCallback((d, e) => ((WaveformControl)d).UpdateWaveformWidth())));

        public double WaveformWidth
        {
            get { return (double)base.GetValue(WaveformControl.WaveformWidthProperty); }
            set { base.SetValue(WaveformControl.WaveformWidthProperty, value); }
        }
#endregion

        public List<int> Waveform { get; set; }

        public WaveformControl()
        {
            base.DefaultStyleKey = typeof(WaveformControl);
            base.Tag = "CantTouchThis";
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this._canvas = base.GetTemplateChild("PART_Canvas") as Canvas;
            this._canvasMask = base.GetTemplateChild("PART_CanvasMask") as Canvas;
            this._horizontalFill = base.GetTemplateChild("HorizontalFill") as Grid;
            base.ValueChanged += this.WaveformControl_ValueChanged;
            this.RenderWaveform();
        }

        private RectangleGeometry _clip
        {
            get { return this._horizontalFill.Clip as RectangleGeometry; }
        }

        private void WaveformControl_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            double percent = e.NewValue / base.Maximum;
            this._clip.Rect = new Rect(0, 0, base.Width * percent, base.ActualHeight/* WAVEFORM_ITEM_MAX_HEIGHT*/);            
        }

        private void UpdateWaveformWidth()
        {
            this.UpdateWaveformItemsSource(this.Waveform);
        }

        private void UpdateWaveformItemsSource(List<int> waveform)
        {
            if (waveform == null || waveform.Count == 0)
                return;
            int targetLength = (int)(Math.Max(0.0, this.WaveformWidth) / 4.0);
            List<int> intList1 = this.Resample(waveform, targetLength);
            int max = Enumerable.Max(intList1);
            List<int> intList2 = new List<int>();
            List<int>.Enumerator enumerator = intList1.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    int num2 = (int)Math.Round(base.ActualHeight/* WAVEFORM_ITEM_MAX_HEIGHT*/ * ((double)enumerator.Current * 1.0 / (double)max));
                    if (num2 < WAVEFORM_ITEM_MIN_HEIGHT)
                        num2 = WAVEFORM_ITEM_MIN_HEIGHT;
                    if (num2 % 2 != 0)
                        ++num2;
                    intList2.Add(num2);
                }
            }
            finally
            {
                enumerator.Dispose();
            }
            this.ItemsSource = intList2;
        }

        private void RenderWaveform()
        {
            if (this.ItemsSource == null || this._canvas == null || this._canvasMask == null)
                return;
            this._canvas.Children.Clear();
            this._canvasMask.Children.Clear();
            for (int index = 0; index < this.ItemsSource.Count; index++)
            {
                int waveformItem = this.ItemsSource[index];
                int left = index * 4;
                double top = 16.0 - waveformItem / 2.0;
                this._canvas.Children.Add(this.GetWaveformItem(waveformItem, left, top));
                this._canvasMask.Children.Add(this.GetWaveformItem(waveformItem, left, top));
            }

            base.Width = this._canvas.ActualWidth;
        }

        private FrameworkElement GetWaveformItem(int waveformItem, int left, double top)
        {
            Border border = new Border();
            border.Width = WAVEFORM_ITEM_WIDTH;
            border.Height = waveformItem;
            border.CornerRadius = new CornerRadius(WAVEFORM_ITEM_WIDTH);
            border.Margin = new Thickness(0.0, 0.0, WAVEFORM_ITEM_BETWEEN, 0.0);
            border.Style = (Style)Application.Current.Resources["BorderThemeHigh"];//border.Fill = this.Foreground;
            Canvas.SetLeft(border, left);
            Canvas.SetTop(border, top);
            return border;
        }

        private List<int> Resample(List<int> source, int targetLength)
        {
            if (source == null || source.Count == 0 || source.Count == targetLength)
                return source;
            int[] numArray = new int[targetLength];
            if (source.Count < targetLength)
            {
                double num = (double)source.Count / (double)targetLength;
                for (int index = 0; index < targetLength; ++index)
                    numArray[index] = source[(int)((double)index * num)];
            }
            else
            {
                double val2 = (double)source.Count / (double)targetLength;
                double num1 = 0.0;
                double num2 = 0.0;
                int index = 0;
                List<int>.Enumerator enumerator = source.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        int current = enumerator.Current;
                        double num3 = Math.Min(num2 + 1.0, val2) - num2;
                        num1 += (double)current * num3;
                        num2 += num3;
                        if (num2 >= val2 - 0.001)
                        {
                            numArray[index++] = (int)Math.Round(num1 / val2);
                            if (num3 < 1.0)
                            {
                                num2 = 1.0 - num3;
                                num1 = (double)current * num2;
                            }
                            else
                            {
                                num2 = 0.0;
                                num1 = 0.0;
                            }
                        }
                    }
                }
                finally
                {
                    enumerator.Dispose();
                }
                if (num1 > 0.0 && index < targetLength)
                    numArray[index] = (int)Math.Round(num1 / val2);
            }
            return Enumerable.ToList(numArray);
        }
    }
}
