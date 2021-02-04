using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Json;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LunaVK.ViewModels
{
    public class GroupDiscussionViewModel : GenericCollectionViewModel<VKComment>
    {
        public ObservableCollection<string> Pages { get; private set; }
        public ObservableCollection<IOutboundAttachment> Attachments { get; private set; }

        private Action<VKComment> _replyCommentAction;
        private bool _isAddingComment;

        //private int real_offset_UP;
        //private int real_offset_DOWN;
        //private int UpLoaded = 0;
        //private int DownLoaded = 0;
        //private bool NeedOffsetForUp;
        //private bool NeedOffsetForDown;

        //private readonly byte NumberToLoad = 10;

        private uint _gid;
        private uint _tid;
        public uint _startCommentId;

        public bool CanComment { get; private set; }

        //object itemToScroll = null;
        /*
        public bool HasMoreUpItems
        {
            get
            {
                if (_startCommentId > 0)
                {
                    return (base._totalCount - real_offset_UP - this.NumberToLoad) > 0;
                }

                return base._totalCount - base.Items.Count > 0;
            }
        }

        public override bool HasMoreDownItems
        {
            get
            {
                if (base.Items.Count == 0)
                    return true;

                if (_startCommentId > 0)
                {
                    int val = real_offset_DOWN == 0 ? real_offset_UP : real_offset_DOWN;
                    return val > 0;
                }

                return base._totalCount - base.Items.Count > 0;
            }
        }
        */
        public string TextWatermarkText
        {
            get { return LocalizedStrings.GetString("Comment"); }
        }

        public GroupDiscussionViewModel(uint gid, uint tid, Action<VKComment> ReplyCommentAction, bool canComment = true, uint commentId = 0)
        {
            this.Pages = new ObservableCollection<string>();
            this.Attachments = new ObservableCollection<IOutboundAttachment>();

            this._gid = gid;
            this._tid = tid;
            this.CanComment = canComment;
            this._startCommentId = commentId;

            this._replyCommentAction = ReplyCommentAction;
        }

        

        public void ReplyToComment(VKComment commentItem)
        {
            this._replyCommentAction(commentItem);
        }

        public void DeleteComment(VKComment commentItem)
        {

        }


        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKComment>> callback)
        {
            GroupsService.Instance.GetTopicComments(this._gid, this._tid, offset, count, this._startCommentId, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    base._totalCount = result.response.count;
                    /*
                    Execute.ExecuteOnUIThread(() =>
                    {
                        if (this.Items.Count == 0)
                            NeedOffsetForUp = true;

                        if (this.Items.Count == 0)
                        {
                            for (int i = 0; i < base._totalCount / this.NumberToLoad; i++)
                            {
                                this.Pages.Add((i + 1).ToString());
                            }
                        }
                    });


                    real_offset_DOWN = result.response.real_offset;
                    this.DownLoaded += result.response.items.Count;
                    */
                    callback(result.error, result.response.items);
                }
                else
                {
                    callback(result.error, null);
                }
            });
        }

        public bool CanPostComment(string commentText, List<IOutboundAttachment> attachments, uint stickerItemData = 0)
        {
            if ((string.IsNullOrWhiteSpace(commentText) || commentText.Length < 2) && stickerItemData == 0)
            {
                if (attachments.Count != 0)
                {
                    if(!attachments.Any((a) => a.UploadState != OutboundAttachmentUploadState.Completed))
                        return true;
                }
                return false;
            }
            return true;
        }

        internal void AddComment(string commentText, List<IOutboundAttachment> attachments, Action<bool> resultCallback, uint stickerItemData = 0, bool fromGroup = false, string stickerReferrer = "")
        {
            if (!this.CanPostComment(commentText, attachments, stickerItemData))
                resultCallback?.Invoke(false);
            else if (this._isAddingComment)
            {
                resultCallback?.Invoke(false);
            }
            else
            {
                this._isAddingComment = true;
                GroupsService.Instance.AddTopicComment(this._gid, this._tid, commentText, attachments.Select((a) => a.ToString()).ToList(), (result) =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            //CommentItem commentItem = this.CreateCommentItem(AppGlobalStateManager.Current.GlobalState.LoggedInUser, res.ResultData, null);
                            //MyVirtualizingPanel2 virtPanel = this._virtPanel;
                            //int count = this._virtPanel.VirtualizableItems.Count;
                            //List<IVirtualizable> itemsToInsert = new List<IVirtualizable>();
                            //itemsToInsert.Add((IVirtualizable)commentItem);
                            //int num = 0;
                            //virtPanel.InsertRemoveItems(count, itemsToInsert, num != 0, null);
                            //this._virtPanel.ScrollToBottom(true);
                            //this.PublishChangedEvent();
                            base.Items.Insert(0,result.response);
                            base.NotifyPropertyChanged(nameof(base.FooterText));
                            resultCallback?.Invoke(true);
                        });
                    }
                    else
                        Execute.ExecuteOnUIThread((() =>
                        {

                            resultCallback?.Invoke(false);
                        }));

                    this._isAddingComment = false;
                }, stickerItemData, fromGroup, stickerReferrer);
            }
        }


    }
}
