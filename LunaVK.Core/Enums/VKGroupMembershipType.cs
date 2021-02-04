using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.Enums
{
    public enum VKGroupMembershipType : byte
    {
        NotAMember,
        Member,
        NotSure,
        InvitationRejected,
        RequestSent,
        InvitationReceived,
    }
}
