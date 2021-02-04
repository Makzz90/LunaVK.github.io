using System;
using System.Collections.Generic;
using LunaVK.Core.DataObjects;
using Newtonsoft.Json;
using LunaVK.Core.Enums;
using LunaVK.Core.Network;
using LunaVK.Core;
using System.Threading;
using Windows.UI.Xaml;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;

using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json.Linq;
using LunaVK.Core.Utils;

namespace LunaVK.Network
{
    //String url = "https://" + this.server + "?act=a_check&key=" + this.key + "&ts=" + this.ts + "&wait=25&mode=234&version=" + "1";
    public class LongPollServerService
    {
        private DelayedExecutor _de = new DelayedExecutor(5000);
        public CountersArgs _counters = new CountersArgs();
        private CancellationTokenSource ct = new CancellationTokenSource();
        public delegate void UpdatesReceivedEventHandler(List<UpdatesResponse.LongPollServerUpdateData> updates);

        public event LongPollServerService.UpdatesReceivedEventHandler ReceivedUpdates;

        //https://api.vk.com:443/newuim375988312? act=a_check key=963a2ddd43caf36a83f474714cde441f482d6021 ts=1785042013 wait=25 mode=362 version=5 
        //private readonly string _getUpdatesURIFrm = "https://{0}?act=a_check&key={1}&ts={2}&wait=25&mode=66&version=3";
        private readonly string _getUpdatesURIFrm = "https://{0}?act=a_check&key={1}&ts={2}&wait=25&mode=330&version=5";//362

        /*
        2 — получать вложения;
        8 — возвращать расширенный набор событий;
        32 — возвращать pts (это требуется для работы метода messages.getLongPollHistory без ограничения в 256 последних событий);
        64 — в событии с кодом 8 (друг стал онлайн) возвращать дополнительные данные в поле $extra (см. Структура событий);
        128 — возвращать поле random_id (random_id может быть передан при отправке сообщения методом messages.send).
        */

        int i = 0;

        private uint group_id;

        private static LongPollServerService _instance;
        public static LongPollServerService Instance
        {
            get
            {
                if (LongPollServerService._instance == null)
                    LongPollServerService._instance = new LongPollServerService();
                return LongPollServerService._instance;
            }
        }

        public LongPollServerService()
        {
            //Windows.Networking.Connectivity.NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            Window.Current.VisibilityChanged += this.Current_VisibilityChanged;
        }

        /// <summary>
        ///  Происходит при изменении значения свойства Visible.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_VisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            Logger.Instance.Info("LongPoll VisibilityChanged " + e.Visible.ToString());

            if (e.Visible)
            {
                if (Settings.IsAuthorized)
                {
#if DEBUG
                    //System.Diagnostics.Debug.WriteLine("VisibilityChanged: Restart");
#endif
                    this.Restart();
                }
            }
            else
            {
#if DEBUG
                //System.Diagnostics.Debug.WriteLine("VisibilityChanged: Stop");
#endif
                this.Stop();
            }
        }

        /// <summary>
        /// Ожидает очень длинный запрос
        /// </summary>
        /// <param name="token">Бейдж доступа к аккаунту</param>
        /// <param name="requestsSettings"></param>
        private void RunRequestsLoop(string token, LongPollServerResponse requestsSettings)
        {
            //Logger.Instance.Info("LPS:RunRequestsLoop called {0}", requestsSettings == null);
            //System.Diagnostics.Debug.WriteLine("RunRequestLoop: called");
            this.GetUpdates(requestsSettings.server, requestsSettings.key, requestsSettings.ts, (res) =>
            {
                //Logger.Instance.Info("LPS:RunRequestsLoop 1 {0}", res == null);
                if (res == null)//no internet
                {
#if DEBUG
                    //System.Diagnostics.Debug.WriteLine("RunRequestLoop: -" + this.ct.IsCancellationRequested);
#endif
                    if (!this.ct.IsCancellationRequested)
                    {
                        //System.Diagnostics.Debug.WriteLine("RunRequestLoop failed");
                        this.Restart();
                    }
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("RunRequestLoop: +");

                    requestsSettings.ts = res.ts;

                    //Logger.Instance.Info("LPS:RunRequestsLoop 2 {0}", res.Updates.Count);

                    if (res.Updates.Count > 0)
                        this.EnrichUpdateData(res.Updates);
                    //Logger.Instance.Info("LPS:RunRequestsLoop finish");
                    this.RunRequestsLoop(token, requestsSettings);
                    //Logger.Instance.Info("LPS:RunRequestsLoop 4");
                }

            });
        }

        /// <summary>
        /// Рассказать интерфейсу о новых данных
        /// </summary>
        /// <param name="updateData"></param>
        /// <returns></returns>
        public void EnrichUpdateData(List<UpdatesResponse.LongPollServerUpdateData> updateData)
        {
            /*
             * if (u.UpdateType != LongPollServerUpdateType.MessageHasBeenAdded || !u.hasAttachOrForward && !u.isChat || u.message != null)
                  return u.UpdateType == LongPollServerUpdateType.MessageHasBeenRestored;
                return true;
        */
        //public для отладки
            List<uint> messageIds = updateData.Where((u =>
            {
                return (u.fwds != null || u.UpdateType == LongPollServerUpdateType.MessageHasBeenRestored || u.attachments != null || u.UpdateType == LongPollServerUpdateType.MessageUpdate);
            })).Select((u => u.message_id)).ToList();

            
            List<int> associatedUsersIds = updateData.Where<UpdatesResponse.LongPollServerUpdateData>((u =>
            {
                if (u.source_mid != 0 || u.from_admin != 0)
                    return true;

                if (u.UpdateType != LongPollServerUpdateType.ReplacePeerFlag)
                    return u.UpdateType == LongPollServerUpdateType.MessageHasBeenRestored;
                return true;
            })).Select<UpdatesResponse.LongPollServerUpdateData, int>((u => u.user_id)).Distinct().ToList();

            List<uint> messageIds2 = updateData.Where((u =>
            {
                if (u.UpdateType != LongPollServerUpdateType.MessageAdd)
                    return false;

                if (u.user_id > 0)
                    return UsersService.Instance.GetCachedUser((uint)u.user_id) == null;
                return GroupsService.Instance.GetCachedGroup((uint)-u.user_id) == null;
            })).Select((u => u.message_id)).ToList();

            if(messageIds2.Count>0)
            {
                messageIds.AddRange(messageIds2);
            }

            if (messageIds.Count > 0)
            {
                MessagesService.Instance.GetMessages(messageIds, (resMessages) =>
                {
                    if (resMessages != null)
                    {
                        UsersService.Instance.SetCachedUsers(resMessages.profiles);
                        GroupsService.Instance.SetCachedGroups(resMessages.groups);

                        foreach (VKMessage m in resMessages.items)
                        {
                            var update = updateData.First((u) => { return u.message_id == m.id; });
                            update.message = m;
                        }
                    }
                    this.ReceivedUpdates?.Invoke(updateData);
                });
            }
            else
            {
                this.ReceivedUpdates?.Invoke(updateData);
            }
        }

        /// <summary>
        /// Запускает функцию GetLongPollServer с задержкой
        /// </summary>
        /// <param name="fast"></param>
        public void Restart()
        {
#if DEBUG
            Logger.Instance.Info("LongPoll: Restart");
#endif
            //Logger.Instance.Info("LPS:Restart 1");
            this.Stop();
            this.ct = new CancellationTokenSource();
            //Logger.Instance.Info("LPS:Restart 2");
            //
            bool NetworkAvailable = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            this.GetLongPollServer((result) =>
            {
                Logger.Instance.Info("InstantUpdatesManager.Restart callback result = {0}", result.error.error_code);
                if (result.error.error_code == VKErrors.None)
                {
                    this.RunRequestsLoop(Settings.AccessToken, result.response.s);

                    this._counters = result.response.c;

                    EventAggregator.Instance.PublishCounters(this._counters);
#if DEBUG
                    //                System.Diagnostics.Debug.WriteLine("LongPollServer ts: " + temp.response.s.ts);
#endif


                    int unixTimestamp = Core.Utils.Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, false);
                    Settings.ServerMinusLocalTimeDelta = result.response.time - unixTimestamp;
                }
                else
                    this._de.AddToDelayedExecution(this.Restart);
            });
        }

        /// <summary>
        /// Спрашиваем у вк адрес для логпула и
        /// обновляем счётчики
        /// </summary>
        /// <param name="callback"></param>
        private void GetLongPollServer(Action<VKResponse<LongPollServerResponseExtended>> callback)
        {
#if DEBUG
            //System.Diagnostics.Debug.WriteLine("GetLongPollServer");
#endif
            string line = Settings.DEV_SetOffline ? "setOffline" : "setOnline";
            string group = this.group_id > 0 ? (",group_id:"+ this.group_id) : "";
            VKRequestsDispatcher.Execute<LongPollServerResponseExtended>("API.account."+ line +"();var c=API.getCounters();return {c:c,s:API.messages.getLongPollServer({use_ssl:1,lp_version:5"+ group+"}),time:API.utils.getServerTime()};", callback, (jsonString)=>
            {
                jsonString = jsonString.Replace("-1", "0");
                return VKRequestsDispatcher.FixArrayToObject(jsonString, "c");
            }, false, this.ct.Token);
        }

        /// <summary>
        /// Создаёт объявление для интерфеса о смене счётчиков сообщений
        /// </summary>
        /// <param name="c"></param>
        public void SetUnreadMessages(uint c)
        {
            this._counters.messages = c;

            EventAggregator.Instance.PublishCounters(this._counters);
        }

        public void Stop()
        {
            i++;
            Logger.Instance.Info("LongPoll: Stop");
            this.ct.Cancel(true);
            this._de.Cancel();
        }

        /// <summary>
        /// Делаем длинный запрос
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="key"></param>
        /// <param name="ts"></param>
        /// <param name="callback"></param>
        private void GetUpdates(string serverName, string key, long ts, Action<UpdatesResponse> callback)
        {
            //Logger.Instance.Info("LPS:GetUpdates 1");
            //System.Diagnostics.Debug.WriteLine("GetUpdates");
            //Делаем долгий запрос
            JsonWebRequest.SendHTTPRequestAsync(string.Format(this._getUpdatesURIFrm, serverName, key, ts), (jsonResp, IsSucceeded) =>
            {
                //Logger.Instance.Info("LPS:GetUpdates 2 {0}", IsSucceeded);
                if (IsSucceeded)//if (res.ResultCode == LongPollResultCode.Succeeded)
                {
#if DEBUG
                    //if (System.Diagnostics.Debugger.IsAttached)
                    //    System.Diagnostics.Debug.WriteLine("Update-> " + jsonResp);
#endif
                    //Logger.Instance.Info("Longpoll_{0}->{1}",i, jsonResp);

                    var objectGetUpdates = JsonConvert.DeserializeObject<LongPollServerService.RootObjectGetUpdates>(jsonResp);
                    //Logger.Instance.Info("LPS:GetUpdates 3");
                    UpdatesResponse backendResult = new UpdatesResponse();
                    backendResult.ts = objectGetUpdates.ts;
                    //Logger.Instance.Info("LPS:GetUpdates 4 '{0}'", jsonResp);
                    backendResult.Updates = this.ReadUpdatesResponseFromRaw(objectGetUpdates.updates);
                    //Logger.Instance.Info("LPS:GetUpdates 5");
                    callback(backendResult);
                    return;
                }
                else
                {
                    callback(null);
                }
            },null,this.ct.Token);
        }
#if DEBUG
        public void FakeData(string data)
        {
            UpdatesResponse res = new UpdatesResponse();
            var objectGetUpdates = JsonConvert.DeserializeObject<LongPollServerService.RootObjectGetUpdates>(data);
            //ResultData.ts = objectGetUpdates.ts;
            res.Updates = this.ReadUpdatesResponseFromRaw(objectGetUpdates.updates);
            if (res.Updates.Count > 0)
                this.EnrichUpdateData(res.Updates);
        }
#endif
        /*
         * Так алгоритм таков:
         * Вытягиваем данные из лонгпула
         * Парсим и переделываем в классы
         * */


        /// <summary>
        /// Преобразует чистые данные в класс с обновлениями
        /// </summary>
        /// <param name="rawUpdates"></param>
        /// <returns></returns>
        private List<UpdatesResponse.LongPollServerUpdateData> ReadUpdatesResponseFromRaw(List<List<object>> rawUpdates/*, Func<List<object>, UpdatesResponse.LongPollServerUpdateData> getUpdatesForNewMessageFunc*/)
        {
            List<UpdatesResponse.LongPollServerUpdateData> serverUpdateDataList1 = new List<UpdatesResponse.LongPollServerUpdateData>();
            if (rawUpdates != null)
            {
                foreach (List<object> rawUpdate in rawUpdates)
                {
                    if (rawUpdate != null)
                    {
                        LongPollServerUpdateType type = (LongPollServerUpdateType)int.Parse(rawUpdate[0].ToString());
                        switch (type)
                        {
                            case LongPollServerUpdateType.ReplaceMsgFlag:
                                {
                                    //1
                                    /*
                                    $message_id (integer)
                                    $flags (integer)
                                    extra_fields* 
                                    */
                                    break;
                                }
                            case LongPollServerUpdateType.ProcessAddFlags:
                                {

                                    //2 A
                                    /*
                                    $message_id (integer)
                                    $mask (integer)
                                    extra_fields*
                                    */

                                    //Android: 64
                                    if ((int.Parse(rawUpdate[2].ToString()) & 128) == 128)//DELЕTЕD
                                    {
                                        uint message_id = uint.Parse(rawUpdate[1].ToString());
                                        int peer_id = int.Parse(rawUpdate[3].ToString());
                                        
                                        serverUpdateDataList1.Add(new UpdatesResponse.LongPollServerUpdateData()
                                        {
                                            message_id = message_id,
                                            peer_id = peer_id,
                                            UpdateType = LongPollServerUpdateType.MessageHasBeenDeleted
                                        });
                                    }
                                    break;
                                }
                            case LongPollServerUpdateType.ClearMsgFlags:
                                {
                                    //3 A
                                    /*
                                    $message_id (integer)
                                    $mask (integer)
                                    extra_fields*
                                    */
                                    int mask = int.Parse(rawUpdate[2].ToString());

                                    //UNREAD
                                    if ((mask & 1) == 1)
                                    {
                                        uint message_id = uint.Parse(rawUpdate[1].ToString());
                                        if (rawUpdate.Count >= 4)
                                        {
                                            int peer_id = int.Parse(rawUpdate[3].ToString());

                                            serverUpdateDataList1.Add(new UpdatesResponse.LongPollServerUpdateData()
                                            {
                                                message_id = message_id,
                                                peer_id = peer_id,
                                                UpdateType = LongPollServerUpdateType.MessageHasBeenRead
                                            });
                                        }
                                    }

                                    //DELЕTЕD
                                    if ((mask & 128) == 128)
                                    {
                                        /*
                                        UpdatesResponse.LongPollServerUpdateData serverUpdateData = this.ReadUserOrChatIds(rawUpdate);
                                        if (serverUpdateData != null)
                                        {
                                            serverUpdateData.UpdateType = LongPollServerUpdateType.MessageHasBeenRestored;
                                            serverUpdateDataList1.Add(serverUpdateData);
                                        }*/
                                        uint message_id = uint.Parse(rawUpdate[1].ToString());
                                        int peer_id = int.Parse(rawUpdate[3].ToString());

                                        serverUpdateDataList1.Add(new UpdatesResponse.LongPollServerUpdateData()
                                        {
                                            message_id = message_id,
                                            peer_id = peer_id,
                                            UpdateType = LongPollServerUpdateType.MessageHasBeenRestored
                                        });
                                    }
                                    break;
                                }
                            case LongPollServerUpdateType.MessageAdd:
                                {
                                    //4 A
                                    /*
                                    $message_id (integer)
                                    $flags (integer)
                                    extra_fields*
                                    */

                                    //[4,2899,8195,2000000007,1556718081,"",{"source_act":"chat_create","source_text":"8","from":"375988312"},{},1,0]]}
                                    UpdatesResponse.LongPollServerUpdateData serverUpdateData = this.GetUpdateDataForNewMessageLongPollData(rawUpdate);
                                    serverUpdateDataList1.Add(serverUpdateData);
                                    break;
                                }
                            case LongPollServerUpdateType.MessageUpdate:
                                {
                                    //5 A
                                    /*
                                    $message_id (integer)
                                    $mask(integer)
                                    $peer_id(integer)
                                    $timestamp(integer)
                                    $new_text(string)
                                    [$attachments](array)
                                    0 
                                    */
                                    UpdatesResponse.LongPollServerUpdateData serverUpdateData = this.GetUpdateDataForNewMessageLongPollData(rawUpdate);
                                    serverUpdateDataList1.Add(serverUpdateData);
                                    break;
                                }
                            case LongPollServerUpdateType.IncomingMessagesRead:
                                {
                                    //6 A
                                    /*
                                    $peer_id (integer)
                                    $local_id (integer)
                                    */
                                    int peer_id = int.Parse(rawUpdate[1].ToString());
                                    var serverUpdateData = new UpdatesResponse.LongPollServerUpdateData()
                                    {
                                        UpdateType = type,
                                        peer_id = peer_id,
                                        message_id = uint.Parse(rawUpdate[2].ToString())
                                    };

                                    serverUpdateDataList1.Add(serverUpdateData);
                                    break;
                                }
                            case LongPollServerUpdateType.OutcominggMessagesRead:
                                {
                                    //7 A
                                    /*
                                    $peer_id (integer)
                                    $local_id (integer)
                                    */
                                    int peer_id = int.Parse(rawUpdate[1].ToString());
                                    var serverUpdateData = new UpdatesResponse.LongPollServerUpdateData()
                                    {
                                        UpdateType = type,
                                        peer_id = peer_id,
                                        message_id = uint.Parse(rawUpdate[2].ToString())
                                    };

                                    serverUpdateDataList1.Add(serverUpdateData);
                                    break;
                                }
                            case LongPollServerUpdateType.UserBecameOnline:
                                {
                                    //8

                                    //-$user_id (integer)
                                    //$extra(integer)
                                    //$timestamp(integer)

                                    int user_id = -int.Parse(rawUpdate[1].ToString());
                                    int platform = int.Parse(rawUpdate[2].ToString()) % 256;
                                    int timestamp = int.Parse(rawUpdate[3].ToString());

                                    serverUpdateDataList1.Add(new UpdatesResponse.LongPollServerUpdateData()
                                    {
                                        UpdateType = type,
                                        user_id = user_id,
                                        Platform = platform,
                                        timestamp = timestamp
                                    });

                                    break;
                                }
                            case LongPollServerUpdateType.UserBecameOffline:
                                {
                                    //9

                                    //-$user_id (integer)
                                    //$flags (integer)
                                    //$timestamp(integer) 


                                    int user_id = -int.Parse(rawUpdate[1].ToString());
                                    int flags = int.Parse(rawUpdate[2].ToString());
                                    int timestamp = int.Parse(rawUpdate[3].ToString());

                                    serverUpdateDataList1.Add(new UpdatesResponse.LongPollServerUpdateData()
                                    {
                                        UpdateType = type,
                                        user_id = user_id,
                                        timestamp = timestamp
                                    });
                                    break;
                                }
                            case LongPollServerUpdateType.ClearPeerFlags:
                                {
                                    //10
                                    /*
                                    $peer_id (integer)
                                    $mask (integer)
                                    */
                                    int peer_id = int.Parse(rawUpdate[1].ToString());

                                    serverUpdateDataList1.Add(new UpdatesResponse.LongPollServerUpdateData()
                                    {
                                        peer_id = peer_id,
                                        UpdateType = LongPollServerUpdateType.ClearPeerFlags
                                    });
                                    break;
                                }
                            case LongPollServerUpdateType.ReplacePeerFlag:
                                {
                                    //11
                                    /*
                                    $peer_id (integer)
                                    $flags (integer)
                                     * */
                                    break;
                                }
                            case LongPollServerUpdateType.ChatParamsWereChanged:
                                {
                                    //12
                                    /*
                                    $peer_id (integer)
                                    $mask (integer)
                                     * */
                                    int peer_id = int.Parse(rawUpdate[1].ToString());
                                    int mask = int.Parse(rawUpdate[2].ToString());
                                    var temp = this.FlagFromMask(mask);
                                    break;
                                }
                            case LongPollServerUpdateType.ChatDeleteMsgs:
                                {
                                    //13
                                    /*
                                    $peer_id (integer)
                                    $local_id (integer)
                                     * */
                                    break;
                                }
                            case LongPollServerUpdateType.RestoreMsgs:
                                {
                                    //14
                                    /*
                                    $peer_id (integer)
                                    $local_id (integer)
                                     * */
                                    break;
                                }

                            case LongPollServerUpdateType.ChatParamsChanged:
                                {
                                    //51
                                    /*
                                    $chat_id  (integer)
                                    $self (integer) 
                                    */
                                    //Воу воу! Бывает отрицательным! -1440592153
                                    uint chat_id;
                                    if (!uint.TryParse(rawUpdate[1].ToString(), out chat_id))
                                        break;

                                    serverUpdateDataList1.Add(new UpdatesResponse.LongPollServerUpdateData()
                                    {
                                        UpdateType = LongPollServerUpdateType.ChatParamsWereChanged,
                                        peer_id = (int)chat_id + 2000000000,//chat_id = chat_id
                                    });
                                    break;
                                }
                            case LongPollServerUpdateType.ChatInfoChanged:
                                {
                                    //52
                                    /*
                                    $type_id (integer)
                                    $peer_id (integer)
                                    $info(integer)
                                    */
                                    int type_id = int.Parse(rawUpdate[1].ToString());// идентификатор типа измения в чате
                                    int peer_id = int.Parse(rawUpdate[2].ToString());
                                    int info = int.Parse(rawUpdate[3].ToString());
                                    serverUpdateDataList1.Add(new UpdatesResponse.LongPollServerUpdateData()
                                    {
                                        UpdateType = type,
                                        type_id = (UpdatesResponse.LongPollServerUpdateData.type)type_id,
                                        peer_id = peer_id,
                                        info = info
                                    });
                                    break;
                                }
                            case LongPollServerUpdateType.UserIsTyping2:
                            case LongPollServerUpdateType.UserIsTyping:
                                {
                                    //61
                                    /*
                                    $user_id  (integer)
                                    $flags (integer) 
                                    */
                                    int user_id = int.Parse(rawUpdate[1].ToString());
                                    serverUpdateDataList1.Add(new UpdatesResponse.LongPollServerUpdateData()
                                    {
                                        UpdateType = LongPollServerUpdateType.UserIsTyping,
                                        user_id = user_id,
                                        peer_id = user_id
                                    });
                                    break;
                                }
                            case LongPollServerUpdateType.UserIsTypingInChat:
                                {
                                    //62
                                    /*
                                    $user_id  (integer)
                                    $chat_id (integer) 
                                     * */
                                    int user_id = int.Parse(rawUpdate[1].ToString());
                                    uint chat_id = uint.Parse(rawUpdate[2].ToString());
                                    serverUpdateDataList1.Add(new UpdatesResponse.LongPollServerUpdateData()
                                    {
                                        UpdateType = type,
                                        user_id = user_id,
                                        peer_id = (int)chat_id + 2000000000,
                                    });
                                    break;
                                }
                            case LongPollServerUpdateType.UserCalled:
                                {
                                    //70
                                    /*
                                    $user_id  (integer)
                                    $call_id (integer)
                                     * */
                                    int i = 0;
                                    break;
                                }

                            case LongPollServerUpdateType.NewCounter:
                                {
                                    //80	$count  (integer)
                                    //Счетчик сообщений стал равен $count. 
                                    uint result = 0;

                                    if (rawUpdate.Count > 1 && uint.TryParse(rawUpdate[1].ToString(), out result))
                                    {
                                        //serverUpdateDataList1.Add(new UpdatesResponse.LongPollServerUpdateData()
                                        //{
                                        //    UpdateType = type,
                                        //    Counter = result
                                        //});
                                        this.SetUnreadMessages(result);
                                    }
                                    break;
                                }
                            case LongPollServerUpdateType.processnotyfysettings:
                                {
                                    //114
                                    /*
                                    $peer_id (integer)
                                    $sound (integer)
                                    $disabled_until (integer)
                                    */
                                    //V5: {peer_id,sound,disabled_until}

                                    /*
                                    int peer_id = int.Parse(rawUpdate[1].ToString());
                                    int sound = int.Parse(rawUpdate[2].ToString());
                                    if (rawUpdate.Count > 3)
                                    {
                                        int disabled_until = int.Parse(rawUpdate[3].ToString());
                                    }
                                    serverUpdateDataList1.Add(new UpdatesResponse.LongPollServerUpdateData()
                                    {
                                        UpdateType = type,
                                        peer_id = peer_id,
                                    });*/

                                    var ps = JsonConvert.DeserializeObject<PushSettings>(rawUpdate[1].ToString());
                                    serverUpdateDataList1.Add(new UpdatesResponse.LongPollServerUpdateData()
                                    {
                                        UpdateType = type,
                                        peer_id = ps.peer_id,
                                    });
                                    break;
                                }
                            case LongPollServerUpdateType.IncommingCall:
                                {
                                    int debugme = 0;
                                    /*
                                    {{
  "sessionGuid": "44e23bf352bb00b1e6338f97584e0be1",
  "signaling_data": "{\"audio\":{\"codecs\":[\"opus-uwb\",\"opus\",\"isac\",\"speex-wb\",\"speex\",\"g729\",\"pcma\",\"pcmu\",\"g722\",\"ilbc\"]},\"candidate\":[{\"generation\":0,\"ip\":\"192.168.1.87\",\"name\":\"audio_rtp\",\"network_name\":\"0\",\"password\":\"hSPwMI0Ku/cwJJc4\",\"port\":\"62642\",\"priority\":1.0,\"proto\":\"udp\",\"type\":\"local\",\"username\":\"NYFQH29ShDMClbMR\"}],\"fast_connect\":2,\"jb_flags\":3,\"peerList\":[],\"timeoutSec\":60,\"useragent\":{\"caps\":3847,\"state\":7,\"ua_ver\":\"VKM_PP_build_10\",\"voip_ver\":\"voip win32 release version:0.0.0.0 date:Dec 15 2018 01:44:05\"},\"video\":{\"cap\":{\"cmpl\":-6,\"fps\":24,\"height\":720,\"width\":1280},\"codecs\":[\"h264\",\"vp8\"]},\"zrtp-hash\":\"1.10 0b2276ae6771261f6f4cb88a4538ac75fa5e6338098eaf6dccc3370146726bd5\"}\n",
  "type": "invite",
  "user_id": 375988312,
  "video": false,
  "msg_hash": "5c873d1f8a557",
  "first_name": "Тест",
  "last_name": "Тестович",
  "sex": 2,
  "photo_max": "https://vk.com/images/camera_200.png?ava=1",
  "photo_max_orig": "https://vk.com/images/camera_200.png?ava=1",
  "crop_rect": false,
  "verified": false
}}
                                     */
                                    break;
                                }
                            default:
                                {
                                    int debugme = 0;
                                    break;
                                }
                        }
                    }
                }
            }
            return serverUpdateDataList1;
        }

        private UpdatesResponse.LongPollServerUpdateData.flag FlagFromMask(int mask)
        {
            var flags = new UpdatesResponse.LongPollServerUpdateData.flag();
            flags.UNREAD = (mask & 1) != 0;
            flags.OUTBOX = (mask & 2) != 0;
            flags.REPLIED = (mask & 4) != 0;
            flags.IMPORTANT = (mask & 8) != 0;
            flags.CHAT = (mask & 16) != 0;
            flags.FRIENDS = (mask & 32) != 0;
            flags.SPAM = (mask & 64) != 0;
            flags.DELЕTЕD = (mask & 128) != 0;
            flags.FIXED = (mask & 256) != 0;
            flags.MEDIA = (mask & 512) != 0;
            flags.HIDDEN = (mask & 65536) != 0;
            flags.DELЕTЕD_FOR_ALL = (mask & 131072) != 0;
            return flags;
        }

        //{"ts":1754854172,"updates":[[7,2000000017,52533,0],[4,54847,532481,2000000017,1525036141,"",{"source_act":"chat_kick_user","source_mid":"375988312","from":"375988312"},{}],[80,1,0],[51,17],[52,7,2000000017,375988312]]}
        //Из беседы         {"ts":1781428068,"updates":[[7,2000000017,54895,0],[4,54896,532497  ,2000000017 ,1525071240 ,"7",{"from":"375988312"},{}]]}
        //Просто сообщение  {"ts":1781428085,"updates":[[7,375988312,54889,0] ,[4,54898,17      ,375988312  ,1525071538 ,"2",{"title":" ... "},{}],[80,3,0]]}
        //Стикер            {"ts":1781428139,"updates":[[7,375988312,54898,0] ,[4,54903,529     ,375988312  ,1525071852 ,"" ,{"title":" ... "},{"attach1_product_id":"3","attach1_type":"sticker","attach1":"101"}]]}
        //Вернулся в беседу 4,55138,532481,2000000017,1525078438,"",{"source_act":"chat_invite_user","source_mid":"375988312","from":"375988312"},{}
        private UpdatesResponse.LongPollServerUpdateData GetUpdateDataForNewMessageLongPollData(List<object> updateDataRaw)
        {
            //Logger.Instance.Info("GetUpdateDataForNewMessageLongPollData 1");
            int type = int.Parse(updateDataRaw[0].ToString());

            UpdatesResponse.LongPollServerUpdateData serverUpdateData = new UpdatesResponse.LongPollServerUpdateData();

            if (type == 5)
                serverUpdateData.UpdateType = LongPollServerUpdateType.MessageUpdate;
            else if (type == 4)
                serverUpdateData.UpdateType = LongPollServerUpdateType.MessageAdd;
            else
                serverUpdateData.UpdateType = LongPollServerUpdateType.ReplacePeerFlag;

            uint message_id = uint.Parse(updateDataRaw[1].ToString());
            int mask = int.Parse(updateDataRaw[2].ToString());//(int.Parse(updateDataRaw[2].ToString()) & 2) == 2 ? VKMessageType.Sent : VKMessageType.Received;
            //
            serverUpdateData.flags = this.FlagFromMask(mask);

            //
            int peer_id = int.Parse(updateDataRaw[3].ToString());
            int timestamp = int.Parse(updateDataRaw[4].ToString());
            string text = updateDataRaw[5].ToString();
            string extended = updateDataRaw[6].ToString();

            //random_id, если параметр был передан в messages.send
            //int random_id = int.Parse(updateDataRaw[7].ToString());

            //bool flag2 = false;
            //bool flag3 = false;
            //long num4 = 0;
            //Logger.Instance.Info("GetUpdateDataForNewMessageLongPollData 2   '{0}'", updateDataRaw.Count);
            /*
             * $peer_id (integer) — идентификатор назначения.
             * Для пользователя: id пользователя.
             * Для групповой беседы: 2000000000 + id беседы.
             * Для сообщества: -id сообщества либо id сообщества + 1000000000 (для version = 0). 
             * */
            if (updateDataRaw.Count > 7)
            {
                //{ "attach1_product_id": "3", "attach1_type": "sticker", "attach1": "112" }
                //{ "fwd": "375988312_1366,375988312_1368" }
                //{ "attach1_type":"doc", "attach1":"375988312_454648115" }

                //photo, video, audio, doc, wall, sticker, link, money

                UpdatesResponse.LongPollServerUpdateData.attach[] attachments = new UpdatesResponse.LongPollServerUpdateData.attach[50];

                foreach (KeyValuePair<string, JToken> keyValuePair in updateDataRaw[7] as JObject)
                {
                    if (keyValuePair.Key == "fwd")
                    {
                        serverUpdateData.fwds = ParseFwds(keyValuePair.Value.ToString());
                    }
                    else if (keyValuePair.Key.StartsWith("attach"))
                    {
                        string value = keyValuePair.Value.ToString();
                        if (string.IsNullOrEmpty(value))
                            continue;

                        //Regex _friendsReg = new Regex("attach(?<number>\\d+)(_(?<field>\\S+))?");
                        Regex _friendsReg = new Regex(@"attach(?<id>\d+)_*(?<field>\S+)?");
                        Match m = _friendsReg.Match(keyValuePair.Key);

                        if (!m.Success)
                            continue;

                        int id = int.Parse(m.Groups["id"].Value);
                        string field = m.Groups["field"].Value;

                        if (attachments[id] == null)
                            attachments[id] = new UpdatesResponse.LongPollServerUpdateData.attach();

                        attachments[id].id = id;

                        switch (field)
                        {
                            case "product_id":
                                {
                                    attachments[id].product_id = int.Parse(value);
                                    break;
                                }
                            case "type":
                                {
                                    attachments[id].type = value;
                                    break;
                                }
                            case "":
                                {
                                    int temp_id;
                                    if (int.TryParse(value, out temp_id))
                                        attachments[id].item_id = temp_id;
                                    else
                                    {
                                        string[] split = value.Split('_');

                                        attachments[id].owner_id = int.Parse(split[0]);
                                        //attachments[id].item_id = int.Parse(split[1]);
                                        int item_id;
                                        if(int.TryParse(split[1], out item_id))
                                        {
                                            attachments[id].item_id = item_id;
                                        }
                                    }
                                    break;
                                }
                        }
                        
                    }

                }

                foreach (UpdatesResponse.LongPollServerUpdateData.attach a in attachments)
                {
                    if (a == null)
                        continue;

                    if (serverUpdateData.attachments == null)
                        serverUpdateData.attachments = new List<UpdatesResponse.LongPollServerUpdateData.attach>();

                    serverUpdateData.attachments.Add(a);
                }
            }

            serverUpdateData.message_id = message_id;
            serverUpdateData.peer_id = peer_id;

            if (peer_id > 2000000000)
            {
                foreach (KeyValuePair<string, JToken> keyValuePair in updateDataRaw[6] as JObject)
                {
                    if (keyValuePair.Key == "source_mid")
                    {
                        serverUpdateData.source_mid = int.Parse(keyValuePair.Value.ToString());
                    }
                    else if (keyValuePair.Key == "from_admin")
                    {
                        serverUpdateData.from_admin = int.Parse(keyValuePair.Value.ToString());
                    }
                    else if (keyValuePair.Key == "source_act")
                    {
                        string value = keyValuePair.Value.ToString();
                        VKChatMessageActionType temp_act = VKChatMessageActionType.None;
                        switch (value)
                        {
                            case "chat_photo_update": temp_act = VKChatMessageActionType.ChatPhotoUpdate; break;
                            case "chat_photo_remove": temp_act = VKChatMessageActionType.ChatPhotoRemove; break;
                            case "chat_create": temp_act = VKChatMessageActionType.ChatCreate; break;
                            case "chat_title_update": temp_act = VKChatMessageActionType.ChatTitleUpdate; break;
                            case "chat_invite_user": temp_act = VKChatMessageActionType.ChatInviteUser; break;
                            case "chat_kick_user": temp_act = VKChatMessageActionType.ChatKickUser; break;

                            case "chat_pin_message": temp_act = VKChatMessageActionType.ChatPinMessage; break;
                            case "chat_unpin_message": temp_act = VKChatMessageActionType.ChatUnpinMessage; break;
                            case "chat_invite_user_by_link": temp_act = VKChatMessageActionType.ChatInviteUserByLink; break;
                                //default: return VKChatMessageActionType.None;
                        }

                        serverUpdateData.source_act = temp_act;
                    }
                    else if (keyValuePair.Key == "from")
                    {
                        //В беседе
                        serverUpdateData.user_id = int.Parse(keyValuePair.Value.ToString());
                    }
                    else if (keyValuePair.Key == "source_text")
                    {
                        serverUpdateData.source_text = keyValuePair.Value.ToString();
                    }
                    else if (keyValuePair.Key == "source_old_text")
                    {
                        serverUpdateData.source_old_text = keyValuePair.Value.ToString();
                    }
                }
            }
            else
            {
                serverUpdateData.user_id = peer_id;
            }

            if (!string.IsNullOrEmpty(extended))
            {
                //{"emoji":"1","title":" ... ","keyboard":{"one_time":false,"buttons":[[{"action":{"type":"text","payload":"{}","label":"Правда"},"color":"positive"}],[{"action":{"type":"text","payload":"{}","label":"Миф"},"color":"negative"}]]}}
                serverUpdateData.extended = extended;
            }
            //Logger.Instance.Info("GetUpdateDataForNewMessageLongPollData 3 '{0}'", text);
            serverUpdateData.timestamp = timestamp;

            //serverUpdateData.text = System.Net.WebUtility.HtmlDecode(text);
            //serverUpdateData.text = Windows.Data.Html.HtmlUtilities.ConvertToText(text);//на 10.0.10586 вылеты
            serverUpdateData.text = text.ForUI();//text.Replace("<br>", Environment.NewLine);

            //Logger.Instance.Info("GetUpdateDataForNewMessageLongPollData finish");
            return serverUpdateData;
        }

        private List<UpdatesResponse.LongPollServerUpdateData.fwd> ParseFwds(string input)
        {
            List<UpdatesResponse.LongPollServerUpdateData.fwd> o = new List<UpdatesResponse.LongPollServerUpdateData.fwd>();
            Regex _friendsReg = new Regex("((?<user_id>\\d+)_(?<msg_id>\\d+))");
            MatchCollection mc = _friendsReg.Matches(input);
            foreach (Match m in mc)
            {
                int user_id = int.Parse(m.Groups["user_id"].Value);
                int msg_id = int.Parse(m.Groups["msg_id"].Value);
                UpdatesResponse.LongPollServerUpdateData.fwd i = new UpdatesResponse.LongPollServerUpdateData.fwd(user_id, msg_id);
                o.Add(i);
            }
            return o;
        }

        public void SwitchGroup(uint id)
        {
            this.group_id = id;
            this.Restart();
        }
        /*
        private UpdatesResponse.LongPollServerUpdateData ReadUserOrChatIds(List<object> updateDataRaw)
        {
            if (updateDataRaw == null || updateDataRaw.Count < 4)
                return null;
            uint num1 = uint.Parse(updateDataRaw[1].ToString());
            int user_id = 0;
            uint chat_id = 0;
            if (updateDataRaw.Count >= 4)
            {
                int num4 = int.Parse(updateDataRaw[3].ToString());
                if (num4 - 2000000000 >= 0)
                {
                    //flag = true;
                    chat_id = num4 - 2000000000L;
                }
                else
                {
                    //flag = false;
                    user_id = num4;
                }
            }
            return new UpdatesResponse.LongPollServerUpdateData()
            {
                user_id = user_id,
                chat_id = chat_id,
                message_id = num1
            };
        }
        */
        public class RootObjectGetUpdates
        {
            public int ts { get; set; }
            public List<List<object>> updates { get; set; }
        }

        public class PushSettings
        {
            public int peer_id { get; set; }
            /// <summary>
            /// указывает, до какого времени оповещения для чата отключены
            /// -1 — отключены навсегда (бессрочно). 
            /// </summary>
            public int disabled_until { get; set; }

            /// <summary>
            /// указывает, включен ли звук оповещений
            /// </summary>
            [JsonConverter(typeof(Core.Json.VKBooleanConverter))]
            public bool sound { get; set; }
        }

        
    }
}


/*
case 2:
                                        processAddFlags(ev.getInt(1), ev.getInt(3), ev.getInt(2));
                                        break;
                                    case 3:
                                        processClearFlags(ev.getInt(1), ev.getInt(3), ev.getInt(2));
                                        break;
                                    case 4:
                                        processMessageAdd(ev.getInt(1), ev.getInt(3), ev.getInt(2), ev.getInt(4), ev.getString(6), ev.getString(5), ev.optJSONObject(7), ev.getInt(8));
                                        break;
                                    case 5:
                                        processMessageUpdate(ev.getInt(1), ev.getInt(3), ev.getInt(2), ev.getInt(4), ev.getString(6), ev.getString(5), ev.optJSONObject(7), ev.getInt(8));
                                        break;
                                    case 6:
                                        processReadUpto(ev.getInt(1), ev.getInt(2), true);
                                        break;
                                    case 7:
                                        processReadUpto(ev.getInt(1), ev.getInt(2), false);
                                        break;
                                    case 8:
                                        int onl = 1;
                                        int lpo = ev.getInt(2) & 255;
                                        if (lpo == 1) {
                                            onl = 2;
                                        }
                                        if (lpo == 4 || lpo == 2 || lpo == 3 || lpo == 5) {
                                            onl = 3;
                                        }
                                        processOnlineChange(-ev.getInt(1), onl);
                                        break;
                                    case 9:
                                        processOnlineChange(-ev.getInt(1), 0);
                                        break;
                                    case 61:
                                        processTyping(ev.getInt(1), ev.getInt(1));
                                        break;
                                    case 62:
                                        processTyping(2000000000 + ev.getInt(2), ev.getInt(1));
                                        break;
                                    case 80:
                                        break;
                                    case 114:
                                        processNotifySettings(ev.getJSONObject(1));
                                        break;
                                    default:
                                        Log.w("vk_longpoll", "Unknown event " + ev.toString());
                                        break;
 * 
 *     private static final int EVENT_CHAT_CHANGED = 51;
    private static final int EVENT_CHAT_TYPING = 62;
    private static final int EVENT_FRIEND_OFFLINE = 9;
    private static final int EVENT_FRIEND_ONLINE = 8;
    private static final int EVENT_MSG_ADD = 4;
    private static final int EVENT_MSG_ADD_EXTENDED = 101;
    private static final int EVENT_MSG_DELETE = 0;
    private static final int EVENT_MSG_FLAG_ADD = 2;
    private static final int EVENT_MSG_FLAG_CLEAR = 3;
    private static final int EVENT_MSG_FLAG_REPLACE = 1;
    private static final int EVENT_MSG_IN_READ_UPTO = 6;
    private static final int EVENT_MSG_OUT_READ_UPTO = 7;
    private static final int EVENT_MSG_UPDATE = 5;
    private static final int EVENT_NOTIFY_SETTINGS = 114;
    private static final int EVENT_UPDATE_COUNTER = 80;
    private static final int EVENT_USER_TYPING = 61;
    private static final int EVENT_VOIP_HANGUP = 112;
    private static final int EVENT_VOIP_INCOMING = 110;
    private static final int EVENT_VOIP_REPLIED = 111;
 * 
 * public static final int MSG_CHAT = 16;
    public static final int MSG_DELETED = 128;
    public static final int MSG_FIXED = 256;
    public static final int MSG_FRIENDS = 32;
    public static final int MSG_IMPORTANT = 8;
    public static final int MSG_MEDIA = 512;
    public static final int MSG_OUTBOX = 2;
    public static final int MSG_REPLIED = 4;
    public static final int MSG_SPAM = 64;
    public static final int MSG_UNREAD = 1;
    public static final int NOTIFY_ID_MESSAGE = 10;
    public static final int ONLINE_TYPE_ANDROID = 4;
    public static final int ONLINE_TYPE_DEFAULT = 7;
    public static final int ONLINE_TYPE_IPAD = 3;
    public static final int ONLINE_TYPE_IPHONE = 2;
    public static final int ONLINE_TYPE_MOBILE = 1;
    public static final int ONLINE_TYPE_WINDOWS8 = 6;
    public static final int ONLINE_TYPE_WINPHONE = 5;
 * */
