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

using System.Collections.ObjectModel;
using Windows.Storage;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;
using System.ComponentModel;
using LunaVK.Library;
using LunaVK.Network;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;

using Windows.Devices.Geolocation;
using LunaVK.Framework;
using LunaVK.Core.ViewModels;
using LunaVK.Core.Library;
using LunaVK.Core.Enums;
using LunaVK.Core.Network;
using LunaVK.Core;

using LunaVK.ViewModels;
using Windows.UI.Xaml.Shapes;
using LunaVK.Core.Framework;
using Windows.Media.Capture;
using Windows.Foundation.Metadata;

namespace LunaVK.UC
{
    public sealed partial class AttachmentPickerUC : UserControl
    {
        public ObservableCollection<NamedAttachmentType> AttachmentTypes { get; private set; }

        public PhotoPickerAlbumsViewModel PhotosViewModel { get; private set; }
        public DocumentsViewModel DocumentsViewModel { get; private set; }
        //public AudioPageViewModel.GenericCollectionMy AudiosViewModel { get; private set; }
        public PickerAudiosViewModel AudiosViewModel { get; private set; }
        public PhotoPickerMyPhotosViewModel MyPhotosViewModel { get; private set; }
        public PickerGraffitiViewModel GraffitiViewModel { get; private set; }
        public GiftsCatalogViewModel GiftsViewModel { get; private set; }
        public VideoCatalogViewModel.GenericCollectionVideos MyVideosViewModel { get; private set; }

        Geolocator _watcher;
        IconUC MapPin;

        /// <summary>
        /// Количество уже вложенных файлов
        /// </summary>
        byte Exists = 0;

        /// <summary>
        /// Ограничение на количество вложений
        /// </summary>
        byte Maximum = 10;

        //        public Action<RenderTargetBitmap> SendGraffitiAction;
        public Action<IReadOnlyList<IOutboundAttachment>> AttachmentsAction;
        //public Action<VKDocument> DocumentAction;
        public Action DrawGraffiti;

#if WINDOWS_PHONE_APP || WINDOWS_UWP
        Windows.UI.Xaml.Controls.Maps.MapControl map;
#else
        Bing.Maps.Map map;
#endif
        public AttachmentPickerUC()
        {
            this.PhotosViewModel = new PhotoPickerAlbumsViewModel();
            this.DocumentsViewModel = new DocumentsViewModel((int)Settings.UserId);
            //this.AudiosViewModel = new AudioPageViewModel.GenericCollectionMy(0);
            this.AudiosViewModel = new PickerAudiosViewModel();
            this.MyPhotosViewModel = new PhotoPickerMyPhotosViewModel();
            this.GraffitiViewModel = new PickerGraffitiViewModel();
            this.GiftsViewModel = new GiftsCatalogViewModel();
            this.MyVideosViewModel = new VideoCatalogViewModel.GenericCollectionVideos(0);

            this.AttachmentTypes = new ObservableCollection<NamedAttachmentType>();
            base.DataContext = this;

            this.InitializeComponent();
            this.Unloaded += AttachmentPickerUC_Unloaded;
            this._listViewTypes.SelectionChanged += this.itemsControl_SelectionChanged;
        }

        public AttachmentPickerUC(byte exists, byte max, NamedAttachmentEnum type = NamedAttachmentEnum.None ) : this()
        {
            this.Exists = exists;
            this.Maximum = max;

            if(type== NamedAttachmentEnum.PhotoVideo)
            {
                this._listViewTypes.SelectionChanged -= this.itemsControl_SelectionChanged;
                var photoItem = this.pivot.Items[0];
                this.pivot.Items.Clear();
                this.pivot.Items.Add(photoItem);
                this._topPannel.Visibility = Visibility.Collapsed;
                //this._photoHeader.Height = 48;
                //this._photoHeader.Background = new SolidColorBrush(Windows.UI.Colors.Blue);
            }
        }
        /*
        public void Initialize(NamedAttachmentEnum types, byte exists)
        {
            this.Exists = exists;
            byte flip_offs = 0;

            if ((types & NamedAttachmentEnum.PhotoVideo) > 0)
                this.Add(NamedAttachmentEnum.PhotoVideo);
            else
            {
                this.pivot.Items.RemoveAt(0);
                flip_offs++;
            }

            if ((types & NamedAttachmentEnum.Audio) > 0)
                this.Add(NamedAttachmentEnum.Audio);
            else
            {
                this.pivot.Items.RemoveAt(1 - flip_offs);
                flip_offs++;
            }

            if ((types & NamedAttachmentEnum.Graffiti) > 0)
                this.Add(NamedAttachmentEnum.Graffiti);
            else
            {
                this.pivot.Items.RemoveAt(2 - flip_offs);
                flip_offs++;
            }

            if ((types & NamedAttachmentEnum.Money) > 0)
                this.Add(NamedAttachmentEnum.Money);
            else
            {
                this.pivot.Items.RemoveAt(3 - flip_offs);
                flip_offs++;
            }

            if ((types & NamedAttachmentEnum.Gift) > 0)
                this.Add(NamedAttachmentEnum.Gift);
            else
            {
                this.pivot.Items.RemoveAt(4 - flip_offs);
                flip_offs++;
            }

            if ((types & NamedAttachmentEnum.Document) > 0)
                this.Add(NamedAttachmentEnum.Document);
            else
            {
                this.pivot.Items.RemoveAt(5 - flip_offs);
                flip_offs++;
            }

            if ((types & NamedAttachmentEnum.Location) > 0)
                this.Add(NamedAttachmentEnum.Location);
            else
            {
                this.pivot.Items.RemoveAt(6 - flip_offs);
                flip_offs++;
            }

            if ((types & NamedAttachmentEnum.PhotoMy) > 0)
                this.Add(NamedAttachmentEnum.PhotoMy);
            else
            {
                this.pivot.Items.RemoveAt(7 - flip_offs);
                flip_offs++;
            }

            if ((types & NamedAttachmentEnum.VideoMy) > 0)
                this.Add(NamedAttachmentEnum.VideoMy);
            else
            {
                this.pivot.Items.RemoveAt(8 - flip_offs);
                flip_offs++;
            }
        }
        */
        void AttachmentPickerUC_Unloaded(object sender, RoutedEventArgs e)
        {
            this._listViewTypes.SelectionChanged -= itemsControl_SelectionChanged;
            TaskScheduler2.Clear();
            this.ClearAppBar();
        }

        private void itemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;

            foreach (var selected in e.AddedItems)
            {
                ListViewItem item = lv.ContainerFromItem(selected) as ListViewItem;
                VisualStateManager.GoToState(item, "Selected", true);
            }

            foreach (var unselected in e.RemovedItems)
            {
                ListViewItem item = lv.ContainerFromItem(unselected) as ListViewItem;
                VisualStateManager.GoToState(item, "Unselected", true);
            }
        }

        public void Add(NamedAttachmentEnum type)
        {
            switch (type)
            {
                case NamedAttachmentEnum.PhotoVideo:
                    {
                        this.AttachmentTypes.Add(new NamedAttachmentType("\xEB9F", LocalizedStrings.GetString("NewsFeedPhotos/Content") + "/" + LocalizedStrings.GetString("NewsFeedVideos/Content"), type));
                        break;
                    }
                case NamedAttachmentEnum.Audio:
                    {
                        this.AttachmentTypes.Add(new NamedAttachmentType("\xE8D6", "Audio", type));
                        break;
                    }
                case NamedAttachmentEnum.Graffiti:
                    {
                        this.AttachmentTypes.Add(new NamedAttachmentType("\xEE56", LocalizedStrings.GetString("AttachmentType_Graffiti"), type));
                        //this._graffitiDrawUC.SendAction = this.HandleSendGraffiti;
                        break;
                    }
                case NamedAttachmentEnum.Money:
                    {
                        this.AttachmentTypes.Add(new NamedAttachmentType("\xE8D6", "Money", type));
                        break;
                    }
                case NamedAttachmentEnum.Gift:
                    {
                        this.AttachmentTypes.Add(new NamedAttachmentType("\xE88C", "Gift", type));
                        break;
                    }
                case NamedAttachmentEnum.Document:
                    {
                        this.AttachmentTypes.Add(new NamedAttachmentType("\xE8E5", "Document", type));
                        break;
                    }
                case NamedAttachmentEnum.Location:
                    {
                        this.AttachmentTypes.Add(new NamedAttachmentType("\xE819", LocalizedStrings.GetString("AttachmentType_Location"), type));
#if WINDOWS_PHONE_APP || WINDOWS_UWP
                        map = new Windows.UI.Xaml.Controls.Maps.MapControl();
                        //map.MapServiceToken = "1jh4FPILRSo9J1ADKx2CgA";
                        map.MapTapped += map_MapTapped;
#else
                        map = new Bing.Maps.Map();
                        map.Credentials = "AmOFJiHtBeytoES_ZZxm9yiLe8hYQaaC7GUO-Fi-yX7RhhZ8n7pyn-17I9SyDlG7";
                        map.Tapped += map_Tapped;
#endif
                        map.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        MapPin = new IconUC() { Glyph = "\xEB49", FontSize = 20, Foreground = (SolidColorBrush)Application.Current.Resources["PhoneAccentColorBrush"] };
                        MapPin.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        map.Children.Add(MapPin);



                        this.itemLocation.Children.Add(map);
                        break;
                    }
                case NamedAttachmentEnum.PhotoMy:
                    {
                        this.AttachmentTypes.Add(new NamedAttachmentType("\xEB9F", "Photo VK", type));
                        break;
                    }
                case NamedAttachmentEnum.VideoMy:
                    {
                        this.AttachmentTypes.Add(new NamedAttachmentType("\xE714", "Video VK", type));
                        break;
                    }
            }

        }




        /*
        private void HandleSendGraffiti(RenderTargetBitmap img)
        {
            if (this.SendGraffitiAction != null)
                this.SendGraffitiAction(img);
        }
        */


        public class PickerAudiosViewModel : GenericCollectionViewModel<VKAudio>
        {
            public int UserId = 0;
            AudioPageViewModel.GenericCollectionMy my = new AudioPageViewModel.GenericCollectionMy(0);

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKAudio>> callback)
            {
                this.my.GetData(offset, count, (error, items) =>
                {
                    if (error.error_code != VKErrors.None)
                        callback(error, null);
                    else
                    {
                        callback(error, items.Where((i)=>i is VKAudio).Select((i)=>(VKAudio)i).ToList());
                        base._totalCount = this.my._totalCount;
                    }
                });
                /*
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                //parameters["offset"] = this.Items.Count.ToString();
                //parameters["count"] = "30";
                AudioService.Instance.GetAllAudio((result) =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                }, UserId);
                */
            }
        }

        public class PickerGraffitiViewModel : GenericCollectionViewModel<VKDocument>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKDocument>> callback)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                //parameters["offset"] = this.Items.Count.ToString();
                //parameters["count"] = "30";

                VKRequestsDispatcher.DispatchRequestToVK<List<VKDocument>>("messages.getRecentGraffities", parameters, (result)=> {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = 1;//result.response.count;
                        callback(result.error, result.response);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }
        }

        public class GiftsCatalogViewModel : ISupportUpDownIncrementalLoading
        {
            public ObservableCollection<GiftsService.GiftsSection> Items { get; private set; }

            uint _maximum;

            public async Task LoadUpAsync()
            {
                throw new NotImplementedException();
            }

            public bool HasMoreUpItems
            {
                get { return false; }
            }

            public bool HasMoreDownItems
            {
                get { return this._maximum - this.Items.Count > 0; }
            }

            public GiftsCatalogViewModel()
            {
                this.Items = new ObservableCollection<GiftsService.GiftsSection>();
            }

            public async Task<object> Reload()
            {
                this.Items.Clear();
                //this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Reloading);
                await LoadDownAsync(true);
                return null;
            }

            public async Task LoadDownAsync(bool InReload = false)
            {
                //Dictionary<string, string> parameters = new Dictionary<string, string>();
                //parameters["offset"] = this.Items.Count.ToString();
                //parameters["count"] = "30";

                GiftsService.Instance.GetCatalog(0, null, (result) => {
                    if (result != null && result.error.error_code == VKErrors.None)
                    {
                        Execute.ExecuteOnUIThread(() => {
                            foreach (var section in result.response)
                            {
                                this.Items.Add(section);
                            }
                        });
                    }
                });
            }
        }
        /*
        public class PickerDocumentsViewModel : GenericCollectionViewModel<VKDocument>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKDocument>> callback)
            {
                DocumentsService.Instance.GetDocuments((result)=> {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                },offset,count);
            }
        }
        */
        public class NamedAttachmentType
        {
            /*
             * В коментах : фото,видео,док,аудио + графити андроид
             * в сообщениях: фото,видео,аудио,граффити,док,"подарок","местоположение"
             * */
            public string Icon { get; private set; }
            public string Title { get; private set; }
            public NamedAttachmentEnum Type;

            public NamedAttachmentType(string icon, string title, NamedAttachmentEnum type)
            {
                this.Icon = icon;
                this.Title = title;
                this.Type = type;
            }

        }

        public enum NamedAttachmentEnum : short
        {
            None = 0,

            PhotoVideo = 2,
            Audio = 4,
            Graffiti = 8,
            Money = 16,
            Gift = 32,
            Document = 64,
            Location = 128,
            PhotoMy = 256,
            VideoMy = 512
        }

        private void Folder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            PhotoPickerAlbumsViewModel.NamedFolder f = element.DataContext as PhotoPickerAlbumsViewModel.NamedFolder;

            this.PhotosViewModel.SetNewsSource(f.Folder);
            this.PhotosViewModel.UpdateUI();
        }

        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gv = sender as GridView;
            var panel = (ItemsWrapGrid)gv.ItemsPanelRoot;
            panel.Orientation = Orientation.Horizontal;

            double colums = Math.Max(2, e.NewSize.Width / 125);

            panel.MaximumRowsOrColumns = (int)colums;

            panel.ItemHeight = panel.ItemWidth = e.NewSize.Width / (int)colums;
            //panel.ItemHeight = 60;
        }
        

        private void Category_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            NamedAttachmentType vm = element.DataContext as NamedAttachmentType;
            if (vm.Type == NamedAttachmentEnum.PhotoVideo)
            {
                this.pivot.SelectedIndex = 0;
            }
            else if (vm.Type == NamedAttachmentEnum.Document)
            {
                this.pivot.SelectedIndex = 1;
            }
            else if (vm.Type == NamedAttachmentEnum.Audio)
            {
                this.pivot.SelectedIndex = 2;
            }
            else if (vm.Type == NamedAttachmentEnum.Graffiti)
            {
                this.pivot.SelectedIndex = 3;
            }
            else if (vm.Type == NamedAttachmentEnum.Location)
            {
                this.pivot.SelectedIndex = 4;
            }
        }
        /*
        private void UpdateTopOverlay(NamedAttachmentEnum type)
        {
            string text = "";
            Visibility vis = Visibility.Collapsed;

            switch(type)
            {
                case NamedAttachmentEnum.PhotoVideo:
                    {
                        text = "All photos";
                        vis = Visibility.Visible;
                        break;
                    }
                case NamedAttachmentEnum.Audio:
                    {
                        text = "Music";
                        break;
                    }
                case NamedAttachmentEnum.Document:
                    {
                        text = "Documents";
                        break;
                    }
                default:
                    {
                        text = type.ToString();
                        break;
                    }
            }

            this._overlayText.Text = text;
            this._overlayIcon.Visibility = vis;
        }
        */
        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Pivot fv = sender as Pivot;

            PivotItem item = (PivotItem)fv.SelectedItem;
            if (item.DataContext is PhotoPickerAlbumsViewModel vmPhotos)
            {
                //this.UpdateTopOverlay(NamedAttachmentEnum.PhotoVideo);
            }
            /*
            else if (item.DataContext is PickerDocumentsViewModel vm)
            {
                //if (vm.Documents.Count == 0)
                //    vm.Reload();
                this.ClearAppBar();
            }
            */
            //else if (item.DataContext is PhotoPickerMyPhotosViewModel vmMyPhotos)
            //{
            //    if (vmMyPhotos.Photos.Count == 0)
            //        vmMyPhotos.Reload();
            //    this.ClearAppBar();
            //}
            else if (item.Tag != null && item.Tag.ToString() == "location")
            {
                this.Geo();
            }
            else if (item.DataContext is DocumentsViewModel vmDocuments)
            {
                //this.UpdateTopOverlay(NamedAttachmentEnum.Document);
                if (vmDocuments.Items.Count == 0)
                    vmDocuments.Reload();
            }
            else if (item.DataContext is PickerGraffitiViewModel vmGraffiti)
            {
                //this.UpdateTopOverlay(NamedAttachmentEnum.Graffiti);
                if (vmGraffiti.Items.Count == 0)
                    vmGraffiti.LoadDownAsync(true);
            }
            else if (item.DataContext is GiftsCatalogViewModel vmGifts)
            {
                //this.UpdateTopOverlay(NamedAttachmentEnum.Gift);
                if (vmGifts.Items.Count == 0)
                    vmGifts.Reload();
            }
            

            this.PhotosViewModel.SelectedPhotos.Clear();
            this.PhotosViewModel.UpdateUI();
        }

        private void ClearAppBar()
        {
            if (this.Page == null)
                return;

            if (this.Page.BottomAppBar != null)
                this.Page.BottomAppBar = null;
        }

        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        private Page Page
        {
            get { return CustomFrame.Instance.Content as Page; }
        }

        private void BuildAppBar()
        {
            //MapsSettings.ApplicationContext.ApplicationId = ("55677f7c-3dab-4a57-95b2-4efd44a0e692");
            //MapsSettings.ApplicationContext.AuthenticationToken = ("1jh4FPILRSo9J1ADKx2CgA");
            AppBarButton btn = new AppBarButton() { Icon = new SymbolIcon(Symbol.Accept), Label = "применить" };

            btn.Command = new DelegateCommand((a) =>
            {
                List<IOutboundAttachment> ret = new List<IOutboundAttachment>();

                OutboundGeoAttachment at = new OutboundGeoAttachment(this.Latitude, this.Longitude);

                ret.Add(at);

                this.AttachmentsAction?.Invoke(ret);
            });
            CommandBar applicationBar = new CommandBar();
            applicationBar.PrimaryCommands.Add(btn);
            this.Page.BottomAppBar = applicationBar;
        }

        private void Geo()
        {
            if (_watcher != null)
            {
                _watcher.GetGeopositionAsync();
                this.BuildAppBar();
                return;
            }
            //https://docs.microsoft.com/en-us/windows/uwp/maps-and-location/get-location
            //var accessStatus = await Geolocator.RequestAccessAsync();//uwp
            _watcher = new Geolocator();

            _watcher.StatusChanged += Watcher_OnStatusChanged;
            _watcher.PositionChanged += Watcher_OnPositionChanged;
        }

        void Watcher_OnStatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            switch (args.Status)
            {
                case PositionStatus.Ready:
                    {
                        LunaVK.Core.Framework.Execute.ExecuteOnUIThread(() => {
                            this.map.Visibility = Windows.UI.Xaml.Visibility.Visible;
                            this.BuildAppBar();
                        });

                        break;
                    }
                case PositionStatus.Initializing:
                    {
                        LunaVK.Core.Framework.Execute.ExecuteOnUIThread(() => {
                            this.locationStatusIcon.Glyph = "\xEC43";
                            this.locationStatusText.Text = "Инициализация";
                        });

                        break;
                    }

                case PositionStatus.Disabled:
                    {
                        LunaVK.Core.Framework.Execute.ExecuteOnUIThread(() =>
                        {
                            this.locationStatusIcon.Glyph = "\xE7BA";
                            this.locationStatusText.Text = "Выключено";
                        });

                        break;
                    }
            }
        }

        void Watcher_OnPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            var point = args.Position.Coordinate.Point;
            this.Latitude = point.Position.Latitude;
            this.Longitude = point.Position.Longitude;
#if WINDOWS_PHONE_APP || WINDOWS_UWP
            LunaVK.Core.Framework.Execute.ExecuteOnUIThread(() =>
            {
                //map.RequestedTheme = ElementTheme.Dark;
                map.Center = point;
                map.ZoomLevel = 16.0;
                //MapPin.Visibility = Windows.UI.Xaml.Visibility.Visible;
                //map.Children.Add(MapPin);
                map.ColorScheme = LunaVK.Core.Settings.BackgroundType == false ? Windows.UI.Xaml.Controls.Maps.MapColorScheme.Dark : Windows.UI.Xaml.Controls.Maps.MapColorScheme.Light;
                var element = new Windows.UI.Xaml.Controls.Maps.MapIcon();
                //element.Title="lol";
                element.Location = point;
                map.MapElements.Add(element);
            });
#else
            Bing.Maps.Location location = new Bing.Maps.Location();
            location.Latitude = point.Position.Latitude;
            location.Longitude = point.Position.Longitude;
            map.SetView(location);
#endif

            this.StopGeoWatcher();
        }

#if WINDOWS_PHONE_APP || WINDOWS_UWP
        void map_MapTapped(Windows.UI.Xaml.Controls.Maps.MapControl sender, Windows.UI.Xaml.Controls.Maps.MapInputEventArgs args)
        {
            Geopoint geopoint = args.Location;
            var element = this.map.MapElements[0] as Windows.UI.Xaml.Controls.Maps.MapIcon;
            element.Location = geopoint;
            this.Latitude = geopoint.Position.Latitude;
            this.Longitude = geopoint.Position.Longitude;
        }
#endif
        void map_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var ttv = this.map.TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));


            Point point = e.GetPosition(null);
            point.X -= screenCoords.X;
            point.Y -= screenCoords.Y;
#if WINDOWS_PHONE_APP || WINDOWS_UWP
            if (map.MapElements.Count > 0)
            {
                Geopoint geopoint;
                this.map.GetLocationFromOffset(point, out geopoint);
                var element = this.map.MapElements[0] as Windows.UI.Xaml.Controls.Maps.MapIcon;
                //BasicGeoposition b = new BasicGeoposition() { Longitude };
                //Geopoint geopoint = new Geopoint(  b );
                element.Location = geopoint;
            }
#else


            Bing.Maps.Location result;
            if (map.TryPixelToLocation(point, out result))
            {
                //Bing.Maps.Search.SearchManager.
                MapPin.Visibility = Windows.UI.Xaml.Visibility.Visible;
                Bing.Maps.MapLayer.SetPosition(MapPin, result);

                this.Latitude = result.Latitude;
                this.Longitude = result.Longitude;
            }
#endif
        }

        private void StopGeoWatcher()
        {
            _watcher.StatusChanged -= Watcher_OnStatusChanged;
            _watcher.PositionChanged -= Watcher_OnPositionChanged;
            //        this.SetProgressIndicator(false);
        }

        private void _brd_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            int selected = this.PhotosViewModel.Attached;

            FrameworkElement element = sender as FrameworkElement;
            PhotoPickerAlbumsViewModel.AlbumPhoto vm = element.DataContext as PhotoPickerAlbumsViewModel.AlbumPhoto;
            GridViewItem item = this._variableGridView.ContainerFromItem(vm) as GridViewItem;

            if (this.PhotosViewModel.SelectedPhotos.Contains(vm))
            {

                selected--;
                vm.Number = 0;
                vm.IsSelected = false;

                this.PhotosViewModel.SelectedPhotos.Remove(vm);
            }
            else
            {
                if (selected + this.Exists > 9)
                {
                    this.PhotosViewModel.SelectedPhotos.Remove(vm);
                    return;
                }


                this.PhotosViewModel.SelectedPhotos.Add(vm);

                vm.Number = selected + 1;


                vm.IsSelected = true;
            }

            var orderPhotos = this.PhotosViewModel.SelectedPhotos.OrderBy((p2) => { return p2.Number; });

            int j = 1;

            foreach (var photo in orderPhotos)
            {
                photo.Number = j;
                j++;
            }


            this.PhotosViewModel.UpdateUI();
        }


        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            img.Animate(0, 1, "Opacity", 600);
        }



        private async void AttachAction_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TaskScheduler2.Clear();

            List<IOutboundAttachment> ret = new List<IOutboundAttachment>();

            foreach (var item in this.PhotosViewModel.Photos)
            {
                if (item.IsSelected)
                {
                    if (item.IsVideoVisibility == Visibility.Collapsed)
                    {
                        /*
                        OutboundPhotoAttachment a = new OutboundPhotoAttachment();
                        a.sf = item.sf;
                        a.ImgWidth = item.BitmapImage.PixelWidth;
                        a.ImgHeight = item.BitmapImage.PixelHeight;
                        a.LocalUrl2 = item.BitmapImage;
                        ret.Add(a);
                        */
                        OutboundPhotoAttachment a = await OutboundPhotoAttachment.CreateForUploadNewPhoto(item.sf);
                        ret.Add(a);
                    }
                    else
                    {
                        OutboundVideoAttachment a = new OutboundVideoAttachment(item.sf);

                        ret.Add(a);
                    }

                }
            }

            this.AttachmentsAction?.Invoke(ret);
        }

        private void Document_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VKDocument vm = (sender as FrameworkElement).DataContext as VKDocument;


            List<IOutboundAttachment> ret = new List<IOutboundAttachment>();
            OutboundDocumentAttachment at = new OutboundDocumentAttachment(vm);
            ret.Add(at);
            this.AttachmentsAction?.Invoke(ret);
            //this.DocumentAction?.Invoke(vm);
        }

        private async void UploadFile_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileOpenPicker fileOpenPicker = new Windows.Storage.Pickers.FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add("*");

            fileOpenPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;

            StorageFile file = await fileOpenPicker.PickSingleFileAsync();

            if (file != null)
            {
                OutboundDocumentAttachment a = new OutboundDocumentAttachment(file);
                a._sf = file;

                BitmapImage bimg = new BitmapImage();

                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    bimg.SetSource(stream);
                }
                a.LocalUrl2 = bimg;

                List<IOutboundAttachment> ret = new List<IOutboundAttachment>();
                ret.Add(a);

                this.AttachmentsAction?.Invoke(ret);
            }
        }

        


        













        private void _listViewTypes_Loaded(object sender, RoutedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.Items.Add(new NamedAttachmentType("\xEB9F", "Фото/Видео", NamedAttachmentEnum.PhotoVideo));
            lv.Items.Add(new NamedAttachmentType("\xE8D6", "Audio", NamedAttachmentEnum.Audio));
            lv.Items.Add(new NamedAttachmentType("\xE819", LocalizedStrings.GetString("AttachmentType_Location"), NamedAttachmentEnum.Location));
            lv.Items.Add(new NamedAttachmentType("\xE88C", "Gift", NamedAttachmentEnum.Gift));
            lv.Items.Add(new NamedAttachmentType("\xE8C7", "Money", NamedAttachmentEnum.Money));
            lv.Items.Add(new NamedAttachmentType("\xE8E5", "Document", NamedAttachmentEnum.Document));
            lv.Items.Add(new NamedAttachmentType("\xEE56", LocalizedStrings.GetString("AttachmentType_Graffiti"), NamedAttachmentEnum.Graffiti));
            lv.Items.Add(new NamedAttachmentType("\xEB9F", "Photo VK", NamedAttachmentEnum.PhotoMy));
            lv.Items.Add(new NamedAttachmentType("\xE714", "Video VK", NamedAttachmentEnum.VideoMy));
        }
        /*
        private void _variableGridView_Loaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
                return;
            ScrollViewer scrollviewers = element.GetScrollViewer();
            scrollviewers.ViewChanged += Scrollviewers_ViewChanged;
        }

        private void Scrollviewers_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;

            double newValue = 106 - sv.VerticalOffset;

            if (newValue < -10)//94-56
                newValue = -10;

            //Debug.WriteLine(newValue);

            _lvTransform.Y = _lvTransform2.Y = newValue;


            overPanel.Opacity = this.CalculateOpacity(newValue, 64);
            overPanel.IsHitTestVisible = overPanel.Opacity == 1.0;
        }
        */
        private double CalculateOpacity(double verticalPos, double maxSP)
        {
            double num1;
            if (verticalPos > maxSP)
            {
                num1 = 0;
            }
            else
            {
                num1 = (maxSP - verticalPos) / maxSP;
            }

            if (num1 > 1)
                num1 = 1.0;

            return num1;
        }

        private void Ell_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            Border el = sender as Border;

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                object _blinkBrush = Application.Current.Resources["SystemControlTransparentRevealBorderBrush"];
                if (_blinkBrush != null)
                {
                    el.BorderThickness = new Thickness(2);
                    el.BorderBrush = _blinkBrush as Brush;//кстати, работает
                }
            }
            */
        }

        private void Pivot_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            Pivot pivot = sender as Pivot;
            this.DocumentsViewModel.SubPage = pivot.SelectedIndex;
            if (this.DocumentsViewModel.Items[this.DocumentsViewModel.SubPage].Items.Count == 0)
            {
                this.DocumentsViewModel.Items[this.DocumentsViewModel.SubPage].Reload();
            }
        }

        private void ExtendedListView3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ListView lv = sender as ListView;
            //var vm = lv.SelectedItem as VKDocument;
            var vm = e.AddedItems[0] as VKDocument;

            List<IOutboundAttachment> ret = new List<IOutboundAttachment>();
            OutboundDocumentAttachment at = new OutboundDocumentAttachment(vm);
            ret.Add(at);
            this.AttachmentsAction?.Invoke(ret);


            //this.DocumentAction?.Invoke(vm);
        }

        private void OverPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            PivotItem sitem = (PivotItem)this.pivot.SelectedItem;
            if (!(sitem.DataContext is PhotoPickerAlbumsViewModel))
            {
                return;
            }
            

            MenuFlyout menu = new MenuFlyout();



            MenuFlyoutItem item0 = new MenuFlyoutItem()
            {
                Text = "All Photos",
                Command = new DelegateCommand((args) => {
                    this.PhotosViewModel.SetNewsSource(KnownFolders.PicturesLibrary);
                }),
//                Icon = new FontIcon() { Glyph= "\xECA5" }//BUG
            };
            menu.Items.Add(item0);

            if (this.PhotosViewModel.Folders.Count > 0)
            {
                MenuFlyoutSeparator separator = new MenuFlyoutSeparator();
                menu.Items.Add(separator);
            }

            foreach (var folder in this.PhotosViewModel.Folders)
            {
                MenuFlyoutItem item = new MenuFlyoutItem()
                {
                    Text = folder.FolderName,
                    Command = new DelegateCommand((args) => {
                        this.PhotosViewModel.SetNewsSource(folder.Folder);
                    })
                };
                menu.Items.Add(item);
            }

            MenuFlyoutSeparator separator2 = new MenuFlyoutSeparator();
            menu.Items.Add(separator2);

            MenuFlyoutItem item2 = new MenuFlyoutItem()
            {
                Text = "Другое фото",
                Command = new DelegateCommand((args) =>
                {
                    //this.PhotosViewModel.SetNewsSource(KnownFolders.PicturesLibrary);
                    this.UploadPhoto_Tapped(null, null);
                }),
//                Icon = new FontIcon() { Glyph = "\xEB9F" }//BUG
            };
            menu.Items.Add(item2);

            MenuFlyoutItem item3 = new MenuFlyoutItem()
            {
                Text = "Другое видео",
                Command = new DelegateCommand((args) =>
                {
                    this.UploadVideo_Tapped(null,null);
                }),
 //               Icon = new FontIcon() { Glyph = "\xEC80" }//BUG
            };
            menu.Items.Add(item3);


            menu.ShowAt(sender as FrameworkElement);
        }

        private void Graffiti_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            //GridView lv = sender as GridView;
            //var vm = lv.SelectedItem as VKDocument;
            var vm = e.AddedItems[0] as VKDocument;

            List<IOutboundAttachment> ret = new List<IOutboundAttachment>();
            OutboundDocumentAttachment at = new OutboundDocumentAttachment(vm);
            ret.Add(at);
            this.AttachmentsAction?.Invoke(ret);


            //this.DocumentAction?.Invoke(vm);
        }

        private void DrawGraffiti_Click(object sender, RoutedEventArgs e)
        {
            this.DrawGraffiti?.Invoke();
        }

        private void MyPhotos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = e.AddedItems[0] as VKPhoto;


            /*
            IOutboundAttachment a;
            
            var photo = new OutboundPhotoAttachment();
            photo._photo = vm;
            photo.LocalUrl2 = new BitmapImage(new Uri(vm.MaxPhoto));
            a = photo;
            */
            IOutboundAttachment a = OutboundPhotoAttachment.CreateForChoosingExistingPhoto(vm, 0, false);


            List<IOutboundAttachment> ret = new List<IOutboundAttachment>() { a };

            this.AttachmentsAction?.Invoke(ret);
        }

        private void MyVideos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = e.AddedItems[0] as VKVideoBase;



            IOutboundAttachment a = new OutboundVideoAttachment(vm);

            List<IOutboundAttachment> ret = new List<IOutboundAttachment>{ a };

            this.AttachmentsAction?.Invoke(ret);
        }

        private async void TakePhoto_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            CameraCaptureUI cameraCaptureUI = new CameraCaptureUI();
            cameraCaptureUI.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.HighestAvailable;

            var file = await cameraCaptureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if(file!=null)
            {
                /*
                OutboundPhotoAttachment a = new OutboundPhotoAttachment();
                a.sf = file;

                BitmapImage bimg = new BitmapImage();

                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    bimg.SetSource(stream);
                }
                a.LocalUrl2 = bimg;

                List<IOutboundAttachment> ret = new List<IOutboundAttachment>();
                ret.Add(a);
                */
                OutboundPhotoAttachment a = await OutboundPhotoAttachment.CreateForUploadNewPhoto(file);
                List<IOutboundAttachment> ret = new List<IOutboundAttachment>() { a };
                this.AttachmentsAction?.Invoke(ret);
            }
        }

        private async void TakeVideo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            CameraCaptureUI cameraCaptureUI = new CameraCaptureUI();
            cameraCaptureUI.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.HighestAvailable;

            var file = await cameraCaptureUI.CaptureFileAsync(CameraCaptureUIMode.Video);
            if (file != null)
            {
                OutboundVideoAttachment a = new OutboundVideoAttachment(file);

                //BitmapImage bimg = new BitmapImage();

                //using (var stream = await file.OpenAsync(FileAccessMode.Read))
                //{
                //    bimg.SetSource(stream);
                //}
                //a.LocalUrl2 = bimg;

                List<IOutboundAttachment> ret = new List<IOutboundAttachment>();
                ret.Add(a);

                this.AttachmentsAction?.Invoke(ret);
            }
        }





        private async void UploadPhoto_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile file = await this.SelectSinglePhotoOrVideo(true);

            if (file != null)
            {
                /*
                OutboundPhotoAttachment a = new OutboundPhotoAttachment();
                a.sf = file;

                BitmapImage bimg = new BitmapImage();

                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    bimg.SetSource(stream);
                }
                a.LocalUrl2 = bimg;
                */
                OutboundPhotoAttachment a = await OutboundPhotoAttachment.CreateForUploadNewPhoto(file);
                List<IOutboundAttachment> ret = new List<IOutboundAttachment>() { a };

                string faToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                a.token = faToken;
                this.AttachmentsAction?.Invoke(ret);
            }
        }

        private async Task<StorageFile> SelectSinglePhotoOrVideo(bool isPhoto)
        {
            Windows.Storage.Pickers.FileOpenPicker fileOpenPicker = new Windows.Storage.Pickers.FileOpenPicker();
            if(isPhoto)
            {
                //Допустимые форматы: JPG, PNG, GIF.
                fileOpenPicker.FileTypeFilter.Add(".jpeg");
                fileOpenPicker.FileTypeFilter.Add(".jpg");
                fileOpenPicker.FileTypeFilter.Add(".png");
                fileOpenPicker.FileTypeFilter.Add(".gif");
                fileOpenPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            }
            else
            {
                fileOpenPicker.FileTypeFilter.Add(".avi");
                fileOpenPicker.FileTypeFilter.Add(".mp4");
                fileOpenPicker.FileTypeFilter.Add(".mpeg");
                fileOpenPicker.FileTypeFilter.Add(".wmv");
                fileOpenPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            }

            /*
            var view = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView();
            view.Activated += this.view_Activated;
            
            fileOpenPicker.PickSingleFileAndContinue();
            */

            StorageFile file = await fileOpenPicker.PickSingleFileAsync();
            return file;
        }

        private async void UploadVideo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile file = await this.SelectSinglePhotoOrVideo(false);

            if (file != null)
            {
                OutboundVideoAttachment a = new OutboundVideoAttachment(file);
                List<IOutboundAttachment> ret = new List<IOutboundAttachment>();
                ret.Add(a);
                this.AttachmentsAction?.Invoke(ret);
            }
        }

        private void Back_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            List<IOutboundAttachment> ret = new List<IOutboundAttachment>();
            this.AttachmentsAction?.Invoke(ret);
        }

        private void ExtendedListView3_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var vm = e.AddedItems[0] as VKAudio;



            IOutboundAttachment a = new OutboundAudioAttachment(vm);

            List<IOutboundAttachment> ret = new List<IOutboundAttachment>{ a };

            this.AttachmentsAction?.Invoke(ret);
        }

        private void Photo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            PhotoPickerAlbumsViewModel.AlbumPhoto vm = element.DataContext as PhotoPickerAlbumsViewModel.AlbumPhoto;
            if(vm.IsVideoVisibility== Visibility.Collapsed)
                NavigatorImpl.Instance.NavigateToPhotoPickerPhotos(0,vm.sf);
        }
        /*
private async void view_Activated(Windows.ApplicationModel.Core.CoreApplicationView sender, Windows.ApplicationModel.Activation.IActivatedEventArgs args)
{
var fargs = args as Windows.ApplicationModel.Activation.FileOpenPickerContinuationEventArgs;
sender.Activated -= this.view_Activated;

if (fargs.Files.Count > 0)
{
StorageFile file = fargs.Files[0];
if (file != null)
{
BitmapImage bimg = new BitmapImage();

using (var stream = await file.OpenAsync(FileAccessMode.Read))
{
bimg.SetSource(stream);
}

OutboundPhotoAttachment a = new OutboundPhotoAttachment() { sf = file, LocalUrl2 = bimg };

List<IOutboundAttachment> ret = new List<IOutboundAttachment>();
ret.Add(a);

if (this.AttachmentsAction != null)
this.AttachmentsAction(ret);
}
}
}
*/
    }
}
