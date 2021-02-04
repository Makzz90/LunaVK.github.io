using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.ViewModels;
using System.Threading.Tasks;
using LunaVK.Core.Network;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using System.Linq;
using Windows.Storage;
using System.Diagnostics;
using LunaVK.Core.Framework;
using System.IO;
using LunaVK.Core.Utils;

namespace LunaVK.Core.Library
{
    public class OutboundMessageViewModel : ViewModelBase, IBinarySerializable
    {
        public List<IOutboundAttachment> Attachments;

        /// <summary>
        /// Сообщение было отослано и сервер вернул его ид.
        /// Происходит после выгрузки вложений
        /// </summary>
        public event EventHandler<uint> MessageSent;
        public event EventHandler UploadFinished;

        public uint StickerItem = 0;
        public int PeerId;
        public string MessageText = string.Empty;
        public string Payload = null;
        private uint? GroupId;

        private Guid _uploadJobId = Guid.Empty;

        private double _uploadProgress;
        public double UploadProgress
        {
            get
            {
                return this._uploadProgress;
            }
            set
            {
                this._uploadProgress = value;
                base.NotifyPropertyChanged("UploadProgress");
                //base.NotifyPropertyChanged("IsUploadingVisibility");
            }
        }

        /// <summary>
        /// Показывать ли шкалу выгрузки?
        /// </summary>
        private bool _isUploading;
        public bool IsUploading
        {
            get
            {
                return this._isUploading;
            }
            set
            {
                this._isUploading = value;
                this.NotifyPropertyChanged("IsUploadingVisibility");
            }
        }

        public Visibility IsUploadingVisibility
        {
            get
            {
                if (this.IsUploading && this.CountUploadableAttachments > 0)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        
        
        public OutboundMessageViewModel(int peer_id, uint? groupId)
        {
            this.PeerId = peer_id;
            this.GroupId = groupId;
            this.OutboundMessageStatus = OutboundMessageStatus.SendingNow;
        }

        public OutboundMessageViewModel()
        {
        }

        internal void RemoveAttachment(IOutboundAttachment outboundAttCont)
        {
            this.Attachments.Remove(outboundAttCont);
        }

        public event EventHandler<OutboundMessageStatus> OutboundMessageStatusChanged;

        private OutboundMessageStatus _outboundMessageStatus;
        public OutboundMessageStatus OutboundMessageStatus
        {
            get
            {
                return this._outboundMessageStatus;
            }
            set
            {
                if (this._outboundMessageStatus == value)
                    return;
                this._outboundMessageStatus = value;
                if (this.OutboundMessageStatusChanged != null)
                    this.OutboundMessageStatusChanged(this, value);
            }
        }
        

        

        public void Send()
        {
            this.IsUploading = true;
            this._uploadJobId = Guid.NewGuid();
            this.UploadProgress = 0.0;
            this.StartSendingByAttachmentInd(0, this._uploadJobId);
        }

        /// <summary>
        /// Поочереди выгружаем вложения на сервер
        /// А когда закончим, то отправляем сообщение
        /// </summary>
        /// <param name="attachmentInd"></param>
        /// <param name="jobId"></param>
        private void StartSendingByAttachmentInd(int attachmentInd, Guid jobId)
        {
            if (jobId != this._uploadJobId)
                return;
            if (this.Attachments==null || attachmentInd >= this.Attachments.Count)
            {
                this.UploadFinished?.Invoke(this, EventArgs.Empty);

                this.DoSend();
                return;
            }

            IOutboundAttachment currentAttachment = this.Attachments[attachmentInd];
            if (currentAttachment.IsUploadAttachment)
            {
                double previousProgress = 0.0;
                currentAttachment.Upload(() =>
                {
                    if (jobId != this._uploadJobId)
                        return;

                    this.StartSendingByAttachmentInd(attachmentInd + 1, jobId);
                }, (progress) =>
                {
                    this.UploadProgressHandler(progress - previousProgress);
                    previousProgress = progress;
                });
            }
            else
            {
                this.StartSendingByAttachmentInd(attachmentInd + 1, jobId);
            }
        }

        /// <summary>
        /// Мы закончики выгружать вложения, так что пора всё отправлять
        /// </summary>
        private void DoSend()
        {
            //Dictionary<string, string> parameters = new Dictionary<string, string>();

            List<uint> ForwardedMessagesIds = null;

            List<IOutboundAttachment> temp_a = null;
            if (this.Attachments != null && this.Attachments.Count > 0)
            {
                IOutboundAttachment forwardedMessages = this.Attachments.FirstOrDefault((a => a is OutboundForwardedMessages));
                if (forwardedMessages != null)
                {
                    ForwardedMessagesIds = new List<uint>((forwardedMessages as OutboundForwardedMessages).Messages.Select<VKMessage, uint>((m => m.id)));
                }

                temp_a = new List<IOutboundAttachment>();

                for (int i = 0; i < this.Attachments.Count && i < 10; i++)
                {
                    IOutboundAttachment oa = this.Attachments[i];
                    if (oa.UploadState != OutboundAttachmentUploadState.Completed)
                        continue;

                    if (oa is OutboundPhotoAttachment oaPhoto)
                    {
                        if(oa.UploadState == OutboundAttachmentUploadState.Completed)//картинка с сервера
                        {

                        }
                        else
                        {
                            /*
                            StorageFile f = oaPhoto.sf;
                            UploadPhotoResponseData ret = await Library.DocumentsService.Instance.UploadPhotoToDialog(f);
                            if (ret == null || ret.photo.Length < 5)
                                continue;
                            MessagesService.Instance.SavePhoto(ret,(p,error)=> {
                                if (error == VKErrors.None)
                                    oaPhoto.MediaId = p.id;
                            });
                            */
                            //BUG???
                        }
                        
                        //todo:owner
                    }
                    else if(oa is OutboundGeoAttachment geo)
                    {
                        //parameters["lat"] = geo.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        //parameters["long"] = geo.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        continue;
                    }

                    temp_a.Add(oa);
                }
            }


            
            MessagesService.Instance.SendMessage(this.PeerId, (res =>
            {
                //Debug.Assert(res != null && res.error.error_code == VKErrors.None);
                if (res != null && res.error.error_code == VKErrors.None)
                {
                    //this._deliveredMessageId = res.ResultData.response;
                    //this._deliveryDateTime = DateTime.UtcNow;
                    //Logger.Instance.Info("OutboundMessageViewModel deliveryId = " + (object)this._deliveredMessageId);
                    this.OutboundMessageStatus = OutboundMessageStatus.Delivered;
                    this.MessageSent?.Invoke(this, res.response);
                }
                else
                {
                    this.OutboundMessageStatus = OutboundMessageStatus.Failed;
                    this.MessageSent?.Invoke(this, 0);
                }
                

            }), this.MessageText, this.Attachments, ForwardedMessagesIds, this.StickerItem, this.Payload, this.GroupId);
            this.IsUploading = false;
        }
        
        private void UploadProgressHandler(double deltaProgress)
        {
            if (this.CountUploadableAttachments <= 0)
                return;
            this.UploadProgress = this.UploadProgress + deltaProgress / (double)this.CountUploadableAttachments;
            //Debug.WriteLine("UploadProgressHandler " + this.UploadProgress + "%");
        }

        public int CountUploadableAttachments
        {
            get
            {
                return this.Attachments.Count(a => a.IsUploadAttachment);
            }
        }

        public void AddVoiceMessageAttachment(StorageFile file, int duration, List<int> waveform)
        {
            this.Attachments.Add(new OutboundVoiceMessageAttachment(file, duration, waveform));
        }









        public void Write(BinaryWriter writer)
        {
            writer.Write(3);
            writer.WriteString(this.MessageText);
            if (this.Attachments == null)
                this.Attachments = new List<IOutboundAttachment>();
            writer.WriteList<OutboundAttachmentContainer>(this.Attachments.Select((a => new OutboundAttachmentContainer(a))).ToList());
            writer.Write((byte)this._outboundMessageStatus);
            //writer.Write(this._deliveredMessageId);
            writer.Write(this.PeerId);
            //writer.Write(this._deliveryDateTime);
            writer.Write(this.StickerItem);
            //writer.WriteString(this.StickerReferrer);
            //writer.Write(this.GraffitiAttachmentItem, false);
        }

        public void Read(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            this.MessageText = reader.ReadString();

            List<OutboundAttachmentContainer> source = reader.ReadList<OutboundAttachmentContainer>();
            if(source.Count>0)
            {
                this.Attachments = new List<IOutboundAttachment>();
                foreach (IOutboundAttachment outboundAttachment in source.Select(c => c.OutboundAttachment))
                    this.Attachments.Add(outboundAttachment);
            }
            

            this._outboundMessageStatus = (OutboundMessageStatus)reader.ReadByte();
            if (this._outboundMessageStatus == OutboundMessageStatus.SendingNow)
                this._outboundMessageStatus = OutboundMessageStatus.Failed;
            //this._deliveredMessageId = reader.ReadInt64();
            this.PeerId = reader.ReadInt32();
            //this._deliveryDateTime = reader.ReadDateTime();
            this.StickerItem = reader.ReadUInt32();
            //this.StickerReferrer = reader.ReadString();
            //this.GraffitiAttachmentItem = reader.ReadGeneric<GraffitiAttachmentItem>();
        }
    }
}
