using System;
using System.Collections.Generic;
using Windows.Networking.PushNotifications;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using LunaVK.Core.Network;
using LunaVK.Core.Enums;
using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Library;
using System.Diagnostics;
using LunaVK.Core.Utils;
using LunaVK.UC;
using LunaVK.Core.Framework;
using Windows.Storage;
using System.IO;
using Windows.UI.Xaml;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace LunaVK.Library
{
    public class PushNotifications
    {
        private bool _is_attached;
        public bool IsPushHidden = true;
        private PushNotificationChannel channel;
        private DelayedExecutor _de = null;

        private static PushNotifications _instance;
        public static PushNotifications Instance
        {
            get
            {
                if (PushNotifications._instance == null)
                    PushNotifications._instance = new PushNotifications();
                return PushNotifications._instance;
            }
        }

        public PushNotifications()
        {
            Execute.ExecuteOnUIThread(() =>
            {
                if (Window.Current != null)
                    Window.Current.VisibilityChanged += this.Current_VisibilityChanged;
            });
        }

        ~PushNotifications()
        {
            Execute.ExecuteOnUIThread(() =>
            {
                if (Window.Current != null)
                    Window.Current.VisibilityChanged -= this.Current_VisibilityChanged;
            });
        }

        private void Current_VisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            this.IsPushHidden = e.Visible;
            Logger.Instance.Info("PushNotifications VisibilityChanged {0}", e.Visible);
        }

        /// <summary>
        /// Ахринеть как важно регистрировать фоновые задачи
        /// </summary>
        public void RegisterTasks()
        {
            //Эксперимент показал что можно из фона это делать
            Task.Run(async () =>
            {
                //var backgroundAccessStatus = BackgroundExecutionManager.GetAccessStatus();
                

                BackgroundAccessStatus backgroundRequestStatus = await BackgroundExecutionManager.RequestAccessAsync();
                Logger.Instance.Info("Status of tasks: " + backgroundRequestStatus.ToString());

#if WINDOWS_UWP
                BackgroundTaskUtils.RegisterBackgroundTask("ToastNotificationComponent.NotificationBackgroundTask", new ToastNotificationActionTrigger());
#endif
                BackgroundTaskUtils.RegisterBackgroundTask("RawNotificationBackgroundComponent.RawNotificationBackgroundTask", new PushNotificationTrigger());

                BackgroundTaskUtils.RegisterBackgroundTask("ScheduledUpdater.ScheduledAgentTask", new TimeTrigger(15, false)); //If FreshnessTime is set to less than 15 minutes, an exception is thrown when attempting to register the background task.
            });
        }

        public void UnRegisterTasks()
        {
#if WINDOWS_UWP
            BackgroundTaskUtils.UnRegisterBackgroundTask("ToastNotificationComponent.NotificationBackgroundTask");
#endif
            BackgroundTaskUtils.UnRegisterBackgroundTask("RawNotificationBackgroundComponent.RawNotificationBackgroundTask");

            BackgroundTaskUtils.UnRegisterBackgroundTask("ScheduledUpdater.ScheduledAgentTask");
        }

        /// <summary>
        /// Возвращает идентификатор оборудования.
        /// </summary>
        public string GetHardwareID
        {
            get
            {
                string result = Settings.UserId + "_";

                try
                {
                    //if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.System.Profile.HardwareIdentification"))
                    //{
                    /*
                    Windows 10 Anniversary Edition (introduced in 10.0.14393.0)
                    Windows Desktop Extension SDK (introduced in 10.0.10240.0)
                    Windows Mobile Extension SDK (introduced in 10.0.10240.0)
                    */
                    var token = Windows.System.Profile.HardwareIdentification.GetPackageSpecificToken(null);
                    var hardwareId = token.Id;
                    var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(hardwareId);

                    byte[] bytes = new byte[hardwareId.Length];
                    dataReader.ReadBytes(bytes);

                    result += Convert.ToBase64String(bytes).Replace("=", "");
                }
                catch
                {
                    var deviceInformation = new EasClientDeviceInformation();
                    string Id = deviceInformation.Id.ToString();
                    result += Id;
                }
                //}
                //else
                //{
                    
                //}
                

                return result;
            }
        }

        public void UpdateDeviceRegistration(Action<bool> calback = null, bool force_unregister = false)
        {
            Task.Run(async () =>
            {
                if (force_unregister == false)
                {
                    try
                    {
                        if (this.channel == null)
                        {
                            this.channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

                            Logger.Instance.Info("UpdateDeviceRegistration: Channel for push opened");

                            ApplicationData.Current.LocalSettings.DeleteContainer("Settings2");

                            this.RegisterTasks();
                        }
                    }
                    catch (Exception ex) // нет интернета?
                    {
                        this._de = null;
                        this._de = new DelayedExecutor(60000);
                        this._de.AddToDelayedExecution(() => { this.UpdateDeviceRegistration(); });
                        Logger.Instance.Error("UpdateDeviceRegistration: failed", ex);
                        Execute.ExecuteOnUIThread(() =>
                        {
                            new GenericInfoUC(8000).ShowAndHideLater("UpdateDeviceRegistration: failed to open channel\n" + ex.Message);
                        });
                        calback?.Invoke(false);
                        return;
                    }
                }

                if (Settings.PushNotificationsEnabled != 0 && force_unregister == false)
                {
                    if (!this._is_attached)
                    {
                        Logger.Instance.Info("UpdateDeviceRegistration: attached");

                        this.channel.PushNotificationReceived += this.channel_PushNotificationReceived;
                        this._is_attached = true;
                    }

                    calback?.Invoke(true);

                    if (this.channel.Uri == Settings.LastPushNotificationsUri)
                    {
                        return;
                    }
                    else
                    {
                        Settings.LastPushNotificationsUri = this.channel.Uri;
                    }

                    string temp = "";

                    if (Settings.PushNotificationsEnabled > 1)
                        temp = Settings.CustomPushNotificationsServer;

                    temp += this.channel.Uri;

                    if (Settings.PushNotificationsEnabled > 2)
                        temp += "&ext=1";
                
                    AccountService.Instance.RegisterDevice(this.GetHardwareID, temp, Settings.PushSettings.ToString(), (result) =>
                    {
                        if (result.error.error_code == VKErrors.None && result.response == 1)
                        {
                            if (!this._is_attached)
                            {
                                Logger.Instance.Info("UpdateDeviceRegistration: attached and registered");

                                this.channel.PushNotificationReceived += this.channel_PushNotificationReceived;
                                this._is_attached = true;
                            }

                            Logger.Instance.Info("UpdateDeviceRegistration: registered");
                            calback?.Invoke(true);
                        }
                        else
                        {
                            Logger.Instance.Error("UpdateDeviceRegistration: register failed", result.error);
                            //Execute.ExecuteOnUIThread(() =>
                            //{
                            //    new GenericInfoUC(6000).ShowAndHideLater("UpdateDeviceRegistration: register failed" + result.error.error_code + result.error.error_msg);
                            //});
                            calback?.Invoke(false);
                        }
                    });
                }
                else
                {
                    AccountService.Instance.UnregisterDevice(this.GetHardwareID, (result) =>
                    {
                        if (result.error.error_code == VKErrors.None && result.response == 1)
                        {
                            if (this._is_attached)
                            {
                                Logger.Instance.Info("UpdateDeviceRegistration: deattached");

                                Settings.LastPushNotificationsUri = null;
                                
                                this.channel.PushNotificationReceived -= this.channel_PushNotificationReceived;
                                
                                this._is_attached = false;
                            }
                            if (this.channel != null)
                            {
                                this.channel.Close();
                                this.channel = null;
                            }
                            this.UnRegisterTasks();
                            calback?.Invoke(true);
                        }
                        else
                        {
                            Logger.Instance.Error("UpdateDeviceRegistration: unregister failed", result.error);
                            //Execute.ExecuteOnUIThread(() =>
                            //{
                            //    new GenericInfoUC(6000).ShowAndHideLater("UpdateDeviceRegistration: unregister failed" + result.error.error_code + result.error.error_msg);
                            //});
                            calback?.Invoke(false);
                        }
                    });
                }
            });
        }
        
        /*
        public async void GetPushSettings()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["device_id"] = this.GetHardwareID();
            VKResponse<PushSettings> e = await RequestsDispatcher.GetResponse<PushSettings>("account.getPushSettings", parameters);
            int i = 0;
        }

        class PushSettings
        {
            //conversations
            public int disabled { get; set; }
            Settings settings { get; set; }
            public int disabled_until { get; set; }

            class Settings
            {
                public string msg { get; set; }
                public string chat { get; set; }
                public string friend { get; set; }
                public string friend_found { get; set; }
                public string friend_accepted { get; set; }
                public string reply { get; set; }
                public string comment { get; set; }
                public string mention { get; set; }
                public string like { get; set; }
                public string repost { get; set; }
                public string wall_post { get; set; }
                public string wall_publish { get; set; }
                public string group_invite { get; set; }
                public string group_accepted { get; set; }
                public string event_soon { get; set; }
                public string tag_photo { get; set; }
                public string app_request { get; set; }
                public string sdk_open { get; set; }
                public string new_post { get; set; }
                public string birthday { get; set; }
                public string money { get; set; }
                public string live { get; set; }
                public string gift { get; set; }
                public string story_reply { get; set; }
                public string interest_post { get; set; }
                public string call { get; set; }
                public string associated_events { get; set; }
                public string private_group_post { get; set; }
                public string chat_mention { get; set; }
                public string vk_apps_open_url { get; set; }
            }
            
             * "conversations": {
   "count": 5,
   "items": [{
    "peer_id": 2000000001,
    "sound": 1,
    "disabled_until": 0
   }, {
    "peer_id": 2000000009,
    "sound": 1,
    "disabled_until": -1
   }, {
    "peer_id": 2000000010,
    "sound": 1,
    "disabled_until": -1
   }, {
    "peer_id": 2000000014,
    "sound": 1,
    "disabled_until": -1
   }, {
    "peer_id": 2000000019,
    "sound": 1,
    "disabled_until": -1
   }]
             * 
        }
        */

                    private void channel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            if (args.NotificationType == PushNotificationType.Toast)
            {
                ToastNotification t = args.ToastNotification;

                XmlDocument toastXML = t.Content;
#if DEBUG
                string ttt = toastXML.GetXml();
#endif
                XmlNodeList l = toastXML.GetElementsByTagName("toast");
                Debug.Assert(l.Count > 0);
                if (l.Count>0)
                {
                    Debug.Assert(l[0].Attributes.Count > 0);
                    if (l[0].Attributes.Count>0)
                    {
                        IXmlNode temp2 = l[0].Attributes[0];
                        string launch = temp2.InnerText;
                        Dictionary<string, string> paramDict = launch.ParseQueryString();
                        //Это с пуш с сообщением?
                        if (paramDict.ContainsKey("msg_id") && paramDict.ContainsKey("uid"))
                        {
                            t.Tag = paramDict["uid"];
                            args.Cancel = this.IsPushHidden;
                        }

//#if DEBUG
                        Logger.Instance.Info("PushNotificationReceived: {0}", args.Cancel ? "Cancel" : "Not cancel");
//#endif
                    }
                }



            }
            else if (args.NotificationType == PushNotificationType.Raw)
            {
                string content = args.RawNotification.Content;
                if (content.Contains("msg_id=") && content.Contains("uid="))
                {
                    args.Cancel = this.IsPushHidden;
//#if DEBUG
                    Logger.Instance.Info("PushNotificationReceived [RAW]: {0}", args.Cancel ? "Cancel" : "Not cancel");
//#endif
                }
            }
        }
    }
}
