using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.ViewModels;
using LunaVK.Framework;
using LunaVK.UC;
using LunaVK.UC.PopUp;
using LunaVK.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.Pages.Group.Management
{
    public sealed partial class LinksPage : PageBase
    {
        public LinksPage()
        {
            this.InitializeComponent();
            this.Loaded += LinksPage_Loaded;
        }

        private void LinksPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.mainScroll.GetListView.ReorderMode = ListViewReorderMode.Enabled;
            this.mainScroll.GetListView.CanReorderItems = true;
            //this.mainScroll.GetListView.CanDrag = true;
            this.mainScroll.GetListView.AllowDrop = true;
        }

        private LinksViewModel VM
        {
            get { return base.DataContext as LinksViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            uint CommunityId = (uint)e.Parameter;
            base.DataContext = new LinksViewModel(CommunityId);

            base.Title = LocalizedStrings.GetString("Management_Links/Title");
        }

        private void Border_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            var vm = element.DataContext as VKGroupLink;

            PopUP2 menu = new PopUP2();
            PopUP2.PopUpItem item = new PopUP2.PopUpItem() { Text = "Изменить" };

            //ContextMenu_OnEditClicked
            item.Command = new DelegateCommand(this.ContextMenu_OnEditClicked);
            menu.Items.Add(item);



            PopUP2.PopUpItem item2 = new PopUP2.PopUpItem() { Text = "Удалить" };
            //ContextMenu_OnDeleteClicked
            item2.Command = new DelegateCommand((args) =>
            {
                //if (linkHeader == null || MessageBox.Show(CommonResources.GenericConfirmation, CommonResources.LinkRemoving, (MessageBoxButton)1) != MessageBoxResult.OK)
                //    return;
                this.VM.DeleteLink(vm);
            });
            menu.Items.Add(item2);



            menu.ShowAt(element);
        }

        private void ContextMenu_OnEditClicked(object args)
        {
            LinkCreationUC sharePostUC = new LinkCreationUC(0);

            PopUpService statusChangePopup = new PopUpService
            {
                Child = sharePostUC
            };
            //sharePostUC.SendTap = (users, title) => {
            //    statusChangePopup.Hide();
            //    this.CreateChat(users, title);
            //};
            statusChangePopup.OverrideBackKey = true;
            statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            statusChangePopup.Show();
        }

        private void Border_Tapped_1(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            
        }
    }
}
