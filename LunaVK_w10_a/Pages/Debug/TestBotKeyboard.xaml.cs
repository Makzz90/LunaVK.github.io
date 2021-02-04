using LunaVK.Core.DataObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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
    public sealed partial class TestBotKeyboard : Page
    {
        public TestBotKeyboard()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.ProcessText("\"buttons\":[[{\"action\":{\"type\":\"text\",\"label\":\"❤️\",\"payload\":\"1\"},\"color\":\"positive\"},{\"action\":{\"type\":\"text\",\"label\":\"💌\",\"payload\":\"2\"},\"color\":\"positive\"},{\"action\":{\"type\":\"text\",\"label\":\"👎\",\"payload\":\"3\"},\"color\":\"negative\"},{\"action\":{\"type\":\"text\",\"label\":\"💤\",\"payload\":\"4\"},\"color\":\"default\"}]]");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.ProcessText("\"buttons\":[[{\"action\":{\"type\":\"text\",\"payload\":\"{}\",\"label\":\"Правда\"},\"color\":\"positive\"},{\"action\":{\"type\":\"text\",\"payload\":\"{}\",\"label\":\"Миф\"},\"color\":\"negative\"}]]");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.ProcessText("\"buttons\":[[{\"action\": {\"type\": \"text\",\"label\": \"Карты Сбербанка\",\"payload\": \"{}\"},\"color\": \"positive\"},{\"action\": {\"type\": \"text\",\"label\": \"Поиск отделений и банкоматов\",\"payload\": \"{}\"},\"color\": \"positive\"}]]");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.ProcessText("\"buttons\":[[{\"action\":{\"type\":\"text\",\"label\":\"VIP Магазин\",\"payload\":\"\"},\"color\":\"positive\"}],[{\"action\":{\"type\":\"text\",\"label\":\"Мой счёт\",\"payload\":\"\"},\"color\":\"primary\"},{\"action\":{\"type\":\"text\",\"label\":\"LootBox\",\"payload\":\"\"},\"color\":\"negative\"},{\"action\":{\"type\":\"text\",\"label\":\"Магазин\",\"payload\":\"\"},\"color\":\"primary\"}],[{\"action\":{\"type\":\"text\",\"label\":\"Рейтинг новичков\",\"payload\":\"\"},\"color\":\"default\"},{\"action\":{\"type\":\"text\",\"label\":\"Битва\",\"payload\":\"\"},\"color\":\"negative\"},{\"action\":{\"type\":\"text\",\"label\":\"Обмен\",\"payload\":\"\"},\"color\":\"default\"}]]");
        }

        private void ProcessText(string json)
        {
            this._out.Text = json;

            Regex QueryStringRegex = new Regex(@"""buttons"":(.+|\n)+", RegexOptions.Singleline);

            var match = QueryStringRegex.Match(json);
            if (match.Success)
            {
                var str = match.Groups[1].Value;
                ObservableCollection<List<VKBotKeyboard.KeyboardButton>> BotKeyboardButtons = new ObservableCollection<List<VKBotKeyboard.KeyboardButton>>();

                var buttons = Newtonsoft.Json.JsonConvert.DeserializeObject<List<List<VKBotKeyboard.KeyboardButton>>>(str);

                foreach (var button in buttons)
                {
                    BotKeyboardButtons.Add(button);
                }

                this._botKeyboard.ItemsSource = BotKeyboardButtons;
            }
        }

        
    }
}
