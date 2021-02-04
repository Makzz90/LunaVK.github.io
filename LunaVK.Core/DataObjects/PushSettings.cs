using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LunaVK.Core.DataObjects
{
    public class PushSettings
    {
        private static readonly string On = "on";
        private static readonly string Off = "off";

        /// <summary>
        /// личные сообщения. Может принимать массив значений:
        /// no_sound – отключить звук;
        /// no_text – не передавать текст сообщения.
        /// </summary>
        public bool msg { get; set; }

        public bool msg_no_text { get; set; }

        public bool msg_no_sound { get; set; }

        /// <summary>
        /// групповые чаты. Может принимать массив значений:
        /// no_sound – отключить звук;
        /// no_text – не передавать текст сообщения.
        /// </summary>
        public bool chat { get; set; }

        public bool chat_no_text { get; set; }

        public bool chat_no_sound { get; set; }

        /// <summary>
        /// запрос на добавления в друзья. Может принимать массив значений:
        /// mutual – уведомлять только при наличии общих друзей.
        /// </summary>
        public bool friend { get; set; }

        public bool friend_mutual { get; set; }

        /// <summary>
        /// регистрация импортированного контакта;
        /// </summary>
        public bool friend_found { get; set; }

        /// <summary>
        /// подтверждение заявки в друзья;
        /// </summary>
        public bool friend_accepted { get; set; }

        /// <summary>
        /// ответы;
        /// </summary>
        public bool reply { get; set; }

        /// <summary>
        /// комментарии. Может принимать массив значений:
        /// fr_of_fr – уведомления только от друзей и друзей друзей.
        /// </summary>
        public bool comment { get; set; }

        public bool comment_fr_of_fr { get; set; }

        /// <summary>
        /// упоминания. Может принимать массив значений:
        /// fr_of_fr – уведомления только от друзей и друзей друзей.
        /// </summary>
        public bool mention { get; set; }

        public bool mention_fr_of_fr { get; set; }

        /// <summary>
        /// отметки "Мне нравится". Может принимать массив значений:
        /// fr_of_fr – уведомления только от друзей и друзей друзей.
        /// </summary>
        public bool like { get; set; }

        public bool like_fr_of_fr { get; set; }

        /// <summary>
        /// действия "Рассказать друзьям". Может принимать массив значений:
        /// fr_of_fr – уведомления только от друзей и друзей друзей.
        /// </summary>
        public bool repost { get; set; }

        public bool repost_fr_of_fr { get; set; }

        /// <summary>
        /// новая запись на стене пользователя;
        /// </summary>
        public bool wall_post { get; set; }

        /// <summary>
        /// размещение предложенной новости;
        /// </summary>
        public bool wall_publish { get; set; }

        /// <summary>
        /// приглашение в сообщество;
        /// </summary>
        public bool group_invite { get; set; }

        /// <summary>
        /// подтверждение заявки на вступление в группу;
        /// </summary>
        public bool group_accepted { get; set; }

        /// <summary>
        /// ближайшие мероприятия;
        /// </summary>
        public bool event_soon { get; set; }

        /// <summary>
        /// отметки на фотографиях. Может принимать массив значений:
        /// fr_of_fr – уведомления только от друзей и друзей друзей.
        /// </summary>
        public bool tag_photo { get; set; }

        public bool tag_photo_fr_of_fr { get; set; }

        /// <summary>
        /// запросы в приложениях;
        /// </summary>
        public bool app_request { get; set; }

        /// <summary>
        /// установка приложения;
        /// </summary>
        public bool sdk_open { get; set; }

        /// <summary>
        /// записи выбранных людей и сообществ;
        /// </summary>
        public bool new_post { get; set; }

        /// <summary>
        /// уведомления о днях рождениях на текущую дату.
        /// </summary>
        public bool birthday { get; set; }

        public override string ToString()
        {
            Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
            dictionary["msg"] = PushSettings.GetOnOffStr(this.msg);
            if (this.msg_no_text)
                dictionary["msg"].Add("no_text");
            if (this.msg_no_sound)
                dictionary["msg"].Add("no_sound");

            dictionary["chat"] = PushSettings.GetOnOffStr(this.chat);
            if (this.chat_no_text)
                dictionary["chat"].Add("no_text");
            if (this.chat_no_sound)
                dictionary["chat"].Add("no_sound");

            dictionary["friend"] = PushSettings.GetOnOffStr(this.friend);
            if (this.friend_mutual)
                dictionary["friend"].Add("mutual");

            dictionary["friend_found"] = PushSettings.GetOnOffStr(this.friend_found);

            dictionary["friend_accepted"] = PushSettings.GetOnOffStr(this.friend_accepted);

            dictionary["reply"] = PushSettings.GetOnOffStr(this.reply);

            dictionary["comment"] = PushSettings.GetOnOffStr(this.comment);
            if (this.comment_fr_of_fr)
                dictionary["comment"].Add("fr_of_fr");

            dictionary["mention"] = PushSettings.GetOnOffStr(this.mention);
            if (this.mention_fr_of_fr)
                dictionary["mention"].Add("fr_of_fr");

            dictionary["like"] = PushSettings.GetOnOffStr(this.like);
            if (this.like_fr_of_fr)
                dictionary["like"].Add("fr_of_fr");

            dictionary["repost"] = PushSettings.GetOnOffStr(this.repost);
            if (this.repost_fr_of_fr)
                dictionary["repost"].Add("fr_of_fr");

            dictionary["wall_post"] = PushSettings.GetOnOffStr(this.wall_post);

            dictionary["wall_publish"] = PushSettings.GetOnOffStr(this.wall_publish);

            dictionary["group_invite"] = PushSettings.GetOnOffStr(this.group_invite);

            dictionary["group_accepted"] = PushSettings.GetOnOffStr(this.group_accepted);

            dictionary["event_soon"] = PushSettings.GetOnOffStr(this.event_soon);

            dictionary["tag_photo"] = PushSettings.GetOnOffStr(this.tag_photo);
            if (this.tag_photo_fr_of_fr)
                dictionary["tag_photo"].Add("fr_of_fr");

            dictionary["app_request"] = PushSettings.GetOnOffStr(this.app_request);

            dictionary["sdk_open"] = PushSettings.GetOnOffStr(this.sdk_open);

            dictionary["new_post"] = PushSettings.GetOnOffStr(this.new_post);

            dictionary["birthday"] = PushSettings.GetOnOffStr(this.birthday);

            return JsonConvert.SerializeObject(dictionary);
        }

        private static List<string> GetOnOffStr(bool b)
        {
            List<string> o = new List<string>();
            o.Add(b ? PushSettings.On : PushSettings.Off);
            return o;
        }

        public PushSettings()
        {
            this.msg = true;
            this.msg_no_sound = false;
            this.chat = true;
            this.chat_no_sound = false;
            this.friend = true;
            this.friend_found = true;
            this.friend_accepted = true;
            this.reply = true;
            this.comment = true;
            this.mention = true;
            this.like = true;
            this.repost = true;
            this.wall_post = true;
            this.wall_publish = true;
            this.group_invite = true;
            this.group_accepted = true;
            this.event_soon = true;
            this.tag_photo = true;
            this.app_request = true;
            this.sdk_open = true;
            this.new_post = true;
            this.birthday = true;
        }
    }
}
