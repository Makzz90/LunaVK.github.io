using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

using static Windows.ApplicationModel.Resources.Core.ResourceContext;
using static Windows.ApplicationModel.DesignMode;
using Windows.UI.ViewManagement;
using Windows.Graphics.Display;
using LunaVK.Core.Utils;

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    [TemplateVisualState(Name = ClosedState, GroupName = DisplayModeStates)]
    [TemplateVisualState(Name = ClosedCompactLeftState, GroupName = DisplayModeStates)]
    [TemplateVisualState(Name = ClosedCompactRightState, GroupName = DisplayModeStates)]
    [TemplateVisualState(Name = OpenOverlayLeftState, GroupName = DisplayModeStates)]
    [TemplateVisualState(Name = OpenOverlayRightState, GroupName = DisplayModeStates)]
    [TemplateVisualState(Name = OpenInlineLeftState, GroupName = DisplayModeStates)]
    [TemplateVisualState(Name = OpenInlineRightState, GroupName = DisplayModeStates)]
    [TemplateVisualState(Name = OpenCompactOverlayLeftState, GroupName = DisplayModeStates)]
    [TemplateVisualState(Name = OpenCompactOverlayRightState, GroupName = DisplayModeStates)]
    [TemplateVisualState(Name = OverlayNotVisibleState, GroupName = OverlayVisibilityStates)]
    [TemplateVisualState(Name = OverlayVisibleState, GroupName = OverlayVisibilityStates)]
    public partial class SplitView2 : ContentControl
    {
        private const string DisplayModeStates = "DisplayModeStates";
        private const string OverlayVisibilityStates = "OverlayVisibilityStates";

        private const string ClosedState = "Closed";
        private const string ClosedCompactLeftState = "ClosedCompactLeft";
        private const string ClosedCompactRightState = "ClosedCompactRight";
        private const string OpenOverlayLeftState = "OpenOverlayLeft";
        private const string OpenOverlayRightState = "OpenOverlayRight";
        private const string OpenInlineLeftState = "OpenInlineLeft";
        private const string OpenInlineRightState = "OpenInlineRight";
        private const string OpenCompactOverlayLeftState = "OpenCompactOverlayLeft";
        private const string OpenCompactOverlayRightState = "OpenCompactOverlayRight";

        private const string OverlayNotVisibleState = "OverlayNotVisible";
        private const string OverlayVisibleState = "OverlayVisible";

        //private VisualStateGroup _selectionStateGroup;
        private RectangleGeometry PaneClipRectangle;
        private Grid PaneRoot;
        private VisualStateGroup _selectionStateGroup;
        private Grid ContentRoot;
        private CompositeTransform PaneTransform;
        internal Windows.UI.Xaml.Shapes.Rectangle LightDismissLayer;

        public SplitView2()
        {
            base.DefaultStyleKey = typeof(SplitView2);

            base.Loaded += OnLoaded;
            base.Unloaded += OnUnloaded;
        }

        
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.PaneClipRectangle = GetTemplateChild("PaneClipRectangle") as RectangleGeometry;
            this.PaneRoot = GetTemplateChild("PaneRoot") as Grid;
            this.ContentRoot = GetTemplateChild("ContentRoot") as Grid;
            this.ContentRoot.ManipulationMode = /*ManipulationModes.System |*/ ManipulationModes.TranslateX;
            this.ContentRoot.ManipulationDelta += this.ContentRoot_ManipulationDelta;
            this.ContentRoot.ManipulationCompleted += this.ContentRoot_ManipulationCompleted;
            

            this.PaneTransform = this.PaneRoot.RenderTransform as CompositeTransform;

            this.LightDismissLayer = GetTemplateChild("LightDismissLayer") as Windows.UI.Xaml.Shapes.Rectangle;
            this.LightDismissLayer.Tapped += this.OnDismissLayerTapped;

            this.PaneRoot.SizeChanged += this.PaneRoot_SizeChanged;

            //VisualStateManager.GoToState(this, ClosedState, false);
            this.SetVisualState(false);
        }
        
        public class DeviceInformation
        {
            public static ApplicationViewOrientation Orientation => DisplayInformation.GetForCurrentView().CurrentOrientation.ToString().Contains("Landscape") ? ApplicationViewOrientation.Landscape : ApplicationViewOrientation.Portrait;


            public static bool IsMobile => GetForCurrentView().QualifierValues["DeviceFamily"] == "Mobile" ? true : false;


            public static DisplayInformation DisplayInformation => DisplayInformation.GetForCurrentView();


            //public static Frame DisplayFrame => Window.Current.Content == null ? null : Window.Current.Content as Frame;
        }

        

        private void PaneRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.PaneClipRectangle.Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled == false)
            {
                _selectionStateGroup = (VisualStateGroup)GetTemplateChild(DisplayModeStates);
                
                if (_selectionStateGroup != null)
                {
                    _selectionStateGroup.CurrentStateChanged += this.OnSelectionStateChanged;
                }
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled == false)
            {
                _selectionStateGroup = (VisualStateGroup)GetTemplateChild(DisplayModeStates);
                
                if (_selectionStateGroup != null)
                {
                    _selectionStateGroup.CurrentStateChanged -= this.OnSelectionStateChanged;
                    _selectionStateGroup = null;
                }
            }

            if(this.PaneRoot!=null)
                this.PaneRoot.SizeChanged -= this.PaneRoot_SizeChanged;
        }

        private void OnSelectionStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (this.IsPaneOpen)
                this.PaneOpened?.Invoke(this, e);
            else
                this.PaneClosed?.Invoke(this, e);
        }

        private void SetVisualState(bool animate)
        {
            if (this.IsPaneLocked == true)
                return;

            if (this.DisplayMode == SplitViewDisplayMode.CompactOverlay && DeviceInformation.IsMobile)
            {
                this.DisplayMode = SplitViewDisplayMode.Overlay;
                return;
            }

            if (this.IsPaneOpen)
                this.PaneOpening?.Invoke(this, null);
            else
                this.PaneClosing?.Invoke(this, null);
            /*
            if(this._selectionStateGroup == null)//BugFix: у телефона не получить VisualStateGroup - выкручиваемся так
            {
                if (this.IsPaneOpen)
                    this.PaneOpened?.Invoke(this, null);
                else
                    this.PaneClosed?.Invoke(this, null);
            }
            */
            


            string state;
            if (this.DisplayMode == SplitViewDisplayMode.CompactInline)
            {
                state = this.IsPaneOpen ? OpenInlineLeftState : ClosedCompactLeftState;
            }
            else if (this.DisplayMode == SplitViewDisplayMode.CompactOverlay)
            {
                state = this.IsPaneOpen ? OpenCompactOverlayLeftState : ClosedCompactLeftState;
            }
            else if (this.DisplayMode == SplitViewDisplayMode.Inline)
            {
                state = this.IsPaneOpen ? OpenInlineLeftState : ClosedState;
            }
            else//SplitViewDisplayMode.Overlay
            {
                state = this.IsPaneOpen ? OpenOverlayLeftState : ClosedState;

                if (this.PaneRoot != null && this.IsPaneOpen)
                {
                    //if (this.PaneTransform.TranslateX == 0)
                    //    this.PaneTransform.TranslateX = this.NegativeOpenPaneLength;
                }
            }

            //System.Diagnostics.Debug.WriteLine(state);


            VisualStateManager.GoToState(this, state, animate);
        }

        //public SplitViewPanePlacement PanePlacement { get; set; }

#region PaneBackground
        /// <summary>
        /// Возвращает или задает объект Brush, применяемую к фону области Pane элемента управления.
        /// </summary>
        public Brush PaneBackground
        {
            get { return (Brush)GetValue(PaneBackgroundProperty); }
            set { SetValue(PaneBackgroundProperty, value); }
        }

        public static readonly DependencyProperty PaneBackgroundProperty = DependencyProperty.Register( nameof(PaneBackground), typeof(Brush), typeof(SplitView2), new PropertyMetadata(null));
#endregion

#region Pane
        /// <summary>
        /// Возвращает или задает содержимое панели SplitView.
        /// </summary>
        public UIElement Pane
        {
            get { return (UIElement)GetValue(PaneProperty); }
            set { SetValue(PaneProperty, value); }
        }

        public static readonly DependencyProperty PaneProperty = DependencyProperty.Register(nameof(Pane), typeof(UIElement), typeof(SplitView2), new PropertyMetadata(null));
#endregion

#region OpenPaneLength
        /// <summary>
        /// Возвращает или задает ширину панели SplitView, если она полностью развернута.
        /// </summary>
        public double OpenPaneLength
        {
            get { return (double)GetValue(OpenPaneLengthProperty); }
            set { SetValue(OpenPaneLengthProperty, value); }
        }

        public static readonly DependencyProperty OpenPaneLengthProperty = DependencyProperty.Register( nameof(OpenPaneLength), typeof(double), typeof(SplitView2), new PropertyMetadata(320d));
#endregion

#region IsPaneOpen
        public bool IsPaneOpen
        {
            get { return (bool)GetValue(IsPaneOpenProperty); }
            set { SetValue(IsPaneOpenProperty, value); }
        }

        public static readonly DependencyProperty IsPaneOpenProperty = DependencyProperty.Register(nameof(IsPaneOpen), typeof(bool), typeof(SplitView2), new PropertyMetadata(false, OnIsPaneOpenChanged));

        private static void OnIsPaneOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (SplitView2)d;
            view.SetVisualState(true);
        }
#endregion

        

#region DisplayMode
        public SplitViewDisplayMode DisplayMode
        {
            get { return (SplitViewDisplayMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }

        public static readonly DependencyProperty DisplayModeProperty = DependencyProperty.Register(nameof(DisplayMode), typeof(SplitViewDisplayMode), typeof(SplitView2), new PropertyMetadata(SplitViewDisplayMode.Overlay, OnDisplayModeChanged));

        private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (SplitView2)d;
            view.SetVisualState(true);
            //view.OnDetailsCommandBarChanged();
        }
#endregion

#region DisplayMode
        public bool IsPaneLocked
        {
            get { return (bool)GetValue(IsPaneLockedProperty); }
            set { SetValue(IsPaneLockedProperty, value); }
        }

        public static readonly DependencyProperty IsPaneLockedProperty = DependencyProperty.Register(nameof(IsPaneLocked), typeof(bool), typeof(SplitView2), new PropertyMetadata(false, OnIsPaneLockedChanged));

        private static void OnIsPaneLockedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (SplitView2)d;
            bool locked = (bool)e.NewValue;
            if (locked == false)
                view.SetVisualState(false);
            else
            {
                VisualStateManager.GoToState(view, ClosedState, false);
            }
        }
#endregion
        /*
#region Content
        public UIElement Content
        {
            get { return (UIElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(UIElement), typeof(SplitView2), new PropertyMetadata(null));
#endregion
    */
        #region CompactPaneLength
        /// <summary>
        /// Возвращает или задает ширину области SplitView в компактном режиме отображения.
        /// </summary>
        public double CompactPaneLength
        {
            get { return (double)GetValue(CompactPaneLengthProperty); }
            set { SetValue(CompactPaneLengthProperty, value); }
        }

        public static readonly DependencyProperty CompactPaneLengthProperty = DependencyProperty.Register(nameof(CompactPaneLength), typeof(double), typeof(SplitView2), new PropertyMetadata(48d));
#endregion










#region SplitViewTemplateSettings
        public GridLength CompactPaneGridLength { get { return new GridLength(this.CompactPaneLength); } }

        /// <summary>
        /// Gets the negative of the OpenPaneLength value.
        /// </summary>
        public double NegativeOpenPaneLength { get { return -this.OpenPaneLength; } }

        /// <summary>
        /// Gets the negative of the value calculated by subtracting the CompactPaneLength value from the OpenPaneLength value.
        /// </summary>
        public double NegativeOpenPaneLengthMinusCompactLength
        {
            get { return -(this.OpenPaneLength - this.CompactPaneLength); }
        }

        public GridLength OpenPaneGridLength { get { return new GridLength(this.OpenPaneLength); } }

        /// <summary>
        /// Gets a value calculated by subtracting the CompactPaneLength value from the OpenPaneLength value.
        /// </summary>
        public double OpenPaneLengthMinusCompactLength
        {
            get { return this.OpenPaneLength - this.CompactPaneLength; }
        }
#endregion




        //
        // Сводка:
        //     Происходит при закрытии панели SplitView.
        public event TypedEventHandler<SplitView2, object> PaneClosed;
        //
        // Сводка:
        //     Происходит при закрытии панели SplitView.
        public event TypedEventHandler<SplitView2, SplitViewPaneClosingEventArgs> PaneClosing;
        //
        // Сводка:
        //     Происходит, когда панель SplitView открыта.
        public event TypedEventHandler<SplitView2, object> PaneOpened;
        //
        // Сводка:
        //     Происходит при открытии панели SplitView.
        public event TypedEventHandler<SplitView2, object> PaneOpening;


        public void ActivateSwipe(bool status)
        {
            this.ContentRoot.Tag = status ? null : "CantTouchThis";
            //this.ContentRoot.ManipulationDelta -= ContentRoot_ManipulationDelta;
            //this.ContentRoot.ManipulationCompleted -= ContentRoot_ManipulationCompleted;
            //this.CloseSwipeablePane();
        }

        private void ContentRoot_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (this.IsPaneOpen)
            {
                e.Handled = true;
                return;
            }

            if (this.PaneTransform.TranslateX == 0)
                this.PaneTransform.TranslateX = this.NegativeOpenPaneLength;
            this.PaneRoot.Visibility = Visibility.Visible;
            this.PaneClipRectangle.Rect = new Rect(0, 0, this.PaneRoot.ActualWidth, this.PaneRoot.ActualHeight);
        }


        private void ContentRoot_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (DisplayMode != SplitViewDisplayMode.Overlay)
                return;

            if (e.Container is FrameworkElement element)
            {
                if (element.Tag is string s)
                {
                    if (s == "CantTouchThis")
                    {
                        element.CancelDirectManipulations();
                        return;
                    }
                }
            }

            var x = PaneTransform.TranslateX + e.Delta.Translation.X;

            // keep the pan within the bountry
            if (x > 0)
            {
                PaneTransform.TranslateX = 0;
                return;
            }
            PaneTransform.TranslateX += e.Delta.Translation.X;
        }

        private void ContentRoot_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            //if (e.Container != ContentRoot)
            //    return;

            if (DisplayMode != SplitViewDisplayMode.Overlay)
                return;

            if (e.Container is FrameworkElement element)
            {
                if (element.Tag is string s)
                {
                    if (s == "CantTouchThis")
                    {
                        element.CancelDirectManipulations();
                        return;
                    }
                }
            }

            double vel = e.Velocities.Linear.X;
            double x = e.Cumulative.Translation.X;
            bool need_change = (this.IsPaneOpen ? (x < -this.OpenPaneLength / 2.0 || vel < -1.5) : (x > this.OpenPaneLength / 2.0 || vel > 1.5));

            if (need_change)
                this.IsPaneOpen = !this.IsPaneOpen;
            else
            {
                this.PaneTransform.Animate(PaneTransform.TranslateX, this.IsPaneOpen ? 0 : this.NegativeOpenPaneLength, "TranslateX", 100);
            }
            /*
            if (e.Cumulative.Translation.X > 100)
            {
                if (this.IsPaneOpen == true)
                {
                    this.PaneTransform.Animate(PaneTransform.TranslateX, 0, "TranslateX", 100);
                    return;
                }
                this.IsPaneOpen = true;
            }
            else
            {
                if (this.IsPaneOpen == false)
                {
                    this.PaneTransform.Animate(PaneTransform.TranslateX, this.NegativeOpenPaneLength, "TranslateX", 100);
                    return;
                }
                this.IsPaneOpen = false;
            }
            */
        }

        void OnDismissLayerTapped(object sender, TappedRoutedEventArgs e)
        {
            this.IsPaneOpen = false;
        }
    }
}
