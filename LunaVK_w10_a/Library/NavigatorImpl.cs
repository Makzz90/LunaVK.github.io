using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using LunaVK.Framework;
using System.Text.RegularExpressions;
using LunaVK.Core.DataObjects;
using LunaVK.Core;
using LunaVK.Core.Enums;
using LunaVK.Core.Utils;
using System.Linq;
using LunaVK.ViewModels;
using LunaVK.Core.Library;
using LunaVK.Pages;
using System.Diagnostics;
using LunaVK.Core.Framework;
using LunaVK.UC;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml.Media;
using Windows.Foundation.Metadata;

namespace LunaVK.Library
{
    public class NavigatorImpl
    {
        private static NavigatorImpl _instance;
        public static NavigatorImpl Instance
        {
            get
            {
                if (NavigatorImpl._instance == null)
                {
                    NavigatorImpl._instance = new NavigatorImpl();
                    CustomFrame.Instance.Navigating += NavigatorImpl._instance.NavigationService_Navigating;
                }
                return NavigatorImpl._instance;
            }
        }

        void NavigationService_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Parameter == null && CustomFrame.Instance.Content != null)//страница без параметров
            {

                string temp = CustomFrame.Instance.Content.ToString();
                if (temp == e.SourcePageType.FullName)
                {
                    e.Cancel = true;
                    return;
                }
            }
            /*
            if (e.Parameter == null || CustomFrame.Instance.Content == null || !(CustomFrame.Instance.Content is PageBase))
                return;

            PagesParams page_param = (CustomFrame.Instance.Content as PageBase).NavigationParameter as PagesParams;//Dictionary<string,int> page_param = (NavigatorImpl.NavigationService.Content as PageBase).NavigationParameter as Dictionary<string,int>;
            PagesParams nav_param = e.Parameter as PagesParams;//Dictionary<string,int> nav_param = e.Parameter as Dictionary<string,int>;

            if (page_param == null || nav_param == null)
                return;

            bool equal = false;

            if (nav_param.chat_id > 0)
            {
                if (page_param.chat_id == nav_param.chat_id)
                    equal = true;
            }
            else
            {
                if (page_param.user_id == nav_param.user_id)
                    equal = true;
            }

            if (equal)
                e.Cancel = true;

            int i = 0;
            */
        }

        public void NavigateToAudio(int ownerId, string ownerName)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("OwnerId", ownerId);
            QueryString.Add("OwnerName", ownerName);
            this.Navigate(typeof(MusicPage), QueryString);
        }
        
        public void NavigateToConversations(uint? groupId = null)
        {
            /*
            if (CustomFrame.Instance.Content is DialogsConversationPage2 page)
            {
                page.BackAction();
                CustomFrame.Instance.OpenCloseMenu(false);
            }
            else
            {
                this.Navigate(typeof(DialogsConversationPage2));
            }
            */
            this.NavigateToConversation(0, groupId);
        }
        
        public void NavigateToConversation(int peerId, uint? groupId = null)
        {
            if(CustomFrame.Instance.Content is DialogsConversationPage2 page)
            {
                if(peerId==0)
                {
                    page.BackAction();
                }
                else
                {
                    page.SelectConversation(peerId);
                }
                
            }
            else
            {
                Dictionary<string, int> QueryString = new Dictionary<string, int>();
                if(peerId!=0)
                    QueryString["PeerId"] = peerId;
                if(groupId.HasValue)
                    QueryString["GroupId"] = (int)groupId.Value;
                this.Navigate(typeof(DialogsConversationPage2), QueryString);
            }
            
        }
        /*
        public void NavigateToConversation(int peerId, int messageId = 0, bool isContactSellerMode = false)
        {
            Dictionary<string, int> QueryString = new Dictionary<string, int>();


            QueryString["PeerId"] = peerId;
            //QueryString["FromLookup"] =
            QueryString["MessageId"] = messageId;
            //QueryString["IsContactProductSellerMode"] =




            this.Navigate(typeof(Pages.DialogsConversationPage2), QueryString);
        }
    */

        /// <summary>
        /// Навигация в сообщество или пользователя
        /// </summary>
        /// <param name="Id"></param>
        public void NavigateToProfilePage(int Id)
        {            
            if(Id<0)
                this.Navigate(typeof(LunaVK.Pages.Group.GroupPage), (uint)(-Id));
            else
                this.Navigate(typeof(ProfilePage), (uint)Id);
        }

        public void NavigateToFeedback()
        {
            this.Navigate(typeof(NotificationsPage));
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">ИД чата</param>
        public void NavigateToChatEditPage(int id)
        {
            //Dictionary<string, int> QueryString = new Dictionary<string, long>();
            //QueryString.Add("Id", Id);
            this.Navigate(typeof(ChatEditPage), id);
        }

        public void NavigateToFriends(int userId, string userName = null)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("Id", userId);
            if(userName!=null)
                QueryString.Add("UserName", userName);
            this.Navigate(typeof(FriendsPage), QueryString);
        }

        public void NavigateToGroups(int userId)
        {
            Dictionary<string, int> QueryString = new Dictionary<string, int>();
            QueryString.Add("Id", userId);
            this.Navigate(typeof(GroupsPage), QueryString);
        }

        /// <summary>
        /// На страницу с фото-альбомами
        /// </summary>
        /// <param name="userId"></param>
        public void NavigateToPhotoAlbums(int ownerId, string ownerName)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("Id", ownerId);
            QueryString.Add("OwnerName", ownerName);
            this.Navigate(typeof(PhotoAlbumPage), QueryString);
        }

        public void NavigateToPhotosOfAlbum(int ownerId, int albumId, string albumName)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("OwnerId", ownerId);
            QueryString.Add("AlbumId", albumId);
            QueryString.Add("AlbumName", albumName);
            this.Navigate(typeof(PhotosPage), QueryString);
        }

        public void NavigateToAllPhotos(int ownerId, string ownerName)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("Id", ownerId);
            QueryString.Add("OwnerName", ownerName);
            this.Navigate(typeof(AllPhotosPage), QueryString);
        }

        public void NavigateToVideoCatalog()
        {
            this.Navigate(typeof(VideoCatalogPage));
        }

        /// <summary>
        /// Переход к списку видео у группы/пользователя
        /// </summary>
        /// <param name="userOrGroupId"></param>
        public void NavigateToVideos(int ownerId, string ownerName = "")
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("OwnerId", ownerId);
            QueryString.Add("OwnerName", ownerName);
            //this.Navigate(typeof(Pages.Group.GroupVideosPage),QueryString);
            this.Navigate(typeof(Pages.VideoAlbumsListPage), QueryString);
        }
        /*
        public void NavigateToVideoAlbumsList(int ownerId,string ownerName = "")
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("OwnerId", ownerId);
            QueryString.Add("OwnerName", ownerName);
            this.Navigate(typeof(Pages.VideoAlbumsListPage), QueryString);
        }
        */
        public void NavigateToVideoAlbum(int albumId, string albumName, int ownerId = 0)
        {
            //string navStr = string.Format("/VKClient.Video;component/VideoPage.xaml?PickMode={0}&UserOrGroupId={1}&IsGroup={2}&AlbumId={3}&AlbumName={4}", pickMode.ToString(), userOrGroupId, isGroup, albumId.ToString(), Extensions.ForURL(albumName));
            


            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("AlbumId", albumId);
            QueryString.Add("OwnerId", ownerId);
            QueryString.Add("AlbumName", albumName);
            this.Navigate(typeof(VideoPage), QueryString);
        }

        public void NavigateToSettings()
        {
            this.Navigate(typeof(SettingsPage));
        }

        public void NavigateToFavorites()
        {
            this.Navigate(typeof(FavoritesPage));
        }

        public void NavigateToLikes()
        {
            this.Navigate(typeof(MyLikesPage));
        }

        public void NavigateToDownloads()
        {
            this.Navigate(typeof(MediaDownloadPage));
        }

        public static bool GoBack()
        {
            if (CustomFrame.Instance.CanGoBack)
            {
                CustomFrame.Instance.GoBack();
                return true;
            }
            return false;
        }

        private void Navigate(Type navStr, object parameter = null)
        {
            //NavigationTransitionInfo info = new NavigationTransitionInfo();

            //Logger.Instance.Info("Navigator.Navigate, navStr={0} [{1}]", navStr.FullName, parameter);

            CustomFrame.Instance.Navigate(navStr, parameter/*,new DrillInNavigationTransitionInfo()*/);//анимация перехода не работает :(
            
            CustomFrame.Instance.OpenCloseMenu(false);

        }

        public void ClearBackStack()
        {
            CustomFrame.Instance.BackStack.Clear();
        }

        public void NavigateToNewsFeed(string query = null)
        {
            this.Navigate(typeof(NewsPage), query);
        }

        public void NavigateToWallPostComments(int ownerId, uint postId, uint commentId = 0, object postData = null)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("OwnerId", ownerId);
            QueryString.Add("ItemId", postId);
            QueryString.Add("CommentId", commentId);

            if (postData != null)
                QueryString.Add("Data", postData);

            this.Navigate(typeof(CommentsPage), QueryString);
            //PostCommentsPage
        }

        /// <summary>
        /// Добавляем плеер во фрейм
        /// </summary>
        /// <param name="ownerId">Владелец видеозаписи</param>
        /// <param name="videoId">ИД видеозаписи</param>
        /// <param name="accessKey"></param>
        /// <param name="video"></param>
        /// <param name="sender">Изображение на фоне</param>
        public void NavigateToVideoWithComments(int ownerId, uint videoId, string accessKey = "", VKVideoBase video = null, object sender = null)
        {
            VideoViewerUC.Show(ownerId, videoId, accessKey, video, sender);




            /*



            var temp = CustomFrame.Instance.VideoViewerElement;

            if (sender == null)//переход по ссылке
            {
                VideoCommentsViewModel vm0 = temp.DataContext as VideoCommentsViewModel;
                
                if (vm0 == null)
                {
                    vm0 = new VideoCommentsViewModel(ownerId, videoId, accessKey);
                    temp.InitViewModel(vm0);
                }

                PopUpService pop0 = new PopUpService();
                pop0.Child = new VideoViewerUC() { DataContext = vm0 };
                pop0.OverrideBackKey = true;
                pop0.AnimationTypeChild = PopUpService.AnimationTypes.Fade;
                pop0.BackgroundBrush = null;
                pop0.Show();

                
                CompositeTransform renderTransform0 = temp.RenderTransform as CompositeTransform;
                if(renderTransform0==null)
                {
                    renderTransform0 = new CompositeTransform();
                    temp.RenderTransform = renderTransform0;
                }
                temp.MakeNormal();

                if (renderTransform0 != null)
                {
                    var ease = new Windows.UI.Xaml.Media.Animation.QuarticEase() { EasingMode = Windows.UI.Xaml.Media.Animation.EasingMode.EaseOut };

                    renderTransform0.Animate(renderTransform0.TranslateX, 0, "TranslateX", 600, 0, ease);
                    renderTransform0.Animate(renderTransform0.TranslateY, 0, "TranslateY", 600, 0, ease);
                    renderTransform0.Animate(renderTransform0.ScaleX, 1, "ScaleX", 600, 0, ease);
                    renderTransform0.Animate(renderTransform0.ScaleY, 1, "ScaleY", 600, 0, ease);
                }

                return;
            }


            Debug.Assert(sender != null);

            VideoCommentsViewModel vm = new VideoCommentsViewModel(ownerId, videoId, accessKey);

            PopUpService pop = new PopUpService();
            pop.Child = new UC.VideoViewerUC() { DataContext = vm };
            pop.OverrideBackKey = true;
            pop.AnimationTypeChild = PopUpService.AnimationTypes.Fade;
            pop.BackgroundBrush = null;
            pop.Show();

            
            //temp.DataContext = vm;
            temp.InitViewModel(vm);
            CompositeTransform renderTransform = temp.RenderTransform as CompositeTransform;

            if (renderTransform == null )
            {
                if (sender != null)
                {
                    Common.ImageAnimator imageAnimator = new Common.ImageAnimator(200, null);
                    imageAnimator.AnimateIn((sender as FrameworkElement), temp);
                }
            }
            else
            {
                temp.MakeNormal();

                renderTransform.Animate(renderTransform.TranslateX, 0, "TranslateX", 600, 0, null);
                renderTransform.Animate(renderTransform.TranslateY, 0, "TranslateY", 600, 0, null);
                renderTransform.Animate(renderTransform.ScaleX, 1, "ScaleX", 600, 0, null, null);
                renderTransform.Animate(renderTransform.ScaleY, 1, "ScaleY", 600, 0, null, null);
            }
            */
        }

        public void NavigateToPhotoWithComments(int ownerId, uint photoId, string accessKey = "", VKPhoto photo = null)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("OwnerId", ownerId);
            QueryString.Add("ItemId", photoId);

            if (!string.IsNullOrEmpty(accessKey))
                QueryString.Add("AccessKey", accessKey);

            if (photo != null)
                QueryString.Add("Data", photo);
//            this.Navigate(typeof(PhotoCommentsPage), QueryString);
            this.Navigate(typeof(CommentsPage), QueryString);
        }

        public void NavigateToCommunityManagement(uint communityId, VKGroupType communityType, VKAdminLevel adminLevel)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("CommunityId", communityId);
            QueryString.Add("CommunityType", communityType);
            QueryString.Add("AdminLevel", adminLevel);
            this.Navigate(typeof(GroupManagementPage), QueryString);//Management/MainPage.xaml
        }
        /*
        public void NavigateToMarketPage()
        {
            this.Navigate(typeof(MarketPage));
        }
        */
        public void NavigateToMarket(int ownerId)
        {
            this.Navigate(typeof(MarketMainPage), ownerId);
        }
        public void NavigateToConversationMaterials(int peerId)
        {
            this.Navigate(typeof(ConversationMaterialsPage), peerId);
        }

        /// <summary>
        /// Переход к заявкам в группу
        /// </summary>
        /// <param name="communityId"></param>
        public void NavigateToCommunityManagementRequests(uint communityId)
        {
            this.Navigate(typeof(Pages.Group.Management.RequestsPage), communityId);
        }

        /// <summary>
        /// Переход к подписчикам группы
        /// </summary>
        /// <param name="communityId"></param>
        public void NavigateToCommunitySubscribers(uint communityId, VKGroupType communityType/*, bool isManagement = false, bool isPicker = false, bool isBlockingPicker = false*/)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("CommunityId", communityId);
            QueryString.Add("CommunityType", communityType);
            this.Navigate(typeof(Pages.Group.CommunitySubscribersPage), QueryString);
        }

        /// <summary>
        /// Переход к руководителям группы
        /// </summary>
        /// <param name="communityId"></param>
        public void NavigateToCommunityManagementManagers(uint communityId/*, GroupType communityType*/)
        {
            this.Navigate(typeof(Pages.Group.Management.ManagersPage), communityId/*, (int)communityType)*/);
        }

        public void NavigateToCommunityManagementInformation(uint communityId)
        {
//            this.Navigate(typeof(Pages.Group.Management.CommunityInformationPage),communityId);
        }

        public void NavigateToCommunityManagementServices(uint communityId)
        {
            this.Navigate(typeof(Pages.Group.Management.ServicesPage),communityId);
        }

        public void NavigateToWebUri(string uri, bool forceWebNavigation = false)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return;

            if (uri.StartsWith("tel:"))
            {
                //PhoneCallTask phoneCallTask = new PhoneCallTask();
                //string str = uri.Substring(4);
                //phoneCallTask.PhoneNumber = str;
                //phoneCallTask.Show();

            }
            else if (uri.StartsWith("vk.cc/"))
            {
                AccountService.Instance.CheckLink(uri, (result) =>
                {
                    if(result.error.error_code == VKErrors.None)
                    {
                        this.NavigateToWebUri(result.response.link);
                    }
                    else
                    {
                        this.NavigateToWebUri(uri);
                    }
                });
                return;
            }
            else
            {
                if (!uri.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) && !uri.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase))
                    uri = "http://" + uri;

                bool flag = false;
                if (!forceWebNavigation)
                    flag = this.GetWithinAppNavigationUri(uri);
                if (flag)
                    return;

                Task.Run(async() =>
                {
                    LauncherOptions options = new LauncherOptions();
                    
                    if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
                    {
                        options.IgnoreAppUriHandlers = true;
                    }
                    
                    await Launcher.LaunchUriAsync(new Uri(uri), options);
                });
            }
        }

        private void NavigateToGame(VKGame app, int sourceId, string uri)
        {
            if (app == null)
                return;
            int id = app.id;
            bool num1 = this.TryOpenGame(app);
            string utmParamsStr = "";
            if (!string.IsNullOrEmpty(uri))
            {
                int num2 = uri.IndexOf("?");
                int num3 = uri.IndexOf("#");
                if (num2 > -1 || num3 > -1)
                {
                    int startIndex = -1;
                    if (num2 > -1)
                    {
                        if (num3 > -1)
                            startIndex = num2 >= num3 ? num3 : num2 + 1;
                    }
                    else
                        startIndex = num3;
                    if (startIndex > -1)
                        utmParamsStr = uri.Substring(startIndex);
                }
                else
                {
                    utmParamsStr = uri;//my
                }
            }
            if (num1 == false && id != 0)
            {
               this.NavigateToProfileAppPage((uint)id, sourceId, app.title, utmParamsStr);
                
            }
        }

        public void NavigateToGroupRecommendations(object sender)//(int categoryId, string categoryName)
        {
            this.Navigate(typeof(RecommendedGroupsPage));
        }

        public void NavigateToSuggestedSourcesPage(object sender)
        {
            this.Navigate(typeof(SuggestedSourcesPage));
        }

        public void NavigateToUsersSearch(string query = "")
        {
            this.Navigate(typeof(SearchResultsPage), query); //UsersSearchResultsPage
        }

        public void NavigateToProfileAppPage(uint appId, int ownerId, string appName, string utmParamsStr = "")
        {
         //   utmParamsStr = HttpUtility.UrlEncode(utmParamsStr);
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("AppId", appId);
            QueryString.Add("OwnerId", ownerId);
            QueryString.Add("AppName", appName);
            if(!string.IsNullOrEmpty(utmParamsStr))
                QueryString.Add("UtmParams", utmParamsStr);
            
            this.Navigate(typeof(ProfileAppPage), QueryString);
        }

        public void NavigateToAudioPlayer(VKPlaylist playlist = null, int trackNumber = -1)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            
            if(trackNumber!=-1)
                QueryString.Add("TrackNumber", trackNumber);

            if (playlist != null)
                QueryString.Add("Playlist", playlist);

            this.Navigate(typeof(AudioPlayer), QueryString);
        }

        private bool TryOpenGame(VKGame game)
        {
            bool result = false;
            
            if (!string.IsNullOrEmpty(game.platform_id) && game.is_in_catalog)
            {
                result = true;
                /*
                Execute.ExecuteOnUIThread(delegate
                {
                    PageBase currentPage = FramePageUtils.CurrentPage;
                    if (currentPage == null || currentPage is OpenUrlPage)
                    {
                        this.NavigateToGames(game.id, false);
                        return;
                    }
                    Grid grid = currentPage.Content as Grid;
                    FrameworkElement root = null;
                    if (((grid != null) ? grid.Children : null) != null && grid.Children.Count > 0)
                    {
                        root = (grid.Children[0] as FrameworkElement);
                    }
                    PageBase arg_94_0 = currentPage;
                    List<object> expr_70 = new List<object>();
                    expr_70.Add(game);
                    arg_94_0.OpenGamesPopup(expr_70, fromPush ? GamesClickSource.push : GamesClickSource.catalog, "", 0, root);
                });*/
                
            }
            return result;
        }

        private bool _isNavigatingToGame;

        private void NavigateToGame(int appId, int sourceId, string uri)
        {
            if (this._isNavigatingToGame)
                return;
            this._isNavigatingToGame = true;

            AppsService.Instance.GetApp(appId, (result =>
            {
                this._isNavigatingToGame = false;
                if (result.error.error_code == VKErrors.None)
                {
                    VKGame app;
                    if (result.response == null)
                    {
                        app = null;
                    }
                    else
                    {
                        app = result.response.items.FirstOrDefault();
                    }
                    Execute.ExecuteOnUIThread(() =>
                    { 
                        this.NavigateToGame(app, sourceId, uri);
                    });
                }
                //else
               //     GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
            }));
        }

        public void NavigateToDocuments(int ownerId = 0/*, bool isOwnerCommunityAdmined = false*/)
        {
            this.Navigate(typeof(DocumentsPage), ownerId);
        }

        public void NavigateToAlbum(VKPlaylist playlist)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("Playlist", playlist);
            this.Navigate(typeof(Pages.Audio.AlbumPage), QueryString);
        }

        public void NavigateToGroupDiscussions(int gid, string name, VKAdminLevel adminLevel, bool isPublicPage, bool canCreateTopic)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("GroupId", gid);
            QueryString.Add("Name", name);
            QueryString.Add("AdminLevel", adminLevel);
            QueryString.Add("IsPublicPage", isPublicPage);
            QueryString.Add("CanCreateTopic", canCreateTopic);
            this.Navigate(typeof(Pages.Group.GroupDiscussionsPage), QueryString);
        }

        /// <summary>
        /// Переход к обсуждению
        /// </summary>
        /// <param name="groupId">ИД группы без минуса</param>
        /// <param name="topicId">ИД обсуждения</param>
        /// <param name="topicName">Название обсуждения</param>
        /// <param name="canComment"></param>
        public void NavigateToGroupDiscussion(uint groupId, uint topicId, string topicName = "", bool canComment = true, uint commentId = 0)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("GroupId", groupId);
            QueryString.Add("TopicId", topicId);
            QueryString.Add("TopicName", topicName);
            //QueryString.Add("KnownCommentsCount", knownCommentsCount);
            //QueryString.Add("LoadFromTheEnd", loadFromEnd);
            QueryString.Add("CanComment", canComment);
            QueryString.Add("CommentId", commentId);
            this.Navigate(typeof(Pages.Group.GroupDiscussionPage), QueryString);
        }

        public void NavigateToNewWallPost(WallPostViewModel.Mode mode1 = WallPostViewModel.Mode.NewWallPost, int userOrGroupId = 0, VKAdminLevel adminLevel = 0, bool isPublicPage = false, VKWallPost data = null)
        {
            //if (userOrGroupId == 0)
            //    userOrGroupId = (int)Settings.UserId;
            
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("UserOrGroupId", userOrGroupId);
            QueryString.Add("AdminLevel", adminLevel);
            QueryString.Add("IsPublicPage", isPublicPage);
            
            QueryString.Add("Mode", mode1);
            //QueryString.Add("FromWallPostPage", isFromWallPostPage);
            //QueryString.Add("IsPostponed", isPostponed);
            if(data!=null)
                QueryString.Add("Data", data);
            //IsPopupNavigation

            this.Navigate(typeof(NewPostPage), QueryString);
        }
        /*
        public void NavigateToCreateEditPoll(int ownerId, int pollId = 0, VKPoll poll = null)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();

            if (poll != null)
                QueryString.Add("Poll", poll);
            this.Navigate(string.Format("/VKClient.Common;component/CreateEditPollPage.xaml?OwnerId={0}&PollId={1}", ownerId, pollId) + "&IsPopupNavigation=True");
        }
        */

        public void NavigateToPostsSearch(int ownerId, string nameGen = "", string q = "", string domain = "")
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("OwnerId", ownerId);
            QueryString.Add("NameGen", nameGen);
            QueryString.Add("Query", q);
            QueryString.Add("Domain", domain);
            this.Navigate(typeof(Pages.Group.PostsSearchPage), QueryString);
        }

        public void NavigateToStoryCreate()
        {
            this.Navigate(typeof(CreateStoryPage));
        }

        public void NavigateToCommunityManagementLinks(uint communityId)
        {
            this.Navigate(typeof(Pages.Group.Management.LinksPage), communityId);
        }

        public void NavigateToManageSources(bool hidenSourcesMode)
        {
            this.Navigate(typeof(ManageSourcesPage), hidenSourcesMode);
        }

        public void NavigateToGroupWikiPages(uint groupId, string title = "")
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("GroupId", groupId);
            QueryString.Add("Title", title);
            this.Navigate(typeof(Pages.Group.WikiListPage), QueryString);
        }

        public void NavigateToProduct(int ownerId, uint productId)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("OwnerId", ownerId);
            QueryString.Add("ProductId", productId);
            this.Navigate(typeof(Pages.ProductPage), QueryString);
        }

        public void NavigateToStickersStore(long userOrChatId = 0, bool isChat = false)
        {
            this.Navigate(typeof(StickersStorePage));
        }

        private static void ShowStickersPack(string stickersPackName)
        {
            stickersPackName = stickersPackName.Replace("/", "");
            if (string.IsNullOrWhiteSpace(stickersPackName))
                return;
            StickersPackViewUC.Show(stickersPackName, "link");
        }

        public void NavigateToRegistrationPage()
        {
            this.Navigate(typeof(RegistrationPage));
        }
        

        #region WEB_PARCER
        /// <summary>
        /// Навигация внутри приложение, если ссылка ВК. Возвращает правду, если навигация будет внутри.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="fromPush"></param>
        /// <param name="customCallback"></param>
        /// <returns></returns>
        public bool GetWithinAppNavigationUri(string uri, bool fromPush = false, Action<bool> customCallback = null)
        {
            if (!NavigatorImpl.IsVKUri(uri))
                return false;
            string uri1 = uri;
            int num = uri1.IndexOf("://");
            if (num > -1)
                uri1 = uri1.Remove(0, num + 3);
            int count = uri1.IndexOf("/");
            if (count > -1)
                uri1 = uri1.Remove(0, count);
            if (uri1.StartsWith("dev/") || uri1.StartsWith("dev") && uri1.Length == 3)
                return false;
            Dictionary<string, string> queryString = uri.ParseQueryString();
            if (uri1.StartsWith("/feed") && queryString.ContainsKey("section") && queryString["section"] == "search")
            {
                //bug: если ссылка в конце обрезанного текста, то в неё имеются три точки (...) на конце
                if (queryString.ContainsKey("q") && queryString["q"].Contains("@"))
                {
                    string[] s = queryString["q"].Split('@');
                    this.NavigateToPostsSearch(0, "", s[0], s[1]);
                }
                else
                {
                    this.NavigateToNewsFeed(queryString.ContainsKey("q") ? queryString["q"] : "");
                    //this.NavigateToNewsSearch(HttpUtility.UrlDecode(queryString.ContainsKey("q") ? queryString["q"] : ""));
                }

                return true;
            }
            int ownerId;
            int id2;
            int id3;
            string objName;
            string objSub;
            NavigatorImpl.NavType navigationType = this.GetNavigationType(uri1, out ownerId, out id2, out id3, out objName, out objSub);
            if (navigationType == NavigatorImpl.NavType.none)
                return false;
            if (ownerId == 0)
                ownerId = (int)Settings.UserId;
            //bool flag = true;
            switch (navigationType)
            {
                case NavigatorImpl.NavType.friends:
              //      this.NavigateToFriends(id1, "", false, FriendsPageMode.Default);
                    break;
                case NavigatorImpl.NavType.communities:
            //        this.NavigateToGroups(AppGlobalStateManager.Current.LoggedInUserId, "", false, 0, 0, "", false, "", 0L);
                    break;
                case NavigatorImpl.NavType.dialogs:
                    this.NavigateToConversations();
                    break;
                case NavigatorImpl.NavType.news:
             //       this.NavigateToNewsFeed(0, false);
                    break;
                case NavigatorImpl.NavType.tagPhoto:
             //       this.NavigateToPhotoAlbum(Math.Abs(id1), id1 < 0, "2", "0", "", 0, "", "", false, 0, false);
                    break;
                case NavigatorImpl.NavType.albums:
            //        this.NavigateToPhotoAlbums(false, Math.Abs(id1), id1 < 0, 0);
                    break;
                
                case NavigatorImpl.NavType.dialog:
              //      this.NavigateToConversation(id1, false, false, "", 0, false);
                    break;
                case NavigatorImpl.NavType.profile:
                    this.NavigateToProfilePage(ownerId);
                    break;
                case NavigatorImpl.NavType.community:
                    this.NavigateToProfilePage(-ownerId);
                    break;
                case NavigatorImpl.NavType.board:
             //       this.NavigateToGroupDiscussions(id1, "", 0, false, false);
                    break;
                case NavigatorImpl.NavType.album:
                    //long albumIdLong = AlbumTypeHelper.GetAlbumIdLong(id2String);
                    //AlbumType albumType = AlbumTypeHelper.GetAlbumType(albumIdLong);
                    //this.NavigateToPhotoAlbum(Math.Abs(id1), id1 < 0, albumType.ToString(), albumIdLong.ToString(), "", 0, "", "", false, 0, false);
                    this.NavigateToPhotosOfAlbum(ownerId, id2,"");
                    break;
                case NavigatorImpl.NavType.video:
                    this.NavigateToVideoWithComments(ownerId, (uint)id2);
                    break;
                case NavigatorImpl.NavType.audios:
             //       this.NavigateToAudio(0, Math.Abs(id1), id1 < 0, 0, 0, "");
                    break;
                case NavigatorImpl.NavType.topic:
                    this.NavigateToGroupDiscussion((uint)-ownerId, (uint)id2);
                    break;
                case NavigatorImpl.NavType.photo:
                    this.NavigateToPhotoWithComments(ownerId, (uint)id2);
                    break;
                case NavigatorImpl.NavType.wallPost:
                    this.NavigateToWallPostComments(ownerId,(uint)id2, (uint)id3);
                    break;
                case NavigatorImpl.NavType.namedObject:
                    this.ResolveScreenNameNavigationObject(uri, objName);
                    break;
                case NavigatorImpl.NavType.stickersSettings:
               //     this.NavigateToStickersManage();
                    break;
                case NavigatorImpl.NavType.settings:
                    this.NavigateToSettings();
                    break;
                case NavigatorImpl.NavType.feedback:
                    this.NavigateToFeedback();
                    break;
                case NavigatorImpl.NavType.videos:
            //        this.NavigateToVideo(false, Math.Abs(id1), id1 < 0, false);
                    break;
                case NavigatorImpl.NavType.fave:
                    this.NavigateToFavorites();
                    break;
                case NavigatorImpl.NavType.apps:
                   this.NavigateToGames(0/*, fromPush*/);
                    //flag = false;
                    break;
                case NavigatorImpl.NavType.marketAlbum:
              //      this.NavigateToMarketAlbumProducts(id1, id2, null);
                    break;
                case NavigatorImpl.NavType.market:
                    this.NavigateToMarket(ownerId);
                    break;
                case NavigatorImpl.NavType.product:
                    this.NavigateToProduct(ownerId, (uint)id2);
                    break;
                case NavigatorImpl.NavType.stickers:
                    this.NavigateToStickersStore(0, false);
                    break;
                case NavigatorImpl.NavType.stickersPack:
                    NavigatorImpl.ShowStickersPack(objSub);
                    break;
                case NavigatorImpl.NavType.recommendedNews:
               //     this.NavigateToNewsFeed(NewsSources.Suggestions.PickableItem.ID, false);
                    break;
                case NavigatorImpl.NavType.app:
                    this.NavigateToGame(ownerId, id2, uri);
                    break;
                case NavigatorImpl.NavType.gifts:
                //    EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.link, GiftPurchaseStepsAction.gifts_page));
                //    this.NavigateToGifts(id1, "", "");
                    break;
                case NavigatorImpl.NavType.giftsCatalog:
              //      EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.link, GiftPurchaseStepsAction.store));
               //     this.NavigateToGiftsCatalog(0, false);
                    break;
                case NavigatorImpl.NavType.podcasts:
                    {
                        this.NavigateToPodcasts(ownerId, "");
                        break;
                    }
            }
            //return flag;
            return true;
        }

        private static bool IsVKUri(string uri)
        {
            uri = uri.ToLowerInvariant();
            uri = uri.Replace("http://", "").Replace("https://", "");
            if (uri.StartsWith("m.") || uri.StartsWith("t.") || uri.StartsWith("0."))
                uri = uri.Remove(0, 2);
            if (uri.StartsWith("www.") || uri.StartsWith("new."))
                uri = uri.Remove(0, 4);
            if (!uri.StartsWith("vk.com/") && !uri.StartsWith("vkontakte.ru/"))
                return uri.StartsWith("vk.me/");
            return true;
        }

        

        private bool _isResolvingScreenName;

        private void ResolveScreenNameNavigationObject(string uri, string objName)
        {
            if (this._isResolvingScreenName)
                return;

            this._isResolvingScreenName = true;
            AccountService.Instance.ResolveScreenName(objName.Replace("/", ""), (result) => {

                this._isResolvingScreenName = false;

                if (result.error.error_code == VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() => { 
                        bool flag = false;
                        if (!string.IsNullOrEmpty(uri) && uri.StartsWith("http://vk.me/"))
                        {
                            flag = true;
                        }

                        string type = result.response.type;
                        int object_id = result.response.object_id;
                        if (type == "user")
                        {
                            if (flag)
                                this.NavigateToConversation(object_id);
                            else
                                this.NavigateToProfilePage(object_id);
                        }
                        else if (type == "group")
                        {
                            if (flag)
                                this.NavigateToConversation(-object_id);
                            else
                                this.NavigateToProfilePage(-object_id);
                        }
                        else if (type == "application" || type == "vk_app" /*&& AppGlobalStateManager.Current.GlobalState.GamesSectionEnabled*/)
                        {
                            //Game app = res.ResultData.app;
                            //this.NavigateToGame(app, 0, uri, fromPush, customCallback);
                            //return;

                            this.NavigateToGame(object_id, 0, uri);
                        }
                        else
                            this.NavigateToWebUri(uri, true);
                    });
                }
                else
                    this.NavigateToWebUri(uri, true);
            });

            
        }

        private NavigatorImpl.NavType GetNavigationType(string uri, out int id1, out int id2, out int id3, out string obj, out string objSub)
        {
            id1 = id2 = id3 = 0;
            obj = objSub = "";
            foreach (NavigatorImpl.NavigationTypeMatch navTypes1 in this._navTypesList)
            {
                if (navTypes1.Check(uri))
                {
                    if (navTypes1.SubTypes.Count > 0)
                    {
                        foreach (string subType in navTypes1.SubTypes)
                        {
                            foreach (NavigatorImpl.NavigationTypeMatch navTypes2 in this._navTypesList)
                            {
                                if (navTypes2.Check(subType))
                                {
                                    id1 = navTypes2.Id1;
                                    id2 = navTypes2.Id2;
                                    //id3 = navTypes2.Id3;
                                    obj = navTypes2.ObjName;
                                    objSub = navTypes2.ObjSubName;
                                    return navTypes2.MatchType;
                                }
                            }
                        }
                    }
                    id1 = navTypes1.Id1;
                    id2 = navTypes1.Id2;
                    id3 = navTypes1.Id3;
                    obj = navTypes1.ObjName;
                    objSub = navTypes1.ObjSubName;
                    return navTypes1.MatchType;
                }
            }
            return NavigatorImpl.NavType.none;
        }




#endregion

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aid">ИД альбома</param>
        /// <param name="albumType">Тип альбома (wall,profile,saved)</param>
        /// <param name="userOrGroupId">Владелец</param>
        /// <param name="photosCount">Всего изображений</param>
        /// <param name="selectedPhotoIndex">Что показываем?</param>
        /// <param name="photos"></param>
        /// <param name="getImageByIdFunc"></param>
        public void NavigateToImageViewer(string aid, ImageViewerViewModel.AlbumType albumType, int userOrGroupId, uint photosCount, int selectedPhotoIndex, List<VKPhoto> photos, Func<int, Border> getImageByIdFunc)
        {
            UC.ImageViewerDecoratorUC.ShowPhotosFromAlbum(aid, albumType, userOrGroupId, photosCount, selectedPhotoIndex, photos, getImageByIdFunc);
        }

        public void NavigateToImageViewer(uint photosCount, int initialOffset, int selectedPhotoIndex, List<VKPhoto> photos, ImageViewerViewModel.ViewerMode viewerMode, Func<int, Border> getImageByIdFunc = null, bool hideActions = false)
        {
            UC.ImageViewerDecoratorUC.ShowPhotosById(photosCount, initialOffset, selectedPhotoIndex, photos, viewerMode, getImageByIdFunc, hideActions);
        }

        public void NaviateToImageViewerPhotoFeed(int userOrGroupId, string aid, uint photosCount, int selectedPhotoIndex, DateTime date, List<VKPhoto> photos, string mode, Func<int, Border> getImageByIdFunc)
        {
            UC.ImageViewerDecoratorUC.ShowPhotosFromFeed(userOrGroupId, aid, photosCount, selectedPhotoIndex, date, photos, mode, getImageByIdFunc);
        }
    
        //public void NavigateToImageViewer(uint photosCount, int selectedPhotoIndex, ObservableCollection<VKPhoto> photos, Action<Action<bool>> loadMoreFunc, Func<int, Image> getImageByIdFunc)
        //{
        //    UC.ImageViewerDecoratorUC.ShowPhotosById(photosCount, initialOffset, selectedPhotoIndex, photos, getImageByIdFunc, hideActions);
        //}

        public void NavigateToCommunityManagementBlacklist(uint communityId/*, GroupType communityType*/)
        {
            this.Navigate(typeof(Pages.Group.Management.BlacklistPage), communityId);
        }

        public void NavigateToSubscriptions(uint userId)
        {
            this.Navigate(typeof(SubscriptionsPage), userId);
        }

        public void NavigateToLikesPage(int ownerId, uint itemId, LikeObjectType type, int knownCount = 0, bool selectFriendLikes = false)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("OwnerId", ownerId);
            QueryString.Add("ItemId", itemId);
            QueryString.Add("Type", (byte)type);
            QueryString.Add("KnownCount", knownCount);
            QueryString.Add("SelectFriendLikes", selectFriendLikes);
            this.Navigate(typeof(LikesPage), QueryString);
        }

        public void NavigateToSuggestedPostponedPostsPage(int userOrGroupId, int mode)
        {
            Dictionary<string, int> QueryString = new Dictionary<string, int>();
            QueryString.Add("UserOrGroupId", userOrGroupId);
            QueryString.Add("Mode", mode);
            this.Navigate(typeof(SuggestedPostponedPostsPage), QueryString);
        }

        public void NavigateToGames(long gameId = 0)
        {
            this.Navigate(typeof(GamesMainPage), gameId);
        }

        public void NavigateToStickersManage()
        {
            this.Navigate(typeof(StickersManagePage));
        }

        public void NavigateShareExternalContentpage(Windows.ApplicationModel.DataTransfer.ShareTarget.ShareOperation shareOperation)
        {
            this.Navigate(typeof(ShareExternalContentPage), shareOperation);
        }

        public void NavigateToPodcasts(int ownerId, string ownerName)
        {
            this.Navigate(typeof(PodcastsPage), ownerId);
        }

        public void NavigateToArticles(int ownerId, string ownerName)
        {
            this.Navigate(typeof(ArticlesPage), ownerId);
        }

        public void NavigateToEditProfile()
        {
            this.Navigate(typeof(SettingsEditProfilePage));
        }

        public void NavigateToPhotoPickerPhotos(int maxAllowedToSelect, Windows.Storage.StorageFile pickToStorageFile = null)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("MaxAllowedToSelect", maxAllowedToSelect);
            //QueryString.Add("OwnPhotoPick", ownPhotoPick);

            if(pickToStorageFile!=null)
                QueryString.Add("PickToStorageFile", pickToStorageFile);
            //QueryString.Add("IsPopupNavigation", true);
            this.Navigate(typeof(Pages.PhotoPickerPhotos), QueryString);
        }

        public void NavigateToChangePassword()
        {
            this.Navigate(typeof(ChangePasswordPage));
        }

        public void NavigateToBirthdaysPage()
        {
            this.Navigate(typeof(BirthdaysPage));
        }

        public void NavigateToAddNewVideo(string filePath, int ownerId)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("VideoToUploadPath", filePath);
            QueryString.Add("OwnerId", ownerId);

            this.Navigate(typeof(AddEditVideoPage) , QueryString);
        }

        public void NavigateToEditVideo(int ownerId, uint videoId, VKVideoBase video = null)
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("OwnerId", ownerId);
            QueryString.Add("VideoId", videoId);
            QueryString.Add("VideoForEdit", video);

            //if (video != null)
            //    ParametersRepository.SetParameterForId("VideoForEdit", video);

            this.Navigate(typeof(AddEditVideoPage), QueryString);
        }

        public void NavigateToGifts(uint userId, string firstName = "", string firstNameGen = "")
        {
            Dictionary<string, object> QueryString = new Dictionary<string, object>();
            QueryString.Add("UserId", userId);
            QueryString.Add("FirstName", firstName);
            QueryString.Add("FirstNameGen", firstNameGen);

            this.Navigate(typeof(GiftsPage), QueryString);
        }

        #region REGex
        private static readonly Regex _friendsReg = new Regex("/friends(\\?id=[0-9])?");
        private static readonly Regex _communitiesReg = new Regex("/groups(\\s|$)");
        private static readonly Regex _dialogsReg = new Regex("/(im|mail)(\\s|$)");
        private static readonly Regex _dialogReg = new Regex("/write[-0-9]+");
        private static readonly Regex _wallReplyReg = new Regex("/wall[-0-9]+_[0-9]+\\?reply=[0-9]+");
        private static readonly Regex _wallReg = new Regex("/wall[-0-9]+_[0-9]+");
        private static readonly Regex _feedWallReg = new Regex("/feed?w=wall[-0-9]+_[0-9]+");
        private static readonly Regex _audiosReg = new Regex("/audios[-0-9]+");
        private static readonly Regex _newsReg = new Regex("/feed(\\s|$)");
        private static readonly Regex _recommendedNewsReg = new Regex("/feed\\?section=recommended(\\s|$)");
        private static readonly Regex _feedbackReg = new Regex("/feed\\?section=notifications(\\s|$)");
        private static readonly Regex _profileReg = new Regex("/(id|wall)[0-9]+");
        private static readonly Regex _communityReg = new Regex("/(club|event|public|wall)[-0-9]+");
        private static readonly Regex _photosReg = new Regex("/(photos|albums)[-0-9]+");
        private static readonly Regex _photoReg = new Regex("/photo[-0-9]+_[0-9]+");
        private static readonly Regex _albumReg = new Regex("/album[-0-9]+_[0-9]+");
        private static readonly Regex _tagReg = new Regex("/tag[0-9]+");
        private static readonly Regex _videosReg = new Regex("/videos[-0-9]+");
        private static readonly Regex _videoReg = new Regex("/video[-0-9]+_[0-9]+");
        private static readonly Regex _videoTimeReg = new Regex("/video[-0-9]+_[0-9]+?t=(\\s|$)");
        private static readonly Regex _boardReg = new Regex("/board[0-9]+");
        private static readonly Regex _topicReg = new Regex("/topic[-0-9]+_[0-9]+");
        private static readonly Regex _stickersSettingsReg = new Regex("/stickers/settings(\\s|$)");
        private static readonly Regex _settingsReg = new Regex("/settings(\\s|$)");
        private static readonly Regex _stickersReg = new Regex("/stickers(\\s|\\?|$)");
        private static readonly Regex _stickersPackReg = new Regex("/stickers([\\/A-Za-z0-9]+)");
        private static readonly Regex _faveReg = new Regex("/fave(\\s|$)");
        private static readonly Regex _appsReg = new Regex("/apps(\\s|$)");
        private static readonly Regex _appReg = new Regex("/app[-0-9]+_[-0-9]+");
        private static readonly Regex _marketAlbumReg = new Regex("/market[-0-9]+\\?section=album_[-0-9]+");
        private static readonly Regex _marketReg = new Regex("/market[-0-9]+");
        private static readonly Regex _productReg = new Regex("/product[-0-9]+_[0-9]+");
        private static readonly Regex _giftsReg = new Regex("/gifts[0-9]+");
        private static readonly Regex _giftsCatalog = new Regex("/gifts(\\s|$)");
        private static readonly Regex _namedObjReg = new Regex("/[A-Za-z0-9\\\\._-]+");
        //
        private static readonly Regex _podcastsReg = new Regex("/podcasts[-0-9]+");
        //
        public static readonly Regex Regex_Mention = new Regex("\\[(id|club)(\\d+)(?:\\:([a-z0-9_\\-]+))?\\|([^\\$]+?)\\]", RegexOptions.IgnoreCase);
        public static readonly Regex Regex_DomainMention = new Regex("(\\*|@)((id|club|event|public)\\d+)\\s*\\((.+?)\\)", RegexOptions.IgnoreCase);

        private readonly List<NavigatorImpl.NavigationTypeMatch> _navTypesList = new List<NavigatorImpl.NavigationTypeMatch>()
        {
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._friendsReg, NavigatorImpl.NavType.friends),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._communitiesReg, NavigatorImpl.NavType.communities),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._dialogsReg, NavigatorImpl.NavType.dialogs),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._dialogReg, NavigatorImpl.NavType.dialog),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._wallReplyReg, NavigatorImpl.NavType.wallPost),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._wallReg, NavigatorImpl.NavType.wallPost),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._feedWallReg, NavigatorImpl.NavType.wallPost),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._audiosReg, NavigatorImpl.NavType.audios),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._newsReg, NavigatorImpl.NavType.news),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._recommendedNewsReg, NavigatorImpl.NavType.recommendedNews),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._feedbackReg, NavigatorImpl.NavType.feedback),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._profileReg, NavigatorImpl.NavType.profile),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._communityReg, NavigatorImpl.NavType.community),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._photosReg, NavigatorImpl.NavType.albums),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._photoReg, NavigatorImpl.NavType.photo),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._albumReg, NavigatorImpl.NavType.album),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._tagReg, NavigatorImpl.NavType.tagPhoto),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._videosReg, NavigatorImpl.NavType.videos),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._videoReg, NavigatorImpl.NavType.video),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._videoTimeReg, NavigatorImpl.NavType.video),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._boardReg, NavigatorImpl.NavType.board),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._topicReg, NavigatorImpl.NavType.topic),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._stickersSettingsReg, NavigatorImpl.NavType.stickersSettings),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._settingsReg, NavigatorImpl.NavType.settings),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._faveReg, NavigatorImpl.NavType.fave),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._appsReg, NavigatorImpl.NavType.apps),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._appReg, NavigatorImpl.NavType.app),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._marketAlbumReg, NavigatorImpl.NavType.marketAlbum),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._marketReg, NavigatorImpl.NavType.market),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._productReg, NavigatorImpl.NavType.product),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._stickersReg, NavigatorImpl.NavType.stickers),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._stickersPackReg, NavigatorImpl.NavType.stickersPack),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._giftsReg, NavigatorImpl.NavType.gifts),
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._giftsCatalog, NavigatorImpl.NavType.giftsCatalog),
            //
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._podcastsReg, NavigatorImpl.NavType.podcasts),
            //
            new NavigatorImpl.NavigationTypeMatch(NavigatorImpl._namedObjReg, NavigatorImpl.NavType.namedObject),
        };

        public class NavigationTypeMatch
        {
            private readonly Regex _idsRegEx = new Regex("\\-?[0-9]+");
            private readonly Regex _queryParamsRegex = new Regex("(\\?|\\&)([^=]+)\\=([^&]+)");

            private readonly Regex _regEx;
            public NavigatorImpl.NavType MatchType { get; private set; }

            public int Id1 { get; private set; }
            public int Id2 { get; private set; }
            public int Id3 { get; private set; }
            public List<string> SubTypes { get; private set; }
            public string ObjName { get; private set; }
            public string ObjSubName { get; private set; }

            public NavigationTypeMatch(Regex regExp, NavigatorImpl.NavType navType)
            {
                this._regEx = regExp;
                this.MatchType = navType;
            }

            public bool Check(string uri)
            {
                MatchCollection matchCollection1 = this._regEx.Matches(uri);
                if (matchCollection1.Count == 0)
                    return false;
                Match match1 = matchCollection1[0];
                this.ObjName = match1.Value;
                if (match1.Groups.Count > 0)
                    this.ObjSubName = match1.Groups[match1.Groups.Count - 1].Value;
                MatchCollection matchCollection2 = this._idsRegEx.Matches(this.ObjName);
                if (matchCollection2.Count > 0)
                {
                    int result;
                    this.Id1 = int.TryParse(matchCollection2[0].Value, out result) ? result : 0;
                }
                if (matchCollection2.Count > 1)
                {
                    int result;
                    this.Id2 = int.TryParse(matchCollection2[1].Value, out result) ? result : 0;
                }
                if (matchCollection2.Count > 2)
                {
                    int result;
                    this.Id3 = int.TryParse(matchCollection2[2].Value, out result) ? result : 0;
                }
                MatchCollection matchCollection3 = this._queryParamsRegex.Matches(uri);
                this.SubTypes = new List<string>();
                foreach (Match match2 in matchCollection3)
                {
                    if (match2.Groups.Count == 4 && match2.Groups[2].Value == "w")
                        this.SubTypes.Add("/" + match2.Groups[match2.Groups.Count - 1].Value);
                }
                return true;
            }
        }
#endregion
        public enum NavType
        {
            none,
            friends,
            communities,
            dialogs,
            news,
            tagPhoto,
            albums,
            profile,
            dialog,
            community,
            board,
            album,
            video,
            audios,
            topic,
            photo,
            wallPost,
            namedObject,
            stickersSettings,
            settings,
            feedback,
            videos,
            fave,
            apps,
            marketAlbum,
            market,
            product,
            stickers,
            stickersPack,
            recommendedNews,
            app,
            gifts,
            giftsCatalog,
            podcasts,
        }
    }
}
