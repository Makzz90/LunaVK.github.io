using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

using System.Text.RegularExpressions;
using Windows.UI.Popups;
using LunaVK.Framework;
using Windows.UI.ViewManagement;
using LunaVK.Core;
using LunaVK.Pages;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.ViewModels;
using LunaVK.Common;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Notifications;
using Windows.ApplicationModel.Background;
using Windows.Web.Http;
using LunaVK.Core.Network;
using Windows.Foundation.Metadata;
using Windows.Globalization;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.System;

namespace LunaVK
{
    /// <summary>
    /// Обеспечивает зависящее от конкретного приложения поведение, дополняющее класс Application по умолчанию.
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        /// Инициализирует одноэлементный объект приложения. Это первая выполняемая строка разрабатываемого
        /// кода; поэтому она является логическим эквивалентом main() или WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            base.Suspending += this.OnSuspending;
            base.Resuming += this.OnResuming;
            this.UnhandledException += this.OnUnhandledException;
#if WINDOWS_UWP
            this.RequestedTheme = ApplicationTheme.Dark; // BugFix - Чтобы темы менялись на лету
#endif
        }

        /// <summary>
        /// Вызывается при обычном запуске приложения пользователем. Будут использоваться другие точки входа,
        /// если приложение запускается для открытия конкретного файла, отображения
        /// результатов поиска и т. д.
        /// А так же вызывается после OnSuspending
        /// </summary>
        /// <param name="e">Сведения о запросе и обработке запуска.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Logger.Instance.Info("<---------------------------- Launched =========================>");

            this.InitFrame();

            CustomFrame rootFrame = Window.Current.Content as CustomFrame;            

            if (rootFrame.Content == null)
            {
                if (Settings.IsAuthorized)
                {
                    if (!this.NavigateFromArgs(e.Arguments))
                        Library.NavigatorImpl.Instance.NavigateToNewsFeed();
#if DEBUG
                    //rootFrame.Navigate(typeof(Pages.Debug.TestMerge));
                    //rootFrame.Navigate(typeof(TestRectangleLayout));
                    //rootFrame.Navigate(typeof(TestNotificationsPanel));
                    //rootFrame.Navigate(typeof(TestTextAnimation));
                    //Library.NavigatorImpl.Instance.NavigateToWallPostComments(4763, -154148777/*, 4777*/);
                    //Library.NavigatorImpl.Instance.NavigateToVideos(-123861343);
                    //Library.NavigatorImpl.Instance.NavigateToCommunityManagementInformation(34534270/*154148777*/);
                    //Library.NavigatorImpl.Instance.NavigateToConversation(-157369801);
                    //rootFrame.Navigate(typeof(TestAttach));
                    //Library.NavigatorImpl.Instance.NavigateToChatEditPage(20);
                    //Library.NavigatorImpl.Instance.NavigateToFavorites();
                    //rootFrame.Navigate(typeof(AboutPage));

                    //Library.NavigatorImpl.Instance.NavigateToAudioPlayer();
                    //Library.NavigatorImpl.Instance.NavigateToConversationMaterials(344817594);

                    //rootFrame.Navigate(typeof(Pages.Debug.TestSticker));
                    //rootFrame.Navigate(typeof(Pages.Debug.TestControls));
                    //rootFrame.Navigate(typeof(StickersStorePage));
                    //Library.NavigatorImpl.Instance.NavigateToProfilePage(506700881);
                    //Library.NavigatorImpl.Instance.NavigateToGroupDiscussion(16315, 38911499, "", true, 29678);
                    //Library.NavigatorImpl.Instance.NavigateToStoryCreate();
                    //rootFrame.Navigate(typeof(Pages.Debug.TestMasterDetils));
                    //rootFrame.Navigate(typeof(Pages.Debug.TestTile));
                    //rootFrame.Navigate(typeof(Pages.Debug.TestAcrylic));
                    //rootFrame.Navigate(typeof(TestLoadMore));
                    //rootFrame.Navigate(typeof(Pages.Debug.TestDragDrop));
                    //rootFrame.Navigate(typeof(Pages.Debug.TestLikesControl));
                    //rootFrame.Navigate(typeof(Pages.Debug.TestEmoji));
                    //rootFrame.Navigate(typeof(Pages.Debug.TestMsgitem));
                    //Library.NavigatorImpl.Instance.NavigateToUsersSearch();
                    //Library.NavigatorImpl.Instance.NavigateToCommunityManagementLinks(154148777);
                    //rootFrame.Navigate(typeof(TestPush));
                    //Library.NavigatorImpl.Instance.NavigateToAllPhotos(-16315, "Starcon");
                    //Library.NavigatorImpl.Instance.NavigateToProfilePage(-22822305);
                    //Library.NavigatorImpl.Instance.NavigateToProfilePage(-17557426);
                    //Library.NavigatorImpl.Instance.NavigateToStickersManage();
                    //Library.NavigatorImpl.Instance.NavigateToUsersSearch();

                    //rootFrame.Navigate(typeof(Pages.Debug.TestControls_Part2));

                    //rootFrame.Navigate(typeof(Pages.Debug.TestResponse));

                    //rootFrame.Navigate(typeof(Pages.Debug.TestListGroup));
                    //Library.NavigatorImpl.Instance.NavigateToNewWallPost(WallPostViewModel.Mode.NewWallPost,-155775051, Core.Enums.VKAdminLevel.Admin);

                    //rootFrame.Navigate(typeof(Pages.Debug.TestSplitView2));
                    //Library.NavigatorImpl.Instance.NavigateToEditProfile();
                    //Library.NavigatorImpl.Instance.NavigateToConversation(171234857);
                    //Library.NavigatorImpl.Instance.NavigateToProfilePage(-36338110);//KudaGo

                    //rootFrame.Navigate(typeof(Pages.Debug.TestFillRowControl));
                    //Library.NavigatorImpl.Instance.NavigateToProfilePage(-86529522);//VK Fest

                    //rootFrame.Navigate(typeof(Pages.Debug.TestRawNotification));
                    //Library.NavigatorImpl.Instance.NavigateToProfilePage(-47824846);//группа с закрытой стеной и wiki_page
                    //Library.NavigatorImpl.Instance.NavigateToGroupWikiPages(47824846, "Anime");
                    //Library.NavigatorImpl.Instance.NavigateToProfilePage(535438957);//Беглов
                    //rootFrame.Navigate(typeof(Pages.Debug.ViewColors));
                    //Library.NavigatorImpl.Instance.NavigateToProfilePage(-7383958);//группа с магазином
                    //rootFrame.Navigate(typeof(SettingsPrivacyPage));

                    //rootFrame.Navigate(typeof(Pages.Debug.TestPlayer));
                    //rootFrame.Navigate(typeof(Pages.Debug.TestVideoPlayerPanel));

                    //rootFrame.Navigate(typeof(Pages.Debug.TestNavigationView));
                    //Library.NavigatorImpl.Instance.NavigateToCommunityManagement(155775051, Core.Enums.VKGroupType.Page, Core.Enums.VKAdminLevel.Admin);
                    //Library.NavigatorImpl.Instance.NavigateToCommunityManagement(196072411, Core.Enums.VKGroupType.Group, Core.Enums.VKAdminLevel.Admin);

                    //rootFrame.Navigate(typeof(Pages.Debug.TestBotKeyboard));
                    //rootFrame.Navigate(typeof(Pages.Debug.TestPhotoEditor));
                    //Library.NavigatorImpl.Instance.NavigateToPhotoPickerPhotos(3);
                    //Library.NavigatorImpl.Instance.NavigateToDownloads();
                    //rootFrame.Navigate(typeof(Pages.Debug.TestDownload));

#endif
                }
                else
                {

                    if (rootFrame.IsDevicePhone || ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.Animation.ConnectedAnimationService") == false)//Всё-таки на телефоне отсутствует ConnectedAnimation
                    {
//#if DEBUG
//                        rootFrame.Navigate(typeof(Pages.Debug.TestVirtualBar));
//#else
                        rootFrame.Navigate(typeof(LoginPage));
//#endif
                    }
                    else
                    {
                        SplashPage extendedSplash = new SplashPage(e.SplashScreen);
                        rootFrame.Content = extendedSplash;
                    }
                    
                    /*
                    if(ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3) && ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.Animation.ConnectedAnimationService"))
                    {
                        SplashPage extendedSplash = new SplashPage(e.SplashScreen);
                        rootFrame.Content = extendedSplash;
                    }
                    else
                    {
                        rootFrame.Navigate(typeof(LoginPage));
                    }
                    */
                }
            }
            else
            {
                this.NavigateFromArgs(e.Arguments);
            }

            

            PrimaryTileManager.Instance.ResetContent();
        }

        private bool NavigateFromArgs(string args)
        {
#if DEBUG
            if(System.Diagnostics.Debugger.IsAttached)
            {
                //args = "sound=default&sandbox=0&from_id=-154148777&place=wall-154148777_3444";
                //args = "push_id=msg_375988312_42927&badge=7&uid=375988312&msg_id=42927";
                //args = "type=mention&place=topic-155775051_38965377&uid=375988312&badge=1&_genSrv=622117&sound=default&reply_id=4&sandbox=0";



                //args = "badge=3&sound=default&try_internal=1&url=https%3A%2F%2Fvk.com%2Fwall-55527953_32032&_genSrv=807116&type=open_url&sandbox=0&log_date=1565445117";
                //args = "url=https://vk.com/loftproektetagi";
                //args = "type=open_url&url=https://vk.com/wall-26493942_3869175";
            }
#endif
            if (string.IsNullOrEmpty(args))
                return false;

            Logger.Instance.Info("NavigateFromArgs: " + args);

            args = args.Replace("&amp;", "&");
            //Активация приложения из пуша

            //badge - количество непрочитанных сообщений у пользователя
            //uid - ID пользователя, отправившего сообщения 
            //msg_id - ID сообщения 


            //CustomUriMapper : UriMapperBase

            /*
sound=default&_genSrv=605318&from_id=-55527953&place=wall-55527953_27955&badge=7&sandbox=0 (больше так не приходит?)

push_id=msg_-157369801_42921&uid=-157369801&msg_id=42921 //сберкот прислал стикер

push_id=msg_375988312_42927&badge=7&uid=375988312&msg_id=42927 //сообщение от пользователя

uid=2000000017&msg_id=42928&push_id=chat_2000000017_42928 // сообщение из беседы

uid=3934776&group_id=159224886//елена боровик приглашает в сообщество

from_id=-55527953&place=wall-55527953_28016//сообщество опубликовало запись

uid=449217456&type=friend // заявка в друзья

type=mention&place=topic-155775051_38965377&uid=375988312 // упоминание в обсуждении 

type=friend_found&uid=527914199 // друг зарегестрировался в ВК

type=open_url&url=https%3A%2F%2Fvk.com%2Fwall-162756747_50702 // сообщество опубликовало запись

            launch="badge=22&amp;log_date=1589216664&amp;_genSrv=626730&amp;msg_id=615102&amp;sound=default&amp;sandbox=0&amp;uid=2000000091&amp;push_id=chat_2000000091_615102"
            */

            Dictionary<string, string> paramDict = args.ParseQueryString();
            // Пришло сообщение
            if (paramDict.ContainsKey("msg_id") && paramDict.ContainsKey("uid"))
            {
                uint message_id = uint.Parse(paramDict["msg_id"]);
                int peer_id = int.Parse(paramDict["uid"]);//ЭТО peer_id!!!
                int chat_id = 0;
                if (paramDict.ContainsKey("push_id"))
                {
                    string push_id = paramDict["push_id"];
                    string[] temp2 = push_id.Split('_');
                    if (temp2[0] == "chat")
                    {
                        chat_id = int.Parse(temp2[1]) - 2000000000;
                        peer_id = 0;
                    }
                }
                if (chat_id == 0)
                    Library.NavigatorImpl.Instance.NavigateToConversation(peer_id);
                else
                    Library.NavigatorImpl.Instance.NavigateToConversation(chat_id + 2000000000);
                //Library.NavigatorImpl.Instance.NavigateToConversation(user_id, chat_id);
                return true;
            }

            //_genSrv=623611&badge=4&sound=default&sandbox=0&     from_id=-55527953&place=wall-55527953_28016//сообщество опубликовало запись
            if (paramDict.ContainsKey("place"))
            {
                string text2 = paramDict["place"];
                
                if (text2.Substring(0, 4) == "wall")//wall-55527953_28016
                {
                    string[] temp2 = text2.Remove(0, 4).Split('_');

                    int ownerId = int.Parse(temp2[0]);
                    uint postId = uint.Parse(temp2[1]);

                    Library.NavigatorImpl.Instance.NavigateToWallPostComments(ownerId,postId);
                    return true;
                }
                else if (text2.Substring(0, 5) == "topic")//topic-155775051_38965377
                {
                    string[] temp2 = text2.Remove(0, 5).Split('_');

                    int groupId = int.Parse(temp2[0]);
                    uint topicId = uint.Parse(temp2[1]);
                    uint commentId = 0;
                    if (paramDict.ContainsKey("reply_id"))
                    {
                        string text3 = paramDict["reply_id"];
                        commentId = uint.Parse(text3);
                    }

                    Library.NavigatorImpl.Instance.NavigateToGroupDiscussion((uint)-groupId, topicId,"",true, commentId);
                    return true;
                }

                    //return new Uri(NavigatorImpl.GetNavigateToPostCommentsNavStr(long.Parse(text2.Remove(0, text2.IndexOf('_') + 1)), ownerId, false, 0, 0, "") + "&ClearBackStack=true", UriKind.Relative);
            }

            //url=https%3A%2F%2Fvk.com%2Fwall-55527953_32032&type=open_url&sandbox=0&log_date=1565445117
            if (paramDict.ContainsKey("url"))
            {
                string url0 = paramDict["url"];
                string url = System.Net.WebUtility.UrlDecode(url0);
                //Windows.Data.Html.HtmlUtilities.ConvertToText(text);//на 10.0.10586 вылеты
                Library.NavigatorImpl.Instance.NavigateToWebUri(url);
                return true;
            }

            if(paramDict.ContainsKey("action"))
            {
                //action=openFolderDownload&amp;downloadId={FileName}&amp;folder=temp
                //action=openFileDownload&amp;downloadId={FileName}&amp;folder=temp

                if (paramDict["action"] == "openFolderDownload")
                {
                    Task.Run(async () =>
                    {
                        var file = await KnownFolders.DocumentsLibrary.GetFileAsync(paramDict["downloadId"]);
                        if (file != null)
                        {
                            var opts = new FolderLauncherOptions();
                            opts.ItemsToSelect.Add(file);
                            await Launcher.LaunchFolderAsync(KnownFolders.DocumentsLibrary, opts);
                        }
                    });

                    return true;
                }

                if (paramDict["action"] == "viewDownloads")
                {
                    Task.Run(async () =>
                    {
                        await Launcher.LaunchFolderAsync(KnownFolders.DocumentsLibrary);
                    });
                    Library.NavigatorImpl.Instance.NavigateToDownloads();
                    return true;
                }


                if (paramDict["action"] == "openFileDownload")
                {
                    Task.Run(async () =>
                    {
                        var file = await KnownFolders.DocumentsLibrary.GetFileAsync(paramDict["downloadId"]);
                        await Launcher.LaunchFileAsync(file);
                    });

                    return true;
                }
            }

            return false;
        }

        /*
         * Если push-уведомление имеет тип активации переднего плана,
         * вызовите этот метод из переопределения метода OnActivated в своем
         * приложении и передайте аргументы, доступные в объекте ToastNotificationActivatedEventArgs,
         * передаваемом этому методу. В следующем примере кода предполагается,
         * что файл кода содержит операторы using для пространств имен 
         * */
        protected override void OnActivated(IActivatedEventArgs args)
        {
            bool isEmpty = Window.Current.Content == null;

            base.OnActivated(args);
            
            this.InitFrame();
#if WINDOWS_UWP
            if (args.Kind == ActivationKind.ToastNotification)
            {
                ToastNotificationActivatedEventArgs toastArgs = args as ToastNotificationActivatedEventArgs;
                string arguments = toastArgs.Argument;//log_date=1568900813&_genSrv=626229&uid=375988312&msg_id=78328&sound=default&sandbox=0&push_id=msg_375988312_78328&badge=5

                //var inp = toastArgs.UserInput;
                //var temp = inp["textBox"];

                //Library.NavigatorImpl.Instance.NavigateToNewsFeed();
                this.NavigateFromArgs(arguments);
            }
#endif

            

            if (args.Kind == ActivationKind.Protocol)
            {
                if (isEmpty)
                {
                    if (Settings.IsAuthorized)
                    {
                        

                    }
                    else
                    {
                        CustomFrame rootFrame = Window.Current.Content as CustomFrame;
                        rootFrame.Navigate(typeof(LoginPage));
                        return;
                    }
                }


                var protocolArgs = (ProtocolActivatedEventArgs)args;
                string uri = protocolArgs.Uri.AbsoluteUri;

                Library.NavigatorImpl.Instance.NavigateToWebUri(uri);
            }
        }
#if USE_PlaybackSession
        //introduced v10.0.14393.0 Windows.Foundation.UniversalApiContract (introduced v3.0)
        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs a)
        {
            Logger.Instance.Info("OnBackgroundActivated");

            base.OnBackgroundActivated(a);

            BackgroundTaskDeferral deferral = a.TaskInstance.GetDeferral(); // Get a deferral since we're executing async code

            if (!Settings.IsAuthorized)// мы вышли из аккаунта и отвечаем в оставшемся пуше
            {
                deferral.Complete();
                return;
            }

            if (a.TaskInstance.TriggerDetails is ToastNotificationActionTriggerDetail details)
            {
                IReadOnlyDictionary<string, string> args = details.Argument.Replace("&amp;", "&").ParseQueryString();

                Logger.Instance.Info("NotificationBackgroundTask args: " + details.Argument);

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
                                switch (Settings.DEV_BackgroundAnswerMethode)
                                {
                                    case 1:
                                        {
                                            Dictionary<string, string> parameters = new Dictionary<string, string>();
                                            parameters["message"] = message;
                                            parameters["peer_id"] = peer_id.ToString();
                                            parameters["random_id"] = Convert.ToInt32((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();
                                            parameters["v"] = Constants.API_VERSION;
                                            parameters["access_token"] = Settings.AccessToken;
                                            //this.GetResponse("messages.send", parameters, deferral);

                                            try
                                            {
                                                using (HttpClient client = new HttpClient())
                                                {
                                                    var response = await client.PostAsync(new Uri(VKRequestsDispatcher.RequestUriFrm + "messages.send"), new HttpFormUrlEncodedContent(parameters));
                                                    string content = await response.Content.ReadAsStringAsync();
                                                    //Settings.DEV_BackgroundData += ("NotificationBackgroundTask result1: '" + content + "'\n");

                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                //Settings.DEV_BackgroundData += ("NotificationBackgroundTask fail1: '" + ex.Message + "'\n");
                                            }
                                            finally
                                            {
                                                deferral.Complete();
                                            }


                                            break;
                                        }
                                    case 2:
                                        {

                                            Dictionary<string, string> parameters = new Dictionary<string, string>();
                                            parameters["message"] = message;
                                            parameters["peer_id"] = peer_id.ToString();
                                            parameters["random_id"] = Convert.ToInt32((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();
                                            parameters["v"] = Constants.API_VERSION;
                                            parameters["access_token"] = Settings.AccessToken;
                                            //VKRequestsDispatcher.DispatchRequestToVK<int>("messages.send", parameters, (result) => { deferral.Complete(); });

                                            try
                                            {
                                                var client = new System.Net.Http.HttpClient();
                                                var response = await client.PostAsync(new Uri(VKRequestsDispatcher.RequestUriFrm + "messages.send"), new System.Net.Http.FormUrlEncodedContent(parameters));
                                                string content = await response.Content.ReadAsStringAsync();
                                                //Settings.DEV_BackgroundData += ("NotificationBackgroundTask result2: '" + content + "'\n");
                                            }
                                            catch (Exception ex)
                                            {
                                                //Settings.DEV_BackgroundData += ("NotificationBackgroundTask fail2: '" + ex.Message + "'\n");
                                            }
                                            finally
                                            {
                                                deferral.Complete();
                                            }

                                            break;
                                        }
                                    default://0
                                        {
                                            MessagesService.Instance.SendMessage(peer_id, (result) =>
                                            {
                                                //if (result.error.error_code != LunaVK.Core.Enums.VKErrors.None)
                                                //    Logger.Instance.Error("Can't send msg from background", result.error.error_code);
                                                //Settings.DEV_BackgroundData += ("NotificationBackgroundTask result0: '" + result.error.error_code + "'\n");//
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
                            FavoritesService.Instance.AddRemovePost(owner_id, item_id, true, (result) => { deferral.Complete(); });
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
#endif
        /*
        private void ApplyScale()
        {
            int scale = Settings.UIScale;
            if (scale == 100)
                return;

            this.DoScale("Double20", scale);
            this.DoScale("Double40", scale);
            this.DoScale("Double50", scale);
            this.DoScale("Double55", scale);
            this.DoScale("Double64", scale);
            this.DoScale("Double72", scale);
            this.DoScale("Double96", scale);
            this.DoScale("FontSizeSmall", scale);
            this.DoScale("FontSizeContent", scale);
            this.DoScale("FontSizeLarge", scale);
            this.DoScale("FontSizeExtraLarge", scale);
        }
        */

        
            /*
        private Windows.UI.Color ChangeColorBrightness(Windows.UI.Color color, float correction)
        {
            float r = color.R;
            float g = color.G;
            float b = color.B;

            if (correction < 0)
            {
                correction = 1 + correction;
                r *= correction;
                g *= correction;
                b *= correction;
            }
            else
            {
                r = (255 - r) * correction + r;
                g = (255 - g) * correction + g;
                b = (255 - b) * correction + b;
            }
            return Windows.UI.Color.FromArgb(color.A, (byte)r, (byte)g, (byte)b);
        }
        */
        private void DoScale(string field, double scale)
        {
            Application.Current.Resources[field] = (double)Application.Current.Resources[field] * scale / 100.0;
        }
        /*
        private void ApplyTheme(CustomFrame frame)
        {
            frame.RequestedTheme = Settings.BackgroundType ? ElementTheme.Light : ElementTheme.Dark;
        }
        */
        

        /// <summary>
        /// Вызывается при приостановке выполнения приложения.  Состояние приложения сохраняется
        /// без учета информации о том, будет ли оно завершено или возобновлено с неизменным
        /// содержимым памяти.
        /// </summary>
        /// <param name="sender">Источник запроса приостановки.</param>
        /// <param name="e">Сведения о запросе приостановки.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            Logger.Instance.Info("OnSuspending");

            // Allowed only 5 seconds to do the storage
            var deferral = e.SuspendingOperation.GetDeferral();
            await AudioCacheManager.Instance.Save();
            //StickersAutoSuggestDictionary.Instance.SaveState();
            DialogsViewModel.Save();
            Logger.Instance.Save();
            deferral.Complete();
        }

        protected override void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            ShareOperation shareOperation = args.ShareOperation;
            if(!Settings.IsAuthorized)
            {
                shareOperation.ReportCompleted();
                return;
            }
            // Code to handle activation goes here.

            this.InitFrame();
            
            
            if (shareOperation != null)
            {
                DataPackageView data = shareOperation.Data;
                shareOperation.ReportStarted();

                if (data.Contains(StandardDataFormats.StorageItems) || data.Contains(StandardDataFormats.WebLink) || data.Contains(StandardDataFormats.Text))
                {
                    //ShareContentDataProviderManager.StoreDataProvider(new ShareExternalContentDataProvider(shareOperation));

                    Library.NavigatorImpl.Instance.NavigateShareExternalContentpage(shareOperation);
                }
            }




            bool isPeopleShare = false;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                // Make sure the current OS version includes the My People feature before
                // accessing the ShareOperation.Contacts property
                isPeopleShare = (args.ShareOperation.Contacts.Count > 0);
            }

            if (isPeopleShare)
            {
                // Show share UI for MyPeople contact(s)
            }
            else
            {
                // Show standard share UI for unpinned contacts
            }

        }

        /// <summary>
        /// Происходит при переходе приложения из состояния приостановки в состояние выполнения.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResuming(object sender, object e)
        {
            System.Diagnostics.Debug.WriteLine("App_Resuming");
        }

        /// <summary>
        /// Происходит, когда исключение может быть обработано кодом приложения в том виде,
        //     в каком оно передается из ошибки среды выполнения Windows машинного уровня. В
        //     данных события приложения могут помечать экземпляр события как обработанный.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Logger.Instance.Error("UNHANDLED", e.Exception);
            if (Settings.DEV_IsLogsPopupEnabled)
            {
                var res = MessageBox.Show("Unhandled error", e.Message + Environment.NewLine + e.Exception.StackTrace, MessageBox.MessageBoxButton.OK);
            }
        }

        private void InitFrame()
        {
            CustomFrame rootFrame = Window.Current.Content as CustomFrame;

            if (rootFrame == null)
            {
                Settings.Initialize();

                if(Settings.LanguageSettings!=0)
                {
                    ApplicationLanguages.PrimaryLanguageOverride = Windows.System.UserProfile.GlobalizationPreferences.Languages[Settings.LanguageSettings-1];
                }
                

                ThemeManager.ApplyColors();
            }

#if WINDOWS_PHONE_APP
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);//Задействуем системный трей

            StatusBar.GetForCurrentView().BackgroundColor = Windows.UI.Colors.Transparent;
#endif
            // Не повторяйте инициализацию приложения, если в окне уже имеется содержимое,
            // только обеспечьте активность окна
            if (rootFrame == null)
            {
                CustomFrame.Instance = new CustomFrame(); // Создание фрейма, который станет контекстом навигации
                rootFrame = CustomFrame.Instance;
                rootFrame.Loaded += this.RootFrame_Loaded;

                if (rootFrame.IsDevicePhone)
                {
                    var bar = StatusBar.GetForCurrentView();
                    bar.BackgroundColor = Windows.UI.Colors.Transparent;
                    bar.ForegroundColor = Windows.UI.Colors.White;
                }

                //
                rootFrame.RequestedTheme = Settings.BackgroundType ? ElementTheme.Light : ElementTheme.Dark;//this.ApplyTheme(rootFrame);
                //
                /*
#if WINDOWS_UWP
                //this allows nav bar and status bar to overlay the app
                var view = ApplicationView.GetForCurrentView();

                view.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
                view.ExitFullScreenMode();
#endif
*/
                // TODO: Измените это значение на размер кэша, подходящий для вашего приложения
                rootFrame.CacheSize = 1;

                //   if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                //   {
                // TODO: Загрузить состояние из ранее приостановленного приложения
                //    }

                Window.Current.Content = rootFrame;// Размещение фрейма в текущем окне

                //
                Window.Current.Activate(); // Обеспечение активности текущего окна
            }
        }

        private void RootFrame_Loaded(object sender, RoutedEventArgs e)
        {
            Library.MessengerStateManager.Instance.Initialize();
            if (Settings.IsAuthorized)
            {
                Library.PushNotifications.Instance.UpdateDeviceRegistration();

            }
        }
        /*
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            this.RemoveCurrentDeactivationSettings();
            this.RespondToDeactivationOrClose();
        }

        private void RespondToDeactivationOrClose()
        {
            AppGlobalStateManager.Current.GlobalState.LastDeactivatedTime = DateTime.Now;
            BGAudioPlayerWrapper.Instance.RespondToAppDeactivation();
            this.SaveState();
            TileManager.Instance.ResetContent();
        }

        private void RestoreState(bool initialLaunch)
        {
            AppGlobalStateManager.Current.Initialize(true);
            CacheManager.TryDeserialize((IBinarySerializable)ImageCache.Current, App._imageDictionaryKey, CacheManager.DataType.CachedData);
            CountersManager.Current.Restore();
            AudioCacheManager.Instance.EnsureCachingInRunning();
            ConversationsViewModelUpdatesListener.Listen();
            ConversationViewModelCache.Current.SubscribeToUpdates();
            MediaLRUCache instance = MediaLRUCache.Instance;
            StickersAutoSuggestDictionary.Instance.RestoreStateAsync();
        }

        private void SaveState()
        {
            ConversationsViewModel.Save();
            CountersManager.Current.Save();
            AppGlobalStateManager.Current.SaveState();
            CacheManager.TrySerialize((IBinarySerializable)ImageCache.Current, App._imageDictionaryKey, false, CacheManager.DataType.CachedData);
            VeryLowProfileImageLoader.SaveState();
            SubscriptionFromPostManager.Instance.Save();
            ConversationViewModelCache.Current.FlushToPersistentStorage();
            AudioCacheManager.Instance.Save();
            MediaLRUCache.Instance.Save();
            StickersAutoSuggestDictionary.Instance.SaveState();
        }
        */
    }
}
 