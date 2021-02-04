using LunaVK.Framework;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LunaVK.UC.Controls
{
    public partial class NavigationViewItem : ContentControl
    {
#region Icon
        public IconElement Icon
        {
            get { return (IconElement)base.GetValue(IconProperty); }
            set { base.SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(bool), typeof(NavigationViewItem), new PropertyMetadata(null));
        #endregion

#region Count
        public string Count
        {
            get { return (string)base.GetValue(CountProperty); }
            set { base.SetValue(CountProperty, value); }
        }

        public static readonly DependencyProperty CountProperty = DependencyProperty.Register(nameof(Count), typeof(string), typeof(NavigationViewItem), new PropertyMetadata("", CountChanged));

        private static void CountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NavigationViewItem lv = (NavigationViewItem)d;
            string val = (string)e.NewValue;
            
            VisualStateManager.GoToState(lv, string.IsNullOrEmpty(val) || val == "0" ? "CountHidden" : "CountVisible", true);
        }
#endregion

        public NavigationViewItem()
        {
            if (CustomFrame.Instance != null)
                CustomFrame.Instance.MenuOpenChanged += this.CFrame_MenuOpenChanged;

            this.Loaded += NavigationViewItem_Loaded;
            this.Unloaded += NavigationViewItem_Unloaded;
        }

        private void NavigationViewItem_Unloaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            
            element.PointerEntered -= Element_PointerEntered;
            element.PointerExited -= Element_PointerExited;
            element.PointerCaptureLost -= Element_PointerExited;
            
        }

        private void NavigationViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (!(element.Parent is NavigationView))
            {
                element.PointerEntered += Element_PointerEntered;
                element.PointerExited += Element_PointerExited;
                element.PointerCaptureLost += Element_PointerExited;
            }
        }

        private void Element_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Control ctrl = sender as Control;
            if (ctrl == null)
                return;
            
            VisualStateManager.GoToState(ctrl, "Normal", true);
        }

        private void Element_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Control ctrl = sender as Control;
            if (ctrl == null)
                return;
            
            VisualStateManager.GoToState(ctrl, "PointerOver", true);
        }

        private void CFrame_MenuOpenChanged(object sender, bool e)
        {
            VisualStateManager.GoToState(this, e ? "Opened" : "Closed", true);
        }
        /*
        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
            VisualStateManager.GoToState(this, "PointerOver", true);
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            VisualStateManager.GoToState(this, "Normal", true);
        }

        protected override void OnPointerCaptureLost(PointerRoutedEventArgs e)
        {
            base.OnPointerCaptureLost(e);
            VisualStateManager.GoToState(this, "Normal", true);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            VisualStateManager.GoToState(this, "Pressed", true);
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            VisualStateManager.GoToState(this, "PointerOver", true);
        }
        */
    }
}
