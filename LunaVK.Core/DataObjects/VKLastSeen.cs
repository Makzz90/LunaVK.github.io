using System;
using Newtonsoft.Json;
using LunaVK.Core.Json;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Объект времени последнего посещения ВКонтакте. UserStatus
    /// </summary>
    public sealed class VKLastSeen : IBinarySerializable
    {
        /// <summary>
        /// Платформа, с которой заходил пользователь.
        /// </summary>
        public VKPlatform platform { get; set; }

        /// <summary>
        /// Время и дата последнего посещения.
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime time { get; set; }

        public void Write(BinaryWriter writer)
        {
            writer.Write((byte)this.platform);
            writer.Write(this.time.ToBinary());
        }

        public void Read(BinaryReader reader)
        {
            this.platform = (VKPlatform)reader.ReadByte();
            this.time = DateTime.FromBinary( reader.ReadInt64());
        }
        /*
#region VM
        public bool Online;

        public string OnlineApp;

        public string AppString
        {
            get
            {
                if (string.IsNullOrEmpty(this.OnlineApp))
                    return "";

                string tttt = "";
                switch (this.OnlineApp)
                {
                    case "3502561":
                        {
                            tttt = "Windows Phone";
                            break;
                        }
                    case "3140623":
                        {
                            tttt = "iPhone";
                            break;
                        }
                    case "2274003":
                        {
                            tttt = "Android";
                            break;
                        }
                    case "2685278":
                        {
                            tttt = "KateMobile";
                            break;
                        }
                    case "3265802":
                        {
                            tttt = "Api console";
                            break;
                        }
                    case "3682744":
                        {
                            tttt = "iPad";
                            break;
                        }
                    case "5674548":
                        {
                            tttt = "ВКонтакте Pro";
                            break;
                        }
                    case "5316500":
                        {
                            tttt = "VFeed pro";
                            break;
                        }
                    case "4542624":
                        {
                            tttt = "Black VK";
                            break;
                        }
                    case "5632485":
                        {
                            tttt = "Space VK";
                            break;
                        }
                    case "6244854":
                        {
                            tttt = "Luna VK";
                            break;
                        }
                    default:
                        {
#if DEBUG
                            System.Diagnostics.Debug.WriteLine(" -> " + tttt);
#endif
                            break;
                        }
                }
                return tttt;
            }
        }
#endregion
*/
    }
}
