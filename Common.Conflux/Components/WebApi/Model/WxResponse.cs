using Conflux.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Conflux.Components.WebApi.Model
{

    // ===================================================================================
    // Base Request Class
    // ===================================================================================
    public class WxResponse
    {
        // SystemError, AppError, DataSingle, DataList, Message
        public WebResponseType Type { get; set; }
        public string MessageTitle { get; set; }
        public string MessageInfo { get; set; }
        public string MessageAdditional { get; set; }
        public int NumEntities { get; set; }
        public string Json { get; set; }
        public WxResponse()
        {
            Type = WebResponseType.Undefined;
            MessageTitle = "";
            MessageInfo = "";
            MessageAdditional = "";
            NumEntities = 0;
            Json = null;
        }

    }
}
