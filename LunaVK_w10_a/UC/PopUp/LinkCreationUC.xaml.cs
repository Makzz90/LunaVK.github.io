using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
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

//LinkCreationPage
//LinkCreationViewModel

namespace LunaVK.UC.PopUp
{
    public sealed partial class LinkCreationUC : UserControl
    {
        private readonly VKGroupLink _link;
        private readonly uint _communityId;
        public Action<VKGroupLink> Done;

        public LinkCreationUC(uint gId, VKGroupLink link = null)
        {
            this.InitializeComponent();





            this._link = link;
            this._communityId = gId;

            if (link == null)
            {
                this._title.Text = LocalizedStrings.GetString("LinkAdding");
            }
            else
            {
                this._title.Text = LocalizedStrings.GetString("LinkEditing");
                this._adress.IsEnabled = false;

                this._adress.Text = link.name.ForUI();
                this._description.Text = link.desc.ForUI();
                
            }
        }

        public void AddEditLink()
        {
            if (this._link == null)
            {
                string url = this._adress.Text;
                if (!url.Contains("://"))
                    url = (url.Contains("vk.com") || url.Contains("vkontakte.ru") || url.Contains("vk.cc") ? "https://" : "http://") + url;

                GroupsService.Instance.AddLink(this._communityId, url, this._description.Text, (result) =>
                {
                    if (result.error.error_code == Core.Enums.VKErrors.None)
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            this.Done?.Invoke(result.response);
                        });
                    }
                });
            }
            else
            {
                GroupsService.Instance.EditLink(this._communityId, this._link.id, this._description.Text, (result) =>
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        if (result.error.error_code == Core.Enums.VKErrors.None && result.response == 1)
                        {
                            if (this._link.edit_title)
                                this._link.name = this._description.Text;
                            else
                                this._link.desc = this._description.Text;

                            this.Done?.Invoke(this._link);
                        }
                    });
                });
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.AddEditLink();
        }
    }
}
