using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.UC;
using LunaVK.ViewModels;
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
using LunaVK.Core.Utils;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using Windows.Networking.Connectivity;
using LunaVK.Core.Framework;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Windows.System;
using Windows.Data.Xml.Dom;
using LunaVK.Common;
using Windows.Storage;

namespace LunaVK.Pages
{
    public sealed partial class DialogsConversationPage2 : PageBase
    {
        int SelectPeer = 0;
        private bool _updateConversaationAfterFail;
        private uint? group_id;

        public delegate void LoadedEvent(bool isLoaded);
        public static LoadedEvent OnIsLoaded;

        public DialogsConversationPage2()
        {
            this.InitializeComponent();

            this.Burger.DataContext = MenuViewModel.Instance;
            
            this._exListView.Loaded2 += (s, e) =>
            {
                DialogsViewModel.Instance.IsOnScreen = true;

                DialogsViewModel.Instance.SubscribedListView = this._exListView.GetListView;

                this.ucPullToRefresh.TrackListBox(this._exListView);

                if (CustomFrame.Instance.Header.IsVisible == true)
                    CustomFrame.Instance.Header.IsVisible = false;

                CustomFrame.Instance.Header.HideSandwitchButton = true;

                if (this.group_id.HasValue)
                    base.DataContext = new DialogsViewModel(this.group_id.Value);
                else
                {
                    base.DataContext = DialogsViewModel.Instance;//ConversationsViewModelTemp
                }

                this.VM.LoadingStatusUpdated += this.HandleLoadingStatusUpdated;

                this.SearchClosed = this.SearchClosedClicked;
                this.ServerSearch = this.VM.ServerSearch;


                if (this.SelectPeer!=0)
                    this.SelectConversation(this.SelectPeer);

                NetworkInformation.NetworkStatusChanged += this.NetworkInformation_NetworkStatusChanged;

                if (CustomFrame.Instance.IsDevicePhone == false)
                    this._refreshBtn.Visibility = Visibility.Visible;

                base.InitializeProgressIndicator();
                this._exListView.UpdteHookLoadingStatus();

                this.ProcessParametersRepository();
            };

            Window.Current.VisibilityChanged += this.Current_VisibilityChanged;

            this.detailed.BackCall = this.BackAction;

            this._detailsView.IsSelectedChanged += this._detailsView_IsSelectedChanged;
            this._detailsView.DetailsCommandBar = new CommandBar() { Visibility = Visibility.Collapsed };
            this.detailed.CmdBar = this._detailsView.DetailsCommandBar;

            if (Settings.StickersAutoSuggestEnabled)
            {
                
                this._exListView.Loaded2 += this._exListView_Loaded2;
            }

			Loaded += DialogsConversationPage2_Loaded;
            this.Unloaded += this.DialogsConversationPage2_Unloaded;
        }

		private void DialogsConversationPage2_Loaded(object sender, RoutedEventArgs e)
		{
            OnIsLoaded.Invoke(true);
		}

		private async void ProcessParametersRepository()
        {
            string parameterForIdAndReset2 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as string;
            if(parameterForIdAndReset2!=null)
            {
                StorageFolder pictureFolder = KnownFolders.SavedPictures;
                StorageFile file = await pictureFolder.GetFileAsync(parameterForIdAndReset2);
                OutboundPhotoAttachment a = await OutboundPhotoAttachment.CreateForUploadNewPhoto(file);

                List<IOutboundAttachment> ret = new List<IOutboundAttachment>();
                ret.Add(a);
                this.detailed.HandleAttachmentsAction(ret);
                this.detailed.UcNewMessage.ActivateSendButton(true);
            }
        }

        private DialogsViewModel VM
        {
            get { return base.DataContext as DialogsViewModel; }
        }

        private void _exListView_Loaded2(object sender, RoutedEventArgs e)
        {
            StickersAutoSuggestDictionary.Instance.EnsureDictIsLoadedAndUpToDate(false);
        }

        private void DialogsConversationPage2_Unloaded(object sender, RoutedEventArgs e)
        {
            OnIsLoaded.Invoke(false);
            if(StickersAutoSuggestDictionary.Instance.Count>0)
                StickersAutoSuggestDictionary.Instance.SaveState();
            StickersAutoSuggestDictionary.Instance.Clear();

            if(this.group_id.HasValue)
                Network.LongPollServerService.Instance.SwitchGroup(0);
        }

        private void _header_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is Grid)
            {
                this._exListView.GetInsideScrollViewer.ChangeView(0, 0, 1);
                e.Handled = true;
            }
        }

        public int UserOrCharId
        {
            get
            {
                if (this.detailed.DataContext == null)
                    return -1;

                return this.detailed.VM.conversation.peer.id;
            }
        }

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            /*
            if (e.NotificationType != NetworkNotificationType.InterfaceConnected)
                return;
            Execute.ExecuteOnUIThread(() =>
            {
                if (this.ConversationVM != null)
                    this.ConversationVM.EnsureConversationIsUpToDate(false, 0, null);
            }));
            */
            //Bug: если включать расшаривание интернета, то будет частое обновление списка диалогов
            //todo: возможно лучше проверять DialogsViewModel.Instance.IsOnScreen

            

            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            bool IsNetworkAvailable = (connectionProfile != null && (connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess || connectionProfile.IsWwanConnectionProfile));
            Execute.ExecuteOnUIThread(() =>
            {
                if (this.VM.InSearch)
                    return;

                if (IsNetworkAvailable)
                {
                    this._updateConversaationAfterFail = true;
                
                   this._exListView.Reload();
                }
                else
                {
                    this._ucTitle.SubTitle = "Ожидание сети...";
                }
            });
        }

        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {
            if (status == ProfileLoadingStatus.Reloading)
                this._ucTitle.SubTitle = "Обновление...";
            else if (status == ProfileLoadingStatus.ReloadingFailed)
                this._ucTitle.SubTitle = "Ожидание сети...";
            else
            {
                this._ucTitle.SubTitle = "";
                if(this._updateConversaationAfterFail)
                {
                    this._updateConversaationAfterFail = false;

                    if (this.UserOrCharId != -1)
                        this.detailed.HistoryVM.EnsureConversationIsUpToDate(false, 0);
                }
                
            }
        }

        public void SelectConversation(int peer_id)
        {
            this.SelectPeer = peer_id;

            this._detailsView.SelectedItemFast();

            ConversationWithLastMsg new_d = null;

            if (this.VM.Items.Count > 0)
            {
                new_d = this.VM.Items.FirstOrDefault((d) => d.conversation.peer.id == peer_id);
            }

            if (new_d == null)
            {
                new_d = new ConversationWithLastMsg();
                new_d.conversation = new VKConversation();
                new_d.conversation.peer = new VKConversation.ConversationPeer();
                new_d.conversation.peer.id = peer_id;
                new_d.conversation.can_write = new VKConversation.ConversationCanWrite();

                if (peer_id < 0)
                    new_d.conversation.peer.type = VKConversationPeerType.Group;
            }

            this.SelectConversation(new_d);
        }

        private void SelectConversation(ConversationWithLastMsg new_d)
        {
            if (this.VM.SubscribedListView == null)
            {
                this.VM.SubscribedListView = this._exListView.GetListView;
            }

            if (this.VM.SubscribedListView != null)
                this.VM.SubscribedListView.SelectedItem = new_d;
            
            this.detailed.SetData(new_d, this.VM.InSearch,this.group_id);

#region Toast history manipulation
            var history = Windows.UI.Notifications.ToastNotificationManager.History;
            var toastList = history.GetHistory();
            foreach (var t in toastList)
            {
                XmlDocument toastXML = t.Content;
                XmlNodeList l = toastXML.GetElementsByTagName("toast");
                //Debug.Assert(l.Count > 0);
                if (l.Count > 0)
                {
                    //Debug.Assert(l[0].Attributes.Count > 0);
                    if (l[0].Attributes.Count > 0)
                    {
                        IXmlNode node = l[0].Attributes[0];
                        string launch = node.InnerText;

                        //Это с пуш с сообщением?
                        //sound=default&_genSrv=626231&uid=2000000017&badge=7&sandbox=0&msg_id=42928&push_id=chat_2000000017_42928 // сообщение из беседы
                        //badge=2&sound=default&log_date=1584005544&_genSrv=626626&msg_id=86029&sandbox=0&uid=375988312&push_id=msg_375988312_86029

                        Dictionary<string, string> paramDict = launch.ParseQueryString();
                        if (paramDict.ContainsKey("msg_id") && paramDict.ContainsKey("uid"))
                        {
                            int toast_peer_id = int.Parse(paramDict["uid"]);
                            t.Tag = toast_peer_id.ToString();//это не работает
                        }
                    }
                }
            }

            int peer_id = new_d.conversation.peer.id;
            history.Remove(peer_id.ToString());
#endregion
        }

        /*
                private void _detailsView_Loaded(object sender, RoutedEventArgs e)
                {
                    if(!this._masterRoot.Children.Contains(DialogsConversationPage2.ConversationsUCInstance))//BugFix: навигация на ту же страницу
                        this._masterRoot.Children.Insert(0, DialogsConversationPage2.ConversationsUCInstance);
                }
        */
        private void _detailsView_IsSelectedChanged(object sender, bool e)
        {
            if(e==false)
            {
                this.detailed.SetData(null);
                if (this.VM.SubscribedListView != null)
                    this.VM.SubscribedListView.SelectedItem = null;
            }
        }

        private bool NeedUpdateAfterMinimize;

        private void Current_VisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            if (e.Visible)
            {
                if (this.NeedUpdateAfterMinimize)
                {
                    this.VM.LoadDownAsync(true);
                    this.NeedUpdateAfterMinimize = false;
                }
            }
            else
            {
                if(!this.VM.InSearch)
                    this.NeedUpdateAfterMinimize = true;
            }
        }
        


        

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("SelectPeer"))
            {
                this.SelectPeer = (int)pageState["SelectPeer"];
            }
            else
            {
                if (navigationParameter != null)
                {
                    Dictionary<string, int> QueryString = navigationParameter as Dictionary<string, int>;
                    if(QueryString.ContainsKey("PeerId"))
                        this.SelectPeer = (int)QueryString["PeerId"];
                    if (QueryString.ContainsKey("GroupId"))
                        this.group_id = (uint)QueryString["GroupId"];
                }
            }
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            if (this.VM.SubscribedListView != null)
            {
                if (this.VM.SubscribedListView.SelectedItem is ConversationWithLastMsg new_d)
                    pageState["SelectPeer"] = new_d.conversation.peer.id;
            }

            this.VM.LoadingStatusUpdated -= this.HandleLoadingStatusUpdated;
            this.VM.IsOnScreen = false;
            if (this.VM.SubscribedListView != null)
            {
                this.VM.SubscribedListView.SelectedItem = null;
                this.VM.SubscribedListView = null;
            }
            CustomFrame.Instance.Header.IsVisible = true;
            CustomFrame.Instance.Header.HideSandwitchButton = false;
            Window.Current.VisibilityChanged -= this.Current_VisibilityChanged;
            NetworkInformation.NetworkStatusChanged -= this.NetworkInformation_NetworkStatusChanged;
            
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

        private void ItemDialogUC_ShowMenuTap(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            this.ShowMenu(element);
        }

        private void ItemDialogUC_BackTap(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as ConversationWithLastMsg;
            //
            if(this.detailed.VM == vm)
                return;
            //
            this._detailsView.SelectedItem = true;
            this.SelectConversation(vm);
        }

        private void ItemDialogUC_AvatrTap(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as ConversationWithLastMsg;
            if (vm.conversation.peer.type == VKConversationPeerType.User)
            {
                this.VM.SubscribedListView = null;
                NavigatorImpl.Instance.NavigateToProfilePage(vm.conversation.peer.local_id);
            }
            else if(vm.conversation.peer.type == VKConversationPeerType.Group)
            {
                this.VM.SubscribedListView = null;
                NavigatorImpl.Instance.NavigateToProfilePage(vm.conversation.peer.id);
            }
            else if (vm.conversation.peer.type == VKConversationPeerType.Chat)
            {
                this.VM.SubscribedListView = null;
                NavigatorImpl.Instance.NavigateToChatEditPage(vm.conversation.peer.local_id);
            }
        }

        private void Burger_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CustomFrame.Instance.OpenCloseMenu();
            e.Handled = true;
        }

        /// <summary>
        /// Нажали на верхнюю стрелку
        /// </summary>
        public void BackAction()
        {
            this._detailsView.SelectedItem = false;
        }

        private void ShowMenu(FrameworkElement element)
        {
            var vm = element.DataContext as ConversationWithLastMsg;
            
            PopUP2 menu = new PopUP2();
            PopUP2.PopUpItem item = new PopUP2.PopUpItem();

            item.Text = LocalizedStrings.GetString(vm.conversation.AreDisabledNow ? "TurnOnNotifications" : "TurnOffNotifications");

            item.Command = new DelegateCommand((args) => {
                this.VM.SilentUnsilent(vm);
            });
            menu.Items.Add(item);

            PopUP2.PopUpItem item2 = new PopUP2.PopUpItem() { Text = LocalizedStrings.GetString("Delete") };
            item2.Command = new DelegateCommand((args) => {
                this.VM.DeleteConversation(vm);
            });
            menu.Items.Add(item2);

            menu.ShowAt(element);
        }

        private void appBarButtonAdd_Click(object sender, TappedRoutedEventArgs e)
        {
            CreateChatUC sharePostUC = new CreateChatUC();

            PopUpService statusChangePopup = new PopUpService
            {
                Child = sharePostUC
            };
            sharePostUC.SendTap = (users, title)=> {
                statusChangePopup.Hide();
                this.CreateChat(users, title);
            };
            statusChangePopup.OverrideBackKey = true;
            statusChangePopup.AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed;
            statusChangePopup.Show();
        }

        private void CreateChat(IReadOnlyList<int> users, string title)
        {
            MessagesService.Instance.CreateChat(users, title, (result) => { });
        }

        protected override void HandleOnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            bool result = this.detailed.UcNewMessage.HidePanel();
            if (result)
                e.Cancel = true;
        }

        private void appBarButtonRefresh_Click(object sender, TappedRoutedEventArgs e)
        {
            this.VM.LoadDownAsync(true);
        }

        

#region Search
        private void appBarButtonSearch_Click(object sender, TappedRoutedEventArgs e)
        {
            this.ActivateSearch(true);


            this.VM.InSearch = true;
            this.OldSource = this._exListView.ItemsSource;
            this._exListView.ItemsSource = this.VM.SearchItems;
        }

        private void Timer_Tick(object sender, object e)
        {
            (sender as DispatcherTimer).Stop();
            this.ServerSearch?.Invoke(this.searchTextBox.Text);
        }

        DispatcherTimer searchTimer;
        public Action<string> ServerSearch;
        public Action<string> LocalSearch;
        public Action SearchClosed;
        private object OldSource;

        private void SearchTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrEmpty(textBox.Text))
            {
                if (this.searchTimer.IsEnabled)
                    this.searchTimer.Stop();

                this.ServerSearch?.Invoke(textBox.Text);
            }
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.searchTimer.IsEnabled)
                this.searchTimer.Stop();

            string text = (sender as TextBox).Text;

            if (!string.IsNullOrEmpty(text))
                this.searchTimer.Start();

            this.LocalSearch?.Invoke(text);
        }

        private void CloseSearch_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.ActivateSearch(false);
        }

        public void ActivateSearch(bool status, bool noCallback = false, /*bool moreOptions = false,*/ string searchQ = "")
        {
            if (status)
            {
                //if (moreOptions)
                //    this._moreSearchBrd.Visibility = Visibility.Visible;

                this.searchPanel.Visibility = Visibility.Visible;
                this.searchTextBox.Focus(FocusState.Keyboard);
                if (this.searchTimer == null)
                {
                    this.searchTimer = new DispatcherTimer();
                    this.searchTimer.Interval = TimeSpan.FromSeconds(1);
                    this.searchTimer.Tick += Timer_Tick;
                }
                
                this.searchTextBox.Text = searchQ;
                this.searchTextBox.TextChanged += this.searchTextBox_TextChanged;
            }
            else
            {
                //this._moreSearchBrd.Visibility = 
                this.searchPanel.Visibility = Visibility.Collapsed;

                if (this.SearchClosed != null && !noCallback)
                    this.SearchClosed();

                if (this.searchTimer != null)
                {
                    if (this.searchTimer.IsEnabled)
                        this.searchTimer.Stop();

                    this.searchTimer.Tick -= Timer_Tick;
                    this.searchTimer = null;
                }

                this.searchTextBox.TextChanged -= this.searchTextBox_TextChanged;
                this.searchTextBox.Text = "";
            }
        }

        private void SearchClosedClicked()
        {
//            DialogsConversationPage2.ConversationsUCInstance.ExtendedListView.ItemsSource = this.OldSource;
            this._exListView.ItemsSource = this.OldSource;
            this.VM.InSearch = false;
        }
#endregion
    }
}
