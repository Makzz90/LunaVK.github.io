using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace LunaVK.Core
{
    public class SecondaryTileManager
    {
        private static SecondaryTileManager _instance;
        public static SecondaryTileManager Instance
        {
            get
            {
                if (SecondaryTileManager._instance == null)
                    SecondaryTileManager._instance = new SecondaryTileManager();
                return SecondaryTileManager._instance;
            }
        }
        
        public void SendTile(string tag = "")
        {
            string from = "Test testovich";
            string body = "Check out these awesome photos I took while in New Zealand!";
            string image = "https://sun9-2.userapi.com/c850224/v850224275/e3ad8/DGpG9014UJE.jpg?ava=1";
            /*
            TileContent content0 = new TileContent()
            {
                
                Visual = new TileVisual()
                {
                    TileMedium = new TileBinding()
                    {
                        //DisplayName = 'VK',
                        Content = new TileBindingContentAdaptive()
                        {
                            Children = {
                                new AdaptiveText() { Text = tag+from },
                                new AdaptiveText() { Text = body, HintStyle = AdaptiveTextStyle.CaptionSubtle, HintMaxLines = 3, HintWrap = true }
                            },
                            PeekImage = new TilePeekImage()
                            {
                                Source = 'https://sun9-2.userapi.com/c850224/v850224275/e3ad8/DGpG9014UJE.jpg?ava=1',
                                HintCrop = TilePeekImageCrop.Circle
                            }

                        }
                    },

                    TileWide = new TileBinding()
                    {
                        //DisplayName = 'VK2',
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new AdaptiveGroup()
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 30,
                                            Children =
                                            {
                                                new AdaptiveImage()
                                                {
                                                    HintCrop = AdaptiveImageCrop.Circle,
                                                    Source = 'https://sun9-2.userapi.com/c850224/v850224275/e3ad8/DGpG9014UJE.jpg?ava=1'
                                                }
                                            }
                                        },

                                        new AdaptiveSubgroup()
                                        {
                                            Children = {
                                                new AdaptiveText() { Text = from, HintStyle = AdaptiveTextStyle.Subtitle },
                                                new AdaptiveText() { Text = body, HintStyle = AdaptiveTextStyle.CaptionSubtle, HintMaxLines = 2, HintWrap = true }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },





                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            TextStacking = TileTextStacking.Center,
                            Children =
                            {
                                new AdaptiveGroup()
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup() { HintWeight = 1 },
                                        new AdaptiveSubgroup() { HintWeight = 2,
                                            Children =
                                            {
                                                new AdaptiveImage()
                                                {
                                                    HintCrop = AdaptiveImageCrop.Circle,
                                                    Source = 'https://sun9-2.userapi.com/c850224/v850224275/e3ad8/DGpG9014UJE.jpg?ava=1'
                                                }
                                            }
                                        },
                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 1
                                        }
                                    }
                                },
                                new AdaptiveText()
                                {
                                    Text = from,
                                    HintStyle = AdaptiveTextStyle.Title,
                                    HintAlign = AdaptiveTextAlign.Center
                                },
                                new AdaptiveText()
                                {
                                    Text = body,
                                    HintStyle = AdaptiveTextStyle.SubtitleSubtle,
                                    HintAlign = AdaptiveTextAlign.Center
                                }
                            }
                        }
                    }
                }
            };
            */
            string xml = @"<tile>
                          <visual>
                            <binding template='TileMedium' hint-textStacking='center'>
                                <group>
                                    <subgroup hint-weight='1'/>
                                    <subgroup hint-weight='1'>
                                        <image src='" + image + @"' hint-crop='circle' hint-removeMargin='true'/>
                                    </subgroup>
                                    <subgroup hint-weight='1'/>
                                    <text hint-style='captionSubtle' hint-align='center'>" + from + @"</text>
                                </group>
                            </binding>

                            <binding template='TileWide'>
                                <group>
                                    <subgroup hint-weight='30'>
                                        <image src='" + image + @"' hint-crop='circle'/>
                                    </subgroup>
                                    <subgroup>
                                        <text hint-style='bodySubtle'>" + from + @"</text>
                                    </subgroup>
                                </group>
                            </binding>

                            <binding template='TileLarge' hint-textStacking='center'>
                                <group>
                                    <subgroup hint-weight='1'/>
                                    <subgroup hint-weight='2'>
                                        <image src='" + image + @"' hint-crop='circle'/>
                                    </subgroup>
                                    <subgroup hint-weight='1'/>
                                </group>
                                <text hint-style='captionSubtle' hint-align='center'>" + from + @"</text>
                            </binding>
                          </visual>
                        </tile>";
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            var notification = new TileNotification(xmlDocument);
            if (!string.IsNullOrEmpty(tag))
                notification.Tag = tag;
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
        }
        
        public void UpdateAllExistingTiles(Action<bool> completionCallback)
        {/*
            //Logger.Instance.Info("Entering SecondaryTileManager.UpdateAllExistingTiles");
            this.ProcessTile(0, ((IEnumerable<ShellTile>)ShellTile.ActiveTiles.Where<ShellTile>((Func<ShellTile, bool>)(t =>
            {
                if (t.NavigationUri != null)
                    return t.NavigationUri.OriginalString.Contains("/VKClient.Common;component/Profiles/Shared/Views/ProfilePage.xaml?UserOrGroupId=");
                return false;
            })).OrderBy<ShellTile, int>(new Func<ShellTile, int>(this.GetLastUpdatedDate))).ToList<ShellTile>(), completionCallback);*/




            completionCallback(true);
        }
        /*
        private void ProcessTile(int ind, List<ShellTile> list, Action<bool> completionCallback)
        {
            if (ind >= list.Count)
            {
                completionCallback(true);
            }
            else
            {
                ShellTile tile = list[ind];
                Dictionary<string, string> queryString = tile.NavigationUri.ParseQueryString();
                bool isGroup = queryString.ContainsKey("GroupId");
                long userOrGroupId = isGroup ? long.Parse(queryString["GroupId"]) : long.Parse(queryString["UserOrGroupId"]);
                string name = queryString["Name"];
                this.GetSecondaryTileData(userOrGroupId, isGroup, name, 5, (Action<bool, CycleTileData>)((res, resData) =>
                {
                    if (res)
                    {
                        Logger.Instance.Info("Updating secondary tile with new info " + tile.NavigationUri);
                        try
                        {
                            tile.Update((ShellTileData)resData);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else
                        Logger.Instance.Error("Failed to process tile update " + tile.NavigationUri);
                    this.ProcessTile(ind + 1, list, completionCallback);
                }), null);
            }
        }*/


        public static List<WaitHandle> DownloadImages(List<string> Uris, string tileType, string objId)
        {
            List<WaitHandle> waitHandleList = new List<WaitHandle>();
            foreach (var uri in Uris)
            {
                string str = uri;
                EventWaitHandle threadFinish = new EventWaitHandle(false, EventResetMode.ManualReset);
                waitHandleList.Add(threadFinish);

                Stream cachedImageStream = null;// ImageCache.Current.GetCachedImageStream(str);
                string localName = SecondaryTileManager.GetLocalNameFor(tileType, objId, ".jpg");
                if (cachedImageStream != null)
                {
                    try
                    {
                        using (IsolatedStorageFileStream storageFileStream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile(localName, FileMode.Create, FileAccess.ReadWrite))
                        {
                            using (cachedImageStream)
                            {
                                byte[] buffer = new byte[1024];
                                while (cachedImageStream.Read(buffer, 0, buffer.Length) > 0)
                                    storageFileStream.Write(buffer, 0, buffer.Length);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //Logger.Instance.Error("SecondaryTileManager.DownloadImages failed", ex);
                    }
                    finally
                    {
                        threadFinish.Set();
                    }
                }
                else
                {
                    try
                    {
                        HttpWebRequest request = WebRequest.CreateHttp(str);
                        request.BeginGetResponse((ir =>
                        {
                            try
                            {
                                WebResponse response = request.EndGetResponse(ir);
                                /*
                                using (IsolatedStorageFileStream storageFileStream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile(localName, FileMode.Create, FileAccess.ReadWrite))
                                {
                                    using (Stream responseStream = response.GetResponseStream())
                                    {
                                        byte[] buffer = new byte[1024];
                                        while (responseStream.Read(buffer, 0, buffer.Length) > 0)
                                            storageFileStream.Write(buffer, 0, buffer.Length);
                                    }
                                }
                                */
                                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    storeForApplication.CreateDirectory("ShellContent");
                                    using (IsolatedStorageFileStream storageFileStream = storeForApplication.CreateFile(localName))
                                    {
                                        //BinaryWriter writer = new BinaryWriter(storageFileStream);
                                        //obj.Write(writer);
                                        using (Stream responseStream = response.GetResponseStream())
                                        {
                                            byte[] buffer = new byte[1024];
                                            while (responseStream.Read(buffer, 0, buffer.Length) > 0)
                                                storageFileStream.Write(buffer, 0, buffer.Length);
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }
                            finally
                            {
                                threadFinish.Set();
                            }
                        }), null);
                    }
                    catch (Exception)
                    {
                        threadFinish.Set();
                    }
                }
            }
            return waitHandleList;
        }

        /// <summary>
        /// "ShellContent/" + tileType + "_" + objId + extension"
        /// </summary>
        /// <param name="tileType"></param>
        /// <param name="objId"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetLocalNameFor(string tileType, string objId, string extension = "")
        {
            return "ShellContent\\" + tileType + "_" + objId + extension;
        }

        public async void AddContent(string title, string action, string tag, string image)
        {
            /*
            TileContent content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            TextStacking = TileTextStacking.Center,
                            Children =
                            {
                                new AdaptiveGroup()
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup() { HintWeight = 1 },
                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 1,
                                            Children =
                                            {
                                                new AdaptiveImage()
                                                {
                                                    HintCrop = AdaptiveImageCrop.Circle,
                                                    HintRemoveMargin = true,
                                                    Source = image
                                                }
                                            }
                                        },
                                        new AdaptiveSubgroup() { HintWeight = 1 },
                                    }
                                },
                                new AdaptiveText()
                                {
                                    Text = title,
                                    HintStyle = AdaptiveTextStyle.Caption,
                                    HintAlign = AdaptiveTextAlign.Center
                                },
                            }
                        }
                    },

                    TileWide = new TileBinding()
                    {
                        //DisplayName = "VK2",
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new AdaptiveGroup()
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 30,
                                            Children =
                                            {
                                                new AdaptiveImage()
                                                {
                                                    HintCrop = AdaptiveImageCrop.Circle,
                                                    Source = image
                                                }
                                            }
                                        },

                                        new AdaptiveSubgroup()
                                        {
                                            Children = {
                                                new AdaptiveText() { Text = title, HintStyle = AdaptiveTextStyle.Subtitle },
                                                //new AdaptiveText() { Text = subtitle, HintStyle = AdaptiveTextStyle.CaptionSubtle, HintMaxLines = 2, HintWrap = true }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },


                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            TextStacking = TileTextStacking.Center,
                            Children =
                            {
                                new AdaptiveGroup()
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup() { HintWeight = 1 },
                                        new AdaptiveSubgroup() { HintWeight = 2,
                                            Children =
                                            {
                                                new AdaptiveImage() { HintCrop = AdaptiveImageCrop.Circle, Source = image }
                                            }
                                        },
                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 1
                                        }
                                    }
                                },
                                new AdaptiveText() { Text = title, HintStyle = AdaptiveTextStyle.Title, HintAlign = AdaptiveTextAlign.Center },
                                //new AdaptiveText() { Text = subtitle, HintStyle = AdaptiveTextStyle.SubtitleSubtle, HintAlign = AdaptiveTextAlign.Center }
                            }
                        }
                    }
                }
            };
            */

            string xml = @"<tile>
                          <visual>
                            <binding template='TileMedium' hint-textStacking='center'>
                                <group>
                                    <subgroup hint-weight='1'/>
                                    <subgroup hint-weight='1'>
                                        <image src='"+ image+ @"' hint-crop='circle' hint-removeMargin='true'/>
                                    </subgroup>
                                    <subgroup hint-weight='1'/>
                                    <text hint-style='captionSubtle' hint-align='center'>" + title + @"</text>
                                </group>
                            </binding>

                            <binding template='TileWide'>
                                <group>
                                    <subgroup hint-weight='30'>
                                        <image src='" + image + @"' hint-crop='circle'/>
                                    </subgroup>
                                    <subgroup>
                                        <text hint-style='bodySubtle'>" + title + @"</text>
                                    </subgroup>
                                </group>
                            </binding>

                            <binding template='TileLarge' hint-textStacking='center'>
                                <group>
                                    <subgroup hint-weight='1'/>
                                    <subgroup hint-weight='2'>
                                        <image src='" + image + @"' hint-crop='circle'/>
                                    </subgroup>
                                    <subgroup hint-weight='1'/>
                                </group>
                                <text hint-style='captionSubtle' hint-align='center'>" + title + @"</text>
                            </binding>
                          </visual>
                        </tile>";

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            var notification = new TileNotification(xmlDocument);
            notification.Tag = tag;


            List<string> imagesUris = new List<string>() { image };
            List<WaitHandle> waitHandles = SecondaryTileManager.DownloadImages(imagesUris, "Avatar", tag);
            if (!WaitHandle.WaitAll(waitHandles.ToArray(), 10000))
            {
                //completionCallback(false);
                int i = 0;
            }
            else
            {
                string localUri = SecondaryTileManager.GetLocalNameFor("Avatar", tag, ".jpg");

                


                // Construct a unique tile ID, which you will need to use later for updating the tile
                string tileId = tag;

                // Use a display name you like
                string displayName = title;

                // Provide all the required info in arguments so that when user
                // clicks your tile, you can navigate them to the correct content
                string arguments = action;
                //image = "ms-appx:///Assets/Img/Square71x71Logo.png";
                //Uri img = new Uri(image);
                string temp2 = "ms-appdata:///local/" + localUri;
                Uri img = new Uri(temp2);
                // Initialize the tile with required arguments
                SecondaryTile tile = new SecondaryTile(
                    tileId,
                    displayName,
                    arguments,
                    img,
                    TileSize.Default);

                if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent(("Windows.Phone.UI.Input.HardwareButtons"))))
                {
                    //secondaryTile.VisualElements.Wide310x150Logo = wide310x150Logo;
                    //secondaryTile.VisualElements.ShowNameOnWide310x150Logo = true;
                    //secondaryTile.VisualElements.Square310x310Logo = square310x310Logo;
                    //secondaryTile.VisualElements.ShowNameOnSquare310x310Logo = true;
                }

                // Pin the tile
                bool isPinned = await tile.RequestCreateAsync();

                // Check if the secondary tile is pinned
                bool isExists = SecondaryTile.Exists(tileId);
                // TODO: Update UI to reflect whether user can now either unpin or pin
            }











            /*
             * // Initialize a secondary tile with the same tile ID you want removed
            SecondaryTile toBeDeleted = new SecondaryTile(tileId);

            // And then unpin the tile
            await toBeDeleted.RequestDeleteAsync();
            */
        }
}
}
