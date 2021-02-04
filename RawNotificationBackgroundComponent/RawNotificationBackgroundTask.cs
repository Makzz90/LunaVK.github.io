using System.Collections.Generic;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using LunaVK.Core.Network;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Windows.Storage;

namespace RawNotificationBackgroundComponent
{
    public sealed class RawNotificationBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var container = ApplicationData.Current.LocalSettings.CreateContainer("Settings2", ApplicationDataCreateDisposition.Always);
            container.Values["Data"] = "RawNotificationBackgroundTask";

            // Get a deferral since we're executing async code
            var deferral = taskInstance.GetDeferral();

            RawNotification notification = (RawNotification)taskInstance.TriggerDetails;

            string json = notification.Content;//.Replace("&amp;", "&");
            
            var response = JsonConvert.DeserializeObject<RawNotify>(json);
            /*
            string launch = response.launch.Replace("&amp;", "&");
            string toast_peer_id = "";

            Dictionary<string, string> paramDict = launch.ParseQueryString();


            string toastXmlString = "<toast version='1' launch='" + response.launch + "'>";
            toastXmlString += "<visual><binding template='ToastGeneric'>";// ToastGeneric ToastImageAndText02
            toastXmlString += ("<image id='1' hint-crop='circle' placement='appLogoOverride' src='" + response.image + "'/>");
            toastXmlString += ("<text id='1'>" + response.text1.Replace("&", "&amp;") + "</text>");
            toastXmlString += ("<text id='2'>" + response.text2.Replace("&", "&amp;") + "</text>");

            if (paramDict.ContainsKey("push_id"))
            {
                string push_id = paramDict["push_id"];
                string[] temp2 = push_id.Split('_');
                int peer_id = 0;
                if (temp2[0] == "chat" || temp2[0] == "msg")
                {
                    peer_id = int.Parse(temp2[1]);
                    toast_peer_id = peer_id.ToString();
                }

                uint msg_id = uint.Parse(temp2[2]);
                //
                if (response.text2.EndsWith("стикер") || response.text2.EndsWith("фотографию")
                    || response.text2.EndsWith("sticker") || response.text2.EndsWith("photo"))
                {
                    MessagesService.Instance.GetMessages(new List<uint>() { msg_id }, (result) => { 
                    if (result!=null)
                    {
                        VKMessage msg = result.items[0];
                        if (msg.attachments != null && msg.attachments.Count > 0)
                        {
                            var attach = msg.attachments[0];
                            if (attach.type == LunaVK.Core.Enums.VKAttachmentType.Sticker)
                            {
                                toastXmlString += "<image src='" + attach.sticker.photo_256 + "' />";
                            }
                            else if (attach.type == LunaVK.Core.Enums.VKAttachmentType.Photo)
                            {
                                toastXmlString += "<image src='" + attach.photo.photo_604 + "' />";
                            }

                        }

                    }
                    });
                }

                toastXmlString += "</binding></visual>";

                toastXmlString += "<actions>";
                toastXmlString += "<input id='textBox' type='text' placeHolderContent='Написать ответ' />";
                toastXmlString += "<action content='Send' arguments='action=reply&amp;push_id=" + push_id + "' activationType='background' hint-inputId='textBox' imageUri='Assets/Icons/send.png' />";//важно: content
                toastXmlString += "</actions>";

                if (response.audio != null)
                {
                    if (response.audio.silent == "true")
                        toastXmlString += "<audio silent='true'/>";
                    else
                        toastXmlString += "<audio src='ms-appx:///Assets/Mp3/bb2.mp3' />";
                }
                else
                {
                    toastXmlString += "<audio src='ms-appx:///Assets/Mp3/bb2.mp3' />";
                }

            }
            else if (paramDict.ContainsKey("type"))
            {
                toastXmlString += "</binding></visual>";
                //sound=default&amp;_genSrv=807117&amp;type=open_url&amp;try_internal=1&amp;url=https%3A%2F%2Fvk.com%2Fwall-155775051_162&amp;sandbox=0&amp;log_date=1585657792
                string type = paramDict["type"];
                if (type == "friend")
                {
                    int user_id = int.Parse(paramDict["uid"]);

                    toastXmlString += "<actions>";

                    toastXmlString += "<action content='Добавить' arguments='action=friend_add&amp;uid=" + user_id + "' activationType='background'   />";
                    toastXmlString += "<action content='Скрыть' arguments='action=friend_hide&amp;uid=" + user_id + "' activationType='background'   />";

                    toastXmlString += "</actions>";
                }

                toastXmlString += "<audio src='ms-appx:///Assets/Mp3/bb1.mp3' />";
            }
            else
            {
                toastXmlString += "</binding></visual>";
                toastXmlString += "<audio src='ms-appx:///Assets/Mp3/bb1.mp3' />";
            }



            toastXmlString += "</toast>";


            XmlDocument xml = new XmlDocument();
            xml.LoadXml(toastXmlString);
            ToastNotification toast = new ToastNotification(xml);

            if(!string.IsNullOrEmpty(toast_peer_id))
                toast.Tag = toast_peer_id;

            ToastNotificationManager.CreateToastNotifier().Show(toast);
            */

            string launch = response.launch.Replace("&amp;", "&");
            string toastTag = "";

            Dictionary<string, string> paramDict = launch.ParseQueryString();
            XmlElement actionsElement = null;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            if(response.audio == null)//такого не бывает, но на всякий случай
                response.audio = new RawNotify.Audio();

            response.audio.src = "ms-appx:///Assets/Mp3/bb1.mp3";

            if (paramDict.ContainsKey("push_id"))
            {
                string push_id = paramDict["push_id"];
                string[] temp2 = push_id.Split('_');
                int peer_id = 0;
                if (temp2[0] == "chat" || temp2[0] == "msg")
                {
                    peer_id = int.Parse(temp2[1]);
                    toastTag = peer_id.ToString();
                }

                uint msg_id = uint.Parse(temp2[2]);
                //

                if (actionsElement == null)
                {
                    actionsElement = toastXml.CreateElement("actions");
                    toastXml.DocumentElement.AppendChild(actionsElement);
                }

                XmlElement inputElement = toastXml.CreateElement("input");
                inputElement.SetAttribute("id", "textBox");
                inputElement.SetAttribute("type", "text");
                inputElement.SetAttribute("placeHolderContent", "Написать ответ");

                XmlElement actionElement = toastXml.CreateElement("action");
                actionElement.SetAttribute("content", "Send");//важно: content
                actionElement.SetAttribute("arguments", "action=reply&amp;push_id=" + push_id);
                actionElement.SetAttribute("activationType", "background");
                actionElement.SetAttribute("hint-inputId", "textBox");
                actionElement.SetAttribute("imageUri", "Assets/Icons/send.png");

                actionsElement.AppendChild(inputElement);
                actionsElement.AppendChild(actionElement);

                response.audio.src = "ms-appx:///Assets/Mp3/bb2.mp3";

                if (response.text2.EndsWith("стикер") || response.text2.EndsWith("фотографию")
                    || response.text2.EndsWith("sticker") || response.text2.EndsWith("photo"))
                {
                    try
                    {
                        CancellationTokenSource token = new CancellationTokenSource();

                        MessagesService.Instance.GetMessages(new List<uint>() { msg_id }, (result) =>
                        {
                            if (result != null)
                            {
                                VKMessage msg = result.items[0];
                                if (msg.attachments != null && msg.attachments.Count > 0)
                                {
                                    VKAttachment attach = msg.attachments[0];
                                    if (attach.type == LunaVK.Core.Enums.VKAttachmentType.Sticker)
                                    {
                                        //toastXmlString += "<image src='" + attach.sticker.photo_256 + "' />";

                                        XmlElement bindingNode = toastXml.GetElementsByTagName("binding")[0] as XmlElement;

                                        XmlElement imageElement = toastXml.CreateElement("image");
                                        imageElement.SetAttribute("src", attach.sticker.photo_256);
                                        //imageElement.SetAttribute("placement", "hero");//Обрезается :(
                                        bindingNode.AppendChild(imageElement);
                                    }
                                    else if (attach.type == LunaVK.Core.Enums.VKAttachmentType.Photo)
                                    {
                                        //toastXmlString += "<image src='" + attach.photo.photo_604 + "' />";

                                        XmlElement bindingNode = toastXml.GetElementsByTagName("binding")[0] as XmlElement;

                                        XmlElement imageElement = toastXml.CreateElement("image");
                                        imageElement.SetAttribute("src", attach.photo.photo_604);
                                        if (attach.photo.TrueWidth > attach.photo.TrueHeight)
                                            imageElement.SetAttribute("placement", "hero");
                                        bindingNode.AppendChild(imageElement);
                                    }
                                }
                            }
                            token.Cancel();
                        });
                        await Task.Delay(5000, token.Token);
                    }
                    catch
                    {

                    }
                }



            }
            else if (paramDict.ContainsKey("type"))
            {
                string type = paramDict["type"];
                if (type == "friend")
                {
                    int user_id = int.Parse(paramDict["uid"]);

                    if (actionsElement == null)
                    {
                        actionsElement = toastXml.CreateElement("actions");
                        toastXml.DocumentElement.AppendChild(actionsElement);
                    }


                    XmlElement actionAddElement = toastXml.CreateElement("action");
                    actionAddElement.SetAttribute("content", "Добавить");
                    actionAddElement.SetAttribute("arguments", "action=friend_add&amp;uid=" + user_id);
                    actionAddElement.SetAttribute("activationType", "background");

                    XmlElement actionHideElement = toastXml.CreateElement("action");
                    actionHideElement.SetAttribute("content", "Скрыть");
                    actionHideElement.SetAttribute("arguments", "action=friend_hide&amp;uid=" + user_id);
                    actionHideElement.SetAttribute("activationType", "background");


                    actionsElement.AppendChild(actionAddElement);
                    actionsElement.AppendChild(actionHideElement);
                }
                else if (type == "open_url" && paramDict.ContainsKey("url"))
                {
                    //sound=default&amp;_genSrv=807117&amp;type=open_url&amp;try_internal=1&amp;url=https%3A%2F%2Fvk.com%2Fwall-155775051_162&amp;sandbox=0&amp;log_date=1585657792
                    string url0 = paramDict["url"];//https%3A%2F%2Fvk.com%2Fwall-155775051_162
                    string url = System.Net.WebUtility.UrlDecode(url0);//https://vk.com/wall-155775051_162
                    Match m = _wallReplyReg.Match(url);
                    if (m.Success && m.Groups.Count > 1)
                    {
                        int ownerId = int.Parse(m.Groups[1].Value);
                        uint postId = uint.Parse(m.Groups[2].Value);

                        toastTag = "wall" + ownerId + "_" + postId;

                        try
                        {
                            CancellationTokenSource token = new CancellationTokenSource();
                            WallService.Instance.GetWallById(ownerId, postId, (result) =>
                            {
                                if (result.error.error_code == LunaVK.Core.Enums.VKErrors.None)
                                {
                                    if (result.response.items.Count > 0)
                                    {
                                        VKWallPost post = result.response.items[0];
                                        if (post.owner_id > 0)
                                            response.image = result.response.profiles.First((u) => u.id == post.owner_id).photo_100;
                                        else
                                            response.image = result.response.groups.First((g) => g.id == (-post.owner_id)).photo_100;

                                        response.text2 = post.text;

                                        if (post.attachments != null && post.attachments.Count > 0)
                                        {
                                            VKAttachment attach = post.attachments[0];
                                            if (attach.type == LunaVK.Core.Enums.VKAttachmentType.Photo)
                                            {
                                                //toastXmlString += "<image src='" + attach.photo.photo_604 + "' />";

                                                XmlElement bindingNode = toastXml.GetElementsByTagName("binding")[0] as XmlElement;

                                                XmlElement imageElement = toastXml.CreateElement("image");
                                                imageElement.SetAttribute("src", attach.photo.photo_604);
                                                if (attach.photo.TrueWidth > attach.photo.TrueHeight)
                                                    imageElement.SetAttribute("placement", "hero");
                                                bindingNode.AppendChild(imageElement);
                                            }
                                            else if (attach.type == LunaVK.Core.Enums.VKAttachmentType.Video)
                                            {
                                                XmlElement bindingNode = toastXml.GetElementsByTagName("binding")[0] as XmlElement;

                                                XmlElement imageElement = toastXml.CreateElement("image");
                                                imageElement.SetAttribute("src", attach.video.ImageUri);
                                                bindingNode.AppendChild(imageElement);

                                                if (string.IsNullOrEmpty(response.text2))
                                                    response.text2 = "\uD83D\uDCFA [Видеозапись]";//https://emojipedia.org/television/
                                            }
                                        }
                                    }
                                }
                                token.Cancel();
                            });
                            await Task.Delay(5000, token.Token);
                        }
                        catch
                        {

                        }

                        if (actionsElement == null)
                        {
                            actionsElement = toastXml.CreateElement("actions");
                            toastXml.DocumentElement.AppendChild(actionsElement);
                        }


                        XmlElement actionAddElement = toastXml.CreateElement("action");
                        actionAddElement.SetAttribute("content", "Нравится");
                        actionAddElement.SetAttribute("arguments", "action=like&amp;item=" + toastTag);
                        actionAddElement.SetAttribute("activationType", "background");

                        XmlElement actionHideElement = toastXml.CreateElement("action");
                        actionHideElement.SetAttribute("content", "В избранное");
                        actionHideElement.SetAttribute("arguments", "action=fave&amp;item=" + toastTag);
                        actionHideElement.SetAttribute("activationType", "background");

                        actionsElement.AppendChild(actionAddElement);
                        actionsElement.AppendChild(actionHideElement);
                    }
                }

            }

            this.SendNotification(toastXml, response, toastTag);

            deferral.Complete();
        }

        private void SendNotification(XmlDocument toastXml, RawNotify data, string tag = "")
        {
            XmlDocument xml = this.Generate(toastXml, data);

            ToastNotification toast = new ToastNotification(xml);

            if (!string.IsNullOrEmpty(tag))
                toast.Tag = tag;

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private XmlDocument Generate(XmlDocument toastXml, RawNotify data)
        {
            toastXml.DocumentElement.SetAttribute("launch", data.launch);

            XmlElement bindingNode = toastXml.GetElementsByTagName("binding")[0] as XmlElement;
            bindingNode.SetAttribute("template", "ToastGeneric");

            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");//Элементы "текст" из шаблона
            stringElements[0].AppendChild(toastXml.CreateTextNode(data.text1));
            stringElements[1].AppendChild(toastXml.CreateTextNode(data.text2));

            if (!string.IsNullOrEmpty(data.image))
            {
                XmlElement imageElement = toastXml.GetElementsByTagName("image")[0] as XmlElement;
                imageElement.SetAttribute("src", data.image);
                imageElement.SetAttribute("hint-crop", "circle");
                imageElement.SetAttribute("placement", "appLogoOverride");
            }

            XmlElement audioElement = toastXml.CreateElement("audio");
            audioElement.SetAttribute("src", data.audio.src);
            toastXml.DocumentElement.AppendChild(audioElement);

            if (data.audio.silent == "true")
                audioElement.SetAttribute("silent", "true");

            return toastXml;
        }

        private readonly Regex _wallReplyReg = new Regex("/wall([-0-9]+)_([0-9]+)");

        private class RawNotify
        {
            /// <summary>
            /// Аватарка
            /// </summary>
            public string image { get; set; }

            /// <summary>
            /// Обычно это имя пользователя
            /// </summary>
            public string text1 { get; set; }

            /// <summary>
            /// Текст сообщения или текст поста
            /// </summary>
            public string text2 { get; set; }
            public string launch { get; set; }
            public Audio audio { get; set; }

            public class Audio
            {
                public string src { get; set; }
                public string silent { get; set; }
            }
        }
    }
}
