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

// Шаблон элемента пользовательского элемента управления задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234236

namespace LunaVK.UC.Attachment
{
    public sealed partial class OutboundMessageAttachment : UserControl
    {
        public event EventHandler<object> Temp;
        /*
        public static readonly DependencyProperty RefreshCommandProperty = DependencyProperty.Register("DeleteCommand", typeof(Action<object>), typeof(OutboundMessageAttachment), new PropertyMetadata(null));
        public Action<object> DeleteCommand
        {
            get { return (Action<object>)GetValue(RefreshCommandProperty); }
            set { SetValue(RefreshCommandProperty, value); }
        }
        */
        public OutboundMessageAttachment()
        {
            this.InitializeComponent();
        }

        private void Delete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.Temp != null)
                this.Temp(sender,this.DataContext);

        }
    }
}
