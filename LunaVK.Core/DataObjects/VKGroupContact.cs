using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.DataObjects
{
    public class VKGroupContact
    {
        /// <summary>
        /// идентификатор пользователя
        /// </summary>
        public int user_id { get; set; }

        /// <summary>
        /// должность
        /// </summary>
        public string desc { get; set; }

        /// <summary>
        /// номер телефона
        /// </summary>
        public string phone { get; set; }

        /// <summary>
        /// адрес e-mail
        /// </summary>
        public string email { get; set; }
    }
}
