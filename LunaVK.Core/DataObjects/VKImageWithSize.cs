﻿using Newtonsoft.Json;
using LunaVK.Core.Json;
using LunaVK.Core.Framework;
using System.IO;
using LunaVK.Core.Utils;

namespace LunaVK.Core.DataObjects
{
    public class VKImageWithSize : IBinarySerializable
    {
        /// <summary>
        /// URL копии
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// ширина копии
        /// </summary>
        public int width { get; set; }

        /// <summary>
        /// высота копии
        /// </summary>
        public int height { get; set; }


        /// <summary>
        /// обозначение размера и пропорций копии.
        /// </summary>
        [JsonConverter(typeof(StringToCharConverter))]
        public char type { get; set; }//для фотографий

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.WriteString(this.url);
            writer.Write(this.type);
            writer.Write(this.width);
            writer.Write(this.height);
            
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.url = reader.ReadString();
            this.type = reader.ReadChar();
            this.width = reader.ReadInt32(); 
            this.height = reader.ReadInt32();
        }
    }
}


/*
Возможные значения поля type

a - 200x89
b - 400x179
c - 200x200
d - 100x100
e - 50x50
k - 1074x480
l - 537x240

s — пропорциональная копия изображения с максимальной стороной 75px;
m — пропорциональная копия изображения с максимальной стороной 130px;
x — пропорциональная копия изображения с максимальной стороной 604px;

o — если соотношение "ширина/высота" исходного изображения меньше или равно 3:2, то пропорциональная копия с максимальной шириной 130px. Если соотношение "ширина/высота" больше 3:2, то копия обрезанного слева изображения с максимальной шириной 130px и соотношением сторон 3:2.
p — если соотношение "ширина/высота" исходного изображения меньше или равно 3:2, то пропорциональная копия с максимальной шириной 200px. Если соотношение "ширина/высота" больше 3:2, то копия обрезанного слева и справа изображения с максимальной шириной 200px и соотношением сторон 3:2.
q — если соотношение "ширина/высота" исходного изображения меньше или равно 3:2, то пропорциональная копия с максимальной шириной 320px. Если соотношение "ширина/высота" больше 3:2, то копия обрезанного слева и справа изображения с максимальной шириной 320px и соотношением сторон 3:2.
r — если соотношение "ширина/высота" исходного изображения меньше или равно 3:2, то пропорциональная копия с максимальной шириной 510px. Если соотношение "ширина/высота" больше 3:2, то копия обрезанного слева и справа изображения с максимальной шириной 510px и соотношением сторон 3:2

y — пропорциональная копия изображения с максимальной стороной 807px;
z — пропорциональная копия изображения с максимальным размером 1080x1024;
w — пропорциональная копия изображения с максимальным размером 2560x2048px.

temp
*/