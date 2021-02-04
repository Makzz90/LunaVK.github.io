using System;
using System.Collections.Generic;

namespace LunaVK.Core.Library
{
    public interface ISupportCache
    {
        void GetDataFromCache(Action<bool, IReadOnlyList<object>> callback);
    }
}
