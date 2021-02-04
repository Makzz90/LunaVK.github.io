using Newtonsoft.Json;
using LunaVK.Core.Json;
using System;
using Newtonsoft.Json.Converters;
using LunaVK.Core.Enums;
using System.Collections.Generic;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using System.ComponentModel;
using LunaVK.Core.Framework;
using System.IO;
using LunaVK.Core.Utils;
using System.Linq;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Запись на стене
    /// https://vk.com/dev/objects/post
    /// Topic
    /// </summary>
    public class VKWallPost : INotifyPropertyChanged, IBinarySerializable
    {
        /// <summary>
        /// Идентификатор записи на стене ее владельца.
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// Идентификатор владельца стены, на которой размещена запись.
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// Идентификатор автора записи.
        /// </summary>
        public int from_id { get; set; }

        /// <summary>
        /// идентификатор администратора, который опубликовал запись
        /// </summary>
        public int created_by { get; set; }

        /// <summary>
        /// Дата публикации записи.
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime date { get; set; }

        /// <summary>
        /// Полный текст записи.
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// идентификатор владельца записи, в ответ на которую была оставлена текущая
        /// </summary>
        public int reply_owner_id { get; set; }

        /// <summary>
        /// идентификатор записи, в ответ на которую была оставлена текущая
        /// </summary>
        public int reply_post_id { get; set; }

        /// <summary>
        /// 1, если запись была создана с опцией «Только для друзей»
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool friends_only { get; set; }

        /// <summary>
        /// Содержит информацию о комментариях к записи.
        /// </summary>
        public VKComments comments { get; set; }

        /// <summary>
        /// информация о лайках к комментарию, объект с полями
        /// </summary>
        public VKLikes likes { get; set; }

        /// <summary>
        /// Информация о репостах записи.
        /// </summary>
        public VKReposts reposts { get; set; }

        /// <summary>
        /// информация о просмотрах записи
        /// </summary>
        public VKViews views { get; set; }

        /// <summary>
        /// Тип записи.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public VKNewsfeedPostType post_type { get; set; }

        /// <summary>
        /// Способ размещения записи.
        /// </summary>
        public VKPostSource post_source { get; set; }

        /// <summary>
        /// Список вложений.
        /// </summary>
        public List<VKAttachment> attachments { get; set; }

        /// <summary>
        /// информация о местоположении
        /// </summary>
        public VKGeo geo { get; set; }

        /// <summary>
        /// идентификатор автора, если запись была опубликована от имени сообщества и подписана пользователем
        /// </summary>
        public int signer_id { get; set; }

        /// <summary>
        /// массив, содержащий историю репостов для записи.
        /// Возвращается только в том случае, если запись является
        /// репостом. Каждый из объектов массива, в свою очередь,
        /// является объектом-записью стандартного формата. 
        /// </summary>
        public List<VKWallPost> copy_history { get; set; }

        /// <summary>
        /// может ли текущий пользователь закрепить запись
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_pin { get; set; }

        /// <summary>
        /// может ли текущий пользователь удалить запись
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_delete { get; set; }

        /// <summary>
        /// может ли текущий пользователь редактировать запись
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_edit { get; set; }

        /// <summary>
        /// информация о том, что запись закреплена
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_pinned { get; set; }

        /// <summary>
        /// информация о том, содержит ли запись отметку "реклама"
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool marked_as_ads { get; set; }

        //
        //
        /// <summary>
        /// идентификатор отложенной записи; 
        /// </summary>
        public int postponed_id { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_publish { get; set; }


        public NewsActivity activity { get; set; }

        public PostCopyright copyright { get; set; }

        public bool is_favorite { get; set; }

#region VM
        public VKBaseDataForGroupOrUser Owner { get; set; }

        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName = null)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateUI()
        {
            this.NotifyPropertyChanged("likes");
            this.NotifyPropertyChanged("IsLiked");
        }

        public /*override*/ int OwnerId
        {
            get
            {
                if (this.owner_id == 0)
                    return this.from_id;
                return this.owner_id;
            }
        }

        public /*override*/ uint PostId
        {
            get { return this.id; }
        }

        public /*override*/ bool IsPinned
        {
            get { return this.is_pinned; }
        }

        public VKAdminLevel AdminLevel
        {
            get
            {
                if (this.Owner != null)
                {
                    if (this.Owner is VKGroup)
                    {
                        return (this.Owner as VKGroup).admin_level;
                    }
                }
                return VKAdminLevel.None;
            }
        }

        public bool IsSuggestedPostponed
        {
            get
            {
                if (!this.IsSuggested)
                    return this.IsPostponed;
                return true;
            }
        }

        public bool IsSuggested
        {
            get { return this.post_type == VKNewsfeedPostType.suggest; }
        }

        public bool IsPostponed
        {
            get { return this.post_type == VKNewsfeedPostType.postpone; }
        }

        public bool CanReport
        {
            get { return this.from_id != Settings.UserId && !this.IsPostponed; }
        }

        public bool IsLiked
        {
            get { return this.likes != null && this.likes.user_likes == true; }
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

                if (this.views == null && this.likes == null && this.comments == null)
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }

        public bool CanGoToOriginal
        {
            get
            {
                if (!this.IsRepost)
                    return false;
                if (this.copy_history[0].post_type != VKNewsfeedPostType.post)
                    return this.copy_history[0].post_type == VKNewsfeedPostType.reply;
                return true;
            }
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

        public Visibility AdsVisibility
        {
            get { return this.marked_as_ads.ToVisiblity(); }
        }

        public Action _deletedItemCallback;
        public Action IgnoreNewsfeedItemCallback;

        public string ExtraText
        {
            get
            {
                if (!this.GetIsProfilePhoto || this.IsRepost)
                    return UIStringFormatterHelper.FormatDateTimeForUI(this.date);

                bool isGroup = this.owner_id < 0;

                if (isGroup)
                    return LocalizedStrings.GetString("Photo_UpdatedProfileCommunity");

                return LocalizedStrings.GetString((Owner as VKUser).IsFemale ? "Photo_UpdatedProfileFemale" : "Photo_UpdatedProfileMale");
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

                return this.IsProfilePhoto(photo, this.post_source);
            }
        }

        private bool IsProfilePhoto(VKPhoto photo, VKPostSource postSource)
        {
            if (photo != null && photo.album_id == VKAlbumPhoto.ProfilePhotos && postSource != null)
                return postSource.data == "profile_photo";
            return false;
        }
        #endregion

#region IBinarySerializable
        public void Write(BinaryWriter writer)
        {
            writer.Write(2);
            writer.Write(this.id);
            //writer.Write(this.to_id);
            writer.Write(this.from_id);
            writer.Write(this.date);
            writer.WriteList(this.attachments);
            writer.Write(this.signer_id);
            writer.WriteList(this.copy_history);
            writer.Write(this.reply_post_id);
            writer.Write(this.marked_as_ads);
            writer.WriteString(this.text);
            writer.Write(this.owner_id);

            int owner = this.from_id != 0 ? this.from_id : this.owner_id;

            if (owner > 0)
                writer.Write(this.Owner as VKUser);
            else
                writer.Write(this.Owner as VKGroup);

            if(this.signer_id>0)
                writer.Write(this.Signer as VKUser);

            writer.Write(this.IsFooterHiden);
            writer.Write(this.IsRepost);
            writer.Write(this.reposts);
            writer.Write(this.copyright);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadUInt32();
            //this.to_id = reader.ReadInt64();
            this.from_id = reader.ReadInt32();
            this.date = reader.ReadDateTime();
            this.attachments = reader.ReadList<VKAttachment>();
            this.signer_id = reader.ReadInt32();
            this.copy_history = reader.ReadList<VKWallPost>();
            this.reply_post_id = reader.ReadInt32();
            this.marked_as_ads = reader.ReadBoolean();
            this.text = reader.ReadString();
            this.owner_id = reader.ReadInt32();

            int owner = this.from_id != 0 ? this.from_id : this.owner_id;

            if (owner > 0)
                this.Owner = reader.ReadGeneric<VKUser>();
            else
                this.Owner = reader.ReadGeneric<VKGroup>();

            if (this.signer_id > 0)
                this.Signer = reader.ReadGeneric<VKUser>();

            this.IsFooterHiden = reader.ReadBoolean();
            this.IsRepost = reader.ReadBoolean();
            this.reposts = reader.ReadGeneric<VKReposts>();
            this.copyright= reader.ReadGeneric<PostCopyright>();
        }
#endregion
    }
}
