using LunaVK.Core.DataObjects;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.UC;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

//UsersSearchParamsPage

namespace LunaVK.Pages
{
    public sealed partial class UsersSearchResultsPage : PageBase
    {
        public UsersSearchResultsPage()
        {
            this.InitializeComponent();
            base.DataContext = new UsersSearchParamsViewModel();
            this.Loaded += UsersSearchResultsPage_Loaded;
            //this.searchTimer.Tick += SearchTimer_Tick;
            this.listBoxUsers.Loaded2 += InsideScrollViewerLoaded;
            //this.listBoxUsers.NeedReload = false;
        }

        private void UsersSearchResultsPage_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.ActivateSearch(true, false, this.VM.SearchName, true);
            CustomFrame.Instance.Header.ActivateMoreOptionsInSearch(true);
            CustomFrame.Instance.Header.ServerSearch = this.HandleSearch;
            CustomFrame.Instance.Header.MoreSearchClicked += this.MoreOptionsClicked;
        }

        private void MoreOptionsClicked()
        {
            PopUpService statusChangePopup = new PopUpService();

            FrameworkElement element = null;

 //           if(this._pivot.SelectedIndex==1)
//            {
                UsersSearchParamsUC sharePostUC = new UsersSearchParamsUC();
                sharePostUC.DataContext = base.DataContext;
                sharePostUC.Done = () => {
                    statusChangePopup.Hide();
                    this.VM.Items.Clear();

                    this.listBoxUsers.NeedReload = true;
                    this.listBoxUsers.Reload();
                };
                element = sharePostUC;
/*            }
            else if (this._pivot.SelectedIndex == 2)
            {
                GroupsSearchParamsUC sharePostUC = new GroupsSearchParamsUC();
                sharePostUC.DataContext = base.DataContext;
                sharePostUC.Done = () => {
                    statusChangePopup.Hide();
                    this.VM.Items.Clear();

                    this.listBoxUsers.NeedReload = true;
                    this.listBoxUsers.Reload();
                };
                element = sharePostUC;
            }
            */
            statusChangePopup.Child = element;
            
            statusChangePopup.OverrideBackKey = true;
            statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            statusChangePopup.Show();
        }

        private void HandleSearch(string q)
        {
            this.VM.SearchName = q;

            this.VM.Items.Clear();

            this.listBoxUsers.NeedReload = true;
            this.listBoxUsers.Reload();
        }

        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //CustomFrame.Instance.HeaderWithMenu.IsVisible = true;
            CustomFrame.Instance.Header.MoreSearchClicked -= this.MoreOptionsClicked;
        }

        private UsersSearchParamsViewModel VM
        {
            get { return base.DataContext as UsersSearchParamsViewModel; }
        }
        
        private void Clear_OnTap(object sender, TappedRoutedEventArgs e)
        {
            this.VM.Clear();
        }

        private void BaseProfileItem_BackTap(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKUser;
            NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }

        private double VerticalOffset = 0;

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this.VM;
            if (this.listBoxUsers.GetInsideScrollViewer != null)
            {
                pageState["ScrollOffset"] = this.listBoxUsers.GetInsideScrollViewer.VerticalOffset;
            }
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                this.VerticalOffset = (double)pageState["ScrollOffset"];
                this.listBoxUsers.NeedReload = false;
            }
            else
            {
                //Dictionary<string, int> QueryString = navigationParameter as Dictionary<string, int>;
                //int Id = QueryString["Id"];
                string q = (string)navigationParameter;
                this.DataContext = new UsersSearchParamsViewModel() { SearchName = q };
                
            }
        }

        private void InsideScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            if (this.VerticalOffset != 0)
                this.listBoxUsers.GetInsideScrollViewer.ChangeView(0, this.VerticalOffset, 1.0f);
        }
    }
}
