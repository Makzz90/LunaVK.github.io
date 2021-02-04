using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.DataObjects
{
    public class VKPhotoVideoTag
    {
        /// <summary>
        /// идентификатор пользователя, которому соответствует отметка; 
        /// </summary>
        public int user_id { get; set; }

        /// <summary>
        /// идентификатор отметки; 
        /// </summary>
        public int id { get; set; }
        
        /// <summary>
        /// идентификатор пользователя, сделавшего отметку; 
        /// </summary>
        public int placer_id { get; set; }

        /// <summary>
        /// название отметки; 
        /// </summary>
        public string tagged_name { get; set; }

        /// <summary>
        /// дата добавления отметки в формате unixtime; 
        /// </summary>
        public int date { get; set; }

        /// <summary>
        /// координаты прямоугольной области, на которой
        /// сделана отметка (верхний левый угол и нижний правый угол) в процентах; 
        /// </summary>
        public double x { get; set; }

        public double y { get; set; }

        public double x2 { get; set; }

        public double y2 { get; set; }

        /// <summary>
        /// статус отметки (1 — подтвержденная, 0 — неподтвержденная). 
        /// </summary>
        public int viewed { get; set; }
    }
}
