using LunaVK.Core.Utils;
using LunaVK.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace LunaVK.UC.Controls
{
    //[TemplatePart(Name = "_lv", Type = typeof(FrameworkElement))]
    public partial class SwipeThroughControl : Control
    {
        private List<FrameworkElement> _elements;
        private Grid LayoutRoot;
        private int _selectedIndex = -1;
        private bool _isAnimating;
        public event TypedEventHandler<SwipeThroughControl, int> SelectionChanged;
        private readonly EasingFunctionBase ANIMATION_EASING;
//        internal ItemsControl ItemsControlFooter;
        internal ScrollViewer filtersScrollViewer;
        public event EventHandler ItemsCleared;
        private ListView lv;

        public double NextElementMargin { get; set; }
        /*
#region FooterItems
        public static readonly DependencyProperty FooterItemsProperty = DependencyProperty.Register("FooterItems", typeof(IList), typeof(SwipeThroughControl), new PropertyMetadata(null));
        public IList FooterItems
        {
            get { return (IList)base.GetValue(SwipeThroughControl.FooterItemsProperty); }
            set { base.SetValue(SwipeThroughControl.FooterItemsProperty, value); }
        }

        private static void ItemsSource_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SwipeThroughControl slideView = (SwipeThroughControl)d;
            slideView.UpdateSources(true,false,false);
            // ISSUE: explicit reference operation
            if (e.NewValue != null)
                slideView.SelectedIndex = 0;
            //slideView.StartAutoSlide();
        }
#endregion
        */
#region FooterItemTemplate
        public static readonly DependencyProperty FooterItemTemplateProperty = DependencyProperty.Register("FooterItemTemplate", typeof(DataTemplate), typeof(SwipeThroughControl), new PropertyMetadata(null));
        public DataTemplate FooterItemTemplate
        {
            get { return (DataTemplate)base.GetValue(SwipeThroughControl.FooterItemTemplateProperty); }
            set { base.SetValue(SwipeThroughControl.FooterItemTemplateProperty, value); }
        }
#endregion

#region ItemTemplate
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(SwipeThroughControl), new PropertyMetadata(null));
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)base.GetValue(SwipeThroughControl.ItemTemplateProperty); }
            set { base.SetValue(SwipeThroughControl.ItemTemplateProperty, value); }
        }
#endregion

#region FooterBackground
        /// <summary>
        /// Gets or sets the Brush to apply to the background of the list area of the control.
        /// </summary>
        /// <returns>The Brush to apply to the background of the list area of the control.</returns>
        public Brush FooterBackground
        {
            get { return (Brush)GetValue(FooterBackgroundProperty); }
            set { SetValue(FooterBackgroundProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="MasterPaneBackground"/> dependency property.
        /// </summary>
        /// <returns>The identifier for the <see cref="MasterPaneBackground"/> dependency property.</returns>
        public static readonly DependencyProperty FooterBackgroundProperty = DependencyProperty.Register( nameof(FooterBackground), typeof(Brush), typeof(SwipeThroughControl), new PropertyMetadata(null));
#endregion

        //public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(SwipeThroughControl), new PropertyMetadata(-1));

        public SwipeThroughControl()
        {
            this.ANIMATION_EASING = new CubicEase() { EasingMode = EasingMode.EaseOut };
//            this.FooterItems = new ObservableCollection<object>();
        }
        
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            this.LayoutRoot = this.GetTemplateChild("_layoutRoot") as Grid;
//            this.ItemsControlFooter = this.GetTemplateChild("_itemsControlFooter") as ItemsControl;
//            this.filtersScrollViewer = this.GetTemplateChild("_filtersScrollViewer") as ScrollViewer;
this.lv = this.GetTemplateChild("_lv") as ListView;
            this.lv.SelectionChanged += Lv_SelectionChanged;
            
 //           this._items = new ObservableCollection<object>();

            this.LayoutRoot.Loaded += LayoutRoot_Loaded;

            //this.EnsureElements();
            //this.SelectedIndex = 0;


            

//            this.ItemsControlFooter.Tapped += ItemsControlFooter_Tapped;
 //           this.DataContext = this;
            //DataContextChanged += SwipeThroughControl_DataContextChanged;

            var _settingTab = this.GetTemplateChild("_settingTab") as FrameworkElement;
            _settingTab.Tapped += _settingTab_Tapped;
        }

        private void _settingTab_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToStickersManage();
        }

        private void Lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;

            if (listView.SelectedItem == null)
                return;
            
            this.SelectedIndex = listView.SelectedIndex;

            var item = listView.SelectedItem;

            // Calculations relative to screen or ListView
            var listViewItem = (FrameworkElement)listView.ContainerFromItem(item);
            //
            if (listViewItem == null)///todo:не должно быть так
                return;
            //
            var topLeft = listViewItem .TransformToVisual(listView).TransformPoint(new Point()).X;
            var lvih = listViewItem.ActualWidth;
            var lvh = listView.ActualWidth;
            var desiredTopLeft = (lvh - lvih) / 2.0;
            var desiredDelta = topLeft - desiredTopLeft;

            // Calculations relative to the ScrollViewer within the ListView
            var scrollViewer = listView.GetScrollViewer();
            var currentOffset = scrollViewer.HorizontalOffset;
            var desiredOffset = currentOffset + desiredDelta;
            scrollViewer.ChangeView(desiredOffset, 0, 1.0f, false);

        }

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(IList), typeof(SwipeThroughControl), new PropertyMetadata(null, new PropertyChangedCallback(ItemsChangedCallback) ));

        /*
        private ObservableCollection<object> _items;
        public ObservableCollection<object> Items
        {
            get
            {
                return this._items;
            }
            set
            {
                if (this._items != null)
                    this._items.CollectionChanged -= this.ItemsOnCollectionChanged;
                this._items = value;
                if (this._items != null)
                    this._items.CollectionChanged += this.ItemsOnCollectionChanged;
                //this.OnPropertyChanged("Items");
                this.EnsureElements();
                this.ArrangeElements();
                this.SelectedIndex = 0;
            }
        }
        */
        public IList Items
        {
            get { return (IList)base.GetValue(ItemsProperty); }
            set { base.SetValue(ItemsProperty, value); }
        }

        private static void ItemsChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null)
                return;

            if (args.NewValue == args.OldValue)
                return;

            var parent = dependencyObject as SwipeThroughControl;

            //if (parent == null)
            //    return;

            var obsList = args.NewValue as INotifyCollectionChanged;
            if (obsList != null)
            {
                obsList.CollectionChanged += parent.ItemsOnCollectionChanged;
                parent.EnsureElements();
                parent.ArrangeElements();
                parent.SelectedIndex = 0;
            }
            }

            private void ItemsOnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(this.lv.Items.Count > 0 && this.lv.SelectedIndex == -1)
                this.lv.SelectedIndex = 0;



            this.UpdateSources(true, true, true);
            if (this.SelectedIndex >= this.Items.Count)
                this.SelectedIndex = this.Items.Count - 1;
            if (this.Items.Count != 0 || this.ItemsCleared == null)
                return;
            this.ItemsCleared(this, EventArgs.Empty);
        }

        private void SwipeThroughControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            this.UpdateSources(false, true, true);
            DataContextChanged -= SwipeThroughControl_DataContextChanged;
        }

        private FrameworkElement CurrentElement
        {
            get { return this._elements[1]; }
        }

        public int SelectedIndex
        {
            get
            {
                return this._selectedIndex;
            }
            set
            {
                
                if (this._selectedIndex == value)
                    return;

                int selectedIndex = this._selectedIndex;
                //
                if (selectedIndex == -1)
                    selectedIndex = 0;
                //
                this._selectedIndex = value;
                //this.OnPropertyChanged("SelectedIndex");
                this.UpdateIsSelected();

                System.Diagnostics.Debug.WriteLine("_selectedIndex - selectedIndex " + (this._selectedIndex - selectedIndex));

                /*
                if (this._selectedIndex - selectedIndex == 2)
                {
                    //this.Swap(0, 2);
                    //this.UpdateSources(false, true, true);
                    //this.ArrangeElements();

                    
                    this._selectedIndex = value - 1;
                    this.UpdateSources(false, false, true);
                    (this._elements[1].RenderTransform as TranslateTransform).X = -1;
                    this.HandleDragCompleted(-80);
                }
                else if (selectedIndex - this._selectedIndex == 2)
                {
                    //this.Swap(0, 2);
                    //this.UpdateSources(true, true, false);
                    //this.ArrangeElements();

                    this._selectedIndex = value + 1;
                    this.UpdateSources(true, false, false);
                    (this._elements[1].RenderTransform as TranslateTransform).X = 1;
                    this.HandleDragCompleted(80);
                }
                else if (this._selectedIndex - selectedIndex == 1)//вперёд
                {
                    //this.MoveToNextOrPrevious(true);
                    //this.ArrangeElements();
                    this._selectedIndex = value - 1;
                    this.UpdateSources(false, false, true);
                    (this._elements[1].RenderTransform as TranslateTransform).X = -1;
                    this.HandleDragCompleted(-80);
                }
                else if (selectedIndex - this._selectedIndex == 1)//назад
                {
                    //this.MoveToNextOrPrevious(false);
                    //this.ArrangeElements();
                    this._selectedIndex = value + 1;
                    this.UpdateSources(true, false, false);
                    (this._elements[1].RenderTransform as TranslateTransform).X = 1;
                    this.HandleDragCompleted(80);
                }
                else
                    this.UpdateSources(false, new bool?());
                */
                if (this._selectedIndex - selectedIndex > 0)//вперёд
                {
                    this._selectedIndex = value - 1;
                    this.UpdateSources(false, false, true);
                    (this._elements[1].RenderTransform as TranslateTransform).X = -1;
                    this.HandleDragCompleted(-80, ()=> { this.UpdateSources(true, false, true); });
                }
                else if(this._selectedIndex - selectedIndex < 0)//назад
                {
                    this._selectedIndex = value + 1;
                    this.UpdateSources(true, false, false);
                    (this._elements[1].RenderTransform as TranslateTransform).X = 1;
                    this.HandleDragCompleted(80, () => { this.UpdateSources(true, false, true); } );
                }
                else
                {
                    this.UpdateSources(false, new bool?());
                    //this.UpdateIsSelected();
                }
            }
        }

        private void UpdateSources(bool update0, bool update1, bool update2)
        {
            //
            if (this._elements == null)
                return;
            //
            if (update0)
                this.SetDataContext(this._elements[0], this.GetItem(this._selectedIndex - 1));
            if (update1)
                this.SetDataContext(this._elements[1], this.GetItem(this._selectedIndex));
            if (update2)
                this.SetDataContext(this._elements[2], this.GetItem(this._selectedIndex + 1));
//            this.SetActiveState(this._elements[0], false);
//            this.SetActiveState(this._elements[1], true);
//            this.SetActiveState(this._elements[2], false);
        }

        private void UpdateSources(bool keepCurrentAsIs = false, bool? movedForvard = null)
        {
            //
            if (this._elements == null)
                return;
            //
            if (!keepCurrentAsIs && !movedForvard.HasValue)
                this.SetDataContext(this._elements[1], this.GetItem(this._selectedIndex));
            if (!movedForvard.HasValue ? true : (!movedForvard.Value ? true : false))
                this.SetDataContext(this._elements[0], this.GetItem(this._selectedIndex - 1));
            if (!movedForvard.HasValue ? true : (movedForvard.Value ? true : false))
                this.SetDataContext(this._elements[2], this.GetItem(this._selectedIndex + 1));
//            this.SetActiveState(this._elements[0], false);
//            this.SetActiveState(this._elements[1], true);
//            this.SetActiveState(this._elements[2], false);
        }
        /*
        private void SetActiveState(FrameworkElement frameworkElement, bool isActive)
        {
            //ISupportState supportState = frameworkElement as ISupportState;
            //if (supportState == null)
            //    return;
            //supportState.SetState(isActive);
            //VisualStateManager.GoToState(frameworkElement, "Selected", true);
        }
        */

            /// <summary>
            /// Меняем элементы местами, применяем данные
            /// </summary>
            /// <param name="next"></param>
        private void MoveToNextOrPrevious(bool next)
        {
            if (next)
            {
                this.Swap(0, 1);
                this.Swap(1, 2);
            }
            else
            {
                this.Swap(1, 2);
                this.Swap(0, 1);
            }
            this.UpdateSources(false, new bool?(next));
        }

        private object GetItem(int ind)
        {
            if (ind < 0 || ind >= this.Items.Count)
                return null;
            return this.Items[ind];
        }

        private void SetDataContext(FrameworkElement frameworkElement, object dc)
        {
            //ISupportDataContext supportDataContext = frameworkElement as ISupportDataContext;
            //if (supportDataContext != null)
            //    supportDataContext.SetDataContext(dc);
            //else
            //    frameworkElement.DataContext = dc;
            ((frameworkElement as Border).Child as ContentPresenter).Content = dc;
        }

        private void UpdateIsSelected()
        {
            if (this.Items == null)
                return;
            
            if(this.lv !=null && this.lv.Items.Count>0)
                this.lv.SelectedIndex = this.SelectedIndex;
//            this.ScrollToIndex(this.SelectedIndex, null);
        }

        /// <summary>
        /// Поменять местами два элемента
        /// </summary>
        /// <param name="ind1"></param>
        /// <param name="ind2"></param>
        private void Swap(int ind1, int ind2)
        {
            List<FrameworkElement> elements = this._elements;
            FrameworkElement element = elements[ind1];
            elements[ind1] = elements[ind2];
            elements[ind2] = element;
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

            //this.ArrangeElements();
            //this.SelectedIndex = 0;

            //FrameworkElement fr = sender as FrameworkElement;
            this.LayoutRoot.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, base.ActualWidth, base.ActualHeight) };
            this.SizeChanged += OnSizeChanged;
        }

        private double ArrangeWidth
        {
            get { return this.ActualWidth - this.NextElementMargin; }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //

            //FrameworkElement fr = sender as FrameworkElement;
            this.LayoutRoot.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };
            
            if (this._elements == null)
                return;
            //
            this.ArrangeElements();
            //this.ScrollToIndex(this.SelectedIndex, null);
        }

        private void ScrollToIndex(int p, Action callback)
        {
            if (this.filtersScrollViewer == null)
            {
                this.filtersScrollViewer = this.lv.GetScrollViewer();
                return;
            }
            //
            //
            if (this.filtersScrollViewer.ActualWidth == 0.0 || this.Items.Count == 0)
                return;
            double svWidth = this.filtersScrollViewer.ActualWidth;
            double icWidth = this.filtersScrollViewer.ScrollableWidth * 2;//this.ItemsControlFooter.ActualWidth;
            double item_width = icWidth / this.Items.Count;

 //           this.ScrollToOffset(Math.Min(Math.Max((p * item_width) + (item_width/2) - svWidth / 2, 0), this.filtersScrollViewer.ScrollableWidth), callback);
        }

        private void ScrollToOffset(double to, Action callback)
        {
            this.filtersScrollViewer.ChangeView(to,0,1.0f,false);
        }

        /// <summary>
        /// Сместить элементы
        /// </summary>
        private void ArrangeElements()
        {
            if (this._elements == null)
                return;

            double num = this.SelectedIndex != 0 ? (this.SelectedIndex != this.Items.Count - 1 ? 0 : this.NextElementMargin) : 0;
            (this._elements[0].RenderTransform as TranslateTransform).X = -this.ArrangeWidth + num;
            (this._elements[1].RenderTransform as TranslateTransform).X = num;
            (this._elements[2].RenderTransform as TranslateTransform).X = this.ArrangeWidth + num;
        }

        private void EnsureElements()
        {
            //
            if (this.LayoutRoot == null)
                return;
            //

            if (this._elements != null)
                return;
            this._elements = new List<FrameworkElement>(3);
            
            this.LayoutRoot.Children.Clear();
            for (int index = 0; index < 3; ++index)
            {
                Border border1 = new Border();

                //border1.BorderBrush = new SolidColorBrush(Colors.Blue);
                //border1.BorderThickness = new Thickness(index);

                border1.RenderTransform = new TranslateTransform();
                //BitmapCache bitmapCache = new BitmapCache();
                //border1.CacheMode = ((CacheMode)bitmapCache);
                border1.Background = new SolidColorBrush(Colors.Transparent);
            
                //border1.ManipulationDelta += this.Element_OnManipulationDelta;
                //border1.ManipulationCompleted += this.Element_OnManipulationCompleted;
                //border1.ManipulationMode = ManipulationModes.TranslateX;

                ContentPresenter contentPresenter1 = new ContentPresenter();
                contentPresenter1.ContentTemplate = this.ItemTemplate;
                //BindingOperations.SetBinding(contentPresenter1, ContentPresenter.ContentProperty, new Binding());
                border1.Child = contentPresenter1;

                this.LayoutRoot.Children.Add(border1);
                this._elements.Add(border1);
            }

            this.LayoutRoot.ManipulationDelta += this.Element_OnManipulationDelta;
            this.LayoutRoot.ManipulationCompleted += this.Element_OnManipulationCompleted;
            this.LayoutRoot.ManipulationMode = ManipulationModes.TranslateX;
        }
    

        private void Element_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (Math.Abs(e.Delta.Translation.Y) > 1)
            {
                (sender as FrameworkElement).CancelDirectManipulations();
                return;
            }
            
            e.Handled = true;
            this.HandleDragDelta(e.Delta.Translation.X);
        }

        private void Element_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("SwipeThrou_ManipulationCompleted");
            e.Handled = true;
            this.HandleDragCompleted(e.Cumulative.Translation.X);
        }

        private void HandleDragDelta(double hDelta)
        {
            if (this._isAnimating)
                return;
            TranslateTransform renderTransform1 = this.CurrentElement.RenderTransform as TranslateTransform;
            if (this._selectedIndex == 0 && hDelta > 0 && renderTransform1.X > 0 || this._selectedIndex == this.Items.Count - 1 && hDelta < 0 && renderTransform1.X < 0)
                hDelta /= 3;
            foreach (var element in this._elements)
            {
                TranslateTransform renderTransform2 = element.RenderTransform as TranslateTransform;
                renderTransform2.X = renderTransform2.X + hDelta;
            }
        }


        private void HandleDragCompleted(double hVelocity, Action calback = null)
        {
            if (this._isAnimating)
                return;
            // this._isInVerticalSwipe = false;
            
            bool? moveNext = new bool?();
            double x = (this._elements[1].RenderTransform as TranslateTransform).X;
            
            if ((hVelocity < -70 && x < 0 || x <= -base.Width / 2) && this._selectedIndex < this.Items.Count - 1)
                moveNext = new bool?(true);
            else if ((hVelocity > 70 && x > 0 || x >= base.Width / 2) && this._selectedIndex > 0)
                moveNext = new bool?(false);
            bool flag1 = this.SelectedIndex <= 1;
            bool flag2 = this.SelectedIndex >= this.Items.Count - 2;
            
            double num3;
            if ((moveNext.GetValueOrDefault() == true ? (moveNext.HasValue ? true : false) : false) != false)
            {
                num3 = !flag2 ? -this.ArrangeWidth : -this.ArrangeWidth + this.NextElementMargin;
            }
            else
            {
                num3 = (moveNext.GetValueOrDefault() == false ? (moveNext.HasValue ? true : false) : false) == false ? (this.SelectedIndex <= this.Items.Count - 2 ? 0 : (this.Items.Count <= 1 ? 0 : this.NextElementMargin)) : (!flag1 ? this.ArrangeWidth : this.ArrangeWidth);
            }
            double delta = num3 - x;
            /*
            if (moveNext.HasValue && moveNext.Value)
            {
                this.AnimateTwoElementsOnDragComplete(this._elements[1], this._elements[2], delta, (() =>
                {
                    this.MoveToNextOrPrevious(moveNext.Value);
                    this.ArrangeElements();
                }), moveNext.HasValue);
                this.ChangeCurrentInd(moveNext.Value);
            }
            else if (moveNext.HasValue && !moveNext.Value)
            {
                this.AnimateTwoElementsOnDragComplete(this._elements[0], this._elements[1], delta, (() =>
                {
                    this.MoveToNextOrPrevious(moveNext.Value);
                    this.ArrangeElements();
                }), moveNext.HasValue);
                this.ChangeCurrentInd(moveNext.Value);
            }*/
            if (moveNext.HasValue)
            {
                FrameworkElement el1 = moveNext.Value ? this._elements[1] : this._elements[0];
                FrameworkElement el2 = moveNext.Value ? this._elements[2] : this._elements[1];
                this.AnimateTwoElementsOnDragComplete(el1, el2, delta, (() =>
                {
                    this.MoveToNextOrPrevious(moveNext.Value);
                    this.ArrangeElements();
                    calback?.Invoke();
                }), moveNext.HasValue);
                this.ChangeCurrentInd(moveNext.Value);
            }
            else
            {
                if (delta == 0)
                    return;
                this.AnimateElementOnDragComplete(this._elements[0], delta, null, moveNext.HasValue);
                this.AnimateElementOnDragComplete(this._elements[1], delta, null, moveNext.HasValue);
                this.AnimateElementOnDragComplete(this._elements[2], delta, this.ArrangeElements, moveNext.HasValue);
            }
        }

        /// <summary>
        /// Присваиваем индекс, объявляем об изменении, выделяем в подвале индекс
        /// </summary>
        /// <param name="next"></param>
        private void ChangeCurrentInd(bool next)
        {
            this._selectedIndex = !next ? this._selectedIndex - 1 : this._selectedIndex + 1;
            this.SelectionChanged?.Invoke(this, this._selectedIndex);
            //this.OnPropertyChanged("SelectedIndex");
            this.UpdateIsSelected();
        }

        private void AnimateElementOnDragComplete(FrameworkElement element, double delta, Action completedCallback, bool movingToNextOrPrevious)
        {
            int duration = 500;//movingToNextOrPrevious ? 200 : 175;
            TranslateTransform renderTransform = element.RenderTransform as TranslateTransform;
            renderTransform.Animate(renderTransform.X, renderTransform.X + delta, "X", duration, 0, this.ANIMATION_EASING, completedCallback);
        }

        private void AnimateTwoElementsOnDragComplete(FrameworkElement element1, FrameworkElement element2, double delta, Action completedCallback, bool movingToNextOrPrevious)
        {
            this._isAnimating = true;
            int num = movingToNextOrPrevious ? 200 : 175;
            var animInfoList = new List<AnimationUtils.AnimationInfo>();
            TranslateTransform renderTransform1 = element1.RenderTransform as TranslateTransform;
            TranslateTransform renderTransform2 = element2.RenderTransform as TranslateTransform;
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                from = renderTransform1.X,
                to = renderTransform1.X + delta,
                propertyPath = "X",
                duration = num,
                target = renderTransform1,
                easing = this.ANIMATION_EASING
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                from = renderTransform2.X,
                to = renderTransform2.X + delta,
                propertyPath = "X",
                duration = num,
                target = renderTransform2,
                easing = this.ANIMATION_EASING
            });
            
            AnimationUtils.AnimateSeveral(animInfoList, 0, ()=> { this._isAnimating = false; completedCallback(); });
        }

        private void ItemsControlFooter_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (e.OriginalSource as FrameworkElement).DataContext;
            int num = this.Items.IndexOf(vm);

            if (num < 0 || this.SelectedIndex == num)
                return;
            this.SelectedIndex = num;
        }
    }
}
/*
 * иконка магазина
 * this._stickersSlideView.HeaderItems = new List<object>((IEnumerable<object>) StickersSettings.Instance.CreateStoreSpriteListItem());
        this._stickersSlideView.FooterItems = new List<object>((IEnumerable<object>) StickersSettings.Instance.CreateSettingsSpriteListItem());
        (
        */
/*
 * IsEmoji
 * IsRecentStickers
 * this._stickersSlideView.Items = new ObservableCollection<object>((IEnumerable<object>) StickersSettings.Instance.CreateSpriteListItemData());
 * */


