namespace LunaVK.Core.Enums
{
    public enum LongPollServerUpdateType
    {
        /// <summary>
        /// Замена флагов сообщения (FLAGS:=$flags).
        /// $message_id (integer)
        /// $flags (integer)
        /// extra_fields* 
        /// </summary>
        ReplaceMsgFlag = 1,

        /// <summary>
        /// Установка флагов сообщения (FLAGS|=$mask).
        /// $message_id (integer)
        /// $flags (integer)
        /// extra_fields* 
        /// </summary>
        ProcessAddFlags,

        /// <summary>
        /// Сброс флагов сообщения (FLAGS&=~$mask).
        /// </summary>
        ClearMsgFlags,

        /// <summary>
        /// Добавление нового сообщения.
        /// Возможно и системных?
        /// </summary>
        MessageAdd,

        /// <summary>
        /// Редактирование сообщения.
        /// $message_id (integer)
        /// $mask(integer)
        /// $peer_id(integer)
        /// $timestamp(integer)
        /// $new_text(string)
        /// [$attachments](array)0 
        /// </summary>
        MessageUpdate,//5
        
        /// <summary>
        /// Прочтение всех входящих сообщений в $peer_id, пришедших до сообщения с $local_id.
        /// </summary>
        IncomingMessagesRead,

        /// <summary>
        /// Прочтение всех исходящих сообщений в $peer_id, пришедших до сообщения с $local_id.
        /// </summary>
        OutcominggMessagesRead,

        /// <summary>
        /// Друг $user_id стал онлайн. $extra не равен 0, если в mode был передан флаг 64. В младшем байте (остаток от деления на 256) числа extra лежит идентификатор платформы (см. 7. Платформы).
        /// $timestamp — время последнего действия пользователя $user_id на сайте.
        /// </summary>
        UserBecameOnline,//8

        /// <summary>
        /// Друг $user_id стал оффлайн ($flags равен 0, если пользователь покинул сайт и 1, если оффлайн по таймауту ) .
        /// $timestamp — время последнего действия пользователя $user_id на сайте.
        /// </summary>
        UserBecameOffline,

        /// <summary>
        /// Сброс флагов диалога $peer_id. Соответствует операции (PEER_FLAGS &=~ $flags).
        /// Только для диалогов сообществ.
        /// </summary>
        ClearPeerFlags,

        /// <summary>
        /// Замена флагов диалога $peer_id. Соответствует операции (PEER_FLAGS := $flags).
        /// Только для диалогов сообществ.
        /// </summary>
        ReplacePeerFlag,

        /// <summary>
        /// Установка флагов диалога $peer_id. Соответствует операции (PEER_FLAGS|= $flags).
        /// Только для диалогов сообществ. 
        /// </summary>
        ChatParamsWereChanged,

        /// <summary>
        /// Удаление всех сообщений в диалоге $peer_id с идентификаторами вплоть до $local_id.
        /// </summary>
        ChatDeleteMsgs,
        
        /// <summary>
        /// Восстановление недавно удаленных сообщений в диалоге $peer_id с идентификаторами вплоть до $local_id.
        /// </summary>
        RestoreMsgs,
        
        /// <summary>
        /// Один из параметров (состав, тема) беседы $chat_id были изменены.
        /// $self — 1 или 0 (вызваны ли изменения самим пользователем).
        /// </summary>
        ChatParamsChanged = 51,

        /*
         52,6,2000000017,460389]
         * 52
         * action_type?
         * peer_id
         * user_id
         * */
        /// <summary>
        /// Изменение информации чата $peer_id с типом $type_id
        /// </summary>
        ChatInfoChanged = 52,

        /// <summary>
        /// Пользователь $user_id набирает текст в диалоге. Событие приходит раз в ~5 секунд при наборе текста.
        /// $flags = 1.
        /// </summary>
        UserIsTyping = 61,

        /// <summary>
        /// Пользователь $user_id набирает текст в беседе $chat_id.
        /// </summary>
        UserIsTypingInChat,

        UserIsTyping2,//в новой версии лонгпула [63,375988312,[375988312],1,1553923516]

        /// <summary>
        /// Нет в документации
        /// </summary>
        UserIsRecordingVoice = 64,

        /// <summary>
        /// Пользователь $user_id совершил звонок с идентификатором $call_id.
        /// </summary>
        UserCalled = 70,
        
        /// <summary>
        /// Счетчик в левом меню стал равен $count.
        /// </summary>
        NewCounter = 80,

        /// <summary>
        /// Изменились настройки оповещений. $peer_id — идентификатор чата/собеседника,
        /// '$sound — 1/0, включены/выключены звуковые оповещения,
        /// $disabled_until — выключение оповещений на необходимый срок (-1: навсегда, ''0 
        /// </summary>
        processnotyfysettings = 114,

        /// <summary>
        /// Входящий звонок
        /// {"type":"decline","subtype":"handled_by_another_instance","user_id":"460389","sessionGuid":"359f22b21697d5f2acfb856189c31267","msg_hash":"5c873878c45e8"}
        /// </summary>
        IncommingCall,



        //Кастомные значения, для флагов
        MessageHasBeenDeleted = 200,
        MessageHasBeenRestored = 201,
        MessageHasBeenRead = 202,
    }
}

/* off vk
public enum LongPollServerUpdateType
  {
    IncomingMessagesRead = 6,
    UserBecameOnline = 8,
    UserBecameOffline = 9,
    MessageHasBeenRead = 10,
    MessageHasBeenAdded = 11,
    ChatParamsWereChanged = 12,
    MessageHasBeenDeleted = 20,
    UserIsTyping = 61,
    UserIsTypingInChat = 62,
    NewCounter = 80,
    MessageHasBeenRestored = 1000000,
  }
*/


/*
Флаги сообщений
Каждое сообщение имеет флаг — значение, полученное суммированием любых из следующих параметров. 

+1 	UNREAD 	сообщение не прочитано
+2 	OUTBOX 	исходящее сообщение
+4 	REPLIED 	на сообщение был создан ответ
+8 	IMPORTANT 	помеченное сообщение
+16 	CHAT 	сообщение отправлено через чат. Обратите внимание, этот флаг устаревший и вскоре перестанет поддерживаться.
+32 	FRIENDS 	сообщение отправлено другом. Не применяется для сообщений из групповых бесед.
+64 	SPAM 	сообщение помечено как "Спам"
+128 	DELЕTЕD 	сообщение удалено (в корзине)
+256 	FIXED 	сообщение проверено пользователем на спам. Обратите внимание, этот флаг устаревший и вскоре перестанет поддерживаться.
+512 	MEDIA 	сообщение содержит медиаконтент. Обратите внимание, этот флаг устаревший и вскоре перестанет поддерживаться.
+65536 	HIDDEN 	приветственное сообщение от сообщества. Диалог с таким сообщением не нужно поднимать в списке (отображать его только при открытии диалога напрямую). Флаг недоступен для версий <2.
+131072 		сообщение удалено для всех получателей. Флаг недоступен для версий <3. 
*/