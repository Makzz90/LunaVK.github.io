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
    public sealed partial class SettingsNotificationSectionUC : UserControl
    {
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(SettingsNotificationSectionUC), new PropertyMetadata(false, IsChecked_OnChanged));
        public bool IsChecked
        {
            get { return (bool)base.GetValue(IsCheckedProperty); }
            set { base.SetValue(IsCheckedProperty, value); }
        }


        public static readonly DependencyProperty IsSub1CheckedProperty = DependencyProperty.Register("IsSub1Checked", typeof(bool), typeof(SettingsNotificationSectionUC), new PropertyMetadata(false, IsSub1Checked_OnChanged));
        public bool IsSub1Checked
        {
            get { return (bool)base.GetValue(IsSub1CheckedProperty); }
            set { base.SetValue(IsSub1CheckedProperty, value); }
        }

        public static readonly DependencyProperty IsSub2CheckedProperty = DependencyProperty.Register("IsSub2Checked", typeof(bool), typeof(SettingsNotificationSectionUC), new PropertyMetadata(false, IsSub2Checked_OnChanged));
        public bool IsSub2Checked
        {
            get { return (bool)base.GetValue(IsSub2CheckedProperty); }
            set { base.SetValue(IsSub2CheckedProperty, value); }
        }

        /// <summary>
        /// Происходит при установке
        /// </summary>
        public event EventHandler<RoutedEventArgs> Checked;

        private void FireCheckedEvent(object sender)
        {
            if (this.Initialized)
            {
                if (this.Checked != null)
                    this.Checked(sender, new RoutedEventArgs());
            }
        }

        public SettingsNotificationSectionUC()
        {
            this.InitializeComponent();
            this.togg.Checked += togg_Checked;
            this.tSubTitle1.Unchecked += tSubTitle1_Checked;
            this.tSubTitle1.Checked += tSubTitle1_Checked;
            this.tSubTitle2.Checked += tSubTitle2_Checked;
            this.tSubTitle2.Unchecked += tSubTitle2_Checked;

            this.Loaded += SettingsNotificationSectionUC_Loaded;
        }

        private bool Initialized;

        void SettingsNotificationSectionUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialized = true;
            this.Loaded -= SettingsNotificationSectionUC_Loaded;
        }

        void tSubTitle2_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            base.SetValue(IsSub2CheckedProperty, cb.IsChecked);
        }

        void tSubTitle1_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            base.SetValue(IsSub1CheckedProperty, cb.IsChecked);
        }

        void togg_Checked(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = sender as ToggleSwitch;
            base.SetValue(IsCheckedProperty, ts.IsChecked);
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

        public string SubTitle1
        {
            get { return this.tSubTitle1.Content as string; }
            set
            {
                this.tSubTitle1.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.tSubTitle1.Content = value;
            }
        }

        public string SubTitle2
        {
            get { return this.tSubTitle2.Content as string; }
            set
            {
                this.tSubTitle2.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.tSubTitle2.Content = value;
            }
        }

        private static void IsChecked_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsNotificationSectionUC toggleControl = (SettingsNotificationSectionUC)d;
            bool b = (bool)e.NewValue;
            toggleControl.SetValue(SettingsNotificationSectionUC.IsCheckedProperty, b);
            toggleControl.togg.IsChecked = b;
            toggleControl.FireCheckedEvent(toggleControl.togg);
        }

        private static void IsSub1Checked_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsNotificationSectionUC toggleControl = (SettingsNotificationSectionUC)d;
            bool b = (bool)e.NewValue;
            toggleControl.SetValue(IsSub1CheckedProperty, b);
            toggleControl.tSubTitle1.IsChecked = b;
            toggleControl.FireCheckedEvent(toggleControl.tSubTitle1);
        }

        private static void IsSub2Checked_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsNotificationSectionUC toggleControl = (SettingsNotificationSectionUC)d;
            bool b = (bool)e.NewValue;
            toggleControl.SetValue(IsSub2CheckedProperty, b);
            toggleControl.tSubTitle2.IsChecked = b;
            toggleControl.FireCheckedEvent(toggleControl.tSubTitle2);
        }
    }
}
