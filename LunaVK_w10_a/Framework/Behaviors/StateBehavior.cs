using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xaml.Interactivity;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.Framework.Behaviors
{
    public class StateBehavior : DependencyObject, IBehavior
    {
        /// <summary>
        /// Объект зависимостей, к которому присоединяется поведение.
        /// </summary>
        public DependencyObject AssociatedObject { get; private set; }

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(StateBehavior), new PropertyMetadata(null, StateBehavior.ValuePropertyChanged));

        private static void ValuePropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var behavior = sender as StateBehavior;
            if (behavior.AssociatedObject == null || e.NewValue == null)
                return;

            VisualStateManager.GoToState(behavior.AssociatedObject as Control, e.NewValue as string, true);
        }

        public void Attach(DependencyObject associatedObject)
        {
            var control = associatedObject as Control;
            if (control == null)
                throw new ArgumentException( "EnumStateBehavior can be attached only to Control");

            this.AssociatedObject = associatedObject;
        }

        public void Detach()
        {
            this.AssociatedObject = null;
        }
    }
}
