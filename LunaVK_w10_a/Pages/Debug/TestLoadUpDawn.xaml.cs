using LunaVK.Core.Enums;
using LunaVK.Core.Network;
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

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestLoadUpDawn : Page
    {
        private int real_offset_UP;
        private int real_offset_DOWN;
        private int UpLoaded = 0;
        private int DownLoaded = 0;
        private bool NeedOffsetForUp;
        private bool NeedOffsetForDown;
        private uint maximum = 0;


        public TestLoadUpDawn()
        {
            this.InitializeComponent();
            this.Loaded += TestLoadUpDawn_Loaded;
        }

        private void TestLoadUpDawn_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 2; i <= 22; i++)
            {
                TextBlock tb = new TextBlock() { Text = i.ToString() };
                this._stack.Children.Add(tb);
            }
        }
        
        private async void Button_Click_UP(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = this._group_id.Text;
            parameters["topic_id"] = this._topic_id.Text;
            int offs = UpLoaded;
            if (NeedOffsetForUp)
            {
                offs += 3;
            }
            parameters["offset"] = offs.ToString();
            parameters["count"] = this._count.Text;
            parameters["sort"] = this._sort.SelectedIndex == 0 ? "asc" : "desc";
            parameters["start_comment_id"] = this._start_comment_id.Text;

            /*
            var temp = await RequestsDispatcher.GetResponse<ViewModels.GroupDiscussionViewModel.BoardComments>("board.getComments", parameters);
            if (temp.error.error_code == VKErrors.None)
            {
                maximum = temp.response.count;

                if (this._stackOut.Children.Count == 0)
                    NeedOffsetForDown = true;

                uint s = uint.Parse(this._start_comment_id.Text);
                foreach (var item in temp.response.items)
                {
                    TextBlock tb = new TextBlock() { Text = item.id.ToString() };

                    if(s!=0 && s== (uint)item.id)
                    {
                        tb.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                    }

                    //this._stackOut.Children.Add(tb);
                    this._stackOut.Children.Insert(0, tb);
                }

                real_offset_UP = temp.response.real_offset;
                UpLoaded += temp.response.items.Count;
                //this._real_offs.Text = "real_offset=" + real_offset_UP.ToString();
                Update();
            }*/
        }

        

        private async void Button_Click_DOWN(object sender, RoutedEventArgs e)
        {
            //_last_action.Text = "DOWN";

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = this._group_id.Text;
            parameters["topic_id"] = this._topic_id.Text;
            int offs = DownLoaded;
            if(NeedOffsetForDown)
            {
                offs += 3;
            }
            parameters["offset"] = (-offs).ToString();//this._offset.Text;
            parameters["count"] = this._count.Text;
            parameters["sort"] = this._sort.SelectedIndex == 0 ? "asc" : "desc";
            parameters["start_comment_id"] = this._start_comment_id.Text;

            /*
            var temp = await RequestsDispatcher.GetResponse<ViewModels.GroupDiscussionViewModel.BoardComments>("board.getComments", parameters);
            if (temp.error.error_code == VKErrors.None)
            {
                maximum = temp.response.count;

                if (this._stackOut.Children.Count == 0)
                    NeedOffsetForUp = true;

                temp.response.items.Reverse();
                uint s = uint.Parse(this._start_comment_id.Text);

                foreach (var item in temp.response.items)
                {
                    TextBlock tb = new TextBlock() { Text = item.id.ToString() };

                    if (s != 0 && s == (uint)item.id)
                    {
                        tb.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                    }

                    this._stackOut.Children.Add(tb);
                    //this._stackOut.Children.Insert(0, tb);
                }

                
                real_offset_DOWN = temp.response.real_offset;
                DownLoaded += temp.response.items.Count;
                //this._real_offs.Text = "real_offset=" + real_offset_DOWN.ToString();
                Update();
            }*/
        }

        

        void Update()
        {
            this._info.Text = string.Format("real_offset_DOWN:{0} real_offset_UP:{1} \n DownLoaded:{2} UpLoaded:{3} \n maximum:{4}", real_offset_DOWN, real_offset_UP, DownLoaded, UpLoaded, maximum);

            uint s = uint.Parse(this._start_comment_id.Text);
            if (s==0)
            {
                this._info.Text += string.Format(" UP items left={0} DOWN left={1}", maximum - this._stackOut.Children.Count, 0);
            }
            else
                this._info.Text += string.Format("UP items left={0} DOWN left={1}", maximum- real_offset_UP-3, real_offset_DOWN==0? real_offset_UP: real_offset_DOWN);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this._stackOut.Children.Clear();
            real_offset_DOWN = real_offset_UP = 0;
            UpLoaded = DownLoaded = 0;
            NeedOffsetForUp = NeedOffsetForDown = false;
        }
    }
}
