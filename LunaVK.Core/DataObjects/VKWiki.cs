using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// https://vk.com/dev/_objects_page
    /// </summary>
    public class VKWiki : IBinarySerializable
    {
        /// <summary>
        /// идентификатор вики-страницы. 
        /// </summary>
        public uint id {get;set;}

        /// <summary>
        /// идентификатор сообщества. 
        /// </summary>
        public uint group_id {get;set;}

        /// <summary>
        /// название вики-страницы. 
        /// </summary>
        public string title {get;set;}

        /// <summary>
        /// информация о том, кто может просматривать вики-страницу:
        /// 2 — просматривать страницу могут все; 
        /// 1 — только участники сообщества; 
        /// 0 — только руководители сообщества. 
        /// </summary>
        public uint who_can_view {get;set;}

        /// <summary>
        /// указывает, кто может редактировать вики-страницу:
        /// 2 — редактировать страницу могут все; 
        /// 1 — только участники сообщества; 
        /// 0 — только руководители сообщества. 
        /// </summary>
        public uint who_can_edit {get;set;}

        /// <summary>
        /// дата последнего изменения вики-страницы в формате Unixtime. 
        /// </summary>
        public int edited {get;set;}

        /// <summary>
        /// дата создания вики-страницы в формате Unixtime. 
        /// </summary>
        public int created {get;set;}

        /// <summary>
        /// количество просмотров вики-страницы. 
        /// </summary>
        public uint views {get;set;}

        /// <summary>
        /// адрес страницы для отображения вики-страницы. 
        /// </summary>
        public string view_url {get;set;}

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.id);
            writer.Write(this.group_id);
            writer.WriteString(this.title);
            writer.Write(this.who_can_view);
            writer.Write(this.who_can_edit);
            writer.Write(this.edited);
            writer.Write(this.created);
            writer.Write(this.views);
            writer.WriteString(this.view_url);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadUInt32();
            this.group_id = reader.ReadUInt32();
            this.title = reader.ReadString();
            this.who_can_view = reader.ReadUInt32();
            this.who_can_edit = reader.ReadUInt32();
            this.edited = reader.ReadInt32();
            this.created = reader.ReadInt32();
            this.views = reader.ReadUInt32();
            this.view_url = reader.ReadString();

        }
    }
}
