using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using LunaVK.Core.Enums;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Library;
using LunaVK.ViewModels;
using LunaVK.Framework;
using LunaVK.Core;
using LunaVK.Core.Framework;
using LunaVK.Library;
using LunaVK.Common;

namespace LunaVK.UC
{
    public sealed partial class DebugUC : UserControl
    {
        public DebugUC()
        {
            this.InitializeComponent();
            /*
#if WINDOWS_UWP
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase"))
            {
                LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush black = new LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush();
                black.TintColor = Windows.UI.Colors.Black;
                black.BlurAmount = 10;
                black.BackdropFactor = 0.2f;
                this.back.Background = black;
            }
#else
                this.back.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(150, 0, 0, 0));
#endif
*/
            this.fakeLongpool.Text = "{\"ts\":1755937706,\"pts\":10136243,\"updates\":[[7,-58058397,76037,0],[4,76038,1,-58058397,1559909539,\"Верно ✅ Всё.\",{\"emoji\":\"1\",\"title\":\" ... \"},{},24,0],[80,1,1,0],[4,76039,1,-58058397,1559909539,\"Третий вопрос.<br>❣️15-21 день цикла: Эту фазу называют «зоной спокойствия». В это время многие девушки чувствуют себя расслабленными и могут запросто зависнуть над котиками в интернете.<br><br>🔎Правда или миф?\",{\"emoji\":\"1\",\"title\":\" ... \",\"keyboard\":{\"one_time\":false,\"buttons\":[[{\"action\":{\"type\":\"text\",\"payload\":\"{}\",\"label\":\"Правда\"},\"color\":\"positive\"}],[{\"action\":{\"type\":\"text\",\"payload\":\"{}\",\"label\":\"Миф\"},\"color\":\"negative\"}]]}},{},25,0],[52,11,-58058397,0]]}";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int chatId, userId, userId2 = 0;
            int.TryParse(this.typingUserId.Text, out userId);
            int.TryParse(this.typingUserId2.Text, out userId2);
            int.TryParse(this.typingChatId.Text, out chatId);

            var update = new UpdatesResponse.LongPollServerUpdateData();
            if(chatId>0)
                update.peer_id = chatId + 2000000000;
            else
                update.peer_id = userId;
            update.user_id = userId;
            update.UpdateType = LongPollServerUpdateType.UserIsTyping;

            List<UpdatesResponse.LongPollServerUpdateData> l = new List<UpdatesResponse.LongPollServerUpdateData>();
            l.Add(update);


            if(userId2!=0)
            {
                var update2 = new UpdatesResponse.LongPollServerUpdateData();
                if (chatId > 0)
                    update2.peer_id = chatId + 2000000000;
                else
                    update2.peer_id = userId2;
                update2.user_id = userId2;
                update2.UpdateType = LongPollServerUpdateType.UserIsTyping;
                l.Add(update2);
            }

            Execute.ExecuteOnBackgroundThread(() =>
            { 
                Network.LongPollServerService.Instance.EnrichUpdateData(l);
            });
        }

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.userList == null)
                return;

            this.userList.Children.Clear();
            this.newMsgUserList.Children.Clear();


            foreach(VKBaseDataForGroupOrUser data in UsersService.Instance.CachedUsers)
            {
                if (data is VKGroup)
                    continue;

                TextBlock t = new TextBlock();
                t.IsTextSelectionEnabled = true;
                t.Text = data.Id + " " + data.Title;
                t.FontSize = 18;
                this.userList.Children.Add(t);


                TextBlock t2 = new TextBlock();
                t2.Tag = data.Id;
                t2.Tapped += t2_Tapped;
                t2.FontSize = 20;
                //t2.IsTextSelectionEnabled = true;
                t2.Text = data.Id + " " + data.Title;
                this.newMsgUserList.Children.Add(t2);
            }
        }

        void t2_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TextBlock t = sender as TextBlock;
            this.newMsgUserId.Text = t.Tag.ToString();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int chatId, userId = 0;
            int.TryParse(this.newMsgUserId.Text, out userId);
            int.TryParse(this.newMsgChatId.Text, out chatId);

            VKMessage m = new VKMessage();
            m.text = this.newMsgText.Text;
            m.from_id = userId;
            
            m.date = DateTime.Now;
            m.@out = VKMessageType.Received;
            
            UpdatesResponse.LongPollServerUpdateData update = new UpdatesResponse.LongPollServerUpdateData();
            if(chatId>0)
                update.peer_id = chatId + 2000000000;
            else
                update.peer_id = userId;
            update.user_id = userId;
            update.message = m;
            update.text = this.newMsgText.Text;
            update.UpdateType = LongPollServerUpdateType.MessageAdd;
            update.flags = new UpdatesResponse.LongPollServerUpdateData.flag();

            Random rnd = new Random();

            update.message_id = (uint)rnd.Next(10000, 50000);
            
            List<UpdatesResponse.LongPollServerUpdateData> l = new List<UpdatesResponse.LongPollServerUpdateData>();
            l.Add(update);
            Execute.ExecuteOnBackgroundThread(() => {
                Network.LongPollServerService.Instance.EnrichUpdateData(l);
            });
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            int chatId = 0;
            int.TryParse(this.chatParamChatId.Text, out chatId);

            

            UpdatesResponse.LongPollServerUpdateData update = new UpdatesResponse.LongPollServerUpdateData();
            //if (chatId > 0)
            update.peer_id = chatId + 2000000000;
            //update.message = m;
            //update.text = this.newMsgText.Text;
            update.UpdateType = LongPollServerUpdateType.ChatParamsChanged;
            //update.flags = new UpdatesResponse.LongPollServerUpdateData.flag();

            //Random rnd = new Random();

            //update.message_id = (uint)rnd.Next(10000, 50000);

            List<UpdatesResponse.LongPollServerUpdateData> l = new List<UpdatesResponse.LongPollServerUpdateData>();
            l.Add(update);
            Execute.ExecuteOnBackgroundThread(() =>
            {
                Network.LongPollServerService.Instance.EnrichUpdateData(l);
            });
        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Network.LongPollServerService.Instance.Restart();
        }

        private void TextBlock_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            Network.LongPollServerService.Instance.Stop();
        }
        
        private void BlurAmount_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
#if WINDOWS_UWP
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase"))
            {
                if(this.back.Background is LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush black)
                {
                    black.BlurAmount = (float)e.NewValue;
                }
            }
#endif
            }

            private void BackdropFactor_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
#if WINDOWS_UWP
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase"))
            {
                if (this.back.Background is LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush black)
                {
                    black.BackdropFactor = (float)e.NewValue;
                }
            }
#endif
            }

            private void TintColorFactor_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
#if WINDOWS_UWP
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase"))
            {
                if (this.back.Background is LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush black)
                {
                    black.TintColorFactor = (float)e.NewValue;
                }
            }
#endif
            }

            private void TextBlock_Tapped_2(object sender, TappedRoutedEventArgs e)
        {
            Library.PushNotifications.Instance.RegisterTasks();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            int userId = 0;
            int.TryParse(this.userOnlineUserId.Text, out userId);

            if (userId == 0)
                return;

            UpdatesResponse.LongPollServerUpdateData update = new UpdatesResponse.LongPollServerUpdateData();
            update.user_id = userId;
            update.peer_id = userId;
            update.UpdateType = this.onlineBox.IsChecked == true ? LongPollServerUpdateType.UserBecameOffline : LongPollServerUpdateType.UserBecameOnline;
            update.Platform = 1;
            //update.timestamp = DateTime.Now.To
            List<UpdatesResponse.LongPollServerUpdateData> l = new List<UpdatesResponse.LongPollServerUpdateData>();
            l.Add(update);
            Execute.ExecuteOnBackgroundThread(() =>
            {
                Network.LongPollServerService.Instance.EnrichUpdateData(l);
            });
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            string text = this.fakeLongpool.Text;
#if DEBUG
            Execute.ExecuteOnBackgroundThread(() =>
            {
                Network.LongPollServerService.Instance.FakeData(text);
            });
#endif
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            DialogsViewModel.Save();
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
#if DEBUG
            DialogsViewModel.Instance = null;
#endif
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            CacheManager.TryDelete("Dialogs");
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            if (lv.SelectedIndex == 0)
                CustomFrame.Instance.Navigate(typeof(Pages.Debug.TestResponse));
            else if (lv.SelectedIndex == 1)
                CustomFrame.Instance.Navigate(typeof(Pages.Debug.TestEmoji));
            else if (lv.SelectedIndex == 2)
                NavigatorImpl.Instance.NavigateToVideoWithComments(0, 0);
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            MemoryDiagnosticsHelper.Start(TimeSpan.FromMilliseconds(500), false);
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            MemoryDiagnosticsHelper.Stop();
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            GC.Collect();
        }

        private void AppBarButton_CancelClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void AppBarButton_DockClick(object sender, RoutedEventArgs e)
        {
            this.MaxWidth = double.IsPositiveInfinity(this.MaxWidth) ? 600 : double.PositiveInfinity;
        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            ContactsManager.Instance.EnsureInSyncAsync(true);
        }
    }
}
