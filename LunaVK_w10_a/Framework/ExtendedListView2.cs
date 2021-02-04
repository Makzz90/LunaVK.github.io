using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Windows.UI.Xaml.Media;
using Windows.Foundation;
using System.Collections.ObjectModel;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;

namespace LunaVK.Framework
{
    public class ExtendedListView2 : ItemsControl
    {
        public static readonly DependencyProperty IsFlatProperty = DependencyProperty.Register("IsFlat", typeof(bool), typeof(ExtendedListView2), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty UseHeaderOffsetProperty = DependencyProperty.Register("UseHeaderOffset", typeof(bool), typeof(ExtendedListView2), new PropertyMetadata(true));
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(ExtendedListView2), new PropertyMetadata(null));
        public static readonly DependencyProperty FooterProperty = DependencyProperty.Register("Footer", typeof(object), typeof(ExtendedListView2), new PropertyMetadata(null));
        public static readonly DependencyProperty ReversPullProperty = DependencyProperty.Register("ReversPull", typeof(bool), typeof(ExtendedListView2), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty UseFooterOffsetProperty = DependencyProperty.Register("UseFooterOffset", typeof(bool), typeof(ExtendedListView2), new PropertyMetadata(false));
        public static readonly DependencyProperty IsPullEnabledProperty = DependencyProperty.Register("IsPullEnabled", typeof(bool), typeof(ExtendedListView2), new PropertyMetadata(true, IsPullEnabledChanged));
        public static readonly DependencyProperty IsAdaptiveProperty = DependencyProperty.Register("IsAdaptive", typeof(bool), typeof(ExtendedListView2), new PropertyMetadata(false, IsAdaptiveChanged));
        public static readonly DependencyProperty IsHorizontalProperty = DependencyProperty.Register("IsHorizontal", typeof(bool), typeof(ExtendedListView2), new PropertyMetadata(false));

        public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register("SelectionMode", typeof(ListViewSelectionMode), typeof(ExtendedListView2), new PropertyMetadata(ListViewSelectionMode.None, SelectionModeChanged));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(ExtendedListView2), new PropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (ExtendedListView2)d;
            /*
            view.OnSelectionChanged(new SelectionChangedEventArgs(new List<object> { e.OldValue }, new List<object> { e.NewValue }));

            if (view.SelectedItem != null)//BugFix?
                view.UpdateView(true);

            // If there is no selection, do not remove the DetailsPresenter content but let it animate out.
            if (view.SelectedItem != null)
            {
                view.SetDetailsContent();
            }*/
            if (view.listView != null)
                view.listView.SelectedItem = e.NewValue;
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        private Windows.UI.Xaml.Shapes.Rectangle offsetForHeader;
        private Windows.UI.Xaml.Shapes.Rectangle offsetForFooter;
        private Windows.UI.Xaml.Shapes.Rectangle rect;
        private ScrollViewer inside_scrollViewer;
        private ListView listView;
        private Grid headerGrid;
        private Grid footerGrid;
        public ItemsPresenter ContentItemsPresenter { get; private set; }
        private double lastpullvalue = 0.0;
        public Action InsideScrollViewerLoaded;
        DispatcherTimer timer = new DispatcherTimer();
        private bool InLoading;

        public ListViewSelectionMode SelectionMode
        {
            get { return (ListViewSelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        public bool IsFlat
        {
            get { return (bool)GetValue(IsFlatProperty); }
            set { SetValue(IsFlatProperty, value); }
        }

        public bool ReversPull
        {
            get { return (bool)GetValue(ReversPullProperty); }
            set { SetValue(ReversPullProperty, value); }
        }

        private static void IsPullEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExtendedListView2 lv = (ExtendedListView2)d;
            bool val = (bool)e.NewValue;
            lv.ActivateTimer(val);
        }

        private static void SelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExtendedListView2 lv = (ExtendedListView2)d;
            ListViewSelectionMode val = (ListViewSelectionMode)e.NewValue;
            if(lv.listView!=null)
                lv.listView.SelectionMode = val;
        }

        public bool IsPullEnabled
        {
            get { return (bool)base.GetValue(IsPullEnabledProperty); }
            set { base.SetValue(IsPullEnabledProperty, value); }
        }

        private static void IsAdaptiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExtendedListView2 lv = (ExtendedListView2)d;
            bool val = (bool)e.NewValue;
            lv.ActivateAdaptive(val);
        }

        public bool IsAdaptive
        {
            get { return (bool)base.GetValue(IsAdaptiveProperty); }
            set { base.SetValue(IsAdaptiveProperty, value); }
        }

        public bool IsHorizontal
        {
            get { return (bool)base.GetValue(IsHorizontalProperty); }
            set { base.SetValue(IsHorizontalProperty, value); }
        }

        public bool PreventScroll;

        public void ActivateTimer(bool status)
        {
            if(status==true)
            {
                if (!timer.IsEnabled)
                    timer.Start();
            }
            else
            {
                this.timer.Stop();
            }
        }

//        private DispatcherTimer loadingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };

        public void ActivateAdaptive(bool status)
        {
            
            if(this.ContentItemsPresenter!=null)
                this.ContentItemsPresenter.MaxWidth = status ? 600 : double.PositiveInfinity;
        }

        public bool UseHeaderOffset
        {
            get { return (bool)GetValue(UseHeaderOffsetProperty); }
            set {
                //
                if (this.offsetForHeader!=null)
                    this.offsetForHeader.Height = value ? CustomFrame.Instance.HeaderWithMenu.HeaderHeight : 0;
                //
                SetValue(UseHeaderOffsetProperty, value);
            }
        }

        public bool UseFooterOffset
        {
            get { return (bool)GetValue(UseFooterOffsetProperty); }
            set { SetValue(UseFooterOffsetProperty, value); }
        }

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public object Footer
        {
            get { return GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }
        
        /// <summary>
        /// Это область прокрутки внутри ListView
        /// </summary>
        public ScrollViewer GetInsideScrollViewer
        {
            get { return this.inside_scrollViewer; }
        }

        public ListView GetListView
        {
            get { return this.listView; }
        }

        public ExtendedListView2()
        {
            this.DefaultStyleKey = typeof(ExtendedListView2);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.rect = GetTemplateChild("rect") as Windows.UI.Xaml.Shapes.Rectangle;
            this.listView = this.GetTemplateChild("listView") as ListView;
            this.offsetForHeader = GetTemplateChild("offsetForHeader") as Windows.UI.Xaml.Shapes.Rectangle;
            this.offsetForFooter = GetTemplateChild("offsetForFooter") as Windows.UI.Xaml.Shapes.Rectangle;
            this.headerGrid = GetTemplateChild("headerGrid") as Grid;
            this.footerGrid = GetTemplateChild("footerGrid") as Grid;
            this.listView.Loaded += listView_Loaded;
            this.listView.SelectionMode = this.SelectionMode;
            if(this.IsFlat)
                this.listView.ItemContainerStyle = (Style)Application.Current.Resources["ListViewItemFlatStyle"];

            //
            this.listView.SelectionChanged += (s,a) => {
                if ((s as ListView).SelectedItem == null)
                    return;

                this.SelectedItem = (s as ListView).SelectedItem;
                };
            //

            if (this.Header != null)
                this.headerGrid.Children.Add(this.Header as UIElement);

            if (this.Footer != null)
                this.footerGrid.Children.Add(this.Footer as UIElement);

            //this.listView.DataFetchSize=5;
            //this.listView.IncrementalLoadingThreshold = 10;
            
            //this.listView.Unloaded += listView_Unloaded;
            base.Unloaded += listView_Unloaded;

            //Получает или задает количество данных, выбираемых для операций виртуализации или упреждающей выборки.
            //DataFetchSize

            // Получает или задает пороговый диапазон, который определяет, когда класс ListViewBase начинает упреждающую выборку дополнительных элементов.
            //IncrementalLoadingThreshold
        }

        void listView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.PercentAction(0);

            this.timer.Stop();
            this.timer.Tick -= this.Timer_Tick;

            if (CustomFrame.Instance == null)
                return;

            CustomFrame.Instance.MenuStateChanged -= MenuStateChanged;
            CustomFrame.Instance.HeaderWithMenu.HeaderHeightChanged -= HeaderWithMenu_HeaderHeightChanged;

            Application.Current.EnteredBackground -= Current_EnteredBackground;
            Application.Current.LeavingBackground -= Current_LeavingBackground;
        }

        //private bool InPivot;

        void listView_Loaded(object sender, RoutedEventArgs e)
        {
            //
            if (CustomFrame.Instance == null)
                return;
            //
            ListView lv = sender as ListView;

            Border border = (Border)VisualTreeHelper.GetChild(lv, 0);
            this.inside_scrollViewer = (ScrollViewer)border.Child;
            
            this.inside_scrollViewer.ViewChanged += sv_ViewChanged;
            this.inside_scrollViewer.ViewChanging += inside_scrollViewer_ViewChanging;
            this.inside_scrollViewer.Loaded += inside_scrollViewer_Loaded;

//#if DEBUG
            this.inside_scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
//#endif

            this.ContentItemsPresenter = (ItemsPresenter)this.inside_scrollViewer.Content;
            //
            this.ActivateAdaptive(this.IsAdaptive);
            //
#if WINDOWS_PHONE_APP
            if (LunaVK.Core.Settings.MenuSwipe == true)
            {
#endif
            if (base.Parent is PivotItem pivotItem)//BugFix: чтобы пивот работал нам не надо менять манипуляцию
            {
                if (pivotItem.Parent is Pivot pivot)
                {
                    pivot.SelectionChanged += Pivot_SelectionChanged;
                }
            }
            else
            { 
                this.ContentItemsPresenter.ManipulationMode |= Windows.UI.Xaml.Input.ManipulationModes.TranslateX;//а манипуляцию мы меняем для бокового меню
            }
#if WINDOWS_PHONE_APP
            }
#endif
            timer.Interval = TimeSpan.FromMilliseconds(15);
            timer.Tick += Timer_Tick;
            if(this.IsPullEnabled)
                timer.Start();



            this.offsetForHeader.Height = this.UseHeaderOffset ? CustomFrame.Instance.HeaderWithMenu.HeaderHeight : 0;
            this.offsetForFooter.Height = this.UseFooterOffset ? CustomFrame.Instance.HeaderWithMenu.HeaderHeight : 0;

            CustomFrame.Instance.HeaderWithMenu.HeaderHeightChanged += HeaderWithMenu_HeaderHeightChanged;

            var temp = lv.GetLogicalChildrenByType<ItemsStackPanel>(false);
            var temp2 = temp.ToList();
            temp2[0].ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepItemsInView;

            Application.Current.EnteredBackground += Current_EnteredBackground;
            Application.Current.LeavingBackground += Current_LeavingBackground;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot pivot = sender as Pivot;
            //bool result = IsVisibileToUser(this, (FrameworkElement)Window.Current.Content);

            // var temp = pivot.ContainerFromItem(pivot.SelectedItem);

            //bool result2 = IsVisibileToUser((FrameworkElement)temp, (FrameworkElement)Window.Current.Content);

            if (base.Parent is PivotItem pivotItem)
            {
                if (pivot.SelectedItem == pivotItem)
                    this.Reload();
            }
        }

        private void Current_LeavingBackground(object sender, Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        {
//            if(!this.loadingTimer.IsEnabled)
//                this.loadingTimer.Start();
        }

        private void Current_EnteredBackground(object sender, Windows.ApplicationModel.EnteredBackgroundEventArgs e)
        {
//            if (this.loadingTimer.IsEnabled)
//                this.loadingTimer.Stop();
        }

        void HeaderWithMenu_HeaderHeightChanged(object sender, double e)
        {
            this.offsetForHeader.Height = this.UseHeaderOffset ? e : 0;
        }

        private CustomFrame CFrame
        {
            get { return Window.Current.Content as CustomFrame; }
        }

        void MenuStateChanged(object sender, CustomFrame.MenuStates e)
        {
            //System.Diagnostics.Debug.WriteLine(" new: " + e.ToString() + "menu tr: " + this._menuTransform.X.ToString());


            if ((byte)e > (byte)CustomFrame.MenuStates.StateMenuCollapsedContentStretch)
            {
                
                if (this.inside_scrollViewer!=null)
                    this.inside_scrollViewer.Padding = new Thickness(200,0,200,0);
                //base.MaxWidth = 800;
            }
            else
            {
                //base.MaxWidth = double.PositiveInfinity;
                if (this.inside_scrollViewer != null)
                    this.inside_scrollViewer.Padding = new Thickness();
            }
        }

        void inside_scrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            bool val = this.IsHorizontal;

            this.inside_scrollViewer.VerticalScrollBarVisibility = val ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
            this.inside_scrollViewer.VerticalScrollMode = val ? ScrollMode.Disabled : ScrollMode.Enabled;
            this.inside_scrollViewer.HorizontalScrollBarVisibility = val ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
            this.inside_scrollViewer.HorizontalScrollMode = val ? ScrollMode.Enabled : ScrollMode.Disabled;



            this.InsideScrollViewerLoaded?.Invoke();
            /*
            if (this.IsAdaptive && ( CustomFrame.Instance.MenuState == CustomFrame.MenuStates.StateMenuNarrowContentFixed || CustomFrame.Instance.MenuState == CustomFrame.MenuStates.StateMenuFixedContentFixed ))
            {
                this.inside_scrollViewer.Padding = new Thickness(200, 0, 200, 0);
            }
            else
            {
                this.inside_scrollViewer.Padding = new Thickness();
            }*/
            //
            //
            //bool result = IsVisibileToUser(this, (FrameworkElement)Window.Current.Content);
            //bool result1 = IsVisibileToUser(this.listView, (FrameworkElement)Window.Current.Content);
            //bool result2 = IsVisibileToUser(this.inside_scrollViewer, (FrameworkElement)Window.Current.Content);
            //bool result2 = IsVisibileToUser(this.offsetForHeader, (FrameworkElement)Window.Current.Content);
            //if(!this.InPivot)

            bool flag = false;
            if (base.Parent is PivotItem pivotItem)
            {
                if (pivotItem.Parent is Pivot pivot)
                {
                    if (pivot.SelectedItem == pivotItem)
                        flag = true;
                }
            }
            else
                flag = true;

            if (flag)
                this.Reload();
        }

        bool IsInertial = false;

        void inside_scrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (e.NextView.VerticalOffset == 0)
            {
                this.IsInertial = e.IsInertial;
                this.IsLocked = false;
                if (!timer.IsEnabled && this.IsPullEnabled)
                    timer.Start();
            }
            else
            {
                if (timer.IsEnabled)
                {
                    timer.Stop();
                }

                this.PercentAction(0);
            }
        }

        private bool IsLocked;

        private async void Timer_Tick(object sender, object e)
        {
            if (this.InLoading || this.IsLocked)
                return;

            var ttv = this.rect.TransformToVisual(this);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            screenCoords.Y += 1;//- margin
            if (this.ReversPull)
            {
                screenCoords.Y -= this.inside_scrollViewer.ActualHeight;
                screenCoords.Y *= (-1);
            }

            if (this.IsInertial )
            {
                if (screenCoords.Y < 1)
                    this.IsInertial = false;

                return;
            }
            //
            /*
            //BugFix
            if (this.Parent is PivotItem pivotItem)
            {
                if (pivotItem.Parent is Pivot pivot)//BugFix: если это DataTemplate
                {
                    if (pivot.SelectedItem != pivotItem)
                        return;
                }
                    //screenCoords.Y -= CustomFrame.Instance.HeaderWithMenu.HeaderHeight;//вычитаем высоту шапки если мы в пивоте
                    //screenCoords.Y -= 30;//и название страницы пивота
                    double offset = CustomFrame.Instance.ActualHeight - pivotItem.ActualHeight;
                    screenCoords.Y -= offset;
                
            }
            */
            double percent = (screenCoords.Y / offsetTreshhold * 100.0);
            if (percent < 0)
            {
                this.PercentAction(0);
                this.timer.Stop();
                return;
            }

            if (this.lastpullvalue == percent)
                return;

            if (percent < 1)
            {
                this.PercentAction(0);
                return;
            }

            if (percent >= 100.0)
            {
                this.IsLocked = true;
                CustomFrame.Instance.HeaderWithMenu.ShowProgress(true);
                this.PercentAction(0);
                this.InLoading = true;
                
                if (base.DataContext is ISupportUpDownIncrementalLoading collection)
                {
                    await collection.Reload();
                }

                CustomFrame.Instance.HeaderWithMenu.ShowProgress(false);
                this.InLoading = false;
                return;
            }
            this.PercentAction(percent);
            
            this.lastpullvalue = percent;
        }

        private void PercentAction(double percent)
        {
            this.OnPullPercentageChanged?.Invoke(percent);
        }
        
#if WINDOWS_UWP
        private const double offsetTreshhold = 30;
#else
        private const double offsetTreshhold = 100;
#endif
        public Action<double> OnPullPercentageChanged { get; set; }

        /*
        private async void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate || this.InLoading || this.PreventScroll)
                return;

            ScrollViewer sv = sender as ScrollViewer;

            if (sv.ScrollableHeight - sv.VerticalOffset < 700)
            {
                if (base.DataContext is ISupportLoadMore more)
                {
                    if (more.HasMoreItems == false)
                        return;

                    this.InLoading = true;
                    await more.LoadData();
                    this.InLoading = false;
                }
            }
            else if (sv.VerticalOffset < 130)
            {
                if (base.DataContext is ISupportLoadBack back)
                {
                    if (back.HasBackItems == false)
                        return;

                    this.InLoading = true;
                    await back.LoadDataBack();
                    this.InLoading = false;
                }
            }
        }
        */

        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate || this.InLoading || this.PreventScroll)
                return;

            ScrollViewer sv = sender as ScrollViewer;

            if (sv.ScrollableHeight - sv.VerticalOffset < 700)
            {
                ProcessDown();
            }
            else if (sv.VerticalOffset < 400)
            {
                ProcessUp();
            }
        }
        

        public bool NeedReload = true;

        private bool IsVisibileToUser(FrameworkElement element, FrameworkElement container)
        {
            if (element == null || container == null)
                return false;


            Rect elementBounds = element.TransformToVisual(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect containerBounds = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);

            return (elementBounds.Left < containerBounds.Right && elementBounds.Right > containerBounds.Left);
        }

        /// <summary>
        /// Взывается при срабатывании таймера.
        /// А самый первый раз после Loaded этого ЛистВиев
        /// </summary>
        private void LoadingTimer_Tick(object sender, object e)
        {
            if (this.InLoading || this.PreventScroll)
                return;

            if(base.Parent is PivotItem pivotItem)
            {
                if(pivotItem.Parent is Pivot pivot)
                {
                    if (pivot.SelectedItem != pivotItem)
                        return;
                }
                else
                {
                    bool result = IsVisibileToUser(this, (FrameworkElement)Window.Current.Content);
                    if (!result)
                        return;
                }
            }

            

            if (/*this.inside_scrollViewer.ScrollableHeight == 0*/this.NeedReload)
                this.Reload();
            else if (this.inside_scrollViewer.ScrollableHeight - this.inside_scrollViewer.VerticalOffset < 700)
                this.ProcessDown();
            else if (this.inside_scrollViewer.VerticalOffset < 400)
                this.ProcessUp();
        }

        public async void Reload()
        {
            if (this.InLoading || this.PreventScroll || !this.NeedReload)
                return;


            if (base.DataContext is ISupportUpDownIncrementalLoading collection)
            {
                //if (collection.HasMoreDownItems)
                //{
                    this.InLoading = true;
                    var itemToScroll = await collection.Reload();
                    if (itemToScroll != null)
                        await ScrollToItem(listView, itemToScroll);

                    await Task.Delay(1000);
                    this.NeedReload = false;
                    this.InLoading = false;
                //}
            }
        }

        /// <summary>
        /// Обработать элементы вверху.
        /// </summary>
        private async void ProcessUp()
        {
            if (base.DataContext is ISupportUpDownIncrementalLoading collection)
            {
                if(collection.HasMoreUpItems)
                {
                    this.InLoading = true;
                    await collection.LoadUpAsync();
                    await Task.Delay(1000);
                    this.InLoading = false;
                }
            }
        }

        /// <summary>
        /// Обработать элементы внизу.
        /// </summary>
        private async void ProcessDown()
        {
            if (base.DataContext is ISupportUpDownIncrementalLoading collection)
            {
                if (collection.HasMoreDownItems)
                {
                    this.InLoading = true;
                    await collection.LoadDownAsync();
                    await Task.Delay(1000);
                    this.InLoading = false;
                }
            }
        }

        public Point? GetItemPosition(object item)
        {
            ListViewBase listViewBase = this.listView;

            var scrollViewer = listViewBase.GetScrollViewer(); // get the ScrollViewer withtin the ListView/GridView
            var selectorItem = listViewBase.ContainerFromItem(item) as SelectorItem; // get the SelectorItem to scroll to

            // when it's null, means virtualization is ON and the item hasn't been realized yet
            if (selectorItem == null)
            {
                return null;
            }

            // calculate the position object in order to know how much to scroll to
            var transform = selectorItem.TransformToVisual(Window.Current.Content);
            var position = transform.TransformPoint(new Point(0, 0));

            return position;
        }

#region ScrollToItem
        //https://stackoverflow.com/questions/32557216/windows-10-scrollintoview-is-not-scrolling-to-the-items-in-the-middle-of-a-lis
        public async Task ScrollToItem(ListViewBase listViewBase, object item)
        {
            bool isVirtualizing = false;
            double previousHorizontalOffset = default(double), previousVerticalOffset = default(double);
            
            var scrollViewer = listViewBase.GetScrollViewer(); // get the ScrollViewer withtin the ListView/GridView
            
            var selectorItem = listViewBase.ContainerFromItem(item) as SelectorItem; // get the SelectorItem to scroll to

            // when it's null, means virtualization is ON and the item hasn't been realized yet
            if (selectorItem == null)
            {
                isVirtualizing = true;

                previousHorizontalOffset = scrollViewer.HorizontalOffset;
                previousVerticalOffset = scrollViewer.VerticalOffset;
                
                await ScrollIntoViewAsync(listViewBase,item); // call task-based ScrollIntoViewAsync to realize the item
                
                selectorItem = (SelectorItem)listViewBase.ContainerFromItem(item); // this time the item shouldn't be null again
            }

            // calculate the position object in order to know how much to scroll to
            var transform = selectorItem.TransformToVisual((UIElement)scrollViewer.Content);
            var position = transform.TransformPoint(new Point(0, 0));

            // when virtualized, scroll back to previous position without animation
            if (isVirtualizing)
            {
                await ChangeViewAsync(scrollViewer,previousHorizontalOffset, previousVerticalOffset, true);
            }
            
            scrollViewer.ChangeView(position.X, position.Y, null); // scroll to desired position with animation!
        }

        public async Task ScrollIntoViewAsync(ListViewBase listViewBase, object item)
        {
            var tcs = new TaskCompletionSource<object>();
            var scrollViewer = listViewBase.GetScrollViewer();

            EventHandler<ScrollViewerViewChangedEventArgs> viewChanged = (s, e) => tcs.TrySetResult(null);
            try
            {
                scrollViewer.ViewChanged += viewChanged;
                listViewBase.ScrollIntoView(item, ScrollIntoViewAlignment.Leading);
                await tcs.Task;
            }
            finally
            {
                scrollViewer.ViewChanged -= viewChanged;
            }
        }

        public async Task ChangeViewAsync(ScrollViewer scrollViewer, double? horizontalOffset, double? verticalOffset, bool disableAnimation)
        {
            var tcs = new TaskCompletionSource<object>();

            EventHandler<ScrollViewerViewChangedEventArgs> viewChanged = (s, e) => tcs.TrySetResult(null);
            try
            {
                scrollViewer.ViewChanged += viewChanged;
                scrollViewer.ChangeView(horizontalOffset, verticalOffset, null, disableAnimation);
                await tcs.Task;
            }
            finally
            {
                scrollViewer.ViewChanged -= viewChanged;
            }
        }
#endregion
    }
}
