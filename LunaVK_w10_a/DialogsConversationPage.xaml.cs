using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using LunaVK.UC;
using LunaVK.Core.DataObjects;
//using LunaVK.Network.DataVM;
using LunaVK.Framework;
using LunaVK.Core.Utils;
using LunaVK.Core.Enums;
using LunaVK.Core;
using LunaVK.Core.Library;
using LunaVK.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace LunaVK
{
    public sealed partial class DialogsConversationPage : PageBase
    {        
        UC.PopUP popForFilter = null;
        

        /// <summary>
        /// Мы в режиме выбора диалога?
        /// </summary>
        private bool InDialogSelectionMode = false;

 //       private List<VKMessage> selectedMsgs = new List<VKMessage>();
        //CollectionViewSource groupedItemsViewSource;
        
        
        public DialogsConversationPage()
        {
            this.InitializeComponent();
            this.Loaded += (s, e) =>
            {
                //this.CFrame.HeaderWithMenu.SetTitle(LocalizedStrings.GetString("Menu_Messages/Title"));
                //this.CFrame.HeaderWithMenu.EnableTitleOption(true);
                //this.CFrame.HeaderWithMenu.TitlePanel.Tapped += this._header_OnFilterTap;

                this.ucPullToRefresh.TrackListBox(this.DetailsView.MasterList);//this.CFrame.HeaderWithMenu.PullToRefresh.TrackListBox(this.DetailsView.MasterList);
                if (this.CFrame.HeaderWithMenu.IsVisible==true)
                    this.CFrame.HeaderWithMenu.IsVisible = false;

                this.LetsLoad(SelectPeer);

                if(this.CFrame.IsDevicePhone)
                {
                    this._headerOffs.Children.Remove(this._refreshBtn);
                }
                else
                {
                    this._refreshBtn.Visibility = Visibility.Visible;
                }
            };
            
            this.DataContext = DialogsViewModel.Instance;

            this.DetailsView.Loaded += DetailsView_Loaded;
            this.DetailsView.ViewStateChanged += DetailsView_ViewStateChanged;
            this.DetailsView.SelectionChanged += DetailsView_SelectionChanged1;

            Window.Current.VisibilityChanged += Current_VisibilityChanged;

            this.detailed.BackCall = this.BackAction;

            this.DetailsView.DetailsCommandBar = new CommandBar() { Visibility = Visibility.Collapsed };
            this.detailed.CmdBar = this.DetailsView.DetailsCommandBar;
        }

        private bool NeedUpdateAfterMinimize;

        private void Refresh_Tpped(object sender, TappedRoutedEventArgs e)
        {
            this.DetailsView.Reload();
        }

        void Current_VisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            if (e.Visible)
            {
                if (this.NeedUpdateAfterMinimize)
                {
                    DialogsViewModel.Instance.Reload();
                    this.NeedUpdateAfterMinimize = false;
                }
            }
            else
            {
                this.NeedUpdateAfterMinimize = true;
            }
        }

        private void DetailsView_SelectionChanged1(object sender, SelectionChangedEventArgs e)
        {
            if(DetailsView.SelectedItem==null)
                this.CFrame.HeaderWithMenu.HideSandwitchButton = false;
            /*else
            {

                if (detailed.selectedMsgs.Count > 0)
                {

                    OutboundForwardedMessages at = new OutboundForwardedMessages(detailed.selectedMsgs.ToList<VKMessage>());
                    this.detailed.ConversationVM.Attachments.Add(at);
                    //detailed.selectedMsgs.Clear();
                }
            }*/
        }
        
        private void DetailsView_ViewStateChanged(object sender, MasterDetailsViewState e)
        {
            if (e == MasterDetailsViewState.Details)
                this.CFrame.HeaderWithMenu.HideSandwitchButton = true;
        }

        private void DetailsView_Loaded(object sender, RoutedEventArgs e)
        {
            MasterDetailsView v = sender as MasterDetailsView;

            v.MasterList.PreventScroll = true;
            v.MasterList.NeedReload = false;

            var lv = v.FindChild<ExtendedListView2>() as ExtendedListView2;
            if(lv !=null)
            {
                DialogsViewModel.Instance.SubscribedListView = lv.GetListView;
            }
        }


        void _header_OnFilterTap(object sender, TappedRoutedEventArgs e)
        {
            Point point = e.GetPosition(null);

            if (this.popForFilter == null)
            {
                this.popForFilter = new UC.PopUP();
                this.popForFilter.ItemTapped += _filterPopUp;
            }

            this.popForFilter.ClearItems();
            this.popForFilter.AddItem(0, "Все", "");
            this.popForFilter.AddItem(1, "Важные", "", false);
            this.popForFilter.AddItem(2, "Непрочитанные", "");
            this.popForFilter.Show(point);
            e.Handled = true;
        }

        private void _filterPopUp(object sender, int i)
        {
            this.CFrame.HeaderWithMenu.SetTitle(this.popForFilter.GetTitle(i));
 //           DialogsViewModel.Instance.SetDialogsSource(i);
        }

        

        
        

        

        protected override void HandleOnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (this.detailed.UcNewMessage.HidePanel())
            {
                e.Cancel = true;
                return;
            }
            int i = 0;
        }

        private bool DoNotClearAttach = false;
        int SelectPeer = 0;

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            if (this.CFrame.HeaderWithMenu!=null)
                this.CFrame.HeaderWithMenu.IsVisible = false;
            
            if (e.Parameter is PagesParams)
            {
                PagesParams parameter = e.Parameter as PagesParams;
                this.SelectPeer = parameter.peer_id;
            }
            else
            {
 //               DialogsViewModel.Instance.HistoryVM = new DialogHistoryViewModel();
            }

            this.DataContext = DialogsViewModel.Instance;
            /*
            this.ucNewMessage.DataContext = DialogsViewModel.Instance.HistoryVM;
            this.ArrowDownGrid.DataContext = DialogsViewModel.Instance.HistoryVM;
            */
            if (e.Parameter is IOutboundAttachment)
            {
                this.DoNotClearAttach = true;
 //               this.ConversationVM.Attachments.Add(e.Parameter as IOutboundAttachment);
            }
            
            DialogsViewModel.Instance.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;
        }


        private async void LetsLoad(int select_peer = 0)
        {
            await DialogsViewModel.Instance.Reload();
            if (select_peer!=0)
            {
                await System.Threading.Tasks.Task.Delay(150);
                ConversationWithLastMsg d = DialogsViewModel.Instance.Dialogs.FirstOrDefault((dialog) => { return dialog.conversation.peer.id == select_peer; });
                if (d != null)
                {
                    DialogsViewModel.Instance.SubscribedListView.SelectedItem = d;
                }
                else
                {//todo:навигация к тому, кого нет в списке диалогов.
                    ConversationWithLastMsg new_d = new ConversationWithLastMsg();
                    new_d.conversation = new VKConversation();
                    new_d.conversation.peer = new VKConversation.ConversationPeer();
                    new_d.conversation.peer.id = select_peer;
                }
            }

            //this.DetailsView.MasterList.NeedReload = true;
            this.DetailsView.MasterList.PreventScroll = false;
        }

        

        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {/*
            if (status == ProfileLoadingStatus.Loaded)
            {
                this.ucNewMessage.Visibility = Visibility.Visible;
//                this.ConversationVM.SetReadStatusIfNeeded(100);

                //ScrollToUnreadItem
                var item = this.ConversationVM.Items.LastOrDefault((m) => m is VKMessage && (m as VKMessage).@out == VKMessageType.Received && (m as VKMessage).read_state == false);
                if (item != null)
                {
                    this.eListView.GetListView.ScrollIntoView(item, ScrollIntoViewAlignment.Leading); // Проматываем камеру к последнему непрочитанному сообщению
                }
            }
            else
            {
                this.ucNewMessage.Visibility = Visibility.Collapsed;
            }*/
        }
        
        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            DialogsViewModel.Instance.SubscribedListView = null;
            this.CFrame.HeaderWithMenu.IsVisible = true;
            DialogsViewModel.Instance.LoadingStatusUpdated -= this.HandleLoadingStatusUpdated;
            /*          DialogsConversationPage.ConversationsUCInstance.ConversationSelected = null;
                      
                      this.CFrame.HeaderWithMenu.TitlePanel.Tapped -= this._header_OnFilterTap;

                      this.ConversationVM.Attachments.CollectionChanged -= this.Attachments_CollectionChanged;*/

            Window.Current.VisibilityChanged -= Current_VisibilityChanged;
        }
        /*
        void ConversationsUCInstance_ConversationSelected(ConversationWithLastMsg conversation)
        {
            //this.ConversationVM.Attachments.Clear();

 //           this.ConversationVM.SwitchConversation(conversation, !this.DoNotClearAttach);
//            this.ucNewMessage.ClosePanel();
            this.DoNotClearAttach = false;

 //           this.EditMsgId = 0;
//            this.ucNewMessage.ActivateSendButton(false);

            if (this.InDialogSelectionMode)
            {
                OutboundForwardedMessages at = new OutboundForwardedMessages(this.selectedMsgs.ToList<VKMessage>());
 //               this.ConversationVM.Attachments.Add(at);
                this.InDialogSelectionMode = false;
//                this.eListView.GetListView.SelectionMode = ListViewSelectionMode.None;
                this.selectedMsgs.Clear();
            }
            
        }
        */
        private CustomFrame CFrame
        {
            get { return Window.Current.Content as CustomFrame; }
        }
        
       
        private void DetailsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DialogsViewModel.Instance.CurrentConversation = (sender as MasterDetailsView).SelectedItem as ConversationWithLastMsg;
            //if(DialogsViewModel.Instance.CurrentConversation!=null)
            //    DialogsViewModel.Instance.CurrentConversation.HistoryVM.Reload();

        }

        private void Burger_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.CFrame.OpenCloseMenu();
        }

        public void BackAction()
        {
            this.DetailsView.OnBackRequested(null, null);
        }

        private void ItemDialogUC_Holding(object sender, HoldingRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                FrameworkElement element = sender as FrameworkElement;
                this.ShowMenu(element);
            }
        }

        private void ItemDialogUC_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                return;

            FrameworkElement element = sender as FrameworkElement;
            this.ShowMenu(element);
        }

        private void ShowMenu(FrameworkElement element)
        {
            var vm = element.DataContext as ConversationWithLastMsg;
            
            MenuFlyout menu = new MenuFlyout();
            
            MenuFlyoutItem item = new MenuFlyoutItem();

            if (vm.conversation.push_settings == null || vm.conversation.push_settings.sound)
                item.Text = "Отключить уведомления";
            else
                item.Text = "Включить уведомления";

            item.Command = new DelegateCommand((args)=> {
                DialogsViewModel.Instance.SilentUnsilent(vm);
            });
            menu.Items.Add(item);

            MenuFlyoutItem item2 = new MenuFlyoutItem() { Text = "Удалить" };
            item2.Command = new DelegateCommand((args) => {
                DialogsViewModel.Instance.DeleteConversation(vm);
            });
            menu.Items.Add(item2);

            menu.ShowAt(element);
        }
    }
}
