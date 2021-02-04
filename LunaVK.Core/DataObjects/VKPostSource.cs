using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using LunaVK.Core.Enums;

namespace LunaVK.Core.DataObjects
{
    public sealed class VKPostSource
    {
        private const string TYPE_API = "api";
        private const string TYPE_MOBILE = "mvk";
        private const string PLATFORM_IOS = "ios";
        private const string PLATFORM_IPHONE = "iphone";
        private const string PLATFORM_IPAD = "ipad";
        private const string PLATFORM_ANDROID = "android";
        private const string PLATFORM_WINDOWS = "windows";
        private const string PLATFORM_WINPHONE = "wphone";
        private const string PLATFORM_SNAPSTER = "chronicle";
        private const string PLATFORM_INSTAGRAM = "instagram";
        private const string PLATFORM_PRISMA = "prisma";
        private const string PLATFORM_VINCI = "vinci";

        /// <summary>
        /// Тип источника записи в виде строки.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Платформа, с которой была опубликована запись
        /// в виде строки.
        /// </summary>
        public string platform { get; set; }

        public string data { get; set; }

        // <summary>
        // Тип источника записи.
        // </summary>
        /*
        public VKPostSourceType Type
        {
            get
            {
                switch (_type)
                {
                    case _widget:
                        return VKPostSourceType.Widget;
                    case _api:
                        return VKPostSourceType.API;
                    case _rss:
                        return VKPostSourceType.RSS;
                    case _sms:
                        return VKPostSourceType.SMS;
                    default:
                        return VKPostSourceType.VK;
                }
            }
            set
            {
                switch (value)
                {
                    case VKPostSourceType.Widget:
                        _type = _widget;
                        break;
                    case VKPostSourceType.API:
                        _type = _api;
                        break;
                    case VKPostSourceType.RSS:
                        _type = _rss;
                        break;
                    case VKPostSourceType.SMS:
                        _type = _sms;
                        break;
                    case VKPostSourceType.VK:
                        _type = _vk;
                        break;
                }
            }
        }
        */
        public PostSourcePlatform GetPlatform()
        {
            if (this.type == VKPostSource.TYPE_API)
            {
                if (this.platform == VKPostSource.PLATFORM_IOS || this.platform == VKPostSource.PLATFORM_IPHONE || this.platform == VKPostSource.PLATFORM_IPAD)
                    return PostSourcePlatform.IOS;
                else if (this.platform == VKPostSource.PLATFORM_VINCI)
                    return PostSourcePlatform.Vinci;
                else if (this.platform == VKPostSource.PLATFORM_ANDROID)
                    return PostSourcePlatform.Android;
                else if (this.platform == VKPostSource.PLATFORM_WINDOWS || this.platform == VKPostSource.PLATFORM_WINPHONE)
                    return PostSourcePlatform.Windows;
                else if (this.platform == VKPostSource.PLATFORM_SNAPSTER)
                    return PostSourcePlatform.Snapster;
                else if (this.platform == VKPostSource.PLATFORM_PRISMA)
                    return PostSourcePlatform.Prisma;
                return this.platform == VKPostSource.PLATFORM_INSTAGRAM ? PostSourcePlatform.Instagram : PostSourcePlatform.ThirdParty;
            }
            return this.type == VKPostSource.TYPE_MOBILE ? PostSourcePlatform.Mobile : PostSourcePlatform.None;
        }

        // <summary>
        // Платформа, с которой была опубликована запись.
        // </summary>
        /*
        public VKPostSourcePlatform Platform
        {
            get
            {
                switch (_platform)
                {
                    case _android:
                        return VKPostSourcePlatform.Android;
                    case _ios:
                        return VKPostSourcePlatform.iOS;
                    case _wphone:
                        return VKPostSourcePlatform.WinPhone;
                    default:
                        return VKPostSourcePlatform.NotSpecified;
                }
            }
            set
            {
                switch (value)
                {
                    case VKPostSourcePlatform.NotSpecified:
                        _platform = "";
                        break;
                    case VKPostSourcePlatform.Android:
                        _platform = _android;
                        break;
                    case VKPostSourcePlatform.iOS:
                        _platform = _ios;
                        break;
                    case VKPostSourcePlatform.WinPhone:
                        _platform = _wphone;
                        break;
                }
            }
        }
        */
        // date -- https://vk.com/dev/post_source

        /// <summary>
        /// Ссылка на ресурс, с которого была опубликована запись.
        /// </summary>
        public string url { get; set; }

        public enum PostSourcePlatform
        {
            None,
            ThirdParty,
            Mobile,
            Windows,
            IOS,
            Android,
            Snapster,
            Instagram,
            Prisma,
            Vinci,
        }
    }
}
