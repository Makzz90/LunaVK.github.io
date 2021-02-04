using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestNetworkRequest : Page
    {
        public ObservableCollection<PollOption> PollOptions { get; set; }

        public TestNetworkRequest()
        {
            this.InitializeComponent();
        }

        private void AddAnswer_Click(object sender, RoutedEventArgs e)
        {
            this.PollOptions.Add(new PollOption());
        }

        public class PollOption
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        private void Delete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PollOption vm = (sender as FrameworkElement).DataContext as PollOption;
            this.PollOptions.Remove(vm);
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            PollOption vm = (sender as FrameworkElement).DataContext as PollOption;
            if (vm == null)
                return;
            vm.Name = (sender as TextBox).Text;
        }

        private void Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            PollOption vm = (sender as FrameworkElement).DataContext as PollOption;
            if (vm == null)
                return;
            vm.Value = (sender as TextBox).Text.Replace("\r\n", "\r").Replace("\r", "\r\n");
        }
    }
}
