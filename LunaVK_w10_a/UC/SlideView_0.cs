using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using LunaVK.Core.Utils;
using Windows.UI;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using System.Collections.Specialized;

namespace LunaVK.UC
{
    public class SlideView : Grid, INotifyPropertyChanged
    {
        private const double MOVE_TO_NEXT_VELOCITY_THRESHOLD = 100.0;
        private const int DURATION_BOUNCING = 175;
        private const int DURATION_MOVE_TO_NEXT = 200;
        private readonly EasingFunctionBase ANIMATION_EASING;

        #region ItemTemplate
        /*
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(SlideView), new PropertyMetadata(null,SlideView.ItemTemplate_OnChanged));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)base.GetValue(SlideView.ItemTemplateProperty); }
            set { base.SetValue(SlideView.ItemTemplateProperty, value); }
        }

        private static void ItemTemplate_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SlideView)d).UpdateItemTemplate();
        }

        private void UpdateItemTemplate()
        {
            if (this.ItemTemplate == null)
                return;
            this.CreateElements();
            using (List<FrameworkElement>.Enumerator enumerator = this.internalList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ContentPresenter contentPresenter = this.GetContentPresenter(enumerator.Current);
                    if (contentPresenter != null)
                    {
                        contentPresenter.ContentTemplate = this.ItemTemplate;
                        BindingOperations.SetBinding(contentPresenter, ContentPresenter.ContentProperty, new Binding());
                    }
                }
            }
        }*/

        /// <summary>
        /// Item Template 
        /// </summary>
        public DataTemplate ItemTemplate { get; set; }


#endregion
        public static readonly DependencyProperty IsCycledProperty = DependencyProperty.Register("IsCycled", typeof(bool), typeof(SlideView), new PropertyMetadata(false,SlideView.IsCycled_OnChanged));
        public static readonly DependencyProperty AutoSlideIntervalProperty = DependencyProperty.Register("AutoSlideInterval", typeof(int), typeof(SlideView), new PropertyMetadata(3,SlideView.AutoSlideInterval_OnChanged));


        private int _selectedIndex;
        private List<FrameworkElement> internalList = new List<FrameworkElement>();
        private bool _isAnimating;
        private DispatcherTimer _autoSlideTimer;

        

#region ItemsSource
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable<Object>), typeof(SlideView), new PropertyMetadata(null, SlideView.ItemsSource_OnChanged));

        public IEnumerable<Object> ItemsSource
        {
            get { return (IEnumerable<Object>)base.GetValue(SlideView.ItemsSourceProperty); }
            set { base.SetValue(SlideView.ItemsSourceProperty, value); }
        }

        private static void ItemsSource_OnChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {/*
            SlideView slideView = (SlideView)d;
            slideView.UpdateSources(new bool?());

            if (e.NewValue != null)
                slideView.SelectedIndex = 0;
            slideView.StartAutoSlide();*/
            if (args.NewValue == null)
                return;

            if (args.NewValue == args.OldValue)
                return;

            SlideView parent = dependencyObject as SlideView;

            if (parent == null)
                return;

            var obsList = args.NewValue as INotifyCollectionChanged;

            if (obsList != null)
            {
                obsList.CollectionChanged += (sender, eventArgs) =>
                {
                    switch (eventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Remove:
                            foreach (var oldItem in eventArgs.OldItems)
                            {
                                for (int i = 0; i < parent.internalList.Count; i++)
                                {
                                    var fxElement = parent.internalList[i] as FrameworkElement;
                                    if (fxElement == null || fxElement.DataContext != oldItem)
                                        continue;
                                    parent.RemoveAt(i);
                                }
                            }

                            break;
                        case NotifyCollectionChangedAction.Add:
                            foreach (var newItem in eventArgs.NewItems)
                                parent.CreateItem(newItem);
                            break;
                    }
                };
            }

            parent.Bind();
        }
#endregion

        /// <summary>
        /// Bind all Items
        /// </summary>
        private void Bind()
        {
            if (ItemsSource == null)
                return;

            this.Children.Clear();
            this.internalList.Clear();

            foreach (object item in ItemsSource)
                this.CreateItem(item);

            //            this.Children.Add(rectangle);
        }

        /// <summary>
        /// Create an item (Load data template and bind)
        /// </summary>
        private FrameworkElement CreateItem(object data)
        {
            FrameworkElement element = ItemTemplate.LoadContent() as FrameworkElement;
            if (element == null)
                return null;

            element.DataContext = data;

            TranslateTransform translate = new TranslateTransform() { X = base.Children.Count * base.ActualWidth };
            element.RenderTransform = translate;

            this.internalList.Add(element);
            base.Children.Add(element);

            return element;
        }

        public bool IsCycled
        {
            get { return (bool)base.GetValue(SlideView.IsCycledProperty); }
            set { base.SetValue(SlideView.IsCycledProperty, value); }
        }

        public int AutoSlideInterval
        {
            get { return (int)base.GetValue(SlideView.AutoSlideIntervalProperty); }
            set { base.SetValue(SlideView.AutoSlideIntervalProperty, value); }
        }

        public int SelectedIndex
        {
            get
            {
                return this._selectedIndex;
            }
            set
            {
                int selectedIndex = this._selectedIndex;
                this._selectedIndex = value;
                this.RaisePropertyChanged("SelectedIndex");/*
                if (this._selectedIndex - selectedIndex == 2)
                {
                    this.SwapElements(0, 2);
                    this.UpdateSources(false, true, true);
                    this.ArrangeElements();
                }
                else if (selectedIndex - this._selectedIndex == 2)
                {
                    this.SwapElements(0, 2);
                    this.UpdateSources(true, true, false);
                    this.ArrangeElements();
                }
                else if (this._selectedIndex - selectedIndex == 1)
                {
                    this.MoveToNextOrPrevious(true);
                    this.ArrangeElements();
                }
                else if (selectedIndex - this._selectedIndex == 1)
                {
                    this.MoveToNextOrPrevious(false);
                    this.ArrangeElements();
                }
                else
                    this.UpdateSources(new bool?());
                if (this.SelectionChanged == null)
                    return;
                this.SelectionChanged(this, this._selectedIndex);*/
            }
        }

 //       public IManipulationHandler ParentManipulationHandler { get; set; }

        public event EventHandler<int> SelectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;
        

        public SlideView()
        {
            CubicEase cubicEase = new CubicEase();
            cubicEase.EasingMode = EasingMode.EaseOut;
            ANIMATION_EASING = cubicEase;

            base.SizeChanged += this.OnSizeChanged;
            base.Loaded += this.OnLoaded;
            base.Unloaded += this.OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.StartAutoSlide();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.StopAutoSlide();
        }
        
        

        

        private static void IsCycled_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        //    ((SlideView)d).UpdateSources(new bool?());
        }

        private static void AutoSlideInterval_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SlideView)d).StartAutoSlide();
        }
        /*
        private void CreateElements()
        {
            if (this.internalList != null)
                return;
            this.internalList = new List<FrameworkElement>();
            base.Children.Clear();
            for (int index = 0; index < 3; ++index)
            {
                Border border1 = new Border();
                border1.RenderTransform = new TranslateTransform();
                border1.Background = new SolidColorBrush(Colors.Transparent);
                border1.ManipulationMode = Windows.UI.Xaml.Input.ManipulationModes.TranslateX;
                border1.ManipulationStarted += this.Element_OnManipulationStarted;
                border1.ManipulationDelta += this.Element_OnManipulationDelta;
                border1.ManipulationCompleted += this.Element_OnManipulationCompleted;
                if (this.ItemTemplate != null)
                {
                    ContentPresenter contentPresenter1 = new ContentPresenter() { ContentTemplate = this.ItemTemplate };
                    BindingOperations.SetBinding(contentPresenter1, ContentPresenter.ContentProperty, new Binding());
                    border1.Child = contentPresenter1;
                }
                this.internalList.Add(border1);
                base.Children.Add(border1);
            }
        }
        */
        /*
        private ContentPresenter GetContentPresenter(FrameworkElement element)
        {
            if (element == null)
                return null;
            return (ContentPresenter)((Border)element).Child;
        }

        private TranslateTransform GetElementTransform(int index)
        {
            return this.internalList[index].RenderTransform as TranslateTransform;
        }
        */
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.internalList.Count==0)
                return;

 //           this.ArrangeElements();
        }
        /*
        private void ArrangeElements()
        {
            
            this.GetElementTransform(0).X = (-base.ActualWidth);
            this.GetElementTransform(1).X = 0.0;
            this.GetElementTransform(2).X = (base.ActualWidth);
        }
        
        private void UpdateSources(bool update0, bool update1, bool update2)
        {
            if (update1)
                SlideView.SetDataContext(this.internalList[1], this.GetItem(this._selectedIndex));
            if (update0)
                SlideView.SetDataContext(this.internalList[0], this.GetItem(this._selectedIndex - 1));
            if (!update2)
                return;
            SlideView.SetDataContext(this.internalList[2], this.GetItem(this._selectedIndex + 1));
        }
        
        private void UpdateSources(bool? movedForvard = null)
        {
            if (this.internalList == null)
                return;
            if (!movedForvard.HasValue)
                SlideView.SetDataContext(this.internalList[1], this.GetItem(this._selectedIndex));
            int num = !movedForvard.HasValue ? 1 : (movedForvard.Value ? 1 : 0);
            if ((!movedForvard.HasValue ? 1 : (!movedForvard.Value ? 1 : 0)) != 0)
                SlideView.SetDataContext(this.internalList[0], this.GetItem(this._selectedIndex - 1));
            if (num == 0)
                return;
            SlideView.SetDataContext(this.internalList[2], this.GetItem(this._selectedIndex + 1));
        }

        /// <summary>
        /// Изменяем содержимое элемента
        /// </summary>
        /// <param name="element">Элемент</param>
        /// <param name="dataContext">Данные</param>
        private static void SetDataContext(FrameworkElement element, object dataContext)
        {
  //          ISupportDataContext supportDataContext = element as ISupportDataContext;
  //          if (supportDataContext != null)
  //              supportDataContext.SetDataContext(dataContext);
  //          else
                element.DataContext = dataContext;
        }

        private object GetItem(int index)
        {
            if (this.ItemsSource == null || this.ItemsSource.Count == 0)
                return null;
            if (index < 0)
            {
                if (!this.IsCycled)
                    return null;
                return this.ItemsSource[this.ItemsSource.Count - 1];
            }
            if (index < this.ItemsSource.Count)
                return this.ItemsSource[index];
            if (!this.IsCycled)
                return null;
            return this.ItemsSource[0];
        }
        */
        private void Element_OnManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
        {
            if (this._autoSlideTimer != null)
                this._autoSlideTimer.Stop();
            e.Handled = true;
        }

        private void Element_OnManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            if (/*e.PinchManipulation != null ||*/ this.ItemsSource == null)
                return;
            Point translation = e.Velocities.Linear;
            e.Handled = true;
            this.HandleDragDelta(translation);
        }

        private void Element_OnManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            if (this._autoSlideTimer != null)
                this._autoSlideTimer.Start();
            e.Handled = true;
            if (this.ItemsSource == null)
                return;
            Point linearVelocity = e.Cumulative.Translation;
            this.HandleDragCompleted(linearVelocity.X);
        }

        private void HandleDragDelta(Point translation)
        {
            if (this._isAnimating)
                return;
            
            double x = translation.X;
            using (List<FrameworkElement>.Enumerator enumerator = this.internalList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
       //             TranslateTransform elementTransform = this.GetElementTransform(this.internalList.IndexOf(enumerator.Current));
       //             elementTransform.X += x;
                }
            }
        }

        private void HandleDragCompleted(double hVelocity)
        {
            if (this._isAnimating)
                return;
            double num1 = hVelocity;
            bool moveNext = true;
  //          double x = this.GetElementTransform(1).X;
   //         double num2 = num1;
 //           if ((num2 < -100.0 && x < 0.0 || x <= -base.Width / 2.0) && (this._selectedIndex < /*this.ItemsSource.Count*/ - 1 || this.IsCycled))
 //               moveNext = new bool?(true);
 //           else if ((num2 > 100.0 && x > 0.0 || x >= base.Width / 2.0) && (this._selectedIndex > 0 || this.IsCycled))
  //              moveNext = new bool?(false);
            this.SlideElements(moveNext);
        }

        private void SlideElements(bool moveNext)
        {
            bool flag1 = this.SelectedIndex <= 1;
            bool flag2 = this.SelectedIndex >= 0;// this.ItemsSource.Count - 2;
            bool? nullable1 = moveNext;
            bool flag3 = true;
            double num1;
            if ((nullable1.GetValueOrDefault() == flag3 ? (nullable1.HasValue ? 1 : 0) : 0) != 0)
            {
                num1 = !flag2 ? -base.ActualWidth : -base.ActualWidth;
            }
            else
            {
                bool? nullable2 = moveNext;
                bool flag4 = false;
                num1 = (nullable2.GetValueOrDefault() == flag4 ? (nullable2.HasValue ? 1 : 0) : 0) == 0 ? (this.SelectedIndex <= /*this.ItemsSource.Count -*/ 2 ? (this.SelectedIndex >= 1 ? 0.0 : 0.0) : 0.0) : base.ActualWidth;
            }/*
    //        double x = this.GetElementTransform(1).X;
            double delta1 = num1 - x;
            if (moveNext.HasValue)
            {
                this._isAnimating = true;
                List<int> intList = new List<int>();
                if (moveNext.Value)
                {
                    intList.Add(1);
                    intList.Add(2);
                }
                else
                {
                    intList.Add(0);
                    intList.Add(1);
                }
                this.AnimateElements(intList, delta1, (() =>
                {
                    this.MoveToNextOrPrevious(moveNext.Value);
                    this.ArrangeElements();
                    this._isAnimating = false;
                }), true);
                this.ChangeCurrentInd(moveNext.Value);
        }
            else
            {*/
             //              if (delta1 == 0.0)
             //                 return;
            this._isAnimating = true;
                List<int> intList = new List<int>();
                intList.Add(0);
                intList.Add(1);
                intList.Add(2);
                Action completedCallback = (() => this._isAnimating = false);
                
                this.AnimateElements(intList, /*delta1*/base.ActualWidth, completedCallback, false);
    //        }
        }

        private void AnimateElements(IEnumerable indexes, double delta, Action completedCallback, bool moveNextOrPrevious)
        {
            int dur = moveNextOrPrevious ? 200 : 175;
            var animInfoList = new List<AnimationUtils.AnimationInfo>();
            foreach (int index in indexes)
            {
                TranslateTransform elementTransform = base.Children[index].RenderTransform as TranslateTransform;//this.GetElementTransform(index);
                animInfoList.Add(new AnimationUtils.AnimationInfo()
                {
                    from = elementTransform.X,
                    to = elementTransform.X + delta,
                    propertyPath = "X",
                    duration = dur * index,
                    target = elementTransform,
                    easing = ANIMATION_EASING
                });
            }
            AnimationUtils.AnimateSeveral(animInfoList, 0, completedCallback);
        }

        private void MoveToNextOrPrevious(bool next)
        {
            if (next)
            {
                this.SwapElements(0, 1);
                this.SwapElements(1, 2);
            }
            else
            {
                this.SwapElements(1, 2);
                this.SwapElements(0, 1);
            }
 //           this.UpdateSources(new bool?(next));
        }

        private void ChangeCurrentInd(bool next)
        {/*
            if (next)
            {
                this._selectedIndex = this._selectedIndex + 1;
                if (this.IsCycled && this._selectedIndex >= this.ItemsSource.Count)
                    this._selectedIndex = 0;
            }
            else
            {
                this._selectedIndex = this._selectedIndex - 1;
                if (this.IsCycled && this._selectedIndex < 0)
                    this._selectedIndex = this.ItemsSource.Count - 1;
            }

            this.SelectionChanged?.Invoke(null, this._selectedIndex);
            
            this.RaisePropertyChanged("SelectedIndex");*/
        }

        private void SwapElements(int index1, int index2)
        {
            FrameworkElement element = this.internalList[index1];
            this.internalList[index1] = this.internalList[index2];
            this.internalList[index2] = element;
        }

        private void StartAutoSlide()
        {
            this.StopAutoSlide();
            
            if (this._autoSlideTimer == null)
                this._autoSlideTimer = new DispatcherTimer();
            this._autoSlideTimer.Interval = TimeSpan.FromSeconds(this.AutoSlideInterval);
            this._autoSlideTimer.Tick += this.AutoSlideTimer_OnTick;
            this._autoSlideTimer.Start();
        }

        private void StopAutoSlide()
        {
            if (this._autoSlideTimer == null)
                return;
            this._autoSlideTimer.Tick -= this.AutoSlideTimer_OnTick;
            if (!this._autoSlideTimer.IsEnabled)
                return;
            this._autoSlideTimer.Stop();
        }

        private void AutoSlideTimer_OnTick(object sender, object eventArgs)
        {
            if (this.ItemsSource == null/* || this.ItemsSource.Count < 2*/)
                return;
            this.SlideElements(true);
        }
        
        private void RaisePropertyChanged(string property)
        {
            // ISSUE: reference to a compiler-generated field
            if (this.PropertyChanged == null)
                return;
            LunaVK.Core.Framework.Execute.ExecuteOnUIThread((Action)(() =>
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(property);
                this.PropertyChanged(this, e);
            }));
        }

        /// <summary>
        /// Remove an item at position. Reaffect SelectedIndex
        /// </summary>
        public void RemoveAt(int index)
        {
            var uiElement = this.internalList[index];
            
            // Remove from internal list
            this.internalList.RemoveAt(index);

            // Remove from visual tree
            this.Children.Remove(uiElement);
        }
    }
}
