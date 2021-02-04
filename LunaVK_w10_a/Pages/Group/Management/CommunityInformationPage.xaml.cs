using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using LunaVK.Core.ViewModels;
using LunaVK.Core;
using LunaVK.Core.Enums;
using LunaVK.Core.DataObjects;
using LunaVK.Framework;
using LunaVK.Library;

namespace LunaVK.Pages.Group.Management
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class CommunityInformationPage : PageBase
    {
        //InformationPage.xaml
        public CommunityInformationPage()
        {
            this.InitializeComponent();
            this.Loaded += (s, e) =>
            {
                /*
                //(Window.Current.Content as Framework.CustomFrame).HeaderWithMenu.SetTitle(LocalizedStrings.GetString("Settings"));
                CommandBar applicationBar = new CommandBar();

                AppBarButton btn = new AppBarButton();
                btn.Icon = new SymbolIcon(Symbol.Save);
                btn.Label = "сохранить";
                btn.Command = new DelegateCommand((a) =>
                {
                    this.VM.SaveChanges();
                });
                applicationBar.PrimaryCommands.Add(btn);
                base.BottomAppBar = applicationBar;
                */
                this.VM.LoadData();

                CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE74E", Clicked = this._appBarButton_Click });
            };

            this._main.Visibility = Visibility.Collapsed;
        }

        private void _appBarButton_Click(object sender)
        {
            this.VM.SaveChanges();
        }

        private CommunityInformationViewModel VM
        {
            get { return base.DataContext as CommunityInformationViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.DataContext = new CommunityInformationViewModel((uint)e.Parameter);
            base.HandleOnNavigatedTo(e);
            this.VM.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;

            base.Title = LocalizedStrings.GetString("Management_Information/Title");
        }

        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {
            if(status == ProfileLoadingStatus.Loaded)
            {
                this._main.Visibility = Visibility.Visible;

                if (this.VM.Information.type == VKGroupType.Page)
                {
           //         this.ExtendedForPage.Visibility = Visibility.Visible;
          //          this.ExtendedForGroup.Visibility = Visibility.Collapsed;
                }
                else
                {
           //         this.ExtendedForPage.Visibility = Visibility.Collapsed;
          //          this.ExtendedForGroup.Visibility = Visibility.Visible;
                }
            }

            VisualStateManager.GoToState(this._loading, status.ToString(), true);
        }

        private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//bug
            //this.VM.Information.RefreshUI();
        }
    }
}
