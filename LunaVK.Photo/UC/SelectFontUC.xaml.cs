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

namespace LunaVK.Photo.UC
{
    //PredefinedColorPalettes
    public sealed partial class SelectFontUC : UserControl
    {
        public SelectFontUC()
        {
            this.InitializeComponent();
        }

        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gv = sender as GridView;
            var panel = (ItemsWrapGrid)gv.ItemsPanelRoot;
            panel.Orientation = Orientation.Horizontal;

            double colums = e.NewSize.Width / 200;

            panel.MaximumRowsOrColumns = (int)colums;

            panel.ItemWidth = e.NewSize.Width / (int)colums;
            panel.ItemHeight = panel.ItemWidth / 2;
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
