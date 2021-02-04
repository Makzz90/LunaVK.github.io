using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
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

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace LunaVK.UC
{
    public sealed partial class CreateChatUC : UserControl
    {
        public Action<IReadOnlyList<int>, string> SendTap;

        public CreateChatUC()
        {
            this.InitializeComponent();
            this.Loaded += CreateChatUC_Loaded;
        }

        private void CreateChatUC_Loaded(object sender, RoutedEventArgs e)
        {
            
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["order"] = "hints";
            parameters["count"] = "40";
            parameters["fields"] = "photo_50";

            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKUser>>("friends.get", parameters,(result)=> { 
            if(result.error.error_code== Core.Enums.VKErrors.None)
            {
                    Execute.ExecuteOnUIThread(() => { 
                        foreach (var user in result.response.items)
                            this._lv.Items.Add(user);
                    });
                }
            });
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.UpdateBtn();
        }

        private void UpdateBtn()
        {
            this._btn.IsEnabled = !string.IsNullOrEmpty(this._textBox.Text) || this._lv.SelectedItems.Count>0;
        }

        private void _lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateBtn();
        }

        private void _btn_Click(object sender, RoutedEventArgs e)
        {
            if(this.SendTap!=null)
            {
                if(this._lv.SelectedItems.Count>0)
                {
                    List<int> l = new List<int>();
                    foreach(var item in this._lv.SelectedItems)
                    {
                        var user = item as VKUser;
                        l.Add(user.Id);
                    }
                    this.SendTap(l, this._textBox.Text);
                }
                else
                {
                    this.SendTap(null,this._textBox.Text);
                }
            }
        }
    }
}
