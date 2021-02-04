using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.Enums
{
    public enum VKErrors : int
    {
        /// <summary>
        /// Ошибки не произошло.
        /// </summary>
        None,

        /// <summary>
        /// Неизвестная ошибка.
        /// </summary>
        UnknownError,

        /// <summary>
        /// Приложение выключено.
        /// Необходимо включить приложение в настройках https://vk.com/editapp?id={Ваш API_ID} или использовать тестовый режим (test_mode=1).
        /// </summary>
        ApplicationIsDisabled,

        /// <summary>
        /// Передан неизвестный метод.
        /// Проверьте, правильно ли указано название вызываемого метода: http://vk.com/dev/methods.
        /// </summary>
        UnknownMethod,

        /// <summary>
        /// Неверная подпись.
        /// Проверьте правильность формирования подписи запроса: https://vk.com/dev/api_nohttps.
        /// </summary>
        IncorrectSignature,

        /// <summary>
        /// Авторизация пользователя не удалась.
        /// Убедитесь, что Вы используете верную схему авторизации. Для работы с методами без префикса secure Вам нужно авторизовать пользователя одним из этих способов: http://vk.com/dev/auth_sites, http://vk.com/dev/auth_mobile.
        /// </summary>
        AuthorizationFailed,

        /// <summary>
        /// Слишком много запросов в секунду.
        /// Задайте больший интервал между вызовами или используйте метод execute. Подробнее об ограничениях на частоту вызовов см. на странице http://vk.com/dev/api_requests.
        /// </summary>
        TooManyRequests,

        /// <summary>
        /// Нет прав для выполнения этого действия.
        /// Проверьте, получены ли нужные права доступа при авторизации. Это можно сделать с помощью метода account.getAppPermissions.
        /// </summary>
        PermissionIsDenied,

        /// <summary>
        /// Неверный запрос.
        /// Проверьте синтаксис запроса и список используемых параметров (его можно найти на странице с описанием метода).
        /// </summary>
        InvalidRequest,

        /// <summary>
        /// Слишком много однотипных действий.
        /// Нужно сократить число однотипных обращений. Для более эффективной работы Вы можете использовать execute или JSONP.
        /// </summary>
        FloodControl,

        /// <summary>
        /// Произошла внутренняя ошибка сервера.
        /// Попробуйте повторить запрос позже.
        /// </summary>
        InternalServerError,

        /// <summary>
        /// В тестовом режиме приложение должно быть выключено или пользователь должен быть залогинен. 
        /// Выключите приложение в настройках https://vk.com/editapp?id={Ваш API_ID}
        /// </summary>
        InTestModeAppShouldBeDisabled,

        /// <summary>
        /// Требуестся ввод каптчи.
        /// </summary>
        CaptchaNeeded = 14,

        /// <summary>
        /// Доступ запрещен.
        /// Убедитесь, что Вы используете верные идентификаторы, и доступ к контенту для текущего пользователя есть в полной версии сайта.
        /// </summary>
        AccessDenied,

        /// <summary>
        /// Требуется выполнение запросов по протоколу HTTPS, т.к. пользователь включил настройку, требующую работу через безопасное соединение.
        /// Чтобы избежать появления такой ошибки, в Standalone-приложении Вы можете предварительно проверять состояние этой настройки у пользователя методом account.getInfo.
        /// </summary>
        HTTPAuthorizationFailed,

        /// <summary>
        /// Требуется валидация пользователя.
        /// Убедитесь, что Вы не используете токен, полученный по схеме http://vk.com/dev/auth_mobile, для вызовов с сервера — это запрещено. Процесс валидации пользователя описан на отдельной странице.
        /// </summary>
        ValidationRequed,

        /// <summary>
        /// Контент недоступен.
        /// </summary>
        ContentUnavailable = 19,
        /// <summary>
        /// Данное действие запрещено для не Standalone приложений.
        /// Если ошибка возникает несмотря на то, что Ваше приложение имеет тип Standalone, убедитесь, что при авторизации Вы используете redirect_uri=https://oauth.vk.com/blank.html. Подробнее см. http://vk.com/dev/auth_mobile.
        /// </summary>
        PermissionDeniedForNonStandaloneApps = 20,
        /// <summary>
        /// Данное действие разрешено только для Standalone и Open API приложений.
        /// </summary>
        PermissionAllowedOnlyForStandaloneAndOpenAPIApps = 21,
        /// <summary>
        /// Ошибка загрузки.
        /// </summary>
        DownloadError = 22,

        /// <summary>
        /// Метод был выключен.
        /// Все актуальные методы ВК API, которые доступны в настоящий момент, перечислены здесь: http://vk.com/dev/methods.
        /// </summary>
        MethodDisabled = 23,

        ProfileIsPrivate = 30,

        /// <summary>
        /// Один из необходимых параметров был не передан или неверен. 
        /// Проверьте список требуемых параметров и их формат на странице с описанием метода.
        /// </summary>
        InvalidOrMissingParameter = 100,

        /// <summary>
        /// Неверный API ID приложения.
        /// Найдите приложение в списке администрируемых на странице http://vk.com/apps?act=settings и укажите в запросе верный API_ID (идентификатор приложения).
        /// </summary>
        InvalidAppAPIID = 101,
        /// <summary>
        /// Превышен лимит на количество приглашений в сообщество.
        /// </summary>
        OutOfLimits = 103,
        /// <summary>
        /// Невозможно сохранить файл.
        /// </summary>
        CantSaveFile = 105,
        /// <summary>
        /// Неверный идентификатор пользователя. 
        /// Убедитесь, что Вы используете верный идентификатор. Получить ID по короткому имени можно методом utils.resolveScreenName.
        /// </summary>
        InvalidUserID = 113,
        /// <summary>
        /// Неверный хэш.
        /// </summary>
        InvalidHash = 121,
        /// <summary>
        /// Недопустимый формат аудиозаписи.
        /// </summary>
        InvalidAudioFormat = 123,
        /// <summary>
        /// Неверный timestamp.
        /// Получить актуальное значение Вы можете методом utils.getServerTime.
        /// </summary>
        InvalidTimestamp = 150,
        /// <summary>
        /// Недопустимый идентификатор списка.
        /// </summary>
        InvalidListID = 171,
        /// <summary>
        /// Создано максимальное количество списков.
        /// </summary>
        MaxListsCount = 173,
        /// <summary>
        /// Невозможно добавить в друзья самого себя.
        /// </summary>
        CantAddYourself = 174,
        /// <summary>
        /// Невозможно добавить в друзья пользователя, который занес Вас в свой черный список. 
        /// </summary>
        CantAddIfYouBlocked = 175,
        /// <summary>
        /// Невозможно добавить в друзья пользователя, который занесен в Ваш черный список. 
        /// </summary>
        CantAddABlocked = 176,
        /// <summary>
        /// Доступ к альбому запрещен.
        /// Убедитесь, что Вы используете верные идентификаторы (для пользователей owner_id положительный, для сообществ — отрицательный), и доступ к запрашиваемому контенту для текущего пользователя есть в полной версии сайта.
        /// </summary>
        AccessToAlbumDenied = 200,
        /// <summary>
        /// Доступ к аудио запрещен.
        /// Убедитесь, что Вы используете верные идентификаторы (для пользователей owner_id положительный, для сообществ — отрицательный), и доступ к запрашиваемому контенту для текущего пользователя есть в полной версии сайта.
        /// </summary>
        AccessToAudioDenied = 201,
        /// <summary>
        /// Доступ к видео запрещен.
        /// Убедитесь, что Вы используете верные идентификаторы (для пользователей owner_id положительный, для сообществ — отрицательный), и доступ к запрашиваемому контенту для текущего пользователя есть в полной версии сайта.
        /// </summary>
        AccessToVideoDenied = 202,
        /// <summary>
        /// Доступ к группе запрещен.
        /// Убедитесь, что текущий пользователь является участником или руководителем сообщества (для закрытых и частных групп и встреч).
        /// </summary>
        AccessToGroupDenied = 203,
        /// <summary>
        /// Нет доступа к сохранению видеозаписи.
        /// </summary>
        AccessVideoSaveDenied = 204,

        /// <summary>
        /// Нет доступа для добавления поста. Превышен лимит на число публикаций в сутки, либо на указанное время уже запланирована другая запись, либо для текущего пользователя недоступно размещение записи на этой стене. 
        /// </summary>
        AccessToAddingPostDenied = 214,

        /// <summary>
        /// Рекламный пост уже недавно добавлялся.
        /// </summary>
        AdvertisingPostHasRecentlyAdded = 219,
        /// <summary>
        /// Пользователь отключил трансляцию аудиозаписей в статус.
        /// </summary>
        UserDisableTrackNameBroadcast = 221,
        /// <summary>
        /// Аудиозапись была изъята по запросу правообладателя и не может быть загружена. 
        /// </summary>
        AudioIsProhibited = 270,
        /// <summary>
        /// Нет доступа к опросу.
        /// </summary>
        NoAccessToPoll = 250,
        /// <summary>
        /// Недопустимый идентификатор опроса.
        /// </summary>
        PollIsInvalid = 251,
        /// <summary>
        /// Недопустимый идентификатор ответа.
        /// </summary>
        ResponseIsInvalid = 252,
        /// <summary>
        /// Альбом переполнен.
        /// Перед продолжением работы нужно удалить лишние объекты из альбома или использовать другой альбом.
        /// </summary>
        AlbumIsFull = 300,
        /// <summary>
        /// Неверное имя файла.
        /// </summary>
        InvalidFileName = 301,
        /// <summary>
        /// Создано максимальное количество альбомов или недопустимый размер файла.
        /// </summary>
        MaxAlbumsCountOrInvalidFileSize = 302,
        /// <summary>
        /// Действие запрещено. Вы должны включить переводы голосов в настройках приложения. 
        /// Проверьте настройки приложения: http://vk.com/editapp?id={Ваш API_ID}&section=payments
        /// </summary>
        PermissionToVotesDenied = 500,

        /// <summary>
        /// Нет прав на выполнение данных операций с рекламным кабинетом.
        /// </summary>
        PermissionToAccessObjectsDenied = 600,

        /// <summary>
        /// Произошла ошибка при работе с рекламным кабинетом.
        /// </summary>
        SomeAdsError = 603,

        /// <summary>
        /// Невозможно изменить полномочия создателя.
        /// </summary>
        CredentialsError = 700,

        /// <summary>
        /// Пользователь должен состоять в сообществе.
        /// </summary>
        UserMustBeInCommunity = 701,

        /// <summary>
        /// Достигнут лимит на количество руководителей в сообществе.
        /// </summary>
        LeadersLimitReached = 702,

        /// <summary>
        /// Видео уже добавлено.
        /// </summary>
        VideoHasAlreadyBeenAdded = 800,

        /// <summary>
        /// Комментарии к видео закрыты
        /// </summary>
        VideoCommentsClosed,

        /// <summary>
        /// Неверный идентификатор документа.
        /// </summary>
        WrongDocumentID = 1150,

        /// <summary>
        /// Нет доступа для удаления документа.
        /// </summary>
        AccessToDocumentDenied = 1151,

        /// <summary>
        /// Оригинал фотографии был изменен.
        /// </summary>
        PhotoOriginalWasChanged = 1160,

        /// <summary>
        /// Произошла ошибка соединения.
        /// </summary>
        ConnectionError = 10000,

        /// <summary>
        /// Ввод каптчи отменен.
        /// </summary>
        CaptchaCanceled = 10001,

        /// <summary>
        /// Нет интернет-соединения
        /// </summary>
        NoNetwork = 90909,

        DeserializationError = 90910,
    }
}
