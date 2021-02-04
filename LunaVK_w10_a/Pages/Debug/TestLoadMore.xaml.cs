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
using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using LunaVK.Core.Library;

namespace LunaVK
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestLoadMore : Page
    {
//        public ObservableCollection<IMsgItem> Items { get; private set; }
        public TestLoadMore()
        {
            base.DataContext = this;
 //           this.Items = new ObservableCollection<IMsgItem>();
            this.InitializeComponent();
            //var temp = API.messages.getConversationsById({peer_ids:460389}); return temp.items[0].in_read;

            this.Items = new ObservableCollection<string>();
        }


        public ObservableCollection<string> Items { get; private set; }

        private int lessOffs;
        private bool CanLoadMore;
        private bool CanLoadLess;

        //less
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["count"] = "3";
            parameters["peer_id"] = "74220393";

            parameters["offset"] = lessOffs.ToString();

 //           parameters["start_message_id"] = this.TB.Text;
            //else
            //    parameters["fields"] = "first_name,last_name,first_name_acc,last_name_acc,online,online_mobile,photo_100,photo_50,is_messages_blocked,last_seen,sex,push_settings,domain";
            parameters["extended"] = "1";
            /*
            VKResponse<VKGetMessagesHistoryObject> temp = await RequestsDispatcher.GetResponse<VKGetMessagesHistoryObject>("messages.getHistory", parameters);

            if (temp.error.error_code == LunaVK.Core.Enums.VKErrors.None)
            {
 //               this.LOL.Text = string.Format("last_message_id {0}, in_read {1}", temp.response.conversations[0].last_message_id, temp.response.conversations[0].in_read);

                lessOffs += 3;
                foreach (var item in temp.response.items)
                {
//                    this.Items.Insert(0,item);
                }

//                CanLoadMore = this.Items.Max((m) => (m as VKMessage).id) < temp.response.conversations[0].last_message_id;
                CanLoadLess = temp.response.items.Count == 3;
                System.Diagnostics.Debug.WriteLine(CanLoadMore + " " + CanLoadLess);
            }*/
        }
        //more
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["count"] = "3";
            parameters["peer_id"] = "74220393";
//            if (this.Items.Count > 0)
//            {
//                int c = this.Items.Count((i) => { return i is VKMessage; });
//                parameters["offset"] = c.ToString();
//            }

//            parameters["start_message_id"] = this.TB.Text;
            //else
            //    parameters["fields"] = "first_name,last_name,first_name_acc,last_name_acc,online,online_mobile,photo_100,photo_50,is_messages_blocked,last_seen,sex,push_settings,domain";
            parameters["extended"] = "1";

            /*
            VKResponse<VKGetMessagesHistoryObject> temp = await RequestsDispatcher.GetResponse<VKGetMessagesHistoryObject>("messages.getHistory", parameters);

            if (temp.error.error_code == LunaVK.Core.Enums.VKErrors.None)
            {
                
 //               this.LOL.Text = string.Format("last_message_id {0}, in_read {1}", temp.response.conversations[0].last_message_id, temp.response.conversations[0].in_read);

                foreach(var item in temp.response.items)
                {
//                    this.Items.Add(item);
                }

 //               CanLoadMore = this.Items.Max((m) => (m as VKMessage).id) < temp.response.conversations[0].last_message_id;
                CanLoadLess = temp.response.items.Count == 3;
                System.Diagnostics.Debug.WriteLine(CanLoadMore + " " + CanLoadLess);
            }*/
        }
        //clear
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
//            this.Items.Clear();
        }




        //+1
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //this._listView.Items.Add(this._listView.Items.Count);
            this.Items.Add(this.Items.Count.ToString());
        }

        //Clear
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //this._listView.Items.Clear();
            this.Items.Clear();
        }

        //+5
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            //for(int i=0;i<5;i++)
            //    this._listView.Items.Add(this._listView.Items.Count);
            for (int i = 0; i < 5; i++)
                this.Items.Add(this.Items.Count.ToString());
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            List<string> temp = new List<string>();
            for (int i = 0; i < 10; i++)
                temp.Add(i.ToString());

            base.DataContext = null;
            this.Items = new ObservableCollection<string>(temp);
            base.DataContext = this;
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            List<string> temp = new List<string>();
            for (int i = 0; i < 20; i++)
                temp.Add(i.ToString());

            base.DataContext = null;
            this.Items = new ObservableCollection<string>(temp);
            base.DataContext = this;
        }
    }
}
