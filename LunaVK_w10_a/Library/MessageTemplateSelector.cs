using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using LunaVK.Core.Library;
using LunaVK.Core.DataObjects;

namespace LunaVK.Library
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            ListViewItem lvitem = container as ListViewItem;

            /*
            if (item is MsgDay)
            {
                lvitem.IsHitTestVisible = lvitem.IsEnabled = false;
                return this.DayTemplate;
            }
            else if (item is NewMsgSection)
            {
                lvitem.IsHitTestVisible = lvitem.IsEnabled = false;
                return this.NewMsgTemplate;
            }
            else if (item is MsgAction)
            {
                lvitem.IsHitTestVisible = lvitem.IsEnabled = false;
                return this.ActionTemplate;
            }
            */
            var msg = item as VKMessage;
            if(msg.action!=null)
            {
                lvitem.IsHitTestVisible = lvitem.IsEnabled = false;

                if (msg.action.type == Core.Enums.VKChatMessageActionType.UNREAD_ITEM_ACTION)
                    return this.NewMsgTemplate;

                return this.ActionTemplate;
            }
            lvitem.IsHitTestVisible = lvitem.IsEnabled = true;
            return this.NormalMessageTemplate;
        }
        
        public DataTemplate NormalMessageTemplate { get; set; }

        public DataTemplate DayTemplate { get; set; }

        public DataTemplate ActionTemplate { get; set; }

        public DataTemplate NewMsgTemplate { get; set; }
    }
    /*
    public class MsgDay : IMsgItem
    {
        public string dayText { get; set; }
    }

    public class NewMsgSection : IMsgItem
    {

    }

    public class MsgAction : IMsgItem
    {
        public string actionText { get; set; }
    }
    */
}
