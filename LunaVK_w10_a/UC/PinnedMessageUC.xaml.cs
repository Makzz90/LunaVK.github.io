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
using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;
using LunaVK.Core.Library;
using System.Diagnostics;

namespace LunaVK.UC
{
    public sealed partial class PinnedMessageUC : UserControl
    {
        public Action HideMsgCallback;

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(VKPinnedMessage), typeof(PinnedMessageUC), new PropertyMetadata(default(object), OnDataChanged));

        /// <summary>
        /// Данные.
        /// </summary>
        public VKPinnedMessage Data
        {
            get { return (VKPinnedMessage)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        private static void OnDataChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((PinnedMessageUC)obj).ProcessData();
        }

        public PinnedMessageUC()
        {
            this.InitializeComponent();
            base.Visibility = Visibility.Collapsed;
        }

        private void ProcessData()
        {
            this.MainContent.Children.Clear();

            if (this.Data == null)
            {
                base.Visibility = Visibility.Collapsed;
                return;
            }

            Debug.Assert(this.Data.from_id > 0);
            var u = UsersService.Instance.GetCachedUser((uint)this.Data.from_id);

            this.TextBlockOwner.Text = u.Title;
            this.TextBlockDate.Text = UIStringFormatterHelper.FormatDateTimeForUI(this.Data.date);

            if (!string.IsNullOrEmpty(this.Data.text))
            {
                TextBlock text = new TextBlock();
                text.Text = this.Data.text.Replace('\n',' ');
                text.FontSize = 16;
                text.MaxLines = 1;
                this.MainContent.Children.Add(text);
            }

            base.Visibility = Visibility.Visible;


        }

        private void Hide_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //base.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.HideMsgCallback?.Invoke();
        }
    }
}
