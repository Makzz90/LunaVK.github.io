using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using LunaVK.Core.Json;
using LunaVK.Core.Enums;
using System.Linq;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LunaVK.Core.Library;
using LunaVK.Core.Framework;
using System.IO;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Объект, описывающий личное сообщение
    /// https://vk.com/dev/objects/message
    /// </summary>
    public class VKMessage : INotifyPropertyChanged, IBinarySerializable, ISupportGroup
    {
        /// <summary>
        /// Идентификатор сообщения (отсутствует у пересланных).
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// Дата отправки сообщения.
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime date { get; set; }

        /// <summary>
        /// идентификатор назначения.
        /// Для пользователя: id пользователя.
        /// Для групповой беседы: 2000000000 + id беседы.
        /// Для сообщества: -id сообщества
        /// </summary>
        public int peer_id { get; set; }

        /// <summary>
        /// идентификатор отправителя. 
        /// </summary>
        public int from_id { get; set; }

        /// <summary>
        /// Текст сообщения.
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// идентификатор, используемый при отправке сообщения
        /// Возвращается только для исходящих сообщений
        /// </summary>
        public int random_id { get; set; }

        /// <summary>
        /// произвольный параметр для работы с источниками переходов. 
        /// </summary>
        public string @ref { get; set; }

        /// <summary>
        /// произвольный параметр для работы с источниками переходов. 
        /// </summary>
        public string ref_source { get; set; }

        /// <summary>
        /// медиавложения сообщения (фотографии, ссылки и т.п.)
        /// nullble
        /// </summary>
        public List<VKAttachment> attachments { get; set; }

        /// <summary>
        /// Является ли сообщение важным.
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool important { get; set; }

        /// <summary>
        /// Информация о местоположении.
        /// </summary>
        public VKGeoInMsg geo { get; set; }

        /// <summary>
        /// сервисное поле для сообщений ботам (полезная нагрузка).
        /// </summary>
        public string payload { get; set; }

        /// <summary>
        /// Коллекция пересланных сообщений (если есть).
        /// Максимальное количество элементов — 100
        /// Максимальная глубина вложенности для пересланных сообщений — 45,
        /// общее максимальное количество в цепочке с учетом вложенности — 500.
        /// </summary>
        public List<VKMessage> fwd_messages { get; set; }

        /// <summary>
        /// сообщение, в ответ на которое отправлено текущее. 
        /// </summary>
        public VKMessage reply_message { get; set; }

        /// <summary>
        /// Тип служебного сообщения, если применимо.
        /// null`ble
        /// </summary>
        public MsgAction action { get; set; }

        public VKBotKeyboard keyboard { get; set; }







        /// <summary>
        /// Тип сообщения
        /// 0 — полученное
        /// 1 — отправленное, не возвращается для пересланных сообщений
        /// </summary>
        public VKMessageType @out { get; set; }

        //[JsonConverter(typeof(VKBooleanConverter))]
        public bool is_hidden { get; set; }

        /// <summary>
        /// Дата редактирования сообщения
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime update_time { get; set; }

        public int conversation_message_id { get; set; }

        public class MsgAction : IBinarySerializable
        {
            public VKChatMessageActionType type { get; set; }

            /// <summary>
            /// Идентификатор пользователя (если > 0) или email (если меньше 0),
            /// пригласили или исключили
            /// для служебных сообщений с action = chat_invite_user, chat_invite_user_by_link или chat_kick_user
            /// Идентификатор пользователя, который закрепил/открепил сообщение для action = chat_pin_message или chat_unpin_message.
            /// </summary>
            public int member_id { get; set; }

            /// <summary>
            /// Название беседы.
            /// для служебных сообщений с action = chat_create или chat_title_update
            /// Текст закрепленного сообщения для action = chat_pin_message. 
            /// </summary>
            public string text { get; set; }

            /// <summary>
            /// Email, который пригласили или исключили.
            /// для служебных сообщений с action = chat_invite_user или chat_kick_user и отрицательным action_mid
            /// </summary>
            public string email { get; set; }

            public int conversation_message_id { get; set; }

            #region VM
            public string UIText { get; set; }
            #endregion

            public MsgActionPhoto photo { get; set; }

            public void Write(BinaryWriter writer)
            {
                writer.Write(1);
                writer.Write((byte)this.type);
                writer.Write(this.member_id);
                writer.WriteString(this.text);
                writer.Write(this.photo);
            }

            public void Read(BinaryReader reader)
            {
                reader.ReadInt32();
                this.type = (VKChatMessageActionType)reader.ReadByte();
                this.member_id = reader.ReadInt32();
                this.text = reader.ReadString();
                this.photo = reader.ReadGeneric<MsgActionPhoto>();
            }
        }

        public class MsgActionPhoto : IBinarySerializable
        {
            /// <summary>
            /// URL копии фотографии беседы шириной 50 px. 
            /// </summary>
            public string photo_50 { get; set; }

            /// <summary>
            /// URL копии фотографии беседы шириной 100 px. 
            /// </summary>
            public string photo_100 { get; set; }

            /// <summary>
            /// URL копии фотографии беседы шириной 200 px. 
            /// </summary>
            public string photo_200 { get; set; }

            public void Write(BinaryWriter writer)
            {
                writer.Write(1);
                writer.WriteString(this.photo_50);
                writer.WriteString(this.photo_100);
                writer.WriteString(this.photo_200);
            }

            public void Read(BinaryReader reader)
            {
                reader.ReadInt32();
                this.photo_50 = reader.ReadString();
                this.photo_100 = reader.ReadString();
                this.photo_200 = reader.ReadString();
            }
        }

        #region VM
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return this._isSelected;
            }
            set
            {
                this._isSelected = value;
                //this.NotifyPropertyChanged("PathStyle");
                this.NotifyPropertyChanged("BorderStyle");

            }
        }

        public Style BorderStyle
        {
            get
            {
                if (this.IsSelected)
                    return (Style)Application.Current.Resources["BorderThemeHigh"];

                if (this.action != null /*&& this.action.type != Network.Enums.VKChatMessageActionType.None*/)
                    return (Style)Application.Current.Resources["BorderThemeTransparent"];

                if (this.attachments != null)
                {
                    Func<VKAttachment, bool> func1 = (a =>
                    {
                        if (a.type == VKAttachmentType.Sticker || a.type == VKAttachmentType.Graffiti)
                            return true;
                        if (a.type != VKAttachmentType.Doc)
                            return false;
                        if (a.doc != null && a.doc.IsGraffiti)
                            return true;

                        return false;
                    });

                    if (this.attachments.Any<VKAttachment>(func1))
                        return (Style)Application.Current.Resources["BorderThemeTransparent"];

                    //   if (this.attachments.Any<VKAttachment>((a => a.type == VKAttachmentType.Gift)))
                    //       return (SolidColorBrush)Application.Current.Resources["PhoneDialogGiftMessageBackgroundBrush"];
                }

                if (this.@out == VKMessageType.Sent)
                    return (Style)Application.Current.Resources["BorderThemeMediumLow"];
                return (Style)Application.Current.Resources["BorderThemeLow"];
            }
        }

        public SolidColorBrush ShadowBrush
        {
            get
            {
                if (this.action != null /*&& this.action.type != VKChatMessageActionType.None*/)
                {
                    return null;
                }

                if (this.attachments != null)
                {
                    Func<VKAttachment, bool> func1 = (a =>
                    {
                        if (a.type == VKAttachmentType.Sticker)
                            return true;
                        if (a.type != VKAttachmentType.Doc)
                            return false;
                        if (a.doc != null && a.doc.IsGraffiti)
                            return true;

                        return false;
                    });

                    //есть стикер? делаем прозрачный фон
                    if (this.attachments.Any<VKAttachment>(func1))
                        return null;
                }

                return (SolidColorBrush)Application.Current.Resources["ShadowBrush"];
            }
        }

        public VKBaseDataForGroupOrUser User { get; set; }

        public BitmapImage UserThumb
        {
            get
            {
                if (this.User == null && this.from_id > 0)
                    this.User = UsersService.Instance.GetCachedUser((uint)this.from_id);
                if (this.User == null)
                    return null;
                return new BitmapImage(new Uri(this.User.MinPhoto));;
            }
        }

        public HorizontalAlignment MsgAligment
        {
            get { return this.@out == VKMessageType.Received ? HorizontalAlignment.Left : HorizontalAlignment.Right; }
        }

        public Thickness MsgMargin
        {
            get
            {
                Thickness margin = new Thickness(0);

                if (this.@out == VKMessageType.Received)
                {
                    margin.Right = 60;
                }
                else
                {
                    margin.Left = 60;
                }

                return margin;
            }
        }

        private bool InChat
        {
            get { return peer_id > 2000000000 || peer_id < -2000000000; }
        }

        /// <summary>
        /// Номер столбца для содержимого
        /// </summary>
        public int ContentColumn
        {
            get { return this.@out == VKMessageType.Received ? 1 : 2; }
        }

        /// <summary>
        /// Номер столбца для даты
        /// </summary>
        public int DateColumn
        {
            get { return this.@out == VKMessageType.Received ? 2 : 1; }
        }

        public GridLength ContentColumnWidth
        {
            get
            {
                if (this.@out == VKMessageType.Received)
                    return new GridLength(1, GridUnitType.Star);

                return new GridLength(0, GridUnitType.Auto);
            }
        }

        public GridLength DateColumnWidth
        {
            get
            {
                if (this.@out == VKMessageType.Received)
                    return new GridLength(0, GridUnitType.Auto);

                return new GridLength(1, GridUnitType.Star);
            }
        }

        public Visibility UserThumbVisibility
        {
            get
            {
                return (this.InChat && this.@out == VKMessageType.Received) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public void RefreshUIProperties()
        {
            Execute.ExecuteOnUIThread(() =>
            {
                //this.NotifyPropertyChanged("ReadStateVisibility");
                this.NotifyPropertyChanged("MsgState");
                //this.NotifyPropertyChanged("MsgStateBrush");
                this.NotifyPropertyChanged("important");
                this.NotifyPropertyChanged(nameof(this.FailedVisibility));
            });

        }

        /*
         * private void UpdateState()
    {
      this.UpdateProgress();
      if (this._mvm.Message.@out == 1 && this._mvm.SendStatus == OutboundMessageStatus.Failed)
      {
        if (!((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Contains((UIElement) this._stackPanelCannotSend))
          ((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Add((UIElement) this._stackPanelCannotSend);
        if (((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Contains((UIElement) this._gridDateStatus))
          ((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Remove((UIElement) this._gridDateStatus);
      }
      else
      {
        if (((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Contains((UIElement) this._stackPanelCannotSend))
          ((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Remove((UIElement) this._stackPanelCannotSend);
        if (!((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Contains((UIElement) this._gridDateStatus))
          ((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Add((UIElement) this._gridDateStatus);
      }
      */
        public Visibility FailedVisibility
        {
            get
            {
                return this.@out == VKMessageType.Sent && (this.OutboundMessageVM != null && this.OutboundMessageVM.OutboundMessageStatus == OutboundMessageStatus.Failed) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public CornerRadius MsgCornerRadius
        {
            get
            {
                return this.@out == VKMessageType.Received ? new CornerRadius(0, 17, 17, 17) : new CornerRadius(17, 17, 0, 17);
            }
        }
        /*
        public Visibility ReadStateVisibility
        {
            get
            {
                //todo: надо сделать часики для отправляемых сообщений
                if (this.@out == VKMessageType.Received)//сообщение от собеседника
                    return Visibility.Collapsed;

                return this.read_state == true ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        */

        public string MsgState
        {
            get
            {
                if (this.@out == VKMessageType.Received)//сообщение от собеседника
                    return "";

                if (this.action != null)
                    return "";
                //todo?                
                if (this.read_state == true /*  DialogsWithConversationViewModel.DialogsViewModel.Instance.CurrentConversation.conversation.out_read >= this.id*/)//моё сообщение прочтено
                    return ""; //return "\xE18E";

                if (this.OutboundMessageVM != null)
                {
                    if (this.OutboundMessageVM.OutboundMessageStatus == OutboundMessageStatus.Delivered)//я отправил сообщение, но оно не прочтено
                        return "\xE1F5";//return "\xE73E";//check
                    else if (this.OutboundMessageVM.OutboundMessageStatus == OutboundMessageStatus.Failed)
                        return "\xE783";
                    else if (this.OutboundMessageVM.OutboundMessageStatus == OutboundMessageStatus.SendingNow)
                        return "\xED5A";//clock
                }
                return "\xE1F5";//return "\xE73E";//check
            }
        }

        public bool read_state;

        public OutboundMessageViewModel OutboundMessageVM { get; set; }

        public void Send()
        {
            this.OutboundMessageVM.IsUploading = true;
            this.OutboundMessageVM.MessageSent += this.OutboundMessageVM_MessageSent;
            this.OutboundMessageVM.UploadFinished += this.OutboundMessageVM_UploadFinished;
            this.OutboundMessageVM.Send();
            this.RefreshUIProperties();
        }

        void OutboundMessageVM_UploadFinished(object sender, EventArgs e)
        {
            this.OutboundMessageVM.IsUploading = false;
            this.RefreshUIProperties();
            this.NotifyResourceUriChanged();
        }

        private void NotifyResourceUriChanged()
        {
            //foreach (var attachmentViewModel in this.attachments.Where((a => a.type == VKAttachmentType.Photo)))
            //    attachmentViewModel.NotifyResourceUriChanged();

            if (this.attachments != null && this.attachments.Count > 0)
            {
                Execute.ExecuteOnUIThread(()=> { 
                this.NotifyPropertyChanged("attachments");
                });
            }
        }

        void OutboundMessageVM_MessageSent(object sender, uint e)//_outboundMessage_MessageSent
        {
            //if (this._outboundMessage.OutboundMessageStatus == OutboundMessageStatus.Delivered)
            this.id = e;

            if (this.attachments != null && this.OutboundMessageVM.Attachments != null)//hm.... что-то здесь не так....
            {
                int index = 0;
                foreach (IOutboundAttachment outboundAttachment1 in this.OutboundMessageVM.Attachments)
                {
                    if (index < this.attachments.Count && outboundAttachment1.IsUploadAttachment && outboundAttachment1 is OutboundPhotoAttachment photoAttachment)
                    {
                        var attachment = this.attachments[index];
                        if (attachment.photo != null && photoAttachment._photo != null)
                        {
                            VKPhoto photo = photoAttachment._photo;
                            attachment.photo = photo;
                            //attachment.photo.id = photo.id;
                            //attachment.photo.id = photo.id;
                            //attachment.photo.src = photo.photo_130;
                            //attachment.photo.src_big = photo.photo_604;
                            //attachment.photo.owner_id = photo.owner_id;
                        }
                    }
                    ++index;
                }
            }
            //
            this.NotifyResourceUriChanged();
            //
            this.RefreshUIProperties();
            this.OutboundMessageVM.MessageSent -= this.OutboundMessageVM_MessageSent;
        }

        public Visibility EditedVisibility
        {
            get
            {
                if (this.update_time == DateTime.MinValue)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        //public string Day;

        public static List<int> GetAssociatedUserIds(List<VKMessage> messages, bool includeForwarded = true)
        {
            List<int> source = new List<int>();
            foreach (VKMessage message in messages)
                source.AddRange(message.GetAssociatedUserIds(includeForwarded));
            return source.Distinct().ToList();
        }

        public List<int> GetAssociatedUserIds(bool includeForwarded = true)
        {
            List<int> source = new List<int>();
            source.Add(this.from_id);
            if (this.action != null)
                source.Add(this.action.member_id);
            //                if (messag.action != null)
            //                    source.AddRange(messag.acti.Where((ca) => ca >= -2000000000));
            if (this.fwd_messages != null & includeForwarded)
            {
                foreach (VKMessage fwdMessage in this.fwd_messages)
                    source.AddRange(fwdMessage.GetAssociatedUserIds());
            }
            return source.Distinct().ToList();
        }

        public Visibility BotKeyboardVisibility
        {
            get { return (this.keyboard != null).ToVisiblity(); }
        }

        public List<List<VKBotKeyboard.KeyboardButton>> BotKeyboardButtons
        {
            get
            {
                if(this.keyboard != null)
                {
                    List<List<VKBotKeyboard.KeyboardButton>> ret = new List<List<VKBotKeyboard.KeyboardButton>>();
                    foreach (var button in this.keyboard.buttons)
                    {
                        ret.Add(button);
                    }
                    return ret;
                }
                return null;
            }
        }
        #endregion

        #region ISupportGroup
        public string Key
        {
            get
            {
                if(DateTime.Now.Date == this.date.Date)//сегодня
                {
                    return string.Format("{0}, {1}", LocalizedStrings.GetString("Today"), this.date.ToString("d MMMM"));
                }
                else if (DateTime.Now.AddDays(-1.0).Date == this.date.Date)//Вчера
                {
                    return string.Format("{0}, {1}", LocalizedStrings.GetString("Yesterday").ToLower(), this.date.ToString("d MMMM"));
                }

                string str;

                if (DateTime.Now.Year != this.date.Year)
                    str = this.date.ToString("d MMMM yyyy");
                else
                    str = this.date.ToString("d MMMM");

                return str;
            }
        }
#endregion

        public void Write(BinaryWriter writer)
        {
            writer.Write(3);
            writer.Write(this.id);
            writer.Write(this.date);
            writer.Write(this.@out == VKMessageType.Sent);
            writer.Write(this.peer_id);
            writer.Write(this.read_state);
            //writer.WriteString(this.title);
            //writer.WriteString(this.photo_100);
            writer.Write(this.action);
            //writer.Write(this.push_settings.disabled_until);

            writer.WriteString(this.text);
            //writer.Write(this.chat_id);
            //writer.WriteString(this.chat_active_str);
            //writer.Write(this.admin_id);
            writer.WriteList(this.fwd_messages);
            writer.WriteList<VKAttachment>(this.attachments);
            //           writer.Write<VKGeo>(this.geo);
            writer.Write(this.important);
            //writer.Write(this.deleted);
            //writer.Write(this.users_count);
            //writer.Write(this.action_mid);
            //writer.WriteString(this.action_email);
            //writer.WriteString(this.action_text);
            //writer.Write(this.sticker_id);
            //writer.WriteString(this.photo_200);
            writer.Write(this.from_id);
            writer.Write(this.update_time);
            bool exists = this.OutboundMessageVM != null;
            writer.Write(exists);

            if(exists)
                writer.Write(this.OutboundMessageVM);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadUInt32();
            this.date = reader.ReadDateTime();
            this.@out = reader.ReadBoolean() ? VKMessageType.Sent : VKMessageType.Received;
            this.peer_id = reader.ReadInt32();
            this.read_state = reader.ReadBoolean();
            //this.title = reader.ReadString();
            //this.photo_100 = reader.ReadString();
            this.action = reader.ReadGeneric<MsgAction>();
            //this.push_settings.disabled_until = reader.ReadInt32();

            this.text = reader.ReadString();
            //this.chat_id = reader.ReadInt32();
            //this.chat_active_str = reader.ReadString();
            //this.admin_id = reader.ReadInt32();
            this.fwd_messages = reader.ReadList<VKMessage>();
            this.attachments = reader.ReadList<VKAttachment>();
            //            this.geo = reader.ReadGeneric<Geo>();
            this.important = reader.ReadBoolean();
            //this.deleted = reader.ReadInt32();
            //this.users_count = reader.ReadInt32();
            //this.action_mid = reader.ReadInt64();
            //this.action_email = reader.ReadString();
            //this._action_text = reader.ReadString();
            //this.sticker_id = reader.ReadInt32();
            //this.photo_200 = reader.ReadString();
            this.from_id = reader.ReadInt32();
            this.update_time = reader.ReadDateTime();

            bool exists = reader.ReadBoolean();
            if(exists)
                this.OutboundMessageVM = reader.ReadGeneric<OutboundMessageViewModel>();
        }
#if DEBUG
        public override string ToString()
        {
            string str = this.text;
            if(string.IsNullOrEmpty(this.text))
            {
                str = "12345678910";
            }
            else
            {
                str = ("text:"+this.text);
            }

            if (this.attachments != null && this.attachments.Count > 0)
                str = ("attachments:"+ attachments.Count);

            if (action != null && action.type == VKChatMessageActionType.UNREAD_ITEM_ACTION)
                str = "UNREAD_ITEM_ACTION";

            return string.Format("m id:{0} {1}", this.id, str);
        }
#endif
    }


}
