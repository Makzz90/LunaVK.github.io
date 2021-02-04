using System;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Library;

namespace LunaVK.Core
{
    //AppGlobalStateData
    public static class Settings
    {
        public static string AccessToken
        {
            get { return SettingsHelper.Get<string>(); }
            set { SettingsHelper.Set(value); }
        }

        public static uint UserId
        {
            get { return SettingsHelper.Get<uint>(); }
            set { SettingsHelper.Set(value); }
        }

        public static string LoggedInUserName
        {
            get { return SettingsHelper.Get<string>(); }
            set { SettingsHelper.Set(value); }
        }

        public static string LoggedInUserStatus
        {
            get { return SettingsHelper.Get<string>(); }
            set { SettingsHelper.Set(value); }
        }

        public static string LoggedInUserPhoto
        {
            get { return SettingsHelper.Get<string>(); }
            set { SettingsHelper.Set(value); }
        }

        /// <summary>
        /// Уведомления вне приложения
        /// </summary>
        public static uint PushNotificationsEnabled
        {
            get { return SettingsHelper.Get<uint>(); }
            set { SettingsHelper.Set(value); }
        }

        public static string LastPushNotificationsUri
        {
            get { return SettingsHelper.Get<string>(); }
            set { SettingsHelper.Set(value); }
        }

        /// <summary>
        /// Уведомления внутри приложения (баннер)
        /// </summary>
        public static bool NotificationsEnabled
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        /// <summary>
        /// Вибрировать при уведомлении
        /// </summary>
        public static bool VibrationsEnabled
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        /// <summary>
        /// Подавать звук при уведомлении
        /// </summary>
        public static bool SoundEnabled
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static PushSettings PushSettings
        {
            get { return SettingsHelper.Get<PushSettings>(); }
            set { SettingsHelper.Set(value); }
        }

        public static DateTime PushNotificationsBlockedUntil
        {
            get { return SettingsHelper.Get<DateTime>(); }
            set { SettingsHelper.Set(value); }
        }

        /// <summary>
        /// Тема. 0 - чёрная
        /// </summary>
        public static bool BackgroundType
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static Threelen GifAutoplayType
        {
            get { return (Threelen)SettingsHelper.Get<byte>(); }
            set { SettingsHelper.Set((byte)value); }
        }

        public static Threelen FriendListOrder
        {
            get { return (Threelen)SettingsHelper.Get<byte>(); }
            set { SettingsHelper.Set((byte)value); }
        }

        public static byte UIScale
        {
            get { return SettingsHelper.Get<byte>(); }
            set { SettingsHelper.Set(value); }
        }

        public static byte RoundAvatar
        {
            get { return SettingsHelper.Get<byte>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool UseProxy
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }
        /*
        public static string ProxyAdress
        {
            get { return SettingsHelper.Get<string>(); }
            set { SettingsHelper.Set(value); }
        }
        */
        public static byte AccentColor
        {
            get { return SettingsHelper.Get<byte>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool MenuDivider
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool CmdBarDivider
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static int ServerMinusLocalTimeDelta
        {
            get { return SettingsHelper.Get<int>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool SendByEnter
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool MusicWithWebPage
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool SaveLocationDataOnUpload
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        /// <summary>
        /// http://www.xn-----nlcaiebdb9andydgfuq5v.xn--p1ai/wns.php?channel=
        /// </summary>
        public static string CustomPushNotificationsServer
        {
            get { return SettingsHelper.Get<string>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool AnimatedStickers
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Открывать меню по свайпу?
        /// На вп8.1 вызывает крахи
        /// </summary>
        public static bool MenuSwipe
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }
#endif
        public static DateTime DataLastRefreshTime
        {
            get { return SettingsHelper.Get<DateTime>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool DisableAcrylicHeader
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool DisableAcrylicMenu
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool UseAcrylicHost
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool StickersAutoSuggestEnabled
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }


        public static string SaveFolderDoc
        {
            get { return SettingsHelper.Get<string>(); }
            set { SettingsHelper.Set(value); }
        }

        public static string SaveFolderPhoto
        {
            get { return SettingsHelper.Get<string>(); }
            set { SettingsHelper.Set(value); }
        }

        public static string SaveFolderVoice
        {
            get { return SettingsHelper.Get<string>(); }
            set { SettingsHelper.Set(value); }
        }

        public static string SaveFolderVideo
        {
            get { return SettingsHelper.Get<string>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool StickerPanelAsPopup
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool CustomPopUpMenu
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool HideHeader
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        /// <summary>
        /// Записывать ли информационные логи в файл?
        /// </summary>
        public static bool DEV_IsLogsEnabled
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool DEV_IsLogsAutoSending
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool DEV_IsLogsPopupEnabled
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        /// <summary>
        /// Возвращает или задает тип используемых Emoji-символов.
        /// </summary>
        public static byte EmojiType
        {
            get { return SettingsHelper.Get<byte>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool UI_SmallPreview
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool UI_HideApplicationFrame
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }
        /*
        public static short UI_FontWeight
        {
            get { return SettingsHelper.Get<short>( nameof(UI_FontWeight), 450); }
            set { SettingsHelper.Set(value); }
        }
        */
        public static bool SyncContacts
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool AllowUseLocation
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool AllowSendContacts
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static byte FavoritesDefaultSection
        {
            get { return SettingsHelper.Get<byte>(); }
            set { SettingsHelper.Set(value); }
        }

        /// <summary>
        /// Отключить автоматическую пометку сообщений как прочитанные
        /// </summary>
        public static bool DEV_DisableMarkSeen
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool DEV_SetOffline
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static byte DEV_BackgroundAnswerMethode
        {
            get { return SettingsHelper.Get<byte>(); }
            set { SettingsHelper.Set(value); }
        }

        /// <summary>
        /// Добавить кнопку отладки в меню?
        /// </summary>
        public static bool DEV_AddDebugButton
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static uint DefaultVideoResolution
        {
            get { return SettingsHelper.Get<uint>(); }
            set { SettingsHelper.Set(value); }
        }

        public static uint FileVersion
        {
            get { return SettingsHelper.Get<uint>(); }
            set { SettingsHelper.Set(value); }
        }

        public static byte LanguageSettings
        {
            get { return SettingsHelper.Get<byte>(); }
            set { SettingsHelper.Set(value); }
        }
        
        public static bool CompressPhotosOnUpload
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool SaveLocationOnUpload
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool SaveEditedPhotos
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        public static bool LoadBigPhotosOverMobile
        {
            get { return SettingsHelper.Get<bool>(); }
            set { SettingsHelper.Set(value); }
        }

        /// <summary>
        /// Присваиваем значения по-умолчанию
        /// </summary>
        public static void Initialize()
        {
            if (SettingsHelper.IsExists)
            {
                if (FileVersion == 0)
                {
                    string acces_token = SettingsHelper.GetOld<string>(nameof(AccessToken));
                    string name = SettingsHelper.GetOld<string>(nameof(LoggedInUserName));
                    string status = SettingsHelper.GetOld<string>(nameof(LoggedInUserStatus));
                    string photo = SettingsHelper.GetOld<string>(nameof(LoggedInUserPhoto));
                    uint id = SettingsHelper.GetOld<uint>(nameof(UserId));

                    AccessToken = acces_token;
                    LoggedInUserName = name;
                    LoggedInUserStatus = status;
                    LoggedInUserPhoto=photo;
                    UserId = id;
                    PushSettings = null;
                }

                if (PushSettings!=null)//bugfix:ошибка инициализации
                    return;
            }
            
            PushSettings = new PushSettings();
            PushNotificationsEnabled = 2;//выставляем интерактивные пуши
            NotificationsEnabled = true;
            SoundEnabled = true;
            GifAutoplayType = Threelen.Third;
            BackgroundType = false;
            UIScale = 100;
            RoundAvatar = 100;
            UseProxy = false;
            //ProxyAdress = "";
            AccentColor = 0;
            MenuDivider = false;
            LastPushNotificationsUri = "";
            CmdBarDivider = false;
            CustomPushNotificationsServer = "http://www.xn-----nlcaiebdb9andydgfuq5v.xn--p1ai/wns.php?channel=";
            AnimatedStickers = true;
            StickersAutoSuggestEnabled = true;
#if WINDOWS_PHONE_APP
            MenuSwipe = true;
#endif
            SyncContacts = true;
            DefaultVideoResolution = 360;
            //UI_FontWeight = 450;
            FileVersion = 2;
        }

        public static bool IsAuthorized
        {
            get { return !string.IsNullOrEmpty(AccessToken); }
        }
    }
}
