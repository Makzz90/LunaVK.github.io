using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LunaVK.Framework.Behaviors
{
    public class BindToActualDimensionBehavior : Behavior<FrameworkElement>
    {
        public FrameworkElement SourceControl
        {
            get { return (FrameworkElement)GetValue(SourceControlProperty); }
            set { SetValue(SourceControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SourceControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceControlProperty =
            DependencyProperty.Register("SourceControl", typeof(FrameworkElement), typeof(BindToActualDimensionBehavior), new PropertyMetadata(null, OnSourceControlChanged));

        private static void OnSourceControlChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {

            ((BindToActualDimensionBehavior)dependencyObject).OnSourceControlChanged((FrameworkElement)dependencyPropertyChangedEventArgs.OldValue, (FrameworkElement)dependencyPropertyChangedEventArgs.NewValue);
        }

        private void OnSourceControlChanged(FrameworkElement oldControl, FrameworkElement newControl)
        {
            if (oldControl != null)
            {
                oldControl.LayoutUpdated -= OnSourceLayoutUpdated;
            }
            if (newControl != null)
            {
                newControl.LayoutUpdated += OnSourceLayoutUpdated;
            }
        }
        public double ActualWidth
        {
            get { return (double)GetValue(ActualWidthProperty); }
            set { SetValue(ActualWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActualWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActualWidthProperty =
            DependencyProperty.Register("ActualWidth", typeof(double), typeof(BindToActualDimensionBehavior), new PropertyMetadata(0.0));



        public double ActualHeight
        {
            get { return (double)GetValue(ActualHeightProperty); }
            set { SetValue(ActualHeightProperty, value); }
        }



        // Using a DependencyProperty as the backing store for ActualHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActualHeightProperty =
            DependencyProperty.Register("ActualHeight", typeof(double), typeof(BindToActualDimensionBehavior), new PropertyMetadata(0.0));



        private void UpdateDimensions()
        {
            ActualHeight = SourceControl.ActualHeight;
            ActualWidth = SourceControl.ActualWidth;
        }



        void OnSourceLayoutUpdated(object sender, object e)
        {
            UpdateDimensions();
        }
    }
}
