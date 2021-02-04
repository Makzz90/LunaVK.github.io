using LunaVK.Core;
using LunaVK.Core.DataObjects;
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
    public sealed partial class TestTile : Page
    {
        public TestTile()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SecondaryTileManager.Instance.SendTile(this._tag.Text);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PrimaryTileManager.Instance.AddContent(this._title.Text, this._subtitle.Text, this._tag.Text, this._image.Text);
            
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            PrimaryTileManager.Instance.ResetContent();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            CountersService.Instance.GetCountersWithLastMessage((res) =>
            {
                if (res.error.error_code == LunaVK.Core.Enums.VKErrors.None)
                {
                    PrimaryTileManager.Instance.SetCounter(res.response.Counters.TotalCount);

                    PrimaryTileManager.Instance.ResetContent();

                    byte count = 0;

                    foreach (var conversation in res.response.Convs.items)
                    {
                        VKBaseDataForGroupOrUser owner = null;
                        if (conversation.last_message.from_id > 0)
                            owner = res.response.Convs.profiles.Find((u) => u.id == conversation.last_message.from_id);
                        else
                            owner = res.response.Convs.groups.Find((u) => u.id == -conversation.last_message.from_id);
                        PrimaryTileManager.Instance.AddContent(owner.Title, conversation.last_message.text, "Conversation_" + conversation.conversation.peer.id, owner.MinPhoto);
                        count++;
                    }

                    if (count >= 5)
                        return;
                    //Основная плитка поддерживает только 5 содержимых
                    foreach(var friend in res.response.Friends.items)
                    {
                        PrimaryTileManager.Instance.AddContent("Заявка в друзья", friend.Title, "Friend_" + friend.id, friend.MinPhoto);
                        count++;
                        if (count >= 5)
                            break;
                    }
                }
            });
        }
    }
}
