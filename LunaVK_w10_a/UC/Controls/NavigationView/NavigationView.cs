using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace LunaVK.UC.Controls
{
    public partial class NavigationView : ItemsControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        Storyboard Storyboard1;

        RectangleGeometry _rectGeometry;
        Rectangle _rect;
        ScrollViewer ScrollViewer;
        DoubleAnimationUsingKeyFrames _bottomFrames;
        DoubleAnimationUsingKeyFrames _topFrames;
        Border _brd;
        bool _initialized;
        private DispatcherTimer _localTimer;
        //public event SelectionChangedEventHandler SelectionChanged;


#region Orientation
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(NavigationView), new PropertyMetadata(Orientation.Vertical));

        public Orientation Orientation
        {
            get { return (Orientation)base.GetValue(OrientationProperty); }
            set { base.SetValue(OrientationProperty, value); }
        }
#endregion

        private void ScrollToSelected()
        {
            if (this.Orientation == Orientation.Horizontal)
            {
                var listViewItem = base.ContainerFromIndex(this._selectedIndex) as FrameworkElement;
                var topLeft = listViewItem.TransformToVisual(this.ScrollViewer.Content as FrameworkElement).TransformPoint(new Point(0, 0)).X;
                var lvih = listViewItem.ActualWidth;
                var lvh = this.ScrollViewer.ActualWidth;
                var desiredTopLeft = (lvh - lvih) / 2.0;
                var desiredDelta = topLeft - desiredTopLeft;

                var currentOffset = this.ScrollViewer.HorizontalOffset;
                var desiredOffset = currentOffset + desiredDelta;
                this.ScrollViewer.ChangeView(desiredOffset, 0, 1.0f, false);
            }
            else
            {

            }
        }

#region SelectedIndex
        private int _prevIndex = -1;
        private int _selectedIndex = -1;

        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(NavigationView), new PropertyMetadata(-1, SelectedIndexChanged));
        public int SelectedIndex
        {
            //get { return (int)base.GetValue(SelectedIndexProperty); }
            get { return this._selectedIndex; }
            set { base.SetValue(SelectedIndexProperty, value); }
        }

        private static void SelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NavigationView lv = (NavigationView)d;
            lv.SelectedIndexPreApply((int)e.NewValue);
        }
#endregion

        private void SelectedIndexPreApply(int value)
        {
            if (this._selectedIndex == value)
                return;

            this._selectedIndex = value;
            this.RaisePropertyChanged(nameof(this.SelectedIndex));

            this.SelectedIndexApply(value, true);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object datacontext)
        {
            base.PrepareContainerForItemOverride(element, datacontext);

            var temp = element as FrameworkElement;
            this.HookUpElement(temp);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object datacontext)
        {
            base.ClearContainerForItemOverride(element, datacontext);

            var temp = element as FrameworkElement;
            this.UnHookElement(temp);
        }
        /*
        protected override void OnItemsChanged(object e)
        {
            base.OnItemsChanged(e);
            if (!this._initialized)
                return;

            IVectorChangedEventArgs args = (IVectorChangedEventArgs)e;
            int index = (int)args.Index;
            if(args.CollectionChange == CollectionChange.ItemInserted)
            {
                //this.HookUpElement(index);
            }
            else if(args.CollectionChange == CollectionChange.ItemRemoved)
            {
                this.UnHookElement(index);
                int i = 0;
            }
            
        }
        */
        private void HookUpElement(FrameworkElement element)
        {
            if (element == null)
                return;
            
            element.PointerEntered += Element_PointerEntered;
            element.PointerExited += Element_PointerExited;
            element.PointerCaptureLost += Element_PointerCaptureLost;
            element.PointerPressed += Element_PointerPressed;
            element.PointerReleased += Element_PointerReleased;

            element.Tapped += Element_Tapped;

            //if (this.Orientation == Orientation.Horizontal)
            //    element.Style = (Style)Application.Current.Resources["NavigationViewItemPresenterStyleWhenOnTopPane"];
        }

        private void Element_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //
            this.SelectedIndex = base.Items.IndexOf(sender);
        }

        private void UnHookElement(FrameworkElement element)
        {
            if (element == null)
                return;
            
            element.PointerEntered -= Element_PointerEntered;
            element.PointerExited -= Element_PointerExited;
            element.PointerCaptureLost -= Element_PointerCaptureLost;
            element.PointerPressed -= Element_PointerPressed;
            element.PointerReleased -= Element_PointerReleased;


            element.Tapped -= Element_Tapped;
        }
        /*
        private void HookUpEvents()
        {
            this.UnHookEvents();

            for (int i = 0; i < base.Items.Count; i++)
            {
            //    this.HookUpElement(i);

                //if (this.Orientation == Orientation.Horizontal)
                //    element.Style = (Style)Application.Current.Resources["NavigationViewItemPresenterStyleWhenOnTopPane"];

            //    if (this._selectedIndex == i)
            //    {
            //        element.Loaded += Element_Loaded;
            //    }
            }
        }
        */
        private void UnHookEvents()
        {
            for (int i = 0; i < base.Items.Count; i++)
            {
                var element = base.ContainerFromIndex(i) as Control;
                if (element == null)
                    continue;
                
                element.PointerEntered -= Element_PointerEntered;
                element.PointerExited -= Element_PointerExited;
                element.PointerCaptureLost -= Element_PointerCaptureLost;
                element.PointerPressed -= Element_PointerPressed;
                element.PointerReleased -= Element_PointerReleased;

                element.Tapped -= Element_Tapped;
            }
        }
        

        public void SelectedIndexApply(int value, bool fromRealClick)
        {
            if (!this._initialized)
                return;

            if (value == -1)
            {
                this._brd.Opacity = 0;
                this._prevIndex = -1;

                for (int i = 0; i < base.Items.Count; i++)
                {
                    var item = base.ContainerFromIndex(i) as Control;
                    if (item == null)
                        continue;
                    VisualStateManager.GoToState(item, "Normal", true);
                }

                return;
            }
            
            this._brd.Opacity = 1;

            var element = base.ContainerFromIndex(value) as Control;//FrameworkElement element = sender as FrameworkElement;

            if (_prevIndex == -1)
            {
                _topFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(0);
                _topFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(600);
                _bottomFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(0);
                _bottomFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(600);


                if (this.Orientation == Orientation.Vertical)
                {
                    double newY = element.TransformToVisual(this.ScrollViewer.Content as FrameworkElement).TransformPoint(new Point(0, 0)).Y;
                    double h = this._rectGeometry.Rect.Height;

                    _bottomFrames.KeyFrames[0].Value = -(h - newY - (element.ActualHeight / 2));
                    _bottomFrames.KeyFrames[1].Value = -(h - newY - element.ActualHeight);
                    _topFrames.KeyFrames[0].Value = newY + (element.ActualHeight / 2);
                    _topFrames.KeyFrames[1].Value = newY;

                }
                else
                {
                    double newX = element.TransformToVisual(this.ScrollViewer.Content as FrameworkElement).TransformPoint(new Point(0, 0)).X;

                    double w = this._rectGeometry.Rect.Width;

                    _bottomFrames.KeyFrames[0].Value = -(w - newX - (element.ActualWidth / 2));
                    _bottomFrames.KeyFrames[1].Value = -(w - newX - element.ActualWidth);
                    _topFrames.KeyFrames[0].Value = newX + (element.ActualWidth / 2);
                    _topFrames.KeyFrames[1].Value = newX;

                }
                this.Storyboard1.Begin();

                this._prevIndex = value;//last = element as Control;

                VisualStateManager.GoToState(element, fromRealClick ? "PointerOverSelected" : "Selected", true);

                this.ScrollToSelected();

                return;
            }


            int d800 = 150;// int.Parse(this._number1.Text);//150
            int d1000 = 700;// int.Parse(this._number2.Text);//700
            int d600 = 150;// int.Parse(this._number3.Text);//150

            var last = base.ContainerFromIndex(this._prevIndex) as FrameworkElement;

            if (this.Orientation == Orientation.Vertical)
            {
                double newY = element.TransformToVisual(this.ScrollViewer.Content as FrameworkElement).TransformPoint(new Point(0, 0)).Y;
                double prevY = last.TransformToVisual(this.ScrollViewer.Content as FrameworkElement).TransformPoint(new Point(0, 0)).Y;

                double h = this._rectGeometry.Rect.Height;

                _bottomFrames.KeyFrames[0].Value = -(h - prevY - element.ActualHeight);
                _bottomFrames.KeyFrames[1].Value = -(h - newY - element.ActualHeight);
                _topFrames.KeyFrames[0].Value = prevY;
                _topFrames.KeyFrames[1].Value = newY;

                if (newY > prevY)//сверху вниз
                {
                    _topFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(d800);
                    _topFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(d1000);
                    _bottomFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(0);
                    _bottomFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(d600);
                }
                else
                {
                    _topFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(0);
                    _topFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(d600);
                    _bottomFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(d800);
                    _bottomFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(d1000);
                }
            }
            else
            {
                double newX = element.TransformToVisual(this.ScrollViewer.Content as FrameworkElement).TransformPoint(new Point(0, 0)).X;
                double prevX = last.TransformToVisual(this.ScrollViewer.Content as FrameworkElement).TransformPoint(new Point(0, 0)).X;

                double w = this._rectGeometry.Rect.Width;

                _bottomFrames.KeyFrames[0].Value = -(w - prevX - element.ActualWidth);
                _bottomFrames.KeyFrames[1].Value = -(w - newX - element.ActualWidth);
                _topFrames.KeyFrames[0].Value = prevX;
                _topFrames.KeyFrames[1].Value = newX;

                if (newX > prevX)//слева направо
                {
                    _topFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(d800);
                    _topFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(d1000);
                    _bottomFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(0);
                    _bottomFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(d600);
                }
                else
                {
                    _topFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(0);
                    _topFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(d600);
                    _bottomFrames.KeyFrames[0].KeyTime = TimeSpan.FromMilliseconds(d800);
                    _bottomFrames.KeyFrames[1].KeyTime = TimeSpan.FromMilliseconds(d1000);
                }
            }

            this.Storyboard1.Begin();


            this._prevIndex = value;//last = element as Control;

            for (int i = 0; i < base.Items.Count; i++)
            {
                var item = base.ContainerFromIndex(i) as Control;
                if (item == null)
                    continue;
                VisualStateManager.GoToState(item, this._selectedIndex == i ? "PointerOverSelected" : "Normal", true);
            }

            this.ScrollToSelected();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.Storyboard1 = GetTemplateChild("Storyboard1") as Storyboard;

            this._rectGeometry = GetTemplateChild("_rectGeometry") as RectangleGeometry;
            this._rect = GetTemplateChild("_rect") as Rectangle;

            this._bottomFrames = GetTemplateChild("_bottomFrames") as DoubleAnimationUsingKeyFrames;
            this._topFrames = GetTemplateChild("_topFrames") as DoubleAnimationUsingKeyFrames;
            this._brd = GetTemplateChild("_brd") as Border;

            this.ScrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;

            base.Loaded += NavigationView_Loaded;
            base.Unloaded += NavigationView_Unloaded;
        }
        
        private void NavigationView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.UnHookEvents();
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            if (base.Parent is Grid grid)
            {
                if(grid.Children.Count>1)
                {
                    if( grid.Children[0] is PivotHeaderPanel headerPanel)
                    {
                        List<PivotHeaderItem> list = new List<PivotHeaderItem>();

                        for (int i = 0; i < headerPanel.Children.Count; i++)
                        {
                            var child = headerPanel.Children[i] as PivotHeaderItem;
                            list.Add(child);
                        }

                        foreach (var child in list)
                        {
                            //headerPanel.Children.Remove(child);
                            //base.Items.Add(child);
                            base.Items.Add(new NavigationViewItem() { Content = child.Content, Style = (Style)Application.Current.Resources["NavigationViewItemPresenterStyleWhenOnTopPane"] });
                        }
                    }
                   
                }
            }
            */
            if(this.Storyboard1!=null)
                this.Storyboard1.Stop();

            if (this.Orientation == Orientation.Vertical)
            {
                Storyboard.SetTargetProperty(_bottomFrames, "Y");
                Storyboard.SetTargetProperty(_topFrames, "Y");
                _brd.HorizontalAlignment = HorizontalAlignment.Left;
                _brd.VerticalAlignment = VerticalAlignment.Stretch;
                _rect.Width = 3;
                _rect.Height = double.NaN;

                base.ItemsPanel = null;
                base.VerticalAlignment = VerticalAlignment.Stretch;

                this.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                this.ScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
                this.ScrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
                this.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

                this.ScrollViewer.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
            else
            {
                Storyboard.SetTargetProperty(_bottomFrames, "X");
                Storyboard.SetTargetProperty(_topFrames, "X");

                _brd.HorizontalAlignment = HorizontalAlignment.Stretch;
                _brd.VerticalAlignment = VerticalAlignment.Top;
                _brd.Margin = new Thickness(0, 37, 0, 0);

                _rect.Height = 3;
                _rect.Width = double.NaN;

                ItemsPanelTemplate itemsPanelTemplate = XamlReader.Load(@"<ItemsPanelTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'> <ItemsStackPanel Orientation='Horizontal'/> </ItemsPanelTemplate>") as ItemsPanelTemplate;
                base.ItemsPanel = itemsPanelTemplate;
                base.VerticalAlignment = VerticalAlignment.Top;

                this.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                this.ScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                this.ScrollViewer.HorizontalScrollMode = ScrollMode.Enabled;
                this.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

                this.ScrollViewer.HorizontalAlignment = HorizontalAlignment.Left;
            }
            this._initialized = true;

            var item = base.ContainerFromIndex(base.Items.Count-1) as Control;
            if(item!=null)
                item.Loaded+=this.Element_Loaded;
            else
            {
                this._localTimer = new DispatcherTimer();
                this._localTimer.Interval = TimeSpan.FromSeconds(0.6);
                this._localTimer.Tick += this._localTimer_Tick;
                this._localTimer.Start();
            }
        }

        private void _localTimer_Tick(object sender, object e)
        {
            DispatcherTimer timer = sender as DispatcherTimer;
            timer.Stop();
            this._localTimer = null;

            this.SelectedIndexApply(this._selectedIndex, false);
        }
        /*
        private void Items_VectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs args)
        {
            int index = (int)args.Index;
            if (args.CollectionChange == CollectionChange.ItemInserted)
            {
                //this.HookUpElement(index);
            }
            else if (args.CollectionChange == CollectionChange.ItemRemoved)
            {
                
            }
        }
        */
        private void Element_Loaded(object sender, RoutedEventArgs e)
        {
            //Приходится так извращаться, т.к. надо получить ширину элемента
            (sender as FrameworkElement).Loaded -= Element_Loaded;
            this.SelectedIndexApply(this._selectedIndex,false);
        }
        
        private void Element_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            bool flag = base.IndexFromContainer(sender as FrameworkElement) == this._selectedIndex;
            VisualStateManager.GoToState(sender as Control, flag ? "PointerOverSelected" : "PointerOver", true);
        }

        private void Element_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            bool flag = base.IndexFromContainer(sender as FrameworkElement) == this._selectedIndex;
            VisualStateManager.GoToState(sender as Control, flag ? "PressedSelected" : "Pressed", true);
        }

        private void Element_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            Control ctrl = sender as Control;
            if (ctrl == null)
                return;

            bool flag = base.IndexFromContainer(ctrl) == this._selectedIndex;
            VisualStateManager.GoToState(ctrl, flag ? "Selected" : "Normal", true);
        }

        private void Element_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Control ctrl = sender as Control;
            if (ctrl == null)
                return;

            bool flag = base.IndexFromContainer(ctrl) == this._selectedIndex;
            VisualStateManager.GoToState(ctrl, flag ? "Selected" : "Normal", true);
        }

        private void Element_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Control ctrl = sender as Control;
            if (ctrl == null)
                return;

            bool flag = base.IndexFromContainer(ctrl) == this._selectedIndex;
            VisualStateManager.GoToState(ctrl, flag ? "PointerOverSelected" : "PointerOver", true);
        }

        private void RaisePropertyChanged(string property)
        {
            if (this.PropertyChanged == null)
                return;
            PropertyChangedEventArgs e = new PropertyChangedEventArgs(property);
            this.PropertyChanged(this, e);
        }
    }
}


/*
AlwaysShowHeader
AlwaysShowHeaderProperty
AutoSuggestBox
AutoSuggestBoxProperty
CompactModeThresholdWidth
CompactModeThresholdWidthProperty
CompactPaneLength
CompactPaneLengthProperty
ContentOverlay
ContentOverlayProperty
DisplayMode
DisplayModeProperty
ExpandedModeThresholdWidth
ExpandedModeThresholdWidthProperty
Header
HeaderProperty
HeaderTemplate
HeaderTemplateProperty
IsBackButtonVisible
IsBackButtonVisibleProperty
IsBackEnabled
IsBackEnabledProperty
IsPaneOpen
IsPaneOpenProperty
IsPaneToggleButtonVisible
IsPaneToggleButtonVisibleProperty
IsPaneVisible
IsPaneVisibleProperty
IsSettingsVisible
IsSettingsVisibleProperty
MenuItemContainerStyle
MenuItemContainerStyleProperty
MenuItemContainerStyleSelector
MenuItemContainerStyleSelectorProperty
MenuItems
MenuItemsProperty
MenuItemsSource
MenuItemsSourceProperty
MenuItemTemplate
MenuItemTemplateProperty
MenuItemTemplateSelector
MenuItemTemplateSelectorProperty
OpenPaneLength
OpenPaneLengthProperty
OverflowLabelMode
OverflowLabelModeProperty
PaneCustomContent
PaneCustomContentProperty
PaneDisplayMode
PaneDisplayModeProperty
PaneFooter
PaneFooterProperty
PaneHeader
PaneHeaderProperty
PaneTitle
PaneTitleProperty
PaneToggleButtonStyle
PaneToggleButtonStyleProperty
SelectedItem
SelectedItemProperty
SelectionFollowsFocus
SelectionFollowsFocusProperty
SettingsItem
SettingsItemProperty
ShoulderNavigationEnabled
ShoulderNavigationEnabledProperty
TemplateSettings
TemplateSettingsProperty
*/
