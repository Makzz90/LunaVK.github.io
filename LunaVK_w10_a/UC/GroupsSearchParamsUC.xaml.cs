using LunaVK.ViewModels;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LunaVK.UC
{
    public sealed partial class GroupsSearchParamsUC : UserControl
    {
        public Action Done;

        public GroupsSearchParamsUC()
        {
            this.InitializeComponent();
        }

        private SearchParamsViewModel.GroupsSearchParamsViewModel VM
        {
            get { return base.DataContext as SearchParamsViewModel.GroupsSearchParamsViewModel; }
        }

        private void Clear_OnTap(object sender, TappedRoutedEventArgs e)
        {
            this.VM.Clear();
        }

        private void Apply_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Done?.Invoke();
        }
    }
}
