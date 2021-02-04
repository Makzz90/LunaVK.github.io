using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using System.IO;
using LunaVK.Core.Utils;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Представляет собой вложение в запись.
    /// Объект содержимого зависит от типа вложения.
    /// https://vk.com/dev/attachments_w
    /// </summary>
    public sealed class VKAttachment : IBinarySerializable
    {
        /// <summary>
        /// Объект фотографии
        /// </summary>
        public VKPhoto photo { get; set; }

        /// <summary>
        /// Это устаревший тип вложения. Он может быть возвращен лишь для записей, созданных раньше 2013 года. 
        /// </summary>
        public VKPhoto posted_photo { get; set; }

        /// <summary>
        /// Видеозапись
        /// </summary>
        public VKVideoBase video { get; set; }

        /// <summary>
        /// Аудиозапись
        /// </summary>
        public VKAudio audio { get; set; }

        /// <summary>
        /// Документ
        /// </summary>
        public VKDocument doc { get; set; }

        /// <summary>
        /// Ссылка
        /// </summary>
        public VKLink link { get; set; }

        /// <summary>
        /// Заметка
        /// </summary>
        public VKNote note { get; set; }//todo: no in documentation

        /// <summary>
        /// Опрос
        /// </summary>
        public VKPoll poll { get; set; }//todo: no in documentation

        public VKWiki page { get; set; }

        public VKAlbumPhoto album { get; set; }
        //photos_list

        /// <summary>
        /// Товар 
        /// </summary>
        //public VKMarket market { get; set; }
        public VKMarketItem market { get; set; }

        //market_album

        public VKPrettyCards pretty_cards { get; set; }

        /// <summary>
        /// Запись на стене
        /// </summary>
        public VKWallPost wall { get; set; }

        /// <summary>
        /// комментарий к записи на стене;
        /// </summary>
        public VKComment wall_reply { get; set; }

        /// <summary>
        /// Стикер
        /// </summary>
        public VKSticker sticker { get; set; }



        /// <summary>
        /// Подарок
        /// </summary>
        public VKGift gift { get; set; }

        /// <summary>
        /// Устаревший тип вложения
        /// Может быть возвращён для записи раньше 2013 года
        /// </summary>
        public VKGraffiti graffiti { get; set; }

        public VKNarrative narrative { get; set; }
        //

        public DocPreview.DocPreviewVoiceMessage audio_message { get; set; }

        public VKPodcast podcast { get; set; }

        public VKCall call { get; set; }

        public VKEvent @event { get; set; }

        public VKStory story { get; set; }

        public VKSituationalTheme situational_theme { get; set; }

        public VKGeo geo { get; set; }//custom

        public VKNewsfeedPost newsfeed_post { get; set; }//custom

        public string emoji { get; set; }//custom

        /// <summary>
        /// Тип вложения.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public VKAttachmentType type { get; set; }

        public override string ToString()
        {
            int owner = 0;
            uint media_id = 0;

            switch (this.type)
            {
                case VKAttachmentType.Photo:
                    {
                        owner = this.photo.owner_id;
                        media_id = this.photo.id;
                        break;
                    }
                case VKAttachmentType.Video:
                    {
                        owner = this.video.owner_id;
                        media_id = this.video.id;
                        break;
                    }
                case VKAttachmentType.Audio:
                    {
                        owner = this.audio.owner_id;
                        media_id = this.audio.id;
                        break;
                    }
                case VKAttachmentType.Doc:
                    {
                        owner = this.doc.owner_id;
                        media_id = this.doc.id;
                        break;
                    }
                case VKAttachmentType.Wall:
                    {
                        owner = this.wall.owner_id;
                        media_id = this.wall.id;
                        break;
                    }
                //case VKAttachmentType.Wall_reply:
                //    {
                //        owner = this.Wall_reply.OwnerID;
                //        media_id = this.Wall_reply.ID;
                //        break;
                //    }
                //case VKAttachmentType.Sticker:
                //    {
                //        owner = this.Sticker.OwnerID;
                //        media_id = this.Sticker.ID;
                //        break;
                //    }
                //case VKAttachmentType.Gift:
                //    {
                //        owner = this.Gift.OwnerID;
                //        media_id = this.Gift.ID;
                //        break;
                //    }
                case VKAttachmentType.Repost:
                    {
                        owner = this.newsfeed_post.owner_id;
                        media_id = this.newsfeed_post.id;
                        break;
                    }
                default:
                    {
                        int debugme = 0;
                        break;
                    }
            }

            return string.Format("{0}{1}_{2}", this.type.ToString().ToLower(), owner, media_id);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(7);
            writer.Write((byte)this.type);
            writer.Write<VKAudio>(this.audio);
            writer.Write<VKVideoBase>(this.video);
            writer.Write<VKPhoto>(this.photo);
            writer.Write<VKDocument>(this.doc);
            writer.Write<VKWallPost>(this.wall);
            writer.Write<VKNote>(this.note);
            writer.Write<VKPoll>(this.poll);
            writer.Write<VKSticker>(this.sticker);
            writer.Write<VKGift>(this.gift);
            writer.Write<VKLink>(this.link);
            writer.Write<VKComment>(this.wall_reply);
            writer.Write<VKMarketItem>(this.market);
            writer.Write<VKAlbumPhoto>(this.album);
            
            writer.Write<DocPreview.DocPreviewVoiceMessage>(this.audio_message);
            writer.Write<VKCall>(this.call);
            writer.Write<VKPhoto>(this.posted_photo);
            writer.Write<VKWiki>(this.page);
            writer.Write<VKAlbumPhoto>(this.album);
            //writer.Write<VKPrettyCards>(this.pretty_cards);
            writer.Write<VKGraffiti>(this.graffiti);
            //writer.Write<VKNarrative>(this.narrative);
            writer.Write<VKPodcast>(this.podcast);
            writer.Write<VKNewsfeedPost>(this.newsfeed_post);

            writer.Write<VKGeo>(this.geo);
            writer.Write<VKEvent>(this.@event);
            writer.Write<VKStory>(this.story);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.type = (VKAttachmentType)reader.ReadByte();
            this.audio = reader.ReadGeneric<VKAudio>();
            this.video = reader.ReadGeneric<VKVideoBase>();
            this.photo = reader.ReadGeneric<VKPhoto>();
            this.doc = reader.ReadGeneric<VKDocument>();
            this.wall = reader.ReadGeneric<VKWallPost>();
            this.note = reader.ReadGeneric<VKNote>();
            this.poll = reader.ReadGeneric<VKPoll>();
            this.sticker = reader.ReadGeneric<VKSticker>();
            this.gift = reader.ReadGeneric<VKGift>();
            this.link = reader.ReadGeneric<VKLink>();
            this.wall_reply = reader.ReadGeneric<VKComment>();
            this.market = reader.ReadGeneric<VKMarketItem>();
            this.album = reader.ReadGeneric<VKAlbumPhoto>();
            
            this.audio_message = reader.ReadGeneric<DocPreview.DocPreviewVoiceMessage>();
            this.call = reader.ReadGeneric<VKCall>();
            this.posted_photo = reader.ReadGeneric<VKPhoto>();
            this.page = reader.ReadGeneric<VKWiki>();
            this.album = reader.ReadGeneric<VKAlbumPhoto>();
            //this.pretty_cards = reader.ReadGeneric<VKCall>();
            this.graffiti = reader.ReadGeneric<VKGraffiti>();
            //this.narrative = reader.ReadGeneric<VKCall>();
            this.podcast = reader.ReadGeneric<VKPodcast>();
            this.newsfeed_post = reader.ReadGeneric<VKNewsfeedPost>();

            this.geo = reader.ReadGeneric<VKGeo>();
            this.@event = reader.ReadGeneric<VKEvent>();
            this.story = reader.ReadGeneric<VKStory>();
        }
    }
}
