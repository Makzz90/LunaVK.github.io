using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.ViewModels;
using LunaVK.Framework;
using LunaVK.UC;
using LunaVK.ViewModels;
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

namespace LunaVK.Pages
{
    public sealed partial class GamesMainPage : PageBase
    {
        public GamesMainPage()
        {
            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("Menu_Games/Content");
        }

        private GamesMainViewModel VM
        {
            get { return base.DataContext as GamesMainViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            /*
            base.HandleOnNavigatedTo(e);
            if (this._isInitialized)
                return;
            IDictionary<string, string> queryString = ((Page)this).NavigationContext.QueryString;
            if (queryString.ContainsKey("FromPush"))
            {
                bool result;
                bool.TryParse(queryString["FromPush"], out result);
                AppGlobalStateManager.Current.GlobalState.GamesVisitSource = result ? GamesVisitSource.push : GamesVisitSource.direct;
            }
            else
                AppGlobalStateManager.Current.GlobalState.GamesVisitSource = GamesVisitSource.direct;
            long gameId = 0;
            if (queryString.ContainsKey("GameId"))
                long.TryParse(queryString["GameId"], out gameId);
                */
            GamesMainViewModel vm = new GamesMainViewModel();
            /*
            vm.GamesSectionsVM.LoadData(false, false, (res =>
            {
                if (gameId <= 0L)
                    return;
                vm.OpenGame(gameId);
            }), false);*/
            base.DataContext = vm;
            //this._isInitialized = true;
        }

        private void Game_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKGame;
            
            //this.VM.OpenGame(dataContext.Game.id);
            // FramePageUtils.CurrentPage.OpenGamesPopup(new List<object>((IEnumerable<object>)this._games), GamesClickSource.catalog, "", selectedIndex, null);
            PopUpService pop = new PopUpService();
            GameViewUC child = new GameViewUC();
            GameViewModel viewModel = new GameViewModel(vm);
            child.DataContext = viewModel;
            pop.Child = child;
            pop.OverrideBackKey = true;
            pop.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            pop.Show();
        }

        private void ListBoxGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }
    }
}
