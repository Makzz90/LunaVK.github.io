using LunaVK.Core.DataObjects;
using LunaVK.Library;
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

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace LunaVK.UC.Attachment
{
    public sealed partial class NewsLinkMediumUC : UserControl
    {
        public NewsLinkMediumUC()
        {
            this.InitializeComponent();
        }

        private VKLink VM
        {
            get { return this.DataContext as VKLink; }
        }

        private void LayoutRoot_Tap(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (string.IsNullOrEmpty(this.VM.url))
                return;

            NavigatorImpl.Instance.NavigateToWebUri(this.VM.url);
        }

        private void ActionButton_OnTap(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            string url = string.IsNullOrEmpty( this.VM.button.action.url) ? this.VM.url : this.VM.button.action.url;
            NavigatorImpl.Instance.NavigateToWebUri(this.VM.button.action.url);
        }
    }
}
