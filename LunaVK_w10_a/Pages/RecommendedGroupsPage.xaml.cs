using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace LunaVK.Pages
{
    public sealed partial class RecommendedGroupsPage : PageBase
    {
        public RecommendedGroupsPage()
        {
            this.InitializeComponent();
            this.Loaded += RecommendedGroupsPage_Loaded;
            //this.detailed.ManipulationMode |= ManipulationModes.TranslateX;
            this._detailsView.IsSelectedChanged += _detailsView_IsSelectedChanged;
        }

        private void _detailsView_IsSelectedChanged(object sender, bool e)
        {
            if (e == false)
            {
                //this._detailsView.SelectedItem = false;
                this._exListView.GetListView.SelectedItem = null;
            }
        }

        protected override void HandleOnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (this._detailsView.SelectedItem == true)
            {
                e.Cancel = true;
                this._detailsView.SelectedItem = false;
            }
            
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
            }
            else
            {
                base.DataContext = new RecommendedGroupsViewModel();
            }
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this.VM;

            CustomFrame.Instance.Header.IsVisible = true;
            CustomFrame.Instance.Header.HideSandwitchButton = false;
        }

        private void RecommendedGroupsPage_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.HideSandwitchButton = true;

            if (CustomFrame.Instance.Header.IsVisible == true)
                CustomFrame.Instance.Header.IsVisible = false;

 //           if(this.VM.Items.Count == 0)
 //               this.VM.LoadData();
        }

        public RecommendedGroupsViewModel VM
        {
            get { return base.DataContext as RecommendedGroupsViewModel; }
        }

        private void Burger_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CustomFrame.Instance.OpenCloseMenu();
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as GroupsService.GroupCatalogCategoryPreview;
            this._subTitle.Text = vm.name;
            this._detailsView.SelectedItem = true;
            this.detailed.Items.Clear();
            this.FillItems(vm);
        }

        private void FillItems(GroupsService.GroupCatalogCategoryPreview category)
        {
            if (category.LoadedGroups.Count == 0)
            {
                GroupsService.Instance.GetCatalog(category.id, (result) => {
                    if(result.error.error_code == VKErrors.None)
                    {
                        category.LoadedGroups = result.response.items;

                        Execute.ExecuteOnUIThread(() =>
                        { 
                            foreach (var item in category.LoadedGroups)
                                this.detailed.Items.Add(item);
                        });
                    }

                });
            }
            else
            {
                foreach (var item in category.LoadedGroups)
                    this.detailed.Items.Add(item);
            }
        }

        private void BaseProfileItem_ThirdClick(object sender, RoutedEventArgs e)
        {

        }

        private void Back_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this._detailsView.SelectedItem = false;
            this._exListView.GetListView.SelectedItem = null;
        }

        private void BaseProfileItem_BackTap(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as GroupsService.GroupCatalogCategoryPreview.SearchG;
            NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }
    }
}
