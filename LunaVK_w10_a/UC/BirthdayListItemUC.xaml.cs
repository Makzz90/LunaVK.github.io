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
using LunaVK.Framework;

namespace LunaVK.UC
{
    public sealed partial class BirthdayListItemUC : UserControl
    {
        //public static readonly DependencyProperty UserNameProperty = DependencyProperty.Register("UserName", typeof(string), typeof(BirthdayListItemUC), new PropertyMetadata(new PropertyChangedCallback((d, e) => ((BirthdayListItemUC)d).UpdateName())));
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(BirthdayListItemUC), new PropertyMetadata(new PropertyChangedCallback((d, e) => ((BirthdayListItemUC)d).UpdateDescription())));
        public static readonly DependencyProperty GiftVisibilityProperty = DependencyProperty.Register("GiftVisibility", typeof(Visibility), typeof(BirthdayListItemUC), new PropertyMetadata(new PropertyChangedCallback((d, e) => ((BirthdayListItemUC)d).UpdateGiftVisibility())));

        public BirthdayListItemUC()
        {
            this.InitializeComponent();

            //this.userPhoto.LetsRound();
        }
        /*
        public string UserName
        {
            get
            {
                return (string)base.GetValue(BirthdayListItemUC.UserNameProperty);
            }
            set
            {
                base.SetValue(BirthdayListItemUC.UserNameProperty, value);
            }
        }
        */
        public string Description
        {
            get
            {
                return (string)base.GetValue(BirthdayListItemUC.DescriptionProperty);
            }
            set
            {
                base.SetValue(BirthdayListItemUC.DescriptionProperty, value);
            }
        }

        public Visibility GiftVisibility
        {
            get
            {
                return (Visibility)base.GetValue(BirthdayListItemUC.GiftVisibilityProperty);
            }
            set
            {
                base.SetValue(BirthdayListItemUC.GiftVisibilityProperty, value);
            }
        }
        /*
        private void UpdateName()
        {
            this.textBlockUserName.Text = this.UserName;
            this.UpdateNameSize();
        }
        */
        private void UpdateDescription()
        {
            this.textBlockDescription.Text = this.Description;
            this.textBlockDescription.Visibility = string.IsNullOrEmpty(this.Description) ? Visibility.Collapsed : Visibility.Visible;
//            this.UpdateName();
        }

        private void UpdateGiftVisibility()
        {
            this.borderSendGift.Visibility = this.GiftVisibility;
//            this.UpdateName();
        }

        /*
        private void UpdateNameSize()
        {
            double maxWidth = base.Width - 96.0;
            Thickness margin1;
            if (((UIElement)this.borderSendGift).Visibility == Visibility.Visible)
            {
                double num1 = maxWidth;
                Thickness margin2 = this.borderSendGift.Margin;
                // ISSUE: explicit reference operation
                double num2 = margin2.Left + ((FrameworkElement)this.borderSendGift).Width;
                margin1 = ((FrameworkElement)this.borderSendGift).Margin;
                // ISSUE: explicit reference operation
                double right = margin1.Right;
                double num3 = num2 + right;
                maxWidth = num1 - num3;
            }
            if (!string.IsNullOrEmpty(this.Description))
            {
                double num1 = maxWidth;
                margin1 = ((FrameworkElement)this.textBlockDescription).Margin;
                double num2 = margin1.Left + ((FrameworkElement)this.textBlockDescription).ActualWidth;
                margin1 = ((FrameworkElement)this.textBlockDescription).Margin;
                double right = margin1.Right;
                double num3 = num2 + right;
                maxWidth = num1 - num3;
            }
//            this.textBlockUserName.CorrectText(maxWidth);
        }
        */
        public event EventHandler<TappedRoutedEventArgs> ItemTap;

        public event EventHandler<TappedRoutedEventArgs> GiftTap;
    }
}
