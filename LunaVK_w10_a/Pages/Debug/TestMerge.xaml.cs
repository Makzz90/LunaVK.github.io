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
using System.Collections.ObjectModel;
using LunaVK.Framework;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestMerge : Page
    {
        public ObservableCollection<string> Dialogs { get; private set; }
        public TestMerge()
        {
            this.Dialogs = new ObservableCollection<string>();

            this.InitializeComponent();
            this.DetailsView.DataContext = this;

            for(int i =0;i<20;i++)
            {
                Dialogs.Add("Item #"+i);
            }

            this.Loaded += TestMerge_Loaded;
        }

        private void TestMerge_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.IsVisible = false;
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var selected = DetailsView.SelectedItem;

            var item1 = Dialogs[0];
            var item2 = Dialogs[1];

            Dialogs[0] = item2;
            Dialogs[1] = item1;
            //this.Dialogs.Remove(item1);
            //this.Dialogs.Remove(item2);
            //this.Dialogs.Insert(0,item2);
            //this.Dialogs.Insert(1, item1);

            
            DetailsView.SelectedItem = selected;
        }
    }
}
