using LunaVK.Core.Utils;
using LunaVK.UC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace LunaVK.Framework
{
    public class DragControlsHelper
    {
        public Canvas ParentControl;
        private List<FrameworkElement> Items;

        public DragControlsHelper(Canvas parent)
        {
            this.Items = new List<FrameworkElement>();
            this.ParentControl = parent;
            this.ParentControl.SizeChanged += ParentControl_SizeChanged;
        }

        private void ParentControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.Items.Count == 0)
                return;

            foreach (var item in this.Items)
            {
                var ttv = item.TransformToVisual(this.ParentControl);
                Point screenCoords = ttv.TransformPoint(new Point(0, 0));
                CompositeTransform compositeTransform = item.RenderTransform as CompositeTransform;
                double diff = e.NewSize.Width - (screenCoords.X + (item.ActualWidth* compositeTransform.ScaleX));
                if (diff < 0)
                {
                    (item.RenderTransform as CompositeTransform).TranslateX += diff;
                }
            }
        }

        public void Add(FrameworkElement item)
        {
            var ttv = item.TransformToVisual(this.ParentControl);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));

            Grid parent = item.Parent as Grid;
            parent.Children.Remove(item);
            DragItemUC dragItem = new DragItemUC();
            dragItem.SetContent(item);
            this.ParentControl.Children.Add(dragItem);

            CompositeTransform compositeTransform = new CompositeTransform();
            dragItem.RenderTransform = compositeTransform;
            dragItem.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            dragItem.ManipulationDelta += DragItem_ManipulationDelta;

            dragItem.Loaded += DragItem_Loaded;
            dragItem.Unloaded += this.DragItem_Unloaded;
            dragItem.Close = this.CloseCallback;

            this.Items.Add(dragItem);

            compositeTransform.TranslateX = screenCoords.X;
            compositeTransform.TranslateY = screenCoords.Y;
        }

        public void Remove(FrameworkElement frameworkElement)
        {
            foreach(var item in this.Items)
            {
                DragItemUC dragItemUC = item as DragItemUC;
                dragItemUC.RemoveContent();
                this.ParentControl.Children.Remove(dragItemUC);
                dragItemUC = null;
            }

            this.Items.Clear();
        }

        private void DragItem_Loaded(object sender, RoutedEventArgs e)
        {
            this.MakeCompact(sender as DragItemUC);
        }

        private void DragItem_Unloaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (sender as FrameworkElement);
            this.ParentControl.Children.Remove(element);
            this.Items.Remove(element);
        }

        private void DragItem_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            FrameworkElement element = (sender as FrameworkElement);
            CompositeTransform transform = element.RenderTransform as CompositeTransform;

            if (transform.TranslateX + e.Delta.Translation.X + (element.ActualWidth * transform.ScaleX) > this.ParentControl.ActualWidth)
                return;

            if (transform.TranslateX + e.Delta.Translation.X < 0)
                return;

            if (transform.TranslateY + e.Delta.Translation.Y + (element.ActualHeight * transform.ScaleY) > this.ParentControl.ActualHeight)
                return;

            if (transform.TranslateY + e.Delta.Translation.Y < 0)
                return;

            transform.TranslateX += e.Delta.Translation.X;
            transform.TranslateY += e.Delta.Translation.Y;
        }

        private void CloseCallback(FrameworkElement element)
        {
            this.ParentControl.Children.Remove(element);
            this.Items.Remove(element);
            element = null;
        }

        private void MakeCompact(DragItemUC dragItem)
        {
            Size childSize = new Size(dragItem.ActualWidth, dragItem.ActualHeight);

            double ratio = dragItem.ActualHeight / dragItem.ActualWidth;

            double w = 200;//будущая ширина проигрывателя
            double h = w * ratio;//будущая высота проигрывателя для пропорции

            Rect target = new Rect(this.ParentControl.ActualWidth - w - 10, this.ParentControl.ActualHeight - h - 10, w, h);
            CompositeTransform compositeTransform1 = RectangleUtils.TransformRect(new Rect(new Point(), childSize), target, false);//позиционирует и вычисляет масштаб
            //CustomFrame.Instance.VideoViewerElement.RenderTransform = tr;

            CompositeTransform renderTransform = dragItem.RenderTransform as CompositeTransform;
            //Debug.Assert(renderTransform != null);
            if (renderTransform != null)
            {
                renderTransform.Animate(renderTransform.TranslateX, renderTransform.TranslateX + compositeTransform1.TranslateX, "TranslateX", 600, 0, null);
                renderTransform.Animate(renderTransform.TranslateY, renderTransform.TranslateY + compositeTransform1.TranslateY, "TranslateY", 600, 0, null);
                renderTransform.Animate(renderTransform.ScaleX, compositeTransform1.ScaleX, "ScaleX", 600, 0, null, null);
                renderTransform.Animate(renderTransform.ScaleY, compositeTransform1.ScaleY, "ScaleY", 600, 0, null, null);
            }
            //CustomFrame.Instance.VideoViewerElement.MakeCompact();
        }
    }
}
