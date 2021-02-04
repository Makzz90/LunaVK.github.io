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

using LunaVK.Core.DataObjects;
using LunaVK.Framework;
using LunaVK.Core.ViewModels;
using LunaVK.Core;
using LunaVK.Core.Enums;
using LunaVK.UC;

namespace LunaVK.Pages.Group
{
    /// <summary>
    /// Это страница с видео и альбомами пользователя/группы
    /// </summary>
    public sealed partial class GroupVideosPage : PageBase
    {
        public GroupVideosPage()
        {
            this.InitializeComponent();
        }

        private GroupVideosViewModel VM
        {
            get { return base.DataContext as GroupVideosViewModel; }
        }
        
        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            IDictionary<string, object> QueryString = e.Parameter as IDictionary<string, object>;
            int owner = (int)QueryString["OwnerId"];
            string ownerName = (string)QueryString["OwnerName"];
            
            base.DataContext = new GroupVideosViewModel(owner);

            base.Title = LocalizedStrings.GetString( "Profile_Videos") + " " + ownerName;
        }
        
        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CatalogItemUC item = sender as CatalogItemUC;
            var vm = item.DataContext as VKVideoBase;
            Library.NavigatorImpl.Instance.NavigateToVideoWithComments(vm.owner_id, vm.id, vm.access_key, vm, item.Img);
        }

        private void Album_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CatalogItemUC item = sender as CatalogItemUC;
            var vm = item.DataContext as VKVideoAlbum;
            Library.NavigatorImpl.Instance.NavigateToVideoAlbum(vm.id, vm.title, vm.owner_id);
        }
    }
}
