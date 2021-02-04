using LunaVK.Core;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace LunaVK.Common
{
    public class MessageBox
    {
        /// <summary>
        /// Показать плашку-вопрос
        /// </summary>
        /// <param name="title">ключ для локализации</param>
        /// <param name="content">ключ для локализации или переведённый текст</param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public static async Task<MessageBoxButton> Show(string title, string content, MessageBoxButton buttons = MessageBoxButton.OKCancel)
        {
            string text = LocalizedStrings.GetString(content);
            MessageDialog dialog = new MessageDialog(string.IsNullOrEmpty(text) ? content : text, LocalizedStrings.GetString(title));

            dialog.Commands.Add(new UICommand { Label = "Ok", Id = MessageBoxButton.OK });
            if (buttons == MessageBoxButton.OKCancel)
                dialog.Commands.Add(new UICommand { Label = LocalizedStrings.GetString("Cancel/Text"), Id = MessageBoxButton.Cancel });
            var temp = await dialog.ShowAsync();
            return (MessageBoxButton)temp.Id;
        }

        public enum MessageBoxButton
        {
            Cancel,
            OK,
            OKCancel
        }
    }
}
