using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Library;
using LunaVK.UC;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.Pages
{
    public sealed partial class SuggestedPostponedPostsPage : PageBase
    {
        public SuggestedPostponedPostsPage()
        {
            this.InitializeComponent();
        }

        private SuggestedPostponedPostsViewModel VM
        {
            get { return base.DataContext as SuggestedPostponedPostsViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            Dictionary<string, int> QueryString = e.Parameter as Dictionary<string, int>;
            int UserOrGroupId = QueryString["UserOrGroupId"];
            int Mode = QueryString["Mode"];
            
            base.DataContext = new SuggestedPostponedPostsViewModel(UserOrGroupId, Mode);
            base.Title = this.VM.Title;
        }

        private void buttonDelete_Tap(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKWallPost;
            (sender as Button).IsEnabled = false;
            this.VM.DeletedCallback(vm);
        }

        private void buttonPublish_Tap(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKWallPost;
            this.VM.PublishCallback(vm);
        }

        private void ItemWallPostUC_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var vm = (sender as FrameworkElement).DataContext as VKWallPost;
            NavigatorImpl.Instance.NavigateToNewWallPost(WallPostViewModel.Mode.NewWallPost, vm.owner_id, Core.Enums.VKAdminLevel.None, false, vm);
        }
    }
}
