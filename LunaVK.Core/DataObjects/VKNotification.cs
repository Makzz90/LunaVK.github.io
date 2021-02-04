using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using LunaVK.Core.Network;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// https://vk.com/dev/notifications.get
    /// </summary>
    public class VKNotification
    {
        private object _parsedParent;
        private object _parsedFeedback;

        /// <summary>
        /// тип оповещения
        /// </summary>
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public NotificationType type { get; set; }

        /// <summary>
        /// время появления ответа в формате Unixtime
        /// </summary>
        public int date { get; set; }

        /// <summary>
        /// информация о материале, к которому появился ответ
        /// (запись на стене, комментарий, фотографию, видеозапись или обсуждение)
        /// (отсутствует, если type= follow, mention или wall)
        /// </summary>
        public object parent { get; set; }

        /// <summary>
        /// объект (или массив объектов, если type равно follow, like_* или copy_*), описывающий поступивший ответ
        /// Оповещения о новых подписчиках, пометках «Мне нравится» и скопированных записях могут быть сгруппированы в виде массива
        /// </summary>
        public object feedback { get; set; }

        /// <summary>
        /// объект, описывающий комментарий текущего пользователя, отправленный в ответ на данное оповещение
        /// Отсутствует, если пользователь ещё не давал ответа. 
        /// </summary>
        public object reply { get; set; }

        public object ParsedParent
        {
            get
            {
                if (this._parsedParent != null)
                    return this._parsedParent;
                if (this.parent == null)
                    return null;
                string str = this.parent.ToString();
                switch (this.type)
                {
                    case NotificationType.mention_comments:
                    case NotificationType.comment_post:
                    case NotificationType.like_post:
                    case NotificationType.copy_post:
                        this._parsedParent = JsonConvert.DeserializeObject<VKWallPost>(str);
                        break;
                    case NotificationType.comment_photo:
                    case NotificationType.like_photo:
                    case NotificationType.copy_photo:
                    case NotificationType.mention_comment_photo:
                        this._parsedParent = JsonConvert.DeserializeObject<VKPhoto>(str);
                        break;
                    case NotificationType.comment_video:
                    case NotificationType.like_video:
                    case NotificationType.copy_video:
                    case NotificationType.mention_comment_video:
                        this._parsedParent = JsonConvert.DeserializeObject<VKVideoBase>(str);
                        break;
                    case NotificationType.reply_comment:
                    case NotificationType.reply_comment_photo:
                    case NotificationType.reply_comment_video:
                    case NotificationType.reply_comment_market:
                    case NotificationType.like_comment:
                    case NotificationType.like_comment_photo:
                    case NotificationType.like_comment_video:
                    case NotificationType.like_comment_topic:
                        this._parsedParent = JsonConvert.DeserializeObject<VKComment>(str);
                        break;
                    case NotificationType.reply_topic:
                        this._parsedParent = JsonConvert.DeserializeObject<VKTopic>(str);
                        break;

                    
                }
                return this._parsedParent;
            }
        }

        public object ParsedFeedback
        {
            get
            {
                if (this._parsedFeedback != null)
                    return this._parsedFeedback;
                string str = this.feedback.ToString();
                switch (this.type)
                {
                    case NotificationType.follow:
                    case NotificationType.friend_accepted:
                    case NotificationType.like_post:
                    case NotificationType.like_comment:
                    case NotificationType.like_photo:
                    case NotificationType.like_video:
                    case NotificationType.like_comment_photo:
                    case NotificationType.like_comment_video:
                    case NotificationType.like_comment_topic:
                        this._parsedFeedback = JsonConvert.DeserializeObject<VKCountedItemsObject<FeedbackUser>>(str).items;
                        break;
                    case NotificationType.mention:
                    case NotificationType.wall:
                    case NotificationType.wall_publish:
                        this._parsedFeedback = JsonConvert.DeserializeObject<VKWallPost>(str);
                        break;
                    case NotificationType.mention_comments:
                    case NotificationType.comment_post:
                    case NotificationType.comment_photo:
                    case NotificationType.comment_video:
                    case NotificationType.reply_comment:
                    case NotificationType.reply_topic:
                    case NotificationType.reply_comment_photo:
                    case NotificationType.reply_comment_video:
                    case NotificationType.mention_comment_photo:
                    case NotificationType.mention_comment_video:
                    case NotificationType.reply_comment_market:
                        this._parsedFeedback = JsonConvert.DeserializeObject<VKComment>(str);
                        break;
                    case NotificationType.copy_post:
                    case NotificationType.copy_photo:
                    case NotificationType.copy_video:
                        this._parsedFeedback = JsonConvert.DeserializeObject<VKCountedItemsObject<FeedbackCopyInfo>>(str).items;
                        break;
                    //case NotificationType.money_transfer_received:
                    //case NotificationType.money_transfer_accepted:
                    //case NotificationType.money_transfer_declined:
                    //    this.feedback = JsonConvert.DeserializeObject<MoneyTransfer>(str);
                    //    break;
                }
                if (this._parsedFeedback != null)
                    return this._parsedFeedback;
                if (this.parent == null)
                    return "";
                return this.parent.ToString();
            }
        }

        public enum NotificationType
        {
            /// <summary>
            /// У пользователя появился один или несколько новых подписчиков
            /// </summary>
            follow,

            /// <summary>
            /// Заявка в друзья, отправленная пользователем, была принята
            /// </summary>
            friend_accepted,

            /// <summary>
            /// Была создана запись на чужой стене, содержащая упоминание пользователя
            /// </summary>
            mention,

            /// <summary>
            /// Был оставлен комментарий, содержащий упоминание пользователя
            /// </summary>
            mention_comments,

            /// <summary>
            /// Была добавлена запись на стене пользователя
            /// </summary>
            wall,

            /// <summary>
            /// Была опубликована новость, предложенная пользователем в публичной странице. 
            /// </summary>
            wall_publish,

            /// <summary>
            /// Была опубликована новость, предложенная пользователем в публичной странице
            /// </summary>
            comment_post,
            comment_photo,
            comment_video,

            /// <summary>
            /// Был добавлен новый ответ на комментарий пользователя.
            /// </summary>
            reply_comment,
            reply_comment_photo,
            reply_comment_video,
            reply_comment_market,
            reply_topic,

            /// <summary>
            /// У записи пользователя появилась одна или несколько новых отметок «Мне нравится»
            /// </summary>
            like_post,
            like_comment,
            like_photo,
            like_video,
            like_comment_photo,
            like_comment_video,
            like_comment_topic,

            copy_post,
            copy_photo,
            copy_video,
            
            /// <summary>
            /// Под фотографией был оставлен комментарий, содержащий упоминание пользователя. 
            /// </summary>
            mention_comment_photo,
            mention_comment_video,

            
            
            //money_transfer_received,
            //money_transfer_accepted,
            //money_transfer_declined,
        }


        #region VM
        public VKBaseDataForGroupOrUser Owner;
        #endregion
    }
}
