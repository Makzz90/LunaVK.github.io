using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace LunaVK.Core.DataObjects
{
    public class AutorizationData
    {
        public string AccessToken { get; set; }
        //public int expires_in { get; set; }

        
        public uint UserId { get; set; }
    }
}
