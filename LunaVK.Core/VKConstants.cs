namespace LunaVK.Core
{
    public static class VKConstants
    {
        /// <summary>
        /// 5.120
        /// </summary>
        public static readonly string API_VERSION = "5.120";

        public static readonly string ApplicationID = "6244854";
        public static readonly string Scope = "audio,friends,docs,groups,messages,notes,notifications,notify,offline,pages,photos,stats,status,video,wall,stories,market";
        public static readonly string Redirect = "https://oauth.vk.com/blank.html";

        /// <summary>
        /// https://api.vk.com
        /// </summary>
        public static readonly string HostURL = "https://api.vk.com";

        public static readonly string AVATAR_CAMERA = "https://vk.com/images/camera";
        public static readonly string AVATAR_DEACTIVATED = "https://vk.com/images/deactivated";

        /// <summary>
        /// https://vk.com/images/community
        /// </summary>
        public static readonly string AVATAR_COMMUNITY = "https://vk.com/images/community";

        public static readonly string AVATR_MULTICHAT = "https://vk.com/images/icons/im_multichat";
                        
        /// <summary>
        /// 935.0
        /// </summary>
        public static readonly double MAX_CONTENT_WIDTH = 935.0;

        /// <summary>
        /// 25.0
        /// </summary>
        public static readonly float BLUR_AMOUNT = 25.0f;

        /// <summary>
        /// 0.5
        /// </summary>
        public static readonly float BACKDROP_FACTOR = 0.5f;

        public static readonly short ALBUM_TYPE_SAVED_PHOTOS = -15;

        public static readonly short ALBUM_TYPE_WALL_PHOTOS = -7;

        public static readonly short ALBUM_TYPE_PROFILE_PHOTOS = -6;

        public static readonly int MAX_PHOTO_WIDTH = 2560;
        public static readonly int MAX_PHOTO_HEIGHT = 2048;

        public static readonly int JPEGQUALITY = 90;

        public static int ResizedImageSize
        {
            get { return !Settings.CompressPhotosOnUpload ? 10000000 : 1000000; }
        }
    }
}

/*
1. Права доступа для токена пользователя

notify
(+1) 	Пользователь разрешил отправлять ему уведомления (для flash/iframe-приложений).

friends
(+2) 	Доступ к друзьям.

photos
(+4) 	Доступ к фотографиям.

audio
(+8) 	Доступ к аудиозаписям.

video
(+16) 	Доступ к видеозаписям.

stories
(+64) 	Доступ к историям.

pages
(+128) 	Доступ к wiki-страницам.

+256 	Добавление ссылки на приложение в меню слева.

status
(+1024) 	Доступ к статусу пользователя.

notes
(+2048) 	Доступ к заметкам пользователя.

messages
(+4096) 	Доступ к расширенным методам работы с сообщениями (только для Standalone-приложений).

wall
(+8192) 	Доступ к обычным и расширенным методам работы со стеной.
Данное право доступа по умолчанию недоступно для сайтов (игнорируется при попытке авторизации для приложений с типом «Веб-сайт» или по схеме Authorization Code Flow).

ads
(+32768) 	Доступ к расширенным методам работы с рекламным API. Доступно для авторизации по схеме Implicit Flow или Authorization Code Flow.

offline
(+65536) 	Доступ к API в любое время (при использовании этой опции параметр expires_in, возвращаемый вместе с access_token, содержит 0 — токен бессрочный). Не применяется в Open API.

docs
(+131072) 	Доступ к документам.

groups
(+262144) 	Доступ к группам пользователя.

notifications
(+524288) 	Доступ к оповещениям об ответах пользователю.

stats
(+1048576) 	Доступ к статистике групп и приложений пользователя, администратором которых он является.

email
(+4194304) 	Доступ к email пользователя.

market
(+134217728) 	Доступ к товарам. 

 * 
 * 
 * 
 * 
 * 
 * 
 * 
2. Права доступа для токена сообщества

stories
(+1) 	Доступ к историям.

photos
(+4) 	Доступ к фотографиям.

app_widget
(+64) 	Доступ к виджетам приложений сообществ. Это право можно запросить только с помощью метода Client API showGroupSettingsBox.

messages
(+4096) 	Доступ к сообщениям сообщества.

docs
(+131072) 	Доступ к документам.

manage
(+262144) 	Доступ к администрированию сообщества. 
*/