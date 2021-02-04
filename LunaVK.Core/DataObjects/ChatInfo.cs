using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using LunaVK.Core.Json;
using Windows.UI.Xaml;
using LunaVK.Core.Utils;

namespace LunaVK.Core.DataObjects
{
    public class ChatInfo
    {
        public Chat chat { get; set; }

        /// <summary>
        /// Участники чата
        /// </summary>
        public List<ChatMember> chat_participants { get; set; }

        public List<VKUser> invited_by_users { get; set; }

        /// <summary>
        /// Объект, описывающий чат
        /// </summary>
        public class Chat
        {
            /// <summary>
            /// Идентификатор беседы
            /// </summary>
            public int id { get; set; }

            /// <summary>
            /// Тип диалога
            /// chat
            /// </summary>
            public string type { get; set; }

            /// <summary>
            /// Название беседы
            /// </summary>
            public string title { get; set; }

            /// <summary>
            /// Идентификатор создателя беседы
            /// </summary>
            public string admin_id { get; set; }

            
            /// <summary>
            /// Список участников беседы
            /// </summary>
            public List<int> users { get; set; }

            //public List<ChatMember> users { get; set; }

            public int members_count { get; set; }//no in documentation

            /// <summary>
            /// Настройки оповещения для беседы
            /// </summary>
            public VKPushSettings push_settings { get; set; }
            
            /// <summary>
            /// Обложка
            /// </summary>
            public string photo_50 { get; set; }
            public string photo_100 { get; set; }
            public string photo_200 { get; set; }

            /// <summary>
            /// Пользователь покинул беседу?
            /// </summary>
            [JsonConverter(typeof(VKBooleanConverter))]
            public bool left { get; set; }

            /// <summary>
            /// Пользователя выкинули из беседы?
            /// </summary>
            [JsonConverter(typeof(VKBooleanConverter))]
            public bool kicked { get; set; }

            public bool AreDisabledNow
            {
                get
                {
                    if (this.push_settings == null)
                        return false;
                    if (this.push_settings.disabled_forever)
                        return true;
                    if (this.push_settings.disabled_until == -1)
                        return true;
                    if (this.push_settings.disabled_until > 0)
                        return Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, true) < this.push_settings.disabled_until;
                    return false;
                }
            }
        }

        public class ChatMember : VKUser
        {
            public int invited_by { get; set; }

            public ChatMember()
            {

            }

            public ChatMember(VKUser user, VKBaseDataForGroupOrUser invitedByUser, long chatCreatorId)
            {
                //this._user = user;
                this._invitedByUser = (VKUser)invitedByUser;
                this._chatCreatorId = chatCreatorId;
                long loggedInUserId = Settings.UserId;
            }

            #region VM
            public Visibility ExcludeButtonVisibility
            {
                get
                {
                    long loggedInUserId = Settings.UserId;
                    if (this.id == loggedInUserId)
                        return Visibility.Collapsed;

                    //return this._chatCreatorId != loggedInUserId && this._invitedByUser.id != loggedInUserId || this.id == loggedInUserId ? Visibility.Collapsed : Visibility.Visible;
                    return this._chatCreatorId == loggedInUserId || this._invitedByUser.Id == loggedInUserId ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            //public VKProfileBase _user;
            public VKUser _invitedByUser;

            public long _chatCreatorId;
            /*
            public string Name
            {
                get
                {
                    return this._user.Title;
                }
            }

            public string Photo
            {
                get
                {
                    return this._user.photo_100;
                }
            }
            */
            public string Information
            {
                get
                {
                    if (this.id == this._chatCreatorId)
                        return LocalizedStrings.GetString("ChatCreator");
                    return string.Format(LocalizedStrings.GetString(this._invitedByUser.sex != Enums.VKUserSex.Female ? "InvitedToChatBy_M" : "InvitedToChatBy_F"), this._invitedByUser.Title);
                }
            }

            //private Visibility _excludeButtonVisibility;
            //public Visibility ExcludeButtonVisibility { get; set; }
            #endregion
        }
    }
}
