
using LunaVK.Core.Framework;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    //OwnCounters
    //https://vk.com/dev/account.getCounters
    public class CountersArgs : IBinarySerializable
    {
        /// <summary>
        /// новые заявки в друзья
        /// </summary>
        public uint friends;

        /// <summary>
        /// новые сообщения
        /// </summary>
        public uint messages;

        /// <summary>
        /// новые отметки на фотографиях
        /// </summary>
        public uint photos;

        /// <summary>
        /// новые отметки на видеозаписях
        /// </summary>
        public uint videos;

        /// <summary>
        /// подарки
        /// </summary>
        public uint gifts;

        /// <summary>
        /// события
        /// </summary>
        public uint events;

        /// <summary>
        /// сообщества
        /// </summary>
        public uint groups;

        /// <summary>
        /// ответы
        /// </summary>
        public uint notifications;

        /// <summary>
        /// предлагаемые друзья
        /// </summary>
        public uint friends_suggestions;

        /// <summary>
        /// запросы в мобильных играх
        /// </summary>
        public uint sdk;

        /// <summary>
        /// уведомления от приложений
        /// </summary>
        public uint app_requests;

        /// <summary>
        /// рекомендации друзей
        /// </summary>
        public uint friends_recommendations;

        public uint menu_discover_badge;

        public uint faves;

        public uint TotalCount
        {
            get
            {
                return this.notifications + this.messages + this.friends + this.groups/* + this.GamesItem.Count*/;
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(3);
            writer.Write(this.friends);
            writer.Write(this.messages);
            writer.Write(this.photos);
            writer.Write(this.videos);
            writer.Write(this.groups);
            writer.Write(this.notifications);
            writer.Write(this.sdk);
            writer.Write(this.app_requests);

            writer.Write(this.friends_recommendations);
            writer.Write(this.gifts);
            writer.Write(this.menu_discover_badge);
            writer.Write(this.faves);
        }

        public void Read(BinaryReader reader)
        {
            int num1 = reader.ReadInt32();
            this.friends = reader.ReadUInt32();
            this.messages = reader.ReadUInt32();
            this.photos = reader.ReadUInt32();
            this.videos = reader.ReadUInt32();
            this.groups = reader.ReadUInt32();
            this.notifications = reader.ReadUInt32();
            this.sdk = reader.ReadUInt32();
            this.app_requests = reader.ReadUInt32();

            this.friends_recommendations = reader.ReadUInt32();
            this.gifts = reader.ReadUInt32();
            this.menu_discover_badge = reader.ReadUInt32();
            this.faves = reader.ReadUInt32();
        }
    }
}
