using LunaVK.Core.DataObjects;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.Pages.Debug
{
    public sealed partial class TestMsgitem : Page
    {
        public TestMsgitem()
        {
            this.InitializeComponent();
            this._msg.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VKMessage vm = new VKMessage();
            if (_text.IsChecked.Value == true)
                vm.text = "Temp";

            if(_fwdmsgs.Value>0)
            {
                vm.fwd_messages = new List<VKMessage>();
                for (int i=0;i< _fwdmsgs.Value;i++)
                {
                    VKMessage fwd = new VKMessage();
                    fwd.text = i.ToString();
                    vm.fwd_messages.Add(fwd);
                }
            }

            if (_img.Value > 0)
            {
                if (vm.attachments == null)
                    vm.attachments = new List<VKAttachment>();

                //vm.fwd_messages = new List<VKMessage>();
                for (int i = 0; i < _img.Value; i++)
                {
                    VKAttachment a = new VKAttachment() { type = Core.Enums.VKAttachmentType.Photo };
                    VKPhoto photo = new VKPhoto();
                    photo.sizes = new Dictionary<char, VKImageWithSize>();
                    photo.sizes.Add('x', new VKImageWithSize() { height = 87, width = 130, type = 'x', url = "https://sun9-70.userapi.com/c543102/v543102785/597/PCpyU4hS9ys.jpg" });
                    a.photo = photo;
                    vm.attachments.Add(a);
                }
            }

            if (_doc.Value > 0)
            {
                if (vm.attachments == null)
                    vm.attachments = new List<VKAttachment>();
                
                for (int i = 0; i < _doc.Value; i++)
                {
                    VKAttachment a = new VKAttachment() { type = Core.Enums.VKAttachmentType.Doc };
                    VKDocument doc = new VKDocument() { type = Core.Enums.VKDocumentType.UNKNOWN, title = "Doc", size = 10000 * (i+1) };
                    
                    a.doc = doc;
                    vm.attachments.Add(a);
                }
            }

            if (_link.Value > 0)
            {
                if (vm.attachments == null)
                    vm.attachments = new List<VKAttachment>();
                
                for (int i = 0; i < _link.Value; i++)
                {
                    VKAttachment a = new VKAttachment() { type = Core.Enums.VKAttachmentType.Link };

                    VKPhoto photo = new VKPhoto();
                    photo.sizes = new Dictionary<char, VKImageWithSize>();
                    photo.sizes.Add('m', new VKImageWithSize() { height = 87, width = 130, type = 'm', url = "https://sun9-28.userapi.com/c836530/v836530771/627da/JP2CvmlOqiU.jpg?ava=1" });


                    VKLink link = new VKLink() {  description="Desc", title = "Title", caption = "Caption", photo = photo };

                    a.link = link;
                    vm.attachments.Add(a);
                }
            }

            if (_audio.Value > 0)
            {
                if (vm.attachments == null)
                    vm.attachments = new List<VKAttachment>();

                for (int i = 0; i < _audio.Value; i++)
                {
                    VKAttachment a = new VKAttachment() { type = Core.Enums.VKAttachmentType.Audio };
                    VKAudio audio = new VKAudio() { artist="A", title = "T", duration = 50 };
                    a.audio = audio;
                    vm.attachments.Add(a);
                }
            }

            _msg.Data = vm;
        }

        public Style BorderStyle
        {
            get
            {
                return (Style)Application.Current.Resources["BorderThemeLow"];
            }
        }
    }
}
