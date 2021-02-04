using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace LunaVK.UC
{
    public sealed partial class ServiceOptionUC : UserControl
    {
        /*
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.RegisterAttached("IsChecked", typeof(bool), typeof(ServiceOptionUC), new PropertyMetadata(false, IsChecked_OnChanged));
        public bool IsChecked
        {
            get { return (bool)base.GetValue(IsCheckedProperty); }
            set { base.SetValue(IsCheckedProperty, value); }
        }

        private static void IsChecked_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ServiceOptionUC toggleControl = (ServiceOptionUC)d;
            bool b = (bool)e.NewValue;
            toggleControl.togg.IsChecked = b;
        }
        */
        public static readonly DependencyProperty IsExtendedFormProperty = DependencyProperty.RegisterAttached("IsExtendedForm", typeof(bool), typeof(ServiceOptionUC), new PropertyMetadata(false, IsExtendedForm_OnChanged));
        public bool IsExtendedForm
        {
            get { return (bool)base.GetValue(IsExtendedFormProperty); }
            set { base.SetValue(IsExtendedFormProperty, value); }
        }
        //2
        private static void IsExtendedForm_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ServiceOptionUC toggleControl = (ServiceOptionUC)d;
            bool b = (bool)e.NewValue;
            
            toggleControl.combo.Visibility = b ? Visibility.Visible : Visibility.Collapsed;
            toggleControl.togg.Visibility = b ? Visibility.Collapsed : Visibility.Visible;
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached("Value", typeof(int), typeof(ServiceOptionUC), new PropertyMetadata(-1, Value_OnChanged));
        public int Value
        {
            get { return (int)base.GetValue(ValueProperty); }
            set { base.SetValue(ValueProperty, value); }
        }
        //3
        private static void Value_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ServiceOptionUC toggleControl = (ServiceOptionUC)d;
            int value = (int)e.NewValue;
            if (value == -1)
                return;

            toggleControl.UpdateComboBox();
        }

        public static readonly DependencyProperty OptionsProperty = DependencyProperty.RegisterAttached("Options", typeof(int), typeof(ServiceOptionUC), new PropertyMetadata(0/*, Options_OnChanged*/));
        public int Options
        {
            get { return (int)base.GetValue(OptionsProperty); }
            set { base.SetValue(OptionsProperty, value); }
        }

        private void UpdateComboBox()
        {
            if (this.IsExtendedForm)
            {
                this.combo.Items.Clear();

                this.combo.Items.Add("Отключены");

                switch (this.Options)
                {
                    case 2:
                        {

                            this.combo.Items.Add("Включены");
                            break;
                        }
                    case 3:
                        {
                            this.combo.Items.Add("Открытые");
                            this.combo.Items.Add("Ограниченные");
                            break;
                        }
                    case 4:
                        {
                            this.combo.Items.Add("Открытые");
                            this.combo.Items.Add("Ограниченные");
                            this.combo.Items.Add("Закрытая");
                            break;
                        }
                }

                this.combo.SelectedIndex = this.Value;
            }
            else
            {
                this.Options = 2;
                this.togg.IsChecked = this.Value == 1;
            }
        }
        //1
        private static void Options_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            /*
            ServiceOptionUC toggleControl = (ServiceOptionUC)d;
            int count = (int)e.NewValue;
            if (!toggleControl.IsExtendedForm)
                count = 2;

            toggleControl.combo.Items.Clear();
            toggleControl.combo.Items.Add("Отключены");

            switch(count)
            {
                case 2:
                    {
                        
                        toggleControl.combo.Items.Add("Включены");
                        break;
                    }
                case 3:
                    {
                        toggleControl.combo.Items.Add("Открытые");
                        toggleControl.combo.Items.Add("Ограниченные");
                        break;
                    }
            }*/
            
        }

        public ServiceOptionUC()
        {
            this.InitializeComponent();
            this.togg.Checked += togg_Checked;
            this.combo.SelectionChanged += combo_SelectionChanged;
        }

        void combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if(this.Value != cb.SelectedIndex)
                this.Value = cb.SelectedIndex;
        }

        void togg_Checked(object sender, RoutedEventArgs e)
        {
            ToggleSwitch t = sender as ToggleSwitch;
            int val = t.IsChecked ? 1 : 0;
            if(this.Value != val)
                this.Value = val;
            //if (this.IsChecked != t.IsChecked)
            //    this.IsChecked = t.IsChecked;
        }

        public string Icon
        {
            get { return this.fIcon.Glyph; }
            set
            {
                this.fIcon.Glyph = value;
            }
        }

        public string Title
        {
            get { return this.tTitle.Text; }
            set
            {
                this.tTitle.Text = value;
            }
        }

        public object ItemsSource
        {
            get
            {
                return this.combo.ItemsSource;
            }
        }

        //public event EventHandler<RoutedEventArgs> IsChecked
        //{
        //    add
        //    {
        //        this.togg.Checked += value;
        //    }
        //    remove
        //    {
        //        this.togg.Checked -= value;
        //    }
        //}
    }
}
