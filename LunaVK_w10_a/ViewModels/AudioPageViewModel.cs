using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LunaVK.ViewModels
{
    /// <summary>
    /// Это модель для страницы с музыкой пользователя/группы
    /// </summary>
    public class AudioPageViewModel
    {
        public GenericCollectionMy MyMusicVM { get; private set; }
//        public GenericCollectionCatalog RecommendationsMusicVM { get; private set; }
        public AudiosSearchViewModel SearchVM { get; private set; }

        public AudioPageViewModel(int owner)
        {
            this.MyMusicVM = new GenericCollectionMy(owner);
            this.SearchVM = new AudiosSearchViewModel();

//            if (owner == Settings.UserId)
//                this.RecommendationsMusicVM = new GenericCollectionCatalog();
        }

        public void RemoveAlbum(VKPlaylist album)
        {
            if (album.id == AllAudioViewModel.SAVED_ALBUM_ID)
            {

            }
        }

        public class GenericCollectionMy : GenericCollectionViewModel<IMusicPageItem>
        {
            public int OwnerId { get; private set; }
            public ObservableGroupingCollection<IMusicPageItem> GroupedItems { get; private set; }

            public GenericCollectionMy(int owner)
            {
                this.OwnerId = owner;
                this.GroupedItems = new ObservableGroupingCollection<IMusicPageItem>(base.Items);
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<IMusicPageItem>> callback)
            {
                offset = base.Items.Where((i) => i is VKAudio).Count();


                AudioService.Instance.GetAllTracksAndAlbums(this.OwnerId, offset, count, (result) =>
                {
                    List<IMusicPageItem> items = new List<IMusicPageItem>();

                    if (offset == 0)
                    {
                        MusicPageItemAlbums albums = new MusicPageItemAlbums();

                        //Если это наше устройство
                        if (Settings.UserId == this.OwnerId)
                        {
                            if (AudioCacheManager.Instance.DownloadedList.Count > 0)
                            {
                                VKPlaylist saved = new VKPlaylist() { id = AllAudioViewModel.SAVED_ALBUM_ID, owner_name = Settings.LoggedInUserName, title = "Сохранённые" };
                                albums.Add(saved);
                            }
                            //todo: засунуть плейлист сохранённых в общий список
                            foreach (VKPlaylist album in AudioCacheManager.Instance.PlayLists)
                            {
                                albums.Add(album);
                            }
                        }

                        if (result.error.error_code == VKErrors.None)
                        {
                            foreach (VKPlaylist album in result.response.albums)
                                albums.Add(album);
                        }

                        if (albums.Count > 0)
                            items.Add(albums);
                    }

                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.audios_count;

                        foreach (VKAudio track in result.response.audios)
                            items.Add(new MusicPageItemTrack(track));
                        
                        callback(result.error, items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return "пусто";//LocalizedStrings.GetString("NoFriends");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneAudio", "TwoFourAudioFrm", "FiveOrMoreAudioFrm");
                }
            }
        }

        /*
        public class GenericCollectionCatalog : ISupportUpDownIncrementalLoading
        {
            public ObservableCollection<VKAudio> Items { get; private set; }

            public Action<ProfileLoadingStatus> LoadingStatusUpdated { get; set; }

            public async Task LoadUpAsync()
            {
                throw new NotImplementedException();
            }

            public bool HasMoreUpItems
            {
                get { return false; }
            }

            public uint maximum = 0;

            private string nextFrom = "";

            public bool HasMoreDownItems { get { return this.Items.Count == 0 || !string.IsNullOrEmpty(this.nextFrom); } }

            public async Task LoadDownAsync(bool InReload = false)
            {
                if (InReload)
                {
                    this.nextFrom = "";

                    this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Reloading);
                }


                
            //id блоков, которые необходимо вернуть в ответе.
            //Может содержать значения:

            //feed — видео из ленты новостей пользователя;
            //ugc — популярное;
            //top — выбор редакции;
            //series — сериалы и телешоу;
            //other — прочие блоки.

            //по умолчанию feed,ugc,series,other, список слов, разделенных через запятую
            

                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["items_count"] = "4";//по 4 видео в каталоге, но не больше 16
                parameters["extended"] = "1";//Если был передан параметр extended=1, возвращаются дополнительные объекты profiles и groups, 
                parameters["filters"] = "other,top,series,ugc,live";
                parameters["from"] = this.nextFrom;
                
                var temp = await RequestsDispatcher.GetResponse<VKCountedItemsObject<VKAudio>>("audio.getRecommendations", parameters);

                //            string code = "var catlogs = API.video.getCatalog({items_count:4,extended:1,filters:\"other,top,series,ugc\"}); var my_videos = API.video.get({count:3}); return {catlogs:catlogs,my_videos:my_videos};";
                //            VKResponse<GetCatalogResponse> temp = await RequestsDispatcher.Execute<GetCatalogResponse>(code);

                if (temp.error.error_code != VKErrors.None)
                {
                    this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.LoadingFailed);

                    return;
                }
                
                
                this.nextFrom = temp.response.next;

                this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Loaded);

                foreach (var catalog in temp.response.items)
                {

                    //System.Diagnostics.Debug.WriteLine(string.Format("id {0} type {1} name {2} view {3}", catalog.id, catalog.type, catalog.name, catalog.view));

                    foreach (var c in catalog.items)
                    {
                        if (c.type == "album")
                        {
                            int i = 0;
                        }
                        if (c.owner_id < 0 && temp.response.groups != null)
                            c.Owner = temp.response.groups.Find(ow => ow.id == (-c.owner_id));
                        else
                            c.Owner = temp.response.profiles.Find(ow => ow.id == c.owner_id);
                    }


                    this.Items.Add(catalog);
                }
            }

            public async Task<object> Reload()
            {
                this.Items.Clear();

                //this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Reloading);
                await LoadDownAsync(true);
                return null;
            }

            public GenericCollectionCatalog()
            {
                this.Items = new ObservableCollection<VKAudio>();
            }
        }
*/

        public class AudiosSearchViewModel : GenericCollectionViewModel<IMusicPageItem>
        {
            public string q = string.Empty;
            public ObservableGroupingCollection<IMusicPageItem> GroupedItems { get; private set; }

            public AudiosSearchViewModel()
            {
                this.GroupedItems = new ObservableGroupingCollection<IMusicPageItem>(base.Items);
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<IMusicPageItem>> callback)
            {
                if (offset == 0)
                    base.SetInProgress(true);
                

                AudioService.Instance.SearchTracks(this.q, offset, count, (result) =>
                {
                     base.SetInProgress(false);

                     if (result.error.error_code == VKErrors.None)
                     {
                         base._totalCount = 1;// result.response.audios_count;

                         List<IMusicPageItem> items = new List<IMusicPageItem>();
                         MusicPageItemAlbums albums = new MusicPageItemAlbums();


                         foreach (var artist in result.response.artists)
                             items.Add(new MusicPageItemArtist(artist));

                         foreach (VKPlaylist album in result.response.albums)
                             albums.Add(album);

                         if(albums.Count>0)
                             items.Add(albums);


                         foreach (VKAudio track in result.response.audios)
                             items.Add(new MusicPageItemTrack(track));

                         //BUG: добавлять в существующие разделы!
                         /*
                         MusicPageItemArtists artists = new MusicPageItemArtists();
                         foreach (var artist in result.response.artists)
                         {
                             artists._List.Add(artist);
                         }

                         if (artists._List.Count > 0)
                             base.Items.Add(artists);





                         MusicPageItemAlbums albums = new MusicPageItemAlbums();
                         foreach (var album in result.response.albums)
                         {
                             albums.AddItem(album);
                         }
                         if (albums._List[0].Count > 0)
                             base.Items.Add(albums);



                         var item = base.Items.FirstOrDefault((i) => i is MusicPageItemTracks);
                         MusicPageItemTracks tracks = item == null ? new MusicPageItemTracks() : (MusicPageItemTracks)item;
                         foreach (var audio in result.response.audios)
                         {
                             tracks._List.Add(audio);
                         }

                         if (tracks._List.Count > 0)
                             base.Items.Add(tracks);
                         */
                         callback(result.error, items);


#if X86
                         if (Settings.PushSettings.app_request)
                         {
                             var temp = Settings.PushSettings;
                             temp.app_request = false;
                             Settings.PushSettings = temp;
                         }
#endif
                         
                     }
                     else
                     {
                         callback(result.error, null);
                     }
                 });
            }
        }

        /*
        public class MusicPageItemAlbums : IMusicPageItem
        {
            public string Title2 { get { return LocalizedStrings.GetString("VideoCatalog_Albums"); } }
            public List<List<VKPlaylist>> _List { get; private set; }
            public void AddItem(VKPlaylist playlist)
            {
                this._List[0].Add(playlist);
            }

            public MusicPageItemAlbums()
            {
                this._List = new List<List<VKPlaylist>>();
                var temp = new List<VKPlaylist>();
                this._List.Add(temp);
            }
        }

        public class MusicPageItemTracks : IMusicPageItem
        {
            public string Title2 { get { return LocalizedStrings.GetString("Profile_Audios"); } }
            public ObservableCollection<VKAudio> _List { get; set; }

            public MusicPageItemTracks()
            {
                this._List = new ObservableCollection<VKAudio>();
            }
        }

        public class MusicPageItemArtists : IMusicPageItem
        {
            public string Title2 { get { return LocalizedStrings.GetString("Artist"); } }
            public List<VKBaseDataForGroupOrUser> _List { get; set; }

            public MusicPageItemArtists()
            {
                this._List = new List<VKBaseDataForGroupOrUser>();
            }
        }
        */
        public class MusicPageItemAlbums : List<VKPlaylist>, IMusicPageItem
        {
            public string Key
            {
                get { return LocalizedStrings.GetString("VideoCatalog_Albums"); }
            }
        }

        public class MusicPageItemTrack : VKAudio, IMusicPageItem
        {
            public MusicPageItemTrack(VKAudio audio)
            {
                base.id = audio.id;
                base.title = audio.title;
                base.owner_id = audio.owner_id;
                base.artist = audio.artist;
                base.url = audio.url;
                base.duration = audio.duration;
                base.cover = audio.cover;
                base.actionHash = audio.actionHash;
                base.urlHash = audio.urlHash;
            }

            public string Key
            {
                get { return LocalizedStrings.GetString("Profile_Audios"); }
            }
        }

        public class MusicPageItemArtist : VKGroup, IMusicPageItem
        {
            public MusicPageItemArtist(VKGroup group)
            {
                base.id = group.id;
                base.name = group.name;
            }

            public string Key
            {
                get { return LocalizedStrings.GetString("Artist"); }
            }
        }


        

        /// <summary>
        /// Получаем список всех плейлистов
        /// </summary>
        /// <returns></returns>
        public /*async Task<IReadOnlyList<VKPlaylist>>*/IReadOnlyList<VKPlaylist> GetPlaylists()
        {
            return /*await*/ CacheManager2.ReadTextFromFile<List<VKPlaylist>>("Playlists");
        }

        public void WritePlaylists(List<VKPlaylist> ps)
        {
            CacheManager2.WriteTextToFile<List<VKPlaylist>>("Playlists", ps);
        }
    }
}
