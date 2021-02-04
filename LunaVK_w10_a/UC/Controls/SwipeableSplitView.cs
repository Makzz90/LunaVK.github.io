using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;


//https://github.com/JustinXinLiu/SwipeableSplitView/blob/master/GestureDemo/SwipeableSplitView.cs

namespace LunaVK.UC
{
    public sealed class SwipeableSplitView : SplitView
    {
        private Grid PaneRoot;
        private Grid ContentRoot;
        CompositeTransform _paneRootTransform;
        internal Windows.UI.Xaml.Shapes.Rectangle LightDismissLayer;
//        public event EventHandler<bool> _paneOpenChanged;
//        private EasingFunctionBase _menuEasing;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.PaneRoot = GetTemplateChild("PaneRoot") as Grid;
            this.ContentRoot = GetTemplateChild("ContentRoot") as Grid;
            this.LightDismissLayer = GetTemplateChild("LightDismissLayer") as Windows.UI.Xaml.Shapes.Rectangle;
            //this.LightDismissLayer.Fill = new SolidColorBrush(Windows.UI.Colors.Green);
            this.LightDismissLayer.Tapped += this.OnDismissLayerTapped;

            _paneRootTransform = PaneRoot.RenderTransform as CompositeTransform;
            this.ContentRoot.ManipulationMode = /*ManipulationModes.System |*/ ManipulationModes.TranslateX;
            this.ContentRoot.ManipulationDelta += this.ContentRoot_ManipulationDelta;
            this.ContentRoot.ManipulationCompleted += this.ContentRoot_ManipulationCompleted;

            // initialization
            OnDisplayModeChanged(null, null);
            RegisterPropertyChangedCallback(DisplayModeProperty, OnDisplayModeChanged);

            base.PaneClosed += PaneClosedCallback;
            base.RegisterPropertyChangedCallback(IsPaneOpenProperty, PaneOpenCallback);

//            this._IsPaneOpen2 = this.IsPaneOpen;
            this.PaneClosedCallback(this , null);



            var temp = GetTemplateChild("DisplayModeStates") as VisualStateGroup;
            //int i = 0;
            foreach(VisualTransition tr in temp.Transitions)
            {
                storyboards.Add(tr.Storyboard);
                //System.Diagnostics.Debug.WriteLine(string.Format("#{0} from{1} t0{2}",i, tr.From, tr.To));
                //i++;
            }



            var rootGrid = this.PaneRoot.Parent as Grid;
            
            //OpenSwipeablePaneAnimation = GetStoryboard(rootGrid,"OpenSwipeablePane");
//            CloseSwipeablePaneAnimation = GetStoryboard(rootGrid,"CloseSwipeablePane");
        }

        List<Storyboard> storyboards = new List<Storyboard>();
        int story = -1;
        /*
        public Storyboard GetStoryboard( FrameworkElement element, string name, string message = null)
        {
            var storyboard = element.Resources[name] as Storyboard;

            if (storyboard == null)
            {
                if (message == null)
                {
                    message = $"Storyboard '{name}' cannot be found! Check the default Generic.xaml.";
                }

                throw new NullReferenceException(message);
            }

            return storyboard;
        }
        */

        Storyboard _openSwipeablePane;
        Storyboard _closeSwipeablePane;

        // safely subscribe/unsubscribe animation completed events here
        internal Storyboard OpenSwipeablePaneAnimation
        {
            get { return _openSwipeablePane; }
            set
            {
                if (_openSwipeablePane != null)
                {
                    _openSwipeablePane.Completed -= OnOpenSwipeablePaneCompleted;
                }

                _openSwipeablePane = value;

                if (_openSwipeablePane != null)
                {
                    _openSwipeablePane.Completed += OnOpenSwipeablePaneCompleted;
                }
            }
        }

        // safely subscribe/unsubscribe animation completed events here
        internal Storyboard CloseSwipeablePaneAnimation
        {
            get { return _closeSwipeablePane; }
            set
            {
                if (_closeSwipeablePane != null)
                {
                    _closeSwipeablePane.Completed -= OnCloseSwipeablePaneCompleted;
                }

                _closeSwipeablePane = value;

                if (_closeSwipeablePane != null)
                {
                    _closeSwipeablePane.Completed += OnCloseSwipeablePaneCompleted;
                }
            }
        }

        void OnOpenSwipeablePaneCompleted(object sender, object e)
        {
            this.LightDismissLayer.Visibility = Visibility.Visible;
            this.LightDismissLayer.IsHitTestVisible = true;
        }

        void OnCloseSwipeablePaneCompleted(object sender, object e)
        {
            this.LightDismissLayer.Visibility = Visibility.Collapsed;
            this.LightDismissLayer.IsHitTestVisible = false;
        }

#region IsSwipeablePaneOpen
        public bool IsSwipeablePaneOpen
        {
            get { return (bool)GetValue(IsSwipeablePaneOpenProperty); }
            set { SetValue(IsSwipeablePaneOpenProperty, value); }
        }

        public static readonly DependencyProperty IsSwipeablePaneOpenProperty = DependencyProperty.Register(nameof(IsSwipeablePaneOpen), typeof(bool), typeof(SwipeableSplitView), new PropertyMetadata(false, OnIsSwipeablePaneOpenChanged));

        static void OnIsSwipeablePaneOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SwipeableSplitView splitView = (SwipeableSplitView)d;

            switch (splitView.DisplayMode)
            {
                case SplitViewDisplayMode.Inline:
                case SplitViewDisplayMode.CompactOverlay:
                case SplitViewDisplayMode.CompactInline:
                    {
                        splitView.IsPaneOpen = (bool)e.NewValue;

                        if (splitView.IsPaneOpen)
                            splitView.OnOpenSwipeablePaneCompleted(null, null);
                        else
                            splitView.OnCloseSwipeablePaneCompleted(null, null);
                    }
                    break;

                case SplitViewDisplayMode.Overlay:
                    {
                        
                        //if (splitView.OpenSwipeablePaneAnimation == null || splitView.CloseSwipeablePaneAnimation == null) return;
                        if ((bool)e.NewValue)
                        {
                            //OpenOverlayRight OpenOverlayLeft
                            //ClosedCompactLeft OpenCompactOverlayLeft
                            //ClosedCompactRight OpenCompactOverlayRight
                            //OpenInlineLeft OpenInlineRight
                            //Closed
                            //OverlayNotVisible OverlayVisible
                            //VisualStateManager.GoToState(splitView, "OverlayVisible", false);

                            //VisualStateManager.GoToState(splitView, "OpenOverlayLeft", true);

                            //if(splitView.story!=-1)
                            //    splitView.storyboards[splitView.story].Begin();

                            //splitView.storyboards[0].Begin();
                            
                            if (splitView.OpenSwipeablePaneAnimation == null || splitView._paneRootTransform.TranslateX != splitView.TemplateSettings.NegativeOpenPaneLength)
                                splitView.OpenSwipeablePaneAnimation = splitView._paneRootTransform.Animate(splitView._paneRootTransform.TranslateX, 0, "TranslateX", 100);
                            else
                                splitView.OpenSwipeablePaneAnimation.Begin();
                            //splitView.IsPaneOpen = true;
                            //splitView.OpenSwipeablePane();
                            //splitView.LightDismissLayer.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            //splitView.IsPaneOpen = false;
                            //VisualStateManager.GoToState(splitView, "Closed", true);
                            //splitView.storyboards[4].Begin();

                            //splitView.LightDismissLayer.Visibility = Visibility.Collapsed;
                            //splitView.CloseSwipeablePane();
                            if (splitView.CloseSwipeablePaneAnimation == null || splitView._paneRootTransform.TranslateX != 0)
                                splitView.CloseSwipeablePaneAnimation = splitView._paneRootTransform.Animate(splitView._paneRootTransform.TranslateX, -splitView.OpenPaneLength, "TranslateX", 100);
                            else
                                splitView.CloseSwipeablePaneAnimation.Begin();
                        }
                        break;
                    }
            }
        }
#endregion IsSwipeablePaneOpen







        private void PaneOpenCallback(DependencyObject sender, DependencyProperty dp)
        {
            this.PaneClosedCallback(sender as SplitView, null);
        }

        private void PaneClosedCallback(SplitView sender, object args)
        {
//            this._IsPaneOpen2 = this.IsPaneOpen;
//            this._paneOpenChanged?.Invoke(sender, this.IsPaneOpen2);
        }
        
/*

        /// <summary>
        /// Событие открытия меню
        /// </summary>
        public event EventHandler<bool> PaneOpenChanged
        {
            add
            {
                this._paneOpenChanged += value;
                value(this, this.IsPaneOpen2);
            }
            remove
            {
                this._paneOpenChanged -= value;
            }
        }
*/
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

            if (e.Cumulative.Translation.X > 100)
                OpenSwipeablePane();
            else
                CloseSwipeablePane();
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

            var x = _paneRootTransform.TranslateX + e.Delta.Translation.X;

            // keep the pan within the bountry
            if (x > 0)
            {
                _paneRootTransform.TranslateX = 0;
                return;
            }
            _paneRootTransform.TranslateX += e.Delta.Translation.X;
        }

        public void ActivateSwipe(bool status)
        {
            this.ContentRoot.Tag = status ? null : "CantTouchThis";
            //this.ContentRoot.ManipulationDelta -= ContentRoot_ManipulationDelta;
            //this.ContentRoot.ManipulationCompleted -= ContentRoot_ManipulationCompleted;
            //this.CloseSwipeablePane();
        }

        void OpenSwipeablePane()
        {
            //           base.IsPaneOpen = true;
            //            this._IsPaneOpen2 = true;
            //            _paneRootTransform.Animate(_paneRootTransform.TranslateX, 0, "TranslateX", 200, 0, this._menuEasing);
            //           this.DismissLayer.Visibility = Visibility.Visible;
            //            this._paneOpenChanged?.Invoke(this, true);
            
            if (IsSwipeablePaneOpen)
            {
                OpenSwipeablePaneAnimation.Begin();
            }
            else
            {
                IsSwipeablePaneOpen = true;
            }
            
        }

        void CloseSwipeablePane()
        {
            //            base.IsPaneOpen = false;
            //            this._IsPaneOpen2 = false;
            //           _paneRootTransform.Animate(_paneRootTransform.TranslateX, -base.OpenPaneLength, "TranslateX", 200, 0, this._menuEasing);
            //           this.DismissLayer.Visibility = Visibility.Collapsed;
            //           this._paneOpenChanged?.Invoke(this, false);

            
            if (!IsSwipeablePaneOpen)
            {
                CloseSwipeablePaneAnimation.Begin();
            }
            else
            {
                IsSwipeablePaneOpen = false;
            }
            
        }

        /*
        private bool _IsPaneOpen2;

        /// <summary>
        /// Возвращает или задает значение, указывающее, развернута ли панель SplitView на
        //  полную ширину. Реагирует и на свайп и на системное событие открытия.
        /// </summary>
        public bool IsPaneOpen2
        {
            get { return this._IsPaneOpen2; }
            set
            {
                this._IsPaneOpen2 = value;
                if(DisplayMode!= SplitViewDisplayMode.Overlay)
                {
                    IsPaneOpen = value;
                    return;
                }

                if (value == true)
                    OpenSwipeablePane();
                else
                    CloseSwipeablePane();
            }
        }
        */
        

#region native property change handlers
        void OnDisplayModeChanged(DependencyObject sender, DependencyProperty dp)
        {
            switch (DisplayMode)
            {
                case SplitViewDisplayMode.Inline:
                case SplitViewDisplayMode.CompactOverlay:
                case SplitViewDisplayMode.CompactInline:
                    _paneRootTransform.TranslateX = 0;
                    PaneRoot.Visibility = Visibility.Collapsed;
                    LightDismissLayer.Visibility = Visibility.Collapsed;
                    break;

                case SplitViewDisplayMode.Overlay:
                    _paneRootTransform.TranslateX = -OpenPaneLength;
                    PaneRoot.Visibility = Visibility.Visible;
                    //DismissLayer.Visibility = Visibility.Visible;
                    break;
            }
        }
#endregion

#region DismissLayer tap event handlers
        void OnDismissLayerTapped(object sender, TappedRoutedEventArgs e)
        {
            /*
            if (DisplayMode != SplitViewDisplayMode.Overlay)
            {
//                this._paneOpenChanged?.Invoke(this, IsPaneOpen);
                return;
            }

            CloseSwipeablePane();
            */
            this.IsSwipeablePaneOpen = false;
        }
#endregion
        
    }
}
