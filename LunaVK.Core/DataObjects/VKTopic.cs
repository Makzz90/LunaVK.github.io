using LunaVK.Core.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Содержит информацию о теме в обсуждениях сообщества
    /// </summary>
    public class VKTopic
    {
        /// <summary>
        /// идентификатор темы
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// идентификатор сообщества, содержащего тему в обсуждениях (со знаком "минус")
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// заголовок темы
        /// </summary>
        public string title { get; set; }
        
        /// <summary>
        /// дата создания темы в формате unixtime
        /// </summary>
        public int created { get; set; }

        /// <summary>
        /// идентификатор пользователя, создавшего тему;
        /// </summary>
        public int created_by { get; set; }

        /// <summary>
        ///  дата последнего сообщения в формате unixtime; 
        /// </summary>
        public int updated { get; set; }

        /// <summary>
        ///  идентификатор пользователя, оставившего последнее сообщение; 
        /// </summary>
        public int updated_by { get; set; }

        /// <summary>
        /// 1, если тема является закрытой (в ней нельзя оставлять сообщения); 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_closed { get; set; }

        /// <summary>
        /// 1, если тема является прилепленной (находится в начале списка тем); 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_fixed { get; set; }

        /// <summary>
        /// число сообщений в теме. 
        /// </summary>
        public int comments { get; set; }



        public string last_comment { get; set; }

        //public override string ToString()
        //{
        //    return string.Format("topic{0}_{1}", this.owner_id, this.id);
        //}
    }
}
