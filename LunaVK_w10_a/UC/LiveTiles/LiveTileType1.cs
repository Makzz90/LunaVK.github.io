using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using LunaVK.Core.Utils;

namespace LunaVK.UC.LiveTiles
{
    public class LiveTileType1 : Grid
    {
        DispatcherTimer timer = new DispatcherTimer();
        private int index = 0;

        /// <summary>
        /// Хранилище для всех элементов UIElement
        /// </summary>
        private List<UIElement> internalList = new List<UIElement>();

        

#region ItemsSource
        /// <summary>
        /// Items source : Better if ObservableCollection :)
        /// </summary>
        public IEnumerable<Object> ItemsSource
        {
            get { return (IEnumerable<Object>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable<Object>), typeof(LiveTileType1), new PropertyMetadata(null, ItemsSourceChangedCallback));

        private static void ItemsSourceChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null)
                return;

            if (args.NewValue == args.OldValue)
                return;

            LiveTileType1 parent = dependencyObject as LiveTileType1;

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
                                parent.CreateItem(newItem, 0);
                            break;
                    }
                };
            }

            parent.Bind();
        }
#endregion

       

#region TransitionDuration
        /// <summary>
        /// Duration of the easing function animation (ms)
        /// </summary>
        public int Interval
        {
            get { return (int)GetValue(IntervalDurationProperty); }
            set { SetValue(IntervalDurationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TransitionDuration.
        public static readonly DependencyProperty IntervalDurationProperty = DependencyProperty.Register("Interval", typeof(int), typeof(LiveTileType1), new PropertyMetadata(3, OnIntervalChanged));

        private static void OnIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LiveTileType1 lightStone = (LiveTileType1)d;
            lightStone.timer.Interval = TimeSpan.FromSeconds((int)e.NewValue);
        }
#endregion

        public LiveTileType1()
        {
            base.SizeChanged += LiveTileType1_SizeChanged;
            this.timer.Interval = TimeSpan.FromSeconds(4);
            this.timer.Tick += timer_Tick;
            this.timer.Start();
        }

        bool toDOWN;

        void timer_Tick(object sender, object e)
        {
            if (internalList.Count < 2)
                return;

            if(toDOWN)
                index--;
            else
                index++;

            //System.Diagnostics.Debug.WriteLine("index {0} {1}", index, toDOWN ? "toDOWN" : "toUP");
            if (index < 0)
                return;

            TranslateTransform transform = internalList[index].RenderTransform as TranslateTransform;

            ExponentialEase ease = new ExponentialEase() {  EasingMode= EasingMode.EaseInOut, Exponent=8};

            if (toDOWN)
                transform.Animate(0, base.ActualHeight, "Y", 1500, 0, ease);
            else
                transform.Animate(base.ActualHeight, 0, "Y", 1500, 0, ease);

            if (index == internalList.Count - 1 && toDOWN == false)
            {
                toDOWN = true;
                index++;
            }
            else if (index == 1 && toDOWN == true)
            {
                toDOWN = false;
                index--;
            }
        }

        void LiveTileType1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            base.Clip = new RectangleGeometry() { Rect = new Rect(0,0,e.NewSize.Width,e.NewSize.Height) };
        }
        
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
        private FrameworkElement CreateItem(object item, Double opacity = 1)
        {
            FrameworkElement element = ItemTemplate.LoadContent() as FrameworkElement;
            if (element == null)
                return null;

            element.DataContext = item;
            element.Opacity = opacity;
            
            TranslateTransform translate = new TranslateTransform();
            translate.Y = base.Children.Count == 0 ? 0 : base.ActualHeight;
            element.RenderTransform = translate;

            this.internalList.Add(element);
            base.Children.Add(element);

            return element;
        }

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
        
        /// <summary>
        /// Item Template 
        /// </summary>
        public DataTemplate ItemTemplate { get; set; }
    }
}
