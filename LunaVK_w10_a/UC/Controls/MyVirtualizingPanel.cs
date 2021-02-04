using LunaVK.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace LunaVK.UC.Controls
{
    public class MyVirtualizingPanel : Canvas
    {
        public double LoadUnloadThreshold = 500.0;
        public double LoadedHeightUpwards = 500.0;
        public double LoadedHeightUpwardsNotScrolling = 500.0;
        public double LoadedHeightDownwards = 1200.0;
        public double LoadedHeightDownwardsNotScrolling = 1200.0;

        public ScrollViewer ScrollViewer { get; private set; }
        public List<IVirtualizable> VirtualizableItems { get; private set; }
        private Canvas _itemsPanel = new Canvas();
        private bool _isScrolling;
        public bool KeepScrollPositionWhenAddingItems;

#region ItemsSource
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IList<IVirtualizable>), typeof(MyVirtualizingPanel), new PropertyMetadata(new PropertyChangedCallback(MyVirtualizingPanel.OnItemsSourcePropertyChanged)));
        
        public IList<IVirtualizable> ItemsSource
        {
            get { return (IList<IVirtualizable>)base.GetValue(MyVirtualizingPanel.ItemsSourceProperty); }
            set {  base.SetValue(MyVirtualizingPanel.ItemsSourceProperty, value); }
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MyVirtualizingPanel virtualizingPanel = d as MyVirtualizingPanel;
            if (virtualizingPanel == null)
                return;
            virtualizingPanel.ClearItems();
            
            INotifyCollectionChanged oldValue = e.OldValue as INotifyCollectionChanged;
            if (oldValue != null)
                virtualizingPanel.UnhookCollectionChanged(oldValue);
            
            INotifyCollectionChanged newValue = e.NewValue as INotifyCollectionChanged;
            if (newValue == null)
                return;
            List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
            IEnumerator enumerator = (newValue as ICollection).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object current = enumerator.Current;
                    if (current is IVirtualizable)
                        virtualizableList.Add(current as IVirtualizable);
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
            virtualizingPanel.AddItems(virtualizableList);
            virtualizingPanel.HookUpCollectionChanged(newValue);
        }
#endregion

        public MyVirtualizingPanel()
        {
            this.ScrollViewer = new ScrollViewer();
            this.ScrollViewer.Content = this._itemsPanel;
            base.Children.Add(this.ScrollViewer);
            this.VirtualizableItems = new List<IVirtualizable>();
        }

        public void HookUpCollectionChanged(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += this.collection_CollectionChanged;
        }

        public void UnhookCollectionChanged(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= this.collection_CollectionChanged;
        }

        private void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            MyVirtualizingPanel virtualizingPanel = this;
            List<IVirtualizable> itemsToInsert = new List<IVirtualizable>();
            if (e.NewItems != null)
            {
                IEnumerator enumerator = e.NewItems.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        if (current is IVirtualizable)
                            itemsToInsert.Add(current as IVirtualizable);
                    }
                }
                finally
                {
                    IDisposable disposable = enumerator as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
            List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
            if (e.OldItems != null)
            {
                IEnumerator enumerator = e.OldItems.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        if (current is IVirtualizable)
                            virtualizableList.Add(current as IVirtualizable);
                    }
                }
                finally
                {
                    IDisposable disposable = enumerator as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewStartingIndex >= virtualizingPanel.VirtualizableItems.Count)
                    virtualizingPanel.AddItems(itemsToInsert);
                else
                    virtualizingPanel.InsertRemoveItems(e.NewStartingIndex, itemsToInsert, virtualizingPanel.KeepScrollPositionWhenAddingItems, null);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                virtualizingPanel.ClearItems();
                virtualizingPanel.AddItems((IEnumerable<IVirtualizable>)virtualizingPanel.ItemsSource);
            }
            else
            {
                if (e.Action != NotifyCollectionChangedAction.Remove || virtualizableList.Count <= 0)
                    return;
                virtualizingPanel.RemoveItem(virtualizableList[0]);
            }
        }

        public void InsertRemoveItems(int index, List<IVirtualizable> itemsToInsert, bool keepItemsBelowIndexFixed = false, IVirtualizable itemToRemove = null)
        {
            try
            {
                bool flag = false;
                if (keepItemsBelowIndexFixed)
                {
                    double num = 0.0;
                    for (int index1 = 0; index1 < index; ++index1)
                        num += this.VirtualizableItems[index1].FixedHeight ;
                    if (num < this.ScrollViewer.VerticalOffset + this.ScrollViewer.ViewportHeight)
                        flag = true;
                }
                //this._loadedSegment = new Segment();
                double num1 = itemsToInsert.Sum<IVirtualizable>((Func<IVirtualizable, double>)(i => i.FixedHeight ));
               // this.SetResetParent(true, (IEnumerable<IVirtualizable>)itemsToInsert);
                this.VirtualizableItems.InsertRange(index, (IEnumerable<IVirtualizable>)itemsToInsert);
                if (itemToRemove != null)
                {
                    //itemToRemove.ChangeState(VirtualizableState.Unloaded);
                    double num2 = num1;
                    double fixedHeight = itemToRemove.FixedHeight;
                    //Thickness margin = itemToRemove.Margin;
                    //double top = margin.Top;
                    //double num3 = fixedHeight + top;
                    //margin = itemToRemove.Margin;
                    //double bottom = margin.Bottom;
                    //double num4 = num3 + bottom;
                    //num1 = num2 - num4;
                    //this.SetResetParent(false, itemToRemove);
                    this.VirtualizableItems.Remove(itemToRemove);
                }
                //this.RearrangeAllItems();
                if (flag)
                {
                    //this._changingVerticalOffset = true;
                    //this.Log("SCROLLING TO " + this._listScrollViewer.VerticalOffset + (object)num1 + " scroll height : " + (object)this._listScrollViewer.ExtentHeight);
                    this.ScrollViewer.ScrollToVerticalOffset(this.ScrollViewer.VerticalOffset + num1);
                    //this._changingVerticalOffset = false;
                }
                this.PerformLoadUnload(this._isScrolling ? VirtualizableState.LoadedPartially : VirtualizableState.LoadedFully);
            }
            catch
            {
            }
        }

        public void RemoveItem(IVirtualizable itemToBeRemoved)
        {
            //itemToBeRemoved.ChangeState(VirtualizableState.Unloaded);
            //this.SetResetParent(false, itemToBeRemoved);
            this.VirtualizableItems.Remove(itemToBeRemoved);
            //this._loadedSegment = new Segment();
            //this.RearrangeAllItems();
            this.PerformLoadUnload(this._isScrolling ? VirtualizableState.LoadedPartially : VirtualizableState.LoadedFully);
        }

        public void ClearItems()
        {
            List<IVirtualizable>.Enumerator enumerator = this.VirtualizableItems.GetEnumerator();
            try
            {
                //while (enumerator.MoveNext())
                //    enumerator.Current.ChangeState(VirtualizableState.Unloaded);
            }
            finally
            {
                enumerator.Dispose();
            }
            //this.SetResetParent(false, (IEnumerable<IVirtualizable>)this.VirtualizableItems);
            this.VirtualizableItems.Clear();
            this.ClearChildren();
            //this._loadedSegment = new Segment();
            //this._thresholdPointIndexes.Clear();
            this.ScrollViewer.ScrollToVerticalOffset(0.0);
            this.ChangeHeight(0.0);
        }


        private void AddToChildren(UIElement element)
        {
            if (element.CacheMode == null)
                element.CacheMode = new BitmapCache();
            if (this._itemsPanel.Children.Contains(element))
                return;
            /*
            if (element.Projection == null && this._upsideDown)
            {
                PlaneProjection planeProjection = new PlaneProjection();
                planeProjection.RotationZ = 180;
                element.Projection = ((Projection)planeProjection);
            }
            */
            this._itemsPanel.Children.Add(element);
        }

        private void RemoveFromChildren(UIElement element)
        {
            this._itemsPanel.Children.Remove(element);
        }


        private void ClearChildren()
        {
            this._itemsPanel.Children.Clear();
        }

        public void AddItems(IEnumerable<IVirtualizable> _itemsToBeAdded)
        {
            double num1 = 0.0;
            if (this.VirtualizableItems.Count > 0)
                num1 = this.VirtualizableItems.Sum((Func<IVirtualizable, double>)(vi => vi.FixedHeight));
            foreach (IVirtualizable virtualizable1 in _itemsToBeAdded)
            {
                if (virtualizable1 == null)
                    throw new Exception("Can only add virtualizable items.");
                IVirtualizable virtualizable2 = virtualizable1;
                //double left = virtualizable1.Margin.Left;
                //double top = virtualizable1.Margin.Top + num1;
                //Thickness margin = virtualizable1.Margin;
               // double right = margin.Right;
                //margin = virtualizable1.Margin;
                //double bottom = margin.Bottom;
               // Thickness thickness = new Thickness(left, top, right, bottom);
               // virtualizable2.ViewMargin = thickness;
                //this.SetResetParent(true, virtualizable1);
                this.VirtualizableItems.Add(virtualizable1);
                double num2 = virtualizable1.FixedHeight;
                //foreach (int coveredPoint in this.GetCoveredPoints(num1, num1 + num2))
                //    this._thresholdPointIndexes[coveredPoint] = this.VirtualizableItems.Count - 1;
                num1 += num2;
            }
            this.PerformLoadUnload(this._isScrolling ? VirtualizableState.LoadedPartially : VirtualizableState.LoadedFully);
            this.ChangeHeight(num1);
            //this.Log(string.Format("MyVirtualizingPanel.AddItems"));
        }

        public void ChangeHeight(double height)
        {
            base.Height = height;
        }

        private double GetRealOffset()
        {
            return this.ScrollViewer.VerticalOffset;// + this.DeltaOffset;
        }

        private void PerformLoadUnload(VirtualizableState desiredState, bool bypassUnload = false)
        {
            if (this.VirtualizableItems.Count == 0)
                return;
            double realOffset = this.GetRealOffset();
            bool flag1 = false;
            Thickness viewMargin1;
            if (desiredState == VirtualizableState.LoadedFully/* || this._loadedSegment.IsEmpty*/)
            {
                flag1 = true;
            }
            else
            {
                //int lowerBound = this._loadedSegment.LowerBound;
                //int upperBound = this._loadedSegment.UpperBound;
                //Thickness viewMargin2 = this.VirtualizableItems[lowerBound].ViewMargin;

                //double top = ((Thickness)@viewMargin2).Top;
                //viewMargin1 = this.VirtualizableItems[upperBound].ViewMargin;

                double num = 0;// viewMargin1.Top + this.VirtualizableItems[upperBound].FixedHeight;
                if (realOffset  < 500.0 || num - realOffset < 1500.0)
                    flag1 = true;
            }
            if (!flag1)
                return;
            int key = (int)Math.Floor(realOffset - realOffset % this.LoadUnloadThreshold);
            int num1 = /*this._thresholdPointIndexes.ContainsKey(key) ? this._thresholdPointIndexes[key] :*/ -1;
            int upperBoundInd;
            int lowerBoundInd = upperBoundInd = num1 < 0 ? 0 : num1;
            int index1 = lowerBoundInd;
            double num2 = this._isScrolling ? this.LoadedHeightUpwards : this.LoadedHeightDownwardsNotScrolling;
            double num3 = this._isScrolling ? this.LoadedHeightDownwards : this.LoadedHeightDownwardsNotScrolling;
            for (; lowerBoundInd > 0; --lowerBoundInd)
            {
                double num4 = realOffset;
                //viewMargin1 = this.VirtualizableItems[lowerBoundInd].ViewMargin;
                
                double top = ((Thickness)@viewMargin1).Top;
                if (num4 - top >= num2)
                    break;
            }
            bool flag2 = false;
            bool flag3 = false;
            for (; upperBoundInd < this.VirtualizableItems.Count - 1; ++upperBoundInd)
            {
                /*
                viewMargin1 = this.VirtualizableItems[upperBoundInd].ViewMargin;
                
                if (((Thickness)@viewMargin1).Top - realOffset < num3)
                {
                    if (!flag2)
                    {
                        viewMargin1 = this.VirtualizableItems[upperBoundInd].ViewMargin;
                        
                        if (((Thickness)@viewMargin1).Top >= realOffset)
                        {
                            viewMargin1 = this.VirtualizableItems[upperBoundInd].ViewMargin;
                            
                            if (((Thickness)@viewMargin1).Top - realOffset > 300.0 && upperBoundInd > 0)
                            {
                                index1 = upperBoundInd - 1;
                                flag3 = true;
                            }
                            else
                                index1 = upperBoundInd;
                            flag2 = true;
                        }
                    }
                    
                }
                else
                {
                    break;
                }
                */
            }
 //           this.SetLoadedBounds(lowerBoundInd, upperBoundInd, desiredState, bypassUnload);
            if (flag2)
            {
                //if (flag3)
                //    this.VirtualizableItems[index1 + 1].IsOnScreen();
                //this.VirtualizableItems[index1].IsOnScreen();
            }
            /*
            if (!this._enableLog)
                return;
            string str = "Loaded indexes : ";
            for (int index2 = 0; index2 < this.VirtualizableItems.Count; ++index2)
            {
                if (this.VirtualizableItems[index2].CurrentState != VirtualizableState.Unloaded)
                    str = string.Concat(str, index2, ",");
            }
            this.Log(str);
            */
        }

        public enum VirtualizableState
        {
            Unloaded,
            LoadedPartially,
            LoadedFully,
        }
    }
}
