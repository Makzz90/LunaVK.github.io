using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace LunaVK.UC.Controls
{
    public partial class ExtendedListView3 : ItemsControl, ISemanticZoomInformation
    {
#region HeaderTemplate
        /// <summary>
        /// Gets or sets the DataTemplate used to display the details.
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="MasterTemplate"/> dependency property.
        /// </summary>
        /// <returns>The identifier for the <see cref="MasterTemplate"/> dependency property.</returns>
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(ExtendedListView3), new PropertyMetadata(null));
        #endregion

#region ReorderMode
        public ListViewReorderMode ReorderMode
        {
            get { return (ListViewReorderMode)GetValue(ReorderModeProperty); }
            set { SetValue(ReorderModeProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="MasterTemplate"/> dependency property.
        /// </summary>
        /// <returns>The identifier for the <see cref="MasterTemplate"/> dependency property.</returns>
        public static readonly DependencyProperty ReorderModeProperty = DependencyProperty.Register(nameof(ReorderMode), typeof(ListViewReorderMode), typeof(ExtendedListView3), new PropertyMetadata(ListViewReorderMode.Disabled));
#endregion



#region CanReorderItems
        public static readonly DependencyProperty CanReorderItemsProperty = DependencyProperty.Register(nameof(CanReorderItems), typeof(bool), typeof(ExtendedListView3), new PropertyMetadata(false));
        public bool CanReorderItems
        {
            get { return (bool)GetValue(CanReorderItemsProperty); }
            set { SetValue(CanReorderItemsProperty, value); }
        }
#endregion

#region Header
        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register( nameof(Header), typeof(object), typeof(ExtendedListView3), new PropertyMetadata(null));
#endregion

#region Footer
        public object Footer
        {
            get { return GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        public static readonly DependencyProperty FooterProperty = DependencyProperty.Register(nameof(Footer), typeof(object), typeof(ExtendedListView3), new PropertyMetadata(null));
#endregion

#region IsPullEnabled
        public static readonly DependencyProperty IsPullEnabledProperty = DependencyProperty.Register("IsPullEnabled", typeof(bool), typeof(ExtendedListView3), new PropertyMetadata(true, IsPullEnabledChanged));

        public bool IsPullEnabled
        {
            get { return (bool)base.GetValue(IsPullEnabledProperty); }
            set { base.SetValue(IsPullEnabledProperty, value); }
        }

        private static void IsPullEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExtendedListView3 lv = (ExtendedListView3)d;
            bool val = (bool)e.NewValue;
            lv.ActivateTimer(val);
        }

        public void ActivateTimer(bool status)
        {
            if (status == true)
            {
                if (!timer.IsEnabled)
                    timer.Start();
            }
            else
            {
                this.timer.Stop();
            }
        }
#endregion

#region IsHorizontal
        public bool IsHorizontal
        {
            get { return (bool)base.GetValue(IsHorizontalProperty); }
            set { base.SetValue(IsHorizontalProperty, value); }
        }

        public static readonly DependencyProperty IsHorizontalProperty = DependencyProperty.Register(nameof(IsHorizontal), typeof(bool), typeof(ExtendedListView3), new PropertyMetadata(false));
#endregion

#region UseHeaderOffset
        public static readonly DependencyProperty UseHeaderOffsetProperty = DependencyProperty.Register(nameof(UseHeaderOffset), typeof(bool), typeof(ExtendedListView3), new PropertyMetadata(true));

        public bool UseHeaderOffset
        {
            get { return (bool)GetValue(UseHeaderOffsetProperty); }
            set
            {
                //
                if (this.offsetForHeader != null)
                    this.offsetForHeader.Height = value ? CustomFrame.Instance.Header.HeaderHeight : 0;
                //
                SetValue(UseHeaderOffsetProperty, value);
            }
        }
#endregion

#region SelectionMode
        public ListViewSelectionMode SelectionMode
        {
            get { return (ListViewSelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register(nameof(SelectionMode), typeof(ListViewSelectionMode), typeof(ExtendedListView3), new PropertyMetadata(ListViewSelectionMode.None));
#endregion

#region IsFlat
        public static readonly DependencyProperty IsFlatProperty = DependencyProperty.Register(nameof(IsFlat), typeof(bool), typeof(ExtendedListView3), new PropertyMetadata(default(bool)));
        public bool IsFlat
        {
            get { return (bool)GetValue(IsFlatProperty); }
            set { SetValue(IsFlatProperty, value); }
        }
#endregion

#region NoContent
        /// <summary>
        /// Gets or sets the content to dsiplay when there is no item selected in the master list.
        /// </summary>
        public object NoContent
        {
            get { return GetValue(NoContentProperty); }
            set { SetValue(NoContentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="NoSelectionContent"/> dependency property.
        /// </summary>
        /// <returns>The identifier for the <see cref="NoSelectionContent"/> dependency property.</returns>
        public static readonly DependencyProperty NoContentProperty = DependencyProperty.Register(nameof(NoContent), typeof(object), typeof(ExtendedListView3), new PropertyMetadata(null));
        #endregion

#region NeedReload
        public bool NeedReload
        {
            get { return (bool)base.GetValue(NeedReloadProperty); }
            set { base.SetValue(NeedReloadProperty, value); }
        }

        public static readonly DependencyProperty NeedReloadProperty = DependencyProperty.Register(nameof(NeedReload), typeof(bool), typeof(ExtendedListView3), new PropertyMetadata(true));
#endregion

        //private event SelectionChangedEventHandler _selectionChanged;
        public event SelectionChangedEventHandler SelectionChanged;

        private Windows.UI.Xaml.Shapes.Rectangle rect;
        DispatcherTimer timer = new DispatcherTimer();
        private bool InLoading;
        private bool IsLocked;
        private bool IsInertial = false;
        private double lastpullvalue = 0.0;
        private ListView listView;
        public Action<double> OnPullPercentageChanged;
        private ScrollViewer inside_scrollViewer;
        public ItemsPresenter ContentItemsPresenter { get; private set; }
        public bool PreventScroll;
        private Windows.UI.Xaml.Shapes.Rectangle offsetForHeader;
        private FooterUC _footer;
        private LoadingUC _loading;
        private ContentPresenter NoContentPresenter;
        private DelayedExecutor _de = new DelayedExecutor(600);


#if WINDOWS_UWP
        private const double offsetTreshhold = 30;
#else
        private const double offsetTreshhold = 100;
#endif

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            this.rect = GetTemplateChild("rect") as Windows.UI.Xaml.Shapes.Rectangle;
            this.listView = this.GetTemplateChild("listView") as ListView;
            this._footer = this.GetTemplateChild("_footer") as FooterUC;
            this._loading = this.GetTemplateChild("_loading") as LoadingUC;
            this.NoContentPresenter = this.GetTemplateChild("NoContentPresenter") as ContentPresenter;
            
            this.listView.SelectionChanged += this.SelectionChanged;
            //
            if (Application.Current.Resources.ContainsKey("ListViewItemRevealStyle"))
            {
                var reveal = (Style)Application.Current.Resources["ListViewItemRevealStyle"];
                /*
                if (reveal != null)
                {
                    Setter search = null;
                    foreach (var set in reveal.Setters)
                    {
                        if(set is Setter setter)
                        {
                            var type = setter.Property.GetType();
                            int i = 0;
                        }
                    }

                    if (search != null)
                        reveal.Setters.Remove(search);
                    this.listView.ItemContainerStyle = reveal;
                }*/
                //ListViewItemPresenter p = new ListViewItemPresenter();
                //p.RevealBackground = new 
            }

            //
            //if (this.listView.ItemsPanel==null)
            //{
            //    ItemsPanelTemplate itemsPanelTemplate = XamlReader.Load(@"<ItemsPanelTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'> <VirtualizingStackPanel/> </ItemsPanelTemplate>") as ItemsPanelTemplate;
            //    this.listView.ItemsPanel = itemsPanelTemplate;
            //}
            if (base.ItemsPanel != null)
            {
                this.listView.ItemsPanel = base.ItemsPanel;
            }

            this.offsetForHeader = GetTemplateChild("offsetForHeader") as Windows.UI.Xaml.Shapes.Rectangle;
            this.listView.Loaded += this.ListView_Loaded;

            if (this.GroupStyle.Count>0)
            {
                this.listView.GroupStyle.Clear();
                foreach (var s in this.GroupStyle)
                    this.listView.GroupStyle.Add(s);
            }

            this.Unloaded += this.ExtendedListView3_Unloaded;
        }
        
        private void ExtendedListView3_Unloaded(object sender, RoutedEventArgs e)
        {
            if (CustomFrame.Instance == null)
                return;

            this.listView.Loaded -= this.ListView_Loaded;
            this.listView.SelectionChanged -= this.SelectionChanged;

            Debug.Assert(this.inside_scrollViewer != null);

            //if (this.inside_scrollViewer != null)//очень странный баг
            //{
                this.inside_scrollViewer.ViewChanged -= sv_ViewChanged;
                this.inside_scrollViewer.ViewChanging -= inside_scrollViewer_ViewChanging;
                this.inside_scrollViewer.Loaded -= inside_scrollViewer_Loaded;
            //}
            
            CustomFrame.Instance.Header.HeaderHeightChanged -= HeaderWithMenu_HeaderHeightChanged;
            

            if (this.timer.IsEnabled)
                this.timer.Stop();

            this.timer.Tick -= Timer_Tick;

            if (base.DataContext is ISupportDownIncrementalLoading incrementalLoading)
            {
                incrementalLoading.LoadingStatusUpdated -= this.HandleLoadingStatusUpdated;
                base.DataContextChanged -= this.ExtendedListView3_DataContextChanged;
            }

            if (base.Parent is PivotItem pivotItem)
            {
                if (pivotItem.Parent is Pivot pivot)
                {
                    pivot.SelectionChanged -= Pivot_SelectionChanged;
                }
            }

            this._loading.TryAgainCmd = null;
            this._footer.TryAgainCmd = null;
        }

        /// <summary>
        /// ScrollViewer загружен
        /// </summary>
        public event RoutedEventHandler Loaded2;

        public ListView GetListView
        {
            get { return this.listView; }
        }

        /// <summary>
        /// Это область прокрутки внутри ListView
        /// </summary>
        public ScrollViewer GetInsideScrollViewer
        {
            get { return this.inside_scrollViewer; }
        }

        private int _needScroll;

        private void ListView_Loaded(object sender, RoutedEventArgs e)
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
            //this.inside_scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //#endif

            this.ContentItemsPresenter = (ItemsPresenter)this.inside_scrollViewer.Content;
            //var tempChild = (ContentControl) VisualTreeHelper.GetChild(this.ContentItemsPresenter, 0);
            //tempChild.Background = new SolidColorBrush(Windows.UI.Colors.Red);
            //
//            this.ActivateAdaptive(this.IsAdaptive);
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
                if(!this.IsHorizontal)
                    this.ContentItemsPresenter.ManipulationMode |= Windows.UI.Xaml.Input.ManipulationModes.TranslateX;//а манипуляцию мы меняем для бокового меню
            }
#if WINDOWS_PHONE_APP
            }
#endif
            timer.Interval = TimeSpan.FromMilliseconds(15);
            timer.Tick += Timer_Tick;
            if (this.IsPullEnabled)
                timer.Start();


            
            //            this.offsetForFooter.Height = this.UseFooterOffset ? CustomFrame.Instance.HeaderWithMenu.HeaderHeight : 0;

            CustomFrame.Instance.Header.HeaderHeightChanged += HeaderWithMenu_HeaderHeightChanged;
            
            //Application.Current.EnteredBackground += Current_EnteredBackground;
            //Application.Current.LeavingBackground += Current_LeavingBackground;

            if(base.DataContext is ISupportDownIncrementalLoading incrementalLoading)
            {
                incrementalLoading.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;
                base.DataContextChanged += this.ExtendedListView3_DataContextChanged;
            }

            this._loading.TryAgainCmd = this.Reload;
            this._footer.TryAgainCmd = this.Reload;

			WindowTitleUC.OnFullScreenMode += WindowTitleUC_OnFullScreenMode;
			WindowTitleUC.OnWindowedMode += WindowTitleUC_OnWindowedMode;
        }

		private void WindowTitleUC_OnWindowedMode()
		{
            offsetForHeader.Height = 80;
        }

		private void WindowTitleUC_OnFullScreenMode()
        {
            offsetForHeader.Height = 48;
        }

		private void ExtendedListView3_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            //todo: возможная утечка памяти: а если DataContext у списка станет пустышкой, то мы не отпишеся от LoadingStatusUpdated
            if (args.NewValue!=null)
            {
                if (args.NewValue is ISupportDownIncrementalLoading incrementalLoading)
                {
                    incrementalLoading.LoadingStatusUpdated -= this.HandleLoadingStatusUpdated;
                    incrementalLoading.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;
                    /*
                    if(incrementalLoading.CurrentLoadingStatus == ProfileLoadingStatus.Empty)
                    {
                        this.NoContentPresenter.Visibility = Visibility.Visible;
                        return;
                    }
                    */
                    this.HandleLoadingStatusUpdated(incrementalLoading.CurrentLoadingStatus == ProfileLoadingStatus.Reloading ? ProfileLoadingStatus.Loaded : incrementalLoading.CurrentLoadingStatus);
                }
            }

            this.NoContentPresenter.Visibility = Visibility.Collapsed;
        }

        private void TryAgain()
        {
            if(base.DataContext is ISupportReload reload)
            {
                this.NeedReload = true;
                reload.Reload();
            }
        }
        
        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {
            if (status == ProfileLoadingStatus.Empty)
                this.NoContentPresenter.Visibility = Visibility.Visible;
            else
                this.NoContentPresenter.Visibility = Visibility.Collapsed;

            string str = status.ToString();
            VisualStateManager.GoToState(this._footer, str, false);

            if (status == ProfileLoadingStatus.Reloading || status == ProfileLoadingStatus.ReloadingFailed)
            {
                if (base.ItemsSource is IList list)
                {
                    if (list.Count == 0)
                        VisualStateManager.GoToState(this._loading, str, false);
                }
                else
                {
                    VisualStateManager.GoToState(this._loading, str, false);//todo: этой строки тут не было, но для работы группированных списокв она нужна. Не мешает где ещё?
                }
            }
            else
            {
                VisualStateManager.GoToState(this._loading, str, false);
            }
        }

        void HeaderWithMenu_HeaderHeightChanged(object sender, double e)
        {
            this.offsetForHeader.Height = this.UseHeaderOffset ? e : 0;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot pivot = sender as Pivot;
            
            if (base.Parent is PivotItem pivotItem)
            {
                if (pivot.SelectedItem == pivotItem)
                {
                    //Что если мы переключились на страницу, на которой уже были?
                    if (base.DataContext is ISupportDownIncrementalLoading collection)
                    {
                        //если мы уже загрузили данные и оказалось что список пуст в базе вк, то не надо ещё раз загружать
                        if (collection.CurrentLoadingStatus!= ProfileLoadingStatus.Empty)
                        {
                            if (base.ItemsSource is IList list)
                            {
                                if (list.Count == 0)
                                    this.Reload();
                            }
                        }
                    }


                    pivot.SelectionChanged -= Pivot_SelectionChanged;
                }
            }
        }

        private async void Timer_Tick(object sender, object e)
        {
            if (this.InLoading || this.IsLocked)
                return;

            var ttv = this.rect.TransformToVisual(this);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            screenCoords.Y += 1;//- margin

            if (this.IsInertial)
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
            //
            double percent = (screenCoords.Y / offsetTreshhold * 100.0);
            if (percent < 0)
            {
                this.PercentAction(0);
                this.timer.Stop();
                return;
            }

            if (this.lastpullvalue == percent)
                return;

            this.lastpullvalue = percent;

            if (percent < 2)
            {
                this.PercentAction(0);
                return;
            }

            if (percent >= 100.0)
            {
                /*
                this.IsLocked = true;//Bug: если DataContext не ISupportUpDownIncrementalLoading, то блочится навсегда
                CustomFrame.Instance.HeaderWithMenu.ShowProgress(true);
                this.PercentAction(0);
                this.InLoading = true;

                if (base.DataContext is ISupportUpDownIncrementalLoading collection)
                {
                    await collection.Reload();
                }

                CustomFrame.Instance.HeaderWithMenu.ShowProgress(false);
                this.InLoading = false;
                */

                this.IsLocked = true;
                this.NeedReload = true;
                this.Reload();
                this.PercentAction(0);
                return;
            }
            this.PercentAction(percent);
        }

        private void PercentAction(double percent)
        {
            this.OnPullPercentageChanged?.Invoke(percent);
        }

        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate || this.InLoading || this.PreventScroll)
                return;

            ScrollViewer sv = sender as ScrollViewer;

            if (this.IsHorizontal)
            {
                if (sv.ScrollableWidth - sv.HorizontalOffset < 700)
                {
                    this.ProcessDown();
                }
                else if (sv.HorizontalOffset < 400)
                {
                    this.ProcessUp();
                }
            }
            else
            {
                if (sv.ScrollableHeight - sv.VerticalOffset < 700)
                {
                    this.ProcessDown();
                }
                else if (sv.VerticalOffset < 400)
                {
                    this.ProcessUp();
                }
            }
        }

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

        private void ActivateIsHorizontal(bool val)
        {
            this.inside_scrollViewer.VerticalScrollBarVisibility = val ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
            this.inside_scrollViewer.VerticalScrollMode = val ? ScrollMode.Disabled : ScrollMode.Enabled;
            this.inside_scrollViewer.HorizontalScrollBarVisibility = val ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
            this.inside_scrollViewer.HorizontalScrollMode = val ? ScrollMode.Enabled : ScrollMode.Disabled;
            /*
             * <ListView.ItemsPanel>
                    <ItemsPanelTemplate>
                        <StackPanel Orientation="Horizontal"/>
                    </ItemsPanelTemplate>
                </ListView.ItemsPanel>
                */
            var temp = this.ContentItemsPresenter.FindChild<ItemsStackPanel>();
            int i = 9;
            if(temp is ItemsStackPanel stack)
            {
                stack.Orientation = val ? Orientation.Horizontal : Orientation.Vertical;
            }
        }

        void inside_scrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded2?.Invoke(sender, e);

            this.ActivateIsHorizontal(this.IsHorizontal);



//            this.InsideScrollViewerLoaded?.Invoke();

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

            //
            //
            if(this._forceToBottom)
            {
                this._forceToBottom = false;
                this.ScrollToBottom();
            }
            else if(this._forceToItem!=null)
            {
                this.ScrollToItem(this.listView, this._forceToItem);
                this._forceToItem = null;
            }
        }

        public void UpdteHookLoadingStatus()
        {
            if (base.DataContext is ISupportDownIncrementalLoading incrementalLoading)
            {
                incrementalLoading.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;
                //base.DataContextChanged += this.ExtendedListView3_DataContextChanged;
            }
        }


        /// <summary>
        /// Происходит при переключении страницы Пивота, PullToRefresh, загрузке ScrollView
        /// </summary>
        public async void Reload()
        {
            if (this.InLoading || this.PreventScroll || !this.NeedReload)
                return;


            if (base.DataContext is ISupportUpDownIncrementalLoading collection)
            {
                this.InLoading = true;
                var itemToScroll = await collection.Reload();
                if (itemToScroll != null)
                    await ScrollToItem(this.listView, itemToScroll);

                await Task.Delay(1000);
                this.NeedReload = false;
                this.InLoading = false;
            }
            else if (base.DataContext is ISupportReload reload)
            {
                reload.Reload();
            }
            else if (base.DataContext is ISupportDownIncrementalLoading collection2)
            {
                collection2.Reload();//collection2.LoadDownAsync(true);
            }
        }

        /// <summary>
        /// Обработать элементы вверху.
        /// </summary>
        private async void ProcessUp()
        {
            if (base.DataContext is ISupportUpDownIncrementalLoading collection)
            {
                if (collection.HasMoreUpItems)
                {
                    this.InLoading = true;
                    await collection.LoadUpAsync();
                    await Task.Delay(1000);
                    this.InLoading = false;
                }
            }
            else if (base.DataContext is ISupportUpIncrementalLoading collection2)
            {
                if (collection2.CurrentLoadingStatus == ProfileLoadingStatus.Loaded && collection2.HasMoreUpItems)
                {
                    if (!this._de.IsActive)
                        this._de.AddToDelayedExecution(() => { collection2.LoadUpAsync(); });
                    //collection2.LoadUpAsync();
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
            else if (base.DataContext is ISupportDownIncrementalLoading collection2)
            {
                if (collection2.CurrentLoadingStatus == ProfileLoadingStatus.Loaded && collection2.HasMoreDownItems)
                {
                    if(!this._de.IsActive)
                        this._de.AddToDelayedExecution(() => { collection2.LoadDownAsync(); } );
                    //collection2.LoadDownAsync();
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
        private async Task ScrollToItem(ListViewBase listViewBase, object item)
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

                await ScrollIntoViewAsync(listViewBase, item); // call task-based ScrollIntoViewAsync to realize the item

                selectorItem = (SelectorItem)listViewBase.ContainerFromItem(item); // this time the item shouldn't be null again
            }

            //
            if (selectorItem == null)
                return;//Странный баг
            //
            // calculate the position object in order to know how much to scroll to
            var transform = selectorItem.TransformToVisual((UIElement)scrollViewer.Content);
            var position = transform.TransformPoint(new Point(0, 0));

            // when virtualized, scroll back to previous position without animation
            if (isVirtualizing)
            {
                await ChangeViewAsync(scrollViewer, previousHorizontalOffset, previousVerticalOffset, true);
            }

            scrollViewer.ChangeView(position.X, position.Y, null, true);
            scrollViewer.ChangeView(position.X, position.Y, null, true);
            //
            this._forceToItem = null;
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
                double v = scrollViewer.VerticalOffset;
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
                double v = scrollViewer.VerticalOffset;
            }
            finally
            {
                scrollViewer.ViewChanged -= viewChanged;
            }
        }

        public void ScrollTo(double offset)
        {
            if (offset < 0.0)
                offset = 0.0;
            //this._changingVerticalOffset = true;
            this.inside_scrollViewer.ChangeView(null, offset,null);
            //this._changingVerticalOffset = false;
            //this.PerformLoadUnload(VirtualizableState.LoadedFully);
        }

        private bool _forceToBottom;
        private object _forceToItem;

        public void ScrollToBottom(bool toBottom = true, bool disableAnimation = true)
        {
            //if (this._isScrollingToUpOrBottom)
            //    return;
            //this._isScrollingToUpOrBottom = true;
            //this.EnsureFocusIsOnPage();
            //this._notReactToScroll = true;
            //this._savedDelta = this.DeltaOffset;
            //this.DeltaOffset = !toBottom ? -this._listScrollViewer.VerticalOffset : this._listScrollViewer.ExtentHeight - this._listScrollViewer.ViewportHeight - this._listScrollViewer.VerticalOffset;
            //this.Log("PrepareForScrollToBottom");
            //this.PerformLoadUnload2(VirtualizableState.LoadedPartially, true);
            //this.UnbindFromScrollViewer();
            //this.ScrollViewer.ScrollToTopOrBottom(toBottom, (Action)(() =>
            //{
            //    this._isScrollingToUpOrBottom = false;
            //    this.ScrollToBottomCompleted(toBottom);
            //}));

            //Debug.Assert(this.inside_scrollViewer!=null);
            if (this.inside_scrollViewer == null)
            {
                this._forceToBottom = true;
                return;
            }

            double to = this.inside_scrollViewer.ScrollableHeight == 0 ? this.inside_scrollViewer.ActualHeight : this.inside_scrollViewer.ScrollableHeight;
            bool res = this.inside_scrollViewer.ChangeView(null, to, null, disableAnimation);
            int i = 0;
        }

        public async void ScrollToItem(object item)
        {
            if (this.inside_scrollViewer == null)
            {
                this._forceToItem = item;
                return;
            }
            await ScrollToItem(this.listView, item);
        }
 #endregion



#region ISemanticZoomInformation
        
        public void CompleteViewChange()
        {
            
        }

        public void CompleteViewChangeFrom(SemanticZoomLocation source, SemanticZoomLocation destination)
        {
            this.IsActiveView = false;
        }

        public void CompleteViewChangeTo(SemanticZoomLocation source, SemanticZoomLocation destination)
        {
            this.IsActiveView = true;
        }

        public void InitializeViewChange()
        {
            
        }

        public bool IsActiveView { get; set; }

        public bool IsZoomedInView { get; set; }

        public void MakeVisible(SemanticZoomLocation item)
        {
            this.SemanticZoomOwner.IsZoomedInViewActive = (this.Equals(this.SemanticZoomOwner.ZoomedInView));

            /*
            if (item.Bounds.Left != 0.5)
            {
                if (base.Items.Count == 1)
                {
                    foreach (UIElement element in this.Children)
                    {
                        if (element.GetType() == typeof(ScrollViewer))
                        {
                            ((ScrollViewer)element).ScrollToHorizontalOffset(item.Bounds.Left);
                        }
                    }
                }
            }
            */
            
            if(item.Item is IList list)
            {
                this.ScrollIntoViewAsync(this.listView, list[0]);
            }
        }

        public SemanticZoom SemanticZoomOwner { get; set; }

        public void StartViewChangeFrom(SemanticZoomLocation source, SemanticZoomLocation destination)
        {
            
        }

        public void StartViewChangeTo(SemanticZoomLocation source, SemanticZoomLocation destination)
        {
            
        }

#endregion


    }



}
