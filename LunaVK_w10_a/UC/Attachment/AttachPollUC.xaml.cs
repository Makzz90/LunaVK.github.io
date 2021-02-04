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
using LunaVK.Core.Network;
using LunaVK.Core.Utils;

namespace LunaVK.UC.Attachment
{
    //PollUC
    //PollAnswerUC
    public sealed partial class AttachPollUC : UserControl
    {
        public AttachPollUC()
        {
            this.InitializeComponent();
        }

        private VKPoll VM
        {
            get { return this.DataContext as VKPoll; }
        }

        public AttachPollUC(VKPoll a) : this()
        {
            foreach(var answer in a.answers)
            {
                answer._pollVM = a;
            }

            this.DataContext = a;
        }

        private void PollAnswerUC_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKPollAnswers;
            if(this.VM.multiple)
            {
                if (this.VM.can_vote == false)
                    return;

                if (this.VM.answer_ids.Contains(vm.id))
                    this.VM.answer_ids.Remove(vm.id);
                else
                    this.VM.answer_ids.Add(vm.id);

                this._textVotes.Visibility = this.VM.answer_ids.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
                this._btnVote.Visibility = this.VM.answer_ids.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            else if(this.VM.answer_ids.Count == 0 && this.VM.can_vote)
            {
                this.VM.can_vote = false;
                this.VM.votes++;
                this.VM.answer_ids.Add(vm.id);
                foreach(var answer in this.VM.answers)
                {
                    answer.ReadData();
                }
                this.AddVote();
            }
        }

        public void AddVote()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = this.VM.owner_id.ToString();
            parameters["poll_id"] = this.VM.id.ToString();
            parameters["answer_ids"] = this.VM.answer_ids.GetCommaSeparated();//список идентификаторов ответа (для опроса с мультивыбором). 

            VKRequestsDispatcher.DispatchRequestToVK<int>("polls.addVote", parameters, null );
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;

            this._textVotes.Visibility = lv.SelectedItems.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
            this._btnVote.Visibility = lv.SelectedItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PollAnswerUC_CheckTap(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKPollAnswers;

            if (this.VM.answer_ids.Contains(vm.id))
                this.VM.answer_ids.Remove(vm.id);
            else
                this.VM.answer_ids.Add(vm.id);

            this._textVotes.Visibility = this.VM.answer_ids.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
            this._btnVote.Visibility = this.VM.answer_ids.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void _btnVote_Click(object sender, RoutedEventArgs e)
        {
            this.VM.can_vote = false;
            this.VM.votes++;
            //this.VM.answer_ids.Add(vm.id);
            foreach (var answer in this.VM.answers)
            {
                answer.ReadData();
            }

            this.AddVote();
        }
    }
}