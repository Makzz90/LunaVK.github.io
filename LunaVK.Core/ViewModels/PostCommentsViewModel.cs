using System;
using System.Collections.Generic;
using System.Text;
using LunaVK.Core.DataObjects;
using System.Collections.ObjectModel;
using LunaVK.Core.Library;
using System.Threading.Tasks;
using LunaVK.Core.Network;
using LunaVK.Core.Enums;
using System.Linq;
using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using Newtonsoft.Json;
using LunaVK.Core.Json;

namespace LunaVK.Core.ViewModels
{
    //LikesItem внедряет в страницу
    public class PostCommentsViewModel : ViewModelBase, ISupportUpDownIncrementalLoading
    {
        public ObservableCollection<VKComment> Items { get; private set; }
        public VKBaseDataForPostOrNews WallPostData { get; private set; }
//        private int offset = 0;
//        public int offsetFoPrev { get; private set; }
        uint _postId;
        int _ownerId;
        uint _commentId;
        public Action<ProfileLoadingStatus> LoadingStatusUpdated;
        public int RealOffset;
        public List<VKUser> Likes { get; private set; }//тоже нужно!
        public ObservableCollection<IOutboundAttachment> Attachments { get; set; }
        private Action<VKComment> _replyCommentAction;

        private int real_offset_UP;
        private int real_offset_DOWN;
        private int UpLoaded = 0;
        private int DownLoaded = 0;
        private bool NeedOffsetForDown;

        private readonly byte NumberToLoad = 10;

        public string TextWatermarkText
        {
            get { return LocalizedStrings.GetString("Comment"); }
        }

        /// <summary>
        /// Всего коментариев
        /// </summary>
        public uint Count;
        

        public async Task LoadUpAsync()
        {
            throw new NotImplementedException();
        }

        public bool HasMoreUpItems
        {
            get { return false; }
        }

        public bool HasMoreDownItems
        {
            get
            {
                if (this.Items.Count == 0)
                    return true;

                if (_commentId > 0)
                {
                    int val = real_offset_DOWN == 0 ? real_offset_UP : real_offset_DOWN;
                    return val > 0;
                }

                return this.Count - this.Items.Count > 0;
            }
        }

        public PostCommentsViewModel(uint postId, int ownerId, uint commentId, Action<VKComment> ReplyCommentAction, VKBaseDataForPostOrNews postData = null)
        {
            this.Items = new ObservableCollection<VKComment>();
            this._postId=postId;
            this._ownerId = ownerId;
            this._commentId = commentId;
            this.WallPostData = postData;
            this.Attachments = new ObservableCollection<IOutboundAttachment>();
            this._replyCommentAction = ReplyCommentAction;
        }
        
        public async Task<object> Reload()
        {
//            this.offset = 0;
//            this.offsetFoPrev = 0;

            this.Items.Clear();
            this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Reloading);
            await LoadDownAsync(true);
            return null;
        }

        public async Task LoadDownAsync(bool InReload = false)
        {
            if (!InReload)
            {
                this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Loading);
            }


            int offs = DownLoaded;
            if (NeedOffsetForDown)
                offs += this.NumberToLoad;
            string code = "var comments = API.wall.getComments({post_id:" + this._postId + ", owner_id:" + this._ownerId + ", offset:" + offs + ", count:20, thread_items_count:1, extended:1, need_likes:1";
            if(this._commentId!=0)
            {
                code += ", start_comment_id:" + this._commentId;
            }
            code += ", fields:\"last_name_dat,first_name_dat,photo_50\", sort:\"desc\"";
            code += "});";

            code += "var likesAll=[]; var datUsersNames = comments.items@.reply_to_user + comments.items@.from_id;var users2 = API.users.get({user_ids:datUsersNames, fields:\"first_name_dat,last_name_dat\"});";
            code += "comments.profiles = comments.profiles + users2;";

            if (this.WallPostData == null)
            {
                code += "var wallPosts = API.wall.getById({posts:\"" + this._ownerId + "_" + this._postId + "\",extended:1});";
                code += "comments.wall_post = wallPosts.items[0];";
                code += "comments.profiles = comments.profiles + wallPosts.profiles;";
                code += "comments.groups = comments.groups + wallPosts.groups;";
            }

            if(InReload)
                code += "likesAll = API.likes.getList({item_id:" + this._postId + ", owner_id:" + this._ownerId + ", count:20, extended:1,fields:\"photo_50\", type:\"post\"});";

            code += "return {comments:comments,likes:likesAll};";

            var temp = await RequestsDispatcher.Execute<GetWallPostResponseData>(code, (jsonStr) =>
                {
                    //jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Users2");
                    //jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Groups");
                    //jsonStr = VKRequestsDispatcher.FixArrayToObject(jsonStr, "likes");
                    jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "items");
                    return jsonStr;
                }
            );

            if (temp.error.error_code!= VKErrors.None)
            {
                this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.LoadingFailed);
                return;
            }

            if (this.WallPostData == null)
            {
                this.WallPostData = temp.response.wall_post;

                VKBaseDataForGroupOrUser owner = null;

                if(this.WallPostData.OwnerId<0)
                    owner = temp.response.comments.groups.Find((pro) => pro.id == (-this.WallPostData.OwnerId));
                else
                    owner = temp.response.comments.profiles.Find((pro) => pro.id == (this.WallPostData.OwnerId));
                this.WallPostData.Owner = owner;
            }

            if (InReload)
            {
                this.Likes = temp.response.likes.items;
                //this.WallPostData.likes.users = temp.response.likes.items;
                base.NotifyPropertyChanged("WallPostData");
            }

            this.RealOffset = temp.response.comments.real_offset;
            this.Count = temp.response.comments.count;
            
            UsersService.Instance.SetCachedUsers(temp.response.comments.profiles);//для подписчика поста нужно
//            UsersService.Instance.SetCachedUsers(temp2.response.groups);

            //if (temp2.response.WallPost!=null)
            //    UsersService.Instance.SetCachedUsers(temp2.response.WallPost.profiles);
            
            foreach (VKComment c in temp.response.comments.items)
            {
                if(c.reply_to_user!=0)
                {
                    if (c.reply_to_user < 0)
                    {
                        VKGroup g = temp.response.comments.groups.Find((group) => group.id == -c.reply_to_user);
                        c._replyToUserDat = g.Title;
                    }
                    else
                    {
                        VKUser r = temp.response.comments.profiles.Find((user) => user.id == c.reply_to_user);
                        c._replyToUserDat = r.first_name_dat + " " + r.last_name_dat;
                    }
                }

                VKBaseDataForGroupOrUser owner = null;

                if(c.from_id < 0)
                    owner = temp.response.comments.groups.Find((pro) => pro.id == (-c.from_id));
                else
                    owner = temp.response.comments.profiles.Find((pro) => pro.id == (c.from_id));

                if(owner == null)
                {
                    owner = new VKUser() { photo_50 = "https://vk.com/images/wall/deleted_avatar_50.png", first_name = "Комментарий удалён пользователем или руководителем страницы" };
                }

                c.User = owner;
                
                c.ReplyTapped = () => { this.ReplyToComment(c); };
                c.DeleteTapped = () => { this.DeleteComment(c); };

                if(c.thread!=null && c.thread.count>0)
                {
                    VKBaseDataForGroupOrUser buttonOwner = null;

                    var thread_c = c.thread.items[0];
                    if (thread_c.from_id < 0)
                        owner = temp.response.comments.groups.Find((pro) => pro.id == (-thread_c.from_id));
                    else
                        owner = temp.response.comments.profiles.Find((pro) => pro.id == (thread_c.from_id));
                    thread_c.User = owner;
                    if (buttonOwner == null)
                        buttonOwner = owner;

                    if (c.thread.count == 1)
                    {
                        c.thread.items[0].ReplyTapped = () => { this.ReplyToComment(c); };
                        c.thread.items[0].DeleteTapped = () => { this.DeleteComment(c); };
                    }
                    else
                    {
                        c.Button = new VKComment.BottomButtonData() { TotalComments = c.thread.count, User = buttonOwner };
                        c.thread.items.Clear();
                        c.LoadMoreInThread = () => { this.LoadMoreInThread(c); };
                    }
                }

                c.Marked = this._commentId == c.id;

                this.Items.Add(c);
            }

            this.real_offset_DOWN = temp.response.comments.real_offset;
            this.DownLoaded += temp.response.comments.items.Count;
            this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Loaded);

//            this.offset += 20;
        }

        private async void LoadMoreInThread(VKComment commentItem)
        {
            if (commentItem.InLoadingThred == true)
                return;

            commentItem.InLoadingThred = true;
            commentItem.InLoadingMore = true;
            commentItem.RefreshUI();
            
            string code = "var comments = API.wall.getComments({post_id:" + this._postId + ", owner_id:" + this._ownerId + ", offset:" + commentItem.thread.items.Count + ", count:20, extended:1, need_likes:1, fields:\"last_name_dat,first_name_dat,photo_50\", comment_id:" + commentItem.id + "});";

            code += "var datUsersNames = comments.items@.reply_to_user + comments.items@.from_id;var users2 = API.users.get({user_ids:datUsersNames, fields:\"first_name_dat,last_name_dat\"});";
            code += "comments.profiles = comments.profiles + users2;";
            
            

            code += "return comments;";

            var temp = await RequestsDispatcher.Execute<GetWallPostResponseData>(code, (jsonStr) =>
            {
                //jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Users2");
                //jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Groups");
                //jsonStr = VKRequestsDispatcher.FixArrayToObject(jsonStr, "likes");
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "items");
                return jsonStr;
            }
            );
            
            commentItem.InLoadingThred = false;
            commentItem.InLoadingMore = false;

            if (temp.error.error_code != VKErrors.None)
            {
                commentItem.RefreshUI();
                this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.LoadingFailed);
                return;
            }

            commentItem.Button = null;

            foreach (VKComment c in temp.response.comments.items)
            {
                if (c.reply_to_user != 0)
                {
                    if (c.reply_to_user < 0)
                    {
                        VKGroup g = temp.response.comments.groups.Find((group) => group.id == -c.reply_to_user);
                        c._replyToUserDat = g.Title;
                    }
                    else
                    {
                        VKUser r = temp.response.comments.profiles.Find((user) => user.id == c.reply_to_user);
                        c._replyToUserDat = r.first_name_dat + " " + r.last_name_dat;
                    }
                }

                VKBaseDataForGroupOrUser owner = null;

                if (c.from_id < 0)
                    owner = temp.response.comments.groups.Find((pro) => pro.id == (-c.from_id));
                else
                    owner = temp.response.comments.profiles.Find((pro) => pro.id == (c.from_id));

                c.User = owner;

                //c.owner_id = (int)this._ownerId;
                c.ReplyTapped = () => { this.ReplyToComment(c); };
                c.DeleteTapped = () => { this.DeleteComment(c); };
                
                commentItem.thread.items.Add(c);
            }
            
            commentItem.RefreshUI();
        }

        public void ReplyToComment(VKComment commentItem)
        {
            this._replyCommentAction(commentItem);
        }

        public async void DeleteComment(VKComment commentItem)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = this._ownerId.ToString();
            parameters["comment_id"] = commentItem.id.ToString();

            VKResponse<int> temp = await RequestsDispatcher.GetResponse<int>("wall.deleteComment", parameters);

            if (temp.error.error_code == VKErrors.None)
            {
                if(this.Items.Contains(commentItem))
                    this.Items.Remove(commentItem);
                else
                {
                    foreach(var comment in this.Items)
                    {
                        foreach(var t in comment.thread.items)
                        {
                            if (t == commentItem)
                            {
                                comment.thread.items.Remove(commentItem);
                                return;
                            }
                        }
                    }
                }
            }
        }

        public async void PostComment(string text, uint replyCommentId = 0, bool fromGroup = false, List<IOutboundAttachment> attachments = null, Action<bool> callback = null, int sticker_id = 0)
        {
            VKComment comment = new VKComment();
            comment.text = text;
            comment.from_id = (int)Settings.UserId;
            comment.likes = new VKLikes();
            comment.date = DateTime.Now;
            comment.post_id = replyCommentId;
            comment.User = UsersService.Instance.GetCachedUser(Settings.UserId);
            comment.DeleteTapped = () => { this.DeleteComment(comment); };
            //
            if (sticker_id != 0)
            {
                VKSticker sticker = new VKSticker() { sticker_id = sticker_id };
                sticker.SetImage("https://vk.com/images/stickers/" + sticker_id + "/256b.png");

                VKAttachment at = new VKAttachment() { sticker = sticker, type = VKAttachmentType.Sticker };

                List<VKAttachment> temp_msg_attachments = new List<VKAttachment>();
                temp_msg_attachments.Add(at);
                comment.attachments = temp_msg_attachments;
            }

            //
            
            string code = "var new_comment_id = API.wall.createComment({";

            code += ("owner_id:"+ this._ownerId);
            code += (",post_id:" + this._postId);
            code += (",message:\"" + text + "\"");

            if (replyCommentId != 0)
                code += (",reply_to_comment:" + replyCommentId);

            if (fromGroup == true)
                code += (",from_group:" + this._ownerId);
            
            if (sticker_id != 0)
                code += (",sticker_id:" + sticker_id);

            if (attachments != null && attachments.Count > 0)
            {
                var att = attachments.Select<IOutboundAttachment, string>((a => a.ToString())).ToList();
                code += (",attachments:\"" + att.GetCommaSeparated() + "\"");
            }

            code += "});";

            if (replyCommentId != 0)
            {
                uint comment_id = replyCommentId;

                foreach(var item in this.Items)
                {
                    if (item.id == replyCommentId)
                        break;

                    foreach(var thread_item in item.thread.items)
                    {
                        if (thread_item.id == replyCommentId)
                        {
                            comment_id = (uint)thread_item.parents_stack[0];
                            break;
                        }
                    }
                }

                code += "var last_comments = API.wall.getComments({post_id:" + this._postId + ", owner_id:" + this._ownerId + ", count:20, extended:1, sort:\"desc\", comment_id:"+ comment_id+"});";
            }
            else
                code += "var last_comments = API.wall.getComments({post_id:" + this._postId + ", owner_id:" + this._ownerId + ", count:1, thread_items_count:1, extended:1, start_comment_id:new_comment_id.comment_id});";

            code += "return last_comments;";

            var temp = await RequestsDispatcher.Execute<GetWallPostResponseData>(code);
            if (temp.error.error_code == VKErrors.None)
            {
                this.RealOffset = temp.response.comments.real_offset;
                this.Count = temp.response.comments.count;

                VKComment replyComment = null;

                if (replyCommentId != 0)
                {
                    replyComment = this.Items.FirstOrDefault((c) => c.id == replyCommentId);
                    if (replyComment != null)
                    {
                        //if (replyComment.thread == null)
                            replyComment.thread = new VKComment.Thread();
                        replyComment.thread.items = new ObservableCollection<VKComment>();


                    }
                }


                foreach (VKComment c in temp.response.comments.items)
                {
                    if (c.reply_to_user != 0)
                    {
                        if (c.reply_to_user < 0)
                        {
                            VKGroup g = temp.response.comments.groups.Find((group) => group.id == -c.reply_to_user);
                            c._replyToUserDat = g.Title;
                        }
                        else
                        {
                            VKUser r = temp.response.comments.profiles.Find((user) => user.id == c.reply_to_user);
                            c._replyToUserDat = r.first_name_dat + " " + r.last_name_dat;
                        }
                    }

                    VKBaseDataForGroupOrUser owner = null;

                    if (c.from_id < 0)
                        owner = temp.response.comments.groups.Find((pro) => pro.id == (-c.from_id));
                    else
                        owner = temp.response.comments.profiles.Find((pro) => pro.id == (c.from_id));

                    c.User = owner;

                    //c.owner_id = (int)this._ownerId;
                    c.ReplyTapped = () => { this.ReplyToComment(c); };
                    c.DeleteTapped = () => { this.DeleteComment(c); };

                    if (c.thread != null && c.thread.count > 0)
                    {
                        VKBaseDataForGroupOrUser buttonOwner = null;

                        var thread_c = c.thread.items[0];
                        if (thread_c.from_id < 0)
                            owner = temp.response.comments.groups.Find((pro) => pro.id == (-thread_c.from_id));
                        else
                            owner = temp.response.comments.profiles.Find((pro) => pro.id == (thread_c.from_id));
                        thread_c.User = owner;
                        if (buttonOwner == null)
                            buttonOwner = owner;

                        if (c.thread.count == 1)
                        {
                            c.thread.items[0].ReplyTapped = () => { this.ReplyToComment(c); };
                            c.thread.items[0].DeleteTapped = () => { this.DeleteComment(c); };
                        }
                        else
                        {
                            c.Button = new VKComment.BottomButtonData() { TotalComments = c.thread.count, User = buttonOwner };
                            c.thread.items.Clear();
                            c.LoadMoreInThread = () => { this.LoadMoreInThread(c); };
                        }
                    }

                    c.Marked = this._commentId == c.id;

                    if (replyCommentId != 0)
                    {
                        if (replyComment != null)
                        {
                            var temppp = replyComment.thread.items.FirstOrDefault((p) => p.id == c.id);
                            if (temppp == null)
                            {
                                replyComment.thread.count++;
                                replyComment.thread.items.Add(c);
                            }
                        }
                        else// if(c.id== replyCommentId)
                        {
                            int parent = c.parents_stack[0];
                            replyComment = this.Items.FirstOrDefault((p) => p.id == parent);
                            if (replyComment != null)
                            {
                                var temppp = replyComment.thread.items.FirstOrDefault((p) => p.id == c.id);
                                if (temppp == null)
                                {
                                    replyComment.thread.count++;
                                    replyComment.thread.items.Add(c);
                                }
                            }
                        }
                    }
                    else
                    {
                        this.Items.Insert(0, c);
                    }
                }

                if (replyComment != null)
                    replyComment.RefreshUI();
                callback?.Invoke(true);
            }
            else
            {
                callback?.Invoke(false);
            }
            /*
             * string.Format("\r\n\r\nvar new_comment_id = API.wall.addComment({{\r\n    owner_id: {0},
             * \r\n    post_id: {1},\r\n    text: \"{2}\",\r\n    from_group: {3},
             * \r\n    sticker_id: {4},\r\n    reply_to_comment: {5},\r\n    attachments: \"{6}\",
             * \r\n    sticker_referrer: \"{7}\"\r\n}}).comment_id;\r\n
             * \r\nvar last_comments = API.wall.getComments({{\r\n    owner_id: {8},
             * \r\n    post_id: {9},\r\n    need_likes: 1,\r\n    count: 10,
             * \r\n    sort: \"desc\",\r\n    preview_length: 0,
             * \r\n    allow_group_comments: 1\r\n}}).items;\r\n
             * \r\nvar i = last_comments.length - 1;\r\nwhile (i >= 0)\r
             * \n{{\r\n    if (last_comments[i].id == new_comment_id)\r\n        return last_comments[i];\r
             * \n\r\n    i = i - 1;\r\n}}\r\n\r\nreturn null;\r\n\r\n                ", 
             * 
             * ownerId, postId, text.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r"), (fromGroup ? "1" : "0"), stickerId, replyToCid, attachmentIds.GetCommaSeparated(","), stickerReferrer, ownerId, postId)
             * */
        }


        public bool CanPostComment(string text, List<IOutboundAttachment> attachments)
        {
            if ((string.IsNullOrWhiteSpace(text) || text.Length < 2))
            {
                if (attachments.Count != 0)
                {
                    bool result = attachments.Any(a => { return a.UploadState != OutboundAttachmentUploadState.Completed; });
                    if (!result)
                        return true;
                }
                return false;
            }
        
            return true;
        }

        /*
        public class TempComment
        {
            //{"response":{"comment_id":3971}}
            public uint comment_id { get; set; }
        }

        
        public class WallComments : VKCountedItemsObject<VKComment>
        {
            public int real_offset { get; set; }
        }

        public class WallPostClass
        {
            public List<VKWallPost> items { get; set; }

            public List<VKUser> profiles { get; set; }

            public List<VKGroup> groups { get; set; }
        }

        
        public class GetWallPostResponseData
        {
            public VKCountedItemsObject<VKUser> likes { get; set; }

            public WallPostClass WallPost { get; set; }

            public WallComments comments { get; set; }

            /// <summary>
            /// wallPost@.from_id + wallPost@.to_id + wallPost@.signer_id + wallPost[0].copy_history@.owner_id + wallPost[0].copy_history@.from_id
            /// </summary>
            public List<VKUser> Users { get; set; }
            public List<VKGroup> Groups { get; set; }

            
            /// <summary>
            /// datUsersNames
            /// </summary>
            public List<VKUser> Users2 { get; set; }
        }
        */

        public class GetWallPostResponseData
        {
            public Comments comments { get; set; }

            public VKWallPost wall_post { get; set; }

            public VKCountedItemsObject<VKUser> likes { get; set; }



            public class Comments : VKCountedItemsObject<VKComment>
            {
                public int real_offset { get; set; }

                /// <summary>
                /// количество комментариев в ветке. 
                /// </summary>
                public int current_level_count { get; set; }

                /// <summary>
                /// может ли текущий пользователь оставлять комментарии в этой ветке. 
                /// </summary>
                [JsonConverter(typeof(VKBooleanConverter))]
                public bool can_post { get; set; }

                /// <summary>
                /// нужно ли отображать кнопку «ответить» в ветке. 
                /// </summary>
                [JsonConverter(typeof(VKBooleanConverter))]
                public bool show_reply_button { get; set; }

                /// <summary>
                /// могут ли сообщества оставлять комментарии в ветке. 
                /// </summary>
                [JsonConverter(typeof(VKBooleanConverter))]
                public bool groups_can_post { get; set; }
            }
        }

        public void UploadAttachments()
        {
            //Execute.ExecuteOnUIThread((Action)(() =>
            //{
                IOutboundAttachment attachment = this.Attachments.FirstOrDefault((a => a.UploadState == OutboundAttachmentUploadState.NotStarted));
                if (attachment == null)
                    return;
                this.UploadAttachment(attachment, new Action(this.UploadAttachments));
            //}));
        }

        public void UploadAttachment(IOutboundAttachment attachment, Action callback = null)
        {
            if (attachment.UploadState == OutboundAttachmentUploadState.Completed)
                return;
            this.UpdateUploadingStatus(true);
            attachment.Upload((() => Execute.ExecuteOnUIThread((() =>
            {
                this.UpdateUploadingStatus(false);
                //base.NotifyPropertyChanged("CanPublish");
                if (callback == null)
                    return;
                callback();
            }))));
        }
        /*
        public bool CanPublish
        {
            get
            {
                ObservableCollection<IOutboundAttachment> outboundAttachments1 = this.Attachments;
                Func<IOutboundAttachment, bool> func1 = (Func<IOutboundAttachment, bool>)(a => a.AttachmentId != "timestamp");
                if (Enumerable.Any<IOutboundAttachment>(outboundAttachments1, (Func<IOutboundAttachment, bool>)func1) || !string.IsNullOrWhiteSpace(this._text))
                {
                    ObservableCollection<IOutboundAttachment> outboundAttachments2 = this.Attachments;
                    Func<IOutboundAttachment, bool> func3 = (Func<IOutboundAttachment, bool>)(a => a.UploadState == OutboundAttachmentUploadState.Completed);
                    if (Enumerable.All<IOutboundAttachment>(outboundAttachments2, (Func<IOutboundAttachment, bool>)func3) && this._mode != WallPostViewModel.Mode.NewTopic)
                        goto label_6;
                }
                if (this._mode != WallPostViewModel.Mode.EditWallPost || !this._editWallRepost)
                {
                    if (this._mode == WallPostViewModel.Mode.NewTopic && !string.IsNullOrWhiteSpace(this._topicTitle) && !string.IsNullOrWhiteSpace(this._text))
                        return Enumerable.All<IOutboundAttachment>(this.OutboundAttachments, (Func<IOutboundAttachment, bool>)(a => a.UploadState == OutboundAttachmentUploadState.Completed));
                    return false;
                }
            label_6:
                return true;
            }
        }
        */
        public void UpdateUploadingStatus(bool forceTrue = false)
        {
            //ObservableCollection<IOutboundAttachment> outboundAttachments = this.OutboundAttachments;
            //Func<IOutboundAttachment, bool> func1 = (Func<IOutboundAttachment, bool>)(a => a.UploadState != OutboundAttachmentUploadState.Uploading);
            //if (Enumerable.All<IOutboundAttachment>(outboundAttachments, (Func<IOutboundAttachment, bool>)func1) && !forceTrue)
            //    this.SetInProgress(false, "");
            //else
            //    this.SetInProgress(true, CommonResources.WallPost_UploadingAttachments);
        }
    }
}
/*
 * public enum LikeObjectType
  {
    post,
    comment,
    photo,
    audio,
    video,
    note,
    photo_comment,
    video_comment,
    topic_comment,
    post_ads,
    market,
    market_comment,
  }
 * */
