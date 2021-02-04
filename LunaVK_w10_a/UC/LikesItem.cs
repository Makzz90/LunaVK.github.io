using LunaVK.Core.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace LunaVK.UC
{
    public class LikesItem : Grid
    {
        private List<FrameworkElement> internalList;
        private DispatcherTimer _autoSlideTimer;

#region AutoSlideInterval
        public static readonly DependencyProperty AutoSlideIntervalProperty = DependencyProperty.Register("AutoSlideInterval", typeof(double), typeof(LikesItem), new PropertyMetadata(0.0));
        public double AutoSlideInterval
        {
            get { return (double)base.GetValue(LikesItem.AutoSlideIntervalProperty); }
            set { base.SetValue(LikesItem.AutoSlideIntervalProperty, value); }
        }
#endregion

#region ItemTemplate
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(LikesItem), new PropertyMetadata(null));
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)base.GetValue(LikesItem.ItemTemplateProperty); }
            set { base.SetValue(LikesItem.ItemTemplateProperty, value); }
        }
#endregion


#region ItemsSource
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IList), typeof(LikesItem), new PropertyMetadata(null, LikesItem.ItemsSource_OnChanged));
        public IList ItemsSource
        {
            get { return (IList)base.GetValue(LikesItem.ItemsSourceProperty); }
            set { base.SetValue(LikesItem.ItemsSourceProperty, value); }
        }
        private static void ItemsSource_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LikesItem slideView = (LikesItem)d;
            
            if( e.NewValue is INotifyCollectionChanged obsList)
            {
                obsList.CollectionChanged += (sender, eventArgs) =>
                {
                    switch (eventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Remove:
                            foreach (var oldItem in eventArgs.OldItems)
                            {
                                for (int i = 0; i < slideView.internalList.Count; i++)
                                {
                                    var fxElement = slideView.internalList[i] as FrameworkElement;
                                    if (fxElement == null || fxElement.DataContext != oldItem)
                                        continue;
                                    slideView.RemoveAt(i);
                                }
                            }

                            break;
                        case NotifyCollectionChangedAction.Add:
                            foreach (var newItem in eventArgs.NewItems)
                            {
                                slideView.CreateElement(newItem);
                            }
                            break;
                    }

                    slideView.UpdateViews();
                };
            }
            else if(e.NewValue is IList list)
            {
                foreach (var newItem in list)
                {
                    slideView.CreateElement(newItem);
                }
                slideView.AnimateOnStart();
                slideView.StartAutoSlide();
                slideView.Unloaded += (s,ea)=> { slideView.StopAutoSlide(); };
            }
        }

#endregion

        /// <summary>
        /// Remove an item at position. Reaffect SelectedIndex
        /// </summary>
        public void RemoveAt(int index)
        {
            var uiElement = this.internalList[index];

            //  this.isUpdatingPosition = true;
            // Remove from internal list
            this.internalList.RemoveAt(index);

            // Remove from visual tree
            this.Children.Remove(uiElement);
        }
        
        

        private object GetItem(int index)
        {
            if (this.ItemsSource == null || this.ItemsSource.Count == 0)
                return null;
            if (index < 0)
            {
                //if (!this.IsCycled)
                //    return null;
                return this.ItemsSource[this.ItemsSource.Count - 1];
            }
            if (index < this.ItemsSource.Count)
                return this.ItemsSource[index];
            //if (!this.IsCycled)
            //    return null;
            return this.ItemsSource[0];
        }

        private void StartAutoSlide()
        {
            this.StopAutoSlide();
            double autoSlideInterval = this.AutoSlideInterval;
            if (autoSlideInterval == 0 || this.ItemsSource.Count < 5)
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

            FrameworkElement elementToRemove = base.Children.Last() as FrameworkElement;
            elementToRemove.Animate(1, 0, "Opacity", 200, 0, null, () => {

                base.Children.Remove(elementToRemove);

                FrameworkElement last = base.Children.Last() as FrameworkElement;
                FrameworkElement first = base.Children.First() as FrameworkElement;
                double firsOffs = (first.RenderTransform as TranslateTransform).X;

                int index = this.internalList.IndexOf(first);
                if (index == 0)
                    index = this.internalList.Count;
                var newElement = this.internalList[index - 1];
                newElement.Opacity = 0;
                (newElement.RenderTransform as TranslateTransform).X = firsOffs;


                double offs = (last.RenderTransform as TranslateTransform).X;
                List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
                foreach (var element in base.Children)
                {
                    TranslateTransform elementTransform = element.RenderTransform as TranslateTransform;
                    animInfoList.Add(new AnimationUtils.AnimationInfo()
                    {
                        from = elementTransform.X,
                        to = elementTransform.X - offs,
                        propertyPath = "X",
                        duration = 200,
                        target = elementTransform,
                    });
                }

                animInfoList.Add(new AnimationUtils.AnimationInfo() {
                    from = 0,
                    to = 1,
                    propertyPath = "Opacity",
                    duration = 550,
                    target = newElement,
                });

                AnimationUtils.AnimateSeveral(animInfoList);
                
                base.Children.Insert(0, newElement);
            });
        }

        private FrameworkElement CreateElement(object item)
        {
            FrameworkElement element = ItemTemplate.LoadContent() as FrameworkElement;
            element.DataContext = item;
            element.RenderTransform = new TranslateTransform();
            element.HorizontalAlignment = HorizontalAlignment.Left;
            element.Opacity = 0;

            if (this.internalList == null)
                this.internalList = new List<FrameworkElement>();
            this.internalList.Insert(0, element);//this.internalList.Add(element);

            if (base.Children.Count < 5)
            {
                base.Children.Insert(0,element);//base.Children.Add(element);
                double multiplier = this.GetOffset();
                TranslateTransform elementTransform = element.RenderTransform as TranslateTransform;
                elementTransform.X = (base.Children.Count-1) * multiplier;
            }
            return element;
        }

        private void AnimateOnStart()
        {
            int i = 0;
            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
            foreach (var element in base.Children)
            {
                animInfoList.Add(new AnimationUtils.AnimationInfo()
                {
                    from = element.Opacity,
                    to = 1,
                    propertyPath = "Opacity",
                    duration = 300 + (i * 100),
                    target = element,
                    //easing = SlideView.ANIMATION_EASING
                });

                i++;
            }
            AnimationUtils.AnimateSeveral(animInfoList);
        }

        private double GetOffset()
        {
            int max = Math.Max(this.internalList.Count, 5);
            double multiplier = 0;
            double needWidth = this.ActualHeight * max;

            if (this.ActualWidth - needWidth >= 0)
            {
                multiplier = this.ActualHeight;
            }
            else
            {
                double temp = needWidth - this.ActualWidth;
                multiplier = this.ActualHeight - (temp / (max - 1));
            }
            return multiplier;
        }

        private void UpdateViews()
        {
            double num4 = 0;
            double multiplier = this.GetOffset();

            //System.Diagnostics.Debug.WriteLine(multiplier);

            int i = 0;
            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
            foreach (var element in this.internalList)
            {
                element.HorizontalAlignment = HorizontalAlignment.Right;

                TranslateTransform elementTransform = element.RenderTransform as TranslateTransform;
                animInfoList.Add(new AnimationUtils.AnimationInfo()
                {
                    from = elementTransform.X,
                    to = num4,
                    propertyPath = "X",
                    duration = 200 + (i * 200),
                    target = elementTransform,
                    //easing = SlideView.ANIMATION_EASING
                });

                i++;
                //num4 -= multiplier;right aligm
                num4 += multiplier;
            }
            AnimationUtils.AnimateSeveral(animInfoList);


            /*
            foreach (var element in this.internalList)
            {
                element.HorizontalAlignment = HorizontalAlignment.Right;
                (element.RenderTransform as TranslateTransform).X = num4;
                num4 -= multiplier;
            }*/
        }
        }
}
