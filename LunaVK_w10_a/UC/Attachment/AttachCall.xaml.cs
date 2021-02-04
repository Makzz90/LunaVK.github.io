using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Controls;

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace LunaVK.UC.Attachment
{
    public sealed partial class AttachCall : UserControl
    {
        public AttachCall()
        {
            this.InitializeComponent();
        }

        public AttachCall(VKCall call)
            : this()
        {
            

            this._tipe.Text = LocalizedStrings.GetString(call.initiator_id == Settings.UserId ? "AttachmentType_OutcomingCall" : "AttachmentType_IncomingCall");

            switch (call.state)
            {
                case "canceled_by_receiver":
                case "canceled_by_initiator":
                    {
                        this._duration.Text = "Отменён";
                        break;
                    }
                case "reached":
                    {
                        this._duration.Text = UIStringFormatterHelper.FormatDateTimeForUIShort(call.time,false);
                        break;
                    }
            }
        }
    }
}
