using LunaVK.Core.Library;
using LunaVK.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
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

namespace LunaVK.UC.AttachmentPickers
{
    public sealed partial class LocationPickerUC : UserControl
    {
        public Action<IReadOnlyList<IOutboundAttachment>> AttachmentsAction;

        IconUC MapPin;
        Geolocator _watcher;
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        public LocationPickerUC()
        {
            this.InitializeComponent();
            map.MapServiceToken = "1jh4FPILRSo9J1ADKx2CgA";
            this.Loaded += LocationPickerUC_Loaded;
        }
        
        private void LocationPickerUC_Loaded(object sender, RoutedEventArgs e)
        {
            MapPin = new IconUC() { Glyph = "\xEB49", FontSize = 20, Foreground = (SolidColorBrush)Application.Current.Resources["PhoneAccentColorBrush"] };
            MapPin.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            map.Children.Add(MapPin);
            

            _watcher = new Geolocator();

            _watcher.StatusChanged += Watcher_OnStatusChanged;
            _watcher.PositionChanged += Watcher_OnPositionChanged;

            _watcher.GetGeopositionAsync();
        }
        
        private Page Page
        {
            get { return CustomFrame.Instance.Content as Page; }
        }

        private void BuildAppBar()
        {
            CommandBar applicationBar = new CommandBar();

            AppBarButton btn = new AppBarButton() { Icon = new SymbolIcon(Symbol.Accept), Label = "применить" };

            btn.Command = new DelegateCommand((a) =>
            {
                List<IOutboundAttachment> ret = new List<IOutboundAttachment>();

                OutboundGeoAttachment at = new OutboundGeoAttachment(this.Latitude, this.Longitude);

                ret.Add(at);

                this.AttachmentsAction?.Invoke(ret);
            });

            AppBarButton btn2 = new AppBarButton() { Icon = new SymbolIcon(Symbol.Cancel), Label = "отмена" };

            btn2.Command = new DelegateCommand((a) =>
            {
                List<IOutboundAttachment> ret = new List<IOutboundAttachment>();
                this.AttachmentsAction?.Invoke(ret);
            });
            
            applicationBar.PrimaryCommands.Add(btn);
            applicationBar.PrimaryCommands.Add(btn2);

            this.Page.BottomAppBar = applicationBar;
        }

        void Watcher_OnStatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            switch (args.Status)
            {
                case PositionStatus.Ready:
                    {
                        LunaVK.Core.Framework.Execute.ExecuteOnUIThread(() =>
                        {
                            this.map.Visibility = Windows.UI.Xaml.Visibility.Visible;
                            this.BuildAppBar();
                        });

                        break;
                    }
                case PositionStatus.Initializing:
                    {
                        LunaVK.Core.Framework.Execute.ExecuteOnUIThread(() =>
                        {
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

        private void StopGeoWatcher()
        {
            _watcher.StatusChanged -= Watcher_OnStatusChanged;
            _watcher.PositionChanged -= Watcher_OnPositionChanged;
            //        this.SetProgressIndicator(false);
        }
    }
}
