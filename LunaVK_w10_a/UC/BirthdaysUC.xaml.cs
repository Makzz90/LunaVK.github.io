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

// Шаблон элемента пользовательского элемента управления задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234236

namespace LunaVK.UC
{
    public sealed partial class BirthdaysUC : UserControl
    {
        public BirthdaysUC()
        {
            this.InitializeComponent();
        }

        private void Birthday_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            //BirthdaysUC.NavigateToUserProfile(sender, false);
        }

        private void SendGift_OnTap(object sender, TappedRoutedEventArgs e)
        {
            //BirthdaysUC.NavigateToGiftsCatalog(sender, false);
        }

        private void Header_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            NavigatorImpl.Instance.NavigateToBirthdaysPage();
        }
    }
}
