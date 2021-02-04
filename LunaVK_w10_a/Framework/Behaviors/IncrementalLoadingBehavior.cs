using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xaml.Interactivity;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media;
using LunaVK.Core.Library;

namespace LunaVK.Framework.Behaviors
{
    public class IncrementalLoadingBehavior : DependencyObject, IBehavior
    {
        private ScrollViewer inside_scrollViewer;
        private ListView listView;
        public DependencyObject AssociatedObject { get; private set; }
        private bool InLoading;

        /// <summary>
        /// Присоединяет поведние к объекту зависимостей.
        /// </summary>
        /// <param name="associatedObject">Объект, к которому присоединяется поведение.</param>
        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = associatedObject;

            listView = associatedObject as ListView;
            if (listView == null)
                return;
            
            ((FrameworkElement)this.AssociatedObject).Loaded += OnAssociatedObjectLoaded;     
        }

        private void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)this.AssociatedObject).Loaded -= OnAssociatedObjectLoaded;

        //    if (this.AssociatedObject is ListViewBase)
        //        this.inside_scrollViewer = ((ListViewBase)this.AssociatedObject).GetFirstOrDefaultDescendantOfType<ScrollViewer>();

            if (this.inside_scrollViewer == null)
                return;

            //this.inside_scrollViewer.Loaded += this.OnElementLoaded;
            this.inside_scrollViewer.ViewChanging += this.OnElementViewChanging;
        }
        /*
        void listView_Loaded(object sender, RoutedEventArgs e)
        {
            var temp = this.listView.FindChild<ScrollViewer>();
            DependencyObject o =VisualTreeHelper.GetChild(this.listView, 0);
            Border border = (Border)VisualTreeHelper.GetChild(this.listView, 0);
            this.inside_scrollViewer = (ScrollViewer)border.Child;

            this.inside_scrollViewer.ViewChanged += sv_ViewChanged;
        }
        */

        private void OnElementViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {

        }

        private async void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate || this.InLoading)
                return;

            ScrollViewer sv = sender as ScrollViewer;
            if (sv.VerticalScrollMode != ScrollMode.Disabled)
            {
                if (sv.ScrollableHeight - sv.VerticalOffset < 700)
                {
                    if (listView.DataContext is ISupportLoadMore)
                    {
                        if ((listView.DataContext as ISupportLoadMore).HasMoreItems == false)
                            return;

                        this.InLoading = true;
                        await (listView.DataContext as ISupportLoadMore).LoadData();
                        this.InLoading = false;
                    }
                }
            }
            else
            {
                if (sv.ScrollableWidth - sv.HorizontalOffset < 700)
                {
                    if (listView.DataContext is ISupportLoadMore)
                    {
                        if ((listView.DataContext as ISupportLoadMore).HasMoreItems == false)
                            return;

                        this.InLoading = true;
                        await (listView.DataContext as ISupportLoadMore).LoadData();
                        this.InLoading = false;
                    }
                }
            }
        }

        public void Detach()
        {
            this.AssociatedObject = null;
        }
    }
}
