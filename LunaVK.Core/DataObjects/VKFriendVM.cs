using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.DataObjects
{
    public class VKFriendVM : VKUser
    {
        public string CountryWithCity
        {
            get
            {
                string ret = "";
                if (base.country != null)
                    ret = base.country.title;
                if (base.city!=null)
                    ret += (", " + base.city.title);
                return ret;
            }
        }

        public string RequsetMsg { get; set; }

        /// <summary>
        /// 0 - друзья
        /// 1 - заявки
        /// 2 - исходящие
        /// 3 - предполагаемые
        /// </summary>
        public ushort Mode;

        public string PrimaryBtnText
        {
            get
            {
                if(this.Mode == 1)
                    return "Принять";

                return null;
            }
        }

        public string SecondaryBtnText
        {
            get
            {
                if (this.Mode == 1)
                    return "Скрыть";
                else if (this.Mode == 2)
                    return "Отменить";
                return null;
            }
        }

        public string ThirdBtnText
        {
            get
            {
                if (this.Mode == 0)
                    return "\xE8BD";//message icon
                else if (this.Mode == 3)
                    return "\xE8FA";//Add icon
                return null;
            }
        }
    }
}
