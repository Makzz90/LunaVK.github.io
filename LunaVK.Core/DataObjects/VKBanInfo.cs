using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.DataObjects
{
    public class VKBanInfo
    {
        /// <summary>
        /// срок окончания блокировки в формате unixtime
        /// </summary>
        public int end_date { get; set; }

        /// <summary>
        /// комментарий к блокировке
        /// </summary>
        public string comment { get; set; }
    }
}
