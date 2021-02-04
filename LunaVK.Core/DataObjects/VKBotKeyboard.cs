using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace LunaVK.Core.DataObjects
{
    public class VKBotKeyboard : IBinarySerializable
    {
        /*
         {
            "one_time": false,
            "author_id": -157369801,
            "buttons": [
                [{
                    "action": {
                        "type": "text",
                        "label": "Карты Сбербанка",
                        "payload": "{}"
                    },
                    "color": "positive"
                }],
                [{
                    "action": {
                        "type": "text",
                        "label": "Поиск отделений и банкоматов",
                        "payload": "{}"
                    },
                    "color": "positive"
                }]
            ]
        }
         * */
        public bool one_time { get; set; }
        public int author_id { get; set; }
        public List<List<KeyboardButton>> buttons { get; set; }

        public class KeyboardButton
        {
            public KeyboardAction action { get; set; }

            public string color { get; set; }//positive,primary,negative,default

#region VM
            public string Label
            {
                get { return this.action.label; }
            }

            public Brush Color
            {
                get
                {
                    if (this.color == "positive")
                    {
                        return new SolidColorBrush(ColorExtensions.FromString("#4ab34c"));
                    }
                    else if (this.color == "negative")
                    {
                        return new SolidColorBrush(ColorExtensions.FromString("#eb5050"));
                    }
                    if (this.color == "default")
                    {
                        return new SolidColorBrush(ColorExtensions.FromString("#b3b3b3"));
                    }
                    if (this.color == "primary")
                    {
                        return new SolidColorBrush(ColorExtensions.FromString("#5181B8"));
                    }
                    return new SolidColorBrush(ColorExtensions.FromString("#51b851"));//web
                }
            }
#endregion
        }

        public class KeyboardAction
        {
            public string type { get; set; }//text vkpay open_app location open_link callback

            public string label { get; set; }

            public string payload { get; set; }//"{"button": "3"}"
        }

        public bool inline { get; set; }//внутри сообщения инлайн?

#region IBinarySerializable
        public void Write(BinaryWriter writer)
        {
            //TODO
            writer.Write(1);
            writer.Write(this.author_id);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.author_id = reader.ReadInt32();
        }
#endregion
    }
}
