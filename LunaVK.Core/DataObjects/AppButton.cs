using LunaVK.Core.Enums;
using LunaVK.Core.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.DataObjects
{
    public class AppButton
    {
        //public int app_id { get; set; }

        public string title { get; set; }

        public VKGroupActionBtnType action_type { get; set; }

        //public List<CoverImage> images { get; set; }

        public TargetAppButton target { get; set; }
        public class TargetAppButton
        {
            public int user_id { get; set; }

            public string email { get; set; }

            public string phone { get; set; }

            public string url { get; set; }

            public string schema { get; set; }

            public uint app_id { get; set; }

            [JsonConverter(typeof(VKBooleanConverter))]
            public bool is_internal { get; set; }

            public string google_store_url { get; set; }
        }
    }
}

/*
public e(JSONObject paramJSONObject)
  {
    this();
    this.l = paramJSONObject.optString("action_type");
    String str = this.l;
    if (str != null) {
      switch (str.hashCode())
      {
      default: 
        break;
      case 1928092749: 
        if (str.equals("call_phone")) {
          m = 1;
        }
        break;
      case 814528549: 
        if (str.equals("send_email")) {
          m = 0;
        }
        break;
      case 548631606: 
        if (str.equals("call_vk")) {
          m = 2;
        }
        break;
      case -504306182: 
        if (str.equals("open_url")) {
          m = 3;
        }
        break;
      case -504325460: 
        if (str.equals("open_app")) {
          m = 5;
        }
        break;
      case -1472831294: 
        if (str.equals("open_internal_url")) {
          m = 4;
        }
        break;
      case -1699113812: 
        if (str.equals("open_group_app")) {
          m = 6;
        }
        break;
      }
    }
    int m = -1;
    this.b = m;
    this.c = paramJSONObject.optString("title");
    paramJSONObject = paramJSONObject.optJSONObject("target");
    if (paramJSONObject != null)
    {
      this.d = paramJSONObject.optInt("user_id");
      this.f = paramJSONObject.optString("email");
      this.g = paramJSONObject.optString("phone");
      this.h = paramJSONObject.optString("url");
      this.i = paramJSONObject.optString("schema");
      this.e = paramJSONObject.optInt("app_id");
      this.j = paramJSONObject.optBoolean("is_internal");
      this.k = paramJSONObject.optString("google_store_url");
    }
  }

    */
