using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.Pages.Debug
{
    public sealed partial class TestListGroup : Page
    {
        private ConversationWithLastMsg conversation;

        public TestListGroup()
        {
            this.InitializeComponent();
            this.Loaded += TestListGroup_Loaded;
        }

        private void TestListGroup_Loaded(object sender, RoutedEventArgs e)
        {
            this.conversation = new ConversationWithLastMsg();
            this.conversation.conversation = new VKConversation();
            this.conversation.conversation.peer = new VKConversation.ConversationPeer();
            this.conversation.conversation.peer.id = 1;

//            this.detailed.DataContext = this.conversation;
//            this.detailed.DataChanged();
            this.detailed.SetData(this.conversation);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Add 1 item
            VKMessage msg = this.Generate();
//            this.conversation.HistoryVM.Items.Add(msg);
        }

        private VKMessage Generate()
        {
            VKMessage msg = new VKMessage();
            msg.date = DateTime.Now;
            if(!string.IsNullOrEmpty(this._tbDay.Text))
            {
                int days = int.Parse(this._tbDay.Text);
                msg.date = DateTime.Now.AddDays(days);
            }

            msg.@out = this._cbOut.IsChecked.Value ? VKMessageType.Sent : VKMessageType.Received;
            msg.read_state = this._cbReaded.IsChecked.Value;
//            msg.id = (uint)this.conversation.HistoryVM.Items.Count;
            msg.text = string.Format("id: {0} {1} {2}", msg.id, msg.@out == VKMessageType.Received ? "Rcv" : "Snd", msg.read_state ? "readed" : "unreaded");
            return msg;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Add unread item

            int num = 0;


            //В самом конце списка самые новые/последние сообщения
/*
            for (int index = 0; index < this.conversation.HistoryVM.Items.Count; index++)
            {
                if (this.conversation.HistoryVM.Items[index].@out == VKMessageType.Received && this.conversation.HistoryVM.Items[index].read_state == false)
                {
                    num = index;
                    break;
                }

            }

            int i = num + 1;//+1 т.к. этот элемент уже в списке

            this.conversation.HistoryVM.Items.Insert(i, new VKMessage() { action = new VKMessage.MsgAction() { type = VKChatMessageActionType.UNREAD_ITEM_ACTION }, date = this.conversation.HistoryVM.Items[num].date });
*/
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
/*
            var messageViewModel = this.conversation.HistoryVM.Items.FirstOrDefault(m => m.action != null && m.action.type == VKChatMessageActionType.UNREAD_ITEM_ACTION);
            if (messageViewModel != null)
            {
                this.conversation.HistoryVM.Items.Remove(messageViewModel);
            }
*/
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
//            this.conversation.HistoryVM.EnsureUnreadItem();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
 //           this.conversation.HistoryVM.Items.Clear();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            int pos = int.Parse( this._tbPosition.Text);
            VKMessage msg = this.Generate();
//            this.conversation.HistoryVM.Items.Insert(pos,msg);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
//            this.conversation.HistoryVM.Items.Move(int.Parse(this._tbOld.Text), int.Parse(this._tbNew.Text));
        }
    }
}
