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

using LunaVK.ViewModels;
using LunaVK.Framework;
using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.UC;
using LunaVK.Library;

namespace LunaVK
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ChatEditPage : PageBase
    {
        public ChatEditPage()
        {
            this.InitializeComponent();
            base.Title = LocalizedStrings.GetString("Chat");
        }

        private ChatEditViewModel VM
        {
            get
            {
                return this.DataContext as ChatEditViewModel;
            }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            int id = (int)e.Parameter;

            base.HandleOnNavigatedTo(e);

            this.DataContext = new ChatEditViewModel(id);
            //this.VM.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;
            this.VM.LoadData();
        }

        private void ExcludeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var vm = (sender as FrameworkElement).DataContext as ChatInfo.ChatMember;
            //if (dataContext == null || MessageBox.Show(CommonResources.GenericConfirmation, CommonResources.ChatMemberExcluding, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
            //    return;
            this.VM.ExcludeMember(vm);
        }

        private void ConversationMaterials_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            Library.NavigatorImpl.Instance.NavigateToConversationMaterials(this.VM.ChatId + 2000000000);
        }

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var vm = (sender as FrameworkElement).DataContext as VKUser;
            NavigatorImpl.Instance.NavigateToProfilePage(vm.Id);
        }

        private void LeaveButton_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            //MessageBox.Show(CommonResources.GenericConfirmation, CommonResources.ChatMemberExcluding, MessageBoxButton.OKCancel) != MessageBoxResult.OK
            this.VM.LeaveChat();
        }


        private void NotificationsSound_OnClicked(object sender, TappedRoutedEventArgs e)
        {
            this.VM.SwitchNotificationsSoundMode();
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
                return;
            //((Control)this).Focus();
            if (!(this.TitleBox.Text != this.VM.Title))
                return;
            this.VM.ChangeTitle(this.TitleBox.Text);
        }

        private void TitleBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //this.TextBoxPanel.IsOpen = false;
            if (!(this.TitleBox.Text != this.VM.Title))
                return;
            this.VM.ChangeTitle(this.TitleBox.Text);
        }

        private void AvatarUC_Tapped(object sender, TappedRoutedEventArgs e)
        {
            /*
             * <toolkit:ContextMenuService.ContextMenu>
                                <toolkit:ContextMenu Visibility="{Binding IsPhotoMenuEnabled, Converter={StaticResource BoolToVisibilityConverter}}"                                         
                                                     
                                                     
                                                     IsZoomEnabled="False">
                                    <toolkit:MenuItem Header="{Binding Path=LocalizedResources.Settings_ChangePhoto, Source={StaticResource LocalizedStrings}}"
                                                      Click="ChangePhoto_OnClicked"/>
                                    <toolkit:MenuItem Header="{Binding Path=LocalizedResources.DeletePhoto, Source={StaticResource LocalizedStrings}}"
                                                      Click="DeletePhoto_OnClicked"/>
                                </toolkit:ContextMenu>
                            </toolkit:ContextMenuService.ContextMenu>
                            */
            //                if(!this.VM.IsPhotoMenuEnabled)
            //{
            //    return;

            //}
            FrameworkElement element = sender as FrameworkElement;

            PopUP2 menu = new PopUP2();
            PopUP2.PopUpItem item = new PopUP2.PopUpItem() { Text = "Settings_ChangePhoto" };
            item.Command = new DelegateCommand((args) => {
                this.ChangePhoto_OnClicked();
            });
            menu.Items.Add(item);


            if (this.VM.IsPhotoMenuEnabled)
            {
                PopUP2.PopUpItem item2 = new PopUP2.PopUpItem() { Text = "DeletePhoto" };
                item2.Command = new DelegateCommand((args) =>
                {
                    this.DeletePhoto_OnClicked();
                });
                menu.Items.Add(item2);
            }

            menu.ShowAt(element);
        }

        private void ChangePhoto_OnClicked()
        {
            //NavigatorImpl.Instance.NavigateToPhotoPickerPhotos(1, true, false);
        }

        private void DeletePhoto_OnClicked()
        {
            //if (MessageBox.Show(CommonResources.GenericConfirmation, CommonResources.DeleteOnePhoto, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
            //    return;
            this.VM.DeletePhoto();
        }
    }
}
