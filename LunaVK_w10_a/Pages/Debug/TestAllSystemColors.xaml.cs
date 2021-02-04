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
    public sealed partial class TestAllSystemColors : Page
    {
        public ObservableCollection<SystemColor> Items { get; private set; }

        public TestAllSystemColors()
        {
            this.InitializeComponent();

            this.Items = new ObservableCollection<SystemColor>();
            base.DataContext = this;

            this.Loaded += TestAllSystemColors_Loaded;
        }

        private void TestAllSystemColors_Loaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public class SystemColor
        {
            public string ColorName { get; set; }

            public SolidColorBrush ColorBrushLight { get; set; }

            public SolidColorBrush ColorBrushDark { get; set; }
        }
    }
}
