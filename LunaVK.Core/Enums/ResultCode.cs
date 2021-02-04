using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.Enums
{
    public enum ResultCode
    {
//        DeserializationError = -10000,
        UploadingFailed = -2,
        CommunicationFailed = -1,
        Succeeded = 0,

        /// <summary>
        /// Произошла неизвестная ошибка.
        /// </summary>
        UnknownError,

        /// <summary>
        /// Приложение выключено.
        /// </summary>
        AppDisabled,

        /// <summary>
        /// Передан неизвестный метод.
        /// </summary>
        UnknownMethod,

        /// <summary>
        /// Неверная подпись.
        /// </summary>
        IncorrectSignature,

        /// <summary>
        /// Авторизация пользователя не удалась.
        /// </summary>
        UserAuthorizationFailed,

        /// <summary>
        /// Слишком много запросов в секунду.
        /// </summary>
        TooManyRequestsPerSecond,

        /// <summary>
        /// Нет прав для выполнения этого действия.
        /// </summary>
        NotAllowed,

        /// <summary>
        /// Неверный запрос.
        /// </summary>
        IncorrectRequest,

        /// <summary>
        /// Слишком много однотипных действий.
        /// </summary>
        FloodControlEnabled,

        /// <summary>
        /// Произошла внутренняя ошибка сервера.
        /// </summary>
        InternalServerError,

        /// <summary>
        /// В тестовом режиме приложение должно быть выключено или пользователь должен быть залогинен.
        /// </summary>
        DisableAppInTestMode,

        /// <summary>
        /// Требуется ввод кода с картинки (Captcha).
        /// </summary>
        CaptchaRequired = 14,

        /// <summary>
        /// Доступ запрещён.
        /// </summary>
        AccessDenied,

        /// <summary>
        /// Требуется выполнение запросов по протоколу HTTPS,
        /// т.к. пользователь включил настройку, требующую работу через безопасное соединение. 
        /// </summary>
        UseHTTPS,

        /// <summary>
        /// Требуется валидация пользователя.
        /// </summary>
        ValidationRequired,

        /// <summary>
        /// Страница удалена или заблокирована.
        /// </summary>
        DeletedOrBanned,

        /// <summary>
        /// Данное действие запрещено для не Standalone приложений.
        /// </summary>
        NotAllowedForApp,

        /// <summary>
        /// Данное действие разрешено только для Standalone и Open API приложений. 
        /// </summary>
        UseStandalone,

        /// <summary>
        /// Метод был выключен.
        /// </summary>
        MethedeDisabled,

        /// <summary>
        /// Требуется подтверждение со стороны пользователя. 
        /// </summary>
        ConfirmationRequired,

        TokenConfirmationRequired,//no in documentation

        /// <summary>
        /// Ключ доступа сообщества недействителен.
        /// </summary>
        ComunityTokenExipired = 27,

        /// <summary>
        /// Ключ доступа приложения недействителен.
        /// </summary>
        AppTokenExipired,

        /// <summary>
        /// Достигнут количественный лимит на вызов метода
        /// </summary>
        MethodeLimitReached,

        /// <summary>
        /// Один из необходимых параметров был не передан или неверен.
        /// </summary>
        WrongParameter = 100,

        /// <summary>
        /// Неверный API ID приложения.
        /// </summary>
        AppIdIncorrect,

//        OutOfLimits = 103,

        /// <summary>
        /// Неверный идентификатор пользователя. 
        /// </summary>
        InvalidUserIds = 113,

//        InvalidAudioFormat = 123,
//        CannotAddYourself = 174,
//        UserIsBlackListed = 175,
//        PricavySettingsRestriction = 176,
        /// <summary>
        /// Неверный timestamp.
        /// </summary>
        WrongTimeStamp = 150,

        /// <summary>
        /// Доступ к альбому запрещён. 
        /// </summary>
        AccessDeniedAlbum = 200,

        /// <summary>
        /// Доступ к аудио запрещён.
        /// </summary>
        AccessDeniedAudio,

        /// <summary>
        /// Доступ к группе запрещён.
        /// </summary>
        AccessDeniedComunity = 203,

//        AccessDeniedExtended = 204,
//        PostsLimitOrAlreadyScheduled = 214,
//        AudioIsExcludedByRightholder = 270,
//        AudioFileSizeLimitReached = 302,

        /// <summary>
        /// Альбом переполнен.
        /// </summary>
        MaximumLimitReached = 300,
//        Unauthorized = 401,

        /// <summary>
        /// Действие запрещено.
        /// Вы должны включить переводы голосов в настройках приложения
        /// </summary>
        NotAllowedhMoney = 500,

        /// <summary>
        /// Нет прав на выполнение данных операций с рекламным кабинетом.
        /// </summary>
        NotAllowedForAds = 600,

        /// <summary>
        /// Произошла ошибка при работе с рекламным кабинетом.
        /// </summary>
        ErrorAds = 603

//        WrongPhoneNumberFormat = 1000,
//        UserAlreadyInvited = 1003,
//        PhoneAlreadyRegistered = 1004,
//        InvalidCode = 1110,
        //BadPassword = 1111,
        //Processing = 1112,
        //ProductNotFound = 1211,
        //VideoNotFound = 1212,
        //CatalogIsNotAvailable = 1310,
        //CatalogCategoriesAreNotAvailable = 1311,
        //WallIsDisabled = 10006,
        //NewLongPollServerRequested = 100000,
        //WrongUsernameOrPassword = 100001,
        //CaptchaControlCancelled = 100002,
        //ValidationCancelledOrFailed = 100003,
        //ConfirmationCancelled = 100004,
        //BalanceRefillCancelled = 100005,
    }
}
