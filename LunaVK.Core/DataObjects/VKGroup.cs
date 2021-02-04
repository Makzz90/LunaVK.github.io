using System.Collections.Generic;
using Newtonsoft.Json;
using LunaVK.Core.Enums;
using LunaVK.Core.Json;
using System;
using LunaVK.Core.Framework;
using System.IO;
using LunaVK.Core.Utils;
using LunaVK.Core.Library;

namespace LunaVK.Core.DataObjects
{
    public class VKGroup : VKBaseDataForGroupOrUser, IBinarySerializable, ISupportGroup
    {
public uint id { get; set; }

        /// <summary>
        /// Название сообщества.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Короткий адрес сообщества.
        /// </summary>
        public string screen_name { get; set; }

        /// <summary>
        /// Является ли сообщество закрытым.
        /// </summary>
        public VKGroupIsClosed is_closed { get; set; }

[JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
public VKIsDeactivated deactivated { get; set; }

        /// <summary>
        /// Является ли пользователь администратором сообщества.
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_admin { get; set; }

        /// <summary>
        /// Полномочия пользователя в сообществе.
        /// </summary>
        public VKAdminLevel admin_level { get; set; }

        /// <summary>
        /// Является ли пользователь участником сообщества.
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_member { get; set; }

        
        

        /// <summary>
        /// идентификатор пользователя, который отправил приглашение в сообщество
        /// </summary>
        public int invited_by { get; set; }

        /// <summary>
        /// тип сообщества
        /// </summary>
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public VKGroupType type { get; set; }

public string photo_50 { get; set; }

public string photo_100 { get; set; }

public string photo_200 { get; set; }






        /// <summary>
        /// строка состояния публичной страницы. У групп возвращается строковое значение, открыта ли группа или нет, а у событий дата начала. 
        /// </summary>
        public string activity { get; set; }

        /// <summary>
        /// возрастное ограничение.
        /// 1 — нет; 
        /// 2 — 16+; 
        /// 3 — 18+. 
        /// </summary>
        public int age_limits { get; set; }

        /// <summary>
        /// информация о занесении в черный список сообщества
        /// </summary>
        public VKBanInfo ban_info { get; set; }

        /// <summary>
        /// может ли текущий пользователь создать новое обсуждение в группе
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_create_topic { get; set; }

        /// <summary>
        /// может ли текущий пользователь написать сообщение сообществу
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_message { get; set; }

[JsonConverter(typeof(VKBooleanConverter))]
public bool can_post { get; set; }

[JsonConverter(typeof(VKBooleanConverter))]
public bool can_see_all_posts { get; set; }

public VKCounters counters { get; set; }

        /// <summary>
        /// может ли текущий пользователь загружать документы в группу
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_upload_doc { get; set; }

        /// <summary>
        /// может ли текущий пользователь загружать видеозаписи в группу
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_upload_video { get; set; }

        /// <summary>
        /// город, указанный в информации о сообществе
        /// </summary>
        public VKCity city { get; set; }

        /// <summary>
        /// информация из блока контактов публичной страницы
        /// </summary>
        public List<VKGroupContact> contacts { get; set; }

        

        /// <summary>
        /// страна, указанная в информации о сообществе
        /// </summary>
        public VKCountry country { get; set; }


        public VKCover cover { get; set; }

        public CropPhoto crop_photo { get; set; }

        /// <summary>
        /// текст описания сообщества
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// идентификатор закрепленной записи
        /// </summary>
        public int fixed_post { get; set; }

        /// <summary>
        /// информация о том, установлена ли у сообщества главная фотография
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool has_photo { get; set; }

[JsonConverter(typeof(VKBooleanConverter))]
public bool is_favorite { get; set; }

[JsonConverter(typeof(VKBooleanConverter))]
public bool is_hidden_from_feed { get; set; }

        /// <summary>
        /// заблокированы ли сообщения от этого сообщества
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_messages_blocked { get; set; }

        /// <summary>
        /// информация из блока ссылок сообщества
        /// </summary>
        public List<VKGroupLink> links { get; set; }

        /// <summary>
        /// идентификатор основного фотоальбома
        /// </summary>
        public int main_album_id { get; set; }

        /// <summary>
        /// информация о главной секции
        /// </summary>
        public VKGroupMainSection main_section { get; set; }

        /// <summary>
        /// информация о магазине
        /// </summary>
        public VKGroupMarket market { get; set; }

        /// <summary>
        /// статус участника текущего пользователя
        /// 0 — не является участником; 
        /// 1 — является участником; 
        /// 2 — не уверен, что посетит мероприятие; 
        /// 3 — отклонил приглашение; 
        /// 4 — подал заявку на вступление; 
        /// 5 — приглашен. 
        /// </summary>
        public VKGroupMembershipType member_status { get; set; }

        /// <summary>
        /// количество участников
        /// </summary>
        public int members_count { get; set; }

        /// <summary>
        /// место, указанное в информации о сообществе
        /// </summary>
        public VKPlace place { get; set; }

        /// <summary>
        /// Текст описания для поля start_date
        /// (возвращается для публичных страниц)
        /// </summary>
        public string public_date_label { get; set; }

        /// <summary>
        /// адрес сайта из поля «веб-сайт» в описании сообщества
        /// </summary>
        public string site { get; set; }

        /// <summary>
        /// для встреч содержат время начала и окончания встречи в формате unixtime
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime start_date { get; set; }

        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime finish_date { get; set; }
        
        /// <summary>
        /// статус сообщества
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// информация о том, есть ли у сообщества «огонёк»
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool trending { get; set; }

[JsonConverter(typeof(VKBooleanConverter))]
public bool verified { get; set; }

        /// <summary>
        /// название главной вики-страницы
        /// </summary>
        public string wiki_page { get; set; }


#region UNDOCUMENTED
        /// <summary>
        /// Пользователь подписан на уведомления?
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_subscribed { get; set; }

        /// <summary>
        /// Рекламодатель
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_advertiser { get; set; }

        public VideoLiveClass video_live { get; set; }

        public class VideoLiveClass
        {
            [JsonConverter(typeof(VKBooleanConverter))]
            public bool enabled { get; set; }

            [JsonConverter(typeof(VKBooleanConverter))]
            public bool is_notifications_blocked { get; set; }
        }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool using_vkpay_market_app { get; set; }


        //[JsonConverter(typeof(VKBooleanConverter))] оно и так возвращается текстом
        public bool has_market_app { get; set; }


        public AppButton action_button { get; set; }//app_button

        public LiveCovers live_covers { get; set; }
        public class LiveCovers
        {
            //{"is_enabled":true,"story_ids":["-16315_1399511"]}
            [JsonConverter(typeof(VKBooleanConverter))]
            public bool is_enabled { get; set; }

            [JsonConverter(typeof(VKBooleanConverter))]
            public bool is_scalable { get; set; }

            //public List<VKStory> items { get; set; }
            public List<string> story_ids { get; set; }
        }

        public VKGroupMainSection secondary_section { get; set; }

        public OnlineStatus online_status { get; set; }

        public class OnlineStatus
        {
            /// <summary>
            /// none answer_mark online
            /// </summary>
            public string status { get; set; }

            public int minutes { get; set; }
        }

        public string phone { get; set; }

        
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_video_live_notifications_blocked { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_upload_story { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_ask { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_ask_anonymous { get; set; }
        #endregion

        /// <summary>
        /// стена. Возможные значения:
        ///0 — выключена;
        ///1 — открытая;
        ///2 — ограниченная;
        ///3 — закрытая.
        /// </summary>
        public int wall { get; set; }

        public string domain { get; set; }

        

        /*
         * 
         * "addresses": {
"is_enabled": true,
"main_address_id": 54248
},
         * */

#region VM
        public string UIMembersCount
        {
            get
            {
                return UIStringFormatterHelper.FormatNumberOfSomething(this.members_count, "OneMemberFrm", "TwoFourMembersFrm", "FiveMembersFrm");
            }
        }

        public string Inviter
        {
            get;
            set;
        }
        
        public bool CanJoin
        {
            get
            {
                if ((this.member_status != VKGroupMembershipType.Member && this.member_status != VKGroupMembershipType.NotSure && this.is_closed != VKGroupIsClosed.Private || this.member_status == VKGroupMembershipType.InvitationReceived) && (this.ban_info == null || this.ban_info.end_date > 0))
                    return this.deactivated == VKIsDeactivated.None;
                return false;
            }
        }

        public string City
        {
            get
            {
                if (this.city == null)
                    return "";

                return this.city.title;
            }
        }

        public bool IsModeratorOrHigher
        {
            get
            {
                return (byte)this.admin_level >= 1;
            }
        }

        public bool IsEditorOrHigher
        {
            get
            {
                return (byte) this.admin_level >= 2;
            }
    }

        public bool IsAdminOrHigher
        {
        get
        {
            return (byte)this.admin_level >= 3;
        }
    }
#endregion

#region VKBaseDataForGroupOrUser
        /// <summary>
        /// -id
        /// </summary>
        public int Id
        {
            get { return (int)(-this.id); }
        }

        public string Title
        {
            get { return this.name; }
        }

        public string MinPhoto
        {
            get
            {
                string str = this.photo_50;
                if (string.IsNullOrEmpty(str))
                    str = this.photo_100;
                if (string.IsNullOrEmpty(str))
                    str = this.photo_200;
                return str;
            }
        }

        public bool IsVerified
        {
            get { return this.verified; }
        }

        public VKIsDeactivated Deactivated
        {
            get { return this.deactivated; }
        }

        public VKCounters Counters
        {
            get { return this.counters; }
        }
        
        public bool IsSubscribed
        {
            get
            {
                return this.is_subscribed;
            }
            set
            {
                this.is_subscribed = value;
            }
        }

        public bool IsFavorite
        {
            get
            {
                return this.is_favorite;
            }
            set
            {
                this.is_favorite = value;
            }
        }
        
#endregion

        public Network.VKCountedItemsObject<VKUser> friends { get; set; }

        public List<VKUser> contactsUsers { get; set; }

#region ISupportGroup
        public string Key
        {
            get
            {
                return LocalizedStrings.GetString(this.start_date > DateTime.Now ? "GroupsListPage_FutureEvents" : "GroupsListPage_PastEvents");
            }
        }
 #endregion

        public VKGroupMainSection MainSectionType
        {
            get
            {
                return this.main_section;
            }
        }

#region IBinarySerializable
        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.id);
            writer.WriteString(this.name);
            writer.WriteString(this.photo_50);
            writer.WriteString(this.photo_100);
            writer.Write(this.verified);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadUInt32();
            this.name = reader.ReadString();
            this.photo_50 = reader.ReadString();
            this.photo_100 = reader.ReadString();
            this.verified = reader.ReadBoolean();
        }
#endregion
    }
}
