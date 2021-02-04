using LunaVK.Core.Library;
using LunaVK.Core.ViewModels;
using LunaVK.ViewModels;
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
    public sealed partial class EditPrivacyUC : UserControl
    {
        PrivacySetting ps;

        private EditPrivacyUC()
        {
            this.InitializeComponent();
        }

        public EditPrivacyUC(PrivacySettingItem s) :this()
        {
            base.DataContext = new EditPrivacyViewModel(s);
            this.ps = s;

            var binding = new Binding() { Mode = BindingMode.OneWay, Path = new PropertyPath("IsInProgress"), Source = base.DataContext };
            BindingOperations.SetBinding(this._progress, ProgressBar.IsIndeterminateProperty, binding);
        }

        private EditPrivacyViewModel VM
        {
            get { return base.DataContext as EditPrivacyViewModel; }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckRadioButtons(e.RemovedItems, false);
            CheckRadioButtons(e.AddedItems, true);
        }

        private void CheckRadioButtons(IList<object> radioButtons, bool isChecked)
        {
            foreach (object item in radioButtons)
            {
                ListViewItem lbi = this._lv.ContainerFromItem(item) as ListViewItem;
                
                if (lbi != null)
                {
                    var presenter = VisualTreeHelper.GetChild(lbi, 0) as ListViewItemPresenter;
                    var radio = VisualTreeHelper.GetChild(presenter, 0) as RadioButton;

                    if (radio != null)
                        radio.IsChecked = isChecked;
                }
            }
        }

        private void _lv_Loaded(object sender, RoutedEventArgs e)
        {
            var temp = SettingsPrivacyViewModel.supported_categories.Find((s) => s.value == this.ps.value.category);
            ListView lv = sender as ListView;
            object stemp = lv.SelectedItem;
            lv.SelectedItem = null;
            lv.SelectedItem = stemp;//BugFix: чтобы ListView_SelectionChanged завёлся

            this.VM.IsOKState = true;
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem sel = (sender as RadioButton).Parent as ListBoxItem;
            int newIndex = this._lv.IndexFromContainer(sel); ;
            this._lv.SelectedIndex = newIndex;
            //UpdatePrivacyCallback

            //this.VM.Save();
        }

        private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as EditPrivacyViewModel.FriendHeader;
            this.VM.AllowedDeniedCollection.Remove(vm);

            if(vm.data!=null)
            {
                if (this.VM.Setting.value.owners.allowed != null)
                    this.VM.Setting.value.owners.allowed.Remove(vm.data.Id);
                if (this.VM.Setting.value.owners.excluded != null)
                    this.VM.Setting.value.owners.excluded.Remove(vm.data.Id);
            }
            if(vm._friendsList!=null)
            {
                if (this.VM.Setting.value.lists.allowed != null)
                    this.VM.Setting.value.lists.allowed.Remove(vm._friendsList.id);
                if (this.VM.Setting.value.lists.excluded != null)
                    this.VM.Setting.value.lists.excluded.Remove(vm._friendsList.id);
            }

            if (this.VM.AllowedDeniedCollection.Count == 0)
                this.VM.Category = this.VM.SupportedCategories.First();
            this.VM.Save();
        }





        private ObservableCollection<EditPrivacyViewModel.FriendHeader> suggestions = new ObservableCollection<EditPrivacyViewModel.FriendHeader>();

        private void Search_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this._searchBox.Visibility = Visibility.Visible;

            this._tbCustom.Visibility = Visibility.Collapsed;
            this._tbPick.Visibility = Visibility.Collapsed;
        }

        private void _searchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Set sender.Text. You can use args.SelectedItem to build your text string.
            //var selectedItem = args.SelectedItem.ToString();
            
            var vm = args.SelectedItem as EditPrivacyViewModel.FriendHeader;
            vm.DeleteVisibility = Visibility.Visible;
            if (this.VM.Category.value == "some")
            {
                if (vm.data != null)
                {
                    if (this.VM.Setting.value.owners == null)
                    {
                        this.VM.Setting.value.owners = new PrivacySetting.PrivacyTypeClass2.PrivacyTypeClassOwners();
                        this.VM.Setting.value.owners.allowed = new List<int>();
                    }
                    this.VM.Setting.value.owners.allowed.Add(vm.data.Id);

                    this.VM.Setting.Profiles = new List<Core.DataObjects.VKUser>() { new Core.DataObjects.VKUser() { id = (uint)vm.data.Id, first_name = vm.data.Title } };
                }
                if (vm._friendsList != null)
                {
                    if (this.VM.Setting.value.lists == null)
                    {
                        this.VM.Setting.value.lists = new PrivacySetting.PrivacyTypeClass2.PrivacyTypeClassOwners();
                        this.VM.Setting.value.lists.allowed = new List<int>();
                    }
                    this.VM.Setting.value.lists.allowed.Add(vm._friendsList.id);
                }
            }
            else
            {
                if (vm.data != null)
                {
                    if (this.VM.Setting.value.owners == null)
                    {
                        this.VM.Setting.value.owners = new PrivacySetting.PrivacyTypeClass2.PrivacyTypeClassOwners();
                        this.VM.Setting.value.owners.excluded = new List<int>();
                    }
                    this.VM.Setting.value.owners.excluded.Add(vm.data.Id);

                    this.VM.Setting.Profiles = new List<Core.DataObjects.VKUser>() { new Core.DataObjects.VKUser() { id = (uint)vm.data.Id, first_name = vm.data.Title } };
                }
                if (vm._friendsList != null)
                {
                    if (this.VM.Setting.value.lists == null)
                    {
                        this.VM.Setting.value.lists = new PrivacySetting.PrivacyTypeClass2.PrivacyTypeClassOwners();
                        this.VM.Setting.value.lists.excluded = new List<int>();
                    }
                    this.VM.Setting.value.lists.excluded.Add(vm._friendsList.id);
                }
            }

            sender.ItemsSource = null;
            this.VM.AllowedDeniedCollection.Add(vm);
            this.VM.Save();
        }

        private void _searchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Only get results when it was a user typing,
            // otherwise assume the value got filled in by TextMemberPath
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                //Set the ItemsSource to be your filtered dataset

                this.BuildSource(sender);
            }
        }

        private void _searchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
                sender.Text = "";
            }
            else
            {
                // Use args.QueryText to determine what to do.
                //sender.Text = sender.Text;
            }
        }

        private void BuildSource(AutoSuggestBox sender)
        {
            suggestions.Clear();

            string text = sender.Text;
            /*
            if (text.Contains("."))
            {
                var items = _methods.Where((m) => m.StartsWith(text));
                suggestions = new ObservableCollection<string>(items);
            }
            else
            {
                var itemsParts = _methods.Select((m) => m.Contains(".") ? m.Substring(0, m.IndexOf('.')) : m);
                var items = itemsParts.Where((m) => m.StartsWith(text)).Distinct();
                suggestions = new ObservableCollection<string>(items);

            }*/
            var temp = this.VM.LookFor(text);
            if(temp.friends != null)
            {
                foreach(var h in temp.friends)
                {
                    suggestions.Add(new EditPrivacyViewModel.FriendHeader() { data = h, DeleteVisibility = Visibility.Collapsed });
                }
                
            }
            if (temp.friendLists != null)
            {
                foreach (var h in temp.friendLists)
                {
                    suggestions.Add(new EditPrivacyViewModel.FriendHeader() { _friendsList = h, DeleteVisibility = Visibility.Collapsed });
                }

            }
            sender.ItemsSource = suggestions;
        }

        private void _searchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this._searchBox.Visibility = Visibility.Collapsed;

            this._tbCustom.Visibility = Visibility.Visible;
            this._tbPick.Visibility = Visibility.Visible;
        }
    }
}
