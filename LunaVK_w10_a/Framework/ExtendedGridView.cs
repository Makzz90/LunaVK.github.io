using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using LunaVK.Core.Library;
using System.Threading.Tasks;
using System;
using LunaVK.Core.Enums;

namespace LunaVK.Framework
{
    public class ExtendedGridView : GridView 
    {
        public static readonly DependencyProperty UseHeaderOffsetProperty = DependencyProperty.Register("UseHeaderOffset", typeof(bool), typeof(ExtendedGridView), new PropertyMetadata(true));
        public static readonly DependencyProperty HeaderProperty2 = DependencyProperty.Register("Header2", typeof(object), typeof(ExtendedGridView), new PropertyMetadata(null));
        public object Header2
        {
            get { return GetValue(HeaderProperty2); }
            set { SetValue(HeaderProperty2, value); }
        }

        //Контейнер очень крупный и лучше через таймер обновлять
        private DispatcherTimer loadingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };

        public bool UseHeaderOffset
        {
            get { return (bool)GetValue(UseHeaderOffsetProperty); }
            set
            {
                //
                //if (this.offsetForHeader != null)
                //    this.offsetForHeader.Height = value ? (Window.Current.Content as Framework.CustomFrame).HeaderWithMenu.HeaderHeight : 0;
                //
                SetValue(UseHeaderOffsetProperty, value);
            }
        }

        private ScrollViewer scrollViewer;
        private bool InLoading;
        private Rectangle Offset = new Rectangle();

        public ExtendedGridView()
        {
            base.SelectionMode = ListViewSelectionMode.None;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (CustomFrame.Instance == null)
                return;
            
            this.scrollViewer = this.GetTemplateChild("ScrollViewer") as ScrollViewer;
            //this.scrollViewer.ViewChanged += scrollViewer_ViewChanged;
            base.Unloaded += ExtendedGridView_Unloaded;
            this.Loaded += ExtendedGridView_Loaded;

            CustomFrame.Instance.HeaderWithMenu.HeaderHeightChanged += HeaderWithMenu_HeaderHeightChanged;
            //
            base.ItemContainerTransitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
            base.ItemContainerTransitions.Add(new Windows.UI.Xaml.Media.Animation.EntranceThemeTransition() {FromHorizontalOffset=0, FromVerticalOffset=50, IsStaggeringEnabled=true });
            base.ItemContainerTransitions.Add(new Windows.UI.Xaml.Media.Animation.AddDeleteThemeTransition());
            base.ItemContainerTransitions.Add(new Windows.UI.Xaml.Media.Animation.RepositionThemeTransition());
            //
            
        }

        void ExtendedGridView_Loaded(object sender, RoutedEventArgs e)
        {
            StackPanel sp = new StackPanel();
            base.Header = sp;

            if (this.UseHeaderOffset)
                sp.Children.Add(this.Offset);

            if (this.Header2 != null)
                sp.Children.Add(this.Header2 as UIElement);

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

        public bool NeedReload = true;

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
            else if (this.scrollViewer.ScrollableHeight - this.scrollViewer.VerticalOffset < 700)
                this.ProcessDown();
            else if (this.scrollViewer.VerticalOffset < 300)
                this.ProcessUp();
        }

        private async void Reload()
        {
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
                collection2.LoadDownAsync(true);
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
                    collection2.LoadDownAsync();
            }
        }

        void ExtendedGridView_Unloaded(object sender, RoutedEventArgs e)
        {
            //this.scrollViewer.ViewChanged -= scrollViewer_ViewChanged;
            CustomFrame.Instance.HeaderWithMenu.HeaderHeightChanged -= HeaderWithMenu_HeaderHeightChanged;

            this.loadingTimer.Stop();
            this.loadingTimer.Tick -= this.LoadingTimer_Tick;
        }

        private void HeaderWithMenu_HeaderHeightChanged(object sender, double e)
        {
            this.Offset.Height = e;
        }

        async void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate || this.InLoading)
                return;

            ScrollViewer sv = sender as ScrollViewer;

            bool res = false;

            if (sv.VerticalScrollMode == ScrollMode.Disabled)
                res = sv.ScrollableWidth - sv.HorizontalOffset < 700;
            else
                res = sv.ScrollableHeight - sv.VerticalOffset < 700;

            if (res)
            {
                if (base.DataContext is ISupportUpDownIncrementalLoading collection)
                {
                    if (collection.HasMoreDownItems == false)
                        return;

                    this.InLoading = true;
                    await collection.LoadDownAsync();
                    await Task.Delay(1000);
                    this.InLoading = false;
                }
            }
        }
    }
}
