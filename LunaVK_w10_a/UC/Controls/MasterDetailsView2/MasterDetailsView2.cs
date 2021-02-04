// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LunaVK.Core.Utils;
using Windows.Foundation.Metadata;

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    /// <summary>
    /// Panel that allows for a Master/Details pattern.
    /// </summary>
//    [TemplatePart(Name = PartDetailsPresenter, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = PartDetailsPanel, Type = typeof(FrameworkElement))]
    [TemplateVisualState(Name = NoSelectionNarrowState, GroupName = SelectionStates)]
    [TemplateVisualState(Name = NoSelectionWideState, GroupName = SelectionStates)]
    [TemplateVisualState(Name = HasSelectionWideState, GroupName = SelectionStates)]
    [TemplateVisualState(Name = HasSelectionNarrowState, GroupName = SelectionStates)]
    [TemplateVisualState(Name = NarrowState, GroupName = WidthStates)]
    [TemplateVisualState(Name = WideState, GroupName = WidthStates)]
    public partial class MasterDetailsView2 : ItemsControl
    {
        private const string PartDetailsPresenter = "DetailsPresenter";
        private const string PartDetailsPanel = "DetailsPanel";
        private const string PartMasterPresenter = "MasterPresenter";
        private const string NarrowState = "NarrowState";
        private const string WideState = "WideState";
        private const string WidthStates = "WidthStates";
        private const string SelectionStates = "SelectionStates";
        private const string HasSelectionNarrowState = "HasSelectionNarrow";
        private const string HasSelectionWideState = "HasSelectionWide";
        private const string NoSelectionNarrowState = "NoSelectionNarrow";
        private const string NoSelectionWideState = "NoSelectionWide";

        private AppViewBackButtonVisibility _previousBackButtonVisibility;
        private ContentPresenter _detailsPresenter;
        private ContentPresenter _masterPresenter;
        private VisualStateGroup _selectionStateGroup;
        private TranslateTransform DetailsPresenterTransform;
        private TranslateTransform MasterPanelTransform;
        private FrameworkElement MasterPanel;
        private FrameworkElement DetailsPanel;
        private FrameworkElement ResizeLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="MasterDetailsView2"/> class.
        /// </summary>
        public MasterDetailsView2()
        {
            DefaultStyleKey = typeof(MasterDetailsView2);

            base.Loaded += OnLoaded;
            base.Unloaded += OnUnloaded;
        }

        /// <summary>
        /// Occurs when the view state changes
        /// </summary>
        public event EventHandler<MasterDetailsViewState> ViewStateChanged;

        public event EventHandler<bool> IsSelectedChanged;
        
        /// <summary>
        /// Invoked whenever application code or internal processes (such as a rebuilding layout pass) call
        /// ApplyTemplate. In simplest terms, this means the method is called just before a UI element displays
        /// in your app. Override this method to influence the default post-template logic of a class.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _detailsPresenter = (ContentPresenter)GetTemplateChild(PartDetailsPresenter);
            _masterPresenter = (ContentPresenter)GetTemplateChild(PartMasterPresenter);
            DetailsPresenterTransform = GetTemplateChild("DetailsPresenterTransform") as TranslateTransform;
            MasterPanelTransform = GetTemplateChild("MasterPanelTransform") as TranslateTransform;
            //
            MasterPanel = GetTemplateChild("MasterPanel") as FrameworkElement;
            DetailsPanel = GetTemplateChild(PartDetailsPanel) as FrameworkElement;
            DetailsPanel.ManipulationMode = ManipulationModes.TranslateX;
            DetailsPanel.ManipulationDelta += Element_ManipulationDelta;
            DetailsPanel.ManipulationCompleted += DetailsPanel_ManipulationCompleted;
            DetailsPanel.ManipulationStarted += DetailsPanel_ManipulationStarted;

            ResizeLine = GetTemplateChild("ResizeLine") as FrameworkElement;
            ResizeLine.ManipulationMode = ManipulationModes.TranslateX;
            ResizeLine.ManipulationDelta += ResizeLine_ManipulationDelta;

            OnDetailsCommandBarChanged();
            OnMasterCommandBarChanged();

            SizeChanged -= MasterDetailsView_SizeChanged;
            SizeChanged += MasterDetailsView_SizeChanged;

            UpdateView(false);
        }

        private void ResizeLine_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double x = e.Delta.Translation.X;
            if (this.MasterPaneWidth + x > 550 || this.MasterPaneWidth + x < 320)
                return;
            this.MasterPaneWidth += x;
            this.CompactModeThresholdWidth = this.MasterPaneWidth * 2;
        }

        private void DetailsPanel_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (this.ViewState != MasterDetailsViewState.Details)
                return;

            MasterPanel.Visibility = Visibility.Visible;
        }

        private void DetailsPanel_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("DetailsPanel_ManipulationCompleted");
            if (this.ViewState != MasterDetailsViewState.Details)
                return;

            e.Handled = true;

            double vel = e.Velocities.Linear.X;
            double x = e.Cumulative.Translation.X;
            bool need_change = (this.SelectedItem == false ? (x < -base.ActualWidth / 2.0 || vel < -1.5) : (x > base.ActualWidth / 2.0 || vel > 1.5));
            if (need_change)
            {
                MasterPanelTransform.Animate(MasterPanelTransform.X, 0, "X", 400);
                DetailsPresenterTransform.Animate(DetailsPresenterTransform.X, 300, "X", 200, 0, null, () => {
                    _selectedItem = false;
                    this.IsSelectedChanged?.Invoke(this, false);
                    this.UpdateView(false);
                });
            }
            else
            {
                DetailsPresenterTransform.Animate(DetailsPresenterTransform.X, 0, "X", 200);
                MasterPanelTransform.Animate(MasterPanelTransform.X, -90, "X", 400);

            }
            /*
            if (e.Cumulative.Translation.X > 100)
            {
                MasterPanelTransform.Animate(MasterPanelTransform.X, 0, "X", 500);
                DetailsPresenterTransform.Animate(DetailsPresenterTransform.X, 300, "X", 300,0,null,()=> {
                    _selectedItem = false;
                    this.IsSelectedChanged?.Invoke(this, false);
                    this.UpdateView(false);
                });
            }
            else
            {
                DetailsPresenterTransform.Animate(DetailsPresenterTransform.X, 0, "X", 300);
                MasterPanelTransform.Animate(MasterPanelTransform.X, -90, "X", 500);
            }
            */
        }

        private void Element_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (this.ViewState != MasterDetailsViewState.Details)
            {
                if (e.Container is FrameworkElement element0)
                {
                    element0.CancelDirectManipulations();
                }

                return;
            }

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

            e.Handled = true;

            var x = DetailsPresenterTransform.X + e.Delta.Translation.X;

            // keep the pan within the bountry
            if (x < 0)
            {
                MasterPanelTransform.X = DetailsPresenterTransform.X = 0;
                return;
            }

            DetailsPresenterTransform.X += e.Delta.Translation.X;

            double new_x = -90 + DetailsPresenterTransform.X / 2.0;

            if (new_x > 0)
                new_x = 0;
            MasterPanelTransform.X = new_x;
        }
        
#region DetailsTemplate
        /// <summary>
        /// Gets or sets the DataTemplate used to display the details.
        /// </summary>
        public DataTemplate DetailsTemplate
        {
            get { return (DataTemplate)GetValue(DetailsTemplateProperty); }
            set { SetValue(DetailsTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="DetailsTemplate"/> dependency property.
        /// </summary>
        /// <returns>The identifier for the <see cref="DetailsTemplate"/> dependency property.</returns>
        public static readonly DependencyProperty DetailsTemplateProperty = DependencyProperty.Register(
            nameof(DetailsTemplate),
            typeof(DataTemplate),
            typeof(MasterDetailsView2),
            new PropertyMetadata(null));
#endregion

#region MasterTemplate
        /// <summary>
        /// Gets or sets the DataTemplate used to display the details.
        /// </summary>
        public DataTemplate MasterTemplate
        {
            get { return (DataTemplate)GetValue(MasterTemplateProperty); }
            set { SetValue(MasterTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="MasterTemplate"/> dependency property.
        /// </summary>
        /// <returns>The identifier for the <see cref="MasterTemplate"/> dependency property.</returns>
        public static readonly DependencyProperty MasterTemplateProperty = DependencyProperty.Register(
            nameof(MasterTemplate),
            typeof(DataTemplate),
            typeof(MasterDetailsView2),
            new PropertyMetadata(null));
#endregion

#region MasterPaneBackground
        /// <summary>
        /// Gets or sets the Brush to apply to the background of the list area of the control.
        /// </summary>
        /// <returns>The Brush to apply to the background of the list area of the control.</returns>
        public Brush MasterPaneBackground
        {
            get { return (Brush)GetValue(MasterPaneBackgroundProperty); }
            set { SetValue(MasterPaneBackgroundProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="MasterPaneBackground"/> dependency property.
        /// </summary>
        /// <returns>The identifier for the <see cref="MasterPaneBackground"/> dependency property.</returns>
        public static readonly DependencyProperty MasterPaneBackgroundProperty = DependencyProperty.Register(
            nameof(MasterPaneBackground),
            typeof(Brush),
            typeof(MasterDetailsView2),
            new PropertyMetadata(null));
#endregion

#region Details
        public object Details
        {
            get { return GetValue(DetailsProperty); }
            set { SetValue(DetailsProperty, value); }
        }

        public static readonly DependencyProperty DetailsProperty = DependencyProperty.Register(
            nameof(Details),
            typeof(object),
            typeof(MasterDetailsView2),
            new PropertyMetadata(null));
#endregion

#region Master
        public object Master
        {
            get { return GetValue(MasterProperty); }
            set { SetValue(MasterProperty, value); }
        }

        public static readonly DependencyProperty MasterProperty = DependencyProperty.Register(
            nameof(Master),
            typeof(object),
            typeof(MasterDetailsView2),
            new PropertyMetadata(null));
#endregion
        
#region MasterHeaderTemplate
        /// <summary>
        /// Gets or sets the DataTemplate used to display the content of the master pane's header.
        /// </summary>
        /// <returns>
        /// The template that specifies the visualization of the master pane header object. The default is null.
        /// </returns>
        public DataTemplate MasterHeaderTemplate
        {
            get { return (DataTemplate)GetValue(MasterHeaderTemplateProperty); }
            set { SetValue(MasterHeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="MasterHeaderTemplate"/> dependency property.
        /// </summary>
        /// <returns>The identifier for the <see cref="MasterHeaderTemplate"/> dependency property.</returns>
        public static readonly DependencyProperty MasterHeaderTemplateProperty = DependencyProperty.Register(
            nameof(MasterHeaderTemplate),
            typeof(DataTemplate),
            typeof(MasterDetailsView2),
            new PropertyMetadata(null));
#endregion

#region MasterPaneWidth
        /// <summary>
        /// Gets or sets the width of the master pane when the view is expanded.
        /// </summary>
        /// <returns>
        /// The width of the SplitView pane when it's fully expanded. The default is 320
        /// device-independent pixel (DIP).
        /// </returns>
        public double MasterPaneWidth
        {
            get { return (double)GetValue(MasterPaneWidthProperty); }
            set { SetValue(MasterPaneWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="MasterPaneWidth"/> dependency property.
        /// </summary>
        /// <returns>The identifier for the <see cref="MasterPaneWidth"/> dependency property.</returns>
        public static readonly DependencyProperty MasterPaneWidthProperty = DependencyProperty.Register(
            nameof(MasterPaneWidth),
            typeof(double),
            typeof(MasterDetailsView2),
            new PropertyMetadata(320d));
#endregion

#region DetailsCommandBar
        /// <summary>
        /// Gets or sets the <see cref="CommandBar"/> for the details section.
        /// </summary>
        public CommandBar DetailsCommandBar
        {
            get { return (CommandBar)GetValue(DetailsCommandBarProperty); }
            set { SetValue(DetailsCommandBarProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="DetailsCommandBar"/> dependency property
        /// </summary>
        /// <returns>The identifier for the <see cref="DetailsCommandBar"/> dependency property.</returns>
        public static readonly DependencyProperty DetailsCommandBarProperty = DependencyProperty.Register(
            nameof(DetailsCommandBar),
            typeof(CommandBar),
            typeof(MasterDetailsView2),
            new PropertyMetadata(null, OnDetailsCommandBarChanged));

        /// <summary>
        /// Fired when the DetailsCommandBar changes.
        /// </summary>
        /// <param name="d">The sender</param>
        /// <param name="e">The event args</param>
        private static void OnDetailsCommandBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (MasterDetailsView2)d;
            view.OnDetailsCommandBarChanged();
        }
#endregion

#region NoSelectionContent
        /// <summary>
        /// Gets or sets the content to dsiplay when there is no item selected in the master list.
        /// </summary>
        public object NoSelectionContent
        {
            get { return GetValue(NoSelectionContentProperty); }
            set { SetValue(NoSelectionContentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="NoSelectionContent"/> dependency property.
        /// </summary>
        /// <returns>The identifier for the <see cref="NoSelectionContent"/> dependency property.</returns>
        public static readonly DependencyProperty NoSelectionContentProperty = DependencyProperty.Register(
            nameof(NoSelectionContent),
            typeof(object),
            typeof(MasterDetailsView2),
            new PropertyMetadata(null));
#endregion

#region NoSelectionContentTemplate
        /// <summary>
        /// Gets or sets the DataTemplate used to display the content when there is no selection.
        /// </summary>
        /// <returns>
        /// The template that specifies the visualization of the content when there is no
        /// selection. The default is null.
        /// </returns>
        public DataTemplate NoSelectionContentTemplate
        {
            get { return (DataTemplate)GetValue(NoSelectionContentTemplateProperty); }
            set { SetValue(NoSelectionContentTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="NoSelectionContentTemplate"/> dependency property.
        /// </summary>
        /// <returns>The identifier for the <see cref="NoSelectionContentTemplate"/> dependency property.</returns>
        public static readonly DependencyProperty NoSelectionContentTemplateProperty = DependencyProperty.Register(
            nameof(NoSelectionContentTemplate),
            typeof(DataTemplate),
            typeof(MasterDetailsView2),
            new PropertyMetadata(null));
#endregion

#region ViewState
        /// <summary>
        /// Gets the current visual state of the control
        /// </summary>
        public MasterDetailsViewState ViewState
        {
            get { return (MasterDetailsViewState)GetValue(ViewStateProperty); }
            private set { SetValue(ViewStateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ViewState"/> dependency property
        /// </summary>
        /// <returns>The identifier for the <see cref="ViewState"/> dependency property.</returns>
        public static readonly DependencyProperty ViewStateProperty = DependencyProperty.Register(
            nameof(ViewState),
            typeof(MasterDetailsViewState),
            typeof(MasterDetailsView2),
            new PropertyMetadata(default(MasterDetailsViewState)));
#endregion

#region CompactModeThresholdWidth
        /// <summary>
        /// Gets or sets the Threshold width that witll trigger the control to go into compact mode
        /// </summary>
        public double CompactModeThresholdWidth
        {
            get { return (double)GetValue(CompactModeThresholdWidthProperty); }
            set { SetValue(CompactModeThresholdWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="CompactModeThresholdWidth"/> dependancy property
        /// </summary>
        public static readonly DependencyProperty CompactModeThresholdWidthProperty = DependencyProperty.Register(
            nameof(CompactModeThresholdWidth),
            typeof(double),
            typeof(MasterDetailsView2),
            new PropertyMetadata(720d, OnCompactModeThresholdWidthChanged));

        /// <summary>
        /// Fired when CompactModeThresholdWIdthChanged
        /// </summary>
        /// <param name="d">The sender</param>
        /// <param name="e">The event args</param>
        private static void OnCompactModeThresholdWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MasterDetailsView2)d).HandleStateChanges();
        }
#endregion

#region MasterCommandBar
        /// <summary>
        /// Gets or sets the <see cref="CommandBar"/> for the master section.
        /// </summary>
        public CommandBar MasterCommandBar
        {
            get { return (CommandBar)GetValue(MasterCommandBarProperty); }
            set { SetValue(MasterCommandBarProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="MasterCommandBar"/> dependency property
        /// </summary>
        /// <returns>The identifier for the <see cref="MasterCommandBar"/> dependency property.</returns>
        public static readonly DependencyProperty MasterCommandBarProperty = DependencyProperty.Register(
            nameof(MasterCommandBar),
            typeof(CommandBar),
            typeof(MasterDetailsView2),
            new PropertyMetadata(null, OnMasterCommandBarChanged));

        /// <summary>
        /// Fired when the MasterCommandBar changes.
        /// </summary>
        /// <param name="d">The sender</param>
        /// <param name="e">The event args</param>
        private static void OnMasterCommandBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (MasterDetailsView2)d;
            view.OnMasterCommandBarChanged();
        }
#endregion

        /// <summary>
        /// Gets or sets a function for mapping the selected item to a different model.
        /// This new model will be the DataContext of the Details area.
        /// </summary>
        //public Func<object, object> MapDetails { get; set; }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled == false)
            {
                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
                _selectionStateGroup = (VisualStateGroup)GetTemplateChild(SelectionStates);
                if (_selectionStateGroup != null)
                {
                    _selectionStateGroup.CurrentStateChanged += OnSelectionStateChanged;
                }

                UpdateView(true);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled == false)
            {
                SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
                _selectionStateGroup = (VisualStateGroup)GetTemplateChild(SelectionStates);
                if (_selectionStateGroup != null)
                {
                    _selectionStateGroup.CurrentStateChanged -= OnSelectionStateChanged;
                    _selectionStateGroup = null;
                }
            }
        }

        private void MasterDetailsView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // if size is changing
            if ((e.PreviousSize.Width < CompactModeThresholdWidth && e.NewSize.Width >= CompactModeThresholdWidth) ||
                (e.PreviousSize.Width >= CompactModeThresholdWidth && e.NewSize.Width < CompactModeThresholdWidth))
            {
                HandleStateChanges();
            }
        }

        private void HandleStateChanges()
        {
            UpdateView(true);
            //SetListSelectionWithKeyboardFocusOnVisualStateChanged(ViewState);
        }

        /// <summary>
        /// Closes the details pane if we are in narrow state
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="args">The event args</param>
        public void OnBackRequested(object sender, BackRequestedEventArgs args)
        {
            if (args != null)
            {
                if (args.Handled == true)
                    return;
            }

            if (SelectedItem == true)
            {
                SelectedItem = false;

                if (args != null)
                    args.Handled = true;
                //
                //OnSelectionChanged(new SelectionChangedEventArgs(new List<object> { null }, new List<object> { null }));

            }
        }

        /// <summary>
        /// Выставляем VisualState
        /// </summary>
        /// <param name="animate"></param>
        private void UpdateView(bool animate)
        {
            UpdateViewState();
            SetVisualState(animate);
        }

        /// <summary>
        /// Sets the back button visibility based on the current visual state and selected item
        /// </summary>
        private void SetBackButtonVisibility(MasterDetailsViewState previousState)
        {
            if (DesignMode.DesignModeEnabled)
            {
                return;
            }

            if (ViewState == MasterDetailsViewState.Details)
            {
                var navigationManager = SystemNavigationManager.GetForCurrentView();
                _previousBackButtonVisibility = navigationManager.AppViewBackButtonVisibility;

                navigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            else if (previousState == MasterDetailsViewState.Details)
            {
                // Make sure we show the back button if the stack can navigate back
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = _previousBackButtonVisibility;
            }
        }
        
        public void SelectedItemFast()
        {
            _selectedItem = true;
            DetailsPresenterTransform.X = 0;
            MasterPanelTransform.X = 0;
            this.UpdateView(false);
            this.IsSelectedChanged?.Invoke(this, true);
        }

        private bool _selectedItem;
        public bool SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (_selectedItem == value)
                    return;

                _selectedItem = value;
                if(value == true)
                {
                    DetailsPresenterTransform.X =0;
                    MasterPanelTransform.X = 0;
                }
                else
                {
                    MasterPanelTransform.X = 0;
                    DetailsPresenterTransform.X = 0;
                }
                this.IsSelectedChanged?.Invoke(this, value);
                this.UpdateView(true);
            }
        }

        /// <summary>
        /// Оповещвем об изменении вида
        /// и о видимости кнопки Назад
        /// </summary>
        private void UpdateViewState()
        {
            var previousState = ViewState;

            if (ActualWidth < CompactModeThresholdWidth)
            {
                ViewState = SelectedItem == false ? MasterDetailsViewState.Master : MasterDetailsViewState.Details;
            }
            else
            {
                ViewState = MasterDetailsViewState.Both;
            }

            if (previousState != ViewState)
            {
                ViewStateChanged?.Invoke(this, ViewState);
                SetBackButtonVisibility(previousState);
            }
            //
            //SetBackButtonVisibility(ViewState);
            //
        }

        private void SetVisualState(bool animate)
        {
            if(DetailsPresenterTransform!=null)
                DetailsPresenterTransform.X = 0;//BugFix?

            if(MasterPanelTransform!=null)
                MasterPanelTransform.X = 0;//BugFix?

            string state;
            string noSelectionState;
            string hasSelectionState;
            if (base.ActualWidth < this.CompactModeThresholdWidth)
            {
                state = NarrowState;
                noSelectionState = NoSelectionNarrowState;
                hasSelectionState = HasSelectionNarrowState;

                //if (SelectedItem == false)
                //    animate = false;
            }
            else
            {
                state = WideState;
                noSelectionState = NoSelectionWideState;
                hasSelectionState = HasSelectionWideState;
            }

            VisualStateManager.GoToState(this, SelectedItem == false ? noSelectionState : hasSelectionState, animate);
            VisualStateManager.GoToState(this, state, animate);
        }
        
        private void OnMasterCommandBarChanged()
        {
            OnCommandBarChanged("MasterCommandBarPanel", MasterCommandBar);
        }

        private void OnDetailsCommandBarChanged()
        {
            OnCommandBarChanged("DetailsCommandBarPanel", DetailsCommandBar);
        }

        private void OnCommandBarChanged(string panelName, CommandBar commandbar)
        {
            var panel = GetTemplateChild(panelName) as Panel;
            if (panel == null)
            {
                return;
            }

            panel.Children.Clear();
            if (commandbar != null)
            {
                panel.Children.Add(commandbar);
            }
        }
        
        /// <summary>
        /// Fires when the selection state of the control changes
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e">the event args</param>
        /// <remarks>
        /// Sets focus to the item list when the viewState is not Details.
        /// Sets whether the selected item should change when focused with the keyboard.
        /// </remarks>
        private void OnSelectionStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            try
            {
                SetFocus(ViewState);
            }
            catch
            {

            }
            //SetListSelectionWithKeyboardFocusOnVisualStateChanged(ViewState);
        }

        /// <summary>
        /// Sets focus to the relevant control based on the viewState.
        /// </summary>
        /// <param name="viewState">the view state</param>
        private void SetFocus(MasterDetailsViewState viewState)
        {
            if (viewState != MasterDetailsViewState.Details)
            {
                //FocusItemList();
            }
            else
            {
                FocusFirstFocusableElementInDetails();
            }
        }

        /// <summary>
        /// Sets focus to the first focusable element in the details template
        /// </summary>
        private void FocusFirstFocusableElementInDetails()
        {
            if (DetailsPanel is DependencyObject details)
            {
                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4))
                {
                    var focusableElement = FocusManager.FindFirstFocusableElement(details);
                    if (focusableElement != null && focusableElement is Control control)
                    {
                        control.Focus(FocusState.Programmatic);
                        //FocusManager.TryFocusAsync(control, FocusState.Programmatic);
                    }
                }
            }
        }

        /// <summary>
        /// The <see cref="MasterDetailsView"/> state.
        /// </summary>
        public enum MasterDetailsViewState
        {
            /// <summary>
            /// Only the Master view is shown
            /// </summary>
            Master,

            /// <summary>
            /// Only the Details view is shown
            /// </summary>
            Details,

            /// <summary>
            /// Both the Master and Details views are shown
            /// </summary>
            Both
        }
    }
}