using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;
using Windows.UI.Xaml;
using LunaVK.Core.DataObjects;

namespace LunaVK.Core.ViewModels
{
    public class ConversationAvatarViewModel
    {
        public List<string> Images = new List<string>();

        

        public bool Online { get; set; }
        public Enums.VKPlatform platform;
        public string online_app;

        public string PlatformIcon
        {
            get
            {
                if (this.Online == false)
                    return "";

                if (online_app == "2274003")
                    this.platform = Enums.VKPlatform.Android;

                switch (this.platform)
                {
                    case Enums.VKPlatform.Mobile:
                        {
                            return "\xEE64";
                        }
                    case Enums.VKPlatform.Web:
                        {
                            return "\xF137";
                        }
                    case Enums.VKPlatform.iPad:
                    case Enums.VKPlatform.iPhone:
                        {
                            return "\xEE77";
                        }
                    case Enums.VKPlatform.Android:
                        {
                            return "\xEE79";
                        }
                    case Enums.VKPlatform.Windows:
                    case Enums.VKPlatform.WindowsPhone:
                        {
                            return "\xEE63";
                        }
                    default:
                        {
#if DEBUG
                            System.Diagnostics.Debug.WriteLine("PlatformIcon: "+this.platform.ToString() + " online_app: " + online_app);
#endif
                            break;

                        }
                }
                return "\xF137";//WEB
            }
        }
    }
}

/*
         *  Список client_id популярных приложений

4996844 vk mp3 mod
2274003 Android
3140623 iPhone
3682744 iPad
3502561 WP
3697615 Windows
3032107 Vika for Blackberry
2685278 Kate Mobile
3469984 Lynt
3698024 Instagram
4856776 stellio
4580399 snapster для android
4986954 snapster для iPhone
5422643 Telegram
4967124 vkmd
5910839 DarkVK
5044491 Candy
4083558 VFeed
3900090 Xbox 720
5984585 Тапочек
3900094 бутерброд
3900098 домофон
5023680 калькулятор
3900102 psp
3998121 Вутюг
4147789 ВОкно
5014514 Ад ¯\_(ツ)_/¯
5036399 Google Glass™👓
2023699 Mail.ru Агент
5547666 Node.js
5540633 VB
5911580 vkmp4 mod
5914912 Qutter
4856309 SweetVK
4630925 полиглот
4445970 amberfrog
3757640 mira
4831060 zeus
4894723 messenger
4994316 Phoenix
4757672 rocket
4973839 ВКонтакте ГЕО
5021699 Fast VK
3933207 FLiPSi
4645689 VBots
4187848 Вечный онлайн
5116270 4ert
5116357 Голубиная почта
5116373 Голубиная почта 
         * */