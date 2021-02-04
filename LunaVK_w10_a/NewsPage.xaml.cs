using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using LunaVK.Core.ViewModels;
using LunaVK.Core;
using LunaVK.Core.Enums;
using LunaVK.Core.DataObjects;

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using LunaVK.UC;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.Core.Network;
using Windows.UI.Xaml.Controls;
using LunaVK.ViewModels;
using LunaVK.Core.Framework;
using LunaVK.Common;
using System.Collections.Generic;
using System;
using System.Linq;

namespace LunaVK
{
    //NewsfeedNewPostUC
    public sealed partial class NewsPage : PageBase
    {
        //private double _scrollPosition;
        //private int _lastSource;
        private PopUpService _flyoutStory;
        private Framework.HideHeaderHelper _hideHelper;
        private NewsSearchViewModel searchViewModel = null;
        private NewsViewModel _newsViewModel;

        public NewsPage()
        {
            this.InitializeComponent();
            this.MainScroll.Loaded += this.MainScroll_Loaded;
            this.MainScroll.Loaded2 += this.InsideScrollViewerLoaded;
            base.Loaded += NewsPage_Loaded;
            this.NewsfeedHeader.OnFreshNewsTap = this.OnFreshNewsTap;
        }

        private void NewsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.searchViewModel != null)
                this._appBarButtonSearch_Click(null);
        }

        private void MainScroll_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.PullToRefresh.TrackListBox(this.MainScroll);
            
            CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE721", Clicked = this._appBarButtonSearch_Click });
            CustomFrame.Instance.Header.HeaderGrid.Tapped += this.OnHeaderTap;

            CustomFrame.Instance.Header.TitleOption = true;

            CustomFrame.Instance.Header.TitlePanel.Tapped += this.OpenNewsSourcePicker;




            CustomFrame.Instance.Header.SearchClosed = this.SearchClosed;
            CustomFrame.Instance.Header.ServerSearch = this.OnServerSearch;
        }

        private void OnServerSearch(string text)
        {
            this.searchViewModel.q = text;
            this.searchViewModel.Items.Clear();
            this.MainScroll.NeedReload = true;
            this.MainScroll.Reload();
        }




        private void _appBarButtonSearch_Click(object sender)
        {
            if (this._hideHelper != null)
            {
                this._hideHelper.Activate(false);
                this._hideHelper.UpdateFreshNewsState( FreshNewsState.NoNews);
            }

            if (this.searchViewModel != null)
            {
                CustomFrame.Instance.Header.ActivateSearch(true, true, this.searchViewModel.q);
            }
            else
            {
                CustomFrame.Instance.Header.ActivateSearch(true);
                this.searchViewModel = new NewsSearchViewModel();
            }
            
            base.DataContext = this.searchViewModel;

            this.SecondContent.IsHitTestVisible = false;
            this.SecondContent.Opacity = 0;

            //
            base.InitializeProgressIndicator();
        }

        private void SearchClosed()
        {
            base.DataContext = this._newsViewModel;
            this.searchViewModel = null;

            if (this._hideHelper != null)
                this._hideHelper.Activate(true);

            if (this._newsViewModel.Items.Count == 0)
            {
                this.MainScroll.NeedReload = true;
                this.MainScroll.Reload();
            }

            this.SecondContent.IsHitTestVisible = true;
            this.SecondContent.Opacity = 1;
        }
        
        private void InsideScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            if (this._newsViewModel._scrollPosition > 0)
            {
                this.MainScroll.GetInsideScrollViewer.ChangeView(0, this._newsViewModel._scrollPosition, 1.0f);
            }

            //if (Settings.HideHeader == false)
            //    this._hideHelper = null;
            //else
                this._hideHelper = new Framework.HideHeaderHelper(CustomFrame.Instance.Header, this.MainScroll.GetInsideScrollViewer, this.NewsfeedHeader);
            //
            //
            this._hideHelper.Activate(true);
            if (base.DataContext is NewsViewModel)
            {
                if (this._newsViewModel.Items.Count > 0)
                {
                    this._newsViewModel.CheckForFreshNewsIfNeeded();
                }
            }
        }

        private void OpenNewsSourcePicker(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            PopUP2 menu = new PopUP2();
            PopUP2.PopUpItem item0 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("NewsFeedNews/Content"), Icon = new IconUC() { Glyph = "\xE8A1" }, CommandParameter="0" };
            item0.Command = new DelegateCommand((args) => { this._picker_ItemTapped(args); });
            menu.Items.Add(item0);

            PopUP2.PopUpItem item1 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("NewsFeedSuggestions/Content"), Icon = new IconUC() { Glyph = "\xE8EB" }, CommandParameter = "1" };
            item1.Command = new DelegateCommand((args) => { this._picker_ItemTapped(args); });
            menu.Items.Add(item1);

            PopUP2.PopUpItem item2 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("NewsFeedPhotos/Content"), Icon = new IconUC() { Glyph = "\xEB9F" }, CommandParameter = "2" };
            item2.Command = new DelegateCommand((args) => { this._picker_ItemTapped(args); });
            menu.Items.Add(item2);

            PopUP2.PopUpItem item3 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("NewsFeedVideos/Content"), Icon = new IconUC() { Glyph = "\xE714" }, CommandParameter = "3" };
            item3.Command = new DelegateCommand((args) => { this._picker_ItemTapped(args); });
            menu.Items.Add(item3);

            PopUP2.PopUpItem item4 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("NewsFeedFriends/Content"), Icon = new IconUC() { Glyph = "\xE77B" }, CommandParameter = "4" };
            item4.Command = new DelegateCommand((args) => { this._picker_ItemTapped(args); });
            menu.Items.Add(item4);

            menu.ShowAt(sender as FrameworkElement);
        }

        private void _picker_ItemTapped(object args)
        {
            this._lv.SelectedIndex = int.Parse(args as string);
        }

        private void SetSource(int i, string title, bool need_reload = true)
        {
            //if (this.searchViewModel != null)
            //    return;

            this._newsViewModel.SetNewsSource(i, need_reload);
            base.Title = title;
            //
            //
            if(this._hideHelper!=null)
                this._hideHelper.UpdateFreshNewsState( FreshNewsState.NoNews);
        }

        private void OnHeaderTap(object sender, TappedRoutedEventArgs e)
        {
            this.MainScroll.GetInsideScrollViewer.ChangeView(0, 0, 1f, true);
            if (e != null)
                e.Handled = true;
        }

        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (this._newsViewModel.Items.Count > 200)
                this._newsViewModel.Items.Clear();
            else
            {
                if (this.MainScroll.GetInsideScrollViewer == null)//очень быстрое перемещение в отладке :)
                    return;

                this._newsViewModel._scrollPosition = this.MainScroll.GetInsideScrollViewer.VerticalOffset;
            }

            CustomFrame.Instance.Header.HeaderGrid.Tapped -= this.OnHeaderTap;
            CustomFrame.Instance.Header.TitlePanel.Tapped -= this.OpenNewsSourcePicker;

            if(this._hideHelper!=null)
                this._hideHelper.Reset();
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            if (this.searchViewModel != null)
            {
                pageState["SearchData"] = this.searchViewModel;
                pageState["SearchScrollPosition"] = this.MainScroll.GetInsideScrollViewer.VerticalOffset;
            }

            if (this._newsViewModel.Items.Count > 0)
            {
                CacheManager.TrySerializeAsync(this._newsViewModel, "News");
            }
        }
        
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            this._newsViewModel = new NewsViewModel();
            bool from_cache = CacheManager.TryDeserialize(this._newsViewModel, "News");

            



            if (pageState != null && pageState.ContainsKey("SearchData"))
            {
                this.searchViewModel = (NewsSearchViewModel)pageState["SearchData"];
                base.DataContext = this.searchViewModel;
                this.MainScroll.NeedReload = false;
            }
            else
            {
                if (navigationParameter == null)
                {
                    base.DataContext = this._newsViewModel;
                    
                    if (this._newsViewModel.Items.Count > 0)
                    {
                        this.MainScroll.NeedReload = false;
                        //this._newsViewModel.CheckForFreshNewsIfNeeded();
                    }
                    
                }
                else
                {
                    string q = navigationParameter as string;

                    this.searchViewModel = new NewsSearchViewModel();
                    this.searchViewModel.q = q;
                    base.DataContext = this.searchViewModel;
                    this.MainScroll.NeedReload = true;
                }
            }

            if(from_cache)
            {
                foreach(var item in this._newsViewModel.Items)
                {
                    item.IgnoreNewsfeedItemCallback = () => { this._newsViewModel.IgnoreNewsFeedItem(item); };
                    item.HideSourceItemsCallback = () => { this._newsViewModel.HideSourceItemsCallback(item); };
                }
                this._newsViewModel.CurrentLoadingStatus = ProfileLoadingStatus.Loaded;
            }

            this._newsViewModel.FreshNewsStateChangedCallback = this.FreshNewsStateChangedCallback;
        }

        private void Story_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var story = (sender as FrameworkElement).DataContext as NewsViewModel.NewStory;

            StoryViewer viwer = new StoryViewer(story);

            this._flyoutStory = new PopUpService();
            this._flyoutStory.OverrideBackKey = true;
            this._flyoutStory.AnimationTypeChild = PopUpService.AnimationTypes.Slide;
            this._flyoutStory.Child = viwer;

            this._flyoutStory.Show();

            viwer.Done = () => { this._flyoutStory.Hide(); };
        }

        private void NewPost_OnTap(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            NavigatorImpl.Instance.NavigateToNewWallPost();
        }

        private void Photo_OnTap(object sender, TappedRoutedEventArgs e)
        {
            //ParametersRepository.SetParameterForId("GoPickImage", true);
            //Navigator.Current.NavigateToNewWallPost(0, false, 0, false, false, false);
            e.Handled = true;
        }

        private void NewStory_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            NavigatorImpl.Instance.NavigateToStoryCreate();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            ListViewItem item = lv.SelectedItem as ListViewItem;
            this.SetSource(lv.SelectedIndex, (string)item.Content);
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            ListView lv = sender as ListView;
            //
            if(this._newsViewModel.NewsSource!=0)
            {
                ListViewItem item = lv.Items[this._newsViewModel.NewsSource] as ListViewItem;
                this.SetSource(this._newsViewModel.NewsSource, (string)item.Content,false);
                lv.SelectedIndex = this._newsViewModel.NewsSource;
            }
            else
            {
                base.Title = LocalizedStrings.GetString("Menu_News");
            }
            //
            lv.SelectionChanged += this.ListView_SelectionChanged;
        }

        private void FreshNewsStateChangedCallback(FreshNewsState state)
        {
            Execute.ExecuteOnUIThread((() =>
            {
                if (this.searchViewModel != null)
                    return;

                if (state == FreshNewsState.Reload)
                {
                    this._newsViewModel.ReplaceAllWithPendingFreshNews();
                    this.OnHeaderTap(null,null);

                    state = this._newsViewModel.FreshNewsState;
                }
                this.NewsfeedHeader.IsLoadingFreshNews = false;
                this._hideHelper.UpdateFreshNewsState(state);
                //if (state == FreshNewsState.NoNews)
                //    return;
                //this._hideHelper.ShowFreshNews();
            }));
        }
        /*
        private void BorderFreshNews_OnTap(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            this.OnFreshNewsTap();
        }
        */
        private void OnFreshNewsTap()
        {
            //if (this._hideHelper == null)
            //    return;
            NewsViewModel instance = this._newsViewModel;
            switch (instance.FreshNewsState)
            {
                case FreshNewsState.Insert:
                    this.OnHeaderTap(null,null);
                    instance.InsertFreshNews();


                    break;
                case FreshNewsState.Reload:
                    //if (instance.AreFreshNewsUpToDate && instance.HasFreshNewsToInsert)
                    //{
                    instance.ReplaceAllWithPendingFreshNews();
                        this.OnHeaderTap(null, null);
                        //break;
                    //}

                    this.NewsfeedHeader.IsLoadingFreshNews = true;
                    //instance.ReloadNews(false, false, true);
                    break;
            }
        }

        /*
         * public static void HideAd(string adData, string adObjectType, Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["ad_data"] = adData;
      parameters["object_type"] = adObjectType;
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Audio.Base.ResponseWithId>("adsint.hideAd", parameters, callback, (Func<string, VKClient.Audio.Base.ResponseWithId>) (jsonStr => new VKClient.Audio.Base.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public static void ReportAd(string adData, ReportAdReason reportAdReason, Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["ad_data"] = adData;
      parameters["reason"] = reportAdReason.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Audio.Base.ResponseWithId>("adsint.reportAd", parameters, callback, (Func<string, VKClient.Audio.Base.ResponseWithId>) (jsonStr => new VKClient.Audio.Base.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }
    */
    }
}
