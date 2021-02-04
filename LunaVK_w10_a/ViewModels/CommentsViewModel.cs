using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using LunaVK.Library;
using LunaVK.UC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace LunaVK.ViewModels
{
    public class CommentsViewModel : GenericCollectionViewModel<VKComment>
    {
        public uint _itemId;
        public int _ownerId;
        private uint _commentId;
        private readonly string _accessKey = "";

        public List<VKUser> Likes { get; private set; }//тоже нужно!
        public ObservableCollection<IOutboundAttachment> Attachments { get; set; }
        private Action<VKComment> _replyCommentAction;

        public VKWallPost WallPostData { get; private set; }
        public VKPhoto Photo { get; private set; }

        public int RealOffset;
        private int real_offset_UP;
        private int real_offset_DOWN;
        private int UpLoaded = 0;
        private int DownLoaded = 0;
        private bool NeedOffsetForDown;

        /// <summary>
        /// Всего коментариев
        /// </summary>
        public uint Count;

        public Visibility PostVisibility
        {
            get
            {
                return (this.Photo == null ).ToVisiblity();
            }
        }

        public Visibility PhotoVisibility
        {
            get { return (this.Photo != null ).ToVisiblity(); }
        }

        public Visibility ContentVisibility
        {
            get
            {
                return (base.CurrentLoadingStatus == ProfileLoadingStatus.Loaded || base.CurrentLoadingStatus == ProfileLoadingStatus.Empty).ToVisiblity();
            }
        }

        public string TextWatermarkText
        {
            get { return LocalizedStrings.GetString("Comment"); }
        }

        public Visibility BotKeyboardVisibility
        {
            get { return Visibility.Collapsed; }
        }

        public object BotKeyboardButtons { get; private set; }

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

        //public PostCommentsViewModel(NewsItemDataWithUsersAndGroupsInfo wallPostData, long postId, long ownerId, MyVirtualizingPanel2 panel, Action loadedCallback, Action<CommentItem> replyCommentAction, long knownPollId, long knownPollOwnerId)
        public CommentsViewModel(int ownerId, uint postId, uint commentId, Action<VKComment> replyCommentAction, VKWallPost postData = null) : this(ownerId, postId, replyCommentAction)
        {
            this._commentId = commentId;
            this.WallPostData = postData;
        }

        public CommentsViewModel(VKPhoto photo, Action<VKComment> replyCommentAction) : this(photo.owner_id, photo.id, replyCommentAction)
        {
            this.Photo = photo;
            this._accessKey = photo.access_key;
        }

        private CommentsViewModel(int ownerId, uint itemId, Action<VKComment> ReplyCommentAction)
        {
            this._ownerId = ownerId;
            this._itemId = itemId;
            this.Attachments = new ObservableCollection<IOutboundAttachment>();
            this._replyCommentAction = ReplyCommentAction;
        }




        public override bool HasMoreDownItems
        {
            get
            {
                if (this._totalCount.HasValue == false)
                    return true;

                //if (this.Items.Count == 0)
                //return true;

                if (_commentId > 0)
                {
                    int val = real_offset_DOWN == 0 ? real_offset_UP : real_offset_DOWN;
                    return val > 0;
                }

                return this.Count - this.DownLoaded/*this.Items.Count*/ > 0;
            }
        }


        public override void OnRefresh()
        {
            base.OnRefresh();
            //this.Photo = null;
            //this.WallPostData = null;
            //base.NotifyPropertyChanged(nameof(this.Photo));
            //base.NotifyPropertyChanged(nameof(this.WallPostData));
            base.NotifyPropertyChanged(nameof(this.PhotoVisibility));
            base.NotifyPropertyChanged(nameof(this.PostVisibility));
            
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKComment>> callback)
        {
            if(offset==0)
            {
                base.NotifyPropertyChanged(nameof(this.ContentVisibility));
            }

            if (this.Photo == null)
            {
                WallService.Instance.GetWallPostByIdWithComments(this._ownerId, this._itemId, offset, 0, offset == 0, (result) =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        if (result.response == null)//пост удалён
                        {
                            callback(result.error, null);
                            Execute.ExecuteOnUIThread(() =>
                            {
                                MessageDialog dialog = new MessageDialog(LocalizedStrings.GetString("WallPostIsNotAvailable"), LocalizedStrings.GetString("Error_AccessDenied"));
                                var res = dialog.ShowAsync();
                                NavigatorImpl.GoBack();
                            });
                            return;
                        }

                        base._totalCount = result.response.comments.count;

                        if (offset == 0)
                        {
                            this.Likes = result.response.likes.items;
                            base.NotifyPropertyChanged(nameof(this.Likes));

                            this.WallPostData = result.response.wall_post;

                            base.NotifyPropertyChanged(nameof(this.WallPostData));

                            
                        }













                        foreach (VKComment c in result.response.comments.items)
                        {
                            c.ReplyTapped = () => { this.ReplyToComment(c); };
                            c.DeleteTapped = () => { this.DeleteComment(c); };

                            if (c.thread != null && c.thread.count > 0)
                            {
                                if (c.thread.count == 1)
                                {
                                    c.thread.items[0].ReplyTapped = () => { this.ReplyToComment(c); };
                                    c.thread.items[0].DeleteTapped = () => { this.DeleteComment(c); };
                                }
                                else
                                {
                                    //c.thread.items.Clear();
                                    c.LoadMoreInThread = () => { this.LoadMoreInThread(c); };
                                }

                                //
                                //
                                //
                                this.DownLoaded += c.thread.count;
                            }

                            c.Marked = this._commentId == c.id;
                        }

                        this.RealOffset = result.response.comments.real_offset;
                        this.Count = result.response.comments.count;
                        this.real_offset_DOWN = result.response.comments.real_offset;
                        this.DownLoaded += result.response.comments.items.Count;

                        

                        callback(result.error, result.response.comments.items);

                        if (offset == 0)
                        {
                            base.NotifyPropertyChanged(nameof(this.LikesCount));
                            base.NotifyPropertyChanged(nameof(this.RepostsCount));
                            base.NotifyPropertyChanged(nameof(this.ContentVisibility));
                        }
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }
            else
            {
                PhotosService.Instance.GetPhotoWithFullInfo(this._ownerId, this._itemId, this._accessKey, offset, (result) =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.Comments == null ? 0 : result.response.Comments.count;

                        this.Photo = result.response.Photo;
                        this.NotifyPropertyChanged("Photo");
                        base.NotifyPropertyChanged("LikesCount");
                        base.NotifyPropertyChanged("RepostsCount");


                        callback(result.error, result.response.Comments == null ? null : result.response.Comments.items);

                        if (offset == 0)
                        {
                            this.Likes = result.response.LikesAllIds.items;
                            base.NotifyPropertyChanged("Likes");

                            if (this.Photo.owner_id > 0)
                                this.Owner = result.response.Users.FirstOrDefault((u) => u.id == this.Photo.owner_id);
                            else
                                this.Owner = result.response.Groups.FirstOrDefault((g) => g.id == (-this.Photo.owner_id));
                            base.NotifyPropertyChanged("Owner");

                            base.NotifyPropertyChanged(nameof(this.PhotoVisibility));
                            base.NotifyPropertyChanged(nameof(this.PostVisibility));

                            base.NotifyPropertyChanged(nameof(this.ContentVisibility));
                        }
                    }
                });
            }
        }

        public VKBaseDataForGroupOrUser Owner { get; set; }

        public void ReplyToComment(VKComment commentItem)
        {
            this._replyCommentAction(commentItem);
        }

        private async void LoadMoreInThread(VKComment commentItem)
        {
            if (commentItem.InLoadingThred == true)
                return;

            commentItem.InLoadingThred = true;
            commentItem.InLoadingMore = true;
            commentItem.RefreshUI();

            string code = "var comments = API.wall.getComments({post_id:" + this._itemId + ", owner_id:" + this._ownerId + ", offset:" + commentItem.thread.items.Count + ", count:20, extended:1, need_likes:1, fields:\"last_name_dat,first_name_dat,photo_50\", comment_id:" + commentItem.id + "});";

            code += "var datUsersNames = comments.items@.reply_to_user + comments.items@.from_id;var users2 = API.users.get({user_ids:datUsersNames, fields:\"first_name_dat,last_name_dat\"});";
            code += "comments.profiles = comments.profiles + users2;";



            code += "return comments;";

            VKRequestsDispatcher.Execute<VKCountedItemsObject<VKComment>>(code, (result) =>
            {


                commentItem.InLoadingThred = false;
                commentItem.InLoadingMore = false;

                if (result.error.error_code != VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        commentItem.RefreshUI();
                        this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.LoadingFailed);
                    });
                    return;
                }

                commentItem.Button = null;

                foreach (VKComment c in result.response.items)
                {
                    if (c.reply_to_user != 0)
                    {
                        if (c.reply_to_user < 0)
                        {
                            VKGroup g = result.response.groups.Find((group) => group.id == -c.reply_to_user);
                            c._replyToUserDat = g.Title;
                        }
                        else
                        {
                            VKUser r = result.response.profiles.Find((user) => user.id == c.reply_to_user);
                            c._replyToUserDat = r.first_name_dat + " " + r.last_name_dat;
                        }
                    }

                    VKBaseDataForGroupOrUser owner = null;

                    if (c.from_id < 0)
                        owner = result.response.groups.Find((pro) => pro.id == (-c.from_id));
                    else
                        owner = result.response.profiles.Find((pro) => pro.id == (c.from_id));

                    c.User = owner;
                    Execute.ExecuteOnUIThread(() =>
                    {
                        //c.owner_id = (int)this._ownerId;
                        c.ReplyTapped = () => { this.ReplyToComment(c); };
                        c.DeleteTapped = () => { this.DeleteComment(c); };


                        commentItem.thread.items.Add(c);
                    });
                }
                Execute.ExecuteOnUIThread(() =>
                {
                    commentItem.RefreshUI();
                });


            }, (jsonStr) =>
            {
                //jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Users2");
                //jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Groups");
                //jsonStr = VKRequestsDispatcher.FixArrayToObject(jsonStr, "likes");
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "items");
                return jsonStr;
            }
            );


        }

        //public void AddComment(VKComment comment, List<string> attachmentIds, bool fromGroup, Action<bool> callback, string stickerReferrer = "")
        public void AddComment(string text, uint replyCommentId = 0, bool fromGroup = false, Action<bool> callback = null, uint sticker_id = 0)
        {
            var attachmentIds = this.Attachments.Select<IOutboundAttachment, string>((a => a.ToString())).ToList();
            //if (this._adding)
            //{
            //    callback.Invoke(false, null);
            //}
            //else
            //{
            //    this._adding = true;
            if (this.Photo != null)
            {
                PhotosService.Instance.CreateComment(this._ownerId, this._itemId, replyCommentId, text, fromGroup, attachmentIds, (res =>
                {
                    if (res.error.error_code == VKErrors.None)
                    {
                        this.Photo.comments.count++;
                        res.response.CanDelete = true;
                        Execute.ExecuteOnUIThread(() =>
                        {
                            this.Items.Add(res.response);
                        });
                        callback(true);
                    }
                    else
                        callback(false);
                    //this._adding = false;
                }), this._accessKey, sticker_id);
                //}
            }
            else
            {
                /*
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
                */
                //

                string code = "var new_comment_id = API.wall.createComment({owner_id:" + this._ownerId+ ",post_id:"+ this._itemId;
                
                if(!string.IsNullOrEmpty(text))
                    code += (",message:\"" + text + "\"");

                if (replyCommentId != 0)
                    code += (",reply_to_comment:" + replyCommentId);

                if (fromGroup == true)
                    code += (",from_group:" + (-this._ownerId));

                if (sticker_id != 0)
                    code += (",sticker_id:" + sticker_id);

                if (attachmentIds != null && attachmentIds.Count > 0)
                {
                    code += (",attachments:\"" + attachmentIds.GetCommaSeparated() + "\"");
                }

                code += "});";

                if (replyCommentId != 0)
                {
                    uint comment_id = replyCommentId;

                    foreach (var item in this.Items)
                    {
                        if (item.id == replyCommentId)
                            break;

                        foreach (var thread_item in item.thread.items)
                        {
                            if (thread_item.id == replyCommentId)
                            {
                                comment_id = (uint)thread_item.parents_stack[0];
                                break;
                            }
                        }
                    }

                    code += "var last_comments = API.wall.getComments({post_id:" + this._itemId + ", owner_id:" + this._ownerId + ", count:20, extended:1, sort:\"desc\", comment_id:" + comment_id + "});";
                }
                else
                    code += "var last_comments = API.wall.getComments({post_id:" + this._itemId + ", owner_id:" + this._ownerId + ", count:1, thread_items_count:1, extended:1, start_comment_id:new_comment_id.comment_id});";

                code += "return last_comments;";

                //code = System.Net.WebUtility.HtmlEncode(code);
                int i = 0;
                VKRequestsDispatcher.Execute<WallService.GetWallPostResponseData.Comments>(code, (result) =>
                 {
                     Execute.ExecuteOnUIThread(() =>
                     {
                         if (result.error.error_code != VKErrors.None)
                         {
                             GenericInfoUC.ShowBasedOnResult("", result.error);
                             callback?.Invoke(false);
                             return;
                         }

                         if (result.execute_errors != null && result.execute_errors.Count > 0)
                         {
                             GenericInfoUC.ShowBasedOnResult("", result.execute_errors[0]);
                             callback?.Invoke(false);
                             return;
                         }


                         this.RealOffset = result.response.real_offset;
                         this.Count = result.response.count;

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


                         foreach (VKComment c in result.response.items)
                         {
                             if (c.reply_to_user != 0)
                             {
                                 if (c.reply_to_user < 0)
                                 {
                                     VKGroup g = result.response.groups.Find((group) => group.id == -c.reply_to_user);
                                     c._replyToUserDat = g.Title;
                                 }
                                 else
                                 {
                                     VKUser r = result.response.profiles.Find((user) => user.id == c.reply_to_user);
                                     c._replyToUserDat = r.first_name_dat + " " + r.last_name_dat;
                                 }
                             }

                             VKBaseDataForGroupOrUser owner = null;

                             if (c.from_id < 0)
                                 owner = result.response.groups.Find((pro) => pro.id == (-c.from_id));
                             else
                                 owner = result.response.profiles.Find((pro) => pro.id == (c.from_id));

                             c.User = owner;

                            //c.owner_id = (int)this._ownerId;
                            c.ReplyTapped = () => { this.ReplyToComment(c); };
                             c.DeleteTapped = () => { this.DeleteComment(c); };

                             if (c.thread != null && c.thread.count > 0)
                             {
                                 VKBaseDataForGroupOrUser buttonOwner = null;

                                 var thread_c = c.thread.items[0];
                                 if (thread_c.from_id < 0)
                                     owner = result.response.groups.Find((pro) => pro.id == (-thread_c.from_id));
                                 else
                                     owner = result.response.profiles.Find((pro) => pro.id == (thread_c.from_id));
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
                             //
                             //var cachedGroup = result.response.groups.Find((g) => g.id == (-ownerId));
                             c.CanDelete = true;//c.from_id == Settings.UserId || c.owner_id == Settings.UserId;// || c.owner_id < 0 && cachedGroup != null && (c.from_id > 0 && cachedGroup.IsModeratorOrHigher == true || cachedGroup.IsEditorOrHigher);
                                                                                                        //
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
                                 base.Items.Insert(0, c);
                                 base._totalCount++;
                                 base.NotifyPropertyChanged(nameof(base.StatusText));
                             }
                         }

                         if (replyComment != null)
                             replyComment.RefreshUI();
                         callback?.Invoke(true);
                     });
                 });

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

            base.NotifyPropertyChanged(nameof(base.FooterText));
        }

        public void DeleteComment(VKComment comment)
        {
            if (this.Photo != null)
            {
                this.Photo.comments.count--;
                PhotosService.Instance.DeleteComment(this._ownerId, this._itemId, comment.id, (result) =>
                {
                    if (result.error.error_code == VKErrors.None && result.response == 1)
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            if (this.Items == null)
                                return;
                            this.Items.Remove(comment);
                        });

                        base._totalCount--;
                    }
                });
            }
            else
            {
                WallService.Instance.DeleteComment(this._ownerId, comment.id, (result) =>
                {
                    if (result.error.error_code == VKErrors.None && result.response == 1)
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            if (this.Items.Contains(comment))
                                this.Items.Remove(comment);
                            else
                            {
                                foreach (var c in this.Items)
                                {
                                    if (c.thread.items.Contains(comment))
                                    {
                                        c.thread.items.Remove(comment);
                                        break;
                                    }
                                }
                            }
                        });

                        base._totalCount--;
                    }
                });
            }

            base.NotifyPropertyChanged(nameof(base.FooterText));
        }

        public uint LikesCount
        {
            get
            {
                if (this.WallPostData != null && this.WallPostData.likes != null)
                    return this.WallPostData.likes.count;
                if (this.Photo != null && this.Photo.likes != null)
                    return this.Photo.likes.count;
                return 0;
            }
        }

        public uint RepostsCount
        {
            get
            {
                if (this.WallPostData != null && this.WallPostData.reposts != null)
                    return this.WallPostData.reposts.count;
                if (this.Photo != null && this.Photo.reposts != null)
                    return this.Photo.reposts.count;
                return 0;
            }
        }

        public override string GetFooterTextForCount
        {
            get
            {
                if (base._totalCount == 0)
                    return LocalizedStrings.GetString("PostCommentsPage_NoComments");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "PostCommentPage_OneCommentFrm", "PostCommentPage_TwoFourCommentsFrm", "PostCommentPage_FiveCommentsFrm");
            }
        }

        public bool IsLiked
        {
            get
            {
                if (this.WallPostData != null && this.WallPostData.likes != null)
                    return this.WallPostData.likes.user_likes;
                if (this.Photo != null && this.Photo.likes != null)
                    return this.Photo.likes.user_likes;
                return false;
            }
        }

        private bool _inSync;

        public void AddRemoveLike()
        {
            if (this._inSync)
                return;

            LikesService.Instance.AddRemoveLike(this.WallPostData.likes.user_likes == false, this._ownerId, this._itemId, /*vm.post_type*/ LikeObjectType.post, (result) =>
            {
                this._inSync = false;
                Execute.ExecuteOnUIThread(() =>
                {
                    if (result != -1)
                    {
                        this.WallPostData.likes.count = (uint)result;
                        this.WallPostData.likes.user_likes = !this.WallPostData.likes.user_likes;

                        base.NotifyPropertyChanged(nameof(this.LikesCount));
                        base.NotifyPropertyChanged(nameof(this.IsLiked));
                    }
                });
            });
        }

        public void UploadAttachments()
        {
            Execute.ExecuteOnUIThread(() =>
            {
                IOutboundAttachment attachment = this.Attachments.FirstOrDefault((a => a.UploadState == OutboundAttachmentUploadState.NotStarted));
                if (attachment == null)
                {
                    CanPublish?.Invoke();
                    return;
                }
                this.UploadAttachment(attachment, this.UploadAttachments);
            });
        }

        private void UploadAttachment(IOutboundAttachment attachment, Action callback = null)
        {
            if (attachment.UploadState == OutboundAttachmentUploadState.Completed)
                return;
            //this.UpdateUploadingStatus(true);
            attachment.Upload(() =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    //this.UpdateUploadingStatus(false);
                    //base.NotifyPropertyChanged(nameof(this.CanPublish));
                    callback?.Invoke();
                });
            });
        }
        /*
        public bool CanPublish
        {
            get
            {
                ObservableCollection<IOutboundAttachment> outboundAttachments1 = this.Attachments;
                Func<IOutboundAttachment, bool> func1 = (a => a is OutboundTimerAttachment);
                if (Enumerable.Any<IOutboundAttachment>(outboundAttachments1, func1) || !string.IsNullOrWhiteSpace(this._text))
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
        public Action CanPublish;
    }
}
