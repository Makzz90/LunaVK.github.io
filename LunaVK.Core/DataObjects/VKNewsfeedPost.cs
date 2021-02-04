using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using LunaVK.Core.Json;
using LunaVK.Core.Enums;
using Windows.UI.Xaml;
using System.ComponentModel;
using LunaVK.Core.Network;
using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System.IO;
using System.Linq;

//NewsItem

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// https://vk.com/dev/newsfeed.get
    /// NewsItem
    /// </summary>
    public class VKNewsfeedPost : INotifyPropertyChanged, IBinarySerializable
    {
        /// <summary>
        /// тип списка новости, соответствующий одному из значений параметра filters
        /// </summary>        
        [JsonConverter(typeof(StringEnumConverter))]
        public VKNewsfeedFilters type { get; set; }

        /// <summary>
        /// Идентификатор источника новости (положительный — новость пользователя, отрицательный — новость группы);
        /// </summary>
        public int source_id { get; set; }

        /// <summary>
        /// время публикации новости в формате unixtime;
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime date { get; set; }

        /// <summary>
        /// Идентификатор записи на стене владельца.
        /// </summary>
        public uint post_id { get; set; }

        /// <summary>
        /// находится в записях со стен, содержит тип новости (post или copy); 
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public VKNewsfeedPostType post_type { get; set; }

        /// <summary>
        /// Способ размещения записи.
        /// </summary>
        public VKPostSource post_source { get; set; }

        /// <summary>
        /// передается в случае, если этот пост сделан при удалении; 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool final_post { get; set; }

        /// <summary>
        /// Идентификатор владельца стены, у которого была скопирована запись.
        /// </summary>
        public int copy_owner_id { get; set; }

        /// <summary>
        /// Идентификатор записи на стене пользователя,
        /// у которго была скопирована запись.
        /// </summary>
        public int copy_post_id { get; set; }

        /// <summary>
        /// — массив, содержащий историю репостов для записи.
        /// Возвращается только в том случае, если запись является репостом.
        /// Каждый из объектов массива, в свою очередь, является объектом-записью стандартного формата.
        /// БЫВАЕТ ПУСТЫМ
        /// </summary>
        public List<VKNewsfeedPost> copy_history { get; set; }

        /// <summary>
        /// Дата скопированной записи.
        /// находится в записях со стен, если сообщение является копией сообщения с чужой стены
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime copy_post_date { get; set; }

        /// <summary>
        /// находится в записях со стен и содержит текст записи; 
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// содержит 1, если текущий пользователь может редактировать запись; 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_edit { get; set; }

        /// <summary>
        /// возвращается, если пользователь может удалить новость, всегда содержит 1; 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_delete { get; set; }

        /// <summary>
        /// Информация о комментариях к записи.
        /// </summary>
        public VKComments comments { get; set; }

        /// <summary>
        /// Информация об отметках Мне нравится.
        /// </summary>
        public VKLikes likes { get; set; }

        /// <summary>
        /// Информация о репостах.
        /// </summary> 
        public VKReposts reposts { get; set; }

        /// <summary>
        /// Список вложений.
        /// </summary>
        public List<VKAttachment> attachments { get; set; }

        /// <summary>
        /// информация о местоположении
        /// </summary>
        public VKGeo geo { get; set; }


        /*
         * находятся в объектах соответствующих типов (кроме записей со стен) и содержат
         * информацию о количестве объектов и до 5 последних объектов,
         * связанных с данной новостью.
         * */
        public VKCountedItemsObject<VKPhoto> photos { get; set; }
        public VKCountedItemsObject<VKPhoto> photo_tags { get; set; }
        public List<VKNotes> notes { get; set; }
        public VKCountedItemsObject<VKFriends> friends { get; set; }

        /// <summary>
        /// (нет в документации)
        /// </summary>
        public VKCountedItemsObject<VKVideoBase> video { get; set; }

        /// <summary>
        /// Идентификатор владельца копируемой новости
        /// (нет в документации)
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// Появляется при поиске
        /// (нет в документации)
        /// </summary>
        public uint id { get; set; }


        /// <summary>
        /// Количество просмотров новости
        /// (нет в документации)
        /// </summary>
        public VKViews views { get; set; }

        public class VKFriends
        {
            public int uid { get; set; }
        }

        public class VKNotes
        {
            /// <summary>
            /// идентификатор заметки
            /// </summary>
            public int id { get; set; }

            /// <summary>
            /// идентификатор владельца заметки
            /// </summary>
            public int owner_id { get; set; }

            /// <summary>
            /// заголовок заметки
            /// </summary>
            public string title { get; set; }

            /// <summary>
            /// количество комментариев к заметке
            /// </summary>
            public int comments { get; set; }
        }

        /// <summary>
        /// информация о том, содержит ли запись отметку "реклама"
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool marked_as_ads { get; set; }

        /// <summary>
        /// идентификатор автора, если запись была опубликована от имени сообщества и подписана пользователем
        /// </summary>
        public int signer_id { get; set; }

        public PostCopyright copyright { get; set; }

        public bool is_favorite { get; set; }

        #region ADs
        //https://vk.com/dev/feed_ads

        /// <summary>
        /// заголовок рекламного блока. 
        /// </summary>
        public string ads_title { get; set; }

        /// <summary>
        /// ID позиции в новостной ленте, часть 1. 
        /// </summary>
        public int ads_id1 { get; set; }

        /// <summary>
        /// ID позиции в новостной ленте, часть 1. 
        /// </summary>
        public int ads_id2 { get; set; }

        public List<VKAd> ads { get; set; }

        public string track_code { get; set; }
#endregion

 #region VM
        public void UpdateUI()
        {
            this.NotifyPropertyChanged(nameof(this.likes));
            this.NotifyPropertyChanged(nameof(this.IsLiked));
        }
        
        public bool IsLiked
        {
            get { return this.likes != null && this.likes.user_likes == true; }
        }

        public bool IsPinned
        {
            get { return false; }//todo: избавится от этого
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName = null)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public VKBaseDataForGroupOrUser Owner { get; set; }
        
        public /*override*/ int OwnerId
        {
            get
            {
                if (this.source_id == 0)
                    return owner_id;
                return this.source_id;
            }
        }

        public /*override*/ uint PostId
        {
            get
            {
                if (this.post_id == 0)
                    return this.id;
                return this.post_id;
            }
        }
        
        public uint PhotoTagsCount
        {
            get
            {
                if (this.photo_tags != null)
                    return this.photo_tags.count;
                return 0;
            }
        }

        public uint PhotosCount
        {
            get
            {
                if (this.photos != null)
                    return this.photos.count;
                return 0;
            }
        }

        public List<VKPhoto> Photo_tags
        {
            get
            {
                if (this.photo_tags != null)
                    return this.photo_tags.items;
                return null;
            }
        }

        public List<VKPhoto> Photos
        {
            get
            {
                if (this.photos != null)
                    return this.photos.items;
                return null;
            }
        }

        public bool CanReport
        {
            get
            {
                //return this.DataPost.CanReport();
                return this.OwnerId != Settings.UserId;
            }
        }

        public bool CanLike
        {
            get
            {
                if (this.likes == null)
                    return false;

                return this.likes.can_like || this.likes.user_likes;
            }
        }

        public bool IsRepost { get; set; }

        public bool IsFooterHiden { get; set; }

        public Visibility FooterVisibility
        {
            get
            {
                if (this.IsFooterHiden)
                    return Visibility.Collapsed;

                if(this.views == null && this.likes == null && this.comments == null)
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }

        public string GetCommentsHeaderText
        {
            get
            {
                if (this.comments == null)
                    return "";

                return UIStringFormatterHelper.FormatNumberOfSomething((int)this.comments.count, "LastOfOneCommentFrm", "LastOfTwoFourCommentsFrm", "LastOfFiveCommentsFrm");
            }
        }

        public string ExtraText
        {
            get
            {
                if (!this.GetIsProfilePhoto || IsRepost)
                    return UIStringFormatterHelper.FormatDateTimeForUI(this.date);

                bool isGroup = this.source_id < 0;

                if (isGroup)
                    return LocalizedStrings.GetString("Photo_UpdatedProfileCommunity");
                //if (!isMale)
                //    return LocalizedStrings.GetString("Photo_UpdatedProfileFemale");
                return LocalizedStrings.GetString("Photo_UpdatedProfileMale");
            }
        }
        
        private bool GetIsProfilePhoto
        {
            get
            {
                VKPhoto photo;
                if (this.attachments == null)
                {
                    photo = null;
                }
                else
                {
                    var m0 = this.attachments.FirstOrDefault((a => a.type == VKAttachmentType.Photo));
                    photo = m0 != null ? m0.photo : null;
                }

                if (photo == null)
                    return false;

                return IsProfilePhoto(photo, this.post_source);
            }
        }

        private bool IsProfilePhoto(VKPhoto photo, VKPostSource postSource)
        {
            if (photo != null && photo.album_id == VKAlbumPhoto.ProfilePhotos && postSource != null)
                return postSource.data == "profile_photo";
            return false;
        }

        public Visibility AdsVisibility
        {
            get { return this.marked_as_ads.ToVisiblity(); }
        }

        public VKBaseDataForGroupOrUser Signer { get; set; }

        public Visibility SignerVisibility
        {
            get
            {
                if (this.Signer == null || this.signer_id == 0)
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }

        public Visibility CopyrightVisibility
        {
            get
            {
                if (this.copyright == null)
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }

        public string CopyrightText
        {
            get
            {
                if (this.copyright == null)
                    return "";

                return "Источник: " + this.copyright.name;
            }
        }
        /*
        private static string GetExtraText(bool isProfilePhoto, bool isRepost, bool isMale, bool isGroup)
        {
            if (!isProfilePhoto || isRepost)
                return "";
            if (isGroup)
                return CommonResources.Photo_UpdatedProfileCommunity;
            if (!isMale)
                return CommonResources.Photo_UpdatedProfileFemale;
            return CommonResources.Photo_UpdatedProfileMale;
        }
        private static string GetExtraTextEnd(bool isReply)
        {
            if (!isReply)
                return "";
            return CommonResources.OnPost;
        }
        */
        //can_doubt_category
        //can_set_category
        public Action IgnoreNewsfeedItemCallback;
        public Action HideSourceItemsCallback;
        public Action _deletedItemCallback;
#endregion

#region IBinarySerializable
        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.id);
            writer.Write(this.date);
            writer.WriteList(this.attachments);
            writer.WriteList(this.copy_history);//уже во вложениях оно есть :(
            writer.WriteString(this.text);
            writer.Write(this.owner_id);
            writer.Write(this.source_id);
            writer.Write(this.views);
            writer.Write(this.likes);
            writer.Write(this.comments);
            writer.Write(this.post_id);

            int owner = this.source_id != 0 ? this.source_id : this.owner_id;

            if (owner > 0)
                writer.Write(this.Owner as VKUser);
            else
                writer.Write(this.Owner as VKGroup);
            
            writer.Write(this.IsFooterHiden);
            writer.Write(this.IsRepost);
            writer.Write(this.reposts);
            writer.Write(this.marked_as_ads);
            writer.Write(this.signer_id);

            if (this.signer_id > 0)
                writer.Write(this.Signer as VKUser);

            writer.Write(this.copyright);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadUInt32();
            this.date = reader.ReadDateTime();
            this.attachments = reader.ReadList<VKAttachment>();
            this.copy_history = reader.ReadList<VKNewsfeedPost>();
            this.text = reader.ReadString();
            this.owner_id = reader.ReadInt32();
            this.source_id = reader.ReadInt32();
            this.views = reader.ReadGeneric<VKViews>();
            this.likes = reader.ReadGeneric<VKLikes>();
            this.comments = reader.ReadGeneric<VKComments>();
            this.post_id = reader.ReadUInt32();

            int owner = this.source_id != 0 ? this.source_id : this.owner_id;

            if (owner > 0)
                this.Owner = reader.ReadGeneric<VKUser>();
            else
                this.Owner = reader.ReadGeneric<VKGroup>();
            
            this.IsFooterHiden = reader.ReadBoolean();
            this.IsRepost = reader.ReadBoolean();
            this.reposts = reader.ReadGeneric<VKReposts>();
            this.marked_as_ads = reader.ReadBoolean();
            this.signer_id = reader.ReadInt32();

            if (this.signer_id > 0)
                this.Signer = reader.ReadGeneric<VKUser>();

            this.copyright = reader.ReadGeneric<PostCopyright>();
        }
#endregion
    }
}
