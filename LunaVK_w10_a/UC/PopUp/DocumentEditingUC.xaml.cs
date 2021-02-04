using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.UC.PopUp
{
    public sealed partial class DocumentEditingUC : UserControl
    {
        VKDocument _doc;
        public Action<VKDocument> Done;

        public DocumentEditingUC(VKDocument document)
        {
            this.InitializeComponent();

            this._doc = document;
            this._tbTitle.Text = document.title;

            if(document.tags!=null)
                this._tbTags.Text = document.tags.GetCommaSeparated();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this._tbTitle.IsEnabled = this._tbTags.IsEnabled = this._btn.IsEnabled = false;
            //base.SetInProgress(true, "");
            //this.IsFormEnabled = false;
            DocumentsService.Instance.Edit(this._doc.owner_id, this._doc.id, this._tbTitle.Text, this._tbTags.Text, (result)=>
            {
                Execute.ExecuteOnUIThread(()=>
                {
                    this._tbTitle.IsEnabled = this._tbTags.IsEnabled = this._btn.IsEnabled = true;

                    if (result.error.error_code == Core.Enums.VKErrors.None && result.response == 1)
                    {
                        this._doc.title = this._tbTitle.Text;
                        if (string.IsNullOrEmpty(this._tbTags.Text))
                            this._doc.tags = null;
                        else
                            this._doc.tags = this._tbTags.Text.Split(',').ToList();
                        this.Done?.Invoke(this._doc);
                    }
                    //this.SetInProgress(false, "");
                    //this.IsFormEnabled = true;
                    //VKClient.Common.UC.GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
                });
            });
        }
    }
}
