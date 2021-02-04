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

using Newtonsoft.Json;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using LunaVK.Core;


namespace LunaVK
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestPush : Page
    {
        public TestPush()
        {
            this.InitializeComponent();



            string toastXmlString = "<toast  launch='action=viewPhoto&amp;photoId=92187'>";//version='1'
            //toastXmlString += "<header id='1234' title='Header title' arguments='action=test'/>";
            toastXmlString += "<visual><binding template='ToastImageAndText02' branding='none'>";// ToastGeneric ToastImageAndText02
            toastXmlString += ("<image id='1' hint-crop='circle' placement='appLogoOverride' src='" + VKConstants.AVATAR_DEACTIVATED + "_100.png" + "'/>");
            toastXmlString += ("<text id='1'>" + "Оля Фабрицкая (Магнитола🎶🎺&#1...)".Replace("&","&amp;") + "</text>");
            toastXmlString += ("<text id='2'>" + "Прислал стикер" + "</text>");

            //toastXmlString += "<image src='https://vk.com/images/stickers/6159/256.png' />";

            toastXmlString += "</binding></visual>";

            //toastXmlString += "<actions>";
            //toastXmlString += "<input id='textBox' type='text' placeHolderContent='Написать ответ' />";
            //toastXmlString += "<action content='Send' arguments='action=reply' activationType='background' hint-inputId='textBox' imageUri='Assets/Icons/send.png' />";//важно: content
            //toastXmlString += "</actions>";

            //toastXmlString += "<audio src='ms-appx:///Assets/Mp3/bb1.mp3'/>";


            toastXmlString += "</toast>";


            this.TB.Text = toastXmlString;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string toastXmlString = this.TB.Text;


            XmlDocument xml = new XmlDocument();
            xml.LoadXml(toastXmlString);

            var not = ToastNotificationManager.CreateToastNotifier();
            not.Show(new ToastNotification(xml));


            int j=0;
        }
    }
}
