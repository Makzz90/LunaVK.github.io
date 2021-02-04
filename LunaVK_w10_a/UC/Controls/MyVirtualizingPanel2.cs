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
    public class MyVirtualizingPanel2 : Canvas
    {
        public List<IVirtualizable> VirtualizableItems { get; private set; }
        private bool _isEditing;
        private ScrollViewer _listScrollViewer;
        public bool KeepScrollPositionWhenAddingItems;
        public Func<object, IVirtualizable> CreateVirtItemFunc { get; set; }
        public double LoadUnloadThreshold = 500.0;
        public double LoadedHeightUpwards = 500.0;
        public double LoadedHeightUpwardsNotScrolling = 500.0;
        public double LoadedHeightDownwards = 1200.0;
        public double LoadedHeightDownwardsNotScrolling = 1200.0;
        private Dictionary<int, int> _thresholdPointIndexes = new Dictionary<int, int>();
        private bool _isScrolling;
        public double DeltaOffset;
        private Canvas _itemsPanel = new Canvas();

        public MyVirtualizingPanel2()
        {
            this._listScrollViewer = new ScrollViewer();
            this._listScrollViewer.Content=this._itemsPanel;
            base.Children.Add(this._listScrollViewer);
            this.VirtualizableItems = new List<IVirtualizable>();
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IList), typeof(MyVirtualizingPanel2), new PropertyMetadata(new PropertyChangedCallback(OnItemsSourcePropertyChanged)));
        public IList ItemsSource
        {
            get { return (IList)base.GetValue(MyVirtualizingPanel2.ItemsSourceProperty); }
            set { base.SetValue(MyVirtualizingPanel2.ItemsSourceProperty, value); }
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MyVirtualizingPanel2 virtualizingPanel2 = d as MyVirtualizingPanel2;
            if (virtualizingPanel2 == null)
                return;
            virtualizingPanel2.ClearItems();
            
            INotifyCollectionChanged oldValue = e.OldValue as INotifyCollectionChanged;
            if (oldValue != null)
                virtualizingPanel2.UnhookCollectionChanged(oldValue);
            
            INotifyCollectionChanged newValue = e.NewValue as INotifyCollectionChanged;
            if (newValue != null)
            {
                List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
                IEnumerator enumerator = (newValue as ICollection).GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        if (current is IVirtualizable)
                            virtualizableList.Add(current as IVirtualizable);
                        //else if (virtualizingPanel2.CreateVirtItemFunc != null)
                        //    virtualizableList.Add(virtualizingPanel2.CreateVirtItemFunc(current));
                    }
                }
                finally
                {
                    IDisposable disposable = enumerator as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
                virtualizingPanel2.AddItems(virtualizableList);
                virtualizingPanel2.HookUpCollectionChanged(newValue);
                virtualizingPanel2.SubscribeOnEdit();
            }
            else
                virtualizingPanel2.UnsubscribeOnEdit();
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
            //this.SetResetParent(false, this.VirtualizableItems);
            this.VirtualizableItems.Clear();
            //this.ClearChildren();
            //this._loadedSegment = new Segment();
            //this._thresholdPointIndexes.Clear();
            if (this._listScrollViewer != null)
                this._listScrollViewer.ScrollToVerticalOffset(0.0);
            this.ChangeHeight(0.0);
        }

        public void HookUpCollectionChanged(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += this.collection_CollectionChanged;
        }

        public void UnhookCollectionChanged(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= this.collection_CollectionChanged;
        }


        private void UnsubscribeOnEdit()
        {
            /*
            if (this._editable == null)
                return;
            this._editable.StartedEdit -= new EventHandler(this.editable_StartedEdit);
            this._editable.EndedEdit -= new EventHandler(this.editable_EndedEdit);
            this._editable = null;
            */
        }

        private void SubscribeOnEdit()
        {
            /*
            ISupportCollectionEdit dataContext = base.DataContext as ISupportCollectionEdit;
            if (dataContext == null || this._editable == dataContext)
                return;
            this.UnsubscribeOnEdit();
            dataContext.StartedEdit += new EventHandler(this.editable_StartedEdit);
            dataContext.EndedEdit += new EventHandler(this.editable_EndedEdit);
            this._editable = dataContext;
            */
        }

        private void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            MyVirtualizingPanel2 virtualizingPanel2 = this;
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
                        //else if (this.CreateVirtItemFunc != null)
                        //    itemsToInsert.Add(this.CreateVirtItemFunc(current));
                    }
                }
                finally
                {
                    IDisposable disposable = enumerator as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
            List<IVirtualizable> virtualizableList1 = new List<IVirtualizable>();
            if (e.OldItems != null)
            {
                IEnumerator enumerator = e.OldItems.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        if (current is IVirtualizable)
                            virtualizableList1.Add(current as IVirtualizable);
                        //else if (e.OldStartingIndex >= 0 && e.OldStartingIndex < virtualizingPanel2.VirtualizableItems.Count)
                        //{
                        //    IVirtualizable virtualizableItem = virtualizingPanel2.VirtualizableItems[e.OldStartingIndex];
                        //    virtualizingPanel2.RemoveItem(virtualizableItem);
                        //}
                    }
                }
                finally
                {
                    IDisposable disposable = enumerator as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
            //if (this._isEditing)
            //    this._addedWhileEdited.AddRange(itemsToInsert);
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewStartingIndex >= virtualizingPanel2.VirtualizableItems.Count)
                    virtualizingPanel2.AddItems(itemsToInsert);
                else
                    virtualizingPanel2.InsertRemoveItems(e.NewStartingIndex, itemsToInsert, virtualizingPanel2.KeepScrollPositionWhenAddingItems, null);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                virtualizingPanel2.ClearItems();
                List<IVirtualizable> virtualizableList2 = new List<IVirtualizable>();
                IEnumerator enumerator = virtualizingPanel2.ItemsSource.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        virtualizableList2.Add(current is IVirtualizable ? current as IVirtualizable : this.CreateVirtItemFunc(current));
                    }
                }
                finally
                {
                    IDisposable disposable = enumerator as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
                virtualizingPanel2.AddItems(virtualizableList2);
            }
            else
            {
                if (e.Action != NotifyCollectionChangedAction.Remove || virtualizableList1.Count <= 0)
                    return;
                virtualizingPanel2.RemoveItem(virtualizableList1[0]);
            }
        }

        public void RemoveItem(IVirtualizable itemToBeRemoved)
        {
            /*
            itemToBeRemoved.ChangeState(VirtualizableState.Unloaded);
            this.SetResetParent(false, itemToBeRemoved);
            this.VirtualizableItems.Remove(itemToBeRemoved);
            this._loadedSegment = new Segment();
            this.RearrangeAllItems(); */
            this.PerformLoadUnload(this._isScrolling ? VirtualizableState.LoadedPartially : VirtualizableState.LoadedFully);
           
        }

        public void AddItems(IEnumerable<IVirtualizable> _itemsToBeAdded)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            double num1 = 0.0;
            if (this.VirtualizableItems.Count > 0)
                num1 = Enumerable.Sum<IVirtualizable>(this.VirtualizableItems, (Func<IVirtualizable, double>)(vi =>
                {
                    double fixedHeight = vi.FixedHeight;
                    Thickness margin1 = new Thickness();//vi.Margin;
                    
                    double top = margin1.Top;
                    double num2 = fixedHeight + top;
                    Thickness margin2 = new Thickness();//vi.Margin;
                    
                    double bottom = ((Thickness)@margin2).Bottom;
                    return num2 + bottom;
                }));
            IEnumerator<IVirtualizable> enumerator1 = _itemsToBeAdded.GetEnumerator();
            try
            {
                while (enumerator1.MoveNext())
                {
                    IVirtualizable current = enumerator1.Current;
                    if (current == null)
                        throw new Exception("Can only add virtualizable items.");
                    IVirtualizable virtualizable = current;
                    
                    //virtualizable.ViewMargin = thickness;
                    //this.SetResetParent(true, current);
                    this.VirtualizableItems.Add(current);
                    double fixedHeight = current.FixedHeight;
                    //Thickness margin4 = current.Margin;
                    
                    
                    double num3 = fixedHeight ;
                   // Thickness margin5 = current.Margin;
                    
                    double num4 = num3 ;
                    List<int>.Enumerator enumerator2 = this.GetCoveredPoints(num1, num1 + num4).GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                            this._thresholdPointIndexes[enumerator2.Current] = this.VirtualizableItems.Count - 1;
                    }
                    finally
                    {
                        enumerator2.Dispose();
                    }
                    num1 += num4;
                }
            }
            finally
            {
                if (enumerator1 != null)
                    enumerator1.Dispose();
            }
            this.PerformLoadUnload(this._isScrolling ? VirtualizableState.LoadedPartially : VirtualizableState.LoadedFully);
            this.ChangeHeight(num1);
            //stopwatch.Stop();
            //this.Log(string.Format("MyVirtualizingPanel2.AddItems {0}", stopwatch.ElapsedMilliseconds));
        }

        public void InsertRemoveItems(int index, List<IVirtualizable> itemsToInsert, bool keepItemsBelowIndexFixed = false, IVirtualizable itemToRemove = null)
        {
            try
            {
                bool flag = false;
                if (keepItemsBelowIndexFixed)
                {
                    double num1 = 0.0;
                    for (int index1 = 0; index1 < index; ++index1)
                    {
                        double num2 = num1;
                        double fixedHeight = this.VirtualizableItems[index1].FixedHeight;
                        Thickness margin1 = new Thickness();//this.VirtualizableItems[index1].Margin;
                        
                        double top = margin1.Top;
                        double num3 = fixedHeight + top;
                        //Thickness margin2 = this.VirtualizableItems[index1].Margin;
                        
                        double bottom = margin1.Bottom;
                        double num4 = num3 + bottom;
                        num1 = num2 + num4;
                    }
                    if (num1 < this._listScrollViewer.VerticalOffset + this._listScrollViewer.ViewportHeight)
                        flag = true;
                }
                //this._loadedSegment = new Segment();
                double num5 = Enumerable.Sum<IVirtualizable>(itemsToInsert, (Func<IVirtualizable, double>)(i =>
                {
                    double fixedHeight = i.FixedHeight;
                    Thickness margin1 = new Thickness();//i.Margin;
                    
                    double top = margin1.Top;
                    double num = fixedHeight + top;
                    //Thickness margin2 = i.Margin;
                    
                    double bottom = margin1.Bottom;
                    return num + bottom;
                }));
                //this.SetResetParent(true, itemsToInsert);
                this.VirtualizableItems.InsertRange(index, itemsToInsert);
                if (itemToRemove != null)
                {
                    //itemToRemove.ChangeState(VirtualizableState.Unloaded);
                    double num1 = num5;
                    double fixedHeight = itemToRemove.FixedHeight;
                    Thickness margin = new Thickness();//itemToRemove.Margin;
                    
                    double top = margin.Top;
                    double num2 = fixedHeight + top;
                    margin = new Thickness();//itemToRemove.Margin;
                    
                    double bottom = margin.Bottom;
                    double num3 = num2 + bottom;
                    num5 = num1 - num3;
                    //this.SetResetParent(false, itemToRemove);
                    this.VirtualizableItems.Remove(itemToRemove);
                }
                this.RearrangeAllItems();
                if (flag)
                {
                    //this._changingVerticalOffset = true;
                    this.Log(string.Concat("SCROLLING TO ",this._listScrollViewer.VerticalOffset, num5, " scroll height : ", this._listScrollViewer.ExtentHeight ));
                    this._listScrollViewer.ScrollToVerticalOffset(this._listScrollViewer.VerticalOffset + num5);
                    //this._changingVerticalOffset = false;
                }
                this.PerformLoadUnload(this._isScrolling ? VirtualizableState.LoadedPartially : VirtualizableState.LoadedFully);
            }
            catch (Exception)
            {
            }
        }

        public void RearrangeAllItems()
        {
            double num1 = 0.0;
            this._thresholdPointIndexes.Clear();
            int num2 = 0;
            List<IVirtualizable>.Enumerator enumerator1 = this.VirtualizableItems.GetEnumerator();
            try
            {
                while (enumerator1.MoveNext())
                {
                    IVirtualizable current = enumerator1.Current;
                    if (current == null)
                        throw new Exception("Can only add virtualizable items.");
                    IVirtualizable virtualizable = current;
                    Thickness margin = new Thickness();//current.Margin;
                    
                    double left = margin.Left;
                    //margin = current.Margin;
                    
                    double num3 = margin.Top + num1;
                    //margin = current.Margin;
                    
                    double right = margin.Right;
                    //margin = current.Margin;
                    
                    double bottom1 = margin.Bottom;
                    Thickness thickness = new Thickness(left, num3, right, bottom1);
                    //virtualizable.ViewMargin = thickness;
                    double fixedHeight = current.FixedHeight;
                    //margin = current.Margin;
                    
                    double top = margin.Top;
                    double num4 = fixedHeight + top;
                    //margin = current.Margin;
                    
                    double bottom2 = margin.Bottom;
                    double num5 = num4 + bottom2;
                    List<int>.Enumerator enumerator2 = this.GetCoveredPoints(num1, num1 + num5).GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                            this._thresholdPointIndexes[enumerator2.Current] = num2;
                    }
                    finally
                    {
                        enumerator2.Dispose();
                    }
                    num1 += num5;
                    ++num2;
                }
            }
            finally
            {
                enumerator1.Dispose();
            }
            this.ChangeHeight(num1);
            this._listScrollViewer.UpdateLayout();
        }

        private double GetRealOffset()
        {
            return this._listScrollViewer.VerticalOffset + this.DeltaOffset;
        }

        private void PerformLoadUnload(VirtualizableState desiredState, bool bypassUnload = false)
        {
            if (this.VirtualizableItems.Count == 0)
                return;
            double realOffset = this.GetRealOffset();
            bool flag1 = false;
            Thickness viewMargin1;
            if (desiredState == VirtualizableState.LoadedFully /*|| this._loadedSegment.IsEmpty*/)
            {
                flag1 = true;
            }
            else
            {
                /*
                int lowerBound = this._loadedSegment.LowerBound;
                int upperBound = this._loadedSegment.UpperBound;
                Thickness viewMargin2 = this.VirtualizableItems[lowerBound].ViewMargin;
                
                double top = ((Thickness)@viewMargin2).Top;
                viewMargin1 = this.VirtualizableItems[upperBound].ViewMargin;
                
                double num = ((Thickness)@viewMargin1).Top + this.VirtualizableItems[upperBound].FixedHeight;
                if (realOffset - top < 500.0 || num - realOffset < 1500.0)
                    flag1 = true;
                    */
            }
            if (flag1)
            {
                int key = (int)Math.Floor(realOffset - realOffset % this.LoadUnloadThreshold);
                int num1 = this._thresholdPointIndexes.ContainsKey(key) ? this._thresholdPointIndexes[key] : -1;
                int upperBoundInd;
                int lowerBoundInd = upperBoundInd = num1 < 0 ? 0 : num1;
                int index1 = lowerBoundInd;
                double num2 = this._isScrolling ? this.LoadedHeightUpwards : this.LoadedHeightDownwardsNotScrolling;
                double num3 = this._isScrolling ? this.LoadedHeightDownwards : this.LoadedHeightDownwardsNotScrolling;
                for (; lowerBoundInd > 0; --lowerBoundInd)
                {
                    double num4 = realOffset;
                    //viewMargin1 = this.VirtualizableItems[lowerBoundInd].ViewMargin;
                    
                    double top = viewMargin1.Top;
                    if (num4 - top >= num2)
                        break;
                }
                bool flag2 = false;
                bool flag3 = false;
                for (; upperBoundInd < this.VirtualizableItems.Count - 1; ++upperBoundInd)
                {
                    //viewMargin1 = this.VirtualizableItems[upperBoundInd].ViewMargin;
                    
                    if (((Thickness)@viewMargin1).Top - realOffset < num3)
                    {
                        if (!flag2)
                        {
                            //viewMargin1 = this.VirtualizableItems[upperBoundInd].ViewMargin;
                            
                            if (((Thickness)@viewMargin1).Top >= realOffset)
                            {
                                //viewMargin1 = this.VirtualizableItems[upperBoundInd].ViewMargin;
                                
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
                        break;
                }
                //this.SetLoadedBounds(lowerBoundInd, upperBoundInd, desiredState, bypassUnload);
                if (flag2)
                {
                    //if (flag3)
                    //    this.VirtualizableItems[index1 + 1].IsOnScreen();
                    //this.VirtualizableItems[index1].IsOnScreen();
                }
                //if (this._enableLog)
                //{
                    string str = "Loaded indexes : ";
                    for (int index2 = 0; index2 < this.VirtualizableItems.Count; ++index2)
                    {
                        //if (this.VirtualizableItems[index2].CurrentState != VirtualizableState.Unloaded)
                            str = string.Concat(str, index2, ",");
                    }
                    this.Log(str);
               // }
            }
            //this.TrackImpressions();
            //this.TrackItemsPosition();
        }
        /*
        private void TrackImpressions()
        {
            double verticalOffset = this._listScrollViewer.VerticalOffset;
            double num1 = verticalOffset + this._listScrollViewer.ViewportHeight;
            for (int lowerBound = this._loadedSegment.LowerBound; lowerBound <= this._loadedSegment.UpperBound; ++lowerBound)
            {
                ISupportImpressionTracking virtualizableItem = this.VirtualizableItems[lowerBound] as ISupportImpressionTracking;
                if (virtualizableItem != null)
                {
                    Thickness viewMargin = this.VirtualizableItems[lowerBound].ViewMargin;
                    
                    double num2 = ((Thickness)@viewMargin).Top + this.OffsetY - this.ExtraOffsetY;
                    double num3 = num2 + this.VirtualizableItems[lowerBound].FixedHeight;
                    if (num2 >= verticalOffset && num2 <= num1)
                        virtualizableItem.TopIsOnScreen();
                    if (num3 >= verticalOffset && num3 <= num1)
                        virtualizableItem.BottomIsOnScreen();
                }
            }
        }

        private void TrackItemsPosition()
        {
            double num = this.OffsetY - this.ExtraOffsetY;
            Rect bounds1 = new Rect(0.0, this.ScrollViewer.VerticalOffset, 0.0, Math.Max(0.0, this.ScrollViewer.ViewportHeight - this.ExtraOffsetY));
            for (int lowerBound = this._loadedSegment.LowerBound; lowerBound <= this._loadedSegment.UpperBound; ++lowerBound)
            {
                VirtualizableItemBase virtualizableItem1 = this.VirtualizableItems[lowerBound] as VirtualizableItemBase;
                if (virtualizableItem1 != null)
                {
                    Thickness viewMargin = this.VirtualizableItems[lowerBound].ViewMargin;
                    
                    double offset1 = ((Thickness)@viewMargin).Top + num;
                    ISupportPositionTracking virtualizableItem2 = this.VirtualizableItems[lowerBound] as ISupportPositionTracking;
                    if (virtualizableItem2 != null)
                    {
                        Rect bounds2 = bounds1;
                        double offset2 = offset1;
                        virtualizableItem2.TrackPositionChanged(bounds2, offset2);
                    }
                    this.TrackItemChildrenPosition(virtualizableItem1, bounds1, offset1, "-");
                }
            }
        }

        private void TrackItemChildrenPosition(VirtualizableItemBase parent, Rect bounds, double offset, string tag)
        {
            ObservableCollection<IVirtualizable> observableCollection = parent != null ? parent.VirtualizableChildren : null;
            if (observableCollection == null || ((Collection<IVirtualizable>)observableCollection).Count == 0)
                return;
            IEnumerator<VirtualizableItemBase> enumerator = ((IEnumerable<VirtualizableItemBase>)Enumerable.OfType<VirtualizableItemBase>((IEnumerable)observableCollection)).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    VirtualizableItemBase current = enumerator.Current;
                    double num = offset;
                    Thickness viewMargin = current.ViewMargin;
                    
                    double top = ((Thickness)@viewMargin).Top;
                    double offset1 = num + top;
                    ISupportPositionTracking positionTracking = current as ISupportPositionTracking;
                    if (positionTracking != null)
                    {
                        Rect bounds1 = bounds;
                        double offset2 = offset1;
                        positionTracking.TrackPositionChanged(bounds1, offset2);
                    }
                    this.TrackItemChildrenPosition(current, bounds, offset1, string.Concat(tag, "-"));
                }
            }
            finally
            {
                if (enumerator != null)
                    enumerator.Dispose();
            }
        }
        */
        public void ChangeHeight(double height)
        {
            base.Height = height;
        }

        private void Log(string str)
        {
            
        }

        private List<int> GetCoveredPoints(double from, double to)
        {
            List<int> intList = new List<int>();
            double d = from - from % this.LoadUnloadThreshold;
            while (d <= to)
            {
                if (d >= from)
                    intList.Add((int)Math.Floor(d));
                d += this.LoadUnloadThreshold;
            }
            return intList;
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
                UIElement uiElement = element;
                PlaneProjection planeProjection = new PlaneProjection();
                double num = 180.0;
                planeProjection.RotationZ = num;
                uiElement.Projection = ((Projection)planeProjection);
            }
            */
           this._itemsPanel.Children.Add(element);
        }

        private void RemoveFromChildren(UIElement element)
        {
            this._itemsPanel.Children.Remove(element);
        }

        public enum VirtualizableState
        {
            Unloaded,
            LoadedPartially,
            LoadedFully,
        }
    }
}
