using Conflux.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Conflux.Components.WebApi.Model
{

    // ===================================================================================
    // Base Request Class
    // ===================================================================================
    public class WxRequest
    {
        public long WebUserId { get; set; }
        public string ApiKey { get; set; }
        public WebRequestType RequestType { get; set; }
        public string Request { get; set; }
        public WxFilter[] Filters { get; set; }
    }
}
