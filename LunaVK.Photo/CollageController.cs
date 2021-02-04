using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace LunaVK.Photo
{
    public class CollageController
    {
        public readonly Grid _parent;
        private List<ContainedShapeControlContainer> _shapes;
        public Action<ContainedShapeControlContainer> ShapeSelected;

        public CollageController(Grid parent)
        {
            this._parent = parent;
            this._shapes = new List<ContainedShapeControlContainer>();

            //this._parent.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
            this._parent.Tapped += _parent_Tapped;
        }

        public List<ContainedShapeControlContainer> Shapes
        {
            get { return this._shapes; }
        }

        private void _parent_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if ((sender as FrameworkElement) != (e.OriginalSource as FrameworkElement))
                return;

            this.UnselectAll();

            if (this.ShapeSelected != null)
                this.ShapeSelected(null);

            e.Handled = true;
        }

        public void UnselectAll()
        {
            foreach (var sh in this._shapes)
            {
                sh.IsInEditMode = sh.ParentContainer.IsSelected = false;
                sh.FadeIn();
            }

            if (this.ShapeSelected != null)
                this.ShapeSelected(null);
        }

        public void AddShape(ContainedShapeControlContainer shape)
        {
            //foreach (var sh in this._shapes)
            //    sh.IsInEditMode = false;

            shape.ParentContainer.OnDelete = this.OnDelete;
            shape.ParentContainer.OnTap = this.OnSelected;

            this._parent.Children.Add(shape.ParentContainer as FrameworkElement);
            //shape.IsInEditMode = true;
            this._shapes.Add(shape);
            this.OnSelected(shape.ParentContainer);
        }

        private void OnDelete(ICompositeShape shape)
        {
            //foreach (var sh in this._shapes)
            //    sh.IsInEditMode = false;
            this.UnselectAll();

            ContainedShapeControlContainer toDelete = this._shapes.First(s => s.ParentContainer == shape);
            this._parent.Children.Remove(toDelete.ParentContainer as FrameworkElement);
            this._shapes.Remove(toDelete);
        }

        public void DeleteAll()
        {
            this._shapes.Clear();
            this._parent.Children.Clear();
        }

        public void OnSelected(ICompositeShape shape)
        {
            foreach (var sh in this._shapes)
            {
                if (sh.ParentContainer != shape)
                {
                    sh.ParentContainer.IsSelected = sh.IsInEditMode = false;
                    sh.FadeOut();

                }
                else
                {
                    sh.ParentContainer.IsSelected = sh.IsInEditMode = true;
                    sh.FadeIn();

                    if (this.ShapeSelected != null)
                        this.ShapeSelected(sh);
                }
            }
        }
    }
}
