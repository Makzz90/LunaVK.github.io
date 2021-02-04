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

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace LunaVK.UC
{
    public sealed partial class CreateEditPollUC : UserControl
    {
        public ObservableCollection<PollOption> PollOptions { get; set; }
        public Action CancelClick;

        public CreateEditPollUC()
        {
            base.DataContext = this;

            this.InitializeComponent();

            this.PollOptions = new ObservableCollection<PollOption>();
            this.PollOptions.Add(new PollOption());
            this.PollOptions.CollectionChanged += PollOptions_CollectionChanged;
        }

        private void PollOptions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this._btnMore.IsEnabled = this.PollOptions.Count <= 10;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.CancelClick?.Invoke();
            int i = 0;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
        }

        public class PollOption
        {
            public string Text { get; set; }
        }

        private void AddAnswer_Click(object sender, RoutedEventArgs e)
        {
            this.PollOptions.Add(new PollOption());
        }

        private void Delete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.PollOptions.Count == 1)
                return;

            PollOption vm = (sender as FrameworkElement).DataContext as PollOption;
            this.PollOptions.Remove(vm);
        }
    }
}
