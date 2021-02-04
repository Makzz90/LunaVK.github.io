using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using LunaVK.Framework;
using LunaVK.ViewModels;
using LunaVK.Core;
using LunaVK.Core.Enums;
using LunaVK.Core.DataObjects;
using LunaVK.Core.ViewModels;
using LunaVK.Library;
using LunaVK.UC;


#if WINDOWS_PHONE_APP
using Windows.Media.Playback;
#endif

namespace LunaVK
{
    /// <summary>
    /// Страница со списком треков
    /// </summary>
    public sealed partial class MusicPage : PageBase
    {
        //CollectionViewSource groupedItemsViewSource;
        //CollectionViewSource groupedItemsSearchViewSource;
        //private AudioPageViewModel.AudiosSearchViewModel searchViewModel = null;
        private bool _inSearch;

        public MusicPage()
        {
            this.InitializeComponent();
            //base.Title = LocalizedStrings.GetString("Menu_Audios/Content");
            this.Loaded += MusicPage_Loaded;

            CustomFrame.Instance.Header.SearchClosed = this.SearchClosed;
            CustomFrame.Instance.Header.ServerSearch = this.OnServerSearch;


#if WINDOWS_PHONE_APP
//            ForegroundPlaylistManager.Instance.CurrentStateChanged += this.CurrentStateChanged;
#endif
        }

        private void OnServerSearch(string text)
        {
            this.VM.SearchVM.q = text;
            this.VM.SearchVM.Items.Clear();
            this.mainScroll.NeedReload = true;
            this.mainScroll.Reload();
        }

        private void SearchClosed()
        {
            this._inSearch = false;

            this.mainScroll.DataContext = this.VM.MyMusicVM;
/*
            this.recomendScroll.NeedReload = true;

            if (this.VM.MyMusicVM.OwnerId == Settings.UserId)
            {
                this._root.Children.Remove(this._pivotItemMy);
                this._pivot.Items.Insert(0, this._pivotItemMy);
                this._pivot.Visibility = Visibility.Visible;//this._root.Children.Add(this._pivot);
            }
*/            
            Binding binding2 = new Binding() { Source = this.MusicSource };
            this.mainScroll.SetBinding(UC.Controls.ExtendedListView3.ItemsSourceProperty, binding2);

            if(this.VM.MyMusicVM._totalCount == null)
            {
                this.mainScroll.NeedReload = true;
                this.mainScroll.Reload();
            }
        }

        private void MusicPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.VM.MyMusicVM.OwnerId != Settings.UserId)
            {
                //Что это за херня?
                //this.mainScroll.Reload();//BugFix: т.к. мы удалил его из пивота, то надо запустить загрузку данных
            }
            else
                CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE721", Clicked = this.SearchClicked });

            //CustomFrame.Instance.Header.TitleOption = true;
        }


#if WINDOWS_PHONE_APP
        private void CurrentStateChanged(object sender, MediaPlayerState state)
        {
//            VKAudio a = this.VM.Items[ForegroundPlaylistManager.Instance.CurrentTrack];
//            a.UpdateUI();
        }
#endif

        public AudioPageViewModel VM
        {
            get { return base.DataContext as AudioPageViewModel; }
        }

        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //AudioPlayerViewModel.Instance.TrackChanged -= this.CurrentTrackChanged;
            //}
//            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = true;

            CustomFrame.Instance.Header.SearchClosed = null;
            CustomFrame.Instance.Header.ServerSearch = null;
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            //            AudioPlayerViewModel.Instance.TrackChanged += this.CurrentTrackChanged;

//            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = false;

            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                this.mainScroll.NeedReload = false;
                base.Title = LocalizedStrings.GetString("Menu_Audios/Content");//todo
            }
            else
            {
                //PagesParams parameter = navigationParameter as PagesParams;
                //base.DataContext = new AudioPageViewModel(parameter.user_id);

                IDictionary<string, object> QueryString = navigationParameter as IDictionary<string, object>;
                int owner = (int)QueryString["OwnerId"];
                string ownerName = (string)QueryString["OwnerName"];

                if (owner == Settings.UserId)
                    ownerName = "";


                base.DataContext = new AudioPageViewModel(owner);

                base.Title = LocalizedStrings.GetString("Menu_Audios/Content") + " " + ownerName;
                
                
            }
/*
            if (this.VM.MyMusicVM.OwnerId != Settings.UserId)
            {
                // Убираем раздел рекомендаций
                this._pivot.Items.Remove(this._pivotItemMy);
                this._pivot.Items.Remove(this._pivotItemRecomended);
                this._root.Children.Remove(this._pivot);
                this._root.Children.Add(this._pivotItemMy);
            }
*/
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this.VM;
        }
        
        private void Album_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKPlaylist;
            NavigatorImpl.Instance.NavigateToAlbum(vm);
        }

        private void AudioTrackUC_CoverClick(object sender, RoutedEventArgs e)
        {
            VKAudio vm = (sender as FrameworkElement).DataContext as VKAudio;

            if (AudioPlayerViewModel2.Instance.CurrentTrack == vm)
                AudioPlayerViewModel2.Instance.PlayPause();
            else
            {
                List<VKAudio> l = null;
                if (this._inSearch)
                {
                    l = this.VM.SearchVM.Items.Where((a) => a is VKAudio).Select((a) => (VKAudio)a).ToList();
                    
                }
                else
                {
                    l = this.VM.MyMusicVM.Items.Where((a) => a is VKAudio).Select((a)=>(VKAudio)a).ToList();
                }

//                AudioPlayerViewModel.Instance.CurentTrackId = l.IndexOf(vm);
//                AudioPlayerViewModel.Instance.FillTracks(l, this.VM.MyMusicVM.OwnerId>0?("user"+ this.VM.MyMusicVM.OwnerId):("club"+(-this.VM.MyMusicVM.OwnerId)));
//                AudioPlayerViewModel.Instance.PlayTrack(vm);
                AudioPlayerViewModel2.Instance.FillTracks(l, this.VM.MyMusicVM.OwnerId > 0 ? ("user" + this.VM.MyMusicVM.OwnerId) : ("club" + (-this.VM.MyMusicVM.OwnerId)));
                AudioPlayerViewModel2.Instance.PlayTrack(vm);
            }
        }
        /*
        private void AudioTrackUC_BackClick(object sender, RoutedEventArgs e)
        {
            VKAudio vm = (sender as FrameworkElement).DataContext as VKAudio;

            VKPlaylist playlist = new VKPlaylist("несохранённый плейлист", -100, this.VM.MyMusicVM.OwnerId, this.VM.MyMusicVM.Items.Where((a => a is VKAudio)).Select<object, VKAudio>((a => (VKAudio)a)).ToList());

            //           NavigatorImpl.Instance.NavigateToAudioPlayer(playlist, this.VM.Items.IndexOf(vm));
        }
        */

        

        private void SearchClicked(object sender)
        {
            this._inSearch = true;
            //CustomFrame.Instance.HeaderWithMenu.ActivateSearch(true);
            //       if (this.VM.SearchVM != null)//todo: проверять не в поиске ли мы?
            //       {
            CustomFrame.Instance.Header.ActivateSearch(true, true, this.VM.SearchVM.q);
     //       }
     //       else
     //       {
     //           CustomFrame.Instance.Header.ActivateSearch(true);
                //this.VM.SearchVM = new AudioPageViewModel.AudiosSearchViewModel();
    //        }


            this.mainScroll.DataContext = this.VM.SearchVM;//this.OldSource = this.mainScroll.ItemsSource;
/*
            this.recomendScroll.NeedReload = false;

            if (this.VM.MyMusicVM.OwnerId == Settings.UserId)
            {
                this._pivot.Items.Remove(this._pivotItemMy);
                this._pivot.Visibility = Visibility.Collapsed;//this._root.Children.Remove(this._pivot);
                this._root.Children.Add(this._pivotItemMy);
            }
*/

            Binding binding2 = new Binding() { Source = this.groupedItemsViewSource };
            this.mainScroll.SetBinding(UC.Controls.ExtendedListView3.ItemsSourceProperty, binding2);

            base.InitializeProgressIndicator(this.VM.SearchVM);
        }

        private void Album_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                return;

            FrameworkElement element = sender as FrameworkElement;
//            this.ShowMenu(element);
        }

        private void Album_Holding(object sender, HoldingRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                FrameworkElement element = sender as FrameworkElement;
//                this.ShowMenu(element);
            }
        }

        private void ShowMenu(FrameworkElement element)
        {
            var vm = element.DataContext as VKPlaylist;

            //if (vm.id != AllAudioViewModel.SAVED_ALBUM_ID)
            //    return;

            PopUP2 menu = new PopUP2();
            PopUP2.PopUpItem item = new PopUP2.PopUpItem() { Text = "Удалить", Command = new DelegateCommand((args) => {
                this.VM.RemoveAlbum(vm);
            } )};


            
        }

        private void MainScroll_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }
    }


}
