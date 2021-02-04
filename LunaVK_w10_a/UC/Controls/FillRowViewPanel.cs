using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.UC.Controls
{
    public class FillRowViewPanel : Panel
    {
#region MinRowItemsCount
        public int MinRowItemsCount
        {
            get { return (int)GetValue(MinRowItemsCountProperty); }
            set { SetValue(MinRowItemsCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinRowItemsCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinRowItemsCountProperty = DependencyProperty.Register("MinRowItemsCount", typeof(int), typeof(FillRowViewPanel), new PropertyMetadata(0));
#endregion


        protected override Size MeasureOverride(Size availableSize)
        {
            Size size = base.MeasureOverride(availableSize);
            return size;
        }
        
        /// <summary>
        /// обеспечивающие логику измерения и упорядочения дочерних элементов
        /// </summary>
        /// <param name="availableSize">Это размер, который родительский объект использовал при вызове метода Measure</param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size availableSize)
        {
            //base.Children - по 2, по 3 элемента

            double childrenWidth = 0;
            double childrenHeight = 0;
            foreach (var child in base.Children)
            {
                //if (child is ContentPresenter cc && cc.Content is ThumbnailsLayoutHelper.IThumbnailSupport iResizable)
                if ((child as FrameworkElement).DataContext is ThumbnailsLayoutHelper.IThumbnailSupport iResizable)
                {
                    //double aspectratio = iResizable.Width / iResizable.Height;//соотношение сторон
                    double width = iResizable.Width;// aspectratio * availableSize.Height;
                    childrenWidth += width;



                    double height = iResizable.Height;// aspectratio * availableSize.Width;
                    childrenHeight = Math.Max(height, childrenHeight);
                }
            }


            //double ratio = childrenWidth / availableSize.Width;//множитель?
            double ratio = availableSize.Width / childrenWidth;
            childrenHeight *= ratio;
            double x = 0;
            //int count = Children.Count;
            foreach (var child in base.Children)
            {
                //var temp = (child as FrameworkElement).DataContext;
                //if(temp is ThumbnailsLayoutHelper.IThumbnailSupport iResizable0)
                //{
                //    int i = 0;
                //}
                //if (child is ContentPresenter cc && cc.Content is ThumbnailsLayoutHelper.IThumbnailSupport iResizable)//ContentControl
                if((child as FrameworkElement).DataContext is ThumbnailsLayoutHelper.IThumbnailSupport iResizable)
                {
                    double height = childrenHeight;


                    double width = iResizable.Width * height / iResizable.Height;
                    double width2 = iResizable.Width * ratio;

                    width = width2;
                    /*
                    //if children count is less than MinRowItemsCount and chidren total width less than finalwidth, it don't need to stretch children
                    if (count < MinRowItemsCount && ratio < 1)
                    {
                        //to nothing
                        int i = 0;
                    }
                    else
                    {
                        width /= ratio;
                    }
                    */

                    System.Diagnostics.Debug.Write(string.Format("{0}x{1} ", (int)width,(int)height));
                    (child as FrameworkElement).Width = width;
                    (child as FrameworkElement).Height = height;
                    child.Arrange(new Rect(x, 0, width, height));
                    x += width;
                }
            }
            System.Diagnostics.Debug.WriteLine("\n");

            availableSize.Height = childrenHeight;
            return base.ArrangeOverride(availableSize);
        }
    }
}
