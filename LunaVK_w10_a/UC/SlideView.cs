using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using Windows.UI;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using LunaVK.Core.Utils;
using System.Collections.Specialized;

namespace LunaVK.UC
{
    public class SlideView : Grid, INotifyPropertyChanged
    {
        private const double MOVE_TO_NEXT_VELOCITY_THRESHOLD = 100.0;
        private const int DURATION_BOUNCING = 175;
        private const int DURATION_MOVE_TO_NEXT = 200;
        private static readonly EasingFunctionBase ANIMATION_EASING;
        private int _selectedIndex;
        private List<FrameworkElement> _elements;
        private bool _isAnimating;
        private DispatcherTimer _autoSlideTimer;


        public event EventHandler<int> SelectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

#region ItemTemplate
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(SlideView), new PropertyMetadata(null, SlideView.ItemTemplate_OnChanged));
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
            using (List<FrameworkElement>.Enumerator enumerator = this._elements.GetEnumerator())
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
        }
#endregion

#region ItemsSource
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IList), typeof(SlideView), new PropertyMetadata(null, SlideView.ItemsSource_OnChanged));
        public IList ItemsSource
        {
            get { return (IList)base.GetValue(SlideView.ItemsSourceProperty); }
            set { base.SetValue(SlideView.ItemsSourceProperty, value); }
        }
        private static void ItemsSource_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SlideView slideView = (SlideView)d;
            slideView.UpdateSources(new bool?());
            if (e.NewValue != null)
                slideView.SelectedIndex = 0;
            slideView.StartAutoSlide();
            //
            //
            INotifyCollectionChanged oldValue = e.OldValue as INotifyCollectionChanged;
            if (oldValue != null)
                slideView.UnhookCollectionChanged(oldValue);

            INotifyCollectionChanged newValue = e.NewValue as INotifyCollectionChanged;
            if (newValue != null)
                slideView.HookUpCollectionChanged( newValue);
        }

        public void HookUpCollectionChanged(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += this.NewValue_CollectionChanged;
        }

        public void UnhookCollectionChanged(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= this.NewValue_CollectionChanged;
        }

        private void NewValue_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.UpdateSources(new bool?());
        }
#endregion

#region IsCycled
        public static readonly DependencyProperty IsCycledProperty = DependencyProperty.Register(nameof(IsCycled), typeof(bool), typeof(SlideView), new PropertyMetadata(true, SlideView.IsCycled_OnChanged));
        public bool IsCycled
        {
            get { return (bool)base.GetValue(SlideView.IsCycledProperty); }
            set { base.SetValue(SlideView.IsCycledProperty, value); }
        }

        private static void IsCycled_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SlideView)d).UpdateSources(new bool?());
        }
#endregion

        public static readonly DependencyProperty AutoSlideIntervalProperty = DependencyProperty.Register("AutoSlideInterval", typeof(double), typeof(SlideView), new PropertyMetadata(5.0, SlideView.AutoSlideInterval_OnChanged));
        public double AutoSlideInterval
        {
            get { return (double)base.GetValue(SlideView.AutoSlideIntervalProperty); }
            set { base.SetValue(SlideView.AutoSlideIntervalProperty, value); }
        }
        private static void AutoSlideInterval_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SlideView)d).StartAutoSlide();
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
                this.NotifyPropertyChanged(nameof(this.SelectedIndex));
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
                this.SelectionChanged(this, this._selectedIndex);
            }
        }



        

        static SlideView()
        {
            CubicEase cubicEase = new CubicEase() { EasingMode = EasingMode.EaseOut };
            SlideView.ANIMATION_EASING = cubicEase;
        }

        public SlideView()
        {
            base.SizeChanged += this.OnSizeChanged;
            base.Loaded += this.OnLoaded;
            base.Unloaded += this.OnUnloaded;

            //
            //
            //this.CreateElements();
            //this.ArrangeElements();
            //this.UpdateSources(new bool?());
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.StartAutoSlide();
            this.SelectionChanged?.Invoke(this, 0);
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.StopAutoSlide();
            base.SizeChanged -= this.OnSizeChanged;
        }


        

        

        

        

        private void CreateElements()
        {
            if (this._elements != null)
                return;
            this._elements = new List<FrameworkElement>(3);
            this.Children.Clear();
            for (int index = 0; index < 3; index++)
            {
                Border border1 = new Border();
                border1.RenderTransform = new TranslateTransform();
                border1.CacheMode = new BitmapCache();
                border1.Background = new SolidColorBrush(Colors.Transparent);
                border1.ManipulationStarted += this.Element_OnManipulationStarted;
                border1.ManipulationDelta += this.Element_OnManipulationDelta;
                border1.ManipulationCompleted += this.Element_OnManipulationCompleted;
                border1.ManipulationMode = Windows.UI.Xaml.Input.ManipulationModes.TranslateX;
                if (this.ItemTemplate != null)
                {
                    ContentPresenter contentPresenter1 = new ContentPresenter();
                    contentPresenter1.ContentTemplate = this.ItemTemplate;
                    BindingOperations.SetBinding(contentPresenter1, ContentPresenter.ContentProperty, new Binding());
                    border1.Child = contentPresenter1;
                }
                this._elements.Add(border1);
                this.Children.Add(border1);
            }
        }

        private ContentPresenter GetContentPresenter(FrameworkElement element)
        {
            if (element == null)
                return null;
            return ((Border)element).Child as ContentPresenter;
        }

        private TranslateTransform GetElementTransform(int index)
        {
            return (this._elements[index]).RenderTransform as TranslateTransform;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ArrangeElements();
            FrameworkElement fr = sender as FrameworkElement;
            fr.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };
        }

        private void ArrangeElements()
        {
            if (this._elements == null)
                return;
            this.GetElementTransform(0).X = (-base.ActualWidth);
            this.GetElementTransform(1).X = 0.0;
            this.GetElementTransform(2).X = (base.ActualWidth);
        }

        private void UpdateSources(bool update0, bool update1, bool update2)
        {
            if (update1)
                SlideView.SetDataContext(this._elements[1], this.GetItem(this._selectedIndex));
            if (update0)
                SlideView.SetDataContext(this._elements[0], this.GetItem(this._selectedIndex - 1));
            if (!update2)
                return;
            SlideView.SetDataContext(this._elements[2], this.GetItem(this._selectedIndex + 1));
        }

        private void UpdateSources(bool? movedForvard = null)
        {
            if (this._elements == null)
                return;
            if (!movedForvard.HasValue)
                SlideView.SetDataContext(this._elements[1], this.GetItem(this._selectedIndex));
            int num = !movedForvard.HasValue ? 1 : (movedForvard.Value ? 1 : 0);
            if ((!movedForvard.HasValue ? 1 : (!movedForvard.Value ? 1 : 0)) != 0)
                SlideView.SetDataContext(this._elements[0], this.GetItem(this._selectedIndex - 1));
            if (num == 0)
                return;
            SlideView.SetDataContext(this._elements[2], this.GetItem(this._selectedIndex + 1));
        }

        private static void SetDataContext(FrameworkElement element, object dataContext)
        {
            //ISupportDataContext supportDataContext = element as ISupportDataContext;
            //if (supportDataContext != null)
            //  supportDataContext.SetDataContext(dataContext);
            //else
            element.DataContext = dataContext;
        }

        private object GetItem(int index)
        {
            if (this.ItemsSource == null || this.ItemsSource.Count == 0)
                return null;
            if (index < 0)
            {
                return this.IsCycled ? this.ItemsSource[this.ItemsSource.Count - 1] : null;
            }

            if (index < this.ItemsSource.Count)
                return this.ItemsSource[index];
            
            return this.IsCycled ? this.ItemsSource[0] : null;
        }

        private void Element_OnManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
        {
            this._autoSlideTimer?.Stop();

            if(this.ItemsSource.Count == 1)
            {
                e.Container.CancelDirectManipulations();
            }

            e.Handled = true;
        }

        private void Element_OnManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;

            if (/*e.PinchManipulation != null ||*/ this.ItemsSource == null)
                return;
            Point translation = e.Delta.Translation;


            this.HandleDragDelta(translation);

        }

        private void Element_OnManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            if (this._autoSlideTimer != null)
                this._autoSlideTimer.Start();

            e.Handled = true;
            if (this.ItemsSource == null)
                return;
            this.HandleDragCompleted(e.Cumulative.Translation.X);
        }

        private void HandleDragDelta(Point translation)
        {
            if (this._isAnimating)
                return;
            double x = translation.X;
            if (Math.Abs(translation.Y) > Math.Abs(x))
                return;
            if ((this._selectedIndex == 0 && x > 0.0 || this._selectedIndex == this.ItemsSource.Count - 1 && x < 0.0) && !this.IsCycled)
                x /= 3.0;
            using (List<FrameworkElement>.Enumerator enumerator = this._elements.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    TranslateTransform elementTransform = this.GetElementTransform(this._elements.IndexOf(enumerator.Current));
                    double num = elementTransform.X + x;
                    elementTransform.X = num;
                }
            }
        }

        private void HandleDragCompleted(double hVelocity)
        {
            if (this._isAnimating)
                return;
            double num1 = hVelocity;
            bool? moveNext = new bool?();
            double x = this.GetElementTransform(1).X;
            double num2 = num1;
            if ((num2 < -100.0 && x < 0.0 || x <= -base.ActualWidth / 2.0) && (this._selectedIndex < this.ItemsSource.Count - 1 || this.IsCycled))
                moveNext = new bool?(true);
            else if ((num2 > 100.0 && x > 0.0 || x >= base.ActualWidth / 2.0) && (this._selectedIndex > 0 || this.IsCycled))
                moveNext = new bool?(false);
            this.SlideElements(moveNext);
        }

        private void SlideElements(bool? moveNext)
        {
            bool flag1 = this.SelectedIndex <= 1;
            bool flag2 = this.SelectedIndex >= this.ItemsSource.Count - 2;
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
                num1 = (nullable2.GetValueOrDefault() == flag4 ? (nullable2.HasValue ? 1 : 0) : 0) == 0 ? (this.SelectedIndex <= this.ItemsSource.Count - 2 ? (this.SelectedIndex >= 1 ? 0.0 : 0.0) : (this.ItemsSource.Count <= 1 ? 0.0 : 0.0)) : (!flag1 ? base.ActualWidth : base.ActualWidth);
            }
            double x = this.GetElementTransform(1).X;
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
                this.AnimateElements(intList, delta1, (Action)(() =>
              {
                  this.MoveToNextOrPrevious(moveNext.Value);
                  this.ArrangeElements();
                  this._isAnimating = false;
              }), true);
                this.ChangeCurrentInd(moveNext.Value);
            }
            else
            {
                if (delta1 == 0.0)
                    return;
                this._isAnimating = true;
                List<int> intList = new List<int>();
                intList.Add(0);
                intList.Add(1);
                intList.Add(2);
                
                this.AnimateElements(intList, delta1, (() => this._isAnimating = false), false);
            }
        }

        private void AnimateElements(IEnumerable indexes, double delta, Action completedCallback, bool moveNextOrPrevious)
        {
            int num = moveNextOrPrevious ? 200 : 175;
            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
            foreach (int index in indexes)
            {
                if (index < this._elements.Count)
                {
                    TranslateTransform elementTransform = this.GetElementTransform(index);
                    animInfoList.Add(new AnimationUtils.AnimationInfo()
                    {
                        from = elementTransform.X,
                        to = elementTransform.X + delta,
                        propertyPath = "X",
                        duration = num,
                        target = elementTransform,
                        easing = SlideView.ANIMATION_EASING
                    });
                }
            }
            AnimationUtils.AnimateSeveral(animInfoList, new int?(0), completedCallback);
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
            this.UpdateSources(new bool?(next));
        }

        private void ChangeCurrentInd(bool next)
        {
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
            this.SelectionChanged?.Invoke(this, this._selectedIndex);
            this.NotifyPropertyChanged("SelectedIndex");
        }

        private void SwapElements(int index1, int index2)
        {
            FrameworkElement element = this._elements[index1];
            this._elements[index1] = this._elements[index2];
            this._elements[index2] = element;
        }

        private void StartAutoSlide()
        {
            this.StopAutoSlide();
            double autoSlideInterval = this.AutoSlideInterval;
            if (autoSlideInterval==0)
              return;
            if (this._autoSlideTimer == null)
                this._autoSlideTimer = new DispatcherTimer();
            this._autoSlideTimer.Interval = TimeSpan.FromSeconds(autoSlideInterval);
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
            if (this.ItemsSource == null || this.ItemsSource.Count < 2)
                return;
            this.SlideElements(new bool?(true));
        }

        private void NotifyPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression.Body.NodeType != ExpressionType.MemberAccess)
                return;
            this.RaisePropertyChanged(((MemberExpression)propertyExpression.Body).Member.Name);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.RaisePropertyChanged(propertyName);
        }

        private void RaisePropertyChanged(string property)
        {
            // ISSUE: reference to a compiler-generated field
            if (this.PropertyChanged == null)
                return;
            Core.Framework.Execute.ExecuteOnUIThread(() =>
             {
                 // ISSUE: reference to a compiler-generated field
                 PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
                 if (propertyChanged == null)
                     return;
                 PropertyChangedEventArgs e = new PropertyChangedEventArgs(property);
                 propertyChanged(this, e);
             });
        }
    }
}
