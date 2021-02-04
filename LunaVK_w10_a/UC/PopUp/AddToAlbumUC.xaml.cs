using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LunaVK.UC.PopUp
{
    //AddToAlbumPage
    public sealed partial class AddToAlbumUC : UserControl
    {
        public ObservableCollection<VKVideoAlbum> Items { get; private set; }
        public Action Done;
        private int _ownerId;
        private uint _videoId;
        private List<int> _albumsByVideoIds = new List<int>();


        public AddToAlbumUC()
        {
            this.InitializeComponent();

            this.Items = new ObservableCollection<VKVideoAlbum>();
            base.DataContext = this;
            this.Loaded += AddToAlbumUC_Loaded;
        }

        public AddToAlbumUC(int ownerId, uint videoId):this()
        {
            this._ownerId = ownerId;
            this._videoId = videoId;
        }

        private void AddToAlbumUC_Loaded(object sender, RoutedEventArgs e)
        {
            VideoService.Instance.GetAddToAlbumInfo((int)Settings.UserId, this._ownerId, this._videoId, (result) => {
                if (result.error.error_code != Core.Enums.VKErrors.None)
                    return;

                Execute.ExecuteOnUIThread(() => {
                    this._albumsByVideoIds = result.response.AlbumsByVideo;
                    foreach (var va in result.response.Albums.items)
                    {
                        this.Items.Add(va);
                        bool IsSelected = this._albumsByVideoIds.Contains(va.id);
                        if(IsSelected)
                        {
                            this._lv.SelectedItems.Add(va);
                        }
                    }                    
                });
            });
        }

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(this._lv.SelectedItems.Count>0)
            {
                List<int> list1 = this._lv.SelectedItems.Select((item) => ((VKVideoAlbum)item).id).ToList();
                List<int> list2 = this.Items.Select((item) => item.id).Except(list1).ToList();

                VideoService.Instance.AddRemoveToAlbums(this._videoId, this._ownerId, (int)Settings.UserId, list1, list2, (result) => {
                    Execute.ExecuteOnUIThread(() => {
                        if(result.error.error_code == Core.Enums.VKErrors.None)
                        {
                            this.Done();
                        }
                    });
                });
            }
        }
    }
}
