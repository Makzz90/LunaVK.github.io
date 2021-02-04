using LunaVK.Core.Enums;
using LunaVK.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.Pages.Group
{
    public sealed partial class WikiListPage : PageBase
    {
        public WikiListPage()
        {
            this.InitializeComponent();
            this.Loaded += this.WikiListPage_Loaded;
        }

        private void WikiListPage_Loaded(object sender, RoutedEventArgs e)
        {
            base.Title = this.VM.Title;
            this.VM.Reload();
        }

        private WikiPagesViewModel VM
        {
            get { return base.DataContext as WikiPagesViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            Dictionary<string, object> QueryString = e.Parameter as Dictionary<string, object>;
            uint GroupId = (uint)QueryString["GroupId"];
            string Title = (string)QueryString["Title"];
            base.DataContext = new WikiPagesViewModel(GroupId, Title);

            this.VM.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;
        }

        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {
            switch (status)
            {
                case ProfileLoadingStatus.Reloading:
                    {
                        this._pivot.SelectionChanged -= this.Pivot_SelectionChanged;
                        break;
                    }
                case ProfileLoadingStatus.Loaded:
                {
                        this._pivot.SelectionChanged += this.Pivot_SelectionChanged;
                        var first = this.VM.Items.FirstOrDefault((i) => i.Title == this.VM.Title);
                        if(first!=null)
                        {
                            int index = this.VM.Items.IndexOf(first);
                            this._pivot.SelectedIndex = index;
                        }
                        
                        break;
                }
            }
        }


        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.VM.Items.Count == 0)
                return;//BugFix: если нажали перезагрузку

            Pivot pivot = sender as Pivot;
            int SubPage = pivot.SelectedIndex;
            if (!this.VM.Items[SubPage].IsLoaded)
            {
                this.VM.Items[SubPage].LoadData();
            }
        }

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            sender.Settings.IsJavaScriptEnabled = true;
            var vm = sender.DataContext as WikiPageSectionViewModel;
            if (vm.ViewUrl == args.Uri.OriginalString)
            {
                return;
            }
            args.Cancel = true;
            Library.NavigatorImpl.Instance.NavigateToWebUri(args.Uri.OriginalString);
        }
    }
}
