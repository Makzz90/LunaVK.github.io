using LunaVK.Core.Framework;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace LunaVK.Core
{
    //https://docs.microsoft.com/ru-ru/windows/uwp/design/shell/tiles-and-notifications/create-adaptive-tiles
    //https://docs.microsoft.com/en-us/previous-versions/windows/apps/hh761491(v=win.10)?redirectedfrom=MSDN
    //https://docs.microsoft.com/ru-ru/windows/uwp/design/shell/tiles-and-notifications/tile-schema#adaptivetextstyle
    public class PrimaryTileManager
    {
        private static PrimaryTileManager _instance;
        public static PrimaryTileManager Instance
        {
            get
            {
                if (PrimaryTileManager._instance == null)
                {
                    PrimaryTileManager._instance = new PrimaryTileManager();
                    //TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);//Позволяет плитке ставить в очередь до пяти уведомлений. Это включает очередь уведомлений на плитках всех размеров.
                }
                return PrimaryTileManager._instance;
            }
        }

        public void AddContent(string title, string subtitle, string tag, string image)
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
                                new AdaptiveText()
                                {
                                    Text = subtitle,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintAlign = AdaptiveTextAlign.Center
                                }
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
                                                new AdaptiveText() { Text = subtitle, HintStyle = AdaptiveTextStyle.CaptionSubtle, HintMaxLines = 2, HintWrap = true }
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
                                new AdaptiveText() { Text = subtitle, HintStyle = AdaptiveTextStyle.SubtitleSubtle, HintAlign = AdaptiveTextAlign.Center }
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
                                    <text hint-style='caption' hint-align='center'>" + title + @"</text>
                                    <text hint-style='captionSubtle' hint-align='center'>" + subtitle + @"</text>
                                </group>
                            </binding>

                            <binding template='TileWide'>
                                <group>
                                    <subgroup hint-weight='30'>
                                        <image src='" + image + @"' hint-crop='circle'/>
                                    </subgroup>
                                    <subgroup>
                                        <text hint-style='bodySubtle'>" + title + @"</text>
                                        <text hint-style='captionSubtle' hint-align='center' hint-maxLines='2' hint-wrap='true'>" + subtitle + @"</text>
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
                                <text hint-style='captionSubtle' hint-align='center' hint-wrap='true'>" + subtitle + @"</text>
                            </binding>
                          </visual>
                        </tile>";

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            var notification = new TileNotification(xmlDocument);
            notification.Tag = tag;
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
        }

        public void SetCounter(uint count)
        {
            var b = BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            var xml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            var atr = xml.GetElementsByTagName("badge");
            atr[0].Attributes.GetNamedItem("value").NodeValue = count == 0 ? "" : count.ToString();
            var n = new BadgeNotification(xml);
            b.Update(n);
        }

        public void ResetContent()
        {
            try
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            }
            catch(Exception ex)
            {
                int i = 0;
            }
        }
    }
}
