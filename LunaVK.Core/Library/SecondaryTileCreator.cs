using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace LunaVK.Core.Library
{
    public class SecondaryTileCreator
    {
        public static void CreateTileFor(int userOrGroupId, string name, string smallPhoto, string action, Action<bool> completionCallback )
        {
            string str = string.IsNullOrWhiteSpace(name) ? "" : System.Net.WebUtility.UrlEncode(name);
            Uri uri = new Uri(string.Format("/VKClient.Common;component/Profiles/Shared/Views/ProfilePage.xaml?UserOrGroupId={0}&Name={1}&ClearBackStack=True", userOrGroupId, str), UriKind.Relative);
            /*
            SecondaryTileManager.Instance.GetSecondaryTileData(userOrGroupId, isGroup, name, 3, ((res, resData) =>
            {
                if (res)
                    ShellTile.Create(uri, (ShellTileData)resData, true);
                completionCallback(res);
            }), smallPhoto);*/
            SecondaryTileManager.Instance.AddContent(name, action, userOrGroupId > 0 ? ("id"+ userOrGroupId) : ("club"+(-userOrGroupId)), smallPhoto);
        }

        public static void CreateTileForConversation(int peerId, string name, List<string> imagesUris, Action<bool> completionCallback)
        {
            //Action<string> action=null;
            //Execute.ExecuteOnUIThread((Action)(() =>
            //{
                string tileType = "Conversation";
                string objId = peerId.ToString() + Settings.UserId.ToString();
                imagesUris = imagesUris.Take(8).ToList();
                List<WaitHandle> waitHandles = SecondaryTileManager.DownloadImages(imagesUris, tileType, objId);
                //new Thread((ThreadStart)(() =>
                //{
                    if (!WaitHandle.WaitAll(waitHandles.ToArray(), 10000))
                    {
                        completionCallback(false);
                    }
                    else
                    {
                        List<string> localUris = new List<string>();
                        for (int ind = 0; ind < imagesUris.Count; ++ind)
                        {
                            if (!string.IsNullOrWhiteSpace(imagesUris[ind]))
                                localUris.Add("/" + SecondaryTileManager.GetLocalNameFor(tileType, objId, ".jpg"));
                        }
                        ConversationTileImageFormatter.CreateTileImage(localUris, peerId, (uriStr =>
                        {
                            if (!string.IsNullOrEmpty(uriStr))
                            {
                                //FlipTileData flipTileData = new FlipTileData();
                                Uri uri = new Uri("isostore:" + uriStr, UriKind.Absolute);
                                //flipTileData.SmallBackgroundImage = uri;
                                //flipTileData.BackgroundImage = uri;
                                //flipTileData.Title = name;
                                //ShellTile.Create(new Uri(string.Format("/VKMessenger;component/Views/ConversationPage.xaml?UserOrChatId={0}&IsChat={1}&FromLookup={2}&NewMessageContents={3}&TileLoggedInUserId={4}&ClearBackStack=True", (object)userOrChatId, (object)isChat, (object)false, (object)false, (object)AppGlobalStateManager.Current.LoggedInUserId), UriKind.Relative), (ShellTileData)flipTileData, false);
                                completionCallback(true);
                            }
                            else
                                completionCallback(false);
                        }));
                    }
                //})).Start();
            //}));
        }

        

        
    }
}
