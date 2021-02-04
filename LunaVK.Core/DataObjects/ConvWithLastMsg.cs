using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.DataObjects
{
    public class ConvWithLastMsg
    {
        public VKConversation conversation { get; set; }
        public VKMessage last_message { get; set; }
    }
}
