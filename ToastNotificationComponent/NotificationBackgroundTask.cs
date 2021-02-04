using System.Collections.Generic;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using LunaVK.Core.Network;
using System;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using Windows.Web.Http;
using LunaVK.Core;
using Windows.Storage;
using System.Threading.Tasks;

namespace ToastNotificationComponent
{
    public sealed class NotificationBackgroundTask : IBackgroundTask
    {
        /*
        private async void GetResponse(string methode, Dictionary<string, string> parameters, BackgroundTaskDeferral deferral)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync(new Uri(VKRequestsDispatcher.RequestUriFrm + methode), new HttpFormUrlEncodedContent(parameters));
                deferral.Complete();
            }
        }
        */
        /*
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral(); // Get a deferral since we're executing async code

            var container = ApplicationData.Current.LocalSettings.CreateContainer("Settings2", ApplicationDataCreateDisposition.Always);
            container.Values["Data"] = "NotificationBackgroundTask";
            deferral.Complete();
        }
        */
        
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var container = ApplicationData.Current.LocalSettings.CreateContainer("Settings2", ApplicationDataCreateDisposition.Always);
            container.Values["Data"] = "NotificationBackgroundTask";

            BackgroundTaskDeferral deferral = taskInstance.GetDeferral(); // Get a deferral since we're executing async code

            if(!Settings.IsAuthorized)// мы вышли из аккаунта и отвечаем в оставшемся пуше
            {
                deferral.Complete();
                return;
            }

            if (taskInstance.TriggerDetails is ToastNotificationActionTriggerDetail details) // If it's a toast notification action
            {
                IReadOnlyDictionary<string, string> args = details.Argument.Replace("&amp;", "&").ParseQueryString();

                container.Values["Data"] = ("NotificationBackgroundTask args: '" + details.Argument + "'\n");//Logger.Instance.Info("NotificationBackgroundTask args: " + details.Argument);

                switch (args["action"])
                {
                    case "call_ignore":
                        {
                            break;
                        }
                    case "reply":
                        {
                            //ush_id=msg_375988312_42927
                            //push_id=chat_2000000017_42928 // сообщение из беседы

                            var inp = details.UserInput;
                            string message = inp["textBox"] as string;
                            string push_id = args["push_id"];
                            string[] temp2 = push_id.Split('_');

                            int peer_id = 0;

                            if (temp2[0] == "chat")
                            {
                                peer_id = int.Parse(temp2[1]);
                            }
                            else if (temp2[0] == "msg")
                            {
                                int user_id = int.Parse(temp2[1]);
                                peer_id = user_id;
                            }

                            if (peer_id != 0)
                            {
                                switch(Settings.DEV_BackgroundAnswerMethode)
                                {
                                    
                                    case 1:
                                        {
                                            Task.Run(async() => { 
                                                Dictionary<string, string> parameters = new Dictionary<string, string>();
                                                parameters["message"] = message;
                                                parameters["peer_id"] = peer_id.ToString();
                                                parameters["random_id"] = Convert.ToInt32((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();
                                                parameters["v"] = VKConstants.API_VERSION;
                                                parameters["access_token"] = Settings.AccessToken;
                                                //this.GetResponse("messages.send", parameters, deferral);

                                                try
                                                {
                                                    using (HttpClient client = new HttpClient())
                                                    {
                                                        var response = await client.PostAsync(new Uri(VKRequestsDispatcher.RequestUriFrm + "messages.send"), new HttpFormUrlEncodedContent(parameters));
                                                        string content = await response.Content.ReadAsStringAsync();
                                                        container.Values["Data"] = ("NotificationBackgroundTask result1: '" + content + "'\n");

                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    container.Values["Data"] = ("NotificationBackgroundTask fail1: '" + ex.Message + "'\n");
                                                }
                                                finally
                                                {
                                                    deferral.Complete();
                                                }
                                            });

                                            break;
                                        }
                                    case 2:
                                        {
                                            Task.Run(async () => {
                                                Dictionary<string, string> parameters = new Dictionary<string, string>();
                                                parameters["message"] = message;
                                                parameters["peer_id"] = peer_id.ToString();
                                                parameters["random_id"] = Convert.ToInt32((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();
                                                parameters["v"] = VKConstants.API_VERSION;
                                                parameters["access_token"] = Settings.AccessToken;
                                                //VKRequestsDispatcher.DispatchRequestToVK<int>("messages.send", parameters, (result) => { deferral.Complete(); });

                                                try
                                                {
                                                    var client = new System.Net.Http.HttpClient();
                                                    var response = await client.PostAsync(new Uri(VKRequestsDispatcher.RequestUriFrm + "messages.send"), new System.Net.Http.FormUrlEncodedContent(parameters));
                                                    string content = await response.Content.ReadAsStringAsync();
                                                    container.Values["Data"] = ("NotificationBackgroundTask result2: '" + content + "'\n");
                                                }
                                                catch(Exception ex)
                                                {
                                                    container.Values["Data"] = ("NotificationBackgroundTask fail2: '" + ex.Message + "'\n");
                                                }
                                                finally
                                                {
                                                    deferral.Complete();
                                                }
                                            });
                                            break;
                                        }
                                    default://0
                                        {
                                            //это сработало
                                            MessagesService.Instance.SendMessage(peer_id, (result) =>
                                            {
                                                //if (result.error.error_code != LunaVK.Core.Enums.VKErrors.None)
                                                //    Logger.Instance.Error("Can't send msg from background", result.error.error_code);
                                                container.Values["Data"] = ("NotificationBackgroundTask result0: '" + result.error.error_code + "'\n");//
                                                deferral.Complete();
                                            }, message);
                                            break;
                                        }
                                }
                            }

                            break;
                        }
                    case "friend_add":
                        {

                            uint uid = uint.Parse(args["uid"]);
                            Dictionary<string, string> parameters = new Dictionary<string, string>();
                            parameters["user_id"] = uid.ToString();
                            //await this.GetResponse("friends.add", parameters, deferral);
                            UsersService.Instance.FriendAdd(uid, "", (result) => { deferral.Complete(); });
                            break;
                        }
                    case "friend_hide":
                        {
                            uint uid = uint.Parse(args["uid"]);
                            Dictionary<string, string> parameters = new Dictionary<string, string>();
                            parameters["user_id"] = uid.ToString();
                            //await this.GetResponse("friends.delete", parameters, deferral);
                            UsersService.Instance.FriendDelete(uid, (result) => { deferral.Complete(); });
                            break;
                        }
                    case "like":
                        {
                            string item = args["item"];
                            item = item.Substring(4);
                            string[] temp2 = item.Split('_');
                            int owner_id = int.Parse(temp2[0]);
                            uint item_id = uint.Parse(temp2[1]);
                            LikesService.Instance.AddRemoveLike(true, owner_id, item_id, LunaVK.Core.Enums.LikeObjectType.post, (result) => { deferral.Complete(); });
                            break;
                        }
                    case "fave":
                        {
                            string item = args["item"];//post-1234
                            item = item.Substring(4);
                            string[] temp2 = item.Split('_');
                            int owner_id = int.Parse(temp2[0]);
                            uint item_id = uint.Parse(temp2[1]);
                            FavoritesService.Instance.AddRemovePost( owner_id, item_id, true, (result) => { deferral.Complete(); });
                            break;
                        }
                    default:
                        {
                            deferral.Complete();
                            break;
                        }
                }
            }
        }
        
    }
}
