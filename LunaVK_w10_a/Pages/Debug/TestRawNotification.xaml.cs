using LunaVK.Core.DataObjects;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;
using LunaVK.Network;
using LunaVK.Core;
using LunaVK.Library;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;
using Windows.Storage;
using LunaVK.Core.Framework;

namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestRawNotification : Page
    {
        private readonly Regex _wallReplyReg = new Regex("/wall([-0-9]+)_([0-9]+)");
        WNS_Helper w = new WNS_Helper();

        public TestRawNotification()
        {
            this.InitializeComponent();

            this._tbLaunch.Text = "{'text1':'New post from Test2','text2':'4','audio':{'src':'ms-winsoundevent:Notification.Default','silent':''},'launch':'sound=default&amp;_genSrv=807117&amp;type=open_url&amp;try_internal=1&amp;url=https%3A%2F%2Fvk.com%2Fwall-36338110_485895&amp;sandbox=0&amp;log_date=1585657792'}";






            this.tb.Text = "<?xml version=\"1.0\" encoding=\"utf-8\"?><toast launch=\"sound=default&amp;push_id=msg_375988312_61181&amp;sandbox=0&amp;uid=375988312&amp;msg_id=61181\"><visual><binding template=\"ToastImageAndText02\"><image id=\"1\" src=\"https://sun9-9.userapi.com/c840628/v840628547/88832/NnkSMFxaTxw.jpg?ava=1\"  hint-crop=\"circle\" /><text id=\"1\">(Luna | Макеты, предложения)</text><text id=\"2\">Блок с квадратиками</text></binding></visual><audio src=\"ms-winsoundevent:Notification.Default\" loop=\"false\"/><actions> <input type=\"text\" id=\"textBox\"/> <action content=\"background\" arguments=\"action=reply&amp;push_id=msg_375988312_61181\" activationType=\"background\" imageUri=\"Assets/Icons/send.png\" hint-inputId=\"textBox\"  /> </actions></toast>";
            this._tbChannel.Text = string.IsNullOrEmpty( Settings.LastPushNotificationsUri ) ? "" : Settings.LastPushNotificationsUri;

            PushNotifications.Instance.IsPushHidden=false;

            this.Unloaded += TestRawNotification_Unloaded;
        }

        private void TestRawNotification_Unloaded(object sender, RoutedEventArgs e)
        {
            PushNotifications.Instance.IsPushHidden=true;
        }

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

        

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            /*
            string json = this._tbLaunch.Text.Replace('\'','\"');

            RawNotify response = JsonConvert.DeserializeObject<RawNotify>(json);

            string launch = response.launch.Replace("&amp;", "&");
            string toastTag = "";

            Dictionary<string, string> paramDict = launch.ParseQueryString();
            XmlElement actionsElement = null;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            if (response.audio == null)//такого не бывает, но на всякий случай
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

                        toastTag = "post" + ownerId + "_" + postId;

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
                                            else if(attach.type == LunaVK.Core.Enums.VKAttachmentType.Video)
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
            */
            string json = this._tbLaunch.Text.Replace('\'', '\"');

            string notificationType = "wns/raw";
            string contentType = "application/octet-stream";

            _tbResult2.Text = "In process...";

            string channel = this._tbChannel.Text;
            if (this._cb.IsChecked.Value)
            {
                channel = Settings.CustomPushNotificationsServer + channel;
                if (Settings.PushNotificationsEnabled > 2)
                    channel += "&ext=1";
            }
            string temp = await w.PostToWns("q6ykRNGlbTknZkTFdYdSsjMaeXEZ0CY2", "ms-app://s-1-15-2-1984189995-2494523741-1751573263-2652388568-2789401851-2275521763-2415750372", channel, json, notificationType, contentType);
            //string temp = await w.PostToWns("q6ykRNGlbTknZkTFdYdSsjMaeXEZ0CY2", "ms-app://s-1-15-2-1984189995-2494523741-1751573263-2652388568-2789401851-2275521763-2415750372", channelUri, "<?xml version=\"1.0\" encoding=\"utf-8\"?><badge value=\"50\" />", "wns/badge", contentType);
            _tbResult2.Text = "Result: " + temp;
        }

        private void SendNotification(XmlDocument toastXml, RawNotify data, string tag = "")
        {
            XmlDocument xml = this.Generate(toastXml, data);

            //string sss = xml.GetXml();
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








        //https://apps.dev.microsoft.com/?mkt=ru-ru&referrer=https%3a%2f%2fhabr.com%2f#/application/sapi/000000004C1FCEBF
        private async void Button_Click2(object sender, RoutedEventArgs e)
        {
            string notificationType = "wns/toast";
            string contentType = "";// "application/octet-stream";
            
            _tbResult.Text = "In process...";

            string channel = this._tbChannel.Text;
            if(this._cb.IsChecked.Value)
            {
                channel = Settings.CustomPushNotificationsServer + channel;
            }
            string temp = await w.PostToWns("q6ykRNGlbTknZkTFdYdSsjMaeXEZ0CY2", "ms-app://s-1-15-2-1984189995-2494523741-1751573263-2652388568-2789401851-2275521763-2415750372", channel, this.tb.Text, notificationType, contentType);
            //string temp = await w.PostToWns("q6ykRNGlbTknZkTFdYdSsjMaeXEZ0CY2", "ms-app://s-1-15-2-1984189995-2494523741-1751573263-2652388568-2789401851-2275521763-2415750372", channelUri, "<?xml version=\"1.0\" encoding=\"utf-8\"?><badge value=\"50\" />", "wns/badge", contentType);
            _tbResult.Text = "Result: " + temp;
        }






























        private void Button_Click_7(object sender, RoutedEventArgs e)//reg
        {
            Task.Run(async () => {
                BackgroundAccessStatus backgroundRequestStatus = await BackgroundExecutionManager.RequestAccessAsync();
                Execute.ExecuteOnUIThread(() =>
                {
                    this._tb.Text = backgroundRequestStatus.ToString();
                });
                    BackgroundTaskUtils.RegisterBackgroundTask("ToastNotificationComponent.NotificationBackgroundTask", new ToastNotificationActionTrigger());
            });
        }

        private async void Button_Click_5(object sender, RoutedEventArgs e)
        {
            BackgroundAccessStatus backgroundRequestStatus = await BackgroundExecutionManager.RequestAccessAsync();
            
            byte i = BackgroundTaskUtils.RegisterBackgroundTask("ToastNotificationComponent.NotificationBackgroundTask", new ToastNotificationActionTrigger());
            this._tb.Text = backgroundRequestStatus.ToString() + "_" + i;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)//unreg
        {
            this._tb.Text = BackgroundTaskUtils.UnRegisterBackgroundTask("ToastNotificationComponent.NotificationBackgroundTask").ToString();
        }


        //https://stackoverflow.com/questions/1091945/what-characters-do-i-need-to-escape-in-xml-documents
        /*
        "   &quot;
        '   &apos;
        <   &lt;
        >   &gt;
        &   &amp;
        */
        string xml = @"
            <toast>
                <visual>
                    <binding template='ToastGeneric'>
                        <image id='1' src='https://sun9-9.userapi.com/c840628/v840628547/88832/NnkSMFxaTxw.jpg?ava=1' hint-crop='circle'/>
                        <text id='1'>(Luna | Макеты, предложения)</text>
                        <text id='2'>Блок с квадратиками</text>
                    </binding>
                </visual>
                <actions>
                    <action content='Like' arguments='lol' activationType='background'/>
                </actions>
                <audio src='ms-winsoundevent:Notification.Default' loop='false'/>
            </toast>";

        string xml2 = "<?xml version=\"1.0\" encoding=\"utf-8\"?><toast launch=\"sound=default&amp;push_id=msg_375988312_61181&amp;sandbox=0&amp;uid=375988312&amp;msg_id=61181\"><visual><binding template=\"ToastImageAndText02\"><image id=\"1\" src=\"https://sun9-9.userapi.com/c840628/v840628547/88832/NnkSMFxaTxw.jpg?ava=1\"  hint-crop=\"circle\" /><text id=\"1\">(Luna | Макеты, предложения)</text><text id=\"2\">Блок с квадратиками</text></binding></visual><audio src=\"ms-winsoundevent:Notification.Default\" loop=\"false\"/><actions> <input type=\"text\" id=\"textBox\"/> <action content=\"background\" arguments=\"action=reply&amp;push_id=msg_375988312_61181\" activationType=\"background\" imageUri=\"Assets/Icons/send.png\" hint-inputId=\"textBox\"  /> </actions></toast>";

        //https://apps.dev.microsoft.com/?mkt=ru-ru&referrer=https%3a%2f%2fhabr.com%2f#/application/sapi/0000000044231D37
        private async void Button_Click_2(object sender, RoutedEventArgs e)//send
        {
            string notificationType = "wns/toast";
            string contentType = "";

            this.xml = this.xml.Replace("'", "\"");
            this.xml = this.xml2;

            //var xmlDocument = new XmlDocument();
            //xmlDocument.LoadXml(xml);

            string temp = await w.PostToWns("q6ykRNGlbTknZkTFdYdSsjMaeXEZ0CY2", "ms-app://s-1-15-2-1984189995-2494523741-1751573263-2652388568-2789401851-2275521763-2415750372", this.channel.Uri, this.xml, notificationType, contentType);
            this._tb.Text = temp;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)//read
        {
            var container = ApplicationData.Current.LocalSettings.CreateContainer("Settings2", ApplicationDataCreateDisposition.Always);
            object value;
            if (container.Values.TryGetValue("Data", out value))
            {
                this._tb.Text = (string)value;
            }
            else
            {
                this._tb.Text = "Not exists :(";
            }
        }

        private PushNotificationChannel channel;

        private async void Button_Click_4(object sender, RoutedEventArgs e)//open
        {
            ApplicationData.Current.LocalSettings.DeleteContainer("Settings2");
            try
            {
                if (this.channel == null)
                {
                    //this.RegisterTasks();

                    this.channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

                    this._tb.Text = "Channel opened";
                }
            }
            catch (Exception ex) // нет интернета?
            {
                this._tb.Text = "UpdateDeviceRegistration: failed" + ex.Message;
                return;
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (this.channel != null)
                this.channel.Close();
            this.channel = null;
            this._tb.Text = "Chnnel closed";
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            Task.Run(async() =>
            {
                ApplicationData.Current.LocalSettings.DeleteContainer("Settings2");
                try
                {
                    if (this.channel == null)
                    {
                        //this.RegisterTasks();

                        this.channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                        Execute.ExecuteOnUIThread(() => {
                            this._tb.Text = "Channel opened";
                        });
                    }
                }
                catch (Exception ex) // нет интернета?
                {
                    Execute.ExecuteOnUIThread(() => {
                        this._tb.Text = "UpdateDeviceRegistration: failed" + ex.Message;
                    });
                    return;
                }
            });
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            //ToastNotificationComponent.NotificationBackgroundTask task = new ToastNotificationComponent.NotificationBackgroundTask();
            //task.Run(null);
        }
    }
}
