using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.Enums
{
    // Возможные значения: user, chat, group, email
    public enum VKConversationPeerType : byte
    {
        User,
        Chat,
        Group,
        Email
    }
}
