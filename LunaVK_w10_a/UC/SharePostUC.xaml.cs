using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

using LunaVK.ViewModels;
using LunaVK.Core.Library;
using Windows.ApplicationModel.DataTransfer;
using LunaVK.Framework;
using System.Collections.ObjectModel;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.UC.PopUp;
using LunaVK.Core.Enums;
using LunaVK.Core;

namespace LunaVK.UC
{
    public sealed partial class SharePostUC : UserControl
    {
        private WallService.RepostObject _type;
        private int _ownerId;
        private uint _itemId;
        private string _accessToken;
        private string _link;
        public Action Done;
        public Action<IReadOnlyList<ConversationWithLastMsg>> SendTap;


        public SharePostUC(string title, WallService.RepostObject type, int owner, uint id, string access_token = "", string link = "")
        {
            this._type = type;
            this._ownerId = owner;
            this._itemId = id;
            this._accessToken = access_token;
            this._link = link;

            this.InitializeComponent();
            this.Loaded += SharePostUC_Loaded;
            this.Unloaded += SharePostUC_Unloaded;

            this._ucTitle.Title = LocalizedStrings.GetString("ShareWallPost_Share/Text") + " " + title;
        }
        
        private bool _optionsHiden;

        public void HideOptions()
        {
            //this._bottomPanel.Height = 50;
            this._optionsHiden = true;
            this._panelButtons.Visibility = Visibility.Collapsed;
            this._textBox.Visibility = Visibility.Collapsed;
            this._panelButtons.Items.Clear();
        }

        void SharePostUC_Unloaded(object sender, RoutedEventArgs e)
        {
            this._listConversations.SelectionChanged -= itemsControl_SelectionChanged;
        }

        void SharePostUC_Loaded(object sender, RoutedEventArgs e)
        {
            this._listConversations.ItemsSource = DialogsViewModel.Instance.Items;
            if (DialogsViewModel.Instance.Items.Count == 0)
            {
                DialogsViewModel.Instance.LoadDownAsync();
                DialogsViewModel.Instance.LoadingStatusUpdated += this.OnLoadingStatusUpdated;

                this.progress.Visibility = Visibility.Visible;
            }
            
            this._listConversations.SelectionChanged += this.itemsControl_SelectionChanged;
            //if(!this._optionsHiden)
                VisualStateManager.GoToState(this, "DeactivateSend", false);
        }
        
        private void OnLoadingStatusUpdated(ProfileLoadingStatus status)
        {
            this.progress.Visibility = Visibility.Collapsed;
        }

        private void itemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;

            foreach(var selected in e.AddedItems)
            {
                ListViewItem item = lv.ContainerFromItem(selected) as ListViewItem;
                VisualStateManager.GoToState(item, "Selected", true);
            }

            foreach (var unselected in e.RemovedItems)
            {
                ListViewItem item = lv.ContainerFromItem(unselected) as ListViewItem;
                VisualStateManager.GoToState(item, "Unselected", true);
            }

            //if(!this._optionsHiden)
            //    this.subButtons.Visibility = lv.SelectedItems.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

            //this._button.IsEnabled = lv.SelectedItems.Count > 0;

            
            VisualStateManager.GoToState(this, lv.SelectedItems.Count > 0 ? "ActivateSend" : "DeactivateSend", true);



            if (lv.SelectedItems.Count > 0)
            {
                List<string> list = new List<string>();
                foreach (var item in lv.SelectedItems)
                {
                    ConversationWithLastMsg conversation = (ConversationWithLastMsg)item;
                    list.Add(conversation.Title);

                    if (list.Count >= 2)
                        break;
                }

                string temp = string.Join(", ", list);

                if (lv.SelectedItems.Count > list.Count)
                    temp += (" и ещё " + (lv.SelectedItems.Count - list.Count));

                this._ucTitle.SubTitle = temp;
            }
            else
            {
                this._ucTitle.SubTitle = "";
            }




        }

        private string MediaId
        {
            get
            {
                if (this._ownerId == 0 && this._itemId == 0)
                    return "";

                string media = string.Format("{0}{1}_{2}", this._type, this._ownerId, this._itemId);
                if (!string.IsNullOrEmpty(this._accessToken))
                    media += ("_"+this._accessToken);
                return media;
            }
        }

        private string Link
        {
            get
            {
                string temp = "https://";
                if (CustomFrame.Instance.IsDevicePhone)
                    temp += "m.";
                temp += "vk.com/";
                if (string.IsNullOrEmpty(this._link))
                    temp += this.MediaId;
                else
                    temp += this._link;
                return temp;
            }
        }
        
        private ObservableCollection<VKGroup> Groups { get; set; }
        //private bool _inWork;

        private void SubButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;

            switch (lv.SelectedIndex)
            {
                case 0:
                    {
                        //На своей странице
                        WallService.Instance.Repost(this._ownerId, this._itemId, this._textBox.Text, this._type, 0, null);
                        this.Done?.Invoke();
                        break;
                    }
                case 1:
                    {
                        //На странице сообщества

                        this.progress.Visibility = Visibility.Visible;

                        VisualStateManager.GoToState(this, "ActiveGroups", true);
                        
                        if (this.Groups == null)
                            this.Groups = new ObservableCollection<VKGroup>();
                        this._listGroups.ItemsSource = this.Groups;

                        this._listGroups.SelectionChanged += this._listGroups_SelectionChanged;

                        GroupsService.Instance.GetUserGroups(0, 0, 30, "editor", (result) =>
                        {
                            Execute.ExecuteOnUIThread(() =>
                            {
                                if (result.error.error_code== VKErrors.None)
                                {
                                
                                        foreach(var item in result.response.items)
                                        {
                                            this.Groups.Add(item);
                                        }
                                    
                                
                                }
                                this.progress.Visibility = Visibility.Collapsed;
                            });
                        });
                        break;
                    }
                case 2:
                    {
                        //Открыть QR-код
                        this.Done?.Invoke();

                        BarcodeUC share = new BarcodeUC(this.Link);
                        PopUpService popUp = new PopUpService { Child = share, OverrideBackKey = true, AnimationTypeChild = PopUpService.AnimationTypes.SlideInversed };
                        popUp.Show();


                        break;
                    }
                case 3:
                    {
                        //Скопировать ссылку

                        DataPackage dataPackage = new DataPackage();
                        dataPackage.SetText(this.Link);
                        Clipboard.SetContent(dataPackage);
                        
                        break;
                    }
                case 4:
                    {
                        //Добавить в историю
                        this.Done?.Invoke();
                        break;
                    }
                case 5:
                    {
                        //Ещё
                        DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                        dataTransferManager.DataRequested += DataTransferManager_DataRequested;
                        DataTransferManager.ShowShareUI();
                        this.Done?.Invoke();
                        break;
                    }
            }


            lv.SelectionMode = ListViewSelectionMode.None;
            lv.SelectionMode = ListViewSelectionMode.Single;
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            
            args.Request.Data.SetText(this.Link);
            args.Request.Data.Properties.Title = this._ucTitle.Title;
            //args.Request.Data.Properties.Description = "Поделиться сылкой :)";

            sender.DataRequested -= DataTransferManager_DataRequested;
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            if(this.SendTap!=null)
            {
                var list = this._listConversations.SelectedItems.Select(item => item as ConversationWithLastMsg).ToList();
                this.SendTap.Invoke(list);
                return;
            }

            this._listConversations.SelectionChanged -= this.itemsControl_SelectionChanged;//BugFix: после отправки нам не надо выделять элементы
            this._listGroups.SelectionChanged -= this._listGroups_SelectionChanged;

            if (this._listConversations.SelectedItems.Count > 0)
            {
                var list = this._listConversations.SelectedItems.Select(item => item as ConversationWithLastMsg).ToList();

                if (!string.IsNullOrEmpty(this._link))
                    this._textBox.Text = this.Link;
                DialogsViewModel.Instance.MultipleSend(list, this._textBox.Text, this.MediaId);
            }
            else if(this._listGroups.SelectedItems.Count > 0)
            {
                VKGroup group = this._listGroups.SelectedItem as VKGroup;
                WallService.Instance.Repost(this._ownerId, this._itemId, this._textBox.Text, this._type, group.id, (result)=> {
                    if(result.error.error_code == Core.Enums.VKErrors.None)
                    {

                    }
                });
            }
            this.Done?.Invoke();
        }

        private void _listGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           VisualStateManager.GoToState(this,  "ActivateSend" , false);
        }
    }
}
