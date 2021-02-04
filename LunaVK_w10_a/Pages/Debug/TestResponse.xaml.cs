using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using LunaVK.Core.Network;
using LunaVK.Core;
using LunaVK.Core.Framework;
using System.Collections.ObjectModel;
using LunaVK.Framework;
using LunaVK.Library;
using Newtonsoft.Json.Linq;

namespace LunaVK.Pages.Debug
{
    public sealed partial class TestResponse : PageBase
    {
        public ObservableCollection<PollOption> PollOptions { get; set; }
        private bool _inProcess;

        public TestResponse()
        {
            base.DataContext = this;

            base.Title = "Тест ответа сервера ВК";

            this.InitializeComponent();

            this.PollOptions = new ObservableCollection<PollOption>();

            base.Loaded += TestResponse_Loaded;
        }

        private void TestResponse_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.OptionsMenu.Add(new OptionsMenuItem() { Icon = "\xE815", Clicked = this._appBarButton_Click });
        }

        private void _appBarButton_Click(object sender)
        {
            this.Button_Click(sender, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this._inProcess)
                return;

            this._tbOut.Text = "";

            Dictionary<string, string> parameters = new Dictionary<string, string>();


            foreach (var param in this.PollOptions)
            {
                parameters.Add(param.Name, param.Value ?? "");
            }

            if (!parameters.ContainsKey("v"))
                parameters["v"] = VKConstants.API_VERSION;
            if (!parameters.ContainsKey("access_token"))
                parameters["access_token"] = Settings.AccessToken;
            if (!parameters.ContainsKey("lang"))
                parameters["lang"] = "ru";

            this._inProcess = true;
            this._prog.IsIndeterminate = true;
            string baseUrl = "https://api.vk.com/method/" + this._tbMethod.Text;
            JsonWebRequest.SendHTTPRequestAsync(baseUrl, parameters, ((jsonResp, IsSucceeded) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    this._inProcess = false;
                    this._prog.IsIndeterminate = false;
                    if (IsSucceeded)
                    {
                        this._tbOut.Text = JToken.Parse(jsonResp).ToString(Newtonsoft.Json.Formatting.Indented);
                        //this._tbOut.Text = jsonResp;
                    }
                });
            }));
        }

        public class PollOption
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        private void AddAnswer_Click(object sender, RoutedEventArgs e)
        {
            this.PollOptions.Add(new PollOption());
        }

        private void Delete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PollOption vm = (sender as FrameworkElement).DataContext as PollOption;
            this.PollOptions.Remove(vm);
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            PollOption vm = (sender as FrameworkElement).DataContext as PollOption;
            if (vm == null)
                return;
            vm.Name = (sender as TextBox).Text;
        }

        private void Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            PollOption vm = (sender as FrameworkElement).DataContext as PollOption;
            if (vm == null)
                return;
            vm.Value = (sender as TextBox).Text.Replace("\r\n", "\r").Replace("\r", "\r\n");
        }

        private ObservableCollection<string> suggestions = new ObservableCollection<string>();

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Only get results when it was a user typing,
            // otherwise assume the value got filled in by TextMemberPath
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                //Set the ItemsSource to be your filtered dataset

                this.BuildSource(sender);
            }
        }

        private void BuildSource(AutoSuggestBox sender)
        {
            suggestions.Clear();

            string text = sender.Text;
            if (text.Contains("."))
            {
                var items = _methods.Where((m) => m.StartsWith(text));
                suggestions = new ObservableCollection<string>(items);
            }
            else
            {
                var itemsParts = _methods.Select((m) => m.Contains(".") ? m.Substring(0, m.IndexOf('.')) : m);
                var items = itemsParts.Where((m) => m.StartsWith(text)).Distinct();
                suggestions = new ObservableCollection<string>(items);

            }
            sender.ItemsSource = suggestions;
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Set sender.Text. You can use args.SelectedItem to build your text string.
            var selectedItem = args.SelectedItem.ToString();
            sender.Text = selectedItem;
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
                sender.Text = args.ChosenSuggestion.ToString();
            }
            else
            {
                // Use args.QueryText to determine what to do.
                //sender.Text = sender.Text;
            }
        }

        private void _tbMethod_GotFocus(object sender, RoutedEventArgs e)
        {
            this.BuildSource(sender as AutoSuggestBox);
        }

        private readonly List<string> _methods = new List<string>()
        {
            "account.ban",
            "account.changePassword",
            "account.getActiveOffers",
            "account.getAppPermissions",
            "account.getBanned",
            "account.getCounters",
            "account.getInfo",
            "account.getProfileInfo",
            "account.getPushSettings",
            "account.registerDevice",
            "account.saveProfileInfo",
            "account.setInfo",
            "account.setNameInMenu",
            "account.setOffline",
            "account.setOnline",
            "account.setPushSettings",
            "account.setSilenceMode",
            "account.unban",
            "account.unregisterDevice",
            "appWidgets.getAppImageUploadServer",
            "appWidgets.getAppImages",
            "appWidgets.getGroupImageUploadServer",
            "appWidgets.getGroupImages",
            "appWidgets.getImagesById",
            "appWidgets.saveAppImage",
            "appWidgets.saveGroupImage",
            "appWidgets.update",
            "apps.deleteAppRequests",
            "apps.get",
            "apps.getCatalog",
            "apps.getFriendsList",
            "apps.getLeaderboard",
            "apps.getScopes",
            "apps.getScore",
            "apps.promoHasActiveGift",
            "apps.promoUseGift",
            "apps.sendRequest",
            "auth.checkPhone",
            "auth.restore",
            "board.addTopic",
            "board.closeTopic",
            "board.createComment",
            "board.deleteComment",
            "board.deleteTopic",
            "board.editComment",
            "board.editTopic",
            "board.fixTopic",
            "board.getComments",
            "board.getTopics",
            "board.openTopic",
            "board.restoreComment",
            "board.unfixTopic",
            "database.getChairs",
            "database.getCities",
            "database.getCitiesById",
            "database.getCountries",
            "database.getCountriesById",
            "database.getFaculties",
            "database.getMetroStations",
            "database.getMetroStationsById",
            "database.getRegions",
            "database.getSchoolClasses",
            "database.getSchools",
            "database.getUniversities",
            "docs.add",
            "docs.delete",
            "docs.edit",
            "docs.get",
            "docs.getById",
            "docs.getMessagesUploadServer",
            "docs.getTypes",
            "docs.getUploadServer",
            "docs.getWallUploadServer",
            "docs.save",
            "docs.search",
            "execute",
            "fave.addArticle",
            "fave.addLink",
            "fave.addPage",
            "fave.addPost",
            "fave.addProduct",
            "fave.addTag",
            "fave.addVideo",
            "fave.editTag",
            "fave.get",
            "fave.getPages",
            "fave.getTags",
            "fave.markSeen",
            "fave.removeArticle",
            "fave.removeLink",
            "fave.removePage",
            "fave.removePost",
            "fave.removeProduct",
            "fave.removeTag",
            "fave.removeVideo",
            "fave.reorderTags",
            "fave.setPageTags",
            "fave.setTags",
            "fave.trackPageInteraction",
            "friends.add",
            "friends.addList",
            "friends.areFriends",
            "friends.delete",
            "friends.deleteAllRequests",
            "friends.deleteList",
            "friends.edit",
            "friends.editList",
            "friends.get",
            "friends.getAppUsers",
            "friends.getByPhones",
            "friends.getLists",
            "friends.getMutual",
            "friends.getOnline",
            "friends.getRecent",
            "friends.getRequests",
            "friends.getSuggestions",
            "friends.search",
            "gifts.get",
            "groups.addAddress",
            "groups.addCallbackServer",
            "groups.addLink",
            "groups.approveRequest",
            "groups.ban",
            "groups.create",
            "groups.deleteAddress",
            "groups.deleteCallbackServer",
            "groups.deleteLink",
            "groups.disableOnline",
            "groups.edit",
            "groups.editAddress",
            "groups.editCallbackServer",
            "groups.editLink",
            "groups.editManager",
            "groups.enableOnline",
            "groups.get",
            "groups.getAddresses",
            "groups.getBanned",
            "groups.getById",
            "groups.getCallbackConfirmationCode",
            "groups.getCallbackServers",
            "groups.getCallbackSettings",
            "groups.getCatalog",
            "groups.getCatalogInfo",
            "groups.getInvitedUsers",
            "groups.getInvites",
            "groups.getLongPollServer",
            "groups.getLongPollSettings",
            "groups.getMembers",
            "groups.getOnlineStatus",
            "groups.getRequests",
            "groups.getSettings",
            "groups.getTokenPermissions",
            "groups.invite",
            "groups.isMember",
            "groups.join",
            "groups.leave",
            "groups.removeUser",
            "groups.reorderLink",
            "groups.search",
            "groups.setCallbackSettings",
            "groups.setLongPollSettings",
            "groups.setSettings",
            "groups.unban",
            "leadForms.create",
            "leadForms.delete",
            "leadForms.get",
            "leadForms.getLeads",
            "leadForms.getUploadURL",
            "leadForms.list",
            "leadForms.update",
            "likes.add",
            "likes.delete",
            "likes.getList",
            "likes.isLiked",
            "market.add",
            "market.addAlbum",
            "market.addToAlbum",
            "market.createComment",
            "market.delete",
            "market.deleteAlbum",
            "market.deleteComment",
            "market.edit",
            "market.editAlbum",
            "market.editComment",
            "market.get",
            "market.getAlbumById",
            "market.getAlbums",
            "market.getById",
            "market.getCategories",
            "market.getComments",
            "market.removeFromAlbum",
            "market.reorderAlbums",
            "market.reorderItems",
            "market.report",
            "market.reportComment",
            "market.restore",
            "market.restoreComment",
            "market.search",
            "messages.addChatUser",
            "messages.allowMessagesFromGroup",
            "messages.createChat",
            "messages.delete",
            "messages.deleteChatPhoto",
            "messages.deleteConversation",
            "messages.denyMessagesFromGroup",
            "messages.edit",
            "messages.editChat",
            "messages.getByConversationMessageId",
            "messages.getById",
            "messages.getChat",
            "messages.getChatPreview",
            "messages.getConversationMembers",
            "messages.getConversations",
            "messages.getConversationsById",
            "messages.getHistory",
            "messages.getHistoryAttachments",
            "messages.getImportantMessages",
            "messages.getInviteLink",
            "messages.getLastActivity",
            "messages.getLongPollHistory",
            "messages.getLongPollServer",
            "messages.isMessagesFromGroupAllowed",
            "messages.joinChatByInviteLink",
            "messages.markAsAnsweredConversation",
            "messages.markAsImportant",
            "messages.markAsImportantConversation",
            "messages.markAsRead",
            "messages.pin",
            "messages.removeChatUser",
            "messages.restore",
            "messages.search",
            "messages.searchConversations",
            "messages.send",
            "messages.setActivity",
            "messages.setChatPhoto",
            "messages.unpin",
            "newsfeed.addBan",
            "newsfeed.deleteBan",
            "newsfeed.deleteList",
            "newsfeed.get",
            "newsfeed.getBanned",
            "newsfeed.getComments",
            "newsfeed.getLists",
            "newsfeed.getMentions",
            "newsfeed.getRecommended",
            "newsfeed.getSuggestedSources",
            "newsfeed.ignoreItem",
            "newsfeed.saveList",
            "newsfeed.search",
            "newsfeed.unignoreItem",
            "newsfeed.unsubscribe",
            "notes.add",
            "notes.createComment",
            "notes.delete",
            "notes.deleteComment",
            "notes.edit",
            "notes.editComment",
            "notes.get",
            "notes.getById",
            "notes.getComments",
            "notes.restoreComment",
            "notifications.get",
            "notifications.markAsViewed",
            "notifications.sendMessage",
            "pages.clearCache",
            "pages.get",
            "pages.getHistory",
            "pages.getTitles",
            "pages.getVersion",
            "pages.parseWiki",
            "pages.save",
            "pages.saveAccess",
            "photos.confirmTag",
            "photos.copy",
            "photos.createAlbum",
            "photos.createComment",
            "photos.delete",
            "photos.deleteAlbum",
            "photos.deleteComment",
            "photos.edit",
            "photos.editAlbum",
            "photos.editComment",
            "photos.get",
            "photos.getAlbums",
            "photos.getAlbumsCount",
            "photos.getAll",
            "photos.getAllComments",
            "photos.getById",
            "photos.getChatUploadServer",
            "photos.getComments",
            "photos.getMarketAlbumUploadServer",
            "photos.getMarketUploadServer",
            "photos.getMessagesUploadServer",
            "photos.getNewTags",
            "photos.getOwnerCoverPhotoUploadServer",
            "photos.getOwnerPhotoUploadServer",
            "photos.getTags",
            "photos.getUploadServer",
            "photos.getUserPhotos",
            "photos.getWallUploadServer",
            "photos.makeCover",
            "photos.move",
            "photos.putTag",
            "photos.removeTag",
            "photos.reorderAlbums",
            "photos.reorderPhotos",
            "photos.report",
            "photos.reportComment",
            "photos.restore",
            "photos.restoreComment",
            "photos.save",
            "photos.saveMarketAlbumPhoto",
            "photos.saveMarketPhoto",
            "photos.saveMessagesPhoto",
            "photos.saveOwnerCoverPhoto",
            "photos.saveOwnerPhoto",
            "photos.saveWallPhoto",
            "photos.search",
            "podcasts.clearRecentSearches",
            "podcasts.getPopular",
            "podcasts.getRecentSearchRequests",
            "podcasts.search",
            "polls.addVote",
            "polls.create",
            "polls.deleteVote",
            "polls.edit",
            "polls.getBackgrounds",
            "polls.getById",
            "polls.getPhotoUploadServer",
            "polls.getVoters",
            "polls.savePhoto",
            "prettyCards.create",
            "prettyCards.delete",
            "prettyCards.edit",
            "prettyCards.get",
            "prettyCards.getById",
            "prettyCards.getUploadURL",
            "search.getHints",
            "stats.get",
            "stats.getPostReach",
            "stats.trackVisitor",
            "status.get",
            "status.set",
            "storage.get",
            "storage.getKeys",
            "storage.set",
            "stories.banOwner",
            "stories.delete",
            "stories.get",
            "stories.getBanned",
            "stories.getById",
            "stories.getPhotoUploadServer",
            "stories.getReplies",
            "stories.getStats",
            "stories.getVideoUploadServer",
            "stories.getViewers",
            "stories.hideAllReplies",
            "stories.hideReply",
            "stories.search",
            "stories.sendInteraction",
            "stories.unbanOwner",
            "streaming.getServerUrl",
            "streaming.getSettings",
            "streaming.getStats",
            "streaming.getStem",
            "streaming.setSettings",
            "users.get",
            "users.getFollowers",
            "users.getSubscriptions",
            "users.report",
            "users.search",
            "utils.checkLink",
            "utils.deleteFromLastShortened",
            "utils.getLastShortenedLinks",
            "utils.getLinkStats",
            "utils.getServerTime",
            "utils.getShortLink",
            "utils.resolveScreenName",
            "video.add",
            "video.addAlbum",
            "video.addToAlbum",
            "video.createComment",
            "video.delete",
            "video.deleteAlbum",
            "video.deleteComment",
            "video.edit",
            "video.editAlbum",
            "video.editComment",
            "video.get",
            "video.getAlbumById",
            "video.getAlbums",
            "video.getAlbumsByVideo",
            "video.getComments",
            "video.removeFromAlbum",
            "video.reorderAlbums",
            "video.reorderVideos",
            "video.report",
            "video.reportComment",
            "video.restore",
            "video.restoreComment",
            "video.save",
            "video.search",
            "wall.checkCopyrightLink",
            "wall.closeComments",
            "wall.createComment",
            "wall.delete",
            "wall.deleteComment",
            "wall.edit",
            "wall.editAdsStealth",
            "wall.editComment",
            "wall.get",
            "wall.getById",
            "wall.getComment",
            "wall.getComments",
            "wall.getReposts",
            "wall.openComments",
            "wall.pin",
            "wall.post",
            "wall.postAdsStealth",
            "wall.reportComment",
            "wall.reportPost",
            "wall.repost",
            "wall.restore",
            "wall.restoreComment",
            "wall.search",
            "wall.unpin",
            "widgets.getComments",
            "widgets.getPages"
        };
    }
}
