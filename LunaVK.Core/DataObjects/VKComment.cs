using System;
using System.Collections.Generic;
using LunaVK.Core.Json;
using Newtonsoft.Json;
using Windows.UI.Xaml;
using System.IO;
using LunaVK.Core.Utils;
using LunaVK.Core.Framework;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Комментарий к записи на стене
    /// https://vk.com/dev/attachments_m
    /// </summary>
    public class VKComment : ViewModels.ViewModelBase, IBinarySerializable
    {
        /// <summary>
        /// идентификатор комментария
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// идентификатор автора комментария
        /// </summary>
        public int from_id { get; set; }

        /// <summary>
        /// идентификатор записи, к которой был оставлен комментарий
        /// </summary>
        public uint post_id { get; set; }

        /// <summary>
        /// идентификатор владельца стены
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// дата создания комментария в формате unixtime
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime date { get; set; }

        /// <summary>
        /// текст комментария
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// информация о лайках к комментарию, объект с полями
        /// </summary>
        public VKLikes likes { get; set; }

        /// <summary>
        /// идентификатор пользователя или сообщества,
        /// в ответ которому оставлен текущий комментарий (если применимо). 
        /// </summary>
        public int reply_to_user { get; set; }

        /// <summary>
        /// идентификатор комментария, в ответ на который оставлен текущий (если применимо).
        /// </summary>
        public uint reply_to_comment { get; set; }

        /// <summary>
        /// медиавложения комментария (фотографии, ссылки и т.п.).
        /// </summary>
        public List<VKAttachment> attachments { get; set; }

        /// <summary>
        /// массив идентификаторов родительских комментариев. 
        /// </summary>
        public List<int> parents_stack { get; set; }

        /// <summary>
        /// Информация о вложенной ветке комментариев, объект с полями: 
        /// </summary>
        public Thread thread { get; set; }

        public VKWallPost post { get; set; }

        public VKPhoto photo { get; set; }

        public VKVideoBase video { get; set; }

        public VKTopic topic { get; set; }

        public class Thread
        {
            /// <summary>
            /// количество комментариев в ветке. 
            /// </summary>
            public int count { get; set; }

            /// <summary>
            /// массив объектов комментариев к записи (только для метода wall.getComments). 
            /// </summary>
            public System.Collections.ObjectModel.ObservableCollection<VKComment> items { get; set; }

            /// <summary>
            /// может ли текущий пользователь оставлять комментарии в этой ветке. 
            /// </summary>
            [JsonConverter(typeof(VKBooleanConverter))]
            public bool can_post { get; set; }

            /// <summary>
            /// нужно ли отображать кнопку «ответить» в ветке. 
            /// </summary>
            [JsonConverter(typeof(VKBooleanConverter))]
            public bool show_reply_button { get; set; }

            /// <summary>
            /// могут ли сообщества оставлять комментарии в ветке. 
            /// </summary>
            [JsonConverter(typeof(VKBooleanConverter))]
            public bool groups_can_post { get; set; }
        }
        
#region VM
        public string _replyToUserDat { get; set; }
        public VKBaseDataForGroupOrUser User { get; set; }
        public Action ReplyTapped;
        public Action DeleteTapped;
        public Action LoadMoreInThread;

        public bool CanDelete;/*
        {
            get
            {
                var cachedGroup = Library.GroupsService.Instance.GetCachedGroup((uint)this.owner_id);

                return this.from_id == Settings.UserId || this.owner_id == Settings.UserId || this.owner_id < 0 && cachedGroup != null && (this.from_id > 0 && cachedGroup.admin_level > 0);
            }
        }*/
        public bool CanEdit;

        public bool Marked { get; set; }

        public BottomButtonData Button { get; set; }

        public Visibility ButtonVisibility
        {
            get
            {
                return this.Button == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public void RefreshUI()
        {
            base.NotifyPropertyChanged("ButtonVisibility");
            base.NotifyPropertyChanged("InLoadingThred");
            base.NotifyPropertyChanged("likes");
            base.NotifyPropertyChanged("thread");

            base.NotifyPropertyChanged("LoadMoreText");
            base.NotifyPropertyChanged("ButtonLoadMoreVisibility");
            base.NotifyPropertyChanged("InLoadingMore");
        }

        public bool InLoadingThred { get; set; }

        public bool InLoadingMore { get; set; }

        public string LoadMoreText
        {
            get
            {
                if (this.thread == null)
                    return String.Empty;

                if (this.thread.count == 0)//у фото коментов бывает
                    return String.Empty;

                return string.Format( "Загрузить ещё {0} комментариев", Math.Min(20,this.thread.count - this.thread.items.Count));
            }
        }

        public Visibility ButtonLoadMoreVisibility
        {
            get
            {
                if (this.Button != null || this.thread == null)
                    return Visibility.Collapsed;

                if(this.thread.count==0)//у фото коментов бывает
                    return Visibility.Collapsed;

                return this.thread.count - this.thread.items.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public class BottomButtonData
        {
            /// <summary>
            /// Кто последний написал комментарий?
            /// </summary>
            public VKBaseDataForGroupOrUser User { get; set; }

            public int TotalComments { get; set; }

            public string Subtitle
            {
                get
                {
                    string temp = UIStringFormatterHelper.FormatNumberOfSomething(this.TotalComments, "OneAnswerFrm", "TwoFourAnswersFrm", "FiveAnswersFrm", true);
                    return "ответил \u00b7 " + temp;
                }
            }
        }
#endregion

#region IBinarySerializable
        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            //writer.Write(this.TopicId);
            //writer.Write(this.GroupId);
            //writer.Write(this.PostId);
            //writer.Write(this.PhotoId);
            //writer.Write(this.VideoId);
            writer.Write(this.id);

            writer.Write(this.from_id);
            writer.Write(this.date);
            writer.WriteString(this.text);
            //writer.Write(this.likes);
            writer.Write(this.reply_to_user);
            writer.Write(this.reply_to_comment);
            writer.WriteList(this.attachments);
            //writer.Write(this.sticker_id);
            writer.Write(this.post_id);
            writer.Write(this.owner_id);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            //this.TopicId = reader.ReadInt64();
            //this.GroupId = reader.ReadInt64();
            //this.PostId = reader.ReadInt64();
            //this.PhotoId = reader.ReadInt64();
            //this.VideoId = reader.ReadInt64();
            this.id = reader.ReadUInt32();

            this.from_id = reader.ReadInt32();
            this.date = reader.ReadDateTime();
            this.text = reader.ReadString();
            //this.likes = reader.ReadGeneric<Likes>();
            this.reply_to_user = reader.ReadInt32();
            this.reply_to_comment = reader.ReadUInt32();
            this.attachments = reader.ReadList<VKAttachment>();
            //this.sticker_id = reader.ReadInt32();
            this.post_id = reader.ReadUInt32();
            this.owner_id = reader.ReadInt32();
        }
#endregion
    }
}
