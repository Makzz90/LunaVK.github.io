using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Network
{
    public class BackendResult<T, Z>
    {
        public T ResultData { get; set; }
        public Z ResultCode { get; set; }

        public BackendResult(Z resultCode, T resultData)
        {
            this.ResultData = resultData;
            this.ResultCode = resultCode;
        }

        public BackendResult()
        {

        }
    }
}
