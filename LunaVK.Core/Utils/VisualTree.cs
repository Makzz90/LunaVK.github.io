using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LunaVK.Core.Utils
{
    public static class VisualTree
    {
        public static ScrollViewer GetScrollViewer(this DependencyObject element)
        {
            if (element is ScrollViewer)
            {
                return (ScrollViewer)element;
            }
            int lol = VisualTreeHelper.GetChildrenCount(element);
            for (var i = 0; i < lol; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);

                var result = GetScrollViewer(child);
                if (result == null) continue;

                return result;
            }

            return null;
        }

        public static DependencyObject FindChild<T>(this DependencyObject element)
        {
            if (element is T)
                return element;
            
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);

                var result = VisualTree.FindChild<T>(child);
                if (result == null)
                    continue;

                return result;
            }

            return null;
        }

        /// <summary>
        /// Получаем вложенные элементы по типу
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="applyTemplates"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetLogicalChildrenByType<T>(this FrameworkElement parent, bool applyTemplates) where T : FrameworkElement
        {
            if (applyTemplates && parent is Control)
                ((Control)parent).ApplyTemplate();
            Queue<FrameworkElement> queue = new Queue<FrameworkElement>(Enumerable.OfType<FrameworkElement>((IEnumerable)((DependencyObject)parent).GetVisualChildren()));
            while (queue.Count > 0)
            {
                FrameworkElement element = queue.Dequeue();
                if (applyTemplates && element is Control)
                    ((Control)element).ApplyTemplate();
                if (element is T)
                    yield return (T)element;
                IEnumerator<FrameworkElement> enumerator = (Enumerable.OfType<FrameworkElement>((IEnumerable)((DependencyObject)element).GetVisualChildren())).GetEnumerator();
                try
                {
                    while (((IEnumerator)enumerator).MoveNext())
                        queue.Enqueue(enumerator.Current);
                }
                finally
                {
                    if (enumerator != null)
                        ((IDisposable)enumerator).Dispose();
                }
                element = null;
            }
        }

        public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject parent)
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            int num;
            for (int i = 0; i < childrenCount; i = num + 1)
            {
                yield return VisualTreeHelper.GetChild(parent, i);
                num = i;
            }
            yield break;
        }

        public static T GetFirstAncestorOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            return start.GetAncestorsOfType<T>().FirstOrDefault();
        }

        public static IEnumerable<T> GetAncestorsOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            return start.GetAncestors().OfType<T>();
        }

        public static IEnumerable<DependencyObject> GetAncestors(this DependencyObject start)
        {
            var parent = VisualTreeHelper.GetParent(start);

            while (parent != null)
            {
                yield return parent;
                parent = VisualTreeHelper.GetParent(parent);
            }
        }









        /*
        public static T GetFirstOrDefaultDescendantOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            return start.GetDescendantsOfType<T>().FirstOrDefault();
        }

        public static IEnumerable<T> GetDescendantsOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            return start.GetDescendants().OfType<T>();
        }

        public static IEnumerable<DependencyObject> GetDescendants(this DependencyObject start)
        {
            var queue = new Queue<DependencyObject>();
            var count = VisualTreeHelper.GetChildrenCount(start);

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(start, i);
                yield return child;
                queue.Enqueue(child);
            }

            while (queue.Count > 0)
            {
                var parent = queue.Dequeue();
                var count2 = VisualTreeHelper.GetChildrenCount(parent);

                for (int i = 0; i < count2; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    yield return child;
                    queue.Enqueue(child);
                }
            }
        }*/
    }
}
