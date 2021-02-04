using LunaVK.Core;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace LunaVK.UC.Controls
{
    public partial class ExtendedGridView2 : ItemsControl
    {
        private bool InLoading;
        public bool NeedReload = true;
        private bool IsLocked;
        private bool IsInertial = false;
        private double lastpullvalue = 0.0;
        public event SelectionChangedEventHandler SelectionChanged;
        private FooterUC _footer;
        private LoadingUC _loading;
        private ContentPresenter NoContentPresenter;
        private Rectangle rect;
        private GridView gridView;
        private ScrollViewer inside_scrollViewer;
        private Rectangle offsetForHeader;
        DispatcherTimer timer = new DispatcherTimer();
        public ItemsPresenter ContentItemsPresenter { get; private set; }

        //Контейнер очень крупный и лучше через таймер обновлять
        private DispatcherTimer loadingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };

        public Action<double> OnPullPercentageChanged;
        private Core.Utils.DelayedExecutor _de = new Core.Utils.DelayedExecutor(350);

        /// <summary>
        /// ScrollViewer загружен
        /// </summary>
        public event RoutedEventHandler Loaded2;
        bool small = Settings.UI_SmallPreview;

#if WINDOWS_UWP
        private const double offsetTreshhold = 30;
#else
        private const double offsetTreshhold = 100;
#endif

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.rect = GetTemplateChild("rect") as Rectangle;
            this.gridView = this.GetTemplateChild("gridView") as GridView;
            this._footer = this.GetTemplateChild("_footer") as FooterUC;
            this._loading = this.GetTemplateChild("_loading") as LoadingUC;
            this.NoContentPresenter = this.GetTemplateChild("NoContentPresenter") as ContentPresenter;
            this.offsetForHeader = GetTemplateChild("offsetForHeader") as Rectangle;

            base.Loaded += ExtendedGridView2_Loaded;
            base.Unloaded += ExtendedGridView2_Unloaded;

            this.gridView.SelectionChanged += this.SelectionChanged;
            this.gridView.Loaded += GridView_Loaded;
            this.gridView.SizeChanged += GridView_SizeChanged;
        }
        

        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gv = sender as GridView;
            var panel = (ItemsWrapGrid)gv.ItemsPanelRoot;

            //     panel.Orientation = Orientation.Horizontal;

            double colums = e.NewSize.Width / this.ItemWidth;
            if (this.small)
                colums *= 1.3;

            panel.MaximumRowsOrColumns = (int)colums;

            panel.ItemWidth = e.NewSize.Width / (int)colums;
            double percent = panel.ItemWidth / this.ItemWidth;
            panel.ItemHeight = percent * this.ItemHeight;
        }

        private void GridView_Loaded(object sender, RoutedEventArgs e)
        {
            if (CustomFrame.Instance == null)
                return;

            GridView lv = sender as GridView;

            Border border = (Border)VisualTreeHelper.GetChild(lv, 0);
            this.inside_scrollViewer = (ScrollViewer)border.Child;
            this.inside_scrollViewer.Loaded += this.inside_scrollViewer_Loaded;


            CustomFrame.Instance.Header.HeaderHeightChanged += HeaderWithMenu_HeaderHeightChanged;

            //this.inside_scrollViewer.ViewChanged += sv_ViewChanged;
            if (base.DataContext is ISupportDownIncrementalLoading incrementalLoading)
            {
                incrementalLoading.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;
            }


            GridView gv = sender as GridView;
            var panel = (ItemsWrapGrid)gv.ItemsPanelRoot;

            //     panel.Orientation = Orientation.Horizontal;

            double colums = gv.ActualWidth / this.ItemWidth;
            if (this.small)
                colums *= 1.3;
            panel.MaximumRowsOrColumns = (int)colums;

            panel.ItemWidth = gv.ActualWidth / (int)colums;
            double percent = panel.ItemWidth / this.ItemWidth;
            panel.ItemHeight = percent * this.ItemHeight;

            this.ContentItemsPresenter = (ItemsPresenter)this.inside_scrollViewer.Content;
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
            timer.Tick += this.Timer_Tick;
            timer.Start();

            this._loading.TryAgainCmd = this.Reload;
            this._footer.TryAgainCmd = this.Reload;
        }

        private void inside_scrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded2?.Invoke(sender, e);
        }

        void HeaderWithMenu_HeaderHeightChanged(object sender, double e)
        {
            this.offsetForHeader.Height = this.UseHeaderOffset ? e : 0;
        }

        private void ExtendedGridView2_Loaded(object sender, RoutedEventArgs e)
        {
            this.loadingTimer.Tick += LoadingTimer_Tick;
            this.LoadingTimer_Tick(null, null);
            this.loadingTimer.Start();

            if (base.Parent is PivotItem pivotItem)//BugFix: чтобы пивот работал нам не надо менять манипуляцию
            {
                if (pivotItem.Parent is Pivot pivot)
                {
                    pivot.SelectionChanged += Pivot_SelectionChanged;
                }
            }
        }

        private void ExtendedGridView2_Unloaded(object sender, RoutedEventArgs e)
        {
            if (CustomFrame.Instance == null)
                return;

            this.gridView.SelectionChanged -= this.SelectionChanged;
            CustomFrame.Instance.Header.HeaderHeightChanged -= this.HeaderWithMenu_HeaderHeightChanged;

            this.loadingTimer.Stop();
            this.loadingTimer.Tick -= this.LoadingTimer_Tick;

            this.timer.Stop();
            this.timer.Tick -= this.Timer_Tick;

            this._loading.TryAgainCmd = null;
            this._footer.TryAgainCmd = null;

            this.inside_scrollViewer.Loaded -= this.inside_scrollViewer_Loaded;
        }

        /// <summary>
        /// Взывается при срабатывании таймера.
        /// А самый первый раз после Loaded этого ЛистВиев
        /// </summary>
        private void LoadingTimer_Tick(object sender, object e)
        {
            if (this.InLoading /*|| this.PreventScroll*/)
                return;

            if (base.Parent is PivotItem pivotItem)
            {
                if (pivotItem.Parent is Pivot pivot)
                {
                    if (pivot.SelectedItem != pivotItem)
                        return;
                }
            }

            if (/*this.inside_scrollViewer.ScrollableHeight == 0*/this.NeedReload)
                this.Reload();
            else if (this.inside_scrollViewer.ScrollableHeight - this.inside_scrollViewer.VerticalOffset < 700     /*&& this.inside_scrollViewer.ScrollableHeight>0*/)
                this.ProcessDown();
            else if (this.inside_scrollViewer.VerticalOffset < 300                          /*&& this.inside_scrollViewer.ScrollableHeight > 0*/)
                this.ProcessUp();
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot pivot = sender as Pivot;
            //bool result = IsVisibileToUser(this, (FrameworkElement)Window.Current.Content);

            // var temp = pivot.ContainerFromItem(pivot.SelectedItem);

            //bool result2 = IsVisibileToUser((FrameworkElement)temp, (FrameworkElement)Window.Current.Content);

            if (base.Parent is PivotItem pivotItem)
            {
                if (pivot.SelectedItem == pivotItem && this.NeedReload)
                    this.Reload();
            }
        }

        public async void Reload()
        {
            if (this.InLoading || !this.NeedReload)
                return;
            //


            if (base.DataContext is ISupportUpDownIncrementalLoading collection)
            {
                if (collection.HasMoreDownItems)
                {
                    this.InLoading = true;
                    var itemToScroll = await collection.Reload();
                    //if (itemToScroll != null)
                    //    await ScrollToItem(listView, itemToScroll);

                    await Task.Delay(1000);
                    this.NeedReload = false;
                    this.InLoading = false;
                }
            }
            else if (base.DataContext is ISupportDownIncrementalLoading collection2)
            {
                this.NeedReload = false;
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
                    if (!this._de.IsActive)
                        this._de.AddToDelayedExecution(() => { collection2.LoadDownAsync(); });
                    //collection2.LoadDownAsync();
                }
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
            VisualStateManager.GoToState(this._loading, str, false);
        }

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
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(ExtendedGridView2), new PropertyMetadata(null));
#endregion

#region UseHeaderOffset
        public static readonly DependencyProperty UseHeaderOffsetProperty = DependencyProperty.Register(nameof(UseHeaderOffset), typeof(bool), typeof(ExtendedGridView2), new PropertyMetadata(true));

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

        public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register(nameof(SelectionMode), typeof(ListViewSelectionMode), typeof(ExtendedGridView2), new PropertyMetadata(ListViewSelectionMode.None));
        #endregion

        #region Header
        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(object), typeof(ExtendedGridView2), new PropertyMetadata(null));
        #endregion

        #region Footer
        public object Footer
        {
            get { return GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        public static readonly DependencyProperty FooterProperty = DependencyProperty.Register(nameof(Footer), typeof(object), typeof(ExtendedGridView2), new PropertyMetadata(null));
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
        public static readonly DependencyProperty NoContentProperty = DependencyProperty.Register(nameof(NoContent), typeof(object), typeof(ExtendedGridView2), new PropertyMetadata(null));
#endregion

#region ItemWidth
        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(ExtendedGridView2), new PropertyMetadata(128.0));

        public double ItemWidth
        {
            get { return (double)base.GetValue(ItemWidthProperty); }
            set { base.SetValue(ItemWidthProperty, value); }
        }
#endregion

#region ItemHeight
        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(ExtendedGridView2), new PropertyMetadata(128.0));

        public double ItemHeight
        {
            get { return (double)base.GetValue(ItemHeightProperty); }
            set { base.SetValue(ItemHeightProperty, value); }
        }
#endregion



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
                this.IsLocked = true;//Bug: если DataContext не ISupportUpDownIncrementalLoading, то блочится навсегда
                //CustomFrame.Instance.Header.ShowProgress(true);
                this.PercentAction(0);
                this.InLoading = true;

                if (base.DataContext is ISupportUpDownIncrementalLoading collection)
                {
                    await collection.Reload();
                }
                else if(base.DataContext is ISupportReload reload)
                {
                    reload.Reload();
                }

                //CustomFrame.Instance.Header.ShowProgress(false);
                this.InLoading = false;
                return;
            }
            this.PercentAction(percent);
        }

        private void PercentAction(double percent)
        {
            this.OnPullPercentageChanged?.Invoke(percent);
        }

        public GridView GetGridView
        {
            get { return this.gridView; }
        }

        /// <summary>
        /// Это область прокрутки внутри ListView
        /// </summary>
        public ScrollViewer GetInsideScrollViewer
        {
            get { return this.inside_scrollViewer; }
        }
    }
}
