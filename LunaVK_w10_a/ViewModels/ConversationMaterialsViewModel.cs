using System;
using System.Collections.Generic;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core;

namespace LunaVK.ViewModels
{
    public class ConversationMaterialsViewModel
    {
        public GenericCollectionMaterials PhotosVM { get; private set; }
        public GenericCollectionMaterials VideosVM { get; private set; }
        public GenericCollectionMaterials AudiosVM { get; private set; }
        public GenericCollectionMaterials DocumentsVM { get; private set; }
        public GenericCollectionMaterials LinksVM { get; private set; }
        public GenericCollectionMaterials MarketsVM { get; private set; }
        public GenericCollectionMaterials PostsVM { get; private set; }
        public GenericCollectionMaterials ShareVM { get; private set; }
        public GenericCollectionMaterials GraffitiVM { get; private set; }
        public GenericCollectionMaterials AudioMsgsVM { get; private set; }

        //public int SubPage = 0;
        public int PeerId { get; private set; }

        public ConversationMaterialsViewModel(int peerId)
        {
            this.PhotosVM = new GenericCollectionMaterials("photo", peerId);//фотографии
            this.VideosVM = new GenericCollectionMaterials("video", peerId);//видеозаписи
            this.AudiosVM = new GenericCollectionMaterials("audio", peerId);//аудиозаписи
            this.DocumentsVM = new GenericCollectionMaterials("doc", peerId);//документы
            this.LinksVM = new GenericCollectionMaterials("link", peerId);//ссылки
            this.MarketsVM = new GenericCollectionMaterials("market", peerId);//товары
            this.PostsVM = new GenericCollectionMaterials("wall", peerId);//записи
            this.ShareVM = new GenericCollectionMaterials("share", peerId);//ссылки, товары и записи.
            this.GraffitiVM = new GenericCollectionMaterials("graffiti", peerId);
            this.AudioMsgsVM = new GenericCollectionMaterials("audio_message", peerId);
        }


        public class GenericCollectionMaterials : GenericCollectionViewModel<object>
        {
            private int _peerId;
            private string _mediaType;
            public GenericCollectionMaterials(string media_type, int peer_id)
            {
                this._mediaType = media_type;
                this._peerId = peer_id;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<object>> callback)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["count"] = count.ToString();
                parameters["peer_id"] = this._peerId.ToString();
                parameters["media_type"] = this._mediaType;

                if (!string.IsNullOrEmpty(base._nextFrom))
                    parameters["start_from"] = base._nextFrom;

                VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<ConversationMaterial>>("messages.getHistoryAttachments", parameters,(result)=>
                {
                    if(result.error.error_code == VKErrors.None)
                    {
                        if(result.response.next_from==null)
                            base._totalCount = result.response.count;

                        base._nextFrom = result.response.next_from;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }
            /*
            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return "Нет эдементов";
                    //return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneFriendFrm", "TwoFourFriendsFrm", "FiveFriendsFrm");
                    return base._totalCount.ToString();
                }
            }
            */
        }

        public sealed class ConversationMaterial
        {
            public int message_id { get; set; }
            public VKAttachment attachment { get; set; }
        }
    }
}
