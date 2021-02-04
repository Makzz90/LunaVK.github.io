using System;
using System.Collections.Generic;
using System.Text;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using LunaVK.Core.Network;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core;
using LunaVK.Core.Utils;
using Windows.Storage;
using System.IO;
using System.Linq;

namespace LunaVK.ViewModels
{
    public class DocumentsViewModel : GenericCollectionViewModel<DocumentsSectionViewModel>
    {
        private int OwnerId;
        public int SubPage = 0;

        public DocumentsViewModel(int ownerId)
        {
            this.OwnerId = ownerId;
        }
        
        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<DocumentsSectionViewModel>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = this.OwnerId.ToString();

            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<DocType>>("docs.getTypes", parameters,(result)=>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    base._totalCount = result.response.count;
                    List<DocumentsSectionViewModel> list = new List<DocumentsSectionViewModel>();
                    foreach (var type in result.response.items)
                    {
                        list.Add(new DocumentsSectionViewModel(this.OwnerId, type.name, type.id));
                    }
                    callback(result.error, list);
                }
                else
                {
                    callback(result.error, null);
                }
            });
        }

        public void DeleteDocument(VKDocument item)
        {
            DocumentsSectionViewModel collection = base.Items[this.SubPage];

            /*
            DocumentEditedOrDeletedEvent editedOrDeletedEvent1 = new DocumentEditedOrDeletedEvent();
            editedOrDeletedEvent1.OwnerId = item.Document.owner_id;
            editedOrDeletedEvent1.Id = item.Document.id;
            editedOrDeletedEvent1.Title = documentHeader.Name;
            editedOrDeletedEvent1.SizeString = documentHeader.GetSizeString();
            int num1 = 0;
            editedOrDeletedEvent1.IsEdited = num1 != 0;
            int num2 = this.CurrentSection.Items.TotalCount - 1;
            editedOrDeletedEvent1.NewDocumentsCount = num2;
            string str = string.Format("doc{0}_{1}", documentHeader.Document.owner_id, documentHeader.Document.id);
            editedOrDeletedEvent1.NewFirstDocumentId = str;
            DocumentEditedOrDeletedEvent editedOrDeletedEvent2 = editedOrDeletedEvent1;
            EventAggregator.Current.Publish(editedOrDeletedEvent2);
            */
            DocumentsService.Instance.Delete(item.owner_id, item.id, (result)=>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    collection.Items.Remove(item);
                });
            });
        }

        public void UploadDocuments(IReadOnlyList<StorageFile> files)
        {
            base.SetInProgress(true, "");
            int j = 0;
            Action<VKDocument> callback = null;
            callback = (result)=>
            {
                if (result!=null)
                {
                    int i;
                    if (j == files.Count - 1)
                    {
                        this.SetInProgress(false, "");
                        return;
                    }
                    /*
                    Execute.ExecuteOnUIThread(()=>
                    {
                        DocumentUploadedEvent message = new DocumentUploadedEvent
                        {
                            OwnerId = this.OwnerId,
                            Document = result.ResultData
                        };
                        EventAggregator.Current.Publish(message);
                    });
                    */
                    i = j;
                    j++;
                    this.UploadFile(files[i], callback);
                    return;
                }
                this.SetInProgress(false, "");
                //ExtendedMessageBox.ShowSafe(CommonResources.Error_Generic, CommonResources.Error);
            };
            this.UploadFile(files[j], callback);
        }

        private void UploadFile(StorageFile file, Action<VKDocument> callback)
        {
            DocumentsService.Instance.ReadFully(file, (bytes) =>
            {
                DocumentsService.Instance.UploadDocument(0, file.Name, file.FileType, bytes, (result)=>
                {
                    if(result!=null)
                    {
                        callback(result.doc);
                        Execute.ExecuteOnUIThread(() =>
                        {
                            DocumentsSectionViewModel section = this.Items.FirstOrDefault((a) => a.SectionId == result.doc.type);
                            if(section!=null)
                            {
                                section.Items.Insert(0, result.doc);
                            }
                        });
                    }
                    else
                    {
                        callback(null);
                    }
                });
            });
        }

        public class DocType
        {
            /// <summary>
            /// идентификатор типа. 
            /// </summary>
            public VKDocumentType id { get; set; }

            /// <summary>
            /// название типа. 
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// число документов; 
            /// </summary>
            public uint count { get; set; }
        }
    }

    public sealed class DocumentsSectionViewModel : GenericCollectionViewModel<VKDocument>
    {
        private readonly int OwnerId;
        public string Title { get; private set; }
        public VKDocumentType SectionId { get; private set; }

        public DocumentsSectionViewModel(int ownerId, string title, VKDocumentType sectionId)
        {
            this.OwnerId = ownerId;
            this.Title = title;
            this.SectionId = sectionId;
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKDocument>> callback)
        {
            DocumentsService.Instance.GetDocuments((result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    base._totalCount = result.response.count;
                    callback(result.error, result.response.items);
                }
                else
                {
                    callback(result.error, null);
                }
            }, offset, count, this.OwnerId, this.SectionId, true);
        }

        public override string GetFooterTextForCount
        {
            get
            {
                if (base._totalCount <= 0)
                    return LocalizedStrings.GetString("NoDocuments");
                
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneDocument", "TwoFourDocumentsFrm", "FiveMoreDocumentsFrm");
            }
        }

        //Заглушка для PivotItem.Header
        public override string ToString()
        {
            return this.Title;
        }
    }
}
