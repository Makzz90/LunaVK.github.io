using LunaVK.Framework;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.UC
{
    public sealed partial class GenericSearchUC : UserControl
    {
        private UIElement elementForHide;
        public GenericSearchViewModelBase<object> ViewModel;
        public Action<object> SelectedItemCallback;
        public Action Close;
        private DispatcherTimer searchTimer;
        public Action MoreSearchClicked;

        public GenericSearchUC(GenericSearchViewModelBase<object> vm, DataTemplate itemTemplate, UIElement backElement, bool moreOptions = false)
        {
            this.elementForHide = backElement;
            this.ViewModel = vm;
            this.InitializeComponent();

            this.searchResultsListBox.NeedReload = false;
            this.searchResultsListBox.DataContext = vm;
            this.searchResultsListBox.ItemTemplate = itemTemplate;

            this.Loaded += GenericSearchUC_Loaded;
            this.Unloaded += GenericSearchUC_Unloaded;

            this.searchTimer = new DispatcherTimer();
            this.searchTimer.Interval = TimeSpan.FromMilliseconds(500);
            this.searchTimer.Tick += Timer_Tick;

            this._moreSearchBrd.Visibility = moreOptions ? Visibility.Visible : Visibility.Collapsed;

            InputPane.GetForCurrentView().Showing += Keyboard_Showing;
        }

        private void Keyboard_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            args.EnsuredFocusedElementInView = true;
        }

        private void GenericSearchUC_Unloaded(object sender, RoutedEventArgs e)
        {
            //this.elementForHide.IsHitTestVisible = true;
//            this.elementForHide.Opacity = 1;

            this.elementForHide.Visibility = Visibility.Visible;
            CustomFrame.Instance.Header.HideMoreOptions = false;

            InputPane.GetForCurrentView().Showing -= Keyboard_Showing;
        }

        private void GenericSearchUC_Loaded(object sender, RoutedEventArgs e)
        {
            //this.elementForHide.IsHitTestVisible = false;
//            this.elementForHide.Opacity = 0.5;

            this.searchTextBox.Focus(FocusState.Keyboard);
            CustomFrame.Instance.Header.HideMoreOptions = true;
        }

        private void SearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.searchTimer.IsEnabled)
                this.searchTimer.Stop();

            string text = (sender as TextBox).Text;
            //this.ViewModel.SearchString = text;
            if (string.IsNullOrEmpty(text))
            {
                this.ViewModel.OnRefresh();
                this.elementForHide.Visibility = Visibility.Visible;
                this.searchResultsListBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.searchTimer.Start();
                this.elementForHide.Visibility = Visibility.Collapsed;
                this.searchResultsListBox.Visibility = Visibility.Visible;
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            (sender as DispatcherTimer).Stop();
            this.ViewModel.SearchString = this.searchTextBox.Text;
        }

        private void SearchTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            string text = (sender as TextBox).Text;

            if (e.Key == Windows.System.VirtualKey.Enter /*&& !string.IsNullOrEmpty(textBox.Text)*/)
            {
                
                if (this.searchTimer.IsEnabled)
                    this.searchTimer.Stop();
                this.ViewModel.SearchString = text;
            }
        }

        private void SearchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;

            if(e.AddedItems.Count>0)
                this.SelectedItemCallback?.Invoke(e.AddedItems[0]);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close?.Invoke();
        }

        private void MoreSearch_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this.MoreSearchClicked?.Invoke();
        }
    }
}
