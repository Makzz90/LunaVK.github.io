using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.DataObjects
{
    public class VKUserOrGroupSource
    {
        public int id { get; set; }

        public string name { get; set; }

        public string type { get; set; }

        public int is_member { get; set; }

        public string activity { get; set; }

        public int is_closed { get; set; }

        public string photo_200 { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string photo_max { get; set; }

        public int verified { get; set; }

        public int friend_status { get; set; }

        public VKOccupation occupation { get; set; }

        public VKCity city { get; set; }

        public VKCountry country { get; set; }

        public VKUser GetUser()
        {
            if (!(this.type == "profile"))
                return null;
            return new VKUser()
            {
            //    id = this.id,
                first_name = this.first_name,
                last_name = this.last_name,
                photo_max = this.photo_max,
            //    verified = this.verified,
            //    friend_status = this.friend_status,
                occupation = this.occupation,
                city = this.city,
                country = this.country
            };
        }

        public VKGroup GetGroup()
        {
            if (!(this.type != "profile"))
                return null;
            return new VKGroup()
            {
                //    id = this.id,
                name = this.name,
            //    is_member = this.is_member,
            //   activity = this.activity,
            //   is_closed = this.is_closed,
            //    photo_200 = this.photo_200,
            //    verified = this.verified
            };
        }
    }
}
